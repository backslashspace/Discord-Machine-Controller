using System;

namespace Configurator
{
    internal static partial class Program
    {
        private static void GetChannelID()
        {
            Console.Write($" Enter the target Channel ID");

            State.ChannelID = ID();

            ClearUserSelectionArea(16);

            Console.WriteLine(" - Set channel id to: " + State.ChannelID);
        }

        private static UInt64 ID()
        {
            Console.Write("\n\nID> ");

            while (true)
            {
                String input = Console.ReadLine().Trim();

                try
                {
                    if (input.Length > 15)
                    {
                        return UInt64.Parse(input);
                    }
                }
                catch { }

                Err(input.Length);
            }

            static void Err(Int32 inputLength)
            {
                Console.CursorTop -= 1;

                Console.CursorLeft = 4 + (inputLength == 0 ? -1 : inputLength);

                Console.Write($" - invalid input\nID> ");
            }
        }
    }
}