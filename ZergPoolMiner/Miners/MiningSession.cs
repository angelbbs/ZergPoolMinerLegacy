using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZergPoolMiner.Algorithms;
using ZergPoolMiner.Configs;
using ZergPoolMiner.Devices;
using ZergPoolMiner.Forms;
using ZergPoolMiner.Interfaces;
using ZergPoolMiner.Miners.Grouping;
using ZergPoolMiner.Stats;
using ZergPoolMiner.Switching;
using ZergPoolMinerLegacy.Common.Enums;
//using ZergPoolMinerLegacy.Overclock;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace ZergPoolMiner.Miners
{
    using GroupedDevices = SortedSet<string>;

    public class MiningSession
    {
        private const string Tag = "MiningSession";
        private const string DoubleFormat = "F12";

        // session varibles fixed
        //public static string _miningLocation;

        public static string _wallet;
        public static string _password;
        public static List<MiningDevice> _miningDevices;
        private readonly IMainFormRatesComunication _mainFormRatesComunication;

        private readonly AlgorithmSwitchingManager _switchingManager;

        // session varibles changing
        // GroupDevices hash code doesn't work correctly use string instead
        //Dictionary<GroupedDevices, GroupMiners> _groupedDevicesMiners;
        public static Dictionary<string, GroupMiner> _runningGroupMiners = new();

        private bool _isProfitable;

        private bool _isConnectedToInternet;
        private readonly bool _isMiningRegardlesOfProfit;

        // timers
        private readonly Timer _preventSleepTimer;
        //public static Timer _smaCheckTimer;

        // check internet connection
        private readonly Timer _internetCheckTimer;
        public static bool FuncAttached = false;

        public bool IsMiningEnabled => _miningDevices.Count > 0;

        private bool IsCurrentlyIdle => !IsMiningEnabled || !_isConnectedToInternet || !_isProfitable;
        public static int[] _ticks;
        public static bool KawpowLiteForceStop = false;
        public static List<Coin> DevicesCoinList = new();

        public class Coin
        {
            public string _Coin;
            public AlgorithmType _Algorithm;
            public AlgorithmType _DualAlgorithm;
        }

        public List<int> ActiveDeviceIndexes
        {
            get
            {
                var minerIDs = new List<int>();
                if (!IsCurrentlyIdle)
                {
                    foreach (var miner in _runningGroupMiners.Values)
                    {
                        minerIDs.AddRange(miner.DevIndexes);
                    }
                }

                return minerIDs;
            }
        }

        public static void StopEvent()
        {

        }

        public static List<ComputeDevice> _ComputeDevices = new();
        public MiningSession(List<ComputeDevice> devices,
            IMainFormRatesComunication mainFormRatesComunication,
            string wallet, string password)
        {
            _ComputeDevices = devices;
            _ticks = new int[devices.Count];
            for (int d = 0; d < _ticks.Length; d++)
            {
                _ticks[d] = 999;
            }
            // init fixed
            _mainFormRatesComunication = mainFormRatesComunication;
            // _miningLocation = miningLocation;
            _switchingManager = new AlgorithmSwitchingManager();
            _miningDevices = GroupSetupUtils.GetMiningDevices(devices, true);

            _wallet = wallet;
            _password = password;
            
            // initial settup
            if (_miningDevices.Count > 0)
            {
                GroupSetupUtils.AvarageSpeeds(_miningDevices);
            }
            // init timer stuff
            _preventSleepTimer = new Timer();
            _preventSleepTimer.Elapsed += PreventSleepTimer_Tick;
            // sleep time is minimal 1 minute
            _preventSleepTimer.Interval = 20 * 1000; // leave this interval, it works

            // set internet checking
            _internetCheckTimer = new Timer();
            _internetCheckTimer.Elapsed += InternetCheckTimer_Tick;
            _internetCheckTimer.Interval = 1 * 1000 * 60; // every minute

            // assume profitable
            _isProfitable = true;
            // assume we have internet
            _isConnectedToInternet = true;

            if (!FuncAttached)
            {
                Helpers.ConsolePrint("MiningSession", "Process attached");
                AlgorithmSwitchingManager.SmaCheck += SwichMostProfitableGroupUpMethod;
                /*
                if (_miningDevices.Count > 0)
                {
                    SwichMostProfitableGroupUpMethod(null, null);
                }
                */
                //ZergPoolMiner.Switching.AlgorithmSwitchingManager.Stop();
                //ZergPoolMiner.Switching.AlgorithmSwitchingManager.Start();
                FuncAttached = true;
            } else
            {
                Helpers.ConsolePrint("MiningSession", "Process NOT attached");
            }

            if (IsMiningEnabled)
            {
                _preventSleepTimer.Start();
                _internetCheckTimer.Start();
            }
            AlgorithmSwitchingManager.Stop();
            AlgorithmSwitchingManager.Start();
            _isMiningRegardlesOfProfit = ConfigManager.GeneralConfig.MinimumProfit == 0;

        }

        #region Timers stuff

        private void InternetCheckTimer_Tick(object sender, EventArgs e)
        {
            if (ConfigManager.GeneralConfig.IdleWhenNoInternetAccess)
            {
                _isConnectedToInternet = Helpers.IsConnectedToInternet();
            }
        }

        private void PreventSleepTimer_Tick(object sender, ElapsedEventArgs e)
        {
            // when mining keep system awake, prevent sleep
            Helpers.PreventSleep();
        }

        #endregion

        #region Start/Stop
        private static void KillProcessAndChildren(int pid)
        {
            // Cannot close 'system idle process'.
            if (pid == 0)
            {
                return;
            }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
                    ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }

        public static void RestartMiner(string ProcessTag)
        {
            if (_runningGroupMiners != null)
            {
                foreach (var groupMiner in _runningGroupMiners.Values)
                {
                    if (groupMiner.Miner.ProcessTag().Contains(ProcessTag))
                    {
                        try
                        {
                            int k = ProcessTag.IndexOf("pid(");
                            int i = ProcessTag.IndexOf(")|bin");
                            var cpid = ProcessTag.Substring(k + 4, i - k - 4).Trim();

                            int pid = int.Parse(cpid, CultureInfo.InvariantCulture);
                            KillProcessAndChildren(pid);
                        }
                        catch (Exception e)
                        {
                            Helpers.ConsolePrintError("Restart miner", "RestartMiner(): " + e.Message);
                        }
                    }
                }
            }
        }

        public static void SuspendResumeMiner(string ProcessTag, bool suspend)
        {
            if (_runningGroupMiners != null)
            {
                foreach (var groupMiner in _runningGroupMiners.Values)
                {
                    if (groupMiner.Miner.ProcessTag().Contains(ProcessTag))
                    {
                        try
                        {
                            int k = ProcessTag.IndexOf("pid(");
                            int i = ProcessTag.IndexOf(")|bin");
                            var cpid = ProcessTag.Substring(k + 4, i - k - 4).Trim();
                            int pid = int.Parse(cpid, CultureInfo.InvariantCulture);
                            var process = Process.GetProcessById(pid);
                            if (suspend)
                            {
                                Miner.suspendedPidList.Add(pid);
                                process.Suspend();
                            } else
                            {
                                process.Resume();
                                Miner.suspendedPidList.Remove(pid);
                            }
                        }
                        catch (Exception e)
                        {
                            Helpers.ConsolePrintError("SuspendResumeMiner", e.Message);
                        }
                    }
                }
            }
        }

        public void StopAllMiners()
        {
            if (_runningGroupMiners != null)
            {
                try
                {
                    foreach (var groupMiner in _runningGroupMiners.Values)
                    {
                        groupMiner.End();
                    }
                }
                catch (Exception e)
                {
                    Helpers.ConsolePrintError("StopAllMiners", e.ToString());
                }
                _runningGroupMiners = new Dictionary<string, GroupMiner>();
            }


            //_switchingManager.Stop();
            AlgorithmSwitchingManager.Stop();

            _mainFormRatesComunication?.ClearRatesAll();

            // restroe/enable sleep
            _preventSleepTimer.Stop();
            _internetCheckTimer.Stop();
            Helpers.AllowMonitorPowerdownAndSleep();
        }

        public void StopAllMinersNonProfitable()
        {
            if (_runningGroupMiners != null)
            {
                foreach (var groupMiner in _runningGroupMiners.Values)
                {
                    groupMiner.End();
                }

                _runningGroupMiners = new Dictionary<string, GroupMiner>();
            }

            _mainFormRatesComunication?.ClearRates(-1);
        }

        #endregion Start/Stop

        private static string CalcGroupedDevicesKey(GroupedDevices group)
        {
            return string.Join(", ", group);
        }

        public string GetActiveMinersGroup()
        {
            if (IsCurrentlyIdle)
            {
                return "IDLE";
            }

            var activeMinersGroup = "";

            //get unique miner groups like CPU, NVIDIA, AMD,...
            var uniqueMinerGroups = new HashSet<string>();
            foreach (var miningDevice in _miningDevices)
            {
                //if (miningDevice.MostProfitableKey != AlgorithmType.NONE) {
                uniqueMinerGroups.Add(GroupNames.GetNameGeneral(miningDevice.Device.DeviceType));
                //}
            }

            if (uniqueMinerGroups.Count > 0 && _isProfitable)
            {
                activeMinersGroup = string.Join("/", uniqueMinerGroups);
            }

            return activeMinersGroup;
        }

        public double GetTotalRate()
        {
            double totalRate = 0;

            if (_runningGroupMiners != null)
            {
                totalRate += _runningGroupMiners.Values.Sum(groupMiner => groupMiner.CurrentRate);
            }

            return totalRate;
        }

        public double GetTotalPowerRate()
        {
            double totalPowerRate = 0;

            if (_runningGroupMiners != null)
            {
                totalPowerRate += _runningGroupMiners.Values.Sum(groupMiner => groupMiner.PowerRate);
            }

            return totalPowerRate;
        }
        public double GetTotalPower()
        {
            double totalPower = 0;

            if (_runningGroupMiners != null)
            {
                totalPower += _runningGroupMiners.Values.Sum(groupMiner => groupMiner.TotalPower);
            }

            return totalPower;
        }
        // full of state
        private bool CheckIfProfitable(double currentProfit, bool log = true)
        {
            var profitableDevices = new List<MiningPair>();
            var mostProfitWithoutPower = 0.0d;

            foreach (var device in _miningDevices)
            {
                // check if device has profitable algo
                if (device.HasProfitableAlgo())
                {
                    profitableDevices.Add(device.GetMostProfitablePair());
                    mostProfitWithoutPower += device.GetMostProfitValueWithPower + 
                        device.GetMostProfitValueWithPower * (device.diff / 100);
                }
            }

            var totalRate = MinersManager.GetTotalRate();
            var currentProfitUsd = ExchangeRateApi.ConvertBTCToNationalCurrency(currentProfit);
            _isProfitable =
                _isMiningRegardlesOfProfit
                || !_isMiningRegardlesOfProfit && currentProfitUsd >= ConfigManager.GeneralConfig.MinimumProfit;
            if (log)
            {
                if (!_isProfitable)
                {
                    Helpers.ConsolePrint(Tag,
                        "Current Global profit: NOT PROFITABLE MinProfit " +
                        ConfigManager.GeneralConfig.MinimumProfit.ToString("F2") + " " + 
                        ConfigManager.GeneralConfig.DisplayCurrency + "/Day");
                }
                else
                {
                    /*
                    var profitabilityInfo = _isMiningRegardlesOfProfit
                        ? "mine always regardless of profit"
                        : ConfigManager.GeneralConfig.MinimumProfit.ToString("F8") + " USD/Day";
                    Helpers.ConsolePrint(Tag, "Current Global profit: IS PROFITABLE MinProfit " + profitabilityInfo);
                    */
                }
            }

            return _isProfitable;
        }

        private bool CheckIfShouldMine(double currentProfit, bool log = true)
        {
            // if profitable and connected to internet mine
            var shouldMine = CheckIfProfitable(currentProfit, log) && _isConnectedToInternet;
            if (shouldMine)
            {
                _mainFormRatesComunication.HideNotProfitable();
            }
            else
            {
                if (!_isConnectedToInternet)
                {
                    // change msg
                    if (log) Helpers.ConsolePrint(Tag, "NO INTERNET!!! Stopping mining.");
                    _mainFormRatesComunication.ShowNotProfitable(
                        International.GetText("Form_Main_MINING_NO_INTERNET_CONNECTION"));
                }
                else
                {
                    if (ConfigManager.GeneralConfig.Force_mining_if_nonprofitable)
                    {
                        shouldMine = true;
                    }
                    else
                    {
                        Helpers.ConsolePrint("CheckIfShouldMine", "MINING NOT PROFITABLE!");
                        _mainFormRatesComunication.ShowNotProfitable(
                            International.GetText("Form_Main_MINING_NOT_PROFITABLE"));
                    }
                }

                // return don't group
                StopAllMinersNonProfitable();
            }

            return shouldMine;
        }

        private double prev_percDiff = 0.0d;
        private bool coinChanged = false;
        public static List<string> coinFail = new();
        //задать порог 20%, иначе монета, у которой не найден блок, будет майниться до упора
        private int cointreshold = 20;
        public void SwichMostProfitableGroupUpMethod(object sender, SmaUpdateEventArgs e)
        {
            if (Miner.minerRestarting) return;

            try
            {
                AlgorithmSwitchingManager.SmaCheckTimerOnElapsedRun = true;
                var profitableDevices = new List<MiningPair>();
                var mostProfit = 0.0d;
                var currentProfit = 0.0d;
                coinFail.Clear();
                var interval = AlgorithmSwitchingManager._smaCheckTimer.Interval / 1000;

                foreach (var device in _miningDevices)
                {
                    //fix current minig coin 
                    var checks = new List<GroupMiner>(_runningGroupMiners.Values);
                    foreach (var groupMiners in checks)
                    {
                        var di = groupMiners.DevBusIdIndexes;
                        if (di.Contains(device.Device.BusID))
                        {
                            device.Device._DeviceCurrentMiningCoin = groupMiners.Coin;
                            device.DeviceCurrentMiningCoin = groupMiners.Coin;
                        }
                    }

                    var c = Stats.Stats.CoinList.Find(e => e.symbol.Equals(device.Device._DeviceCurrentMiningCoin));
                    if (c is object && c != null)
                    {
                        if (c.timesincelast_shared + 600 > device.Device.coinMiningTime && device.diffPercent <= cointreshold)
                        {
                            if (!coinFail.Contains(device.Device._DeviceCurrentMiningCoin))
                            {
                                if (!coinFail.Contains(device.Device.Coin))
                                {
                                    coinFail.Add(device.Device.Coin);
                                    /*
                                    Helpers.ConsolePrint(Tag, "No block found for " +
                                        device.Device.Coin);
 */                                   
                                }
                            }
                        }
                        if (c.timesincelast_shared + 600 < device.Device.coinMiningTime &&
                            coinFail.Contains(device.Device.Coin))
                        {
                            Helpers.ConsolePrint(Tag, "Block found for " + device.Device.Coin + " " +
                                c.timesincelast_shared.ToString() + " - " + device.Device.coinMiningTime.ToString());
                            coinFail.Remove(device.Device.Coin);
                        }

                        if (coinFail.Contains(device.Device.Coin) && device.diffPercent > cointreshold)
                        {
                            Helpers.ConsolePrint(Tag, device.Device.Coin + " Diff above " + 
                                cointreshold.ToString() + "% ");
                            coinFail.Remove(device.Device.Coin);
                        }
                    }

                    foreach (var _device in _ComputeDevices)
                    {
                        if (device.Device.BusID == _device.BusID)
                        {
                            _device._DeviceCurrentMiningCoin = device.DeviceCurrentMiningCoin;
                            _device._DeviceMostProfitableCoin = device.DeviceMostProfitableCoin;
                            _device._CurrentProfitableAlgorithmType = device.CurrentProfitableAlgorithmType;
                            _device._MostProfitableAlgorithmType = device.MostProfitableAlgorithmType;
                            _device._CurrentProfitableMinerBaseType = device.CurrentProfitableMinerBaseType;
                            _device._MostProfitableMinerBaseType = device.MostProfitableMinerBaseType;
                            _device._diffPercent = device.diffPercent;
                        }
                    }
                    
                    device.Device.coinMiningTime = device.Device.coinMiningTime + interval;//seconds

                    // calculate profits
                    device.CalculateProfits(e.NormalizedProfits);
                    // check if device has profitable algo
                    if (device.HasProfitableAlgo())
                    {
                        profitableDevices.Add(device.GetMostProfitablePair());
                        if (ConfigManager.GeneralConfig.with_power)
                        {
                            /*
                            mostProfit += device.GetMostProfitValueWithoutPower + 
                                device.GetMostProfitValueWithoutPower * (device.diff / 100);
                            currentProfit += device.GetCurrentProfitValue;
                            */
                            currentProfit = currentProfit + device.GetCurrentProfitValueWithPower;
                            mostProfit = mostProfit + device.GetMostProfitValueWithPower;
                        }
                        else
                        {
                            /*
                            mostProfit += device.GetMostProfitValueWithPower + 
                                device.GetMostProfitValueWithPower * (device.diff / 100);
                            currentProfit += device.GetCurrentProfitValueWithPower;
                            */
                            currentProfit = currentProfit + device.GetCurrentProfitValue;
                            mostProfit = mostProfit + device.GetMostProfitValueWithoutPower;
                        }
                    }
                }

                var stringBuilderFull = new StringBuilder();
                stringBuilderFull.AppendLine("Current device profits:");
                double smaTmp = 0;

                bool needSwitch = false;
                bool forceSwitch = false;
                bool algoZero = false;
                string algoZeroS = "";
                string coinZeroS = "";
                Form_Main.KawpowLiteEnabled = false;

                foreach (var device in _miningDevices)
                {
                    foreach (var algo in device.Algorithms)
                    {
                        if (algo.ZergPoolID == AlgorithmType.KawPowLite)
                        {
                            Form_Main.KawpowLiteEnabled = true;
                        }
                        smaTmp = smaTmp + algo.CurNhmSmaDataVal;

                        if (algo.CurPayingRatio + algo.CurSecondPayingRatio == 0)
                        {
                            if ((AlgorithmType)device.Device.AlgorithmID == algo.ZergPoolID ||
                                (AlgorithmType)device.Device.SecondAlgorithmID == algo.DualZergPoolID)
                            {
                                algoZero = true;
                                algoZeroS = algo.AlgorithmName;
                                coinZeroS = algo.CurrentMiningCoin;

                                coinFail.Remove(coinZeroS);//иначе не переключится и на графике будет провал
                            }
                        }
                    }
                    // most profitable
                    string mostProfitStrWithPower = ExchangeRateApi.ConvertBTCToNationalCurrency(
                        device.GetMostProfitValueWithPower).ToString("F2") + " " +
                        ConfigManager.GeneralConfig.DisplayCurrency;
                    string mostProfitStrWithoutPower = ExchangeRateApi.ConvertBTCToNationalCurrency(
                        device.GetMostProfitValueWithoutPower).ToString("F2") + " " +
                        ConfigManager.GeneralConfig.DisplayCurrency;

                    string currentProfitStrWithPower = ExchangeRateApi.ConvertBTCToNationalCurrency(device.GetCurrentProfitValueWithPower).ToString("F2") + " " +
                        ConfigManager.GeneralConfig.DisplayCurrency;
                    string currentProfitStrWithoutPower = ExchangeRateApi.ConvertBTCToNationalCurrency(device.GetCurrentProfitValue).ToString("F2") + " " +
                        ConfigManager.GeneralConfig.DisplayCurrency;
                    var _diff = Math.Abs((device.GetMostProfitValueWithPower - device.GetCurrentProfitValueWithPower) / 100);

                    double a = Math.Max(device.GetCurrentProfitValue, device.GetMostProfitValueWithoutPower);
                    double b = Math.Min(device.GetCurrentProfitValue, device.GetMostProfitValueWithoutPower);
                    device.diffPercent = ((a - b)) / Math.Abs(b) * 100;

                    /*
                    if (double.IsNaN(device.diffPercent))
                    {
                        device.diffPercent = 100;
                        Helpers.ConsolePrint($"BusID {device.Device.BusID}", $"({ device.Device.GetFullName()} run disabled algo");
                    }
                    */

                    Helpers.ConsolePrint($"BusID {device.Device.BusID}", $"({ device.Device.GetFullName()}):\n" +
                        $"CURRENT ALGO:\t\t{device.GetCurrentProfitableString()} ({device.DeviceCurrentMiningCoin}) " +//!неправильно
                        $"\tPROFIT: {currentProfitStrWithoutPower} with pwr: {currentProfitStrWithPower}\n" +
                        $"MOST PROFIT ALGO:\t{device.GetMostProfitableString()} ({device.DeviceMostProfitableCoin}) " +
                        $"\tPROFIT: {mostProfitStrWithoutPower} with pwr: {mostProfitStrWithPower}. {(device.diffPercent).ToString("F2")} %");
                    
                    if (!device.DeviceCurrentMiningCoin.ToLower().Equals("none") &&
                        !device.DeviceMostProfitableCoin.ToLower().Equals("none") &&
                        !device.DeviceCurrentMiningCoin.ToLower().Equals(device.DeviceMostProfitableCoin.ToLower()))//coin switch
                    {
                        coinChanged = true;
                    }

                    device.CoinChange = device.DeviceCurrentMiningCoin + "->" + device.DeviceMostProfitableCoin;

                    foreach (var _device in _ComputeDevices)
                    {
                        if (device.Device.BusID == _device.BusID)
                        {
                            _device._DeviceCurrentMiningCoin = device.DeviceCurrentMiningCoin;
                            _device._DeviceMostProfitableCoin = device.DeviceMostProfitableCoin;
                            _device._CurrentProfitableAlgorithmType = device.CurrentProfitableAlgorithmType;
                            _device._MostProfitableAlgorithmType = device.MostProfitableAlgorithmType;
                            _device._CurrentProfitableMinerBaseType = device.CurrentProfitableMinerBaseType;
                            _device._MostProfitableMinerBaseType = device.MostProfitableMinerBaseType;
                            _device._diffPercent = device.diffPercent;
                        }
                    }
                    

                    /*
                    if (_needswitch)
                    {
                        Helpers.ConsolePrint("MiningSession", "Need switch " + ((AlgorithmType)device.Device.AlgorithmID).ToString() + " " +
                            device.DeviceCurrentMiningCoin + " -> " + device.DeviceMostProfitableCoin +
                            " because diff is " + device.diff.ToString("F2") + " %");
                    }
                    */

                    if (device.needSwitch &&
                        !device.DeviceCurrentMiningCoin.ToLower().Equals("none") &&
                        !device.DeviceMostProfitableCoin.ToLower().Equals("none"))//coin switch
                    {
                        needSwitch = true;
                        forceSwitch = true;
                        device.needSwitch = false;
                        Helpers.ConsolePrint("MiningSession", "Need switch " + ((AlgorithmType)device.Device.AlgorithmID).ToString() + " " +
                            device.DeviceCurrentMiningCoin + " -> " + device.DeviceMostProfitableCoin +
                            " because coin is inactive");
                        coinFail.Remove(device.DeviceCurrentMiningCoin);
                    }

                    if (double.IsInfinity(device.diff) ||
                        double.IsInfinity(device.diffPercent))//api bug
                    {
                        needSwitch = true;
                        forceSwitch = true;
                        device.needSwitch = false;
                        Helpers.ConsolePrint("MiningSession", "Need switch " + ((AlgorithmType)device.Device.AlgorithmID).ToString() + " " +
                            device.DeviceCurrentMiningCoin + " -> " + device.DeviceMostProfitableCoin +
                            " because API bug");
                        coinFail.Remove(device.DeviceCurrentMiningCoin);
                    }

                    /*
                    if (device.DeviceCurrentMiningCoin.ToLower().Equals(device.DeviceMostProfitableCoin.ToLower()))
                    {
                        _needswitch = false;
                        forceSwitch = false;
                    }
                    */
                }
                Form_Main.smaCount = 0;
                if (smaTmp == 0)
                {
                    if (Miner.IsRunningNew)
                    {
                        Form_Main.smaCount++;
                    }
                    else
                    {
                        Form_Main.smaCount = 0;
                    }
                    if (Form_Main.smaCount > 3)
                    {
                        Helpers.ConsolePrint("SwichMostProfitableGroupUpMethod", "Using previous SMA");
                    }

                    if (Form_Main.smaCount > 15)
                    {
                        Helpers.ConsolePrint(Tag, "SMA Error. Restart program");
                        Form_Main.MakeRestart(0);
                        return;
                    }
                }
                // check if should mine
                // Only check if profitable inside this method when getting SMA data, cheching during mining is not reliable
                if (CheckIfShouldMine(mostProfit) == false)
                {
                    foreach (var device in _miningDevices)
                    {
                        device.SetNotMining();
                    }
                    AlgorithmSwitchingManager.SmaCheckTimerOnElapsedRun = false;
                    return;
                }
                // check profit threshold

                double percDiff = 0.0d;

                bool bFormSettings = false;
                FormCollection fc = Application.OpenForms;
                foreach (Form frm in fc)
                {
                    if (frm.Name == "Form_Settings")
                    {
                        bFormSettings = true;
                        break;
                    }
                    else
                    {

                    }
                }

                bool By_profitability_of_all_devices = ConfigManager.GeneralConfig.By_profitability_of_all_devices;
                double SwitchProfitabilityThreshold = ConfigManager.GeneralConfig.SwitchProfitabilityThreshold;

                if (ConfigManager.GeneralConfig.AdaptiveAlgo)
                {
                    By_profitability_of_all_devices = true;
                    SwitchProfitabilityThreshold = 0.05;
                }

                if (By_profitability_of_all_devices)
                {
                    if (ConfigManager.GeneralConfig.with_power)
                    {
                        Helpers.ConsolePrint("Profitability of all devices with power",
                            $"** Current profit {ExchangeRateApi.ConvertBTCToNationalCurrency(currentProfit).ToString("F2")}, " +
                            $"Profit after switching {ExchangeRateApi.ConvertBTCToNationalCurrency(mostProfit).ToString("F2")}");
                    }
                    else
                    {
                        Helpers.ConsolePrint("Profitability of all devices",
                            $"** Current profit {ExchangeRateApi.ConvertBTCToNationalCurrency(currentProfit).ToString("F2")}, " +
                            $"Profit after switching {ExchangeRateApi.ConvertBTCToNationalCurrency(mostProfit).ToString("F2")}");
                    }

                    double a = Math.Max(currentProfit, mostProfit);
                    double b = Math.Min(currentProfit, mostProfit);
                    percDiff = ((a - b)) / Math.Abs(b);

                    //percDiff = Math.Abs((mostProfit - currentProfit) / 100);
                    if (currentProfit == 0)//first run
                    {
                        coinChanged = true;
                    }
                    if (!coinChanged)
                    {
                        Helpers.ConsolePrint(Tag, 
                        $"NO SWITCH. There is no more profitable coin");
                        _ticks[0] = 0;
                        return;
                    }
                    /*
                    if (percDiff > 10 && percDiff < 1000) //1000%
                    {
                        Helpers.ConsolePrint(Tag,
                        $"SWITCH DISABLED due api error. Profit diff is {Math.Round(percDiff * 100, 2):f2}%, current threshold {ConfigManager.GeneralConfig.SwitchProfitabilityThreshold * 100}%");
                        return;
                    }
                    */
                    if (double.IsNaN(percDiff))//only for testing disabled algos
                    {
                        //percDiff = 1;
                        //Helpers.ConsolePrint(Tag, "Run disabled algo");
                    }
                    if (percDiff > SwitchProfitabilityThreshold && coinChanged)
                    {
                        if (Form_Main.adaptiveRunning && !algoZero)
                        {
                            Helpers.ConsolePrint(Tag, "Switching temporary disabled because adaptive algo is running");
                            needSwitch = false;
                            // RESTORE OLD PROFITS STATE
                            foreach (var device in _miningDevices)
                            {
                                device.RestoreOldProfitsState();
                            }
                            return;
                        }

                        if (!double.IsInfinity(percDiff) && percDiff > 0)
                        {
                            _ticks[0] = (int)(_ticks[0] + percDiff);
                        }

                        if (_ticks[0] + 1 >= AlgorithmSwitchingManager._ticksForStable || forceSwitch)
                        {
                            if (prev_percDiff > percDiff + percDiff * 0.2 && !needSwitch)
                            {
                                if (!KawpowLiteForceStop)
                                {
                                    _ticks[0] = 0;
                                    needSwitch = false;
                                    Helpers.ConsolePrint(Tag,
                                        $"Switching delayed due profit down. Profit diff is " +
                                        $"{Math.Round(percDiff * 100, 2):f2}%, current threshold " +
                                        $"{SwitchProfitabilityThreshold * 100}%");
                                    foreach (var device in _miningDevices)
                                    {
                                        device.RestoreOldProfitsState();
                                    }
                                }
                            }
                            else
                            {
                                double actualProfit = 0d;
                                double localProfit = 0d;
                                foreach (var __algoProperty in Stats.Stats.algosProperty)
                                {
                                    var _algoProperty = __algoProperty.Value;
                                    actualProfit = actualProfit + _algoProperty.actualProfit;
                                    localProfit = localProfit + _algoProperty.localProfit;
                                }
                                /*
                                Helpers.ConsolePrint(Tag, $"current profit { Math.Round(actualProfit / localProfit * 100, 2):f2}% " +
                                            $"profit after switching {Math.Round(percDiff * 100, 2):f2}%");
                                */
                                if (percDiff <= 5 || double.IsInfinity(percDiff) || double.IsNaN(percDiff) || forceSwitch || coinChanged)
                                {
                                    actualProfit = 0d;
                                    localProfit = 0d;
                                    foreach (var __algoProperty in Stats.Stats.algosProperty)
                                    {
                                        var _algoProperty = __algoProperty.Value;
                                        actualProfit = actualProfit + _algoProperty.actualProfit;
                                        localProfit = localProfit + _algoProperty.localProfit;
                                    }
                                    _ticks[0] = 0;
                                    needSwitch = true;
                                    forceSwitch = true;
                                    coinChanged = true;
                                    Helpers.ConsolePrint(Tag,
                                        $"Will SWITCH. Profit diff is {Math.Round(percDiff * 100, 2):f2}%, current threshold {SwitchProfitabilityThreshold * 100}%");
                                } else
                                {
                                    _ticks[0]++;
                                    needSwitch = false;
                                    Helpers.ConsolePrint(Tag,
                                        $"Will NOT SWITCH. Profit diff is {Math.Round(percDiff * 100, 2):f2}% above 500%. Maybe API bug");

                                    // RESTORE OLD PROFITS STATE
                                    if (!KawpowLiteForceStop)
                                    {
                                        foreach (var device in _miningDevices)
                                        {
                                            device.RestoreOldProfitsState();
                                        }
                                    }
                                }
                            }

                            if (bFormSettings)
                            {
                                Helpers.ConsolePrint(Tag,
                                        "Switching delayed due dialog Settings opened");
                                needSwitch = false;
                                foreach (var device in _miningDevices)
                                {
                                    device.RestoreOldProfitsState();
                                }
                            }
                        }
                        else
                        {
                            _ticks[0]++;
                            needSwitch = false;
                            Helpers.ConsolePrint(Tag, $"Will NOT SWITCH. Profit diff is {Math.Round(percDiff * 100, 2):f2}%. Switching period has not been exceeded: " +
                                (_ticks[0] * interval / 60).ToString() + "/" + (AlgorithmSwitchingManager._ticksForStable * interval / 60).ToString() + " min");

                            // RESTORE OLD PROFITS STATE
                            if (!KawpowLiteForceStop)
                            {
                                foreach (var device in _miningDevices)
                                {
                                    device.RestoreOldProfitsState();
                                }
                            }
                        }
                    } else
                    {
                        // don't switch
                        Helpers.ConsolePrint(Tag,
                            $"{"Total rig profit"}: Will NOT SWITCH profit diff is {Math.Round(percDiff * 100, 2):f2}%, current threshold {SwitchProfitabilityThreshold * 100}%");

                        // RESTORE OLD PROFITS STATE
                        if (!KawpowLiteForceStop)
                        {
                            foreach (var device in _miningDevices)
                            {
                                device.RestoreOldProfitsState();
                            }
                        }
                    }
                }
                else
                {
                    foreach (var device in _miningDevices)
                    {
                        mostProfit = device.GetMostProfitValueWithoutPower + device.GetMostProfitValueWithoutPower * (device.diff / 100);
                        currentProfit = device.GetCurrentProfitValue;

                        if (currentProfit == 0)//first run
                        {
                            coinChanged = true;
                        }
                        //if (!coinChanged)
                        if (device.DeviceCurrentMiningCoin.Equals(device.DeviceMostProfitableCoin))
                        {
                            Helpers.ConsolePrint(Tag, $"{device.Device.GetFullName()}: NO SWITCH." +
                                $" There is no more profitable coin");
                            _ticks[device.Device.Index] = 0;
                            continue;
                        }

                        var a = Math.Max(currentProfit, mostProfit);
                        var b = Math.Min(currentProfit, mostProfit);
                        percDiff = ((a - b)) / Math.Abs(b);
                        /*
                        if (percDiff > 10 && percDiff < 1000) //1000%
                        {
                            Helpers.ConsolePrint(Tag,
                            $"SWITCH DISABLED due api error. Profit diff is {Math.Round(percDiff * 100, 2):f2}%, current threshold {ConfigManager.GeneralConfig.SwitchProfitabilityThreshold * 100}%");
                            return;
                        }
                        */
                        if (percDiff > SwitchProfitabilityThreshold &&
                            !device.DeviceCurrentMiningCoin.Equals(device.DeviceMostProfitableCoin))
                        {
                            if (Form_Main.adaptiveRunning && !algoZero)
                            {
                                Helpers.ConsolePrint(Tag, "Switching temporary disabled because adaptive algo is running");
                                needSwitch = false;
                                device.RestoreOldProfitsState();
                                continue;
                            }

                            _ticks[device.Device.Index]++;
                            _ticks[device.Device.Index] = (int)(_ticks[device.Device.Index] + percDiff);

                            if (_ticks[device.Device.Index] + 1 >= AlgorithmSwitchingManager._ticksForStable ||
                                forceSwitch)
                            {
                                if (prev_percDiff > percDiff + percDiff * 0.2 && !needSwitch)
                                {
                                    _ticks[device.Device.Index] = 0;
                                    needSwitch = false;
                                    Helpers.ConsolePrint(Tag,
                                        $"Switching delayed due profit down. Profit diff is {Math.Round(percDiff * 100, 2):f2}%, current threshold {SwitchProfitabilityThreshold * 100}%");
                                    device.RestoreOldProfitsState();
                                }
                                else
                                {
                                    if (percDiff <= 5 || double.IsInfinity(percDiff) || forceSwitch ||
                                        coinChanged)
                                    {
                                        Helpers.ConsolePrint(Tag,
                                            $"{device.Device.GetFullName()}: Will SWITCH (" +
                                            device.DeviceCurrentMiningCoin + "->" +
                                            device.DeviceMostProfitableCoin + $") profit diff is " +
                                            $"{Math.Round(percDiff * 100, 2)}%, current threshold " +
                                            $"{SwitchProfitabilityThreshold * 100}%");
                                        _ticks[device.Device.Index] = 0;
                                        needSwitch = true;
                                        forceSwitch = true;
                                    }
                                    else
                                    {
                                        _ticks[device.Device.Index] = 0;
                                        needSwitch = false;
                                        Helpers.ConsolePrint(Tag,
                                            $"Will NOT SWITCH. Profit diff is {Math.Round(percDiff * 100, 2):f2}% above 500%. Maybe API bug");

                                        // RESTORE OLD PROFITS STATE
                                        device.RestoreOldProfitsState();
                                    }
                                }

                                if (bFormSettings)
                                {
                                    Helpers.ConsolePrint(Tag,
                                            "Switching delayed due dialog Settings opened");
                                    needSwitch = false;
                                    device.RestoreOldProfitsState();
                                }
                            }
                            else
                            {
                                needSwitch = false;
                                if (!KawpowLiteForceStop)
                                {
                                    Helpers.ConsolePrint(Tag, $"{device.Device.GetFullName()}: Will NOT SWITCH. Profit diff is {Math.Round(percDiff * 100, 2):f2}%. Switching period has not been exceeded: " +
                                    (_ticks[device.Device.Index] * interval / 60).ToString() + "/" + (AlgorithmSwitchingManager._ticksForStable * interval / 60).ToString() + " min");

                                    //удалить устройства, для которых не нужно переключение не получится,
                                    //если порог переключения (percDiff) превышен, а время(_ticks) нет,
                                    //то всё-равно переключится,
                                    //т.к. переключалка(NewGrouping(profitableDevices) не знает о _ticks
                                    //в реальности это выглядит так - если одному GPU надо переключиться
                                    //на другой алгоритм (превышен порог(percDiff) и время(_ticks)),
                                    //то это переключение влияет на другие GPU, у которых также превышен
                                    //порог переключения, но не (_ticks).
                                    //Переключалка(NewGrouping(profitableDevices) формирует новые группы карт
                                    //Менять этот атавизм, оставшийся от nicehash я не буду!
                                    var itemToRemove = profitableDevices.SingleOrDefault(r => r.Device.BusID == device.Device.BusID);
                                    if (itemToRemove != null)
                                    {
                                        device.RestoreOldProfitsState();
                                        if (device.HasProfitableAlgo())
                                        {
                                            profitableDevices.Remove(itemToRemove);
                                            profitableDevices.Add(device.GetMostProfitablePair());
                                        }
                                    }
                                }
                            }
                        } else
                        {
                            // don't switch
                            Helpers.ConsolePrint(Tag,
                                $"{device.Device.GetFullName()}: Will NOT SWITCH profit diff less - {percDiff * 100:f2}%, current threshold {SwitchProfitabilityThreshold * 100}%");

                            //удалить устройства, для которых не нужно переключение
                            var itemToRemove = profitableDevices.SingleOrDefault(r => r.Device.BusID == device.Device.BusID);
                            if (itemToRemove != null)
                            {
                                device.RestoreOldProfitsState();
                                if (device.HasProfitableAlgo())
                                {
                                    profitableDevices.Remove(itemToRemove);
                                    profitableDevices.Add(device.GetMostProfitablePair());
                                }
                            }

                            // RESTORE OLD PROFITS STATE
                            if (!KawpowLiteForceStop)
                            {
                                device.RestoreOldProfitsState();
                            }
                        }
                    }
                }
                prev_percDiff = percDiff;
                Form_Main._NeedMiningStart = false;
                //forceSwitch = false;

                if (algoZero)
                {
                    Helpers.ConsolePrint(Tag, "Force switch from zero or deleted algo " + algoZeroS);
                    needSwitch = true;
                }

                if (!needSwitch && !forceSwitch)
                {
                    AlgorithmSwitchingManager.SmaCheckTimerOnElapsedRun = false;
                    return;
                }

                //чтоб после переключения не было еще одного переключения
                if (By_profitability_of_all_devices)
                {
                    _ticks[0] = 0;
                }
                else
                {
                    foreach (var device in _miningDevices)
                    {
                        _ticks[device.Device.Index] = 0;
                    }
                }

                if (ConfigManager.GeneralConfig.AdaptiveAlgo)
                {
                    foreach (var pd in Enumerable.Reverse(profitableDevices).ToList())
                    {
                        if (coinFail.Contains(pd.Algorithm.CurrentMiningCoin) &&
                                pd.Algorithm.CurrentMiningCoin != pd.Algorithm.MostProfitCoin &&
                                pd.Device._diffPercent < cointreshold)
                        {
                            string effort = "";
                            var c = Stats.Stats.CoinList.Find(e => e.symbol.Equals(pd.Algorithm.CurrentMiningCoin));
                            if (c is object && c != null)
                            {
                                effort = " effort " + c.effort.ToString() + "%";
                            }
                            Helpers.ConsolePrint(Tag, $"GPU{pd.Device.ID} {pd.Device.Name} Switching canceled. " +
                            pd.Algorithm.CurrentMiningCoin + " block not found" +
                            effort);

                            foreach (var _device in _miningDevices)
                            {
                                if (_device.Device.BusID == pd.Device.BusID)
                                {
                                    var itemToRemove = profitableDevices.SingleOrDefault(r => r.Device.BusID == pd.Device.BusID);
                                    if (itemToRemove != null)
                                    {
                                        _device.RestoreOldProfitsState();
                                        if (_device.HasProfitableAlgo())
                                        {
                                            profitableDevices.Remove(itemToRemove);
                                            profitableDevices.Add(_device.GetCurrentProfitablePair());
                                        }

                                        _device.GetCurrentProfitablePair().Device._CurrentProfitableAlgorithmType =
                                            _device.GetCurrentProfitablePair().Device._MostProfitableAlgorithmType;
                                        _device.GetCurrentProfitablePair().Device._DeviceMostProfitableCoin =
                                            _device.GetCurrentProfitablePair().Device._DeviceCurrentMiningCoin;
                                    }
                                }
                            }
                        }
                    }
                }
                Enumerable.Reverse(profitableDevices).ToList();
                NewGrouping(profitableDevices);
            } catch (Exception ex)
            {
                Helpers.ConsolePrintError("SwichMostProfitableGroupUpMethod", ex.ToString());
            }
        }

        private string coin = "none";

        private void NewGrouping(List<MiningPair> profitableDevices)
        {
            // group new miners
            var newGroupedMiningPairs = new Dictionary<string, List<MiningPair>>();

            for (var first = 0; first < profitableDevices.Count; ++first)
            {
                var firstDev = profitableDevices[first].Device;
            }
            
            // group devices with same supported algorithms
            {
                var currentGroupedDevices = new List<GroupedDevices>();
                for (var first = 0; first < profitableDevices.Count; ++first)
                {
                    var firstDev = profitableDevices[first].Device;
                    
                    // check if is in group
                    var isInGroup = currentGroupedDevices.Any(groupedDevices => groupedDevices.Contains(firstDev.Uuid));

                    // if device is not in any group create new group and check if other device should group
                    if (isInGroup == false)
                    {
                        var newGroup = new GroupedDevices();
                        var miningPairs = new List<MiningPair>()
                        {
                            profitableDevices[first]
                        };
                        newGroup.Add(firstDev.Uuid);
                        for (var second = first + 1; second < profitableDevices.Count; ++second)
                        {
                            // check if we should group
                            var firstPair = profitableDevices[first];
                            var secondPair = profitableDevices[second];
                            if (GroupingLogic.ShouldGroup(firstPair, secondPair))
                            {
                                var secondDev = profitableDevices[second].Device;
                                newGroup.Add(secondDev.Uuid);
                                miningPairs.Add(profitableDevices[second]);
                            }
                        }
                        currentGroupedDevices.Add(newGroup);
                        newGroupedMiningPairs[CalcGroupedDevicesKey(newGroup)] = miningPairs;
                    }
                }
            }

            {
                // check which groupMiners should be stopped and which ones should be started and which to keep running
                var toStopGroupMiners = new Dictionary<string, GroupMiner>();
                var toRunNewGroupMiners = new Dictionary<string, GroupMiner>();
                var noChangeGroupMiners = new Dictionary<string, GroupMiner>();
                // check what to stop/update
                foreach (var runningGroupKey in _runningGroupMiners.Keys)
                {
                    if (newGroupedMiningPairs.ContainsKey(runningGroupKey) == false)
                    {
                        // runningGroupKey not in new group definately needs to be stopped and removed from curently running
                        toStopGroupMiners[runningGroupKey] = _runningGroupMiners[runningGroupKey];
                    }
                    else
                    {
                        // runningGroupKey is contained but needs to check if mining algorithm is changed
                        var miningPairs = newGroupedMiningPairs[runningGroupKey];
                        var newAlgoType = GetMinerPairAlgorithmType(miningPairs);
                        if (newAlgoType != AlgorithmType.NONE && newAlgoType != AlgorithmType.INVALID)
                        {
                            var _coinChanged = false;
                            foreach (var mPair in _runningGroupMiners[runningGroupKey].Miner.MiningSetup.MiningPairs)
                            {
                                if (mPair.Algorithm is Algorithm algo
                                    && algo.CurrentMiningCoin != algo.MostProfitCoin)
                                {
                                    if (coinFail.Contains(algo.CurrentMiningCoin) && ConfigManager.GeneralConfig.AdaptiveAlgo)
                                    {
                                       // mPair.Algorithm.CurrentMiningCoin = mPair.Algorithm.MostProfitCoin;
                                        newAlgoType = mPair.Device._CurrentProfitableAlgorithmType;
                                        algo.MostProfitCoin = algo.CurrentMiningCoin;
                                        mPair.Algorithm.MostProfitCoin = mPair.Algorithm.CurrentMiningCoin;
                                        mPair.Device._MostProfitableAlgorithmType = mPair.Device._CurrentProfitableAlgorithmType;
                                        mPair.Device._MostProfitableMinerBaseType = mPair.Device._CurrentProfitableMinerBaseType;
                                        mPair.Device._DeviceMostProfitableCoin = mPair.Device._DeviceCurrentMiningCoin;
                                        //mPair.Algorithm.DualZergPoolID = mPair.Device._CurrentProfitableAlgorithmType;
                                    }
                                    else
                                    {
                                        _coinChanged = true;
                                        break;
                                    }
                                }
                            }

                            // if algoType valid and different from currently running update
                            if (newAlgoType != _runningGroupMiners[runningGroupKey].DualAlgorithmType || _coinChanged)
                            {
                                // remove current one and schedule to stop mining
                                toStopGroupMiners[runningGroupKey] = _runningGroupMiners[runningGroupKey];

                                // create new one TODO check if DaggerHashimoto
                                GroupMiner newGroupMiner = null;

                                if (newGroupMiner == null)
                                {
                                    newGroupMiner = new GroupMiner(miningPairs, runningGroupKey);
                                }

                                toRunNewGroupMiners[runningGroupKey] = newGroupMiner;
                            }
                            else
                            {
                                noChangeGroupMiners[runningGroupKey] = _runningGroupMiners[runningGroupKey];
                            }
                        }
                    }
                }

                coinChanged = false;

                // check brand new
                foreach (var kvp in newGroupedMiningPairs)
                {
                    var key = kvp.Key;
                    var miningPairs = kvp.Value;
                    if (_runningGroupMiners.ContainsKey(key) == false)
                    {
                        var newGroupMiner = new GroupMiner(miningPairs, key);
                        toRunNewGroupMiners[key] = newGroupMiner;
                    }
                }

                if ((toStopGroupMiners.Values.Count > 0) || (toRunNewGroupMiners.Values.Count > 0))
                {
                    var stringBuilderPreviousAlgo = new StringBuilder();
                    var stringBuilderCurrentAlgo = new StringBuilder();
                    var stringBuilderNoChangeAlgo = new StringBuilder();

                    Form_Main.SwitchCount++;
                    Helpers.ConsolePrint("SWITCHING", "Number of switches: " +
                        Form_Main.SwitchCount.ToString() + " Uptime: " +
                        Form_Main.Uptime.ToString(@"d\ \d\a\y\s\ hh\:mm\:ss"));

                    // stop old miners
                    foreach (var toStop in toStopGroupMiners.Values)
                    {
                        stringBuilderPreviousAlgo.Append($"{toStop.DevicesInfoString}: {toStop.AlgorithmType}({toStop.Coin}), ");
                        toStop.Stop();
                        toStop.StartMinerTime = new DateTime(0);
                        _runningGroupMiners.Remove(toStop.Key);
                    }
                    // start new miners
                    foreach (var toStart in toRunNewGroupMiners.Values)
                    {
                        foreach (var device in _miningDevices)
                        {
                            device.Device.coinMiningTime = 0;
                            device.DeviceCurrentMiningCoin = toStart.Coin;
                            foreach (var algo in device.Algorithms)
                            {
                                if (algo.ZergPoolID == device.MostProfitableAlgorithmType ||
                                    algo.SecondaryZergPoolID == device.MostProfitableAlgorithmType)
                                {
                                    algo.CurrentMiningCoin = toStart.Coin;
                                }
                            }
                        }

                        toStart.StartMinerTime = DateTime.Now;
                        stringBuilderCurrentAlgo.Append($"{toStart.DevicesInfoString}: {toStart.AlgorithmType}({toStart.Coin}) : {toStart.DualAlgorithmType}, ");

                        if (stringBuilderPreviousAlgo.Length > 0)
                            Helpers.ConsolePrint(Tag, $"Stop Mining: {stringBuilderPreviousAlgo}");

                        if (stringBuilderCurrentAlgo.Length > 0)
                            Helpers.ConsolePrint(Tag, $"Now Mining : {stringBuilderCurrentAlgo}");

                        if (stringBuilderNoChangeAlgo.Length > 0)
                            Helpers.ConsolePrint(Tag, $"No change  : {stringBuilderNoChangeAlgo}");

                        _password = _password.Replace("mc=" + coin, "mc=" + toStart.Coin);
                        //_password = _password.Replace("mc=" + coin, "mc=KIIRO");//test
                        coin = toStart.Coin;
                        if (ConfigManager.GeneralConfig.ServiceLocation == 0)
                        {
                            toStart.Start(_wallet, _password);
                        }
                        else
                        {
                            toStart.Start(_wallet, _password);
                        }
                        _runningGroupMiners[toStart.Key] = toStart;
                    }
                    // which miners dosen't change
                    foreach (var noChange in noChangeGroupMiners.Values)
                        stringBuilderNoChangeAlgo.Append($"{noChange.DevicesInfoString}: {noChange.AlgorithmType}({noChange.Coin}), ");
                }
            }
            AlgorithmSwitchingManager.SmaCheckTimerOnElapsedRun = false;
            _mainFormRatesComunication?.ForceMinerStatsUpdate();
        }


        private AlgorithmType GetMinerPairAlgorithmType(List<MiningPair> miningPairs)
        {
            if (miningPairs.Count > 0)
            {
                return miningPairs[0].Algorithm.DualZergPoolID;
            }

            return AlgorithmType.NONE;
        }

        private void GMinersRestart(List<Miner> _checks)
        {
            foreach (Miner m in _checks)
            {
                try
                {
                    if (m.needChildRestart)
                    {
                        Thread.Sleep(6000);
                        Helpers.ConsolePrint(m.MinerTag(), "Restart gminer process after ZIL round");
                        Process proc = Process.GetProcessById(m.ChildProcess());
                        if (proc != new Process()) proc.Kill();
                    }
                }
                catch (ArgumentException)
                {
                    // Process already exited.
                }
            }
            _checks.Clear();
        }

        public async Task MinerStatsCheck()
        {
            var currentProfit = 0.0d;
            _mainFormRatesComunication.ClearRates(_runningGroupMiners.Count);
            var checks = new List<GroupMiner>(_runningGroupMiners.Values);
            var _checks = new List<Miner>();
            try
            {
                DevicesCoinList.Clear();
                foreach (var groupMiners in checks)
                {
                    Coin c = new();
                    c._Algorithm = groupMiners.AlgorithmType;
                    c._DualAlgorithm = groupMiners.DualAlgorithmType;
                    c._Coin = groupMiners.Coin;
                    DevicesCoinList.Add(c);

                    Miner m = groupMiners.Miner;

                    if (!m.IsRunning || m.IsUpdatingApi || m == null) continue;

                    m.IsUpdatingApi = true;
                    try
                    {
                        new Task(() => m.GetSummaryAsync()).Start();
                    }
                    catch (NullReferenceException ex)
                    {
                        Helpers.ConsolePrintError("MinerStatsCheck", ex.ToString());
                    }

                    var ad = m.GetApiData();
                    m.IsUpdatingApi = false;
                    // set rates
                    if (ad != null)
                    {
                        /*
                        Form_Main.RateNoZil = _zil.RateNoZil;
                        Form_Main.RateZil = _zil.RateZil;
                        if (ad.ZilRound)
                        {
                            if (ad.SecondaryAlgorithmID != AlgorithmType.NONE)//single
                            {
                                AlgosProfitData.TryGetPaying(AlgorithmType.ZIL, out var secPaying);
                                if (ConfigManager.GeneralConfig.ZIL_mining_state != 1) secPaying = 0;
                                groupMiners.CurrentRate = secPaying * ad.SecondarySpeed * Algorithm.Mult;
                            }
                            if (ad.ThirdAlgorithmID != AlgorithmType.NONE)//dual
                            {
                                AlgosProfitData.TryGetPaying(AlgorithmType.ZIL, out var thirdPaying);
                                if (ConfigManager.GeneralConfig.ZIL_mining_state != 1) thirdPaying = 0;
                                groupMiners.CurrentRate = thirdPaying * ad.ThirdSpeed * Algorithm.Mult;
                            }
                            if (Form_additional_mining.isAlgoZIL(ad.AlgorithmName, groupMiners.MinerBaseType, groupMiners.DeviceType))
                            {
                                Form_Main.RateZil += groupMiners.CurrentRate;
                                _zil.RateZilCount++;
                            }
                        }
                        else
                        */
                        {
                            if (ad.AlgorithmID != AlgorithmType.NONE)
                            {
                                AlgosProfitData.TryGetPaying(ad.AlgorithmID, out var paying);
                                groupMiners.CurrentRate = paying.currentProfit * ad.Speed * Algorithm.Mult;
                                //отображение прибыльности текущей монеты
                                var coin = Stats.Stats.CoinList.Find(e => e.symbol.Equals(groupMiners.Coin));
                                if (coin is object && coin != null)
                                {
                                    if (ad.AlgorithmID.ToString().ToLower().Equals(coin.algo.ToLower()))
                                    {
                                        if (ConfigManager.GeneralConfig.AdaptiveAlgo)
                                        {
                                            groupMiners.CurrentRate = coin.adaptive_profit * ad.Speed * Algorithm.Mult;
                                        }
                                        else
                                        {
                                            groupMiners.CurrentRate = coin.profit * ad.Speed * Algorithm.Mult;
                                        }
                                    }
                                }
                            }

                            if (ad.SecondaryAlgorithmID != AlgorithmType.NONE)
                            {
                                AlgosProfitData.TryGetPaying(ad.SecondaryAlgorithmID, out var secPaying);
                                groupMiners.CurrentRate += secPaying.currentProfit * ad.SecondarySpeed * Algorithm.Mult;
                                var coin = Stats.Stats.CoinList.Find(e => e.symbol.Equals(groupMiners.Coin));
                                if (coin is object && coin != null)
                                {
                                    if (ad.SecondaryAlgorithmID.ToString().ToLower().Equals(coin.algo.ToLower()))
                                    {
                                        if (ConfigManager.GeneralConfig.AdaptiveAlgo)
                                        {
                                            groupMiners.CurrentRate += coin.adaptive_profit * ad.SecondarySpeed * Algorithm.Mult;
                                        }
                                        else
                                        {
                                            groupMiners.CurrentRate += coin.profit * ad.SecondarySpeed * Algorithm.Mult;
                                        }
                                    }
                                }
                            }
                            if (ad.ThirdAlgorithmID != AlgorithmType.NONE)
                            {
                                AlgosProfitData.TryGetPaying(ad.ThirdAlgorithmID, out var thirdPaying);
                                groupMiners.CurrentRate += thirdPaying.currentProfit * ad.ThirdSpeed * Algorithm.Mult;
                                var coin = Stats.Stats.CoinList.Find(e => e.symbol.Equals(groupMiners.Coin));
                                if (coin is object && coin != null)
                                {
                                    if (ad.ThirdAlgorithmID.ToString().ToLower().Equals(coin.algo.ToLower()))
                                    {
                                        if (ConfigManager.GeneralConfig.AdaptiveAlgo)
                                        {
                                            groupMiners.CurrentRate += coin.adaptive_profit * ad.ThirdSpeed * Algorithm.Mult;
                                        }
                                        else
                                        {
                                            groupMiners.CurrentRate += coin.profit * ad.ThirdSpeed * Algorithm.Mult;
                                        }
                                    }
                                }
                            }
                            /*
                            if (Form_additional_mining.isAlgoZIL(ad.AlgorithmName, groupMiners.MinerBaseType, groupMiners.DeviceType))
                            {
                                Form_Main.RateNoZil += groupMiners.CurrentRate;
                                _zil.RateNoZilCount++;

                                if (_zil.RateNoZilCount >= 100000)
                                {
                                    _zil.RateNoZilCount = _zil.RateNoZilCount / 20;
                                    _zil.RateZilCount = _zil.RateZilCount / 20;
                                    _zil.RateNoZil = _zil.RateNoZil / 20;
                                    _zil.RateZil = _zil.RateZil / 20;
                                }
                            }
                            */
                        }
                        /*
                        Form_Main.ZilFactor = _zil.ZilFactor;

                        if (Form_Main.RateZil * 100 < Form_Main.RateNoZil)//fix overprice from api
                        {
                            Form_Main.RateNoZil = Form_Main.RateNoZil / 100;
                        }

                        _zil.RateNoZil = Form_Main.RateNoZil;
                        _zil.RateZil = Form_Main.RateZil;

                        double _RateNoZil = Form_Main.RateNoZil / _zil.RateNoZilCount;//0.0067
                        double _RateZil = Form_Main.RateZil   / _zil.RateZilCount;

                        if (_zil.RateNoZilCount != 0)
                        {
                            _zil.ZilRatio = (double)((double)_zil.RateZilCount / (double)_zil.RateNoZilCount);
                        }
                        double _zilRatio = _zil.ZilRatio * 0.75;

                        Form_Main.ZilFactor = Math.Round((_RateZil * _zilRatio) / _RateNoZil, 3);

                        if (Form_Main.ZilFactor > 0.1)
                        {
                            _RateZil = _RateZil * 0.5;
                            Form_Main.RateZil = Form_Main.RateZil * 0.5;
                            Form_Main.ZilFactor = Form_Main.ZilFactor * 0.5;
                            _zil.RateZil = Form_Main.RateZil;
                        }
                        if (Form_Main.ZilFactor < 0.01)
                        {
                            _RateZil = _RateZil * 2;
                            Form_Main.RateZil = Form_Main.RateZil * 2;
                            Form_Main.ZilFactor = Form_Main.ZilFactor * 2;
                            _zil.RateZil = Form_Main.RateZil;
                        }
                        _zil.ZilFactor = Form_Main.ZilFactor;

                        if (double.IsNaN(Form_Main.ZilFactor)) Form_Main.ZilFactor = 0.0d;
                        if (double.IsNaN(_RateZil)) _RateZil = 0.0d;
                        if (double.IsNaN(_RateNoZil)) _RateZil = 0.0d;
                        if (Form_additional_mining.isAlgoZIL(ad.AlgorithmName, groupMiners.MinerBaseType, groupMiners.DeviceType))
                        {
                            groupMiners.CurrentRate += groupMiners.CurrentRate * Form_Main.ZilFactor;
                        }
                        */
                        double TotalPower = MinersManager.GetTotalPowerRate(); 
                        double psuE = (double)ConfigManager.GeneralConfig.PowerPSU / 100;
                        double totalPowerUsage = (TotalPower + (int)ConfigManager.GeneralConfig.PowerMB) / psuE;
                        groupMiners.PowerRate = ExchangeRateApi.GetKwhPriceInBtc() * totalPowerUsage * 24 / 1000;//Вт
                    }
                    else
                    {
                        if (groupMiners.AlgorithmType != AlgorithmType.KawPowLite)
                        {
                            groupMiners.CurrentRate = 0;
                        }
                        ad = new ApiData(groupMiners.DualAlgorithmType);
                    }
                    currentProfit += groupMiners.CurrentRate;
                    // Update GUI
                    _mainFormRatesComunication.AddRateInfo(m.MinerTag(), ad,
                        m.IsApiReadException, m.ProcessTag(), groupMiners, checks.Count);
                }
            }
            catch (Exception e)
            {
                Helpers.ConsolePrintError("Exception: ", e.ToString());
            }
            if (Form_Main.needGMinerRestart)
            {
                Form_Main.needGMinerRestart = false;
                new Task(() => GMinersRestart(_checks)).Start();
            }
            /*
            try
            {
                var s = JsonConvert.SerializeObject(_zil, Formatting.Indented);
                Helpers.WriteAllTextWithBackup("configs\\zil.json", s);
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrintError("MinerStatsCheck", ex.ToString());
            }
            */
            GC.Collect();
        }
        private class Zil
        {
            public double RateZil;
            public int RateZilCount;
            public double RateNoZil;
            public int RateNoZilCount;
            public double ZilRatio;
            public double ZilFactor;
        }

    }
}
