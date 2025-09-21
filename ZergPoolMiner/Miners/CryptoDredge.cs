using ZergPoolMiner.Algorithms;
using ZergPoolMiner.Configs;
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
using ZergPoolMiner.Stats;

namespace ZergPoolMiner.Miners
{
    public class CryptoDredge : Miner
    {
        public CryptoDredge() : base("CryptoDredge")
        { }
        private double Total = 0;
        private const int TotalDelim = 2;
        private int _benchmarkTimeWait = 180;
        private double _power = 0.0d;
        double _powerUsage = 0;

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 60 * 1000 * 8;
        }

        public override void Start(string wallet, string password)
        {
            if (!IsInit)
            {
                Helpers.ConsolePrint(MinerTag(), "MiningSetup is not initialized exiting Start()");
                return;
            }

            var apiBind = " --api-bind 127.0.0.1:" + ApiPort + " ";
            IsApiReadException = false;

            string _wallet = "--user " + wallet;
            string _password = " -p " + password + " ";
            var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
            _algo = _algo.Replace("cryptonight_gpu", "cngpu");

            string proxy = "";
            if (ConfigManager.GeneralConfig.EnableProxy)
            {
                //proxy = "-x socks5://" + Stats.Stats.CurrentProxyIP + ":" + Stats.Stats.CurrentProxySocks5SPort + " ";
                proxy = "-x socks5://127.0.0.1:" + Socks5Relay.Port;
            }

            LastCommandLine =  " --algo " + _algo +
            " " + apiBind +
                    " -o stratum+ssl://" + GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower()) + " " +
                    _wallet + " " + _password +
                    proxy +
                    " -d " + GetDevicesCommandString() + " " +
                ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.NVIDIA) + " ";

            
            ProcessHandle = _Start();
        }
        
        protected override string GetDevicesCommandString()
        {
            var deviceStringCommand = " ";

            var ids = MiningSetup.MiningPairs.Select(mPair => mPair.Device.ID.ToString()).ToList();
            deviceStringCommand += string.Join(",", ids);

            return deviceStringCommand;
        }
        protected override void _Stop(MinerStopType willswitch)
        {
            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
            Thread.Sleep(200);
            try {
                if (ProcessHandle is object)
                {
                    ProcessHandle.SendCtrlC((uint)Process.GetCurrentProcess().Id);
                }
            } catch 
            { }
            Thread.Sleep(200);
            foreach (var process in Process.GetProcessesByName("CryptoDredge"))
            {
                try
                {
                    process.Kill();
                    Thread.Sleep(200);
                    process.Kill();
                }
                catch (Exception e) { Helpers.ConsolePrint(MinerDeviceName, e.ToString()); }
            }
        }

        // new decoupled benchmarking routines

        #region Decoupled benchmarking routines

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            BenchmarkAlgorithm.DeviceType = DeviceType.NVIDIA;
            var apiBind = " --api-bind 127.0.0.1:" + ApiPort;
            var commandLine = "";
            _benchmarkTimeWait = time;

            string serverUrl = Form_Main.regionList[ConfigManager.GeneralConfig.ServiceLocation].RegionLocation +
                "mine.zergpool.com";
            var algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();

            var pool = "";
            var _a = Stats.Stats.CoinList.FirstOrDefault(item => item.algo.ToLower() == algo.ToLower());

            string proxy = "";
            if (ConfigManager.GeneralConfig.EnableProxy)
            {
                //proxy = "-x socks5://" + Stats.Stats.CurrentProxyIP + ":" + Stats.Stats.CurrentProxySocks5SPort + " ";
                proxy = "-x socks5://127.0.0.1:" + Socks5Relay.Port;
            }

            if (_a is object && _a != null)
            {
                pool = Links.CheckDNS(algo + serverUrl) + ":" + _a.port.ToString();
            }
            else
            {
                Helpers.ConsolePrint("CryptoDredge", "Not found " + algo + " in MiningAlgorithmsList. Try fix it.");

                _a = Stats.Stats.CoinList.FirstOrDefault(item => item.algo.ToLower() == algo.ToLower());
                pool = Links.CheckDNS(algo + serverUrl) + ":" + _a.tls_port.ToString() + " ";
            }

            string failover = "";
            switch (MiningSetup.CurrentAlgorithmType)
            {
                case AlgorithmType.Allium:
                    failover = $" -o stratum+tcp://allium.eu.mine.zpool.ca:6433 -u {Globals.DemoUser} -p c=LTC ";
                    break;
                case AlgorithmType.NeoScrypt:
                    failover = $" -o stratum+tcp://neoscrypt.eu.mine.zpool.ca:4233 -u {Globals.DemoUser} -p c=LTC ";
                    break;
                case AlgorithmType.SHA256csm:
                    failover = $" -o stratum+tcp://sha256csm.eu.mine.zpool.ca:3341 -u {Globals.DemoUser} -p c=LTC ";
                    break;
                default:
                    break;
            }

            var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
            _algo = _algo.Replace("cryptonight_gpu", "cngpu");

            return " " + "--algo " + _algo +
            failover +
            $" -o {pool} -u {Globals.DemoUser} -p c=LTC " +
            proxy +
            apiBind +
            " --retry-pause 1 -d" + GetDevicesCommandString() + " " +
                ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, DeviceType.NVIDIA) + " --no-color ";

            return commandLine;
        }

        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata)
        {
            CheckOutdata(outdata);
        }
        protected override bool BenchmarkParseLine(string outdata)
        {
            return true;
        }

        protected override void BenchmarkThreadRoutine(object commandLine)
        {
            BenchmarkSignalQuit = false;
            BenchmarkSignalHanged = false;
            BenchmarkSignalFinnished = false;
            BenchmarkException = null;
            double repeats = 0.0d;
            double summspeed = 0.0d;

            int delay_before_calc_hashrate = 10;
            int MinerStartDelay = 10;

            Thread.Sleep(ConfigManager.GeneralConfig.MinerRestartDelayMS);

            try
            {
                if (MiningSetup.CurrentAlgorithmType.Equals(AlgorithmType.FiroPow))
                {
                    delay_before_calc_hashrate = 10;
                    MinerStartDelay = 20;
                }
                if (MiningSetup.CurrentAlgorithmType.Equals(AlgorithmType.Allium))
                {
                    _benchmarkTimeWait = _benchmarkTimeWait + 120;
                }
                Helpers.ConsolePrint("BENCHMARK", "Benchmark starts");
                Helpers.ConsolePrint(MinerTag(), "Benchmark should end in: " + _benchmarkTimeWait + " seconds");
                BenchmarkHandle = BenchmarkStartProcess((string)commandLine);
                //BenchmarkHandle.WaitForExit(_benchmarkTimeWait + 2);
                var benchmarkTimer = new Stopwatch();
                benchmarkTimer.Reset();
                benchmarkTimer.Start();

                BenchmarkProcessStatus = BenchmarkProcessStatus.Running;
                BenchmarkThreadRoutineStartSettup(); //need for benchmark log
                while (IsActiveProcess(BenchmarkHandle.Id))
                {
                    if (benchmarkTimer.Elapsed.TotalSeconds >= (_benchmarkTimeWait + 60)
                        || BenchmarkSignalQuit
                        || BenchmarkSignalFinnished
                        || BenchmarkSignalHanged
                        || BenchmarkSignalTimedout
                        || BenchmarkException != null)
                    {
                        var imageName = MinerExeName.Replace(".exe", "");
                        // maybe will have to KILL process
                        EndBenchmarkProcces();
                        //  KillMinerBase(imageName);
                        if (BenchmarkSignalTimedout)
                        {
                            throw new Exception("Benchmark timedout");
                        }

                        if (BenchmarkException != null)
                        {
                            throw BenchmarkException;
                        }

                        if (BenchmarkSignalQuit)
                        {
                            throw new Exception("Termined by user request");
                        }

                        if (BenchmarkSignalFinnished)
                        {
                            break;
                        }

                        //keepRunning = false;
                        break;
                    }
                    // wait a second due api request
                    Thread.Sleep(1000);

                    var ad = GetSummaryAsync();
                    if (ad.Result != null && ad.Result.Speed > 0)
                    {
                        _powerUsage += _power;
                        repeats++;
                        double benchProgress = repeats / (_benchmarkTimeWait - MinerStartDelay - 15);
                        BenchmarkAlgorithm.BenchmarkProgressPercent = (int)(benchProgress * 100);
                        if (repeats > delay_before_calc_hashrate)
                        {
                            Helpers.ConsolePrint(MinerTag(), "Useful API Speed: " + ad.Result.Speed.ToString() + " power: " + _power.ToString());
                            summspeed += ad.Result.Speed;
                        }
                        else
                        {
                            Helpers.ConsolePrint(MinerTag(), "Delayed API Speed: " + ad.Result.Speed.ToString());
                        }

                        if (repeats >= _benchmarkTimeWait - MinerStartDelay - 15)
                        {
                            Helpers.ConsolePrint(MinerTag(), "Benchmark ended");
                            ad.Dispose();
                            benchmarkTimer.Stop();

                            BenchmarkHandle.Kill();
                            BenchmarkHandle.Dispose();
                            EndBenchmarkProcces();

                            break;
                        }

                    }
                }
                BenchmarkAlgorithm.BenchmarkSpeed = Math.Round(summspeed / (repeats - delay_before_calc_hashrate), 2);
                BenchmarkAlgorithm.PowerUsageBenchmark = (_powerUsage / repeats);
            }
            catch (Exception ex)
            {
                BenchmarkThreadRoutineCatch(ex);
            }
            finally
            {

                BenchmarkThreadRoutineFinish();
            }
        }
        #endregion // Decoupled benchmarking routines

        private ApiData ad;
        public override ApiData GetApiData()
        {
            return ad;
        }
        public override async Task<ApiData> GetSummaryAsync()
        {
            CurrentMinerReadStatus = MinerApiReadStatus.NONE;
            double tmp = 0;

            string resp = null;
            try
            {
                //var bytesToSend = Encoding.ASCII.GetBytes("summary");
                var bytesToSend = Encoding.ASCII.GetBytes("threads");
                var client = new TcpClient("127.0.0.1", ApiPort);
                var nwStream = client.GetStream();
                await nwStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
                var bytesToRead = new byte[client.ReceiveBufferSize];
                var bytesRead = await nwStream.ReadAsync(bytesToRead, 0, client.ReceiveBufferSize);
                var respStr = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                //Helpers.ConsolePrint(MinerTag(), "API: " + respStr);
                client.Close();
                resp = respStr;
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint(MinerTag(), "GetSummary exception: " + ex.Message);
                CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                ad.Speed = 0;
                ad.SecondarySpeed = 0;
                ad.ThirdSpeed = 0;
                return ad;
            }

            ad = new ApiData(MiningSetup.CurrentAlgorithmType, MiningSetup.CurrentSecondaryAlgorithmType);

            if (resp != null)
            {
                var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.ID).ToList();
                if (Form_Main.NVIDIA_orderBug)
                {
                    sortedMinerPairs.Sort((a, b) => a.Device.ID.CompareTo(b.Device.ID));
                }
                string[] mass = resp.Split('|');
                string parse = "";
                try
                {
                    for (int i = 0; i < sortedMinerPairs.Count(); i++)
                    {
                        var st = mass[i].IndexOf(";KHS=");
                        var e = mass[i].IndexOf(";KHW=");
                        parse = mass[i].Substring(st + 5, e - st - 5).Trim();
                        //double hr = Double.Parse(parse, CultureInfo.InvariantCulture);
                        Double.TryParse(parse, out double hr);
                        //Helpers.ConsolePrint(MinerTag(), parse + " - " + hr.ToString());
                        sortedMinerPairs[i].Device.MiningHashrate = hr * 1000;
                        _power = sortedMinerPairs[i].Device.PowerUsage;
                        tmp = tmp + hr;
                    }

                }
                catch (Exception e)
                {
                    Helpers.ConsolePrint(MinerTag(), "GetSummary exception: " + e.ToString());
                    /*
                    MessageBox.Show("Unsupported miner version - " + MiningSetup.MinerPath,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    */
                    BenchmarkSignalFinnished = true;
                }
                ad.Speed = tmp * 1000;

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
                    Restart();
                }
            }
            return ad;
        }


    }

}
