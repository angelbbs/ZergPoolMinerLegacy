using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZergPoolMiner.Configs;
using ZergPoolMiner.Miners.Grouping;

namespace ZergPoolMiner.Devices
{
    //limiting the maximum temperature of gpu core and gpu memory
    public static class HeatingControl
    {
        private static bool _running = false;
        public static void CheckTemperature()
        {
            if (_running) return;
            _running = true;
            double memtempLimit = ConfigManager.GeneralConfig.GPUmemTempTreshold;
            double coretempLimit = ConfigManager.GeneralConfig.GPUcoreTempTreshold;
            try
            {
                foreach (var dev in ComputeDeviceManager.Available.Devices)
                {
                    var busID = dev.BusID;
                    var name = dev.Name;
                    var dt = dev.DeviceType;
                    var mn = dev.MinerName;
                    double coretemp = dev.Temp;
                    double memtemp = dev.TempMemory;
                    var d = Miner._allPidData.Find(e => e.ids.Contains(busID));
                    if (d is object && d != null && dt != ZergPoolMinerLegacy.Common.Enums.DeviceType.CPU)
                    {
                        if (memtemp >= memtempLimit || coretemp >= coretempLimit)
                        {
                            dev.overheating = true;
                            var process = Process.GetProcessById(d.Pid);
                            Miner.suspendedPidList.Add(d.Pid);
                            process.Suspend();
                            Helpers.ConsolePrint("HeatingControl", "busID:" + busID.ToString() + " (" +
                                name + " " + 
                                mn +
                                ") OVERHEATED! " +
                                "Core temp: " + coretemp.ToString() + " " +
                                "Memory temp: " + memtemp.ToString() + ". " +
                                "Suspend for " + ConfigManager.GeneralConfig.GPUoverheatSuspendTime.ToString() +
                                " seconds");
                            int TickSleep = 0;
                            int SleepTime = ConfigManager.GeneralConfig.GPUoverheatSuspendTime;
                            do
                            {
                                Thread.Sleep(1000);
                                TickSleep++;
                            } while (TickSleep < SleepTime && IsProcessRunning(d.Pid));

                            //var sleep = (int)(2000 * (memtemp - memtempLimit));
                            //Thread.Sleep(sleep);
                            if (process is object) process.Resume();
                            Miner.suspendedPidList.Remove(d.Pid);
                            dev.overheating = false;
                            Helpers.ConsolePrint("HeatingControl", "busID: " + busID.ToString() + " " +
                                name + " "  +
                                mn +
                                " Resume mining");
                        }
                    }
                }
            } catch (Exception ex)
            {
                Helpers.ConsolePrintError("HeatingControl", ex.ToString());
            }
            _running = false;
        }
        private static bool IsProcessRunning(int id)
        {
            try
            {
                Process[] allProcessesOnLocalMachine = Process.GetProcesses();
                foreach (Process process in allProcessesOnLocalMachine)
                {
                    if (process.Id == id) return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }
    }
    public static class ProcessExtension
    {
        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);

        public static void Suspend(this Process process)
        {
            foreach (ProcessThread thread in process.Threads)
            {
                var pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }
                SuspendThread(pOpenThread);
            }
        }

        public static void Resume(this Process process)
        {
            foreach (ProcessThread thread in process.Threads)
            {
                var pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }
                ResumeThread(pOpenThread);
            }
        }

        [DllImport("ntdll.dll", PreserveSig = false)]
        public static extern void NtSuspendProcess(IntPtr processHandle);
        [DllImport("ntdll.dll", PreserveSig = false)]
        public static extern void NtResumeProcess(IntPtr processHandle);
    }
}
