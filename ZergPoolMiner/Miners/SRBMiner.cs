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
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ZergPoolMiner.Miners
{
    public class SRBMiner : Miner
    {
        private readonly int GPUPlatformNumber;
        private int benchmarkTimeWait = 180;
        private int _benchmarkTimeWait = 180;

        private const int TotalDelim = 2;
        private double _power = 0.0d;

        public SRBMiner() : base("SRBMiner")
        {
            CurrentMinerReadStatus = MinerApiReadStatus.GOT_READ;
            GPUPlatformNumber = ComputeDeviceManager.Available.AmdOpenCLPlatformNum;
        }

        public override void Start(string wallet, string password)
        {
            LastCommandLine = GetStartCommand(wallet, password);
            ProcessHandle = _Start();
        }
        private new string GetServer(string algo)
        {
            string ret = "";
            try
            {
                algo = algo.Replace("-", "_");
                var _a = Stats.Stats.MiningAlgorithmsList.FirstOrDefault(item => item.name.ToLower() == algo.ToLower());

                string serverUrl = Form_Main.regionList[ConfigManager.GeneralConfig.ServiceLocation].RegionLocation +
                    "mine.zergpool.com";

                ret = "--pool " + Links.CheckDNS(algo + serverUrl).Replace("stratum+tcp://", "stratum+ssl://") +
                    ":" + _a.tls_port.ToString();
            } catch (Exception ex)
            {
                Helpers.ConsolePrint("GetServer", "Error in " + algo + " " + ex.ToString());
                ret = "error_in_list_of_algos.err:1111";
            }

            return ret + " ";
        }
        
        private string GetStartCommand(string wallet, string password)
        {
            string ZilMining = "";
            string disablePlatform = "--disable-gpu-nvidia ";
            DeviceType devtype = DeviceType.NVIDIA;
            var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();
            foreach (var mPair in sortedMinerPairs)
            {
                devtype = mPair.Device.DeviceType;
            }

            if (Form_additional_mining.isAlgoZIL(MiningSetup.AlgorithmName, MinerBaseType.SRBMiner, devtype))
            {
                ZilClient.needConnectionZIL = true;
                ZilClient.StartZilMonitor();
            }

            if (Form_additional_mining.isAlgoZIL(MiningSetup.AlgorithmName, MinerBaseType.SRBMiner, devtype) &&
                ConfigManager.GeneralConfig.ZIL_mining_state == 1)
            {
                //прокси не используется
                ZilMining = " --zil-enable --zil-pool stratum+tcp://daggerhashimoto.auto.nicehash.com:9200 --zil-wallet " +
                            ConfigManager.GeneralConfig.Wallet + " --zil-esm 2 --disable-worker-watchdog ";
            }
            if (Form_additional_mining.isAlgoZIL(MiningSetup.AlgorithmName, MinerBaseType.SRBMiner, devtype) &&
                ConfigManager.GeneralConfig.ZIL_mining_state == 2)
            {
                //прокси не используется
                ZilMining = " --zil-enable --zil-pool " + ConfigManager.GeneralConfig.ZIL_mining_pool + ":" +
                    ConfigManager.GeneralConfig.ZIL_mining_port + " --zil-wallet " +
                            ConfigManager.GeneralConfig.ZIL_mining_wallet + "." + "worker" + " --zil-esm 2 --disable-worker-watchdog ";
            }

            if (devtype == DeviceType.CPU)
            {
                disablePlatform = "--disable-gpu ";
            }
            if (devtype == DeviceType.AMD)
            {
                disablePlatform = "--disable-cpu --disable-gpu-nvidia --disable-gpu-intel ";
            }
            if (devtype == DeviceType.INTEL)
            {
                disablePlatform = "--disable-cpu --disable-gpu-nvidia --disable-gpu-amd ";
            }
            if (devtype == DeviceType.NVIDIA)
            {
                disablePlatform = "--disable-cpu --disable-gpu-intel --disable-gpu-amd ";
            }

            var extras = ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, devtype);
            try
            {
                string _wallet = "--wallet " + wallet;
                string _password = " --password " + password;
                var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
                _algo = _algo.Replace("sha512256d", "sha512_256d_radiant");

                if (MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.NONE)
                {
                    return " --algorithm " + _algo + " " +
                disablePlatform + $"--api-enable --api-port {ApiPort} {extras} " +
                        GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower()) + " " +
                        _wallet + " " + _password +
                        " --gpu-id " +
                        GetDevicesCommandString().Trim();
                } else
                {
                    var _algo2 = MiningSetup.CurrentSecondaryAlgorithmType.ToString().ToLower();
                    _algo2 = _algo2.Replace("sha512256d", "sha512_256d_radiant");

                    return " --algorithm " + _algo + " " +
                        GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower()) + " " +
                        _wallet + " " + _password + " " +
                        " --algorithm " + _algo2 + " " +
                        GetServer(MiningSetup.CurrentSecondaryAlgorithmType.ToString().ToLower()) + " " +
                        _wallet + " " + _password +
                        " " + disablePlatform + $"--api-enable --api-port {ApiPort} {extras} " +
                        " --gpu-id " +
                        GetDevicesCommandString().Trim();
                }

            } catch (Exception ex)
            {
                Helpers.ConsolePrint("GetStartCommand", ex.ToString());
            }
            
            return "unsupported algo";

        }

        protected override string GetDevicesCommandString()
        {
            ad = new ApiData(MiningSetup.CurrentAlgorithmType, MiningSetup.CurrentSecondaryAlgorithmType, MiningSetup.MiningPairs[0]);
            ad.ThirdAlgorithmID = AlgorithmType.NONE;
            
            var deviceStringCommand = " ";
            var ids = MiningSetup.MiningPairs.Select(mPair => mPair.Device.IDByBus.ToString()).ToList();
            ids.Sort();
            deviceStringCommand += string.Join("!", ids);

            return deviceStringCommand;
        }
        private string GetStartBenchmarkCommand(string btcAddress, string worker)
        {
            string disablePlatform = "--disable-gpu-nvidia ";
            DeviceType devtype = DeviceType.NVIDIA;
            var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();
            foreach (var mPair in sortedMinerPairs)
            {
                devtype = mPair.Device.DeviceType;
            }
            BenchmarkAlgorithm.DeviceType = devtype;

            if (devtype == DeviceType.CPU)
            {
                disablePlatform = "--disable-gpu ";
            }
            if (devtype == DeviceType.AMD)
            {
                disablePlatform = "--disable-cpu --disable-gpu-nvidia --disable-gpu-intel ";
            }
            if (devtype == DeviceType.INTEL)
            {
                disablePlatform = "--disable-cpu --disable-gpu-nvidia --disable-gpu-amd ";
            }
            if (devtype == DeviceType.NVIDIA)
            {
                disablePlatform = "--disable-cpu --disable-gpu-intel --disable-gpu-amd ";
            }

            var extras = ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, devtype);

            string serverUrl = Form_Main.regionList[ConfigManager.GeneralConfig.ServiceLocation].RegionLocation +
                "mine.zergpool.com";

            var algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
            Stats.Stats.MiningAlgorithms _a = new();
            
            var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
            _algo = _algo.Replace("sha512256d", "sha512_256d_radiant");

            if (MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.NONE)
            {
                return " " + disablePlatform + "--algorithm " + _algo + " " +
                    GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower()) +
                    $" --wallet {Globals.DemoUser} --password c=LTC" +
                    $" --api-enable --api-port {ApiPort} {extras}" + " --gpu-id " +
                    GetDevicesCommandString().Trim();
            }
            else
            {

                var _algo2 = MiningSetup.CurrentSecondaryAlgorithmType.ToString().ToLower();
                _algo2 = _algo2.Replace("sha512256d", "sha512_256d_radiant");

                return " " + disablePlatform + "--algorithm " + _algo + " " +
                     GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower()) +
                    $" --wallet {Globals.DemoUser} --password c=LTC" +
                    " --algorithm " + _algo2 + " " +
                    GetServer(MiningSetup.CurrentSecondaryAlgorithmType.ToString().ToLower()) +
                    $" --wallet {Globals.DemoUser} --password c=LTC" +
                    $" --api-enable --api-port {ApiPort} {extras}" + " --gpu-id " +
                    GetDevicesCommandString().Trim();
            }
        }

        protected override void _Stop(MinerStopType willswitch)
        {
            Helpers.ConsolePrint("SRBMINER Stop", "");
            DeviceType devtype = DeviceType.AMD;
            var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();
            foreach (var mPair in sortedMinerPairs)
            {
                devtype = mPair.Device.DeviceType;
            }

            Stop_cpu_ccminer_sgminer_nheqminer(willswitch);
            StopDriver();
        }

        private void StopDriver()
        {
            //srbminer driver
            var CMDconfigHandleWD = new Process

            {
                StartInfo =
                {
                    FileName = "sc.exe"
                }
            };

            CMDconfigHandleWD.StartInfo.Arguments = "stop winio";
            CMDconfigHandleWD.StartInfo.UseShellExecute = false;
            CMDconfigHandleWD.StartInfo.CreateNoWindow = true;
            CMDconfigHandleWD.Start();
        }

        protected override int GetMaxCooldownTimeInMilliseconds()
        {
            return 60 * 1000 * 5;  // 5 min
        }

        private ApiData ad;
        public override ApiData GetApiData()
        {
            return ad;
        }

        public override async Task<ApiData> GetSummaryAsync()
        {
            string ResponseFromSRBMiner;
            try
            {
                HttpWebRequest WR = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:" + ApiPort.ToString());
                WR.UserAgent = "GET / HTTP/1.1\r\n\r\n";
                WR.Timeout = 3 * 1000;
                WR.Credentials = CredentialCache.DefaultCredentials;
                WebResponse Response = WR.GetResponse();
                Stream SS = Response.GetResponseStream();
                SS.ReadTimeout = 4 * 1000;
                StreamReader Reader = new StreamReader(SS);
                ResponseFromSRBMiner = await Reader.ReadToEndAsync();

                Reader.Close();
                Response.Close();
                WR.Abort();
                SS.Close();
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("API Exception", ex.Message);
                CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                return null;
            }

            dynamic resp = JsonConvert.DeserializeObject(ResponseFromSRBMiner);
            //Helpers.ConsolePrint("API ->:", ResponseFromSRBMiner.ToString());
            
            if (!MiningSetup.CurrentSecondaryAlgorithmType.Equals(AlgorithmType.NONE))
            {
                ad.SecondaryAlgorithmID = MiningSetup.CurrentSecondaryAlgorithmType;
            }

            double totalsMain = 0;
            double totalsSecond = 0;
            double totalsThird = 0;

            try
            {
                DeviceType devtype = DeviceType.AMD;
                var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();
                foreach (var mPair in sortedMinerPairs)
                {
                    devtype = mPair.Device.DeviceType;
                }
                ad.ZilRound = false;
                if (resp != null)
                {
                    if (MiningSetup.CurrentSecondaryAlgorithmType.Equals(AlgorithmType.NONE) &&
                        !ResponseFromSRBMiner.ToLower().Contains("\"name\": \"zil\"") &&
                        devtype != DeviceType.CPU)//single, no zil
                    {
                        foreach (var mPair in sortedMinerPairs)
                        {
                            try
                            {
                                string token = $"algorithms[0].hashrate.gpu.gpu{mPair.Device.IDByBus}";
                                var hash = resp.SelectToken(token);
                                int gpu_hr = (int)Convert.ToDouble(hash, CultureInfo.InvariantCulture.NumberFormat);
                                mPair.Device.MiningHashrate = gpu_hr;
                                _power = mPair.Device.PowerUsage;
                                mPair.Device.AlgorithmID = (int)MiningSetup.CurrentAlgorithmType;
                                mPair.Device.SecondAlgorithmID = (int)MiningSetup.CurrentSecondaryAlgorithmType;
                                mPair.Device.ThirdAlgorithmID = (int)AlgorithmType.NONE;
                            }
                            catch (Exception ex)
                            {
                                Helpers.ConsolePrint("API Exception:", ex.ToString());
                            }
                        }
                        dynamic _tm = resp.algorithms[0].hashrate.gpu.total;
                        if (_tm != null)
                        {
                            totalsMain = resp.algorithms[0].hashrate.gpu.total;
                        }
                    }

                    if (MiningSetup.CurrentSecondaryAlgorithmType.Equals(AlgorithmType.NONE) &&
                        ResponseFromSRBMiner.ToLower().Contains("\"name\": \"zil\"") &&
                        devtype != DeviceType.CPU)//single, + zil
                    {
                        foreach (var mPair in sortedMinerPairs)
                        {
                            try
                            {
                                string token0 = $"algorithms[0].hashrate.gpu.gpu{mPair.Device.IDByBus}";
                                var hash0 = resp.SelectToken(token0);
                                int gpu_hr0 = (int)Convert.ToInt32(hash0, CultureInfo.InvariantCulture.NumberFormat);
                                mPair.Device.MiningHashrate = gpu_hr0;

                                string token1 = $"algorithms[1].hashrate.gpu.gpu{mPair.Device.IDByBus}";
                                var hash1 = resp.SelectToken(token1);
                                int gpu_hr1 = (int)Convert.ToInt32(hash1, CultureInfo.InvariantCulture.NumberFormat);
                                mPair.Device.MiningHashrateSecond = gpu_hr1;

                                string tokenerrors = $"algorithms[0].gpu_compute_errors.gpu{mPair.Device.IDByBus}";
                                var hasherror = resp.SelectToken(tokenerrors);
                                int gpu_compute_errors = (int)Convert.ToInt32(hasherror, CultureInfo.InvariantCulture.NumberFormat);
                                //total_gpu_compute_errors = + gpu_compute_errors;
                                /*
                                if (gpu_compute_errors >= 10)
                                {
                                    Helpers.ConsolePrint("GetSummaryAsync", "RESTART SRBMiner due rejects above limit: " + total_gpu_compute_errors.ToString());
                                    total_gpu_compute_errors = 0;
                                    Restart();
                                }
                                */
                                if (Form_Main.isZilRound)
                                {
                                    mPair.Device.MiningHashrate = 0;
                                    mPair.Device.MiningHashrateThird = 0;
                                    mPair.Device.AlgorithmID = (int)AlgorithmType.NONE;
                                    mPair.Device.SecondAlgorithmID = (int)AlgorithmType.Ethash;
                                    mPair.Device.ThirdAlgorithmID = (int)AlgorithmType.NONE;
                                }
                                else
                                {
                                    mPair.Device.AlgorithmID = (int)MiningSetup.CurrentAlgorithmType;
                                    mPair.Device.MiningHashrateSecond = 0;
                                    mPair.Device.MiningHashrateThird = 0;
                                    mPair.Device.SecondAlgorithmID = (int)AlgorithmType.NONE;
                                    mPair.Device.ThirdAlgorithmID = (int)AlgorithmType.NONE;
                                }
                                _power = mPair.Device.PowerUsage;
                            }
                            catch (Exception ex)
                            {
                                Helpers.ConsolePrint("API Exception:", ex.ToString());
                            }
                        }
                        try
                        {
                            totalsMain = resp.algorithms[0].hashrate.gpu.total;
                            totalsSecond = resp.algorithms[1].hashrate.gpu.total;
                        }
                        catch
                        {

                        }
                        /*
                        Helpers.ConsolePrint("******", "isZilRound?: " + Form_Main.isZilRound.ToString() +
                            " totalsMain: " + totalsMain.ToString() +
                            " totalsSecond: " + totalsSecond.ToString());
                        */
                    }

                    if (!MiningSetup.CurrentSecondaryAlgorithmType.Equals(AlgorithmType.NONE) &&
                        devtype != DeviceType.CPU)//dual no zil
                    {
                        foreach (var mPair in sortedMinerPairs)
                        {
                            try
                            {
                                string token0 = $"algorithms[0].hashrate.gpu.gpu{mPair.Device.IDByBus}";
                                var hash0 = resp.SelectToken(token0);
                                int gpu_hr0 = (int)Convert.ToInt32(hash0, CultureInfo.InvariantCulture.NumberFormat);

                                string token1 = $"algorithms[1].hashrate.gpu.gpu{mPair.Device.IDByBus}";
                                var hash1 = resp.SelectToken(token1);
                                int gpu_hr1 = (int)Convert.ToInt32(hash1, CultureInfo.InvariantCulture.NumberFormat);

                                mPair.Device.MiningHashrate = gpu_hr0;
                                mPair.Device.MiningHashrateSecond = gpu_hr1;
                                _power = mPair.Device.PowerUsage;
                                mPair.Device.AlgorithmID = (int)MiningSetup.CurrentAlgorithmType;
                                mPair.Device.SecondAlgorithmID = (int)MiningSetup.CurrentSecondaryAlgorithmType;
                                mPair.Device.ThirdAlgorithmID = (int)AlgorithmType.NONE;
                            }
                            catch (Exception ex)
                            {
                                Helpers.ConsolePrint("API Exception:", ex.ToString());
                            }
                        }
                        try
                        {
                            totalsMain = resp.algorithms[0].hashrate.gpu.total;
                        }
                        catch
                        {
                            //totalsMain = resp.algorithms[0].hashrate.1min;
                        }
                        try
                        {
                            totalsSecond = resp.algorithms[1].hashrate.gpu.total;
                        }
                        catch
                        {
                            totalsSecond = 0;
                        }
                    }

                    if (!MiningSetup.CurrentSecondaryAlgorithmType.Equals(AlgorithmType.NONE) &&
                        ResponseFromSRBMiner.ToLower().Contains("\"name\": \"zil\"") &&
                        devtype != DeviceType.CPU)//dual + zil
                    {
                        foreach (var mPair in sortedMinerPairs)
                        {
                            try
                            {
                                string token0 = $"algorithms[0].hashrate.gpu.gpu{mPair.Device.IDByBus}";
                                var hash0 = resp.SelectToken(token0);
                                int gpu_hr0 = (int)Convert.ToInt32(hash0, CultureInfo.InvariantCulture.NumberFormat);

                                string token1 = $"algorithms[1].hashrate.gpu.gpu{mPair.Device.IDByBus}";
                                var hash1 = resp.SelectToken(token1);
                                int gpu_hr1 = (int)Convert.ToInt32(hash1, CultureInfo.InvariantCulture.NumberFormat);

                                string token2 = $"algorithms[2].hashrate.gpu.gpu{mPair.Device.IDByBus}";
                                var hash2 = resp.SelectToken(token2);
                                int gpu_hr2 = (int)Convert.ToInt32(hash2, CultureInfo.InvariantCulture.NumberFormat);

                                mPair.Device.MiningHashrate = gpu_hr0;
                                mPair.Device.MiningHashrateSecond = gpu_hr1;
                                mPair.Device.MiningHashrateThird = gpu_hr2;

                                if (Form_Main.isZilRound)
                                {
                                    mPair.Device.MiningHashrate = 0;
                                    mPair.Device.MiningHashrateSecond = 0;
                                    mPair.Device.AlgorithmID = (int)AlgorithmType.NONE;
                                    mPair.Device.SecondAlgorithmID = (int)AlgorithmType.NONE;
                                    mPair.Device.ThirdAlgorithmID = (int)AlgorithmType.Ethash;
                                }
                                else
                                {
                                    mPair.Device.MiningHashrateThird = 0;
                                    mPair.Device.ThirdAlgorithmID = (int)AlgorithmType.NONE;
                                }

                                _power = mPair.Device.PowerUsage;
                            }
                            catch (Exception ex)
                            {
                                Helpers.ConsolePrint("API Exception:", ex.ToString());
                            }
                        }
                        totalsMain = resp.algorithms[0].hashrate.gpu.total;
                        totalsSecond = resp.algorithms[1].hashrate.gpu.total;
                        totalsThird = resp.algorithms[2].hashrate.gpu.total;
                    }

                    if (devtype == DeviceType.CPU)
                    {
                        try
                        {
                            totalsMain = resp.algorithms[0].hashrate.cpu.total;
                        }
                        catch (Exception ex)
                        {
                            totalsMain = 0;
                        }
                        foreach (var mPair in sortedMinerPairs)
                        {
                            mPair.Device.MiningHashrate = totalsMain;
                            _power = mPair.Device.PowerUsage;
                            mPair.Device.AlgorithmID = (int)MiningSetup.CurrentAlgorithmType;
                            mPair.Device.SecondAlgorithmID = (int)MiningSetup.CurrentSecondaryAlgorithmType;
                            mPair.Device.ThirdAlgorithmID = (int)AlgorithmType.NONE;
                        }
                    }


                    ad.Speed = totalsMain;
                    ad.SecondarySpeed = totalsSecond;
                    ad.ThirdSpeed = totalsThird;

                    if (ad.Speed + ad.SecondarySpeed + ad.ThirdSpeed == 0)
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
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("API error", ex.ToString());
                CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                ad.Speed = 0;
                return ad;
            }

            ad.ZilRound = false;
            ad.Speed = totalsMain;
            ad.SecondarySpeed = totalsSecond;
            ad.ThirdSpeed = totalsThird;

            if (Form_Main.isZilRound)
            {
                if (MiningSetup.CurrentSecondaryAlgorithmType != AlgorithmType.NONE)//dual
                {
                    if (ResponseFromSRBMiner.ToLower().Contains("\"name\": \"zil\""))//dual+zil
                    {
                        ad.Speed = 0;
                        ad.SecondarySpeed = 0;
                        ad.ThirdSpeed = totalsThird;
                        ad.ZilRound = true;
                        ad.AlgorithmID = AlgorithmType.NONE;
                        ad.SecondaryAlgorithmID = AlgorithmType.NONE;
                        ad.ThirdAlgorithmID = AlgorithmType.Ethash;
                    }
                }
                else
                {
                    if (ResponseFromSRBMiner.ToLower().Contains("\"name\": \"zil\"") && totalsSecond > 0)//+zil
                    {
                        ad.Speed = 0;
                        ad.SecondarySpeed = totalsSecond;
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
                        ad.Speed = totalsMain;
                        ad.SecondarySpeed = totalsSecond;
                    }
                }
                else
                {
                    //if (_algo.ToLower().Contains("zil"))
                    {
                        ad.Speed = totalsMain;
                        ad.SecondarySpeed = 0;
                        ad.SecondaryAlgorithmID = AlgorithmType.NONE;
                    }
                }
            }


            Thread.Sleep(1);
            return ad;
        }

        protected override bool IsApiEof(byte third, byte second, byte last)
        {
            return third == 0x7d && second == 0xa && last == 0x7d;
        }

        #region Benchmark

        protected override string BenchmarkCreateCommandLine(Algorithm algorithm, int time)
        {
            benchmarkTimeWait = time;
            _benchmarkTimeWait = time;
            return GetStartBenchmarkCommand(Globals.DemoUser, Miner.GetFullWorkerName());
        }
        
        protected override void BenchmarkOutputErrorDataReceivedImpl(string outdata)
        {
            CheckOutdata(outdata);
        }
        protected override bool BenchmarkParseLine(string outdata)
        {
            return true;
        }
        #endregion
    }

}
