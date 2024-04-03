using System;
using System.Threading.Tasks;

namespace Link_Slave.Control
{
    internal static partial class Shutdown
    {
        internal static void ServiceComponents(Boolean unsafeShutdown = true)
        {
            if (unsafeShutdown)
            {
                Task.Delay(5120).Wait();                
            }
            else
            {
                Log.FastLog("Win32", "Service shutdown initiated", xLogSeverity.Info);
            }

            WorkerThread.Worker_WasCanceled = true;
            while (WorkerThread.Worker != null && WorkerThread.Worker.IsAlive)
            {
                Task.Delay(64).Wait();
            }
            Log.FastLog("Shutdown", "Stopped main worker thread", xLogSeverity.Info);

            if (unsafeShutdown)
            {
                Environment.Exit(1);
            }
        }
    }
}