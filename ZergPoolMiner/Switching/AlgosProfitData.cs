using Newtonsoft.Json;
using ZergPoolMiner.Configs;
using ZergPoolMinerLegacy.Common.Enums;
using ZergPoolMinerLegacy.Overclock;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ZergPoolMiner.Switching
{
    public static class AlgosProfitData
    {
        public static bool Initialized { get; private set; }
        public static bool HasData { get; private set; }
        private static Dictionary<AlgorithmType, SmaTmp> _currentAlgosProfit;
        private static Dictionary<AlgorithmType, Sma> _finalAlgosProfit;

        private static void InitializeFinalizeAlgosProfitList()
        {
            if (Initialized) return;
            Helpers.ConsolePrint("AlgosProfitData", "Try initialize AlgosProfitData");
            _currentAlgosProfit = null;
            _finalAlgosProfit = null;
            _currentAlgosProfit = new Dictionary<AlgorithmType, SmaTmp>();
            _finalAlgosProfit = new Dictionary<AlgorithmType, Sma>();
            
            foreach (AlgorithmType algo in Enum.GetValues(typeof(AlgorithmType)))
            {
                if (algo >= 0)
                {
                    var paying = 0d;
                        HasData = true;

                    _currentAlgosProfit[algo] = new SmaTmp
                    {
                        Name = algo.ToString().ToLower(),
                        Coin = "Unknown",
                        Algo = (int)algo,
                        Paying = paying
                    };

                    _finalAlgosProfit[algo] = new Sma
                    {
                        Name = algo.ToString().ToLower(),
                        Coin = "Unknown",
                        Algo = (int)algo,
                        Paying = paying
                    };
                }

                if (algo == AlgorithmType.ZIL)
                {
                    var paying = 0d;
                    HasData = true;

                    _currentAlgosProfit[algo] = new SmaTmp
                    {
                        Name = algo.ToString().ToLower(),
                        Coin = "Unknown",
                        Algo = (int)algo,
                        Paying = paying
                    };

                    _finalAlgosProfit[algo] = new Sma
                    {
                        Name = algo.ToString().ToLower(),
                        Coin = "Unknown",
                        Algo = (int)algo,
                        Paying = paying
                    };

                }

                if (algo == AlgorithmType.KawPowLite)
                {
                    var paying = 0d;
                    HasData = true;

                    _currentAlgosProfit[algo] = new SmaTmp
                    {
                        Name = algo.ToString().ToLower(),
                        Coin = "Unknown",
                        Algo = (int)algo,
                        Paying = paying
                    };
                    _finalAlgosProfit[algo] = new Sma
                    {
                        Name = algo.ToString().ToLower(),
                        Coin = "Unknown",
                        Algo = (int)algo,
                        Paying = paying
                    };
                }
            }
            Initialized = true;
            FinalizeAlgosProfitList();
        }

        public static void InitializeIfNeeded()
        {
            if (!Initialized) InitializeFinalizeAlgosProfitList();
        }

        #region Update Methods

        /// <summary>
        /// Change SMA profit for one algo
        /// </summary>
        public static void UpdatePayingForAlgo(AlgorithmType algo, string coin, double paying, bool average = false)
        {
            InitializeIfNeeded();
            CheckInit();
            //if (double.IsNaN(paying)) return;
            lock (_currentAlgosProfit)
            {
                if (!_currentAlgosProfit.ContainsKey(algo))
                    //Helpers.ConsolePrint("UpdatePayingForAlgo", "Algo not setup in SMA: " + algo);
                    throw new ArgumentException("Algo not setup in SMA");

                _currentAlgosProfit[algo].Coin = coin;
                //if (paying != 0)
                {
                    if (_currentAlgosProfit[algo].Paying > 0 && paying > _currentAlgosProfit[algo].Paying * 15000)
                    {
                        Helpers.ConsolePrint("AlgosProfitData", "AlgosProfitData API bug. " + algo.ToString() + ": " +
                            "old value: " + _currentAlgosProfit[algo].Paying.ToString() + " new value: " + paying);
                        paying = _currentAlgosProfit[algo].Paying;
                        return;
                    }

                    if (paying > 1000000)
                    {
                        Helpers.ConsolePrint("AlgosProfitData", "AlgosProfitData API bug. " + algo.ToString() + ": " + paying);
                        return;
                    }


                    if (paying > 0 && _currentAlgosProfit[algo].Paying > 0 &&
                        paying > _currentAlgosProfit[algo].Paying * 2)
                    {
                        Helpers.ConsolePrint("AlgosProfitData", "(" + algo.ToString() + ") New profit is above 100%. Averaging.");
                        paying = _currentAlgosProfit[algo].Paying + paying / 50;
                    }

                    if (paying > 0 && _currentAlgosProfit[algo].Paying > 0 &&
                        paying > _currentAlgosProfit[algo].Paying * 1.5)
                    {
                        Helpers.ConsolePrint("AlgosProfitData", "(" + algo.ToString() + ") New profit is above 50%. Averaging.");
                        paying = _currentAlgosProfit[algo].Paying + paying / 25;
                    }

                    if (paying > 0 && _currentAlgosProfit[algo].Paying > 0 &&
                        paying > _currentAlgosProfit[algo].Paying * 1.2)
                    {
                        Helpers.ConsolePrint("AlgosProfitData", "(" + algo.ToString() + ") New profit is above 20%. Averaging.");
                        paying = _currentAlgosProfit[algo].Paying + paying / 10;
                    }

                    if (average && paying > 0 && _currentAlgosProfit[algo].Paying > 0)
                    {
                        _currentAlgosProfit[algo].Paying = (paying + _currentAlgosProfit[algo].Paying) / 2;
                    }
                    else
                    {
                        _currentAlgosProfit[algo].Paying = paying;
                    }
                }
                if (algo == AlgorithmType.KawPowLite)
                {
                    _currentAlgosProfit[algo].Paying = paying;
                }
            }
            HasData = true;
        }

        #endregion

        # region Get Methods

        /// <summary>
        /// Attempt to get SMA for an algorithm
        /// </summary>
        /// <param name="algo">Algorithm</param>
        /// <param name="sma">Variable to place SMA in</param>
        /// <returns>True iff we know about this algo</returns>
        public static bool TryGetSma(AlgorithmType algo, out Sma sma)
        {
            InitializeIfNeeded();
            CheckInit();
            lock (_finalAlgosProfit)
            {
                if (_finalAlgosProfit.ContainsKey(algo))
                {
                    sma = _finalAlgosProfit[algo];
                    return true;
                }
            }

            sma = null;
            return false;
        }

        public static void FinalizeAlgosProfitList()
        {
            /*
            Random r = new Random();
            int r1 = r.Next(5, 15);
            Thread.Sleep(100 * r1);
            */
            //Helpers.ConsolePrint("AlgosProfitData", "Finalize profit of algoritms");
            InitializeIfNeeded();
            CheckInit();
            _finalAlgosProfit.Clear();

            lock (_finalAlgosProfit)
            {
                try
                {
                    foreach (var final_sma in _currentAlgosProfit)
                    {
                        Sma v = new Sma();
                        v.Name = final_sma.Value.Name;
                        v.Coin = final_sma.Value.Coin;
                        v.Algo = final_sma.Value.Algo;
                        v.Paying = final_sma.Value.Paying;
                        _finalAlgosProfit.Add(final_sma.Key, v);
                    }
                }
                catch (Exception ex)
                {
                    Helpers.ConsolePrint("AlgosProfitData", ex.ToString());
                }
            }
        }

        public static bool TryGetPaying(AlgorithmType algo, out AlgorithmSwitchingManager.MostProfitableCoin paying)
        {
            InitializeIfNeeded();
            CheckInit();
            paying = new AlgorithmSwitchingManager.MostProfitableCoin();
            if (TryGetSma(algo, out Sma sma))
            {
                paying.profit = sma.Paying;
                paying.coin = sma.Coin;
                if (algo == AlgorithmType.KawPowLite && !AlgorithmSwitchingManager.KawpowLiteGoodEpoch)
                {
                    paying.profit = 0.0d;
                }

                //���� ������������ ���������� ������ ��������� � ������� ������
                paying.currentProfit = paying.profit;
                var c = Stats.Stats.CoinList.Find(e => e.symbol.Equals(sma.Coin));
                if (c is object && c != null)
                {
                    if (algo.ToString().ToLower().Equals(c.algo.ToLower()))
                    {
                        var current_profit = c.estimate_current * c.adaptive_factor;
                        paying.currentProfit = current_profit;
                        paying.profit = current_profit;
                    }
                }
                return true;
            }

            paying = new AlgorithmSwitchingManager.MostProfitableCoin();
            return false;
        }

        #endregion

        #region Get Methods


        public static Dictionary<AlgorithmType, AlgorithmSwitchingManager.MostProfitableCoin> FilteredCurrentProfits()
        {
            CheckInit();
            var dict = new Dictionary<AlgorithmType, AlgorithmSwitchingManager.MostProfitableCoin>();
            try
            {
                lock (_finalAlgosProfit)
                {
                    foreach (var kvp in _finalAlgosProfit)
                    {
                        AlgorithmSwitchingManager.MostProfitableCoin mpc = new();
                        mpc.coin = kvp.Value.Coin;
                        mpc.profit = kvp.Value.Paying;
                        dict.Add(kvp.Key, mpc);
                        //dict[kvp.Key].coin = kvp.Value.Coin;
                        //dict[kvp.Key].profit = kvp.Value.Paying;
                    }
                }
            } catch (Exception ex)
            {
                Helpers.ConsolePrint("FilteredCurrentProfits", ex.ToString());
            }
            return dict;
        }

        #endregion

        private static void CheckInit()
        {
            if (!Initialized)
                throw new InvalidOperationException("NHSmaData cannot be used before initialization");
        }
    }
}
