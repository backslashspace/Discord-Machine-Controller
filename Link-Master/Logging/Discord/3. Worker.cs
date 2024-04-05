using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Link_Master.Logging
{
    internal static partial class Log
    {
        internal static List<LogConsole.ConsoleMessage> DiscordLogQueue = new();
        internal static readonly Object DiscordLogQueue_LOCK = new();

        //

        internal static void DiscordLogWorker()
        {
            while (!WorkerThreads.DiscordLogWorker_WasCanceled)
            {
                while (DiscordLogQueue.Count == 0)
                {
                    if (WorkerThreads.DiscordLogWorker_WasCanceled)
                    {
                        return;
                    }

                    Task.Delay(128).Wait();
                }

                LogConsole.ConsoleMessage consoleMessage;

                lock (DiscordLogQueue_LOCK)
                {
                    consoleMessage = DiscordLogQueue[0];
                    DiscordLogQueue.RemoveAt(0);
                }

                String log;

                log = $"[{consoleMessage.TimeStamp:dd.MM.yyyy HH:mm:ss}] [";

                switch (consoleMessage.Severity)
                {
                    case xLogSeverity.Info:
                        log += "Info";
                        break;
                    case xLogSeverity.Debug:
                        log += "Debug";
                        break;
                    case xLogSeverity.Warning:
                        log += "Warning";
                        break;
                    case xLogSeverity.Verbose:
                        if ((Boolean)CurrentConfig.GatewayDebug)
                        {
                            continue;   //will mess up logging to discord (log gets logged)
                        }
                        log += "Verbose";
                        break;
                    case xLogSeverity.Error:
                        log += "Error";
                        break;
                    case xLogSeverity.Critical:
                        log += "Critical";
                        break;
                    case xLogSeverity.Alert:
                        log += "Alert";
                        break;
                }

                log += $"]-[{consoleMessage.Source}]\t\t\t\t";

                log += consoleMessage.Message;

                if (log.Length > 4000)
                {
                    Link_Master.Log.FastLog("PushLogDiscord", $"Log message was too long, cutting output..", xLogSeverity.Warning);

                    log = log.Substring(0, 4000);
                }

                if (consoleMessage.Severity == xLogSeverity.Critical || consoleMessage.Severity == xLogSeverity.Error || consoleMessage.Severity == xLogSeverity.Alert)
                {
                    log += $"\n<@{CurrentConfig.DiscordAdminID}>";
                }

                try
                {
                    if (!Client.IsConnected || Client.BlockNew)
                    {
                        continue;
                    }

                    Task task = CurrentConfig.LogChannel.SendMessageAsync(log);

                    task.Wait();

                    if (task.Status == TaskStatus.Canceled)
                    {
                        Console.Write("");

                        return;
                    }

                    if (task.Exception != null)
                    {
                        throw task.Exception;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is TaskCanceledException)
                    {
                        continue;
                    }

                    Link_Master.Log.FastLog("Logging", $"Unable to push log to discord: ({ex.InnerException.GetType().Name}) => {ex.InnerException.Message}", xLogSeverity.Error, bypassDiscord: true);
                }
            }
        }
    }
}