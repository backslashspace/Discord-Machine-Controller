using Link_Slave.Worker;
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

                Environment.Exit(1);
            }
            else
            {
                Log.FastLog("Win32", "Service shutdown initiated", xLogSeverity.Info);
            }

            WorkerThread.Worker_WasCanceled = true;

            if (MonitorWorkerExit())
            {
                Log.FastLog("Shutdown", "Unable to stop main worker thread, force exiting", xLogSeverity.Info);

                Environment.Exit(1);
            }
            else
            {
                Log.FastLog("Shutdown", "Stopped main worker thread, exiting", xLogSeverity.Info);
            }
        }

        private static Boolean MonitorWorkerExit()
        {
            for (Byte b = 0; b < 30 && WorkerThread.Worker != null && WorkerThread.Worker.IsAlive; ++b)
            {
                Task.Delay(128).Wait();
            }

            Client.socket.Close(0);

            for (Byte b = 0; b < 30 && WorkerThread.Worker != null && WorkerThread.Worker.IsAlive; ++b)
            {
                Task.Delay(128).Wait();
            }

            if (WorkerThread.Worker != null && WorkerThread.Worker.IsAlive)
            {
                return true;
            }

            return false;
        }
    }
}