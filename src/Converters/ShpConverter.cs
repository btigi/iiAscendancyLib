using ii.AscendancyLib.Binary;
using ii.AscendancyLib.Files;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace ii.AscendancyLib.Converters
{
    public class ShpConverter
    {
        private string fname;
        private bool Debug { get; set; }

        public ShpFile Read(string filename)
        {
            fname = Path.GetFileName(filename);
            using FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var f = Read(fs);
            return f;
        }

        public ShpFile Read(Stream s)
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

                    var width = imageInfo.Width + 1;
                    var height = imageInfo.Height + 1;
                    var bitmap = new Bitmap(width, height);

                    var bData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                    var size = bData.Stride * bData.Height;
                    var data = new byte[size];
                    Marshal.Copy(bData.Scan0, data, 0, size);
                    var cnt = 0;
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            var s = DefaultPalette.defaultPalette[pixels[x, y]];
                            var c = DefaultPalette.GetColour(s);
                            data[cnt] = c.B;
                            data[cnt + 1] = c.G;
                            data[cnt + 2] = c.R;
                            data[cnt + 3] = c.A;
                            cnt += 4;
                        }
                    }

                    Marshal.Copy(data, 0, bData.Scan0, data.Length);
                    bitmap.UnlockBits(bData);

                    shpFile.Images.Add(bitmap);
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
    }
}