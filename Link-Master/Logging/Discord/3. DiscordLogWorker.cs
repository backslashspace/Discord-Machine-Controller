using Discord;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Link_Master.Logging
{
    internal static partial class Log
    {
        internal static List<LogConsole.ConsoleMessage> DiscordLogQueue = new();
        internal static readonly Object DiscordLogQueue_LOCK = new();

        //

        internal static async void DiscordLogWorker(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                while (DiscordLogQueue.Count == 0)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    await Task.Delay(128);
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

                log += $"]-[{consoleMessage.Source}]\t\t\t\t";

                log += consoleMessage.Message;

                if (log.Length > 4000)
                {
                    Link_Master.Log.FastLog("PushLogDiscord", $"Log message was too long, cutting output..", LogSeverity.Warning);

                    log = log.Substring(0, 4000);
                }

                if (consoleMessage.Severity == LogSeverity.Critical || consoleMessage.Severity == LogSeverity.Error)
                {
                    log += $"\n<@{CurrentConfig.DiscordAdminID}>";
                }

                try
                {
                    if (!Client.IsConnected)
                    {
                        continue;
                    }

                    Task task = CurrentConfig.LogChannel.SendMessageAsync(log);

                    await task;

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

                    Link_Master.Log.FastLog("Logging", $"Unable to push log to discord: {ex.Message}, {ex.InnerException}", LogSeverity.Error, bypassDiscord: true);
                }
            }
        }
    }
}