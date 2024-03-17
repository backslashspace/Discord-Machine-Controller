using Discord;
using Link_Master.Worker;
using System;
using System.Threading.Tasks;

namespace Link_Master
{
    internal partial class Program
    {

        internal static void Start(String[] args)
        {
            //Task.Delay(10000).Wait();

            Log.FastLog("Win32", "Service start initiated", LogSeverity.Info);

            Worker.Control.Boot.Initiate();
        }

        internal static void Stop()
        {
            Worker.Control.Shutdown.ServiceComponents(false);
        }
    }
}