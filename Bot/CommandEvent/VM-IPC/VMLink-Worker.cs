using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using Discord;
using System.Collections.Concurrent;

namespace DC_SRV_VM_LINK.Bot
{
    internal static partial class VMLink
    {
        internal static ConcurrentDictionary<UInt64, VirtualMachine> linkedVMs = new();

        internal readonly struct VirtualMachine
        {
            internal VirtualMachine(ref UInt64 channelID, IPAddress iPAddress)
            {
                ChannelID = channelID;
                Address = iPAddress;

                CommandQueue = new();
                Results = new();
            }

            internal readonly UInt64 ChannelID;
            internal readonly IPAddress Address;

            internal readonly ConcurrentQueue<Command> CommandQueue;

            internal readonly ConcurrentQueue<Result> Results;
        }

        internal readonly struct Result
        {
            internal Result(Byte iD, Byte[] result)
            {
                ID = iD;
                ResultData = result;
            }

            internal readonly Byte ID;
            internal readonly Byte[] ResultData;
        }

        internal readonly struct Command
        {
            internal Command(Byte iD, CommandAction action, Byte[] data)
            {
                ID = iD;
                CommandAction = action;
                Data = data;
            }

            internal readonly Byte ID;
            internal readonly CommandAction CommandAction;
            internal readonly Byte[] Data;
        }

        internal enum CommandAction
        {
            UAliveQuestionMark = 0x01,
            YuesAmIAlive = 0x02,

            EnumScripts = 0x03,
            ExecuteScript = 0x04,
            RemoteDownload = 0x05
        }

        //

        internal static void LinkWorker(Socket socket, UInt64 channelID)
        {
            ChannelLink channelLink = CurrentConfig.vmChannelLink[channelID];

            try
            {
                while (true)
                {
                    while (linkedVMs[channelID].CommandQueue.IsEmpty)
                    {
                        Task.Delay(4096).Wait();

                        if (!EndpointIsAlive(ref socket, ref channelLink))
                        {
                            RemoveLinkFromState(ref socket, ref channelLink);

                            socket.Disconnect(false);
                            socket.Close();

                            return;
                        }
                    }

                    //

                    linkedVMs[channelID].CommandQueue.TryPeek(out Command request);
                    
                    Byte[] response;

                    try
                    {
                        AES_FastSocket.SendTCP(ref socket, new Byte[] { (Byte)request.CommandAction }, channelLink.AES_Key, channelLink.HMAC_Key);

                        if (request.CommandAction == CommandAction.RemoteDownload || request.CommandAction == CommandAction.ExecuteScript)
                        {
                            socket.ReceiveTimeout = 51200;

                            AES_FastSocket.SendTCP(ref socket, request.Data, channelLink.AES_Key, channelLink.HMAC_Key);
                        }
                        else
                        {
                            socket.ReceiveTimeout = 25600;
                        }

                        response = AES_FastSocket.ReceiveTCP(ref socket, channelLink.AES_Key, channelLink.HMAC_Key);

                        socket.ReceiveTimeout = 3840;
                    }
                    catch (Exception ex)
                    {
                        if (ex is SocketException e)
                        {
                            SRV.FastLog("VM-Link", $"Endpoint with IP [{(socket.RemoteEndPoint as IPEndPoint).Address}] failed to return response in time with the following error: {e.SocketErrorCode}, closing connection", LogSeverity.Error).Wait();

                            RemoveLinkFromState(ref socket, ref channelLink);

                            socket.Disconnect(false);
                            socket.Close();

                            return;
                        }

                        throw;
                    }

                    //
                    linkedVMs[channelID].Results.Enqueue(new Result(request.ID, response));

                    linkedVMs[channelID].CommandQueue.TryDequeue(out _);
                }
            }
            catch (Exception ex)
            {
                if (ex is SocketException)
                {
                    SRV.FastLog("VM-Link", $"Link error in '{channelLink.Name}'@{(socket.RemoteEndPoint as IPEndPoint).Address}, destroying link", LogSeverity.Error).Wait();
                }
                else if (ex is InvalidDataException e)
                {
                    SRV.FastLog("VM-Link", $"Received invalid data from '{channelLink.Name}'@{(socket.RemoteEndPoint as IPEndPoint).Address}, destroying link:\n{e.Message}", LogSeverity.Error).Wait();
                }
                else
                {
                    SRV.FastLog("VM-Link", $"Unknow link error in '{channelLink.Name}'@{(socket.RemoteEndPoint as IPEndPoint).Address}, destroying link\n\n{ex.Message}\n{ex.Source}\n{ex.StackTrace}", LogSeverity.Error).Wait();
                }

                RemoveLinkFromState(ref socket, ref channelLink);

                socket.Disconnect(false);
                socket.Close();
            }
        }

        //

        private static Boolean EndpointIsAlive(ref Socket socket, ref ChannelLink channelLink)
        {
            try
            {
                AES_FastSocket.SendTCP(ref socket, new Byte[] { (Byte)CommandAction.UAliveQuestionMark }, channelLink.AES_Key, channelLink.HMAC_Key);

                if (AES_FastSocket.ReceiveTCP(ref socket, channelLink.AES_Key, channelLink.HMAC_Key)[0] != (Byte)CommandAction.YuesAmIAlive)
                {
                    throw new InvalidDataException();
                }
            }
            catch
            {
                SRV.FastLog("VM-Link", $"VM '{channelLink.Name}' from {(socket.RemoteEndPoint as IPEndPoint).Address} has disconnected", LogSeverity.Info).Wait();

                return false;
            }

            return true;
        }

        private static void RemoveLinkFromState(ref Socket socket, ref ChannelLink channelLink)
        {
            try
            {
                if (!linkedVMs.TryRemove(channelLink.ChannelID, out _))
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                SRV.FastLog("VM-Link", $"Unable to remove VM '{channelLink.Name}' ({(socket.RemoteEndPoint as IPEndPoint).Address}) from ConcurrentDictionary", LogSeverity.Error).Wait();
                SRV.FastLog("VM-Link", $"Message: {ex.Message}\n\nSource: {ex.Source}\n\nStackTrace: {ex.StackTrace}", LogSeverity.Verbose).Wait();
            }
        }
    }
}