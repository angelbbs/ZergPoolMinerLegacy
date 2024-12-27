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

namespace ZergPoolMiner.Miners
{
    public class Rigel : Miner
    {
        private int _benchmarkTimeWait = 120;
        private const double DevFee = 2.0;
        protected AlgorithmType SecondaryAlgorithmType = AlgorithmType.NONE;
        private double _power = 0.0d;
        int _apiErrors = 0;
        bool isZILround = false;
        int RejectsLimit = 0;

        public Rigel() : base("Rigel")
        {
            ConectionType = NhmConectionType.NONE;
        }

        public override void Start(string wallet, string password)
        {
            string url = "";
            RejectsLimit = ConfigManager.GeneralConfig.KAWPOW_Rigel_Max_Rejects;
            LastCommandLine = GetStartCommand(url, wallet, password);
            ProcessHandle = _Start();
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            DeviceType devtype = DeviceType.NVIDIA;
            var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();
            foreach (var mPair in sortedMinerPairs)
            {
                devtype = mPair.Device.DeviceType;
            }

            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
            KillRigel();
        }
        private string GetStartCommand(string url, string wallet, string password)
        {
            var algo = "";
            var algo2 = "";
            var algoName = "";
            var algoName2 = "";
            var nicehashstratum = "";
            var ssl = "";
            string port = "";
            string port2 = "";

            string ZilMining = "";
            string MainMining = "";
            string ZilAlgo = "";
            DeviceType devtype = DeviceType.NVIDIA;
            var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();
            foreach (var mPair in sortedMinerPairs)
            {
                devtype = mPair.Device.DeviceType;
            }

            if (Form_additional_mining.isAlgoZIL(MiningSetup.AlgorithmName, MinerBaseType.Rigel, devtype))
            {
                ZilClient.needConnectionZIL = true;
                ZilClient.StartZilMonitor();
            }

            if (Form_additional_mining.isAlgoZIL(MiningSetup.AlgorithmName, MinerBaseType.Rigel, devtype) &&
                MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.NONE)//single
            {
                //прокси не используется
                ZilAlgo = "+zil";
                MainMining = "[1]";
                if (ConfigManager.GeneralConfig.ZIL_mining_state == 1)
                {
                    ZilMining = " -o [2]ethstratum+tcp://daggerhashimoto.auto.nicehash.com:9200 -u [2]" + ConfigManager.GeneralConfig.Wallet + " ";
                }
                if (ConfigManager.GeneralConfig.ZIL_mining_state == 2)
                {
                    ZilMining = " -o [2]ethstratum+tcp://" +
                        ConfigManager.GeneralConfig.ZIL_mining_pool.Replace("stratum+tcp://", "") + ":" + ConfigManager.GeneralConfig.ZIL_mining_port +
                        " -u [2]" + ConfigManager.GeneralConfig.ZIL_mining_wallet + "." + "worker" + " ";
                }
            }
            if (Form_additional_mining.isAlgoZIL(MiningSetup.AlgorithmName, MinerBaseType.Rigel, devtype) &&
                MiningSetup.CurrentSecondaryAlgorithmType != AlgorithmType.NONE)//dual
            {
                //прокси не используется
                ZilAlgo = "+zil";
                MainMining = "";
                if (ConfigManager.GeneralConfig.ZIL_mining_state == 1)
                {
                    ZilMining = " -o [3]ethstratum+tcp://daggerhashimoto.auto.nicehash.com:9200 -u [3]" + ConfigManager.GeneralConfig.Wallet + " ";
                }
                if (ConfigManager.GeneralConfig.ZIL_mining_state == 2)
                {
                    ZilMining = " -o [3]ethstratum+tcp://" +
                        ConfigManager.GeneralConfig.ZIL_mining_pool.Replace("stratum+tcp://", "") + ":" + ConfigManager.GeneralConfig.ZIL_mining_port +
                        " -u [3]" + ConfigManager.GeneralConfig.ZIL_mining_wallet + "." + "worker" + " ";
                }
            }

            if (MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.NONE)//single
            {
                string _wallet = "-u " + wallet;
                string _password = "-p " + password + " ";
                var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
                _algo = _algo.Replace("equihash125", "equihash125_4");
                _algo = _algo.Replace("equihash144", "equihash144_5 --pers auto");

                return " -a " + _algo +
                " " + $"--api-bind 127.0.0.1:{ApiPort} " + " " +
                        "-o stratum+ssl://" + GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower()) + " " +
                        _wallet + " " + _password +
                        GetDevicesCommandString().Trim();

            }
            else //dual
            {
                string _wallet = "-u [1]" + wallet + " -u [2]" + wallet;
                string _password = "-p [1]" + password + " " +
                    " -p [2]" + password + " ";
                var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
                var _algo2 = MiningSetup.CurrentSecondaryAlgorithmType.ToString().ToLower();

                return " -a " + _algo + "+" + _algo2 +
                " " + $"--api-bind 127.0.0.1:{ApiPort} " + " " +
                        GetServerDual(MiningSetup.CurrentAlgorithmType.ToString().ToLower(),
                        MiningSetup.CurrentSecondaryAlgorithmType.ToString().ToLower()) + " " +
                        _wallet + " " + _password + " " +
                        GetDevicesCommandString().Trim();
            }
            return "Ooops";
        }

        public string GetServerDual(string algo, string algo2)
        {
            string ret = "";
            try
            {
                algo = algo.Replace("-", "_");
                var _a = Stats.Stats.MiningAlgorithmsList.FirstOrDefault(item => item.name.ToLower() == algo.ToLower());
                var _a2 = Stats.Stats.MiningAlgorithmsList.FirstOrDefault(item => item.name.ToLower() == algo2.ToLower());
                string serverUrl = Form_Main.regionList[ConfigManager.GeneralConfig.ServiceLocation].RegionLocation +
                    "mine.zergpool.com";
                ret = "-o [1]" + Links.CheckDNS(algo + serverUrl) + ":" + _a.port.ToString() + " " +
                    "-o [2]" + Links.CheckDNS(algo2 + serverUrl) + ":" + _a2.port.ToString();
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("GetServer", "Error " + ex.ToString());
                ret = "error_in_list_of_algos.err:1111";
            }
            return ret;
        }
        protected override string GetDevicesCommandString()
        {
            ad = new ApiData(MiningSetup.CurrentAlgorithmType, MiningSetup.CurrentSecondaryAlgorithmType, MiningSetup.MiningPairs[0]);
            ad.ThirdAlgorithmID = AlgorithmType.NONE;

            var deviceStringCommand = " --no-watchdog -d ";
            var ids = new List<string>();
            var zil = new List<string>();
            var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();
            var extra = "";
            int id;

            DeviceType devtype = DeviceType.NVIDIA;
            foreach (var mPair in sortedMinerPairs)
            {
                devtype = mPair.Device.DeviceType;
            }

            foreach (var mPair in sortedMinerPairs)
            {
                id = mPair.Device.IDByBus;

                if (mPair.Device.DeviceType == DeviceType.NVIDIA)
                {
                    extra = ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.NVIDIA);
                }
                else
                {
                    extra = ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.AMD);
                }

                {
                    ids.Add(id.ToString());
                }

                if (Form_additional_mining.isAlgoZIL(MiningSetup.AlgorithmName, MinerBaseType.Rigel, devtype))
                {
                    if (mPair.Device.GpuRam / 1024 > 9 * 1024 * 1024)
                    {
                        zil.Add("on");
                    }
                    else
                    {
                        zil.Add("off");
                    }
                }
            }

            deviceStringCommand += string.Join(",", ids);
            if (Form_additional_mining.isAlgoZIL(MiningSetup.AlgorithmName, MinerBaseType.Rigel, devtype))
            {
                deviceStringCommand += " --zil-cache-dag " + string.Join(",", zil);
            }
            deviceStringCommand = deviceStringCommand + extra + " ";

            return deviceStringCommand;
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
                var RigelHandle = new Process
                {
                    StartInfo =
                {
                    FileName = "taskkill.exe"
                }
                };
                RigelHandle.StartInfo.Arguments = "/PID " + pid.ToString() + " /F /T";
                RigelHandle.StartInfo.UseShellExecute = false;
                RigelHandle.StartInfo.CreateNoWindow = true;
                RigelHandle.Start();
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
            /*
            Thread.Sleep(100);
            try
            {
                Process proc = Process.GetProcessById(pid);
                if (proc != new Process()) proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
            */
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
                    Helpers.ConsolePrint("BENCHMARK", "Rigel.exe PID: " + pid.ToString());
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
        public void KillRigel()
        {
            try
            {
                int k = ProcessTag().IndexOf("pid(");
                int i = ProcessTag().IndexOf(")|bin");
                var cpid = ProcessTag().Substring(k + 4, i - k - 4).Trim();

                int pid = int.Parse(cpid, CultureInfo.InvariantCulture);
                Helpers.ConsolePrint("Rigel", "kill Rigel.exe PID: " + pid.ToString());
                KillProcessAndChildren(pid);
                if (ProcessHandle is object) ProcessHandle.Close();
            }
            catch { }
            //if (IsKillAllUsedMinerProcs) KillAllUsedMinerProcesses();

        }
        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            _benchmarkTimeWait = time;
            var ret = "";
            if (MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.NONE)//single
            {
                string wallet = "-u " + Globals.DemoUser;
                string password = " -p c=LTC ";
                var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
                _algo = _algo.Replace("xelisv2_pepew", "xelishashv2");

                ret = " --no-colour -a " + _algo +
                " " + $"--api-bind 127.0.0.1:{ApiPort} " + " " +
                        "-o stratum+ssl://" + GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower()) + " " +
                        wallet + " " + password + " " +
                        GetDevicesCommandString().Trim();
            }
            else //duals
            {
                string wallet = "-u [1]" + Globals.DemoUser + " -u [2]" + Globals.DemoUser;
                string password = " -p [1]c=LTC -p [2]c=LTC";
                var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
                var _algo2 = MiningSetup.CurrentSecondaryAlgorithmType.ToString().ToLower();

                return " --no-colour -a " + _algo + "+" + _algo2 +
                " " + $"--api-bind 127.0.0.1:{ApiPort} " + " " +
                        GetServerDual(MiningSetup.CurrentAlgorithmType.ToString().ToLower(),
                        MiningSetup.CurrentSecondaryAlgorithmType.ToString().ToLower()) + " " +
                        wallet + " " + password + " " +
                        GetDevicesCommandString().Trim();
            }
            return ret;
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

        private ApiData ad;
        public override ApiData GetApiData()
        {
            return ad;
        }
        public override async Task<ApiData> GetSummaryAsync()
        {
            CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
            string ResponseFromRigel;
            double total = 0;
            double total2 = 0;
            double totalZIL = 0;
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
                ResponseFromRigel = await Reader.ReadToEndAsync();
                //Helpers.ConsolePrint("->", ResponseFromRigel);
                if (ResponseFromRigel.Length == 0 || (ResponseFromRigel[0] != '{' && ResponseFromRigel[0] != '['))
                    throw new Exception("Not JSON!");
                Reader.Close();
                Response.Close();
            }
            catch (Exception)
            {
                _apiErrors++;
                Helpers.ConsolePrint("GetSummaryAsync", "Rigel-API ERRORs count: " + _apiErrors.ToString());
                if (_apiErrors > 60)
                {
                    CurrentMinerReadStatus = MinerApiReadStatus.RESTART;
                    Helpers.ConsolePrint("GetSummaryAsync", "Need RESTART Rigel");
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
            //return null;
            ResponseFromRigel = ResponseFromRigel.Replace("-nan", "0.00");
            ResponseFromRigel = ResponseFromRigel.Replace("(ind)", "");
            //Helpers.ConsolePrint("->", ResponseFromRigel);
            string _miner = "";
            try
            {
                dynamic resp = JsonConvert.DeserializeObject(ResponseFromRigel);
                if (resp != null)
                {
                    var devices = resp.devices;
                    string algorithm = resp.algorithm;
                    string zil_state = "";
                    zil_state = resp.zil_state;
                    if (string.IsNullOrEmpty(zil_state))
                    {
                        zil_state = "";
                    }

                    double[] hashrates = new double[devices.Count];
                    double[] hashrates2 = new double[devices.Count];
                    double[] hashratesZIL = new double[devices.Count];
                    int _dev = 0;
                    foreach (var d in resp.devices)
                    {
                        int id = d.id;
                        string name = d.name;
                        bool selected = d.selected;

                        double hashrate = 0.0d;
                        double hashrate2 = 0.0d;
                        double hashrateZIL = 0.0d;

                        ulong _hashrate = 0ul;
                        ulong _hashrate2 = 0ul;
                        ulong _hashrateZIL = 0ul;
                        if (selected)
                        {
                            if (MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.NONE)//single
                            {
                                string _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
                                string token = $"devices[{_dev}].hashrate.{_algo}";
                                var hash = resp.SelectToken(token);
                                _hashrate = (ulong)Convert.ToUInt64(hash, CultureInfo.InvariantCulture.NumberFormat);
                                hashrate = _hashrate;
                            }
                            else //dual
                            {
                                string _algo1 = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
                                string token1 = $"devices[{_dev}].hashrate.{_algo1}";
                                var hash1 = resp.SelectToken(token1);
                                _hashrate = (ulong)Convert.ToUInt64(hash1, CultureInfo.InvariantCulture.NumberFormat);
                                string _algo2 = MiningSetup.CurrentSecondaryAlgorithmType.ToString().ToLower();
                                string token2 = $"devices[{_dev}].hashrate.{_algo2}";
                                var hash2 = resp.SelectToken(token2);
                                _hashrate2 = (ulong)Convert.ToUInt64(hash2, CultureInfo.InvariantCulture.NumberFormat);

                                hashrate = (double)_hashrate;
                                hashrate2 = (double)_hashrate2;
                            }

                            if (d.hashrate.zil == null)
                            {
                                _hashrateZIL = 0ul;
                            }
                            else
                            {
                                _hashrateZIL = d.hashrate.zil;
                            }
                            hashrateZIL = (double)_hashrateZIL;

                            if (zil_state.Contains("mining"))
                            {
                                isZILround = true;
                                Form_Main.isForceZilRound = true;
                                //Helpers.ConsolePrint("Rigel", "_hashrateZIL: " + hashrateZIL.ToString());
                            }
                            else
                            {
                                isZILround = false;
                                Form_Main.isForceZilRound = false;
                                //Helpers.ConsolePrint("Rigel", "isZILround = false");
                            }
                        }

                        total = total + hashrate;
                        if (isZILround)
                        {
                            total2 = total2 + hashrateZIL;
                            hashrates2[_dev] = hashrateZIL;
                        }
                        else
                        {
                            total2 = total2 + hashrate2;
                            hashrates2[_dev] = hashrate2;
                        }
                        totalZIL = totalZIL + hashrateZIL;

                        hashrates[_dev] = hashrate;
                        hashratesZIL[_dev] = hashrateZIL;

                        _dev++;
                    }
                    //int dev = 0;
                    var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();
                    if (Form_Main.NVIDIA_orderBug)
                    {
                        sortedMinerPairs.Sort((a, b) => a.Device.ID.CompareTo(b.Device.ID));
                    }

                    foreach (var mPair in sortedMinerPairs)
                    {
                        _power = mPair.Device.PowerUsage;
                        mPair.Device.MiningHashrate = hashrates[mPair.Device.ID];
                        mPair.Device.MiningHashrateSecond = hashrates2[mPair.Device.ID];

                        if (MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.NONE)//single
                        {
                            if (isZILround)
                            {
                                mPair.Device.MiningHashrate = 0;
                                mPair.Device.MiningHashrateThird = 0;
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
                        } else //dual
                        {
                            if (isZILround)
                            {
                                mPair.Device.MiningHashrate = 0;
                                mPair.Device.MiningHashrateThird = 0;
                                mPair.Device.AlgorithmID = (int)AlgorithmType.NONE;
                                mPair.Device.SecondAlgorithmID = (int)AlgorithmType.Ethash;
                                mPair.Device.ThirdAlgorithmID = (int)AlgorithmType.NONE;
                            }
                            else
                            {
                                mPair.Device.MiningHashrateThird = 0;
                                mPair.Device.AlgorithmID = (int)MiningSetup.CurrentAlgorithmType;
                                mPair.Device.SecondAlgorithmID = (int)MiningSetup.CurrentSecondaryAlgorithmType;
                                mPair.Device.ThirdAlgorithmID = (int)AlgorithmType.NONE;
                            }
                        }
                    }

                }
                else
                {
                    Helpers.ConsolePrint("Rigel:", "resp - null");
                }
                _apiErrors = 0;
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("Rigel API:", ex.ToString());
            }
            finally
            {
                ad.ZilRound = false;
                ad.Speed = total;
                ad.SecondarySpeed = total2;
                ad.ThirdSpeed = totalZIL;

                if (isZILround)
                {
                    if (MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.NONE)//single+zil
                    {
                        ad.Speed = 0;
                        ad.SecondarySpeed = totalZIL;
                        ad.ThirdSpeed = 0;
                        ad.ZilRound = true;
                        ad.AlgorithmID = AlgorithmType.NONE;
                        ad.SecondaryAlgorithmID = AlgorithmType.Ethash;
                        ad.ThirdAlgorithmID = AlgorithmType.NONE;
                    }
                    else
                    {
                        ad.Speed = 0;
                        ad.SecondarySpeed = 0;
                        ad.ThirdSpeed = totalZIL;
                        ad.ZilRound = true;
                        ad.AlgorithmID = AlgorithmType.NONE;
                        ad.SecondaryAlgorithmID = AlgorithmType.NONE;
                        ad.ThirdAlgorithmID = AlgorithmType.Ethash;
                    }
                }
                else
                {
                    ad.ZilRound = false;
                    ad.ThirdSpeed = 0;
                    ad.ThirdAlgorithmID = AlgorithmType.NONE;

                    if (MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.NONE)//single no zil
                    {
                        ad.Speed = total;
                        ad.SecondarySpeed = 0;
                        ad.ThirdSpeed = 0;
                        ad.SecondaryAlgorithmID = AlgorithmType.NONE;
                        ad.ThirdAlgorithmID = AlgorithmType.NONE;
                    }
                    else
                    {

                    }

                }

                if (ad.Speed == 0 && ad.SecondarySpeed == 0 && ad.ThirdSpeed == 0)
                {
                    CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                }
                else
                {
                    CurrentMinerReadStatus = MinerApiReadStatus.GOT_READ;
                    DeviceType devtype = DeviceType.NVIDIA;
                    var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();
                    foreach (var mPair in sortedMinerPairs)
                    {
                        devtype = mPair.Device.DeviceType;
                    }
                }

            }

            Thread.Sleep(100);

            return ad;
        }
    }

}
