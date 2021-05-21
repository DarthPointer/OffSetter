using System;
using System.Collections.Generic;
using System.IO;
using DRGOffSetterLib;

namespace OffSetter
{
    class Program
    {
        private static List<Block> blocks = new List<Block> { new NamesMap(), new ImportsMap(), new ExportsMap(), new Exports() };
        private static Dictionary<string, int> modeKeysToBlockIndex = new Dictionary<string, int> {
            { "-n" , 0 },
            { "-i" , 1 },
            { "-edef" , 2 },
            { "-e" , 3 }
        };
        private static string mutedKey = "-m";
        private static string replaceKey = "-r";

        static void Main(string[] args)
        {
            RunData runData = ProcessArgs(args);

            if (runData.fileName.Length == 0)
            {
                Console.WriteLine("Need filename to operate!");
            }
            else
            {
                Span<byte> span = File.ReadAllBytes(runData.fileName);

                if (runData.modeCode != -1)
                {
                    ExecuteMode(span, runData.modeCode, 0, runData.modeArgs, runData.mutedMode);
                }
                else
                {
                    ExecuteAllModes(span, runData.modeArgs, runData.mutedMode);
                }

                WriteResult(span, runData.fileName, runData.replaceMode);

                if (!runData.mutedMode) Console.WriteLine("Done!");
            }

            if (!runData.mutedMode) Console.ReadKey();
        }

        private static RunData ProcessArgs(string[] args)
        {
            RunData result = new RunData();
            result.fileName = args[0];

            foreach (string arg in args.AsSpan(1))
            {
                if (modeKeysToBlockIndex.ContainsKey(arg))
                {
                    result.modeCode = modeKeysToBlockIndex[arg];
                }
                else if (mutedKey == arg)
                {
                    result.mutedMode = true;
                }
                else if (replaceKey == arg)
                {
                    result.replaceMode = true;
                }
                else
                {
                    result.modeArgs.Add(arg);
                }
            }

            return result;
        }

        private static int ExecuteMode(Span<byte> span, int modeIndex, int cumulativeOffset, List<string> modeArgs, bool mutedMode)
        {
            Block block = blocks[modeIndex];

            Dictionary<RequiredOffSettingData, Int32> args = new Dictionary<RequiredOffSettingData, int>();

            BlockAttribute blockAttribute = (BlockAttribute)Attribute.GetCustomAttribute(block.GetType(), typeof(BlockAttribute));

            string humanReadableBlockName = blockAttribute.humanReadableBlockName;
            RequiredOffSettingData blockArgs = blockAttribute.offSettingArguments;

            int sizeChange = 0;
            if ((blockArgs & RequiredOffSettingData.SizeChange) != RequiredOffSettingData.None)
            {
                sizeChange = int.Parse(UseOrRequestArg($"Input size change for {humanReadableBlockName}", modeArgs, mutedMode));

                args[RequiredOffSettingData.SizeChange] = sizeChange;
            }

            if ((blockArgs & RequiredOffSettingData.SizeChangeOffset) != RequiredOffSettingData.None)
            {
                int sizeChangeOffset = int.Parse(UseOrRequestArg($"Input size change offset for {humanReadableBlockName}", modeArgs, mutedMode));

                args[RequiredOffSettingData.SizeChangeOffset] = sizeChangeOffset + cumulativeOffset;
            }

            if ((blockArgs & RequiredOffSettingData.CountChange) != RequiredOffSettingData.None)
            {
                int countChange = int.Parse(UseOrRequestArg($"Input count change for {humanReadableBlockName}", modeArgs, mutedMode));

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

        private static void ExecuteAllModes(Span<byte> span, List<string> modeArgs, bool mutedMode)
        {
            throw new NotImplementedException("All-modes execution is disabled as currently it is considered too difficult to use reliably.");

            int cumulativeOffset = 0;
            for (int modeIndex = 0; modeIndex < blocks.Count; modeIndex++)
            {
                cumulativeOffset = ExecuteMode(span, modeIndex, cumulativeOffset, modeArgs, mutedMode);
            }
        }

        private static string UseOrRequestArg(string requestMessage, List<string> modeArgs, bool mutedMode)
        {
            if (modeArgs.Count > 0)
            {
                return modeArgs.TakeArg();
            }
            else if (!mutedMode)
            {
                Console.WriteLine(requestMessage);
                return Console.ReadLine();
            }
            else
            {
                throw new Exception("Insufficient number of arguments passed on launch in muted mode");
            }
        }

        private static void WriteResult(Span<byte> span, string fileName, bool replaceMode)
        {
            if (replaceMode)
            {
                if (File.Exists(fileName + ".offset")) File.Delete(fileName + ".offset");
                Directory.Move(fileName, fileName + ".offset");
                File.WriteAllBytes(fileName, span.ToArray());
            }
            else
            {
                File.WriteAllBytes(fileName + ".offset", span.ToArray());
            }
        }

        private record RunData
        {
            public bool mutedMode = false;
            public bool replaceMode = false;
            public string fileName = "";
            public int modeCode = -1;

            public List<string> modeArgs = new List<string>();
        }
    }
}
