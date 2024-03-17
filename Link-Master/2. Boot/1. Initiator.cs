using Discord;

namespace Link_Master.Worker.Control
{
    internal static partial class Boot
    {
        internal static void Initiate()
        {
            WorkerThreads.LogWorker = new(() => Log.Worker(Log.tokenSource.Token))
            {
                Name = "Log Worker",
            };
            WorkerThreads.LogWorker.Start();
            Log.FastLog("Initiator", "Started log worker", LogSeverity.Info);

            WorkerThreads.LocalConsoleLogWorker = new(() => LogConsole.ConsoleServer(LogConsole.tokenSource.Token))
            {
                Name = "Local Log TCP Server",
            };
            WorkerThreads.LocalConsoleLogWorker.Start();
            Log.FastLog("Initiator", $"Started local console log server", LogSeverity.Info);

            ConfigLoader.Load();


            //connect

            //ConfigLoader.PostLoad();
        }
    }
}