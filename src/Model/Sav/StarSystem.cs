using ii.AscendancyLib.Model.Sav.Enum;
using System.Runtime.InteropServices;

namespace ii.AscendancyLib.Model.Sav
{
    // 96 bytes
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class StarSystem
    {
        public StarType StarType; // 0
        public int SystemId; // 1
        public int X; // 5
        public int Y; // 9
        public int Z; // 13
        public byte Unknown_17; // 17
        public byte Unknown_18; // 18
        public byte Unknown_19; // 19 - planatary indicator (rings), set high bit to indicate home world (TODO: of who)
        public byte Unknown_20; // 20
        public char ShipIndicator; // 21 - bitmask of player indices
        public byte Unknown_22; // 22
        public byte Unknown_23; // 23
        public byte Unknown_24; // 24
        public short Unknown_25; // 25
        public byte Unknown_27; // 27
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 12)]
        public char[] Name; // 28
        public int Unknown_40; // 40 Always 0
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 24)]
        public byte[] Unknown_44; // 44
        public short StarLaneCount; // 68
        public ushort SizeOfPlanetBlock; // 70 i.e. sizeOf(Planet) * planet count ?
        public short Unknown_72; // 72 Always 34 or 35
        public ushort SizeOfPlanetBlock_2; // 74 i.e. sizeOf(Planet) * (planet count + 1) ?
        public short Unknown_76; // 76 Always 34 or 35
        public ushort SizeOfPlanetBlock_3; // 78 i.e. sizeOf(Planet) * planet count ?
        public short Unknown_80; // 80 Always 34 or 35
        public ushort SizeOfPlanetBlock_4; // 82 i.e. sizeOf(Planet) * planet count ?
        public short Unknown_84; // 84 Always 34 or 35
        public ushort SizeOfPlanetBlock_5; // 86 i.e. sizeOf(Planet) * planet count ?
        public short Unknown_88; // 86 Always 34 or 35
        public short PlanetCount; // 90
        public int Unknown_92; // 92  Always 0xffffffff
    }
}