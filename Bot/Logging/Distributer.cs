using Discord;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DC_SRV_VM_LINK.Bot
{
    internal static partial class SRV
    {
        private static async Task DistributeLog(LogMessage formattedLogMessage, Boolean bypassClientQueue = false)
        {
            DateTime timeStamp = DateTime.Now;

            PushIPC(ref formattedLogMessage, ref timeStamp, ref bypassClientQueue);

            PushFile(ref formattedLogMessage, ref timeStamp);

            if (CurrentConfig.logChannel != null && IsConnected)
            {
                await PushDiscord(formattedLogMessage, timeStamp);
            }
        }

        private static void PushIPC(ref LogMessage formattedLogMessage, ref DateTime timeStamp, ref Boolean bypassClientQueue)
        {
            if (IPCAdapter.latestLog.Count < 4096)
            {
                IPCAdapter.latestLog.Add(new IPCData.ExtendedLogMessage(formattedLogMessage.Source, formattedLogMessage.Message, formattedLogMessage.Severity, timeStamp));
            }
            else
            {
                IPCAdapter.latestLog.RemoveAt(0);

                IPCAdapter.latestLog.Add(new IPCData.ExtendedLogMessage(formattedLogMessage.Source, formattedLogMessage.Message, formattedLogMessage.Severity, timeStamp));
            }

            if (IPCAdapter.pipeServer != null && !bypassClientQueue)
            {
                if (IPCAdapter.pipeServer.IsConnected)
                {
                    IPCAdapter.queue.Enqueue(new IPCData.ExtendedLogMessage(formattedLogMessage.Source, formattedLogMessage.Message, formattedLogMessage.Severity, timeStamp));
                }
            }
        }

        private static readonly Object fileLock = new();
        private static void PushFile(ref LogMessage formattedLogMessage, ref DateTime timeStamp)
        {
            try
            {
                if (!Directory.Exists($"{Service.Program.assemblyPath}\\\\logs"))
                {
                    Directory.CreateDirectory($"{Service.Program.assemblyPath}\\\\logs");
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

                lock (fileLock)
                {
                    using (StreamWriter streamWriter = new($"{Service.Program.assemblyPath}\\logs\\{timeStamp:dd.MM.yyyy}.txt", true, Encoding.UTF8))
                    {
                        streamWriter.WriteLine(logLine);
                    }
                }
            }
            catch (Exception ex)
            {
                SRV.FastLog("Initiator", $"{Lang.log_critical_cannot_create_or_access_log_dir} 5120ms", LogSeverity.Critical).Wait();
                SRV.FastLog("Initiator", ex.Message, LogSeverity.Verbose).Wait();

                Exit.Service();
            }
        }
        internal static void EndMe()
        {
            using (StreamWriter streamWriter = new($"{Service.Program.assemblyPath}\\logs\\{DateTime.Now:dd.MM.yyyy}.txt", true, Encoding.UTF8))
            {
                streamWriter.WriteLine();
            }
        }

        private static async Task PushDiscord(LogMessage formattedLogMessage, DateTime timeStamp)
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

            if (formattedLogMessage.Severity == LogSeverity.Critical || formattedLogMessage.Severity == LogSeverity.Error)
            {
                log += $"\n<@{CurrentConfig.discordAdminID}>";
            }

            await CurrentConfig.logChannel.SendMessageAsync(log);
        }
    }
}