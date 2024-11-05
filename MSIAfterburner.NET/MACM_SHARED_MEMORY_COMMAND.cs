using System;

namespace MSI.Afterburner
{
    [Flags]
    public enum MACM_SHARED_MEMORY_COMMAND : uint
    {
        None = 0,
        INIT = 11206656, // 0x00AB0000
        FLUSH = 11206657, // 0x00AB0001
    }
}
