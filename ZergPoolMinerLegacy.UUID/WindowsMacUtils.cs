using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace ZergPoolMinerLegacy.UUID
{
    public static class WindowsMacUtils
    {
        [DllImport("rpcrt4.dll", SetLastError = true)]
        private static extern int UuidCreateSequential(out System.Guid guid);

        private const long RPC_S_OK = 0L;
        private const long RPC_S_UUID_LOCAL_ONLY = 1824L;
        private const long RPC_S_UUID_NO_ADDRESS = 1739L;


        public static string GetMAC_UUID()
        {
            try
            {
                System.Guid guid;
                UuidCreateSequential(out guid);
                // Console.WriteLine(guid);
                // Console.WriteLine(GetMACAddress());
                var splitted = guid.ToString().Split('-');
                var last = splitted.LastOrDefault();
                if (last != null) return last;
            }
            catch (Exception)
            {
                //                Logger.Error("NHM.UUID", $"WindowsMacUtils.GetMAC_UUID: {e.Message}");
            }
            //            Logger.Warn("NHM.UUID", $"WindowsMacUtils.GetMAC_UUID FALLBACK");
            return System.Guid.NewGuid().ToString();
        }
        public static string GetMACAddress()
        {
            try
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                string sMacAddress = "";
                foreach (NetworkInterface adapter in nics)
                {
                    if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                        adapter.OperationalStatus == OperationalStatus.Up)
                    {
                        IPInterfaceProperties properties = adapter.GetIPProperties();
                        sMacAddress = adapter.GetPhysicalAddress().ToString();
                        if (sMacAddress.Split('0').Length - 1 < 3)
                        {
                            return "-" + sMacAddress.Substring(8);
                        }
                    }
                }
            } catch (Exception ex)
            {

            }
            return "";
            
            /*
            string firstMacAddress = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up && 
                nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(nic => nic.GetPhysicalAddress().ToString()).FirstOrDefault();
            return firstMacAddress;
            */
        }
    }
}
