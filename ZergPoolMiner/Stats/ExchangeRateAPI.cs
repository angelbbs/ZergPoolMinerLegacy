using Newtonsoft.Json;
using ZergPoolMiner.Configs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;
using ZergPoolMiner.Forms;
using System.Xml;
using Timer = System.Windows.Forms.Timer;
using SystemTimer = System.Timers.Timer;
using System.Windows.Forms;

namespace ZergPoolMiner.Stats
{
    internal static class ExchangeRateApi
    {
        private static readonly ConcurrentDictionary<string, double> ExchangesFiat = new ConcurrentDictionary<string, double>();
        public static readonly ConcurrentDictionary<string, double> MarketRatesList = new ConcurrentDictionary<string, double>();
        private static double _usdBtcRate = -1;
        public static double BTCcostUSD = 0d;
        public static List<string> CoinsList = new List<string>();

        public static List<string> excludeExchange = new List<string>();
        public class ExchangeRateJson
        {
            public List<Dictionary<string, string>> exchanges { get; set; }
            public Dictionary<string, double> exchanges_fiat { get; set; }
        }

       
        public static string ActiveDisplayCurrency = "USD";

        public static void InitCoinsList()
        {
            CoinsList.Clear();
            Wallets.Wallets.InitCoinsList();
            foreach (var coin in Wallets.Wallets.CoinsList)
            {
                if (coin.Coin.Length <= 4 && !CoinsList.Contains(coin.Coin))
                {
                    CoinsList.Add(coin.Coin);
                }

                if (coin.Coin.Length > 4 && !CoinsList.Contains(coin.Coin.Substring(0, 4)))
                {
                    CoinsList.Add(coin.Coin.Substring(0, 4));
                }
            }
        }

        public static List<ConcurrentDictionary<string, double>> GetApi()
        {
            List<ConcurrentDictionary<string, double>> exchanges_rates = new();
            exchanges_rates.Add(GetRatesgateio());
            exchanges_rates.Add(GetRatesTradeogre());
            exchanges_rates.Add(GetRatesNonkyc());
            exchanges_rates.Add(GetRatesCoinex());
            exchanges_rates.Add(GetRatesMexc());
            //exchanges_rates.Add(GetRatesSafetrade());
            return exchanges_rates;
        }

        public static void CourseAlignment(List<ConcurrentDictionary<string, double>> courses)
        {
            foreach (var exchange in courses)
            {
                if (exchange == null) return;
                if (exchange.Count == 0) continue;
                foreach (var key in exchange)
                {
                    if (!MarketRatesList.ContainsKey(key.Key))
                    {
                        MarketRatesList.AddOrUpdate(key.Key, exchange[key.Key], (k, v) => exchange[k]);
                    } else
                    {
                        var item = MarketRatesList.FirstOrDefault(kvp => kvp.Key == key.Key);
                        double r = (key.Value + item.Value) / 2;
                        MarketRatesList.AddOrUpdate(key.Key, r, (k, v) => exchange[k]);
                    }
                }
            }

            try
            {
                foreach (var c in MarketRatesList)
                {
                    if (c.Key.Equals("BTC")) BTCcostUSD = c.Value;
                }
                var json = JsonConvert.SerializeObject(MarketRatesList, Newtonsoft.Json.Formatting.Indented);
                Helpers.WriteAllTextWithBackup("configs\\ExchangeRates.json", json);
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("ExchangeRateApi", ex.ToString());
            }

            Interlocked.Exchange(ref _usdBtcRate, BTCcostUSD);
            Helpers.ConsolePrint("ExchangeRateApi", $"BTC rate updated: {BTCcostUSD} USD");
        }

        public static void GetBTCRate(List<ConcurrentDictionary<string, double>> courses = null)
        {
            InitCoinsList();
            foreach (var c in Wallets.Wallets.CoinsList)
            {
                if (!MarketRatesList.ContainsKey(c.Coin))
                {
                    MarketRatesList.TryAdd(c.Coin, 1d);
                }
            }
            try
            {
                if (File.Exists("configs\\ExchangeRates.json"))
                {
                    var text = File.ReadAllText("configs\\ExchangeRates.json");
                    if (string.IsNullOrEmpty(text.Trim('\0')))
                    {
                        File.Delete("configs\\ExchangeRates.json");
                    }
                    else
                    {
                        var values = JsonConvert.DeserializeObject<Dictionary<string, double>>(text);
                        foreach (var c in values)
                        {
                            MarketRatesList.AddOrUpdate(c.Key, c.Value, (k, v) => values[k]);
                            if (c.Key.Equals("BTC")) BTCcostUSD = c.Value;
                        }

                        if (courses == null)
                        {
                            courses = new List<ConcurrentDictionary<string, double>>();
                            CourseAlignment(courses);
                            return;
                        }
                    }
                }
                if (courses == null)
                {
                    CourseAlignment(GetApi());
                }
                else
                {
                    CourseAlignment(courses);
                }
            } catch (Exception ex)
            {
                Helpers.ConsolePrint("ExchangeRateApi", ex.ToString());
            }
        }
        public static ConcurrentDictionary<string, double> GetRatesTradeogre()
        {
            Helpers.ConsolePrint("ExchangeRateApi", "Trying get cryptocurrency exchange rates from Tradeogre");
            ConcurrentDictionary<string, double> _MarketRatesList = new ConcurrentDictionary<string, double>();
            string ResponseFromAPI = GetAPI(Links.TradeogreMarket);

            try
            {
                dynamic resp = JsonConvert.DeserializeObject(ResponseFromAPI);
                if (resp != null)
                {
                    foreach (JObject o in resp)
                    {
                        foreach (var item in o)
                        {
                            var pair = item.Key.ToUpper();
                            if (!pair.Contains("-USDT") && !pair.Contains("USDT-")) continue;
                            pair = pair.Replace("-", "/");
                            var _item = item.Value;
                            JToken value = _item.Value<JToken>("price") ?? null;
                            string basename = (string)(_item.Value<JToken>("basename") ?? "");

                            if (excludeExchange.Contains(basename)) continue;

                            double.TryParse(value.ToString(), out double rate);


                            if (pair.Equals("BTC/USDT"))
                            {
                                BTCcostUSD = rate;
                                Helpers.ConsolePrint("ExchangeRateApi", "BTC rate " + BTCcostUSD.ToString() + " USD");
                            }
                            pair = pair.Replace("/USDT", "");
                            _MarketRatesList.TryAdd(pair, rate);

                            foreach (var coin in CoinsList)
                            {
                                if (coin.Equals(pair))
                                {
                                    _MarketRatesList.TryAdd(coin, rate);
                                }
                            }

                        }

                    }
                    return _MarketRatesList;
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("ExchangeRateApi", ex.ToString());
            }
            return _MarketRatesList;
        }
        
        public static ConcurrentDictionary<string, double> GetRatesMexc()
        {
            ConcurrentDictionary<string, double> _MarketRatesList = new ConcurrentDictionary<string, double>();
            Helpers.ConsolePrint("ExchangeRateApi", "Trying get cryptocurrency exchange rates from Mexc");
            string ResponseFromAPI = GetAPI(Links.mexcMarket);
            
            try
            {
                dynamic resp = JsonConvert.DeserializeObject(ResponseFromAPI);
                if (resp != null)
                {
                    foreach (var pair in resp)
                    {
                        string symbol = pair.symbol;

                        if (!symbol.Contains("USDT")) continue;
                        symbol = symbol.Replace("USDT", "");

                        string _price = pair.price;
                        double.TryParse(_price, out double price);

                        _MarketRatesList.TryAdd(symbol, price);
                    }
                    return _MarketRatesList;
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("ExchangeRateApi", ex.ToString());
            }
            return _MarketRatesList;
        }
        public static ConcurrentDictionary<string, double> GetRatesSafetrade()
        {
            ConcurrentDictionary<string, double> _MarketRatesList = new ConcurrentDictionary<string, double>();

            Helpers.ConsolePrint("ExchangeRateApi", "Trying get cryptocurrency exchange rates from SafeTrade");
            string ResponseFromAPI = GetAPI(Links.safetradeMarket);
            
            try
            {
                dynamic resp = JsonConvert.DeserializeObject(ResponseFromAPI);
                if (resp != null)
                {
                    foreach (var pair in resp)
                    {
                        string symbol = pair.symbol;

                        if (!symbol.Contains("USDT")) continue;
                        symbol = symbol.Replace("USDT", "");

                        string _price = pair.price;
                        double.TryParse(_price, out double price);

                        _MarketRatesList.TryAdd(symbol, price);
                    }
                    return _MarketRatesList;
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("ExchangeRateApi", ex.ToString());
            }
            return _MarketRatesList;
        }
        public static ConcurrentDictionary<string, double> GetRatesCoinex()
        {
            ConcurrentDictionary<string, double> _MarketRatesList = new ConcurrentDictionary<string, double>();
            string ResponseFromAPI;
            Helpers.ConsolePrint("ExchangeRateApi", "Trying get cryptocurrency exchange rates from Coinex");
            try
            {
                HttpWebRequest WR = (HttpWebRequest)WebRequest.Create(Links.coinexMarket);
                WR.UserAgent = "GET / HTTP/1.1\r\n\r\n";
                WR.Timeout = 5 * 1000;
                WR.Credentials = CredentialCache.DefaultCredentials;
                WebResponse Response = WR.GetResponse();
                Stream SS = Response.GetResponseStream();
                SS.ReadTimeout = 5 * 1000;
                StreamReader Reader = new StreamReader(SS);
                reader_timer = new SystemTimer();
                reader_timer.Elapsed += StopReader;
                reader_timer.Interval = 5000;
                reader_timer.Start();
                ResponseFromAPI = Reader.ReadToEnd();
                Reader.Close();
                Response.Close();
                reader_timer.Stop();
                reader_timer.Dispose();
                Helpers.ConsolePrint("GetRates", "Received: " + ResponseFromAPI.Length.ToString() + " bytes");
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("ExchangeRateApi", ex.ToString());
                return _MarketRatesList;
            }

            try
            {
                dynamic resp = JsonConvert.DeserializeObject(ResponseFromAPI);
                if (resp != null)
                {
                    foreach (var pair in resp.data)
                    {
                        string symbol = pair.market;

                        if (symbol.Contains("_INDEX")) continue;
                        if (!symbol.Contains("USDT")) continue;
                        symbol = symbol.Replace("USDT", "");

                        string _last = pair.last;
                        double.TryParse(_last, out double last);

                        _MarketRatesList.TryAdd(symbol, last);
                    }
                    return _MarketRatesList;
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("ExchangeRateApi", ex.ToString());
            }
            return _MarketRatesList;
        }
        public static ConcurrentDictionary<string, double> GetRatesNonkyc()
        {
            ConcurrentDictionary<string, double> _MarketRatesList = new ConcurrentDictionary<string, double>();
            string ResponseFromAPI;
            Helpers.ConsolePrint("ExchangeRateApi", "Trying get cryptocurrency exchange rates from Nonkyc");
            try
            {
                HttpWebRequest WR = (HttpWebRequest)WebRequest.Create(Links.NonkycMarket);
                WR.UserAgent = "GET / HTTP/1.1\r\n\r\n";
                WR.Timeout = 5 * 1000;
                WR.Credentials = CredentialCache.DefaultCredentials;
                WebResponse Response = WR.GetResponse();
                Stream SS = Response.GetResponseStream();
                SS.ReadTimeout = 5 * 1000;
                StreamReader Reader = new StreamReader(SS);
                reader_timer = new SystemTimer();
                reader_timer.Elapsed += StopReader;
                reader_timer.Interval = 5000;
                reader_timer.Start();
                ResponseFromAPI = Reader.ReadToEnd();
                Reader.Close();
                Response.Close();
                reader_timer.Stop();
                reader_timer.Dispose();
                Helpers.ConsolePrint("GetRates", "Received: " + ResponseFromAPI.Length.ToString() + " bytes");
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("ExchangeRateApi", ex.ToString());
                return _MarketRatesList;
            }

            try
            {
                dynamic resp = JsonConvert.DeserializeObject(ResponseFromAPI);
                if (resp != null)
                {
                    foreach (var pair in resp)
                    {
                        string symbol = pair.symbol;
                        string primaryName = pair.primaryName;

                        if (excludeExchange.Contains(primaryName)) continue;

                        if (!symbol.Contains("/USDT") && !symbol.Contains("USDT/")) continue;
                        string _lastPrice = pair.lastPrice;
                        double.TryParse(_lastPrice, out double lastPrice);

                        if (symbol.Equals("BTC/USDT"))
                        {
                            BTCcostUSD = lastPrice;
                            Helpers.ConsolePrint("ExchangeRateApi", "BTC rate " + BTCcostUSD.ToString() + " USD");
                        }
                        symbol = symbol.Replace("/USDT", "");
                        _MarketRatesList.TryAdd(symbol, lastPrice);

                        foreach (var coin in CoinsList)
                        {
                            if (coin.Equals(symbol))
                            {
                                _MarketRatesList.TryAdd(coin, lastPrice);
                            }
                        }

                    }
                    return _MarketRatesList;
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("ExchangeRateApi", ex.ToString());
            }
            return _MarketRatesList;
        }

        public static long ConvertToUnixTimestamp(DateTime dateTime)
        {
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan timeSpan = dateTime - unixEpoch;
            return (long)timeSpan.TotalSeconds;
        }

        private static StreamReader Reader;
        private static SystemTimer reader_timer;
        private static void StopReader(object sender, EventArgs e)
        {
            //Helpers.ConsolePrint("StopReader", "Timeout");
            Application.DoEvents();
            try
            {
                if (Reader is object && Reader != null)
                {
                    Reader.Close();
                    reader_timer.Stop();
                    reader_timer.Dispose();
                }
            } catch (Exception ex)
            {
                Helpers.ConsolePrint("StopReader", ex.ToString());
            }
        }

        private static string GetAPI(string url)
        {
            string ResponseFromAPI = "";

            try
            {
                DateTime currentDateTime = DateTime.Now;
                long unixTimestamp = ConvertToUnixTimestamp(currentDateTime);

                HttpWebRequest WR = (HttpWebRequest)WebRequest.Create(url);

                if (ConfigManager.GeneralConfig.EnableProxy)
                {
                    if (string.IsNullOrEmpty(Stats.CurrentProxyIP))
                    {
                        Stats.CurrentProxyIP = ProxyCheck.HttpsProxyList[0].Ip;
                        Stats.CurrentProxyHTTPSPort = ProxyCheck.HttpsProxyList[0].HTTPSPort;
                        Stats.CurrentProxySocks5SPort = ProxyCheck.HttpsProxyList[0].Socks5Port;
                    }
                    WebProxy proxy = new WebProxy(Stats.CurrentProxyIP, Stats.CurrentProxyHTTPSPort);
                    WR.Proxy = proxy;
                }
                //WR.UserAgent = "GET / HTTP/1.1\r\n\r\n";
                WR.Accept = "application/json";
                WR.ContentType = "application/json";
                WR.Headers.Add("Timestamp", unixTimestamp.ToString());
                WR.Timeout = 10 * 1000;
                WR.Credentials = CredentialCache.DefaultCredentials;

                bool success = false;
                new Thread(() =>
                {
                    Thread.Sleep(1000 * 12);
                    if (WR is object && WR is not null && !success)
                    {
                        WR.Abort();
                    }
                }).Start();

                WebResponse Response = WR.GetResponse();
                Stream SS = Response.GetResponseStream();
                SS.ReadTimeout = 10 * 1000;
                Reader = new StreamReader(SS);
                Reader.BaseStream.ReadTimeout = 10000;

                reader_timer = new SystemTimer();
                reader_timer.Elapsed += StopReader;
                reader_timer.Interval = 10000;
                reader_timer.Start();

                success = true;
                ResponseFromAPI = Reader.ReadToEnd();
                Reader.Close();
                Response.Close();
                reader_timer.Stop();
                reader_timer.Dispose();
                if (SS is object && SS != null)
                {
                    SS.Dispose();
                }
                if (WR is object && WR != null)
                {
                    WR.Abort();
                }

                    Helpers.ConsolePrint("GetRates", "Received: " + ResponseFromAPI.Length.ToString() + " bytes");

            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("ExchangeRateApi", ex.ToString());
                Helpers.ConsolePrint("GetRates", "Received: " + ResponseFromAPI.Length.ToString() + " bytes");
                return "";
            }
            return ResponseFromAPI;
        }

        public static ConcurrentDictionary<string, double> GetRatesgateio()
        {
            ConcurrentDictionary<string, double> _MarketRatesList = new ConcurrentDictionary<string, double>();
            string ResponseFromAPI = "";
            
            if (ConfigManager.GeneralConfig.EnableProxy)
            {
                Helpers.ConsolePrint("ExchangeRateApi", "Trying get cryptocurrency exchange rates from Gate.io " +
                    "via proxy " + Stats.CurrentProxyIP);
            } else
            {
                Helpers.ConsolePrint("ExchangeRateApi", "Trying get cryptocurrency exchange rates from Gate.io");
            }

            ResponseFromAPI = GetAPI(Links.gateioMarket);

            try
            {
                dynamic resp = JsonConvert.DeserializeObject(ResponseFromAPI);
                if (resp != null)
                {
                    foreach (var pair in resp.data)
                    {
                        string symbol = pair.pair;
                        string name_en = pair.name_en;

                        if (excludeExchange.Contains(name_en)) continue;

                        symbol = symbol.ToUpper().Replace("_", "/");
                        if (!symbol.Contains("/USDT") && !symbol.Contains("USDT/")) continue;
                        string _lastPrice = pair.rate;
                        double.TryParse(_lastPrice, out double lastPrice);

                        if (symbol.Equals("BTC/USDT"))
                        {
                            BTCcostUSD = lastPrice;
                            Helpers.ConsolePrint("ExchangeRateApi", "BTC rate " + BTCcostUSD.ToString() + " USD");
                        }
                        symbol = symbol.Replace("/USDT", "");
                        _MarketRatesList.TryAdd(symbol, lastPrice);

                        foreach (var coin in CoinsList)
                        {
                            if (coin.Equals(symbol))
                            {
                                _MarketRatesList.TryAdd(coin, lastPrice);
                            }
                        }

                    }
                    return _MarketRatesList;
                }

            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("ExchangeRateApi", ex.ToString());
            }
            return _MarketRatesList;
        }
        private static bool ConverterActive => ConfigManager.GeneralConfig.DisplayCurrency != "USD";

        public static List<ConcurrentDictionary<string, double>> GetFiatApi()
        {
            List<ConcurrentDictionary<string, double>> exchanges_fiat = new();
            exchanges_fiat.Add(GetExchangeRates1());
            exchanges_fiat.Add(GetExchangeRates2());
            exchanges_fiat.Add(GetExchangeRates3());

            return exchanges_fiat;
        }

        public static void FiatCourseAlignment(List<ConcurrentDictionary<string, double>> courses)
        {
            foreach (var exchange in courses)
            {
                if (exchange.Count == 0) continue;
                foreach (var key in exchange)
                {
                    if (!ExchangesFiat.ContainsKey(key.Key))
                    {
                        ExchangesFiat.AddOrUpdate(key.Key, exchange[key.Key], (k, v) => exchange[k]);
                    }
                    else
                    {
                        var item = ExchangesFiat.FirstOrDefault(kvp => kvp.Key == key.Key);
                        double r = (key.Value + item.Value) / 2;
                        ExchangesFiat.AddOrUpdate(key.Key, r, (k, v) => exchange[k]);
                    }
                }
            }
            try
            {
                var json = JsonConvert.SerializeObject(ExchangesFiat, Newtonsoft.Json.Formatting.Indented);
                Helpers.WriteAllTextWithBackup("configs\\ExchangesFiat.json", json);
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("ExchangeRateApi", ex.ToString());
            }
        }

        public static void GetFiatRate(List<ConcurrentDictionary<string, double>> courses = null)
        {
            try
            {
                if (File.Exists("configs\\ExchangesFiat.json"))
                {
                    var text = File.ReadAllText("configs\\ExchangesFiat.json");
                    if (string.IsNullOrEmpty(text.Trim('\0')))
                    {
                        File.Delete("configs\\ExchangesFiat.json");
                    }
                    else
                    {
                        var values = JsonConvert.DeserializeObject<Dictionary<string, double>>(text);
                        foreach (var c in values)
                        {
                            ExchangesFiat.TryAdd(c.Key, c.Value);
                        }
                    }
                }

                if (courses == null)
                {
                    FiatCourseAlignment(GetFiatApi());
                }
                else
                {
                    FiatCourseAlignment(courses);
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("ExchangeRateApi", ex.ToString());
            }
        }

        private static string[] GetDisplayCurrencies()
        {
            string[] _cur = null;
            try
            {
                if (File.Exists("configs\\CurrenciesList.json"))
                {
                    string cur = File.ReadAllText("configs\\CurrenciesList.json");
                    if (string.IsNullOrEmpty(cur.Trim('\0')))
                    {
                        File.Delete("configs\\CurrenciesList.json");
                    }
                    else
                    {
                        JArray json = JArray.Parse(cur);
                        _cur = json.OfType<object>().Select(o => o.ToString()).ToArray();
                    }
                }
                else
                {
                    _cur = Form_Settings.currencys;
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("GetExchangeRates", ex.ToString());
            }
            return _cur;
        }
        public static ConcurrentDictionary<string, double> GetExchangeRates1()
        {
            Helpers.ConsolePrint("ExchangeRateApi", "Trying get national currency exchange rates from " +
                Links.floatratesURL);
            ConcurrentDictionary<string, double> exchanges_fiat = new ConcurrentDictionary<string, double>();
            string ResponseFromAPI;
            var _cur = GetDisplayCurrencies();
            try
            {
                HttpWebRequest WR = (HttpWebRequest)WebRequest.Create(Links.floatratesURL);
                WR.UserAgent = "GET / HTTP/1.1\r\n\r\n";
                WR.Timeout = 5 * 1000;
                WR.Credentials = CredentialCache.DefaultCredentials;
                WebResponse Response = WR.GetResponse();
                Stream SS = Response.GetResponseStream();
                SS.ReadTimeout = 5 * 1000;
                StreamReader Reader = new StreamReader(SS);
                reader_timer = new SystemTimer();
                reader_timer.Elapsed += StopReader;
                reader_timer.Interval = 5000;
                reader_timer.Start();
                ResponseFromAPI = Reader.ReadToEnd();
                Reader.Close();
                Response.Close();
                reader_timer.Stop();
                reader_timer.Dispose();
                Helpers.ConsolePrint("GetRates", "Received: " + ResponseFromAPI.Length.ToString() + " bytes");
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("ExchangeRateApi", ex.ToString());
                return exchanges_fiat;
            }

            try
            {
                var data = JObject.Parse(ResponseFromAPI);
                foreach (var item in data)
                {
                    if (_cur.Contains(item.Key.ToUpper()))
                    {
                        var coin = item.Value;
                        JToken value = coin.Value<JToken>("rate") ?? null;
                        double.TryParse(value.ToString(), out double rate);
                        if (Form_Settings.currencys.Contains(item.Key.ToUpper()))
                        {
                            exchanges_fiat.TryAdd(item.Key.ToUpper(), rate);
                        }
                    }
                }
                return exchanges_fiat;
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("ExchangeRateApi", ex.ToString());
            }
            return exchanges_fiat;
        }
        public static ConcurrentDictionary<string, double> GetExchangeRates2()
        {
            Helpers.ConsolePrint("ExchangeRateApi", "Trying get national currency exchange rates from " +
    Links.fawazahmedURL);
            ConcurrentDictionary<string, double> exchanges_fiat = new ConcurrentDictionary<string, double>();
            string ResponseFromAPI;
            var _cur = GetDisplayCurrencies();
            try
            {
                HttpWebRequest WR = (HttpWebRequest)WebRequest.Create(Links.fawazahmedURL);
                WR.UserAgent = "GET / HTTP/1.1\r\n\r\n";
                WR.Timeout = 5 * 1000;
                WR.Credentials = CredentialCache.DefaultCredentials;
                WebResponse Response = WR.GetResponse();
                Stream SS = Response.GetResponseStream();
                SS.ReadTimeout = 5 * 1000;
                StreamReader Reader = new StreamReader(SS);
                reader_timer = new SystemTimer();
                reader_timer.Elapsed += StopReader;
                reader_timer.Interval = 5000;
                reader_timer.Start();
                ResponseFromAPI = Reader.ReadToEnd();
                Reader.Close();
                Response.Close();
                reader_timer.Stop();
                reader_timer.Dispose();
                Helpers.ConsolePrint("GetRates", "Received: " + ResponseFromAPI.Length.ToString() + " bytes");
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("ExchangeRateApi", ex.ToString());
                return exchanges_fiat;
            }

            try
            {
                dynamic resp = JsonConvert.DeserializeObject(ResponseFromAPI);
                if (resp != null)
                {
                    JObject pair = resp.usd;
                    foreach (var item in pair)
                    {
                        if (_cur.Contains(item.Key.ToUpper()))
                        {
                            var coin = item.Key.ToUpper();
                            double rate = (double)item.Value;
                            if (Form_Settings.currencys.Contains(item.Key.ToUpper()))
                            {
                                exchanges_fiat.TryAdd(coin, rate);
                            }
                        }
                    }
                    return exchanges_fiat;
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("ExchangeRateApi", ex.ToString());
            }
            return exchanges_fiat;
        }

        public class Root
        {
            public ValCurs ValCurs { get; set; }
        }
        public class ValCurs
        {
            public List<Valute> Valute { get; set; }
        }
        public class Valute
        {
            public string ID { get; set; }
            public string NumCode { get; set; }
            public string CharCode { get; set; }
            public string Nominal { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public string VunitRate { get; set; }
        }

        public static ConcurrentDictionary<string, double> GetExchangeRates3()
        {
            Helpers.ConsolePrint("ExchangeRateApi", "Trying get national currency exchange rates from " +
    Links.cbrURL);
            ConcurrentDictionary<string, double> exchanges_fiat = new ConcurrentDictionary<string, double>();
            string ResponseFromAPI;
            var _cur = GetDisplayCurrencies();
            try
            {
                HttpWebRequest WR = (HttpWebRequest)WebRequest.Create(Links.cbrURL);
                WR.UserAgent = "GET / HTTP/1.1\r\n\r\n";
                WR.Timeout = 5 * 1000;
                WR.Credentials = CredentialCache.DefaultCredentials;
                WebResponse Response = WR.GetResponse();
                Stream SS = Response.GetResponseStream();
                SS.ReadTimeout = 5 * 1000;
                StreamReader Reader = new StreamReader(SS);
                reader_timer = new SystemTimer();
                reader_timer.Elapsed += StopReader;
                reader_timer.Interval = 5000;
                reader_timer.Start();
                ResponseFromAPI = Reader.ReadToEnd();
                Reader.Close();
                Response.Close();
                reader_timer.Stop();
                reader_timer.Dispose();
                Helpers.ConsolePrint("GetRates", "Received: " + ResponseFromAPI.Length.ToString() + " bytes");
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("ExchangeRateApi", ex.ToString());
                return exchanges_fiat;
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(ResponseFromAPI);
                string jsonText = JsonConvert.SerializeXmlNode(doc);

                Root cbr = JsonConvert.DeserializeObject<Root>(jsonText);

                double usd = 0d;
                double cur = 0d;
                int nominal = 0;
                if (cbr != null)
                {
                    foreach (var item in cbr.ValCurs.Valute)
                    {
                        if (item.CharCode.ToLower().Equals("usd"))
                        {
                            double.TryParse(item.Value, out usd);
                            break;
                        }
                    }
                    foreach (var item in cbr.ValCurs.Valute)
                    {
                        double.TryParse(item.Value, out cur);
                        int.TryParse(item.Nominal, out nominal);
                        if (Form_Settings.currencys.Contains(item.CharCode.ToUpper()))
                        {
                            exchanges_fiat.TryAdd(item.CharCode.ToUpper(), (nominal * usd / cur));
                        }
                        
                    }
                    return exchanges_fiat;
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("ExchangeRateApi", ex.ToString());
            }
            return exchanges_fiat;
        }
        
        public static void UpdateExchangesFiat(Dictionary<string, double> newExchanges)
        {
            try
            {
                if (newExchanges == null) return;
                foreach (var key in newExchanges.Keys)
                {
                    ExchangesFiat.AddOrUpdate(key, newExchanges[key], (k, v) => newExchanges[k]);
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("ExchangeRateApi", ex.Message);
            }
        }

        public static double ConvertUSDToNationalCurrency(double usd)
        {
            if (ExchangesFiat.Count == 0)
            {
                return usd;
            }

            if (ExchangesFiat.TryGetValue(ActiveDisplayCurrency, out var fiatExchangeRate))
            {
                return usd * fiatExchangeRate;
            }

            Helpers.ConsolePrint("ExchangeRateApi", "Unknown Currency Tag:" + ActiveDisplayCurrency + ". Falling back to USD rates");
            ActiveDisplayCurrency = "USD";
            return usd;
        }
        public static double ConvertBTCToNationalCurrency(double btc)//mBTC
        {
            if (ExchangesFiat.Count == 0)
            {
                return btc;
            }
            double _usdratio = 0d;
            if (ExchangesFiat.TryGetValue(ActiveDisplayCurrency, out var CoinExchangeRate))
            {
                _usdratio =  btc * BTCcostUSD * CoinExchangeRate;
            }
            return _usdratio;
        }
        public static double ConvertNationalCurrencyToBTC(double coin)//mBTC
        {
            if (ExchangesFiat.Count == 0)
            {
                return coin;
            }
            double _btc = 0d;
            if (ExchangesFiat.TryGetValue(ActiveDisplayCurrency, out var CoinExchangeRate))
            {
                _btc = coin / BTCcostUSD / CoinExchangeRate;
            }
            return _btc;
        }
        public static double ConvertPayoutCurrencyToNationalCurrency(double coin)
        {
            if (ExchangesFiat.Count == 0)
            {
                return coin;
            }
            double _usdratio = 0d;
            double _btcratio = 0d;
            if (MarketRatesList.TryGetValue(ConfigManager.GeneralConfig.PayoutCurrency, out var coinExchangeRate))
            {
                _usdratio = coin * coinExchangeRate;
                _btcratio = _usdratio / BTCcostUSD;
                return ConvertBTCToNationalCurrency(_btcratio);
            }
            /*
            if (ExchangesFiat.TryGetValue(ActiveDisplayCurrency, out var USDExchangeRate))
            {
                _usdratio = coin * BTCcost * USDExchangeRate;
            }
            */
            return _usdratio;
        }
        public static double ConvertBTCToPayoutCurrency(double btc)
        {
            if (MarketRatesList.Count == 0 || ConfigManager.GeneralConfig.PayoutCurrency == "BTC")
            {
                return btc;
            }
            double _coinratio = 0d;
            if (MarketRatesList.TryGetValue(ConfigManager.GeneralConfig.PayoutCurrency, out var coinExchangeRate))
            {
                _coinratio = (btc * BTCcostUSD) / coinExchangeRate;
                return _coinratio;
            }

            Helpers.ConsolePrint("ExchangeRateApi", "Unknown Currency Tag:" + ConfigManager.GeneralConfig.PayoutCurrency + "! Falling back to BTC rates");
            ConfigManager.GeneralConfig.PayoutCurrency = "BTC";
            return btc;
        }

        public static double GetPayoutCurrencyExchangeRate()
        {
            //return UsdBtcRate > 0 ? UsdBtcRate : 0.0;
            MarketRatesList.TryGetValue(ConfigManager.GeneralConfig.PayoutCurrency, out var btcExchangeRate);
            return btcExchangeRate;
        }

        /// <summary>
        /// Get price of kW-h in BTC if it is set and exchanges are working
        /// Otherwise, returns 0
        /// </summary>
        public static double GetKwhPriceInBtc()
        {
            var price = Form_Main.GetKwhPrice();
            if (price <= 0) return 0;
            
            if (BTCcostUSD <= 0)
            {
                return 0;
            }
            return ConvertNationalCurrencyToBTC(price);
        }
    }

}
