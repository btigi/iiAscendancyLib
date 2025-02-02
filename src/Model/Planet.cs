using ii.AscendancyLib.Model.Enum;
using System.Runtime.InteropServices;

namespace ii.AscendancyLib.Model
{
    // 123 bytes
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Planet
    {
        public short Unknown_0; // Always 0
        public float X; // 2150 is edge of system
        public float Y; // Always 0 (-ve values move upwards)
        public float Z;
        public short SystemId;
        public short PlanetIndex; // e.g. Sol I -> 1, Sol II -> 2, Sol III -> 3
        public ushort PlanetStructureBlockSize; // i.e. TotalSquareCount * sizeof(PlanetStructure), tiny = 68, small = 84, medium = 132, large = 220, enormous = 332
        public short Unknown_18; // Always 33 ?
        public PlanetSize PlanetSize;
        public PlanetType PlanetType;
        public short PlanetSquareCount; // Number of planet squares
        public short TotalSquareCount; // Number of total squares
        public short FreeSquareCount; // Number of free squares
        public short OrbitalSquareCount; // Number of space squares
        public short BlackSquareCount; // Number of black squares
        public short Unknown_34; // Always 0?
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 30)]
        public char[] Name;
        public short CurrentPopulation;
        public short Industry;
        public short Research;
        public short Prosperity;
        public short MaximumPopulationCurrent;
        public short UsedPopulation;
        public short PopulationGrowthDiscount; // Subtract this from the growth rate (i.e. 50) to determine when population will next grow
        public short DaysRemainingOnCurrentProject;
        public short CurrentProjectSquare;
        public PlanItem CurrentProject;
        public short MaximumPopulationBase;
        public byte OwnerRaceIndex; // 0xff = no-one
        public short ShieldStrength; // i.e. (Planetary shield count * 256) + (Orbital shield count * 258)
        public ManagementType ManagementType;
        public int Unknown_102; // Always 0 ?
        public int Unknown_106; // Always 0 ?
        public byte XenoarcheologicalRuins; // 0 - no, 2 - yes
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 18)]
        public byte[] Unknown_108;
    }
}