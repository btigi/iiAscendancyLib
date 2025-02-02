using ii.AscendancyLib.Model.Sav.Enum;
using System.Runtime.InteropServices;

namespace ii.AscendancyLib.Model.Sav
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PlanetStructure
    {
        public SquareType SquareType;
        public Structure StructureId;
        public byte Complete; //  0 - no, 1 - yes
        public byte Special; // Special

        // Special:
        //  For ships this is the ship index
        //  For shields this is the shield strength (orbital shield = 15, orbital mega shield = 35)
        //  For planetary defences attack strength (orbital missile base = 1, short range whopper = 3)
    }
}