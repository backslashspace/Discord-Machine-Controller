using Discord;

namespace Link_Master.Control
{
    internal static partial class Boot
    {
        private static void StartLogWorker()
        {
            Log.FastLog("Initiator", $"Starting local console log server", LogSeverity.Info);
            WorkerThreads.LocalConsoleLogWorker = new(() => Logging.LogConsole.ConsoleServer(WorkerThreads.LocalConsoleLogWorker_TokenSource.Token))
            {
                Name = "Local Log TCP Server",
            };
            WorkerThreads.LocalConsoleLogWorker.Start();

            Log.FastLog("Initiator", $"Starting discord log worker", LogSeverity.Info);
            WorkerThreads.DiscordLogWorker = new(() => Logging.Log.DiscordLogWorker(WorkerThreads.DiscordLogWorker_TokenSource.Token))
            {
                Name = "Discord Log Worker",
            };
            WorkerThreads.DiscordLogWorker.Start();
        }
    }
}