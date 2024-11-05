using System;

namespace MSI.Afterburner
{
    [Serializable]
    public struct MACM_SHARED_MEMORY_HEADER
    {
        public uint signature;
        public uint version;
        public uint headerSize;
        public uint gpuEntryCount;
        public uint gpuEntrySize;
        public uint masterGpu;
        public MACM_SHARED_MEMORY_FLAG flags;
        public uint time;
        public MACM_SHARED_MEMORY_COMMAND command;
    }
}
