using Discord;
using System;
using System.Threading.Tasks;

namespace Link_Master.Control
{
    internal static partial class Shutdown
    {
        internal static void ServiceComponents(Boolean unsafeShutdown = true)
        {
            Client.BlockNew = true;

            if (unsafeShutdown)
            {
                Task.Delay(5120).Wait();

            }
            else
            {
                Log.FastLog("Win32", "Service shutdown initiated", xLogSeverity.Info);
            }

            StopLinkWorker();

            DisconnectDiscord(ref unsafeShutdown);

            StopLogFacility();

            if (unsafeShutdown)
            {
                Environment.Exit(1);
            }
        }

        private static void StopLinkWorker()
        {
            WorkerThreads.LinkFactory_WasCanceled = true;
            while (WorkerThreads.LinkFactory != null && WorkerThreads.LinkFactory.IsAlive)
            {
                Task.Delay(64).Wait();
            }
            Log.FastLog("Shutdown", "Stopped new link factory", xLogSeverity.Info);

            if (WorkerThreads.Links != null)
            {
                for (UInt16 i = 0; i < WorkerThreads.Links.Count; ++i)
                {
                    WorkerThreads.Links[i].CancelToken.Cancel();

                    while (WorkerThreads.Links[i].Worker != null && WorkerThreads.Links[i].Worker.IsAlive)
                    {
                        Task.Delay(64).Wait();
                    }
                }
            }
            Log.FastLog("Shutdown", "Stopped all link workers", xLogSeverity.Info);
        }
    }
}