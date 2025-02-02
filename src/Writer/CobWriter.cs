using ii.AscendancyLib.Binary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ii.AscendancyLib.Writer
{
    public class CobWriter
    {
        public bool Write(string outputFile, string directory, string cobRootDirectory, string filter)
        {
            const int MaximumFileNameLength = 50;
            var fileNames = new List<FileName>();
            var fileOffsets = new List<Int32>();
            var files = new List<byte[]>();

            var filesToAdd = Directory.EnumerateFiles(directory, filter, SearchOption.AllDirectories);

            foreach (var file in filesToAdd)
            {
                var filename = new FileName();
                filename.filename = file.Replace(cobRootDirectory, String.Empty).ToArray();
                if (filename.filename.Length < MaximumFileNameLength)
                {
                    Array.Resize(ref filename.filename, MaximumFileNameLength);
                }
                fileNames.Add(filename);
                files.Add(File.ReadAllBytes(file));
            }

            // Files will start after the filecount and filename and offset blocks. Filenames are 50 bytes, offsets are 4 bytes
            var initialFileOffset = 4 + (fileNames.Count * 50) + (fileNames.Count * 4) ;
            var runningOffset = 0;
            for (int i = 0; i < files.Count; i++ )
            {
                fileOffsets.Add(initialFileOffset + runningOffset);
                runningOffset += files[i].Length;
            }

            using var bw = new BinaryWriter(File.OpenWrite(outputFile));
            bw.Write(fileNames.Count);

            foreach (var filename in fileNames)
            {
                bw.Write(filename.filename);
            }

            foreach (var fileOffset in fileOffsets)
            {
                bw.Write(fileOffset);
            }

            foreach (var file in files)
            {
                bw.Write(file);
            }
            bw.Flush();

            return true;
        }
    }
}