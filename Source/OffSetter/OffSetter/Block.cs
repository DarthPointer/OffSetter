using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DRGOffSetterLib;

namespace OffSetter
{
    abstract class Block
    {
        protected abstract ReadOnlyCollection<Int32> GetAffectedGlobalOffsets();

        public void OffSet(Span<byte> span, Dictionary<RequiredOffSettingData, Int32> args)
        {
            if (args.ContainsKey(RequiredOffSettingData.SizeChange))
            {
                foreach (Int32 offset in GetAffectedGlobalOffsets())
                {
                    DOLib.AddToInt32ByOffset(span, args[RequiredOffSettingData.SizeChange], offset);
                }
            }

            LocalOffSet(span, args);
        }

        protected abstract void LocalOffSet(Span<byte> span, Dictionary<RequiredOffSettingData, Int32> args);

        public abstract void PreviousBlocksOffSet(Span<byte> span, Int32 sizeChange);
    }

    [Block(humanReadableBlockName = "names map", offSettingArguments = RequiredOffSettingData.SizeChange | RequiredOffSettingData.CountChange)]
    class NamesMap : Block
    {
        private static Int32[] nameCountOffsets = { 41, 117 };
        private static Int32[] affectedOffsetsOffsets = { 24, 61, 69, 73, 165, 169, 189 };
        protected override ReadOnlyCollection<Int32> GetAffectedGlobalOffsets()
        {
            return Array.AsReadOnly(affectedOffsetsOffsets);
        }

        protected override void LocalOffSet(Span<byte> span, Dictionary<RequiredOffSettingData, int> args)
        {
            foreach (Int32 offset in nameCountOffsets)
            {
                DOLib.AddToInt32ByOffset(span, args[RequiredOffSettingData.CountChange], offset);
            }
        }

        public override void PreviousBlocksOffSet(Span<byte> span, int sizeChange) { }
    }

    [Block(humanReadableBlockName = "imports map", offSettingArguments = RequiredOffSettingData.SizeChange | RequiredOffSettingData.CountChange)]
    class ImportsMap : Block
    {
        private static Int32 importCountOffset = 65;
        private static Int32[] affectedOffsetsOffsets = { 24, 61, 73, 165, 169, 189 };
        protected override ReadOnlyCollection<Int32> GetAffectedGlobalOffsets()
        {
            return Array.AsReadOnly(affectedOffsetsOffsets);
        }

        protected override void LocalOffSet(Span<byte> span, Dictionary<RequiredOffSettingData, int> args)
        {
            DOLib.AddToInt32ByOffset(span, args[RequiredOffSettingData.CountChange], importCountOffset);
        }

        public override void PreviousBlocksOffSet(Span<byte> span, int sizeChange) { }
    }

    [Block(humanReadableBlockName = "exports map", offSettingArguments = RequiredOffSettingData.SizeChange | RequiredOffSettingData.CountChange)]
    class ExportsMap : Block
    {
        private static Int32 exportCountOffset = 57;
        private static Int32[] affectedOffsetsOffsets = { 24, 73, 165, 169, 189 };

        protected override ReadOnlyCollection<Int32> GetAffectedGlobalOffsets()
        {
            return Array.AsReadOnly(affectedOffsetsOffsets);
        }

        protected override void LocalOffSet(Span<byte> span, Dictionary<RequiredOffSettingData, int> args)
        {
            DOLib.AddToInt32ByOffset(span, args[RequiredOffSettingData.CountChange], exportCountOffset);
        }

        public override void PreviousBlocksOffSet(Span<byte> span, int sizeChange) { }
    }

    [Block(humanReadableBlockName = "exported (.uexp) data", offSettingArguments = RequiredOffSettingData.SizeChange | RequiredOffSettingData.SizeChangeOffset)]
    class Exports : Block
    {
        private static Int32 exportsCountOffset = 57;
        private static Int32 exportsMapOffsetOffset = 61;
        private static Int32 relativeSerialOffsetOffset = 36;
        private static Int32 relativeSerialSizeOffset = 28;
        private static Int32 exportReferenceSize = 104;


        private Int32[] affectedOffsetsOffsets = { 169 };
        protected override ReadOnlyCollection<Int32> GetAffectedGlobalOffsets()
        {
            return Array.AsReadOnly(affectedOffsetsOffsets);
        }

        protected override void LocalOffSet(Span<byte> span, Dictionary<RequiredOffSettingData, int> args)
        {
            SerialOffsetting(span, args[RequiredOffSettingData.SizeChange], args[RequiredOffSettingData.SizeChangeOffset]);
        }

        public override void PreviousBlocksOffSet(Span<byte> span, int sizeChange)
        {
            SerialOffsetting(span, sizeChange, 0);
        }

        #region serial offsetting tools

        private static void SerialOffsetting(Span<byte> span, Int32 sizeChange, Int32 sizeChangeOffset)
        {
            Int32 exportsCount = DOLib.Int32FromSpanOffset(span, exportsCountOffset);
            Int32 exportsMapOffset = DOLib.Int32FromSpanOffset(span, exportsMapOffsetOffset);

            Int32 currentReferenceOffset = exportsMapOffset;
            bool applyOffsetChange = false;
            int processedReferences = 0;

            while (processedReferences < exportsCount)
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

        private static RelativeSizeChangeDirection CheckOffsetAffected(Span<byte> span, Int32 sizeChangeSerialOffset, Int32 referenceOffset)
        {
            Int32 serialOffset = DOLib.Int32FromSpanOffset(span, referenceOffset + relativeSerialOffsetOffset);

            if (serialOffset > sizeChangeSerialOffset) return RelativeSizeChangeDirection.before;
            else if (serialOffset == sizeChangeSerialOffset) return RelativeSizeChangeDirection.here;
            else /*if (serialOffset < sizeChangeSerialOffset)*/ return RelativeSizeChangeDirection.after;
        }

        private static void ApplySerialSizeChange(Span<byte> span, Int32 sizeChange, Int32 referenceOffset)
        {
            DOLib.AddToInt32ByOffset(span, sizeChange, referenceOffset + relativeSerialSizeOffset);
        }

        private static void ApplySerialOffsetChange(Span<byte> span, Int32 sizeChange, Int32 referenceOffset)
        {
            DOLib.AddToInt32ByOffset(span, sizeChange, referenceOffset + relativeSerialOffsetOffset);
        }

        private enum RelativeSizeChangeDirection
        {
            after = 0,
            here = 1,
            before = 2
        }

        #endregion
    }

    public enum RequiredOffSettingData
    {
        None = 0,
        SizeChange = 1,
        SizeChangeOffset = 2,
        CountChange = 4
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class BlockAttribute : Attribute
    {
        public RequiredOffSettingData offSettingArguments;
        public string humanReadableBlockName;
    }
}
