using Discord;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Link_Master.Logging
{
    internal static partial class LogConsole
    {
        internal static List<ConsoleMessage> logHistory = new();
        internal readonly static Object logHistory_LOCK = new();

        internal static List<ConsoleMessage> liveQueue = new();
        internal readonly static Object liveQueue_LOCK = new();

        internal static Socket socket;
        internal static Socket listener;

        internal static Boolean IsInLiveLogMode = false;

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
                                continue;   //if cancel was requested while socket was in Accept()
                            }

                            Link_Master.Log.FastLog("Console", "Client connected", LogSeverity.Info);

                            SendPastLog();

                            IsInLiveLogMode = true;
                            LiveLogLoop(ref cancellationToken);

                            continue;
                        }
                        catch (Exception ex)
                        {
                            if (ex is SocketException x)
                            {
                                Link_Master.Log.FastLog("Console", "Client disconnected", LogSeverity.Info);
                            }
                            else
                            {
                                Link_Master.Log.FastLog("Console", $"Connection broke for following reason: {ex.Message}", LogSeverity.Warning);

                                Task.Delay(3840).Wait();
                            }
                        }

                        ResetWorker();  //error
                    }

                    ResetWorker(768);   //give time so client can read SendGoodbye()

                    return;     //when cancel requested
                }
                catch (Exception ex)
                {
                    ++restartAttempts;

                    if (restartAttempts < 5)
                    {
                        Link_Master.Log.FastLog("Console", $"The console log thread threw an unknown exception: {ex.Message}\n\nThis is the '{restartAttempts}' out of 5 allowed attempting to restart the worker.", LogSeverity.Critical);

                        continue;
                    }

                    Link_Master.Log.FastLog("Console", $"The console thread threw an unknown exception: {ex.Message}\n\nReached the maximum amount of issues ({restartAttempts}), terminating", LogSeverity.Critical);

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

                for (Byte b = 0; b < 16 && accepter.ThreadState != ThreadState.Stopped; ++b)
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

            lock (liveQueue_LOCK)
            {
                liveQueue = new();
            }
            
            IsInLiveLogMode = false;
        }

        private static void SendPastLog()
        {
            Byte[] rawLog;

            lock (logHistory_LOCK)
            {
                rawLog = Serialize(logHistory);
            }

            xSocket.TCP_Send(ref socket, ref rawLog);
        }

        private static void LiveLogLoop(ref CancellationToken cancellationToken)
        {
            Byte[] buffer;

            while (!cancellationToken.IsCancellationRequested)
            {
                if (liveQueue.Count == 0)
                {
                    Task.Delay(256).Wait();

                    AliveCheck();

                    continue;
                }

                ConsoleMessage log;

                lock (liveQueue_LOCK)
                {
                    log = liveQueue[0];
                    liveQueue.RemoveAt(0);
                }

                buffer = Serialize(log);

                xSocket.TCP_Send(ref socket, ref buffer);
            }

            Link_Master.Log.FastLog("Console", $"Worker shutdown was initiated, disconnecting client", LogSeverity.Info, bypassIPCLog_Live: true);

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