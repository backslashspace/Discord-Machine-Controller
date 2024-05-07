using System;

namespace Configurator
{
    internal static partial class Program
    {
        private static void GetEndpointName()
        {
            Console.Write($" Use hostname as name?\n\n {State.Name}");

            if (AskYesNo())
            {
                ClearUserSelectionArea(12);

                Console.WriteLine(" - Name: '" + State.Name + "'\n");
            }
            else
            {
                Console.Write("\n Enter the Endpoint Name");

                State.Name = Name();

                ClearUserSelectionArea(12);

                Console.WriteLine(" - Name: '" + State.Name + "'\n");
            }
        }

        private static String Name()
        {
            Console.Write("\n\nName> ");

            while (true)
            {
                String input = Console.ReadLine().Trim();

                if (input.Contains("\"") || input.Length == 0)
                {
                    Err(input.Length);

                    continue;
                }

                return input;
            }

            static void Err(Int32 inputLength)
            {
                Console.CursorTop -= 1;

                Console.CursorLeft = 6 + (inputLength == 0 ? -1 : inputLength);

                Console.Write($" - invalid input\nName> ");
            }
        }
    }
}