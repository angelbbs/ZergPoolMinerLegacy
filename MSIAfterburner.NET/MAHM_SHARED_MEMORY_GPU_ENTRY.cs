using System;
using System.Runtime.InteropServices;

namespace MSI.Afterburner
{
    [Serializable]
    public struct MAHM_SHARED_MEMORY_GPU_ENTRY
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
        public char[] gpuId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
        public char[] family;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
        public char[] device;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
        public char[] driver;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
        public char[] BIOS;
        public uint memAmount;
    }
}
