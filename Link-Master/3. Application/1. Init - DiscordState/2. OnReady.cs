using System.Threading.Tasks;

namespace Link_Master.Worker
{
    internal static partial class Bot
    {
        private static async Task OnReady()
        {
            if (Client.WasReadyAtLeastOnce)
            {
                return;
            }

            Log.FastLog("Initiator", "Attempting to verify discord related configs", xLogSeverity.Info);

            VerifyConfig();

            Log.FastLog("Initiator", "Successfully verified config", xLogSeverity.Info);

            if (CurrentConfig.CmdRegisterOnBoot == true)
            {
                await RegisterCommands();
            }

            Program.LoadTime.Stop();
            Log.FastLog("Initiator", $"Service fully loaded in {Program.LoadTime.ElapsedMilliseconds}ms", xLogSeverity.Info);
            Program.LoadTime = null;

            Client.WasReadyAtLeastOnce = true;
        }
    }
}