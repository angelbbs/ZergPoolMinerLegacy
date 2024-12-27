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
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZergPoolMiner.Miners
{
    public class miniZ : Miner
    {
#pragma warning disable IDE1006
        private class Result
        {
            public uint gpuid { get; set; }
            public uint cudaid { get; set; }
            public string busid { get; set; }
            public uint gpu_status { get; set; }
            public int solver { get; set; }
            public int temperature { get; set; }
            public uint gpu_power_usage { get; set; }
            public double speed_sps { get; set; }
            public double speed_is { get; set; }
            public uint accepted_shares { get; set; }
            public uint rejected_shares { get; set; }
        }

        private class JsonApiResponse
        {
            public uint id { get; set; }
            public string method { get; set; }
            public object error { get; set; }
            public string pers { get; set; }
            public List<Result> result { get; set; }
        }
#pragma warning restore IDE1006

        private int _benchmarkTimeWait = 2 * 45;
        private const string LookForStart = "(";
        private const string LookForEnd = ")sol/s";
        private double prevSpeed = 0;
        private double _power = 0.0d;
        double _powerUsage = 0;
        private int errorCount = 0;
        string logFile = "";
        public miniZ() : base("miniZ")
        {
            ConectionType = NhmConectionType.NONE;
        }

        public override void Start(string wallet, string password)
        {
            IsApiReadException = false;
            LastCommandLine = GetStartCommand(wallet, password);
            ProcessHandle = _Start();
        }

        private string GetStartCommand(string wallet, string password)
        {
            string ZilMining = "";
            DeviceType devtype = DeviceType.NVIDIA;
            var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();
            foreach (var mPair in sortedMinerPairs)
            {
                devtype = mPair.Device.DeviceType;
            }

            if (Form_additional_mining.isAlgoZIL(MiningSetup.AlgorithmName, MinerBaseType.miniZ, devtype))
            {
                ZilClient.needConnectionZIL = true;
                ZilClient.StartZilMonitor();
            }

            if (!MinerVersion.Get_miniZ().MinerVersion.Trim().Equals("2.1c"))
            {
                if (Form_additional_mining.isAlgoZIL(MiningSetup.AlgorithmName, MinerBaseType.miniZ, devtype) &&
                ConfigManager.GeneralConfig.ZIL_mining_state == 1)
                {
                    //������ �� ������������
                    ZilMining = " --url=zil://" + ConfigManager.GeneralConfig.Wallet + "@daggerhashimoto.auto.nicehash.com:9200";//�� �������� �� �����
                    /*
                    logFile = GetDeviceID() + ".csv";
                    log = " --csv=" + logFile + " --log-period=1";
                    try
                    {
                    if (File.Exists("miners\\miniz\\" + logFile)) File.Delete("miners\\miniz\\" + logFile);
                    }
                    catch (Exception ex)
                    {
                        Helpers.ConsolePrint("GetStartCommand", ex.ToString());
                    }
                    */
                }
            }
            if (Form_additional_mining.isAlgoZIL(MiningSetup.AlgorithmName, MinerBaseType.miniZ, devtype) &&
                ConfigManager.GeneralConfig.ZIL_mining_state == 2)
            {
                ZilMining = " --url " + ConfigManager.GeneralConfig.ZIL_mining_wallet + "." + wallet + "@" +
                    ConfigManager.GeneralConfig.ZIL_mining_pool.Replace("stratum+tcp://", "") + ":" +
                    ConfigManager.GeneralConfig.ZIL_mining_port;
            }

            string sColor = "";
            if (Form_Main.GetWinVer(Environment.OSVersion.Version) < 8)
            {
                sColor = " --nocolour";
            }

            string _password = " --pass=" + password.Trim() + " ";
            var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
            _algo = _algo.Replace("equihash125", "125,4");
            _algo = _algo.Replace("equihash144", "144,5 --pers auto");
            _algo = _algo.Replace("equihash192", "192,7 --pers auto");
            _algo = _algo.Replace("evrprogpow", "progpow");

            return " --par=" + _algo +
            " " + ZilMining + " --telemetry=" + ApiPort +
            " --url=ssl://" + wallet + "@" + GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower()) + " " +
                    _password +
                    GetDevicesCommandString().Trim();
        }

        protected override string GetDevicesCommandString()
        {
            string platform = "";
            string deviceStringCommand = "";
            try
            {
                foreach (var pair in MiningSetup.MiningPairs)
                {
                    if (pair.Device.DeviceType == DeviceType.NVIDIA)
                    {
                        platform = " --nvidia ";
                    }
                    else
                    {
                        platform = " --pci-order --amd ";
                    }
                }
                if (platform.Contains("nvidia"))
                {
                    deviceStringCommand = platform + MiningSetup.MiningPairs.Aggregate(" --cuda-devices ",
                    (current, nvidiaPair) => current + (nvidiaPair.Device.IDByBus + " "));
                    deviceStringCommand +=
                        " " + ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.NVIDIA);
                }
                else
                {
                    deviceStringCommand = platform + MiningSetup.MiningPairs.Aggregate(" -cd ",
                    (current, amdPair) => current + (amdPair.Device.IDByBus + " "));
                    deviceStringCommand +=
                        " " + ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.AMD);
                }
            } catch (Exception ex)
            {
                Helpers.ConsolePrint("GetDevicesCommandString", ex.ToString());
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

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            var ret = "";
            try
            {
                var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
                _algo = _algo.Replace("equihash125", "125,4");
                _algo = _algo.Replace("equihash144", "144,5 --pers auto");
                _algo = _algo.Replace("equihash192", "192,7 --pers auto");
                _algo = _algo.Replace("evrprogpow", "progpow --pers auto");
                ret = GetDevicesCommandString()
                      + "--nocolour --par=" + _algo +
                      " --url=ssl://" + Globals.DemoUser + "@" + GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower())
                      + " --pass=c=LTC" + " --telemetry=" + ApiPort;
                _benchmarkTimeWait = time;

            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("BenchmarkCreateCommandLine", ex.ToString());
            }
            return ret;
        }

        // stub benchmarks read from file
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
            Helpers.ConsolePrint("BENCHMARK", outdata);
            return false;
        }
        */
        protected double GetNumber(string outdata)
        {
            return GetNumber(outdata, LookForStart, LookForEnd);
        }

        protected double GetNumber(string outdata, string lookForStart, string lookForEnd)
        {
            try
            {
                double mult = 1;
                var speedStart = outdata.IndexOf(lookForStart.ToLower());
                var speed = outdata.Substring(speedStart, outdata.Length - speedStart);
                speed = speed.Replace(lookForStart.ToLower(), "");
                speed = speed.Substring(0, speed.IndexOf(lookForEnd.ToLower()));

                if (speed.Contains("k"))
                {
                    mult = 1000;
                    speed = speed.Replace("k", "");
                }
                else if (speed.Contains("m"))
                {
                    mult = 1000000;
                    speed = speed.Replace("m", "");
                }

                //Helpers.ConsolePrint("speed", speed);
                speed = speed.Trim();
                try
                {
                    return double.Parse(speed, CultureInfo.InvariantCulture) * mult;
                }
                catch
                {
                    MessageBox.Show("Unsupported miner version - " + MiningSetup.MinerPath,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    BenchmarkSignalFinnished = true;
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("GetNumber",
                    ex.Message + " | args => " + outdata + " | " + lookForEnd + " | " + lookForStart);
                MessageBox.Show("Unsupported miner version - " + MiningSetup.MinerPath,
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return 0;
        }

        private ApiData ad;
        public override ApiData GetApiData()
        {
            return ad;
        }

        public override async Task<ApiData> GetSummaryAsync()
        {
            CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
            ad = new ApiData(MiningSetup.CurrentAlgorithmType);
            /*
            if (firstStart)
            //          if (ad.Speed <= 0.0001)
            {
                ad = new ApiData(MiningSetup.CurrentAlgorithmType);
                Thread.Sleep(5000);
                ad.Speed = 0;
                firstStart = false;
                return ad;
            }
            */
            JsonApiResponse resp = null;
            string respStr = "";
            string pers = "";
            try
            {
                byte[] bytesToSend;
                if (Form_Main.isZilRound)
                {
                   bytesToSend = Encoding.ASCII.GetBytes(variables.miniZ_toSend_zil);
                } else
                {
                    bytesToSend = Encoding.ASCII.GetBytes(variables.miniZ_toSend);
                }
                var client = new TcpClient("127.0.0.1", ApiPort);
                client.ReceiveTimeout = 2000;
                var nwStream = client.GetStream();
                await nwStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                var bytesToRead = new byte[client.ReceiveBufferSize];
                nwStream.ReadTimeout = 2000;

                StreamReader Reader = new StreamReader(nwStream);
                Reader.BaseStream.ReadTimeout = 3 * 1000;
                respStr = await Reader.ReadToEndAsync();

                //Helpers.ConsolePrint("miniZ API:", respStr);
                respStr = respStr.Substring(respStr.IndexOf('{'), respStr.Length - respStr.IndexOf('{'));
                //Helpers.ConsolePrint("miniZ API:", respStr);
                if (!respStr.Contains("}]}") && prevSpeed != 0)
                {
                    errorCount = 0;
                    client.Close();
                    CurrentMinerReadStatus = MinerApiReadStatus.GOT_READ;
                    if ((ad.AlgorithmID == AlgorithmType.Equihash125 ||
                        ad.AlgorithmID == AlgorithmType.Equihash144 ||
                        ad.AlgorithmID == AlgorithmType.Equihash192) && ad.Speed > 10000)
                    {
                        ad.Speed = 0;
                    }
                    else
                    {
                        ad.Speed = prevSpeed;
                    }
                    return ad;
                }
                resp = JsonConvert.DeserializeObject<JsonApiResponse>(respStr, Globals.JsonSettings);
                client.Close();
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("miniZ API error", ex.Message);
                errorCount++;
                CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                ad.Speed = 0;
                /*
                if (errorCount > 20)
                {
                    Helpers.ConsolePrint("miniZ API error", "Need Restart miner");
                    CurrentMinerReadStatus = MinerApiReadStatus.RESTART;
                    ad.Speed = 0;
                    ad.SecondarySpeed = 0;
                    ad.ThirdSpeed = 0;
                    return ad;
                }
                */
            }

            ad = new ApiData(MiningSetup.CurrentAlgorithmType, MiningSetup.CurrentSecondaryAlgorithmType);
            var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();

            DeviceType devtype = DeviceType.NVIDIA;
            try
            {
                if (resp != null && resp.error == null)
                {
                    ad.Speed = resp.result.Aggregate<Result, double>(0, (current, t1) => current + t1.speed_sps);
                    //Helpers.ConsolePrint("************", "prevSpeed: " + prevSpeed.ToString() +  " ad.Speed: " + ad.Speed.ToString());
                    if (ad.Speed == 0 && prevSpeed > 1)
                    {
                        ad.Speed = prevSpeed;
                    }
                    if (ad.Speed > prevSpeed * 10000 && prevSpeed > 1)
                    {
                        ad.Speed = prevSpeed;
                    }
                    if ((ad.AlgorithmID == AlgorithmType.Equihash125 ||
                        ad.AlgorithmID == AlgorithmType.Equihash144 ||
                        ad.AlgorithmID == AlgorithmType.Equihash192) && ad.Speed > 10000)
                    {
                        ad.Speed = 0;
                    }
                    ad.SecondarySpeed = resp.result.Aggregate<Result, double>(0, (current, t1) => current + t1.speed_is) * 1000000;
                    pers = resp.pers;
                    /*
                    if (pers.Contains("zil"))
                    {
                        Form_Main.isZilRound = true;
                    } else
                    {
                        Form_Main.isZilRound = false;
                    }
                    */
                        double[] hashrates = new double[resp.result.Count];
                    //double[] hashrates2 = new double[resp.result.Count];
                    for (var i = 0; i < resp.result.Count; i++)
                    {
                        hashrates[i] = resp.result[i].speed_sps;
                        //hashrates2[i] = resp.result[i].speed_sps;
                    }
                    int dev = 0;
                    if (Form_Main.NVIDIA_orderBug)
                    {
                        sortedMinerPairs.Sort((a, b) => a.Device.ID.CompareTo(b.Device.ID));
                    }
                    double total = 0.0d;
                    foreach (var mPair in sortedMinerPairs)
                    {
                        _power = mPair.Device.PowerUsage;
                        mPair.Device.MiningHashrate = hashrates[dev];
                        mPair.Device.MiningHashrateSecond = 0;

                        if (MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.NONE)//single
                        {
                            if (Form_Main.isZilRound && Form_additional_mining.isAlgoZIL(MiningSetup.AlgorithmName, MinerBaseType.miniZ, mPair.Device.DeviceType))
                            {
                                //double hashrate = readCSV(mPair.Device.ID);
                                total = total + hashrates[dev];
                                mPair.Device.MiningHashrate = 0;
                                if (hashrates[dev] > 0)
                                {
                                    mPair.Device.MiningHashrateSecond = hashrates[dev];
                                }
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

                    CurrentMinerReadStatus = MinerApiReadStatus.GOT_READ;

                    if (ad.Speed == 0)
                    {
                        CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                    }
                    else
                    {
                        CurrentMinerReadStatus = MinerApiReadStatus.GOT_READ;
                        sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();
                        foreach (var mPair in sortedMinerPairs)
                        {
                            devtype = mPair.Device.DeviceType;
                        }

                    }

                    prevSpeed = ad.Speed;
                    if (Form_Main.isZilRound && total > 0 && Form_additional_mining.isAlgoZIL(MiningSetup.AlgorithmName, MinerBaseType.miniZ, devtype))
                    {
                        ad.SecondarySpeed = total;
                        ad.Speed = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint(MinerTag(), ex.ToString());

                CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                ad.Speed = prevSpeed;
            }


            foreach (var mPair in sortedMinerPairs)
            {
                devtype = mPair.Device.DeviceType;
            }

            if (Form_Main.isZilRound && Form_additional_mining.isAlgoZIL(MiningSetup.AlgorithmName, MinerBaseType.miniZ, devtype))
            {
                if (pers.Contains("zil"))//+zil
                {
                    ad.Speed = 0;
                    ad.ThirdSpeed = 0;
                    ad.ZilRound = true;
                    ad.AlgorithmID = AlgorithmType.NONE;
                    ad.SecondaryAlgorithmID = AlgorithmType.Ethash;
                    ad.ThirdAlgorithmID = AlgorithmType.NONE;
                }
                else
                {
                    ad.ZilRound = false;
                    ad.ThirdSpeed = 0;
                    ad.ThirdAlgorithmID = AlgorithmType.NONE;
                    ad.SecondarySpeed = 0;
                    ad.SecondaryAlgorithmID = AlgorithmType.NONE;
                }
            }
            else
            {
                ad.ZilRound = false;
                ad.ThirdSpeed = 0;
                ad.ThirdAlgorithmID = AlgorithmType.NONE;
                ad.SecondarySpeed = 0;
                ad.SecondaryAlgorithmID = AlgorithmType.NONE;
            }
            return ad;
        }
        private double readCSV(int gpiId)
        {
            string headLine = "";
            string line = "";
            string lineTmp = "";
            try
            {
                if (File.Exists("miners\\miniz\\" + logFile))
                {
                    using (StreamReader sr = new StreamReader("miners\\miniz\\" + logFile))
                    {
                        headLine = sr.ReadLine();
                        int i = 0;
                        while ((lineTmp = sr.ReadLine()) != null)
                        {
                            line = lineTmp;
                            i++;
                        }
                    }
                } else
                {
                    Helpers.ConsolePrint("readCSV", "File miners\\miniz\\" + logFile + " not exist");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("readCSV", ex.ToString());
                return 0;
            }

            int c = 0;
            try
            {
                string[] headrow = headLine.Split(',');
                int pos = headLine.IndexOf("sols_" + gpiId.ToString());
                if (pos < 0)
                {
                    //   pos = headLine.IndexOf("sum_Sols");
                    try
                    {
                        if (File.Exists("miners\\miniz\\" + logFile)) File.Delete("miners\\miniz\\" + logFile);
                    }
                    catch (Exception ex)
                    {
                        Helpers.ConsolePrint("readCSV", ex.ToString());
                    }
                    return 0;
                }
                string t = headLine.Substring(0, pos);
                int startPos = t.Count(f => (f == ',')) + 1;
                string[] row = line.Split(',');
                double.TryParse(row[startPos], out double hashrate);
                return hashrate;
            } catch (Exception ex)
            {
                Helpers.ConsolePrint("readCSV", ex.ToString());
                return 0;
            }
            return 0;
        }
        protected override void _Stop(MinerStopType willswitch)
        {
            Helpers.ConsolePrint("miniZ Stop", "");
            DeviceType devtype = DeviceType.NVIDIA;
            var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();
            foreach (var mPair in sortedMinerPairs)
            {
                devtype = mPair.Device.DeviceType;
            }
            if (Form_Main.ZilMonitorRunning &&
                Form_additional_mining.isAlgoZIL(MiningSetup.AlgorithmName, MinerBaseType.miniZ, devtype))
            {
                //ZilClient.needConnectionZIL = false;
            }
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
        }

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 60 * 1000 * 5; // 5 minute max, whole waiting time 75seconds
        }
    }
}
