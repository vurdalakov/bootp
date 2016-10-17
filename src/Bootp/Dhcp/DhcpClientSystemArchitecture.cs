namespace dhcp
{
    using System;

    public enum DhcpClientSystemArchitecture : UInt16
    {
        ia86Pc = 0,             // legacy BIOS
        NecPc98 = 1,
        ia64pc = 2,
        DecAlpha = 3,
        x86 = 4,
        IntelLeanClient = 5,
        EfiIa32 = 6,            // EFI x86
        EfiBc = 7,              // EFI x64
        EfiXscale = 8,
        Efix8664 = 9            // EFI x64
    }
}
