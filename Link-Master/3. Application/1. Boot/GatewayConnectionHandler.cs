using System;
using System.Threading.Tasks;

namespace Link_Master.Worker
{
    internal static partial class Bot
    {
        private static Task GatewayDisconnected(Exception exception)
        {
            Client.IsConnected = false;

            return Task.CompletedTask;
        }

        private static Task GatewayConnected()
        {
            Client.IsConnected = true;

            return Task.CompletedTask;
        }
    }
}