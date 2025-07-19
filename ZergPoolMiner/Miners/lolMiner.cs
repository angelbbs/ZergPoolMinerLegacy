using Newtonsoft.Json;
using ZergPoolMiner.Algorithms;
using ZergPoolMiner.Configs;
using ZergPoolMiner.Devices;
using ZergPoolMiner.Miners.Grouping;
using ZergPoolMiner.Miners.Parsing;
using ZergPoolMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static ZergPoolMiner.Devices.ComputeDeviceManager;
using ZergPoolMiner.Stats;

namespace ZergPoolMiner.Miners
{
    class lolMiner : Miner
    {
        private readonly int GPUPlatformNumber;
        Stopwatch _benchmarkTimer = new Stopwatch();
        private int _benchmarkTimeWait = 180;
        private double _power = 0.0d;
        double _powerUsage = 0;
        string platform = "";
        private int APIerrorsCount = 0;
        public lolMiner()
            : base("lolMiner")
        {
            GPUPlatformNumber = ComputeDeviceManager.Available.AmdOpenCLPlatformNum;
            IsKillAllUsedMinerProcs = true;
            IsNeverHideMiningWindow = true;

        }

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 60 * 1000;
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
        }
        /*
        private string GetServerDual(string algo, string algo2, string algo2pool, string btcAdress, string worker, string port, string port2)
        {
            string ret = "";
            string ssl = "";
            string dualssl = "";
            string psw = "x";
            string _worker = "";
            string _dualworker = "";
            if (ConfigManager.GeneralConfig.StaleProxy) psw = "stale";
            if (ConfigManager.GeneralConfig.ProxySSL)
            {
                port = "4" + port;
                port2 = "4" + port;
                ssl = "--tls on ";
                dualssl = "--tls on ";
            }
            else
            {
                port = "1" + port;
                port2 = "1" + port;
                ssl = "--tls off ";
                dualssl = "--tls off ";
            }
            if (worker != null)
            {
                _worker = " --worker " + worker;
                _dualworker = " --dualworker " + worker;
            }
            else
            {
                _worker = "";
                _dualworker = "";
            }
            foreach (string serverUrl in Globals.MiningLocation)
            {
                if (serverUrl.Contains("auto"))
                {
                    ret = ret + " --pool " + Links.CheckDNS(algo + "." + serverUrl).Replace("stratum+tcp://", "") + ":9200 --user " +
                        btcAdress + _worker + " --pass " + psw + " --tls off " +
                        " --dualmode " + algo2 + " --dualpool " + Links.CheckDNS(algo2pool + "." + serverUrl).Replace("stratum+tcp://", "") + ":9200 " + " --dualuser " +
                        btcAdress + _dualworker + " --dualpass " + psw + " --dualtls off ";
                    if (!ConfigManager.GeneralConfig.ProxyAsFailover) break;
                }
                else
                {
                    ret = ret + " --pool " + Links.CheckDNS(algo + "." + serverUrl).Replace("stratum+tcp://", "") + ":" + port + " --user " +
                        btcAdress + _worker + " --pass " + psw + " " + ssl +
                        " --dualmode " + algo2 + " --dualpool " + Links.CheckDNS(algo2pool + "." + serverUrl).Replace("stratum+tcp://", "") + ":" + port2 + " --dualuser " +
                        btcAdress + _dualworker + " --dualpass " + psw + " " + dualssl;
                }
            }
            return ret;
        }
        */
        public override void Start(string wallet, string password)
        {
            string url = "";
            if (!IsInit)
            {
                Helpers.ConsolePrint(MinerTag(), "MiningSetup is not initialized exiting Start()");
                return;
            }
            var param = "";
            foreach (var pair in MiningSetup.MiningPairs)
            {
                if (pair.Device.DeviceType == DeviceType.NVIDIA)
                {
                    platform = "nvidia";
                    param = ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.NVIDIA).Trim();
                }
                if (pair.Device.DeviceType == DeviceType.AMD)
                {
                    platform = "amd";
                    param = ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.AMD).Trim();
                }
                if (pair.Device.DeviceType == DeviceType.INTEL)
                {
                    platform = "intel";
                    param = ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.INTEL).Trim();
                }
            }

            IsApiReadException = false;

            string _password = " --pass=" + password.Trim() + " ";
            var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
            _algo = _algo.Replace("equihash125", "-a FLUX");
            _algo = _algo.Replace("equihash144", "--coin AUTO144_5");
            _algo = _algo.Replace("equihash192", "--coin AUTO192_7");
            _algo = _algo.Replace("karlsenhashv2", "-a KARLSENV2");
            _algo = _algo.Replace("karlsenhash", "-a KARLSEN");
            _algo = _algo.Replace("pyrinhashv2", "-a PYRINV2");
            _algo = _algo.Replace("sha512256d", "-a RADIANT");
            _algo = _algo.Replace("nexapow", "-a NEXA");
            _algo = _algo.Replace("hoohash", "-a hoohash");
            _algo = _algo.Replace("ethash", "-a ETHASH");

            string proxy = "";
            if (ConfigManager.GeneralConfig.EnableProxy)
            {
                //proxy = "--socks5 " + Stats.Stats.CurrentProxyIP + ":" + Stats.Stats.CurrentProxySocks5SPort + " ";
                proxy = "--socks5 127.0.0.1:" + Socks5Relay.Port;
            }

            if (MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.NONE)
            {
                
                LastCommandLine = _algo +
                    " --tls on -p " + GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower()) +
                    " -u " + wallet + _password + " " + proxy +
                   " --apiport " + ApiPort + 
                   " --devices " + GetDevicesCommandString().Trim();
            } else
            {
                LastCommandLine = _algo +
                    " --tls on -p " + GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower()) +
                    " -u " + ConfigManager.GeneralConfig.Wallet + _password +
                    "--dualmode PYRINV2DUAL " +
                    " --dualpool " + GetServer(MiningSetup.CurrentSecondaryAlgorithmType.ToString().ToLower()) +
                    " --dualuser " + ConfigManager.GeneralConfig.Wallet + 
                    " --dualpass c=" + ConfigManager.GeneralConfig.PayoutCurrency + Form_Main._PayoutTreshold +
                    ",ID=" + Miner.GetFullWorkerName() +
                   " --devices " + GetDevicesCommandString().Trim() +
                   " --apiport " + ApiPort;
            }
            
            string sColor = "";
            if (Form_Main.GetWinVer(Environment.OSVersion.Version) < 8)
            {
                sColor = " --nocolor";
            }
            LastCommandLine += sColor;
            ProcessHandle = _Start();
        }


        #region Decoupled benchmarking routines

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            var CommandLine = "";
            var param = "";
            foreach (var pair in MiningSetup.MiningPairs)
            {
                if (pair.Device.DeviceType == DeviceType.NVIDIA)
                {
                    platform = "nvidia";
                    param = " " + ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.NVIDIA).Trim();
                }
                if (pair.Device.DeviceType == DeviceType.AMD)
                {
                    platform = "amd";
                    param = " " + ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.AMD).Trim();
                }
                if (pair.Device.DeviceType == DeviceType.INTEL)
                {
                    platform = "intel";
                    param = " " + ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.INTEL).Trim();
                }
            }
            // demo for benchmark
            var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
            _algo = _algo.Replace("equihash125", "-a FLUX");
            _algo = _algo.Replace("equihash144", "--coin AUTO144_5");
            _algo = _algo.Replace("equihash192", "--coin AUTO192_7");
            _algo = _algo.Replace("karlsenhashv2", "-a KARLSENV2");
            _algo = _algo.Replace("karlsenhash", "-a KARLSEN");
            _algo = _algo.Replace("pyrinhashv2", "-a PYRINV2");
            _algo = _algo.Replace("sha512256d", "-a RADIANT");
            _algo = _algo.Replace("nexapow", "-a NEXA");
            _algo = _algo.Replace("ethash", "-a ETHASH");

            string proxy = "";
            if (ConfigManager.GeneralConfig.EnableProxy)
            {
                //proxy = "--proxy " + Stats.Stats.CurrentProxyIP + ":" + Stats.Stats.CurrentProxySocks5SPort + " ";
                proxy = "--socks5 127.0.0.1:" + Socks5Relay.Port;
            }

            string failover = "";
            switch (MiningSetup.CurrentAlgorithmType)
            {
                case AlgorithmType.KarlsenHashV2:
                    failover = $" -p kls.2miners.com:12020 -u bc1qun08kg08wwdsszrymg8z4la5d6ygckg9nxh4pq --pass x ";
                    break;
                case AlgorithmType.NexaPow:
                    failover = $" -p nexa.2miners.com:15050 -u bc1qun08kg08wwdsszrymg8z4la5d6ygckg9nxh4pq --pass x ";
                    break;
                case AlgorithmType.Equihash125:
                    failover = $" -p flux.2miners.com:19090 -u t1e4GBC9UUZVaJSeML9HgrKbJUm61GQ3Y8q --pass x ";
                    break;
                case AlgorithmType.Equihash144:
                    failover = $" -p equihash125.eu.mine.zpool.ca:12125 -u LPeihdgf7JRQUNq5cwZbBQQgEmh1m7DSgH --pass c=LTC ";
                    break;
                default:
                    break;
            }

            if (MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.NONE)
            {
                CommandLine = _algo +
                    " --tls on -p " + GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower()) + " " +
                " -u " + Globals.DemoUser +
                " --pass c=LTC " +
                " " + failover +
                proxy +
                " --apiport " + ApiPort +
                " --devices " + GetDevicesCommandString().Trim();
            }
            /*
            else if (MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.PyrinHashV2)
            {
                CommandLine = _algo +
                    " -p " + GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower()) +
                    " -u " + Globals.DemoUser +
                    " --pass=c=LTC" +
                    " --dualmode PYRINV2DUAL " +
                    " --dualpool " + GetServer(MiningSetup.CurrentSecondaryAlgorithmType.ToString().ToLower()) +
                    " --dualuser " + Globals.DemoUser +
                    " --dualpass c=LTC" +
                    " --devices " + GetDevicesCommandString().Trim() +
                    " --apiport " + ApiPort;
            }
            */
            //duals
            /*
            if (MiningSetup.CurrentAlgorithmType == AlgorithmType.FishHash && MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.Alephium)
            {
                CommandLine = "--algo FISHHASH " +
                                " --pool " + Links.CheckDNS("ru.ironfish.herominers.com:1145").Replace("stratum+tcp://", "") + " --user fb8aaaf8594143a4007c9fe0e0056bd3ca55848d0f5247f7eee8918ca8345521.lolMiner --pass x" +
                                " --dualmode ALEPHDUAL --dualpool " + Links.CheckDNS("ru.alephium.herominers.com:1199").Replace("stratum + tcp://", "") + " --dualuser 12bjcHBTbdqW3zfDc84qq8z6RNZr33oXgqqaYdZRUD5qC.lolMiner --dualpass x" +
                                              param +
                                " --devices ";
            }
            */
            _benchmarkTimeWait = time;
            string sColor = "";
            if (Form_Main.GetWinVer(Environment.OSVersion.Version) < 8)
            {
                sColor = " --nocolor";
            }
            CommandLine += sColor;
            return CommandLine;

        }

        protected override string GetDevicesCommandString()
        {
            ad = new ApiData(MiningSetup.CurrentAlgorithmType, MiningSetup.CurrentSecondaryAlgorithmType, MiningSetup.MiningPairs[0]);
            ad.ThirdAlgorithmID = AlgorithmType.NONE;

            var deviceStringCommand = " ";
            var ids = new List<string>();
            var amdDeviceCount = ComputeDeviceManager.Query.AmdDevices.Count;
            var intelDeviceCount = ComputeDeviceManager.Query.IntelDevices.Count;
            var allDeviceCount = ComputeDeviceManager.Query.GpuCount;
            Helpers.ConsolePrint("lolMinerIndexing", $"Found {allDeviceCount} Total GPU devices");
            Helpers.ConsolePrint("lolMinerIndexing", $"Found {amdDeviceCount} AMD devices");
            Helpers.ConsolePrint("lolMinerIndexing", $"Found {intelDeviceCount} INTEL devices");
            //   var ids = MiningSetup.MiningPairs.Select(mPair => mPair.Device.ID.ToString()).ToList();
            //var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.DeviceType).ToList();
            var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.lolMinerBusID).ToList();
            foreach (var mPair in sortedMinerPairs)
            {
                //список карт выводить --devices 999
                //double id = mPair.Device.IDByBus + allDeviceCount - amdDeviceCount;
                int id = (int)mPair.Device.lolMinerBusID;

                if (id < 0)
                {
                    Helpers.ConsolePrint("lolMinerIndexing", "ID too low: " + id + " skipping device");
                    continue;
                }
                /*
                if (mPair.Device.DeviceType == DeviceType.NVIDIA)
                {
                    Helpers.ConsolePrint("lolMinerIndexing", "NVIDIA found. Increasing index");
                    id ++;
                }
                */
                Helpers.ConsolePrint("lolMinerIndexing", "Minind ID: " + id);
                {
                    ids.Add(id.ToString());
                }

            }


            deviceStringCommand += string.Join(",", ids);

            return deviceStringCommand + " --watchdog exit ";
        }
        protected override bool BenchmarkParseLine(string outdata)
        {
            return true;
        }


        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata)
        {
            CheckOutdata(outdata);
        }


        #endregion // Decoupled benchmarking routines

        public class lolResponse
        {
            public List<lolGpuResult> result { get; set; }
        }

        public class lolGpuResult
        {
            public double sol_ps { get; set; } = 0;
        }
        // TODO _currentMinerReadStatus

        private ApiData ad;
        public override ApiData GetApiData()
        {
            return ad;
        }

        private int[] errors = new int[0];
        public override async Task<ApiData> GetSummaryAsync()
        {
            CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
            string ResponseFromlolMiner;
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
                ResponseFromlolMiner = await Reader.ReadToEndAsync();
                //Helpers.ConsolePrint("API: ", ResponseFromlolMiner);
                //if (ResponseFromlolMiner.Length == 0 || (ResponseFromlolMiner[0] != '{' && ResponseFromlolMiner[0] != '['))
                //    throw new Exception("Not JSON!");
                Reader.Close();
                Response.Close();
            }
            catch (Exception)
            {
                return null;
            }

            if (ResponseFromlolMiner == null)
            {
                CurrentMinerReadStatus = MinerApiReadStatus.NONE;
                return null;
            }
            try
            {
                dynamic resp = JsonConvert.DeserializeObject(ResponseFromlolMiner.Replace("\"-nan(ind)\"", "0.0"));
                //Helpers.ConsolePrint("->", ResponseFromlolMiner);
                int mult = 1;
                if (resp != null)
                {
                    int Num_Workers = resp.Num_Workers;
                    if (errors.Length != Num_Workers)
                    {
                        Array.Resize<int>(ref errors, Num_Workers);
                    }
                    if (Num_Workers == 0) return null;
                    int Num_Algorithms = resp.Num_Algorithms;
                    //Helpers.ConsolePrint("API: ", "Num_Workers: " + Num_Workers.ToString());
                    //Helpers.ConsolePrint("API: ", "Num_Algorithms: " + Num_Algorithms.ToString());
                    double[] Total_Performance = new double[Num_Algorithms];
                    double[] Total_Performance2 = new double[Num_Algorithms];
                    double[] hashrates = new double[Num_Workers];
                    double[] hashrates2 = new double[Num_Workers];
                    double totals = 0.0d;
                    double totals2 = 0.0d;
                    if (Num_Algorithms == 1)//single
                    {
                        for (int alg = 0; alg < Num_Algorithms; alg++)
                        {
                            string Algorithm = resp.Algorithms[alg].Algorithm;
                            Total_Performance[alg] = resp.Algorithms[alg].Total_Performance * resp.Algorithms[alg].Performance_Factor;
                            //Helpers.ConsolePrint("API: ", "Algorithm: " + resp.Algorithms[alg].Algorithm);
                            //Helpers.ConsolePrint("API: ", "Total_Performance: " + Total_Performance[alg].ToString());
                            totals = Total_Performance[alg];
                            for (int w = 0; w < Num_Workers; w++)
                            {
                                hashrates[w] = resp.Algorithms[alg].Worker_Performance[w] * resp.Algorithms[alg].Performance_Factor;
                                //Helpers.ConsolePrint("API: ", "hashrates: " + hashrates[w].ToString());
                            }
                        }
                        //ad.SecondaryAlgorithmID = AlgorithmType.NONE;
                    }
                    else //duals
                    {
                        for (int alg = 0; alg < Num_Algorithms; alg++)
                        {
                            string Algorithm = resp.Algorithms[alg].Algorithm;
                            if (Algorithm.ToLower().Contains("karlsenhash"))
                            {
                                Total_Performance[alg] = resp.Algorithms[alg].Total_Performance * resp.Algorithms[alg].Performance_Factor;
                                //Helpers.ConsolePrint("API: ", "Algorithm: " + resp.Algorithms[alg].Algorithm);
                                //Helpers.ConsolePrint("API: ", "Total_Performance: " + Total_Performance[alg].ToString());
                                totals = Total_Performance[alg];
                                for (int w = 0; w < Num_Workers; w++)
                                {
                                    hashrates[w] = resp.Algorithms[alg].Worker_Performance[w] * resp.Algorithms[alg].Performance_Factor;
                                }
                            }
                            
                            if (Algorithm.ToLower().Contains("heavyhash-pyrin"))
                            {
                                Total_Performance2[alg] = resp.Algorithms[alg].Total_Performance * resp.Algorithms[alg].Performance_Factor;
                                //Helpers.ConsolePrint("API: ", "Algorithm: " + resp.Algorithms[alg].Algorithm);
                                //Helpers.ConsolePrint("API: ", "Total_Performance: " + Total_Performance[alg].ToString());
                                totals2 = Total_Performance2[alg];
                                for (int w = 0; w < Num_Workers; w++)
                                {
                                    hashrates2[w] = resp.Algorithms[alg].Worker_Performance[w] * resp.Algorithms[alg].Performance_Factor;
                                }
                                ad.SecondaryAlgorithmID = MiningSetup.CurrentSecondaryAlgorithmType;
                            }
                            
                        }
                        ad.ThirdAlgorithmID = AlgorithmType.NONE;
                    }

                    ad.Speed = totals;
                    ad.SecondarySpeed = totals2;
                    if (Num_Workers > 0)
                    {
                        int dev = 0;
                        var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.BusID).ToList();

                        if (Form_Main.NVIDIA_orderBug)
                        {
                            sortedMinerPairs.Sort((a, b) => a.Device.ID.CompareTo(b.Device.ID));
                        }
                        foreach (var mPair in sortedMinerPairs)
                        {
                            _power = mPair.Device.PowerUsage;
                            mPair.Device.MiningHashrate = hashrates[dev];
                            mPair.Device.MiningHashrateSecond = hashrates2[dev];

                            if (Num_Algorithms == 1 && hashrates[dev] == 0)
                            {
                                //CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                                //Helpers.ConsolePrint("lolMiner API", "Device " + dev.ToString() + " zero hashrate");
                                errors[dev]++;
                            }
                            if (Num_Algorithms == 2 && hashrates[dev] == 0)
                            {
                                //CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                                //Helpers.ConsolePrint("lolMiner API", "Device " + dev.ToString() + " zero main hashrate");
                                errors[dev]++;
                            }
                            if (Num_Algorithms == 2 && hashrates2[dev] == 0)
                            {
                                //CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                                //Helpers.ConsolePrint("lolMiner API", "Device " + dev.ToString() + " zero second hashrate");
                                errors[dev]++;
                            }

                            if (Num_Algorithms == 1 && hashrates[dev] != 0)
                            {
                                CurrentMinerReadStatus = MinerApiReadStatus.GOT_READ;
                                errors[dev] = 0;
                            }
                            if (Num_Algorithms == 2 && hashrates[dev] != 0 && hashrates2[dev] != 0)
                            {
                                CurrentMinerReadStatus = MinerApiReadStatus.GOT_READ;
                                errors[dev] = 0;
                            }
                            dev++;
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint(MinerTag(), e.ToString());
            }
            /*
            foreach (var d in errors)
            {
                if (d >= 100)
                {
                    Helpers.ConsolePrint(MinerTag(), "Too many API errors. Need Restarting miner");
                    CurrentMinerReadStatus = MinerApiReadStatus.RESTART;
                    ad.Speed = 0;
                    ad.SecondarySpeed = 0;
                    ad.ThirdSpeed = 0;
                    return ad;
                }
            }
            */
            Thread.Sleep(100);
            return ad;
        }
    }
}
