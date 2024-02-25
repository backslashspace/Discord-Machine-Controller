using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DC_SRV_VM_LINK.Bot
{
    internal static partial class SRV
    {
        private static async Task FormattedResponseAsync(SocketSlashCommand command, String message, [Optional] Color? embed_color)
        {
            embed_color ??= Color.Red;

            EmbedBuilder error = new()
            {
                Color = embed_color,
                Title = $"/{command.Data.Name}",
                Description = message
            };

            await command.RespondAsync(embed: error.Build());
        }

        private static async Task FormattedMessageAsync(SocketSlashCommand command, String message, [Optional] Color? embed_color)
        {
            embed_color ??= Color.Purple;

            EmbedBuilder error = new()
            {
                Color = embed_color,
                Title = $"/{command.Data.Name}",
                Description = message
            };

            await command.Channel.SendMessageAsync(embed: error.Build());
        }

        private static async Task ErrorRespondAsync(SocketSlashCommand command, String message)
        {
            EmbedBuilder error = new()
            {
                Color = Color.Red,
                Title = $"{Lang.cmd_execution_error} `/{command.CommandName}`",
                Description = message
            };

            await command.RespondAsync(embed: error.Build());
        }
    }
}