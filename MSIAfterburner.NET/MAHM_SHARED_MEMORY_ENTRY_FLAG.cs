using System;

namespace MSI.Afterburner
{
    [Flags]
    public enum MAHM_SHARED_MEMORY_ENTRY_FLAG : uint
    {
        None = 0,
        SHOW_IN_OSD = 1,
        SHOW_IN_LCD = 2,
        SHOW_IN_TRAY = 4,
    }
}
