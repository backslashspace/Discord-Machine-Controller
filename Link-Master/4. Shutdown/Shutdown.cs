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

                Log.FastLog("Win32", "Service shutdown initiated", LogSeverity.Info);
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
            WorkerThreads.LinkManager?.Abort();
            while (WorkerThreads.LinkManager != null && WorkerThreads.LinkManager.IsAlive)
            {
                Task.Delay(64).Wait();
            }
            Log.FastLog("Shutdown", "Stopped new link manager", LogSeverity.Info);

            if (WorkerThreads.Links != null)
            {
                for (UInt16 i = 0; i < WorkerThreads.Links.Count; ++i)
                {
                    WorkerThreads.Links[i]?.Abort();

                    while (WorkerThreads.Links[i] != null && WorkerThreads.Links[i].IsAlive)
                    {
                        Task.Delay(64).Wait();
                    }
                }
            }
            Log.FastLog("Shutdown", "Stopped all link workers", LogSeverity.Info);
        }
    }
}