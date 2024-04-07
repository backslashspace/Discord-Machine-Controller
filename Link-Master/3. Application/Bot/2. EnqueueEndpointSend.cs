using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Link_Master.Worker
{
    internal static partial class Bot
    {
        private static async Task<Command> TryEnqueue(ChannelLink channelLink, CommandAction action, SocketSlashCommand command, Byte[] optionalCommandData = null)
        {
            Byte newCommandID = 0;
            String errorString = null;

            lock (ActiveMachineLinks[channelLink.ChannelID].CommandQueue_Lock)
            {
                try
                {
                RESTART:

                    foreach (Command queuedCommand in ActiveMachineLinks[channelLink.ChannelID].CommandQueue)
                    {
                        if (queuedCommand.ID == newCommandID)
                        {
                            ++newCommandID;

                            goto RESTART;
                        }
                    }
                }
                catch
                {
                    errorString = "Unable to access command queue for link, endpoint not connected? | too many items in command queue for link (queue > 255)";
                    goto ERROR;
                }

                Command remoteCommand = new((Byte)(newCommandID), action, optionalCommandData);

                try
                {
                    ActiveMachineLinks[channelLink.ChannelID].CommandQueue.Enqueue(remoteCommand);  //gets send by LinkWorker thread

                    ++ActiveMachineLinks[channelLink.ChannelID].WaiterThreadCount;
                }
                catch
                {
                    errorString = "Unable to enqueue request, link down?";
                    goto ERROR;
                }

                return remoteCommand;
            }

        ERROR:
            if (!Client.BlockNew)
            {
                await FormattedErrorRespondAsync(command, errorString);
            }   

            throw new InvalidOperationException();
        }
    }
}