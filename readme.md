iiAscendancyLib
=========

iiAscendancyLib is a C# library targetting .NET10, supporting the modification of files relating to Ascendancy, the 1995 4X science fiction turn-based strategy computer game.
The library supports:

| Name     | Read | Write | Comment |
|----------|:----:|-------|:--------|
| COB      | ✔   |   ✔   |
| FLC      | ➜   |   ✗   | Autodesk FLC, see ii.FLC
| FNT      | ✔   |   ✔   |
| RAW      | ✔   |   ✔   |
| SAV      | ✔   |   ✔   | File format is not fully decoded
| SHP      | ✔   |   ✔   |
| WAV      | ✔   |   ✗   |
| VOC      | ✔   |   ✔   |


## Usage

Sample code to use the library is provided below.

```csharp
using ii.AscendancyLib.Reader;
using ii.AscendancyLib.Writer;
using SixLabors.ImageSharp;

var gameDir = @"D:\Games\ascendancy\ASCEND\";
var assetDir = @"D:\data\Ascendancy\";

//----------------------------------------------------------------------
//----------------------------------------------------------------------

Console.WriteLine($"Extracting all files to {assetDir}");
// Open a COB file
var cobFiles = new string[] { "ASCEND00.COB", "ASCEND01.COB", "ASCEND02.COB" };
var cr = new CobReader();
foreach (var cobFile in cobFiles)
{
	var file = cr.Read(Path.Combine(gameDir, cobFile));
	Console.WriteLine($"{cobFile} contains {file.files.Count} files");

	// Save all files to disk
	for (int i = 0; i < file.files.Count; i++)
	{
		var fileName = string.Join("", file.fileNames[i].filename).Split('\0')[0].ToString();
		fileName = fileName.StartsWith("\\") ? fileName.Substring(1) : fileName;
		fileName = Path.Combine(assetDir, Path.GetFileNameWithoutExtension(cobFile), fileName);
		var directory = Path.GetDirectoryName(fileName);
		Directory.CreateDirectory(directory);
		using var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
		fs.Write(file.files[i], 0, file.files[i].Length);
		fs.Flush(flushToDisk: true);
	}
}

// Create a new COB file from the files just extracted
Console.WriteLine($"Repacing ASCEND00.COB into 00COPY.COB");
var cobWriter = new CobWriter();
cobWriter.Write(Path.Combine(assetDir, "00COPY.COB"), Path.Combine(assetDir, "ASCEND00"), Path.Combine(assetDir, "ASCEND00\\"), "*.*");

//----------------------------------------------------------------------
//----------------------------------------------------------------------

// Convert a VOC to a WAV
Console.WriteLine($"Converting VOC to WAV");
var vocReader = new VocReader();
var convertedVoc = vocReader.Read(Path.Combine(assetDir, "ASCEND01", "data", "blueexit.voc"), true);

// Convert a RAW to a WAV
Console.WriteLine($"Converting RAW to WAV");
var rawReader = new RawReader();
var convertedRaw = rawReader.Read(Path.Combine(assetDir, "ASCEND01", "data", "shield.voc"));

//----------------------------------------------------------------------
//----------------------------------------------------------------------

//  Convert all SHP files
Console.WriteLine($"Converting all SHP files in ASCEND01.COB and saving the first frame");
var shpReader = new ShpReader();
List<string> shps = new List<string>(Directory.EnumerateFiles(Path.Combine(assetDir, "ASCEND01", "data"), "*.shp"));
foreach (var shp in shps)
{
	var shpFile = shpReader.Read(shp);
	shpFile.Images.First().SaveAsJpeg(Path.Combine(assetDir, "ASCEND01", $"{Path.GetFileNameWithoutExtension(shp)}.bmp"));
}

//----------------------------------------------------------------------
//----------------------------------------------------------------------

Console.WriteLine($"Loading resume.gam");
var savReader = new SavReader();
var savegame = savReader.Read(Path.Combine(gameDir, "resume.gam"));

Console.WriteLine($"Saving resume.new");
var savWriter = new SavWriter();
savWriter.Write(Path.Combine(assetDir, "resume.new"), savegame);
```

## Compiling

To clone and run this application, you'll need [Git](https://git-scm.com) and [.NET](https://dotnet.microsoft.com/) installed on your computer. From your command line:

```
# Clone this repository
$ git clone https://github.com/btigi/iiAscendancyLib

# Go into the repository
$ cd src

# Build  the app
$ dotnet build
```

## Licencing

iiAscendancyLib is licenced under the MIT License. Full licence details are available in licence.md
