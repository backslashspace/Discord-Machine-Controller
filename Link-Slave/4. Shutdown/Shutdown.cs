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

            //

            if (unsafeShutdown)
            {
                Environment.Exit(1);
            }
        }
    }
}