using ii.AscendancyLib.Model.Sav.Enum;
using System.Collections.Generic;

namespace ii.AscendancyLib.Model.Sav
{
    public class SavFile
    {
        public Header Header;
        public List<ResearchItem> ResearchItems = new List<ResearchItem>();
        public List<short> CurrentResearchItems = new List<short>();
        public byte[] Unknown1 = new byte[10];
        public int Unknown2; // always 0
        public ManagementType ResearchManagementType;
        public int Unknown3; // always -1
        public List<StarSystem> StarSystems = new List<StarSystem>();
        public List<PlanetStructure> PlanetStructures = new List<PlanetStructure>();
        public List<Planet> Planets = new List<Planet>();
        public short Unknown4;
        public List<Ship> Ships = new List<Ship>();
        public short Unknown5;
        public List<Diplomacy> Diplomacies = new List<Diplomacy>();
        public short Unknown6;
        public List<StarLane> StarLanes = new List<StarLane>();
        public List<StarLaneCount> StarLaneCounts = new List<StarLaneCount>();
        public byte[] Unknown7 = new byte[7672];
    }
}