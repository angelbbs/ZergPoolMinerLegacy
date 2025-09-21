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
    public class Rigel : Miner
    {
        private int _benchmarkTimeWait = 120;
        private const double DevFee = 2.0;
        protected AlgorithmType SecondaryAlgorithmType = AlgorithmType.NONE;
        private double _power = 0.0d;
        int _apiErrors = 0;
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

            string MainMining = "";
            DeviceType devtype = DeviceType.NVIDIA;
            var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();
            foreach (var mPair in sortedMinerPairs)
            {
                devtype = mPair.Device.DeviceType;
            }

            string proxy = "";
            if (ConfigManager.GeneralConfig.EnableProxy)
            {
                //proxy = "--proxy " + Stats.Stats.CurrentProxyIP + ":" + Stats.Stats.CurrentProxySocks5SPort + " ";
                proxy = "--proxy 127.0.0.1:" + Socks5Relay.Port;
            }

            if (MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.NONE)//single
            {
                string _wallet = "-u " + wallet;
                string _password = "-p " + password + " ";
                var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
                _algo = _algo.Replace("equihash125", "equihash125_4");
                _algo = _algo.Replace("equihash144", "equihash144_5 --pers auto");

                return " -a " + _algo +
                " " + $"--api-bind 127.0.0.1:{ApiPort} " + " --no-strict-ssl " +
                        "-o stratum+ssl://" + GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower()) + " " +
                        proxy + " " +
                        _wallet + " " + _password +
                        GetDevicesCommandString().Trim();

            }
            else //dual
            {
                string _wallet = "-u [1]" + wallet + " -u [2]" + wallet;
                string coin1 = password.Split(',')[2].Replace("mc=", "").Split('+')[0];
                string coin2 = password.Split(',')[2].Replace("mc=", "").Split('+')[1];

                string password1 = password.Replace("mc=" + coin1 + "+" + coin2, "mc=" + coin1);
                string password2 = password.Replace("mc=" + coin1 + "+" + coin2, "mc=" + coin2);

                string _password = "-p [1]" + password1.Trim().Replace("+", "/") + " " +
                    " -p [2]" + password2.Trim().Replace("+", "/") + " ";

                var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
                var _algo2 = MiningSetup.CurrentSecondaryAlgorithmType.ToString().ToLower();

                return " -a " + _algo + "+" + _algo2 +
                " " + $"--api-bind 127.0.0.1:{ApiPort} " + " --no-strict-ssl " +
                        GetServerDual(MiningSetup.CurrentAlgorithmType.ToString().ToLower(),
                        MiningSetup.CurrentSecondaryAlgorithmType.ToString().ToLower()) + " " +
                        proxy + " " +
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
                var _a = Stats.Stats.CoinList.FirstOrDefault(item => item.algo.ToLower() == algo.ToLower());
                var _a2 = Stats.Stats.CoinList.FirstOrDefault(item => item.algo.ToLower() == algo2.ToLower());
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
            }

            deviceStringCommand += string.Join(",", ids);
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
                    //if (IsKillAllUsedMinerProcs) KillAllUsedMinerProcesses();
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

            string proxy = "";
            if (ConfigManager.GeneralConfig.EnableProxy)
            {
                //proxy = "--proxy " + Stats.Stats.CurrentProxyIP + ":" + Stats.Stats.CurrentProxySocks5SPort + " ";
                proxy = "--proxy 127.0.0.1:" + Socks5Relay.Port;
            }

            string failover = "";
            switch (MiningSetup.CurrentAlgorithmType)
            {
                case AlgorithmType.Ethash:
                    failover = $"-o stratum+tcp://ethw.2miners.com:2020 -u bc1qun08kg08wwdsszrymg8z4la5d6ygckg9nxh4pq -p x ";
                    break;
                case AlgorithmType.KawPow:
                    failover = $"-o stratum+tcp://rvn.2miners.com:6060 -u bc1qun08kg08wwdsszrymg8z4la5d6ygckg9nxh4pq -p x ";
                    break;
                case AlgorithmType.Ethashb3:
                    failover = $"-o stratum+tcp://eu.mining4people.com:3454 -u 0xcd0E6454702D676B165cE7Dc6E42f3F692f7F147 -p x ";
                    break;
                case AlgorithmType.NexaPow:
                    failover = $"-o stratum+tcp://nexa.2miners.com:5050 -u bc1qun08kg08wwdsszrymg8z4la5d6ygckg9nxh4pq -p x ";
                    break;
                case AlgorithmType.KarlsenHashV2:
                    failover = $"-o stratum+tcp://kls.2miners.com:2020 -u bc1qun08kg08wwdsszrymg8z4la5d6ygckg9nxh4pq -p x ";
                    break;
                case AlgorithmType.SHA512256d:
                    failover = $"-o stratum+tcp://sha512256d.eu.mine.zpool.ca:3342 -u LPeihdgf7JRQUNq5cwZbBQQgEmh1m7DSgH -p c=LTC ";
                    break;
                default:
                    break;
            }

            if (MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.NONE)//single
            {
                string wallet = "-u " + Globals.DemoUser;
                string password = " -p c=LTC ";
                var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();

                ret = " --no-strict-ssl --no-colour -a " + _algo +
                " " + $"--api-bind 127.0.0.1:{ApiPort} " + " " +
                        "-o stratum+ssl://" + GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower()) + " " +
                        wallet + " " + password + " " +
                        failover + " " +
                        proxy + " " +
                        GetDevicesCommandString().Trim();
            }
            else //duals
            {
                string wallet = "-u [1]" + Globals.DemoUser + " -u [2]" + Globals.DemoUser;
                string password = " -p [1]c=LTC -p [2]c=LTC";
                var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
                var _algo2 = MiningSetup.CurrentSecondaryAlgorithmType.ToString().ToLower();

                return " --no-strict-ssl --no-colour -a " + _algo + "+" + _algo2 +
                " " + $"--api-bind 127.0.0.1:{ApiPort} " + " " +
                        GetServerDual(MiningSetup.CurrentAlgorithmType.ToString().ToLower(),
                        MiningSetup.CurrentSecondaryAlgorithmType.ToString().ToLower()) + " " +
                        proxy + " " +
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
                ResponseFromRigel = ResponseFromRigel.Replace("-nan", "0.00");
                ResponseFromRigel = ResponseFromRigel.Replace("(ind)", "");
                //Helpers.ConsolePrint("->", ResponseFromRigel);
                if (ResponseFromRigel.Length == 0 || (ResponseFromRigel[0] != '{' && ResponseFromRigel[0] != '['))
                    throw new Exception("Not JSON!");
                Reader.Close();
                Response.Close();
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("rigel API Exception", ex.Message);
                CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                ad.Speed = 0;
                ad.SecondarySpeed = 0;
                ad.ThirdSpeed = 0;
                return ad;
            }
            //return null;
            //Helpers.ConsolePrint("->", ResponseFromRigel);
            string _miner = "";
            try
            {
                dynamic resp = JsonConvert.DeserializeObject(ResponseFromRigel);
                if (resp != null)
                {
                    var devices = resp.devices;
                    string algorithm = resp.algorithm;

                    double[] hashrates = new double[devices.Count];
                    double[] hashrates2 = new double[devices.Count];

                    int _dev = 0;
                    foreach (var d in resp.devices)
                    {
                        int id = d.id;
                        string name = d.name;
                        bool selected = d.selected;

                        double hashrate = 0.0d;
                        double hashrate2 = 0.0d;

                        ulong _hashrate = 0ul;
                        ulong _hashrate2 = 0ul;
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
                        }

                        total = total + hashrate;
                        total2 = total2 + hashrate2;
                        hashrates2[_dev] = hashrate2;
                        hashrates[_dev] = hashrate;

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
                            mPair.Device.MiningHashrateSecond = 0;
                            mPair.Device.MiningHashrateThird = 0;
                            mPair.Device.AlgorithmID = (int)MiningSetup.CurrentAlgorithmType;
                            mPair.Device.SecondAlgorithmID = (int)MiningSetup.CurrentSecondaryAlgorithmType;
                            mPair.Device.ThirdAlgorithmID = (int)AlgorithmType.NONE;
                        }
                        else //dual
                        {
                            mPair.Device.MiningHashrateThird = 0;
                            mPair.Device.AlgorithmID = (int)MiningSetup.CurrentAlgorithmType;
                            mPair.Device.SecondAlgorithmID = (int)MiningSetup.CurrentSecondaryAlgorithmType;
                            mPair.Device.ThirdAlgorithmID = (int)AlgorithmType.NONE;
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
                ad.Speed = total;
                ad.SecondarySpeed = total2;
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
