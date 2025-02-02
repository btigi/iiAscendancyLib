iiAscendancyLib
=========

iiAscendancyLib is a C# library targetting .NET Standard 2.0, supporting the modification of files relating to Ascendancy, the 1995 4X science fiction turn-based strategy computer game.
The library supports:

- cob - read, write
- raw - read
- sav - read, write
- shp - read
- wav - read
- voc - read

## Usage

Sample code to use the library is provided below.

```
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
var vc = new VocConverter();
vc.ConvertVoc(Path.Combine(assetDir, "ASCEND01", "data", "blueexit.voc"), Path.Combine(assetDir, "ASCEND01", "blueexit.wav"), true);

// Convert a RAW to a WAV
Console.WriteLine($"Converting RAW to WAV");
var rawConverter = new RawConverter();
rawConverter.ConvertRaw(Path.Combine(assetDir, "ASCEND01", "data", "shield.voc"), Path.Combine(assetDir, "ASCEND01", "shield.wav"));

// Convert a RAW to a WAV and play it
Console.WriteLine($"Converting RAW to WAV and playing it");
rawConverter.ConvertRaw(Path.Combine(assetDir, "ASCEND02", "data", "theme04.raw"), Path.Combine(assetDir, "ASCEND02", "theme04.raw.wav"));
var player = new SoundPlayer { SoundLocation = Path.Combine(assetDir, "ASCEND02", "theme04.raw.wav") };

player.Play();
Console.WriteLine("Playing theme04 - press [Enter] key to stop");
Console.ReadLine();
player.Stop();

//----------------------------------------------------------------------
//----------------------------------------------------------------------

//  Convert all SHP files
Console.WriteLine($"Converting all SHP files in ASCEND01.COB and saving the first frame");
var converter = new ShpConverter();
List<string> shps = new List<string>(Directory.EnumerateFiles(Path.Combine(assetDir, "ASCEND01", "data"), "*.shp"));
foreach (var shp in shps)
{
    try
    {
        var shpFile = converter.Read(shp);
        shpFile.Images.First().Save(Path.Combine(assetDir, "ASCEND01", $"{Path.GetFileNameWithoutExtension(shp)}.bmp"), ImageFormat.Bmp);
    }
    catch (Exception ex)
    {
        Console.WriteLine(shp + " " + ex.ToString());
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
    }
}

//----------------------------------------------------------------------
//----------------------------------------------------------------------

Console.WriteLine($"Loading resume.gam");
var savReader = new SavReader();
var savegame = savReader.Load(Path.Combine(gameDir, "resume.gam"));

Console.WriteLine($"Saving resume.new");
var savWriter = new SavWriter();
savWriter.Save(Path.Combine(assetDir, "resume.new"), savegame);
```

## Download

Compiled downloads are not available.

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