using Discord.WebSocket;
using Discord;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Link_Master.Worker
{
    internal static partial class Bot
    {
        private static async Task FormattedResponseAsync(SocketSlashCommand command, String message, [Optional] Color? embed_color)
        {
            embed_color ??= Color.Red;

            EmbedBuilder formattedResponse = new()
            {
                Color = embed_color,
                Title = $"/{command.Data.Name}",
                Description = message
            };

            if (!Client.BlockNew)
            {
                await command.RespondAsync(embed: formattedResponse.Build());
            }
        }

        private static async Task FormattedErrorRespondAsync(SocketSlashCommand command, String message)
        {
            EmbedBuilder formattedError = new()
            {
                Color = Color.Red,
                Title = $"Failed to execute `/{command.CommandName}`",
                Description = message
            };

            if (!Client.BlockNew)
            {
                await command.RespondAsync(embed: formattedError.Build());
            }
        }

        private static async Task FormattedMessageAsync(SocketSlashCommand command, String message, [Optional] Color? embed_color)
        {
            embed_color ??= Color.Purple;

            EmbedBuilder formattedMessage = new()
            {
                Color = embed_color,
                Title = $"/{command.Data.Name}",
                Description = message
            };

            if (!Client.BlockNew)
            {
                await command.Channel.SendMessageAsync(embed: formattedMessage.Build());
            }
        }
    }
}