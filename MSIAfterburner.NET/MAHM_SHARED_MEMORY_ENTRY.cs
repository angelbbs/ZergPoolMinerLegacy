using System;
using System.Runtime.InteropServices;

namespace MSI.Afterburner
{
    [Serializable]
    public struct MAHM_SHARED_MEMORY_ENTRY
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
        public char[] srcName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
        public char[] srcUnits;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
        public char[] localizedSrcName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
        public char[] localizedSrcUnits;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
        public char[] recommendedFormat;
        public float data;
        public float minLimit;
        public float maxLimit;
        public MAHM_SHARED_MEMORY_ENTRY_FLAG flags;
        public uint gpu;
        public uint srcId;
    }
}
