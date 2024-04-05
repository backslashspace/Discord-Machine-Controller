namespace Link_Master.Control
{
    internal static partial class Start
    {
        internal static async void Initiate()
        {
            #region Logging
            Log.FastLog("Initiator", $"Starting local console log server", xLogSeverity.Info);
            WorkerThreads.LocalConsoleLogWorker = new(() => Logging.LogConsole.ConsoleServer());
            WorkerThreads.LocalConsoleLogWorker.Name = "Local Log TCP Server";
            WorkerThreads.LocalConsoleLogWorker.Start();

            Log.FastLog("Initiator", $"Starting discord log worker", xLogSeverity.Info);
            WorkerThreads.DiscordLogWorker = new(() => Logging.Log.DiscordLogWorker());
            WorkerThreads.DiscordLogWorker.Name = "Discord Log Worker";
            WorkerThreads.DiscordLogWorker.Start();
            #endregion

            Log.FastLog("Initiator", "Loading config", xLogSeverity.Info);
            ConfigLoader.Load();
            Log.FastLog("Initiator", "Config loaded", xLogSeverity.Info);

            Log.FastLog("Initiator", "Starting machine link factory", xLogSeverity.Info);
            WorkerThreads.LinkFactory = new(() => Worker.LinkFactory.Worker());
            WorkerThreads.LinkFactory.Name = "Link Factory";
            WorkerThreads.LinkFactory.Start();

            Log.FastLog("Initiator", "Attempting to connect to discord", xLogSeverity.Info);
            await Worker.Bot.Connect();
        }
    }
}