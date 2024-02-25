using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VMLink_Slave;
using System.Threading;
using System.IO;

namespace LinkSlave
{
    internal static partial class Client
    {
        internal static String assemblyPath;

        internal static Socket socket;

        //

        internal static void ConnectionLoop(CancellationToken cancellation)
        {
        Restart:
            try
            {
                while (true)
                {
                    Log.Print("Attempting to connect to server", LogSeverity.Info);

                    try
                    {
                        CreateSocket();

                        Connect(ref cancellation);

                        if (cancellation.IsCancellationRequested)
                        {
                            return;
                        }

                        AuthMe();

                        PassivePingPong(ref cancellation);
                    }
                    catch (Exception ex)
                    {
                        Log.Print("Disconnected from server", LogSeverity.Warning);

                        try
                        {
                            socket.Disconnect(false);
                            socket.Close();
                            socket.Dispose();
                        }
                        catch { }
                        
                        if (cancellation.IsCancellationRequested)
                        {
                            return;
                        }

                        if (ex is SocketException e)
                        {
                            if (e.SocketErrorCode == SocketError.ConnectionAborted)
                            {
                                Wait();
                            }
                        }
                        else if (ex is InvalidDataException)
                        {
                            Wait();
                        }

                        static void Wait()
                        {
                            Log.Print($"Connection aborted, not registered on server?", LogSeverity.Error);

                            Task.Delay(10240).Wait();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Print($"Unknown important error, ples report to admin when seeing this. here is some more info:\nMesssage: {e.Message}\nTrace: {e.StackTrace}\nSource. {e.Source}", LogSeverity.Critical);

                Task.Delay(1024).Wait();

                goto Restart;
            }
        }

        //

        private static void CreateSocket()
        {
            socket = new(CurrentConfig.serverIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            socket.SendTimeout = 3840;
            socket.ReceiveTimeout = -1;
        }

        private static void Connect(ref CancellationToken cancellation)
        {
            IPEndPoint remoteEndpoint = new(CurrentConfig.serverIP, CurrentConfig.tcpPort);

            while (true)
            {
                if (cancellation.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    socket.Connect(remoteEndpoint);

                    break;
                }
                catch
                {
                    Task.Delay(512).Wait();
                }
            }

            Log.Print("Connected to server", LogSeverity.Info);
        }

        private static void AuthMe()
        {
            socket.ReceiveTimeout = 3840;

            try
            {
                Byte[] authBytes = Encoding.UTF8.GetBytes($"ID:\"{CurrentConfig.channelID}\"Guid:\"{CurrentConfig.guid}\"");

                FastSocket.SendTCP(ref socket, authBytes);

                //challenge
                Byte[] challengeToSolve = AES_FastSocket.ReceiveTCP(ref socket, CurrentConfig.AES_Key, CurrentConfig.HMAC_Key);

                UInt64 challengeAsInt = BitConverter.ToUInt64(challengeToSolve, 0);

                UInt64 solvedChallenge;

                if (challengeAsInt < 9446744073709551615)
                {
                    if (challengeAsInt > 65000)
                    {
                        solvedChallenge = challengeAsInt + UInt32.MaxValue;
                    }
                    else
                    {
                        solvedChallenge = challengeAsInt + Byte.MaxValue;
                    }
                }
                else
                {
                    if (challengeAsInt > UInt64.MaxValue - 65000)
                    {
                        solvedChallenge = challengeAsInt - Byte.MaxValue;
                    }
                    else
                    {
                        solvedChallenge = challengeAsInt + (Int16.MaxValue - 420);
                    }
                }

                AES_FastSocket.SendTCP(ref socket, BitConverter.GetBytes(solvedChallenge), CurrentConfig.AES_Key, CurrentConfig.HMAC_Key);
            }
            catch (Exception ex)
            {
                if (ex is SocketException e)
                {
                    Log.Print($"Failed to authenticate at server with the following error: '{e.SocketErrorCode}', terminating.", LogSeverity.Error);
                }
                else
                {
                    Log.Print($"Failed to authenticate at server with the following error message: '{ex.Message}', terminating.", LogSeverity.Error);
                }

                throw;
            }

            socket.ReceiveTimeout = -1;

            Log.Print("Successfully authenticated at server", LogSeverity.Info);
        }

        //

        internal static void Exit(Boolean normalExit = false)
        {
            Log.EndMe();

            if (normalExit)
            {
                Environment.Exit(0);
            }
            else
            {
                Environment.Exit(1);
            }
        }
    }
}