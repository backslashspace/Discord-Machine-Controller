using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DC_SRV_VM_LINK.Bot
{
    internal static partial class VM_Manager
    {
        internal static void NewLinkManager()
        {
            try
            {
                IPAddress ipAddress = CurrentConfig.tcpListenerIP;
                IPEndPoint localEndPoint = new(ipAddress, (Int32)CurrentConfig.tcpListenerPort);

                Socket listener = new(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    listener.Bind(localEndPoint);
                }
                catch
                {
                    SRV.FastLog("Link-Manager", $"Unable to bind port [{CurrentConfig.tcpListenerPort}] to [{CurrentConfig.tcpListenerIP}], terminating in 5120ms", LogSeverity.Critical).Wait();

                    Exit.Service();
                }

                listener.Listen(12);

                while (true)
                {
                ContinueOuter:

                    Socket socket = listener.Accept();

                    socket.ReceiveTimeout = 3840;
                    socket.SendTimeout = 3840;

                    //

                    String rawChannel_Guid;
                    UInt64 channelID;
                    ChannelLink link;

                    //get endpoint auth details
                    try
                    {
                        Byte[] rawDataBufferSize = new Byte[4];
                        socket.Receive(rawDataBufferSize);
                        Int32 dataBufferSize = BitConverter.ToInt32(rawDataBufferSize, 0);

                        if (dataBufferSize > UInt16.MaxValue)
                        {
                            throw new InvalidDataException();
                        }

                        socket.Send(new Byte[] { 0b01010101 });

                        Byte[] data = new Byte[dataBufferSize];
                        socket.Receive(data);

                        rawChannel_Guid = Encoding.UTF8.GetString(data);
                    }
                    catch (Exception ex)
                    {
                        if (ex is SocketException e)
                        {
                            SRV.FastLog("Link-Manager", $"Endpoint with IP [{(socket.RemoteEndPoint as IPEndPoint).Address}] failed pre-auth step with the following error: {e.SocketErrorCode}, closing connection", LogSeverity.Error).Wait();
                        }
                        else if (ex is InvalidDataException)
                        {
                            SRV.FastLog("Link-Manager", $"Endpoint with IP [{(socket.RemoteEndPoint as IPEndPoint).Address}] send invalid data in pre-auth step (requested buffer to big), closing connection", LogSeverity.Error).Wait();
                        }
                        else
                        {
                            SRV.FastLog("Link-Manager", $"Endpoint with IP [{(socket.RemoteEndPoint as IPEndPoint).Address}] send invalid data in pre-auth step, closing connection", LogSeverity.Error).Wait();
                        }

                        socket.Close();
                        socket.Dispose();

                        goto ContinueOuter;
                    }

                    //verify client details
                    try
                    {
                        channelID = VerifyData(ref rawChannel_Guid);
                    }
                    catch (Exception ex)
                    {
                        if (ex is KeyNotFoundException e)
                        {
                            SRV.FastLog("Link-Manager", $"Endpoint with IP [{(socket.RemoteEndPoint as IPEndPoint).Address}] not registered, closing connection", LogSeverity.Warning).Wait();
                        }
                        else
                        {
                            SRV.FastLog("Link-Manager", $"Endpoint with IP [{(socket.RemoteEndPoint as IPEndPoint).Address}] send invalid data during authentication [{ex.Message}], closing connection", LogSeverity.Error).Wait();
                        }

                        socket.Close();
                        socket.Dispose();

                        goto ContinueOuter;
                    }

                    if (IsAlreadyConnected(ref socket, ref channelID))
                    {
                        socket.Disconnect(false);
                        socket.Close();

                        goto ContinueOuter;
                    }

                    //get associated config link and perform challenge 
                    try
                    {
                        link = CurrentConfig.vmChannelLink[channelID];

                        ChallengeRequest(ref socket, ref link);
                    }
                    catch (Exception ex)
                    {
                        SRV.FastLog("Link-Manager", $"Endpoint with IP [{(socket.RemoteEndPoint as IPEndPoint).Address}] failed to perform challenge request: [{ex.Message}], closing connection", LogSeverity.Error).Wait();

                        socket.Close();
                        socket.Dispose();

                        goto ContinueOuter;
                    }

                    //register and start link thread
                    VMLink.VirtualMachine newVM = new(ref channelID, (socket.RemoteEndPoint as IPEndPoint).Address);

                    if (!VMLink.linkedVMs.TryAdd(channelID, newVM))
                    {
                        try
                        {
                            SRV.FastLog("Link-Manager", $"Unable to add new VM '{CurrentConfig.vmChannelLink[channelID].Name}', shutting down link and closing socket", LogSeverity.Error).Wait();

                            if (!VMLink.linkedVMs.TryRemove(channelID, out _))
                            {
                                SRV.FastLog("Link-Manager", $"Unable to remove VM '{CurrentConfig.vmChannelLink[channelID].Name}'", LogSeverity.Error).Wait();
                            }
                        }
                        catch 
                        {
                            SRV.FastLog("Link-Manager", $"An unknown error occurred while attempting to remove '{CurrentConfig.vmChannelLink[channelID].Name}'", LogSeverity.Critical).Wait();
                        }

                        socket.Close();
                        socket.Dispose();

                        goto ContinueOuter;
                    }

                    Thread connectionHandler = new(() => VMLink.LinkWorker(socket, channelID))
                    {
                        IsBackground = true,
                        Name = $"Link-Worker:{CurrentConfig.vmChannelLink[channelID].Name}@{(socket.RemoteEndPoint as IPEndPoint).Address}"
                    };
                    connectionHandler.Start();

                    SRV.FastLog("Link-Manager", $"VM '{CurrentConfig.vmChannelLink[channelID].Name}' connected from {(socket.RemoteEndPoint as IPEndPoint).Address}", LogSeverity.Info).Wait();

                    Task.Delay(512).Wait();
                }
            }
            catch (Exception ex)
            {
                SRV.FastLog("Link-Manager", $"Unknow error in ConnectionManager, terminating in 5120ms", LogSeverity.Critical).Wait();
                SRV.FastLog("Link-Manager", $"Message: {ex.Message}\n\nSource: {ex.Source}\n\nStackTrace: {ex.StackTrace}", LogSeverity.Verbose).Wait();

                Exit.Service();
            }
        }

        //

        private static UInt64 VerifyData(ref String rawChannel_Guid)
        {
            Guid guid;
            UInt64 channelID;

            Match match = Regex.Match(rawChannel_Guid, "ID:\"(\\d+?)\"");
            if (match.Success)
            {
                channelID = UInt64.Parse(match.Groups[1].Value);
            }
            else
            {
                throw new InvalidDataException("no valid channelID found");
            }

            match = Regex.Match(rawChannel_Guid, "Guid:\"([-a-zA-Z\\d]+?)\"");
            if (match.Success)
            {
                guid = Guid.Parse(match.Groups[1].Value);
            }
            else
            {
                throw new InvalidDataException("no valid vm guid found");
            }

            if (CurrentConfig.vmChannelLink[channelID].Guid != guid)
            {
                throw new InvalidDataException("endpoint send invalid combination of channel and guid");
            }

            return channelID;
        }

        private static void ChallengeRequest(ref Socket socket, ref ChannelLink link)
        {
            Random rnd = new();
            Byte[] rawInt = new byte[8];
            rnd.NextBytes(rawInt);

            UInt64 randomInt = BitConverter.ToUInt64(rawInt, 0);

            AES_FastSocket.SendTCP(ref socket, rawInt, link.AES_Key, link.HMAC_Key);

            Byte[] response = AES_FastSocket.ReceiveTCP(ref socket, link.AES_Key, link.HMAC_Key);

            UInt64 expectedResponse;
            if (randomInt < 9446744073709551615)
            {
                if (randomInt > 65000)
                {
                    expectedResponse = randomInt + UInt32.MaxValue;
                }
                else
                {
                    expectedResponse = randomInt + Byte.MaxValue;
                }
            }
            else
            {
                if (randomInt > UInt64.MaxValue - 65000)
                {
                    expectedResponse = randomInt - Byte.MaxValue;
                }
                else
                {
                    expectedResponse = randomInt + (Int16.MaxValue - 420);
                }
            }

            if (BitConverter.ToUInt64(response, 0) != expectedResponse)
            {
                throw new InvalidDataException($"invalid challenge response, expected [{expectedResponse}] received [{BitConverter.ToUInt64(response, 0)}]");
            }
        }

        private static Boolean IsAlreadyConnected(ref Socket socket, ref UInt64 channelID)
        {
            foreach (VMLink.VirtualMachine vm in VMLink.linkedVMs.Values)
            {
                if (vm.Address.Equals((socket.RemoteEndPoint as IPEndPoint).Address))
                {
                    WarnAlreadyConnected($"with address {vm.Address}");

                    return true;
                }

                if (vm.ChannelID == channelID)
                {
                    WarnAlreadyConnected($" for channel {vm.ChannelID}");

                    return true;
                }
            }

            return false;

            static void WarnAlreadyConnected(String subject)
            {
                SRV.FastLog("Link-Manager", $"VM {subject} has already connected", LogSeverity.Warning).Wait();
            }
        }
    }
}