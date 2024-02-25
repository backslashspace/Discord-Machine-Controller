using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;

namespace DC_SRV_VM_LINK.Bot
{
    internal static partial class IPCAdapter
    {
        internal static List<IPCData.ExtendedLogMessage> latestLog = new();
        internal static ConcurrentQueue<IPCData.ExtendedLogMessage> queue = new();
        internal static NamedPipeServerStream pipeServer;

        internal static async void LogClientHandler(String pipeUser)
        {
            queue = new ConcurrentQueue<IPCData.ExtendedLogMessage>();

            await SRV.FastLog("Initiator", $"Started client log dispatcher thread", LogSeverity.Info);

            PipeSecurity pipeSecurity = new();
            pipeSecurity.SetAccessRule(new PipeAccessRule(new NTAccount(Environment.MachineName, pipeUser), PipeAccessRights.ReadWrite, AccessControlType.Allow));

            pipeServer = new("90436256a2031114e5ebf0a311c95dacbdabb851", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 0, 0, pipeSecurity);

            while (true)
            {
                try
                {
                Reconnect:

                    pipeServer.WaitForConnection();

                    await SRV.FastLog("IPC-Console", "Client connected", LogSeverity.Info, true);

                    while (true)
                    {
                        switch (pipeServer.ReadByte())
                        {
                            case -1:
                                await ResetPipe("Client disconnected", LogSeverity.Info);
                                goto Reconnect;

                            case (Int32)RequestType.SendPastLog:
                                SendPastLog();
                                break;

                            case (Int32)RequestType.SendUpdates:
                                SendUpdates();
                                break;

                            default:
                                await ResetPipe("Unknown client request", LogSeverity.Error);
                                goto Reconnect;
                        }
                    }
                }
                catch (Exception e)
                {
                    await ResetPipe("Pipe is broken" + e.Message, LogSeverity.Error);

                    Task.Delay(3840).Wait();
                }
            }

            
        }

        private static async Task ResetPipe(String message = null, LogSeverity severity = LogSeverity.Error)
        {
            pipeServer.Disconnect();

            message ??= "Pipe is broken";

            await SRV.FastLog("IPC-Console", message, severity);
        }

        private static void SendPastLog()
        {
            Byte[] rawLog = Serialize(latestLog);

            pipeServer.Write(BitConverter.GetBytes(rawLog.Length), 0, 4);
            pipeServer.WaitForPipeDrain();

            pipeServer.Write(rawLog, 0, rawLog.Length);
            pipeServer.WaitForPipeDrain();
        }

        private static void SendUpdates()
        {
            Byte[] rawLog;

            while (true)
            {
                if (queue.IsEmpty)
                {
                    Task.Delay(512).Wait();

                    try
                    {
                        pipeServer.WriteByte((Byte)RequestType.DataNotAvailable);
                        pipeServer.WaitForPipeDrain();

                        continue;
                    }
                    catch
                    {
                        return;
                    }
                }

                pipeServer.WriteByte((Byte)RequestType.DataAvailable);
                pipeServer.WaitForPipeDrain();

                queue.TryDequeue(out IPCData.ExtendedLogMessage log);

                rawLog = Serialize(log);

                pipeServer.Write(BitConverter.GetBytes(rawLog.Length), 0, 4);
                pipeServer.WaitForPipeDrain();

                pipeServer.Write(rawLog, 0, rawLog.Length);
                pipeServer.WaitForPipeDrain();
            }
        }

        //# # # # # # # # # # # # # # # # #

        private enum RequestType : Byte
        {
            SendPastLog = 0x11,
            SendUpdates = 0x22,
            DataNotAvailable = 0xFA,
            DataAvailable = 0xDA
        }
    }
}