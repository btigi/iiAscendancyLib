using System.Runtime.InteropServices;

namespace ii.AscendancyLib.Binary
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FileName
    {
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 50)]
        public char[] filename;
    }
}