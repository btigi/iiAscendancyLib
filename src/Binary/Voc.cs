using System;
using System.Runtime.InteropServices;

namespace ii.AscendancyLib.Binary
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VocHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 20)]
        public char[] Identifier;
        public Int16 FirstDataBlock;
        public byte MajorVersion;
        public byte MinorVersion;
        public Int16 Checksum;
    }

    public enum VocBlockType : byte
    {
        Terminator,
        NewSampleData,
        SampleData,
        Silence,
        Marker,
        Text,
        StartRepetition,
        EndRepetition,
        AdditionalInfo,
        NewSampleData2
    }

    public enum VocDataPackingMethod
    {
        Unpacked,
        Packed4Bits,
        Packed2p6Bits,
        Packed2Bits
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VocSampleFormat
    {
        public byte SampleRate;
        public VocDataPackingMethod DataPackingMethod;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VocSampleFormat2
    {
        public UInt32 SamplesPerSecond;
        public byte BitsPerSample;
        public byte Channels;
        public Int16 Format;
        public UInt32 Reserved;
    }
}