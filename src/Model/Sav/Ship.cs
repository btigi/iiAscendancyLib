using ii.AscendancyLib.Model.Sav.Enum;
using System.Runtime.InteropServices;

namespace ii.AscendancyLib.Model.Sav
{
    // 358 bytes
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Ship
    {
        public short Unknown_0;
        public int ShipId;
        public int AttackStrength; // ship display screen, 1 = 0.25 blocks
        public int ShieldStrength; // ship display screen, 1 = 0.25 blocks
        public int StarlaneTravelMultiplierBlue; // i.e. 1 = default time, 2 = twice as quick, 3 = three times as quick. Game adds Blue and Red to determine the multiplier
        public int StarlaneTravelMultiplierRed; // i.e. 1 = default time, 2 = twice as quick, 3 = three times as quick. Game adds Blue and Red to determine the multiplier
        public int EngineStrength; // ship display screen, 1 = 0.25 blocks
        public int Unknown_26;
        public int PowerStrength; // ship display screen, 1 = 0.25 blocks
        public int Unknown_34;
        public int ScannerStrength; // ship display screen, 1 = 0.25 blocks
        public int Unknown_42;
        public int Unknown_46;
        public int Unknown_50;
        public int Unknown_54;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 30)]
        public char[] Name;
        public int Unknown_88; // Day ship construction commenced ?
        public short OwnerRaceIndex;
        public byte U01;
        public byte U02;
        public byte U03;
        public byte U04;
        public byte U05;
        public byte Order; // 0 - under construction, 1 - move to starlane, 7 - move to position (in system)
        public byte U07;
        public byte U08;
        public byte U09;
        public byte U10;
        public byte U11;
        public byte U12;
        public byte U13;
        public byte U14;
        public byte U15;
        public byte U16;
        public byte U17;
        public byte U18;
        public byte U19;
        public byte U20;
        public float DestinationX;
        public float DestinationY;
        public float DestinationZ;
        public byte U33;
        public byte U34;
        public byte U35;
        public byte U36;
        public byte U37;
        public byte U38;
        public byte U39;
        public byte U40;
        public byte U41;
        public byte U42;
        public byte U43;
        public byte U44;
        public byte U45;
        public byte U46;
        public byte U47;
        public byte U48;
        public int Power;
        public int Integrity;
        public int Unknown1;
        public int Moves;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 7)]
        public byte[] Unknown10;
        public float XAxis;
        public float YAxis;
        public float ZAxis;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 25)]
        public Gizmo[] Gizmos;
        public int SlotCount;
        public short SlotCountUsed;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Gizmo
    {
        public GizmoType Index;
        public byte Depleted; // charges remaining
        public byte Unknown_2;
        public byte Unknown_3;
        public byte Unknown_4;
        public byte Unknown_5;
        public byte Unknown_6;
    }
}