using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZergPoolMiner.Configs;
using ZergPoolMiner.Miners;
using ZergPoolMiner.Switching;
using ZergPoolMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.ExceptionServices;
using WebSocketSharp;
using System.Collections;
using System.Collections.Concurrent;
using System.Text;
using ZergPoolMiner.Algorithms;
using ZergPoolMinerLegacy.UUID;
using System.Threading;
using System.Globalization;
using SystemTimer = System.Timers.Timer;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Net.Http;
using SocksSharp;
using SocksSharp.Proxy;
using System.Diagnostics;
using ZergPoolMiner.Devices.Algorithms;
using ZergPoolMiner.Devices;

namespace ZergPoolMiner.Stats
{
    public class Stats
    {
        public static double Balance { get; private set; }
        public static string Version = "";

        public static List<Coin> CoinList = new();
        public class Coin
        {
            public string coinname { get; set; }
            public string algo { get; set; }
            public string symbol { get; set; }
            public double profit { get; set; }
            public double adaptive_profit { get; set; }
            public int port { get; set; }
            public int tls_port { get; set; }
            public double hashrate { get; set; }
            public double adaptive_factor { get; set; }
            public double estimate { get; set; }
            public double estimate_current { get; set; }
            public double estimate_last24h { get; set; }
            public double actual_last24h { get; set; }
            public double mbtc_mh_factor { get; set; }
            public double k_effort { get; set; } = 1.0;
            public double k_average_effort { get; set; } = 1.0;
            public double k_ttf { get; set; } = 1.0;
            public double k_owed { get; set; } = 1.0;
            public double pool_ttf { get; set; }
            public double real_ttf { get; set; }
            public double timesincelast_shared { get; set; }
            public double minpay { get; set; }
            public double minpay_sunday { get; set; }
            public double reward { get; set; }
            public double owed { get; set; }
            public double effort { get; set; }
            public double average_effort { get; set; } = 100;
            public bool apibug { get; set; }
            public bool CPU { get; set; }
            public bool GPU { get; set; }
            public bool FPGA { get; set; }
            public bool ASIC { get; set; }
            public bool tempTTF_Disabled { get; set; }
            public int TTFcount { get; set; } = 0;
            public bool coinTempDeleted = true;
            public bool tempBlock { get; set; }
            public int noautotrade { get; set; }
        }
        public static List<Coin> MinerStatCoinList = new();
        private static int httpsProxyCheck = 0;
        public static string CurrentProxyIP;
        public static int CurrentProxyHTTPSPort = 13150;
        public static int CurrentProxySocks5SPort = 13155;
        public static async Task<string> GetPoolApiAsync(string url, int timeout = 5, bool log = true)
        {
            string responseFromServer = "";
            if (ConfigManager.GeneralConfig.EnableProxy)
            {
                foreach (var proxy in ProxyCheck.HttpsProxyList)
                {
                    if (proxy.Valid)
                    {
                        try
                        {
                            //try direct
                            responseFromServer = await GetPoolApiDataAsync(url, proxy, false, log);
                            if (!string.IsNullOrEmpty(responseFromServer))
                            {
                                if (log)
                                {
                                    Helpers.ConsolePrint("GetPoolApiData", "Received bytes: " +
                                    responseFromServer.Length.ToString() + " directly from " + url);
                                }
                                break;
                            }
                            else
                            {
                                if (log)
                                {
                                    Helpers.ConsolePrintError("GetPoolApiAsync", "Direct connection failure to " + url);
                                }
                            }
                            //proxy
                            responseFromServer = await GetPoolApiDataAsync(url, proxy, true, log);
                            if (!string.IsNullOrEmpty(responseFromServer))
                            {
                                if (log)
                                {
                                    Helpers.ConsolePrint("GetPoolApiData", "Received bytes: " +
                                    responseFromServer.Length.ToString() + " from " + url + " " +
                                    proxy.Ip + ":" + proxy.HTTPSPort);
                                }
                                break;
                            }
                            else
                            {
                                if (log)
                                {
                                    Helpers.ConsolePrintError("GetPoolApiAsync", "Connect fail via proxy: " +
                                    proxy.Ip + ":" + proxy.HTTPSPort.ToString());
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Helpers.ConsolePrintError("GetPoolApiAsync", ex.ToString());
                        }
                    }
                }

                if (string.IsNullOrEmpty(responseFromServer))
                {
                    //Helpers.ConsolePrintError("GetPoolApiAsync", "All proxy unavailable");
                    new Task(() => ProxyCheck.GetHttpsProxy()).Start();
                }
            } else
            {
                try
                {
                    responseFromServer = await GetPoolApiDataAsync(url, null, false, log);
                    if (!string.IsNullOrEmpty(responseFromServer))
                    {
                        if (log)
                        {
                            Helpers.ConsolePrint("GetPoolApiData", "Received bytes: " +
                        responseFromServer.Length.ToString() + " from " + url + " ");
                        }
                    }
                    else
                    {
                        if (log)
                        {
                            Helpers.ConsolePrintError("GetPoolApiData", "Error getting data from " + url);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Helpers.ConsolePrintError("GetPoolApiAsync", ex.ToString());
                }
            }
            return responseFromServer;
        }

        public static async Task<string> GetPoolApiDataAsync(string url, ProxyChecker.Proxy proxy, 
            bool viaProxy, bool log = true)
        {
            var uri = new Uri(url);
            string host = new Uri(url).Host;
            var responseFromServer = "";
            Random r = new Random();
            var id = "[" + r.Next(100, 999).ToString() + "] ";
            var watch = Stopwatch.StartNew();
            try
            {
                //ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, ssl) => true;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var httpClient = new HttpClient();
                if (viaProxy)
                {
                    if (log)
                    {
                        Helpers.ConsolePrint("GetPoolApiData", id + "Try connect to " + url + " via proxy " +
                        proxy.Ip + ":" + proxy.HTTPSPort.ToString());
                    }
                    var _proxy = new WebProxy
                    {
                        Address = new Uri("http://" + proxy.Ip + ":" + proxy.HTTPSPort.ToString())

                    };
                    //proxy.Credentials = new NetworkCredential(); //Used to set Proxy logins.

                    var proxyClientHandler = new HttpClientHandler
                    {
                        Proxy = _proxy
                    };
                    httpClient = new HttpClient(proxyClientHandler);
                } else
                {
                    if (log)
                    {
                        Helpers.ConsolePrint("GetPoolApiData", id + "Try connect to " + url);
                    }
                }
                using (httpClient)
                {
                    bool success = false;
                    new Thread(() =>
                    {
                        for (int i = 0; i < 15 * 10; i++)
                        {
                            if (Form_Main.ProgramClosing) return;
                            Thread.Sleep(100);
                        }
                        if (httpClient is object && httpClient is not null && !success)
                        {
                            httpClient.Dispose();
                        }
                    }).Start();
                    
                    var response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        success = true;
                        var contents = await response.Content.ReadAsStringAsync();
                        if (contents.Length == 0 || (contents[0] != '{' && contents[0] != '['))
                        {
                            if (log)
                            {
                                Helpers.ConsolePrintError("GetPoolApiDataAsync", id + "Error! Not JSON from " + url +
                                "\r\n" + responseFromServer);
                            }
                            responseFromServer = "";
                        }
                        else
                        {
                            responseFromServer = contents;
                            if (viaProxy)
                            {
                                CurrentProxyIP = proxy.Ip;
                                CurrentProxyHTTPSPort = proxy.HTTPSPort;
                                CurrentProxySocks5SPort = proxy.Socks5Port;
                            }
                        }
                    }
                    else
                    {
                        Helpers.ConsolePrintError("GetPoolApiDataAsync", id + response.ReasonPhrase + ", " +
                            response.StatusCode.ToString() + ", " +
                            response.RequestMessage.ToString() + ", " +
                            response.Headers.ToString() + ", ");
                    }
                }
                if (httpClient is object && httpClient != null)
                {
                    httpClient.Dispose();
                }
            }
            catch (Exception ex)
            {
                var t = (int)watch.ElapsedMilliseconds;
                watch.Stop();
                if (viaProxy)
                {
                    Helpers.ConsolePrintError("GetPoolApiDataAsync", id + "Connection error in " + t.ToString() + " ms " + ex.Message + " " +
                    url + " " + proxy.Ip + ":" + proxy.HTTPSPort.ToString());
                } else
                {
                    Helpers.ConsolePrintError("GetPoolApiDataAsync", id + "Connection error in " + t.ToString() + 
                        " ms " + ex.Message + " " + url);
                }
                //Form_Main.ZergPoolAPIError = ex.Message;
                //Form_Main.ZergPoolAPIError = "Connection error";
                return "";
            }
            Form_Main.ZergPoolAPIError = null;
            return responseFromServer;
        }
        public class AlgoCoin
        {
            public string algo;
            public string coin;
        }

        private static bool _FirstRunGetCoinsAsync;
        private static int delayGetBlock = 0;
        public static async Task<List<Coin>> GetCoinsAsync(string link)
        {
            Helpers.ConsolePrint("Stats", "Trying " + link);
            double correction = 1.0;
            double adaptivecorrection = 1.0;
            List<Coin> coinlist = new List<Coin>();
            List<AlgoCoin> noautotradeCoin = new();
            List<AlgoCoin> zeroHashrateCoin = new();
            List<AlgoCoin> owedCoin = new();
            delayGetBlock++;
            if (delayGetBlock > 15)
            {
                delayGetBlock = 0;
            }
            try
            {
                try
                {
                    if (File.Exists("configs\\CoinList.json"))
                    {
                        var coinsjson = File.ReadAllText("configs\\CoinList.json");
                        coinlist = JsonConvert.DeserializeObject<List<Coin>>(coinsjson);
                    }
                }
                catch (Exception ex)
                {
                    Helpers.ConsolePrintError("Stats", ex.ToString());
                }

                string ResponseFromAPI = await GetPoolApiAsync(link, 7);
                if (ResponseFromAPI != null)
                {
                    var APIdata = JObject.Parse(ResponseFromAPI);
                    foreach (var coinAPI in APIdata)
                    {
                        var coin = coinAPI.Value;
                        string name = coin.Value<string>("name");
                        string algo = coin.Value<string>("algo").Replace("-", "_");
                        string symbol = coin.Value<string>("symbol");
                        var port = coin.Value<int>("port");
                        var tls_port = coin.Value<int>("tls_port");
                        var _estimate = coin.Value<double>("estimate");
                        var _estimate_current = coin.Value<string>("estimate_current");
                        double.TryParse(_estimate_current, out double estimate_current);
                        var _estimate_last24h = coin.Value<string>("estimate_last24h");
                        double.TryParse(_estimate_last24h, out double estimate_last24h);
                        var _actual_last24h = coin.Value<string>("actual_last24h");
                        double.TryParse(_actual_last24h, out double actual_last24h);
                        var mbtc_mh_factor = coin.Value<double>("mbtc_mh_factor");
                        var algotype = coin.Value<int>("algotype");
                        var hashrate = coin.Value<double>("hashrate");
                        var pool_ttf = coin.Value<double>("pool_ttf");
                        var real_ttf = coin.Value<double>("real_ttf");
                        var timesincelast_shared = coin.Value<double>("timesincelast_shared");
                        var owed = coin.Value<double>("owed");
                        var reward = coin.Value<double>("reward");
                        var effort = coin.Value<double>("effort");
                        var minpay = coin.Value<double>("minpay");
                        var minpay_sunday = coin.Value<double>("minpay_sunday");
                        var noautotrade = coin.Value<int>("noautotrade");

                        Coin _coin = new();
                        _coin.coinname = name;
                        _coin.algo = algo;
                        _coin.symbol = symbol;
                        _coin.pool_ttf = pool_ttf;
                        _coin.real_ttf = real_ttf;
                        _coin.timesincelast_shared = timesincelast_shared;
                        _coin.owed = owed;
                        _coin.reward = reward;
                        _coin.effort = effort;
                        _coin.minpay = minpay;
                        _coin.minpay_sunday = minpay_sunday;
                        _coin.port = port;
                        _coin.tls_port = tls_port;
                        _coin.hashrate = hashrate;
                        _coin.adaptive_factor = 1.0;
                        _coin.coinTempDeleted = false;
                        if (!_coin.tempBlock)
                        {
                            _coin.estimate = (estimate_current / mbtc_mh_factor * 1000) * correction;//mBTC/GH
                            _coin.estimate_current = (estimate_current / mbtc_mh_factor * 1000000) * correction;//mBTC/GH
                            _coin.estimate_last24h = (estimate_last24h / mbtc_mh_factor * 1000000) * correction;
                            _coin.actual_last24h = (actual_last24h / mbtc_mh_factor / 1000 * 1000000) *
                                 correction;//zergpool api bug
                        }
                        _coin.mbtc_mh_factor = mbtc_mh_factor;//multiplier,
                                                              //value 1 represents Mh,
                                                              //1000 represents GH,
                                                              //1000000 represents TH,
                                                              //0.001 represents KH
                                                              //miningAlgorithms.algotype = algotype;//integer value of a 4-bit value
                                                              //representing platforms supported.
                                                              //Bit 3 = CPU, bit 2 = GPU,
                                                              //bit 1 = ASIC, bit 0 = FPGA

                        BitArray b = new BitArray(new int[] { algotype });
                        _coin.CPU = b[3];
                        _coin.GPU = b[2];
                        _coin.ASIC = b[1];
                        _coin.FPGA = b[0];
                        _coin.noautotrade = noautotrade;

                        if (!_coin.CPU && !_coin.GPU) continue;

                        //get from CoinList.json
                        var _c = coinlist.Find(a => (a.symbol.ToLower() == _coin.symbol.ToLower()) &&
                                (a.algo.ToLower() == _coin.algo.ToLower()));
                        if (_c is object && _c != null)
                        {
                            _coin.adaptive_factor = _c.adaptive_factor;
                            _coin.average_effort = _c.average_effort;
                            //_coin.TTFcount = _c.TTFcount;
                        }

                        if ((real_ttf > ConfigManager.GeneralConfig.maxTTF) && (_coin.CPU || _coin.GPU) &&
                            !_coin.tempTTF_Disabled)
                        {
                            Helpers.ConsolePrint("Stats", _coin.algo + " (" + _coin.symbol + ") TTF " +
                                    GetTime((int)real_ttf) + ". Disabled");
                            _coin.tempTTF_Disabled = true;
                            _coin.adaptive_profit = 0;
                            _coin.estimate = 0;
                            _coin.estimate_current = 0;
                            _coin.estimate_last24h = 0;
                            _coin.actual_last24h = 0;
                            /*
                            _coin.TTFcount++;
                            if (_coin.TTFcount > 5 || !_FirstRunGetCoinsAsync)//10 min
                            {
                                Helpers.ConsolePrint("Stats", _coin.algo + " (" + _coin.symbol + ") TTF " +
                                    GetTime((int)real_ttf) + ". Disabled");
                                _coin.tempTTF_Disabled = true;
                                _coin.adaptive_profit = 0;
                                _coin.estimate = 0;
                                _coin.estimate_current = 0;
                                _coin.estimate_last24h = 0;
                                _coin.actual_last24h = 0;
                            }
                            */
                        }
                        else
                        {
                            _coin.tempTTF_Disabled = false;
                            _coin.TTFcount = 0;
                        }

                        if ((hashrate == 0) && (_coin.CPU || _coin.GPU))
                        {
                            //Helpers.ConsolePrint("Stats", _coin.algo + " (" + _coin.symbol + ") zero hashrate. Disabled");
                            AlgoCoin zhc = new();
                            zhc.algo = _coin.algo;
                            zhc.coin = _coin.symbol;
                            zeroHashrateCoin.Add(zhc);
                            _coin.tempTTF_Disabled = true;
                            _coin.tempBlock = true;
                            _coin.estimate = 0;
                            _coin.estimate_current = 0;
                            _coin.estimate_last24h = 0;
                            _coin.actual_last24h = 0;
                        }

                        if (noautotrade == 1 && (_coin.CPU || _coin.GPU))
                        {
                            AlgoCoin nac = new();
                            nac.algo = _coin.algo;
                            nac.coin = _coin.symbol;
                            noautotradeCoin.Add(nac);
                            _coin.estimate = 0;
                            _coin.estimate_current = 0;
                            _coin.estimate_last24h = 0;
                            _coin.actual_last24h = 0;
                        }


                        var unstableAlgosList = AlgorithmSwitchingManager.unstableAlgosList.Select(s => s.ToString().ToLower()).ToList();
                        if (unstableAlgosList.Contains(algo.ToLower()))
                        {
                            _coin.estimate = _coin.estimate * 0.3;
                            _coin.estimate_current = _coin.estimate_current * 0.3;
                            _coin.estimate_last24h = _coin.estimate_last24h * 0.3;
                            _coin.actual_last24h = _coin.actual_last24h * 0.3;
                        }

                        //***********
                        bool cpu = false;
                        bool gpu = false;
                        if (delayGetBlock == 2 && ConfigManager.GeneralConfig.AdaptiveAlgo)
                        {
                            foreach (var cd in ComputeDeviceManager.Available.Devices)
                            {
                                if (cd.DeviceType == DeviceType.CPU && cd.Enabled)
                                {
                                    cpu = true;
                                }
                                if ((cd.DeviceType == DeviceType.AMD ||
                                    cd.DeviceType == DeviceType.NVIDIA ||
                                    cd.DeviceType == DeviceType.INTEL) &&
                                    cd.Enabled)
                                {
                                    gpu = true;
                                }
                            }

                            int count = 0;
                            double summ_effort = 0d;
                            int average_effort = 100;

                            if (_coin.hashrate > 0 && !_coin.tempTTF_Disabled && _coin.noautotrade == 0 &&
                                (_coin.CPU == cpu || _coin.GPU == gpu))
                            {
                                ResponseFromAPI = await GetPoolApiAsync(Links.Blocks + _coin.symbol, 1, false);
                                if (!string.IsNullOrEmpty(ResponseFromAPI))
                                {
                                    int blocks_count = 0;
                                    try
                                    {
                                        dynamic data = JsonConvert.DeserializeObject(ResponseFromAPI);
                                        foreach (var coindata in data)
                                        {
                                            blocks_count++;
                                            string coin_algo = coindata.algo;
                                            string coin_symbol = coindata.symbol;
                                            string coin_type = coindata.type;
                                            if (coindata.effort == null) continue;
                                            if (coindata.time == null) continue;

                                            string _coin_effort = (string)coindata.effort;
                                            if (_coin_effort == null) continue;
                                            int coin_effort = 0;
                                            int.TryParse(_coin_effort, out coin_effort);

                                            int block_time = (int)coindata.time;
                                            string category = coindata.category;//"category": "orphan",
                                            if (coin_effort <= 0) continue;

                                            if (blocks_count == 1 && category.Equals("orphan"))//latest block
                                            {
                                                Helpers.ConsolePrint("Stats", "Latest block is orphan. " + coin_algo +
                                                "(" + coin_symbol + "). Temporary block");
                                                _coin.estimate = _coin.estimate * 0.2;
                                                _coin.estimate_current = _coin.estimate_current * 0.2;
                                                _coin.estimate_last24h = _coin.estimate_last24h * 0.2;
                                                _coin.actual_last24h = _coin.actual_last24h * 0.2;
                                                _coin.tempBlock = true;
                                                break;
                                            }

                                            if (coin_type.Equals("shared"))
                                            {
                                                summ_effort = summ_effort + coin_effort;
                                                count++;
                                                var utc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                                if (count >= 50 || utc - block_time > 1209600) break;//14 days
                                            }
                                        }

                                        if (summ_effort < 2) summ_effort = 2;
                                        if (count == 0) count = 1;
                                        average_effort = (int)(summ_effort / count);
                                        //if (average_effort < 50) average_effort = 1000;
                                        //if (average_effort < 80) average_effort = 85;

                                        _coin.average_effort = average_effort;

                                        Helpers.ConsolePrint("GetCoins", _coin.algo + " (" +
                                            _coin.symbol + ") average effort: " +
                                            _coin.average_effort.ToString() + "%");

                                    }
                                    catch (Exception ex)
                                    {
                                        Helpers.ConsolePrintError("GetCoins", ex.ToString());
                                    }
                                }
                                Thread.Sleep(500);

                            }
                        }
                        
                        if (_coin.actual_last24h != 0 && _coin.estimate_current > _coin.actual_last24h * 100)
                        {
                            Helpers.ConsolePrint("Stats", _coin.symbol + " API bug");
                            _coin.estimate = _coin.actual_last24h;
                            _coin.estimate_current = _coin.actual_last24h;
                            _coin.estimate_last24h = _coin.actual_last24h;
                        }

                        //test
                        /*
                        if (_coin.symbol.ToLower().Equals("btg"))
                        {
                            _coin.estimate_current = 9999999;
                            _coin.tempTTF_Disabled = false;
                        }
                        */
                        if (!Stats.coinsBlocked.ContainsKey(_coin.symbol))
                        {
                            _coin.tempBlock = false;
                        }

                        foreach (var c in Stats.coinsBlocked)
                        {
                            if (c.Key.Equals(_coin.symbol) && c.Value.checkTime >= 15)//15 min checking
                            {
                                Helpers.ConsolePrint("Stats", "Actual hashrate is missing from zergpool for 15 minutes for " + _coin.algo +
                                    "(" + c.Key + "). Temporary block");
                                _coin.estimate = _coin.estimate * 0.2;
                                _coin.estimate_current = _coin.estimate_current * 0.2;
                                _coin.estimate_last24h = _coin.estimate_last24h * 0.2;
                                _coin.actual_last24h = _coin.actual_last24h * 0.2;
                                _coin.tempBlock = true;

                                foreach (var miningDevice in MiningSession._miningDevices)
                                {
                                    if (miningDevice.DeviceCurrentMiningCoin.Equals(c.Key) &&
                                                   !_coin.tempTTF_Disabled)//блокируем
                                    {
                                        miningDevice.needSwitch = true;
                                    }
                                }
                            }
                        }

                        //adaptive section
                        double k_owed = 1;
                        var _owed = Math.Abs(_coin.owed / _coin.reward);
                        if (_owed > 2 && (_coin.CPU || _coin.GPU) && !_coin.tempTTF_Disabled)
                        {
                            AlgoCoin oc = new();
                            oc.algo = _coin.algo;
                            oc.coin = _coin.symbol;
                            owedCoin.Add(oc);
                            k_owed = (1 + _owed / 100);
                        }

                        var k_ttf = 1 + _coin.real_ttf / 1000000;
                        if (k_ttf < 1) k_ttf = 1;//если в api вдруг отрицательное значение

                        double k_effort = 1d;
                        //var k_effort = 1 + ((_coin.effort + 800) / 7) / 1000;
                        if (_coin.effort > 120)
                        {
                            k_effort = 1 + (_coin.effort - 120) / 100;
                        }
                        if (_coin.effort < 100) k_effort = 1;

                        var k_average_effort = 1 + (_coin.average_effort - 100) / 200;
                        if (_coin.average_effort < 50) k_average_effort = 10.0;

                        if (_coin.real_ttf < 60 * 15)//15 min
                        {
                            k_effort = 1;
                        }

                        //k_average_effort = 1; //отключим
                        //k_effort = 1;
                        k_ttf = 1;
                        
                        
                        _coin.k_average_effort = k_average_effort;
                        _coin.k_effort = k_effort;
                        _coin.k_owed = k_owed;
                        _coin.k_ttf = k_ttf;

                        if (_coin.actual_last24h != 0)
                        {
                            _coin.adaptive_profit = ((_coin.estimate_current + _coin.actual_last24h) / 2) * _coin.adaptive_factor / k_effort / k_average_effort / k_ttf / k_owed;
                        }
                        else
                        {
                            _coin.adaptive_profit = _coin.estimate_current * _coin.adaptive_factor / k_effort / k_average_effort / k_ttf / k_owed;
                        }
                        /*
                        Helpers.ConsolePrint("*****", _coin.algo + " (" +
                            _coin.symbol + ")" +
                            " real_ttf: " + _coin.real_ttf.ToString() +
                            " k_ttf: " + k_ttf.ToString() +
                            " effort: " + _coin.effort.ToString() +
                            " k_effort: " + k_effort.ToString() +
                            " average_effort: " + _coin.average_effort.ToString() +
                            " k_average_effort: " + k_average_effort.ToString() +
                            " adaptive_profit: " + _coin.adaptive_profit.ToString());
                        */
                        //not adaptive section
                        double _profits = 0d;
                        int profitsCount = 0;
                        if (checkAPIbug(_coin))
                        {
                            _coin.profit = getMin(_coin.estimate_current, _coin.estimate_last24h, _coin.actual_last24h);
                        }
                        else
                        {
                            if (ConfigManager.GeneralConfig.CurrentEstimate)
                            {
                                _profits = _profits + _coin.estimate_current;
                                profitsCount++;
                            }
                            if (ConfigManager.GeneralConfig._24hEstimate)
                            {
                                _profits = _profits + _coin.estimate_last24h;
                                profitsCount++;
                            }
                            if (ConfigManager.GeneralConfig._24hActual)
                            {
                                _profits = _profits + _coin.actual_last24h;
                                profitsCount++;
                            }
                            if (profitsCount != 0)
                            {
                                _profits = _profits / profitsCount;
                            }
                            _coin.profit = _profits;
                        }

                        if (!_coin.coinTempDeleted)
                        {
                            if (CoinList.Exists(a => a.symbol.ToLower() == _coin.symbol.ToLower()) &&
                                CoinList.Exists(a => a.algo.ToLower() == _coin.algo.ToLower()))
                            {
                                _c = CoinList.Find(a => (a.symbol.ToLower() == _coin.symbol.ToLower()) &&
                                (a.algo.ToLower() == _coin.algo.ToLower()));
                                if (_c is object && _c != null)
                                {
                                    CoinList.RemoveAll(a => (a.symbol.ToLower() == _coin.symbol.ToLower()) &&
                                    (a.algo.ToLower() == _coin.algo.ToLower()));
                                    CoinList.Add(_coin);
                                }
                            }
                            else //coin not exist in CoinList.json
                            {
                                Helpers.ConsolePrint("Stats", _coin.algo + " (" + _coin.symbol + ") added");
                                CoinList.Add(_coin);
                            }
                        }
                        else
                        {
                            Helpers.ConsolePrint("Stats", _coin.algo + " (" + _coin.symbol + ") deleted");
                        }
                    }
                    //7771
                    if (APIdata.Count > 10)
                    {
                        foreach (var c in Enumerable.Reverse(CoinList).ToList())
                        {
                            bool founded = false;
                            foreach (var item in APIdata)
                            {
                                var coin = item.Value;
                                string symbol = coin.Value<string>("symbol");
                                if (c.symbol.Equals(symbol))
                                {
                                    founded = true;
                                    break;
                                }
                            }
                            if (!founded)
                            {
                                CoinList.Remove(c);
                                Helpers.ConsolePrint("Stats", "Missing coin " + c.symbol + "(" + c.algo + "). Delete");
                            }
                        }
                    }
                }

                noautotradeCoin.Sort((x, y) => x.algo.CompareTo(y.algo));
                
                string _algo = "";
                string coins = "";
                List<string> _noautotradeCoinList = new();
                foreach (var c in noautotradeCoin)
                {
                    if (_algo.IsNullOrEmpty())
                    {
                        _algo = c.algo;
                    }
                    if (c.algo.Equals(_algo))
                    {
                        coins = coins + c.coin + ", ";
                    } else
                    {
                        _noautotradeCoinList.Add(_algo + "(" + coins.Substring(0, coins.Length - 2) + ")");
                        //Helpers.ConsolePrint("Stats", _algo + " (" + coins.Substring(0, coins.Length - 2) + ") no autotrade. Disabled");
                        coins = "";
                        _algo = c.algo;
                        coins = coins + c.coin + ", ";
                    }
                }
                if (_noautotradeCoinList.Count > 0)
                {
                    Helpers.ConsolePrint("Stats", "No autotrade coins: " + string.Join(", ", _noautotradeCoinList));
                }

                zeroHashrateCoin.Sort((x, y) => x.algo.CompareTo(y.algo));
                _algo = "";
                coins = "";
                List<string> _zeroHashrateCoinList = new();
                foreach (var c in zeroHashrateCoin)
                {
                    if (_algo.IsNullOrEmpty())
                    {
                        _algo = c.algo;
                    }
                    if (c.algo.Equals(_algo))
                    {
                        coins = coins + c.coin + ", ";
                    }
                    else
                    {
                        _zeroHashrateCoinList.Add(_algo + " (" + coins.Substring(0, coins.Length - 2) + ")");
                        //Helpers.ConsolePrint("Stats", _algo + " (" + coins.Substring(0, coins.Length - 2) + ") zero hashrate. Disabled");
                        coins = "";
                        _algo = c.algo;
                        coins = coins + c.coin + ", ";
                    }
                }
                if (_zeroHashrateCoinList.Count > 0)
                {
                    Helpers.ConsolePrint("Stats", "Zero hashrate coins: " + string.Join(", ", _zeroHashrateCoinList));
                }

                owedCoin.Sort((x, y) => x.algo.CompareTo(y.algo));
                _algo = "";
                coins = "";
                List<string> _owedCoinList = new();
                foreach (var c in owedCoin)
                {
                    if (_algo.IsNullOrEmpty())
                    {
                        _algo = c.algo;
                    }
                    if (c.algo.Equals(_algo))
                    {
                        coins = coins + c.coin + ", ";
                    }
                    else
                    {
                        _owedCoinList.Add(_algo + " (" + coins.Substring(0, coins.Length - 2) + ")");
                        coins = "";
                        _algo = c.algo;
                        coins = coins + c.coin + ", ";
                    }
                }
                if (_owedCoinList.Count > 0)
                {
                    Helpers.ConsolePrint("Stats", "Owed coins: " + string.Join(", ", _owedCoinList));
                }

                lock (fileLock)
                {
                    CoinList.Sort((x, y) => x.algo.CompareTo(y.algo));
                    var json = JsonConvert.SerializeObject(CoinList, Formatting.Indented);
                    if (json.Length > 5)
                    {
                        Helpers.WriteAllTextWithBackup("configs\\CoinList.json", json);
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrintError("GetCoins", ex.ToString());
                /*
                try
                {
                    if (File.Exists("configs\\CoinList.json"))
                    {
                        var coinsjson = File.ReadAllText("configs\\CoinList.json");
                        if (!coinsjson.StartsWith("["))
                        {
                            File.Delete("configs\\CoinList.json");
                            Helpers.ConsolePrintError("Stats", "Coins database corrupted. Restart program");
                            Form_Main.MakeRestart(1);
                        }
                        else
                        {
                            CoinList = JsonConvert.DeserializeObject<List<Coin>>(coinsjson);
                        }
                    }
                }
                catch (Exception ex1)
                {
                    Helpers.ConsolePrintError("Stats", ex1.ToString());
                }
                */
                var json = File.ReadAllText("configs\\CoinList.json");
                CoinList = (List<Coin>)JsonConvert.DeserializeObject(json);
                _FirstRunGetCoinsAsync = true;
            }
            _FirstRunGetCoinsAsync = true;
            return CoinList;
        }

        public static List<string> zergpoolAlgos = new();
        public static List<Coin> GetMostProfitCoins(List<Coin> _coinlist)
        {
            List<Coin> _CoinList = new();
            List<string> progAlgosList = new();
            List<string> poolAlgosList = new();

            try
            {
                foreach (AlgorithmType alg in Enum.GetValues(typeof(AlgorithmType)))
                {
                    string _alg = alg.ToString().ToLower();
                    //_alg = _alg.Replace("xelisv2_pepew", "xelisv2-pepew");
                    //_alg = _alg.Replace("neoscrypt_xaya", "neoscrypt-xaya");
                    Coin mostProfitCoin = new();
                    if ((int)alg >= 1000 && !alg.ToString().Contains("unused"))
                    {
                        progAlgosList.Add(_alg);
                        foreach (var coin in _coinlist)
                        {
                            coin.coinTempDeleted = false;
                            if (coin.CPU || coin.GPU)
                            {
                                poolAlgosList.Add(coin.algo.ToLower());
                            }
                            if (!zergpoolAlgos.Contains(coin.algo.ToLower()))
                            {
                                zergpoolAlgos.Add(coin.algo.ToLower());
                            }
                            
                            if (coin.algo.ToLower().Equals(_alg))
                            {
                                //foreach (var hashratencoin in ZPoolcoins)
                                {
                                    //Helpers.ConsolePrint(coin.symbol, minerStatcoin.symbol);
                                    /*
                                    if (coin.symbol.Equals(hashratencoin.symbol) && coin.algo.Equals(hashratencoin.algo))
                                    {
                                        Helpers.ConsolePrint(coin.algo.ToLower(), coin.symbol + ": " + 
                                            coin.estimate_current.ToString() + " " +
                                            hashratencoin.symbol + ": " + 
                                            (hashratencoin.estimate_current * coin.mbtc_mh_factor).ToString());
                                    }
                                    */
                                    /*
                                    //по хешрейту
                                    if (coin.hashrate >= defcoin.hashrate && !coin.tempTTF_Disabled &&
                                        !coin.tempDeleted && coin.noautotrade == 0)
                                    {
                                        defcoin = coin;
                                        defcoin.algo = _alg;
                                    }
                                    */

                                    //по монете
                                    //if (coin.hashrate > 0 && coin.estimate_current >= defcoin.estimate_current &&
                                    if (ConfigManager.GeneralConfig.AdaptiveAlgo)
                                    {
                                        if (coin.adaptive_profit >= mostProfitCoin.adaptive_profit)
                                        {
                                            mostProfitCoin = coin;
                                        }
                                    }
                                    else
                                    {
                                        if (coin.profit >= mostProfitCoin.profit)
                                        {
                                            mostProfitCoin = coin;
                                        }
                                    }
                                }
                            }
                        }
                        if (ConfigManager.GeneralConfig.AdaptiveAlgo)
                        {
                            if (!string.IsNullOrEmpty(mostProfitCoin.symbol) && mostProfitCoin.adaptive_profit > 0)
                            {
                                _CoinList.Add(mostProfitCoin);
                            }
                        } else
                        {
                            if (!string.IsNullOrEmpty(mostProfitCoin.symbol) && mostProfitCoin.profit > 0)
                            {
                                _CoinList.Add(mostProfitCoin);
                            }
                        }
                    }
                }

                List<string> _deletedAlgosList = new();
                foreach (var a in progAlgosList)
                {
                    string _alg = a.ToLower();
                    _alg = _alg.Replace("-", "_");
                    /*
                    if (!poolAlgosList.Contains(_alg))
                    {
                        _deletedAlgosList.Add(_alg);
                        //Helpers.ConsolePrint("Stats", "Deleted? - " + _alg);
                        var itemToDelete = _CoinList.Find(r => r.algo.ToLower() == _alg);
                        if (itemToDelete != null)
                        {
                            itemToDelete.coinTempDeleted = true;
                        }
                    } else
                    {
                        var itemToDelete = _CoinList.Find(r => r.algo.ToLower() == _alg);
                        if (itemToDelete != null) itemToDelete.coinTempDeleted = false;
                    }
                    */
                }

                if (_deletedAlgosList.Count > 0)
                {
                    Helpers.ConsolePrint("Stats", "Missing algos: " + string.Join(", ", _deletedAlgosList));
                }

            } catch (Exception ex)
            {
                Helpers.ConsolePrintError("GetMostProfitCoins", ex.ToString());
            }
            return _CoinList;
        }

        public static string GetTime(int seconds)
        {
            TimeSpan ts = new TimeSpan(0, 0, seconds);
            if (ts.Days > 0)
            {
                return ts.Days + " Day(s)";
            }
            else if (ts.Hours > 0)
            {
                return ts.Hours + " Hour(s)";
            }
            else if (ts.Minutes > 0)
            {
                return ts.Minutes + " Minute(s)";
            }
            else
            {
                return ts.Seconds + " Second(s)";
            }
            return "?";
        }

        public static void ClearBalance()
        {
            Balance = 0;
        }

        public static void GetWalletBalance(object sender, EventArgs e)
        {
            GetWalletBalanceAsync(sender, e);
        }

        public static async Task GetWalletBalanceAsync(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ConfigManager.GeneralConfig.Wallet))
            {
                Helpers.ConsolePrint("Stats", "Error getting wallet balance. Wallet not entered");
                return;
            }
            try
            {
                string ResponseFromAPI = await GetPoolApiAsync(Links.WalletBalance + ConfigManager.GeneralConfig.Wallet);
                if (!string.IsNullOrEmpty(ResponseFromAPI))
                {
                    dynamic data = JsonConvert.DeserializeObject(ResponseFromAPI);
                    double unpaid = data.@unpaid;
                    double balance = data.balance;
                    Balance = balance;
                } else
                {
                    //Balance = 0;
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrintError("GetWalletBalance", ex.ToString());
            }
            Helpers.ConsolePrint("Stats", "Wallet balance: " + Balance.ToString() + " " +
                ConfigManager.GeneralConfig.PayoutCurrency + 
                " (" + (Balance * ExchangeRateApi.GetPayoutCurrencyExchangeRate()).ToString("F2") + " " +
                ConfigManager.GeneralConfig.DisplayCurrency + ")");
            return;
        }
        private static readonly object fileLock = new object();
        public class AlgoProperty
        {
            public string algo;
            public string coin;
            public string hashrate;
            public double localhashrate;
            public double actualhashrate;
            public double localProfit;
            public double actualProfit;
            public double adaptive_factor;
            public double adaptive_profit;
            public int ticks;
            public List<double> factorsList = new();
        }

        public static ConcurrentDictionary<string, AlgoProperty> algosProperty = new();
        public static List<CoinProperty> coinsMining = new();
        public class CoinProperty
        {
            public string algorithm;
            public string symbol;
            public string hashrate_shared;
        }
        public static async Task GetWalletBalanceExAsync(object sender, EventArgs e)
        {
            Form_Main.adaptiveRunning = false;
            if (string.IsNullOrEmpty(ConfigManager.GeneralConfig.Wallet))
            {
                Helpers.ConsolePrintError("Stats", "Error getting wallet balance. Wallet not entered");
                return;
            }
            try
            {
                string ResponseFromAPI = await GetPoolApiAsync(Links.WalletBalanceEx + ConfigManager.GeneralConfig.Wallet);
                if (!string.IsNullOrEmpty(ResponseFromAPI))
                {
                    coinsMining.Clear();
                    double overallBTC = 0;
                    dynamic data = JsonConvert.DeserializeObject(ResponseFromAPI);
                    double localProfit = 0d;
                    double chart_actualProfit = 0d;
                    double balance = data.balance;
                    Balance = balance;

                    Helpers.ConsolePrint("Stats", "Wallet balance: " + Balance.ToString() + " " +
                        ConfigManager.GeneralConfig.PayoutCurrency +
                        " (" + (Balance * ExchangeRateApi.GetPayoutCurrencyExchangeRate()).ToString("F2") + " " +
                        ConfigManager.GeneralConfig.DisplayCurrency + ")");

                    if (MiningSession._miningDevices is not object) return;
                    if (MiningSession._miningDevices.Count == 0) return;

                    List<string> currentminingAlgos = new();
                    List<string> currentminingAlgosZergPool = new();
                    foreach (var _coin in CoinList)
                    {
                        if (_coin.adaptive_factor != 0 && _coin.adaptive_factor < 0.95)
                        {
                            _coin.adaptive_factor = _coin.adaptive_factor + 0.00001;
                        }
                        if (_coin.adaptive_factor != 0 && _coin.adaptive_factor > 1.05)
                        {
                            _coin.adaptive_factor = _coin.adaptive_factor - 0.00001;
                        }
                        if (_coin.coinTempDeleted) continue;
                        if (_coin.tempTTF_Disabled) continue;
                        double actualHashrate = 0d;
                        foreach (var cur in data.SelectToken("miners"))
                        {
                            string _algo = cur.SelectToken("algo");
                            _algo = _algo.Replace("xelisv2-pepew", "xelisv2_pepew");
                            _algo = _algo.Replace("neoscrypt-xaya", "neoscrypt_xaya");
                            string _hashrate = cur.SelectToken("hashrate");
                            string _ID = cur.SelectToken("ID");
                            string _password = cur.SelectToken("password");
                            int _subscribe = cur.SelectToken("subscribe");
                            double _accepted = cur.SelectToken("accepted");
                            string coin = _password.Split(',')[2].Replace("mc=", "");

                            if (_coin.algo.ToLower().Equals(_algo.ToLower()) &&
                                _coin.symbol.ToLower().Equals(coin.ToLower()) &&
                            _ID.Equals(Miner.GetFullWorkerName()))
                            {
                                var _algoProperty = new AlgoProperty();
                                double localHashrate = 0d;
                                foreach (var miningDevice in MiningSession._miningDevices)
                                {
                                    if (_coin.algo.ToLower().Equals(((AlgorithmType)miningDevice.Device.AlgorithmID).ToString().ToLower()) &&
                                        _coin.symbol.ToLower().Equals(miningDevice.Device.Coin.ToLower()))
                                    {
                                        localHashrate = localHashrate + miningDevice.Device.MiningHashrate;
                                    }
                                    if (_coin.algo.ToLower().Equals(((AlgorithmType)miningDevice.Device.SecondAlgorithmID).ToString().ToLower()) &&
                                        _coin.symbol.ToLower().Equals(miningDevice.Device.Coin.ToLower()))
                                    {
                                        localHashrate = localHashrate + miningDevice.Device.MiningHashrateSecond;
                                    }
                                    if (!currentminingAlgos.Contains(((AlgorithmType)miningDevice.Device.AlgorithmID).ToString().ToLower()))
                                    {
                                        currentminingAlgos.Add(((AlgorithmType)miningDevice.Device.AlgorithmID).ToString().ToLower());
                                    }
                                    if (!currentminingAlgos.Contains(((AlgorithmType)miningDevice.Device.SecondAlgorithmID).ToString().ToLower()))
                                    {
                                        currentminingAlgos.Add(((AlgorithmType)miningDevice.Device.SecondAlgorithmID).ToString().ToLower());
                                    }
                                }

                                if (!currentminingAlgosZergPool.Contains(_algo.ToLower()))
                                {
                                    currentminingAlgosZergPool.Add(_algo.ToLower());
                                }

                                if (ConfigManager.GeneralConfig.AdaptiveAlgo)
                                {
                                    localProfit = (localHashrate * _coin.adaptive_profit / _coin.adaptive_factor);
                                } else
                                {
                                    localProfit = (localHashrate * _coin.profit);
                                }
                                actualHashrate = actualHashrate + _accepted * (1.00);

                                if (algosProperty.ContainsKey(_coin.algo.ToLower()))
                                {
                                    if (MiningSession._miningDevices.Exists(x => (((AlgorithmType)x.Device.AlgorithmID).ToString().ToLower() == _coin.algo.ToLower()) &&
                                    _coin.symbol.ToLower().Equals(x.Device.Coin.ToLower())))
                                    {
                                        _algoProperty.ticks = algosProperty.FirstOrDefault(x => x.Key.ToLower() == _coin.algo.ToLower()).Value.ticks;
                                    }
                                    if (MiningSession._miningDevices.Exists(x => (((AlgorithmType)x.Device.SecondAlgorithmID).ToString().ToLower() == _coin.algo.ToLower()) &&
                                    _coin.symbol.ToLower().Equals(x.Device.Coin.ToLower())))
                                    {
                                        _algoProperty.ticks = algosProperty.FirstOrDefault(x => x.Key.ToLower() == _coin.algo.ToLower()).Value.ticks;
                                    }
                                    _algoProperty.factorsList = algosProperty.FirstOrDefault(x => x.Key.ToLower() == _coin.algo.ToLower()).Value.factorsList;
                                }
                                _algoProperty.localhashrate = localHashrate;
                                _algoProperty.localProfit = localProfit;
                                _algoProperty.actualhashrate = actualHashrate;
                                _algoProperty.algo = _coin.algo.ToLower();
                                _algoProperty.coin = _coin.symbol.ToLower();
                                _algoProperty.hashrate = _hashrate;
                                _algoProperty.adaptive_factor = _coin.adaptive_factor;

                                if (currentminingAlgos.Contains(_coin.algo.ToLower()))
                                {
                                    algosProperty.AddOrUpdate(_coin.algo.ToLower(), _algoProperty, (k, v) => _algoProperty);
                                }
                            }
                        }
                    }

                    foreach (var cur in data.SelectToken("summary"))
                    {
                        string _symbol = cur.SelectToken("symbol");
                        string _hashrate_shared = cur.SelectToken("hashrate_shared");
                        if (!coinsMining.Exists(a => a.symbol == _symbol))
                        {
                            CoinProperty cp = new();
                            if (CoinList is object && CoinList != null &&
                            CoinList.Count > 0)
                            {
                                var cl = CoinList.FindAll(a => a.symbol == _symbol);
                                if (cl.Count == 0)
                                {
                                    //Helpers.ConsolePrint("*******", "no mining coin " + _symbol);
                                }
                                foreach (var c in cl)
                                {
                                    cp.algorithm = c.algo;
                                    cp.symbol = _symbol;
                                    cp.hashrate_shared = _hashrate_shared;

                                    try
                                    {
                                        var coinMining = algosProperty.FirstOrDefault(a => (a.Key.ToLower() == cp.algorithm.ToLower() &&
                                                                            a.Value.coin.ToLower() == _symbol.ToLower()));
                                        if (coinMining is object and not null && coinMining.Key != null)
                                        {
                                            if (coinMining.Key.ToLower() == cp.algorithm.ToLower() && coinMining.Value.coin.ToLower() == _symbol.ToLower())
                                            {
                                                if (!coinsMining.Contains(cp))
                                                {
                                                    //Helpers.ConsolePrint("******** adding " + cp.symbol, _symbol + "worker hashrate for " + cp.algorithm);
                                                    coinsMining.Add(cp);
                                                }
                                            }
                                            else
                                            {

                                            }
                                        }
                                    } catch (InvalidOperationException ex)
                                    {

                                    }
                                }
                            }
                        }
                    }

                    foreach (var __algoProperty in algosProperty)
                    {
                        double average = 1.0;
                        var _algoProperty = __algoProperty.Value;
                        var coin = CoinList.FirstOrDefault(x => (x.algo.ToLower() == __algoProperty.Key.ToLower() &&
                        x.symbol.ToLower() == __algoProperty.Value.coin.ToLower()));

                        //пусть на графике присутствуют предыдущие значения
                        if (ConfigManager.GeneralConfig.AdaptiveAlgo)
                        {
                            chart_actualProfit = chart_actualProfit + (_algoProperty.actualhashrate * coin.adaptive_profit / coin.adaptive_factor);
                        } else
                        {
                            chart_actualProfit = chart_actualProfit + (_algoProperty.actualhashrate * coin.profit);
                        }

                        //а для расчета только текущие
                        if (!currentminingAlgosZergPool.Contains(_algoProperty.algo.ToLower()))
                        {
                            //Helpers.ConsolePrint("Stats", "Delete current mining algo: " + _algoProperty.algo);
                            algosProperty.TryRemove(_algoProperty.algo, out var r);
                        }

                        if (_algoProperty.localhashrate > 0)
                        {
                            _algoProperty.ticks++;
                            if (ConfigManager.GeneralConfig.AdaptiveAlgo)
                            {
                                _algoProperty.actualProfit = (_algoProperty.actualhashrate * coin.adaptive_profit / coin.adaptive_factor);
                            } else
                            {
                                _algoProperty.actualProfit = (_algoProperty.actualhashrate * coin.profit);
                            }
                            /*
                            if (_algoProperty.ticks >= ConfigManager.GeneralConfig.ticksBeforeAdaptiveStart &&//15 
                                _algoProperty.ticks <= ConfigManager.GeneralConfig.ticksBeforeAdaptiveStart +
                                ConfigManager.GeneralConfig.ticksAdaptiveTuning &&
                                alg.adaptive_factor == 0)
                            {
                                Helpers.ConsolePrint("Stats", alg.name + " adaptive tuning in process");
                                Form_Main.adaptiveRunning = true;
                            }
                            */
                            //if (_algoProperty.factorsList.Count < 100 && _algoProperty.adaptive_factor != 0)
                            if (_algoProperty.factorsList.Count < 30)
                            {
                                for (int i = _algoProperty.factorsList.Count; i < 30; i++)
                                {
                                    if (_algoProperty.adaptive_factor != 0)
                                    {
                                        _algoProperty.factorsList.Add(_algoProperty.adaptive_factor);
                                    }
                                    else
                                    {
                                        _algoProperty.factorsList.Add(0.95);
                                    }
                                }
                            }

                            double factorNow = _algoProperty.actualProfit / _algoProperty.localProfit;

                            if (_algoProperty.ticks >= 15)
                            {
                                _algoProperty.factorsList.Add(factorNow);

                                if (ConfigManager.GeneralConfig.AdaptiveAlgo)//включен по умолчанию
                                {
                                    average = _algoProperty.factorsList.Sum() / _algoProperty.factorsList.Count;
                                    if (_algoProperty.ticks > 30)
                                    {
                                        //Form_Main.adaptiveRunning = false;
                                        if (!double.IsInfinity(average) && !double.IsNaN(average))
                                        {
                                            if (average < 0.2)
                                            {
                                                average = 0.2;
                                            }
                                            if (average > 1.1)
                                            {
                                                average = 1.1;
                                            }
                                            coin.adaptive_factor = average;
                                            CoinList.RemoveAll(a => (a.symbol.ToLower() == coin.symbol.ToLower()) &&
                                                                    (a.algo.ToLower() == coin.algo.ToLower()));
                                            CoinList.Add(coin);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            //algosProperty.TryRemove(_algoProperty.name, out var r);
                        }
                    }

                    overallBTC = chart_actualProfit;
                    Form_Main.TotalActualProfitability = overallBTC;
                    Form_Main.lastRigProfit.currentProfitAPI = overallBTC * Algorithm.Mult;

                    lock (fileLock)
                    {
                        CoinList.Sort((x, y) => x.algo.CompareTo(y.algo));
                        var json = JsonConvert.SerializeObject(CoinList, Formatting.Indented);
                        if (json.Length > 5)
                        {
                            Helpers.WriteAllTextWithBackup("configs\\CoinList.json", json);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrintError("GetWalletBalance", ex.ToString());
            }
            return;
        }

        public static bool emptypool = true;
        [HandleProcessCorruptedStateExceptions]
        public static async Task GetAlgosAsync()
        {
            setAlgos(await GetCoinsAsync(Links.Currencies));
        }

        public static async Task LoadCoinListAsync(bool onlyCached = false)
        {
            List<Coin> coinlist = new List<Coin>();
            try
            {
                if (System.IO.File.Exists("configs\\CoinList.json"))
                {
                    var jsonData = File.ReadAllText("configs\\CoinList.json");
                    if (string.IsNullOrEmpty(jsonData.Trim('\0')))
                    {
                        File.Delete("configs\\CoinList.json");
                    }
                    else
                    {
                        Helpers.ConsolePrint("LoadAlgoritmsList", "Loadind previous CoinList");
                        coinlist = JsonConvert.DeserializeObject<List<Coin>>(jsonData);
                        CoinList = coinlist;
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrintError("LoadAlgoritmsList", ex.ToString());
            }
            setAlgos(coinlist);
            if (onlyCached) return;
            //setAlgos(await GetZergPoolAlgosListExAsync());
        }
        private static double getMin(double a, double b, double c)
        {
            double min = 0;
            if (a != 0)
            {
                min = a;
            }
            else if (b != 0)
            {
                min = b;
            }

            if (min > a && b != 0) min = a;
            if (min > b && b != 0) min = b;
            if (min > c && c != 0) min = c;
            return min;
        }
        private static bool checkAPIbug(Coin coin)
        {
            return false;
            //if (algo.apibug == true) return true;
            double a = coin.estimate_current;
            double b = coin.estimate_last24h;
            double c = coin.actual_last24h;
            if (a > b * 15000 || a > c * 15000 ||
                b > a * 15000 || b > c * 15000 ||
                c > a * 15000 || c > b * 15000)
            {
                Helpers.ConsolePrint("Stats", coin.algo + " API bug");
                coin.apibug = true;
                return true;
            }
            return false;
        }
        private static List<string> delalgos = new();

        private class MostProfitableCoin
        {
            public string coin { get; set; }
            public double currentProfit { get; set; }
        }
        private static void setAlgos(List<Coin> _coinlist)
        {
            var coinlist = GetMostProfitCoins(_coinlist);
            try
            {
                delalgos.Clear();
                if (coinlist != null && coinlist.Count > 0)
                {
                    foreach (AlgorithmType alg in Enum.GetValues(typeof(AlgorithmType)))
                    {
                        if ((int)alg >= 1 && !alg.ToString().Contains("UNUSED"))
                        {
                            delalgos.Add(alg.ToString().ToLower());
                        }
                    }
                }
                Dictionary<AlgorithmType, MostProfitableCoin> data = new();

                foreach (var coin in coinlist)
                {
                    string AlgorithmName = "";
                    string _AlgorithmName = "";
                    foreach (AlgorithmType _algo in Enum.GetValues(typeof(AlgorithmType)))
                    {
                        AlgorithmName = AlgorithmNames.GetName(_algo);
                        _AlgorithmName = AlgorithmNames.GetName(_algo).Replace("_", "-");
                        if (_algo >= 0 && coin.algo.ToUpper().Equals(AlgorithmName.ToUpper()) ||
                            _algo >= 0 && coin.algo.ToUpper().Equals(_AlgorithmName.ToUpper()))
                        {
                            if (!data.ContainsKey(_algo))
                            {
                                MostProfitableCoin _MostProfitableCoin = new();
                                _MostProfitableCoin.coin = coin.symbol;
                                if (ConfigManager.GeneralConfig.AdaptiveAlgo)
                                {
                                    _MostProfitableCoin.currentProfit = coin.adaptive_profit;
                                }
                                else
                                {
                                    _MostProfitableCoin.currentProfit = coin.profit;
                                }
                                if (_MostProfitableCoin.currentProfit > 0)
                                {
                                    data.Add(_algo, _MostProfitableCoin);
                                }
                            }

                            if (delalgos.Exists(e => e.Equals(coin.algo.ToLower()))) delalgos.Remove(coin.algo.ToLower());
                            break;
                        }
                    }
                }

                if (data != null)
                {
                    foreach (var algo in data)
                    {
                        var algoKey = algo.Key;

                        if (algoKey.ToString().Contains("UNUSED"))
                        {
                            continue;
                        }

                        var AlgorithmName = AlgorithmNames.GetName(algoKey);
                        //Helpers.ConsolePrint("** " + algoKey.ToString(), algo.Value.coin + " " + algo.Value.currentProfit.ToString());
                    }
                }

                delalgos.Remove("neoscrypt_xaya");
                delalgos.Remove("xelisv2_pepew");
                SetAlgorithmRates(data, 1, false);
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrintError("setAlgos", ex.ToString());
            }
            AlgosProfitData.FinalizeAlgosProfitList();
        }
       
        public static void CheckNewAlgo()
        {
            List<string> algolist = new();
            foreach (AlgorithmType alg in Enum.GetValues(typeof(AlgorithmType)))
            {
                algolist.Add(alg.ToString().ToLower().Replace("_unused", ""));
            }
            foreach (var algo in zergpoolAlgos)
            {
                if (!algolist.Contains(algo.Replace("-", "_")))
                {
                    Helpers.ConsolePrint("Stats", "Zergpool added algorithm: " + algo);
                }
            }
        }

        public static ConcurrentDictionary<string, CoinBlocked> coinsBlocked = new();
        public class CoinBlocked
        {
            public string coin;
            public int checkTime;
        }
        private static void SetAlgorithmRates(Dictionary<AlgorithmType, MostProfitableCoin> data, int multipl = 1, 
            bool average = false)
        {
            double paying = 0.0d;
            string coin = "Unknown";
            try
            {
                var payingDict = data;
                if (data != null)
                {
                    foreach (var algo in data)
                    {
                        var algoKey = algo.Key;

                        if (algoKey.ToString().Contains("UNUSED"))
                        {
                            continue;
                        }
                        //Helpers.ConsolePrint("SetAlgorithmRates " + algoKey.ToString(), algo.Value.coin + " " + algo.Value.currentProfit.ToString());
                        var AlgorithmName = AlgorithmNames.GetName(algoKey);
                        paying = algo.Value.currentProfit;
                        coin = algo.Value.coin;
                        AlgosProfitData.UpdatePayingForAlgo(algoKey, coin, paying, true);
                    }
                }
                //testing
                //payingDict[AlgorithmType.RandomX] = 12345;
                //NHSmaData.UpdateSmaPaying(payingDict, average);
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("SetAlgorithmRates", e.ToString());
            }
        }
    }
}



