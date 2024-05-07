using System;
using System.Net;
using System.Text.RegularExpressions;

namespace Configurator
{
    internal static partial class Program
    {
        private static void GetMasterIP()
        {
            Console.Write($" Enter the server (Master-Service) ip");

            State.ServerIP = IP();

            ClearUserSelectionArea(14);

            Console.WriteLine(" - Set server ip to: " + State.ServerIP + '\n');
        }

        private static IPAddress IP()
        {
            Console.Write("\n\nIP> ");

            while (true)
            {
                String input = Console.ReadLine().Trim();

                try
                {
                    if (Regex.IsMatch(input, "\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}"))
                    {
                        return IPAddress.Parse(input);
                    }
                }
                catch { }
                
                Err(input.Length);
            }

            static void Err(Int32 inputLength)
            {
                Console.CursorTop -= 1;

                Console.CursorLeft = 4 + (inputLength == 0 ? -1 : inputLength);

                Console.Write($" - invalid input\nIP> ");
            }
        }
    }
}