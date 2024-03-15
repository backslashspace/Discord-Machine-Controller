using Discord;
using System.Threading.Tasks;

namespace Link_Master.Worker
{
    internal static partial class Log
    {
        private static Task DCLogHandler(LogMessage formattedLogMessage)
        {
            Enqueue(formattedLogMessage);

            return Task.CompletedTask;
        }
    }
}