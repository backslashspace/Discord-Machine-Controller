namespace Link_Master.Control
{
    internal static partial class Start
    {
        private static void StartLogWorker()
        {
            Log.FastLog("Initiator", $"Starting local console log server", xLogSeverity.Info);
            WorkerThreads.LocalConsoleLogWorker = new(() => Logging.LogConsole.ConsoleServer())
            {
                Name = "Local Log TCP Server",
            };
            WorkerThreads.LocalConsoleLogWorker.Start();

            Log.FastLog("Initiator", $"Starting discord log worker", xLogSeverity.Info);
            WorkerThreads.DiscordLogWorker = new(() => Logging.Log.DiscordLogWorker())
            {
                Name = "Discord Log Worker",
            };
            WorkerThreads.DiscordLogWorker.Start();
        }
    }
}