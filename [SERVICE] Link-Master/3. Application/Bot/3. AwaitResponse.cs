using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Link_Master.Worker
{
    internal static partial class Bot
    {
        private static void AwaitCommandProcessing(ChannelLink channelLink, Command remoteCommand, SocketSlashCommand slashCommand)
        {
            Result result;
            try
            {
                try
                {
                    result = WaitForResult(channelLink, remoteCommand);
                }
                catch (Exception ex)
                {
                    if (ex is TimeoutException ea)
                    {
                        FormattedMessageAsync(slashCommand, ea.Message, Color.Red).Wait();
                    }
                    else
                    {
                        FormattedMessageAsync(slashCommand, "Link error, endpoint disconnected", Color.Red).Wait();
                    }

                    return;
                }

                DisplayResultDiscord(channelLink, slashCommand, result);
            }
            catch (Exception ex)
            {
                Log.FastLog("Machine-Link", $"An error occurred in '{channelLink.Name}', error was: ({ex.InnerException.GetType().Name}) => {ex.InnerException.Message}", xLogSeverity.Error);
            }
        }

        //

        private static Result WaitForResult(ChannelLink channelLink, Command remoteCommand)
        {
            for (UInt16 i = 0; i < 468; ++i)
            {
                Result result;

                lock (ActiveMachineLinks[channelLink.ChannelID].ResultsQueue_Lock)
                {
                    if (ActiveMachineLinks[channelLink.ChannelID].ResultsQueue.Count != 0)
                    {
                        result = ActiveMachineLinks[channelLink.ChannelID].ResultsQueue.Peek();

                        if (result.ID == remoteCommand.ID)
                        {
                            ActiveMachineLinks[channelLink.ChannelID].ResultsQueue.Dequeue();

                            return result;
                        }
                    }
                }

                Task.Delay(512).Wait();
            }

            throw new TimeoutException("Error: did not receive result within 4 minutes");
        }
    }
}