using Discord;
using System;
using System.Threading.Tasks;

namespace Link_Master.Logging
{
    internal static partial class LogConsole
    {
        private static Boolean IPCIgnoreNew = false;

        internal static async Task PushIPC(LogMessage message, DateTime now, Boolean bypassIPCLog_Live)
        {
            lock (logHistory_LOCK)
            {
                if (logHistory.Count > 4095)
                {
                    logHistory.RemoveAt(0);
                }

                logHistory.Add(new ConsoleMessage(message.Source, message.Message, message.Severity, now));
            }

            if (!IPCIgnoreNew && IsInLiveLogMode && socket != null && !bypassIPCLog_Live)
            {
                Boolean sendWarn = false;

                if (liveQueue.Count > 23)
                {
                    Link_Master.Log.FastLog("IPCPush", "Local log queue couldn't keep up, waiting for drain", LogSeverity.Warning, bypassDiscord: true, bypassIPCLog_Live: true);

                    lock (liveQueue_LOCK)
                    {
                        liveQueue.Insert(0, new LogConsole.ConsoleMessage("Console-Server", "To many logs were created in a short time frame, Console-Client couldn't keep up, ignoring new pushes until queue is drained", LogSeverity.Warning, DateTime.Now));
                    }

                    IPCIgnoreNew = true;
                    await IPCQueueDrain();
                    IPCIgnoreNew = false;

                    sendWarn = true;
                }

                if (sendWarn)
                {
                    Link_Master.Log.FastLog("IPCPush", "Continuing normal operation", LogSeverity.Info, bypassDiscord: true, bypassIPCLog_Live: true);
                }

                lock (liveQueue_LOCK)
                {
                    if (sendWarn)
                    {
                        liveQueue.Add(new ConsoleMessage("Console-Server", "Continuing normal operation", LogSeverity.Info, DateTime.Now));
                    }

                    liveQueue.Add(new ConsoleMessage(message.Source, message.Message, message.Severity, now));
                }
            }
        }

        //

        private static async Task IPCQueueDrain()
        {
            while (liveQueue.Count != 0)
            {
                await Task.Delay(256);
            }
        }
    }
}