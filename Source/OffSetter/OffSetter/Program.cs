using System;
using System.IO;

namespace OffSetter
{
    class Program
    {
        private static Int32[] nameCountOffsets = { 41, 117 };
        private static Int32[] fixedOffetsOffsets = { 24, 61, 69, 73, 165, 169, 189 };

        private static Int32 exportCountOffset = 57;
        private static Int32 exportOffsetOffset = 61;

        private static Int32 relativeSerialOffsetOffset = 36;
        private static Int32 relativeSerialSizeOffset = 28;
        private static Int32 exportReferenceSize = 104;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("The program needs one argument - filename to perform offsets \"offsetting\"");
            }
            else
            {
                Span<byte> bytes = File.ReadAllBytes(args[0]);

                NamesOffsettingInteractive(bytes);

                SerialOffsettingInteractive(bytes);

                File.WriteAllBytes(args[0] + ".offset", bytes.ToArray());

                Console.WriteLine("Done");
            }

            Console.ReadKey();
        }

        private static void NamesOffsettingInteractive(Span<byte> span)
        {
            Console.WriteLine("Input names count change");
            Int32 namesCountChange = Int32.Parse(Console.ReadLine());

            Console.WriteLine("Input names length change");
            Int32 namesLengthChange = Int32.Parse(Console.ReadLine());

            NamesOffsetting(span, namesCountChange, namesLengthChange);
        }

        private static void NamesOffsetting(Span<byte> span, Int32 namesCountChange, Int32 namesLengthChange)
        {
            foreach (Int32 offset in nameCountOffsets)
            {
                AddToInt32ByOffset(span, namesCountChange, offset);
            }

            foreach (Int32 offset in fixedOffetsOffsets)
            {
                AddToInt32ByOffset(span, namesLengthChange, offset);
            }

            SerialOffsetting(span, namesLengthChange, 0);                           // All serial offsets will be affected
        }

        private static void SerialOffsettingInteractive(Span<byte> span)
        {
            Console.WriteLine("Input size change of struct in .uexp");
            Int32 sizeChange = Int32.Parse(Console.ReadLine());

            Console.WriteLine("Input SerialOffset of struct changed size in .uexp");
            Int32 sizeChangeOffset = Int32.Parse(Console.ReadLine());

            SerialOffsetting(span, sizeChange, sizeChangeOffset);
        }

        private static void SerialOffsetting(Span<byte> span, Int32 sizeChange, Int32 sizeChangeOffset)
        {
            Int32 exportCount = Int32FromSpanOffset(span, exportCountOffset);
            Int32 exportOffset = Int32FromSpanOffset(span, exportOffsetOffset);

            Int32 currentReferenceOffset = exportOffset;
            bool applyOffsetChange = false;
            int processedReferences = 0;

            while (processedReferences < exportCount)
            {
                if (!applyOffsetChange)
                {
                    RelativeSizeChangeDirection direction = CheckOffsetAffected(span, sizeChangeOffset, currentReferenceOffset);
                    if (direction == RelativeSizeChangeDirection.here)
                    {
                        ApplySerialSizeChange(span, sizeChange, currentReferenceOffset);
                        applyOffsetChange = true;
                    }

                    if (direction == RelativeSizeChangeDirection.before)
                    {
                        ApplySerialOffsetChange(span, sizeChange, currentReferenceOffset);
                        applyOffsetChange = true;
                    }
                }
                else
                {
                    ApplySerialOffsetChange(span, sizeChange, currentReferenceOffset);
                }

                currentReferenceOffset += exportReferenceSize;
                processedReferences++;
            }
        }

        private static Int32 Int32FromSpanOffset(Span<byte> span, Int32 offset)
        {
            return SpanToInt32(span.Slice(offset, 4));
        }

        private static Int32 SpanToInt32(Span<byte> span)
        {
            return span[0] + 256 * (span[1] + 256 * (span[2] + 256 * span[3]));
        }

        private static RelativeSizeChangeDirection CheckOffsetAffected(Span<byte> span, Int32 sizeChangeSerialOffset, Int32 referenceOffset)
        {
            Int32 serialOffset = Int32FromSpanOffset(span, referenceOffset + relativeSerialOffsetOffset);

            if (serialOffset > sizeChangeSerialOffset) return RelativeSizeChangeDirection.before;
            else if (serialOffset == sizeChangeSerialOffset) return RelativeSizeChangeDirection.here;
            else /*if (serialOffset < sizeChangeSerialOffset)*/ return RelativeSizeChangeDirection.after;
        }

        private static void ApplySerialSizeChange(Span<byte> span, Int32 sizeChange, Int32 referenceOffset)
        {
            AddToInt32ByOffset(span, sizeChange, referenceOffset + relativeSerialSizeOffset);
        }

        private static void ApplySerialOffsetChange(Span<byte> span, Int32 sizeChange, Int32 referenceOffset)
        {
            AddToInt32ByOffset(span, sizeChange, referenceOffset+relativeSerialOffsetOffset);
        }

        private static void AddToInt32ByOffset(Span<byte> span, Int32 value, Int32 offset)
        {
            WriteInt32IntoOffset(span, Int32FromSpanOffset(span, offset) + value, offset);
        }

        private static void WriteInt32IntoOffset(Span<byte> span, Int32 value, Int32 offset)
        {
            Span<byte> scope = span.Slice(offset, 4);
            scope[0] = (byte)(value % 256);
            scope[1] = (byte)(value / 256 % 256);
            scope[2] = (byte)(value / 256 / 256 % 256);
            scope[3] = (byte)(value / 256 / 256 / 256);
        }

        private enum RelativeSizeChangeDirection
        {
            after = 0,
            here = 1,
            before = 2
        }
    }
}
