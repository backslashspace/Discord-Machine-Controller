using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
//
using static Link_Master.Worker.Bot;

namespace Link_Master.Worker
{
    internal static partial class LinkWorker
    {
        private static Byte[] KeepAliveByte = new Byte[] { (Byte)CommandAction.UAliveQuestionMark };

        //

        private static Boolean EndpointIsAlive(ref Socket socket, ref ChannelLink channelLink)
        {
            try
            {
                AES_TCP.Send(ref socket, ref KeepAliveByte, channelLink.AES_Key, channelLink.HMAC_Key);

                if (AES_TCP.Receive(ref socket, channelLink.AES_Key, channelLink.HMAC_Key)[0] != (Byte)CommandAction.YuesAmIAlive)
                {
                    throw new InvalidDataException();
                }
            }
            catch
            {
                Log.FastLog("Machine-Link", $"Endpoint '{channelLink.Name}' ({(socket.RemoteEndPoint as IPEndPoint).Address}) has disconnected", xLogSeverity.Info);

                return false;
            }

            return true;
        }
    }
}