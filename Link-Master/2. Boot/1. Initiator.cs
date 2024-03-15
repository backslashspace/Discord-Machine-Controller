namespace Link_Master.Worker.Control
{
    internal static partial class Boot
    {
        internal static void Initiate()
        {
            WorkerThreads.LogWorker = new(() => Log.Worker(Log.cancellationTokenSource.Token))
            {
                Name = "Log Worker",
            };
            WorkerThreads.LogWorker.Start();

            WorkerThreads.LocalConsoleLogWorker = new(() => LogConsole.ConsoleServer(LogConsole.tokenSource.Token))
            {
                Name = "Local Log TCP Server",
            };
            WorkerThreads.LocalConsoleLogWorker.Start();

            ConfigLoader.Load();


            //connect

            ConfigLoader.PostLoad();
        }
    }
}