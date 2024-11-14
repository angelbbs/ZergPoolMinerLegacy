using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZergPoolMiner.Wallets
{
    [Serializable]
    public static class Wallets
    {
        public class WalletData
        {
            public bool Use;
            public string Coin;
            public double Treshold;
            public string Wallet;
            public string ID;
        }
        public static List<WalletData> WalletDataList = new List<WalletData>();
        public static List<PayoutCoin> CoinsList = new List<PayoutCoin>();
        public class PayoutCoin
        {
            public string Coin;
            public string Name;
            public double Treshold;
        }
        public static void InitCoinsList()
        {
            CoinsList.Clear();
            PayoutCoin payoutCoin = new PayoutCoin();

            payoutCoin.Coin = "BCH";
            payoutCoin.Name = "Bitcoin Cash";
            payoutCoin.Treshold = 0;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "BNB";
            payoutCoin.Name = "BinanceSmartChain";
            payoutCoin.Treshold = 0;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "BTC";
            payoutCoin.Name = "Bitcoin";
            payoutCoin.Treshold = 0;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "CAKE";
            payoutCoin.Name = "Pancake (BEP20)";
            payoutCoin.Treshold = 0;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "DASH";
            payoutCoin.Name = "Dash";
            payoutCoin.Treshold = 0;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "DOGE";
            payoutCoin.Name = "Dogecoin";
            payoutCoin.Treshold = 0;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "ETH";
            payoutCoin.Name = "Ethereum";
            payoutCoin.Treshold = 0;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "FLUX";
            payoutCoin.Name = "Flux";
            payoutCoin.Treshold = 0;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "KAS";
            payoutCoin.Name = "Kaspa";
            payoutCoin.Treshold = 0;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "KLV";
            payoutCoin.Name = "Klever (TRC20)";
            payoutCoin.Treshold = 0;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "LTC";
            payoutCoin.Name = "Litecoin";
            payoutCoin.Treshold = 0;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "RTM";
            payoutCoin.Name = "Raptoreum";
            payoutCoin.Treshold = 0;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "RVN";
            payoutCoin.Name = "Ravencoin";
            payoutCoin.Treshold = 0;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "SHIB";
            payoutCoin.Name = "Shiba Inu(ERC20)";
            payoutCoin.Treshold = 0;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "TRX";
            payoutCoin.Name = "Tron";
            payoutCoin.Treshold = 0;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "USDC";
            payoutCoin.Name = "USD Coin(ERC20)";
            payoutCoin.Treshold = 0;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "USDC-BEP20";
            payoutCoin.Name = "USD Coin(BEP20)";
            payoutCoin.Treshold = 0;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "USDC-TRC20";
            payoutCoin.Name = "USD Coin(TRC20)";
            payoutCoin.Treshold = 0;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "USDT";
            payoutCoin.Name = "Tether (ERC20)";
            payoutCoin.Treshold = 0;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "USDT-BEP20";
            payoutCoin.Name = "Tether (BEP20)";
            payoutCoin.Treshold = 0;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "USDT-TRC20";
            payoutCoin.Name = "Tether (TRC20)";
            payoutCoin.Treshold = 0;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "WIN";
            payoutCoin.Name = "Wink (TRC20)";
            payoutCoin.Treshold = 0;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "XMR";
            payoutCoin.Name = "Monero";
            payoutCoin.Treshold = 0;
            CoinsList.Add(payoutCoin);

            //Stats.Stats.GetCoins();
        }
        public static void InitWallets()
        {
            try
            {
                if (File.Exists("configs\\wallets.json"))
                {
                    string json = File.ReadAllText("configs\\wallets.json");
                    WalletDataList = JsonConvert.DeserializeObject<List<WalletData>>(json);
                }
            } catch (Exception ex)
            {
                Helpers.ConsolePrint("InitWallets", ex.ToString());
            }
        }
    }
}
