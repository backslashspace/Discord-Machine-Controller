using System;
using System.Net.Sockets;
using System.Security;
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
            Byte[] response = new Byte[] { 0b1010_1010 , 0b0101_0101 };

            try
            {
                AES_TCP.Send(ref socket, ref response, channelLink.AES_Key, channelLink.HMAC_Key);
                Log.FastLog("Machine-Link", $"({channelLink.Name}) ready", xLogSeverity.Info);

            OUTER:

                while (!cancellationToken.IsCancellationRequested)
                {
                    while (ActiveMachineLinks[channelLink.ChannelID].CommandQueue.Count == 0)
                    {
                        Task.Delay(2048).Wait();

                        if (cancellationToken.IsCancellationRequested)
                        {
                            goto OUTER;
                        }

                        if (!EndpointIsAlive(ref socket, ref channelLink))
                        {
                            AnnounceDisconnect(ref channelLink, false, ref cancellationToken);
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
                else if (ex is SecurityException)
                {
                    Log.FastLog("Machine-Link", $"[{Thread.CurrentThread.Name}] security violation, destroying link and freeing machine", xLogSeverity.Alert);
                }
                else
                {
                    Log.FastLog("Machine-Link", $"An error occurred in link worker '{Thread.CurrentThread.Name}', the error message was:\n" +
                    $"{ex.Message}\n\nSource: {ex.Source}\n\nStackTrace: {ex.StackTrace}\n\n\t=> Destroying link and freeing machine", xLogSeverity.Error);
                }

                if (ActiveMachineLinks[channelLink.ChannelID].WaiterThreadCount == 0)
                {
                    AnnounceDisconnect(ref channelLink, true, ref cancellationToken);
                }
                
                DeregisterLink(ref socket, ref channelLink);
                Disconnect(ref socket);

                return;
            }

            AnnounceDisconnect(ref channelLink, false, ref cancellationToken);
            DeregisterLink(ref socket, ref channelLink);
            Disconnect(ref socket);
        }
    }
}