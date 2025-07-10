using Newtonsoft.Json;
using ZergPoolMiner.Algorithms;
using ZergPoolMiner.Configs;
using ZergPoolMiner.Forms;
using ZergPoolMiner.Miners.Grouping;
using ZergPoolMiner.Miners.Parsing;
using ZergPoolMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZergPoolMiner.Stats;

namespace ZergPoolMiner.Miners
{
    public class GMiner : Miner
    {
        private int _benchmarkTimeWait;
        private const string LookForStart = " c ";
        private const string LookForStartDual = "h/s + ";
        private const string LookForEnd = "sol/s";
        private const string LookForEndDual = "h/s  ";
        private const double DevFee = 2.0;
        string gminer_var = "";
        protected AlgorithmType SecondaryAlgorithmType = AlgorithmType.NONE;
        private double _power = 0.0d;
        double _powerUsage = 0;
        int _apiErrors = 0;

        public GMiner(AlgorithmType secondaryAlgorithmType) : base("GMiner")
        {
            ConectionType = NhmConectionType.NONE;
            SecondaryAlgorithmType = secondaryAlgorithmType;
            IsMultiType = true;
        }

        public override void Start(string wallet, string password)
        {
            string url = "";

            LastCommandLine = GetStartCommand(url, wallet, password);
            const string vcp = "msvcp120.dll";
            var vcpPath = WorkingDirectory + vcp;
            if (!File.Exists(vcpPath))
            {
                try
                {
                    File.Copy(vcp, vcpPath, true);
                    Helpers.ConsolePrint(MinerTag(), $"Copy from {vcp} to {vcpPath} done");
                }
                catch (Exception e)
                {
                    Helpers.ConsolePrint(MinerTag(), "Copy msvcp.dll failed: " + e.Message);
                }
            }
            ProcessHandle = _Start();
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            Helpers.ConsolePrint("GMINER Stop", "");
            DeviceType devtype = DeviceType.NVIDIA;
            var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();
            foreach (var mPair in sortedMinerPairs)
            {
                devtype = mPair.Device.DeviceType;
            }

            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
            KillGminer();
        }
        private string GetStartCommand(string url, string wallet, string password)
        {
            string ZilMining = "";
            DeviceType devtype = DeviceType.NVIDIA;
            var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();
            foreach (var mPair in sortedMinerPairs)
            {
                devtype = mPair.Device.DeviceType;
            }

            if (Form_additional_mining.isAlgoZIL(MiningSetup.AlgorithmName, MinerBaseType.GMiner, devtype))
            {
                ZilClient.needConnectionZIL = true;
                ZilClient.StartZilMonitor();
            }

            if (Form_additional_mining.isAlgoZIL(MiningSetup.AlgorithmName, MinerBaseType.GMiner, devtype) &&
                ConfigManager.GeneralConfig.ZIL_mining_state == 1)
            {
                ZilMining = " --zilserver stratum+tcp://daggerhashimoto.auto.nicehash.com:9200 --ziluser " + ConfigManager.GeneralConfig.Wallet + " ";
            }

            if (Form_additional_mining.isAlgoZIL(MiningSetup.AlgorithmName, MinerBaseType.GMiner, devtype) &&
                ConfigManager.GeneralConfig.ZIL_mining_state == 2)
            {
                ZilMining = " --zilserver " + ConfigManager.GeneralConfig.ZIL_mining_pool + ":" +
                    ConfigManager.GeneralConfig.ZIL_mining_port + " --ziluser " + ConfigManager.GeneralConfig.ZIL_mining_wallet + "." + wallet + " ";
            }

            string _wallet = "--user " + wallet;

            var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
            _algo = _algo.Replace("equihash125", "equihash125_4");
            _algo = _algo.Replace("equihash144", "equihash144_5 --pers auto");

            string proxy = "";
            if (ConfigManager.GeneralConfig.EnableProxy)
            {
                //proxy = "--proxy " + Stats.Stats.CurrentProxyIP + ":" + Stats.Stats.CurrentProxySocks5SPort + " ";
                proxy = "--proxy 127.0.0.1:" + Socks5Relay.Port + " ";
            }

            return " --algo " + _algo +
            " " + $"--api {ApiPort} " + " " +
                    " --ssl -s " + GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower()) + " " + proxy +
                    "-u " + wallet + " -p " + password + " " +
                    GetDevicesCommandString().Trim();
        }
        /*
        private string GetServerDual(string algo, string algo2, string username, string port, string port2)
        {
            string ret = "";
            string ssl = "";
            string dssl = "";
            string psw = "x";
            if (ConfigManager.GeneralConfig.StaleProxy) psw = "stale";
            if (ConfigManager.GeneralConfig.ProxySSL && Globals.MiningLocation.Length > 1)
            {
                port = "4" + port;
                port2 = "4" + port2;
                ssl = "--ssl 1 ";
                dssl = "--dssl 1 ";
            }
            else
            {
                port = "1" + port;
                port2 = "1" + port2;
                ssl = "--ssl 0 ";
                dssl = "--dssl 0 ";
            }
            foreach (string serverUrl in Globals.MiningLocation)
            {
                if (serverUrl.Contains("auto"))
                {
                    ret = ret + " -s " + Links.CheckDNS(algo + "." + serverUrl).Replace("stratum+tcp://", "") + ":9200 -u " +
                        username + " -p " + psw + " --ssl 0 " +
                        " --dserver " + Links.CheckDNS(algo2 + "." + serverUrl).Replace("stratum+tcp://", "") + ":9200 --duser " +
                        username + " --dpass " + psw + " --dssl 0 ";
                    if (!ConfigManager.GeneralConfig.ProxyAsFailover) break;
                }
                else
                {
                    ret = ret + " -s " + Links.CheckDNS("stratum." + serverUrl).Replace("stratum+tcp://", "") + ":" + port + " -u " +
                        username + " -p " + psw + " " + ssl + " " +
                        " --dserver " + Links.CheckDNS("stratum." + serverUrl).Replace("stratum+tcp://", "") + ":" + port2 + " --duser " +
                        username + " --dpass " + psw + " " + dssl;
                }
            }
            return ret;
        }
        */
        protected override string GetDevicesCommandString()
        {
            var deviceStringCommand = "  --watchdog 0 --devices ";
            var ids = new List<string>();
            var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();
            var extra = "";
            int id;
            foreach (var mPair in sortedMinerPairs)
            {
                id = mPair.Device.IDByBus;

                if (mPair.Device.DeviceType == DeviceType.NVIDIA)
                {
                    gminer_var = " --opencl 0 ";
                    extra = ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.NVIDIA);
                }
                else
                {
                    gminer_var = " --cuda 0 ";
                    extra = ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.AMD);
                }

                {
                    ids.Add(id.ToString());
                }
            }

            deviceStringCommand += string.Join(" ", ids);
            deviceStringCommand = deviceStringCommand + extra + " ";

            return gminer_var + deviceStringCommand;
        }

        protected void KillMinerBase(string exeName)
        {
            foreach (var process in Process.GetProcessesByName(exeName))
            {
                try { process.Kill(); }
                catch (Exception e) { Helpers.ConsolePrint(MinerDeviceName, e.ToString()); }
            }
        }
        private static void KillProcessAndChildren(int pid)
        {
            // Cannot close 'system idle process'.
            if (pid == 0)
            {
                return;
            }

            try
            {
                var GMinerHandle = new Process
                {
                    StartInfo =
                {
                    FileName = "taskkill.exe"
                }
                };
                GMinerHandle.StartInfo.Arguments = "/PID " + pid.ToString() + " /F /T";
                GMinerHandle.StartInfo.UseShellExecute = false;
                GMinerHandle.StartInfo.CreateNoWindow = true;
                GMinerHandle.Start();
            } catch (Exception ex)
            {
                Helpers.ConsolePrint("KillProcessAndChildren", ex.ToString());
            }

            Thread.Sleep(100);
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
                    ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
        }
        public override void EndBenchmarkProcces()
        {
            if (BenchmarkProcessStatus != BenchmarkProcessStatus.Killing && BenchmarkProcessStatus != BenchmarkProcessStatus.DoneKilling)
            {
                BenchmarkProcessStatus = BenchmarkProcessStatus.Killing;
                try
                {
                    Helpers.ConsolePrint("BENCHMARK",
                        $"Trying to kill benchmark process {ProcessTag()} algorithm {BenchmarkAlgorithm.AlgorithmName}");

                    int k = ProcessTag().IndexOf("pid(");
                    int i = ProcessTag().IndexOf(")|bin");
                    var cpid = ProcessTag().Substring(k + 4, i - k - 4).Trim();

                    int pid = int.Parse(cpid, CultureInfo.InvariantCulture);
                    Helpers.ConsolePrint("BENCHMARK", "gminer.exe PID: " + pid.ToString());
                    KillProcessAndChildren(pid);
                    BenchmarkHandle.Kill();
                    BenchmarkHandle.Close();
                    if (IsKillAllUsedMinerProcs) KillAllUsedMinerProcesses();
                }
                catch { }
                finally
                {
                    BenchmarkProcessStatus = BenchmarkProcessStatus.DoneKilling;
                    Helpers.ConsolePrint("BENCHMARK",
                        $"Benchmark process {BenchmarkProcessPath} algorithm {BenchmarkAlgorithm.AlgorithmName} KILLED");
                    //BenchmarkHandle = null;
                }
            }
        }
        public void KillGminer()
        {
            try
            {
                int k = ProcessTag().IndexOf("pid(");
                int i = ProcessTag().IndexOf(")|bin");
                var cpid = ProcessTag().Substring(k + 4, i - k - 4).Trim();

                int pid = int.Parse(cpid, CultureInfo.InvariantCulture);
                Helpers.ConsolePrint("GMINER", "kill gminer.exe PID: " + pid.ToString());
                KillProcessAndChildren(pid);
                if (ProcessHandle is object && ProcessHandle != null)
                {
                    ProcessHandle.Kill();
                    ProcessHandle.Close();
                }
            }
            catch { }
            //if (IsKillAllUsedMinerProcs) KillAllUsedMinerProcesses();

        }
        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            _benchmarkTimeWait = time;
            var ret = "";
            string serverUrl = Form_Main.regionList[ConfigManager.GeneralConfig.ServiceLocation].RegionLocation +
                "mine.zergpool.com";
            var algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
            Stats.Stats.MiningAlgorithms _a = new();
            var pool = "";
            _a = Stats.Stats.MiningAlgorithmsList.FirstOrDefault(item => item.name.ToLower() == algo.ToLower());

            string proxy = "";
            if (ConfigManager.GeneralConfig.EnableProxy)
            {
                //proxy = "--proxy " + Stats.Stats.CurrentProxyIP + ":" + Stats.Stats.CurrentProxySocks5SPort + " ";
                proxy = "--proxy 127.0.0.1:" + Socks5Relay.Port + " ";
            }

            if (_a is object && _a != null)
            {
                pool = Links.CheckDNS(algo + serverUrl).Replace("stratum+tcp://", "") + 
                    " --port " + _a.port.ToString() + " " + proxy;
            }
            else
            {
                Helpers.ConsolePrint("GMiner", "Not found " + algo + " in MiningAlgorithmsList. Try fix it.");
                algo = algo.Replace("_", "-");
                _a = Stats.Stats.MiningAlgorithmsList.FirstOrDefault(item => item.name.ToLower() == algo.ToLower());
                pool = Links.CheckDNS(algo + serverUrl).Replace("stratum+tcp://", "") + 
                    " --port " + _a.port.ToString() + " " + proxy;
            }
            var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
            _algo = _algo.Replace("equihash125", "equihash125_4");
            _algo = _algo.Replace("equihash144", "equihash144_5 --pers auto");

            return " " + "-a " + _algo +
            $" -s {pool} --user {Globals.DemoUser} -p c=LTC" +
            $" --api {ApiPort} " + 
            GetDevicesCommandString().Trim();

            //duals

            /*
            if (MiningSetup.CurrentAlgorithmType == AlgorithmType.ETCHash && MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.IronFish)
            {
                ret = " --color 0 --pec --algo etchash" +
                " --server " + Links.CheckDNS("stratum+tcp://etc.2miners.com:1010").Replace("stratum+tcp://", "") + " --user 0x266b27bd794d1A65ab76842ED85B067B415CD505.GMiner --pass x " +
                "--dalgo ironfish --dserver " + Links.CheckDNS("ru.ironfish.herominers.com:1145").Replace("stratum+tcp://", "") + " --duser fb8aaaf8594143a4007c9fe0e0056bd3ca55848d0f5247f7eee8918ca8345521.GMiner --dpass x " +
                GetDevicesCommandString();
                addTime = 30;
            }
            */
            
            return ret + " --api " + ApiPort;
        }
        protected override bool BenchmarkParseLine(string outdata)
        {
            return true;
        }

        // stub benchmarks read from file
        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata)
        {
            CheckOutdata(outdata);
        }


        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 60 * 1000 * 5;  // 5 min
        }
        protected override bool IsApiEof(byte third, byte second, byte last)
        {
            return third == 0x7d && second == 0xa && last == 0x7d;
        }

        private class JsonApiResponse
        {
            public class Devices
            {
                public int gpu_id { get; set; }
                public double speed { get; set; }
                public double speed2 { get; set; }
                public double speed3 { get; set; }
                public string speed_unit { get; set; }
                public string speed_unit2 { get; set; }
                public string speed_unit3 { get; set; }

            }
            public Devices[] devices { get; set; }
            public string miner { get; set; }
            public string algorithm { get; set; }
        }

        private ApiData ad;
        public override ApiData GetApiData()
        {
            return ad;
        }
        public override async Task<ApiData> GetSummaryAsync()
        {
            //Helpers.ConsolePrint("try API...........", "");
            //ApiData ad;
            CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
            ad = new ApiData(MiningSetup.CurrentAlgorithmType, MiningSetup.CurrentSecondaryAlgorithmType);
            ad.ThirdAlgorithmID = AlgorithmType.NONE;
            DeviceType devtype = DeviceType.NVIDIA;

            string ResponseFromGMiner = "";
            double total = 0;
            double total2 = 0;
            double total3 = 0;
            try
            {
                HttpWebRequest WR = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + ApiPort.ToString() + "/stat");
                WR.UserAgent = "GET / HTTP/1.1\r\n\r\n";
                WR.Timeout = 3 * 1000;
                WR.ReadWriteTimeout = 3 * 1000;
                WR.Credentials = CredentialCache.DefaultCredentials;
                WebResponse Response = WR.GetResponse();
                Stream SS = Response.GetResponseStream();
                SS.ReadTimeout = 2 * 1000;
                StreamReader Reader = new StreamReader(SS);
                Reader.BaseStream.ReadTimeout = 3 * 1000;
                ResponseFromGMiner = await Reader.ReadToEndAsync();
                //Helpers.ConsolePrint("->", ResponseFromGMiner);
                if (ResponseFromGMiner.Length == 0 || (ResponseFromGMiner[0] != '{' && ResponseFromGMiner[0] != '['))
                    throw new WebException();
                Reader.Close();
                Response.Close();
            }
            catch (Exception)
            {
                _apiErrors++;
                Helpers.ConsolePrint("GetSummaryAsync", "GMINER-API ERRORs count: " + _apiErrors.ToString());
                if (_apiErrors > 60)
                {
                    _apiErrors = 0;
                    Helpers.ConsolePrint("GetSummaryAsync", "Need RESTART GMINER");
                    CurrentMinerReadStatus = MinerApiReadStatus.RESTART;
                    ad.Speed = 0;
                    ad.SecondarySpeed = 0;
                    ad.ThirdSpeed = 0;
                    return ad;
                }
                CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                ad.Speed = 0;
                ad.SecondarySpeed = 0;
                ad.ThirdSpeed = 0;
                return ad;
            }
            ad = new ApiData(MiningSetup.CurrentAlgorithmType, MiningSetup.CurrentSecondaryAlgorithmType);
            ad.ThirdAlgorithmID = AlgorithmType.NONE;

            ResponseFromGMiner = ResponseFromGMiner.Replace("-nan", "0.00");
            ResponseFromGMiner = ResponseFromGMiner.Replace("(ind)", "");
            //Helpers.ConsolePrint("->", ResponseFromGMiner);
            string _algo = "";
            string _miner = "";
            try
            {
                dynamic resp = JsonConvert.DeserializeObject<JsonApiResponse>(ResponseFromGMiner);
                if (resp != null)
                {
                    _miner = resp.miner;
                    _algo = resp.algorithm;
                    double[] hashrates = new double[resp.devices.Length];
                    double[] hashrates2 = new double[resp.devices.Length];
                    double[] hashrates3 = new double[resp.devices.Length];
                    for (var i = 0; i < resp.devices.Length; i++)
                    {
                        total = total + resp.devices[i].speed;
                        total2 = total2 + resp.devices[i].speed2;
                        total3 = total3 + resp.devices[i].speed3;
                        hashrates[i] = resp.devices[i].speed;
                        hashrates2[i] = resp.devices[i].speed2;
                        hashrates3[i] = resp.devices[i].speed3;
                        /*
                        Helpers.ConsolePrint("****", " dev: " + i.ToString() + " hr1: " + hashrates[i].ToString() +
                            " hr2: " + hashrates2[i].ToString() +
                            " hr3: " + hashrates3[i].ToString());
                        */
                    }

                    int dev = 0;
                    var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();

                    if (Form_Main.NVIDIA_orderBug)
                    {
                        sortedMinerPairs.Sort((a, b) => a.Device.ID.CompareTo(b.Device.ID));
                    }

                    foreach (var mPair in sortedMinerPairs)
                    {
                        devtype = mPair.Device.DeviceType;
                        _power = mPair.Device.PowerUsage;
                        mPair.Device.MiningHashrate = hashrates[dev];
                        mPair.Device.MiningHashrateSecond = hashrates2[dev];
                        mPair.Device.MiningHashrateThird = hashrates3[dev];
                        //duals
                        /*
                        if ((MiningSetup.CurrentAlgorithmType == AlgorithmType.DaggerHashimoto ||
                            MiningSetup.CurrentAlgorithmType == AlgorithmType.ETCHash) &&
                            MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.IronFish)
                            MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.IronFish)
                        {
                            mPair.Device.MiningHashrate = hashrates[dev];
                            mPair.Device.MiningHashrateSecond = hashrates2[dev];
                            mPair.Device.MiningHashrateThird = 0;
                            mPair.Device.AlgorithmID = (int)MiningSetup.CurrentAlgorithmType;
                            mPair.Device.SecondAlgorithmID = (int)MiningSetup.CurrentSecondaryAlgorithmType;
                            mPair.Device.ThirdAlgorithmID = (int)AlgorithmType.NONE;
                        }

                        if (MiningSetup.CurrentAlgorithmType == AlgorithmType.Autolykos &&
                            MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.IronFish)
                        {
                            if (Form_Main.isZilRound && Form_additional_mining.isAlgoZIL(MiningSetup.AlgorithmName, MinerBaseType.GMiner, devtype))
                            {
                                mPair.Device.MiningHashrate = 0;
                                mPair.Device.MiningHashrateSecond = 0;
                                mPair.Device.AlgorithmID = (int)AlgorithmType.NONE;
                                mPair.Device.SecondAlgorithmID = (int)AlgorithmType.NONE;
                                mPair.Device.ThirdAlgorithmID = (int)AlgorithmType.DaggerHashimoto;
                            }
                            else
                            {
                                mPair.Device.MiningHashrateThird = 0;
                                mPair.Device.AlgorithmID = (int)MiningSetup.CurrentAlgorithmType;
                                mPair.Device.SecondAlgorithmID = (int)MiningSetup.CurrentSecondaryAlgorithmType;
                                mPair.Device.ThirdAlgorithmID = (int)AlgorithmType.NONE;
                            }
                        }
                        if (MiningSetup.CurrentAlgorithmType == AlgorithmType.Octopus &&
                            MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.IronFish)
                        {
                            if (Form_Main.isZilRound && Form_additional_mining.isAlgoZIL(MiningSetup.AlgorithmName, MinerBaseType.GMiner, devtype))
                            {
                                mPair.Device.MiningHashrate = 0;
                                mPair.Device.MiningHashrateSecond = 0;
                                mPair.Device.AlgorithmID = (int)AlgorithmType.NONE;
                                mPair.Device.SecondAlgorithmID = (int)AlgorithmType.NONE;
                                mPair.Device.ThirdAlgorithmID = (int)AlgorithmType.DaggerHashimoto;
                            }
                            else
                            {
                                mPair.Device.MiningHashrateThird = 0;
                                mPair.Device.AlgorithmID = (int)MiningSetup.CurrentAlgorithmType;
                                mPair.Device.SecondAlgorithmID = (int)MiningSetup.CurrentSecondaryAlgorithmType;
                                mPair.Device.ThirdAlgorithmID = (int)AlgorithmType.NONE;
                            }
                        }
                        */
                        //


                        if (MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.NONE)//single
                        {
                            if (Form_Main.isZilRound && Form_additional_mining.isAlgoZIL(MiningSetup.AlgorithmName, MinerBaseType.GMiner, devtype))
                            {
                                mPair.Device.MiningHashrate = 0;
                                mPair.Device.AlgorithmID = (int)AlgorithmType.NONE;
                                mPair.Device.SecondAlgorithmID = (int)AlgorithmType.Ethash;
                                mPair.Device.ThirdAlgorithmID = (int)AlgorithmType.NONE;
                            }
                            else
                            {
                                mPair.Device.MiningHashrateSecond = 0;
                                mPair.Device.MiningHashrateThird = 0;
                                mPair.Device.AlgorithmID = (int)MiningSetup.CurrentAlgorithmType;
                                mPair.Device.SecondAlgorithmID = (int)MiningSetup.CurrentSecondaryAlgorithmType;
                                mPair.Device.ThirdAlgorithmID = (int)AlgorithmType.NONE;
                            }
                        }
                        dev++;
                    }
                }
                else
                {
                    Helpers.ConsolePrint("GMiner:", "resp - null");
                }
                _apiErrors = 0;
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("GMiner API:", ex.ToString());
            }
            finally
            {

                if (!MiningSetup.CurrentSecondaryAlgorithmType.Equals(AlgorithmType.NONE))//???
                {
                    ad.SecondaryAlgorithmID = MiningSetup.CurrentSecondaryAlgorithmType;
                }

                ad.ZilRound = false;
                ad.Speed = total;
                ad.SecondarySpeed = total2;
                ad.ThirdSpeed = total3;

                if (Form_Main.isZilRound && Form_additional_mining.isAlgoZIL(MiningSetup.AlgorithmName, MinerBaseType.GMiner, devtype))
                {
                    if (MiningSetup.CurrentSecondaryAlgorithmType != AlgorithmType.NONE)//dual
                    {
                        if (_algo.ToLower().Contains("zil") && total3 > 0)//dual+zil
                        {
                            ad.Speed = 0;
                            ad.SecondarySpeed = 0;
                            ad.ThirdSpeed = total3;
                            ad.ZilRound = true;
                            ad.AlgorithmID = AlgorithmType.NONE;
                            ad.SecondaryAlgorithmID = AlgorithmType.NONE;
                            ad.ThirdAlgorithmID = AlgorithmType.Ethash;
                        }
                    }
                    else
                    {
                        if (_algo.ToLower().Contains("zil") && total2 > 0)//+zil
                        {
                            ad.Speed = 0;
                            ad.SecondarySpeed = total2;
                            ad.ThirdSpeed = 0;
                            ad.ZilRound = true;
                            ad.AlgorithmID = AlgorithmType.NONE;
                            ad.SecondaryAlgorithmID = AlgorithmType.Ethash;
                        }
                    }
                }
                else
                {
                    ad.ZilRound = false;
                    ad.ThirdSpeed = 0;
                    ad.ThirdAlgorithmID = AlgorithmType.NONE;

                    if (MiningSetup.CurrentSecondaryAlgorithmType != AlgorithmType.NONE)//dual
                    {
                        //if (_algo.ToLower().Contains("zil"))//dual
                        {
                            ad.Speed = total;
                            ad.SecondarySpeed = total2;
                        }
                    }
                    else
                    {
                        //if (_algo.ToLower().Contains("zil"))
                        {
                            ad.Speed = total;
                            ad.SecondarySpeed = 0;
                            ad.SecondaryAlgorithmID = AlgorithmType.NONE;
                        }
                    }
                }

                if (ad.Speed == 0 && ad.SecondarySpeed == 0 && ad.ThirdSpeed == 0)
                {
                    CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                }
                else
                {
                    CurrentMinerReadStatus = MinerApiReadStatus.GOT_READ;
                    var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();
                    foreach (var mPair in sortedMinerPairs)
                    {
                        devtype = mPair.Device.DeviceType;
                    }
                }

            }
            /*
            Helpers.ConsolePrint("*******", "CurrentAlgorithmType: " + MiningSetup.CurrentAlgorithmType.ToString() +
       " CurrentSecondaryAlgorithmType: " + MiningSetup.CurrentSecondaryAlgorithmType.ToString() +
       " Form_Main.isZilRound: " + Form_Main.isZilRound.ToString());
            */
            Thread.Sleep(100);
            /*
            //������� ��-�� ���� � Anti-hacking
            if (fs.Length > offset)
            {
                int count = (int)(fs.Length - offset);
                byte[] array = new byte[count];
                fs.Read(array, 0, count);
                offset = (int)fs.Length;
                string textFromFile = System.Text.Encoding.Default.GetString(array).Trim();
                //Helpers.ConsolePrint(MinerTag(), textFromFile);
                if (textFromFile.Contains("Anti-hacking"))
                {
                    Helpers.ConsolePrint(MinerTag(), "GMiner Anti-hacking bug detected.");
                    ad.Speed = 0;
                    CurrentMinerReadStatus = MinerApiReadStatus.RESTART;
                }
            }
            */
            return ad;
        }
    }
}
