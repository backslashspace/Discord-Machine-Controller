using System;
using System.Threading;
using System.Threading.Tasks;

namespace Link_Slave.Control
{
    internal static partial class Start
    {
        internal static void Initiate()
        {
            Log.FastLog("Initiator", "Loading config", xLogSeverity.Info);
            ConfigLoader.Load();
            Log.FastLog("Initiator", "Config successfully loaded", xLogSeverity.Info);



        }
    }
}