using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
//
using static Link_Master.Worker.Bot;

namespace Link_Master.Worker
{
    internal static partial class LinkFactory
    {
        private static (Boolean endpointIsValid, ChannelLink channelLink, Machine machine) AuthenticateEndpoint()
        {
            Byte[] buffer = new Byte[1];

            //socket.SendTimeout = 0;
            //socket.ReceiveTimeout = 0;

            try
            {
                (Byte nameLength, List<ChannelLink> possibleLinkCandidates) = ReceiveNameLength(ref buffer);

                ChannelLink channelLink = ReceiveIDName(ref buffer, ref nameLength, ref possibleLinkCandidates);

                ValidateGuid(ref channelLink);

                xVersion endpointVersion = VersionExchange(ref buffer, ref channelLink);

                Log.FastLog("Link-Factory", $"Endpoint '{CurrentConfig.MachineChannelLinks[channelLink.ChannelID].Name}' ({(socket.RemoteEndPoint as IPEndPoint).Address}) successfully authenticated (client v{endpointVersion})", xLogSeverity.Info);

                Machine machine = new(channelLink.ChannelID, (socket.RemoteEndPoint as IPEndPoint).Address, ref endpointVersion);

                return (true, channelLink, machine);
            }
            catch (Exception ex)
            {
                if (ex is AccessViolationException)
                {
                    return (false, new(), null);
                }
                else if (ex is SocketException ea)
                {
                    try
                    {
                        Log.FastLog("Link-Factory", $"A network error occurred during authentication from {(socket.RemoteEndPoint as IPEndPoint).Address}, error code was: {ea.SocketErrorCode}", xLogSeverity.Alert);

                        return (false, new(), null);
                    }
                    catch { }
                }

                Log.FastLog("Link-Factory", $"An endpoint attempted to authenticate, but failed with the following error message: {ex.Message}", xLogSeverity.Error);

                return (false, new(), null);
            }
        }

        private static (Byte nameLength, List<ChannelLink> possibleLinkCandidates) ReceiveNameLength(ref Byte[] buffer)
        {
            if (socket.Receive(buffer, 0, 1, SocketFlags.None) != 1)
            {
                Log.FastLog("Link-Factory", $"{(socket.RemoteEndPoint as IPEndPoint).Address} send invalid data during authentication step 1.: [name length], closing connection", xLogSeverity.Alert);

                throw new AccessViolationException();
            }

            //test if in config
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
                Log.FastLog("Link-Factory", $"{(socket.RemoteEndPoint as IPEndPoint).Address}) failed authentication (unknown endpoint reference) , closing connection", xLogSeverity.Alert);

                throw new AccessViolationException();
            }

            return (nameLength, possibleLinkCandidates);
        }

        private static ChannelLink ReceiveIDName(ref byte[] buffer, ref Byte nameLength, ref List<ChannelLink> possibleLinkCandidates)
        {
            xSocket.TCP_Receive(ref socket, out buffer);

            if (buffer.Length != 392)
            {
                Log.FastLog("Link-Factory", $"{(socket.RemoteEndPoint as IPEndPoint).Address} send invalid data during authentication step 2.: [id + name (encrypted)], invalid data length, closing connection", xLogSeverity.Alert);

                throw new AccessViolationException();
            }

            //try try find correct machine
            for (UInt16 i = 0; i < possibleLinkCandidates.Count; ++i)
            {
                Byte[] hmac_Key = possibleLinkCandidates[i].HMAC_Key;
                Byte[] aes_Key = possibleLinkCandidates[i].AES_Key;

                buffer = AES_TCP.UnPack(ref buffer, ref aes_Key, ref hmac_Key);

                UInt64 channelID = BitConverter.ToUInt64(buffer, 0);
                String name = Encoding.UTF8.GetString(buffer, 8, nameLength);

                if (possibleLinkCandidates[i].ChannelID == channelID
                    && possibleLinkCandidates[i].Name == name)
                {
                    return CurrentConfig.MachineChannelLinks[channelID];
                }
            }

            Log.FastLog("Link-Factory", $"Authenticating endpoint {(socket.RemoteEndPoint as IPEndPoint).Address} send invalid combination of name and channel id during authentication step 2.: [id + name (encrypted)], closing connection", xLogSeverity.Alert);

            throw new AccessViolationException();
        }

        private static void ValidateGuid(ref ChannelLink channelLink)
        {
            Byte[] actualGuid = channelLink.Guid.ToByteArray();
            Byte[] receivedGuid = AES_TCP.Receive(ref socket, channelLink.AES_Key, channelLink.HMAC_Key);

            if (receivedGuid.Length != 16)
            {
                Log.FastLog("Link-Factory", $"Authenticating endpoint '{channelLink.Name}' ({(socket.RemoteEndPoint as IPEndPoint).Address}) send invalid data during authentication step 3.: [guid], invalid data length, closing connection  ({(socket.RemoteEndPoint as IPEndPoint).Address}), received {receivedGuid.Length}, expected 16, closing connection", xLogSeverity.Alert);

                throw new AccessViolationException();
            }

            for (Byte b = 0; b < 16; ++b)
            {
                if (actualGuid[b] != receivedGuid[b])
                {
                    Log.FastLog("Link-Factory", $"Authenticating endpoint '{channelLink.Name}' ({(socket.RemoteEndPoint as IPEndPoint).Address}) send invalid data during authentication step 3.: [guid], guid data mismatch at index: {b}, closing connection", xLogSeverity.Alert);

                    throw new AccessViolationException();
                }
            }
        }

        private static xVersion VersionExchange(ref byte[] buffer, ref ChannelLink channelLink)
        {
            buffer = xVersion.GetBytes(ref Program.Version);

            try
            {
                AES_TCP.Send(ref socket, ref buffer, channelLink.AES_Key, channelLink.HMAC_Key);
            }
            catch (Exception ex)
            {
                Log.FastLog("Link-Factory", $"Authenticating endpoint '{channelLink.Name}' ({(socket.RemoteEndPoint as IPEndPoint).Address}), failed to receive server version info with the following error message: {ex.Message}, closing connection", xLogSeverity.Warning);

                throw new AccessViolationException();
            }

            Byte[] rawEndpointXVersion;
            xVersion version;

            try
            {
                rawEndpointXVersion = AES_TCP.Receive(ref socket, channelLink.AES_Key, channelLink.HMAC_Key);

                version = xVersion.GetXVersion(ref rawEndpointXVersion);
            }
            catch (Exception ex)
            {
                Log.FastLog("Link-Factory", $"Authenticating endpoint '{channelLink.Name}' ({(socket.RemoteEndPoint as IPEndPoint).Address}), failed to send version info with the following error message: {ex.Message}, closing connection", xLogSeverity.Error);

                throw new AccessViolationException();
            }

            return version;
        }
    }
}