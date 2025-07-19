using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Security;
using System.Diagnostics;
using System.ComponentModel;
using System.Net.NetworkInformation;
using ZergPoolMinerLegacy.Stats;
using ZergPoolMiner.Configs;

namespace ZergPoolMiner.Stats
{
    public static class Socks5Relay
    {
        public static int Port = 13600;
        public static readonly TcpListener Listener = new TcpListener(IPAddress.Any, Port);
        public static bool started = false;
        const int BufferSize = 4096;

        public static void Socks5RelayStart()
        {
            if (started) return;
            started = true;
            while (CheckRelayPort(Port))
            {
                Port++;
                Thread.Sleep(100);
            }
            Helpers.ConsolePrint("Socks5Relay", "Start relay 127.0.0.1:" + Port + " -> " + Stats.CurrentProxyIP + ":" + Stats.CurrentProxySocks5SPort.ToString());
            ConfigManager.GeneralConfig.RelayPort = Port;
            try
            {
                Listener.Start();
                new Task(() =>
                {
                    while (true)
                    {
                        try
                        {
                            var minerClient = Listener.AcceptTcpClient();
                            if (minerClient.Connected)
                            {
                                Helpers.ConsolePrint("Socks5Relay", "Miner connected to relay 127.0.0.1:" + Port +
                                    " Proxy: " + Stats.CurrentProxyIP + ":" + Stats.CurrentProxySocks5SPort.ToString());
                                new Task(() => AcceptConnection(minerClient)).Start();
                            }
                        } catch (Exception ex)
                        {
                            Helpers.ConsolePrintError("Socks5Relay", ex.Message);
                            started = false;
                            break;
                        }
                    }
                }).Start();
            } catch (Exception ex)
            {
                Helpers.ConsolePrintError("Socks5Relay", ex.ToString());
                started = false;
            }
        }

        private static void AcceptConnection(TcpClient minerClient)
        {
            try
            {
                var minerStream = minerClient.GetStream();
                var proxy = new TcpClient(Stats.CurrentProxyIP, Stats.CurrentProxySocks5SPort);
                NetworkStream proxyStream = proxy.GetStream();
                new Task(() => ReadFromMiner(minerClient, minerStream, proxyStream)).Start();
                new Task(() => ReadFromProxy(minerClient,proxyStream, minerStream)).Start();
            }
            catch (Exception ex)
            {
                started = false;
                Helpers.ConsolePrintError("Socks5Relay", ex.ToString());
                if (minerClient is object && minerClient != null)
                {
                    minerClient.Close();
                    minerClient.Dispose();
                }
            }
        }

        private static void ReadFromProxy(TcpClient minerClient, Stream proxyStream, Stream minerStream)
        {
            var message = new byte[BufferSize];
            while (true)
            {
                int serverBytes = 0;
                try
                {
                    serverBytes = proxyStream.Read(message, 0, BufferSize);
                    minerStream.Write(message, 0, serverBytes);
                }
                catch (Exception ex)
                {
                    //Helpers.ConsolePrintError("Socks5Relay", ex.ToString());
                    break;
                }
                if (serverBytes == 0)
                {
                    break;
                }
            }

            if (minerStream is object && minerStream != null)
            {
                minerStream.Close();
                minerStream.Dispose();
            }
            if (minerClient is object && minerClient != null)
            {
                minerClient.Client.Close();
                minerClient.Client.Dispose();
            }
        }

        private static void ReadFromMiner(TcpClient minerClient, Stream minerStream, Stream proxyStream)
        {
            var message = new byte[BufferSize];
            while (true)
            {
                int minerBytes = 0;
                try
                {
                    minerBytes = minerStream.Read(message, 0, BufferSize);
                    proxyStream.Write(message, 0, minerBytes);
                }
                catch (Exception ex)
                {
                    //Helpers.ConsolePrintError("Socks5Relay", ex.ToString());
                    break;
                }
                if (minerBytes == 0)
                {
                    break;
                }
            }

            if (proxyStream is object && proxyStream != null)
            {
                proxyStream.Close();
                proxyStream.Dispose();
            }
        }


        public static bool CheckRelayPort(int Port)
        {
            try
            {
                List<Connection> _allConnections = new List<Connection>();
                _allConnections.Clear();
                _allConnections.AddRange(NetworkInformation.GetTcpV4Connections());
                Connection.UpdateProcessList();

                for (int i = 1; i < _allConnections.Count; i++)
                {
                    /*
                    Helpers.ConsolePrintError("CheckRelayPort", _allConnections[i].LocalEndPoint.Port.ToString() + " " +
                        _allConnections[i].RemoteEndPoint.Port.ToString() + " " +
                        _allConnections[i].OwningProcess);
                    */
                    if (Port == _allConnections[i].LocalEndPoint.Port ||
                        Port == _allConnections[i].RemoteEndPoint.Port)
                    {
                        var id = _allConnections[i].OwningPid;
                        Helpers.ConsolePrintError("CheckRelayPort", "Relay port in use by " + _allConnections[i].OwningProcess);
                        return true;
                    }
                    
                    Thread.Sleep(1);
                }
                _allConnections.Clear();
                _allConnections = null;

                return false;
            }
            catch (Exception e)
            {
                Helpers.ConsolePrintError("CheckRelayPort", e.ToString());
                Thread.Sleep(500);
            }
            finally
            {

            }
            return false;
        }
    }

    
}
