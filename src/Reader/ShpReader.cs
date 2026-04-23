using ii.AscendancyLib.Binary;
using ii.AscendancyLib.Files;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using ImageInfo = ii.AscendancyLib.Binary.ImageInfo;

namespace ii.AscendancyLib.Reader
{
	public class ShpReader
	{
		public bool Debug { get; set; }

		public ShpFile Read(string filename)
		{
			using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
			return Read(fs);
		}

		public ShpFile Read(Stream s)
		{
			using var br = new BinaryReader(s);
			var shpFile = ParseFile(br);
			br.BaseStream.Seek(0, SeekOrigin.Begin);
			return shpFile;
		}

		private ShpFile ParseFile(BinaryReader br)
		{
			var imageInfos = new List<ImageInfo>();
			var shpFile = new ShpFile();

			var header = (ShpHeader)Common.ReadStruct(br, typeof(ShpHeader));
			if (string.Join(string.Empty, header.Version) != "1.10")
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
				br.BaseStream.Seek(imageInfos[i].ImageOffset, SeekOrigin.Begin);
				var imageInfo = (ImageHeader)Common.ReadStruct(br, typeof(ImageHeader));

				var pixels = new byte[imageInfo.Width + 1, imageInfo.Height + 1];
				for (int line = 0; line < (imageInfo.yEnd - imageInfo.yStart) + 1; line++)
				{
					var lineComplete = false;
					var lineProgress = 0;
					while (!lineComplete)
					{
						if (HasData(br.BaseStream))
						{
							var infoByte = br.ReadByte();
							if (infoByte == 0)
							{
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
									lineComplete = true;
								}
							}

							if ((infoByte & Common.Bit0) == 0)
							{
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
									lineComplete = true;
								}
							}

							if ((infoByte & Common.Bit0) != 0)
							{
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
										lineComplete = true;
									}
								}
								continue;
							}
						}
						else
						{
							lineComplete = true;
						}
					}
				}

				if (imageInfos[i].PaletteOffset != 0)
				{
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
			return shpFile;
		}

		private static bool HasData(Stream stream)
		{
			return stream.Length != stream.Position;
		}
	}
}
