using ii.AscendancyLib.Model.Sav.Enum;
using System.Runtime.InteropServices;

namespace ii.AscendancyLib.Model.Sav
{
    // 180 bytes
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public int CurrentDay_LoadScreen; // 0
        public int RaceCount; // 4
        public int Unknown_8; // 8 Always 0
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 7)]
        public Race[] Race; // 12
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 7)]
        public int[] Colour; // 40
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 44)]
        public byte[] Unknown_68; // 68
        public int SaveGameIndex; // 113 0-indexed
        public int ResearchBlockSize; // 117 i.e. 7538 (includes 2 byes for item count)
        public int StarSystemSize; // 121 i.e. 96
        public int PlanetSize; // 125 i.e. 123
        public int DiplomacySize; // 129 i.e 494
        public int StarLaneSize; // 133 i.e 39
        public int Unknown_137; // 137 Unknown size? always 414
        public int Unknown_141; // 141 Unknown size? always 13
        public int Unknown_145; // 145 Unknown size? always 218351
        public int CurrentDay_InGame; // 149
        public int LastStarSystemDisplayed; // 153 Index into StarSystem
        public int LastPlanetDisplayed; // 157 Index into Planet
        public WinMechanism WinMechanism; // 161
        public int VictoryPercentage; // 165
        public int Unknown_169; // Usually same as Unknown_177
        public int Unknown_173; // Often same as Unknown_177 / Unknown_169
        public int Unknown_177; // Usually same as Unknown_169
    }
}