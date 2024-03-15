using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Link_Master.Worker
{
    internal static partial class LogConsole
    {
        internal static List<ConsoleMessage> logHistory = new();

        internal static ConcurrentQueue<ConsoleMessage> liveQueue = new();

        internal static Socket socket;

        internal static CancellationTokenSource tokenSource = new();

        //

        internal static void ConsoleServer(CancellationToken cancellationToken)
        {
            Byte issues = 0;

            Log.FastLog("Initiator", $"Started local console log server", LogSeverity.Info);

            while (true)
            {
                try
                {
                    IPEndPoint localEndPoint = new(IPAddress.Loopback, 3001);
                    Socket listener = new(IPAddress.Loopback.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    listener.Bind(localEndPoint);
                    listener.Listen(1);

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            socket = listener.Accept();
                            socket.Blocking = true;
                            socket.NoDelay = true;
                            socket.ReceiveTimeout = 5120;

                            Log.FastLog("Console", "Client connected", LogSeverity.Info, bypassConsole: true);

                            SendPastLog();

                            LiveLogLoop(ref cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            if (ex is SocketException x)
                            {
                                Log.FastLog("Console", "Client disconnected", LogSeverity.Info);
                            }
                            else
                            {
                                Log.FastLog("Console", $"Connection broke for following reason: {ex.Message}", LogSeverity.Warning);

                                Task.Delay(3840).Wait();
                            }
                        }

                        try
                        {
                            socket.Close();
                        }
                        catch { }

                        liveQueue = new();
                    }

                    Log.FastLog("Console", $"Worker shutdown done", LogSeverity.Info);

                    return;
                }
                catch (Exception ex)
                {
                    ++issues;

                    if (issues < 5)
                    {
                        Log.FastLog("Console", $"The console thread threw an unknown exception: {ex.Message}\n\nThis is the '{issues}' out of 5 allowed attempting to restart the worker.", LogSeverity.Critical);

                        continue;
                    }

                    Log.FastLog("Console", $"The console thread threw an unknown exception: {ex.Message}\n\nReached the maximum amount of unknown issues ({issues}), ending and disposing worker.", LogSeverity.Critical);

                    try
                    {
                        socket.Close();
                    }
                    catch { }

                    return;
                }
            }
        }

        //

        private static void SendPastLog()
        {
            Byte[] rawLog = Serialize(logHistory);

            xSocket.TCP_Send(ref socket, ref rawLog);
        }

        private static void LiveLogLoop(ref CancellationToken cancellationToken)
        {
            Byte[] buffer;

            while (!cancellationToken.IsCancellationRequested)
            {
                if (liveQueue.IsEmpty)
                {
                    Task.Delay(256).Wait();

                    AliveCheck();

                    continue;
                }

                liveQueue.TryDequeue(out ConsoleMessage log);

                buffer = Serialize(log);

                xSocket.TCP_Send(ref socket, ref buffer);
            }

            Log.FastLog("Console", $"Worker shutdown was initiated", LogSeverity.Info);

            SendGoodbye();
        }

        //

        private static Byte[] keepAliveBuffer = new Byte[] { 0b10101010 };
        private static void AliveCheck()
        {
            xSocket.TCP_Send(ref socket, ref keepAliveBuffer);
            xSocket.TCP_Receive(ref socket, out _);
        }

        private static void SendGoodbye()
        {
            Byte[] buffer = Serialize(new ConsoleMessage("Console-Server", "Server shutting down, disconnecting console", LogSeverity.Info, DateTime.Now));

            xSocket.TCP_Send(ref socket, ref buffer);
        }
    }
}