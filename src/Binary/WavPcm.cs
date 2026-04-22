using System;
using System.IO;
using System.Text;

namespace ii.AscendancyLib.Binary
{
    public static class WavPcm
    {
        public static void ReadValidatedPcm(byte[] wavFile, out uint sampleRate, out ushort numChannels, out ushort bitsPerSample, out byte[] pcmData)
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
                        throw new InvalidDataException($"Only 1 or 2 channels are supported; WAV has {ch}.");
                    }

                    if (bits != 8 && bits != 16)
                    {
                        throw new InvalidDataException($"Only 8 or 16 bits per sample are supported; WAV has {bits}.");
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
