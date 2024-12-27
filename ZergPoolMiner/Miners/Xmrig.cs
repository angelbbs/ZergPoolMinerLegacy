using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZergPoolMiner.Algorithms;
using ZergPoolMiner.Configs;
using ZergPoolMiner.Miners.Parsing;
using ZergPoolMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZergPoolMiner.Miners
{
    public class Xmrig : Miner
    {
        [DllImport("psapi.dll")]
        public static extern bool EmptyWorkingSet(IntPtr hProcess);

        private int _benchmarkTimeWait = 180;
        private const string LookForStart = "speed 10s/60s/15m";
        private const string LookForEnd = "h/s max";
        private System.Diagnostics.Process CMDconfigHandle;
        private string platform = "";
        string platform_prefix = "";
        private double _power = 0.0d;
        double _powerUsage = 0;
        public Xmrig() : base("Xmrig")
        { }
        public override void Start(string wallet, string password)
        {
            LastCommandLine = GetStartCommand(wallet, password);

            ProcessHandle = _Start();
        }

        protected override string GetDevicesCommandString()
        {
            var deviceStringCommand = " ";
            if (platform == "")//cpu
            {
                return "";
            }
            var ids = MiningSetup.MiningPairs.Select(mPair => mPair.Device.ID.ToString()).ToList();
            deviceStringCommand += string.Join(",", ids);

            return deviceStringCommand;
        }

        private string GetStartCommand(string wallet, string password)
        {
            var extras = ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.CPU);
            foreach (var pair in MiningSetup.MiningPairs)
            {
                if (pair.Device.DeviceType == DeviceType.NVIDIA)
                {
                    platform = " --no-cpu --cuda-devices=";
                }
                else if (pair.Device.DeviceType == DeviceType.AMD)
                {
                    platform = " --no-cpu --opencl-devices=";
                }
                else if (pair.Device.DeviceType == DeviceType.CPU)
                {
                    platform = "";
                }
            }

            string _wallet = "-u " + wallet + ".xmrig" +
                ":" + password.Trim() + " "; 

            if (MiningSetup.CurrentAlgorithmType.Equals(AlgorithmType.RandomX))
            {
                return " --algo=rx/0 " + GetServer("randomx") + _wallet + extras + " --http-port " + ApiPort + 
               platform + " " + $"--http-port { ApiPort} " +
                GetDevicesCommandString().TrimStart();
            }
            if (MiningSetup.CurrentAlgorithmType.Equals(AlgorithmType.Ghostrider))
            {
                return " --algo=gr " + GetServer("ghostrider") + _wallet + extras + " --http-port " + ApiPort +
               platform + " " + $"--http-port { ApiPort} "+
               GetDevicesCommandString().TrimStart();
            }
            return "unsupported algo";
        }
        private string GetServer(string algo)
        {
            string ret = "";
            try
            {
                algo = algo.Replace("-", "_");
                var _a = Stats.Stats.MiningAlgorithmsList.FirstOrDefault(item => item.name.ToLower() == algo.ToLower());

                string serverUrl = Form_Main.regionList[ConfigManager.GeneralConfig.ServiceLocation].RegionLocation +
                    "mine.zergpool.com";
                ret = "-o " + Links.CheckDNS(algo + serverUrl).Replace("stratum+tcp://", "stratum+ssl://") + ":" + _a.tls_port.ToString();
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("GetServer", "Error in " + algo + " " + ex.ToString());
                ret = "error_in_list_of_algos.err:1111";
            }

            return ret + " ";
        }
        private string GetStartBenchmarkCommand()
        {
            foreach (var pair in MiningSetup.MiningPairs)
            {
                if (pair.Device.DeviceType == DeviceType.NVIDIA)
                {
                    platform = " --no-cpu --cuda-devices=";
                    platform_prefix = "nvidia_";
                }
                else if (pair.Device.DeviceType == DeviceType.AMD)
                {
                    platform = " --no-cpu --opencl-devices=";
                    platform_prefix = "amd_"; ;
                }
                else if (pair.Device.DeviceType == DeviceType.CPU)
                {
                    platform = "";
                    platform_prefix = "cpu_";
                }
            }

            string _algo = "";
            if (MiningSetup.CurrentAlgorithmType.Equals(AlgorithmType.RandomX))
            {
                _algo = "rx/0";
            }
            if (MiningSetup.CurrentAlgorithmType.Equals(AlgorithmType.Ghostrider))
            {
                _algo = "gr";
            }

            var extras = ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.CPU);
            string serverUrl = Form_Main.regionList[ConfigManager.GeneralConfig.ServiceLocation].RegionLocation +
                "mine.zergpool.com";
            var algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
            Stats.Stats.MiningAlgorithms _a = new();
            var pool = "";
            _a = Stats.Stats.MiningAlgorithmsList.FirstOrDefault(item => item.name.ToLower() == algo.ToLower());

            if (_a is object && _a != null)
            {
                pool = Links.CheckDNS(algo + serverUrl) + ":" + _a.port.ToString();
            }
            else
            {
                Helpers.ConsolePrint("XMRig", "Not found " + algo + " in MiningAlgorithmsList. Try fix it.");
                algo = algo.Replace("_", "-");
                _a = Stats.Stats.MiningAlgorithmsList.FirstOrDefault(item => item.name.ToLower() == algo.ToLower());
                pool = Links.CheckDNS(algo + serverUrl) + ":" + _a.port.ToString();
            }

            return " " + "-a " + _algo +
            $" -o {pool} -u {Globals.DemoUser} -p c=LTC" +
            $" --http-port {ApiPort} {extras}";
            return "unsupported algo";
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            Helpers.ConsolePrint("XMRIG", "_Stop");
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
        }

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 60 * 1000 * 5; // 5 min
        }

        protected async Task<ApiData> GetSummaryCpuAsyncXMRig(string method = "", bool overrideLoop = false)
        {
            ad = new ApiData(MiningSetup.CurrentAlgorithmType);
            var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();
            try
            {
                HttpWebRequest WR = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + ApiPort.ToString() + "/1/summary");
                WR.UserAgent = "GET / HTTP/1.1\r\n\r\n";
                WR.Timeout = 3 * 1000;
                WR.Credentials = CredentialCache.DefaultCredentials;
                WebResponse Response = WR.GetResponse();
                Stream SS = Response.GetResponseStream();
                SS.ReadTimeout = 2 * 1000;
                StreamReader Reader = new StreamReader(SS);
                var respStr = await Reader.ReadToEndAsync();

                Reader.Close();
                Response.Close();
                WR.Abort();
                SS.Close();
                //Helpers.ConsolePrint(MinerTag(), respStr);

                if (string.IsNullOrEmpty(respStr))
                {
                    CurrentMinerReadStatus = MinerApiReadStatus.NETWORK_EXCEPTION;
                    throw new Exception("Response is empty!");
                }

                dynamic resp = JsonConvert.DeserializeObject(respStr);

                if (resp != null)
                {
                    JArray totals = resp.hashrate.total;
                    foreach (var total in totals)
                    {
                        if (total.Value<string>() == null) continue;
                        ad.Speed = total.Value<double>();
                        break;
                    }
                    foreach (var dev in sortedMinerPairs)
                    {
                        dev.Device.MiningHashrate = ad.Speed;
                        _power = dev.Device.PowerUsage;
                    }

                    if (ad.Speed == 0)
                    {
                        CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                    }
                    else
                    {
                        CurrentMinerReadStatus = MinerApiReadStatus.GOT_READ;
                    }
                }
                else
                {
                    throw new Exception($"Response does not contain speed data: {respStr.Trim()}");
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint(MinerTag(), ex.Message);
            }

            return ad;
        }

        private ApiData ad;
        public override ApiData GetApiData()
        {
            return ad;
        }
        public override async Task<ApiData> GetSummaryAsync()
        {
            return await GetSummaryCpuAsyncXMRig();
        }

        protected override bool IsApiEof(byte third, byte second, byte last)
        {
            return third == 0x7d && second == 0xa && last == 0x7d;
        }

        #region Benchmark

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            _benchmarkTimeWait = time;
            return GetStartBenchmarkCommand();
        }
        
        protected override void ProcessBenchLinesAlternate(string[] lines)
        {
            // Xmrig reports 2.5s and 60s averages, so prefer to use 60s values for benchmark
            // but fall back on 2.5s values if 60s time isn't hit
            var twoSecTotal = 0d;
            var sixtySecTotal = 0d;
            var twoSecCount = 0;
            var sixtySecCount = 0;
            foreach (var line in lines)
            {
                BenchLines.Add(line);
                var lineLowered = line.ToLower();
                if (!lineLowered.Contains(LookForStart)) continue;
                var speeds = Regex.Match(lineLowered, $"{LookForStart} (.+?) {LookForEnd}").Groups[1].Value.Split();

                try
                {
                    if (double.TryParse(speeds[1], out var sixtySecSpeed))
                    {
                        sixtySecTotal += sixtySecSpeed;
                        ++sixtySecCount;
                    }
                    else if (double.TryParse(speeds[0], out var twoSecSpeed))
                    {
                        // Store 10s data in case 60s is never reached
                        twoSecTotal += twoSecSpeed;
                        ++twoSecCount;
                    }
                }
                catch
                {
                    MessageBox.Show("Unsupported miner version - " + MiningSetup.MinerPath,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    BenchmarkSignalFinnished = true;
                    return;
                }
            }

            if (sixtySecCount > 0 && sixtySecTotal > 0)
            {
                // Run iff 60s averages are reported
                BenchmarkAlgorithm.BenchmarkSpeed = sixtySecTotal / sixtySecCount;
            }
            else if (twoSecCount > 0)
            {
                // Run iff no 60s averages are reported but 2.5s are
                BenchmarkAlgorithm.BenchmarkSpeed = twoSecTotal / twoSecCount;
            }
        }

        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata)
        {
            CheckOutdata(outdata);
        }
        protected override bool BenchmarkParseLine(string outdata)
        {
            return true;
        }
        /*
        protected override bool BenchmarkParseLine(string outdata)
        {
            Helpers.ConsolePrint(MinerTag(), outdata);
            return false;
        }
        */
        #endregion
    }
}
