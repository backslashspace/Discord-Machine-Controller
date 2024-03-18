using Discord;
using System;
using System.Threading.Tasks;

namespace Link_Master.Worker.Control
{
    internal static class Shutdown
    {
        internal static void ServiceComponents(Boolean unsafeShutdown = true)
        {
            if (unsafeShutdown)
            {
                Task.Delay(5120).Wait();
            }
            else
            {
                Log.FastLog("Win32", "Service shutdown initiated", LogSeverity.Info);
            }

            Stop.LinkManager();
            Log.FastLog("Shutdown", "Stopped new link manager", LogSeverity.Info);

            Stop.Links();
            Log.FastLog("Shutdown", "Stopped all link workers", LogSeverity.Info);

            Log.FastLog("Shutdown", "Disconnecting from discord", LogSeverity.Info);
            Client.IsConnected = false;
            Bot.Disconnect();

            Log.FastLog("Shutdown", "Stopping console log worker", LogSeverity.Info);
            Stop.StopConsoleLogWorker();
            Log.FastLog("Shutdown", "Stopped console log worker", LogSeverity.Info);

            Log.FastLog("Shutdown", "Stopping log dispatcher and terminating", LogSeverity.Info);
            Stop.StopLogWorker();

            if (unsafeShutdown)
            {
                Environment.Exit(1);
            }
        }

        //

        private static class Stop
        {
            internal static void LinkManager()
            {
                WorkerThreads.LinkManager?.Interrupt();
                while (WorkerThreads.LinkManager != null && WorkerThreads.LinkManager.IsAlive)
                {
                    Task.Delay(128).Wait();
                }
            }

            internal static void Links()
            {
                if (WorkerThreads.Links != null)
                {
                    for (UInt16 i = 0; i < WorkerThreads.Links.Count; ++i)
                    {
                        WorkerThreads.Links[i]?.Interrupt();
                        while (WorkerThreads.Links[i] != null && WorkerThreads.Links[i].IsAlive)
                        {
                            Task.Delay(128).Wait();
                        }
                    }
                }
            }

            internal static void StopConsoleLogWorker()
            {
                LogConsole.tokenSource.Cancel();

                while (WorkerThreads.LocalConsoleLogWorker != null && WorkerThreads.LocalConsoleLogWorker.IsAlive)
                {
                    Task.Delay(256).Wait();
                }
            }

            internal static void StopLogWorker()
            {
                Log.IgnoreNew = true;

                while (!Log.logQueue.IsEmpty)
                {
                    Task.Delay(128).Wait();
                }

                Log.tokenSource.Cancel();

                while (WorkerThreads.LogWorker != null && WorkerThreads.LogWorker.IsAlive)
                {
                    Task.Delay(256).Wait();
                }
            }
        }
    }
}