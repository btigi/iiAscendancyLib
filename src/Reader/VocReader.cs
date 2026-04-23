using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ii.AscendancyLib.Binary;

namespace ii.AscendancyLib.Reader
{
    public class VocReader
    {
        private const string WAV_CHUNK_WAVFILE = "RIFF";
        private const string WAV_CHUNK_FORMAT = "fmt ";
        private const string WAV_CHUNK_FACT = "fact";
        private const string WAV_CHUNK_DATA = "data";
        private const string WAV_FILE_BODY_CONSTANT = "WAVE";
        private readonly string VOC_IDENTIFIER = "Creative Voice File" + (char)0x1a;

        public List<(byte[], string)> Read(string sourceFile, bool fallbackToRaw)
        {
            var result = new List<(byte[], string)>();

            var blockType = VocBlockType.Silence;

            using var fs = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);
            using var s = new MemoryStream();
            using var bw = new BinaryWriter(s);
            {
                var header = (VocHeader)Common.ReadStruct(br, typeof(VocHeader));

                var identifier = VOC_IDENTIFIER.ToCharArray();
                for (var i = 0; i < VOC_IDENTIFIER.Length; i++)
                {
                    if (header.Identifier[i] != identifier[i])
                    {
                        if (fallbackToRaw)
                        {
                            result.Add((new RawReader().Read(sourceFile), string.Empty));
                            return result;
						}
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
                                result.Add(([], Encoding.ASCII.GetString(text)));
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

                                using (var ms = new MemoryStream())
                                {
                                    bw.BaseStream.Position = 0;
                                    bw.BaseStream.CopyTo(ms);
                                    result.Add((ms.ToArray(), string.Empty));
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

                                using (var ms = new MemoryStream())
                                {
                                    bw.BaseStream.Position = 0;
                                    bw.BaseStream.CopyTo(ms);
                                    result.Add((ms.ToArray(), string.Empty));
                                }

                                SampleNumber++;
                                break;
                            default:
                                throw new Exception("Unhandled block type");
                        }
                    }
                } while (blockType != VocBlockType.Terminator);
            }

            return result;
		}

        private static WavChunkFormatBody GetWavChunkFormat()
        {
            return new WavChunkFormatBody
            {
                Codec = 1,
                NumChannels = 1,
                SampleRate = 22050,
                BytesPerSecond = 22050,
                BytesPerSample = 1,
                BitsPerSample = 8,
                Constant0 = 0
            };
        }
    }
}