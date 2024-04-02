using System;

namespace LogViewer
{
    internal static class xConsole
    {
        internal static void PrintLogo()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;

            Console.WriteLine("███████╗ ██████╗██████╗ ██╗██████╗ ████████╗██████╗ ██╗   ██╗███╗   ██╗███╗   ██╗███████╗██████╗ ");
            Console.WriteLine("██╔════╝██╔════╝██╔══██╗██║██╔══██╗╚══██╔══╝██╔══██╗██║   ██║████╗  ██║████╗  ██║██╔════╝██╔══██╗");
            Console.WriteLine("███████╗██║     ██████╔╝██║██████╔╝   ██║   ██████╔╝██║   ██║██╔██╗ ██║██╔██╗ ██║█████╗  ██████╔╝");
            Console.WriteLine("╚════██║██║     ██╔══██╗██║██╔═══╝    ██║   ██╔══██╗██║   ██║██║╚██╗██║██║╚██╗██║██╔══╝  ██╔══██╗");
            Console.WriteLine("███████║╚██████╗██║  ██║██║██║        ██║   ██║  ██║╚██████╔╝██║ ╚████║██║ ╚████║███████╗██║  ██║");
            Console.WriteLine("╚══════╝ ╚═════╝╚═╝  ╚═╝╚═╝╚═╝        ╚═╝   ╚═╝  ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚═╝  ╚═══╝╚══════╝╚═╝  ╚═╝");
        }

        //

        internal static void PrintLog(Program.ConsoleMessage formattedLogMessage)
        {
            UInt16 lineLength = 27;

            System.Console.ForegroundColor = ConsoleColor.DarkGray;
            System.Console.Write($"[{formattedLogMessage.TimeStamp:dd.MM.yyyy HH:mm:ss}] [");

            switch (formattedLogMessage.Severity)
            {
                case Program.xLogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    lineLength += 4;
                    Console.Write("Info");
                    break;
                case Program.xLogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    lineLength += 5;
                    Console.Write("Debug");
                    break;
                case Program.xLogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    lineLength += 7;
                    Console.Write("Warning");
                    break;
                case Program.xLogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    lineLength += 7;
                    Console.Write("Verbose");
                    break;
                case Program.xLogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    lineLength += 5;
                    Console.Write("Error");
                    break;
                case Program.xLogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    lineLength += 8;
                    Console.Write("Critical");
                    break;
                case Program.xLogSeverity.Alert:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    lineLength += 5;
                    Console.Write("Alert");
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

        //

        private static UInt64 RxBytes = 0;
        private static UInt64 TxBytes = 0;
        private static Boolean CountersHaveBeenReset = false;
        private static UInt16 NumOfResets = 0;

        internal static void UpdateConsoleHead(Int32 _RxBytes, Int32 _TxBytes)
        {
            RxBytes += (UInt64)_RxBytes;
            TxBytes += (UInt64)_TxBytes;

            if (RxBytes > UInt64.MaxValue - Int32.MaxValue)
            {
                RxBytes = 0;
                CountersHaveBeenReset = true;
                ++NumOfResets;
            }

            if (TxBytes > UInt64.MaxValue - Int32.MaxValue)
            {
                TxBytes = 0;
                CountersHaveBeenReset = true;
                ++NumOfResets;
            }

            if (CountersHaveBeenReset)
            {
                Console.Title = $"{Program.ProgramName} v{Program.Version}\t (Rx: {RxBytes} | Tx: {TxBytes}) Bytes\t(Counter resets: {NumOfResets})";
            }
            else
            {
                Console.Title = $"{Program.ProgramName} v{Program.Version}\t (Rx: {RxBytes} | Tx: {TxBytes}) Bytes";
            }
        }
    }
}