using ii.AscendancyLib.Files;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;

namespace ii.AscendancyLib.Writer
{
    public class FntWriter
    {
        private const int Signature = 0x00002e31;
        private const int PaletteSize = 256;

        public void Write(string filename, FntFile fntFile, string palFile)
        {
            if (fntFile?.Images is null)
            {
                throw new ArgumentNullException(nameof(fntFile));
            }

            var palette = ReadPalette(palFile);
            int transparentColourIndex = fntFile.TransparentColourIndex;
            if (transparentColourIndex < 0 || transparentColourIndex >= palette.Length)
            {
                throw new InvalidOperationException($"FntFile.TransparentColourIndex ({transparentColourIndex}) is outside the palette range.");
            }

            int characterCount = fntFile.Images.Count;
            int characterHeight = fntFile.CharacterHeight;
            if (characterHeight <= 0)
            {
                throw new InvalidOperationException("FntFile.CharacterHeight must be greater than zero.");
            }

            int headerAndOffsets = 16 + 4 * characterCount;

            var offsets = new int[characterCount];
            int running = headerAndOffsets;
            for (int i = 0; i < characterCount; i++)
            {
                offsets[i] = running;
                var img = fntFile.Images[i];
                if (img is null)
                {
                    running += 4;
                }
                else
                {
                    if (img.Height != characterHeight)
                    {
                        throw new InvalidOperationException($"Character image at index {i} has height {img.Height} but FntFile.CharacterHeight is {characterHeight}.");
                    }

                    running += 4 + img.Width * characterHeight;
                }
            }

            using var fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            using var bw = new BinaryWriter(fs);

            bw.Write(Signature);
            bw.Write(characterCount);
            bw.Write(characterHeight);
            bw.Write(transparentColourIndex);
            for (int i = 0; i < characterCount; i++)
            {
                bw.Write(offsets[i]);
            }

            for (int i = 0; i < characterCount; i++)
            {
                var img = fntFile.Images[i];
                if (img is null)
                {
                    bw.Write(0);
                    continue;
                }

                int width = img.Width;
                bw.Write(width);
                if (width == 0)
                {
                    continue;
                }

                using var rgba = img.CloneAs<Rgba32>();
                for (int y = 0; y < characterHeight; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Rgba32 p = rgba[x, y];
                        if (p.A == 0)
                        {
                            bw.Write((byte)transparentColourIndex);
                        }
                        else
                        {
                            int idx = MapPixelToPaletteIndex(p, palette, transparentColourIndex);
                            bw.Write((byte)idx);
                        }
                    }
                }
            }
        }

        private static int MapPixelToPaletteIndex(Rgba32 pixel, IReadOnlyList<byte[]> palette, int transparentColourIndex)
        {
            for (int i = 0; i < palette.Count; i++)
            {
                if (i == transparentColourIndex)
                {
                    continue;
                }

                var c = palette[i];
                if (c[0] == pixel.B && c[1] == pixel.G && c[2] == pixel.R)
                {
                    return i;
                }
            }

            int best = 0;
            int bestD = int.MaxValue;
            for (int i = 0; i < palette.Count; i++)
            {
                if (i == transparentColourIndex)
                {
                    continue;
                }

                var c = palette[i];
                int d = (c[0] - pixel.B) * (c[0] - pixel.B)
                    + (c[1] - pixel.G) * (c[1] - pixel.G)
                    + (c[2] - pixel.R) * (c[2] - pixel.R);
                if (d < bestD)
                {
                    bestD = d;
                    best = i;
                }
            }

            return best;
        }

        private static byte[][] ReadPalette(string palFile, int size = PaletteSize)
        {
            using var handle = new FileStream(palFile, FileMode.Open, FileAccess.Read);
            var entries = new byte[size][];
            var rgb = new byte[3];
            for (int index = 0; index < size; index++)
            {
                handle.ReadExactly(rgb, 0, 3);
                entries[index] = [(byte)(rgb[0] << 2), (byte)(rgb[1] << 2), (byte)(rgb[2] << 2), (byte)0xFF];
            }
            return entries;
        }
    }
}
