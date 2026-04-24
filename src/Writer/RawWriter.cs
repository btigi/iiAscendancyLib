using System;
using System.IO;
using ii.AscendancyLib.Binary;

namespace ii.AscendancyLib.Writer
{
    public class RawWriter
    {
        public void Write(string filename, byte[] wavFile)
        {
            ArgumentNullException.ThrowIfNull(filename);
            ArgumentNullException.ThrowIfNull(wavFile);

            WavPcm.ReadValidatedPcm(wavFile, out _, out _, out _, out var pcm);
            File.WriteAllBytes(filename, pcm);
        }
    }
}