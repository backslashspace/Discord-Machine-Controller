using Discord.WebSocket;
using Discord;
using System.Threading.Tasks;

namespace Link_Master.Worker
{
    internal static partial class Bot
    {
        private static async Task Help(SocketSlashCommand command)
        {
            EmbedBuilder formattedResponse = new()
            {
                Color = Color.Blue,
                Title = $"/{command.Data.Name}"
            };
            formattedResponse.WithFooter("no");

            if (!Client.BlockNew)
            {
                await command.RespondAsync(embed: formattedResponse.Build());
            }
        }
    }
}