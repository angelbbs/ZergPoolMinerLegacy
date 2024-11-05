using Newtonsoft.Json;
using ZergPoolMiner.Configs;
using ZergPoolMiner.Devices;
using ZergPoolMiner.Switching;
using ZergPoolMinerLegacy.UUID;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace ZergPoolMiner.Stats
{
    public class Socket
    {
        public static WebSocket _webSocket;
        public bool IsAlive => _webSocket.ReadyState == WebSocketState.Open;
        public static string RigID => ConfigManager.GeneralConfig.MachineGuid;

        public static bool IsIPAddress(string ipAddress)
        {
            System.Net.IPAddress address;
            bool isIPAddres = false;
            if (System.Net.IPAddress.TryParse(ipAddress, out address))
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    isIPAddres = true;
                }
            }
            return isIPAddres;
        }

        public static bool CheckForInternetConnection(int timeoutMs = 10000, string url = null)
        {
            try
            {
                url ??= CultureInfo.InstalledUICulture switch
                {
                    { Name: var n } when n.StartsWith("ru") =>
                        "http://www.ya.ru",
                    { Name: var n } when n.StartsWith("en") =>
                        "http://www.google.com",
                    _ =>
                        "http://www.gstatic.com/generate_204",
                };
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.KeepAlive = false;
                request.Timeout = timeoutMs;
                using (var response = (HttpWebResponse)request.GetResponse())
                    return true;
            }
            catch
            {
                return false;
            }
        }

       
        public static void DropIPPort(int processId, string IP, uint port, bool message = true)
        {
            ProcessStartInfo cports;

            cports = new ProcessStartInfo("utils/cports-x64/cports.exe");
            cports.Arguments = "/close * * " + IP + " " + port.ToString() + " " + processId.ToString();
            cports.UseShellExecute = false;
            cports.RedirectStandardError = false;
            cports.RedirectStandardOutput = false;
            cports.CreateNoWindow = true;
            cports.WindowStyle = ProcessWindowStyle.Hidden;
            Helpers.ConsolePrint("DropIPPort", "Drop port " + IP + ":" + port.ToString() + " completed");
            try
            {
                Process.Start(cports);
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("DropIPPort", ex.Message);
            }
            if (message)
            {
                Helpers.ConsolePrint("DropIPPort", "Drop port " + IP + ":" + port.ToString() + " completed");
            }
        }
    }
}
