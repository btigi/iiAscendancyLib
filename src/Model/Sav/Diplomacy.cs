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
        public byte Portait;
        public byte Colour;
        public short Extinct; // 0 = no, -1 = yes
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 409)]
        public byte[] Unknown_7;
        public int SpecialAbilityDiscount; // Subtract this from the special ability usage rate to get the current days until the ability can be used
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 14)]
        public byte[] Unknown_418;
        public short ShipPatience;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 7)]
        public short[] RelationshipToPlayer1Trackers; // numeric representation of relationship, starts at start_attitude
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 7)]
        public RelationshipState[] RelationshipToPlayerStates;
        public short NEG_PLAYER_TURNS;
        public short DECLARE_WAR;
        public short ACCEPT_PEACE;
        public short PROPOSE_PEACE;
        public short BREAK_ALLIANCE;
        public short ACCEPT_ALLIANCE;
        public short PROPOSE_ALLIANCE;
        public short START_ATTITUDE;
        public short BIO_PERIOD;
        public short BIO_MAXPOSITVE;
        public short BIO_MAXNEGATIVE;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 15)]
        public byte[] Unknown_480;
    }
}