using System;
using System.Collections.Generic;
using System.IO;

namespace OffSetter
{
    class Program
    {
        private static List<Block> blocks = new List<Block> { new NamesMap(), new ImportsMap(), new Exports() };
        private static Dictionary<string, int> modeKeysToBlockIndex = new Dictionary<string, int> {
            { "-n" , 0 },
            { "-i" , 1 },
            { "-e" , 2 }
        };

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Need filename to operate!");
            }
            else
            {
                Span<byte> span = File.ReadAllBytes(args[0]);

                if (args.Length > 1)
                {
                    if (modeKeysToBlockIndex.TryGetValue(args[1], out int modeIndex))
                    {
                        ExecuteMode(span, modeIndex, 0);
                    }
                    else
                    {
                        ExecuteAllModes(span);
                    }
                }
                else
                {
                    ExecuteAllModes(span);
                }

                File.WriteAllBytes(args[0] + ".offset", span.ToArray());

                Console.WriteLine("Done!");
            }

            Console.ReadKey();
        }

        static int ExecuteMode(Span<byte> span, int modeIndex, int cumulativeOffset)
        {
            Block block = blocks[modeIndex];

            Dictionary<RequiredOffSettingData, Int32> args = new Dictionary<RequiredOffSettingData, int>();

            BlockAttribute blockAttribute = (BlockAttribute)Attribute.GetCustomAttribute(block.GetType(), typeof(BlockAttribute));

            string humanReadableBlockName = blockAttribute.humanReadableBlockName;
            RequiredOffSettingData blockArgs = blockAttribute.offSettingArguments;

            int sizeChange = 0;
            if ((blockArgs & RequiredOffSettingData.SizeChange) != RequiredOffSettingData.None)
            {
                Console.WriteLine($"Input size change for {humanReadableBlockName}");
                sizeChange = int.Parse(Console.ReadLine());

                args[RequiredOffSettingData.SizeChange] = sizeChange;
            }

            if ((blockArgs & RequiredOffSettingData.SizeChangeOffset) != RequiredOffSettingData.None)
            {
                Console.WriteLine($"Input size change offset for {humanReadableBlockName}");
                int sizeChangeOffset = int.Parse(Console.ReadLine());

                args[RequiredOffSettingData.SizeChangeOffset] = sizeChangeOffset + cumulativeOffset;
            }

            if ((blockArgs & RequiredOffSettingData.CountChange) != RequiredOffSettingData.None)
            {
                Console.WriteLine($"Input count change for {humanReadableBlockName}");
                int countChange = int.Parse(Console.ReadLine());

                args[RequiredOffSettingData.CountChange] = countChange;
            }

            block.OffSet(span, args);

            modeIndex++;

            while (modeIndex < blocks.Count)
            {
                blocks[modeIndex].PreviousBlocksOffSet(span, sizeChange);

                modeIndex++;
            }

            return cumulativeOffset + sizeChange;
        }

        static void ExecuteAllModes(Span<byte> span)
        {
            throw new NotImplementedException("All-modes execution is disabled as currently it is considered too difficult to use reliably.");

            int cumulativeOffset = 0;
            for (int modeIndex = 0; modeIndex < blocks.Count; modeIndex++)
            {
                cumulativeOffset = ExecuteMode(span, modeIndex, cumulativeOffset);
            }
        }
    }
}
