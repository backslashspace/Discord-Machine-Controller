using Discord;
using System.Threading.Tasks;

namespace Link_Master.Control
{
    internal static partial class Shutdown
    {
        private static void StopLogFacility()
        {
            WorkerThreads.DiscordLogWorker_TokenSource.Cancel();
            while (WorkerThreads.DiscordLogWorker != null && WorkerThreads.DiscordLogWorker.IsAlive)
            {
                Task.Delay(64).Wait();
            }
            Log.FastLog("Shutdown", "Stopped discord log worker", LogSeverity.Info);

            WorkerThreads.LocalConsoleLogWorker_TokenSource.Cancel();
            while (WorkerThreads.LocalConsoleLogWorker != null && WorkerThreads.LocalConsoleLogWorker.IsAlive)
            {
                Task.Delay(64).Wait();
            }
            Log.FastLog("Shutdown", "Stopped local tcp log server", LogSeverity.Info);
        }
    }
}