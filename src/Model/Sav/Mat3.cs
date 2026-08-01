using System.Runtime.InteropServices;

namespace ii.AscendancyLib.Model.Sav
{
	// 36 bytes — row-major 3×3 float matrix
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Mat3
	{
		public float M00;
		public float M01;
		public float M02;
		public float M10;
		public float M11;
		public float M12;
		public float M20;
		public float M21;
		public float M22;
	}
}
