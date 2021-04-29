using System;
using System.Collections.Generic;

namespace DRGOffSetterLib
{
    public static class DOLib
    {
        public static Int32 Int32FromSpanOffset(Span<byte> span, Int32 offset)
        {
            return SpanToInt32(span.Slice(offset, 4));
        }

        public static Int32 SpanToInt32(Span<byte> span)
        {
            return span[0] + 256 * (span[1] + 256 * (span[2] + 256 * span[3]));
        }

        public static void AddToInt32ByOffset(Span<byte> span, Int32 value, Int32 offset)
        {
            WriteInt32IntoOffset(span, Int32FromSpanOffset(span, offset) + value, offset);
        }

        public static void WriteInt32IntoOffset(Span<byte> span, Int32 value, Int32 offset)
        {
            Span<byte> scope = span.Slice(offset, 4);

            scope[0] = (byte)(value & 0xFF);
            scope[1] = (byte)((value & 0xFF00) >> 8);
            scope[2] = (byte)((value & 0xFF0000) >> 16);
            scope[3] = (byte)((value & 0xFF000000) >> 24);
        }
    }

    public static class ListExtension
    {
        public static string TakeArg(this List<string> argList)
        {
            string result = argList[0];
            argList.RemoveAt(0);
            return result;
        }
    }
}
