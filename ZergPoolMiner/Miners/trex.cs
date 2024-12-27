using Newtonsoft.Json;
using ZergPoolMiner.Algorithms;
using ZergPoolMiner.Configs;
using ZergPoolMiner.Miners.Grouping;
using ZergPoolMiner.Miners.Parsing;
using ZergPoolMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ZergPoolMiner.Miners
{
    public class trex : Miner
    {
        private int _benchmarkTimeWait = 60;
        private const int TotalDelim = 2;
        private double _power = 0.0d;
        double _powerUsage = 0;
        public FileStream fs;
        private int offset = 0;
        private bool _isDual = false;
        public trex() : base("trex")
        {
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
                ret = " -o " + Links.CheckDNS(algo + serverUrl).Replace("stratum+tcp://", "stratum2+ssl://") + ":" + _a.tls_port.ToString();
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("GetServer", "Error in " + algo + " " + ex.ToString());
                ret = "error_in_list_of_algos.err:1111";
            }
            return ret;
        }
        public override void Start(string wallet, string password)
        {
            if (!IsInit)
            {
                Helpers.ConsolePrint(MinerTag(), "MiningSetup is not initialized exiting Start()");
                return;
            }

            var apiBind = " --api-bind-http 0.0.0.0:" + ApiPort;

            string _wallet = "-u " + wallet;
            string _password = " -p " + password + " ";
            var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();

            LastCommandLine = " --algo " + _algo +
            " " + apiBind +
                    GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower()) + " " +
                    _wallet + " " + _password +
                    " -d " + GetDevicesCommandString() + " --no-watchdog " +
                ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.NVIDIA) + " ";

            
            IsApiReadException = false;

            ProcessHandle = _Start();
        }

        // benchmark stuff
        protected void KillMinerBase(string exeName)
        {
            foreach (var process in Process.GetProcessesByName(exeName))
            {
                try
                {
                    Thread.Sleep(1000);
                    process.Kill();
                }
                catch (Exception e) { Helpers.ConsolePrint(MinerDeviceName, e.ToString()); }
            }
        }

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            var commandLine = "";
            string logFile = algorithm.ZergPoolID.ToString() + GetLogFileName();
            _benchmarkTimeWait = 40;
            try
            {
                if (File.Exists(logFile)) File.Delete(logFile);
            } catch (Exception ex)
            {
                Helpers.ConsolePrint("BenchmarkCreateCommandLine", ex.ToString());
            }

            if (algorithm.ZergPoolID.Equals(AlgorithmType.X16RV2))
            {
                commandLine = "--algo x16rv2 --benchmark" +
                " --gpu-report-interval 1 --no-watchdog --api-bind-http 127.0.0.1:" + ApiPort +
                              " -d ";
                commandLine += GetDevicesCommandString() + " -l " + logFile;
                _benchmarkTimeWait = time;
                return commandLine;
            }
            if (algorithm.ZergPoolID.Equals(AlgorithmType.X21S))
            {
                commandLine = "--algo x21s --benchmark" +
                " --gpu-report-interval 1 --no-watchdog --api-bind-http 127.0.0.1:" + ApiPort +
                              " -d ";
                commandLine += GetDevicesCommandString() + " -l " + logFile;
                _benchmarkTimeWait = time;
                return commandLine;
            }
            if (algorithm.ZergPoolID.Equals(AlgorithmType.X25X))
            {
                commandLine = "--algo x25x --benchmark" +
                " --gpu-report-interval 1 --no-watchdog --api-bind-http 127.0.0.1:" + ApiPort +
                              " -d ";
                commandLine += GetDevicesCommandString() + " -l " + logFile;
                _benchmarkTimeWait = time;
                return commandLine;
            }

            var apiBind = " --api-bind-http 0.0.0.0:" + ApiPort;

            string wallet = "-u " + Globals.DemoUser;
            string password = " -p c=LTC" + " ";
            var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();

            commandLine = " --algo " + _algo +
            " " + apiBind +
                    GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower()) + " " +
                    wallet + " " + password +
                    " -d " + GetDevicesCommandString() + " --no-watchdog " +
                ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.NVIDIA) + " ";

            return commandLine;
        }

        private int count = 0;
        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata)
        {
            if (count >= 5) return;
            double logSpeed = 0.0d;
            if ((MiningSetup.CurrentAlgorithmType.Equals(AlgorithmType.X16RV2) ||
                MiningSetup.CurrentAlgorithmType.Equals(AlgorithmType.X21S) ||
                MiningSetup.CurrentAlgorithmType.Equals(AlgorithmType.X25X)))
                {
                string strStart = "Total:";
                if (outdata.Contains(strStart) && outdata.Contains("H/s"))
                {
                    var speedStart = outdata.IndexOf(strStart);
                    var speed = outdata.Substring(speedStart + strStart.Length, 6);
                    speed = speed.Replace(strStart, "");
                    speed = speed.Replace(" ", "");
                    double.TryParse(speed, out logSpeed);
                    if (outdata.Contains("kH/s")) logSpeed = logSpeed * 1000;
                    if (outdata.Contains("MH/s")) logSpeed = logSpeed * 1000 * 1000;
                    if (outdata.Contains("GH/s")) logSpeed = logSpeed * 1000 * 1000 * 1000;
                    Helpers.ConsolePrint("logSpeed", logSpeed.ToString());
                    count++;
                    BenchmarkAlgorithm.BenchmarkProgressPercent = count * 20;
                }
                if (count >= 5)
                {
                    BenchmarkAlgorithm.BenchmarkProgressPercent = 100;
                    this.BenchmarkAlgorithm.BenchmarkSpeed = logSpeed;
                    this.BenchmarkAlgorithm.PowerUsageBenchmark = MiningSetup.MiningPairs.Select(mPair => mPair.Device.PowerUsage).FirstOrDefault();
                    this.BenchmarkSignalFinnished = true;
                    this.BenchmarkHandle.Kill();
                    this.OnBenchmarkCompleteCalled = false;
                }
            }

            CheckOutdata(outdata);
        }
        protected override bool BenchmarkParseLine(string outdata)
        {
            return true;
        }

        private ApiData ad;
        public override ApiData GetApiData()
        {
            return ad;
        }
        public override async Task<ApiData> GetSummaryAsync()
        {
            CurrentMinerReadStatus = MinerApiReadStatus.NONE;
            string resp = null;
            var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();
            if (Form_Main.NVIDIA_orderBug)
            {
                sortedMinerPairs.Sort((a, b) => a.Device.ID.CompareTo(b.Device.ID));
            }
            try
            {
                HttpWebRequest WR = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + ApiPort.ToString() + "/summary");
                WR.UserAgent = "GET / HTTP/1.1\r\n\r\n";
                WR.Timeout = 3 * 1000;
                WR.Credentials = CredentialCache.DefaultCredentials;
                WebResponse Response = WR.GetResponse();
                Stream SS = Response.GetResponseStream();
                SS.ReadTimeout = 2 * 1000;
                StreamReader Reader = new StreamReader(SS);
                resp = await Reader.ReadToEndAsync();

                Reader.Close();
                Response.Close();
                WR.Abort();
                SS.Close();
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("API", ex.Message);
                return null;
            }

            ad = new ApiData(MiningSetup.CurrentAlgorithmType, MiningSetup.CurrentSecondaryAlgorithmType);

            if (resp != null)
            {
                //Helpers.ConsolePrint(MinerTag(), "API: " + resp);
                try
                {
                    dynamic respJson = JsonConvert.DeserializeObject(resp);
                    int devs = 0;
                    double HashrateSecondTotal = 0.0d;
                    foreach (var dev in respJson.gpus)
                    {
                        sortedMinerPairs[devs].Device.MiningHashrate = dev.hashrate;
                        //Helpers.ConsolePrint("********", "API device_id: " + dev.device_id + " gpu_id: " + dev.gpu_id + " gpu_user_id: " + " hashrate1: " + dev.hashrate);
                        _power = sortedMinerPairs[devs].Device.PowerUsage;
                        devs++;
                    }

                    devs = 0;
                    if (_isDual)
                    {
                        foreach (var dev in respJson.dual_stat.gpus)
                        {
                            //Helpers.ConsolePrint("********", "API device_id: " + dev.device_id + " gpu_id: " + dev.gpu_id + " gpu_user_id: " + " hashrate: " + dev.hashrate);
                            sortedMinerPairs[devs].Device.MiningHashrateSecond = dev.hashrate;
                            HashrateSecondTotal += (double)dev.hashrate;

                            devs++;
                        }
                    }
                    //Helpers.ConsolePrint(MinerTag(), "API total: " + respJson.hashrate);
                    ad.Speed = respJson.hashrate;
                    ad.SecondarySpeed = HashrateSecondTotal;
                }
                catch (Exception ex)
                {
                    Helpers.ConsolePrint("API eror", ex.Message);
                    return null;
                }

                if (ad.Speed == 0)
                {
                    CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                }
                else
                {
                    CurrentMinerReadStatus = MinerApiReadStatus.GOT_READ;
                }

                if (ad.Speed < 0)
                {
                    Helpers.ConsolePrint(MinerTag(), "Reporting negative speeds will restart...");
                    CurrentMinerReadStatus = MinerApiReadStatus.RESTART;
                    ad.Speed = 0;
                    ad.SecondarySpeed = 0;
                    ad.ThirdSpeed = 0;
                    return ad;
                }
            }
            else
            {
                Thread.Sleep(1);
            }

            return ad;
        }


        protected override void _Stop(MinerStopType willswitch)
        {
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
            if (ProcessHandle != null)
            {
                if (!ConfigManager.GeneralConfig.NoForceTRexClose)
                {
                    Thread.Sleep(500);
                    Helpers.ConsolePrint(MinerTag(), ProcessTag() + " Try force killing miner!");
                    try { KillMinerBase("t-rex"); }
                    catch { }
                }
            }
        }

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 60 * 1000 * 5; // 5 minute max, whole waiting time 75seconds
        }
    }
}
