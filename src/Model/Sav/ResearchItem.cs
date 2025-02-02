using ii.AscendancyLib.Model.Sav.Enum;
using System.Runtime.InteropServices;

namespace ii.AscendancyLib.Model.Sav
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ResearchItem
    {
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 60)]
        public char[] Name; // 0
        public ushort ResearchTime; // 60
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 5)]
        public ResearchableItem[] Requires; // 62
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 5)]
        public ResearchableItem[] RequiredBy; // 67
        public byte KnownBy; // 72 Bitmask
        public byte PendingAcknowledgement; // 73 Bitmask. Set to 1 when discovered, set to 0 when research screen opened
        public byte Unknown_74; // 74 Always 0
    }
}