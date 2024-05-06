namespace Link_Slave.Control
{
    internal static partial class Start
    {
        internal static void Initiate()
        {
            Log.FastLog("Initiator", "Loading config", xLogSeverity.Info);
            ConfigLoader.Load();
            Log.FastLog("Initiator", "Config successfully loaded", xLogSeverity.Info);

            WorkerThread.Worker = new(() => Worker.Client.WorkingLoop());
            WorkerThread.Worker.Name = "Main Worker";
            WorkerThread.Worker.Start();

            Log.FastLog("Initiator", "Started main worker thread", xLogSeverity.Info);
        }
    }
}