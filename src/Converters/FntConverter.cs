using ii.AscendancyLib.Files;
using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;

namespace ii.AscendancyLib.Converters
{
    public class FntConverter
    {
        private const int Signature = 0x00002e31;
        private const int PaletteSize = 256;

        public FntFile ConvertFnt(Stream sourceStream, string palFile)
        {
            var fntFile = new FntFile();
            var palette = ReadPalette(palFile);

            using var br = new BinaryReader(sourceStream, System.Text.Encoding.Default, leaveOpen: true);

            var signature = br.ReadInt32();
            if (signature != Signature)
            {
                throw new Exception("Invalid FNT file");
            }

            var characterCount = br.ReadInt32();
            var characterHeight = br.ReadInt32();
            var transparentColourIndex = br.ReadInt32();
            palette[transparentColourIndex][3] = 0x00; // Explicitly set the alpha channel to 0

            // Read all offsets first
            var offsets = new List<int>();
            for (var i = 0; i < characterCount; i++)
            {
                offsets.Add(br.ReadInt32());
            }

            // Read the data for each character
            for (var i = 0; i < characterCount; i++)
            {
                var offChar = offsets[i];
                sourceStream.Seek(offChar, SeekOrigin.Begin);

                var width = br.ReadInt32();
                if (width == 0)
                {
                    continue;
                }

                var image = new Image<Rgba32>(width, characterHeight);
                for (int y = 0; y < characterHeight; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var colorIndex = br.ReadByte();
                        if (colorIndex == transparentColourIndex)
                        {
                            image[x, y] = new Rgba32(0, 0, 0, 0);
                        }
                        else
                        {
                            var color = palette[colorIndex];
                            image[x, y] = new Rgba32(color[2], color[1], color[0], color[3]); // Swap R and B to match BGRA order
                        }
                    }
                }

                fntFile.Images.Add(image);
            }

            return fntFile;
        }

        public FntFile ConvertFnt(string sourceFile, string palFile)
        {
            using var fs = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
            return ConvertFnt(fs, palFile);
        }

        private static byte[][] ReadPalette(string palFile, int size = PaletteSize)
        {
            using var handle = new FileStream(palFile, FileMode.Open, FileAccess.Read);
            var entries = new byte[size][];
            var rgb = new byte[3];
            for (int index = 0; index < size; index++)
            {
                handle.Read(rgb, 0, 3);
                entries[index] = [(byte)(rgb[0] << 2), (byte)(rgb[1] << 2), (byte)(rgb[2] << 2), (byte)0xFF];
            }
            return entries;
        }
    }
}