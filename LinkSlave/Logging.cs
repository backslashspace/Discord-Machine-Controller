using System;
using System.IO;
using System.Text;
using LinkSlave;

namespace VMLink_Slave
{
    internal static class Log
    {
        internal static void Print(String message, LogSeverity severity)
        {
            try
            {
                DateTime timeStamp = DateTime.Now;

                if (!Directory.Exists($"{Client.assemblyPath}\\\\logs"))
                {
                    Directory.CreateDirectory($"{Client.assemblyPath}\\\\logs");
                }

                String logLine = "";
                UInt16 lineLength = 27;

                logLine += $"[{timeStamp:dd.MM.yyyy HH:mm:ss}] [";

                switch (severity)
                {
                    case LogSeverity.Info:
                        lineLength += 5;
                        logLine += "Info]";
                        break;
                    case LogSeverity.Debug:
                        lineLength += 6;
                        logLine += "Debug]";
                        break;
                    case LogSeverity.Warning:
                        lineLength += 8;
                        logLine += "Warning]";
                        break;
                    case LogSeverity.Verbose:
                        lineLength += 8;
                        logLine += "Verbose]";
                        break;
                    case LogSeverity.Error:
                        lineLength += 6;
                        logLine += "Error]";
                        break;
                    case LogSeverity.Critical:
                        lineLength += 9;
                        logLine += "Critical]";
                        break;
                }


                if (lineLength < 52)
                {
                    for (UInt16 i = lineLength; i < 52; ++i)
                    {
                        logLine += " ";
                    }
                }
                else
                {
                    logLine += " ";
                }

                logLine += message;

                using (StreamWriter streamWriter = new($"{Client.assemblyPath}\\logs\\{timeStamp:dd.MM.yyyy}.txt", true, Encoding.UTF8))
                {
                    streamWriter.WriteLine(logLine);
                }
            }
            catch { }
        }

        internal static void EndMe()
        {
            using (StreamWriter streamWriter = new($"{Client.assemblyPath}\\logs\\{DateTime.Now:dd.MM.yyyy}.txt", true, Encoding.UTF8))
            {
                streamWriter.WriteLine();
            }
        }
    }

    internal enum LogSeverity
    {
        Critical = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Verbose = 4,
        Debug = 5
    }
}