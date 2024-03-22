using Discord;
using System;
using System.Threading.Tasks;

namespace Link_Master.Logging
{
    internal static partial class Log
    {
        internal static Task DiscordLogHandler(LogMessage formattedLogMessage)
        {
            Commit(formattedLogMessage, DateTime.Now);

            return Task.CompletedTask;
        }
    }
}