using ii.AscendancyLib.Files;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace ii.AscendancyLib.Converters
{
    public class FntConverter
    {
        private const int Signature = 0x00002e31;
        private const int PaletteSize = 256;

        public FntFile ConvertFnt(string sourceFile, string palFile)
        {
            var fntFile = new FntFile();
            var palette = ReadPalette(palFile);

            using var fs = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);

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
                fs.Seek(offChar, SeekOrigin.Begin);

                var width = br.ReadInt32();
                if (width == 0)
                {
                    continue;
                }

                var pixels = new byte[characterHeight * width * 4];
                for (int y = 0; y < characterHeight; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Array.Copy(palette[br.ReadByte()], 0, pixels, (y * width + x) * 4, 4);
                    }
                }

                var bitmap = new Bitmap(width, characterHeight, PixelFormat.Format32bppArgb);
                var bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
                var ptr = bmpData.Scan0;
                Marshal.Copy(pixels, 0, ptr, pixels.Length);
                bitmap.UnlockBits(bmpData);
                fntFile.Images.Add(bitmap);
            }

            return fntFile;
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