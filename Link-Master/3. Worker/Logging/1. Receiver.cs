﻿using Discord;
using System;

namespace Link_Master.Worker
{
    internal static partial class Log
    {
        internal static Boolean IgnoreNew = false;

        internal struct InternalLogMessage
        {
            internal InternalLogMessage(ref LogMessage logMessage, ref DateTime timeStamp, ref Boolean bypassIPC, ref Boolean bypassDiscord, ref Boolean bypassFile)
            {
                LogMessage = logMessage;
                TimeStamp = timeStamp;
                bypassIPCLog_Live = bypassIPC;
                BypassDiscord = bypassDiscord;
                BypassFile = bypassFile;
            }

            internal LogMessage LogMessage;
            internal DateTime TimeStamp;

            internal Boolean bypassIPCLog_Live;
            internal Boolean BypassDiscord;
            internal Boolean BypassFile;

        }

        internal static void FastLog(String category, String message, LogSeverity logSeverity, Boolean bypassConsole = false, Boolean bypassDiscord = false, Boolean bypassFile = false)
        {
            Enqueue(new LogMessage(logSeverity, category, message), bypassConsole, bypassDiscord, bypassFile);
        }

        internal static void Enqueue(LogMessage message, Boolean bypassIPC = false, Boolean bypassDiscord = false, Boolean bypassFile = false)
        {
            if (!IgnoreNew)
            {
                if (message.Message == null)
                {
                    if (message.Source == null)
                    {
                        message = new(LogSeverity.Error, "Internal", $"It appears that something created a log message that contained 'null' as message & source, this should not happen");
                    }
                    else
                    {
                        message = new(LogSeverity.Warning, message.Source, "{null}");
                    }
                }
                else if (message.Source == null)
                {
                    message = new(LogSeverity.Warning, "{null}", message.Message);
                }

                DateTime now = DateTime.Now;

                InternalLogMessage logData = new(ref message, ref now, ref bypassIPC, ref bypassDiscord, ref bypassFile);

                logQueue.Enqueue(logData);
            }
        }
    }
}