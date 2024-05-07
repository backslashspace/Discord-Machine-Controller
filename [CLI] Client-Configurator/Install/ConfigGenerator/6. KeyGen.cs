using Pcg;
using System;
using System.Threading;

namespace Configurator
{
    internal static partial class Program
    {
        private static void KeyGen()
        {
            Console.Write($" Enter a random 32 bit integer");

            Int32 rand = YRand();

            ClearUserSelectionArea(19);

            Console.WriteLine("   Using: '" + rand + "' for additional randomness\n   => Generating Keys..\n");

            GenerateKeys(ref rand);

            ClearUserSelectionArea(18);

            Console.WriteLine(" - Generated Keys");

            Thread.Sleep(2048);

            ClearUserSelectionArea(12);
        }

        private static Int32 YRand()
        {
            Console.Write("\n\nKeyGen> ");

            while (true)
            {
                String input = Console.ReadLine().Trim();

                try
                {
                    return Int32.Parse(input);
                }
                catch { }

                Err(input.Length);
            }

            static void Err(Int32 inputLength)
            {
                Console.CursorTop -= 1;

                Console.CursorLeft = 8 + (inputLength == 0 ? -1 : inputLength);

                Console.Write($" - invalid input\nKeyGen> ");
            }
        }

        private static void GenerateKeys(ref readonly Int32 userInput)
        {
            Byte[,] normalArray = new Byte[4, 24];

            PcgRandom pcgRandom = new();

            Thread.Sleep((Int32)(pcgRandom.NextDouble() * 1000));

            pcgRandom = new(userInput);

            for (Int16 e = 0; e < 4; ++e)
            {
                if (pcgRandom.Next() < (Int32.MaxValue / 2))

                    pcgRandom = new();

                for (Int16 i = 0; i < 24; ++i)
                {
                    Byte[] buffer = new Byte[1];

                    pcgRandom.NextBytes(buffer);

                    normalArray[e, i] = buffer[0];

                    if (pcgRandom.Next() < (Int32.MaxValue / 2))
                    {
                        Thread.Sleep((Int32)(pcgRandom.NextDouble() * 400));

                        pcgRandom = new(pcgRandom.Next(pcgRandom.Next()));
                    }
                }

                pcgRandom = new((Int32)unchecked((UInt16)(userInput + userInput * pcgRandom.Next())));
            }

            Buffer.BlockCopy(normalArray, 0, State.Keys, 0, 24);
            Buffer.BlockCopy(normalArray, 24, State.Keys, 24, 24);
            Buffer.BlockCopy(normalArray, 48, State.Keys, 48, 24);
            Buffer.BlockCopy(normalArray, 72, State.Keys, 72, 24);
        }
    }
}