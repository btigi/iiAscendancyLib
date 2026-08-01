using ii.AscendancyLib.Model.Sav.Enum;
using System.Runtime.InteropServices;

namespace ii.AscendancyLib.Model.Sav
{
    // 494 bytes
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Diplomacy
    {
        public short Unknown_0; // always 0x0000
        public byte RaceIndex;
        public byte Portrait;
        public byte Colour;
        public short Extinct; // 0 = no, -1 = yes
        public int HomeSystemRef; // stored as id << 16? read as id & 0xFFFF if low word nonzero
        public short Unknown_11; // always 0x0000
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4, SizeConst = 100)]
        public int[] ColonizedSystemRefs;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 3)]
        public byte[] Unknown_413;
        public int SpecialAbilityDiscount; // Subtract this from the special ability usage rate to get the current days until the ability can be used
        public int Unknown_420;
        public int Unknown_424;
        public short Unknown_428; // always 18
        public short Unknown_430; // always 22
        public short Unknown_432;
        public short ShipPatience;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 7)]
        public short[] RelationshipToPlayer1Trackers; // numeric representation of relationship, starts at start_attitude
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 7)]
        public RelationshipState[] RelationshipToPlayerStates; // 0 Unknown, 1 Peace, 2 War, 3 Alliance?
        public short NEG_PLAYER_TURNS;
        public short DECLARE_WAR;
        public short ACCEPT_PEACE;
        public short PROPOSE_PEACE;
        public short BREAK_ALLIANCE;
        public short ACCEPT_ALLIANCE;
        public short PROPOSE_ALLIANCE;
        public short START_ATTITUDE;
        public short BIO_PERIOD;
        public short BIO_MAXPOSITIVE;
        public short BIO_MAXNEGATIVE;
        public float Unknown_479;
        public float Unknown_483;
        public float Unknown_487;
        public short Unknown_491; // always 14
        public byte Unknown_493; // always 0
    }
}