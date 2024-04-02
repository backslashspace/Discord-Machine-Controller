using Discord;
using System.Threading.Tasks;

namespace Link_Master.Control
{
    internal static partial class Shutdown
    {
        private static void StopLogFacility()
        {
            WorkerThreads.DiscordLogWorker_WasCanceled = true;
            while (WorkerThreads.DiscordLogWorker != null && WorkerThreads.DiscordLogWorker.IsAlive)
            {
                Task.Delay(64).Wait();
            }
            Log.FastLog("Shutdown", "Stopped discord log worker", xLogSeverity.Info);

            WorkerThreads.LocalConsoleLogWorker_WasCanceled = true;
            while (WorkerThreads.LocalConsoleLogWorker != null && WorkerThreads.LocalConsoleLogWorker.IsAlive)
            {
                Task.Delay(64).Wait();
            }
            Log.FastLog("Shutdown", "Stopped local tcp log server", xLogSeverity.Info);
        }
    }
}