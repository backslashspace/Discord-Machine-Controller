using Pcg;
using System;
using System.Threading;

namespace Configurator
{
    internal static partial class Program
    {
        private static void CreateConfig()
        {
            Thread.Sleep(1024);

            ClearUserSelectionArea(12);

            String result = "";

            //

            GetEndpointName();

            GetRepoPath();

            GetMasterIP();
            
            GetServicePort();

            GetChannelID();

            State.Guid = Guid.NewGuid();
            Console.WriteLine(" - Created Guid: " + State.Guid + '\n');

            KeyGen();

            Console.WriteLine(" - Extracted Program Files");
            Console.WriteLine(" - Generated Client Configuration");

            Thread.Sleep(2048);

            OutPut();
        }

        //

        private static Boolean AskYesNo()
        {
            Console.Write("\n\nY/N> ");

            while (true)
            {
                String input = Console.ReadLine().Trim();

                if (input.Length != 1)
                {
                    Err(input.Length == 0 ? -1 : input.Length);

                    continue;
                }

                if (input.ToLower() == "y") return true;
                if (input.ToLower() == "n") return false;

                Err(input.Length);
            }

            static void Err(Int32 inputLength)
            {
                Console.CursorTop -= 1;

                Console.CursorLeft = 5 + inputLength;

                Console.Write($" - invalid input\nY/N> ");
            }
        }




        
    }
}