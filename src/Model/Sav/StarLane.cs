using ii.AscendancyLib.Model.Sav.Enum;
using System.Runtime.InteropServices;

namespace ii.AscendancyLib.Model.Sav
{
    // 39 bytes
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class StarLane
    {
        public int SourceSystem;
        public int DestinationSystem;
        public int Source_X;
        public int Source_Y;
        public int Source_Z;
        public int Destination_X;
        public int Destination_Y;
        public int Destination_Z;
        public byte KnownBy; // Bitmask of which players know of this starlane
        public short ControlledByPlayer; // 0 - no, 1 - yes
        public StarlaneType StarlaneType;
    }
}