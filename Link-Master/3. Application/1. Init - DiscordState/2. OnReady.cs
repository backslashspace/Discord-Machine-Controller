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

            Log.FastLog("Initiator", "Service fully loaded", xLogSeverity.Info);

            Client.WasReadyAtLeastOnce = true;
        }
    }
}