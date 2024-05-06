using System;
using System.Threading.Tasks;

namespace Link_Master.Control
{
    internal static partial class Shutdown
    {
        internal static void ServiceComponents(Boolean unsafeShutdown = true)
        {
            if (unsafeShutdown)
            {
                Task.Delay(5120).Wait();

                Client.BlockNew = true;

                DisconnectDiscord(ref unsafeShutdown);

                StopLogFacility();

                Environment.Exit(1);
            }
            else
            {
                Log.FastLog("Win32", "Service shutdown initiated", xLogSeverity.Info);
            }

            StopLinkWorker();   //disconnect message on discord may not be reliably send (discord rate limit / internal timeouts)

            DisconnectDiscord(ref unsafeShutdown);

            StopLogFacility();
        }

        private static void StopLinkWorker()
        {
            WorkerThreads.LinkFactory_WasCanceled = true;
            while (WorkerThreads.LinkFactory != null && WorkerThreads.LinkFactory.IsAlive)
            {
                Task.Delay(64).Wait();
            }
            Log.FastLog("Shutdown", "Stopped new link factory", xLogSeverity.Info);

            Log.FastLog("Shutdown", "Stopping link workers", xLogSeverity.Info);

            foreach (Link link in WorkerThreads.Links.Values)
            {
                link.CancelToken.Cancel();
            }

            Boolean noLoop = false;

            do
            {
                for (Byte timeout = 0; WorkerThreads.Links.Count != 0 && timeout < 50; ++timeout)
                {
                    Task.Delay(64).Wait();
                }

                if (WorkerThreads.Links.Count == 0)
                {
                    break;
                }

                if (noLoop)
                {
                    Log.FastLog("Shutdown", "Second timeout reached - force exiting service..", xLogSeverity.Info);

                    Environment.Exit(1);
                }

                Log.FastLog("Shutdown", "Timeout reached - force closing sockets", xLogSeverity.Info);

                noLoop = true;

                foreach (Link link in WorkerThreads.Links.Values)
                {
                    try
                    {
                        link.Socket.Close(0);
                    }
                    catch { }
                }
            }
            while (WorkerThreads.Links.Count != 0);
            
            Log.FastLog("Shutdown", "Stopped all link workers", xLogSeverity.Info);
        }
    }
}