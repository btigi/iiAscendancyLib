using ii.AscendancyLib.Model.Sav.Enum;
using System.Runtime.InteropServices;

namespace ii.AscendancyLib.Model.Sav
{
	// 14 bytes
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct HomeworldDisplay
	{
		public ushort Unknown_0; // usually 0x01C1
		public ushort Unknown_2; // usually 0x01D1
		public HomeworldDisplayMode Mode; // 1 = show homeworlds, 5 = hide
		public ushort Unknown_6;
		public ushort SpeciesBit; // 2, 4, 8, 16, 32, 64
		public ushort Unknown_10;
		public ushort Unknown_12;
	}
}