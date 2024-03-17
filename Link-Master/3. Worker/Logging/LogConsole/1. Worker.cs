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
        internal static Socket listener;

        internal static Boolean IsInLiveLogMode = false;

        internal static CancellationTokenSource tokenSource = new();

        //

        internal static void ConsoleServer(CancellationToken cancellationToken)
        {
            Byte restartAttempts = 0;

            while (true)
            {
                try
                {
                    IPEndPoint localEndPoint = new(IPAddress.Loopback, 3001);
                    listener = new(IPAddress.Loopback.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    listener.Bind(localEndPoint);
                    listener.Listen(0);

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            if (InterruptibleAccept(ref cancellationToken))
                            {
                                continue;   //cancel was requested while socket was in Accept()
                            }

                            Log.FastLog("Console", "Client connected", LogSeverity.Info);

                            SendPastLog();

                            IsInLiveLogMode = true;

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

                        ResetWorker();
                    }

                    ResetWorker(512);   //give time so client can read SendGoodbye()

                    return;     //when cancel requested
                }
                catch (Exception ex)
                {
                    ++restartAttempts;

                    if (restartAttempts < 5)
                    {
                        Log.FastLog("Console", $"The console thread threw an unknown exception: {ex.Message}\n\nThis is the '{restartAttempts}' out of 5 allowed attempting to restart the worker.", LogSeverity.Critical);

                        continue;
                    }

                    Log.FastLog("Console", $"The console thread threw an unknown exception: {ex.Message}\n\nReached the maximum amount of unknown issues ({restartAttempts}), ending and disposing worker.", LogSeverity.Critical);

                    ResetWorker();

                    Control.Shutdown.ServiceComponents();
                }
            }
        }

        private static Boolean InterruptibleAccept(ref CancellationToken cancellationToken)
        {
            Thread accepter = new(() =>
            {
                try
                {
                    socket = listener.Accept();

                    socket.Blocking = true;
                    socket.NoDelay = true;
                    socket.ReceiveTimeout = 5120;
                }
                catch { }
            });

            accepter.Name = "Console Log connection accepter";
            accepter.Start();

            while (!cancellationToken.IsCancellationRequested && accepter.IsAlive)
            {
                Task.Delay(256).Wait();
            }

            if (cancellationToken.IsCancellationRequested)
            {
                listener.Close(0);

                for (Byte b = 0; b < 16 && accepter.ThreadState != System.Threading.ThreadState.Stopped; ++b)
                {
                    Task.Delay(128);
                }

                return true;
            }

            return false;
        }

        //

        private static void ResetWorker(Int32 delay = 0)
        {
            try
            {
                socket.Close(delay);
            }
            catch { }

            liveQueue = new();
            IsInLiveLogMode = false;
        }

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
                    Task.Delay(512).Wait();

                    AliveCheck();

                    continue;
                }

                liveQueue.TryDequeue(out ConsoleMessage log);

                buffer = Serialize(log);

                xSocket.TCP_Send(ref socket, ref buffer);
            }

            Log.FastLog("Console", $"Worker shutdown was initiated, disconnecting client", LogSeverity.Info);

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