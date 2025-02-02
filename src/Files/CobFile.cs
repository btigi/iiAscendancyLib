using ii.AscendancyLib.Binary;
using System;
using System.Collections.Generic;

namespace ii.AscendancyLib.Files
{
    public class CobFile
    {
        public List<FileName> fileNames = new();
        public List<Int32> fileOffsets = [];
        public List<byte[]> files = [];
    }
}