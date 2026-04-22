using System;
using System.IO;
using System.Runtime.InteropServices;
using ii.AscendancyLib.Binary;

namespace ii.AscendancyLib.Writer
{
    public class VocWriter
    {
        private readonly string VOC_IDENTIFIER = "Creative Voice File" + (char)0x1a;

        public void Write(string filename, byte[] wavFile)
        {
            ArgumentNullException.ThrowIfNull(filename);
            ArgumentNullException.ThrowIfNull(wavFile);

            WavPcm.ReadValidatedPcm(wavFile, out var sampleRate, out var numChannels, out var bitsPerSample, out var pcm);

            var format2Size = Marshal.SizeOf<VocSampleFormat2>();
            var blockPayloadSize = (uint)(format2Size + pcm.Length);
            if (blockPayloadSize > 0xFFFFFF)
            {
                throw new InvalidDataException("WAV PCM data is too large for a 24-bit VOC block size.");
            }

            using var fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            using var bw = new BinaryWriter(fs);

            bw.Write(BuildVocHeader());

            bw.Write((byte)VocBlockType.NewSampleData2);
            WriteUInt24LE(bw, blockPayloadSize);

            var vocFormat2 = new VocSampleFormat2
            {
                SamplesPerSecond = sampleRate,
                BitsPerSample = (byte)bitsPerSample,
                Channels = (byte)numChannels,
                Format = 0,
                Reserved = 0
            };
            bw.Write(Common.WriteStruct(vocFormat2));
            bw.Write(pcm);
            bw.Write((byte)VocBlockType.Terminator);
        }

        private byte[] BuildVocHeader()
        {
            var header = new VocHeader
            {
                Identifier = VOC_IDENTIFIER.ToCharArray(),
                FirstDataBlock = (short)Marshal.SizeOf<VocHeader>(),
                MajorVersion = 1,
                MinorVersion = 20,
                Checksum = 0
            };

            var bytes = Common.WriteStruct(header);
            ushort sum = 0;
            for (var i = 0; i < 24; i++)
            {
                sum += bytes[i];
            }

            var checksum = (ushort)~sum;
            bytes[24] = (byte)(checksum & 0xFF);
            bytes[25] = (byte)(checksum >> 8);
            return bytes;
        }

        private static void WriteUInt24LE(BinaryWriter bw, uint value)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan<uint>(value, 0xFFFFFF);

            bw.Write((byte)(value & 0xFF));
            bw.Write((byte)((value >> 8) & 0xFF));
            bw.Write((byte)((value >> 16) & 0xFF));
        }
    }
}
