using ZergPoolMiner.Algorithms;
using ZergPoolMiner.Configs;
using ZergPoolMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;

namespace ZergPoolMiner.Miners
{
    public class ClaymoreNeoscryptMiner : ClaymoreBaseMiner
    {
        public ClaymoreNeoscryptMiner()
            : base("ClaymoreNeoscryptMiner")
        {
            LookForStart = "ns - total speed:";
        }

        
        public override void Start(string wallet, string password)
        {
            LastCommandLine = " " + GetDevicesCommandString() + " -mport -" + ApiPort + " -pool " +
                GetServer("neoscrypt").Replace("stratum+ssl://", "stratum+tcp://").Replace("14233", "4233") +
                " -wal " + wallet + " -psw " + password + " -dbg -1 -ftime 10 -retrydelay 5";

            ProcessHandle = _Start();
        }

        // benchmark stuff
        protected override bool BenchmarkParseLine(string outdata)
        {
            return true;
        }
        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            BenchmarkAlgorithm.DeviceType = DeviceType.AMD;
            BenchmarkTimeWait = time;
            // demo for benchmark
            return $" {GetDevicesCommandString()} -mport -{ApiPort} -pool " + Links.CheckDNS("stratum+tcp://neoscrypt.mine.zergpool.com") + ":4233 -wal " + Globals.DemoUser + " -psw c=LTC -logfile " + GetLogFileName();
        }

    }
}
