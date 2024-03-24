using Discord;
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

            Log.FastLog("Initiator", "Attempting verify discord related configs", LogSeverity.Info);

            VerifyConfig();

            Log.FastLog("Initiator", "Successfully loaded config", LogSeverity.Info);

            if (CurrentConfig.CmdRegisterOnBoot == true)
            {
                await RegisterCommands();
            }

            Log.FastLog("Initiator", "Service fully loaded", LogSeverity.Info);

            Client.WasReadyAtLeastOnce = true;
        }
    }
}