using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using System.Diagnostics;
using System.Net.Sockets;

namespace ZergPoolMiner.Stats
{
    public class ProxyChecker
    {
        public class Proxy
        {
            public string Ip { get; set; }
            public int HTTPSPort { get; set; }
            public int Socks5Port { get; set; }
            public int Speed { get; set; }
            public bool Valid { get; set; }
        }

        public static List<Proxy> CheckProxies(List<Proxy> _proxies, int timeout)
        {
            var results = new List<Proxy>();

            //Random r = new Random();
            //int rInt = r.Next(0, _proxies.Count - Environment.ProcessorCount - 1);
            //int to = from + Environment.ProcessorCount;
            for (int i = 0; i < _proxies.Count; i++)
            {
                var proxy = _proxies[i];
                //if (results.Count >= 10) break;

                var ip = proxy.Ip;
                var httpsport = proxy.HTTPSPort;
                var socks5port = proxy.Socks5Port;

                var t = Task.Factory.StartNew(() => {
                    using (var socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        bool success = false;
                        new Thread(() =>
                        {
                            Thread.Sleep(timeout);
                            if (!success)
                            {
                                socket.Close();
                            }
                        }).Start();
                        
                        socket.ReceiveTimeout = timeout;
                        socket.SendTimeout = timeout;
                        Proxy pl = new();
                        try
                        {
                            var watch = Stopwatch.StartNew();
                            Helpers.ConsolePrint("CheckProxies", "Try connect to " + ip + ":" + httpsport.ToString());
                            socket.Connect(ip, httpsport);

                            if (socket.Connected)
                            {
                                success = true;
                                pl.Ip = ip;
                                pl.HTTPSPort = httpsport;
                                pl.Speed = (int)watch.ElapsedMilliseconds;
                                pl.Valid = true;

                                object _lock = new object();
                                lock (_lock)
                                {
                                    results.Add(pl);
                                }
                                watch.Stop();
                            }
                        }
                        catch (Exception sex)
                        {
                            //Helpers.ConsolePrint("CheckProxies", sex.Message);
                            /*
                            if ((ProxyCheck.HTTPSInvalidProxyList.FindAll(x => x.Ip == ip).Count() == 0))
                            {
                                pl.Ip = ip;
                                pl.Port = port;
                                ProxyCheck.HTTPSInvalidProxyList.Add(pl);
                            }
                            */
                        }
                    }

                    return Task.CompletedTask;
                });
            }
            return results;
        }
    }
}
