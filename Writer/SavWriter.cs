using ii.AscendancyLib.Model;
using ii.AscendancyLib.Model.Enum;
using System.IO;

namespace ii.AscendancyLib.Writer
{
    public class SavWriter
    {
        public void Save(string filename, SavFile file)
        {
            using (FileStream fsout = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter brout = new BinaryWriter(fsout))
                {
                    brout.Write(Common.WriteStruct(file.Header));

                    brout.Write((short)file.ResearchItems.Count);
                    for (int i = 0; i < file.ResearchItems.Count; i++)
                    {
                        brout.Write(Common.WriteStruct(file.ResearchItems[i]));
                    }

                    for (int i = 0; i < 34; i++)
                    {
                        var t = new ResearchItem();
                        t.RequiredBy = new ResearchableItem[5];
                        t.RequiredBy[0] = ResearchableItem.End;
                        brout.Write(Common.WriteStruct(t));
                    }

                    for (int i = 0; i < 7; i++)
                    {
                        brout.Write(file.CurrentResearchItems[i]);
                    }

                    brout.Write(file.Unknown1);
                    brout.Write(file.Unknown2);
                    brout.Write((int)file.ResearchManagementType);
                    brout.Write(file.Unknown3);

                    brout.Write(file.StarSystems.Count);
                    for (int i = 0; i < file.StarSystems.Count; i++)
                    {
                        brout.Write(Common.WriteStruct(file.StarSystems[i]));
                    }

                    brout.Write(file.PlanetStructures.Count);
                    for (int i = 0; i < file.PlanetStructures.Count; i++)
                    {
                        brout.Write(Common.WriteStruct(file.PlanetStructures[i]));
                    }

                    brout.Write((short)file.Planets.Count);
                    for (int i = 0; i < file.Planets.Count; i++)
                    {
                        brout.Write(Common.WriteStruct(file.Planets[i]));
                    }

                    brout.Write(file.Unknown4);

                    brout.Write((short)file.Ships.Count);
                    for (int i = 0; i < file.Ships.Count; i++)
                    {
                        brout.Write(Common.WriteStruct(file.Ships[i]));
                    }

                    brout.Write(file.Unknown5);

                    brout.Write((short)file.Diplomacies.Count);
                    for (int i = 0; i < file.Diplomacies.Count; i++)
                    {
                        brout.Write(Common.WriteStruct(file.Diplomacies[i]));
                    }

                    brout.Write(file.Unknown6);

                    brout.Write(file.StarLanes.Count);
                    for (int i = 0; i < file.StarLanes.Count; i++)
                    {
                        brout.Write(Common.WriteStruct(file.StarLanes[i]));
                    }

                    for (int i = 0; i < file.StarSystems.Count; i++)
                    {
                        brout.Write(Common.WriteStruct(file.StarLaneCounts[i]));
                    }

                    var t2 = new byte[100];
                    for (int i = 0; i < 100; i++)
                    {
                        t2[i] = 0xff;
                    }
                    for (int i = 0; i < 100 - file.StarSystems.Count; i++)
                    {
                        //TODO: Blank starlanecount structs should all be 0xff
                        //brout.Write(WriteStruct(new StarLaneCount()));
                        brout.Write(t2);
                    }

                    brout.Write(file.Unknown7);
                }
            }
        }
    }
}