using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Link_Master.Worker
{
    internal static partial class Bot
    {
        private static async Task<Command> TryEnqueue(ChannelLink channelLink, CommandAction action, SocketSlashCommand command, Byte[] optionalCommandData = null)
        {
            Byte newCommandID;
            String errorString = null;

            lock (ActiveMachineLinks[channelLink.ChannelID].CommandQueue_Lock)
            {
                try
                {
                    newCommandID = (Byte)ActiveMachineLinks[channelLink.ChannelID].CommandQueue.Count;
                }
                catch
                {
                    errorString = "Unable to access command queue for link, endpoint not connected? | too many items in command queue for link (queue > 255)";
                    goto ERROR;
                }

                Command remoteCommand = new((Byte)(newCommandID + 1), action, optionalCommandData);

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