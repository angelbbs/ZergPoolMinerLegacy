using ZergPoolMiner.Algorithms;
using ZergPoolMiner.Configs;
using ZergPoolMiner.Devices;
using ZergPoolMiner.Miners.Grouping;
using ZergPoolMiner.Miners.Parsing;
using ZergPoolMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZergPoolMiner.Miners
{
    class teamredminer : Miner
    {
        private readonly int GPUPlatformNumber;
        Stopwatch _benchmarkTimer = new Stopwatch();
        private int _benchmarkTimeWait = 180;
        private double _power = 0.0d;
        double _powerUsage = 0;
        int _apiErrors = 0;

        public teamredminer()
            : base("teamredminer")
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
            //    Killteamredminer();
        }

        /*
        private string GetServerDual(string algo, string algoDual, string algoDualPrefix, string username, string port, string portDual)
        {
            string ret = "";
            string ssl = "";
            string psw = "x";
            if (ConfigManager.GeneralConfig.StaleProxy) psw = "stale";
            if (ConfigManager.GeneralConfig.ProxySSL)//teamredminer почему-то падает при подключении к серверам с сертификатом
                                                     //letsencrypt. С локальным самоподписанным всё хорошо
            {
                port = "1" + port;
                portDual = "1" + portDual;
                ssl = "stratum+tcp://";
            }
            else
            {
                port = "1" + port;
                portDual = "1" + portDual;
                ssl = "stratum+tcp://";
            }
            foreach (string serverUrl in Globals.MiningLocation)
            {
                if (serverUrl.Contains("auto"))
                {
                    ret = ret + "-o stratum+tcp://" + Links.CheckDNS(algo + "." + serverUrl).Replace("stratum+tcp://", "") + ":9200 -u " +
                        username + " -p " + psw + " " +
                        algoDualPrefix + " -o stratum+tcp://" + Links.CheckDNS(algoDual + "." + serverUrl).Replace("stratum+tcp://", "") + ":9200 -u " +
                        username + " -p " + psw + " ";
                    if (!ConfigManager.GeneralConfig.ProxyAsFailover) break;
                }
                else
                {
                    ret = ret + "-o " + ssl + Links.CheckDNS(algo + "." + serverUrl).Replace("stratum+tcp://", "") + ":" + port + " -u " +
                        username + " -p " + psw + " " +
                        algoDualPrefix + " -o " + ssl + Links.CheckDNS("stratum." + serverUrl).Replace("stratum+tcp://", "") + ":" + portDual + " -u " +
                        username + " -p " + psw + " ";
                }
            }
            return ret;
        }
        */
        public string GetServer(string algo)
        {
            string ret = "";
            try
            {
                algo = algo.Replace("-", "_");
                var _a = Stats.Stats.MiningAlgorithmsList.FirstOrDefault(item => item.name.ToLower() == algo.ToLower());
                string serverUrl = Form_Main.regionList[ConfigManager.GeneralConfig.ServiceLocation].RegionLocation +
                    "mine.zergpool.com";
                ret = Links.CheckDNS(algo + serverUrl).Replace("stratum+tcp://", "") + ":" + _a.port.ToString();
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
            IsApiReadException = false;

            string algo = "";
            string algo2 = "";
            string port = "";

            string apiBind = " --api_listen=127.0.0.1:" + ApiPort;
            string apiBind2 = "";
            if (!MiningSetup.CurrentSecondaryAlgorithmType.Equals(AlgorithmType.NONE))
            {
                apiBind2 = " --api2_listen=127.0.0.1:" + "1" + ApiPort.ToString();
            }

            var sc = "";
            if (Form_Main.GetWinVer(Environment.OSVersion.Version) < 8)
            {
                sc = variables.TRMiner_add1;
            }

            string _wallet = " -u " + wallet;
            string _password = " -p " + password + " ";
            var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
            _algo = _algo.Replace("karlsenhash", "karlsen");

            LastCommandLine = sc + "" + "-a " + _algo + " " +
            "-o stratum+tcp://" + GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower()) +
            _wallet + " " + _password +
            apiBind + apiBind2 + " " +
            ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.AMD) +
            " -d " + GetDevicesCommandString();

            ProcessHandle = _Start();
        }

        protected override string GetDevicesCommandString()
        {
            var deviceStringCommand = "";
            var ids = new List<string>();
            var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.BusID).ToList();
            int id;
            foreach (var mPair in sortedMinerPairs)
            {
                id = mPair.Device.IDByBus;
                ids.Add(id.ToString());
            }

            deviceStringCommand += string.Join(",", ids);
            return deviceStringCommand;
        }
        // new decoupled benchmarking routines
        #region Decoupled benchmarking routines

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            var CommandLine = "";
            string apiBind = " --api_listen=127.0.0.1:" + ApiPort;
            string apiBind2 = "";
            if (!MiningSetup.CurrentSecondaryAlgorithmType.Equals(AlgorithmType.NONE))
            {
                apiBind2 = " --api2_listen=127.0.0.1:" + "1" + ApiPort.ToString();
            }
            var sc = "";
            _benchmarkTimeWait = time;
            if (Form_Main.GetWinVer(Environment.OSVersion.Version) < 8)
            {
                sc = variables.TRMiner_add1;
            }
            // demo for benchmark
            string username = Globals.DemoUser;
            string wallet = "-u " + Globals.DemoUser;
            string password = " -p c=LTC" +  " ";
            var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
            _algo = _algo.Replace("karlsenhash", "karlsen");

            CommandLine = sc + "" + "-a " + _algo + " " +
            " -o stratum+tcp://" + GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower()) + " " +
            wallet + " " + password +
                              apiBind +
                              " " +
                              ExtraLaunchParametersParser.ParseForMiningSetup(
                                                                MiningSetup,
                                                                DeviceType.AMD) +
                              " -d " + GetDevicesCommandString();

            /*
            if (MiningSetup.CurrentAlgorithmType.Equals(AlgorithmType.Autolykos) &&
                MiningSetup.CurrentSecondaryAlgorithmType.Equals(AlgorithmType.IronFish))
            {
                CommandLine = sc + " -a autolykos2" +
                 " -o " + Links.CheckDNS("stratum+tcp://pool.woolypooly.com:3100") + " -u 9gnVDaLeFa4ETwtrceHepPe9JeaCBGV1PxV5tdNGAvqEmjWF2Lt.teamred" + " -p x -d " + GetDevicesCommandString() +
                 " --iron -o " + Links.CheckDNS("stratum+tcp://ru.ironfish.herominers.com:1145") + " -u fb8aaaf8594143a4007c9fe0e0056bd3ca55848d0f5247f7eee8918ca8345521.teamred" + " -p x -d ";
            }
            */

            return CommandLine;

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

        private ApiData ad;
        public override ApiData GetApiData()
        {
            return ad;
        }

        protected async Task<string> GetApiDataAsync(int port, string dataToSend, bool exitHack = false,
            bool overrideLoop = false)
        {
            string responseFromServer = null;
            try
            {
                var tcpc = new TcpClient("127.0.0.1", port);
                var nwStream = tcpc.GetStream();
                nwStream.ReadTimeout = 2 * 1000;
                nwStream.WriteTimeout = 2 * 1000;

                var bytesToSend = Encoding.ASCII.GetBytes(dataToSend);
                await nwStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);

                var incomingBuffer = new byte[tcpc.ReceiveBufferSize];
                var prevOffset = -1;
                var offset = 0;
                var fin = false;

                while (!fin && tcpc.Client.Connected)
                {
                    var r = await nwStream.ReadAsync(incomingBuffer, offset, tcpc.ReceiveBufferSize - offset);
                    for (var i = offset; i < offset + r; i++)
                    {
                        if (incomingBuffer[i] == 0x7C || incomingBuffer[i] == 0x00
                                                      || (i > 2 && IsApiEof(incomingBuffer[i - 2],
                                                              incomingBuffer[i - 1], incomingBuffer[i]))
                                                      || overrideLoop)
                        {
                            fin = true;
                            break;
                        }

                        // Not working
                        //if (IncomingBuffer[i] == 0x5d || IncomingBuffer[i] == 0x5e) {
                        //    fin = true;
                        //    break;
                        //}
                    }

                    offset += r;
                    if (exitHack)
                    {
                        if (prevOffset == offset)
                        {
                            fin = true;
                            break;
                        }

                        prevOffset = offset;
                    }
                }

                tcpc.Close();

                if (offset > 0)
                    responseFromServer = Encoding.ASCII.GetString(incomingBuffer);
            }
            catch (Exception ex)
            {
                _apiErrors++;
                Helpers.ConsolePrint(MinerTag(), ProcessTag() + " GetAPIData reason: " + ex.Message);
                CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                if (_apiErrors > 60)
                {
                    _apiErrors = 0;
                    Helpers.ConsolePrint("GetApiDataAsync", "Need RESTART TEAMREDMINER");
                    CurrentMinerReadStatus = MinerApiReadStatus.RESTART;
                    ad.Speed = 0;
                    ad.SecondarySpeed = 0;
                    ad.ThirdSpeed = 0;
                    return null;
                }
                return null;
            }
            _apiErrors = 0;
            return responseFromServer;
        }
        public override async Task<ApiData> GetSummaryAsync()
        {
            CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
            var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.BusID).ToList();
            var bytesToSend = "devs";
            string resp1 = await GetApiDataAsync(ApiPort, bytesToSend);
            string resp2 = "";
            if (!MiningSetup.CurrentSecondaryAlgorithmType.Equals(AlgorithmType.NONE))
            {
                int ApiPort2 = ApiPort + 10000;
                resp2 = await GetApiDataAsync(ApiPort2, bytesToSend);
            }
            ad = new ApiData(MiningSetup.CurrentAlgorithmType, MiningSetup.CurrentSecondaryAlgorithmType);
            ad.ThirdAlgorithmID = AlgorithmType.NONE;
            if (resp1 == null)
            {
                CurrentMinerReadStatus = MinerApiReadStatus.NONE;
                ad.Speed = 0.0d;
                return null;
            }
            resp1 = resp1.Trim('\x00');
            resp2 = resp2.Trim('\x00');
           // Helpers.ConsolePrint("API1 <- ", resp1.Trim());
            //Helpers.ConsolePrint("API2 <- ", resp2.Trim());
            if (resp1.Contains("Status=Dead") || resp2.Contains("Status=Dead"))
            {
                Helpers.ConsolePrint("GetSummaryAsync", "Dead GPU detected. Need Restart miner.");
                CurrentMinerReadStatus = MinerApiReadStatus.RESTART;
                ad.Speed = 0;
                ad.SecondarySpeed = 0;
                ad.ThirdSpeed = 0;
                return ad;
            }
            try
            {
                //if (MiningSetup.CurrentSecondaryAlgorithmType.Equals(AlgorithmType.NONE))
                {
                    var devStatus = resp1.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    int dev = 0;
                    double totalSpeed = 0.0d;
                    foreach (var s in devStatus)
                    {
                        if (s.Contains("GPU="))
                        {
                            var st = s.LastIndexOf("MHS 30s=");
                            var e = s.LastIndexOf(",KHS av=");
                            string cSpeed = s.Substring(st + 8, e - st - 8);
                            //Helpers.ConsolePrint("API: ", cSpeed);
                            double.TryParse(cSpeed, out double devSpeed);
                            if (devSpeed > 1000 && MiningSetup.CurrentAlgorithmType == AlgorithmType.KawPow)//1000 MH
                            {
                                Helpers.ConsolePrint("GetSummaryAsync", "Dead GPU#" + dev.ToString() + " detected. Restart miner.");
                                CurrentMinerReadStatus = MinerApiReadStatus.RESTART;
                                ad.Speed = 0;
                                ad.SecondarySpeed = 0;
                                ad.ThirdSpeed = 0;
                                return ad;
                            }
                            sortedMinerPairs[dev].Device.MiningHashrate = devSpeed * 1000000;
                            _power = sortedMinerPairs[dev].Device.PowerUsage;
                            totalSpeed = totalSpeed + devSpeed * 1000000;
                            ad.Speed = totalSpeed;
                            dev++;
                        }

                    }
                }
                if (!MiningSetup.CurrentSecondaryAlgorithmType.Equals(AlgorithmType.NONE))
                {
                    var devStatus = resp2.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    int dev = 0;
                    double totalSpeed2 = 0.0d;
                    ad.SecondaryAlgorithmID = MiningSetup.CurrentSecondaryAlgorithmType;
                    foreach (var s in devStatus)
                    {
                        if (s.Contains("GPU="))
                        {
                            var st = s.LastIndexOf("MHS 30s=");
                            var e = s.LastIndexOf(",KHS av=");
                            string cSpeed = s.Substring(st + 8, e - st - 8);
                            //Helpers.ConsolePrint("API2: ", cSpeed);
                            double.TryParse(cSpeed, out double devSpeed2);
                            sortedMinerPairs[dev].Device.MiningHashrateSecond = devSpeed2 * 1000000;
                            _power = sortedMinerPairs[dev].Device.PowerUsage;
                            //totalSpeed = totalSpeed + devSpeed * 1000000;
                            ad.SecondarySpeed = devSpeed2 * 1000000;
                            dev++;
                        }
                    }
                }
            }
            catch
            {
                CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                return null;
            }

            CurrentMinerReadStatus = MinerApiReadStatus.GOT_READ;
            // check if speed zero
            if (ad.Speed == 0) CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;

            return ad;
        }
    }
}
