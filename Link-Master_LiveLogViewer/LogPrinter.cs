using System;

namespace LogViewer
{
    internal static partial class Program
    {
        private static void PrintLog(IPCLogMessage formattedLogMessage)
        {
            UInt16 lineLength = 27;

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"[{formattedLogMessage.TimeStamp:dd.MM.yyyy HH:mm:ss}] [");

            switch (formattedLogMessage.Severity)
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

            Console.Write(formattedLogMessage.Source);

            Console.ForegroundColor = ConsoleColor.DarkGray;

            Console.Write($"]");

            lineLength += (UInt16)formattedLogMessage.Source.Length;

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

            Console.WriteLine(formattedLogMessage.Message);
        }
    }
}