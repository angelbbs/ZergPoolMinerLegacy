using ZergPoolMinerLegacy.Common.Enums;
using System.Collections.Generic;

namespace ZergPoolMiner.Miners.Grouping
{
    public class MiningSetup
    {
        public List<MiningPair> MiningPairs { get; }
        public string MinerPath { get; }
        public string MinerName { get; }
        public string AlgorithmName { get; }
        public DeviceType DeviceType { get; }
        public AlgorithmType CurrentAlgorithmType { get; }
        public AlgorithmType CurrentSecondaryAlgorithmType { get; }
        public bool IsInit { get; }

        public MiningSetup(List<MiningPair> miningPairs)
        {
            IsInit = false;
            CurrentAlgorithmType = AlgorithmType.NONE;
            if (miningPairs == null || miningPairs.Count <= 0) return;
            MiningPairs = miningPairs;
            MiningPairs.Sort((a, b) => a.Device.ID - b.Device.ID);
            MinerName = miningPairs[0].Algorithm.MinerBaseTypeName;
            AlgorithmName = miningPairs[0].Algorithm.AlgorithmName;
            CurrentAlgorithmType = miningPairs[0].Algorithm.ZergPoolID;
            DeviceType = miningPairs[0].Algorithm.DeviceType;
            CurrentSecondaryAlgorithmType = miningPairs[0].Algorithm.SecondaryZergPoolID;
            MinerPath = miningPairs[0].Algorithm.MinerBinaryPath;
            IsInit = MinerPaths.IsValidMinerPath(MinerPath);
        }
    }
}
