using ii.AscendancyLib.Binary;
using ii.AscendancyLib.Files;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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

        public void FromBitmap(List<string> filenames, string output)
        {
            var imageInfos = new List<ImageInfo>();

            long offset = 0;
            var imagesAdded = 0;
            var imageContentBytes = new List<byte>();
            foreach (var file in filenames)
            {
                imagesAdded++;
                offset = 8 + (imagesAdded * 8) + imageContentBytes.Count;

                var bitmap = new Bitmap(file);

                var imageHeader = new ImageHeader();
                imageHeader.Height = (short)bitmap.Height;
                imageHeader.YCentre = 0;
                imageHeader.XCentre = 0;
                imageHeader.xStart = 0;
                imageHeader.yStart = 0;
                imageHeader.xEnd = (short)bitmap.Width;
                imageHeader.yEnd = (short)bitmap.Height;
                imageHeader.Width = (short)bitmap.Width;

                imageContentBytes.AddRange(Common.WriteStruct(imageHeader));

                var bData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                var size = bData.Stride * bData.Height;
                var data = new byte[size];
                Marshal.Copy(bData.Scan0, data, 0, size);

                var brightnessMultiplier = 4;
                for (int i = 0; i < size / 4; i += 4)
                {
                    var r = data[i + 0] / brightnessMultiplier;
                    var g = data[i + 1] / brightnessMultiplier;
                    var b = data[i + 2] / brightnessMultiplier;
                    var a = data[i + 3] / brightnessMultiplier;
                    var colour = $"{b:X2}{g:X2}{r:X2}";

                    var paleteIndex = DefaultPalette.defaultPalette.IndexOf(colour);
                    //if (colour == "30200C" && false)
                    //{
                    //    imageContentBytes.Add(62);
                    //    imageContentBytes.Add(242);
                    //}
                    //if (colour == "24A410" && false)
                    //{
                    //    imageContentBytes.Add(62);
                    //    imageContentBytes.Add(100);
                    //}
                    //if (colour == "24A410" || true)
                    //{
                    //    imageContentBytes.Add(62);
                    //    imageContentBytes.Add(50);
                    //}

                    imageContentBytes.Add(1);
                    imageContentBytes.Add((byte)paleteIndex);

                    // 0 -> fill the rest of the row with the default colour
                    // 1,x -> fill x bytes with default colour
                    // ((infoByte & Common.Bit0) == 0) -> var length = infoByte >> 1; then 'read a byte to represet the colour, draw length bytes that colour'
                    //if (i == 5)
                    //{
                    //    imageContentBytes.Add(62);
                    //    imageContentBytes.Add(10);
                    //}
                    // ((infoByte & Common.Bit0) != 0) -> var length = infoByte >> 1; then 'read a byte to represet the colour, draw a byte', length times

                    //imageContentBytes.Add(0);
                }

                Marshal.Copy(data, 0, bData.Scan0, data.Length);
                bitmap.UnlockBits(bData);

                var imageInfo = new ImageInfo();
                imageInfo.ImageOffset = (Int32)offset;
                imageInfo.PaletteOffset = 0;
                imageInfos.Add(imageInfo);
            }

            var introBytes = new List<byte>();

            var header = new ShpHeader();
            header.Version = "1.10".ToCharArray();
            header.ImageCount = filenames.Count;

            introBytes.AddRange(Common.WriteStruct(header));

            foreach (var i in imageInfos)
            {
                introBytes.AddRange(Common.WriteStruct(i));
            }

            using var fs = new FileStream(output, FileMode.Create);
            using var bw = new BinaryWriter(fs);
            bw.BaseStream.Write(introBytes.ToArray(), 0, introBytes.Count);
            bw.BaseStream.Write(imageContentBytes.ToArray(), 0, imageContentBytes.Count);
        }
    }
}