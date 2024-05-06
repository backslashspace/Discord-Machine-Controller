using Pcg;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace Configurator
{
    internal static partial class Program
    {
        private static void CreateConfig()
        {
            Thread.Sleep(1024);

            ClearUserSelectionArea(12);

            String result;

            Console.Write($" Use default Script-Directory?\n\n {Config.DefaultScriptRepo}\n\nY/N> ");
            result = InputHelper("[yn]", "Y/N> ", 1);
            if (result == "y" || result == "Y")
            {
                ClearUserSelectionArea(12);

                Console.WriteLine(" - Using default path: '" + Config.DefaultScriptRepo + "'\n");
            }
            else
            {
                Console.Write("\n Enter Path without parentheses\n\nPath> ");
                
                State.RepoPath = InputHelper(".:\\\\[^\\\\]", "Path> ", 300);

                ClearUserSelectionArea(12);

                Console.WriteLine(" - Using repo path: '" + State.RepoPath + "'\n");
            }

            Console.Write($" Enter the server (Master-Service) IP\n\nIP> ");
            while (true)
            {
                try
                {
                    result = InputHelper("\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}", "IP> ", 15);

                    State.ServerIP = IPAddress.Parse(result);

                    ClearUserSelectionArea(13);

                    Console.WriteLine(" - Set server ip to: " + result + '\n');

                    break;
                }
                catch
                {
                    Console.CursorTop = Console.CursorTop - 1;

                    Console.Write($"IP> {result} - invalid input\nIP> ");
                }
            }











            Console.Write($" Use default service port?\n\n {State.ServerIP}:{Config.DefaultServerPort}\n\nY/N> ");
            result = InputHelper("[yn]", "Y/N> ", 1);
            if (result == "y" || result == "Y")
            {
                ClearUserSelectionArea(14);

                Console.WriteLine(" - Using default service port: '" + Config.DefaultServerPort + "'\n");
            }
            else
            {
                Console.Write($" Enter the service Port\n\nPort> ");
                while (true)
                {
                    try
                    {
                        result = InputHelper("\\d{1,5}", "Port> ", 5);

                        State.ServerPort = UInt16.Parse(result);

                        ClearUserSelectionArea(14);

                        Console.WriteLine(" - Set destination port to: " + result + '\n');

                        break;
                    }
                    catch
                    {
                        Console.CursorTop = Console.CursorTop - 1;

                        Console.Write($"Port> {result} - invalid input\nPort> ");
                    }
                }
            }













            

            Console.Write($" Enter the target Channel ID\n\nID> ");
            while (true)
            {
                try
                {
                    result = InputHelper("\\d{15,100}", "ID> ", 100);

                    State.ChannelID = UInt64.Parse(result);

                    ClearUserSelectionArea(15);

                    Console.WriteLine(" - Set channel id to: " + result);

                    break;
                }
                catch
                {
                    Console.CursorTop = Console.CursorTop - 1;

                    Console.Write($"ID> {result} - invalid input\nID> ");
                }
            }

            State.Guid = Guid.NewGuid();
            Console.WriteLine(" - Created Guid: " + State.Guid + '\n');

            Console.Write($" Enter a random 32 bit integer\n\nKeyGen> ");
            while (true)
            {
                try
                {
                    result = InputHelper("\\d{1,30}", "KeyGen> ", 32);

                    Int32.Parse(result);

                    ClearUserSelectionArea(17);

                    Console.WriteLine("\n   Using: '" + result + "' for additional randomness\n   => Generating Keys..\n");

                    break;
                }
                catch
                {
                    Console.CursorTop = Console.CursorTop - 1;

                    Console.Write($"KeyGen> {result} - invalid input\nKeyGen> ");
                }
            }

            GenerateKeys(Int32.Parse(result));

            Console.WriteLine("KeyGen: Done");

            ClearUserSelectionArea(12);

            Console.WriteLine(" - Extracted Program Files");
            Console.WriteLine(" - Generated Client Configuration\n\n");

            Thread.Sleep(2048);
        }

        //

        private static String InputHelper(String pattern, String line, Int32 maxLength)
        {
            String input = line;

            while (true)
            {
                try
                {
                    input = Console.ReadLine().Trim();

                    if (input.Length <= maxLength)
                    {
                        Match match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);

                        if (match.Success)
                        {
                            return input;
                        }
                    }

                    throw new Exception();
                }
                catch
                {
                    Console.CursorTop -= 1;

                    if (input == "")
                    {
                        Console.Write($"\n{line}");
                    }
                    else
                    {
                        Console.CursorLeft = line.Length + input.Length;

                        Console.Write($" - invalid input\n{line}");
                    }
                }
            }
        }

        private static void GenerateKeys(Int32 userInput)
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