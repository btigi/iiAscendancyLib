using ii.AscendancyLib.Model.Sav;
using ii.AscendancyLib.Model.Sav.Enum;
using System.IO;

namespace ii.AscendancyLib.Reader
{
    public class SavReader
    {
        public SavFile Load(string filename)
        {
            var file = new SavFile();
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    file.Header = (Header)Common.ReadStruct(br, typeof(Header));


                    var reseachItemCount = br.ReadInt16();
                    for (int i = 0; i < reseachItemCount; i++)
                    {
                        var ri = (ResearchItem)Common.ReadStruct(br, typeof(ResearchItem));
                        file.ResearchItems.Add(ri);
                    }

                    // Unused research items
                    for (int i = 0; i < 34; i++)
                    {
                        Common.ReadStruct(br, typeof(ResearchItem));
                    }

                    // Current researchs
                    for (int i = 0; i < 7; i++)
                    {
                        file.CurrentResearchItems.Add(br.ReadInt16());
                    }


                    file.Unknown1 = br.ReadBytes(10);
                    file.Unknown2 = br.ReadInt32();
                    file.ResearchManagementType = (ManagementType)br.ReadInt32();
                    file.Unknown3 = br.ReadInt32();


                    var systemCount = br.ReadInt32();
                    for (int i = 0; i < systemCount; i++)
                    {
                        var ss = (StarSystem)Common.ReadStruct(br, typeof(StarSystem));
                        file.StarSystems.Add(ss);
                    }


                    var planetStructureCount = br.ReadInt32();
                    for (int i = 0; i < planetStructureCount; i++)
                    {
                        var ps = (PlanetStructure)Common.ReadStruct(br, typeof(PlanetStructure));
                        file.PlanetStructures.Add(ps);
                    }


                    // Planets (system info view)
                    var planetCount = br.ReadInt16();
                    for (int i = 0; i < planetCount; i++)
                    {
                        var p = (Planet)Common.ReadStruct(br, typeof(Planet));
                        file.Planets.Add(p);
                    }


                    file.Unknown4 = br.ReadInt16();


                    // Ships
                    var shipCount = br.ReadInt16();
                    for (int i = 0; i < shipCount; i++)
                    {
                        var s = (Ship)Common.ReadStruct(br, typeof(Ship));
                        file.Ships.Add(s);
                    }


                    file.Unknown5 = br.ReadInt16();


                    // Diplmacy
                    var diplomacyCount = br.ReadInt16();
                    for (int i = 0; i < diplomacyCount; i++)
                    {
                        var d = (Diplomacy)Common.ReadStruct(br, typeof(Diplomacy));
                        file.Diplomacies.Add(d);
                    }


                    file.Unknown6 = br.ReadInt16();


                    // Star Lanes
                    var starLaneCount = br.ReadInt32();
                    for (int i = 0; i < starLaneCount; i++)
                    {
                        var sl = (StarLane)Common.ReadStruct(br, typeof(StarLane));
                        file.StarLanes.Add(sl);
                    }


                    // There are 100 blocks of 100 bytes each, however only systemCount of them are used
                    // each block has an entry for each system
                    // the value of that entry is the minimum number of starlane jumps from this system to that
                    // the intersection will always therefore be 0 (starlane jumps from this system, to this system, is 0)
                    // e.g. 0 4 5 2 indicates 
                    // from system 0 to system 0 is 0 jumps 
                    // from system 0 to system 1 is 4 jumps 
                    // from system 0 to system 2 is 5 jumps 
                    // from system 0 to system 3 is 2 jumps 
                    for (int i = 0; i < systemCount; i++)
                    {
                        var slc = (StarLaneCount)Common.ReadStruct(br, typeof(StarLaneCount));
                        file.StarLaneCounts.Add(slc);
                    }

                    // Read the unused starlane count records
                    for (int i = 0; i < 100 - systemCount; i++)
                    {
                        Common.ReadStruct(br, typeof(StarLaneCount));
                    }


                    // 7672 unknown bytes
                    file.Unknown7 = br.ReadBytes(7672);
                }
                return file;
            }
        }
    }
}