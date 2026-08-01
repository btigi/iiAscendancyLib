using System.Runtime.InteropServices;

namespace ii.AscendancyLib.Model.Sav
{
	// 12 bytes
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct Vec3
	{
		public float X;
		public float Y;
		public float Z;
	}
}