using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Link_Master.Worker
{
    internal static partial class LinkFactory
    {
        private static (Boolean endpointIsValid, ChannelLink channelLink) GetEndpointInfo()
        {
            //receive name length in plain
            //receive id and name encrypted

            Byte[] buffer = new Byte[1];

            try
            {
                //get name length
                Int32 receivedBytes = socket.Receive(buffer, 0, 1, SocketFlags.None);

                if (receivedBytes != 1)
                {
                    Log.FastLog("Link-Factory", $"Endpoint ({(socket.RemoteEndPoint as IPEndPoint).Address}) attempted to authenticate, but failed to send name length byte, closing connection", xLogSeverity.Alert);

                    return (false, new());
                }

                //test if is in config
                List<ChannelLink> possibleLinkCandidates = new();

                foreach (ChannelLink channelLink in CurrentConfig.MachineChannelLinks.Values)
                {
                    if (channelLink.Name.Length == buffer[0])
                    {
                        possibleLinkCandidates.Add(channelLink);
                    }
                }

                Byte nameLength = buffer[0];

                if (possibleLinkCandidates.Count == 0)
                {
                    Log.FastLog("Link-Factory", $"Unknown endpoint ({(socket.RemoteEndPoint as IPEndPoint).Address}) attempted to authenticate, closing connection", xLogSeverity.Alert);

                    return (false, new());
                }

                //receive id + full name
                buffer = new Byte[308];

                if (socket.Receive(buffer, 0, 308, SocketFlags.None) != 308)
                {
                    Log.FastLog("Link-Factory", $"Endpoint ({(socket.RemoteEndPoint as IPEndPoint).Address}) attempted to authenticate, but failed during id and name transfer, closing connection", xLogSeverity.Alert);

                    return (false, new());
                }

                //try try find correct machine
                for (UInt16 i = 0; i < possibleLinkCandidates.Count; ++i)
                {
                    Byte[] hmac_Key = possibleLinkCandidates[i].HMAC_Key;
                    Byte[] aes_Key = possibleLinkCandidates[i].AES_Key;

                    buffer = AES_TCP.UnPack(ref buffer, ref aes_Key, ref hmac_Key);

                    UInt64 channelID = BitConverter.ToUInt64(buffer, 4);
                    String name = Encoding.UTF8.GetString(buffer, 4, nameLength);

                    if (possibleLinkCandidates[i].ChannelID == channelID
                        && possibleLinkCandidates[i].Name == name)
                    {
                        Log.FastLog("Link-Factory", $"Endpoint '{name}' ({(socket.RemoteEndPoint as IPEndPoint).Address}) successfully completed first authentication stage", xLogSeverity.Info);

                        ChannelLink channelLink = CurrentConfig.MachineChannelLinks[channelID];

                        return (true, channelLink);
                    }
                }

                Log.FastLog("Link-Factory", $"Endpoint ({(socket.RemoteEndPoint as IPEndPoint).Address}) attempted to authenticate, but send invalid combination of name and channel id, closing connection", xLogSeverity.Alert);

                return (false, new());
            }
            catch (Exception ex)
            {
                try
                {
                    Log.FastLog("Link-Factory", $"Endpoint ({(socket.RemoteEndPoint as IPEndPoint).Address}) attempted to authenticate, but failed with the following error message: {ex}", xLogSeverity.Error);
                }
                catch
                {
                    Log.FastLog("Link-Factory", $"An endpoint attempted to authenticate, but failed with the following error message: {ex}", xLogSeverity.Error);
                }

                return (false, new());
            }
        }

        private static Boolean EndpointHasValidGuid(ref ChannelLink channelLink)
        {
            Byte[] actualGuid = channelLink.Guid.ToByteArray();
            Byte[] receivedGuid = AES_TCP.Receive(ref socket, channelLink.AES_Key, channelLink.HMAC_Key);

            if (receivedGuid.Length != 16)
            {
                Log.FastLog("Link-Factory", $"Received invalid guid length from authenticating endpoint with name '{channelLink.Name}' ({(socket.RemoteEndPoint as IPEndPoint).Address}), received {receivedGuid.Length}, expected 16, closing connection", xLogSeverity.Alert);

                return false;
            }

            for (Byte b = 0; b < 16; ++b)
            {
                if (actualGuid[b] != receivedGuid[b])
                {
                    Log.FastLog("Link-Factory", $"Received guid did not match configured guid from authenticating endpoint with name '{channelLink.Name}' ({(socket.RemoteEndPoint as IPEndPoint).Address}), mismatch at index: {b}, closing connection", xLogSeverity.Alert);

                    return false;
                }
            }

            return true;
        }
    }
}