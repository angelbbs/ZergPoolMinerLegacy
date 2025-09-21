using ManagedCuda.Nvml;
using NvAPIWrapper.Native;
using NvAPIWrapper.Native.General;
using NvAPIWrapper.Native.GPU.Structures;
using NVIDIA.NVAPI;
using NvidiaGPUGetDataHost.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using static NvAPIWrapper.Native.GPU.Structures.PrivatePowerPoliciesStatusV1;

namespace NvidiaGPUGetDataHost
{
    class Program
    {
        private static bool isclosing = false;
        [Serializable]
        public struct GpuData
        {
            public uint busID;
            public string Name;
        }

        private readonly nvmlDevice _nvmlDevice;
        private static string nvmlRootPath = "";
        public static byte[] RawSerialize(object anything)
        {
            int length = Marshal.SizeOf(anything);
            IntPtr num = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(anything, num, false);
            byte[] destination = new byte[length];
            Marshal.Copy(num, destination, 0, length);
            Marshal.FreeHGlobal(num);
            return destination;
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

        private static Assembly AppDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains("log4net")) //имя dll
                return Assembly.Load(Resources.log4net); //dll в ресурсах
            return null;
        }

        [STAThread]
        public static void Main()
        {
            if (System.Diagnostics.Process.GetProcessesByName(Application.ProductName).Length > 1)
            {
                return;
            }

            AppDomain.CurrentDomain.AssemblyResolve += AppDomain_AssemblyResolve;

            Logger.ConfigureWithFile();
            Logger.ConsolePrint("NvidiaGPUGetDataHost", "Start");
            try
            {
                uint devCount = 0;
                nvmlReturn ret;
                /*
                var pathVar = Environment.GetEnvironmentVariable("PATH");
                pathVar += ";" + Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) +
                               "\\NVIDIA Corporation\\NVSMI"; ;
                Environment.SetEnvironmentVariable("PATH", pathVar);
                */
                if (!TryAddNvmlToEnvPath()) return;
                nvmlDevice _nvmlDevice = new nvmlDevice();
                nvmlReturn nvmlLoaded = NvmlNativeMethods.nvmlInit();

                if (nvmlLoaded != nvmlReturn.Success)
                {
                    Logger.ConsolePrint("NvidiaGPUGetDataHost", "NVSMI Error: " + nvmlLoaded);
                    return;
                }

                ret = NvmlNativeMethods.nvmlDeviceGetCount(ref devCount);

                if (ret != nvmlReturn.Success)
                {
                    Logger.ConsolePrint("NvidiaGPUGetDataHost", "nvmlDeviceGetCount error: " + ret.ToString());
                    return;
                }
                Logger.ConsolePrint("NvidiaGPUGetDataHost", "NVIDIA devices count: " + devCount.ToString());
                nvmlDevice device = new nvmlDevice();
                nvmlPciInfo devPci = new nvmlPciInfo();
                List<GpuData> GpuDataList = new List<GpuData>();
                for (uint dev = 0; dev < devCount; dev++)
                {
                    NvmlNativeMethods.nvmlDeviceGetHandleByIndex(dev, ref device);
                    NvmlNativeMethods.nvmlDeviceGetName(device, out string devName);
                    NvmlNativeMethods.nvmlDeviceGetPciInfo(device, ref devPci);
                    GpuData _GpuData = new GpuData();
                    _GpuData.busID = devPci.bus;
                    _GpuData.Name = devName;
                    GpuDataList.Add(_GpuData);
                    Logger.ConsolePrint("NvidiaGPUGetDataHost", "NVIDIA device: " + devName + " busID: " + devPci.bus.ToString()); ;
                }
                
                int ticks = 0;
                int errors = 0;

                int devn = 0;
                var _power = 0u;
                var _fan = 0u;
                var _load = 0u;
                var _loadMem = 0u;
                var _temp = 0u;
                var _tempMem = 0u;
                var _tempHotSpot = 0u;

                int size = Marshal.SizeOf(devn) + Marshal.SizeOf(_power) + Marshal.SizeOf(_fan) + Marshal.SizeOf(_load) + Marshal.SizeOf(_loadMem) + Marshal.SizeOf(_temp) + Marshal.SizeOf(_tempMem);

                MemoryMappedFile sharedMemory = MemoryMappedFile.CreateOrOpen("NvidiaGPUGetDataHost", size * devCount + Marshal.SizeOf(devCount));
                do
                {
                    for (int dev = 0; dev < devCount; dev++)
                    {
                        
                        ret = NvmlNativeMethods.nvmlDeviceGetHandleByIndex((uint)dev, ref _nvmlDevice);
                        if (ret != nvmlReturn.Success && ret != nvmlReturn.NVML_ERROR_NO_DATA)
                        {
                            Logger.ConsolePrint("NvidiaGPUGetDataHost", "nvmlDeviceGetHandleByIndex error: " + ret.ToString());
                            if (!ret.ToString().Contains("NotSupported"))
                            {
                                errors++;
                                //break;
                            }
                        }
                        Thread.Sleep(50);
                        //537.58++ на 4060 не работает потребление (546.17, 551.86, 552.44)
                        //537.13 работает
                        ret = NvmlNativeMethods.nvmlDeviceGetPowerUsage(_nvmlDevice, ref _power);// <- mem leak 461.40+
                        NvmlNativeMethods.nvmlDeviceGetName(_nvmlDevice, out string devName);
                        if (devName.Contains("2070"))//570+ driver
                        {
                            _power = _power ^ 512;
                        }
                        //Logger.ConsolePrint("NvidiaGPUGetDataHost", "_power: " + _power.ToString());
                        if (ret != nvmlReturn.Success && ret != nvmlReturn.NVML_ERROR_NO_DATA)
                        {
                            if (!ret.ToString().Contains("NotSupported"))
                            {
                                Logger.ConsolePrint("NvidiaGPUGetDataHost", "nvmlDeviceGetPowerUsage error: " + ret.ToString());
                                errors++;
                            }
                            //break;
                        }
                        Thread.Sleep(50);
                        ret = NvmlNativeMethods.nvmlDeviceGetFanSpeed(_nvmlDevice, ref _fan);
                        if (ret != nvmlReturn.Success && ret != nvmlReturn.NVML_ERROR_NO_DATA)
                        {
                            if (!ret.ToString().Contains("NotSupported"))
                            {
                                Logger.ConsolePrint("NvidiaGPUGetDataHost", "nvmlDeviceGetFanSpeed error: " + ret.ToString());
                                errors++;
                            }
                            //break;
                        }
                        Thread.Sleep(50);
                        var rates = new nvmlUtilization();
                        ret = NvmlNativeMethods.nvmlDeviceGetUtilizationRates(_nvmlDevice, ref rates);
                        if (ret != nvmlReturn.Success && ret != nvmlReturn.NVML_ERROR_NO_DATA)
                        {
                            if (!ret.ToString().Contains("NotSupported"))
                            {
                                Logger.ConsolePrint("NvidiaGPUGetDataHost", "nvmlDeviceGetUtilizationRates error: " + ret.ToString());
                            }
                            //break;
                        }
                        Thread.Sleep(50);
                        _load = rates.gpu;
                        _loadMem = rates.memory;

                        //Logger.ConsolePrint("NvidiaGPUGetDataHost", "_load: " + _load.ToString());
                        ret = NvmlNativeMethods.nvmlDeviceGetTemperature(_nvmlDevice, nvmlTemperatureSensors.Gpu, ref _temp);
                        if (ret != nvmlReturn.Success && ret != nvmlReturn.NVML_ERROR_NO_DATA)
                        {
                            if (!ret.ToString().Contains("NotSupported"))
                            {
                                Logger.ConsolePrint("NvidiaGPUGetDataHost", "nvmlDeviceGetTemperature(Gpu) error: " + ret.ToString());
                                errors++;
                            }
                        }
                        
                        Thread.Sleep(50);
                        bool nvApierror = false;
                        bool GetPhysicalGPUsError = false;
                        bool GetTCCPhysicalGPUsError = false;
                        string _GetPhysicalGPUsError = "";
                        string _GetTCCPhysicalGPUsError = "";
                        try
                        {
                            var gpus0 = NvAPIWrapper.GPU.PhysicalGPU.GetPhysicalGPUs();
                        } catch (Exception ex)
                        {
                            GetPhysicalGPUsError = true;
                            _GetPhysicalGPUsError = ex.Message;
                        }

                        try
                        {
                            var gpus0 = NvAPIWrapper.GPU.PhysicalGPU.GetTCCPhysicalGPUs();
                        }
                        catch (Exception ex1)
                        {
                            GetTCCPhysicalGPUsError = true;
                            _GetTCCPhysicalGPUsError = ex1.Message;
                        }
                        if (GetPhysicalGPUsError && GetTCCPhysicalGPUsError)
                        {
                            Logger.ConsolePrint("NvidiaGPUGetDataHost", "NvAPIWrapper error: " + _GetPhysicalGPUsError + " " +
                                _GetTCCPhysicalGPUsError);
                            nvApierror = true;
                        }

                        if (!nvApierror)
                        {
                            /*
                            List<int> physicalBusIds = new List<int>();
                            int count;
                            var gpuHandles = new NVIDIA.NVAPI.NvPhysicalGpuHandle[64];
                            NvStatus status = NVAPI.NvAPI_EnumPhysicalGPUs(gpuHandles, out count);

                            if (status == NvStatus.OK)
                            {
                                foreach (var h in gpuHandles)
                                {
                                    NVAPI.NvAPI_GPU_GetBusID(h, out int busid);
                                    if (!physicalBusIds.Contains(busid))
                                    {
                                        physicalBusIds.Add(busid);
                                    }
                                }
                            } else
                            {
                                Logger.ConsolePrint("QueryCudaDevices", "Enum physical GPUs failed with status: " + status);
                            }
                            */

                            List<NvAPIWrapper.GPU.PhysicalGPU> gpus = new List<NvAPIWrapper.GPU.PhysicalGPU>();
                            if (!GetPhysicalGPUsError)
                            {
                                gpus.AddRange(NvAPIWrapper.GPU.PhysicalGPU.GetPhysicalGPUs());
                            }
                            if (!GetTCCPhysicalGPUsError)
                            {
                                gpus.AddRange(NvAPIWrapper.GPU.PhysicalGPU.GetTCCPhysicalGPUs());
                            }
                            //NvAPIWrapper.GPU.PhysicalGPU[] gpus = NvAPIWrapper.GPU.PhysicalGPU.GetTCCPhysicalGPUs();
                            var sorted = gpus.OrderBy(x => x.GPUId).ToArray();

                            if (sorted.Count() != devCount)
                            //if (count != devCount)
                            {
                                Logger.ConsolePrint("NvidiaGPUGetDataHost", "GetPhysicalGPUs count missmath: " + sorted.Count().ToString());
                                
                                List<int> busIds = new List<int>();
                                foreach (var _g in sorted)
                                {
                                    busIds.Add(_g.BusInformation.BusId);
                                }
                                
                                foreach (var g in GpuDataList)
                                {
                                    if (!busIds.Contains((int)g.busID))
                                    //if (!physicalBusIds.Contains((int)g.busID))
                                    {
                                        Logger.ConsolePrint("NvidiaGPUGetDataHost", "Stuck GPU: " + g.Name + " BusId: " + g.busID.ToString());
                                    }
                                }
                            }
                            var gpu = sorted[dev];
                            NvmlNativeMethods.nvmlDeviceGetName(_nvmlDevice, out string name);
                            //Logger.ConsolePrint("NvidiaGPUGetDataHost", "dev: " + dev + " nvml.name: " + name + " api.FullName: " + gpu.FullName + " api.GPUId: " + gpu.GPUId.ToString());

                            var handle = GPUApi.GetPhysicalGPUFromGPUID(gpu.GPUId);
                            // find bits
                            var maxBit = 0;
                            for (; maxBit < 32; maxBit++)
                            {
                                try
                                {
                                    GPUApi.QueryThermalSensors(handle, 1u << maxBit);
                                }

                                catch
                                {
                                    break;
                                }
                            }
                            
                            //Logger.ConsolePrint("NvidiaGPUGetDataHost", "maxBit: " + maxBit.ToString());
                            if (maxBit == 0)
                            {
                                return;
                            }

                            float[] t1 = new float[maxBit];
                            try
                            {
                                var temp = GPUApi.QueryThermalSensors(handle, (1u << maxBit) - 1);
                                t1 = temp.Temperatures;
                            }
                            catch
                            {
                                // ignore
                            }
                            /*
                            for (int i = 0; i < maxBit;i++)
                            {
                                Logger.ConsolePrint("NvidiaGPUGetDataHost", "t1[" + i.ToString() + "]: " + t1[i].ToString());
                            }
                            */
                            if (t1.Length >= 10)
                            {
                                _tempMem = (uint)t1[9];// 2-hotspot, 9-mem
                                if (_tempMem <= 0)
                                {
                                    _tempMem = (uint)t1[7];//laptop?

                                    //if (_tempMem <= 0)
                                    //{
                                      //  _tempMem = (uint)t1[2];
                                    //}
                                }
                            }
                            
                            if (_power == 0u)
                            {
                                /* 
                                Logger.ConsolePrint("NvidiaGPUGetDataHost*", GPUApi.GetFullName(handle) + " power:" +  _power.ToString());
                                var p1 = NvAPIWrapper.Native.GPUApi.ClientPowerPoliciesGetInfo(handle).PowerPolicyInfoEntries;
                                var p2 = NvAPIWrapper.Native.GPUApi.ClientPowerPoliciesGetStatus(handle).PowerPolicyStatusEntries;
                                var p3 = NvAPIWrapper.Native.GPUApi.ClientPowerTopologyGetStatus(handle).PowerPolicyStatusEntries;

                                //PrivatePowerPoliciesStatusV1 pol = new PrivatePowerPoliciesStatusV1();
                                //GPUApi.ClientPowerPoliciesSetStatus(handle, pol);

                                var p4 = NvAPIWrapper.Native.GPUApi.GetPerformanceStates20(handle);
                                Logger.ConsolePrint("NvidiaGPUGetDataHost*", "GetPerformanceStates20 Clocks " + p4.Clocks.Count().ToString());
                                Logger.ConsolePrint("NvidiaGPUGetDataHost*", "GetPerformanceStates20 GeneralVoltages " + p4.GeneralVoltages.Count().ToString());
                                Logger.ConsolePrint("NvidiaGPUGetDataHost*", "GetPerformanceStates20 PerformanceStates " + p4.PerformanceStates.Count().ToString());
                                Logger.ConsolePrint("NvidiaGPUGetDataHost*", "GetPerformanceStates20 Voltages " + p4.Voltages.Count().ToString());

                                Logger.ConsolePrint("NvidiaGPUGetDataHost*", "ClientPowerPoliciesGetInfo count " + p1.Count().ToString());
                                foreach (var pt in p1)
                                {
                                    Logger.ConsolePrint("NvidiaGPUGetDataHost", pt.DefaultPowerInPCM.ToString() + " " +
                                        pt.MaximumPowerInPCM.ToString() + " " +
                                      " " + pt.MinimumPowerInPCM.ToString());
                                }
                                Logger.ConsolePrint("NvidiaGPUGetDataHost*", "ClientPowerPoliciesGetStatus count " + p2.Count().ToString());
                                foreach (var pt in p2)
                                {
                                    Logger.ConsolePrint("NvidiaGPUGetDataHost", pt.PerformanceStateId.ToString() + " " +
                                        pt.PowerTargetInPCM.ToString());
                                }
                                Logger.ConsolePrint("NvidiaGPUGetDataHost*", "ClientPowerTopologyGetStatus count " + p3.Count().ToString());
                                foreach (var pt in p3)
                                {
                                    Logger.ConsolePrint("NvidiaGPUGetDataHost", pt.Domain.ToString() + " " + 
                                        pt.PowerUsageInPCM.ToString());
                                    _power = pt.PowerUsageInPCM;
                                }

                                foreach (var pt in p4.Voltages)
                                {
                                    Logger.ConsolePrint("NvidiaGPUGetDataHost", pt.Key.ToString() + " " +
                                        pt.Value.Count().ToString());
                                }
                                */
                            }
                            

                            /*
                            for (int i = 0; i< t1.Count();i++)
                            {
                                Logger.ConsolePrint("NvidiaGPUGetDataHost", "t1[" + i.ToString() + "]: " + t1[i].ToString());
                            }
                            */
                        }
                        Thread.Sleep(50);

                        using (MemoryMappedViewAccessor writer = sharedMemory.CreateViewAccessor(0, size * devCount + Marshal.SizeOf(devCount)))
                        {
                            //if (ret == nvmlReturn.Success)

                            {
                                writer.WriteArray<byte>(0, RawSerialize(devCount), 0, Marshal.SizeOf(devCount));
                            }
                            /*
                            else
                            {
                                writer.WriteArray<byte>(0, RawSerialize(0), 0, Marshal.SizeOf(devCount));
                            }
                            */
                            //writer.WriteArray<byte>(size * dev + Marshal.SizeOf(devCount), BitConverter.GetBytes(Convert.ToInt32((long)_nvmlDevice.Pointer % Int32.MaxValue)), 0, Marshal.SizeOf(dev));
                            writer.WriteArray<byte>(size * dev + Marshal.SizeOf(devCount), BitConverter.GetBytes(dev), 0, Marshal.SizeOf(dev));
                            writer.WriteArray<byte>(size * dev + Marshal.SizeOf(devCount) + Marshal.SizeOf(dev), BitConverter.GetBytes(_power), 0, Marshal.SizeOf(_power));
                            writer.WriteArray<byte>(size * dev + Marshal.SizeOf(devCount) + Marshal.SizeOf(dev) + Marshal.SizeOf(_power), BitConverter.GetBytes(_fan), 0, Marshal.SizeOf(_fan));
                            writer.WriteArray<byte>(size * dev + Marshal.SizeOf(devCount) + Marshal.SizeOf(dev) + Marshal.SizeOf(_power) + Marshal.SizeOf(_fan), BitConverter.GetBytes(_load), 0, Marshal.SizeOf(_load));
                            writer.WriteArray<byte>(size * dev + Marshal.SizeOf(devCount) + Marshal.SizeOf(dev) + Marshal.SizeOf(_power) + Marshal.SizeOf(_fan) + Marshal.SizeOf(_load), BitConverter.GetBytes(_loadMem), 0, Marshal.SizeOf(_loadMem));
                            writer.WriteArray<byte>(size * dev + Marshal.SizeOf(devCount) + Marshal.SizeOf(dev) + Marshal.SizeOf(_power) + Marshal.SizeOf(_fan) + Marshal.SizeOf(_load) + Marshal.SizeOf(_loadMem), BitConverter.GetBytes(_temp), 0, Marshal.SizeOf(_temp));
                            writer.WriteArray<byte>(size * dev + Marshal.SizeOf(devCount) + Marshal.SizeOf(dev) + Marshal.SizeOf(_power) + Marshal.SizeOf(_fan) + Marshal.SizeOf(_load) + Marshal.SizeOf(_loadMem) + Marshal.SizeOf(_temp), BitConverter.GetBytes(_tempMem), 0, Marshal.SizeOf(_tempMem));
                        }
                    }
                    Thread.Sleep(100);
                    //sharedMemory.Dispose();

                    Process currentProc = Process.GetCurrentProcess();
                    double bytesInUse = currentProc.PrivateMemorySize64;
                    if (ticks > 120)
                    {
                        GC.Collect();
                    }
                    if (bytesInUse > 256 * 1048576)
                    {
                        NvmlNativeMethods.nvmlShutdown();
                        Logger.ConsolePrint("NvidiaGPUGetDataHost", "Memory leak exceeded limit in 256MB. Closing");
                        //System.Windows.Forms.Application.Restart();
                        System.Environment.Exit(1);
                    }
                    if (errors > 10)
                    {
                        NvmlNativeMethods.nvmlShutdown();

                        Logger.ConsolePrint("NvidiaGPUGetDataHost", "Too many errors. Closing");
                        //System.Windows.Forms.Application.Restart();
                        System.Environment.Exit(1);
                    }
                    ticks++;
                    Thread.Sleep(100);
                } while (true);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Logger.ConsolePrint("NvidiaGPUGetDataHost", "Exception: " + ex.ToString());
            }
        }
        private static bool TryAddNvmlToEnvPath()
        {
            string defaultpath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) +
                               "\\NVIDIA Corporation\\NVSMI";

            var pathVar = Environment.GetEnvironmentVariable("PATH");
            if (Directory.Exists(defaultpath) && File.Exists("nvidia-smi.exe") && File.Exists("nvml.dll"))
            {
                nvmlRootPath = defaultpath;
                pathVar += ";" + defaultpath;
                Logger.ConsolePrint("NvidiaGPUGetDataHost", $"Adding NVML to PATH='{defaultpath}'");
                Environment.SetEnvironmentVariable("PATH", pathVar);
                return true;
            }
            else
            {
                nvmlRootPath = GetNVMLFiles();
                if (!string.IsNullOrEmpty(nvmlRootPath))
                {
                    pathVar += ";" + nvmlRootPath;
                    Logger.ConsolePrint("NvidiaGPUGetDataHost", $"Adding NVML to PATH='{nvmlRootPath}'");
                    Environment.SetEnvironmentVariable("PATH", pathVar);
                    return true;
                }
            }
            Logger.ConsolePrint("NvidiaGPUGetDataHost", "Warning! nvml.dll or nvidia-smi.exe not found!");
            return false;
        }

        private static string SafeGetProperty(ManagementBaseObject mbo, string key)
        {
            try
            {
                var o = mbo.GetPropertyValue(key);
                if (o != null)
                {
                    return o.ToString();
                }
            }
            catch { }

            return "key is null";
        }
        private static string NVIDIADriver;
        private static string GetNVMLFiles()
        {
            var moc = new ManagementObjectSearcher("root\\CIMV2",
                           "SELECT * FROM Win32_VideoController WHERE PNPDeviceID LIKE 'PCI%'").Get();
            string DriverVersion;
            string Name;
            string PnpDeviceID;
            foreach (var manObj in moc)
            {
                DriverVersion = SafeGetProperty(manObj, "DriverVersion");
                Name = SafeGetProperty(manObj, "Name");
                PnpDeviceID = SafeGetProperty(manObj, "PNPDeviceID");
                if (Name.ToLower().Contains("nvidia") ||
                                PnpDeviceID.Contains("10DE"))
                {
                    NVIDIADriver = DriverVersion;
                }
            }

                DateTime dt = new DateTime();
            string pathToFiles = null;
            string DriverFolder = "C:\\Windows\\System32\\DriverStore\\FileRepository";
            string nvFolder = "\\nv_dispig.inf_amd64_7e5fd280efaa5445";
            /*
            if (File.Exists(DriverFolder + nvFolder + "\\nvidia-smi.exe"))
            {
                return DriverFolder + nvFolder;
            }
            */
            string driverline = "Unknown driver version";
            try
            {
                string[] folders = Directory.GetDirectories(DriverFolder);
                foreach (string folder in folders)
                {
                    string[] files = Directory.GetFiles(folder);
                    foreach (string filename in files)
                    {
                        if (filename.Contains("nvml.dll"))
                        {
                            var inf0 = folder.IndexOf("FileRepository\\", 0) + 15;
                            var inf1 = folder.IndexOf(".inf", 0);
                            var inf = folder.Substring(inf0, inf1 - inf0) + ".inf";

                            DateTime DriverDate = new DateTime();
                            var fdata = File.ReadAllLines(folder + "\\" + inf);

                            foreach (string line in fdata)
                            {
                                if (line.ToLower().Contains("driverver"))
                                {
                                    var i0 = line.IndexOf("= ", 0) + 2;
                                    var i1 = line.IndexOf(", ", 0);
                                    var dd = line.Substring(i0, i1 - i0);
                                    DriverDate = DateTime.ParseExact(dd, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                                    driverline = line;
                                    break;
                                }
                            }

                            //FileInfo fi = new FileInfo(filename);
                            Logger.ConsolePrint("GetNVMLFiles", "Found " + folder + " " + driverline);
                            if (DateTime.Compare(DriverDate, dt) > 0)
                            {
                                dt = DriverDate;
                                pathToFiles = folder;
                            }
                        }
                    }
                    if (driverline.Contains(NVIDIADriver))
                    {
                        pathToFiles = folder;
                        break;
                    }
                }
            }
            catch (System.Exception e)
            {
                Logger.ConsolePrint("GetNVMLFiles", e.ToString());
            }
            return pathToFiles;
        }
        private static string GetNVMLFiles0()
        {
            DateTime dt = new DateTime();
            string pathToFiles = null;
            string DriverFolder = "C:\\Windows\\System32\\DriverStore\\FileRepository";
            string nvFolder = "\\nv_dispig.inf_amd64_7e5fd280efaa5445";
            if (File.Exists(DriverFolder + nvFolder + "\\nvidia-smi.exe"))
            {
                return DriverFolder + nvFolder;
            }
            try
            {
                string[] folders = Directory.GetDirectories(DriverFolder);
                foreach (string folder in folders)
                {
                    string[] files = Directory.GetFiles(folder);
                    foreach (string filename in files)
                    {
                        if (filename.Contains("nvml.dll"))
                        {
                            FileInfo fi = new FileInfo(filename);
                            if (DateTime.Compare(fi.CreationTime, dt) > 0)
                            {
                                dt = fi.CreationTime;
                                pathToFiles = folder;
                            }
                        }
                    }

                }
            }
            catch (System.Exception e)
            {
                Logger.ConsolePrint("GetNVMLFiles", e.ToString());
            }
            return pathToFiles;
        }

    }
}
