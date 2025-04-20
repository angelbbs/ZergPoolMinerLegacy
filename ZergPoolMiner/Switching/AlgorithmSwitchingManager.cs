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
        private static double _smaCheckTime = 180;
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
            AlgorithmType.NeoScrypt,
            AlgorithmType.Megabtx,
            AlgorithmType.Ethashb3,
            AlgorithmType.X16RT,
            AlgorithmType.X16RV2,
            AlgorithmType.X21S,
            AlgorithmType.X25X,
            AlgorithmType.SHA256csm,
            AlgorithmType.SHA512256d,
            AlgorithmType.Equihash144//пока не уберут btg
        };

        /// <summary>
        /// Currently used normalized profits
        /// </summary>
        private static readonly Dictionary<AlgorithmType, MostProfitableCoin> _lastLegitPaying = new Dictionary<AlgorithmType, MostProfitableCoin>();
        public class MostProfitableCoin
        {
            public string coin { get; set; }
            public double profit { get; set; }
            public double currentProfit { get; set; }
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
                Helpers.ConsolePrint(Tag, sb.ToString());
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
                foreach (var algo in history.Keys)
                {
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
                                sb.AppendLine($" TAKEN: new profit {paying.profit:e5} {p1:f2}% " +
                                    $"after {overCount}/{ticks} {cTicks} for {algo} ({paying.coin})");

                                _lastLegitPaying[algo].profit = paying.profit;
                                foreach (var miningDevice in MiningSession._miningDevices)
                                {
                                    if (miningDevice.CurrentProfitableAlgorithmType.Equals(algo))
                                    {
                                        _lastLegitPaying[algo].coin = paying.coin;
                                        //_lastLegitPaying[algo].profit = paying.profit;
                                        miningDevice.diff = p1;
                                    }
                                    else
                                    {
                                        //miningDevice.diff = 0;
                                    }
                                }
                            }
                            else
                            {
                                sb.AppendLine(
                                    $" POSTPONED: new profit {paying.profit:e5} ({paying.coin}) " +
                                    $"(previously {_lastLegitPaying[algo].profit:e5} ({_lastLegitPaying[algo].coin}) {p1:f2}%," +
                                    $" higher for {overCount}/{ticks} {cTicks} for {algo}"
                                );
                            }
                        }
                        else
                        {
                            // Profit has gone down
                            updated = true;

                            sb.AppendLine($" Profit has gone down: old profit {paying.profit:e5} ({_lastLegitPaying[algo].coin}) " +
                                $"(new profit  {_lastLegitPaying[algo].profit:e5} ({paying.coin})) {p1:f2}%," +
                                $" less for {overCount}/{ticks} {cTicks} for {algo}");

                            _lastLegitPaying[algo].profit = paying.profit;
                            foreach (var miningDevice in MiningSession._miningDevices)
                            {
                                if (miningDevice.CurrentProfitableAlgorithmType.Equals(algo))
                                {
                                    _lastLegitPaying[algo].coin = paying.coin;
                                    //_lastLegitPaying[algo].profit = paying.profit;
                                    miningDevice.diff = p1;
                                }
                                else
                                {
                                    //miningDevice.diff = 0;
                                }
                            }
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
                                //Helpers.ConsolePrint("coin: " + coin + " coinsMining", c.symbol);
                            }
                            if (Stats.Stats.coinsMining.Exists(a => a.symbol == coin))
                            {
                                if (Stats.Stats.coinsBlocked.ContainsKey(coin))
                                {
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
                    Stats.Stats.coinsBlocked.AddOrUpdate(_cb.Value.coin, _cb.Value, (k, v) => _cb.Value);
                    if (ct >= 60)//1 hour
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
                //поиск одинакового хешрейта
                /*
                foreach (var miningDevice in MiningSession._miningDevices)
                {
                    var coin = miningDevice.DeviceCurrentMiningCoin;
                    foreach (var ap in Stats.Stats.algosProperty)
                    {
                        var algo = ap.Value.name;
                        var hashrate = ap.Value.hashrate;
                        if (Stats.Stats.coinsMining.Exists(a => a.hashrate_shared == hashrate))
                        {
                            var _coin = Stats.Stats.coinsMining.Find(a => a.hashrate_shared == hashrate);
                            if (hashrate.Equals(_coin.hashrate_shared))
                            {
                                Helpers.ConsolePrint(hashrate, 
                                    _coin.symbol + " " + coin);
                            }
                        }
                    }
                }
                */

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
