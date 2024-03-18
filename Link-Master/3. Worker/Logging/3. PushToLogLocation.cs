using Discord;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Link_Master.Worker
{
    internal static partial class Log
    {
        private static void EnqueueConsole(ref LogMessage formattedLogMessage, ref DateTime timeStamp, ref Boolean bypassIPCLog_Live)
        {
            if (LogConsole.logHistory.Count < 4096)
            {
                LogConsole.logHistory.Add(new LogConsole.ConsoleMessage(formattedLogMessage.Source, formattedLogMessage.Message, formattedLogMessage.Severity, timeStamp));
            }
            else
            {
                LogConsole.logHistory.RemoveAt(0);

                LogConsole.logHistory.Add(new LogConsole.ConsoleMessage(formattedLogMessage.Source, formattedLogMessage.Message, formattedLogMessage.Severity, timeStamp));
            }

            if (LogConsole.IsInLiveLogMode && LogConsole.socket != null && !bypassIPCLog_Live)
            {
                LogConsole.liveQueue.Enqueue(new LogConsole.ConsoleMessage(formattedLogMessage.Source, formattedLogMessage.Message, formattedLogMessage.Severity, timeStamp));
            }
        }

        private static void PushFile(ref LogMessage formattedLogMessage, ref DateTime timeStamp)
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
                    case LogSeverity.Info:
                        lineLength += 4;
                        logLine += "Info";
                        break;
                    case LogSeverity.Debug:
                        lineLength += 5;
                        logLine += "Debug";
                        break;
                    case LogSeverity.Warning:
                        lineLength += 7;
                        logLine += "Warning";
                        break;
                    case LogSeverity.Verbose:
                        lineLength += 7;
                        logLine += "Verbose";
                        break;
                    case LogSeverity.Error:
                        lineLength += 5;
                        logLine += "Error";
                        break;
                    case LogSeverity.Critical:
                        lineLength += 8;
                        logLine += "Critical";
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
                FastLog("Initiator", $"Unable to access or create log directory, terminating", LogSeverity.Critical, bypassFile: true);
                FastLog("Initiator", ex.Message, LogSeverity.Verbose, bypassFile: true);

                Control.Shutdown.ServiceComponents();
            }
        }

        private static void PushDiscord(ref LogMessage formattedLogMessage, ref DateTime timeStamp)
        {
            String log;

            log = $"[{timeStamp:dd.MM.yyyy HH:mm:ss}] [";

            switch (formattedLogMessage.Severity)
            {
                case LogSeverity.Info:
                    log += "Info";
                    break;
                case LogSeverity.Debug:
                    log += "Debug";
                    break;
                case LogSeverity.Warning:
                    log += "Warning";
                    break;
                case LogSeverity.Verbose:
                    log += "Verbose";
                    break;
                case LogSeverity.Error:
                    log += "Error";
                    break;
                case LogSeverity.Critical:
                    log += "Critical";
                    break;
            }

            log += $"]-[{formattedLogMessage.Source}]\t\t\t\t";

            log += formattedLogMessage.Message;

            if (log.Length > 4000)
            {
                FastLog("PushLogDiscord", $"Log message was too long, cutting output..", LogSeverity.Warning);

                log = log.Substring(0, 4000);
            }

            if (formattedLogMessage.Severity == LogSeverity.Critical || formattedLogMessage.Severity == LogSeverity.Error)
            {
                log += $"\n<@{CurrentConfig.DiscordAdminID}>";
            }

            Task.Run(async () =>
            {
                try
                {
                    Task sendTask = CurrentConfig.LogChannel.SendMessageAsync(log);

                    await sendTask;

                    if (sendTask.Exception != null)
                    {
                        FastLog("Logging", $"Unable to send log to discord: {sendTask.Exception.Message}", LogSeverity.Error, bypassDiscord: true);
                    }
                }
                catch (Exception ex)
                {
                    if (!Client.BlockNew)
                    {
                        FastLog("Logging", $"Unable to push log to discord: {ex.Message}", LogSeverity.Error, bypassDiscord: true);
                    }
                }
            }).ConfigureAwait(false);
        }
    }
}