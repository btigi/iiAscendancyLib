using ii.AscendancyLib.Binary;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ii.AscendancyLib.Converters
{
    public class VocConverter
    {
        private const string WAV_CHUNK_WAVFILE = "RIFF";
        private const string WAV_CHUNK_FORMAT = "fmt ";
        private const string WAV_CHUNK_FACT = "fact";
        private const string WAV_CHUNK_DATA = "data";
        private const string WAV_FILE_BODY_CONSTANT = "WAVE";
        private readonly string VOC_IDENTIFIER = "Creative Voice File" + (char)0x1a; // Can't declare this const because it's somehow non-deterministic...

        public void ConvertVoc(string sourceFile, string destFile, bool fallbackToRaw)
        {
            var blockType = VocBlockType.Silence; // Initialize to anything other than terminator

            using var fs = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);
            using var s = new MemoryStream();
            using var bw = new BinaryWriter(s);
            {
                var header = (VocHeader)Common.ReadStruct(br, typeof(VocHeader));

                // If we do this as a direct comparison it always fails, so we take the long way round
                var identifier = VOC_IDENTIFIER.ToCharArray();
                for (var i = 0; i < VOC_IDENTIFIER.Length; i++)
                {
                    if (header.Identifier[i] != identifier[i])
                    {
                        // Many files have the .voc extension but are actually just .raw files
                        if (fallbackToRaw)
                        {
                            var rawConverter = new RawConverter();
                            rawConverter.ConvertRaw(sourceFile, destFile);
                        }
                        return;
                    }
                }

                var SampleNumber = 0;
                var SampleRate = (byte)0;
                var wavChunkHeader = new WavChunkHeader();
                var wavChunkWavFileBody = new WavChunkWavFileBody();
                var wavChunkFactBody = new WavChunkFactBody();
                var wavChunkFormatBody = GetWavChunkFormat();
                var vocSampleFormatSizer = new VocSampleFormat();
                var vocSampleFormat2Sizer = new VocSampleFormat2();
                var wavChunkHeaderSize = Marshal.SizeOf(wavChunkHeader);
                var WavChunkWavFileBodySize = Marshal.SizeOf(wavChunkWavFileBody);
                var WavChunkFactBodySize = Marshal.SizeOf(wavChunkFactBody);
                var WavChunkFormatBodySize = Marshal.SizeOf(wavChunkFormatBody);
                var VocSampleFormatSize = Marshal.SizeOf(vocSampleFormatSizer);
                var VocSampleFormat2Size = Marshal.SizeOf(vocSampleFormat2Sizer);

                var fpath = Path.GetDirectoryName(destFile);
                var fname = Path.GetFileNameWithoutExtension(destFile);
                var fext = Path.GetExtension(destFile);

                br.BaseStream.Seek(header.FirstDataBlock, SeekOrigin.Begin);
                do
                {
                    blockType = (VocBlockType)Common.ReadStruct(br, typeof(byte));
                    if (blockType != VocBlockType.Terminator)
                    {
                        var a = br.ReadByte();
                        var b = br.ReadByte();
                        var c = br.ReadByte();

                        var BlockSize = (UInt32)((c << 16) + (b << 8) + a);

                        switch (blockType)
                        {
                            case VocBlockType.Text:
                                var text = br.ReadBytes((int)BlockSize);
                                File.WriteAllBytes(String.Format(@"{0}{1}_{2}.txt", String.IsNullOrEmpty(fpath) ? String.Empty : fpath + "\\", fname, SampleNumber), text);
                                SampleNumber++;
                                break;

                            case VocBlockType.NewSampleData:
                                var vocSampleFormat = (VocSampleFormat)Common.ReadStruct(br, typeof(VocSampleFormat));
                                if (vocSampleFormat.DataPackingMethod != VocDataPackingMethod.Unpacked)
                                {
                                    throw new Exception("VOC file uses unhandled data packing method (1)");
                                }

                                SampleRate = vocSampleFormat.SampleRate;
                                SampleRate = (byte)Math.Round(-1000000 / (float)(SampleRate - 256));
                                BlockSize -= (UInt32)VocSampleFormatSize;

                                wavChunkHeader.WavChunkID = WAV_CHUNK_WAVFILE.ToArray();
                                wavChunkHeader.ChunkSize = (UInt32)(WavChunkWavFileBodySize + wavChunkHeaderSize + WavChunkFormatBodySize + wavChunkHeaderSize + WavChunkFactBodySize + wavChunkHeaderSize + BlockSize);
                                var xWavChunkHeaderAsBytes = Common.WriteStruct(wavChunkHeader);
                                bw.Write(xWavChunkHeaderAsBytes);

                                wavChunkWavFileBody.ConstantWave = WAV_FILE_BODY_CONSTANT.ToArray();
                                var wavChunkWavFileBodyAsBytes = Common.WriteStruct(wavChunkWavFileBody);
                                bw.Write(wavChunkWavFileBodyAsBytes);

                                wavChunkHeader.WavChunkID = WAV_CHUNK_FORMAT.ToArray();
                                wavChunkHeader.ChunkSize = (UInt32)WavChunkFormatBodySize;
                                var yWavChunkHeaderAsBytes = Common.WriteStruct(wavChunkHeader);
                                bw.Write(yWavChunkHeaderAsBytes);

                                wavChunkFormatBody.SampleRate = SampleRate;
                                wavChunkFormatBody.BytesPerSample = SampleRate;
                                var WavChunkFormatBodyAsBytes = Common.WriteStruct(wavChunkFormatBody);
                                bw.Write(WavChunkFormatBodyAsBytes);

                                wavChunkHeader.WavChunkID = WAV_CHUNK_FACT.ToArray();
                                wavChunkHeader.ChunkSize = (UInt32)WavChunkFactBodySize;
                                var zWavChunkHeaderAsBytes = Common.WriteStruct(wavChunkHeader);
                                bw.Write(zWavChunkHeaderAsBytes);

                                wavChunkFactBody.SizeOfSoundData = BlockSize;
                                var WavChunkFactBodyAsBytes = Common.WriteStruct(wavChunkFactBody);
                                bw.Write(WavChunkFactBodyAsBytes);

                                wavChunkHeader.WavChunkID = WAV_CHUNK_DATA.ToArray();
                                wavChunkHeader.ChunkSize = BlockSize;
                                var aWavChunkHeaderAsBytes = Common.WriteStruct(wavChunkHeader);
                                bw.Write(aWavChunkHeaderAsBytes);

                                fs.CopyTo(bw.BaseStream);

                                using (FileStream outfs = new(String.Format(@"{0}{1}_{2}{3}", String.IsNullOrEmpty(fpath) ? String.Empty : fpath + "\\", fname, SampleNumber, fext), FileMode.Create, FileAccess.Write))
                                {
                                    bw.BaseStream.Position = 0;
                                    bw.BaseStream.CopyTo(outfs);
                                    fs.Flush(flushToDisk: true);
                                }

                                SampleNumber++;
                                break;

                            case VocBlockType.NewSampleData2:
                                var vocSampleFormat2 = (VocSampleFormat2)Common.ReadStruct(br, typeof(VocSampleFormat2));
                                if (vocSampleFormat2.Format != 0)
                                {
                                    throw new Exception("VOC file uses unhandled data packing method (2)");
                                }

                                BlockSize -= (UInt32)VocSampleFormat2Size;

                                wavChunkHeader.WavChunkID = WAV_CHUNK_WAVFILE.ToArray();
                                wavChunkHeader.ChunkSize = (UInt32)(WavChunkWavFileBodySize + wavChunkHeaderSize + WavChunkFormatBodySize + wavChunkHeaderSize + WavChunkFactBodySize + wavChunkHeaderSize + BlockSize);
                                var aaWavChunkHeaderAsBytes = Common.WriteStruct(wavChunkHeader);
                                bw.Write(aaWavChunkHeaderAsBytes);

                                wavChunkWavFileBody.ConstantWave = WAV_FILE_BODY_CONSTANT.ToArray();
                                var bbbwavChunkWavFileBodyAsBytes = Common.WriteStruct(wavChunkWavFileBody);
                                bw.Write(bbbwavChunkWavFileBodyAsBytes);

                                wavChunkHeader.WavChunkID = WAV_CHUNK_FORMAT.ToArray();
                                wavChunkHeader.ChunkSize = (UInt32)WavChunkFormatBodySize;
                                var ccWavChunkHeaderAsBytes = Common.WriteStruct(wavChunkHeader);
                                bw.Write(ccWavChunkHeaderAsBytes);

                                wavChunkFormatBody.NumChannels = vocSampleFormat2.Channels;
                                wavChunkFormatBody.SampleRate = vocSampleFormat2.SamplesPerSecond;
                                wavChunkFormatBody.BytesPerSecond = vocSampleFormat2.SamplesPerSecond;
                                wavChunkFormatBody.BitsPerSample = vocSampleFormat2.BitsPerSample;
                                var ddWavChunkFormatBodyAsBytes = Common.WriteStruct(wavChunkFormatBody);
                                bw.Write(ddWavChunkFormatBodyAsBytes);

                                wavChunkHeader.WavChunkID = WAV_CHUNK_FACT.ToArray();
                                wavChunkHeader.ChunkSize = (UInt32)WavChunkFactBodySize;
                                var eWavChunkHeaderAsBytes = Common.WriteStruct(wavChunkHeader);
                                bw.Write(eWavChunkHeaderAsBytes);

                                wavChunkFactBody.SizeOfSoundData = BlockSize;
                                var fWavChunkFactBodyAsBytes = Common.WriteStruct(wavChunkFactBody);
                                bw.Write(fWavChunkFactBodyAsBytes);

                                wavChunkHeader.WavChunkID = WAV_CHUNK_DATA.ToArray();
                                wavChunkHeader.ChunkSize = BlockSize;
                                var gWavChunkHeaderAsBytes = Common.WriteStruct(wavChunkHeader);
                                bw.Write(gWavChunkHeaderAsBytes);

                                fs.CopyTo(bw.BaseStream);

                                using (var outfs = new FileStream(String.Format(@"{0}{1}_{2}{3}", String.IsNullOrEmpty(fpath) ? String.Empty : fpath + "\\", fname, SampleNumber, fext), FileMode.Create, FileAccess.Write))
                                {
                                    bw.BaseStream.Position = 0;
                                    bw.BaseStream.CopyTo(outfs);
                                    fs.Flush(flushToDisk: true);
                                }

                                SampleNumber++;
                                break;
                            default:
                                throw new Exception("Unhandled block type");
                        }
                    }
                } while (blockType != VocBlockType.Terminator);
            }
        }

        private WavChunkFormatBody GetWavChunkFormat()
        {
            var format = new WavChunkFormatBody
            {
                Codec = 1,
                NumChannels = 1,
                SampleRate = 22050,
                BytesPerSecond = 22050,
                BytesPerSample = 1,
                BitsPerSample = 8,
                Constant0 = 0
            };
            return format;
        }

        public void Write(string filename, byte[] wavFile)
        {
            ArgumentNullException.ThrowIfNull(filename);
            ArgumentNullException.ThrowIfNull(wavFile);

            ReadAndValidateWav(wavFile, out var sampleRate, out var numChannels, out var bitsPerSample, out var pcm);

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
            if (value > 0xFFFFFF)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            bw.Write((byte)(value & 0xFF));
            bw.Write((byte)((value >> 8) & 0xFF));
            bw.Write((byte)((value >> 16) & 0xFF));
        }

        private static void ReadAndValidateWav(byte[] wavFile, out uint sampleRate, out ushort numChannels, out ushort bitsPerSample, out byte[] pcmData)
        {
            using var ms = new MemoryStream(wavFile, writable: false);
            using var br = new BinaryReader(ms);

            if (wavFile.Length < 12)
            {
                throw new InvalidDataException("WAV buffer is too small for a RIFF header.");
            }

            var riff = Encoding.ASCII.GetString(br.ReadBytes(4));
            _ = br.ReadUInt32();
            var wave = Encoding.ASCII.GetString(br.ReadBytes(4));
            if (riff != "RIFF" || wave != "WAVE")
            {
                throw new InvalidDataException("Buffer is not a RIFF/WAVE file.");
            }

            uint? rate = null;
            ushort? channels = null;
            ushort? bps = null;
            ushort? align = null;
            byte[] pcm = null;

            while (ms.Position <= ms.Length - 8)
            {
                var chunkId = Encoding.ASCII.GetString(br.ReadBytes(4));
                var chunkSize = br.ReadUInt32();
                var dataStart = ms.Position;
                var afterChunk = dataStart + chunkSize;

                if (afterChunk > ms.Length)
                {
                    throw new InvalidDataException("WAV chunk extends past end of file.");
                }

                if (chunkId == "fmt ")
                {
                    if (chunkSize < 16)
                    {
                        throw new InvalidDataException("fmt chunk is too small.");
                    }

                    var audioFormat = br.ReadUInt16();
                    var ch = br.ReadUInt16();
                    var sr = br.ReadUInt32();
                    var brate = br.ReadUInt32();
                    var blockAlign = br.ReadUInt16();
                    var bits = br.ReadUInt16();

                    if (audioFormat != 1)
                    {
                        throw new InvalidDataException($"WAV must be integer PCM (format tag 1); got {audioFormat}.");
                    }

                    if (ch < 1 || ch > 2)
                    {
                        throw new InvalidDataException($"VOC output supports 1 or 2 channels; WAV has {ch}.");
                    }

                    if (bits != 8 && bits != 16)
                    {
                        throw new InvalidDataException($"VOC output supports 8 or 16 bits per sample; WAV has {bits}.");
                    }

                    var expectedAlign = (ushort)(ch * bits / 8);
                    if (blockAlign != expectedAlign)
                    {
                        throw new InvalidDataException($"WAV block align is {blockAlign}; expected {expectedAlign} for {ch} channel(s) at {bits} bits.");
                    }

                    if (brate != sr * blockAlign)
                    {
                        throw new InvalidDataException($"WAV byte rate is inconsistent (got {brate}, expected {sr * blockAlign}).");
                    }

                    rate = sr;
                    channels = ch;
                    bps = bits;
                    align = blockAlign;

                    if (chunkSize > 16)
                    {
                        br.BaseStream.Seek(chunkSize - 16, SeekOrigin.Current);
                    }
                }
                else if (chunkId == "data")
                {
                    if (pcm == null)
                    {
                        pcm = br.ReadBytes(checked((int)chunkSize));
                    }
                    else
                    {
                        br.BaseStream.Seek(chunkSize, SeekOrigin.Current);
                    }
                }
                else
                {
                    br.BaseStream.Seek(chunkSize, SeekOrigin.Current);
                }

                ms.Position = afterChunk;
                if ((chunkSize & 1) != 0 && ms.Position < ms.Length)
                {
                    ms.Position++;
                }
            }

            if (rate == null || channels == null || bps == null || pcm == null)
            {
                throw new InvalidDataException("WAV is missing a valid fmt chunk, data chunk, or both.");
            }

            if (pcm.Length % align!.Value != 0)
            {
                throw new InvalidDataException($"WAV data size ({pcm.Length}) is not a multiple of block align ({align}).");
            }

            sampleRate = rate.Value;
            numChannels = channels.Value;
            bitsPerSample = bps.Value;
            pcmData = pcm;
        }
    }
}