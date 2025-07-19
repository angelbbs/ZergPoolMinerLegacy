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

        public static List<MiningAlgorithms> MiningAlgorithmsList = new();
        public class MiningAlgorithms
        {
            public string name { get; set; }
            public string coin { get; set; }
            public int port { get; set; }
            public int tls_port { get; set; }
            public double hashrate { get; set; }
            public double estimate_current { get; set; }
            public double estimate_last24h { get; set; }
            public double actual_last24h { get; set; }
            public double mbtc_mh_factor { get; set; }
            public double profit { get; set; }
            //public double curProfitFactor { get; set; }
            public double adaptive_factor { get; set; }
            public double adaptive_profit { get; set; }
            public bool apibug { get; set; }
            public bool CPU { get; set; }
            public bool GPU { get; set; }
            public bool FPGA { get; set; }
            public bool ASIC { get; set; }
            public bool tempDisabled { get; set; }
            public bool algoTempDeleted { get; set; }
        }

        public static List<Coin> CoinList = new();
        public static List<Coin> MinerStatCoinList = new();
        public class Coin
        {
            public string name { get; set; }
            public string algo { get; set; }
            public string symbol { get; set; }
            public int port { get; set; }
            public int tls_port { get; set; }
            public double hashrate { get; set; }
            public double adaptive_factor { get; set; }
            public double estimate { get; set; }
            public double estimate_current { get; set; }
            public double estimate_last24h { get; set; }
            public double actual_last24h { get; set; }
            public double mbtc_mh_factor { get; set; }
            public double pool_ttf { get; set; }
            public double real_ttf { get; set; }
            public double timesincelast_shared { get; set; }
            public double minpay { get; set; }
            public double minpay_sunday { get; set; }
            public double owed { get; set; }
            public double effort { get; set; }
            public bool apibug { get; set; }
            public bool CPU { get; set; }
            public bool GPU { get; set; }
            public bool FPGA { get; set; }
            public bool ASIC { get; set; }
            public bool tempTTF_Disabled { get; set; }
            public bool coinTempDeleted = true;
            public bool tempBlock { get; set; }
            public int noautotrade { get; set; }
        }

        private static int httpsProxyCheck = 0;
        public static string CurrentProxyIP;
        public static int CurrentProxyHTTPSPort;
        public static int CurrentProxySocks5SPort;
        public static async Task<string> GetPoolApiAsync(string url, int timeout = 5)
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
                            responseFromServer = await GetPoolApiDataAsync(url, proxy, true);
                            if (!string.IsNullOrEmpty(responseFromServer))
                            {
                                Helpers.ConsolePrint("GetPoolApiData", "Received bytes: " +
                                responseFromServer.Length.ToString() + " from " + url + " " +
                                proxy.Ip + ":" + proxy.HTTPSPort);
                                /*
                                CurrentProxyIP = proxy.Ip;
                                CurrentProxyHTTPSPort = proxy.HTTPSPort;
                                CurrentProxySocks5SPort = proxy.Socks5Port;
                                */
                                break;
                            }
                            else
                            {
                                Helpers.ConsolePrintError("GetPoolApiAsync", "Proxy offline: " + 
                                    proxy.Ip + ":" + proxy.HTTPSPort.ToString());
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
                    Helpers.ConsolePrintError("GetPoolApiAsync", "All proxy unavailable");
                    new Task(() => ProxyCheck.GetHttpsProxy()).Start();
                }
            } else
            {
                try
                {
                    responseFromServer = await GetPoolApiDataAsync(url, null, false);
                    if (!string.IsNullOrEmpty(responseFromServer))
                    {
                        Helpers.ConsolePrint("GetPoolApiData", "Received bytes: " +
                        responseFromServer.Length.ToString() + " from " + url + " ");
                    }
                    else
                    {
                        Helpers.ConsolePrintError("GetPoolApiData", "Error getting data from " + url);
                    }
                }
                catch (Exception ex)
                {
                    Helpers.ConsolePrintError("GetPoolApiAsync", ex.ToString());
                }
            }
            return responseFromServer;
        }

        public static async Task<string> GetPoolApiDataAsync(string url, ProxyChecker.Proxy proxy, bool viaProxy)
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
                    Helpers.ConsolePrint("GetPoolApiData", id + "Try connect to " + url + " via proxy " +
                        proxy.Ip + ":" + proxy.HTTPSPort.ToString());
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
                    Helpers.ConsolePrint("GetPoolApiData", id + "Try connect to " + url);
                }
                using (httpClient)
                {
                    bool success = false;
                    new Thread(() =>
                    {
                        Thread.Sleep(1000 * 15);
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
                            Helpers.ConsolePrintError("GetPoolApiDataAsync", id + "Error! Not JSON from " + url);
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
        public static async Task<List<Coin>> GetCoinsAsync(string link)
        {
            Helpers.ConsolePrint("Stats", "Trying " + link);
            double correction = 0.90;
            List<AlgoCoin> noautotradeCoin = new();
            List<AlgoCoin> zeroHashrateCoin = new();
            try
            {
                string ResponseFromAPI = await GetPoolApiAsync(link, 7);
                if (ResponseFromAPI != null)
                {
                    var data = JObject.Parse(ResponseFromAPI);
                    CoinList.Clear();
                    foreach (var item in data)
                    {
                        var coin = item.Value;
                        string name = coin.Value<string>("name");
                        string algo = coin.Value<string>("algo");
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
                        var effort = coin.Value<double>("effort");
                        var minpay = coin.Value<double>("minpay");
                        var minpay_sunday = coin.Value<double>("minpay_sunday");
                        var noautotrade = coin.Value<int>("noautotrade");

                        Coin _coin = new();
                        _coin.name = name;
                        _coin.algo = algo;
                        _coin.symbol = symbol;
                        _coin.pool_ttf = pool_ttf;
                        _coin.real_ttf = real_ttf;
                        _coin.timesincelast_shared = Math.Round(timesincelast_shared / 60, 1);
                        _coin.owed = owed;
                        _coin.effort = effort;
                        _coin.minpay = minpay;
                        _coin.minpay_sunday = minpay_sunday;
                        _coin.port = port;
                        _coin.tls_port = tls_port;
                        _coin.hashrate = hashrate;
                        _coin.adaptive_factor = 0.9;
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

                        if ((real_ttf > ConfigManager.GeneralConfig.maxTTF) && (_coin.CPU || _coin.GPU) &&
                            !_coin.tempTTF_Disabled)
                        {
                            Helpers.ConsolePrint("Stats", _coin.algo + " (" + _coin.symbol + ") TTF " +
                                GetTime((int)real_ttf) + ". Disabled");
                            _coin.tempTTF_Disabled = true;
                            _coin.estimate = _coin.estimate / 99;
                            _coin.estimate_current = _coin.estimate_current / 99;
                            _coin.estimate_last24h = _coin.estimate_last24h / 99;
                            _coin.actual_last24h = _coin.actual_last24h / 99;
                        }
                        else
                        {
                            _coin.tempTTF_Disabled = false;
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
                            _coin.estimate = _coin.estimate / 99;
                            _coin.estimate_current = _coin.estimate_current / 99;
                            _coin.estimate_last24h = _coin.estimate_last24h / 99;
                            _coin.actual_last24h = _coin.actual_last24h / 99;
                        }

                        if (noautotrade == 1 && (_coin.CPU || _coin.GPU))
                        {
                            AlgoCoin nac = new();
                            nac.algo = _coin.algo;
                            nac.coin = _coin.symbol;
                            noautotradeCoin.Add(nac);
                            _coin.estimate = _coin.estimate / 99;
                            _coin.estimate_current = _coin.estimate_current / 99;
                            _coin.estimate_last24h = _coin.estimate_last24h / 99;
                            _coin.actual_last24h = _coin.actual_last24h / 99;
                        }


                        var unstableAlgosList = AlgorithmSwitchingManager.unstableAlgosList.Select(s => s.ToString().ToLower()).ToList();
                        if (unstableAlgosList.Contains(algo.ToLower()))
                        {
                            _coin.estimate = _coin.estimate * 0.3;
                            _coin.estimate_current = _coin.estimate_current * 0.3;
                            _coin.estimate_last24h = _coin.estimate_last24h * 0.3;
                            _coin.actual_last24h = _coin.actual_last24h * 0.3;
                        }

                        if (_coin.owed < -1 && (_coin.CPU || _coin.GPU) && !_coin.tempTTF_Disabled)
                        {
                            _coin.estimate = _coin.estimate * 0.5;
                            _coin.estimate_current = _coin.estimate_current * 0.5;
                            _coin.estimate_last24h = _coin.estimate_last24h * 0.5;
                            _coin.actual_last24h = _coin.actual_last24h * 0.5;
                        }

                        if (_coin.effort / _coin.real_ttf > 500 && (_coin.CPU || _coin.GPU) && !_coin.tempTTF_Disabled)
                        {
                            _coin.estimate = _coin.estimate * 0.5;
                            _coin.estimate_current = _coin.estimate_current * 0.5;
                            _coin.estimate_last24h = _coin.estimate_last24h * 0.5;
                            _coin.actual_last24h = _coin.actual_last24h * 0.5;
                        }

                        if (_coin.effort / _coin.real_ttf > 50 && (_coin.CPU || _coin.GPU) && !_coin.tempTTF_Disabled)
                        {
                            _coin.estimate = _coin.estimate * 0.5;
                            _coin.estimate_current = _coin.estimate_current * 0.5;
                            _coin.estimate_last24h = _coin.estimate_last24h * 0.5;
                            _coin.actual_last24h = _coin.actual_last24h * 0.5;
                        }

                        //18++ hours
                        if ((real_ttf - timesincelast_shared >= 64800) &&
                            (_coin.CPU || _coin.GPU) && !_coin.tempTTF_Disabled)
                        {
                            _coin.estimate = _coin.estimate * 0.70;
                            _coin.estimate_current = _coin.estimate_current * 0.70;
                            _coin.estimate_last24h = _coin.estimate_last24h * 0.70;
                            _coin.actual_last24h = _coin.actual_last24h * 0.70;
                        }
                        //12-18 hours
                        if ((real_ttf - timesincelast_shared >= 43200 && real_ttf - timesincelast_shared < 64800) &&
                            (_coin.CPU || _coin.GPU) && !_coin.tempTTF_Disabled)
                        {
                            _coin.estimate = _coin.estimate * 0.80;
                            _coin.estimate_current = _coin.estimate_current * 0.80;
                            _coin.estimate_last24h = _coin.estimate_last24h * 0.80;
                            _coin.actual_last24h = _coin.actual_last24h * 0.80;
                        }
                        //6-12 hours
                        if ((real_ttf - timesincelast_shared >= 21600 && real_ttf - timesincelast_shared < 43200) &&
                            (_coin.CPU || _coin.GPU) && !_coin.tempTTF_Disabled)
                        {
                            _coin.estimate = _coin.estimate * 0.85;
                            _coin.estimate_current = _coin.estimate_current * 0.85;
                            _coin.estimate_last24h = _coin.estimate_last24h * 0.85;
                            _coin.actual_last24h = _coin.actual_last24h * 0.85;
                        }
                        //2-6 hours
                        if ((real_ttf - timesincelast_shared > 7200 && real_ttf - timesincelast_shared < 21600) &&
                            (_coin.CPU || _coin.GPU) && !_coin.tempTTF_Disabled)
                        {
                            _coin.estimate = _coin.estimate * 0.90;
                            _coin.estimate_current = _coin.estimate_current * 0.90;
                            _coin.estimate_last24h = _coin.estimate_last24h * 0.90;
                            _coin.actual_last24h = _coin.actual_last24h * 0.90;
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
                                Helpers.ConsolePrint("Stats", "No pool hashrate for " + _coin.algo +
                                    "(" + c.Key + "). Temporary block");
                                _coin.estimate = _coin.estimate * 0.5;
                                _coin.estimate_current = _coin.estimate_current * 0.2;
                                _coin.estimate_last24h = _coin.estimate_last24h * 0.2;
                                _coin.actual_last24h = _coin.actual_last24h * 0.2;
                                _coin.tempBlock = true;
                                //_coin.hashrate = 0;
                                //_coin.tempDeleted = true;

                                foreach (var miningDevice in MiningSession._miningDevices)
                                {
                                    /*
                                    //условие не успеет выполниться, т.к. произойдет переключение монеты
                                    if (miningDevice.DeviceCurrentMiningCoin.Equals(c.Key) &&
                                        _coin.tempTTF_Disabled)//уже заблокировано
                                    {
                                        _coin.estimate = _coin.estimate * 0.01;
                                        _coin.estimate_current = _coin.estimate_current * 0.01;
                                        _coin.estimate_last24h = _coin.estimate_last24h * 0.01;
                                        _coin.actual_last24h = _coin.actual_last24h * 0.01;
                                    }
                                    */
                                    //

                                    if (miningDevice.DeviceCurrentMiningCoin.Equals(c.Key) &&
                                                   !_coin.tempTTF_Disabled)//блокируем
                                    {
                                        miningDevice.needSwitch = true;
                                        /*
                                        _coin.tempTTF_Disabled = true;
                                        _coin.estimate = 0;
                                        _coin.estimate_current = 0;
                                        _coin.estimate_last24h = 0;
                                        _coin.actual_last24h = 0;
                                        _coin.hashrate = 0;
                                        */
                                    }
                                }
                            }
                        }

                        if (MiningAlgorithmsList is object && MiningAlgorithmsList != null &&
                            MiningAlgorithmsList.Count > 0)
                        {
                            var ma = MiningAlgorithmsList.Find(a => a.name.ToLower() == _coin.algo.ToLower());
                            if (ma != null)
                            {
                                if (ma.adaptive_factor != 0) _coin.adaptive_factor = ma.adaptive_factor;
                            }
                        }
                        if (!_coin.coinTempDeleted)
                        {
                            CoinList.Add(_coin);
                        } else
                        {
                            Helpers.ConsolePrint("Stats", _coin.algo + " (" + _coin.name + ") deleted");
                        }
                    }
                }

                noautotradeCoin.Sort((x, y) => x.algo.CompareTo(y.algo));
                string _algo = "";
                string coins = "";
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
                        Helpers.ConsolePrint("Stats", _algo + " (" + coins.Substring(0, coins.Length - 2) + ") no autotrade. Disabled");
                        coins = "";
                        _algo = c.algo;
                        coins = coins + c.coin + ", ";
                    }
                }

                zeroHashrateCoin.Sort((x, y) => x.algo.CompareTo(y.algo));
                _algo = "";
                coins = "";
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
                        Helpers.ConsolePrint("Stats", _algo + " (" + coins.Substring(0, coins.Length - 2) + ") zero hashrate. Disabled");
                        coins = "";
                        _algo = c.algo;
                        coins = coins + c.coin + ", ";
                    }
                }

                var json = JsonConvert.SerializeObject(CoinList, Formatting.Indented);
                if (json.Length > 5)
                {
                    Helpers.WriteAllTextWithBackup("configs\\CoinList.json", json);
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrintError("GetCoins", ex.ToString());
                var json = File.ReadAllText("configs\\CoinList.json");
                CoinList = (List<Coin>)JsonConvert.DeserializeObject(json);
            }
            return CoinList;
        }

        public static async Task<List<Coin>> GetZPoolAsync()
        {
            Helpers.ConsolePrint("Stats", "Trying GetZPool");
            double correction = 0.9;
            try
            {
                string ResponseFromAPI = await GetPoolApiAsync(Links.CurrenciesZPool);
                if (ResponseFromAPI != null)
                {
                    var data = JObject.Parse(ResponseFromAPI);
                    MinerStatCoinList.Clear();
                    foreach (var item in data)
                    {
                        var coin = item.Value;
                        string name = coin.Value<string>("name");
                        string algo = coin.Value<string>("algo");
                        string symbol = item.Key;
                        var _estimate = coin.Value<double>("estimate");
                        
                        var mbtc_mh_factor = coin.Value<double>("mbtc_mh_factor");


                        Coin _coin = new();
                        _coin.name = name;
                        _coin.algo = algo;
                        _coin.symbol = symbol;
                        _coin.estimate = (_estimate / mbtc_mh_factor * 1) * correction;//mBTC/GH
                        _coin.estimate_current = (_estimate / mbtc_mh_factor * 1000) * correction;//mBTC/GH
                        _coin.estimate_last24h = (_estimate / mbtc_mh_factor * 1000) * correction;
                        _coin.actual_last24h = (_estimate / mbtc_mh_factor / 1000 * 1000) *
                             correction;//zergpool api bug
                       
                        _coin.mbtc_mh_factor = mbtc_mh_factor;//multiplier,
                                                              //value 1 represents Mh,
                                                              //1000 represents GH,
                                                              //1000000 represents TH,
                                                              //0.001 represents KH
                                                              //miningAlgorithms.algotype = algotype;//integer value of a 4-bit value
                                                              //representing platforms supported.
                                                              //Bit 3 = CPU, bit 2 = GPU,
                                                              //bit 1 = ASIC, bit 0 = FPGA


                        MinerStatCoinList.Add(_coin);
                    }
                }

                //var json = JsonConvert.SerializeObject(CoinList, Formatting.Indented);
                //Helpers.WriteAllTextWithBackup("configs\\CoinList.json", json);

            }
            catch (Exception ex)
            {
                Helpers.ConsolePrintError("GetMinerStat", ex.ToString());
            }
            return MinerStatCoinList;
        }

        //hashrate.no 5d3c49fe72c884c1c8522d13d8edbdd1
        //test cb4a618e1fd885361563c881a1ce3e25
        //ошибки на монетах. нет половины монет. не используется
        public static async Task<List<Coin>> GetHashrateNoAsync()
        {
            Helpers.ConsolePrint("Stats", "Trying GetHashrateNo");
            try
            {
                string ResponseFromAPI = await GetPoolApiAsync(Links.hashrateno + "cb4a618e1fd885361563c881a1ce3e25");
                if (ResponseFromAPI != null)
                {
                    Helpers.ConsolePrint("*", ResponseFromAPI);
                    dynamic data = JsonConvert.DeserializeObject(ResponseFromAPI);
                    MinerStatCoinList.Clear();
                    foreach (var item in data)
                    {
                        string name = item.name;
                        string algo = item.algo;
                        algo = algo.ToLower().Replace("cryptonightgpu", "cryptonight_gpu");
                        algo = algo.ToLower().Replace("zelhash", "equihash125");
                        algo = algo.ToLower().Replace("zhash", "equihash144");
                        string symbol = item.coin;
                        algo = algo.Replace("PLSR-CURVE", "PLSR");
                        algo = algo.Replace("PLSR-MINO", "PLSR");
                        string coinEstimateUnit = item.coinEstimateUnit;
                        string _usdEstimate = item.usdEstimate;
                        double.TryParse(_usdEstimate, out double usdEstimate);

                        double mult = 0;
                        coinEstimateUnit = coinEstimateUnit.ToLower();
                        if (coinEstimateUnit.Equals("h"))
                        {//cryptonight_gpu, dynexsolve, randomx, ghostrider
                            mult = 10000000000;
                        }
                        if (coinEstimateUnit.Equals("kh"))
                        {//flex, xelisv2-pepew, minotaurx, spectrex, verthash, xehash, xelishashv2, cryptonightturtle
                            mult = 10000000;
                        }
                        if (coinEstimateUnit.Equals("mh"))
                        {//abelhash, kawpow, astrixhash, x16rt, autolykos2, ethash, etchash, octopus,
                         //cryptixhash, scrypt, evrprogpow, firopow, hoohash, fishhash, meowpow, nexapow,
                         //progpowz, sha256dt, phihash, curvehash, sha512256d, skydoge, meraki, verushash,
                         //warthog, zilhash
                            mult = 0.01;
                        }
                        if (coinEstimateUnit.Equals("gh"))
                        {
                            mult = 1000000;
                        }
                        if (coinEstimateUnit.Equals("th"))
                        {
                            mult = 1000;
                        }

                        if (coinEstimateUnit.Equals("sol"))
                        {//BEAM, Zhash, ZelHash
                            mult = 10000000000;
                        }
                        if (coinEstimateUnit.Equals("ksol"))
                        {
                            mult = 1;
                        }

                        if (coinEstimateUnit.Equals("it"))
                        {
                            mult = 1000000000;
                        }

                        if (coinEstimateUnit.Equals("mproof"))
                        {
                            mult = 1000;
                        }
                        if (coinEstimateUnit.Equals("gp"))
                        {
                            mult = 1;
                        }

                        double _estimate = ExchangeRateApi.ConvertUSDToNationalCurrency(
                            ExchangeRateApi.ConvertNationalCurrencyToBTC(usdEstimate)) * mult;
                        double estimate_current = _estimate;
                        double estimate_last24h = _estimate;
                        double actual_last24h = _estimate;

                        Coin _coin = new();
                        _coin.name = name;
                        _coin.algo = algo.ToLower();
                        _coin.symbol = symbol;
                        _coin.estimate = estimate_current;
                        _coin.estimate_current = estimate_current;
                        _coin.estimate_last24h = estimate_current;
                        _coin.actual_last24h = estimate_current;


                        MinerStatCoinList.Add(_coin);
                        
                    }
                }

                //var json = JsonConvert.SerializeObject(CoinList, Formatting.Indented);
                //Helpers.WriteAllTextWithBackup("configs\\CoinList.json", json);

            }
            catch (Exception ex)
            {
                Helpers.ConsolePrintError("GetMinerStat", ex.ToString());
            }
            return MinerStatCoinList;
        }

        //ошибки на порядок на разных монетах - не используется
        public static async Task<List<Coin>> GetMinerStatAsync()
        {
            Helpers.ConsolePrint("Stats", "Trying GetMinerStat");
            try
            {
                string ResponseFromAPI = await GetPoolApiAsync(Links.minerstat);
                if (ResponseFromAPI != null)
                {
                    dynamic data = JsonConvert.DeserializeObject(ResponseFromAPI);
                    //double unpaid = data.@unpaid;
                    //var data = JObject.Parse(ResponseFromAPI);
                    MinerStatCoinList.Clear();
                    foreach (var item in data)
                    {
                        /*
                        var coin = item.Value;
                        string name = coin.Value<string>("name");
                        string algo = coin.Value<string>("algorithm");
                        string symbol = coin.Value<string>("coin");
                        var _estimate = coin.Value<double>("reward") * 24;
                        double estimate_current = _estimate;
                        var estimate_last24h = _estimate;
                        var actual_last24h = _estimate;
                        */
                        string name = item.name;
                        string algo = item.algorithm;
                        string symbol = item.coin;
                        string reward_unit = item.reward_unit;
                        double price = item.price;
                        if (price == 0)
                        {
                            price = ExchangeRateApi.MarketRatesList.FirstOrDefault(c => c.Key == symbol).Value;
                        }
                        double _estimate = 0d;
                        double estimate_current = 0d;
                        double estimate_last24h = 0d;
                        double actual_last24h = 0d;
                        if (symbol.Equals(reward_unit))
                        {
                            _estimate = item.reward * price * 24 * 10000;
                            estimate_current = _estimate;
                            estimate_last24h = _estimate;
                            actual_last24h = _estimate;
                        }

                        Coin _coin = new();
                        _coin.name = name;
                        _coin.algo = algo;
                        _coin.symbol = symbol;
                        _coin.estimate = estimate_current;
                        _coin.estimate_current = estimate_current;
                        _coin.estimate_last24h = estimate_current;
                        _coin.actual_last24h = estimate_current;


                        MinerStatCoinList.Add(_coin);
                    }
                }

                //var json = JsonConvert.SerializeObject(CoinList, Formatting.Indented);
                //Helpers.WriteAllTextWithBackup("configs\\CoinList.json", json);

            }
            catch (Exception ex)
            {
                Helpers.ConsolePrintError("GetMinerStat", ex.ToString());
            }
            return MinerStatCoinList;
        }

        public static List<string> zergpoolAlgos = new();
        public static async Task<List<MiningAlgorithms>> GetZergPoolAlgosListExAsync()
        {
            List<string> progAlgosList = new();
            List<string> poolAlgosList = new();
            var coins = await GetCoinsAsync(Links.Currencies);
            //var hashratenocoins = GetHashrateNo();
            //var minerStatcoins = GetMinerStat();
            //var ZPoolcoins = GetZPool();
            if (coins.Count == 0)
            {
                return MiningAlgorithmsList;
            }
            try
            {
                foreach (AlgorithmType alg in Enum.GetValues(typeof(AlgorithmType)))
                {
                    string _alg = alg.ToString().ToLower();
                    _alg = _alg.Replace("xelisv2_pepew", "xelisv2-pepew");
                    _alg = _alg.Replace("neoscrypt_xaya", "neoscrypt-xaya");
                    Coin defcoin = new();
                    if ((int)alg >= 1000 && !alg.ToString().Contains("unused"))
                    {
                        progAlgosList.Add(_alg);
                        foreach (var coin in coins)
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
                                    if (coin.estimate_current >= defcoin.estimate_current)
                                    {
                                        defcoin = coin;
                                        defcoin.algo = _alg;
                                    }
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(defcoin.algo))
                    {
                        MiningAlgorithms miningAlgorithms = new();
                        miningAlgorithms.actual_last24h = defcoin.actual_last24h;
                        miningAlgorithms.ASIC = defcoin.ASIC;
                        miningAlgorithms.CPU = defcoin.CPU;
                        miningAlgorithms.estimate_current = defcoin.estimate_current;
                        miningAlgorithms.estimate_last24h = defcoin.estimate_last24h;
                        miningAlgorithms.FPGA = defcoin.FPGA;
                        miningAlgorithms.GPU = defcoin.GPU;
                        miningAlgorithms.hashrate = defcoin.hashrate;
                        miningAlgorithms.mbtc_mh_factor = defcoin.mbtc_mh_factor;
                        miningAlgorithms.name = alg.ToString().ToLower();
                        miningAlgorithms.port = defcoin.port;
                        miningAlgorithms.tls_port = defcoin.tls_port;
                        miningAlgorithms.coin = defcoin.symbol;

                        if (miningAlgorithms.CPU || miningAlgorithms.GPU)
                        {
                            if (MiningAlgorithmsList.Any(item => item.name.ToLower() == miningAlgorithms.name.ToLower()))
                            {
                                var obj = MiningAlgorithmsList.FirstOrDefault(x => x.name.ToLower() == miningAlgorithms.name.ToLower());
                                if (obj != null)
                                {
                                    obj.hashrate = miningAlgorithms.hashrate;
                                    obj.estimate_current = miningAlgorithms.estimate_current;
                                    obj.estimate_last24h = miningAlgorithms.estimate_last24h;
                                    obj.actual_last24h = miningAlgorithms.actual_last24h;
                                    obj.mbtc_mh_factor = miningAlgorithms.mbtc_mh_factor;
                                    obj.coin = miningAlgorithms.coin;
                                }
                            }
                            else
                            {
                                MiningAlgorithmsList.Add(miningAlgorithms);
                            }
                        }
                    }
                }

                foreach (var a in progAlgosList)
                {
                    string _alg = a.ToLower();
                    _alg = _alg.Replace("xelisv2-pepew", "xelisv2_pepew");
                    _alg = _alg.Replace("neoscrypt-xaya", "neoscrypt_xaya");
                    if (!poolAlgosList.Contains(_alg))
                    {
                        Helpers.ConsolePrint("Stats", "Deleted? - " + _alg);
                        //a.tempDeleted = true;
                        var itemToDelete = MiningAlgorithmsList.Find(r => r.name.ToLower() == _alg);
                        if (itemToDelete != null)
                        {
                            itemToDelete.algoTempDeleted = true;
                        }
                        //MiningAlgorithmsList.RemoveAll(x => x.name.ToLower() == _alg);
                    } else
                    {
                        var itemToDelete = MiningAlgorithmsList.Find(r => r.name.ToLower() == _alg);
                        if (itemToDelete != null) itemToDelete.algoTempDeleted = false;
                    }
                }

                var json = JsonConvert.SerializeObject(MiningAlgorithmsList, Formatting.Indented);
                Helpers.WriteAllTextWithBackup("configs\\AlgoritmsList.json", json);
            } catch (Exception ex)
            {
                Helpers.ConsolePrintError("GetZergPoolAlgosListEx", ex.ToString());
            }
            return MiningAlgorithmsList;
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
        /*
        public static List<MiningAlgorithms> GetZergPoolAlgosList()
        {
            List<string> algosList = new();
            Helpers.ConsolePrint("Stats", "Trying GetZergPoolAlgosList");
            try
            {
                string ResponseFromAPI = GetPoolApiData(Links.PoolStatus);
                if (ResponseFromAPI != null)
                {
                    var data = JObject.Parse(ResponseFromAPI);
                    foreach (var item in data)
                    {
                        var algo = item.Value;

                        string name = algo.Value<string>("name");
                        var port = algo.Value<int>("port");
                        var tls_port = algo.Value<int>("tls_port");
                        var _estimate_current = algo.Value<string>("estimate_current");
                        double.TryParse(_estimate_current, out double estimate_current);
                        var _estimate_last24h = algo.Value<string>("estimate_last24h");
                        double.TryParse(_estimate_last24h, out double estimate_last24h);
                        var _actual_last24h = algo.Value<string>("actual_last24h");
                        double.TryParse(_actual_last24h, out double actual_last24h);
                        var mbtc_mh_factor = algo.Value<double>("mbtc_mh_factor");
                        var algotype = algo.Value<int>("algotype");
                        var hashrate = algo.Value<double>("hashrate");

                        algosList.Add(name.ToLower());

                        MiningAlgorithms miningAlgorithms = new();
                        miningAlgorithms.name = name;
                        miningAlgorithms.port = port;
                        miningAlgorithms.tls_port = tls_port;
                        miningAlgorithms.hashrate = hashrate;
                        miningAlgorithms.estimate_current = (estimate_current / mbtc_mh_factor * 1000000);//mBTC/GH
                        miningAlgorithms.estimate_last24h = (estimate_last24h / mbtc_mh_factor * 1000000);
                        miningAlgorithms.actual_last24h = (actual_last24h / mbtc_mh_factor / 1000 * 1000000);//zergpool api bug
                        miningAlgorithms.mbtc_mh_factor = mbtc_mh_factor;//multiplier,
                                                                         //value 1 represents Mh,
                                                                         //1000 represents GH,
                                                                         //1000000 represents TH,
                                                                         //0.001 represents KH
                                                                         //miningAlgorithms.algotype = algotype;//integer value of a 4-bit value
                                                                         //representing platforms supported.
                                                                         //Bit 3 = CPU, bit 2 = GPU,
                                                                         //bit 1 = ASIC, bit 0 = FPGA

                        BitArray b = new BitArray(new int[] { algotype });
                        miningAlgorithms.CPU = b[3];
                        miningAlgorithms.GPU = b[2];
                        miningAlgorithms.ASIC = b[1];
                        miningAlgorithms.FPGA = b[0];

                        if (miningAlgorithms.CPU || miningAlgorithms.GPU)
                        {
                            if (MiningAlgorithmsList.Any(item => item.name.ToLower() == miningAlgorithms.name.ToLower()))
                            {
                                var obj = MiningAlgorithmsList.FirstOrDefault(x => x.name.ToLower() == miningAlgorithms.name.ToLower());
                                if (obj != null)
                                {
                                    obj.hashrate = miningAlgorithms.hashrate;
                                    obj.estimate_current = miningAlgorithms.estimate_current;
                                    obj.estimate_last24h = miningAlgorithms.estimate_last24h;
                                    obj.actual_last24h = miningAlgorithms.actual_last24h;
                                    obj.mbtc_mh_factor = miningAlgorithms.mbtc_mh_factor;
                                }
                            }
                            else
                            {
                                MiningAlgorithmsList.Add(miningAlgorithms);
                            }
                        }
                    }
                    var arr = MiningAlgorithmsList.ToList();
                    foreach (var a in arr)
                    {
                        if (!algosList.Contains(a.name.ToLower()))
                        {
                            Helpers.ConsolePrint("Stats", "Deleted? - " + a.name);
                            a.tempDeleted = true;
                            //MiningAlgorithmsList.RemoveAll(x => x.name.ToLower() == a.name.ToLower());
                            if (a.adaptive_factor == 0)
                            {
                                var itemToRemove = MiningAlgorithmsList.SingleOrDefault(r => r.name.ToLower() == a.name.ToLower());
                                if (itemToRemove != null)
                                {
                                    MiningAlgorithmsList.Remove(itemToRemove);
                                    //break;
                                }
                            }
                        }
                    }
                    var json = JsonConvert.SerializeObject(MiningAlgorithmsList, Formatting.Indented);
                    Helpers.WriteAllTextWithBackup("configs\\AlgoritmsList.json", json);
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrintError("GetZergPoolAlgosList", ex.ToString());
            }
            return MiningAlgorithmsList;
        }
        */
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

        public class AlgoProperty
        {
            public string name;
            public string hashrate;
            public double localhashrate;
            public double actualhashrate;
            public double localProfit;
            public double actualProfit;
            public double profitRatio = 1d;
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
                    double actualProfit = 0d;
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
                    foreach (var alg in MiningAlgorithmsList)
                    {
                        if (alg.adaptive_factor != 0 && alg.adaptive_factor < 0.95)
                        {
                            alg.adaptive_factor = alg.adaptive_factor + 0.00002;
                        }
                        if (alg.adaptive_factor != 0 && alg.adaptive_factor > 1.01)
                        {
                            alg.adaptive_factor = alg.adaptive_factor - 0.00002;
                        }
                        if (alg.algoTempDeleted) continue;
                        if (alg.tempDisabled) continue;
                        double actualHashrate = 0d;
                        foreach (var cur in data.SelectToken("miners"))
                        {
                            string _algo = cur.SelectToken("algo");
                            _algo = _algo.Replace("xelisv2-pepew", "xelisv2_pepew");
                            _algo = _algo.Replace("neoscrypt-xaya", "neoscrypt_xaya");
                            string _hashrate = cur.SelectToken("hashrate");
                            string _ID = cur.SelectToken("ID");
                            int _subscribe = cur.SelectToken("subscribe");
                            double _accepted = cur.SelectToken("accepted");
                            if (alg.name.ToLower().Equals(_algo.ToLower()) &&
                            _ID.Equals(Miner.GetFullWorkerName()))
                            {
                                var _algoProperty = new AlgoProperty();
                                double localHashrate = 0d;
                                foreach (var miningDevice in MiningSession._miningDevices)
                                {
                                    if (alg.name.ToLower().Equals(((AlgorithmType)miningDevice.Device.AlgorithmID).ToString().ToLower()))
                                    {
                                        localHashrate = localHashrate + miningDevice.Device.MiningHashrate;
                                    }
                                    if (alg.name.ToLower().Equals(((AlgorithmType)miningDevice.Device.SecondAlgorithmID).ToString().ToLower()))
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

                                localProfit = (localHashrate * alg.profit);
                                actualHashrate = actualHashrate + _accepted * (1.00);

                                if (algosProperty.ContainsKey(alg.name.ToLower()))
                                {
                                    if (MiningSession._miningDevices.Exists(x => ((AlgorithmType)x.Device.AlgorithmID).ToString().ToLower() == alg.name.ToLower()))
                                    {
                                        _algoProperty.ticks = algosProperty.FirstOrDefault(x => x.Key.ToLower() == alg.name.ToLower()).Value.ticks;
                                    }
                                    if (MiningSession._miningDevices.Exists(x => ((AlgorithmType)x.Device.SecondAlgorithmID).ToString().ToLower() == alg.name.ToLower()))
                                    {
                                        _algoProperty.ticks = algosProperty.FirstOrDefault(x => x.Key.ToLower() == alg.name.ToLower()).Value.ticks;
                                    }
                                    _algoProperty.factorsList = algosProperty.FirstOrDefault(x => x.Key.ToLower() == alg.name.ToLower()).Value.factorsList;
                                }
                                _algoProperty.localhashrate = localHashrate;
                                _algoProperty.localProfit = (localProfit);
                                _algoProperty.profitRatio = alg.profit;
                                _algoProperty.actualhashrate = actualHashrate;
                                _algoProperty.name = alg.name.ToLower();
                                _algoProperty.hashrate = _hashrate;
                                _algoProperty.adaptive_factor = alg.adaptive_factor;
                                _algoProperty.adaptive_profit = alg.adaptive_profit;
                                /*
                                List<string> stringMiningAlgorithmsList = MiningAlgorithmsList.Select(s => s.name.ToString().ToLower()).ToList();
                                if (!currentminingAlgos.Contains(_algoProperty.name.ToLower()))
                                {
                                    Helpers.ConsolePrint("Stats", "Delete current mining algo: " + _algoProperty.name);
                                    algosProperty.TryRemove(_algoProperty.name, out var r);
                                    //Form_Main.adaptiveRunning = false;
                                }
                                */
                                if (currentminingAlgos.Contains(alg.name.ToLower()))
                                {
                                    algosProperty.AddOrUpdate(alg.name.ToLower(), _algoProperty, (k, v) => _algoProperty);
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
                            //var alg = MiningAlgorithmsList.FirstOrDefault(x => x.name.ToLower() == __algoProperty.Key.ToLower());
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

                                    if (algosProperty.ContainsKey(cp.algorithm))
                                    {
                                        //Helpers.ConsolePrint("******** adding " + cp.symbol, _symbol + "worker hashrate for " + cp.algorithm);
                                        coinsMining.Add(cp);
                                    }
                                    else
                                    {
                                        //Helpers.ConsolePrint("******** " + cp.symbol, "No worker hashrate for " + cp.algorithm);
                                    }
                                }
                            }
                        }
                    }

                    foreach (var __algoProperty in algosProperty)
                    {
                        double average = 1.0;
                        var _algoProperty = __algoProperty.Value;

                        /*
                        List<string> stringMiningAlgorithmsList = MiningAlgorithmsList.Select(s => s.name.ToString().ToLower()).ToList();
                        if (!stringMiningAlgorithmsList.Contains(_algoProperty.name.ToLower()))
                        {
                            Helpers.ConsolePrint("Stats", "Deleted current mining algo: " + _algoProperty.name);
                            algosProperty.TryRemove(_algoProperty.name, out var r);
                            //Form_Main.adaptiveRunning = false;
                        }
                        */
                        var alg = MiningAlgorithmsList.FirstOrDefault(x => x.name.ToLower() == __algoProperty.Key.ToLower());

                        //пусть на графике присутствуют предыдущие значения
                        actualProfit = actualProfit + (_algoProperty.actualhashrate * alg.profit);
                        /*
                        if (alg.adaptive_factor == 0)
                        {
                            actualProfit = actualProfit + (_algoProperty.actualhashrate * alg.profit);
                        } else
                        {
                            actualProfit = actualProfit + (_algoProperty.actualhashrate * alg.adaptive_profit);
                        }
                        */

                        //а для расчета только текущие
                        if (!currentminingAlgosZergPool.Contains(_algoProperty.name.ToLower()))
                        {
                            Helpers.ConsolePrint("Stats", "Delete current mining algo: " + _algoProperty.name);
                            algosProperty.TryRemove(_algoProperty.name, out var r);
                            //Form_Main.adaptiveRunning = false;
                        }

                        if (_algoProperty.localhashrate > 0)
                        {
                            _algoProperty.ticks++;
                            _algoProperty.actualProfit = (_algoProperty.actualhashrate * alg.profit);
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
                            if (_algoProperty.factorsList.Count < 100)
                            {
                                for (int i = _algoProperty.factorsList.Count; i < 100; i++)
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
                            double preaverage = _algoProperty.factorsList.Sum() / _algoProperty.factorsList.Count;

                            if (_algoProperty.ticks >= 15)
                            {
                                _algoProperty.factorsList.Add(factorNow);

                                if (ConfigManager.GeneralConfig.AdaptiveAlgo)//включен по умолчанию
                                {
                                    average = _algoProperty.factorsList.Sum() / _algoProperty.factorsList.Count;
                                    //Form_Main.adaptiveRunning = true;
                                    //if (_algoProperty.ticks > 
                                    //  ConfigManager.GeneralConfig.ticksBeforeAdaptiveStart +
                                    //    ConfigManager.GeneralConfig.ticksAdaptiveTuning)//105
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
                                            alg.adaptive_factor = average;
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
                    overallBTC = actualProfit;
                    Form_Main.TotalActualProfitability = overallBTC;
                    Form_Main.lastRigProfit.currentProfitAPI = overallBTC * Algorithm.Mult;
                    //Helpers.ConsolePrint("GetWalletBalance", "Actual profit: " + actualProfit.ToString());
                }
                else
                {

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
            setAlgos(await GetZergPoolAlgosListExAsync());
        }

        public static async Task LoadAlgoritmsListAsync(bool onlyCached = false)
        {
            List<MiningAlgorithms> algolist = new List<MiningAlgorithms>();
            try
            {
                if (System.IO.File.Exists("configs\\AlgoritmsList.json"))
                {
                    var jsonData = File.ReadAllText("configs\\AlgoritmsList.json");
                    if (string.IsNullOrEmpty(jsonData.Trim('\0')))
                    {
                        File.Delete("configs\\AlgoritmsList.json");
                    }
                    else
                    {
                        Helpers.ConsolePrint("LoadAlgoritmsList", "Loadind previous AlgoritmsList");
                        algolist = JsonConvert.DeserializeObject<List<MiningAlgorithms>>(jsonData);
                        MiningAlgorithmsList = algolist;
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrintError("LoadAlgoritmsList", ex.ToString());
            }
            setAlgos(algolist);
            if (onlyCached) return;
            setAlgos(await GetZergPoolAlgosListExAsync());
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
        private static bool checkAPIbug(MiningAlgorithms algo)
        {
            return false;
            //if (algo.apibug == true) return true;
            double a = algo.estimate_current;
            double b = algo.estimate_last24h;
            double c = algo.actual_last24h;
            if (a > b * 15000 || a > c * 15000 ||
                b > a * 15000 || b > c * 15000 ||
                c > a * 15000 || c > b * 15000)
            {
                Helpers.ConsolePrint("Stats", algo.name + " API bug");
                algo.apibug = true;
                return true;
            }
            return false;
        }
        private static List<string> delalgos = new();
        private static List<MiningAlgorithms> newalgolist = new();
        private class MostProfitableCoin
        {
            public string coin { get; set; }
            public double mostProfit { get; set; }
            public double currentProfit { get; set; }
        }
        private static void setAlgos(List<MiningAlgorithms> algolist)
        {
            try
            {
                delalgos.Clear();
                if (algolist != null && algolist.Count > 0)
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
                var zeroAlgos = new List<string>();

                foreach (var algo in algolist)
                {
                    string AlgorithmName = "";
                    string _AlgorithmName = "";
                    foreach (AlgorithmType _algo in Enum.GetValues(typeof(AlgorithmType)))
                    {
                        AlgorithmName = AlgorithmNames.GetName(_algo);
                        _AlgorithmName = AlgorithmNames.GetName(_algo).Replace("_", "-");
                        if (_algo >= 0 && algo.name.ToUpper().Equals(AlgorithmName.ToUpper()) ||
                            _algo >= 0 && algo.name.ToUpper().Equals(_AlgorithmName.ToUpper()))
                        {
                            double _profit = 0d;
                            int profitsCount = 0;
                            //отключение алгоритма
                            if (algo.estimate_current == 0 || algo.algoTempDeleted || algo.tempDisabled ||
                                algo.hashrate == 0)
                            {
                                _profit = 0;
                                if (algo.hashrate == 0)
                                {
                                    //algo.tempDeleted = true;
                                    zeroAlgos.Add(algo.name);
                                    //continue;
                                }
                            }
                            else
                            {
                                if (ConfigManager.GeneralConfig.CurrentEstimate)
                                {
                                    if (checkAPIbug(algo))
                                    {
                                        _profit = _profit + getMin(algo.estimate_current, algo.estimate_last24h, algo.actual_last24h);
                                    }
                                    else
                                    {
                                        _profit = _profit + algo.estimate_current;
                                    }
                                    profitsCount++;
                                }
                                if (ConfigManager.GeneralConfig._24hEstimate)
                                {
                                    if (checkAPIbug(algo))
                                    {
                                        _profit = _profit + getMin(algo.estimate_current, algo.estimate_last24h, algo.actual_last24h);
                                    }
                                    else
                                    {
                                        _profit = _profit + algo.estimate_last24h;
                                    }
                                    profitsCount++;
                                }
                                if (ConfigManager.GeneralConfig._24hActual)
                                {
                                    if (checkAPIbug(algo))
                                    {
                                        _profit = _profit + getMin(algo.estimate_current, algo.estimate_last24h, algo.actual_last24h);
                                    }
                                    else
                                    {
                                        _profit = _profit + (algo.actual_last24h);
                                    }
                                    profitsCount++;
                                }
                                if (profitsCount != 0)
                                {
                                    _profit = _profit / profitsCount;
                                }
                                if (ConfigManager.GeneralConfig.AdaptiveAlgo)//включен по умолчанию
                                {
                                    _profit = 0;
                                    /*
                                    if (checkAPIbug(algo))
                                    {
                                        _profit = _profit + getMin(algo.estimate_current, algo.estimate_last24h, algo.actual_last24h);
                                    }
                                    else
                                    */
                                    {
                                        _profit = _profit + algo.estimate_current;
                                    }
                                    /*
                                    if (checkAPIbug(algo))
                                    {
                                        _profit = _profit + getMin(algo.estimate_current, algo.estimate_last24h, algo.actual_last24h);
                                    }
                                    else
                                    */
                                    /*
                                     
                                    {
                                        _profit = _profit + algo.estimate_last24h;
                                    }
                                    */
                                    /*
                                    if (checkAPIbug(algo))
                                    {
                                        _profit = _profit + getMin(algo.estimate_current, algo.estimate_last24h, algo.actual_last24h);
                                    }
                                    else
                                    {
                                        _profit = _profit + (algo.actual_last24h);
                                    }
                                    */
                                   // _profit = _profit / 2;
                                }
                            }

                            algo.profit = _profit;
                            if (!data.ContainsKey(_algo))
                            {
                                if (algo.adaptive_factor != 0)
                                {
                                    if (!double.IsNaN(algo.adaptive_factor))
                                    {
                                        algo.adaptive_profit = algo.adaptive_factor * algo.profit;
                                    }
                                    else
                                    {
                                        //algo.adaptive_profit = 0;
                                        //algo.adaptive_factor = 0;
                                    }
                                    MostProfitableCoin _MostProfitableCoin = new();
                                    _MostProfitableCoin.coin = algo.coin;
                                    _MostProfitableCoin.mostProfit = algo.adaptive_profit;
                                    data.Add(_algo, _MostProfitableCoin);
                                } else
                                {
                                    MostProfitableCoin _MostProfitableCoin = new();
                                    _MostProfitableCoin.coin = algo.coin;
                                    _MostProfitableCoin.mostProfit = _profit;
                                    data.Add(_algo, _MostProfitableCoin);
                                }
                            }

                            if (delalgos.Exists(e => e.Equals(algo.name.ToLower()))) delalgos.Remove(algo.name.ToLower());
                            break;
                        }
                    }
                }
                /*
                if (zeroAlgos.Count > 0)
                {
                    string zA = string.Join(", ", zeroAlgos);
                    Helpers.ConsolePrint("Stats", "Zero pool hashrate: " + zA);
                }
                */
                delalgos.Remove("neoscrypt_xaya");
                delalgos.Remove("xelisv2_pepew");
                SetAlgorithmRates(data, 1, false);
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrintError("setAlgos", ex.ToString());
            }
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

                        var AlgorithmName = AlgorithmNames.GetName(algoKey);
                        paying = algo.Value.mostProfit;
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



