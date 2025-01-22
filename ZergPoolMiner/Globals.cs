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

        public static string GetBitcoinUser()
        {
            return BitcoinAddress.ValidateBitcoinAddress(Configs.ConfigManager.GeneralConfig.Wallet.Trim())
                ? Configs.ConfigManager.GeneralConfig.Wallet.Trim()
                : DemoUser;

        }
    }
}
