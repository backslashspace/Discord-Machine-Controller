using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer
{
    internal static partial class Program
    {
        private static void Main()
        {
            Console.Title = $"Pipe Log Viewer v{Assembly.GetExecutingAssembly().GetName().Version}";

            Logo.Print();

            NamedPipeClientStream pipeClient = new(".", "90436256a2031114e5ebf0a311c95dacbdabb851", PipeDirection.InOut, PipeOptions.Asynchronous);

            while (true)
            {
                try
                {
                    ConsoleMSG("> Attempting to attach to service\n", ConsoleColor.DarkGreen);

                    pipeClient.Connect();

                    List<IPCLogMessage> latestLog = GetLatestLog(ref pipeClient);

                    foreach (IPCLogMessage log in latestLog)
                    {
                        PrintLog(log);
                    }

                    UpdateLoop(ref pipeClient);
                }
                catch
                {
                    Console.WriteLine();

                    ConsoleMSG("> Lost connection", ConsoleColor.DarkRed);

                    pipeClient = new(".", "90436256a2031114e5ebf0a311c95dacbdabb851", PipeDirection.InOut, PipeOptions.Asynchronous);
                }
            }

            //# # # # # # # # # # # # # # # # # # # # # # # # # # # # #

            static void ConsoleMSG(String msg, ConsoleColor msgColor)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("Console");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("] ");
                Console.ForegroundColor = msgColor;
                Console.WriteLine(msg);
            }
        }
    }
}