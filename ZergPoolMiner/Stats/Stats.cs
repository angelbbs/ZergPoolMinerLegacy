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

namespace ZergPoolMiner.Stats
{
    public static class Stats
    {
        public static double Balance { get; private set; }
        public static string Version = "";

        public static List<MiningAlgorithms> MiningAlgorithmsList = new();
        public class MiningAlgorithms
        {
            public string name { get; set; }
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
        }

        public static List<Coin> CoinList = new();
        public class Coin
        {
            public string name { get; set; }
            public string algo { get; set; }
            public int port { get; set; }
            public int tls_port { get; set; }
            public double hashrate { get; set; }
            public double estimate_current { get; set; }
            public double estimate_last24h { get; set; }
            public double actual_last24h { get; set; }
            public double mbtc_mh_factor { get; set; }
            public double pool_ttf { get; set; }
            public double real_ttf { get; set; }
            public double minpay { get; set; }
            public double minpay_sunday { get; set; }
            public bool apibug { get; set; }
            public bool CPU { get; set; }
            public bool GPU { get; set; }
            public bool FPGA { get; set; }
            public bool ASIC { get; set; }
        }

        public static string GetPoolApiData(string url)
        {
            var uri = new Uri(url);
            string host = new Uri(url).Host;
            var responseFromServer = "";
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, ssl) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var wr = (HttpWebRequest)WebRequest.Create(new Uri(url));
                string RequestId = System.Guid.NewGuid().ToString().Replace("-", "");

                //wr.UserAgent = "name=Edge;version=100.0.1185.39;buildNumber=1;os=Windows;osVersion=10;deviceVersion=amd64;lang=en";
                //wr.Headers.Add("X-Request-Id", RequestId);
                //wr.Headers.Add("X-User-Lang", "en");

                wr.Host = host;
                wr.Timeout = 5 * 1000;
                var response = wr.GetResponse();
                var ss = response.GetResponseStream();
                if (ss != null && ss is object)
                {
                    ss.ReadTimeout = 3 * 1000;
                    var reader = new StreamReader(ss);
                    responseFromServer = reader.ReadToEnd();
                    if (responseFromServer.Length == 0 || responseFromServer[0] != '{')
                    {
                        Helpers.ConsolePrint("GetPoolApiData", "Error! Not JSON from " +url);
                    }
                    reader.Close();
                }
                response.Close();
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("ERROR", ex.ToString());
                Form_Main.apiConnectionsErrors++;
                return null;
            }
            Form_Main.apiConnectionsErrors = 0;
            return responseFromServer;
        }

        public static List<Coin> GetCoins()
        {
            Helpers.ConsolePrint("Stats", "Trying GetCoins");
            try
            {
                string ResponseFromAPI = GetPoolApiData(Links.Currencies);
                if (ResponseFromAPI != null)
                {
                    var data = JObject.Parse(ResponseFromAPI);
                    CoinList.Clear();
                    foreach (var item in data)
                    {
                        var coin = item.Value;
                        string name = coin.Value<string>("name");
                        string algo = coin.Value<string>("algo");
                        var port = coin.Value<int>("port");
                        var tls_port = coin.Value<int>("tls_port");
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
                        var minpay = coin.Value<double>("minpay");
                        var minpay_sunday = coin.Value<double>("minpay_sunday");

                        Coin _coin = new();
                        _coin.name = name;
                        _coin.algo = algo;
                        _coin.pool_ttf = pool_ttf;
                        _coin.real_ttf = real_ttf;
                        _coin.minpay = minpay;
                        _coin.minpay_sunday = minpay_sunday;
                        _coin.port = port;
                        _coin.tls_port = tls_port;
                        _coin.hashrate = hashrate;
                        _coin.estimate_current = (estimate_current / mbtc_mh_factor * 1000000);//mBTC/GH
                        _coin.estimate_last24h = (estimate_last24h / mbtc_mh_factor * 1000000);
                        _coin.actual_last24h = (actual_last24h / mbtc_mh_factor / 1000 * 1000000);//zergpool api bug
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
                        
                        CoinList.Add(_coin);
                    }
                    /*
                    var json = JsonConvert.SerializeObject(CoinList, Formatting.Indented);
                    Helpers.WriteAllTextWithBackup("configs\\CoinList.json", json);
                    */
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("GetCoins", ex.ToString());
            }
            return CoinList;
        }

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
                            } else
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
                            a.actual_last24h = 0;
                            //a.estimate_current = 0;
                            //a.estimate_last24h = 0;
                            a.hashrate = 0;
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
                Helpers.ConsolePrint("GetZergPoolAlgosList", ex.ToString());
            }
            return MiningAlgorithmsList;
        }
        public static void ClearBalance()
        {
            Balance = 0;
        }

        public static void GetWalletBalance(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ConfigManager.GeneralConfig.Wallet))
            {
                Helpers.ConsolePrint("Stats", "Error getting wallet balance. Wallet not entered");
                return;
            }
            try
            {
                string ResponseFromAPI = GetPoolApiData(Links.WalletBalance + ConfigManager.GeneralConfig.Wallet);
                if (!string.IsNullOrEmpty(ResponseFromAPI))
                {
                    dynamic data = JsonConvert.DeserializeObject(ResponseFromAPI);
                    double unpaid = data.@unpaid;
                    Helpers.ConsolePrint("GetWalletBalance", unpaid.ToString() + " " + ConfigManager.GeneralConfig.PayoutCurrency);
                    Balance = unpaid;
                } else
                {
                    //Balance = 0;
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("GetWalletBalance", ex.ToString());
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
            public double localhashrate;
            public double actualhashrate;
            public double localProfit;
            public double actualProfit;
            public double profitRatio = 1d;
            public int ticks;
            public List<double> ratioList = new();
        }
        public static string GetFullWorkerName()
        {
            string w = WindowsMacUtils.GetMACAddress();
            return ConfigManager.GeneralConfig.WorkerName.Trim() + w;
        }
        public static ConcurrentDictionary<string, AlgoProperty> algosProperty = new();
        public static void GetWalletBalanceEx(object sender, EventArgs e)
        {
            Form_Main.adaptiveRunning = false;
            if (MiningSession._miningDevices is not object) return;
            if (MiningSession._miningDevices.Count == 0) return;

            if (string.IsNullOrEmpty(ConfigManager.GeneralConfig.Wallet))
            {
                Helpers.ConsolePrint("Stats", "Error getting wallet balance. Wallet not entered");
                return;
            }
            try
            {
                string ResponseFromAPI = GetPoolApiData(Links.WalletBalanceEx + ConfigManager.GeneralConfig.Wallet);
                if (!string.IsNullOrEmpty(ResponseFromAPI))
                {
                    dynamic _data = JsonConvert.DeserializeObject(ResponseFromAPI);
                    var a = JsonConvert.SerializeObject(_data, Formatting.Indented);
                    //Helpers.ConsolePrint("<-", a);

                    double overallBTC = 0;
                    dynamic data = JsonConvert.DeserializeObject(ResponseFromAPI);
                    double localProfit = 0d;
                    double localProfitSecond = 0d;
                    double actualProfit = 0d;
                    List<string> miningAlgos = new();
                    foreach (var alg in MiningAlgorithmsList)
                    {
                        double actualHashrate = 0d;
                        foreach (var cur in data.SelectToken("miners"))
                        {
                            string _algo = cur.SelectToken("algo");
                            string _ID = cur.SelectToken("ID");
                            int _subscribe = cur.SelectToken("subscribe");
                            double _accepted = cur.SelectToken("accepted");
                            if (alg.name.ToLower().Equals(_algo.ToLower()) &&
                            _ID.Equals(GetFullWorkerName()))
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
                                    if (!miningAlgos.Contains(((AlgorithmType)miningDevice.Device.AlgorithmID).ToString().ToLower()))
                                    {
                                        miningAlgos.Add(((AlgorithmType)miningDevice.Device.AlgorithmID).ToString().ToLower());
                                    }
                                    if (!miningAlgos.Contains(((AlgorithmType)miningDevice.Device.SecondAlgorithmID).ToString().ToLower()))
                                    {
                                        miningAlgos.Add(((AlgorithmType)miningDevice.Device.SecondAlgorithmID).ToString().ToLower());
                                    }
                                }

                                localProfit = (localHashrate * alg.profit);
                                actualHashrate = actualHashrate + _accepted;

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
                                    _algoProperty.ratioList = algosProperty.FirstOrDefault(x => x.Key.ToLower() == alg.name.ToLower()).Value.ratioList;
                                }
                                _algoProperty.localhashrate = localHashrate;
                                _algoProperty.localProfit = (localProfit);
                                _algoProperty.profitRatio = alg.profit;
                                _algoProperty.actualhashrate = actualHashrate;
                                _algoProperty.name = alg.name.ToLower();
                                algosProperty.AddOrUpdate(alg.name.ToLower(), _algoProperty, (k, v) => _algoProperty);
                            }
                        }
                    }

                    foreach (var __algoProperty in algosProperty)
                    {
                        double average = 1;
                        var _algoProperty = __algoProperty.Value;
                        var alg = MiningAlgorithmsList.FirstOrDefault(x => x.name.ToLower() == __algoProperty.Key.ToLower());

                        //пусть на графике присутствуют предыдущие значения
                        actualProfit = actualProfit + (_algoProperty.actualhashrate * alg.profit);
                        if (_algoProperty.localhashrate != 0)
                        {
                            _algoProperty.ticks++;
                            //а для расчета только текущие
                            _algoProperty.actualProfit = (_algoProperty.actualhashrate * alg.profit);
                            if (_algoProperty.ticks >= ConfigManager.GeneralConfig.ticksBeforeAdaptiveStart &&//15 
                                _algoProperty.ticks <= ConfigManager.GeneralConfig.ticksBeforeAdaptiveStart +
                                ConfigManager.GeneralConfig.ticksAdaptiveTuning &&
                                alg.adaptive_factor == 0)
                            {
                                Helpers.ConsolePrint("Stats", alg.name + " adaptive tuning in process");
                                Form_Main.adaptiveRunning = true;
                            }

                            if (_algoProperty.ticks >= ConfigManager.GeneralConfig.ticksBeforeAdaptiveStart)//15
                            {
                                double profitNow = _algoProperty.actualProfit / _algoProperty.localProfit;
                                _algoProperty.ratioList.Add(profitNow);

                                if (ConfigManager.GeneralConfig.AdaptiveAlgo)//включен по умолчанию
                                {
                                    average = _algoProperty.ratioList.Sum() / _algoProperty.ratioList.Count;
                                    Helpers.ConsolePrint("Stats", alg.name + " adaptive factor: " + average.ToString("F5"));

                                    if (_algoProperty.ticks > 
                                        ConfigManager.GeneralConfig.ticksBeforeAdaptiveStart +
                                            ConfigManager.GeneralConfig.ticksAdaptiveTuning)
                                    {
                                        if (!double.IsInfinity(average) && !double.IsNaN(average))
                                        {
                                            if (alg.adaptive_factor == 0 && average < 0.85)
                                            {
                                                average = 0.85;
                                            }
                                            if (alg.adaptive_factor == 0 && average > 1)
                                            {
                                                average = 1;
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
                        if (!miningAlgos.Contains(_algoProperty.name.ToLower()))
                        {
                            algosProperty.TryRemove(_algoProperty.name, out var r);
                        }
                    }
                    overallBTC = actualProfit;
                    Form_Main.TotalActualProfitability = overallBTC;
                    Form_Main.lastRigProfit.currentProfitAPI = overallBTC * Algorithm.Mult;
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("GetWalletBalance", ex.ToString());
            }
            return;
        }

        public static bool emptypool = true;
        [HandleProcessCorruptedStateExceptions]
        public static void GetAlgos()
        {
            setAlgos(GetZergPoolAlgosList());
        }

        public static void LoadAlgoritmsList(bool onlyCached = false)
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
                Helpers.ConsolePrint("LoadAlgoritmsList", ex.ToString());
            }
            setAlgos(algolist);
            if (onlyCached) return;
            setAlgos(GetZergPoolAlgosList());
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
            if (algo.apibug == true) return true;
            double a = algo.estimate_current;
            double b = algo.estimate_last24h;
            double c = algo.actual_last24h;
            if (a > b * 150 || a > c * 150 ||
                b > a * 150 || b > c * 150 ||
                c > a * 150 || c > b * 150)
            {
                Helpers.ConsolePrint("Stats", algo.name + " API bug");
                algo.apibug = true;
                return true;
            }
            return false;
        }
        private static List<string> delalgos = new();
        private static List<MiningAlgorithms> newalgolist = new();
        private static void setAlgos(List<MiningAlgorithms> algolist)
        {
            try
            {
                delalgos.Clear();
                newalgolist.Clear();
                if (algolist != null && algolist.Count > 0)
                {
                    newalgolist = algolist.ToList();
                    foreach (AlgorithmType alg in Enum.GetValues(typeof(AlgorithmType)))
                    {
                        if ((int)alg >= 1 && !alg.ToString().Contains("UNUSED"))
                        {
                            delalgos.Add(alg.ToString().ToLower());
                        }
                    }
                }
                Dictionary<AlgorithmType, double> data = new();
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
                            //надо добавить проверку на TTF
                            if (algo.estimate_current == 0 || algo.estimate_last24h == 0 ||
                                algo.actual_last24h == 0 || algo.hashrate == 0)
                            {
                                _profit = 0;
                                if (algo.hashrate == 0)
                                {
                                    zeroAlgos.Add(algo.name);
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
                                    algo.adaptive_profit = algo.adaptive_factor * algo.profit;
                                    data.Add(_algo, algo.adaptive_profit);
                                } else
                                {
                                    data.Add(_algo, _profit);
                                }
                            }

                            if (newalgolist.Any(n => n.name.ToLower().Equals(algo.name.ToLower())))
                            {
                                var itemToRemove = newalgolist.Single(r => r.name == algo.name);
                                newalgolist.Remove(itemToRemove);
                            }
                            if (delalgos.Exists(e => e.Equals(algo.name.ToLower()))) delalgos.Remove(algo.name.ToLower());
                            break;
                        }
                    }
                }
                if (zeroAlgos.Count > 0)
                {
                    string zA = string.Join(", ", zeroAlgos);
                    Helpers.ConsolePrint("Stats", "Zero pool hashrate: " + zA);
                }
                delalgos.Remove("neoscrypt_xaya");
                delalgos.Remove("xelisv2_pepew");

                SetAlgorithmRates(data, 1, false);
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("setAlgos", ex.ToString());
            }
        }
       
        public static void CheckNewAlgo()
        {
            foreach (var algo in delalgos)
            {
                Helpers.ConsolePrint("Stats", "Zergpool deleted the algorithm: " + algo);
                foreach (AlgorithmType alg in Enum.GetValues(typeof(AlgorithmType)))
                {
                    if (alg.ToString().ToLower().Equals(algo.ToLower()))
                    {
                        AlgosProfitData.UpdatePayingForAlgo(alg, 0, false);
                    }
                }
            }
            foreach (var algo in newalgolist)
            {
                if (algo.CPU && !algo.FPGA && !algo.GPU)
                {
                    Helpers.ConsolePrint("Stats", "Zergpool added the CPU only algorithm: " + algo.name);
                }
            }
            foreach (var algo in newalgolist)
            {
                if (algo.GPU && !algo.CPU && !algo.FPGA && !algo.ASIC)
                {
                    Helpers.ConsolePrint("Stats", "New GPU only algo: " + algo.name);
                }
            }
            foreach (var algo in newalgolist)
            {
                if (algo.CPU && !algo.ASIC && !algo.FPGA)
                {
                    Helpers.ConsolePrint("Stats", "New CPU algo: " + algo.name);
                }
            }
            foreach (var algo in newalgolist)
            {
                if (algo.GPU && !algo.ASIC)
                {
                    Helpers.ConsolePrint("Stats", "New GPU algo: " + algo.name);
                }
            }
        }
        public static void SetAlgorithmRates(Dictionary<AlgorithmType, double> data, int multipl = 1, 
            bool average = false)
        {
            double paying = 0.0d;
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
                        paying = algo.Value;
                        AlgosProfitData.UpdatePayingForAlgo(algoKey, paying, false);
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



