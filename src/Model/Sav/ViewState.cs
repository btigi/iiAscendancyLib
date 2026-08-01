using System.Runtime.InteropServices;

namespace ii.AscendancyLib.Model.Sav
{
	// 152 bytes
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ViewState
	{
		public float A0;
		public float A1;
		public float A2;
		public float A3;
		public float B0;
		public float B1;
		public float B2;
		public float B3;
		public float Flag; // usually 1
		public float Angle; // degrees
		public float Distance;
		public float Scale; // usually 1
		public float FarOrCosmosScale; // usually 100000
		public float Unknown_52;
		public float PanX;
		public float PanY;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public float[] Matrix;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
		public float[] Tail;
	}
}