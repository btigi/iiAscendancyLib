using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ii.AscendancyLib
{
    public class Common
    {
        public static string TryGetString(char[] chars)
        {
            if ((chars.Length > 0) && (chars[0] != '\0'))
            {
                //return new string(chars).TrimEnd('\0');
                var s = new string(chars);
                var firstNull = s.IndexOf('\0');
                firstNull = firstNull == -1 ? s.Length : firstNull;
                return s.Substring(0, firstNull);
            }
            return String.Empty;
        }

        public static char[] ToSizedCharArray(string c, int length = 8)
        {
            c = c.PadRight(length, '\0');
            var array = new char[length];
            var i = 0;
            while (i < length)
            {
                array[i] = c[i];
                i++;
            }
            return array;
        }

        public static Object ReadStruct(BinaryReader br, Type t)
        {
            var buff = br.ReadBytes(Marshal.SizeOf(t));
            var handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
            var s = (Object)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), t);
            handle.Free();
            return s;
        }

        public static byte[] WriteStruct(object anything)
        {
            var rawsize = Marshal.SizeOf(anything);
            var buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.StructureToPtr(anything, buffer, false);
            var rawdatas = new byte[rawsize];
            Marshal.Copy(buffer, rawdatas, 0, rawsize);
            Marshal.FreeHGlobal(buffer);
            return rawdatas;
        }
    }
}