using System;
using System.Collections.Generic;
using System.IO;
using ii.AscendancyLib.Binary;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ImageInfo = ii.AscendancyLib.Binary.ImageInfo;

namespace ii.AscendancyLib.Writer
{
    public class ShpWriter
    {
        public void Write(List<string> filenames, string output)
        {
            var imageInfos = new List<ImageInfo>();
            var imageContentBytes = new List<byte>();

            const int ShpHeaderSize = 8;
            const int ImageInfoSize = 8;
            var introSize = ShpHeaderSize + filenames.Count * ImageInfoSize;

            foreach (var file in filenames)
            {
                using var image = Image.Load<Rgba32>(file);

                imageInfos.Add(new ImageInfo
                {
                    ImageOffset = introSize + imageContentBytes.Count,
                    PaletteOffset = 0,
                });

                var imageHeader = new ImageHeader
                {
                    Width = (short)image.Width,
                    Height = (short)(image.Height - 1),
                    XCentre = 0,
                    YCentre = 0,
                    xStart = 0,
                    yStart = 0,
                    xEnd = image.Width - 1,
                    yEnd = image.Height - 1,
                };

                imageContentBytes.AddRange(Common.WriteStruct(imageHeader));

                var rowIndices = new byte[image.Width];
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        rowIndices[x] = FindPaletteIndex(image[x, y]);
                    }

                    EncodeRow(rowIndices, imageContentBytes);
                }
            }

            var introBytes = new List<byte>();

            var header = new ShpHeader
            {
                Version = "1.10".ToCharArray(),
                ImageCount = filenames.Count,
            };
            introBytes.AddRange(Common.WriteStruct(header));

            foreach (var info in imageInfos)
            {
                introBytes.AddRange(Common.WriteStruct(info));
            }

            using var fs = new FileStream(output, FileMode.Create);
            using var bw = new BinaryWriter(fs);
            bw.BaseStream.Write([.. introBytes], 0, introBytes.Count);
            bw.BaseStream.Write([.. imageContentBytes], 0, imageContentBytes.Count);
        }

        private static byte FindPaletteIndex(Rgba32 pixel)
        {
            var r = pixel.R / 4;
            var g = pixel.G / 4;
            var b = pixel.B / 4;
            var hex = $"{r:X2}{g:X2}{b:X2}";
            var idx = DefaultPalette.defaultPalette.IndexOf(hex);
            return (byte)(idx < 0 ? 0 : idx);
        }

        private static void EncodeRow(byte[] indices, List<byte> output)
        {
            int i = 0;
            while (i < indices.Length)
            {
                var colour = indices[i];

                if (colour == 0)
                {
                    int j = i;
                    while (j < indices.Length && indices[j] == 0)
                    {
                        j++;
                    }
                    if (j == indices.Length)
                    {
                        break;
                    }

                    int runLength = j - i;
                    while (runLength > 0)
                    {
                        int chunk = Math.Min(runLength, 255);
                        output.Add(1);
                        output.Add((byte)chunk);
                        runLength -= chunk;
                    }
                    i = j;
                }
                else if (i + 1 < indices.Length && indices[i + 1] == colour)
                {
                    int j = i + 1;
                    while (j < indices.Length && indices[j] == colour)
                    {
                        j++;
                    }

                    int runLength = j - i;
                    while (runLength > 0)
                    {
                        int chunk = Math.Min(runLength, 127);
                        output.Add((byte)(chunk << 1));
                        output.Add(colour);
                        runLength -= chunk;
                    }
                    i = j;
                }
                else
                {
                    int j = i;
                    while (j < indices.Length && indices[j] != 0)
                    {
                        if (j > i && j + 1 < indices.Length && indices[j + 1] == indices[j])
                        {
                            break;
                        }
                        j++;
                        if (j - i == 127)
                        {
                            break;
                        }
                    }

                    int litLength = j - i;
                    output.Add((byte)((litLength << 1) | 1));
                    for (int k = i; k < j; k++)
                    {
                        output.Add(indices[k]);
                    }
                    i = j;
                }
            }

            output.Add(0);
        }
    }
}
