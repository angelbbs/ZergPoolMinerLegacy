using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ZergPoolMinerLegacy.Overclock.NativeOverclock;

namespace ZergPoolMinerLegacy.Overclock
{
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
                //PublicFunc.ConsolePrint("pOpenThread: " + pOpenThread.ToString());
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
    

    public static class Toolhelp32
    {
        public const uint Inherit = 0x80000000;
        public const uint SnapModule32 = 0x00000010;
        public const uint SnapAll = SnapHeapList | SnapModule | SnapProcess | SnapThread;
        public const uint SnapHeapList = 0x00000001;
        public const uint SnapProcess = 0x00000002;
        public const uint SnapThread = 0x00000004;
        public const uint SnapModule = 0x00000008;

        [DllImport("kernel32.dll")]
        static extern bool CloseHandle(IntPtr handle);
        [DllImport("kernel32.dll")]
        static extern IntPtr CreateToolhelp32Snapshot(uint flags, int processId);

        public static IEnumerable<T> TakeSnapshot<T>(uint flags, int id) where T : IEntry, new()
        {
            using (var snap = new Snapshot(flags, id))
                for (IEntry entry = new T { }; entry.TryMoveNext(snap, out entry);)
                    yield return (T)entry;
        }

        public interface IEntry
        {
            bool TryMoveNext(Toolhelp32.Snapshot snap, out IEntry entry);
        }

        public struct Snapshot : IDisposable
        {
            void IDisposable.Dispose()
            {
                Toolhelp32.CloseHandle(m_handle);
            }
            public Snapshot(uint flags, int processId)
            {
                m_handle = Toolhelp32.CreateToolhelp32Snapshot(flags, processId);
            }
            IntPtr m_handle;
        }
    }

    public static class Extensions
    {
        public static Process Parent(this Process p)
        {
            var entries = Toolhelp32.TakeSnapshot<PublicFunc.WinProcessEntry>(Toolhelp32.SnapAll, 0);
            var parentid = entries.First(x => x.th32ProcessID == p.Id).th32ParentProcessID;
            return Process.GetProcessById(parentid);
        }
    }

    class PublicFunc
    {
        [ThreadStatic]
        public static IPHostEntry heserver;
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

        [StructLayout(LayoutKind.Sequential)]
        public struct WinProcessEntry : Toolhelp32.IEntry
        {
            [DllImport("kernel32.dll")]
            public static extern bool Process32Next(Toolhelp32.Snapshot snap, ref WinProcessEntry entry);

            public bool TryMoveNext(Toolhelp32.Snapshot snap, out Toolhelp32.IEntry entry)
            {
                var x = new WinProcessEntry { dwSize = Marshal.SizeOf(typeof(WinProcessEntry)) };
                var b = Process32Next(snap, ref x);
                entry = x;
                return b;
            }

            public int dwSize;
            public int cntUsage;
            public int th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public int th32ModuleID;
            public int cntThreads;
            public int th32ParentProcessID;
            public int pcPriClassBase;
            public int dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public String fileName;
            //byte fileName[260];
            //public const int sizeofFileName = 260;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ParentProcessUtilities
        {
            // These members must match PROCESS_BASIC_INFORMATION
            internal IntPtr Reserved1;
            internal IntPtr PebBaseAddress;
            internal IntPtr Reserved2_0;
            internal IntPtr Reserved2_1;
            internal IntPtr UniqueProcessId;
            internal IntPtr InheritedFromUniqueProcessId;

            [DllImport("ntdll.dll")]
            private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref ParentProcessUtilities processInformation, int processInformationLength, out int returnLength);

            /// <summary>
            /// Gets the parent process of the current process.
            /// </summary>
            /// <returns>An instance of the Process class.</returns>
            public static Process GetParentProcess()
            {
                return GetParentProcess(Process.GetCurrentProcess().Handle);
            }

            /// <summary>
            /// Gets the parent process of specified process.
            /// </summary>
            /// <param name="id">The process id.</param>
            /// <returns>An instance of the Process class.</returns>
            public static Process GetParentProcess(int id)
            {
                Process process = Process.GetProcessById(id);
                return GetParentProcess(process.Handle);
            }

            /// <summary>
            /// Gets the parent process of a specified process.
            /// </summary>
            /// <param name="handle">The process handle.</param>
            /// <returns>An instance of the Process class.</returns>
            public static Process GetParentProcess(IntPtr handle)
            {
                ParentProcessUtilities pbi = new ParentProcessUtilities();
                int returnLength;
                int status = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out returnLength);
                if (status != 0)
                {
                    throw new Win32Exception(status);
                }
                try
                {
                    return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
                }
                catch (ArgumentException)
                {
                    // not found
                    return null;
                }
            }
        }


        public static string GetProcessName(int processId)
        {
            string query = "Select * From Win32_Process Where ProcessID = " + processId;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();

            foreach (var process in processList)
            {
                var processName = process["Name"];
                var processPath = process["ExecutablePath"];
                return processName.ToString();
            }
            return "";

        }

        public static int GetChildProcess(int ProcessId, string fname = "miner")
        {
            Process[] localByName = Process.GetProcessesByName(fname);
            try
            {
                foreach (var processName in localByName)
                {
                    /*
                    int t = Process.GetProcessById(processName.Id).Id;
                    int p = GetParentProcess(t);
                    if (p == ProcessId)
                    {
                        return t;
                    }
                    */
                    Process proc = Process.GetProcessById(processName.Id);
                    int t = proc.Id;
                    //int t = Process.GetProcessById(processName.Id).Id;
                    int p = proc.Parent().Id;
                    if (p == ProcessId)
                    {
                        proc.Dispose();
                        processName.Dispose();
                        localByName = null;
                        return t;
                    }
                    Thread.Sleep(1);
                }
            }
            catch (Win32Exception ex)
            {
            }

            return -1;
        }


        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);

        private static bool _consoleInitialized = false;
        private static System.Windows.Forms.RichTextBox richTextBox = new System.Windows.Forms.RichTextBox();
        private static string _debugText = "";

        static int GetWinVer(Version ver)
        {
            if (ver.Major == 6 & ver.Minor == 1)
                return 7;
            else if (ver.Major == 6 & ver.Minor == 2)
                return 8;
            else
                return 10;
        }



        public static bool IsProcessRunning(int id)
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
}
