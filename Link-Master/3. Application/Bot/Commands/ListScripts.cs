using Discord.WebSocket;
using Discord;
using System.Threading.Tasks;
using System.Threading;

namespace Link_Master.Worker
{
    internal static partial class Bot
    {
        private static async Task ListScripts(ChannelLink channelLink, SocketSlashCommand slashCommand)
        {
            Command remoteCommand = await TryEnqueue(channelLink, CommandAction.EnumScripts, slashCommand);

            await FormattedResponseAsync(slashCommand, "Successfully enqueued request", Color.Green);

            Thread thread = new(() => AwaitCommandProcessing(channelLink, remoteCommand, slashCommand))
            {
                Name = $"Endpoint response awaiter ID: {remoteCommand.ID}"
            };

            thread.Start();
        }
    }
}