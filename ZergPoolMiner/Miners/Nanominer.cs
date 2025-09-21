using Newtonsoft.Json;
using ZergPoolMiner.Algorithms;
using ZergPoolMiner.Configs;
using ZergPoolMiner.Devices;
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
    public class Nanominer : Miner
    {
        private int _benchmarkTimeWait = 180;
        string ResponseFromNanominer;
        public string platform = "";
        public FileStream fs;
        private bool IsInBenchmark = false;
        private double _power = 0.0d;
        double _powerUsage = 0;
        int hashrateErrorCount = 0;

        public Nanominer() : base("Nanominer")
        {
            ConectionType = NhmConectionType.NONE;
        }

        public override void Start(string wallet, string password)
        {
            LastCommandLine = GetStartCommand(wallet, password);
            ProcessHandle = _Start();
            try
            {
                do
                {
                    Thread.Sleep(1000);
                } while (!File.Exists("miners\\Nanominer\\" + GetLogFileName()));
                Thread.Sleep(1000);
                fs = new FileStream("miners\\Nanominer\\" + GetLogFileName(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint(MinerTag(), ex.Message);
            }
        }
        private string GetServer(string algo)
        {
            string ret = "";
            try
            {
                algo = algo.Replace("-", "_");
                var _a = Stats.Stats.CoinList.FirstOrDefault(item => item.algo.ToLower() == algo.ToLower());

                string serverUrl = Form_Main.regionList[ConfigManager.GeneralConfig.ServiceLocation].RegionLocation +
                    "mine.zergpool.com";
                ret = "pool1 = " + Links.CheckDNS(algo + serverUrl).Replace("stratum+tcp://", "") + ":" + _a.port.ToString();
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("GetServer", "Error in " + algo + " " + ex.ToString());
                ret = "error_in_list_of_algos.err:1111";
            }

            return ret + " ";
        }
        private string GetStartCommand(string wallet, string password)
        {
            IsInBenchmark = false;
            var param = "";
            DeviceType devtype = DeviceType.NVIDIA;
            foreach (var pair in MiningSetup.MiningPairs)
            {
                devtype = pair.Device.DeviceType;
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
                if (pair.Device.DeviceType == DeviceType.CPU)
                {
                    platform = "cpu";
                    param = ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.CPU).Trim();
                }
            }

            try
            {
                if (File.Exists("miners\\Nanominer\\config_zp_" + platform + ".ini"))
                    File.Delete("miners\\Nanominer\\config_zp_" + platform + ".ini");
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("GetStartCommand", ex.ToString());
            }
            string cfgFile = "";

            if (MiningSetup.CurrentAlgorithmType.Equals(AlgorithmType.VerusHash))
            {
                try
                {
                    if (File.Exists("miners\\Nanominer\\" + GetLogFileName()))
                        File.Delete("miners\\Nanominer\\" + GetLogFileName());
                }
                catch (Exception ex)
                {
                    Helpers.ConsolePrint("GetStartCommand", ex.ToString());
                }
                cfgFile =
                   String.Format("webPort = {0}", ApiPort) + "\n"
                   + String.Format("mport = 0\n")
                   + String.Format("logPath=" + GetLogFileName() + "\n")
                   + String.Format(param) + "\n"
                   + String.Format("coin = VRSC\n")
                   + String.Format("devices = {0}", GetDevicesCommandString()) + "\n"
                   + String.Format("wallet = {0}", wallet.Trim()) + "\n"
                   + String.Format("rigPassword= {0}", password.Trim()) + "\n"
                   + String.Format("protocol = stratum\n")
                   + String.Format("watchdog = false\n")
                   + GetServer("verushash");
            }
            if (MiningSetup.CurrentAlgorithmType.Equals(AlgorithmType.KawPow))
            {
                try
                {
                    if (File.Exists("miners\\Nanominer\\" + GetLogFileName()))
                        File.Delete("miners\\Nanominer\\" + GetLogFileName());
                }
                catch (Exception ex)
                {
                    Helpers.ConsolePrint("GetStartCommand", ex.ToString());
                }
                cfgFile =
                   String.Format("webPort = {0}", ApiPort) + "\n"
                   + String.Format("mport = 0\n")
                   + String.Format("logPath=" + GetLogFileName() + "\n")
                   + String.Format(param) + "\n"
                   + String.Format("coin = RVN\n")
                   + String.Format("devices = {0}", GetDevicesCommandString()) + "\n"
                   + String.Format("wallet = {0}", wallet.Trim()) + "\n"
                   + String.Format("rigPassword= {0}", password.Trim()) + "\n"
                   + String.Format("protocol = stratum\n")
                   + String.Format("watchdog = false\n")
                   + GetServer("kawpow");
            }
            if (MiningSetup.CurrentAlgorithmType.Equals(AlgorithmType.Ethash))
            {
                try
                {
                    if (File.Exists("miners\\Nanominer\\" + GetLogFileName()))
                        File.Delete("miners\\Nanominer\\" + GetLogFileName());
                }
                catch (Exception ex)
                {
                    Helpers.ConsolePrint("GetStartCommand", ex.ToString());
                }
                cfgFile =
                   String.Format("webPort = {0}", ApiPort) + "\n"
                   + String.Format("mport = 0\n")
                   + String.Format("logPath=" + GetLogFileName() + "\n")
                   + String.Format(param) + "\n"
                   + String.Format("coin = ETHW\n")
                   + String.Format("devices = {0}", GetDevicesCommandString()) + "\n"
                   + String.Format("wallet = {0}", wallet.Trim()) + "\n"
                   + String.Format("rigPassword= {0}", password.Trim()) + "\n"
                   + String.Format("protocol = stratum\n")
                   + String.Format("watchdog = false\n")
                   + GetServer("ethash");
            }

            try
            {
                FileStream fs = new FileStream("miners\\Nanominer\\config_zp_" + platform + ".ini", FileMode.Create, FileAccess.Write);
                StreamWriter w = new StreamWriter(fs);
                w.WriteAsync(cfgFile);
                w.Flush();
                w.Close();
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("GetStartCommand", e.ToString());
            }
            
            try
            {
                FileStream fs = new FileStream("miners\\Nanominer\\config_zp_" + platform + ".ini", FileMode.Create, FileAccess.Write);
                StreamWriter w = new StreamWriter(fs);
                w.WriteAsync(cfgFile);
                w.Flush();
                w.Close();
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("GetStartCommand", e.ToString());
            }

            return " config_zp_" + platform + ".ini";
        }


        protected override string GetDevicesCommandString()
        {
            var deviceStringCommand = " ";
            var ids = new List<string>();
            var amdDeviceCount = ComputeDeviceManager.Query.AmdDevices.Count;
            var intelDeviceCount = ComputeDeviceManager.Query.IntelDevices.Count;
            var nvidiaDeviceCount = ComputeDeviceManager.Query._cudaDevices.CudaDevices.Count;
            var allDeviceCount = ComputeDeviceManager.Query.GpuCount;
            Helpers.ConsolePrint("NanominerIndexing", "platform: " + platform);

            var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.BusID).ToList();
            if (Form_Main.NVIDIA_orderBug)
            {
                sortedMinerPairs.Sort((a, b) => a.Device.ID.CompareTo(b.Device.ID));
            }

            Helpers.ConsolePrint("NanominerIndexing", $"Found {allDeviceCount} Total GPU devices");
            Helpers.ConsolePrint("NanominerIndexing", $"Found {nvidiaDeviceCount} NVIDIA devices");
            Helpers.ConsolePrint("NanominerIndexing", $"Found {amdDeviceCount} AMD devices");
            Helpers.ConsolePrint("NanominerIndexing", $"Found {intelDeviceCount} INTEL devices");
            if (platform.Contains("amd"))
            {
                foreach (var mPair in sortedMinerPairs)
                {
                    //int id = (int)mPair.Device.lolMinerBusID + intelDeviceCount + nvidiaDeviceCount;
                    int id = (int)mPair.Device.lolMinerBusID;

                    if (id < 0)
                    {
                        Helpers.ConsolePrint("NanominerIndexing", "ID too low: " + id + " skipping device");
                        continue;
                    }
                    Helpers.ConsolePrint("NanominerIndexing", "Mining ID: " + id);
                    {
                        //devices[dev] = id.ToString();
                        ids.Add(id.ToString());
                        //dev++;
                    }

                }
                deviceStringCommand += string.Join(",", ids);
            }
            if (platform.Contains("intel"))
            {
                foreach (var mPair in sortedMinerPairs)
                {
                    int id = (int)mPair.Device.ID;

                    if (id < 0)
                    {
                        Helpers.ConsolePrint("NanominerIndexing", "ID too low: " + id + " skipping device");
                        continue;
                    }

                    Helpers.ConsolePrint("NanominerIndexing", "Mining ID: " + id);
                    {
                        //devices[dev] = id.ToString();
                        //dev++;
                        ids.Add(id.ToString());
                    }

                }
                deviceStringCommand += string.Join(",", ids);
            }
            if (platform.Contains("nvidia"))
            {
                foreach (var mPair in sortedMinerPairs)
                {
                    int id = mPair.Device.IDByBus;

                    if (id < 0)
                    {
                        Helpers.ConsolePrint("NanominerIndexing", "ID too low: " + id + " skipping device");
                        continue;
                    }

                    Helpers.ConsolePrint("NanominerIndexing", "Mining ID: " + id);
                    {
                        //devices[dev] = id.ToString();
                        //dev++;
                    }
                }
                var ids2 = MiningSetup.MiningPairs.Select(mPair => (mPair.Device.lolMinerBusID).ToString()).ToList();
                deviceStringCommand += string.Join(",", ids2);
            }
            return deviceStringCommand;
        }


        // benchmark stuff
        protected void KillMinerBase(string exeName)
        {
            foreach (var process in Process.GetProcessesByName(exeName))
            {
                try { process.Kill(); }
                catch (Exception e) { Helpers.ConsolePrint(MinerDeviceName, e.ToString()); }
            }
        }

        protected bool IsProcessExist()
        {
            foreach (var process in Process.GetProcessesByName("nanominer"))
            {
                using (ManagementObject mo = new ManagementObject("win32_process.handle='" + process.Id.ToString() + "'"))
                {
                    mo.Get();
                    if (Convert.ToInt32(mo["ParentProcessId"]) == ProcessHandle.Id)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            IsInBenchmark = true;

            if (Form_Main.nanominerCount > 0)
            {
                do
                {
                    Thread.Sleep(1000);
                } while (Form_Main.nanominerCount > 0);
            }
            Form_Main.nanominerCount++;

            foreach (var pair in MiningSetup.MiningPairs)
            {
                if (pair.Device.DeviceType == DeviceType.NVIDIA)
                {
                    platform = "nvidia";
                }
                if (pair.Device.DeviceType == DeviceType.AMD)
                {
                    platform = "amd";
                }
                if (pair.Device.DeviceType == DeviceType.INTEL)
                {
                    platform = "intel";
                }
                if (pair.Device.DeviceType == DeviceType.CPU)
                {
                    platform = "cpu";
                }
            }

            try
            {
                if (File.Exists("miners\\Nanominer\\bench_zp_" + platform + GetDevicesCommandString().Trim(' ') + ".ini"))
                    File.Delete("miners\\Nanominer\\bench_zp_" + platform + GetDevicesCommandString().Trim(' ') + ".ini");

                if (File.Exists("miners\\Nanominer\\bench_zp_second_" + platform + GetDevicesCommandString().Trim(' ') + ".ini"))
                    File.Delete("miners\\Nanominer\\bench_zp_second_" + platform + GetDevicesCommandString().Trim(' ') + ".ini");
            

            if (algorithm.ZergPoolID == AlgorithmType.VerusHash)
            {
                var cfgFile =
                   string.Format("webPort = {0}", ApiPort) + "\n"
                   + string.Format("mport = 0\n")
                   + string.Format("protocol = stratum\n")
                   + string.Format("watchdog = false\n")
                   + ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.CPU).TrimStart(' ') + (char)10
                   + string.Format("coin = VRSC\n")
                   + string.Format("devices = {0}", GetDevicesCommandString()) + "\n"
                   + string.Format("wallet = {0}", Globals.DemoUser) + "\n"
                   + string.Format("rigPassword= {0}", "c=LTC" + 
                   ",ID=" + Miner.GetFullWorkerName()) + "\n"
                   + string.Format("protocol = stratum\n")
                   + string.Format("watchdog = false\n")
                   + GetServer("verushash");

                    FileStream fs = new FileStream("miners\\Nanominer\\bench_zp_" + platform + GetDevicesCommandString().Trim(' ') + ".ini", FileMode.Create, FileAccess.Write);
                    StreamWriter w = new StreamWriter(fs);
                    w.WriteAsync(cfgFile);
                    w.Flush();
                    w.Close();
                _benchmarkTimeWait = time;
            }

            if (algorithm.ZergPoolID == AlgorithmType.KawPow)
            {
                var cfgFile =
                   String.Format("webPort = {0}", ApiPort) + "\n"
                   + String.Format("mport = 0\n")
                   + String.Format("protocol = stratum\n")
                   + String.Format("watchdog = false\n")
                   + ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.AMD).TrimStart(' ') + (char)10
                   + ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.NVIDIA).TrimStart(' ') + (char)10
                   + ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.INTEL).TrimStart(' ') + (char)10
                   + String.Format("[kawpow]\n")
                   + String.Format("devices = {0}", GetDevicesCommandString().Trim(' ')) + "\n"
                   + string.Format("wallet = {0}", Globals.DemoUser) + "\n"
                   + string.Format("rigPassword= {0}", "c=LTC" +
                   ",ID=" + Miner.GetFullWorkerName()) + "\n"
                   + string.Format("protocol = stratum\n")
                   + string.Format("watchdog = false\n")
                   + GetServer("kawpow");

                    FileStream fs = new FileStream("miners\\Nanominer\\bench_zp_" + platform + GetDevicesCommandString().Trim(' ') + ".ini", FileMode.Create, FileAccess.Write);
                    StreamWriter w = new StreamWriter(fs);
                    w.WriteAsync(cfgFile);
                    w.Flush();
                    w.Close();
                _benchmarkTimeWait = time;
            }
                if (algorithm.ZergPoolID == AlgorithmType.Ethash)
                {
                    var cfgFile =
                       String.Format("webPort = {0}", ApiPort) + "\n"
                       + String.Format("mport = 0\n")
                       + String.Format("protocol = stratum\n")
                       + String.Format("watchdog = false\n")
                       + ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.AMD).TrimStart(' ') + (char)10
                       + ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.NVIDIA).TrimStart(' ') + (char)10
                       + ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.INTEL).TrimStart(' ') + (char)10
                       + String.Format("[ethash]\n")
                       + String.Format("devices = {0}", GetDevicesCommandString().Trim(' ')) + "\n"
                       + string.Format("wallet = {0}", Globals.DemoUser) + "\n"
                       + string.Format("rigPassword= {0}", "c=LTC" +
                       ",ID=" + Miner.GetFullWorkerName()) + "\n"
                       + string.Format("protocol = stratum\n")
                       + string.Format("watchdog = false\n")
                       + GetServer("ethash");

                    FileStream fs = new FileStream("miners\\Nanominer\\bench_zp_" + platform + GetDevicesCommandString().Trim(' ') + ".ini", FileMode.Create, FileAccess.Write);
                    StreamWriter w = new StreamWriter(fs);
                    w.WriteAsync(cfgFile);
                    w.Flush();
                    w.Close();
                    _benchmarkTimeWait = time;
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("BenchmarkCreateCommandLine", ex.ToString());
            }

            return " bench_zp_" + platform + GetDevicesCommandString().Trim(' ') + ".ini";

        }

        private static void KillProcessAndChildren(int pid)
        {
            // Cannot close 'system idle process'.
            if (pid == 0)
            {
                return;
            }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
                    ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
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
                        $"Trying to kill benchmark process {BenchmarkProcessPath} algorithm {BenchmarkAlgorithm.AlgorithmName}");

                    int k = ProcessTag().IndexOf("pid(");
                    int i = ProcessTag().IndexOf(")|bin");
                    var cpid = ProcessTag().Substring(k + 4, i - k - 4).Trim();

                    int pid = int.Parse(cpid, CultureInfo.InvariantCulture);
                    Helpers.ConsolePrint("BENCHMARK", "nanominer.exe PID: " + pid.ToString());
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
                }
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

        private ApiData ad;
        public override ApiData GetApiData()
        {
            return ad;
        }
        public override async Task<ApiData> GetSummaryAsync()
        {
            /*
            if (hashrateErrorCount > 12)
            {
                hashrateErrorCount = 0;
                Helpers.ConsolePrint(MinerTag(), "Need Restart nanominer due API error");
                CurrentMinerReadStatus = MinerApiReadStatus.RESTART;
                ad.Speed = 0;
                ad.SecondarySpeed = 0;
                ad.ThirdSpeed = 0;
                return ad;
            }
            */
            CurrentMinerReadStatus = MinerApiReadStatus.WAIT;
            int dSpeed1 = 0;
            int dSpeed2 = 0;
            int gpu_hr = 0;
            var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.BusID).ToList();
            if (Form_Main.NVIDIA_orderBug)
            {
                sortedMinerPairs.Sort((a, b) => a.Device.ID.CompareTo(b.Device.ID));
            }
            try
            {
                HttpWebRequest WR = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + ApiPort.ToString() + "/stats");
                WR.UserAgent = "GET / HTTP/1.1\r\n\r\n";
                WR.Timeout = 5 * 1000;
                WR.Credentials = CredentialCache.DefaultCredentials;
                WebResponse Response = WR.GetResponse();
                Stream SS = Response.GetResponseStream();
                SS.ReadTimeout = 5 * 1000;
                StreamReader Reader = new StreamReader(SS);
                ResponseFromNanominer = await Reader.ReadToEndAsync();
                Reader.Close();
                Response.Close();
                //Helpers.ConsolePrint("API", ResponseFromNanominer);
            }
            catch (Exception ex)
            {
                //hashrateErrorCount++;
                Helpers.ConsolePrint("NanoMiner API Exception", ex.Message);
                CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                ad.Speed = 0;
                ad.SecondarySpeed = 0;
                ad.ThirdSpeed = 0;
                return ad;
            }

            ad = new ApiData(MiningSetup.CurrentAlgorithmType, MiningSetup.CurrentSecondaryAlgorithmType, MiningSetup.MiningPairs[0]);

            bool zilEnabled = false;
            DeviceType devtype = DeviceType.NVIDIA;
            foreach (var pair in MiningSetup.MiningPairs)
            {
                devtype = pair.Device.DeviceType;
            }
            if (Form_additional_mining.isAlgoZIL(MiningSetup.AlgorithmName, MinerBaseType.Nanominer, devtype))
            {
                zilEnabled = true;
            }

            try
            {
                int i = 0;
                
                if (MiningSetup.CurrentAlgorithmType.Equals(AlgorithmType.VerusHash) && MiningSetup.CurrentSecondaryAlgorithmType.Equals(AlgorithmType.NONE))
                {
                    dynamic json = JsonConvert.DeserializeObject(ResponseFromNanominer.Replace("GPU ", "GPU"));
                    if (json == null) return ad;
                    //var cSpeed1 = (json.Algorithms[0].Kawpow);
                    //if (cSpeed1 == null) return ad;
                    //var cSpeed = (json.Algorithms[0].Kawpow.Total.Hashrate);
                    //dSpeed1 = (int)Convert.ToDouble(cSpeed, CultureInfo.InvariantCulture.NumberFormat);
                    string token = "";
                    foreach (var mPair in sortedMinerPairs)
                    {
                        string gpu = "";
                        if (platform.Contains("intel"))
                        {
                            gpu = mPair.Device.ID.ToString();
                        }
                        else
                        {
                            gpu = mPair.Device.lolMinerBusID.ToString();
                        }

                        if (mPair.Device.DeviceType == DeviceType.CPU)
                        {
                            token = $"Algorithms[0].Verushash.CPU.Hashrate";
                        }

                        var hash = (string)json.SelectToken(token);
                        gpu_hr = (int)Convert.ToDouble(hash, CultureInfo.InvariantCulture.NumberFormat);
                        dSpeed1 = gpu_hr;
                        sortedMinerPairs[i].Device.MiningHashrate = gpu_hr;
                        //_power = sortedMinerPairs[i].Device.PowerUsage;
                        _power = mPair.Device.PowerUsage;
                        i++;
                    }
                }

                if (MiningSetup.CurrentAlgorithmType.Equals(AlgorithmType.KawPow) && MiningSetup.CurrentSecondaryAlgorithmType.Equals(AlgorithmType.NONE))
                {
                    dynamic json = JsonConvert.DeserializeObject(ResponseFromNanominer.Replace("GPU ", "GPU"));
                    if (json == null) return ad;
                    var cSpeed1 = (json.Algorithms[0].Kawpow);
                    if (cSpeed1 == null) return ad;
                    var cSpeed = (json.Algorithms[0].Kawpow.Total.Hashrate);
                    dSpeed1 = (int)Convert.ToDouble(cSpeed, CultureInfo.InvariantCulture.NumberFormat);
                    string token = "";
                    foreach (var mPair in sortedMinerPairs)
                    {
                        string gpu = "";
                        if (platform.Contains("intel"))
                        {
                            gpu = mPair.Device.ID.ToString();
                        }
                        else
                        {
                            gpu = mPair.Device.lolMinerBusID.ToString();
                        }

                        sortedMinerPairs[i].Device.MiningHashrate = dSpeed1;
                        //_power = sortedMinerPairs[i].Device.PowerUsage;
                        _power = mPair.Device.PowerUsage;
                        i++;
                    }
                }

                if (MiningSetup.CurrentAlgorithmType.Equals(AlgorithmType.Ethash) && MiningSetup.CurrentSecondaryAlgorithmType.Equals(AlgorithmType.NONE))
                {
                    dynamic json = JsonConvert.DeserializeObject(ResponseFromNanominer.Replace("GPU ", "GPU"));
                    if (json == null) return ad;
                    var cSpeed1 = (json.Algorithms[0].Ethash);
                    if (cSpeed1 == null) return ad;
                    var cSpeed = (json.Algorithms[0].Ethash.Total.Hashrate);
                    dSpeed1 = (int)Convert.ToDouble(cSpeed, CultureInfo.InvariantCulture.NumberFormat);
                    string token = "";
                    foreach (var mPair in sortedMinerPairs)
                    {
                        string gpu = "";
                        if (platform.Contains("intel"))
                        {
                            gpu = mPair.Device.ID.ToString();
                        }
                        else
                        {
                            gpu = mPair.Device.lolMinerBusID.ToString();
                        }

                        sortedMinerPairs[i].Device.MiningHashrate = dSpeed1;
                        //_power = sortedMinerPairs[i].Device.PowerUsage;
                        _power = mPair.Device.PowerUsage;
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                hashrateErrorCount++;
                CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                Helpers.ConsolePrint("API", ex.ToString());
                return null;
            }

            ad.ZilRound = false;
            ad.Speed = dSpeed1;
            ad.SecondarySpeed = 0;
            ad.ThirdSpeed = 0;
            ad.AlgorithmID = MiningSetup.CurrentAlgorithmType;
            ad.SecondaryAlgorithmID = AlgorithmType.NONE;
            ad.ThirdAlgorithmID = AlgorithmType.NONE;

            if (zilEnabled && dSpeed2 > 0)//+zil
            {
                ad.Speed = 0;
                ad.SecondarySpeed = dSpeed2;
                ad.ThirdSpeed = 0;
                ad.ZilRound = true;
                ad.AlgorithmID = AlgorithmType.NONE;
                ad.SecondaryAlgorithmID = AlgorithmType.Ethash;
            }


            if (ad.Speed + ad.SecondarySpeed == 0)
            {
                CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                hashrateErrorCount++;
            }
            else
            {
                CurrentMinerReadStatus = MinerApiReadStatus.GOT_READ;

            }

            Thread.Sleep(10);
            return ad;

        }
        protected override void _Stop(MinerStopType willswitch)
        {
            Helpers.ConsolePrint("Nanominer Stop", "");
            DeviceType devtype = DeviceType.AMD;
            var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();
            foreach (var mPair in sortedMinerPairs)
            {
                devtype = mPair.Device.DeviceType;
            }

            fs.Close();
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
        }

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 60 * 1000 * 5; // 5 minute max, whole waiting time 75seconds
        }
    }
}
