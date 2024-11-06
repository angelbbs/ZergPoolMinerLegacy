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
                        Port = (int)algo + 3333,
                        Name = algo.ToString().ToLower(),
                        Algo = (int)algo,
                        Paying = paying
                    };

                    _finalAlgosProfit[algo] = new Sma
                    {
                        Port = (int)algo + 3333,
                        Name = algo.ToString().ToLower(),
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
                        Port = (int)0,
                        Name = algo.ToString().ToLower(),
                        Algo = (int)algo,
                        Paying = paying
                    };

                    _finalAlgosProfit[algo] = new Sma
                    {
                        Port = (int)0,
                        Name = algo.ToString().ToLower(),
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
                        Port = 3385,
                        Name = algo.ToString().ToLower(),
                        Algo = (int)algo,
                        Paying = paying
                    };
                    _finalAlgosProfit[algo] = new Sma
                    {
                        Port = 3385,
                        Name = algo.ToString().ToLower(),
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
        public static void UpdatePayingForAlgo(AlgorithmType algo, double paying, bool average = false)
        {
            InitializeIfNeeded();
            CheckInit();
            //if (double.IsNaN(paying)) return;
            lock (_currentAlgosProfit)
            {
                if (!_currentAlgosProfit.ContainsKey(algo))
                    throw new ArgumentException("Algo not setup in SMA");

                //if (paying != 0)
                {
                    if (_currentAlgosProfit[algo].Paying > 0 && paying > _currentAlgosProfit[algo].Paying * 100)
                    {
                        Helpers.ConsolePrint("AlgosProfitData", "AlgosProfitData API bug. " + algo.ToString() + ": " +
                            "old value: " + _currentAlgosProfit[algo].Paying.ToString() + " new value: " + paying);
                        return;
                    }

                    if (average)
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
                        v.Algo = final_sma.Value.Algo;
                        v.Name = final_sma.Value.Name;
                        v.Paying = final_sma.Value.Paying;
                        v.Port = final_sma.Value.Port;
                        if (!((AlgorithmType)v.Algo).ToString().Contains("UNUSED"))
                        {
                            //Helpers.ConsolePrint("AlgosProfitData", ((AlgorithmType)v.Algo).ToString() + ":\t" + v.Paying.ToString());
                        }
                        _finalAlgosProfit.Add(final_sma.Key, v);
                    }
                }
                catch (Exception ex)
                {
                    Helpers.ConsolePrint("AlgosProfitData", ex.ToString());
                }
            }
        }

        public static bool TryGetPaying(AlgorithmType algo, out double paying)
        {
            InitializeIfNeeded();
            CheckInit();

            if (TryGetSma(algo, out Sma sma))
            {
                paying = sma.Paying;
                //Helpers.ConsolePrint(algo.ToString(), paying.ToString());
                if (algo == AlgorithmType.KawPowLite && !AlgorithmSwitchingManager.KawpowLiteGoodEpoch)
                {
                    paying = 0.0d;
                }
                return true;
            }

            paying = default(double);
            return false;
        }

        #endregion

        #region Get Methods

        public static Dictionary<AlgorithmType, double> FilteredCurrentProfits()
        {
            CheckInit();
            var dict = new Dictionary<AlgorithmType, double>();
            lock (_finalAlgosProfit)
            {
                foreach (var kvp in _finalAlgosProfit)
                {
                    dict[kvp.Key] = kvp.Value.Paying;
                }
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
