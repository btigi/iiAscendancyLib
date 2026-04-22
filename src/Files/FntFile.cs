#nullable enable
using System.Collections.Generic;
using SixLabors.ImageSharp;

namespace ii.AscendancyLib.Files
{
    public class FntFile
    {
        public int CharacterHeight { get; set; }
        public int TransparentColourIndex { get; set; }
        public List<Image?> Images = new List<Image?>();
    }
}
