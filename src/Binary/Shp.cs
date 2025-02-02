using System;
using System.Runtime.InteropServices;

namespace ii.AscendancyLib.Binary
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ShpHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
        public char[] Version;
        public Int32 ImageCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImageInfo
    {
        public Int32 ImageOffset;
        public Int32 PaletteOffset; // 0 for default
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImageHeader
    {
        public Int16 Height; // need to add 1
        public Int16 Width; // need to add 1
        public Int16 YCentre;
        public Int16 XCentre;
        public Int32 xStart;
        public Int32 yStart;
        public Int32 xEnd;
        public Int32 yEnd;
    }
}