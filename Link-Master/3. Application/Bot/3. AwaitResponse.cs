using Discord.WebSocket;
using Discord;
using System;
using System.Threading.Tasks;

namespace Link_Master.Worker
{
    internal static partial class Bot
    {
        private static async void AwaitCommandProcessing(ChannelLink channelLink, Command remoteCommand, SocketSlashCommand slashCommand)
        {
            Task<Result> endpointResult = WaitForResult(channelLink, remoteCommand);

            await endpointResult;

            if (endpointResult.Exception != null)
            {
                if (endpointResult.Exception.InnerException is TimeoutException ex)
                {
                    await FormattedMessageAsync(slashCommand, ex.Message, Color.Red);
                }
                else
                {
                    await FormattedMessageAsync(slashCommand, "Link error, endpoint disconnected", Color.Red);
                }

                return;
            }

            DisplayResultDiscord(channelLink, slashCommand, endpointResult.Result);
        }

        //

        private static async Task<Result> WaitForResult(ChannelLink channelLink, Command remoteCommand)
        {
            for (UInt16 i = 0; i < 468; ++i)
            {
                Result result;

                lock (ActiveMachineLinks[channelLink.ChannelID].ResultsQueue_Lock)
                {
                    result = ActiveMachineLinks[channelLink.ChannelID].ResultsQueue.Peek();

                    if (result.ID == remoteCommand.ID)
                    {
                        ActiveMachineLinks[channelLink.ChannelID].ResultsQueue.Dequeue();

                        return result;
                    }
                }

                await Task.Delay(512).ConfigureAwait(false);
            }

            throw new TimeoutException("Error: did not receive result within 4 minutes");
        }
    }
}