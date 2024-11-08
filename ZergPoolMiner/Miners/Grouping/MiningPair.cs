using ZergPoolMiner.Algorithms;
using ZergPoolMiner.Devices;

namespace ZergPoolMiner.Miners.Grouping
{
    public class MiningPair
    {
        public readonly ComputeDevice Device;
        public readonly Algorithm Algorithm;
        public string CurrentExtraLaunchParameters;

        public MiningPair(ComputeDevice d, Algorithm a)
        {
            Device = d;
            Algorithm = a;
            CurrentExtraLaunchParameters = Algorithm.ExtraLaunchParameters;
        }
    }
}
