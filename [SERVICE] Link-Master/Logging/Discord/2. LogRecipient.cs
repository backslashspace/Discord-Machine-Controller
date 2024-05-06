using System;
using System.Threading.Tasks;

namespace Link_Master.Logging
{
    internal static partial class Log
    {
        internal static Boolean IgnoreNewDiscord = false;

        private static async Task EnqueueDiscord(xLogMessage message, DateTime now)
        {
            if (!IgnoreNewDiscord && Client.IsConnected && CurrentConfig.LogChannel != null)
            {
                Boolean sendWarn = false;

                if (DiscordLogQueue.Count > 16)
                {
                    Link_Master.Log.FastLog("PushDiscord", "Discord queue was full (rate limit?), waiting for drain", xLogSeverity.Warning, bypassDiscord: true);

                    lock (DiscordLogQueue_LOCK)
                    {
                        DiscordLogQueue.Insert(0, new LogConsole.ConsoleMessage("Discord-Log", "To many logs were created in a short time frame, Discord couldn't keep up (rate limit?), ignoring new pushes until queue is drained", xLogSeverity.Warning, DateTime.Now));
                    }

                    IgnoreNewDiscord = true;
                    await DiscordQueueDrain();
                    IgnoreNewDiscord = false;

                    sendWarn = true;
                }

                if (sendWarn)
                {
                    Link_Master.Log.FastLog("PushDiscord", "Continuing normal operation", xLogSeverity.Info, bypassDiscord: true);
                }

                lock (DiscordLogQueue_LOCK)
                {
                    if (sendWarn)
                    {
                        DiscordLogQueue.Add(new LogConsole.ConsoleMessage("Discord-Log", "Continuing normal operation", xLogSeverity.Verbose, DateTime.Now));
                    }

                    DiscordLogQueue.Add(new LogConsole.ConsoleMessage(message.Source, message.Message, message.Severity, now));
                }
            }
        }

        private static async Task DiscordQueueDrain()
        {
            while (DiscordLogQueue.Count != 0)
            {
                await Task.Delay(256);
            }
        }
    }
}