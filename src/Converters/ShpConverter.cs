using ii.AscendancyLib.Binary;
using ii.AscendancyLib.Files;
using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using ImageInfo = ii.AscendancyLib.Binary.ImageInfo;

namespace ii.AscendancyLib.Converters
{
    public class ShpConverter
    {
        private string fname;
        private bool Debug { get; set; }

        public ShpFile ConvertShp(string filename)
        {
            fname = Path.GetFileName(filename);
            using FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var f = ConvertShp(fs);
            return f;
        }

        public ShpFile ConvertShp(Stream s)
        {
            using BinaryReader br = new(s);
            var shpFile = ParseFile(br);
            br.BaseStream.Seek(0, SeekOrigin.Begin);
            return shpFile;
        }

        private ShpFile ParseFile(BinaryReader br)
        {
            var imageInfos = new List<ImageInfo>();
            var shpFile = new ShpFile();

            var header = (ShpHeader)Common.ReadStruct(br, typeof(ShpHeader));
            if (string.Join(String.Empty, header.Version) != "1.10")
            {
                throw new Exception("Invalid SHP file");
            }

            for (var i = 0; i < header.ImageCount; i++)
            {
                var imageInfo = (ImageInfo)Common.ReadStruct(br, typeof(ImageInfo));
                imageInfos.Add(imageInfo);
            }

            for (var i = 0; i < header.ImageCount; i++)
            {
                try
                {
                    // Read the image
                    br.BaseStream.Seek(imageInfos[i].ImageOffset, SeekOrigin.Begin);
                    var imageInfo = (ImageHeader)Common.ReadStruct(br, typeof(ImageHeader));

                    var pixels = new byte[imageInfo.Width + 1, imageInfo.Height + 1];
                    for (int line = 0; line < (imageInfo.yEnd - imageInfo.yStart) + 1; line++)
                    {
                        var lineComplete = false;
                        var lineProgress = 0;
                        while (!lineComplete)
                        {
                            // We'd normally use peekchar, but it throws errors related to the file encoding
                            if (HasData(br.BaseStream))
                            {
                                var infoByte = br.ReadByte();
                                if (infoByte == 0)
                                {
                                    // fill rest of line with default colour
                                    for (int x = lineProgress; x < imageInfo.xEnd - imageInfo.xStart + 1; x++)
                                    {
                                        pixels[lineProgress, line] = 0;
                                        lineProgress++;
                                    }
                                    lineComplete = true;
                                    continue;
                                }

                                if (infoByte == 1)
                                {
                                    // fill length with default colour
                                    if (HasData(br.BaseStream))
                                    {
                                        var length = br.ReadByte();
                                        for (int x = 0; x < length; x++)
                                        {
                                            pixels[lineProgress, line] = 0;
                                            lineProgress++;
                                        }
                                        continue;
                                    }
                                    else
                                    {
                                        Log($"Skipping frame {i} in {fname}");
                                        lineComplete = true;
                                    }
                                }

                                if ((infoByte & Common.Bit0) == 0)
                                {
                                    // fill length with colour
                                    var length = infoByte >> 1;
                                    if (HasData(br.BaseStream))
                                    {
                                        var colour = br.ReadByte();
                                        for (int x = 0; x < length; x++)
                                        {
                                            pixels[lineProgress, line] = colour;
                                            lineProgress++;
                                        }
                                        continue;
                                    }
                                    else
                                    {
                                        Log($"Skipping frame {i} in {fname}");
                                        lineComplete = true;
                                    }
                                }

                                if ((infoByte & Common.Bit0) != 0)
                                {
                                    // read length bytes
                                    var length = infoByte >> 1;
                                    for (int x = 0; x < length; x++)
                                    {
                                        if (HasData(br.BaseStream))
                                        {
                                            var colour = br.ReadByte();
                                            pixels[lineProgress, line] = colour;
                                            lineProgress++;
                                        }
                                        else
                                        {
                                            Log($"Skipping frame {i} in {fname}");
                                            lineComplete = true;
                                        }
                                    }
                                    continue;
                                }
                            }
                            else
                            {
                                Log($"Skipping frame {i} in {fname}");
                                lineComplete = true;
                            }
                        }
                    }

                    // TODO: Palette handling
                    if (imageInfos[i].PaletteOffset != 0)
                    {
                        Log($"SHP {fname} frame {i} uses a custom palette");
                        br.BaseStream.Seek(imageInfos[i].PaletteOffset, SeekOrigin.Begin);
                    }

                    var width = imageInfo.Width;
                    var height = imageInfo.Height + 1;
                    var image = new Image<Rgba32>(width, height);

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            var s = DefaultPalette.defaultPalette[pixels[x, y]];
                            var c = DefaultPalette.GetColour(s);
                            image[x, y] = c;
                        }
                    }

                    shpFile.Images.Add(image);
                }
                catch
                {
                    Log("Exception writing frame {i} file {fname}");
                }
            }
            return shpFile;
        }

        private bool HasData(Stream stream)
        {
            return stream.Length != stream.Position;
        }

        private void Log(string msg)
        {
            if (Debug)
            {
                Console.WriteLine(msg);
            }
        }

        public void FromBitmap(List<string> filenames, string output)
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
            bw.BaseStream.Write(introBytes.ToArray(), 0, introBytes.Count);
            bw.BaseStream.Write(imageContentBytes.ToArray(), 0, imageContentBytes.Count);
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
            //   infoByte == 0        -> fill rest of line with colour 0 (line terminator)
            //   infoByte == 1, L     -> fill L pixels with colour 0
            //   infoByte even  (>1)  -> length = infoByte >> 1; read colour; fill length pixels
            //   infoByte odd   (>1)  -> length = infoByte >> 1; read length raw palette bytes
            // A terminating 0 is required as it is the only exit from the line loop.

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
                    // Run of 2+ identical non-zero pixels: even-fill form, infoByte = length * 2 (1..127 pixels per chunk)
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
                    // Stretch of single-pixel non-zero colours: odd-literal form,
                    // infoByte = (length << 1) | 1 followed by `length` raw palette bytes (1..127 per chunk).
                    int j = i;
                    while (j < indices.Length && indices[j] != 0)
                    {
                        if (j > i && j + 1 < indices.Length && indices[j + 1] == indices[j])
                        {
                            // The next pixel starts a run of 2+; leave it for even-fill.
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