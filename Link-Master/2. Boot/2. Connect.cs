using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link_Master.Worker
{
    internal static class Debug
    {
        internal static void Test()
        {
            while (true)
            {
                Log.FastLog("Test", "ping", Discord.LogSeverity.Debug);

                Task.Delay(100).Wait();
            }
        }
    }
}