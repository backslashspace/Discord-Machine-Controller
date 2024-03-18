using Discord;
using System.Threading.Tasks;

namespace Link_Master.Worker
{
    internal static partial class Bot
    {
        private static Task DCLogHandler(LogMessage formattedLogMessage)
        {
            Log.Enqueue(formattedLogMessage);

            return Task.CompletedTask;
        }
    }
}