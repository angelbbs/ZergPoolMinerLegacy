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
using ZergPoolMiner.Stats;

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
            new Task(() => Form_Main.DelWinDivert()).Start();
            LastCommandLine = GetStartCommand(wallet, password);
            ProcessHandle = _Start();
        }
        private new string GetServer(string algo)
        {
            string ret = "";
            try
            {
                algo = algo.Replace("-", "_");
                var _a = Stats.Stats.CoinList.FirstOrDefault(item => item.algo.ToLower() == algo.ToLower());

                string serverUrl = Form_Main.regionList[ConfigManager.GeneralConfig.ServiceLocation].RegionLocation +
                    "mine.zergpool.com";

                /*
                if (algo.ToLower().Contains("karlsenhashv2") && ConfigManager.GeneralConfig.EnableProxy)
                {
                    ret = "--pool " +
                        Links.CheckDNS(algo + serverUrl) + ":" + _a.port.ToString() + " " +
                        proxy + " " +
                        "--give-up-limit 1 --retry-time 1";
                }
                else
                */
                {
                    ret = "--give-up-limit 1 --retry-time 1 --pool " +
                        //Links.CheckDNS(algo + serverUrl) + ":" + _a.port.ToString() + " --retry-time 1 ";
                        Links.CheckDNS(algo + serverUrl).Replace("stratum+tcp://", "stratum+ssl://") +
                        ":" + _a.tls_port.ToString();
                }
            } catch (Exception ex)
            {
                Helpers.ConsolePrint("GetServer", "Error in " + algo + " " + ex.ToString());
                ret = "error_in_list_of_algos.err:1111";
            }
            /*
            if (algo.ToLower().Contains("verus"))
            {
                ret = ret.Replace("verushash.mine.zergpool.com", "verushash.na.mine.zergpool.com").
                    Replace("verushash.eu.mine.zergpool.com", "verushash.na.mine.zergpool.com").
                    Replace("verushash.asia.zergpool.com", "verushash.na.mine.zergpool.com");
            }
            */
            /*
            ret = ret.Replace("ethash.mine.zergpool.com", "ethash.na.mine.zergpool.com").
                    Replace("ethash.eu.mine.zergpool.com", "ethash.na.mine.zergpool.com").
                    Replace("ethash.asia.zergpool.com", "ethash.na.mine.zergpool.com");
            */

            return ret + " ";
        }
        
        private string GetStartCommand(string wallet, string password)
        {
            string disablePlatform = "--disable-gpu-nvidia ";
            DeviceType devtype = DeviceType.NVIDIA;
            var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();
            foreach (var mPair in sortedMinerPairs)
            {
                devtype = mPair.Device.DeviceType;
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

            string proxy = "";
            if (ConfigManager.GeneralConfig.EnableProxy)
            {
                //proxy = "--proxy " + Stats.Stats.CurrentProxyIP + ":" + Stats.Stats.CurrentProxySocks5SPort + " ";
                proxy = "--proxy 127.0.0.1:" + Socks5Relay.Port;
            }

            var extras = ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, devtype);
            try
            {
                string _wallet = "--wallet " + wallet;
                string _password = " --password " + password;
                var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
                _algo = _algo.Replace("sha512256d", "sha512_256d_radiant");
                _algo = _algo.Replace("argon2d16000", "argon2d_16000");

                if (MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.NONE)
                {
                    return " --algorithm " + _algo + " " +
                disablePlatform + $"--api-enable --api-port {ApiPort} {extras} " +
                        GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower()) + " " +
                        proxy + " " +
                        _wallet + " " + _password +
                        " --gpu-id " +
                        GetDevicesCommandString().Trim();
                } else
                {
                    var _algo2 = MiningSetup.CurrentSecondaryAlgorithmType.ToString().ToLower();
                    _algo2 = _algo2.Replace("sha512256d", "sha512_256d_radiant");

                    var pass1 = password.Split(',')[2].Replace("mc=", "").Split('+')[0];
                    var pass2 = password.Split(',')[2].Replace("mc=", "").Split('+')[1];

                    return " --algorithm " + _algo + " " +
                        GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower()) + " " +
                        _wallet + " " + _password.Replace("mc=" + pass1 + "+" + pass2, "mc=" + pass1) + " " +
                        " --algorithm " + _algo2 + " " +
                        GetServer(MiningSetup.CurrentSecondaryAlgorithmType.ToString().ToLower()) + " " +
                        proxy + " " +
                        _wallet + " " + _password.Replace("mc=" + pass1 + "+" + pass2, "mc=" + pass2) + " " +
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
            new Task(() => Form_Main.DelWinDivert()).Start();
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

            string proxy = "";
            if (ConfigManager.GeneralConfig.EnableProxy)
            {
                //proxy = "--proxy " + Stats.Stats.CurrentProxyIP + ":" + Stats.Stats.CurrentProxySocks5SPort + " ";
                proxy = "--proxy 127.0.0.1:" + Socks5Relay.Port;
            }

            var extras = ExtraLaunchParametersParser.ParseForMiningSetup(MiningSetup, devtype);

            string serverUrl = Form_Main.regionList[ConfigManager.GeneralConfig.ServiceLocation].RegionLocation +
                "mine.zergpool.com";

            var algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();

            var _algo = MiningSetup.CurrentAlgorithmType.ToString().ToLower();
            _algo = _algo.Replace("sha512256d", "sha512_256d_radiant");
            _algo = _algo.Replace("argon2d16000", "argon2d_16000");

            string mainWallet = Globals.DemoUser;
            string failoverPool = "";
            string failoverWallet = "";
            string failoverPassword = "";
            string failoverPool2 = "";
            string failoverWallet2 = "";
            string failoverPassword2 = "";
            switch (MiningSetup.CurrentAlgorithmType)
            {
                case AlgorithmType.RandomX:
                    failoverPool = ",stratum+tcp://kz.vipor.net:5040";
                    failoverWallet = ",42fV4v2EC4EALhKWKNCEJsErcdJygynt7RJvFZk8HSeYA9srXdJt58D9fQSwZLqGHbijCSMqSP4mU7inEEWNyer6F7PiqeX";
                    break;
                case AlgorithmType.VerusHash:
                    failoverPool = ",stratum+ssl://pool.hashvault.pro:443";
                    failoverWallet = ",RX8dEm1eqgmXmUm4iQ1Vg5LRaxuzophkTJ";
                    break;
                case AlgorithmType.CPUPower:
                    failoverPool = ",stratum+tcp://cpupower.eu.mine.zpool.ca:6240";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.Cryptonight_UPX://tradeogre
                    failoverPool = ",stratum+tcp://fr-upx.miningocean.org:4332";
                    failoverWallet = ",UmVjjd7xBGgDB7JE7XdV9zVuutkk8LQcA6T9f4LRx1WSY7uHqJiaMYrXwPJPPUhdSniY3NYLKxt8EUF5oPJVBS8g37gsyZTm9";
                    break;
                case AlgorithmType.Flex:
                    failoverPool = ",stratum+tcp://flex.eu.mine.zpool.ca:3340";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.Ghostrider:
                    failoverPool = ",stratum+tcp://ghostrider.eu.mine.zpool.ca:5354";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.Mike:
                    failoverPool = ",stratum+tcp://mike.eu.mine.zpool.ca:5356";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.Minotaurx:
                    failoverPool = ",stratum+tcp://minotaurx.eu.mine.zpool.ca:7019";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.Panthera:
                    failoverPool = ",stratum+tcp://mine.scalaproject.io:3333";
                    failoverWallet = ",Ssy2KnStasELAdgN7Nt2XfPw7UoGrWxSX6izJY9AHaqFTGAtvhAck8Ac9UR36qgSgd6bEKdqb4KHWJotFqwNdrSH5BSLH11TnP";
                    break;
                case AlgorithmType.Xelisv2_Pepew:
                    failoverPool = ",stratum+tcp://xelisv2-pepew.eu.mine.zpool.ca:4833";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.Yespower:
                    failoverPool = ",stratum+tcp://yespower.eu.mine.zpool.ca:6234";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.YespowerLTNCG:
                    failoverPool = ",stratum+tcp://yespowerLTNCG.eu.mine.zpool.ca:6245";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.YespowerMGPC:
                    failoverPool = ",stratum+tcp://yespowerMGPC.eu.mine.zpool.ca:6247";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.YespowerR16:
                    failoverPool = ",stratum+tcp://yespowerR16.eu.mine.zpool.ca:6534";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.YespowerSUGAR:
                    failoverPool = ",stratum+tcp://yespowerSUGAR.eu.mine.zpool.ca:6241";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.YespowerTIDE:
                    failoverPool = ",stratum+tcp://yespowerTIDE.eu.mine.zpool.ca:6239";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.YespowerURX:
                    failoverPool = ",stratum+tcp://yespowerURX.eu.mine.zpool.ca:6236";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.YespowerADVC:
                    failoverPool = ",stratum+tcp://yespowerADVC.eu.mine.zpool.ca:6248";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.RandomARQ:
                    failoverPool = ",stratum+tcp://arqma.supportcryptonight.com:9633";
                    failoverWallet = $",aRi1eFMtoYTV1aXoe7NzFr7i39sKzd9xPdaCZkqqUUU9PPcbAN3YpeuM6cU6ifjyEKcvFw4g5y9UH9p2DZT6N4VNNrkFrsvyzod2r7m5v9Srj";
                    break;
                case AlgorithmType.RandomXEQ:
                    failoverPool = ",stratum+tcp://fastpool.xyz:6265";
                    failoverWallet = $",Tsz55sboLxy8An46s69h9wKwZTAg762yC2DAS2bioW9f1br6isYTromTMjjUazYad39jLsD8eYRR9e328BnCpAvh4Rya5HtTUs";
                    break;
                case AlgorithmType.Yescrypt:
                    failoverPool = ",stratum+tcp://yescrypt.eu.mine.zpool.ca:6233";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.YescryptR8:
                    failoverPool = ",stratum+tcp://yescryptR8.eu.mine.zpool.ca:6323";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.YescryptR16:
                    failoverPool = ",stratum+tcp://yescryptR16.eu.mine.zpool.ca:6333";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.YescryptR32:
                    failoverPool = ",stratum+tcp://yescryptR32.eu.mine.zpool.ca:6343";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.Power2b:
                    failoverPool = ",stratum+tcp://power2b.eu.mine.zpool.ca:6242";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.RinHash:
                    failoverPool = ",stratum+tcp://pool.rplant.xyz:7148";
                    failoverWallet = $",rin1xxxxxxxxx1";//fake
                    break;

                case AlgorithmType.VertHash:
                    failoverPool = ",stratum+tcp://verthash.eu.mine.zpool.ca:6144";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.SHA256dt:
                    failoverPool = ",stratum+tcp://sha256.eu.mine.zpool.ca:3333";//fake
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.Ethash:
                    failoverPool = ",ethw.2miners.com:2020";
                    failoverWallet = $",bc1qun08kg08wwdsszrymg8z4la5d6ygckg9nxh4pq";
                    break;
                case AlgorithmType.Ethashb3:
                    failoverPool = ",stratum+tcp://eu.mining4people.com:3454";
                    failoverWallet = $",0xcd0E6454702D676B165cE7Dc6E42f3F692f7F147";
                    break;
                case AlgorithmType.EvrProgPow:
                    failoverPool = ",stratum+tcp://eu.evrpool.org:1111";
                    failoverWallet = $",EbdCsvB491DULZQjfpBGKEhaHURDEFH9Rk";
                    break;
                case AlgorithmType.FiroPow:
                    failoverPool = ",stratum+tcp://firopow.eu.mine.zpool.ca:1326";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.HeavyHash:
                    failoverPool = ",stratum+tcp://heavyhash.eu.mine.zpool.ca:5138";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.KarlsenHashV2:
                    failoverPool = ",stratum+tcp://pool.woolypooly.com:3132";
                    failoverWallet = $",karlsen:qzads9u466f9zvuk79em9er2a4tcy4h023v22araf9wka9etdcf02dxv2rec9";
                    break;
                case AlgorithmType.KawPow:
                    failoverPool = ",stratum+tcp://rvn.2miners.com:6060";
                    failoverWallet = $",bc1qun08kg08wwdsszrymg8z4la5d6ygckg9nxh4pq";
                    break;
                case AlgorithmType.Meraki:
                    failoverPool = ",stratum+tcp://meraki.eu.mine.zpool.ca:3387";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.Cryptonight_GPU:
                    failoverPool = ",stratum+tcp://pool.ryo-currency.com:3333";
                    failoverWallet = $",RYoNsCHsnFSQLS5JUWzd9ee7N7pe44gTE9JUmjYHUgT6RRLL76x35pwiJ9aVU141s6JnBachuzCJWcPFn9zEdvM9WhsSswFbwzB5tKi6LLL6Zh";
                    break;
                case AlgorithmType.HooHash:
                    failoverPool = ",stratum+tcp://nushypool.com:40012";
                    failoverWallet = $",hoosat:qzuy0ydzzmw82ffa8j30m724w4cmwnxk9864meytpkgs0y502pmwzk886446m";
                    break;
                case AlgorithmType.ProgPowZ:
                    failoverPool = ",stratum+tcp://zano.luckypool.io:8866";
                    failoverWallet = $",iZ2NyxEHg87VTyQzUyYL7zgSDbA9Q3zk9V9kZQKwm2ucCw43nHXaTLWVhrDW3Up5tFgEjjZi6Yxh6gouWdLCKddxRVwBGFkFXQw4t5PxP";
                    break;
                case AlgorithmType.CurveHash:
                    failoverPool = ",stratum+tcp://curve.eu.mine.zpool.ca:4633";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.PhiHash:
                    failoverPool = ",stratum+tcp://eu.neuropool.net:10110";
                    failoverWallet = $",PmM5d7PbhocvoUgFK3E1tdNBeN1hU2ipXs";//exbitron
                    break;
                case AlgorithmType.MeowPow:
                    failoverPool = ",stratum+tcp://meowpow.eu.mine.zpool.ca:1327";
                    failoverWallet = $",{Globals.DemoUser}";
                    break;
                default:
                    break;
            }

            switch (MiningSetup.CurrentSecondaryAlgorithmType)
            {
                case AlgorithmType.SHA256dt:
                    failoverPool2 = ",stratum+tcp://sha256.eu.mine.zpool.ca:3333";//fake
                    failoverWallet2 = $",{Globals.DemoUser}";
                    break;
                case AlgorithmType.HooHash:
                    failoverPool2 = ",stratum+tcp://nushypool.com:40012";
                    failoverWallet2 = $",hoosat:qzuy0ydzzmw82ffa8j30m724w4cmwnxk9864meytpkgs0y502pmwzk886446m";
                    break;
                default:
                    break;
            }
            //,mc=* без этого не работает бенч meraki
            if (MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.NONE)
            {
                var mainpool = GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower()).Trim();
                if (mainpool.Contains("error"))
                {
                    mainpool = " --pool " + failoverPool.Replace(",", "");
                    failoverPool = "";
                    mainWallet = "";
                    failoverWallet = failoverWallet.Replace(",", "");
                }

                return " " + disablePlatform + "--algorithm " + _algo + " " +
                    mainpool + failoverPool + " " +
                    $"--wallet {mainWallet}{failoverWallet} --password c=LTC{failoverPassword},mc=*" + " " +
                    proxy + " " +
                    $"--api-enable --api-port {ApiPort} {extras}" + " --give-up-limit 1 --retry-time 1 --gpu-id " +
                    GetDevicesCommandString().Trim();
            }
            else
            {
                var _algo2 = MiningSetup.CurrentSecondaryAlgorithmType.ToString().ToLower();
                _algo2 = _algo2.Replace("sha512256d", "sha512_256d_radiant");

                return " " + disablePlatform + "--algorithm " + _algo + " " +
                     GetServer(MiningSetup.CurrentAlgorithmType.ToString().ToLower()).Trim() + failoverPool + " " +
                    $"--wallet {Globals.DemoUser}{failoverWallet} --password c=LTC{failoverPassword},mc=*" + " " +
                    "--algorithm " + _algo2 + " " +
                    GetServer(MiningSetup.CurrentSecondaryAlgorithmType.ToString().ToLower()).Trim() + failoverPool2 + " " +
                    $"--wallet {Globals.DemoUser}{failoverWallet2} --password c=LTC{failoverPassword2},mc=*" + " " +
                     proxy + " " +
                    $"--api-enable --api-port {ApiPort} {extras}" + " --give-up-limit 1 --retry-time 1 --gpu-id " +
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
                Helpers.ConsolePrint("SRBMiner API Exception", ex.Message);
                CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                ad.Speed = 0;
                ad.SecondarySpeed = 0;
                ad.ThirdSpeed = 0;
                return ad;
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

                if (resp != null)
                {
                    if (MiningSetup.CurrentSecondaryAlgorithmType.Equals(AlgorithmType.NONE) &&
                        devtype != DeviceType.CPU)//single,
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

                    if (!MiningSetup.CurrentSecondaryAlgorithmType.Equals(AlgorithmType.NONE) &&
                        devtype != DeviceType.CPU)//dual 
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
                            catch (OverflowException ex)
                            {
                                Helpers.ConsolePrint("API OverflowException Exception:", ex.ToString());
                                Restart();
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
                //Helpers.ConsolePrint("API error", ex.ToString());
                CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                ad.Speed = 0;
                return ad;
            }

            ad.Speed = totalsMain;
            ad.SecondarySpeed = totalsSecond;
            ad.ThirdSpeed = 0;
            ad.ThirdAlgorithmID = AlgorithmType.NONE;

            if (MiningSetup.CurrentSecondaryAlgorithmType != AlgorithmType.NONE)//dual
            {
                ad.Speed = totalsMain;
                ad.SecondarySpeed = totalsSecond;
            }
            else
            {
                ad.Speed = totalsMain;
                ad.SecondarySpeed = 0;
                ad.SecondaryAlgorithmID = AlgorithmType.NONE;
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
