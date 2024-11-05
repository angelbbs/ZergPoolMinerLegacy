using ZergPoolMiner.Algorithms;
using ZergPoolMiner.Devices;
using ZergPoolMinerLegacy.Common.Enums;

namespace ZergPoolMiner.Miners
{
    public static class MinerFactory
    {
        private static Miner CreateClaymore(Algorithm algorithm)
        {
            switch (algorithm.ZergPoolID)
            {
                case AlgorithmType.NeoScrypt:
                    return new ClaymoreNeoscryptMiner();
            }

            return null;
        }

        public static Miner CreateMiner(DeviceType deviceType, Algorithm algorithm)
        {
            switch (algorithm.MinerBaseType)
            {
                case MinerBaseType.Claymore:
                    return CreateClaymore(algorithm);
                case MinerBaseType.Xmrig:
                    return new Xmrig();
                case MinerBaseType.SRBMiner:
                    return new SRBMiner();
                case MinerBaseType.CryptoDredge:
                    return new CryptoDredge();
                case MinerBaseType.trex:
                    return new trex();
                case MinerBaseType.teamredminer:
                    return new teamredminer();
                case MinerBaseType.GMiner:
                    return new GMiner(algorithm.SecondaryZergPoolID);
                case MinerBaseType.lolMiner:
                    return new lolMiner();
                case MinerBaseType.miniZ:
                    return new miniZ();
                case MinerBaseType.Nanominer:
                    return new Nanominer();
                case MinerBaseType.Rigel:
                    return new Rigel();
            }

            return null;
        }

        // create miner creates new miners based on device type and algorithm/miner path
        public static Miner CreateMiner(ComputeDevice device, Algorithm algorithm)
        {
            if (device != null && algorithm != null)
            {
                return CreateMiner(device.DeviceType, algorithm);
            }

            return null;
        }
    }
}
