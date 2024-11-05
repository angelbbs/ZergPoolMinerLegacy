using ZergPoolMiner.Configs;
using ZergPoolMiner.Devices;
using ZergPoolMiner.Forms;
using ZergPoolMiner.Forms.Components;
using ZergPoolMiner.Interfaces;
using ZergPoolMiner.Miners;
using ZergPoolMiner.Miners.Grouping;
using ZergPoolMiner.Stats;
using ZergPoolMiner.Switching;
using ZergPoolMiner.Utils;
using ZergPoolMinerLegacy.Common.Enums;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Threading;
using System.Windows.Forms;
using SystemTimer = System.Timers.Timer;
using Timer = System.Windows.Forms.Timer;

namespace ZergPoolMiner
{
    using LibreHardwareMonitor.Hardware;
    using Microsoft.Win32;
    using Newtonsoft.Json;
    using ZergPoolMiner.Devices.Querying;
    using ZergPoolMiner.Miners.Grouping;
    using ZergPoolMinerLegacy.Overclock;
    //using OpenHardwareMonitor.Hardware;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing.Imaging;
    using System.IO;
    using System.IO.MemoryMappedFiles;
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using static ZergPoolMiner.Devices.ComputeDeviceManager;
    using static ZergPoolMiner.Devices.ComputeDeviceManager.Query;
    using static ZergPoolMiner.Miners.MinerVersion;
    using Newtonsoft.Json.Linq;
    using System.Collections.Concurrent;
    using System.Drawing.Drawing2D;
    using ZergPoolMiner.Algorithms;

    public partial class Form_Main : Form, Form_Loading.IAfterInitializationCaller, IMainFormRatesComunication
    {
        public static string platform = "ZergPool";
        public static string version = "";
        public Timer _minerStatsCheck;
        public Timer _deviceTelemetryTimer;
        private Timer _startupTimer;
        public static Timer _autostartTimer;
        public static Timer _autostartTimerDelay;
        public static Timer _deviceStatusTimer;
        private Timer _getBalanceTimer;
        private Timer _updateSMATimer;
        private Timer _finalizeSMATimer;
        private Timer _marketsTimer;
        private Timer _chartTimer;
        private int _updateTimerCount;
        private int _updateTimerRestartProgramCount;
        private int _AutoStartMiningDelay = 0;
        private Timer _idleCheck;
        private SystemTimer _computeDevicesCheckTimer;
        public static bool needRestart = false;
        public static bool ShouldRunEthlargement = false;

        public static bool _demoMode;

        public static string _PayoutTreshold = ",pl=0";
        private readonly Random R;

        private Form_Loading _loadingScreen;
        private Form_Benchmark _benchmarkForm;

        private int _flowLayoutPanelVisibleCount = 0;
        public static int _flowLayoutPanelRatesIndex = 0;

        private const string BetaAlphaPostfixString = "";
        const string ForkString = " Fork Fix ";

        private bool _isDeviceDetectionInitialized = false;

        public static bool _isManuallyStarted = false;
        public static bool _NeedMiningStart = false;
        private bool _isNotProfitable = false;

        private Process mainproc = Process.GetCurrentProcess();
        public static double _factorTimeUnit = 1.0;
        public static int nanominerCount = 0;
        private int _mainFormHeight = 0;
        private readonly int _emtpyGroupPanelHeight = 0;
        private int groupBox1Top = 0;
        public static bool firstRun = false;
        public static Color _backColor;
        public static Color _foreColor;
        public static Color _windowColor;
        public static Color _textColor;
        public static double githubBuild = 0.0d;
        public static double gitlabBuild = 0.0d;
        public static double currentBuild = 0.0d;
        public static double currentVersion = 0.0d;
        public static double githubVersion = 0.0d;
        public static double gitlabVersion = 0.0d;
        public static string progName = "";
        public static string browser_download_url = "";
        public static string miners_url = "";
        public static string BackupFileName = "";
        public static string BackupFileDate = "";
        public static bool NewVersionExist = false;
        public static bool CertInstalled = false;
        public static bool LiteAlgos = false;
        public static bool KawpowLite = false;
        public static bool KawpowLiteVisible = false;
        public static bool KawpowLiteEnabled = false;
        public static bool KawpowLite3GB = false;
        public static bool KawpowLite4GB = false;
        public static bool KawpowLite5GB = false;
        public static bool SomeAlgoEnabled = false;
        public static bool DaggerHashimotoMaxEpochUpdated = false;
        public static string GoogleIP = "";
        public static string GoogleAnswer = "";
        public static bool GoogleAvailable = false;
        internal static bool DeviceStatusTimer_FirstTick = false;
        public static Computer thisComputer;
        public static DateTime StartTime = new DateTime();
        public static TimeSpan Uptime;
        private static bool CheckVideoControllersCount = false;
        public static bool AntivirusInstalled = false;
        public static int smaCount = 0;
        private static int ticks = 0;//костыль
        public static double profitabilityFromNH = 0.0d;
        public static double TotalActualProfitability = 0.0d;
        public static List<RigProfitList> RigProfits = new List<Form_Main.RigProfitList>();
        public static RigProfitList lastRigProfit = new Form_Main.RigProfitList();
        public static bool Form_RigProfitChartRunning = false;
        public static bool FormMainMoved = false;
        public static bool MSIAfterburnerAvailabled = false;
        public static bool MSIAfterburnerRunning = false;
        public static bool OverclockEnabled = false;
        public static bool NVIDIA_orderBug = false;
        public static string NVIDIADriver;
        public static bool MiningStarted = false;
        public static int devCur = 0;
        public static double PowerAllDevices = 0;
        public static bool ProgramClosing = false;
        public static Form_Settings settings;// = new Form_Settings();
        public static double totalPowerConsumptionBTC = 0.0d;
        public static double totalPowerRatePayoutCurrency = 0.0d;
        public static double totalPowerRateNational = 0.0d;
        public static double TotalPowerConsumption;
        public static double TotalBTC;
        public static double TotalActualBTC;
        private static ToolTip toolTipStatus = new ToolTip();
        public static bool InBenchmark = false;
        public static bool NHConnectingInProgress = false;
        public static bool DownloadingInProgress = false;
        public static string orgId;
        public static string apiKey;
        public static string apiSecret;
        public static string walletType = "";
        public static string errorAPIkeystring;
        public static bool API_key_validity = false;
        public static bool checkBox_EnableAPI = false;
        public static bool NvAPIerror = false;
        public static Proxy.ProxyDetail[] _ProxyList;
        public static string[] _proxyUrls = { };
        public static int wssConnectionsErrors = 0;
        public static int apiConnectionsErrors = 0;
        public static int TotalConnectionsErrors = 0;
        public static byte[] desktop = new byte[0];
        public static int SwitchCount = 0;
        public static bool ZilMonitorRunning = false;
        public static bool ZilMonitorNicehashRunning = false;
        public static bool isZilRound = false;
        public static bool isForceZilRound = false;
        public static double RateZil = 0.0d;
        public static int RateZilCount = 0;
        public static double RateNoZil = 0.0d;
        public static int RateNoZilCount = 0;
        public static double ZilFactor = 0.04d;
        public static int ZilCount = -1;
        public static bool needGMinerRestart = false;
        public static bool adaptiveRunning = false;
        public static int ZIL_mining_state = 0;

        public MemoryMappedFile MonitorSharedMemory = MemoryMappedFile.CreateOrOpen("MinerLegacyForkFixMonitor", 100);

        //**
        public static string[] ZoneSchedule1 = { "00:00", "23:59", "0.00" };
        public static string[] ZoneSchedule2 = { "07:00", "22:59", "0.00", "23:00", "06:59", "0.00" };
        public static string[] ZoneSchedule3 = { "23:00", "06:59", "0.00", "07:00", "08:59", "0.00", "09:00", "16:59", "0.00", "17:00", "19:59", "0.00", "20:00", "22:59", "0.00" };
        public struct RigProfitList
        {
            public DateTime DateTime;
            public double totalRate;
            public double currentProfit;
            public double currentProfitAPI;
            public double currentPower;
            public double totalPowerRate;
            public double unpaidAmount;
            public bool Success;
            public string Message;
        }
        public static double ChartDataAvail = 0;
        public static int MemoryMappedFileError = 0;
        public static int NVMLDriverError = 0;

        public static List<NvData> gpuList = new List<NvData>();
        [Serializable]
        public struct NvData
        {
            public uint nGpu;
            public uint power;
            public uint fan;
            public uint load;
            public uint loadMem;
            public uint temp;
            public uint tempMem;
        }

        public static List<Region> regionList = new List<Region>();
        [Serializable]
        public struct Region
        {
            public string RegionName;
            public string RegionLocation;
        }

        List<Control> shadowControls = new List<Control>();
        Bitmap shadowBmp = null;
        public Form_Main()
        {
            if (this != null)
            {
                Rectangle screenSize = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                //if (ConfigManager.GeneralConfig.FormLeft + ConfigManager.GeneralConfig.FormWidth <= screenSize.Size.Width)
                {
                    if (ConfigManager.GeneralConfig.FormTop + ConfigManager.GeneralConfig.FormLeft >= 1)
                    {
                        this.Top = ConfigManager.GeneralConfig.FormTop;
                        this.Left = ConfigManager.GeneralConfig.FormLeft;
                    }

                    this.Width = ConfigManager.GeneralConfig.FormWidth;
                    //this.Height = ConfigManager.GeneralConfig.FormHeight;
                    this.Height = this.MinimumSize.Height + ConfigManager.GeneralConfig.DevicesCountIndex * 17 + 1;
                }
                /*
                else
                {
                    // this.Width = 660; // min width
                }
                */
            }

            //WindowState = FormWindowState.Minimized;
            Helpers.ConsolePrint("ZergPool", "Start Form_Main");
            switch (ConfigManager.GeneralConfig.ColorProfileIndex)
            {
                case 0: //default
                    _backColor = ConfigManager.GeneralConfig.ColorProfiles.DefaultColor[0];
                    _foreColor = ConfigManager.GeneralConfig.ColorProfiles.DefaultColor[1];
                    _windowColor = ConfigManager.GeneralConfig.ColorProfiles.DefaultColor[2];
                    _textColor = ConfigManager.GeneralConfig.ColorProfiles.DefaultColor[3];
                    break;
                case 1: //gray
                    _backColor = ConfigManager.GeneralConfig.ColorProfiles.Gray[0];
                    _foreColor = ConfigManager.GeneralConfig.ColorProfiles.Gray[1];
                    _windowColor = ConfigManager.GeneralConfig.ColorProfiles.Gray[2];
                    _textColor = ConfigManager.GeneralConfig.ColorProfiles.Gray[3];
                    break;
                case 2: //dark
                    _backColor = ConfigManager.GeneralConfig.ColorProfiles.Dark[0];
                    _foreColor = ConfigManager.GeneralConfig.ColorProfiles.Dark[1];
                    _windowColor = ConfigManager.GeneralConfig.ColorProfiles.Dark[2];
                    _textColor = ConfigManager.GeneralConfig.ColorProfiles.Dark[3];
                    break;
                case 3: //black
                    _backColor = ConfigManager.GeneralConfig.ColorProfiles.Black[0];
                    _foreColor = ConfigManager.GeneralConfig.ColorProfiles.Black[1];
                    _windowColor = ConfigManager.GeneralConfig.ColorProfiles.Black[2];
                    _textColor = ConfigManager.GeneralConfig.ColorProfiles.Black[3];
                    break;
                case 4: //silver
                    _backColor = ConfigManager.GeneralConfig.ColorProfiles.Silver[0];
                    _foreColor = ConfigManager.GeneralConfig.ColorProfiles.Silver[1];
                    _windowColor = ConfigManager.GeneralConfig.ColorProfiles.Silver[2];
                    _textColor = ConfigManager.GeneralConfig.ColorProfiles.Silver[3];
                    break;
                case 5: //gold
                    _backColor = ConfigManager.GeneralConfig.ColorProfiles.Gold[0];
                    _foreColor = ConfigManager.GeneralConfig.ColorProfiles.Gold[1];
                    _windowColor = ConfigManager.GeneralConfig.ColorProfiles.Gold[2];
                    _textColor = ConfigManager.GeneralConfig.ColorProfiles.Gold[3];
                    break;
                case 6: //darkred
                    _backColor = ConfigManager.GeneralConfig.ColorProfiles.DarkRed[0];
                    _foreColor = ConfigManager.GeneralConfig.ColorProfiles.DarkRed[1];
                    _windowColor = ConfigManager.GeneralConfig.ColorProfiles.DarkRed[2];
                    _textColor = ConfigManager.GeneralConfig.ColorProfiles.DarkRed[3];
                    break;
                case 7: //darkgreen
                    _backColor = ConfigManager.GeneralConfig.ColorProfiles.DarkGreen[0];
                    _foreColor = ConfigManager.GeneralConfig.ColorProfiles.DarkGreen[1];
                    _windowColor = ConfigManager.GeneralConfig.ColorProfiles.DarkGreen[2];
                    _textColor = ConfigManager.GeneralConfig.ColorProfiles.DarkGreen[3];
                    break;
                case 8: //darkblue
                    _backColor = ConfigManager.GeneralConfig.ColorProfiles.DarkBlue[0];
                    _foreColor = ConfigManager.GeneralConfig.ColorProfiles.DarkBlue[1];
                    _windowColor = ConfigManager.GeneralConfig.ColorProfiles.DarkBlue[2];
                    _textColor = ConfigManager.GeneralConfig.ColorProfiles.DarkBlue[3];
                    break;
                case 9: //magenta
                    _backColor = ConfigManager.GeneralConfig.ColorProfiles.DarkMagenta[0];
                    _foreColor = ConfigManager.GeneralConfig.ColorProfiles.DarkMagenta[1];
                    _windowColor = ConfigManager.GeneralConfig.ColorProfiles.DarkMagenta[2];
                    _textColor = ConfigManager.GeneralConfig.ColorProfiles.DarkMagenta[3];
                    break;
                case 10: //orange
                    _backColor = ConfigManager.GeneralConfig.ColorProfiles.DarkOrange[0];
                    _foreColor = ConfigManager.GeneralConfig.ColorProfiles.DarkOrange[1];
                    _windowColor = ConfigManager.GeneralConfig.ColorProfiles.DarkOrange[2];
                    _textColor = ConfigManager.GeneralConfig.ColorProfiles.DarkOrange[3];
                    break;
                case 11: //violet
                    _backColor = ConfigManager.GeneralConfig.ColorProfiles.DarkViolet[0];
                    _foreColor = ConfigManager.GeneralConfig.ColorProfiles.DarkViolet[1];
                    _windowColor = ConfigManager.GeneralConfig.ColorProfiles.DarkViolet[2];
                    _textColor = ConfigManager.GeneralConfig.ColorProfiles.DarkViolet[3];
                    break;
                case 12: //darkslateblue
                    _backColor = ConfigManager.GeneralConfig.ColorProfiles.DarkSlateBlue[0];
                    _foreColor = ConfigManager.GeneralConfig.ColorProfiles.DarkSlateBlue[1];
                    _windowColor = ConfigManager.GeneralConfig.ColorProfiles.DarkSlateBlue[2];
                    _textColor = ConfigManager.GeneralConfig.ColorProfiles.DarkSlateBlue[3];
                    break;
                case 13: //tan
                    _backColor = ConfigManager.GeneralConfig.ColorProfiles.Tan[0];
                    _foreColor = ConfigManager.GeneralConfig.ColorProfiles.Tan[1];
                    _windowColor = ConfigManager.GeneralConfig.ColorProfiles.Tan[2];
                    _textColor = ConfigManager.GeneralConfig.ColorProfiles.Tan[3];
                    break;
                case 14: //zergpool
                    _backColor = ConfigManager.GeneralConfig.ColorProfiles.ZergPool[0];
                    _foreColor = ConfigManager.GeneralConfig.ColorProfiles.ZergPool[1];
                    _windowColor = ConfigManager.GeneralConfig.ColorProfiles.ZergPool[2];
                    _textColor = ConfigManager.GeneralConfig.ColorProfiles.ZergPool[3];
                    break;
                default:
                    _backColor = ConfigManager.GeneralConfig.ColorProfiles.DefaultColor[0];
                    _foreColor = ConfigManager.GeneralConfig.ColorProfiles.DefaultColor[1];
                    _windowColor = ConfigManager.GeneralConfig.ColorProfiles.DefaultColor[2];
                    _textColor = ConfigManager.GeneralConfig.ColorProfiles.DefaultColor[3];
                    break;
            }
            Helpers.ConsolePrint("ZergPool", "Start InitializeComponent");
            StartTime = DateTime.Now;
            Process thisProc = Process.GetCurrentProcess();
            thisProc.PriorityClass = ProcessPriorityClass.High;

            InitializeComponent();
            shadowControls.Add(buttonLogo);
            //buttonChart.Visible = false;
            Icon = Properties.Resources.logo;
            Helpers.ConsolePrint("ZergPool", "Start InitLocalization");

            InitLocalization();
            devicesListViewEnableControl1.Visible = false;
            ComputeDeviceManager.SystemSpecs.QueryAndLog();
            groupBox1Top = groupBox1.Top;

            devicesListViewEnableControl1.Height = 129 + ConfigManager.GeneralConfig.DevicesCountIndex * 17 + 1;
            groupBox1Top += ConfigManager.GeneralConfig.DevicesCountIndex * 17 + 1;
            //this.Height += 16;

            Helpers.ConsolePrint("ZergPool", "Windows version " + GetWinVer(Environment.OSVersion.Version) +
                " (" + Environment.OSVersion.Version.Major.ToString() + "." + Environment.OSVersion.Version.Minor.ToString() +
                " build: " + Environment.OSVersion.Version.Build.ToString() + ")");
            Helpers.ConsolePrint("ZergPool", "Start query RAM");
            comboBoxRegion.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBoxRegion.DrawItem += new DrawItemEventHandler(comboBoxLocation_DrawItem);
            // Log the computer's amount of Total RAM and Page File Size
            var moc = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem").Get();
            foreach (ManagementObject mo in moc)
            {
                var totalRam = long.Parse(mo["TotalVisibleMemorySize"].ToString()) / 1024;
                var pageFileSize = (long.Parse(mo["TotalVirtualMemorySize"].ToString()) / 1024) - totalRam;
                Helpers.ConsolePrint("ZergPool", "Total RAM: " + totalRam + "MB");
                Helpers.ConsolePrint("ZergPool", "Page File Size: " + pageFileSize + "MB");
            }

            R = new Random((int)DateTime.Now.Ticks);

            Text += ForkString;

            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            double.TryParse(version, out var d);
            int.TryParse(version, out var i);

            if (d / i == 1)
            {
                Form_Main.version = i.ToString();
                Text += i.ToString();
            }
            else
            {
                Form_Main.version = d.ToString();
                Text += d.ToString();
            }

            Text += " for " + platform;

            var internalversion = Assembly.GetExecutingAssembly().GetName().Version;
            var buildDate = new DateTime(2000, 1, 1).AddDays(internalversion.Build).AddSeconds(internalversion.Revision * 2);
            var build = buildDate.ToString("u").Replace("-", "").Replace(":", "").Replace("Z", "").Replace(" ", ".");
            Double.TryParse(build.ToString(), out Form_Main.currentBuild);
            Form_Main.currentVersion = ConfigManager.GeneralConfig.ForkFixVersion;

            label_NotProfitable.Visible = false;

            InitMainConfigGuiData();

            // for resizing
            InitFlowPanelStart();

            groupBox1.Height = 32;
            if (groupBox1.Size.Height > 0 && Size.Height > 0)
            {
                _emtpyGroupPanelHeight = groupBox1.Size.Height;
                _mainFormHeight = Size.Height - _emtpyGroupPanelHeight;
            }
            else
            {
                // _emtpyGroupPanelHeight = 59;
                // _mainFormHeight = 330 - _emtpyGroupPanelHeight;
            }
            //_mainFormHeight = Size.Height;
            AntivirusInstalled = Helpers.AntivirusInstalled();
            ClearRatesAll();
            thisProc = Process.GetCurrentProcess();
            thisProc.PriorityClass = ProcessPriorityClass.Normal;
            //
        }
        public Icon IconFromFilePath(string filePath)
        {
            Icon programicon = null;
            try
            {
                programicon = Icon.ExtractAssociatedIcon(filePath);
            }
            catch { }
            return programicon;
        }
        private void InitLocalization()
        {
            MessageBoxManager.Unregister();
            MessageBoxManager.Yes = International.GetText("Global_Yes");
            MessageBoxManager.No = International.GetText("Global_No");
            MessageBoxManager.OK = International.GetText("Global_OK");
            MessageBoxManager.Cancel = International.GetText("Global_Cancel");
            MessageBoxManager.Retry = International.GetText("Global_Retry");
            MessageBoxManager.Register();

            labelServiceLocation.Text = International.GetText("Service_Location") + ":";
            labelWallet.Text = International.GetText("BitcoinAddress") + ":";
            labelWorkerName.Text = International.GetText("WorkerName") + ":";
            if (ConfigManager.GeneralConfig.ShowUptime)
            {
                label_Uptime.Text = International.GetText("Form_Main_Uptime");
                label_Uptime.Visible = true;
            }
            else
            {
                label_Uptime.Visible = false;
            }

            labelWallet.Text = International.GetText("BitcoinAddress") + ":";
            labelPayoutCurrency.Text = International.GetText("labelPayoutCurrency") + ":";
            labelTreshold.Text = International.GetText("labelTreshold") + ":";
            labelWorkerName.Text = International.GetText("WorkerName") + ":";

            linkLabelCheckStats.Text = International.GetText("Form_Main_check_stats");

            toolStripStatusLabelGlobalRateText.Text = International.GetText("Form_Main_global_rate");
            toolStripStatusLabelBTCDayText.Text =
                ConfigManager.GeneralConfig.PayoutCurrency + "/" + 
                International.GetText(ConfigManager.GeneralConfig.TimeUnit.ToString());
            /*
            toolStripStatusLabelBalanceText.Text = (ExchangeRateApi.ActiveDisplayCurrency + "/") +
                                                   International.GetText(
                                                       ConfigManager.GeneralConfig.TimeUnit.ToString()) + "     " +
                                                   International.GetText("Form_Main_balance") + ":";
            */
            if (ConfigManager.GeneralConfig.FiatCurrency)
            {
                toolStripStatusLabelBalanceDollarValue.Text = "(" + ExchangeRateApi.ActiveDisplayCurrency + ")";

                toolStripStatusLabelBalanceText.Text = (ExchangeRateApi.ActiveDisplayCurrency + "/") +
                                                       International.GetText(
                                                           ConfigManager.GeneralConfig.TimeUnit.ToString()) + "     " +
                                                       International.GetText("Form_Main_balance") + ":";
            } else
            {
                toolStripStatusLabelBalanceText.Text = (ConfigManager.GeneralConfig.PayoutCurrency + "/") +
                                       International.GetText(
                                           ConfigManager.GeneralConfig.TimeUnit.ToString()) + "     " +
                                       International.GetText("Form_Main_balance") + ":";
            }

            toolStripStatusLabel_power1.Text = International.GetText("Form_Main_Power1");
            toolStripStatusLabel_power2.Text = "-";
            toolStripStatusLabel_power3.Text = International.GetText("Form_Main_Power3");
            if (ConfigManager.GeneralConfig.ShowTotalPower)
            {
                toolStripStatusLabel_power4.Text = International.GetText("Form_Main_Power4");
                toolStripStatusLabel_power5.Text = "-";
                toolStripStatusLabel_power6.Text = International.GetText("Form_Main_Power6");
            } else
            {
                toolStripStatusLabel_power4.Text = "";
                toolStripStatusLabel_power5.Text = "";
                toolStripStatusLabel_power6.Text = "";
            }
            devicesListViewEnableControl1.InitLocaleMain();

            buttonBenchmark.Text = International.GetText("Form_Main_benchmark");
            buttonSettings.Text = International.GetText("Form_Main_settings");
            buttonStartMining.Text = International.GetText("Form_Main_start");
            buttonStopMining.Text = International.GetText("Form_Main_stop");
            buttonChart.Text = International.GetText("Form_Main_chart");

            label_NotProfitable.Text = International.GetText("Form_Main_MINING_NOT_PROFITABLE");
            groupBox1.Text = International.GetText("Form_Main_Group_Device_Rates");

        }
        public void InitMainConfigGuiData()
        {
            textBoxWallet.Text = " " + ConfigManager.GeneralConfig.Wallet.Trim();
            textBoxWorkerName.Text = " " + ConfigManager.GeneralConfig.WorkerName.Trim();
            textBoxPayoutCurrency.Text = " " + ConfigManager.GeneralConfig.PayoutCurrency.Trim();
            textBoxTreshold.Text = " " + ConfigManager.GeneralConfig.PayoutCurrencyTreshold.ToString("G");

            regionList.Clear();
            Region region = new Region();
            region.RegionName = "Anycast";
            region.RegionLocation = ".";
            regionList.Add(region);
            region = new Region();
            region.RegionName = "North America";
            region.RegionLocation = ".na.";
            regionList.Add(region);
            region = new Region();
            region.RegionName = "Europe";
            region.RegionLocation = ".eu.";
            regionList.Add(region);
            region = new Region();
            region.RegionName = "Asia";
            region.RegionLocation = ".asia.";
            regionList.Add(region);

            //Globals.MiningLocation = { ".", ".na.", ".eu.", ".asia."};

            try
            {
                comboBoxRegion.Items.Clear();
                foreach (var _region in regionList)
                {
                    comboBoxRegion.Items.Add(_region.RegionName);
                }
                if (ConfigManager.GeneralConfig.ServiceLocation >= 0 && ConfigManager.GeneralConfig.ServiceLocation <= 3)
                {
                    comboBoxRegion.SelectedIndex = ConfigManager.GeneralConfig.ServiceLocation;
                }
                else
                {
                    comboBoxRegion.SelectedIndex = 0;
                }
            } catch (Exception ex)
            {
                Helpers.ConsolePrint("InitMainConfigGuiData", ex.ToString());
            }
            
            _demoMode = false;

            // init active display currency after config load
            ExchangeRateApi.ActiveDisplayCurrency = ConfigManager.GeneralConfig.DisplayCurrency;

            // init factor for Time Unit
            switch (ConfigManager.GeneralConfig.TimeUnit)
            {
                case TimeUnitType.Hour:
                    _factorTimeUnit = 1.0 / 24.0;
                    break;
                case TimeUnitType.Day:
                    _factorTimeUnit = 1;
                    break;
                case TimeUnitType.Week:
                    _factorTimeUnit = 7;
                    break;
                case TimeUnitType.Month:
                    _factorTimeUnit = 30;
                    break;
                case TimeUnitType.Year:
                    _factorTimeUnit = 365;
                    break;
            }

            if (_isDeviceDetectionInitialized)
            {
                devicesListViewEnableControl1.ResetComputeDevices(ComputeDeviceManager.Available.Devices);
            }
        }


        public void AfterLoadComplete()
        {
            _loadingScreen = null;
            Enabled = true;

            buttonBenchmark.Enabled = true;
            buttonChart.Enabled = true;
            buttonSettings.Enabled = true;
            buttonStartMining.Enabled = true;
            buttonStopMining.Enabled = false;

            if (ConfigManager.GeneralConfig.AutoStartMining)
            {
                _AutoStartMiningDelay = ConfigManager.GeneralConfig.AutoStartMiningDelay;
                _autostartTimerDelay = new Timer();
                _autostartTimerDelay.Tick += AutoStartTimerDelay_Tick;
                _autostartTimerDelay.Interval = 1000;
                _autostartTimerDelay.Start();

                Thread.Sleep(200);//костыль для очередности запуска таймеров

                _autostartTimer = new Timer();
                _autostartTimer.Tick += AutoStartTimer_Tick;
                _autostartTimer.Interval = Math.Max(2000, ConfigManager.GeneralConfig.AutoStartMiningDelay * 1000);
                _autostartTimer.Start();

                Thread.Sleep(200);
            }

            _idleCheck = new Timer();
            _idleCheck.Tick += IdleCheck_Tick;
            _idleCheck.Interval = 500;
            _idleCheck.Start();

            Thread.Sleep(200);

            _minerStatsCheck = new Timer();
            _minerStatsCheck.Tick += MinerStatsCheck_Tick;
            _minerStatsCheck.Interval = 1000;

            devicesListViewEnableControl1.Visible = true;
            if (ConfigManager.GeneralConfig.StartChartWithProgram == true)
            {
                Form_RigProfitChartRunning = true;
                var chart = new Form_RigProfitChart();
                try
                {
                    if (chart != null)
                    {
                        Rectangle screenSize = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                        //if (ConfigManager.GeneralConfig.ProfitFormLeft + ConfigManager.GeneralConfig.ProfitFormWidth <= screenSize.Size.Width)
                        {
                            if (ConfigManager.GeneralConfig.ProfitFormTop + ConfigManager.GeneralConfig.ProfitFormLeft >= 1)
                            {
                                chart.Top = ConfigManager.GeneralConfig.ProfitFormTop;
                                chart.Left = ConfigManager.GeneralConfig.ProfitFormLeft;
                            }

                            chart.Width = ConfigManager.GeneralConfig.ProfitFormWidth;
                            chart.Height = ConfigManager.GeneralConfig.ProfitFormHeight;
                        }
                        /*
                        else
                        {
                            // chart.Width = 660; // min width
                        }
                        */
                    }
                    if (chart != null) chart.Show();
                }
                catch (Exception er)
                {
                    Helpers.ConsolePrint("chart", er.ToString());
                }
            }
        }


        private void IdleCheck_Tick(object sender, EventArgs e)
        {
            buttonChart.Enabled = !Form_RigProfitChartRunning;
            if (!ConfigManager.GeneralConfig.StartMiningWhenIdle) return;
            if (_isManuallyStarted) return;

            var msIdle = Helpers.GetIdleTime();
            if (_minerStatsCheck.Enabled)
            {
                if (msIdle < (ConfigManager.GeneralConfig.MinIdleSeconds * 1000) && _isManuallyStarted)
                {
                    StopMining();
                    _isManuallyStarted = false;
                    Helpers.ConsolePrint("ZergPool", "Stop from idling mining");
                }
            }

            if (!ConfigManager.GeneralConfig.StartMiningWhenIdle || _isManuallyStarted) return;

            if (_minerStatsCheck.Enabled)
            {
                if (msIdle < (ConfigManager.GeneralConfig.MinIdleSeconds * 1000))
                {
                    StopMining();
                    Helpers.ConsolePrint("ZergPool", "Resumed from idling");
                }
            }
            else
            {
                if (_benchmarkForm == null && (msIdle > (ConfigManager.GeneralConfig.MinIdleSeconds * 1000)))
                {
                    Helpers.ConsolePrint("ZergPool", "Entering idling state");
                    if (StartMining(true) == StartMiningReturnType.ShowNoMining)
                    {
                        _isManuallyStarted = false;
                        StopMining();
                        MessageBox.Show(International.GetText("Form_Main_StartMiningReturnedFalse"),
                            International.GetText("Warning_with_Exclamation"),
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private static void InstallCerts()
        {
            MyWebClient client = new MyWebClient();
            client.UseDefaultCredentials = false;
            try
            {
                if (File.Exists("temp//authrootstl.cab"))
                {
                    File.Delete("temp//authrootstl.cab");
                }
                client.DownloadFile(new Uri("http://ctldl.windowsupdate.com/msdownload/update/v3/static/trustedr/en/authrootstl.cab"), "temp//authrootstl.cab");

                if (File.Exists("temp//authrootstl.cab"))
                {
                    var CMDconfigHandleBackup = new Process
                    {
                        StartInfo =
                {
                    FileName = "utils\\7z.exe"
                }
                    };

                    var cmd7z = new Process
                    {
                        StartInfo =
                {
                    FileName = "utils\\7z.exe"
                }
                    };
                    cmd7z.StartInfo.Arguments = "x -r -y temp\\authrootstl.cab";
                    cmd7z.StartInfo.UseShellExecute = false;
                    cmd7z.StartInfo.CreateNoWindow = true;
                    cmd7z.Start();
                    cmd7z.WaitForExit(1000 * 2);
                    Helpers.ConsolePrint("InstallCerts", "Error code: " + cmd7z.ExitCode);
                    if (File.Exists("authroot.stl"))
                    {
                        //certutil -enterprise -f -v -AddStore "Root" "authroot.stl"
                        ProcessStartInfo cmdcertutil = new ProcessStartInfo();
                        cmdcertutil.FileName = "certutil";
                        cmdcertutil.Arguments = "-enterprise -f -v -AddStore \"Root\" \"authroot.stl\"";
                        cmdcertutil.UseShellExecute = false;
                        cmdcertutil.CreateNoWindow = true;
                        cmdcertutil.RedirectStandardOutput = true;
                        cmdcertutil.RedirectStandardError = true;
                        Process p = Process.Start(cmdcertutil);
                        string o = p.StandardOutput.ReadToEnd();
                        p.WaitForExit(1000 * 5);
                        File.Delete("authroot.stl");
                        //Helpers.ConsolePrint("InstallCerts", o);
                    }
                    File.Delete("temp//authrootstl.cab");
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("InstallCerts", ex.ToString());
                return;
            }
        }

        public static void ProgressBarUpd(DownloadProgressChangedEventArgs e)
        {
            if (Form_Settings.ProgressProgramUpdate != null)
            {
                Form_Settings.ProgressProgramUpdate.Maximum = (int)e.TotalBytesToReceive / 100;
                Form_Settings.ProgressProgramUpdate.Value = (int)e.BytesReceived / 100;
            }
            if ((int)e.TotalBytesToReceive == (int)e.BytesReceived && Form_Settings.ProgressProgramUpdate != null)
            {
                Form_Settings.ProgressProgramUpdate.Visible = false;
            }
        }
        public class Proxy
        {
            [Serializable]
            public class ProxyDetail
            {
                public string NameRU;
                public string NameEN;
                public string Url;
            }
        }
        
        static void ArrayRearrangeAfterItemMove<T>(T[] array, int indexFrom, int indexTo)
        {
            if (indexFrom == indexTo) return;
            T temp = array[indexFrom];
            T value = temp;

            for (int i = indexFrom + 1; i <= indexTo; i++)
            {
                temp = array[i];
                array[i] = value;
                value = temp;
            }

            array[indexFrom] = temp;
        }
        private void CheckUpdates()
        {
            try
            {
                CheckGithub();
                //checkD();
                /*
                Stats.ConnectToGoogle();
                if (GoogleAnswer.Contains("HTTP"))
                {
                    Helpers.ConsolePrint("ConnectToGoogle", "Connect to google OK");
                }
                //checkD();
                new Task(() => checkD()).Start();
                */

            }
            catch (Exception er)
            {
                Helpers.ConsolePrint("CheckGithub", er.ToString());
            }
        }


        public static int ProgressMinimum = 0;
        public static int ProgressMaximum = 100;
        public static int ProgressValue = 0;

        private class TimeInterval
        {
            public TimeSpan From;
            public TimeSpan To;
            public double Price;
        }
        public static double GetKwhPrice()
        {
            double _price = 0.0d;
            List<TimeInterval> zones = new List<TimeInterval>();

            if (ConfigManager.GeneralConfig.PowerTarif == 0)
            {
                try
                {
                    TimeInterval _interval = new();
                    _interval.From = TimeSpan.Parse(Form_Main.ZoneSchedule1[0]);
                    _interval.To = TimeSpan.Parse(Form_Main.ZoneSchedule1[1]);
                    _interval.Price = double.Parse(Form_Main.ZoneSchedule1[2]);
                    zones.Add(_interval);
                }
                catch (FormatException ex)
                {
                    Helpers.ConsolePrint("GetKwhPrice", ex.Message);
                }
            }

            if (ConfigManager.GeneralConfig.PowerTarif == 1)
            {
                try
                {
                    TimeInterval _interval = new();
                    _interval.From = TimeSpan.Parse(Form_Main.ZoneSchedule2[0]);
                    _interval.To = TimeSpan.Parse(Form_Main.ZoneSchedule2[1]);
                    _interval.Price = double.Parse(Form_Main.ZoneSchedule2[2]);
                    zones.Add(_interval);

                    _interval = new();
                    _interval.From = TimeSpan.Parse(Form_Main.ZoneSchedule2[3]);
                    _interval.To = TimeSpan.Parse(Form_Main.ZoneSchedule2[4]);
                    _interval.Price = double.Parse(Form_Main.ZoneSchedule2[5]);
                    zones.Add(_interval);
                }
                catch (FormatException ex)
                {
                    Helpers.ConsolePrint("GetKwhPrice", ex.Message);
                }
            }

            if (ConfigManager.GeneralConfig.PowerTarif == 2)
            {
                try
                {
                    TimeInterval _interval = new();
                    _interval.From = TimeSpan.Parse(Form_Main.ZoneSchedule3[0]);
                    _interval.To = TimeSpan.Parse(Form_Main.ZoneSchedule3[1]);
                    _interval.Price = double.Parse(Form_Main.ZoneSchedule3[2]);
                    zones.Add(_interval);
                    _interval = new();

                    _interval.From = TimeSpan.Parse(Form_Main.ZoneSchedule3[3]);
                    _interval.To = TimeSpan.Parse(Form_Main.ZoneSchedule3[4]);
                    _interval.Price = double.Parse(Form_Main.ZoneSchedule3[5]);
                    zones.Add(_interval);
                    _interval = new();

                    _interval.From = TimeSpan.Parse(Form_Main.ZoneSchedule3[6]);
                    _interval.To = TimeSpan.Parse(Form_Main.ZoneSchedule3[7]);
                    _interval.Price = double.Parse(Form_Main.ZoneSchedule3[8]);
                    zones.Add(_interval);
                    _interval = new();

                    _interval.From = TimeSpan.Parse(Form_Main.ZoneSchedule3[9]);
                    _interval.To = TimeSpan.Parse(Form_Main.ZoneSchedule3[10]);
                    _interval.Price = double.Parse(Form_Main.ZoneSchedule3[11]);
                    zones.Add(_interval);
                    _interval = new();

                    _interval.From = TimeSpan.Parse(Form_Main.ZoneSchedule3[12]);
                    _interval.To = TimeSpan.Parse(Form_Main.ZoneSchedule3[13]);
                    _interval.Price = double.Parse(Form_Main.ZoneSchedule3[14]);
                    zones.Add(_interval);
                    _interval = new();
                }
                catch (FormatException ex)
                {
                    Helpers.ConsolePrint("GetKwhPrice", ex.Message);
                }
            }
            foreach (var zone in zones)
            {
                TimeSpan _From = new TimeSpan();
                TimeSpan _To = new TimeSpan();
                TimeSpan _Add = new TimeSpan(0, 0, 0);
                _From = zone.From;
                _To = zone.To;
                _price = zone.Price;
                if (_To.TotalMilliseconds - _From.TotalMilliseconds < 0)
                {
                    _Add = new TimeSpan(24, 0, 0);
                    _To = _To.Add(_Add);
                }
                if (DateTime.Now.TimeOfDay.Add(_Add) >= _From && DateTime.Now.TimeOfDay.Add(_Add) < _To)
                {
                    return _price;
                }
                if (DateTime.Now.TimeOfDay.IsBetween(_From, _To))//переход через 23:59:59
                {
                    return _price;
                }
            }
            return 0.0d;
        }

        private void StartupTimer_Tick(object sender, EventArgs e)
        {
            //Запускает приложение в классической теме windows. На 7-ке не отображается progressbar
            //Application.VisualStyleState = System.Windows.Forms.VisualStyles.VisualStyleState.NoneEnabled;
            if (!ConfigManager.GeneralConfig.AutoStartMining)
            {
                buttonStopMining.Enabled = false;
                // buttonBTC_Clear.Enabled = true;
            }
            else
            {
                buttonStopMining.Text = buttonStopMining.Text + " ...";
            }


            _startupTimer.Stop();
            _startupTimer = null;

            // Internals Init
            // TODO add loading step
            //_loadingScreen.SetValueAndMsg(3, "Init...");
            MinersSettingsManager.Init();

            if (!Helpers.Is45NetOrHigher())
            {
                MessageBox.Show(International.GetText("NET45_Not_Installed_msg"),
                    International.GetText("Warning_with_Exclamation"),
                    MessageBoxButtons.OK);

                Close();
                return;
            }

            if (!Helpers.Is64BitOperatingSystem)
            {
                MessageBox.Show(International.GetText("Form_Main_x64_Support_Only"),
                    International.GetText("Warning_with_Exclamation"),
                    MessageBoxButtons.OK);

                Close();
                return;
            }
            _loadingScreen.Show();
            _loadingScreen.SetValueAndMsg(5, International.GetText("Form_Main_loadtext_SetEnvironmentVariable"));
            Application.DoEvents();
            Helpers.SetDefaultEnvironmentVariables();
            new Task(() => FlushCache()).Start();

            ZoneSchedule1 = ConfigManager.GeneralConfig.ZoneSchedule1;
            ZoneSchedule2 = ConfigManager.GeneralConfig.ZoneSchedule2;
            ZoneSchedule3 = ConfigManager.GeneralConfig.ZoneSchedule3;

            if (ConfigManager.GeneralConfig.Use_OpenHardwareMonitor)
            {
                Helpers.ConsolePrint("LibreHardwareMonitor", "Init library start...");
                thisComputer = new LibreHardwareMonitor.Hardware.Computer();
                thisComputer.IsGpuEnabled = true;
                thisComputer.IsCpuEnabled = true;
                thisComputer.Open();
                Helpers.ConsolePrint("LibreHardwareMonitor", "Init library end");
            }

            // Query Available ComputeDevices
            _loadingScreen.SetValueAndMsg(10, International.GetText("Form_Main_loadtext_CPU"));

            ComputeDeviceManager.Query.QueryDevices(_loadingScreen);//10-15

            // Init profiles
            Profiles.Profile.InitProfiles();

            if (!ConfigManager.GeneralConfig.DeviceDetection.DisableDetectionNVIDIA)
            {
                if (ComputeDeviceManager.Query.WindowsDisplayAdapters.HasNvidiaVideoController())
                {
                    try
                    {
                        foreach (var process in Process.GetProcessesByName("NvidiaGPUGetDataHost"))
                        {
                            process.Kill();
                        }
                    }
                    catch (Exception)
                    {

                    }
                    Thread.Sleep(200);

                    if (File.Exists("common\\NvidiaGPUGetDataHost.exe"))
                    {
                        var MonitorProc = new Process
                        {
                            StartInfo = { FileName = "common\\NvidiaGPUGetDataHost.exe" }
                        };

                        MonitorProc.StartInfo.UseShellExecute = false;
                        MonitorProc.StartInfo.CreateNoWindow = true;
                        if (MonitorProc.Start())
                        {
                            Helpers.ConsolePrint("NvidiaGPUGetDataHost", "Starting OK");
                        }
                        else
                        {
                            Helpers.ConsolePrint("NvidiaGPUGetDataHost", "Starting ERROR");
                        }
                        Application.DoEvents();
                    }
                }
            }
            Thread.Sleep(500);

            _deviceTelemetryTimer = new Timer();
            _deviceTelemetryTimer.Tick += DeviceTelemetryTimer_Tick;
            _deviceTelemetryTimer.Interval = 1000;
            _deviceTelemetryTimer.Start();

            Application.DoEvents();

            _isDeviceDetectionInitialized = true;

            //_loadingScreen.SetValueAndMsg(15, International.GetText("Form_Main_loadtext_LoadProxyList"));
            Wallets.Wallets.InitWallets();

            /////////////////////////////////////////////
            /////// from here on we have our devices and Miners initialized
            ConfigManager.AfterDeviceQueryInitialization();
            _loadingScreen.SetValueAndMsg(20, International.GetText("Form_Main_loadtext_SaveConfig"));
            var mainproc = Process.GetCurrentProcess();
            if (ConfigManager.GeneralConfig.ProgramMonitoring)
            {
                try
                {
                    var WDHandle = new Process
                    {
                        StartInfo =
                {
                    FileName = "taskkill.exe"
                }
                    };
                    WDHandle.StartInfo.Arguments = "/F /IM startMonitor.cmd";
                    WDHandle.StartInfo.UseShellExecute = false;
                    WDHandle.StartInfo.CreateNoWindow = true;
                    WDHandle.Start();
                }
                catch (Exception ex)
                {
                    Helpers.ConsolePrint("WatchDog", ex.ToString());
                }

                try
                {
                    /*
                    if (File.Exists("utils\\startMonitor.cmd"))
                    {
                        File.Delete("utils\\startMonitor.cmd");
                        File.WriteAllText("utils\\startMonitor.cmd", "start MinerLegacyForkFixMonitor.exe %1");
                    }
                    else
                    {
                        File.WriteAllText("utils\\startMonitor.cmd", "start MinerLegacyForkFixMonitor.exe %1");
                    }
                    */
                    if (File.Exists("MinerLegacyForkFixMonitor.exe"))
                    {
                        var MonitorProc = new Process
                        {
                            StartInfo =
                        {
                             FileName = "utils\\startMonitor.cmd"
                        }
                        };

                        MonitorProc.StartInfo.Arguments = mainproc.Id.ToString();
                        MonitorProc.StartInfo.UseShellExecute = false;
                        MonitorProc.StartInfo.CreateNoWindow = true;
                        if (MonitorProc.Start())
                        {
                            Helpers.ConsolePrint("Watchdog Monitor", "Starting OK");

                        }
                        else
                        {
                            Helpers.ConsolePrint("Watchdog Monitor", "Starting ERROR");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Helpers.ConsolePrint("Watchdog Monitor", ex.Message);
                }
            }
            if (ConfigManager.GeneralConfig.InstallRootCerts)
            {
                new Task(() => InstallCerts()).Start();
            }

            // All devices settup should be initialized in AllDevices
            devicesListViewEnableControl1.ResetComputeDevices(ComputeDeviceManager.Available.Devices);
            // set properties after
            devicesListViewEnableControl1.SaveToGeneralConfig = true;

            if (ConfigManager.GeneralConfig.ABEnableOverclock)
            {
                _loadingScreen.SetValueAndMsg(25, International.GetText("Form_Main_loadtext_MSI_AB"));
                MSIAfterburner.MSIAfterburnerRUN();
                Application.DoEvents();
            }

            flowLayoutPanelRates.Visible = true;

            new Task(() => Firewall.AddToFirewall()).Start();

            try
            {
                if (!File.Exists("configs\\ExcludeExchange.json"))
                {
                    ExchangeRateApi.excludeExchange.Add("Bitgem");
                    var json = JsonConvert.SerializeObject(ExchangeRateApi.excludeExchange, Formatting.Indented);
                    File.WriteAllText("configs\\ExcludeExchange.json", json);
                }
                else
                {
                    var excludetext = File.ReadAllText("configs\\ExcludeExchange.json");
                    ExchangeRateApi.excludeExchange = JsonConvert.DeserializeObject<List<string>>(excludetext);
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("StartupTimer_Tick", ex.ToString());
            }

            _loadingScreen.SetValueAndMsg(30, "Checking server: zergpool.com");
            this.Update();
            this.Refresh();
            Application.DoEvents();
            //****************
            Links.CheckDNS("https://zergpool.com");
            List<string> algos = Enum.GetNames(typeof(AlgorithmType)).ToList();
            Array algosN = Enum.GetValues(typeof(AlgorithmType));

            double loc = 35;

            foreach (var location in regionList)
            {
                string domain;
                domain = location.RegionLocation.ToLower().TrimStart('.') + "mine.zergpool.com";
                _loadingScreen.SetValueAndMsg((int)loc, International.GetText("Form_Main_loadtext_Checking_servers_locations") + ": " + domain.Replace("stratum+tcp://", ""));
                _loadingScreen.Update();
                Links.CheckDNS(domain);
                loc = loc + 5;
                Application.DoEvents();
            }

            _loadingScreen.SetValueAndMsg(55, International.GetText("Form_Main_loadtext_GetAlgosProfit"));
            Helpers.DisableWindowsErrorReporting(ConfigManager.GeneralConfig.DisableWindowsErrorReporting);
            AlgosProfitData.InitializeIfNeeded();
            Stats.Stats.LoadAlgoritmsList();
            AlgosProfitData.FinalizeAlgosProfitList();
            Application.DoEvents();
            try
            {
                if (!File.Exists("configs\\CurrenciesList.json"))
                {
                    string cur = JsonConvert.SerializeObject(Form_Settings.currencys, Formatting.Indented);
                    Helpers.WriteAllTextWithBackup("configs\\CurrenciesList.json", cur);
                } else
                {
                    string[] _cur = null;
                    var cur = File.ReadAllText("configs\\CurrenciesList.json");
                    JArray json = JArray.Parse(cur);
                    _cur = json.OfType<object>().Select(o => o.ToString()).ToArray();
                    Form_Settings.currencys = _cur;
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("StartupTimer_Tick", ex.ToString());
            }

            List<ConcurrentDictionary<string, double>> exchanges_rates = new();
            _loadingScreen.SetValueAndMsg(56, International.GetText("Form_Main_loadtext_GetBTCRate") + 
                " (Xeggex)");
            exchanges_rates.Add(ExchangeRateApi.GetRatesXeggex());
            Application.DoEvents();
            _loadingScreen.SetValueAndMsg(57, International.GetText("Form_Main_loadtext_GetBTCRate") +
                " (Gate.io)");
            exchanges_rates.Add(ExchangeRateApi.GetRatesgateio());
            Application.DoEvents();
            _loadingScreen.SetValueAndMsg(58, International.GetText("Form_Main_loadtext_GetBTCRate") +
                " (Tradeogre)");
            exchanges_rates.Add(ExchangeRateApi.GetRatesTradeogre());
            Application.DoEvents();
            _loadingScreen.SetValueAndMsg(59, International.GetText("Form_Main_loadtext_GetBTCRate") +
                " (Nonkyc)");
            exchanges_rates.Add(ExchangeRateApi.GetRatesNonkyc());
            Application.DoEvents();
            _loadingScreen.SetValueAndMsg(60, International.GetText("Form_Main_loadtext_GetBTCRate") +
                " (Coinex)");
            exchanges_rates.Add(ExchangeRateApi.GetRatesCoinex());
            Application.DoEvents();
            _loadingScreen.SetValueAndMsg(61, International.GetText("Form_Main_loadtext_GetBTCRate") +
                " (Mexc)");
            exchanges_rates.Add(ExchangeRateApi.GetRatesMexc());
            Application.DoEvents();
            _loadingScreen.SetValueAndMsg(62, International.GetText("Form_Main_loadtext_GetBTCRate") +
                " (SafeTrade)");
            exchanges_rates.Add(ExchangeRateApi.GetRatesSafetrade());
            Application.DoEvents();
            ExchangeRateApi.GetBTCRate(exchanges_rates);
            exchanges_rates = null;


            List<ConcurrentDictionary<string, double>> exchanges_fiat = new();
            exchanges_fiat.Add(ExchangeRateApi.GetExchangeRates1());
            Application.DoEvents();
            _loadingScreen.SetValueAndMsg(63, International.GetText("Form_Main_loadtext_GetCurrencyRate"));
            exchanges_fiat.Add(ExchangeRateApi.GetExchangeRates2());
            Application.DoEvents();
            _loadingScreen.SetValueAndMsg(64, International.GetText("Form_Main_loadtext_GetCurrencyRate"));
            exchanges_fiat.Add(ExchangeRateApi.GetExchangeRates3());
            Application.DoEvents();
            _loadingScreen.SetValueAndMsg(65, International.GetText("Form_Main_loadtext_GetCurrencyRate"));
            ExchangeRateApi.GetFiatRate(exchanges_fiat);
            Application.DoEvents();
            exchanges_fiat = null;

            _updateSMATimer = new Timer();
            _updateSMATimer.Tick += UpdateSMATimer_Tick;
            _updateSMATimer.Interval = 1000 * 30;
            _updateTimerCount = 0;
            _updateSMATimer.Start();
            new Task(() => UpdateSMATimer_Tick(null, null)).Start();
            /*
            _finalizeSMATimer = new Timer();
            _finalizeSMATimer.Tick += FinalizeTimer_Tick;
            _finalizeSMATimer.Interval = 1000 * 60;
            _finalizeSMATimer.Start();
            */
            _loadingScreen.SetValueAndMsg(69, International.GetText("Form_Main_loadtext_GetBalance"));
            _getBalanceTimer = new Timer();
            _getBalanceTimer.Tick += Stats.Stats.GetWalletBalance;
            _getBalanceTimer.Interval = 1000 * 60 * 5;
            _getBalanceTimer.Start();
            Stats.Stats.GetWalletBalance(null, null);
            Application.DoEvents();
            Stats.Stats.GetWalletBalanceEx(null, null);
            Application.DoEvents();

            _loadingScreen.SetValueAndMsg(74, International.GetText("Form_Main_loadtext_CheckLatestVersion"));
            _loadingScreen.Update();
            //new Task(() => CheckUpdates()).Start();
            CheckUpdates();
            Application.DoEvents();

            if (ConfigManager.GeneralConfig.ShowHistory)
            {
                new Task(() => Updater.Updater.ShowHistory(false)).Start();
            }
            //new Task(() => ResetProtocols()).Start();

            _loadingScreen.SetValueAndMsg(75, International.GetText("Form_Main_loadtext_CheckMiners"));
            Thread.Sleep(10);
            var runVCRed = !MinersExistanceChecker.IsMinersBinsInit() && !ConfigManager.GeneralConfig.DownloadInit;

            if (!MinersExistanceChecker.IsMinersBinsInit())
            {
                try
                {
                    if (_autostartTimerDelay != null)
                    {
                        _autostartTimerDelay.Stop();
                    }
                    if (_autostartTimer != null)
                    {
                        _autostartTimer.Stop();
                    }
                }
                catch (Exception ex)
                {
                    Helpers.ConsolePrint("Download miners", ex.ToString());
                }

                var result = Utils.MessageBoxEx.Show(International.GetText("Form_Main_bins_folder_files_missing"),
                  International.GetText("Warning_with_Exclamation"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, 5000);

                if (result == DialogResult.Yes)
                {
                    DownloadingInProgress = true;
                    ConfigManager.GeneralConfigFileCommit();
                    {
                        if (Updater.Updater.GetGITHUBVersion() > 0)
                        {
                            //new Task(() => Updater.Updater.EmergencyDownloader(Form_Main.miners_url)).Start();
                            Updater.Updater.EmergencyDownloader(Form_Main.miners_url);
                        }
                        else if (Updater.Updater.GetGITLABVersion() > 0)
                        {
                            //new Task(() => Updater.Updater.EmergencyDownloader(Form_Main.miners_url)).Start();
                            Updater.Updater.EmergencyDownloader(Form_Main.miners_url);
                        }
                    }
                    //блокировка формы блокирует всё
                    /*
                    do
                    {
                        Thread.Sleep(100);
                    } while (DownloadingInProgress);
                    //_autostartTimerDelay.Start();
                    //_autostartTimer.Start();
                    */
                }
            }
            else
            {
                // all good
                ConfigManager.GeneralConfig.DownloadInit = true;
                ConfigManager.GeneralConfigFileCommit();
            }

            if (ConfigManager.GeneralConfig.GetMinersVersions)
            {
                var minerdata = new MinerData();

                lock (MinerVersion.MinerDataList)
                {
                    _loadingScreen.SetValueAndMsg(76, International.GetText("Form_Main_loadtext_GetMinerVersion") + "ClaymoreNeoscrypt");
                    minerdata = MinerVersion.Get_ClaymoreNeoscrypt();
                    MinerVersion.MinerDataList.Add(minerdata);

                    if (ComputeDeviceManager.Query.WindowsDisplayAdapters.HasNvidiaVideoController())
                    {
                        _loadingScreen.SetValueAndMsg(77, International.GetText("Form_Main_loadtext_GetMinerVersion") + "CryptoDredge");
                        minerdata = MinerVersion.Get_CryptoDredge();
                        MinerVersion.MinerDataList.Add(minerdata);
                    }

                    _loadingScreen.SetValueAndMsg(78, International.GetText("Form_Main_loadtext_GetMinerVersion") + "GMiner");
                    minerdata = MinerVersion.Get_GMiner();
                    MinerVersion.MinerDataList.Add(minerdata);

                    _loadingScreen.SetValueAndMsg(79, International.GetText("Form_Main_loadtext_GetMinerVersion") + "lolMiner");
                    minerdata = MinerVersion.Get_lolMiner();
                    MinerVersion.MinerDataList.Add(minerdata);

                    _loadingScreen.SetValueAndMsg(80, International.GetText("Form_Main_loadtext_GetMinerVersion") + "miniZ");
                    minerdata = MinerVersion.Get_miniZ();
                    MinerVersion.MinerDataList.Add(minerdata);

                    _loadingScreen.SetValueAndMsg(81, International.GetText("Form_Main_loadtext_GetMinerVersion") + "Nanominer");
                    minerdata = MinerVersion.Get_nanominer();
                    MinerVersion.MinerDataList.Add(minerdata);
                    /*
                    _loadingScreen.SetValueAndMsg(82, International.GetText("Form_Main_loadtext_GetMinerVersion") + "NBMiner.39.5");
                    minerdata = MinerVersion.Get_NBMiner39_5();
                    MinerVersion.MinerDataList.Add(minerdata);
                    */
                    /*
                    _loadingScreen.SetValueAndMsg(82, International.GetText("Form_Main_loadtext_GetMinerVersion") + "NBMiner");
                    minerdata = MinerVersion.Get_NBMiner();
                    MinerVersion.MinerDataList.Add(minerdata);
                    */
                    _loadingScreen.SetValueAndMsg(83, International.GetText("Form_Main_loadtext_GetMinerVersion") + "PhoenixMiner");
                    minerdata = MinerVersion.Get_Phoenix();
                    MinerVersion.MinerDataList.Add(minerdata);

                    _loadingScreen.SetValueAndMsg(84, International.GetText("Form_Main_loadtext_GetMinerVersion") + "SRBMiner");
                    minerdata = MinerVersion.Get_SRBMiner();
                    MinerVersion.MinerDataList.Add(minerdata);

                    _loadingScreen.SetValueAndMsg(85, International.GetText("Form_Main_loadtext_GetMinerVersion") + "T-Rex");
                    minerdata = MinerVersion.Get_TRex();
                    MinerVersion.MinerDataList.Add(minerdata);

                    _loadingScreen.SetValueAndMsg(86, International.GetText("Form_Main_loadtext_GetMinerVersion") + "TeamRedMiner");
                    minerdata = MinerVersion.Get_TeamRedMiner();
                    MinerVersion.MinerDataList.Add(minerdata);

                    _loadingScreen.SetValueAndMsg(87, International.GetText("Form_Main_loadtext_GetMinerVersion") + "XMRig");
                    minerdata = MinerVersion.Get_XMRig();
                    MinerVersion.MinerDataList.Add(minerdata);

                    _loadingScreen.SetValueAndMsg(88, International.GetText("Form_Main_loadtext_GetMinerVersion") + "Rigel");
                    minerdata = MinerVersion.Get_Rigel();
                    MinerVersion.MinerDataList.Add(minerdata);
                }

                string json = JsonConvert.SerializeObject(MinerDataList, Formatting.Indented);
                try
                {
                    if (File.Exists("Configs\\MinersData.json"))
                    {
                        File.Delete("Configs\\MinersData.json");
                    }
                }
                catch (Exception ex)
                {
                    Helpers.ConsolePrint("CheckMiners", ex.ToString());
                }
                Helpers.WriteAllTextWithBackup("Configs\\MinersData.json", json);
                //new Task(() => MinersGetVersionWatchdog()).Start();
                _loadingScreen.SetValueAndMsg(89, International.GetText("Form_Main_loadtext_CheckProcesses"));
                MinersGetVersionWatchdog();
            }

            _marketsTimer = new Timer();
            _marketsTimer.Tick += MarketTimer_Tick;
            _marketsTimer.Interval = 1000 * 60 * 60 * 3;//3 hour
            _marketsTimer.Start();

            if (ConfigManager.GeneralConfig.EnableRigRemoteView)
            {
                _loadingScreen.SetValueAndMsg(90, "Start internal http server");
                Thread.Sleep(10);
                new Task(() => Server.Listener(true)).Start();
            }
            if (ConfigManager.GeneralConfig.EnableAPI)
            {
                _loadingScreen.SetValueAndMsg(91, "Start internal http server");
                Thread.Sleep(10);
                new Task(() => APIServer.Listener(true)).Start();
            }

            if (ConfigManager.GeneralConfig.ABEnableOverclock)
            {
                bool MSIAfterburnerRunning = true;
                _loadingScreen.SetValueAndMsg(95, "Check MSI Afterburner");
                int countab = 0;
                do
                {
                    Thread.Sleep(1000);
                    countab++;
                    if (Process.GetProcessesByName("MSIAfterburner").Any())
                    {
                        break;
                    } else
                    {
                        MSIAfterburnerRunning = false;
                    }
                } while (countab < 15); //15 sec

                if (!MSIAfterburnerRunning)
                {
                    Thread.Sleep(5000);
                }
                if (!MSIAfterburner.MSIAfterburnerInit())
                {
                    new Task(() =>
                        MessageBox.Show(International.GetText("FormSettings_AB_Error"), "MSI Afterburner error!",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)).Start();
                }

            }
            
            Stats.Stats.CheckNewAlgo();

            _loadingScreen.SetValueAndMsg(100, International.GetText("Form_Main_loadtext_Check_VC_redistributable"));
            InstallVcRedist();

            if (_loadingScreen != null)
            {
                _loadingScreen.FinishLoad();
            }

            if (ConfigManager.GeneralConfig.AlwaysOnTop) this.TopMost = true;

        }

        private static void MinersGetVersionWatchdog()
        {
            Thread.Sleep(100);
            try
            {
                Process localByName = Process.GetProcessById(Process.GetCurrentProcess().Id);
                var query = "Select * From Win32_Process Where ParentProcessId = " + Process.GetCurrentProcess().Id.ToString();
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                ManagementObjectCollection processList = searcher.Get();
                List<string> result = new List<string>();
                foreach (var p in processList)
                {
                    result.Add(p.GetPropertyValue("Name").ToString());
                }

                foreach (var m in result)
                {
                    if (m.Contains("MSIAfterburner") || m.Contains("NvidiaGPUGetDataHost") ||
                        m.Contains("netsh") || m.Contains("cports") || m.Contains("sc") ||
                        m.Contains("igfx") || m.Contains("vc_redist") || m.ToLower().Contains("notepad") ||
                        m.ToLower().Contains("form_splash") || m.ToLower().Contains("reg"))
                    {
                        continue;
                    }
                    Helpers.ConsolePrint("MinersGetVersionWatchdog", "Stuck miner: " + m);
                    try
                    {
                        var WDHandle = new Process
                        {
                            StartInfo =
                {
                    FileName = "taskkill.exe"
                }
                        };
                        WDHandle.StartInfo.Arguments = "/F /IM " + m;
                        WDHandle.StartInfo.UseShellExecute = false;
                        WDHandle.StartInfo.CreateNoWindow = true;
                        WDHandle.Start();
                    }
                    catch (Exception ex)
                    {
                        Helpers.ConsolePrint("MinersGetVersionWatchdog", ex.ToString());
                    }

                }
            } catch (Exception ex)
            {
                Helpers.ConsolePrint("MinersGetVersionWatchdog", ex.ToString());
            }
        }


        [DllImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCache")]
        static extern UInt32 DnsFlushResolverCache();

        [DllImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCacheEntry_A")]
        public static extern int DnsFlushResolverCacheEntry(string hostName);

        public static void FlushCache()
        {
            DnsFlushResolverCache();
            /*
            try
            {
                var vcredistProcess = new Process

                {
                    StartInfo =
                {
                    FileName = "ipconfig.exe"
                }
                };

                vcredistProcess.StartInfo.Arguments = "/flushdns";
                vcredistProcess.StartInfo.UseShellExecute = false;
                vcredistProcess.StartInfo.CreateNoWindow = true;
                vcredistProcess.Start();
                vcredistProcess.WaitForExit();

            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("ipconfig", e.ToString());
            }
            */
        }

        public static void FlushCache(string hostName)
        {
            DnsFlushResolverCacheEntry(hostName);
        }
        private bool IsVcRedistInstalled()
        {
            try
            {
                using (var vcredist = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\x64"))
                {
                    var major = double.Parse(vcredist.GetValue("Major")?.ToString());
                    var minor = double.Parse(vcredist.GetValue("Minor")?.ToString());
                    double verInstalled = major + (double)(minor / 100);

                    var versionInfo = FileVersionInfo.GetVersionInfo("miners\\vc_redist.x64.exe");
                    string version = versionInfo.FileVersion;
                    double.TryParse(version.Split('.')[0], out double verFilemajor);
                    double.TryParse(version.Split('.')[1], out double verFileminor);
                    double verFile = verFilemajor + (double)(verFileminor / 100);

                    if (verFile > verInstalled)
                    {
                        Helpers.ConsolePrint("IsVcRedistInstalled", "File version is newer (" + verFile.ToString() + ") " +
                            "than installed (" + verInstalled.ToString() + ")");
                        return false;
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("VcRedist", e.Message);
            }
            return false;
        }

        public void InstallVcRedist()
        {
            if (IsVcRedistInstalled())
            {
                return;
            }
            Helpers.ConsolePrint("InstallVcRedist", "Try install vcredist");
            try
            {
                var vcredistProcess = new Process

                {
                    StartInfo =
                {
                    FileName = "miners//vc_redist.x64.exe"
                }
                };

                vcredistProcess.StartInfo.Arguments = "/install /passive /norestart";
                vcredistProcess.StartInfo.UseShellExecute = false;
                vcredistProcess.StartInfo.CreateNoWindow = false;
                vcredistProcess.Start();
                vcredistProcess.WaitForExit();
                Helpers.ConsolePrint("InstallVcRedist", "vcredist install completed");
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("VcRedist", e.Message);
            }
        }

        private static void DeviceTelemetryTimer_Tick(object sender, EventArgs e)
        {
            if (WindowsDisplayAdapters.HasIntelVideoController())
            {
                IntelComputeDevice.SetTelemetry();
            }
            if (WindowsDisplayAdapters.HasAMDVideoController())
            {
                AmdComputeDevice.SetTelemetry();
            }
            if (WindowsDisplayAdapters.HasNvidiaVideoController())
            {
                GetNVMLData();
                //CudaComputeDevice.GetNVMLData();
            }
            Profiles.Profile.CheckShedule();
        }
        private void AutoStartTimerDelay_Tick(object sender, EventArgs e)
        {
            if (DownloadingInProgress) return;
            if (ConfigManager.GeneralConfig.AutoStartMining)
            {
                _AutoStartMiningDelay--;
                if (firstRun || _AutoStartMiningDelay < 1)
                {
                    _autostartTimerDelay.Stop();
                    _autostartTimerDelay = null;
                    buttonStopMining.Text = International.GetText("Form_Main_stop");
                    buttonStopMining.Refresh();
                    //AutoStartTimer_Tick(null, null);
                    return;
                }
                else
                {
                    //buttonStartMining.Enabled = false;
                    buttonStopMining.Enabled = true;
                    buttonStopMining.Text = International.GetText("Form_Main_stop") + " (" + _AutoStartMiningDelay.ToString() + ")";
                    buttonStartMining.Update();
                }
            }
            else
            {
                buttonStopMining.Enabled = false;
            }
        }
        private void AutoStartTimer_Tick(object sender, EventArgs e)
        {
            if (DownloadingInProgress) return;
            _autostartTimer.Stop();
            _autostartTimer = null;
            if (ConfigManager.GeneralConfig.AutoStartMining)
            {
                if (firstRun)
                {
                    if (_autostartTimerDelay != null)
                    {
                        _autostartTimerDelay.Stop();
                        _autostartTimerDelay = null;
                        buttonStopMining.Text = International.GetText("Form_Main_stop");
                    }
                    return;
                }
                // well this is started manually as we want it to start at runtime
                _isManuallyStarted = true;
                if (StartMining(false) != StartMiningReturnType.StartMining)
                {
                    _isManuallyStarted = false;
                    StopMining();
                }
            }
        }

        private void SetChildFormCenter(Form form)
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(Location.X + (Width - form.Width) / 2, Location.Y + (Height - form.Height) / 2);
        }

        private void Form_Main_Shown(object sender, EventArgs e)
        {
            try
            {
                if (!Directory.Exists("temp")) Directory.CreateDirectory("temp");
                DirectoryInfo dirInfo = new DirectoryInfo("temp/");

                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    file.Delete();
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("Temp Dir", ex.ToString());
            }

            if (this != null)
            {
                Rectangle screenSize = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                //if (ConfigManager.GeneralConfig.FormLeft + ConfigManager.GeneralConfig.FormWidth <= screenSize.Size.Width)
                {
                    if (ConfigManager.GeneralConfig.FormTop + ConfigManager.GeneralConfig.FormLeft >= 1)
                    {
                        this.Top = ConfigManager.GeneralConfig.FormTop;
                        this.Left = ConfigManager.GeneralConfig.FormLeft;
                    }

                    this.Width = ConfigManager.GeneralConfig.FormWidth;
                    //this.Height = ConfigManager.GeneralConfig.FormHeight;
                    this.Height = this.MinimumSize.Height + ConfigManager.GeneralConfig.DevicesCountIndex * 17 + 1;
                }
                /*
                else
                {
                    // this.Width = 660; // min width
                }
                */
            }

            if (!Configs.ConfigManager.GeneralConfig.MinimizeToTray)
            {
                WindowState = FormWindowState.Normal;
            }
            foreach (var lbl in this.Controls.OfType<Button>())
            {
                lbl.ForeColor = _textColor;
                lbl.FlatStyle = FlatStyle.Flat;
                lbl.FlatAppearance.BorderColor = _textColor;
                lbl.FlatAppearance.BorderSize = 1;
            }

            buttonLogo.FlatAppearance.BorderSize = 0;
            if (ConfigManager.GeneralConfig.ColorProfileIndex == 0 ||
                ConfigManager.GeneralConfig.ColorProfileIndex == 1 ||
                ConfigManager.GeneralConfig.ColorProfileIndex == 4 ||
                ConfigManager.GeneralConfig.ColorProfileIndex == 13)
            {
                buttonLogo.Image = Properties.Resources.ZergPoolLogo105;
            }
            else
            {
                buttonLogo.Image = Properties.Resources.ZergPoolLogo105;
            }
            devicesListViewEnableControl1.BackColor = SystemColors.ControlLightLight;

            this.Enabled = true;

            linkLabelCheckStats.LinkBehavior = LinkBehavior.HoverUnderline;
            linkLabelCheckStats.LinkColor = Color.DarkOliveGreen;
            linkLabelCheckStats.ActiveLinkColor = Color.Green;

            if (ConfigManager.GeneralConfig.ColorProfileIndex != 0)
            {
                if (this != null)
                {
                    this.BackColor = _backColor;
                    this.ForeColor = _foreColor;
                }

                if (ConfigManager.GeneralConfig.ColorProfileIndex == 14)
                {
                    linkLabelCheckStats.LinkColor = Color.DarkOliveGreen;
                    linkLabelCheckStats.ActiveLinkColor = Color.DarkCyan;
                }

                foreach (var lbl in this.Controls.OfType<GroupBox>()) lbl.BackColor = _backColor;

                foreach (var lbl in this.Controls.OfType<HScrollBar>())
                    lbl.BackColor = _backColor;
                foreach (var lbl in this.Controls.OfType<ListBox>()) lbl.BackColor = _backColor;
                foreach (var lbl in this.Controls.OfType<ListControl>()) lbl.BackColor = _backColor;
                foreach (var lbl in this.Controls.OfType<ListView>()) lbl.BackColor = _backColor;
                foreach (var lbl in this.Controls.OfType<ListViewItem>())
                {
                    lbl.BackColor = _backColor;
                    lbl.ForeColor = _textColor;
                }
                foreach (var lbl in this.Controls.OfType<StatusBar>())
                    lbl.BackColor = _backColor;
                foreach (var lbl in this.Controls.OfType<ComboBox>()) lbl.BackColor = _backColor;
                foreach (var lbl in this.Controls.OfType<ComboBox>()) lbl.ForeColor = _foreColor;

                foreach (var lbl in this.Controls.OfType<GroupBox>()) lbl.BackColor = _backColor;
                foreach (var lbl in this.Controls.OfType<GroupBox>()) lbl.ForeColor = _textColor;
                // foreach (var lbl in this.Controls.OfType<ComboBox>()) lbl.ForeColor = _foreColor;

                foreach (var lbl in this.Controls.OfType<TextBox>())
                {
                    lbl.BackColor = _backColor;
                    lbl.ForeColor = _foreColor;
                    lbl.BorderStyle = BorderStyle.FixedSingle;
                }

                try
                {
                    foreach (var lbl in this.Controls.OfType<StatusStrip>()) lbl.BackColor = _backColor;
                    foreach (var lbl in this.Controls.OfType<StatusStrip>()) lbl.ForeColor = _foreColor;
                    foreach (var lbl in this.Controls.OfType<ToolStripStatusLabel>()) lbl.BackColor = _backColor;
                    foreach (var lbl in this.Controls.OfType<ToolStripStatusLabel>()) lbl.ForeColor = _foreColor;
                }
                catch (Exception ex)
                {
                    Helpers.ConsolePrint("ToolStripStatusLabel", ex.ToString());
                }


                foreach (var lbl in this.Controls.OfType<Button>()) lbl.BackColor = _backColor;
                foreach (var lbl in this.Controls.OfType<Button>())
                {
                    lbl.ForeColor = _textColor;
                    lbl.FlatStyle = FlatStyle.Flat;
                    lbl.FlatAppearance.BorderColor = _textColor;
                    lbl.FlatAppearance.BorderSize = 1;
                }

                this.Enabled = true;
                buttonLogo.FlatAppearance.BorderSize = 0;
                //buttonBenchmark.Paint += dropShadow;

                foreach (var lbl in this.Controls.OfType<CheckBox>()) lbl.BackColor = _backColor;
                // DevicesListViewEnableControl.listViewDevices.BackColor = _backColor;
                devicesListViewEnableControl1.BackColor = _backColor;
                devicesListViewEnableControl1.ForeColor = _foreColor;

                foreach (var lbl in this.Controls.OfType<RichTextBox>()) lbl.BackColor = _backColor;
                foreach (var lbl in this.Controls.OfType<RichTextBox>()) lbl.ForeColor = _textColor;
            }

            this.Update();
            this.Refresh();
            // general loading indicator
            const int totalLoadSteps = 100;

            _loadingScreen = new Form_Loading(this,
                International.GetText("Form_Loading_label_LoadingText"),
                International.GetText("Form_Main_loadtext_CPU"), totalLoadSteps);

            SetChildFormCenter(_loadingScreen);
            _loadingScreen.Show();
            _loadingScreen.SetValueAndMsg(0, "Starting...");

            buttonBenchmark.Enabled = false;
            buttonChart.Enabled = false;
            buttonSettings.Enabled = false;
            buttonStartMining.Enabled = false;
            buttonStopMining.Enabled = false;

            _startupTimer = new Timer();
            _startupTimer.Tick += StartupTimer_Tick;
            _startupTimer.Interval = 200;
            _startupTimer.Start();
            textBoxWallet.Enabled = true;

            _deviceStatusTimer = new Timer();
            _deviceStatusTimer.Tick += DeviceStatusTimer_Tick;
            _deviceStatusTimer.Interval = 1000;
            _deviceStatusTimer.Start();

            _chartTimer = new Timer();
            _chartTimer.Tick += ChartTimer_Tick;
            _chartTimer.Interval = 1000 * 60;
            _chartTimer.Start();

            Form_Main.lastRigProfit.DateTime = DateTime.Now;
            /*
            if (!ConfigManager.GeneralConfig.ChartEnable)
            {
                Form_Main.lastRigProfit.totalRate = 0;
                Form_Main.lastRigProfit.currentProfitAPI = 0;
                Form_Main.lastRigProfit.currentProfit = 0;
                Form_Main.lastRigProfit.currentPower = 0;
                Form_Main.lastRigProfit.unpaidAmount = 0;
            }
            else
            {
                if (Form_Main.walletType.Equals("P2SH"))
                {
                    new Task(() => Stats.Stats.GetRigProfit()).Start();
                }
            }
            */
            Form_Main.RigProfits.Add(Form_Main.lastRigProfit);
            try
            {
                _loadingScreen.SetValueAndMsg(1, "Starting...");
            }
            catch (Exception ex)
            {

            }
            Application.DoEvents();
        }
        
        private static void DrawShadowSmooth(GraphicsPath gp, int intensity, int radius, Bitmap dest)
        {
            using (Graphics g = Graphics.FromImage(dest))
            {
                g.Clear(Color.Transparent);
                g.CompositingMode = CompositingMode.SourceCopy;
                double alpha = 0;
                double astep = 0;
                double astepstep = (double)intensity / radius / (radius / 2D);
                for (int thickness = radius; thickness > 0; thickness--)
                {
                    using (Pen p = new Pen(Color.FromArgb((int)alpha, 0, 0, 0), thickness))
                    {
                        p.LineJoin = LineJoin.Round;
                        g.DrawPath(p, gp);
                    }
                    alpha += astep;
                    astep += astepstep;
                }
            }
        }
        private void MarketTimer_Tick(object sender, EventArgs e)
        {
            ExchangeRateApi.GetBTCRate();
            ExchangeRateApi.GetFiatRate();
        }
        private void ChartTimer_Tick(object sender, EventArgs e)
        {
            Form_Main.lastRigProfit.DateTime = DateTime.Now;

            Form_Main.lastRigProfit.totalRate = Math.Round(MinersManager.GetTotalRate(), 9);
            Form_Main.lastRigProfit.totalPowerRate = totalPowerConsumptionBTC;
            Stats.Stats.GetWalletBalanceEx(null, null);

            Form_Main.RigProfits.Add(Form_Main.lastRigProfit);

            foreach (var RigProfit in Form_Main.RigProfits)
            {
                ChartDataAvail = RigProfit.currentProfitAPI + RigProfit.totalRate;
            }
        }
        private void FinalizeTimer_Tick(object sender, EventArgs e)
        {
            new Task(() => FinalizeTimer()).Start();
        }
        private void FinalizeTimer()
        {
            do
            {
                Thread.Sleep(500);
            } while (Uptime.Seconds != 55 && Uptime.Seconds != 25);
            AlgosProfitData.FinalizeAlgosProfitList();
        }

        private void UpdateSMATimer_Tick(object sender, EventArgs e)
        {
            GC.Collect(GC.MaxGeneration);
            //GC.Collect(GC.MaxGeneration);
            //GC.WaitForPendingFinalizers();
            Process currentProc = Process.GetCurrentProcess();
            double bytesInUse = currentProc.PrivateMemorySize64;
            Helpers.ConsolePrint("MEMORY", "Mem used: " + Math.Round(bytesInUse / 1048576, 2).ToString() + "MB");

            Stats.Stats.GetAlgos();
            AlgosProfitData.FinalizeAlgosProfitList();

            _updateTimerCount++;
            int period = 0;
            switch (ConfigManager.GeneralConfig.ProgramUpdateIndex)
            {
                case 0:
                    period = 60;
                    break;
                case 1:
                    period = 180;
                    break;
                case 2:
                    period = 360;
                    break;
                case 3:
                    period = 720;
                    break;
                case 4:
                    period = 1140;
                    break;
            }
            double mult = 60000 / _updateSMATimer.Interval;

            if (_updateTimerCount * mult >= period)
            {
                _updateTimerCount = 0;
                bool newver = false;
                try
                {
                    newver = CheckGithub();
                }
                catch (Exception er)
                {
                    Helpers.ConsolePrint("CheckGithub", er.ToString());
                    return;
                }

                if (ConfigManager.GeneralConfig.ProgramAutoUpdate && newver)
                {
                    Updater.Updater.Downloader(true);
                }
            }
            //***********
            _updateTimerRestartProgramCount++;
            int periodRestartProgram = 0;
            switch (ConfigManager.GeneralConfig.ProgramRestartIndex)
            {
                case 0:
                    periodRestartProgram = -1;
                    break;
                case 1:
                    periodRestartProgram = 12 * 60;
                    break;
                case 2:
                    periodRestartProgram = 24 * 60;
                    break;
                case 3:
                    periodRestartProgram = 72 * 60;
                    break;
                case 4:
                    periodRestartProgram = 168 * 60;
                    break;
            }
            if (periodRestartProgram < 0) return;
            if (_updateTimerRestartProgramCount >= periodRestartProgram)
            {
                MakeRestart(periodRestartProgram);
            }
        }

        public static void StopWinIODriver()
        {
            //srbminer driver
            var CMDconfigHandleWD = new Process

            {
                StartInfo =
                {
                    FileName = "sc.exe"
                }
            };

            CMDconfigHandleWD.StartInfo.Arguments = "stop winio";
            CMDconfigHandleWD.StartInfo.UseShellExecute = false;
            CMDconfigHandleWD.StartInfo.CreateNoWindow = true;
            CMDconfigHandleWD.Start();
        }


        public static void MakeRestart(int periodRestartProgram)
        {
            ProgramClosing = true;
            if (ConfigManager.GeneralConfig.EnableRigRemoteView)
            {
                Server.Listener(false);
            }
            if (ConfigManager.GeneralConfig.EnableAPI)
            {
                APIServer.Listener(false);
            }
            if (ConfigManager.GeneralConfig.ABEnableOverclock)
            {
                if (ConfigManager.GeneralConfig.ABDefaultProgramClosing)
                {
                    foreach (var cdev in ComputeDeviceManager.Available.Devices)
                    {
                        if (cdev.Enabled)
                        {
                            MSIAfterburner.ResetToDefaults(cdev.BusID, cdev.Uuid, ((AlgorithmType)cdev.AlgorithmID).ToString(), false);
                            MSIAfterburner.CommitChanges(false);
                        }
                    }
                    MSIAfterburner.Flush();
                    foreach (var cdev in ComputeDeviceManager.Available.Devices)
                    {
                        if (cdev.Enabled)
                        {
                            MSIAfterburner.ResetCurveLock(cdev.BusID, false);//check lock
                        }
                    }

                    if (MSIAfterburner.locked)
                    {
                        Thread.Sleep(4000);
                        foreach (var cdev in ComputeDeviceManager.Available.Devices)
                        {
                            if (cdev.Enabled)
                            {
                                if (MSIAfterburner.ResetCurveLock(cdev.BusID, true))//unlock
                                {
                                    MSIAfterburner.CommitChanges(false);
                                }
                            }
                        }
                        MSIAfterburner.locked = false;
                        MSIAfterburner.Flush();
                    }
                    Thread.Sleep(2000);
                }
            }
            StopWinIODriver();
            try
            {
                new Task(() => MinersManager.StopAllMiners()).Start();
                //Thread.Sleep(1000);
                //if (Miner._cooldownCheckTimer != null && Miner._cooldownCheckTimer.Enabled)
                  //  new Task(() => Miner._cooldownCheckTimer.Stop()).Start();
                MessageBoxManager.Unregister();
                ConfigManager.GeneralConfigFileCommit();
                Thread.Sleep(1000);

                try
                {
                    if (File.Exists("TEMP\\github.test")) File.Delete("TEMP\\github.test");
                }
                catch (Exception)
                {

                }
                //stop openhardwaremonitor
                    var CMDconfigHandleOHM = new Process

                    {
                        StartInfo =
                        {
                            FileName = "sc.exe"
                        }
                    };

                    CMDconfigHandleOHM.StartInfo.Arguments = "stop winring0_1_2_0";
                    CMDconfigHandleOHM.StartInfo.UseShellExecute = false;
                    CMDconfigHandleOHM.StartInfo.CreateNoWindow = true;
                    CMDconfigHandleOHM.Start();

                CMDconfigHandleOHM = new Process

                    {
                        StartInfo =
                        {
                            FileName = "sc.exe"
                        }
                    };
                /*
                    CMDconfigHandleOHM.StartInfo.Arguments = "stop R0ZergPoolMinerLegacy";
                    CMDconfigHandleOHM.StartInfo.UseShellExecute = false;
                    CMDconfigHandleOHM.StartInfo.CreateNoWindow = true;
                    CMDconfigHandleOHM.Start();
*/
                CMDconfigHandleOHM = new Process

                {
                    StartInfo =
                        {
                            FileName = "sc.exe"
                        }
                };
                /*
                CMDconfigHandleOHM.StartInfo.Arguments = "delete R0ZergPoolMinerLegacy";
                CMDconfigHandleOHM.StartInfo.UseShellExecute = false;
                CMDconfigHandleOHM.StartInfo.CreateNoWindow = true;
                CMDconfigHandleOHM.Start();
                */

                if (GetWinVer(Environment.OSVersion.Version) >= 10)
                {
                    var CMDconfigHandleWD = new Process

                    {
                        StartInfo =
                            {
                                FileName = "sc.exe"
                            }
                    };

                    CMDconfigHandleWD.StartInfo.Arguments = "stop WinDivert1.4";
                    CMDconfigHandleWD.StartInfo.UseShellExecute = false;
                    CMDconfigHandleWD.StartInfo.CreateNoWindow = true;
                    CMDconfigHandleWD.Start();
                }
                Thread.Sleep(500);
                Form_Benchmark.RunCMDAfterBenchmark();

                var RestartProgram = new ProcessStartInfo(Directory.GetCurrentDirectory() + "\\RestartProgram.cmd")
                {
                    WindowStyle = ProcessWindowStyle.Minimized
                };
                if (thisComputer is object) thisComputer.Close();
                Helpers.ConsolePrint("SheduleRestart", "Schedule or config changed restart program after " + (periodRestartProgram / 60).ToString() + "h");
                Process.Start(RestartProgram);


                //CloseChilds(Process.GetCurrentProcess());
                //Thread.Sleep(2);
                //System.Windows.Forms.Application.Restart();
                //Process.GetCurrentProcess().Kill();
                //System.Environment.Exit(1);
            }
            catch (Exception er)
            {
                Helpers.ConsolePrint("SheduleRestart", er.ToString());
                return;
            }
        }

        public bool CheckGithub()
        {
            //Form_Main.currentVersion = 0;//testing проверка загрузки программы
            Helpers.ConsolePrint("GITHUB", "Check new version");
            Helpers.ConsolePrint("GITHUB", "Current version: " + Form_Main.currentVersion.ToString());
            Helpers.ConsolePrint("GITHUB", "Current build: " + Form_Main.currentBuild.ToString());
            bool ret = CheckNewVersion();
            Helpers.ConsolePrint("GITHUB", "GITHUB Version: " + Form_Main.githubVersion.ToString());
            Helpers.ConsolePrint("GITHUB", "GITHUB Build: " + Form_Main.githubBuild.ToString());
            Helpers.ConsolePrint("GITLAB", "GITLAB Version: " + Form_Main.gitlabVersion.ToString());
            //SetVersion(ghv);
            return ret;
        }
        private bool CheckNewVersion()
        {
            return false;
            bool ret = false;
            Form_Main.githubVersion = Updater.Updater.GetGITHUBVersion();
            Form_Main.gitlabVersion = Updater.Updater.GetGITLABVersion();

            if (linkLabelNewVersion != null)
            {
                if (Form_Main.currentBuild < Form_Main.githubBuild)//testing
                {
                    Form_Main.NewVersionExist = true;
                    linkLabelNewVersion.Text = (string.Format(International.GetText("Form_Main_new_build_released").Replace("{0}", "{0}"), ""));
                    linkLabelNewVersion.Visible = true;
                    ret = true;
                }
                if (Form_Main.currentVersion < Form_Main.githubVersion)
                {
                    Form_Main.NewVersionExist = true;
                    linkLabelNewVersion.Text = (string.Format(International.GetText("Form_Main_new_version_released").Replace("v{0}", "{0}"), "Fork Fix " + Form_Main.githubVersion.ToString()));
                    linkLabelNewVersion.Visible = true;
                    return true;
                }
                if (Form_Main.currentVersion < Form_Main.gitlabVersion)
                {
                    Form_Main.NewVersionExist = true;
                    linkLabelNewVersion.Text = (string.Format(International.GetText("Form_Main_new_version_released").Replace("v{0}", "{0}"), "Fork Fix " + Form_Main.gitlabVersion.ToString()));
                    linkLabelNewVersion.Visible = true;
                    return true;
                }
                if (Form_Main.githubVersion <= 0 && Form_Main.gitlabVersion <= 0)
                {
                    Form_Main.NewVersionExist = false;
                    Helpers.ConsolePrint("CheckNewVersion", "FATAL ERROR! GITHUB and GITLAB down");
                    return false;
                }

            }
            return ret;
        }
        private async void MinerStatsCheck_Tick(object sender, EventArgs e)
        {
            ticks++;
            if (ticks > 5)
            {
                _minerStatsCheck.Interval = 1000 * 5;
            }
            if (!_deviceStatusTimer.Enabled & buttonStartMining.Enabled)
            {
                Helpers.ConsolePrint("ERROR", "_deviceStatusTimer fail");
                restartProgram();
            }
            await MinersManager.MinerStatsCheck();
        }

        private static void ComputeDevicesCheckTimer_Tick(object sender, EventArgs e)
        {
            int check = ComputeDeviceManager.Query.CheckVideoControllersCountMismath();
            if (check > -1 && CheckVideoControllersCount)
            {
                // less GPUs than before, ACT!
                try
                {
                    if (ConfigManager.GeneralConfig.RestartWindowsOnCUDA_GPU_Lost)
                    {
                        var onGpusLost = new ProcessStartInfo(Directory.GetCurrentDirectory() + "\\OnGPUsLost.bat")
                        {
                            WindowStyle = ProcessWindowStyle.Minimized
                        };
                        onGpusLost.Arguments = "1 " + check;
                        Helpers.ConsolePrint("ERROR", "Restart Windows due CUDA GPU#" + check.ToString() + " is lost");
                        Process.Start(onGpusLost);
                    }
                    if (ConfigManager.GeneralConfig.RestartDriverOnCUDA_GPU_Lost)
                    {
                        var onGpusLost = new ProcessStartInfo(Directory.GetCurrentDirectory() + "\\OnGPUsLost.bat")
                        {
                            WindowStyle = ProcessWindowStyle.Minimized
                        };
                        onGpusLost.Arguments = "2 " + check;
                        Helpers.ConsolePrint("ERROR", "Restart driver due CUDA GPU#" + check.ToString() + " is lost");
                        Form_Benchmark.RunCMDAfterBenchmark();
                        Thread.Sleep(1000);
                        Process.Start(onGpusLost);
                    }
                }
                catch (Exception ex)
                {
                    Helpers.ConsolePrint("ZergPool", "OnGPUsLost.bat error: " + ex.Message);
                }
            }
            CheckVideoControllersCount = check > -1;
        }

        private void InitFlowPanelStart()
        {
            flowLayoutPanelRates.Controls.Clear();
            // add for every cdev a
            foreach (var cdev in ComputeDeviceManager.Available.Devices)
            {
                if (cdev.Enabled)
                {
                    var newGroupProfitControl = new GroupProfitControl
                    {
                        Visible = false
                    };
                    flowLayoutPanelRates.Controls.Add(newGroupProfitControl);
                    flowLayoutPanelRates.Update();
                    Application.DoEvents();
                }
            }
        }

        public void ClearRatesAll()
        {
            HideNotProfitable();
            ClearRates(-1);
        }
        //67,[67,"1.0000000000e-07"] "paying": 1.012905863405184e-8
        //[67,"1.2872605844e-04" "paying": 0.000012809115794294396
        public void ClearRates(int groupCount)
        {
            if (InvokeRequired)
            {
                Invoke((Action)delegate { ClearRates(groupCount); });
                return;
            }
            if (flowLayoutPanelRates == null) return;
            if (_flowLayoutPanelVisibleCount != groupCount)
            {
                _flowLayoutPanelVisibleCount = groupCount;
                // hide some Controls
                var hideIndex = 0;
                foreach (var control in flowLayoutPanelRates.Controls)
                {
                    ((GroupProfitControl)control).Visible = hideIndex < groupCount;
                    ++hideIndex;
                    flowLayoutPanelRates.Update();
                    //Application.DoEvents();
                }
            }
            _flowLayoutPanelRatesIndex = 0;
            var visibleGroupCount = 1;
            if (groupCount > 0) visibleGroupCount += groupCount;
            double panelHeight = 0;
            var groupBox1Height = _emtpyGroupPanelHeight;
            if (flowLayoutPanelRates.Controls.Count > 0)
            {
                var control = flowLayoutPanelRates.Controls[0];
                panelHeight = (int)((GroupProfitControl)control).Size.Height * 1.1;
                groupBox1Height = (int)((visibleGroupCount) * panelHeight - panelHeight / 3.0f);
            }
            // MiningSession._runningGroupMiners = null;
            groupBox1.Size = new Size(groupBox1.Size.Width, groupBox1Height);

            groupBox1.Top = groupBox1Top;
            // set new height
            int newHeight = _mainFormHeight + groupBox1Height - (int)panelHeight / 2;
            //this.MaximumSize = new Size(-1, newHeight);
            // Form_Main.ActiveForm.MinimumSize.Height = newHeight;
            Size = new Size(Size.Width, newHeight + ConfigManager.GeneralConfig.DevicesCountIndex * 17 + 1);
        }


        public void AddRateInfo(string groupName, string deviceStringInfo, ApiData iApiData, double paying, double power,
           DateTime StartMinerTime, bool isApiGetException, string processTag, GroupMiner groupMiners, int groupCount)
        {
            //Helpers.ConsolePrint("trace", new System.Diagnostics.StackTrace().ToString());

            var apiGetExceptionString = isApiGetException ? " **" : "";
            string speedString = "";
            string algoName = iApiData.AlgorithmName;

            if (Form_additional_mining.isAlgoZIL(algoName, groupMiners.MinerBaseType, groupMiners.DeviceType) &&
                        ConfigManager.GeneralConfig.AdditionalMiningPlusSymbol)
            {
                algoName = algoName + "+";
            }

            if (isZilRound && iApiData.AlgorithmID == AlgorithmType.NONE)
            {
                algoName = "ZIL";
            }

            speedString = Helpers.FormatDualSpeedOutput(iApiData.Speed, iApiData.SecondarySpeed,
                iApiData.ThirdSpeed,
                iApiData.AlgorithmID, iApiData.SecondaryAlgorithmID, iApiData.ThirdAlgorithmID) +
                          algoName + apiGetExceptionString;

            speedString = speedString.Replace("--", "0.000 H/s ");

            if (iApiData.AlgorithmID == AlgorithmType.NONE &&
                iApiData.SecondaryAlgorithmID == AlgorithmType.NONE &&
                iApiData.ThirdAlgorithmID == AlgorithmType.NONE)
            {
                speedString = "...";
            }

            speedString = International.GetText("ListView_Speed") + " " + speedString;

            if (!ConfigManager.GeneralConfig.DecreasePowerCost)
            {
                power = 0;
            }

            string rateCurrencyString = "";
            if (ConfigManager.GeneralConfig.FiatCurrency)
            {
                rateCurrencyString = ExchangeRateApi
                                         .ConvertBTCToNationalCurrency((paying - power) / 1 * _factorTimeUnit)
                                         .ToString("F2", CultureInfo.InvariantCulture)
                                     + $" {ConfigManager.GeneralConfig.DisplayCurrency}/" +
                                     International.GetText(ConfigManager.GeneralConfig.TimeUnit.ToString());
            } else
            {
                rateCurrencyString = ExchangeRateApi
                                         .ConvertBTCToPayoutCurrency((paying - power) / 1 * _factorTimeUnit)
                                         .ToString("F2", CultureInfo.InvariantCulture)
                                     + $" {ConfigManager.GeneralConfig.PayoutCurrency}/" +
                                     International.GetText(ConfigManager.GeneralConfig.TimeUnit.ToString());
            }

            try
            {
                var rateBtcString = FormatPayingOutput(paying, power);
                if (_flowLayoutPanelRatesIndex >= groupCount) return;
                // flowLayoutPanelRatesIndex may be OOB, so catch
                ((GroupProfitControl)flowLayoutPanelRates.Controls[_flowLayoutPanelRatesIndex++])
                    .UpdateProfitStats(groupName, deviceStringInfo, speedString, StartMinerTime, rateBtcString, rateCurrencyString, processTag);

            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("AddRateInfo", ex.ToString());
            }
        }

        public void ShowNotProfitable(string msg)
        {
            if (ConfigManager.GeneralConfig.UseIFTTT)
            {
                if (!_isNotProfitable)
                {
                    _isNotProfitable = true;
                }
            }

            if (InvokeRequired)
            {
                Invoke((Action)delegate
               {
                   ShowNotProfitable(msg);
               });
            }
            else
            {
                label_NotProfitable.Visible = true;
                label_NotProfitable.Text = msg;
                label_NotProfitable.Invalidate();
            }
        }

        public void HideNotProfitable()
        {
            if (ConfigManager.GeneralConfig.UseIFTTT)
            {
                if (_isNotProfitable)
                {
                    _isNotProfitable = false;
                }
            }

            try
            {
                if (InvokeRequired)
                {
                    Invoke((Action)HideNotProfitable);
                }
                else
                {
                    label_NotProfitable.Visible = false;
                    label_NotProfitable.Invalidate();
                }
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("Exception: ", e.ToString());
            }
        }

        public void ForceMinerStatsUpdate()
        {
            try
            {
                new Task(() => MinerStatsCheck_Tick(null, null));
                // BeginInvoke((Action)(() =>
                //{
                // MinerStatsCheck_Tick(null, null);
                //}));
            }
            catch (Exception e)
            {
                Helpers.ConsolePrint("ZergPool", e.ToString());
            }
        }

        private void SummAllPower()
        {
            PowerAllDevices = 0;
            foreach (ComputeDevice computeDevice in Available.Devices)
            {
                PowerAllDevices += computeDevice.PowerUsage;// mem leak on drivers above 461
                //Thread.Sleep(500);
            }
        }
        private void UpdateGlobalRate()
        {
            try
            {
                double psuE = (double)ConfigManager.GeneralConfig.PowerPSU / 100;
                var totalRateBTC = MinersManager.GetTotalRate();

                //var totalRatePayoutCurrency = ExchangeRateApi.ConvertBTCToPayoutCurrency(totalRateBTC);

                var powerString = "";
                double TotalPower = 0;
                TotalPower = MinersManager.GetTotalPowerRate() + PowerAllDevices;
                double totalPower = (TotalPower + (int)ConfigManager.GeneralConfig.PowerMB) / psuE;
                totalPower = Math.Round(totalPower, 0);
                totalPowerConsumptionBTC = ExchangeRateApi.GetKwhPriceInBtc() * totalPower * 24 * _factorTimeUnit / 1000;//Вт
                totalPowerRatePayoutCurrency = ExchangeRateApi.ConvertBTCToPayoutCurrency(totalPowerConsumptionBTC);

                totalPowerRateNational = ExchangeRateApi.ConvertBTCToNationalCurrency(ExchangeRateApi.GetKwhPriceInBtc()) * 
                    totalPower * 24 * _factorTimeUnit / 1000;//Вт
                var powerMB = ExchangeRateApi.GetKwhPriceInBtc() * totalPower * 24 / 1000;//Вт
                double totalPowerRateDecBTC = 0;
                if (ConfigManager.GeneralConfig.DecreasePowerCost)
                {
                    totalPowerRateDecBTC = totalPowerConsumptionBTC;
                }

                if (ConfigManager.GeneralConfig.AutoScaleBTCValues &&
                    ExchangeRateApi.ConvertBTCToPayoutCurrency(totalRateBTC - totalPowerRateDecBTC) < 0.0001)
                {
                    if (totalPowerRatePayoutCurrency != 0)
                    {
                        powerString = "(-" + (ExchangeRateApi.ConvertBTCToPayoutCurrency(totalPowerConsumptionBTC * 1)).ToString("F5", CultureInfo.InvariantCulture) + ") ";
                    }
                    if (ConfigManager.GeneralConfig.DecreasePowerCost)
                    {
                        powerString = "";
                    }

                    toolStripStatusLabelBTCDayText.Text = powerString + " " +
                    "m" + ConfigManager.GeneralConfig.PayoutCurrency + "/" + 
                    International.GetText(ConfigManager.GeneralConfig.TimeUnit.ToString());
                    toolStripStatusLabelGlobalRateValue.Text =
                (ExchangeRateApi.ConvertBTCToPayoutCurrency(totalRateBTC - totalPowerRateDecBTC)).ToString("F5", CultureInfo.InvariantCulture);

                }
                else
                {
                    if (totalPowerRatePayoutCurrency != 0)
                    {
                        powerString = "(-" + (ExchangeRateApi.ConvertBTCToPayoutCurrency(totalPowerConsumptionBTC)).ToString("F5", CultureInfo.InvariantCulture) + ") ";
                    }
                    if (ConfigManager.GeneralConfig.DecreasePowerCost)
                    {
                        powerString = "";
                    }
                    toolStripStatusLabelBTCDayText.Text = powerString + " " +
                        ConfigManager.GeneralConfig.PayoutCurrency + "/" + 
                        International.GetText(ConfigManager.GeneralConfig.TimeUnit.ToString());
                    toolStripStatusLabelGlobalRateValue.Text =
                        (ExchangeRateApi.ConvertBTCToPayoutCurrency(totalRateBTC - totalPowerRateDecBTC)).ToString("F5", CultureInfo.InvariantCulture);
                }

                if (totalPowerRatePayoutCurrency != 0)
                {
                    powerString = "(-" + ExchangeRateApi.ConvertBTCToNationalCurrency((totalPowerConsumptionBTC) * 1)
                            .ToString("F2", CultureInfo.InvariantCulture) + ") ";
                    if (ConfigManager.GeneralConfig.DecreasePowerCost)
                    {
                        powerString = "";
                    }
                }
                else
                {
                    powerString = "";
                }

                toolStripStatusLabelBTCDayValue.Text = ExchangeRateApi.ConvertBTCToNationalCurrency(
                    (totalRateBTC - totalPowerRateDecBTC) * 1)
                    .ToString("F2", CultureInfo.InvariantCulture);

                toolStripStatusLabelBalanceText.Text = powerString + (ExchangeRateApi.ActiveDisplayCurrency + "/") +
                International.GetText(ConfigManager.GeneralConfig.TimeUnit.ToString()) + "   " +
                 International.GetText("Form_Main_balance") + ":";

                BalanceCallback(null, null);
                toolStripStatusLabel_power1.Text = International.GetText("Form_Main_Power1");
                toolStripStatusLabel_power2.Text = totalPower.ToString();
                toolStripStatusLabel_power3.Text = International.GetText("Form_Main_Power3");

                TotalPowerConsumption = TotalPowerConsumption + totalPower / 3600;
                TotalBTC = TotalBTC + (((totalRateBTC) / 24) / _factorTimeUnit) / 3600;
                TotalActualBTC = TotalActualBTC + ((((TotalActualProfitability) / 24) / _factorTimeUnit) / 3600) * Algorithm.Mult;

                //*******
                APIServer.balance = Stats.Stats.Balance * 1;
                APIServer.Rate = totalRateBTC;
                APIServer.Power = totalPower;
                APIServer.TotalPower = TotalPowerConsumption / 1;
                APIServer.PowerRate = totalPowerRatePayoutCurrency;
                APIServer.PowerRateFiat = totalPowerRateNational;
                APIServer.TotalPowerSpentFiat = TotalPowerConsumption * 0.001 * GetKwhPrice();

                if (ConfigManager.GeneralConfig.ShowTotalPower)
                {
                    toolStripStatusLabel_power4.Text = International.GetText("Form_Main_Power4");
                    toolStripStatusLabel_power5.Text = (TotalPowerConsumption / 1).ToString("F1");
                    toolStripStatusLabel_power6.Text = International.GetText("Form_Main_Power6");
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                Helpers.ConsolePrint("UpdateGlobalRate error: ", e.ToString());
            }
        }


        private void BalanceCallback(object sender, EventArgs e)
        {
            try
            {
                var balance = Stats.Stats.Balance;
                //if (balance > 0)

                if (ConfigManager.GeneralConfig.AutoScaleBTCValues && balance < 0.0001)
                {
                    toolStripStatusLabelBalanceBTCCode.Text = "m" + ConfigManager.GeneralConfig.PayoutCurrency;
                    toolStripStatusLabelBalanceBTCValue.Text =
                        (balance * 1).ToString("F5", CultureInfo.InvariantCulture);
                }
                else
                {
                    toolStripStatusLabelBalanceBTCCode.Text = ConfigManager.GeneralConfig.PayoutCurrency;
                    toolStripStatusLabelBalanceBTCValue.Text = balance.ToString("F6", CultureInfo.InvariantCulture);
                }

                var amount = ExchangeRateApi.ConvertPayoutCurrencyToNationalCurrency(balance);

                toolStripStatusLabelBalanceDollarText.Text = amount.ToString("F2", CultureInfo.InvariantCulture);
                if (ConfigManager.GeneralConfig.FiatCurrency)
                {
                    toolStripStatusLabelBalanceDollarValue.Text = $"({ExchangeRateApi.ActiveDisplayCurrency})";
                } else
                {
                    toolStripStatusLabelBalanceDollarValue.Text = $"(USD)";
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("Balance update", ex.ToString());
            }
            //Helpers.ConsolePrint("ZergPool", "Balance updated");
        }


        private void ExchangeCallback(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)UpdateExchange);
            }
            else
            {
                UpdateExchange();
            }
        }

        private void UpdateExchange()
        {
            var br = ExchangeRateApi.GetPayoutCurrencyExchangeRate();
            var currencyRate = International.GetText("BenchmarkRatioRateN_A");
            if (br > 0)
            {
                currencyRate = ExchangeRateApi.ConvertPayoutCurrencyToNationalCurrency(1).ToString();
            }
            try
            {
                if (!ConfigManager.GeneralConfig.DisableTooltips)
                {
                    string tooltip = "";
                    if (ConfigManager.GeneralConfig.FiatCurrency)
                    {
                        tooltip = ("1 " + ConfigManager.GeneralConfig.PayoutCurrency +
                        $" = {currencyRate} {ExchangeRateApi.ActiveDisplayCurrency}");
                    } else
                    {
                        tooltip = ("1 " + ConfigManager.GeneralConfig.PayoutCurrency +
                                                $" = {currencyRate} USD");
                    }
                    toolTip1.AutoPopDelay = 3000;
                    toolTip1.InitialDelay = 1000;
                    toolTip1.ReshowDelay = 5000;
                    toolTip1.ShowAlways = false;
                    toolTip1.IsBalloon = false;
                    toolTip1.SetToolTip(this.statusStrip1, tooltip);
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("UpdateExchange", ex.ToString());
            }
        }


        private bool VerifyMiningAddress(bool showError)
        {
            return true;
            if (true)
            {
                if (!BitcoinAddress.ValidateBitcoinAddress(textBoxWallet.Text.Trim()) && showError)
                {
                    var result = MessageBox.Show(International.GetText("Form_Main_msgbox_InvalidBTCAddressMsg"),
                        International.GetText("Error_with_Exclamation"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                    textBoxWallet.Focus();
                    return false;
                }
            }
            if (!BitcoinAddress.ValidateWorkerName(textBoxWorkerName.Text.Trim()) && showError)
            {
                var result = MessageBox.Show(International.GetText("Form_Main_msgbox_InvalidWorkerNameMsg"),
                    International.GetText("Error_with_Exclamation"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                textBoxWorkerName.Focus();
                return false;
            }
            return true;
        }

        private void LinkLabelCheckStats_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!VerifyMiningAddress(true)) return;

            Process.Start(Links.miningStats + ConfigManager.GeneralConfig.Wallet);
        }

        private void LinkLabelNewVersion_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            settings = new Form_Settings();
            try
            {
                //   SetChildFormCenter(settings);
                settings.tabControlGeneral.SelectedTab = settings.tabPageAbout;
                settings.ShowDialog();
            }
            catch (Exception er)
            {
                Helpers.ConsolePrint("settings", er.ToString());
            }
        }

        [DllImport("ntdll.dll", SetLastError = true)]
        internal static extern uint RtlGetVersion(out OsVersionInfo versionInformation); // return type should be the NtStatus enum

        [StructLayout(LayoutKind.Sequential)]
        internal struct OsVersionInfo
        {
            private readonly uint OsVersionInfoSize;

            internal readonly uint MajorVersion;
            internal readonly uint MinorVersion;

            internal readonly uint BuildNumber;

            internal readonly uint PlatformId;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            private readonly string CSDVersion;
        }
        public static double GetWinVer(Version ver)
        {
            /*
            RtlGetVersion(out var rv);
            var MajorVersion = rv.MajorVersion;
            var MinorVersion = rv.MinorVersion;
            var BuildNumber = rv.BuildNumber;
            */

            if (ver.Major == 6 & ver.Minor == 1)
                return 7;
            else if (ver.Major == 6 & ver.Minor == 2)
                return 8;
            else if (ver.Major == 6 & ver.Minor == 3)
                return 8.1;
            else if (ver.Build >= 22000)
                return 11;
            else if (ver.Major == 10)
                return 10;
            else return -1;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ProgramClosing = true;
            if (AlgorithmSwitchingManager._smaCheckTimer != null)
            {
                AlgorithmSwitchingManager._smaCheckTimer.Stop();
                AlgorithmSwitchingManager._smaCheckTimer.Dispose();
                AlgorithmSwitchingManager._smaCheckTimer = null;
            }

            if (ConfigManager.GeneralConfig.Use_OpenHardwareMonitor)
            {
                thisComputer.Close();
                Helpers.ConsolePrint("LibreHardwareMonitor", "Close library");
            }

            devicesListViewEnableControl1.SaveColumns();
            if (this != null)
            {
                if (ConfigManager.GeneralConfig.Save_windows_size_and_position)
                {
                    ConfigManager.GeneralConfig.FormWidth = this.Width;
                    ConfigManager.GeneralConfig.FormHeight = this.Height;
                    if (this.Top + this.Left >= 1)
                    {
                        ConfigManager.GeneralConfig.FormTop = this.Top;
                        ConfigManager.GeneralConfig.FormLeft = this.Left;
                    }
                }
            }

            if (_deviceStatusTimer != null)
            {
                _deviceStatusTimer.Stop();
                _deviceStatusTimer.Dispose();
            }

            if (ConfigManager.GeneralConfig.EnableRigRemoteView)
            {
                Server.Listener(false);
            }
            if (ConfigManager.GeneralConfig.EnableAPI)
            {
                APIServer.Listener(false);
            }

            if (ConfigManager.GeneralConfig.ABEnableOverclock)
            {
                if (ConfigManager.GeneralConfig.ABDefaultProgramClosing)
                {
                    foreach (var cdev in ComputeDeviceManager.Available.Devices)
                    {
                        if (cdev.Enabled)
                        {
                            MSIAfterburner.ResetToDefaults(cdev.BusID, cdev.Uuid, ((AlgorithmType)cdev.AlgorithmID).ToString(), false);
                            MSIAfterburner.CommitChanges(false);
                        }
                    }
                    MSIAfterburner.Flush();
                    foreach (var cdev in ComputeDeviceManager.Available.Devices)
                    {
                        if (cdev.Enabled)
                        {
                            MSIAfterburner.ResetCurveLock(cdev.BusID, false);//check lock
                        }
                    }

                    if (MSIAfterburner.locked)
                    {
                        Thread.Sleep(4000);
                        foreach (var cdev in ComputeDeviceManager.Available.Devices)
                        {
                            if (cdev.Enabled)
                            {
                                if (MSIAfterburner.ResetCurveLock(cdev.BusID, true))//unlock
                                {
                                    MSIAfterburner.CommitChanges(false);
                                }
                            }
                        }
                        MSIAfterburner.locked = false;
                        MSIAfterburner.Flush();
                    }
                    Thread.Sleep(2000);
                }
            }

            MinersManager.StopAllMiners();
            //if (Miner._cooldownCheckTimer != null && Miner._cooldownCheckTimer.Enabled) Miner._cooldownCheckTimer.Stop();
            MessageBoxManager.Unregister();
            ConfigManager.GeneralConfigFileCommit();
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo("TEMP\\");

                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    if (file.Name.Contains("tmp") || file.Name.Contains("pkt") || file.Name.Contains("dmp") || file.Name.Contains("github.test") ||
                        file.Name.Contains("MinerOptionPackage_"))
                    {
                        file.Delete();
                    }
                }
            }
            catch (Exception)
            {

            }

            try
            {
                foreach (var process in Process.GetProcessesByName("NvidiaGPUGetDataHost"))
                {
                    process.Kill();
                }

            }
            catch (Exception)
            {

            }

            try
            {
                foreach (var process in Process.GetProcessesByName("MinerLegacyForkFixMonitor"))
                {
                    process.Kill();
                }

            }
            catch (Exception)
            {

            }

            try
            {
                foreach (var process in Process.GetProcessesByName("device_detection"))
                {
                    process.Kill();
                }

            }
            catch (Exception)
            {

            }

            //stop openhardwaremonitor
                var CMDconfigHandleOHM = new Process

                {
                    StartInfo =
                {
                    FileName = "sc.exe"
                }
                };

                CMDconfigHandleOHM.StartInfo.Arguments = "stop winring0_1_2_0";
                CMDconfigHandleOHM.StartInfo.UseShellExecute = false;
                CMDconfigHandleOHM.StartInfo.CreateNoWindow = true;
                CMDconfigHandleOHM.Start();

            CMDconfigHandleOHM = new Process

                {
                    StartInfo =
                {
                    FileName = "sc.exe"
                }
                };

            CMDconfigHandleOHM = new Process
            {
                StartInfo =
                {
                    FileName = "sc.exe"
                }
            };

            if (GetWinVer(Environment.OSVersion.Version) >= 10)
            {
                var CMDconfigHandleWD = new Process

                {
                    StartInfo =
                {
                    FileName = "sc.exe"
                }
                };

                CMDconfigHandleWD.StartInfo.Arguments = "stop WinDivert1.4";
                CMDconfigHandleWD.StartInfo.UseShellExecute = false;
                CMDconfigHandleWD.StartInfo.CreateNoWindow = true;
                CMDconfigHandleWD.Start();
            }
            StopWinIODriver();
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher
                        ("Select * From Win32_Process Where ParentProcessID=" + mainproc.Id.ToString());
                ManagementObjectCollection moc = searcher.Get();
                foreach (ManagementObject mo in moc)
                {

                    Process proc = Process.GetProcessById(Convert.ToInt32(mo["ProcessID"]));
                    //Helpers.ConsolePrint("Closing", Convert.ToInt32(mo["ProcessID"]).ToString() + " " + proc.ProcessName);
                    if (!Convert.ToInt32(mo["ProcessID"]).ToString().Contains("ZergPoolMinerLegacy"))
                    {
                        if (proc != null)
                        {
                            proc.Kill();
                        }
                    }


                }
                Process mproc = Process.GetProcessById(mainproc.Id);
                Helpers.ConsolePrint("Closing", mproc.Id.ToString() + " " + mproc.ProcessName);
                //mproc.Kill();
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("Closing", ex.ToString());
            }
        }

        private void ButtonBenchmark_Click(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            Application.DoEvents();
            if (DownloadingInProgress) return;
            _benchmarkForm = new Form_Benchmark();
            //  SetChildFormCenter(_benchmarkForm);
            _benchmarkForm.ShowDialog();
            var startMining = _benchmarkForm.StartMining;
            _benchmarkForm = null;

            InitMainConfigGuiData();
            if (startMining)
            {
                ButtonStartMining_Click(null, null);
            }
        }


        private void ButtonSettings_Click(object sender, EventArgs e)
        {
            if (DownloadingInProgress) return;
            settings = new Form_Settings();
            try
            {
                //   SetChildFormCenter(settings);
                settings.ShowDialog();
            }
            catch (Exception er)
            {
                Helpers.ConsolePrint("settings", er.ToString());
            }
            if (settings.IsChange && settings.IsChangeSaved && settings.IsRestartNeeded)
            {
                MessageBox.Show(
                    International.GetText("Form_Main_Restart_Required_Msg"),
                    International.GetText("Form_Main_Restart_Required_Title"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                MakeRestart(0);
            }
            else if (settings.IsChange && settings.IsChangeSaved)
            {
                InitLocalization();
                InitMainConfigGuiData();
            }
        }

        private void ButtonStartMining_Click(object sender, EventArgs e)
        {
            if (DownloadingInProgress) return;

            _isManuallyStarted = true;
            if (StartMining(true) == StartMiningReturnType.ShowNoMining)
            {
                _isManuallyStarted = false;
                StopMining();
                MessageBox.Show(International.GetText("Form_Main_StartMiningReturnedFalse"),
                    International.GetText("Warning_with_Exclamation"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void ButtonStopMining_Click(object sender, EventArgs e)
        {
            if (DownloadingInProgress) return;
            adaptiveRunning = false;
            firstRun = true;
            _isManuallyStarted = false;
            StopMining();
        }

        private string FormatPayingOutput(double paying, double power)
        {
            string ret;
            if (!ConfigManager.GeneralConfig.DecreasePowerCost)
            {
                power = 0;
            }
            if (ConfigManager.GeneralConfig.AutoScaleBTCValues && ExchangeRateApi.ConvertBTCToPayoutCurrency(paying / 1) < 0.0001)
                ret = (ExchangeRateApi.ConvertBTCToPayoutCurrency(paying - power) * _factorTimeUnit).ToString("F5", CultureInfo.InvariantCulture) +
                    " m" + ConfigManager.GeneralConfig.PayoutCurrency + "/" +
                      International.GetText(ConfigManager.GeneralConfig.TimeUnit.ToString());
            else
                ret = (ExchangeRateApi.ConvertBTCToPayoutCurrency(paying - power) / 1 * _factorTimeUnit).ToString("F5", CultureInfo.InvariantCulture) +
                    " " + ConfigManager.GeneralConfig.PayoutCurrency + "/" +
                      International.GetText(ConfigManager.GeneralConfig.TimeUnit.ToString());

            return ret;
        }

        private void ButtonLogo_Click(object sender, EventArgs e)
        {
            Process.Start(Links.VisitUrl);
        }

        private void ButtonChart_Click(object sender, EventArgs e)
        {
            if (DownloadingInProgress) return;

            var chart = new Form_RigProfitChart();
            try
            {
                Form_RigProfitChartRunning = true;
                buttonChart.Enabled = false;
                chart.Show();
            }
            catch (Exception er)
            {
                Helpers.ConsolePrint("chart", er.ToString());
            }
        }


        // Minimize to system tray if MinimizeToTray is set to true
        private void Form1_Resize(object sender, EventArgs e)
        {
            try
            {
                foreach (var control in flowLayoutPanelRates.Controls)
                {
                    ((GroupProfitControl)control).Width = this.Width - 41;
                }
                //((GroupProfitControl)control).Width = 520;
            } catch
            {

            }

            notifyIcon1.Icon = Properties.Resources.logo;
            notifyIcon1.Text = Application.ProductName + " v" + Application.ProductVersion +
                               "\nDouble-click to restore..";

            if (ConfigManager.GeneralConfig.MinimizeToTray && FormWindowState.Minimized == WindowState)
            {
                notifyIcon1.Visible = true;
                Hide();
            }
            buttonStartMining.Refresh();
            buttonStopMining.Refresh();
        }

        // Restore ZergPoolMiner from the system tray
        private void NotifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        ///////////////////////////////////////
        // Miner control functions
        public enum StartMiningReturnType
        {
            StartMining,
            ShowNoMining,
            IgnoreMsg
        }


        public StartMiningReturnType StartMining(bool showWarnings)
        {
            _NeedMiningStart = true;
            try
            {
                MiningStarted = true;
                if (_autostartTimerDelay != null)
                {
                    _autostartTimerDelay.Stop();
                    _autostartTimerDelay = null;
                    buttonStopMining.Text = International.GetText("Form_Main_stop");
                }
                if (_autostartTimer != null)
                {
                    _autostartTimer.Stop();
                    _autostartTimer = null;
                }
            } catch (Exception ex)
            {
                Helpers.ConsolePrint("StartMining", ex.ToString());
            }

            if (textBoxWallet.Text.Trim().Equals(""))
            {
                if (showWarnings)
                {
                    var result = MessageBox.Show(International.GetText("Form_Main_DemoModeMsg"),
                        International.GetText("Form_Main_DemoModeTitle"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        _demoMode = true;
                        textBoxWallet.Text = International.GetText("Form_Main_DemoModeLabel");
                    }
                    else
                    {
                        return StartMiningReturnType.IgnoreMsg;
                    }
                }
                else
                {
                    return StartMiningReturnType.IgnoreMsg;
                }
            }
            else if (!VerifyMiningAddress(true))
            {
                return StartMiningReturnType.IgnoreMsg;
            }
            var hasData = AlgosProfitData.HasData;
            if (!showWarnings)
            {
                for (var i = 0; i < 10; i++)
                {
                    if (hasData) break;
                    Thread.Sleep(1000);
                    hasData = AlgosProfitData.HasData;
                    Helpers.ConsolePrint("ZergPool", $"After {i}s has data: {hasData}");
                }
            }
            if (!hasData)
            {
                Helpers.ConsolePrint("ZergPool", "No data received within timeout");
                if (showWarnings)
                {
                    MessageBox.Show(International.GetText("Form_Main_msgbox_NullZergPoolDataMsg"),
                        International.GetText("Error_with_Exclamation"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return StartMiningReturnType.IgnoreMsg;
            }
            textBoxWallet.Enabled = false;
            textBoxTreshold.Enabled = false;
            textBoxWorkerName.Enabled = false;
            textBoxPayoutCurrency.Enabled = false;
            comboBoxRegion.Enabled = false;
            Form_Main.smaCount = 0;
            buttonStartMining.Enabled = false;
            devicesListViewEnableControl1.IsMining = true;
            buttonStopMining.Enabled = true;

            // Disable profitable notification on start
            _isNotProfitable = false;
            
            InitFlowPanelStart();
            ClearRatesAll();
            bool isMining;
            _PayoutTreshold = ",pl=" + ConfigManager.GeneralConfig.PayoutCurrencyTreshold.ToString();
            if (_demoMode)
            {
                _PayoutTreshold = ",pl=0";
            }

            isMining = MinersManager.StartInitialize(this, textBoxWorkerName.Text.Trim(),
                ConfigManager.GeneralConfig.Wallet, ConfigManager.GeneralConfig.PayoutCurrency);

            if (!_demoMode) ConfigManager.GeneralConfigFileCommit();
            _minerStatsCheck.Start();

            if (ConfigManager.GeneralConfig.RestartDriverOnCUDA_GPU_Lost || ConfigManager.GeneralConfig.RestartWindowsOnCUDA_GPU_Lost)
            {
                _computeDevicesCheckTimer = new SystemTimer();
                _computeDevicesCheckTimer.Elapsed += ComputeDevicesCheckTimer_Tick;
                _computeDevicesCheckTimer.Interval = 60000;

                _computeDevicesCheckTimer.Start();
            }
            return isMining ? StartMiningReturnType.StartMining : StartMiningReturnType.ShowNoMining;
        }

        WebSocketSharp.WebSocketState _oldState = WebSocketSharp.WebSocketState.Closed;

        private void restartProgram()
        {
            MakeRestart(0);
        }


        private void DeviceStatusTimer_Tick(object sender, EventArgs e)
        {
            if (Profiles.Profile.GetProfilesCount() > 1)
            {
                groupBox1.Text = International.GetText("Form_Main_Group_Device_Rates") +
                                " (" + International.GetText("Form_Benchmark_titleProfile") +
                                " " + ConfigManager.GeneralConfig.ProfileName + ")";
            }

            if (ConfigManager.GeneralConfig.EnableRigRemoteView)
            {
                try
                {
                    if (!Directory.Exists("HTML")) Directory.CreateDirectory("HTML");

                    Rectangle bounds = Screen.GetBounds(Point.Empty);
                    using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                    {
                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                        }
                        using (MemoryStream ms = new MemoryStream())
                        {
                            bitmap.Save(ms, ImageFormat.Png);
                            desktop = ms.ToArray();
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Helpers.ConsolePrint("DeviceStatusTimer_Tick", ex.ToString());
                }
            }

            byte[] b1 = { (byte)254, (byte)254, (byte)254 };//1,2 - reserved
            try
            {
                if (ConfigManager.GeneralConfig.ProgramMonitoring)
                {
                    using (MemoryMappedViewAccessor writer = MonitorSharedMemory.CreateViewAccessor(0, 3))
                    {
                        string s = label_Uptime.Text.Substring(label_Uptime.Text.Length - 2, 2);
                        if (int.TryParse(s, out int sec))
                        {
                            b1[0] = (byte)sec;
                            writer.WriteArray<byte>(0, b1, 0, 3);
                        }
                    }
                }
                if (ConfigManager.GeneralConfig.ShowUptime)
                {
                    var timenow = DateTime.Now;
                    Uptime = timenow.Subtract(StartTime);
                    label_Uptime.Visible = true;
                    label_Uptime.Text = International.GetText("Form_Main_Uptime") + " " +
                                        Uptime.ToString(@"d\ \d\a\y\s\ hh\:mm\:ss");
                        //" Блок зилики: " + ZilCount.ToString();blockzil
                }

                if (ConfigManager.GeneralConfig.Use_OpenHardwareMonitor)
                {
                    try
                    {
                        if (Form_Main.thisComputer != null)
                        {
                            foreach (var hardware in Form_Main.thisComputer.Hardware)
                            {
                                if (hardware is object &&
                                    (hardware.HardwareType == HardwareType.GpuAmd || hardware.HardwareType == HardwareType.Cpu))
                                {
                                    hardware.Update();
                                }
                            }
                        }
                    } catch (Exception ex)
                    {
                        Helpers.ConsolePrint("DeviceStatusTimer_Tick", ex.ToString());
                    }
                }
                if (DeviceStatusTimer_FirstTick)
                {

                }
                DeviceStatusTimer_FirstTick = true;
                ExchangeCallback(null, null);
                UpdateGlobalRate();

                if (needRestart)
                {
                    needRestart = false;
                    restartProgram();
                }
                devicesListViewEnableControl1.SetComputeDevicesStatus(ComputeDeviceManager.Available.Devices);
            }

            catch (Exception ex)
            {
                Helpers.ConsolePrint("DeviceStatusTimer_Tick error: ", ex.ToString());
                Thread.Sleep(500);
            }
            if (NVMLDriverError > 10)
            {
                NVMLDriverError = 0;
                try
                {
                    var onGpusLost = new ProcessStartInfo(Directory.GetCurrentDirectory() + "\\OnGPUsLost.bat")
                    {
                        WindowStyle = ProcessWindowStyle.Minimized
                    };
                    onGpusLost.Arguments = "2 " + "_NVML";
                    Helpers.ConsolePrint("ERROR", "Restart driver due NVML error");
                    Form_Benchmark.RunCMDAfterBenchmark();
                    Thread.Sleep(1000);
                    Process.Start(onGpusLost);
                }
                catch (Exception ex)
                {
                    Helpers.ConsolePrint("DeviceStatusTimer_Tick error: ", ex.ToString());
                }
            }
        }

        private static void GetNVMLData()
        {
            if (!ComputeDeviceManager.Available.HasNvidia)
            {
                return;
            }
            uint devCount = 0;
            uint _dev = 0;
            uint _power = 0u;
            uint _fan = 0u;
            uint _load = 0u;
            uint _loadMem = 0u;
            uint _temp = 0u;
            uint _tempMem = 0u;
            int size = Marshal.SizeOf(_dev) + Marshal.SizeOf(_power) + Marshal.SizeOf(_fan) + Marshal.SizeOf(_load) + Marshal.SizeOf(_loadMem) + Marshal.SizeOf(_temp) + Marshal.SizeOf(_tempMem);
            try
            {
                MemoryMappedFile sharedMemory = MemoryMappedFile.OpenExisting("NvidiaGPUGetDataHost", MemoryMappedFileRights.Read);
                using (MemoryMappedViewAccessor reader = sharedMemory.CreateViewAccessor(0, Marshal.SizeOf(devCount), MemoryMappedFileAccess.Read))
                {
                    devCount = reader.ReadUInt32(0);
                }
                NvData d = new NvData();
                ComputeDeviceManager.CudaDevicesCountFromNVMLHost = (int)devCount;
                gpuList.Clear();
                for (int dev = 0; dev < devCount; dev++)
                {
                    using (MemoryMappedViewAccessor reader = sharedMemory.CreateViewAccessor(0, size * devCount + Marshal.SizeOf(devCount), MemoryMappedFileAccess.Read))
                    {
                        _dev = reader.ReadUInt32(size * dev + Marshal.SizeOf(devCount));
                        _power = reader.ReadUInt32(size * dev + Marshal.SizeOf(devCount) + Marshal.SizeOf(dev));
                        _fan = reader.ReadUInt32(size * dev + Marshal.SizeOf(devCount) + Marshal.SizeOf(dev) + Marshal.SizeOf(_power));
                        _load = reader.ReadUInt32(size * dev + Marshal.SizeOf(devCount) + Marshal.SizeOf(dev) + Marshal.SizeOf(_power) + Marshal.SizeOf(_fan));
                        _loadMem = reader.ReadUInt32(size * dev + Marshal.SizeOf(devCount) + Marshal.SizeOf(dev) + Marshal.SizeOf(_power) + Marshal.SizeOf(_fan) + Marshal.SizeOf(_load));
                        _temp = reader.ReadUInt32(size * dev + Marshal.SizeOf(devCount) + Marshal.SizeOf(dev) + Marshal.SizeOf(_power) + Marshal.SizeOf(_fan) + Marshal.SizeOf(_load) + Marshal.SizeOf(_loadMem));
                        _tempMem = reader.ReadUInt32(size * dev + Marshal.SizeOf(devCount) + Marshal.SizeOf(dev) + Marshal.SizeOf(_power) + Marshal.SizeOf(_fan) + Marshal.SizeOf(_load) + Marshal.SizeOf(_loadMem) + Marshal.SizeOf(_temp));
                        /*
                        Helpers.ConsolePrint("GetNVMLData", "dev: " + dev.ToString() + " _dev: " + _dev.ToString() +
                        " _power: " + _power.ToString() + " _fan: " + _fan.ToString() + " _load: " + _load.ToString() +
                        " _temp: " + _temp.ToString() +
                        " _tempMem: " + _tempMem.ToString());
                        */
                        d.nGpu = _dev;
                        d.power = _power;
                        d.fan = _fan;
                        d.load = _load;
                        d.loadMem = _loadMem;
                        d.temp = _temp;
                        d.tempMem = _tempMem;
                        gpuList.Add(d);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Helpers.ConsolePrint("NVML", "Error! UnauthorizedAccessException. devCount=" + devCount.ToString() + " AvailNVGpus=" + ComputeDeviceManager.Available.AvailNVGpus.ToString());
                try
                {
                    foreach (var process in Process.GetProcessesByName("NvidiaGPUGetDataHost"))
                    {
                        process.Kill();
                    }

                }
                catch (Exception)
                {

                }
                Thread.Sleep(500);
            }
            catch (FileNotFoundException)
            {
                if (MemoryMappedFileError > 5)
                {
                    MemoryMappedFileError = 0;
                    Helpers.ConsolePrint("NVML", "Error! MemoryMappedFile not found " + NVMLDriverError.ToString());
                    if (File.Exists("common\\NvidiaGPUGetDataHost.exe"))
                    {
                        var MonitorProc = new Process
                        {
                            StartInfo = { FileName = "common\\NvidiaGPUGetDataHost.exe" }
                        };

                        MonitorProc.StartInfo.UseShellExecute = false;
                        MonitorProc.StartInfo.CreateNoWindow = true;
                        if (MonitorProc.Start())
                        {
                            Helpers.ConsolePrint("NvidiaGPUGetDataHost", "Starting OK");
                            NVMLDriverError++;
                        }
                        else
                        {
                            Helpers.ConsolePrint("NvidiaGPUGetDataHost", "Starting ERROR");
                        }
                    }
                }
                MemoryMappedFileError++;
                return;
            }
        }

        internal static object RawDeserialize(byte[] rawdatas, Type anytype)
        {
            int num1 = Marshal.SizeOf(anytype);
            if (num1 > rawdatas.Length)
                return (object)null;
            IntPtr num2 = Marshal.AllocHGlobal(num1);
            Marshal.Copy(rawdatas, 0, num2, num1);
            object structure = Marshal.PtrToStructure(num2, anytype);
            Marshal.FreeHGlobal(num2);
            return structure;
        }
        public void StopMining()
        {
            MiningStarted = false;
            ticks = 0;
            _minerStatsCheck.Interval = 1000;
            Form_Main.smaCount = 0;
            AlgorithmSwitchingManager.Stop();

            _minerStatsCheck.Stop();
            _computeDevicesCheckTimer?.Stop();

            _isNotProfitable = false;

            MinersManager.StopAllMiners();
            MiningSession.FuncAttached = false;

            if (ConfigManager.GeneralConfig.ABEnableOverclock)
            {
                if (ConfigManager.GeneralConfig.ABDefaultMiningStopped)
                {
                    foreach (var cdev in ComputeDeviceManager.Available.Devices)
                    {
                        if (cdev.Enabled)
                        {
                            MSIAfterburner.ResetToDefaults(cdev.BusID, cdev.Uuid, ((AlgorithmType)cdev.AlgorithmID).ToString(), false);
                            MSIAfterburner.CommitChanges(false);
                            Thread.Sleep(100);
                        }
                    }
                    MSIAfterburner.Flush();
                    foreach (var cdev in ComputeDeviceManager.Available.Devices)
                    {
                        if (cdev.Enabled)
                        {
                            MSIAfterburner.ResetCurveLock(cdev.BusID, false);//check lock
                        }
                    }

                    if (MSIAfterburner.locked)
                    {
                        Thread.Sleep(4000);
                        foreach (var cdev in ComputeDeviceManager.Available.Devices)
                        {
                            if (cdev.Enabled)
                            {
                                if (MSIAfterburner.ResetCurveLock(cdev.BusID, true))//unlock
                                {
                                    MSIAfterburner.CommitChanges(false);
                                }
                            }
                        }
                        MSIAfterburner.locked = false;
                        MSIAfterburner.Flush();
                    }
                    Thread.Sleep(2000);
                }
            }

            textBoxWallet.Enabled = true;
            textBoxTreshold.Enabled = true;
            textBoxPayoutCurrency.Enabled = true;
            textBoxWorkerName.Enabled = true;
            comboBoxRegion.Enabled = true;
            buttonBenchmark.Enabled = true;

            buttonSettings.Enabled = true;
            devicesListViewEnableControl1.IsMining = false;
            buttonStartMining.Enabled = true;
            buttonStopMining.Enabled = false;

            if (_demoMode)
            {
                _demoMode = false;
                textBoxWallet.Text = ConfigManager.GeneralConfig.Wallet;
            }
        }

        private void comboBoxLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConfigManager.GeneralConfig.ServiceLocation = comboBoxRegion.SelectedIndex;
            ConfigManager.GeneralConfigFileCommit();
            try
            {
                Globals.MiningLocation[Globals.MiningLocation.Length - 1] =
                    regionList[ConfigManager.GeneralConfig.ServiceLocation] + "zergpool.com";
                //ArrayRearrangeAfterItemMove(Globals.MiningLocation, 0, Globals.MiningLocation.Length - 1);
            } catch (Exception ex)
            {
                Helpers.ConsolePrint("comboBoxLocation_SelectedIndexChanged", ex.ToString());
            }
        }

        private void comboBoxLocation_DrawItem(object sender, DrawItemEventArgs e)
        {
            var cmb = (ComboBox)sender;
            if (cmb == null) return;


            e.DrawBackground();

            // change background color
            var bc = new SolidBrush(_backColor);
            var fc = new SolidBrush(_foreColor);
            var wc = new SolidBrush(_windowColor);
            var gr = new SolidBrush(Color.Gray);
            e.Graphics.FillRectangle(bc, e.Bounds);


            // change foreground color
            Brush brush = ((e.State & DrawItemState.Selected) > 0) ? fc : gr;
            if (e.Index >= 0)
            {
                e.Graphics.DrawString(cmb.Items[e.Index].ToString(), cmb.Font, brush, e.Bounds);
                e.DrawFocusRectangle();
            }
        }


        private void devicesListViewEnableControl1_Load(object sender, EventArgs e)
        {

        }

        private void buttonStopMining_EnabledChanged(object sender, EventArgs e)
        {
            if (ConfigManager.GeneralConfig.ColorProfileIndex != 0)
            {
                buttonStopMining.ForeColor = buttonStopMining.Enabled == true ? Form_Main._foreColor : Color.Gray;
                buttonStopMining.BackColor = buttonStopMining.Enabled == true ? Form_Main._backColor : Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            }
        }

        private void buttonStopMining_Paint(object sender, PaintEventArgs e)
        {

        }

        private void buttonStartMining_EnabledChanged(object sender, EventArgs e)
        {
            if (ConfigManager.GeneralConfig.ColorProfileIndex != 0)
            {
                buttonStartMining.ForeColor = buttonStartMining.Enabled == true ? Form_Main._foreColor : Color.Gray;
                buttonStartMining.BackColor = buttonStartMining.Enabled == true ? Form_Main._backColor : Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            }
        }

        private void buttonStartMining_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBoxBTCAddress_new_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void flowLayoutPanelRates_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form_Main_ResizeBegin(object sender, EventArgs e)
        {
            FormMainMoved = true;
        }

        private void Form_Main_ResizeEnd(object sender, EventArgs e)
        {
            FormMainMoved = false;
            ConfigManager.GeneralConfig.FormWidth = this.Width;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStripStatusLabel_power4_MouseHover(object sender, EventArgs e)
        {

        }

        private void statusStrip1_MouseHover(object sender, EventArgs e)
        {
            if (ConfigManager.GeneralConfig.DisableTooltips)
            {
                return;
            }
            string ctooltip = "";
            if (ConfigManager.GeneralConfig.FiatCurrency)
            {
                ctooltip = International.GetText("Form_Main_TotalLocalProfit") +
                    ExchangeRateApi.ConvertBTCToNationalCurrency(TotalBTC).ToString("F2") +
                    " " + ExchangeRateApi.ActiveDisplayCurrency;
            }
            else
            {
                ctooltip = International.GetText("Form_Main_TotalLocalProfit") +
                                    ExchangeRateApi.ConvertBTCToPayoutCurrency(TotalBTC).ToString("F9") +
                                    " " + ConfigManager.GeneralConfig.PayoutCurrency;
            }
            ctooltip += "\r\n";

            if (ConfigManager.GeneralConfig.FiatCurrency)
            {
                ctooltip += International.GetText("Form_Main_TotalActualProfit") +
                    ExchangeRateApi.ConvertBTCToNationalCurrency(TotalActualBTC).ToString("F2") + " " +
                    ConfigManager.GeneralConfig.DisplayCurrency;
            }
            else
            {
                ctooltip += International.GetText("Form_Main_TotalActualProfit") +
                    ExchangeRateApi.ConvertBTCToPayoutCurrency(TotalActualBTC).ToString("F9") + " " +
                    ConfigManager.GeneralConfig.PayoutCurrency;
            }
            ctooltip += "\r\n";


            if (ConfigManager.GeneralConfig.ShowTotalPower)
            {
                if (ConfigManager.GeneralConfig.FiatCurrency)
                {
                    ctooltip += string.Format(International.GetText("Form_Main_TotalPowerConsumptionCost"),
                        (TotalPowerConsumption * 0.001 * GetKwhPrice()).ToString("F2"),
                        ExchangeRateApi.ActiveDisplayCurrency);
                }
                else
                {
                    ctooltip += string.Format(International.GetText("Form_Main_TotalPowerConsumptionCost"),
                                            (TotalPowerConsumption * 0.001 * GetKwhPrice()).ToString("F2"),
                                            ConfigManager.GeneralConfig.PayoutCurrency);
                }
                ctooltip += "\r\n";
            }

            toolTipStatus.AutoPopDelay = 5000;
            toolTipStatus.InitialDelay = 1000;
            toolTipStatus.ReshowDelay = 5000;
            toolTipStatus.ShowAlways = true;
            toolTipStatus.IsBalloon = true;
            toolTipStatus.SetToolTip(this.statusStrip1, ctooltip);
        }

        private void textBoxWorkerName_TextChanged(object sender, EventArgs e)
        {

        }
        public static Color _grey = Color.FromArgb(150, 150, 150);
        public static void DrawGroupBox(GroupBox box, Graphics g, Color textColor, Color borderColor)
        {
            if (box != null)
            {
                Brush textBrush = new SolidBrush(textColor);
                Brush borderBrush = new SolidBrush(borderColor);
                Pen borderPen = new Pen(borderBrush);
                SizeF strSize = g.MeasureString(box.Text, box.Font);
                Rectangle rect = new Rectangle(box.ClientRectangle.X,
                                               box.ClientRectangle.Y + (int)(strSize.Height / 2),
                                               box.ClientRectangle.Width - 1,
                                               box.ClientRectangle.Height - (int)(strSize.Height / 2) - 1);
                var PaddingLeft = 4;
                // Clear text and border
                g.Clear(Form_Main._backColor);
                // Draw text
                g.DrawString(box.Text, box.Font, textBrush, box.Padding.Left + PaddingLeft, 0);
                // Drawing Border
                //Left
                g.DrawLine(borderPen, rect.Location, new Point(rect.X, rect.Y + rect.Height));
                //Right
                g.DrawLine(borderPen, new Point(rect.X + rect.Width, rect.Y), new Point(rect.X + rect.Width, rect.Y + rect.Height));
                //Bottom
                g.DrawLine(borderPen, new Point(rect.X, rect.Y + rect.Height), new Point(rect.X + rect.Width, rect.Y + rect.Height));
                //Top1
                g.DrawLine(borderPen, new Point(rect.X, rect.Y), new Point(rect.X + box.Padding.Left + PaddingLeft, rect.Y));
                //Top2
                g.DrawLine(borderPen, new Point(rect.X + box.Padding.Left + PaddingLeft + (int)(strSize.Width), rect.Y), new Point(rect.X + rect.Width, rect.Y));
            }
        }

        private void groupBox1_Paint(object sender, PaintEventArgs e)
        {
            if (ConfigManager.GeneralConfig.ColorProfileIndex != 14) return;
            GroupBox box = sender as GroupBox;
            Form_Main.DrawGroupBox(box, e.Graphics, Form_Main._foreColor, Form_Main._grey);
        }

        private void Form_Main_Paint(object sender, PaintEventArgs e)
        {
            if (shadowBmp == null || shadowBmp.Size != this.Size)
            {
                shadowBmp?.Dispose();
                shadowBmp = new Bitmap(this.Width, this.Height, PixelFormat.Format32bppArgb);
            }
            foreach (Control control in shadowControls)
            {
                using (GraphicsPath gp = new GraphicsPath())
                {
                    gp.AddRectangle(new Rectangle(control.Location.X + 6, control.Location.Y + 6, 
                        control.Size.Width - 6, control.Size.Height - 6));
                    DrawShadowSmooth(gp, 100, 16, shadowBmp);
                }
                e.Graphics.DrawImage(shadowBmp, new Point(0, 0));
            }
        }
    }

    static class TimeSpanExtensions
    {
        static public bool IsBetween(this TimeSpan time,
                                      TimeSpan startTime, TimeSpan endTime)
        {
            if (endTime == startTime)
            {
                return true;
            }

            if (endTime < startTime)
            {
                return time <= endTime ||
                    time >= startTime;
            }

            return time >= startTime &&
                time <= endTime;
        }
    }
    public class MyWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = 5 * 1000;
            return w;
        }
    }
    
}
