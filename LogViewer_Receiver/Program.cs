using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Reflection;
using System.Threading.Tasks;

namespace LogViewer
{
    internal static partial class Program
    {
        static void Main()
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

                    List<IPCData.ExtendedLogMessage> latestLog = GetLatestLog(ref pipeClient);

                    foreach (IPCData.ExtendedLogMessage log in latestLog)
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

        //# # # # # # # # # # # # # # # # #

        private static List<IPCData.ExtendedLogMessage> GetLatestLog(ref NamedPipeClientStream pipeClient)
        {
            pipeClient.WriteByte((Byte)RequestType.SendPastLog);
            pipeClient.WaitForPipeDrain();

            Byte[] bufferSize = new Byte[4];

            pipeClient.Read(bufferSize, 0, bufferSize.Length);

            Byte[] buffer = new Byte[BitConverter.ToInt32(bufferSize, 0)];
            pipeClient.Read(buffer, 0, buffer.Length);

            return (List<IPCData.ExtendedLogMessage>)Deserialize(buffer, typeof(List<IPCData.ExtendedLogMessage>));
        }

        private static void UpdateLoop(ref NamedPipeClientStream pipeClient)
        {
            pipeClient.WriteByte((Byte)RequestType.SendUpdates);

            IPCData.ExtendedLogMessage log;

            Byte[] bufferSize = new Byte[4];
            Byte[] buffer;

            while (true)
            {
                switch (pipeClient.ReadByte())
                {
                    case (Byte)RequestType.DataAvailable:
                        break;

                    case (Byte)RequestType.DataNotAvailable:
                        Task.Delay(512).Wait();
                        continue;

                        default:
                        throw new Exception();
                }

                pipeClient.Read(bufferSize, 0, 4);

                buffer = new Byte[BitConverter.ToInt32(bufferSize, 0)];
                pipeClient.Read(buffer, 0, buffer.Length);

                log = (IPCData.ExtendedLogMessage)Deserialize(buffer, typeof(IPCData.ExtendedLogMessage));

                PrintLog(log);
            }
        }

        //# # # # # # # # # # # # # # # # #

        private static void PrintLog(IPCData.ExtendedLogMessage formatedLogMessage)
        {
            UInt16 lineLength = 27;

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"[{formatedLogMessage.TimeStamp:dd.MM.yyyy HH:mm:ss}] [");

            switch (formatedLogMessage.Severity)
            {
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    lineLength += 4;
                    Console.Write("Info");
                    break;
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    lineLength += 5;
                    Console.Write("Debug");
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    lineLength += 7;
                    Console.Write("Warning");
                    break;
                case LogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    lineLength += 7;
                    Console.Write("Verbose");
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    lineLength += 5;
                    Console.Write("Error");
                    break;
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    lineLength += 8;
                    Console.Write("Critical");
                    break;
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;

            Console.Write($"]-[");

            Console.ForegroundColor = ConsoleColor.Gray;

            Console.Write(formatedLogMessage.Source);

            Console.ForegroundColor = ConsoleColor.DarkGray;

            Console.Write($"]");

            lineLength += (UInt16)formatedLogMessage.Source.Length;

            if (lineLength < 52)
            {
                for (UInt16 i = lineLength; i < 52; ++i)
                {
                    Console.Write(" ");
                }
            }
            else
            {
                Console.Write(" ");
            }

            Console.ForegroundColor = ConsoleColor.Gray;

            Console.WriteLine(formatedLogMessage.Message);
        }
    }
}