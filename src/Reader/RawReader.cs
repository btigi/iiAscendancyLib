using ii.AscendancyLib.Binary;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ii.AscendancyLib.Reader
{
    public class RawReader
    {
        private const string WAV_CHUNK_WAVFILE = "RIFF";
        private const string WAV_CHUNK_FORMAT = "fmt ";
        private const string WAV_CHUNK_FACT = "fact";
        private const string WAV_CHUNK_DATA = "data";
        private const string WAV_FILE_BODY_CONSTANT = "WAVE";

        public byte[] Read(string sourceFile)
        {
            using var fs = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
            return Read(fs);
        }

        public byte[] Read(byte[] data)
        {
            using var ms = new MemoryStream(data, writable: false);
            return Read(ms);
        }

        public byte[] Read(Stream input)
        {
            ArgumentNullException.ThrowIfNull(input);
            if (!input.CanSeek) throw new ArgumentException("Stream must be seekable.", nameof(input));
            if (input.Position != 0) input.Position = 0;

            using var s = new MemoryStream();
            using var bw = new BinaryWriter(s);

            var wavChunkHeader = new WavChunkHeader();
            var wavChunkWavFileBody = new WavChunkWavFileBody();
            var wavChunkFactBody = new WavChunkFactBody();
            var wavChunkFormatBody = new WavChunkFormatBody
            {
                Codec = 1,
                NumChannels = 1,
                SampleRate = 22050,
                BytesPerSecond = 22050,
                BytesPerSample = 1,
                BitsPerSample = 8,
                Constant0 = 0
            };
            var rawDataSize = (UInt32)input.Length;

            byte[] wavChunkHeaderAsBytes;
            wavChunkHeader.WavChunkID = [.. WAV_CHUNK_WAVFILE];
            wavChunkHeader.ChunkSize = (UInt32)(Marshal.SizeOf(wavChunkWavFileBody) + Marshal.SizeOf(wavChunkHeader) + Marshal.SizeOf(wavChunkFormatBody) + Marshal.SizeOf(wavChunkHeader) + Marshal.SizeOf(wavChunkFactBody) + Marshal.SizeOf(wavChunkHeader) + rawDataSize);
            wavChunkHeaderAsBytes = Common.WriteStruct(wavChunkHeader);
            bw.Write(wavChunkHeaderAsBytes);

            wavChunkWavFileBody.ConstantWave = [.. WAV_FILE_BODY_CONSTANT];
            var wavChunkWavFileBodyAsBytes = Common.WriteStruct(wavChunkWavFileBody);
            bw.Write(wavChunkWavFileBodyAsBytes);

            wavChunkHeader.WavChunkID = [.. WAV_CHUNK_FORMAT];
            wavChunkHeader.ChunkSize = (UInt32)Marshal.SizeOf(wavChunkFormatBody);
            wavChunkHeaderAsBytes = Common.WriteStruct(wavChunkHeader);
            bw.Write(wavChunkHeaderAsBytes);

            var WavChunkFormatBodyAsBytes = Common.WriteStruct(wavChunkFormatBody);
            bw.Write(WavChunkFormatBodyAsBytes);

            wavChunkHeader.WavChunkID = [.. WAV_CHUNK_FACT];
            wavChunkHeader.ChunkSize = (UInt32)Marshal.SizeOf(wavChunkFactBody);
            wavChunkHeaderAsBytes = Common.WriteStruct(wavChunkHeader);
            bw.Write(wavChunkHeaderAsBytes);

            wavChunkFactBody.SizeOfSoundData = rawDataSize;
            var WavChunkFactBodyAsBytes = Common.WriteStruct(wavChunkFactBody);
            bw.Write(WavChunkFactBodyAsBytes);

            wavChunkHeader.WavChunkID = [.. WAV_CHUNK_DATA];
            wavChunkHeader.ChunkSize = rawDataSize;
            wavChunkHeaderAsBytes = Common.WriteStruct(wavChunkHeader);
            bw.Write(wavChunkHeaderAsBytes);

            input.CopyTo(bw.BaseStream);

            using var ms = new MemoryStream();
            bw.BaseStream.Position = 0;
			bw.BaseStream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}