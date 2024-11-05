using System;

namespace MSI.Afterburner
{
    [Flags]
    public enum MACM_SHARED_MEMORY_FLAG : uint
    {
        None = 0,
        LINK = 1,
        SYNC = 2,
        THERMAL = 4,
    }
}
