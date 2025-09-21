using ZergPoolMiner.Configs;
using ZergPoolMiner.Devices;
using ZergPoolMiner.Miners;
using ZergPoolMinerLegacy.Common.Enums;
using ZergPoolMinerLegacy.Overclock;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using ZergPoolMiner.Algorithms;
using System.Collections.Concurrent;
using System.Linq;

namespace ZergPoolMiner.Switching
{
    /// <summary>
    /// Handles profit switching within a mining session
    /// </summary>
    public class AlgorithmSwitchingManager
    {
        private const string Tag = "SwitchingManager";

        /// <summary>
        /// Emitted when the profits are checked
        /// </summary>
        public static event EventHandler<SmaUpdateEventArgs> SmaCheck;

        public static System.Timers.Timer _smaCheckTimer;
        private static readonly Random _random = new Random();//?

        public static int _ticksForStable;
        private static int _ticksForUnstable;
        public static bool SmaCheckTimerOnElapsedRun = false;
        // Simplify accessing config objects
        public static Interval StableRange => ConfigManager.GeneralConfig.SwitchSmaTicksStable;
        public static Interval UnstableRange => ConfigManager.GeneralConfig.SwitchSmaTicksUnstable;
        public static Interval SmaCheckRange => ConfigManager.GeneralConfig.SwitchSmaTimeChangeSeconds;

        //public static int MaxHistory => Math.Max(StableRange.Upper, UnstableRange.Upper);
        public static int MaxHistory => 60;

        public static readonly Dictionary<AlgorithmType, AlgorithmHistory> _algosHistory = new Dictionary<AlgorithmType, AlgorithmHistory>();

        private static bool _hasStarted;
        public static bool KawpowLiteGoodEpoch = false;

        public static List<AlgorithmType> unstableAlgosList = new List<AlgorithmType>()
        {
            AlgorithmType.Allium,
            AlgorithmType.CurveHash,
            AlgorithmType.HooHash,
            AlgorithmType.KarlsenHashV2,
            AlgorithmType.NeoScrypt,
            AlgorithmType.Megabtx,
            AlgorithmType.Ethashb3,
            AlgorithmType.X16RT,
            AlgorithmType.X16RV2,
            AlgorithmType.X21S,
            AlgorithmType.X25X,
            AlgorithmType.SHA256csm,
            AlgorithmType.SHA512256d,
            AlgorithmType.Equihash125,
            AlgorithmType.Equihash144
        };

        /// <summary>
        /// Currently used normalized profits
        /// </summary>
        private static readonly Dictionary<AlgorithmType, MostProfitableCoin> _lastLegitPaying = new Dictionary<AlgorithmType, MostProfitableCoin>();
        public class MostProfitableCoin
        {
            public string coin { get; set; }
            public double profit { get; set; }
            public double currentProfit { get; set; }//?
        }

        public static ConcurrentDictionary<string, CoinsData> coinsDataList = new();
        public class CoinsData
        {
            public string Algorithm { get; set; }
            public string Coin { get; set; }
            public double prevprofit { get; set; }
            public double currentProfit { get; set; }
            public int ticks { get; set; }
        }
        public AlgorithmSwitchingManager()
        {
            var miningDevices = ComputeDeviceManager.Available.Devices;
            try
            {
                foreach (var device in miningDevices)
                {
                    if (device != null && device.Enabled)
                    {
                        var devicesAlgos = device.GetAlgorithmSettings();
                        foreach (var a in devicesAlgos)
                        {
                            if (a.Enabled)
                            {
                                foreach (var kvp in AlgosProfitData.FilteredCurrentProfits())
                                {
                                    if (a.ZergPoolID == kvp.Key || a.DualZergPoolID == kvp.Key || 
                                        a.SecondaryZergPoolID == kvp.Key)
                                    {
                                        _algosHistory[kvp.Key] = new AlgorithmHistory(MaxHistory);
                                        _lastLegitPaying[kvp.Key] = kvp.Value;
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex)
            {
                Helpers.ConsolePrintError("AlgorithmSwitchingManager", ex.ToString());
            }

            /*
            foreach (var kvp in NHSmaData.FilteredCurrentProfits(false))
            {
                _unstableHistory[kvp.Key] = new AlgorithmHistory(MaxHistory);
                _lastLegitPaying[kvp.Key] = kvp.Value;
            }
            */
        }

        public static int GetTicks(AlgorithmType algo)
        {
            if (!unstableAlgosList.Contains(algo)) 
            {
                return 15;
            }
            return 30;
        }

        public static void Start()
        {

            if (_smaCheckTimer == null)
            {
                Helpers.ConsolePrint("AlgorithmSwitchingManager", "Start");
                //_smaCheckTimer = new System.Timers.Timer(1000);
                _smaCheckTimer = new System.Timers.Timer();
                _smaCheckTimer.Interval = 60 * 1000;
                /*
                if (ConfigManager.GeneralConfig.AdaptiveAlgo)
                {
                    _smaCheckTimer.Interval = 300 * 1000;
                } else
                {
                    _smaCheckTimer.Interval = 60 * 1000;
                }
                */
                _smaCheckTimer.Elapsed += SmaCheckTimerOnElapsed;
                _smaCheckTimer.Start();
            }
            SmaCheckNow();
        }
        public static void SmaCheckNow()
        {
            SmaCheckTimerOnElapsed(null, null);
        }

        public static void Stop()
        {
            Helpers.ConsolePrint("AlgorithmSwitchingManager", "Stop");

            if (_smaCheckTimer != null)
            {
                MiningSession.StopEvent();
                _smaCheckTimer?.Stop();
                _smaCheckTimer.Close();
                _smaCheckTimer.Dispose();
                _smaCheckTimer = null;

                try
                {
                    if (SmaCheck != null)
                    {
                        foreach (Delegate d in SmaCheck.GetInvocationList())
                        {
                            SmaCheck -= (EventHandler<SmaUpdateEventArgs>)d;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Helpers.ConsolePrintError("AlgorithmSwitchingManager", ex.ToString());
                }

            }

        }

        /// <summary>
        /// Checks profits and updates normalization based on ticks
        /// </summary>
        public static void SmaCheckTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            SmaCheckTimerOnElapsedRun = true;
            GetTickPeriod();
            var sb = new StringBuilder();
            if (_hasStarted)
            {
                sb.AppendLine("Normalizing profits:");
            }
            var stableUpdated = UpdateProfits(_algosHistory, _ticksForStable, sb);
            if (!stableUpdated && _hasStarted)
            {
                sb.AppendLine("No algos affected (either no SMA update or no algos higher");
            }
            if (_hasStarted)
            {
                //Helpers.ConsolePrint(Tag, sb.ToString());
            }
            else
            {
                _hasStarted = true;
            }
            var args = new SmaUpdateEventArgs(_lastLegitPaying);
            SmaCheck?.Invoke(sender, args);
        }

        /// <summary>
        /// Check profits for a history dict and update if profit has been higher for required ticks or if it is lower
        /// </summary>
        /// <returns>True iff any profits were postponed or updated</returns>
        private static bool UpdateProfits(Dictionary<AlgorithmType, AlgorithmHistory> history, int ticks, StringBuilder sb)
        {
            var updated = false;
            var cTicks = "min";
            try
            {
                /*
                foreach (var coin in Stats.Stats.CoinList)
                {
                    CoinsData cd = new();
                    cd.Algorithm = coin.algo;
                    cd.Coin = coin.symbol;
                    if (!coinsDataList.ContainsKey(coin.algo))
                    {
                        cd.prevprofit = 0;
                    }
                    else
                    {
                        cd.prevprofit = coinsDataList.FirstOrDefault(x => x.Key.ToLower() == coin.algo.ToLower()).Value.currentProfit;
                        cd.currentProfit = Stats.Stats.MiningAlgorithmsList.FirstOrDefault(x => x.name.ToLower() == coin.algo.ToLower()).adaptive_profit;
                    }
                    if (coin.apibug || coin.coinTempDeleted || coin.hashrate == 0 || coin.noautotrade == 1 ||
                        coin.tempBlock || coin.tempTTF_Disabled)
                    {
                        continue;
                    }
                    if (cd.prevprofit < cd.currentProfit && cd.prevprofit != 0)
                    {
                        cd.ticks++;
                    } else
                    {
                        cd.ticks--;
                    }
                    coinsDataList.AddOrUpdate(coin.algo.ToLower(), cd, (k, v) => cd);
                }

                var coinsDataListSortered = coinsDataList.OrderBy(vp => vp.Key);
                foreach (var c in coinsDataListSortered)
                {
                    Helpers.ConsolePrint(c.Key, c.Value.Coin + " " + c.Value.prevprofit.ToString("F5") + " " +
                        c.Value.currentProfit.ToString("F5"));
                }
                */
                foreach (var algo in history.Keys)
                {
                    var _c = Stats.Stats.CoinList.Find(a => a.algo.ToLower() == algo.ToString().ToLower());
                    if (_c is object && _c != null)
                    {

                    } else
                    {
                        continue;
                    }
                    AlgosProfitData.TryGetPaying(algo, out var paying);
                    ticks = GetTicks(algo);
                    
                    if (algo == AlgorithmType.KawPowLite && !KawpowLiteGoodEpoch)
                    {
                        paying.profit = 0;
                    }

                    if (!algo.ToString().Contains("UNUSED"))
                    {
                        double _profit = paying.profit;
                        history[algo].Add(_profit);
                        var overCount = history[algo].CountOverProfit(_lastLegitPaying[algo].profit);
                        //var downCount = history[algo].CountDownProfit(_lastLegitPaying[algo].profit);
                        double p1 = 100 - (_lastLegitPaying[algo].profit / paying.profit) * 100;

                        if (paying.profit > _lastLegitPaying[algo].profit)
                        {
                            updated = true;

                            if (overCount >= ticks)
                            {
                                //_lastLegitPaying[algo].coin = paying.coin;
                                //_lastLegitPaying[algo].profit = paying.profit;
                                sb.AppendLine($" TAKEN: new profit {paying.profit:f5} {p1:f2}% " +
                                    $"after {overCount}/{ticks} {cTicks} for {algo} ({paying.coin})");

                                _lastLegitPaying[algo].profit = paying.profit;
                                _lastLegitPaying[algo].coin = paying.coin;
                            }
                            else
                            {
                                sb.AppendLine(
                                    $" POSTPONED: new profit {paying.profit:f5} ({paying.coin}) " +
                                    $"(previously {_lastLegitPaying[algo].profit:f5} ({_lastLegitPaying[algo].coin}) {p1:f2}%," +
                                    $" higher for {overCount}/{ticks} {cTicks} for {algo}"
                                );
                                _lastLegitPaying[algo].profit = paying.profit;
                                _lastLegitPaying[algo].coin = paying.coin;
                            }
                        }
                        else
                        {
                            // Profit has gone down updated = true;
                            sb.AppendLine($" Profit has gone down: old profit {_lastLegitPaying[algo].profit:f5} ({_lastLegitPaying[algo].coin}) " +
                                $"(new profit  {paying.profit:f5} ({paying.coin})) {p1:f2}%," +
                                $" less for {overCount}/{ticks} {cTicks} for {algo}");

                            _lastLegitPaying[algo].profit = paying.profit;
                            _lastLegitPaying[algo].coin = paying.coin;
                        }
                    }
                }
                
                //проверка неактивных монет
                foreach (var miningDevice in MiningSession._miningDevices)
                {
                    if (miningDevice.DeviceCurrentMiningCoin != null)
                    {
                        Stats.Stats.CoinBlocked _cb = new();
                        List<string> coins = new();
                        if (miningDevice.DeviceCurrentMiningCoin.Contains("+"))
                        {
                            coins.Add(miningDevice.DeviceCurrentMiningCoin.Split('+')[0]);
                            coins.Add(miningDevice.DeviceCurrentMiningCoin.Split('+')[1]);
                        }
                        else
                        {
                            coins.Add(miningDevice.DeviceCurrentMiningCoin);
                        }
                        foreach (var coin in coins)
                        {
                            foreach (var c in Stats.Stats.coinsMining)
                            {
                                //Helpers.ConsolePrint("coin: " + coin, "coinsMining: " + c.symbol);
                            }
                            if (Stats.Stats.coinsMining.Exists(a => a.symbol == coin))
                            {
                                if (Stats.Stats.coinsBlocked.ContainsKey(coin))
                                {
                                    //Helpers.ConsolePrint("coin: " + coin, "Майнится. Удаляем из списка");
                                    //Майнится. Удаляем из списка
                                    bool result = Stats.Stats.coinsBlocked.TryRemove(coin, out var removedItem);
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(coin))//first run
                                {
                                    if (Stats.Stats.coinsBlocked.ContainsKey(coin))
                                    {

                                    }
                                    else
                                    {
                                        //Helpers.ConsolePrint("coin: " + coin, "НЕ добавлено. НЕ майнится. Добавляем");
                                        //НЕ добавлено. НЕ майнится. Добавляем
                                        _cb.coin = coin;
                                        _cb.checkTime = 0;
                                        Stats.Stats.coinsBlocked.AddOrUpdate(_cb.coin, _cb, (k, v) => _cb);
                                    }
                                }
                            }
                        }
                    }
                }

                string toRemove = null;
                foreach (var _cb in Stats.Stats.coinsBlocked)
                {
                    //Добавлено. Обновляем
                    int ct = _cb.Value.checkTime;
                    ct++;
                    _cb.Value.checkTime = ct;
                    //Helpers.ConsolePrint("UpdateProfits", _cb.Value.coin + " blocked count: " + ct.ToString());
                    //Helpers.ConsolePrint("coin: " + _cb.Value.coin, "Добавлено. Обновляем " + ct.ToString());
                    Stats.Stats.coinsBlocked.AddOrUpdate(_cb.Value.coin, _cb.Value, (k, v) => _cb.Value);
                    if (ct >= 180)//3 hour
                    {
                        toRemove = _cb.Value.coin;
                    }
                }
                if (toRemove != null)
                {
                    if (Stats.Stats.coinsBlocked.ContainsKey(toRemove))
                    {
                        //Удаляем из списка
                        bool result = Stats.Stats.coinsBlocked.TryRemove(toRemove, out var removedItem);
                    }
                }
            } catch (Exception ex)
            {
                Helpers.ConsolePrintError("UpdateProfits", ex.ToString());
            }
            return updated;
        }

        private static void GetTickPeriod()
        {
            // Lock in case this gets called simultaneously
            // Random breaks down when called from multiple threads
            lock (_random)
            {
                _ticksForStable = StableRange.RandomInt(_random);
                _ticksForUnstable = UnstableRange.RandomInt(_random);
                //_smaCheckTime = SmaCheckRange.RandomInt(_random);
                /*
                _ticksForStable = 5;
                _ticksForUnstable = 5;
                _smaCheckTime = 5;
                */
                if (ConfigManager.GeneralConfig.SwitchingAlgorithmsIndex == 0)
                {
                    //_smaCheckTime = 60;
                    _ticksForStable = 1;
                    _ticksForUnstable = 1;
                }
                if (ConfigManager.GeneralConfig.SwitchingAlgorithmsIndex == 1)
                {
                    //_smaCheckTime = 60;
                    _ticksForStable = 3;
                    _ticksForUnstable = 3;
                }
                if (ConfigManager.GeneralConfig.SwitchingAlgorithmsIndex == 2)
                {
                    //_smaCheckTime = 60;
                    _ticksForStable = 5;
                    _ticksForUnstable = 5;
                }
                if (ConfigManager.GeneralConfig.SwitchingAlgorithmsIndex == 3)
                {
                    //_smaCheckTime = 60;
                    _ticksForStable = 10;
                    _ticksForUnstable = 10;
                }
                if (ConfigManager.GeneralConfig.SwitchingAlgorithmsIndex == 4)
                {
                    //_smaCheckTime = 60;
                    _ticksForStable = 15;
                    _ticksForUnstable = 15;
                }
                if (ConfigManager.GeneralConfig.SwitchingAlgorithmsIndex == 5)
                {
                    //_smaCheckTime = 60;
                    _ticksForStable = 30;
                    _ticksForUnstable = 30;
                }
                if (ConfigManager.GeneralConfig.SwitchingAlgorithmsIndex == 6)
                {
                    //_smaCheckTime = 60;
                    _ticksForStable = 60;
                    _ticksForUnstable = 60;
                }
                if (ConfigManager.GeneralConfig.ShortTerm)
                {
                    _ticksForStable = 60;
                    _ticksForUnstable = 60;
                }
                if (ConfigManager.GeneralConfig.AdaptiveAlgo)
                {
                    var interval = _smaCheckTimer.Interval / 1000;
                    _ticksForStable = 15;//10 min
                    _ticksForUnstable = 30;//30 min
                }
            }
        }


    }

    /// <inheritdoc />
    /// <summary>
    /// Event args used for reporting fresh normalized profits
    /// </summary>
    public class SmaUpdateEventArgs : EventArgs
    {
        public readonly Dictionary<AlgorithmType, AlgorithmSwitchingManager.MostProfitableCoin> NormalizedProfits;

        public SmaUpdateEventArgs(Dictionary<AlgorithmType, AlgorithmSwitchingManager.MostProfitableCoin> profits)
        {
            NormalizedProfits = profits;
        }
    }
}
