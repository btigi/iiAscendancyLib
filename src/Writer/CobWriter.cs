using ii.AscendancyLib.Binary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ii.AscendancyLib.Writer
{
    public class CobWriter
    {
        private const int MaximumFileNameLength = 50;

        public bool Write(string outputFile, string directory, string cobRootDirectory, string filter)
        {
            var fileNames = new List<FileName>();
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

            return WriteInternal(outputFile, fileNames, files);
        }

        public bool Write(string outputFile, IList<(byte[] content, string path)> entries)
        {
            var fileNames = new List<FileName>();
            var files = new List<byte[]>();

            foreach (var (content, path) in entries)
            {
                var filename = new FileName();
                filename.filename = path.ToArray();
                if (filename.filename.Length < MaximumFileNameLength)
                {
                    Array.Resize(ref filename.filename, MaximumFileNameLength);
                }
                fileNames.Add(filename);
                files.Add(content);
            }

            return WriteInternal(outputFile, fileNames, files);
        }

        private bool WriteInternal(string outputFile, List<FileName> fileNames, List<byte[]> files)
        {
            var fileOffsets = new List<Int32>();

            // Files will start after the filecount and filename and offset blocks. Filenames are 50 bytes, offsets are 4 bytes
            var initialFileOffset = 4 + (fileNames.Count * 50) + (fileNames.Count * 4);
            var runningOffset = 0;
            for (int i = 0; i < files.Count; i++)
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