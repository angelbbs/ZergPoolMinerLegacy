using ZergPoolMinerLegacy.Common.Enums;

namespace ZergPoolMiner.Miners.Grouping
{
    public static class GroupingLogic
    {
        public static bool ShouldGroup(MiningPair a, MiningPair b)
        {
            var canGroup = IsGroupableMinerBaseType(a) && IsGroupableMinerBaseType(b);
            // group if same bin path and same algo type
            if (canGroup && IsSameBinPath(a, b) && IsSameAlgorithmType(a, b) &&
                ((IsNotCpuGroups(a, b) && IsSameDeviceType(a, b))))
                return true;
            return false;
        }

        private static bool IsNotCpuGroups(MiningPair a, MiningPair b)
        {
            return a.Device.DeviceType != DeviceType.CPU && b.Device.DeviceType != DeviceType.CPU;
        }

        private static bool IsSameBinPath(MiningPair a, MiningPair b)
        {
            return a.Algorithm.MinerBinaryPath == b.Algorithm.MinerBinaryPath;
        }
        private static bool IsSameAlgorithmType(MiningPair a, MiningPair b)
        {
            return a.Algorithm.DualZergPoolID == b.Algorithm.DualZergPoolID;
        }

        private static bool IsSameDeviceType(MiningPair a, MiningPair b)
        {
            //
            return a.Device.DeviceType == b.Device.DeviceType;
        }
        private static bool IsGroupableMinerBaseType(MiningPair a)
        {
            return true;
        }
    }
}
