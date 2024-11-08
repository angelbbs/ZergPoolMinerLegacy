using ZergPoolMiner.Algorithms;
using ZergPoolMiner.Devices;

namespace ZergPoolMiner.Interfaces
{
    public interface IBenchmarkForm
    {
        bool InBenchmark { get; }

        void SetCurrentStatus(ComputeDevice device, Algorithm algorithm, string status);
        void AddToStatusCheck(ComputeDevice device, Algorithm algorithm);
        void RemoveFromStatusCheck(ComputeDevice device, Algorithm algorithm);
        void EndBenchmarkForDevice(ComputeDevice device, bool failedAlgos);
        void StepUpBenchmarkStepProgress();
    }
}
