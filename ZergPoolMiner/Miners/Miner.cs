using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ZergPoolMiner.Algorithms;
using ZergPoolMiner.Configs;
using ZergPoolMiner.Devices;
using ZergPoolMiner.Forms;
using ZergPoolMiner.Interfaces;
using ZergPoolMiner.Miners;
using ZergPoolMiner.Miners.Grouping;
using ZergPoolMiner.Stats;
using ZergPoolMinerLegacy.Common.Enums;
using ZergPoolMinerLegacy.Overclock;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;
using ZergPoolMinerLegacy.UUID;

namespace ZergPoolMiner
{
    public class ApiData
    {
        public AlgorithmType AlgorithmID;
        public AlgorithmType SecondaryAlgorithmID;
        public AlgorithmType ThirdAlgorithmID;
        public string AlgorithmName;
        public string AlgorithmNameCustom;
        public string Coin;
        public double Speed;
        public double SecondarySpeed;
        public double ThirdSpeed;
        public double PowerUsage;
        public bool ZilRound;

        public ApiData(AlgorithmType algorithmID, AlgorithmType secondaryAlgorithmID = AlgorithmType.NONE, MiningPair mpairs = null)
        {
            AlgorithmID = algorithmID;
            SecondaryAlgorithmID = secondaryAlgorithmID;

            if (mpairs == null)
            {
                AlgorithmName = AlgorithmNames.GetName(DualAlgorithmID());
                Coin = "None";
            }
            else
            {
                if (mpairs.Algorithm is DualAlgorithm dualAlg)
                {
                    AlgorithmName = dualAlg.DualAlgorithmNameCustom;
                    Coin = dualAlg.CurrentMiningCoin;
                }
                else
                {
                    AlgorithmName = mpairs.Algorithm.AlgorithmNameCustom;
                    Coin = mpairs.Algorithm.CurrentMiningCoin;
                }
            }
            Speed = 0.0;
            SecondarySpeed = 0.0;
            PowerUsage = 0.0;
        }
        public AlgorithmType DualAlgorithmID()
        {
            /*
            if (AlgorithmID == AlgorithmType.Autolykos)
            {
                switch (SecondaryAlgorithmID)
                {
                    case AlgorithmType.DaggerHashimoto:
                        return AlgorithmType.AutolykosZil;
                }
            }
            */
            
            return AlgorithmID;
        }

    }

    //
    public class MinerPidData
    {
        public string MinerBinPath;
        public int Pid = -1;
    }

    public abstract class Miner
    {
        // MinerIDCount used to identify miners creation
        protected static long MinerIDCount { get; private set; }

        public NhmConectionType ConectionType { get; protected set; }

        // used to identify miner instance
        protected readonly long MinerID;

        private string _minerTag;
        public static string MinerDeviceName { get; set; }

        protected int ApiPort { get; set; }

        // if miner has no API bind port for reading curentlly only CryptoNight on ccminer
        public bool IsApiReadException { get; protected set; }

        public bool IsNeverHideMiningWindow { get; protected set; }

        // mining algorithm stuff
        protected bool IsInit { get; private set; }

        public MiningSetup MiningSetup { get; protected set; }

        // sgminer/zcash claymore workaround
        protected bool IsKillAllUsedMinerProcs { get; set; }


        public bool IsRunning { get; protected set; }
        public static bool IsRunningNew { get; protected set; }
        protected string Path { get; private set; }

        protected string LastCommandLine { get; set; }

        // TODO check this
        protected double PreviousTotalMH;

        // the defaults will be
        protected string WorkingDirectory { get; private set; }

        protected string MinerExeName { get; private set; }
        protected MinerProcess ProcessHandle;
        private MinerPidData _currentPidData;
        private readonly List<MinerPidData> _allPidData = new List<MinerPidData>();

        // Benchmark stuff
        public bool BenchmarkSignalQuit;

        public bool BenchmarkSignalHanged;
        private Stopwatch _benchmarkTimeOutStopWatch;
        public bool BenchmarkSignalTimedout;
        protected bool BenchmarkSignalFinnished;
        protected IBenchmarkComunicator BenchmarkComunicator;
        protected bool OnBenchmarkCompleteCalled;
        protected Algorithm BenchmarkAlgorithm { get; set; }
        public BenchmarkProcessStatus BenchmarkProcessStatus { get; protected set; }
        protected string BenchmarkProcessPath { get; set; }
        protected Process BenchmarkHandle { get; set; }
        protected Exception BenchmarkException;
        protected int BenchmarkTimeInSeconds;

        private string _benchmarkLogPath = "";
        protected List<string> BenchLines;

        protected bool TimeoutStandard;


        // TODO maybe set for individual miner cooldown/retries logic variables
        // this replaces MinerAPIGraceSeconds(AMD)
        private const int MinCooldownTimeInMilliseconds = 5 * 1000; // 30 seconds for gminer
        //private const int _MIN_CooldownTimeInMilliseconds = 1000; // TESTING

        //private const int _MAX_CooldownTimeInMilliseconds = 60 * 1000; // 1 minute max, whole waiting time 75seconds
        public int _maxCooldownTimeInMilliseconds; // = GetMaxCooldownTimeInMilliseconds();

        // protected abstract int GetMaxCooldownTimeInMilliseconds();
        private Timer _cooldownCheckTimer;
        protected MinerApiReadStatus CurrentMinerReadStatus { get; set; }
        private int _currentCooldownTimeInSeconds = MinCooldownTimeInMilliseconds;
        private int _currentCooldownTimeInSecondsLeft = MinCooldownTimeInMilliseconds;
        private int CooldownCheck = 0;

        private bool _isEnded;

        public bool IsUpdatingApi = false;
        public int TicksForApiUpdate = 0;

        protected const string HttpHeaderDelimiter = "\r\n\r\n";

        protected bool IsMultiType;
        public static string BenchmarkStringAdd = "";
        public static string InBenchmark = "";
        public bool needChildRestart;

        private int _benchmarkTimeWait = 120;
        private int benchmarkTimeWait = 120;
        private double _power = 0.0d;
        private double _powerUsage = 0;
        private double BenchmarkSpeed = 0.0d;
        private double BenchmarkSpeedSecond = 0.0d;

        protected virtual int GetMaxCooldownTimeInMilliseconds()
        {
            return 60 * 1000 * 10;  // 10 min
        }

        protected Miner(string minerDeviceName)
        {
            ConectionType = NhmConectionType.STRATUM_TCP;
            MiningSetup = new MiningSetup(null);
            IsInit = false;
            MinerID = MinerIDCount++;
            Miner.MinerDeviceName = minerDeviceName;
            //MinerDeviceName = minerDeviceName;
            WorkingDirectory = "";

            IsRunning = false;
            IsRunningNew = IsRunning;
            PreviousTotalMH = 0.0;

            LastCommandLine = "";

            IsApiReadException = false;
            // Only set minimize if hide is false (specific miners will override true after)
            IsNeverHideMiningWindow = ConfigManager.GeneralConfig.MinimizeMiningWindows &&
                                      !ConfigManager.GeneralConfig.HideMiningWindows;
            IsKillAllUsedMinerProcs = false;
            _maxCooldownTimeInMilliseconds = GetMaxCooldownTimeInMilliseconds();
            //
            Helpers.ConsolePrint(MinerTag(), "NEW MINER CREATED");
        }

        ~Miner()
        {
            // free the port
            MinersApiPortsManager.RemovePort(ApiPort);
            //DHClientsStop();
            Helpers.ConsolePrint(MinerTag(), "MINER DESTROYED");
        }

        protected void SetWorkingDirAndProgName(string fullPath)
        {
            WorkingDirectory = "";
            Path = fullPath;
            var lastIndex = fullPath.LastIndexOf("\\") + 1;
            if (lastIndex > 0)
            {
                WorkingDirectory = fullPath.Substring(0, lastIndex);
                MinerExeName = fullPath.Substring(lastIndex);
            }
        }

        private void SetApiPort()
        {
            if (IsInit)
            {
                var minerBase = MiningSetup.MiningPairs[0].Algorithm.MinerBaseType;
                var algoType = MiningSetup.MiningPairs[0].Algorithm.ZergPoolID;
                var devtype = MiningSetup.MiningPairs[0].Device.DeviceType;
                var path = MiningSetup.MinerPath;
                var reservedPorts = MinersSettingsManager.GetPortsListFor(minerBase, path, algoType);
                ApiPort = -1; // not set
                foreach (var reservedPort in reservedPorts)
                {
                    if (MinersApiPortsManager.IsPortAvaliable(reservedPort))
                    {
                        if (minerBase.Equals("hsrneoscrypt"))
                        {
                            ApiPort = 4001;
                        }
                        else
                        {
                            ApiPort = reservedPort;
                        }
                        break;
                    }
                }
                if (minerBase.ToString().Equals("hsrneoscrypt"))
                {
                    ApiPort = 4001;
                }
                else
                {
                    ApiPort = MinersApiPortsManager.GetAvaliablePort();
                }
            }
        }


        public virtual void InitMiningSetup(MiningSetup miningSetup)
        {
            MiningSetup = miningSetup;
            IsInit = MiningSetup.IsInit;
            SetApiPort();
            SetWorkingDirAndProgName(MiningSetup.MinerPath);
            //Thread.Sleep(Math.Max(ConfigManager.GeneralConfig.MinerRestartDelayMS, 500));
        }

        public void InitBenchmarkSetup(MiningPair benchmarkPair)
        {
            InitMiningSetup(new MiningSetup(new List<MiningPair>()
            {
                benchmarkPair
            }));
            BenchmarkAlgorithm = benchmarkPair.Algorithm;
        }
        

        // TAG for identifying miner
        public string MinerTag()
        {
            MinerDeviceName = MiningSetup.MinerName;
            if (_minerTag == null)
            {
                const string mask = "{0}-MINER_ID({1})-DEVICE_IDs({2})";
                // no devices set
                if (!IsInit)
                {
                    return string.Format(mask, MinerDeviceName, MinerID, "NOT_SET");
                }

                // contains ids
                var ids = MiningSetup.MiningPairs.Select(cdevs => cdevs.Device.ID.ToString()).ToList();
                _minerTag = string.Format(mask, MinerDeviceName, MinerID, string.Join(",", ids));
            }

            return _minerTag;
        }

        private static string ProcessTag(MinerPidData pidData)
        {
            return $"[pid({pidData.Pid})|bin({pidData.MinerBinPath})]";
        }

        public string ProcessTag()
        {
            if (_currentPidData == null)
            {
                Helpers.ConsolePrint("ProcessTag", "PidData is NULL. Restart program");
                Stop(MinerStopType.END); // stop miner first
                Thread.Sleep(Math.Max(ConfigManager.GeneralConfig.MinerRestartDelayMS, 500));
                Form_Main.MakeRestart(0);
                return "PidData is NULL";
            } else
            {
                return ProcessTag(_currentPidData);
            }
            return "unknown";
        }

        private static int ChildProcess(MinerPidData pidData)
        {
            return GetChildProcess(pidData.Pid);
        }
        public int ChildProcess()
        {
            return _currentPidData == null ? -1 : ChildProcess(_currentPidData);
        }

        private static int GetParentProcess(int Id)
        {
            int parentPid = 0;
            using (ManagementObject mo = new ManagementObject("win32_process.handle='" + Id.ToString() + "'"))
            {
                mo.Get();
                parentPid = Convert.ToInt32(mo["ParentProcessId"]);
            }
            return parentPid;
        }

        public static int GetChildProcess(int ProcessId, string fname = "miner")
        {
            Process[] localByName = Process.GetProcessesByName(fname);
            foreach (var processName in localByName)
            {
                int t = Process.GetProcessById(processName.Id).Id;
                int p = GetParentProcess(t);
                if (p == ProcessId)
                {
                    return t;
                }
            }
            return -1;
        }

        public void KillAllUsedMinerProcesses()
        {
            var toRemovePidData = new List<MinerPidData>();
            Helpers.ConsolePrint(MinerTag(), "Trying to close all miner processes for this instance:");
            var algo = (int)MiningSetup.CurrentAlgorithmType;
            string strPlatform = "";
            foreach (var pair in MiningSetup.MiningPairs)
            {
                pair.Device.MiningHashrate = 0;
                pair.Device.MiningHashrateSecond = 0;
                pair.Device.MiningHashrateThird = 0;
                pair.Device.MinerName = "";
                pair.Device.State = DeviceState.Stopped;

                pair.Device.AlgorithmID = (int)AlgorithmType.NONE;
                pair.Device.SecondAlgorithmID = (int)AlgorithmType.NONE;
                pair.Device.ThirdAlgorithmID = (int)AlgorithmType.NONE;

                if (pair.Device.DeviceType == DeviceType.NVIDIA)
                {
                    strPlatform = "NVIDIA";
                }
                else if (pair.Device.DeviceType == DeviceType.AMD)
                {
                    strPlatform = "AMD";
                }
                else if (pair.Device.DeviceType == DeviceType.CPU)
                {
                    strPlatform = "CPU";
                }
            }

            foreach (var pidData in _allPidData)
            {
                try
                {
                    var process = Process.GetProcessById(pidData.Pid);
                    if (pidData.MinerBinPath.Contains(process.ProcessName))
                    {
                        Helpers.ConsolePrint(MinerTag(), $"Trying to close {ProcessTag(pidData)}");
                        try
                        {
                            process.CloseMainWindow();
                            //process.Kill();
                            process.Close();
                            //process.WaitForExit(1000 * 20);
                        }
                        catch (InvalidOperationException ioex)
                        {
                            Helpers.ConsolePrint(MinerTag(),
                                $"InvalidOperationException closing {ProcessTag(pidData)}, exMsg {ioex.Message}");
                        }
                        catch (Exception e)
                        {
                            Helpers.ConsolePrint(MinerTag(),
                                $"Exception closing {ProcessTag(pidData)}, exMsg {e.Message}");
                        }
                    }
                }
                catch (Exception e)
                {
                    toRemovePidData.Add(pidData);
                    Helpers.ConsolePrint(MinerTag(), $"Nothing to close {ProcessTag(pidData)}, exMsg {e.Message}");
                }
            }

            _allPidData.RemoveAll(x => toRemovePidData.Contains(x));
        }

        public abstract void Start(string wallet, string password);

        protected abstract void _Stop(MinerStopType willswitch);

        public virtual void Stop(MinerStopType willswitch = MinerStopType.SWITCH)
        {
            var sortedMinerPairs = MiningSetup.MiningPairs.OrderBy(pair => pair.Device.IDByBus).ToList();
            foreach (var mPair in sortedMinerPairs)
            {
                mPair.Device.MiningHashrate = 0;
                mPair.Device.MiningHashrateSecond = 0;
            }

            _cooldownCheckTimer?.Stop();
            _Stop(willswitch);
            PreviousTotalMH = 0.0;
            IsRunning = false;
            IsRunningNew = IsRunning;
            RunCMDBeforeOrAfterMining(false);
        }

        public void End()
        {
            _isEnded = true;
            Stop(MinerStopType.FORCE_END);
        }
        protected void KillProcessAndChildren(int pid)
        {
            // Cannot close 'system idle process'.
            if (pid == 0)
            {
                return;
            }
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher
                        ("Select * From Win32_Process Where ParentProcessID=" + pid);
                ManagementObjectCollection moc = searcher.Get();

                foreach (ManagementObject mo in moc)
                {
                    KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
                }
            }
            catch (Exception er)
            {
                Helpers.ConsolePrint("KillProcessAndChildren", er.ToString());
            }
            finally
            {
                KillAllUsedMinerProcesses();
            }

        }
        protected void Stop_cpu_ccminer_sgminer_nheqminer(MinerStopType willswitch)
        {
            var algo = (int)MiningSetup.CurrentAlgorithmType;
            string strPlatform = "";
            foreach (var pair in MiningSetup.MiningPairs)
            {
                pair.Device.MiningHashrate = 0;
                pair.Device.MiningHashrateSecond = 0;
                pair.Device.MiningHashrateThird = 0;
                pair.Device.MinerName = "";
                pair.Device.State = DeviceState.Stopped;

                pair.Device.AlgorithmID = (int)AlgorithmType.NONE;
                pair.Device.SecondAlgorithmID = (int)AlgorithmType.NONE;
                pair.Device.ThirdAlgorithmID = (int)AlgorithmType.NONE;

                if (pair.Device.DeviceType == DeviceType.NVIDIA)
                {
                    strPlatform = "NVIDIA";
                }
                else if (pair.Device.DeviceType == DeviceType.AMD)
                {
                    strPlatform = "AMD";
                }
                else if (pair.Device.DeviceType == DeviceType.CPU)
                {
                    strPlatform = "CPU";
                }
            }
            if (IsRunning)
            {
                Helpers.ConsolePrint(MinerTag(), ProcessTag() + " Shutting down miner");
            }

            if (ProcessHandle != null)
            {
                ProcessHandle._bRunning = false;
                ProcessHandle.ExitEvent = null;
                int k = ProcessTag().IndexOf("pid(");
                int i = ProcessTag().IndexOf(")|bin");
                var cpid = ProcessTag().Substring(k + 4, i - k - 4).Trim();
                int pid = int.Parse(cpid, CultureInfo.InvariantCulture);

                
                    Helpers.ConsolePrint(MinerTag(), ProcessTag() + " SendCtrlC to stop miner");
                    try { ProcessHandle.SendCtrlC((uint)Process.GetCurrentProcess().Id); } catch { }
                    Thread.Sleep(1000);

                KillProcessAndChildren(pid);

                try
                {
                    if (ProcessHandle is object)
                    {
                        if (ProcessHandle != null)
                        {
                            Helpers.ConsolePrint(MinerTag(), ProcessTag() + " Try force kill miner");
                            ProcessHandle.Kill();
                        }
                    }
                } catch
                {

                }
                //try { ProcessHandle.SendCtrlC((uint)Process.GetCurrentProcess().Id); } catch { }
                if (ProcessHandle != null)
                {
                    ProcessHandle.Close();
                    ProcessHandle = null;
                }

                if (IsKillAllUsedMinerProcs) KillAllUsedMinerProcesses();
            }
        }

        public static bool PiDExist(int processId)
        {
            return true;
            try
            {
                Process[] allProcessesOnLocalMachine = Process.GetProcesses();
                foreach (Process process in allProcessesOnLocalMachine)
                {
                    if (process.Id == processId) return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }

        protected virtual string GetDevicesCommandString()
        {
            var deviceStringCommand = " ";

            var ids = MiningSetup.MiningPairs.Select(mPair => mPair.Device.ID.ToString()).ToList();
            deviceStringCommand += string.Join(",", ids);

            return deviceStringCommand;
        }

        #region BENCHMARK DE-COUPLED Decoupled benchmarking routines

        public int BenchmarkTimeoutInSeconds(int timeInSeconds)
        {
            return timeInSeconds + 120; // wait time plus two minutes
        }

        // TODO remove algorithm
        protected abstract string BenchmarkCreateCommandLine(Algorithm algorithm, int time);

        // The benchmark config and algorithm must guarantee that they are compatible with miner
        // we guarantee algorithm is supported
        // we will not have empty benchmark configs, all benchmark configs will have device list
        public virtual void BenchmarkStart(int time, IBenchmarkComunicator benchmarkComunicator)
        {
            _benchmarkTimeWait = time;
            benchmarkTimeWait = time;
            BenchmarkComunicator = benchmarkComunicator;
            BenchmarkTimeInSeconds = time;
            BenchmarkSignalFinnished = true;
            // check and kill
            BenchmarkHandle = null;
            OnBenchmarkCompleteCalled = false;
            _benchmarkTimeOutStopWatch = null;


            try
            {
                if (!Directory.Exists("logs"))
                {
                    Directory.CreateDirectory("logs");
                }
            }
            catch { }

            BenchLines = new List<string>();
            _benchmarkLogPath =
                $"{Logger.LogPath}Log_{MiningSetup.MiningPairs[0].Device.Uuid}_" +
                $"{MiningSetup.MiningPairs[0].Algorithm.AlgorithmStringID}+" +
                $"{MiningSetup.MiningPairs[0].Algorithm.SecondaryZergPoolID}";

            var commandLine = BenchmarkCreateCommandLine(BenchmarkAlgorithm, time);
            var benchmarkThread = new Thread(BenchmarkThreadRoutine, time);
            benchmarkThread.Start(commandLine);
        }

        protected virtual Process BenchmarkStartProcess(string commandLine)
        {
            RunCMDBeforeOrAfterMining(true);
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Helpers.ConsolePrint(MinerTag(), "Starting benchmark: " + commandLine);

            var benchmarkHandle = new Process
            {
                StartInfo =
                {
                    FileName = MiningSetup.MinerPath
                }
            };

            if (benchmarkHandle.StartInfo.FileName.ToLower().Contains("cryptodredge") && 
                (commandLine.ToLower().Contains("neoscrypt") || 
                commandLine.ToLower().Contains("x16rv2") || 
                commandLine.ToLower().Contains("sha256csm")))
            {
                benchmarkHandle.StartInfo.FileName = benchmarkHandle.StartInfo.FileName.Replace("CryptoDredge.exe", "CryptoDredge.0.25.1.exe");
            }
            if (benchmarkHandle.StartInfo.FileName.ToLower().Contains("cryptodredge") &&
                (commandLine.ToLower().Contains("allium")))
            {
                benchmarkHandle.StartInfo.FileName = benchmarkHandle.StartInfo.FileName.Replace("CryptoDredge.exe", "CryptoDredge.0.23.0.exe");
            }
            if (benchmarkHandle.StartInfo.FileName.ToLower().Contains("t-rex") && (commandLine.ToLower().Contains("x16r") ||
                commandLine.ToLower().Contains("x16rv2") ||
                commandLine.ToLower().Contains("x21s") ||
                commandLine.ToLower().Contains("x25x") ||
                commandLine.ToLower().Contains("megabtx")))
            {
                benchmarkHandle.StartInfo.FileName = benchmarkHandle.StartInfo.FileName.Replace("t-rex.exe", "t-rex.0.19.4.exe");
            }
            if (benchmarkHandle.StartInfo.FileName.ToLower().Contains("gminer") && (commandLine.ToLower().Contains("192_7")))
            {
                benchmarkHandle.StartInfo.FileName = benchmarkHandle.StartInfo.FileName.Replace("miner.exe", "miner275.exe");
            }
            if (benchmarkHandle.StartInfo.FileName.ToLower().Contains("miniz") && 
                (commandLine.ToLower().Contains("h125")))
            {
                benchmarkHandle.StartInfo.FileName = benchmarkHandle.StartInfo.FileName.Replace("miniZ.exe", "miniZ.22c.exe");
            }
            /*
            if (benchmarkHandle.StartInfo.FileName.ToLower().Contains("srbminer") &&
                (commandLine.ToLower().Contains("panthera") ||
                commandLine.ToLower().Contains("randomarq") ||
                commandLine.ToLower().Contains("randomxeq") ||
                commandLine.ToLower().Contains("randomx")))
            {
                benchmarkHandle.StartInfo.FileName = benchmarkHandle.StartInfo.FileName.Replace("SRBMiner-MULTI.exe",
                    "SRBMiner-MULTI256.exe");
            }
            */
            BenchmarkProcessPath = benchmarkHandle.StartInfo.FileName;
            Helpers.ConsolePrint(MinerTag(), "Using miner: " + benchmarkHandle.StartInfo.FileName);
            benchmarkHandle.StartInfo.WorkingDirectory = WorkingDirectory;
            benchmarkHandle.StartInfo.Arguments = commandLine;
            benchmarkHandle.StartInfo.UseShellExecute = false;
            benchmarkHandle.StartInfo.RedirectStandardError = true;
            benchmarkHandle.StartInfo.RedirectStandardOutput = true;
            benchmarkHandle.StartInfo.CreateNoWindow = true;
            benchmarkHandle.OutputDataReceived += BenchmarkOutputErrorDataReceived;
            benchmarkHandle.ErrorDataReceived += BenchmarkOutputErrorDataReceived;
            benchmarkHandle.Exited += BenchmarkHandle_Exited;

            if (!benchmarkHandle.Start()) return null;

            _currentPidData = new MinerPidData
            {
                MinerBinPath = benchmarkHandle.StartInfo.FileName,
                Pid = benchmarkHandle.Id
            };
            _allPidData.Add(_currentPidData);

            return benchmarkHandle;
        }

        private void BenchmarkHandle_Exited(object sender, EventArgs e)
        {
            BenchmarkSignalFinnished = true;
        }

        private void BenchmarkOutputErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            /*
            if (_benchmarkTimeOutStopWatch == null)
            {
                _benchmarkTimeOutStopWatch = new Stopwatch();
                _benchmarkTimeOutStopWatch.Start();
            }
            else if (_benchmarkTimeOutStopWatch.Elapsed.TotalSeconds >
                     BenchmarkTimeoutInSeconds(BenchmarkTimeInSeconds))
            {
                _benchmarkTimeOutStopWatch.Stop();
                BenchmarkSignalTimedout = true;
            }
            */
            var outdata = e.Data;
            if (e.Data != null)
            {
                BenchmarkOutputErrorDataReceivedImpl(outdata);
            }

            // terminate process situations
            if (BenchmarkSignalQuit
                || BenchmarkSignalFinnished
                || BenchmarkSignalHanged
                || BenchmarkSignalTimedout
                || BenchmarkException != null)
            {
                FinishUpBenchmark();
                EndBenchmarkProcces();
            }
        }

        protected virtual void FinishUpBenchmark()
        { }

        protected abstract void BenchmarkOutputErrorDataReceivedImpl(string outdata);

        protected void CheckOutdata(string outdata)
        {
            BenchLines.Add(outdata);
            /*
            // ccminer, cpuminer
            if (outdata.Contains("Cuda error"))
                BenchmarkException = new Exception("CUDA error");
            if (outdata.Contains("is not supported"))
                BenchmarkException = new Exception("N/A");
            if (outdata.Contains("illegal memory access"))
                BenchmarkException = new Exception("CUDA error");
            if (outdata.Contains("unknown error"))
                BenchmarkException = new Exception("Unknown error");
            if (outdata.Contains("No servers could be used! Exiting."))
                BenchmarkException = new Exception("No pools or work can be used for benchmarking");
            //if (outdata.Contains("error") || outdata.Contains("Error"))
            //    BenchmarkException = new Exception("Unknown error #2");
            // Ethminer
            if (outdata.Contains("No GPU device with sufficient memory was found"))
                BenchmarkException = new Exception("[daggerhashimoto] No GPU device with sufficient memory was found.");
            // xmr-stak
            if (outdata.Contains("Press any key to exit"))
                BenchmarkException = new Exception("Xmr-Stak erred, check its logs");
            */
            // lastly parse data
            //Helpers.ConsolePrint("BENCHMARK_CheckOutData", outdata);

        }

        public void InvokeBenchmarkSignalQuit()
        {
            KillAllUsedMinerProcesses();
        }

        protected double BenchmarkParseLine_cpu_ccminer_extra(string outdata)
        {
            if (outdata.Contains("Benchmark: ") && outdata.Contains("/s"))
            {
                var i = outdata.IndexOf("Benchmark:");
                var k = outdata.IndexOf("/s");
                var hashspeed = outdata.Substring(i + 11, k - i - 9);
                Helpers.ConsolePrint("BENCHMARK-CC", "Final Speed: " + hashspeed);

                // save speed
                var b = hashspeed.IndexOf(" ");
                if (b < 0)
                {
                    for (var j = hashspeed.Length - 1; j >= 0; --j)
                    {
                        if (!int.TryParse(hashspeed[j].ToString(), out var _)) continue;
                        b = j;
                        break;
                    }
                }

                if (b >= 0)
                {
                    var speedStr = hashspeed.Substring(0, b);
                    var spd = Helpers.ParseDouble(speedStr);
                    if (hashspeed.Contains("KH/s"))
                        spd *= 1000;
                    else if (hashspeed.Contains("MH/s"))
                        spd *= 1000000;
                    else if (hashspeed.Contains("GH/s"))
                        spd *= 1000000000;

                    return spd;
                }
            }

            return 0.0d;
        }

        public virtual void EndBenchmarkProcces()
        {
            if (BenchmarkHandle != null && BenchmarkProcessStatus != BenchmarkProcessStatus.Killing &&
                BenchmarkProcessStatus != BenchmarkProcessStatus.DoneKilling)
            {
                BenchmarkProcessStatus = BenchmarkProcessStatus.Killing;
                try
                {
                    Helpers.ConsolePrint(MinerTag(), ProcessTag() + " SendCtrlC to stop miner");
                    try
                    {
                        if (ProcessHandle is object)
                        {
                            if (Process.GetCurrentProcess() != null)
                            {
                                ProcessHandle.SendCtrlC((uint)Process.GetCurrentProcess().Id);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Helpers.ConsolePrint("EndBenchmarkProcces", $"algorithm {BenchmarkAlgorithm.AlgorithmName} : " + ex.ToString());
                    }
                    Thread.Sleep(1000);

                    try
                    {
                        int pid = _currentPidData.Pid;
                        if (BenchmarkHandle != null && Process.GetProcessById(pid) != null)
                        {
                            Helpers.ConsolePrint("BENCHMARK-end",
    $"Trying to kill benchmark process {BenchmarkProcessPath} algorithm {BenchmarkAlgorithm.AlgorithmName}");
                            BenchmarkHandle.Kill();
                            BenchmarkHandle.Close();
                            KillAllUsedMinerProcesses();
                        }
                    }
                    catch
                    {

                    }
                }
                catch { }
                finally
                {
                    BenchmarkProcessStatus = BenchmarkProcessStatus.DoneKilling;
                    Helpers.ConsolePrint("BENCHMARK-end",
                        $"Benchmark process {BenchmarkProcessPath} algorithm {BenchmarkAlgorithm.AlgorithmName} CLOSED");
                }
            }
        }


        protected virtual void BenchmarkThreadRoutineStartSettup()
        {
            BenchmarkHandle.BeginErrorReadLine();
            BenchmarkHandle.BeginOutputReadLine();
        }

        protected void BenchmarkThreadRoutineCatch(Exception ex)
        {
            //BenchmarkAlgorithm.BenchmarkSpeed = 0d;
            //BenchmarkAlgorithm.BenchmarkSecondarySpeed = 0d;

            Helpers.ConsolePrint(MinerTag(), "Benchmark Exception: " + ex.Message);
            Helpers.ConsolePrint(MinerTag(), "Benchmark Exception: " + ex.ToString());
            if (BenchmarkComunicator != null && !OnBenchmarkCompleteCalled)
            {
                OnBenchmarkCompleteCalled = true;
                BenchmarkComunicator.OnBenchmarkComplete(false, GetFinalBenchmarkString());
            }
        }

        protected virtual string GetFinalBenchmarkString()
        {
            return BenchmarkSignalTimedout && !TimeoutStandard
                ? International.GetText("Benchmark_Timedout")
                : International.GetText("Benchmark_Terminated");
        }

        protected void BenchmarkThreadRoutineFinish()
        {
            BenchmarkAlgorithm.BenchmarkProgressPercent = 0;
            var status = BenchmarkProcessStatus.Finished;
            RunCMDBeforeOrAfterMining(false);

            if (!BenchmarkAlgorithm.BenchmarkNeeded)
            {
                status = BenchmarkProcessStatus.Success;
            }

            try
            {
                using (StreamWriter sw = File.AppendText(_benchmarkLogPath))
                {
                    foreach (var line in BenchLines)
                    {
                        sw.WriteLine(line);
                    }
                }
            }
            catch { }

            BenchmarkProcessStatus = status;
            if (BenchmarkAlgorithm is DualAlgorithm dualAlg)
            {
                Helpers.ConsolePrint(MinerTag(),
                    "Final Speed: " + Helpers.FormatDualSpeedOutput(BenchmarkAlgorithm.BenchmarkSpeed,
                        BenchmarkAlgorithm.BenchmarkSecondarySpeed, 0, dualAlg.ZergPoolID, dualAlg.DualZergPoolID));
            }
            else
            {
                Helpers.ConsolePrint(MinerTag(),
                    "Final Speed: " + Helpers.FormatDualSpeedOutput(BenchmarkAlgorithm.BenchmarkSpeed, 0, 0,
                        BenchmarkAlgorithm.ZergPoolID, BenchmarkAlgorithm.DualZergPoolID));
            }

            Helpers.ConsolePrint(MinerTag(), "Benchmark ends");
            if (BenchmarkComunicator != null && !OnBenchmarkCompleteCalled)
            {
                OnBenchmarkCompleteCalled = true;
                var isOK = BenchmarkProcessStatus.Success == status;
                var msg = GetFinalBenchmarkString();
                BenchmarkComunicator.OnBenchmarkComplete(isOK, isOK ? "" : msg);
            }
        }

        private Dictionary<AlgorithmType, int> algoBenchmarkProperties = new Dictionary<AlgorithmType, int>()
            {
            //CPU
                {AlgorithmType.CPUPower, 10},//30 on default
                {AlgorithmType.Cryptonight_UPX, 10},
                {AlgorithmType.Flex, 10},
                {AlgorithmType.Ghostrider, 10},
                {AlgorithmType.Mike, 10},
                {AlgorithmType.Minotaurx, 10},
                {AlgorithmType.Panthera, 10},
                {AlgorithmType.RandomX, 10},
                {AlgorithmType.RandomARQ, 10},
                {AlgorithmType.RandomXEQ, 10},
                {AlgorithmType.VerusHash, 10},
                {AlgorithmType.Xelisv2_Pepew, 10},
                {AlgorithmType.Yescrypt, 10},
                {AlgorithmType.YescryptR16, 10},
                //{AlgorithmType.YescryptR32, 10},
                {AlgorithmType.YescryptR8, 10},
                {AlgorithmType.Yespower, 10},
                {AlgorithmType.YespowerLTNCG, 10},
                {AlgorithmType.YespowerMGPC, 10},
                {AlgorithmType.YespowerR16, 10},
                {AlgorithmType.YespowerSUGAR, 10},
                {AlgorithmType.YespowerTIDE, 10},
                {AlgorithmType.YespowerURX, 10},
                //{AlgorithmType.SHA3d, 10},
            //GPU
            {AlgorithmType.Cryptonight_GPU, 15},
            {AlgorithmType.Equihash125, 10},
            {AlgorithmType.Equihash144, 10},
            {AlgorithmType.Equihash192, 10},
            {AlgorithmType.EvrProgPow, 50},
            {AlgorithmType.Ethash, 30},
            {AlgorithmType.Ethashb3, 30},
            //{AlgorithmType.KarlsenHash, 10},
            {AlgorithmType.NexaPow, 10},
            {AlgorithmType.SHA512256d, 10},
            {AlgorithmType.SHA256dt, 10},
            {AlgorithmType.HooHash, 10},
            {AlgorithmType.FiroPow, 30},
            {AlgorithmType.KawPow, 30},
            {AlgorithmType.CurveHash, 60},
            {AlgorithmType.X16RV2, 1},
            {AlgorithmType.X21S, 1},
            {AlgorithmType.X25X, 1},
            };

        protected virtual void BenchmarkThreadRoutine(object commandLine)
        {
            BenchmarkSignalQuit = false;
            BenchmarkSignalHanged = false;
            BenchmarkSignalFinnished = false;
            BenchmarkException = null;
            double repeats = 0;
            double summspeed = 0.0d;
            double summspeedSecond = 0.0d;

            int delay_before_calc_hashrate = 30;
            int overallbenchmarktime = 0;

            Thread.Sleep(ConfigManager.GeneralConfig.MinerRestartDelayMS);

            try
            {
                int showBenchTimeAdd = 0;
                var devtype = MiningSetup.MiningPairs[0].Device.DeviceType;
                if (devtype == DeviceType.CPU)
                {
                    _benchmarkTimeWait = (int)(_benchmarkTimeWait / 1.5);
                    showBenchTimeAdd = 20;
                } else
                {
                    showBenchTimeAdd = 40;
                }

                //
                var _algoBenchmarkProperties = algoBenchmarkProperties.FirstOrDefault(item => item.Key == MiningSetup.CurrentAlgorithmType);
                if (_algoBenchmarkProperties.Key.Equals(MiningSetup.CurrentAlgorithmType))
                {
                    delay_before_calc_hashrate = _algoBenchmarkProperties.Value;
                } else
                {
                    delay_before_calc_hashrate = 30;
                }

                if (BenchmarkAlgorithm.MinerBaseType == MinerBaseType.teamredminer)
                {
                    //_benchmarkTimeWait = _benchmarkTimeWait + 15;
                    delay_before_calc_hashrate = delay_before_calc_hashrate + 20;
                }
                if (BenchmarkAlgorithm.SecondaryZergPoolID != AlgorithmType.NONE && 
                    BenchmarkAlgorithm.MinerBaseType == MinerBaseType.lolMiner)//duals
                {
                    //_benchmarkTimeWait = _benchmarkTimeWait + 30;
                    delay_before_calc_hashrate = delay_before_calc_hashrate + 100;
                }

                benchmarkTimeWait = benchmarkTimeWait + delay_before_calc_hashrate;
                _benchmarkTimeWait = _benchmarkTimeWait + (delay_before_calc_hashrate - 30);
                delay_before_calc_hashrate = Math.Min(delay_before_calc_hashrate, _benchmarkTimeWait);


                Helpers.ConsolePrint(MinerTag(), "Benchmark should be completed in at least ~" +
                    (_benchmarkTimeWait + showBenchTimeAdd).ToString() + " seconds");

                GetBenchmarkSpeed(_benchmarkTimeWait, benchmarkTimeWait, commandLine, ref _powerUsage, 
                    ref _power, ref repeats, ref delay_before_calc_hashrate, ref summspeed, ref summspeedSecond,
                    ref BenchmarkSpeed, ref BenchmarkSpeedSecond, ref overallbenchmarktime);

            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint(MinerTag(), ex.ToString());
                BenchmarkThreadRoutineCatch(ex);
            }
            finally
            {
                EndBenchmarkProcces();
                BenchmarkThreadRoutineFinish();
            }
        }
        public void GetBenchmarkSpeed(int _benchmarkTimeWait,
            int benchmarkTimeWait,
            object commandLine,
            ref double _powerUsage,
            ref double _power,
            ref double repeats,
            ref int delay_before_calc_hashrate,
            ref double summspeed,
            ref double summspeedSecond,
            ref double BenchmarkSpeed,
            ref double BenchmarkSpeedSecond,
            ref int overallbenchmarktime)
        {
            BenchmarkHandle = BenchmarkStartProcess((string)commandLine);
            var benchmarkTimer = new Stopwatch();
            benchmarkTimer.Reset();
            benchmarkTimer.Start();

            BenchmarkProcessStatus = BenchmarkProcessStatus.Running;
            BenchmarkThreadRoutineStartSettup(); //need for benchmark log
            while (IsActiveProcess(BenchmarkHandle.Id))
            {
                try
                {
                    if (benchmarkTimer.Elapsed.TotalSeconds >= (_benchmarkTimeWait + delay_before_calc_hashrate + 300)
                        || BenchmarkSignalQuit
                        || BenchmarkSignalFinnished
                        || BenchmarkSignalHanged
                        || BenchmarkSignalTimedout
                        || BenchmarkException != null)
                    {
                        var imageName = MinerExeName.Replace(".exe", "");
                        // maybe will have to KILL process
                        EndBenchmarkProcces();
                        //  KillMinerBase(imageName);
                        if (BenchmarkSignalTimedout)
                        {
                            Helpers.ConsolePrint(MinerTag(), "Benchmark timedout");
                            //throw new Exception("Benchmark timedout");
                        }

                        if (BenchmarkException != null)
                        {
                            throw BenchmarkException;
                        }

                        if (BenchmarkSignalQuit)
                        {
                            Helpers.ConsolePrint(MinerTag(), "Termined by user request");
                            break;
                            //throw new Exception("Termined by user request");
                        }

                        if (BenchmarkSignalFinnished)
                        {
                            break;
                        }
                        break;
                    }
                    // wait a second due api request
                    Thread.Sleep(1000);
                    overallbenchmarktime++;

                    var ad = GetSummaryAsync();
                    if (ad.Result != null && ad.Result.Speed > 0)
                    {
                        _powerUsage += _power;
                        repeats++;
                        double benchProgress = repeats / _benchmarkTimeWait;
                        BenchmarkAlgorithm.BenchmarkProgressPercent = (int)(benchProgress * 100);
                        if (repeats > delay_before_calc_hashrate)
                        {
                            if (MiningSetup.CurrentSecondaryAlgorithmType == AlgorithmType.NONE)//single
                            {
                                Helpers.ConsolePrint(MinerTag(), "Useful Speed: " + ad.Result.Speed.ToString());
                            }
                            else
                            {
                                Helpers.ConsolePrint(MinerTag(), "Useful Speed: " + ad.Result.Speed.ToString() + " SecondarySpeed: " + ad.Result.SecondarySpeed.ToString());
                            }
                            summspeed += ad.Result.Speed;
                            summspeedSecond += ad.Result.SecondarySpeed;
                        }
                        else
                        {
                            _benchmarkTimeWait = Math.Min(_benchmarkTimeWait, benchmarkTimeWait);
                            Helpers.ConsolePrint(MinerTag(), "Delayed Speed: " + ad.Result.Speed.ToString());
                        }
                        if (repeats >= _benchmarkTimeWait)
                        {
                            BenchmarkSpeed = Math.Round(summspeed / (repeats - delay_before_calc_hashrate), 2);
                            BenchmarkSpeedSecond = Math.Round(summspeedSecond / (repeats - delay_before_calc_hashrate), 2);
                            if (double.IsNaN(BenchmarkSpeed)) BenchmarkSpeed = 0;
                            if (double.IsNaN(BenchmarkSpeedSecond)) BenchmarkSpeedSecond = 0;
                            Helpers.ConsolePrint(MinerTag(), $"Benchmark has been completed. Speed: " + BenchmarkSpeed.ToString() + " SecondarySpeed: " + BenchmarkSpeedSecond.ToString());
                            ad.Dispose();
                            benchmarkTimer.Stop();

                            try
                            {
                                KillProcessAndChildren(BenchmarkHandle.Id);
                                BenchmarkHandle.Kill();
                                BenchmarkHandle.Dispose();
                                EndBenchmarkProcces();
                            }
                            catch (Exception ex)
                            {
                                Helpers.ConsolePrint("GetBenchmarkSpeed*", ex.ToString());
                            }

                            break;
                        }

                    }
                } catch (Exception ex)
                {
                    Helpers.ConsolePrint("GetBenchmarkSpeed", ex.ToString());
                }
            }

            if (BenchmarkAlgorithm.BenchmarkSpeed == 0)
            {
                BenchmarkAlgorithm.BenchmarkSpeed = BenchmarkSpeed;
                BenchmarkAlgorithm.BenchmarkSecondarySpeed = BenchmarkSpeedSecond;
                BenchmarkAlgorithm.PowerUsageBenchmark = (_powerUsage / repeats);
            }
            else //trex read from log file
            {
                BenchmarkAlgorithm.BenchmarkSecondarySpeed = 0;
            }
        }

        public string GetServer(string algo)
        {
            string ret = "";
            try
            {
                algo = algo.Replace("-", "_");
                var _a = Stats.Stats.MiningAlgorithmsList.FirstOrDefault(item => item.name.ToLower() == algo.ToLower());
                string serverUrl = Form_Main.regionList[ConfigManager.GeneralConfig.ServiceLocation].RegionLocation +
                    "mine.zergpool.com";
                ret = Links.CheckDNS(algo + serverUrl).Replace("stratum+tcp://", "") + ":" + _a.tls_port.ToString();
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("GetServer", "Error in " + algo + " " + ex.ToString());
                ret = "error_in_list_of_algos.err:1111";
            }
            return ret;
        }
        
        /// <summary>
        /// When parallel benchmarking each device needs its own log files, so this uniquely identifies for the setup
        /// </summary>
        protected string GetDeviceID()
        {
            var ids = MiningSetup.MiningPairs.Select(x => x.Device.ID);
            var idStr = string.Join(",", ids);

            if (!IsMultiType) return idStr;

            // Miners that use multiple dev types need to also discriminate based on that
            var types = MiningSetup.MiningPairs.Select(x => (int)x.Device.DeviceType);
            return $"{string.Join(",", types)}-{idStr}";
        }

        protected string GetLogFileName()
        {
            return $"{GetDeviceID()}_log.txt";
        }

        protected virtual void ProcessBenchLinesAlternate(string[] lines)
        { }

        protected abstract bool BenchmarkParseLine(string outdata);

        protected bool IsActiveProcess(int pid)
        {
            try
            {
                return Process.GetProcessById(pid) != null;
            }
            catch
            {
                return false;
            }
        }

        #endregion //BENCHMARK DE-COUPLED Decoupled benchmarking routines

        private void MinerDelayStart(string minerpath)
        {
            try
            {
                Process localByName = Process.GetProcessById(Process.GetCurrentProcess().Id);
                var query = "Select * From Win32_Process Where ParentProcessId = " + Process.GetCurrentProcess().Id.ToString();
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                ManagementObjectCollection processList = searcher.Get();
                var result = processList.Cast<ManagementObject>().Select(p =>
                    Process.GetProcessById(Convert.ToInt32(p.GetPropertyValue("ProcessId")))).ToList();

                bool minerrunning = false;
                foreach (var process in result)
                {
                    string m = process.ProcessName;
                    string p = process.MainWindowTitle;
                    if (p.ToLower().Contains(minerpath) && (p.ToLower().Contains("gminer") || p.ToLower().Contains("miniz")))
                    {
                        minerrunning = true;
                        break;
                    }
                }
                if (minerrunning) Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint("MinerDelayStart", ex.ToString());
            }
        }

        private void DetectLiteMode()
        {
            ulong minMem = (ulong)(1024 * 1024 * 8);
            foreach (var pair in MiningSetup.MiningPairs)
            {
                var algo = pair.Algorithm;
                var _computeDevice = pair.Device;
                if (algo.ZergPoolID == AlgorithmType.KawPowLite && algo.Enabled)
                {
                    minMem = Math.Min(minMem, _computeDevice.GpuRam / 1024);
                    if (minMem > (ulong)(1024 * 1024 * 2.7) && minMem < (ulong)(1024 * 1024 * 3.7))
                    {
                        Form_Main.KawpowLite3GB = true;
                        Form_Main.KawpowLite4GB = false;
                        Form_Main.KawpowLite5GB = false;
                    }
                    if (minMem > (ulong)(1024 * 1024 * 3.7) && minMem < (ulong)(1024 * 1024 * 4.7))
                    {
                        Form_Main.KawpowLite3GB = false;
                        Form_Main.KawpowLite4GB = true;
                        Form_Main.KawpowLite5GB = false;
                    }
                    if (minMem > (ulong)(1024 * 1024 * 4.7) && minMem < (ulong)(1024 * 1024 * 5.7))
                    {
                        Form_Main.KawpowLite3GB = false;
                        Form_Main.KawpowLite4GB = false;
                        Form_Main.KawpowLite5GB = true;
                    }
                }
            }
        }

        protected virtual MinerProcess _Start()
        {
            try
            {
                RunCMDBeforeOrAfterMining(true);
            } catch (Exception ex)
            {

            }
            // never start when ended
            if (_isEnded)
            {
                return null;
            }

            PreviousTotalMH = 0.0;
            if (LastCommandLine.Length == 0) return null;

            var P = new MinerProcess();

            if (WorkingDirectory.Length > 1)
            {
                P.StartInfo.WorkingDirectory = WorkingDirectory;
            }

            if (MinersSettingsManager.MinerSystemVariables.ContainsKey(Path))
            {
                foreach (var kvp in MinersSettingsManager.MinerSystemVariables[Path])
                {
                    var envName = kvp.Key;
                    var envValue = kvp.Value;
                    P.StartInfo.EnvironmentVariables[envName] = envValue;
                }
            }

            if (MiningSetup.MinerPath.ToLower().Contains("cryptodredge") && 
                (LastCommandLine.ToLower().Contains("neoscrypt") || 
                LastCommandLine.ToLower().Contains("x16rv2") ||
                LastCommandLine.ToLower().Contains("sha256csm")))
            {
                Path = MiningSetup.MinerPath.Replace("CryptoDredge.exe", "CryptoDredge.0.25.1.exe");
            }
            if (MiningSetup.MinerPath.ToLower().Contains("cryptodredge") &&
                (LastCommandLine.ToLower().Contains("allium")))
            {
                Path = MiningSetup.MinerPath.Replace("CryptoDredge.exe", "CryptoDredge.0.23.0.exe");
            }
            if (MiningSetup.MinerPath.ToLower().Contains("t-rex") && (LastCommandLine.ToLower().Contains("x16r") ||
                LastCommandLine.ToLower().Contains("x16rv2") ||
                LastCommandLine.ToLower().Contains("x21s") ||
                LastCommandLine.ToLower().Contains("x25x") ||
                LastCommandLine.ToLower().Contains("megabtx")))
            {
                Path = MiningSetup.MinerPath.Replace("t-rex.exe", "t-rex.0.19.4.exe");
            }
            
            if (MiningSetup.MinerPath.ToLower().Contains("gminer") && (LastCommandLine.ToLower().Contains("192_7")))
            {
                Path = MiningSetup.MinerPath.Replace("miner.exe", "miner275.exe");
            }

            if (MiningSetup.MinerPath.ToLower().Contains("miniz") && 
                (LastCommandLine.ToLower().Contains("h125")))
            {
                Path = MiningSetup.MinerPath.Replace("miniZ.exe", "miniZ.22c.exe");
            }
            /*
            if (MiningSetup.MinerPath.ToLower().Contains("srbminer") &&
                (LastCommandLine.ToLower().Contains("panthera") ||
                LastCommandLine.ToLower().Contains("randomarq") ||
                LastCommandLine.ToLower().Contains("randomxeq") ||
                LastCommandLine.ToLower().Contains("randomx")))
            {
                Path = MiningSetup.MinerPath.Replace("SRBMiner-MULTI.exe",
                    "SRBMiner-MULTI256.exe");
            }
            */
            P.StartInfo.FileName = Path;

            P.ExitEvent = Miner_Exited;
            LastCommandLine = System.Text.RegularExpressions.Regex.Replace(LastCommandLine, @"\s+", " ");
            P.StartInfo.Arguments = LastCommandLine;
            if (IsNeverHideMiningWindow)
            {
                P.StartInfo.CreateNoWindow = false;
                if (ConfigManager.GeneralConfig.HideMiningWindows || ConfigManager.GeneralConfig.MinimizeMiningWindows)
                {
                    P.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                    P.StartInfo.UseShellExecute = true;
                }
            }
            else
            {
                P.StartInfo.CreateNoWindow = ConfigManager.GeneralConfig.HideMiningWindows;
            }

            P.StartInfo.UseShellExecute = false;

            var coin = LastCommandLine.Substring(LastCommandLine.IndexOf(",mc=") + 4)
                .Substring(0, LastCommandLine.Substring(LastCommandLine.IndexOf(",mc=") + 4).IndexOf(",ID="));

            try
            {
                string strPlatform = "";
                foreach (var pair in MiningSetup.MiningPairs)
                {
                    int a = (int)pair.Algorithm.ZergPoolID;
                    int b = (int)pair.Algorithm.SecondaryZergPoolID;
                    pair.Device.AlgorithmID = a;
                    pair.Device.SecondAlgorithmID = b;
                    pair.Device.MinerName = MinerDeviceName;
                    pair.Device.State = DeviceState.Mining;
                    pair.Device.Coin = coin;
                    
                    if (pair.Device.DeviceType == DeviceType.NVIDIA)
                    {
                        strPlatform = "NVIDIA";
                    }
                    else if (pair.Device.DeviceType == DeviceType.AMD)
                    {
                        strPlatform = "AMD";
                    }
                    else if (pair.Device.DeviceType == DeviceType.CPU)
                    {
                        strPlatform = "CPU";
                    }
                }

                GC.Collect();

                NativeOverclock.OverclockStart(P.Id, -1, -1, Path,
                            strPlatform, "", false);

                MinerDelayStart(Path);

                if (P.Start())
                {
                    _currentPidData = new MinerPidData
                    {
                        MinerBinPath = P.StartInfo.FileName,
                        Pid = P.Id
                    };
                    _allPidData.Add(_currentPidData);

                    Helpers.ConsolePrint(MinerTag(), "Starting miner " + ProcessTag() + " " + LastCommandLine);
                    IsRunning = true;
                    IsRunningNew = IsRunning;

                    int algo = (int)MiningSetup.CurrentAlgorithmType;
                    int algo2 = (int)MiningSetup.CurrentSecondaryAlgorithmType;
                    string w = GetFullWorkerName();

                    NativeOverclock.OverclockStart(P.Id, algo, algo2, Path,
                        strPlatform, coin, false);

                    new Task(() => StartCoolDownTimerChecker()).Start();
                    //StartCoolDownTimerChecker();
                    return P;
                }

                Helpers.ConsolePrint(MinerTag(), "NOT STARTED " + ProcessTag() + " " + LastCommandLine);
                return null;
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint(MinerTag(), ProcessTag() + " _Start: " + ex.Message);
                return null;
            }
        }

        public static string GetFullWorkerName()
        {
            string mac = WindowsMacUtils.GetMACAddress();
            string w = "";

            if (string.IsNullOrEmpty(ConfigManager.GeneralConfig.WorkerName.Trim()))
            {
                w = "demo";
            }
            else
            {
                w = ConfigManager.GeneralConfig.WorkerName.Trim();
            }
            return w + mac;
        }

        protected void StartCoolDownTimerChecker()
        {
            if (ConfigManager.GeneralConfig.CoolDownCheckEnabled)
            {
                Helpers.ConsolePrint(MinerTag(), ProcessTag() + " Starting cooldown checker");
                if (_cooldownCheckTimer != null && _cooldownCheckTimer.Enabled) _cooldownCheckTimer.Stop();
                // cool down init
                _cooldownCheckTimer = new Timer()
                {
                    Interval = MinCooldownTimeInMilliseconds
                };
                _cooldownCheckTimer.Elapsed += MinerCoolingCheck_Tick;
                _cooldownCheckTimer.Start();
                _currentCooldownTimeInSeconds = MinCooldownTimeInMilliseconds;
                _currentCooldownTimeInSecondsLeft = _currentCooldownTimeInSeconds;
            }
            else
            {
                Helpers.ConsolePrint(MinerTag(), "Cooldown checker disabled");
            }

            CurrentMinerReadStatus = MinerApiReadStatus.NONE;
        }

        protected virtual void Miner_Exited()
        {
            ScheduleRestart(6000);
        }

        public static bool minerRestarting = false;
        public static int minerRestartingCount = 0;
        protected void ScheduleRestart(int ms)
        {
            if (ProcessHandle != null)
            {
                if (!ProcessHandle._bRunning) return;
            }
            if (!IsRunning) return;
            minerRestarting = true;
            minerRestartingCount++;

            if (minerRestartingCount > 20)
            {
                Helpers.ConsolePrint(MinerTag(), "Many restarts of miner. Restart program");
                Form_Main.MakeRestart(0);
            }
            var restartInMs = ConfigManager.GeneralConfig.MinerRestartDelayMS > ms
                ? ConfigManager.GeneralConfig.MinerRestartDelayMS
                : ms;
            Helpers.ConsolePrint(MinerTag(), ProcessTag() + $" directly Miner_Exited Will restart in {restartInMs} ms. Coint: " + minerRestartingCount.ToString());
            CooldownCheck = 0;
            var algo = (int)MiningSetup.CurrentAlgorithmType;
            string strPlatform = "";
            foreach (var pair in MiningSetup.MiningPairs)
            {
                pair.Device.MiningHashrate = 0;
                pair.Device.MiningHashrateSecond = 0;
                pair.Device.MiningHashrateThird = 0;
                pair.Device.MinerName = "";
                pair.Device.State = DeviceState.Stopped;

                pair.Device.AlgorithmID = (int)AlgorithmType.NONE;
                pair.Device.SecondAlgorithmID = (int)AlgorithmType.NONE;
                pair.Device.ThirdAlgorithmID = (int)AlgorithmType.NONE;

                if (pair.Device.DeviceType == DeviceType.NVIDIA)
                {
                    strPlatform = "NVIDIA";
                }
                else if (pair.Device.DeviceType == DeviceType.AMD)
                {
                    strPlatform = "AMD";
                }
                else if (pair.Device.DeviceType == DeviceType.CPU)
                {
                    strPlatform = "CPU";
                }
            }

            if (ProcessHandle != null)
            {
                try
                {
                    var p = Process.GetProcessById(ProcessHandle.Id);
                    p.Kill();
                }
                catch (Exception)
                {
                    Helpers.ConsolePrint(MinerTag(), ProcessTag() + "Process not exist.");
                }
            }
            if (!IsRunning) return;
            Thread.Sleep(restartInMs);
            Restart();
            minerRestarting = false;
        }

        protected void Restart()
        {
            if (_isEnded) return;
            var algo = (int)MiningSetup.CurrentAlgorithmType;
            string strPlatform = "";
            foreach (var pair in MiningSetup.MiningPairs)
            {
                pair.Device.MiningHashrate = 0;
                pair.Device.MiningHashrateSecond = 0;
                pair.Device.MiningHashrateThird = 0;
                pair.Device.MinerName = "";
                pair.Device.State = DeviceState.Stopped;

                pair.Device.AlgorithmID = (int)AlgorithmType.NONE;
                pair.Device.SecondAlgorithmID = (int)AlgorithmType.NONE;
                pair.Device.ThirdAlgorithmID = (int)AlgorithmType.NONE;

                if (pair.Device.DeviceType == DeviceType.NVIDIA)
                {
                    strPlatform = "NVIDIA";
                }
                else if (pair.Device.DeviceType == DeviceType.AMD)
                {
                    strPlatform = "AMD";
                }
                else if (pair.Device.DeviceType == DeviceType.CPU)
                {
                    strPlatform = "CPU";
                }
            }

            Helpers.ConsolePrint(MinerTag(), ProcessTag() + " Restarting miner..");
            Stop(MinerStopType.END); // stop miner first
            Thread.Sleep(Math.Max(ConfigManager.GeneralConfig.MinerRestartDelayMS, 500));
            ProcessHandle = _Start(); // start with old command line
        }

        protected virtual bool IsApiEof(byte third, byte second, byte last)
        {
            return false;
        }

        protected async Task<string> GetApiDataAsync(int port, string dataToSend, bool exitHack = false,
            bool overrideLoop = false)
        {
            string responseFromServer = null;
            try
            {
                var tcpc = new TcpClient("127.0.0.1", port);
                var nwStream = tcpc.GetStream();
                nwStream.ReadTimeout = 2 * 1000;
                nwStream.WriteTimeout = 2 * 1000;

                var bytesToSend = Encoding.ASCII.GetBytes(dataToSend);
                await nwStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);

                var incomingBuffer = new byte[tcpc.ReceiveBufferSize];
                var prevOffset = -1;
                var offset = 0;
                var fin = false;

                while (!fin && tcpc.Client.Connected)
                {
                    var r = await nwStream.ReadAsync(incomingBuffer, offset, tcpc.ReceiveBufferSize - offset);
                    for (var i = offset; i < offset + r; i++)
                    {
                        if (incomingBuffer[i] == 0x7C || incomingBuffer[i] == 0x00
                                                      || (i > 2 && IsApiEof(incomingBuffer[i - 2],
                                                              incomingBuffer[i - 1], incomingBuffer[i]))
                                                      || overrideLoop)
                        {
                            fin = true;
                            break;
                        }
                    }

                    offset += r;
                    if (exitHack)
                    {
                        if (prevOffset == offset)
                        {
                            fin = true;
                            break;
                        }

                        prevOffset = offset;
                    }
                }

                tcpc.Close();

                if (offset > 0)
                    responseFromServer = Encoding.ASCII.GetString(incomingBuffer);
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint(MinerTag(), ProcessTag() + " GetAPIData reason: " + ex.Message);
                CurrentMinerReadStatus = MinerApiReadStatus.READ_SPEED_ZERO;
                return null;
            }

            return responseFromServer;
        }

        public abstract Task<ApiData> GetSummaryAsync();
        public abstract ApiData GetApiData();


        protected string GetHttpRequestNhmAgentStrin(string cmd)
        {
            return "GET /" + cmd + " HTTP/1.1\r\n" +
                   "Host: 127.0.0.1\r\n" +
                   "User-Agent: ZergPoolMiner/" + Application.ProductVersion + "\r\n" +
                   "\r\n";
        }

        #region Cooldown/retry logic

        private void MinerCoolingCheck_Tick(object sender, ElapsedEventArgs e)
        {
            int CooldownCheckFailCount = 30; //150 sec

            if (_isEnded)
            {
                End();
                return;
            }
            //Helpers.ConsolePrint(MinerTag(), ProcessTag() + " running: " + ProcessHandle._bRunning.ToString());
            if (ProcessHandle == null)
            {
                CooldownCheck = 100;
                Helpers.ConsolePrint(MinerTag(), ProcessTag() + "Process not exist. Restart miner");
                CooldownCheck = 0;
                Restart();
            }
            if (ProcessHandle != null && !ProcessHandle._bRunning)
            {
                try
                {
                    var p = Process.GetProcessById(ProcessHandle.Id);
                }
                catch (Exception)
                {
                    CooldownCheck = 100;
                    Helpers.ConsolePrint(MinerTag(), ProcessTag() + "Process not exist. Restart miner");
                    CooldownCheck = 0;
                    Restart();
                }
            }

            switch (CurrentMinerReadStatus)
            {
                case MinerApiReadStatus.GOT_READ:
                    //Helpers.ConsolePrint(MinerTag(), ProcessTag() + "MinerApiReadStatus.GOT_READ");
                    CooldownCheck = 0;
                    break;
                case MinerApiReadStatus.READ_SPEED_ZERO:
                    Helpers.ConsolePrint(MinerTag(), ProcessTag() + " READ SPEED ZERO, will cool up " + CooldownCheck.ToString());
                    CooldownCheck++;
                    break;
                case MinerApiReadStatus.RESTART:
                    Helpers.ConsolePrint(MinerTag(), ProcessTag() + "MinerApiReadStatus.RESTART");
                    CooldownCheck = 100;
                    break;
                default:
                    Helpers.ConsolePrint(MinerTag(), ProcessTag() + "MinerApiReadStatus.UNKNOWN");
                    CooldownCheck++;
                    break;
            }
            if (MiningSetup.CurrentAlgorithmType.Equals(AlgorithmType.Yescrypt))
            {
                CooldownCheckFailCount = 50;
            }

            if (CooldownCheck > CooldownCheckFailCount)
            {
                Helpers.ConsolePrint(MinerTag(), ProcessTag() + "API Error. Restart miner");
                CooldownCheck = 0;
                Restart();
            }
        }

        #endregion //Cooldown/retry logic

        private static Timer _deviceMSIABCheckTimer;
        public void CheckMSIABOverclock(object sender, ElapsedEventArgs e)
        {
            foreach (var dev in MiningSetup.MiningPairs)
            {
                if (dev.Device.Enabled)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        string fName = "configs\\profiles\\" + ConfigManager.GeneralConfig.ProfileName.Trim() + "\\overclock\\" +
                            dev.Device.Uuid + "_" + dev.Algorithm.AlgorithmStringID + ".gpu";
                        //Helpers.ConsolePrint(MinerTag(), "Try MSIAfterburner.ApplyFromFile: " + fName);
                        if (MSIAfterburner.CheckFromFile(dev.Device.BusID, fName))
                        {
                            Helpers.ConsolePrint("MSIAfterburner.CheckFromFile", "Compare OK. busID " + dev.Device.BusID.ToString());
                            break;
                        } else
                        {
                            Helpers.ConsolePrint("MSIAfterburner.CheckFromFile", "Compare ERROR. busID " + dev.Device.BusID.ToString() +
                                " Try MSIAfterburner.CheckFromFile: " + fName);
                            MSIAfterburner.CommitChanges(false);
                            Thread.Sleep(200);
                            MSIAfterburner.Flush();
                        }

                        Thread.Sleep(200);
                    }
                }
            }
            //MSIAfterburner.Flush();
            Thread.Sleep(100);
        }
        private Dictionary<string, System.Timers.Timer> timersDict = new Dictionary<string, System.Timers.Timer>();
        protected Process RunCMDBeforeOrAfterMining(bool isBefore)
        {
            try
            {
                if (ConfigManager.GeneralConfig.ABEnableOverclock)
                {
                    if (isBefore)
                    {
                        if (ConfigManager.GeneralConfig.ABMaintaiming)
                        {
                            Helpers.ConsolePrint("CheckMSIABOverclock", "Start timer for " + MinerTag());
                            _deviceMSIABCheckTimer = new Timer();
                            _deviceMSIABCheckTimer.Elapsed += CheckMSIABOverclock;
                            _deviceMSIABCheckTimer.Interval = 1000 * ConfigManager.GeneralConfig.ABMaintaiminginterval;
                            _deviceMSIABCheckTimer.Start();
                            timersDict.Add(MinerTag(), _deviceMSIABCheckTimer);
                        }
                        foreach (var dev in MiningSetup.MiningPairs)
                        {
                            if (dev.Device.Enabled)
                            {
                                for (int i = 0; i < 5; i++)
                                {
                                    string fName = "configs\\profiles\\" + ConfigManager.GeneralConfig.ProfileName.Trim() + "\\overclock\\" +
                                        dev.Device.Uuid + "_" + dev.Algorithm.AlgorithmStringID + ".gpu";
                                    Helpers.ConsolePrint(MinerTag(), "Try MSIAfterburner.ApplyFromFile: " + fName);
                                    if (MSIAfterburner.ApplyFromFile(dev.Device.BusID, fName)) break;
                                    Thread.Sleep(100);
                                    //MSIAfterburner.CommitChanges(false);
                                }
                            }
                        }
                        //MSIAfterburner.Flush();
                        Thread.Sleep(100);
                    }
                    else
                    {
                        if (ConfigManager.GeneralConfig.ABMaintaiming)
                        {
                            foreach (var t in timersDict)
                            {
                                if (t.Key == MinerTag())
                                {
                                    if (t.Value is object)
                                    {
                                        Helpers.ConsolePrint("CheckMSIABOverclock", "Stop timer for " + MinerTag());
                                        t.Value.Stop();
                                        t.Value.Dispose();
                                    }
                                }
                            }
                            timersDict.Remove(MinerTag());
                        }
                    }
                }
                else
                {
                    if (isBefore)
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.ConsolePrint(MinerTag(), ex.ToString());
            }

            bool CreateNoWindow = false;
            var CMDconfigHandle = new Process
            {
                StartInfo =
                {
                    FileName = MiningSetup.MinerPath
                }
            };

            try
            {
                var strPlatform = "";
                var strDual = "SINGLE";
                var strAlgo = AlgorithmNames.GetName(MiningSetup.CurrentAlgorithmType);

                var minername = MinerTag();
                int subStr;
                subStr = MinerTag().IndexOf("-");
                if (subStr > 0)
                {
                    minername = MinerTag().Substring(0, subStr);
                }
                if (minername == "ClaymoreCryptoNight" || minername == "ClaymoreZcash" || minername == "ClaymoreDual" || minername == "ClaymoreNeoscrypt")
                {
                    minername = "Claymore";
                }
                minername = minername.Replace("Z-Enemy", "ZEnemy");

                var gpus = "";
                List<string> l = MiningSetup.MiningPairs.Select(mPair => mPair.Device.IDByBus.ToString()).ToList();
                l.Sort();
                gpus += string.Join(",", l);

                foreach (var pair in MiningSetup.MiningPairs)
                {
                    if ((int)pair.Algorithm.DualZergPoolID <= -10)
                    {
                        strDual = "DUAL";
                    }
                    if (pair.Device.DeviceType == DeviceType.NVIDIA)
                    {
                        strPlatform = "NVIDIA";
                    }
                    else if (pair.Device.DeviceType == DeviceType.AMD)
                    {
                        strPlatform = "AMD";
                    }
                    else if (pair.Device.DeviceType == DeviceType.INTEL)
                    {
                        strPlatform = "INTEL";
                    }
                    else if (pair.Device.DeviceType == DeviceType.CPU)
                    {
                        strPlatform = "CPU";
                    }
                }

                string MinerDir = MiningSetup.MinerPath.Substring(0, MiningSetup.MinerPath.LastIndexOf("\\"));
                if (isBefore)
                {
                    CMDconfigHandle.StartInfo.FileName = "GPU-Scrypt.cmd";
                }
                else
                {
                    CMDconfigHandle.StartInfo.FileName = "GPU-Reset.cmd";
                }

                {
                    var cmd = "";
                    FileStream fs = new FileStream(CMDconfigHandle.StartInfo.FileName, FileMode.Open, FileAccess.Read);
                    StreamReader w = new StreamReader(fs);
                    cmd = w.ReadToEnd();
                    w.Close();

                    if (cmd.ToUpper().Trim().Contains("SET NOVISIBLE=TRUE"))
                    {
                        CreateNoWindow = true;
                    }
                    if (cmd.ToUpper().Trim().Contains("SET RUN=FALSE"))
                    {
                        return null;
                    }
                }
                Helpers.ConsolePrint(MinerTag(), "Using CMD: " + CMDconfigHandle.StartInfo.FileName);

                if (MinersSettingsManager.MinerSystemVariables.ContainsKey(Path))
                {
                    foreach (var kvp in MinersSettingsManager.MinerSystemVariables[Path])
                    {
                        var envName = kvp.Key;
                        var envValue = kvp.Value;
                        CMDconfigHandle.StartInfo.EnvironmentVariables[envName] = envValue;
                    }
                }

                CMDconfigHandle.StartInfo.Arguments = " " + strPlatform + " " + strDual + " " + strAlgo + " \"" + gpus + "\"" + " " + minername;
                CMDconfigHandle.StartInfo.UseShellExecute = false;
                // CMDconfigHandle.StartInfo.RedirectStandardError = true;
                // CMDconfigHandle.StartInfo.RedirectStandardOutput = true;
                CMDconfigHandle.StartInfo.CreateNoWindow = CreateNoWindow;

                Helpers.ConsolePrint(MinerTag(), "Start CMD: " + CMDconfigHandle.StartInfo.FileName + CMDconfigHandle.StartInfo.Arguments);
                CMDconfigHandle.Start();


                try
                {
                    if (!CMDconfigHandle.WaitForExit(60 * 1000))
                    {
                        CMDconfigHandle.Kill();
                        CMDconfigHandle.WaitForExit(5 * 1000);
                        CMDconfigHandle.Close();
                    }
                }
                catch (Exception e)
                {
                    Helpers.ConsolePrint("KillCMDBeforeOrAfterMining", e.ToString());
                }
            } catch (Exception ex)
            {
                Helpers.ConsolePrint(MinerTag(), ex.ToString());
            }
            return CMDconfigHandle;
        }
    }
}
