using System;
using System.Collections.Generic;

namespace ZergPoolMiner.Configs.Data
{
    [Serializable]
    public class DeviceBenchmarkConfig
    {
        public string DeviceUUID = "";
        public string DeviceName = "";
        public int BusID = 0;
        //public int TimeLimit { get; set; }
        public List<AlgorithmConfig> AlgorithmSettings = new List<AlgorithmConfig>();
        public List<DualAlgorithmConfig> DualAlgorithmSettings = new List<DualAlgorithmConfig>();
    }
}
