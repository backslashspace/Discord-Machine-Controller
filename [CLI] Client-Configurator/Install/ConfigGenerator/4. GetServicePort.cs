using System;

namespace Configurator
{
    internal static partial class Program
    {
        private static void GetServicePort()
        {
            Console.Write($" Use default service port?\n\n {State.ServerIP}:{Config.DefaultServerPort}");

            if (AskYesNo())
            {
                ClearUserSelectionArea(15);

                Console.WriteLine(" - Using default service port: '" + Config.DefaultServerPort + "'\n");
            }
            else
            {
                Console.Write($"\n Enter the service Port");

                State.ServerPort = Port();

                ClearUserSelectionArea(15);

                Console.WriteLine(" - Set destination port to: " + State.ServerPort + '\n');
            }
        }

        private static UInt16 Port()
        {
            Console.Write("\n\nPort> ");

            while (true)
            {
                String input = Console.ReadLine().Trim();

                try
                {
                    return UInt16.Parse(input);
                }
                catch { }

                Err(input.Length);
            }

            static void Err(Int32 inputLength)
            {
                Console.CursorTop -= 1;

                Console.CursorLeft = 6 + (inputLength == 0 ? -1 : inputLength);

                Console.Write($" - invalid input\nPort> ");
            }
        }
    }
}