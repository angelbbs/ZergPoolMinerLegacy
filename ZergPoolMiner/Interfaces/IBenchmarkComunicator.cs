namespace ZergPoolMiner.Interfaces
{
    public interface IBenchmarkComunicator
    {
        void OnBenchmarkComplete(bool success, string status);
    }
}
