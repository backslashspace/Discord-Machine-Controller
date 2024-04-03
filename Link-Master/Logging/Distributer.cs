using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Link_Master
{
    internal static partial class Log
    {
        internal static void FastLog(String category, String message, xLogSeverity logSeverity, Boolean bypassIPCLog_Live = false, Boolean bypassDiscord = false, Boolean bypassFile = false)
        {
            Logging.Log.Commit(new xLogMessage(logSeverity, category, message), DateTime.Now, bypassIPCLog_Live, bypassDiscord, bypassFile);
        }
    }
}

namespace Link_Master.Logging
{
    internal static partial class Log
    {
        private static readonly Object Commit_LOCK = new();

        //

        internal static async void Commit(xLogMessage message, DateTime timeStamp, Boolean bypassIPCLog_Live = false, Boolean bypassDiscord = false, Boolean bypassFile = false)
        {
            Task ipcDrain;
            Task discordDrain = Task.CompletedTask;

            NullValueHandler(ref message);

            lock (Commit_LOCK)
            {
                if (!bypassFile)
                {
                    WriteFile(ref message, ref timeStamp);
                }

                ipcDrain = LogConsole.PushIPC(message, timeStamp, bypassIPCLog_Live);

                if (!bypassDiscord)
                {
                    discordDrain = EnqueueDiscord(message, timeStamp);
                }
            }

            await ipcDrain.ConfigureAwait(false);
            await discordDrain.ConfigureAwait(false);
        }

        //

        private static void NullValueHandler(ref xLogMessage message)
        {
            if (message.Message == null)
            {
                if (message.Source == null)
                {
                    message = new(xLogSeverity.Error, "Internal", $"It appears that something created a log message that contained 'null' as message & source, this should not happen");
                }
                else
                {
                    message = new(xLogSeverity.Warning, message.Source, "{null}");
                }
            }
            else if (message.Source == null)
            {
                message = new(xLogSeverity.Warning, "{null}", message.Message);
            }
        }

        private static void WriteFile(ref xLogMessage formattedLogMessage, ref DateTime timeStamp)
        {
            try
            {
                if (!Directory.Exists($"{Program.assemblyPath}\\logs"))
                {
                    Directory.CreateDirectory($"{Program.assemblyPath}\\logs");
                }

                String logLine = "";
                UInt16 lineLength = 27;

                logLine += $"[{timeStamp:dd.MM.yyyy HH:mm:ss}] [";

                switch (formattedLogMessage.Severity)
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

                logLine += $"]-[{formattedLogMessage.Source}]";

                lineLength += (UInt16)formattedLogMessage.Source.Length;

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

                logLine += formattedLogMessage.Message;

                using StreamWriter streamWriter = new($"{Program.assemblyPath}\\logs\\{timeStamp:dd.MM.yyyy}.txt", true, Encoding.UTF8);
                streamWriter.WriteLine(logLine);
            }
            catch (Exception ex)
            {
                LogConsole.PushIPC(new(xLogSeverity.Critical, "Logging", $"Unable to write to log file, error was:\n\n{ex.Message},\n\nterminating"), DateTime.Now, false).Wait();

                Control.Shutdown.ServiceComponents();
            }
        }
    }
}