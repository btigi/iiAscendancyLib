using System;
using System.Runtime.InteropServices;

namespace ii.AscendancyLib.Binary
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct WavChunkHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
        public char[] WavChunkID;
        public UInt32 ChunkSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct WavChunkWavFileBody
    {
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
        public char[] ConstantWave;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct WavChunkFormatBody
    {
        public Int16 Codec; // Usu. 1
        public Int16 NumChannels;
        public UInt32 SampleRate;
        public UInt32 BytesPerSecond;
        public Int16 BytesPerSample;
        public Int16 BitsPerSample;
        public Int16 Constant0;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct WavChunkFactBody
    {
        public UInt32 SizeOfSoundData;
    }
}