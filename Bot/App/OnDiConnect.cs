using System;
using System.Threading.Tasks;

namespace DC_SRV_VM_LINK.Bot
{
    internal static partial class SRV
    {
        private static Task GatewayConnected()
        {
            SetConnectionState(true);

            return Task.CompletedTask;
        }

        private static Task GatewayDisconnected(Exception exception)
        {
            SetConnectionState(false);

            return Task.CompletedTask;
        }

        private static void SetConnectionState(Boolean state)
        {
            IsConnected = state;
        }
    }
}