using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
//
using static Link_Master.Worker.Bot;

namespace Link_Master.Worker
{
    internal static partial class LinkWorker
    {
        internal static void Worker(CancellationToken cancellationToken, ChannelLink channelLink, Socket socket)
        {
            AnnounceConnect(ref channelLink);

            Command request;
            Byte[] response;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    while (ActiveMachineLinks[channelLink.ChannelID].CommandQueue.Count == 0)
                    {
                        Task.Delay(4096).Wait();

                        if (!EndpointIsAlive(ref socket, ref channelLink))
                        {
                            AnnounceDisconnect(ref channelLink, false);

                            DeregisterLink(ref socket, ref channelLink);

                            Disconnect(ref socket);

                            return;
                        }
                    }

                    lock (ActiveMachineLinks[channelLink.ChannelID].CommandQueue_Lock)
                    {
                        request = ActiveMachineLinks[channelLink.ChannelID].CommandQueue.Dequeue();
                    }

                    SendRequest(ref socket, ref request, ref channelLink);

                    ReceiveResponse(ref socket, ref channelLink, ref request, out response);

                    lock (ActiveMachineLinks[channelLink.ChannelID].ResultsQueue_Lock)
                    {
                        ActiveMachineLinks[channelLink.ChannelID].ResultsQueue.Enqueue(new(request.ID, ref response));
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is SocketException e)
                {
                    Log.FastLog("Machine-Link", $"A network error caused an exception in link worker '{Thread.CurrentThread.Name}', the error message was:\n" +
                    $"{e.Message} ({e.SocketErrorCode})\n\t=> Destroying link and freeing machine", xLogSeverity.Error);
                }
                else
                {
                    Log.FastLog("Machine-Link", $"An error occurred in link worker '{Thread.CurrentThread.Name}', the error message was:\n" +
                    $"{ex.Message}\n\nSource: {ex.Source}\n\nStackTrace: {ex.StackTrace}\n\n\t=> Destroying link and freeing machine", xLogSeverity.Error);
                }

                AnnounceDisconnect(ref channelLink, true);

                DeregisterLink(ref socket, ref channelLink);

                Disconnect(ref socket);
            }
        }
    }
}