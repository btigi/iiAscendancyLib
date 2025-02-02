using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ii.AscendancyLib
{
    public class Common
    {
        public const Int32 Bit0 = 1;
        public const Int32 Bit1 = 2 << 0;
        public const Int32 Bit2 = 2 << 1;
        public const Int32 Bit3 = 2 << 2;
        public const Int32 Bit4 = 2 << 3;
        public const Int32 Bit5 = 2 << 4;
        public const Int32 Bit6 = 2 << 5;
        public const Int32 Bit7 = 2 << 6;
        public const Int32 Bit8 = 2 << 7;
        public const Int32 Bit9 = 2 << 8;
        public const Int32 Bit10 = 2 << 9;
        public const Int32 Bit11 = 2 << 10;
        public const Int32 Bit12 = 2 << 11;
        public const Int32 Bit13 = 2 << 12;
        public const Int32 Bit14 = 2 << 13;
        public const Int32 Bit15 = 2 << 14;
        public const Int32 Bit16 = 2 << 15;
        public const Int32 Bit17 = 2 << 16;
        public const Int32 Bit18 = 2 << 17;
        public const Int32 Bit19 = 2 << 18;
        public const Int32 Bit20 = 2 << 19;
        public const Int32 Bit21 = 2 << 20;
        public const Int32 Bit22 = 2 << 21;
        public const Int32 Bit23 = 2 << 22;
        public const Int32 Bit24 = 2 << 23;
        public const Int32 Bit25 = 2 << 24;
        public const Int32 Bit26 = 2 << 25;
        public const Int32 Bit27 = 2 << 26;
        public const Int32 Bit28 = 2 << 27;
        public const Int32 Bit29 = 2 << 28;
        public const Int32 Bit30 = 2 << 29;
        public const Int32 Bit31 = 2 << 30;

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