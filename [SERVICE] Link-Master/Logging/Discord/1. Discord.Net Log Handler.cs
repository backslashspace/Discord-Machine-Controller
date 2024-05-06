using Discord;
using System;
using System.Threading.Tasks;

namespace Link_Master.Logging
{
    internal static partial class Log
    {
        internal static Task DiscordLogHandler(LogMessage logMessage)
        {
            xLogMessage xLogMessage = new((xLogSeverity)logMessage.Severity, logMessage.Source, logMessage.Message, logMessage.Exception);

            Commit(xLogMessage, DateTime.Now);

            return Task.CompletedTask;
        }
    }
}