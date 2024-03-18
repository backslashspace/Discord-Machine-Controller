using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link_Master.Worker
{
    internal static partial class Bot
    {
        private static async Task SlashCommandHandler(SocketSlashCommand command)
        {
            //throw new NotImplementedException();
        }

        private static async Task OnReady()
        {
            //throw new NotImplementedException();
        }

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