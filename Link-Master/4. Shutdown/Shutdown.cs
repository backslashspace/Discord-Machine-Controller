using Discord;
using System;
using System.Threading.Tasks;

namespace Link_Master.Worker.Control
{
    internal static class Shutdown
    {
        internal static void ServiceComponents(Boolean unsafeShutdown = true)
        {
            if (!unsafeShutdown)
            {
                Log.FastLog("Win32", "Service shutdown initiated", LogSeverity.Info);
            }

            Stop.LinkManager();
            Stop.Links();

            Log.FastLog("Shutdown", "Stopped all link workers", LogSeverity.Info);

            Client.IsConnected = false;
            //disconnect

            Log.FastLog("Shutdown", "Stopping log worker, terminating", LogSeverity.Info);

            Stop.DrainThen_EndLogWorker_WithTimeOut();

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

            internal static void DrainThen_EndLogWorker_WithTimeOut()
            {
                try
                {
                    Byte timeOut = 0;
                    while (timeOut < 16 && !Log.logQueue.IsEmpty)
                    {
                        Task.Delay(256).Wait();
                        ++timeOut;
                    }
                }
                catch { }

                Log.cancellationTokenSource.Cancel();

                while (WorkerThreads.LogWorker != null && WorkerThreads.LogWorker.IsAlive)
                {
                    Task.Delay(256).Wait();
                }
            }
        }
    }
}