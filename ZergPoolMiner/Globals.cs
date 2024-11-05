using Newtonsoft.Json;
using ZergPoolMiner.Switching;
using ZergPoolMinerLegacy.Common.Enums;

namespace ZergPoolMiner
{
    public class Globals
    {
        public static string[] MiningLocation = { ".", ".na.", ".eu.", ".asia." };

        public static readonly string DemoUser = "LPCcdmUzWuQKFJNibeH5Ljz42VMfwTXJHq";
        public static readonly string DemoPayoutCurrency = "LTC";
        public static int CurrentTosVer = 4;
        public static JsonSerializerSettings JsonSettings = null;
        public static int ThreadsPerCpu;
        public static bool IsFirstNetworkCheckTimeout = true;
        public static int FirstNetworkCheckTimeoutTimeMs = 500;
        public static int FirstNetworkCheckTimeoutTries = 5;


        public static string GetLocationUrl(AlgorithmType algorithmType, string miningLocation, NhmConectionType conectionType)
        {
            if (!AlgosProfitData.TryGetSma(algorithmType, out var sma)) return "";

            var name = sma.Name;
            var nPort = sma.Port;
            var sslPort = 30000 + nPort;
            string ret = "";

            // NHMConectionType.NONE
            var prefix = "";
            var port = nPort;

            port = 9200;
            switch (conectionType)
            {
                case NhmConectionType.LOCKED:
                    return miningLocation;
                case NhmConectionType.STRATUM_TCP:
                    prefix = "stratum+tcp://";
                    break;
                case NhmConectionType.STRATUM_SSL:
                    prefix = "stratum+ssl://";
                    port = sslPort;
                    break;
            }
            if (miningLocation.Contains("auto"))
            {
                ret = prefix
                   + name
                   + "." + miningLocation
                   + ":"
                   + "9200";
            } else
            {
                ret = prefix
                   + name
                   + "." + miningLocation
                   + ":"
                   + (nPort + 10000).ToString();
            }
            return ret;
        }

        public static string GetBitcoinUser()
        {
            return BitcoinAddress.ValidateBitcoinAddress(Configs.ConfigManager.GeneralConfig.Wallet.Trim())
                ? Configs.ConfigManager.GeneralConfig.Wallet.Trim()
                : DemoUser;

        }
    }
}
