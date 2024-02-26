using Discord;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DC_SRV_VM_LINK.Bot
{
    internal static partial class SRV
    {
        private static Boolean IsInitialized = false;

        private static async Task OnReady()
        {
            if (IsInitialized)
            {
                return;
            }

            await FastLog("Initiator", "Loading post-connect config", LogSeverity.Info);

            ConfigLoader.PostLoad();

            if (CurrentConfig.cmdRegisterOnBoot == true)
            {
                await RegisterCommands();
            }

            Thread connectionHandler = new(() => VM_Manager.NewLinkManager())
            {
                IsBackground = true,
                Name = "VM - ConnectionManager"
            };
            connectionHandler.Start();

            await FastLog("Initiator", "Started virtual machine connection handler", LogSeverity.Info);

            await FastLog("Initiator", "Service fully loaded", LogSeverity.Info);

            IsInitialized = true;

            return;
        }
    }
}