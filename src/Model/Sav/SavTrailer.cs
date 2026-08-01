using ii.AscendancyLib.Model.Sav.Enum;
using System.Runtime.InteropServices;

namespace ii.AscendancyLib.Model.Sav
{
    // 7672 bytes
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SavTrailer
    {
        public int Flag;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
        public int[] SystemOrder;
        public short SystemCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] Unknown_406;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 125)]
        public byte[] Unknown_414;
        public byte GalaxyDisplayFlags;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] Unknown_540;
        public GalaxyDisplayToggle ShowStarlanes;
        public GalaxyDisplayToggle ShowKnownStarlanes;
        public GalaxyDisplayToggle ShowShips;
        public GalaxyDisplayToggle ShowPlanets;
        public int Unknown_559;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Unknown_563;
        public byte HomeworldsHiddenMask; // Bit N set = homeworld markers hidden for species N
        public byte Unknown_568;
        public ViewState GalaxyView; // Galaxy ViewState - current config
        public int GalaxyAngleInt; // matches GalaxyView.Angle?
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Unknown_725;
        // Copies of the galaxy screen config from earlier in this struct
        public GalaxyDisplayToggle ShowStarlanes_2;
        public GalaxyDisplayToggle ShowKnownStarlanes_2;
        public GalaxyDisplayToggle ShowShips_2;
        public GalaxyDisplayToggle ShowPlanets_2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] Unknown_745;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public HomeworldDisplay[] HomeworldDisplays;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1198)]
        public byte[] Unknown_849;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
        public Mat3[] SystemOrientations;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 400)]
        public byte[] Unknown_5647;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
        public Vec3[] SystemCentres;
        public Vec3 CurrentCentre; // Copy of SystemCentres[LastStarSystemDisplayed]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 261)]
        public byte[] Unknown_7259; // more camera config?
        public ViewState SystemView; // Galaxy ViewState - used for 'default view' button?
    }
}