using ii.AscendancyLib.Binary;
using ii.AscendancyLib.Files;
using System;
using System.IO;

namespace ii.AscendancyLib.Reader
{
    public class CobReader
    {
        public CobFile Read(string filename)
        {
            using FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var f = Read(fs);
            return f;
        }

        public CobFile Read(Stream s)
        {
            using BinaryReader br = new(s);
            var cobFile = ParseFile(br);
            br.BaseStream.Seek(0, SeekOrigin.Begin);
            return cobFile;
        }

        private CobFile ParseFile(BinaryReader br)
        {
            var cobFile = new CobFile();

            var fileCount = (Int32)Common.ReadStruct(br, typeof(Int32));
            for (var i = 0; i < fileCount; i++)
            {
                var fileInfo = (FileName)Common.ReadStruct(br, typeof(FileName));
                cobFile.fileNames.Add(fileInfo);
            }

            for (var i = 0; i < fileCount; i++)
            {
                var fileOffset = br.ReadInt32();
                cobFile.fileOffsets.Add(fileOffset);
            }

            for (var i = 0; i < fileCount; i++)
            {
                br.BaseStream.Seek(cobFile.fileOffsets[i], SeekOrigin.Begin);
                var file = br.ReadBytes(i + 1 == fileCount ? (int)br.BaseStream.Length - cobFile.fileOffsets[i] : cobFile.fileOffsets[i + 1] - cobFile.fileOffsets[i]);
                cobFile.files.Add(file);
            }
            return cobFile;
        }
    }
}