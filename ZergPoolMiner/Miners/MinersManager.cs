using ZergPoolMiner.Devices;
using ZergPoolMiner.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZergPoolMiner.Miners
{
    public static class MinersManager
    {
        private static MiningSession _curMiningSession;

        public static void StopAllMiners()
        {
            _curMiningSession?.StopAllMiners();
            _curMiningSession = null;
        }

        public static void StopAllMinersNonProfitable()
        {
            _curMiningSession?.StopAllMinersNonProfitable();
        }

        public static string GetActiveMinersGroup()
        {
            // if no session it is idle
            return _curMiningSession != null ? _curMiningSession.GetActiveMinersGroup() : "IDLE";
        }

        public static List<int> GetActiveMinersIndexes()
        {
            return _curMiningSession != null ? _curMiningSession.ActiveDeviceIndexes : new List<int>();
        }

        public static double GetTotalRate()
        {
            return _curMiningSession?.GetTotalRate() ?? 0;
        }
        public static double GetTotalPowerRate()
        {
            return _curMiningSession?.GetTotalPowerRate() ?? 0;
        }
        public static double GetTotalPower()
        {
            return _curMiningSession?.GetTotalPowerRate() ?? 0;
        }

        public static bool StartInitialize(IMainFormRatesComunication mainFormRatesComunication,
            string wallet, string password)
        {
            _curMiningSession = new MiningSession(ComputeDeviceManager.Available.Devices,
                mainFormRatesComunication, wallet, password);
            return _curMiningSession.IsMiningEnabled;
        }

        public static bool IsMiningEnabled()
        {
            return _curMiningSession != null && _curMiningSession.IsMiningEnabled;
        }


        /// <summary>
        /// SwichMostProfitable should check the best combination for most profit.
        /// Calculate profit for each supported algorithm per device and group.
        /// </summary>
        /// <param name="niceHashData"></param>
        //[Obsolete("Deprecated in favour of AlgorithmSwitchingManager timer")]
        //public static async Task SwichMostProfitableGroupUpMethod()
        //{
        //    if (_curMiningSession != null) await _curMiningSession.SwichMostProfitableGroupUpMethod();
        //}

        public static async Task MinerStatsCheck()
        {
            if (_curMiningSession != null) await _curMiningSession.MinerStatsCheck();
            //if (_curMiningSession != null) new Task(() => _curMiningSession.MinerStatsCheck()).Start();
        }
    }
}
