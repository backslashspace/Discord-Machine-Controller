using Discord;
using System;
using System.Threading.Tasks;

namespace DC_SRV_VM_LINK.Bot
{
    internal class Exit
    {
        internal static void Service(Boolean causedByError = true)
        {
            if (!causedByError)
            {
                SRV.FastLog("Win32", "Service shutdown initiated", LogSeverity.Info).Wait();
            }

            SRV.IsConnected = false;

            try
            {
                SRV.dc_client.LogoutAsync().Wait();
                SRV.dc_client.StopAsync().Wait();
                SRV.dc_client.Dispose();
            }
            catch { }

            if (!causedByError)
            {
                SRV.FastLog("Win32", "Service shutdown completed, terminating in 5120ms", LogSeverity.Info).Wait();
            }

            SRV.EndMe();

            Task.Delay(5120).Wait();

            if (causedByError)
            {
                Environment.Exit(1);
            }
        }
    }
}