using Discord.WebSocket;
using Discord;
using System.Threading.Tasks;

namespace Link_Master.Worker
{
    internal static partial class Bot
    {
        private static async Task Debug(SocketSlashCommand command)
        {
            if (command.User.Id != CurrentConfig.DiscordAdminID)
            {
                await FormattedResponseAsync(command, ResponseStrings.No_Permission);
                return;
            }
            try
            {
                await FormattedResponseAsync(command, "Successfully enqueued request (not)", Color.Green);
            }
            catch { }
        }
    }
}