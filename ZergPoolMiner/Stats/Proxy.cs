using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZergPoolMiner.Stats
{
    public class ProxyCheck
    {
        public static List<ProxyChecker.Proxy> HttpsProxyList = new();
        public static List<ProxyChecker.Proxy> HTTPSInvalidProxyList = new();
        public static ProxyChecker.Proxy CurrentHttpsProxy = new();
        public static void GetHttpsProxy()
        {
            Helpers.ConsolePrint("GetHttpsProxy", "Start check https proxy");
            /*
            try
            {
                if (File.Exists("Configs\\HTTPSInvalidProxyList.json"))
                {
                    var json = File.ReadAllText("Configs\\HTTPSInvalidProxyList.json");
                    HTTPSInvalidProxyList = JsonConvert.DeserializeObject<List<ProxyChecker.Proxy>>(json);
                }
            } catch (Exception ex)
            {
                Helpers.ConsolePrintError("GetHttpsProxy", ex.ToString());
            }
            */
            HttpsProxyList = new();
            List<ProxyChecker.Proxy> _HttpsProxyList = new();

            //string link = @"https://cdn.jsdelivr.net/gh/proxifly/free-proxy-list@main/proxies/protocols/socks5/data.json";

            //https https://github.com/claude89757/free_https_proxies
            //string link = @"https://raw.githubusercontent.com/claude89757/free_https_proxies/refs/heads/main/https_proxies.txt";
            //var list1 = GetProxyList(link).Distinct().ToList().FindAll(x => x.Port == 3128);
            //_HttpsProxyList.AddRange(GetProxyList(link).Distinct().ToList().FindAll(x => x.Port == 3128));

            //https https://github.com/casa-ls/proxy-list
            //string link = @"https://raw.githubusercontent.com/casa-ls/proxy-list/refs/heads/main/http";
            //_HttpsProxyList.AddRange(GetProxyList(link).Distinct().ToList());

            /*
            try
            {
                if (File.Exists("Configs\\HTTPSValidProxy.txt"))
                {
                    var text = File.ReadAllText("Configs\\HTTPSValidProxy.txt");
                    ProxyChecker.Proxy proxy = new();
                    string ip = text.Split(':')[0];
                    string _port = text.Split(':')[1];
                    int.TryParse(_port, out int port);
                    proxy.Ip = ip;
                    proxy.Port = port;
                    proxy.Speed = 0;
                    proxy.Valid = true;
                    object _lock = new object();
                    lock (_lock)
                    {
                        HttpsProxyList.Add(proxy);
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrintError("GetHttpsProxy", ex.ToString());
            }
            */
            /*
            if (_HttpsProxyList.Count > 0)
            {
                int timeout = 800;
                for (int j = 0; j < _HttpsProxyList.Count; j = j + 8)
                //for (int j = 0; j < 256; j = j + 8)
                {
                    if (j + 8 >= _HttpsProxyList.Count) break;
                    List<ProxyChecker.Proxy> tempProxyList = new();
                    for (int i = j; i < 8 + j; i++)
                    {
                        ProxyChecker.Proxy proxy = new();
                        proxy.Ip = _HttpsProxyList[i].Ip;
                        proxy.Port = _HttpsProxyList[i].Port;
                        tempProxyList.Add(proxy);

                        //HttpsProxyList.AddRange(pr);
                        //if (HttpsProxyList.Count > 15) break;
                    }
                    var pr = ProxyChecker.CheckProxies(tempProxyList, timeout);
                    Thread.Sleep(timeout);
                    HttpsProxyList.AddRange(pr);
                }
            }
            */
            ProxyChecker.Proxy proxy = new();
            
            proxy.Ip = "193.106.150.178";
            proxy.HTTPSPort = 13150;
            proxy.Socks5Port = 13155;
            proxy.Valid = true;
            proxy.Speed = 1; 
            
            HttpsProxyList.Add(proxy);

            proxy = new();
            proxy.Ip = "31.58.171.225";
            proxy.HTTPSPort = 13150;
            proxy.Socks5Port = 13155;
            proxy.Valid = true;
            proxy.Speed = 2;
            HttpsProxyList.Add(proxy);
            
            /*
            proxy.Ip = "192.168.1.110";
            proxy.HTTPSPort = 13150;
            proxy.Socks5Port = 13155;
            proxy.Valid = true;
            proxy.Speed = 1;
            HttpsProxyList.Add(proxy);
            */

            Stats.CurrentProxyIP = HttpsProxyList[0].Ip;
            Stats.CurrentProxySocks5SPort = HttpsProxyList[0].Socks5Port;
            HttpsProxyList = HttpsProxyList.OrderBy(s => s.Speed).ToList();
            /*
            foreach (var p in HttpsProxyList)
            {
                Helpers.ConsolePrint("GetHttpsProxy", "Valid https proxy: " + p.Ip + ":" + p.Port.ToString() + " " + p.Speed.ToString() + "ms");
            }
            Helpers.ConsolePrint("GetHttpsProxy", "Valid " + HttpsProxyList.Count.ToString() + " of " + _HttpsProxyList.Count.ToString());
            */
        }
    }
}
