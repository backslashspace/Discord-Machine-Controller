using System;
using System.IO;
using System.Text;

namespace Link_Slave
{
    internal static class Log
    {
        private static readonly Object LOCK = new();

        internal static void FastLog(String source, String message, xLogSeverity severity)
        {
            DateTime timeStamp = DateTime.Now;

            try
            {
                if (!Directory.Exists($"{Program.assemblyPath}\\logs"))
                {
                    Directory.CreateDirectory($"{Program.assemblyPath}\\logs");
                }

                String logLine = "";
                UInt16 lineLength = 27;

                logLine += $"[{timeStamp:dd.MM.yyyy HH:mm:ss}] [";

                switch (severity)
                {
                    case xLogSeverity.Info:
                        lineLength += 4;
                        logLine += "Info";
                        break;
                    case xLogSeverity.Debug:
                        lineLength += 5;
                        logLine += "Debug";
                        break;
                    case xLogSeverity.Warning:
                        lineLength += 7;
                        logLine += "Warning";
                        break;
                    case xLogSeverity.Verbose:
                        lineLength += 7;
                        logLine += "Verbose";
                        break;
                    case xLogSeverity.Error:
                        lineLength += 5;
                        logLine += "Error";
                        break;
                    case xLogSeverity.Critical:
                        lineLength += 8;
                        logLine += "Critical";
                        break;
                    case xLogSeverity.Alert:
                        lineLength += 5;
                        logLine += "Alert";
                        break;
                }

                logLine += $"]-[{source}]";

                lineLength += (UInt16)source.Length;

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

                lock (LOCK)
                {
                    using StreamWriter streamWriter = new($"{Program.assemblyPath}\\logs\\{timeStamp:dd.MM.yyyy}.txt", true, Encoding.UTF8);
                    streamWriter.WriteLine(logLine);
                }
            }
            catch
            {
                Control.Shutdown.ServiceComponents();
            }
        }
    }
}