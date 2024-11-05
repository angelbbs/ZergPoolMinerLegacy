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
            payoutCoin.Treshold = 0.0015;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "BNB";
            payoutCoin.Name = "BinanceSmartChain";
            payoutCoin.Treshold = 0.03258;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "BTC";
            payoutCoin.Name = "Bitcoin";
            payoutCoin.Treshold = 0.0015;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "CAKE";
            payoutCoin.Name = "Pancake (BEP20)";
            payoutCoin.Treshold = 17.36883;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "DASH";
            payoutCoin.Name = "Dash";
            payoutCoin.Treshold = 0.26588;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "DOGE";
            payoutCoin.Name = "Dogecoin";
            payoutCoin.Treshold = 111482.18666;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "ETH";
            payoutCoin.Name = "Ethereum";
            payoutCoin.Treshold = 0.05;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "FLUX";
            payoutCoin.Name = "Flux";
            payoutCoin.Treshold = 5;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "KAS";
            payoutCoin.Name = "Kaspa";
            payoutCoin.Treshold = 50;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "KLV";
            payoutCoin.Name = "Klever (TRC20)";
            payoutCoin.Treshold = 29000.48130;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "LTC";
            payoutCoin.Name = "Litecoin";
            payoutCoin.Treshold = 0.05;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "RTM";
            payoutCoin.Name = "Raptoreum";
            payoutCoin.Treshold = 1184.16067;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "RVN";
            payoutCoin.Name = "Ravencoin";
            payoutCoin.Treshold = 50;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "SHIB";
            payoutCoin.Name = "Shiba Inu(ERC20)";
            payoutCoin.Treshold = 17170329.67033;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "TRX";
            payoutCoin.Name = "Tron";
            payoutCoin.Treshold = 20;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "USDC";
            payoutCoin.Name = "USD Coin(ERC20)";
            payoutCoin.Treshold = 200;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "USDC-BEP20";
            payoutCoin.Name = "USD Coin(BEP20)";
            payoutCoin.Treshold = 25;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "USDC-TRC20";
            payoutCoin.Name = "USD Coin(TRC20)";
            payoutCoin.Treshold = 25;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "USDT";
            payoutCoin.Name = "Tether (ERC20)";
            payoutCoin.Treshold = 200;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "USDT-BEP20";
            payoutCoin.Name = "Tether (BEP20)";
            payoutCoin.Treshold = 25;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "USDT-TRC20";
            payoutCoin.Name = "Tether (TRC20)";
            payoutCoin.Treshold = 25;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "WIN";
            payoutCoin.Name = "Wink (TRC20)";
            payoutCoin.Treshold = 681322.47956;
            CoinsList.Add(payoutCoin);

            payoutCoin = new PayoutCoin();
            payoutCoin.Coin = "XMR";
            payoutCoin.Name = "Monero";
            payoutCoin.Treshold = 0.20930;
            CoinsList.Add(payoutCoin);
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
