using System;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using BSS.Encryption.Fips;
using Org.BouncyCastle.Crypto.Fips;
//
using static Link_Master.Worker.Bot;

namespace Link_Master.Worker
{
    internal static partial class LinkFactory
    {
        private static Socket socket;
        private static Socket listener;

        internal static void Worker()
        {
            FipsSecureRandom random = xFips.GenerateSecureRandom();

            BindPort();

            listener.Listen(5);

            for (Byte retries = 0; retries < 5; ++retries)
            {
                try
                {
                    while (!WorkerThreads.LinkFactory_WasCanceled)
                    {
                        if (InterruptibleAccept())
                        {
                            continue;   //if cancel was requested while socket was in Accept()
                        }

                        (Boolean endpointIsValid, ChannelLink channelLink) = GetEndpointInfo();
                        if (endpointIsValid)
                        {
                            EndConnection(ref random);

                            continue;
                        }

                        if (ActiveMachineLinks.ContainsKey(channelLink.ChannelID))
                        {
                            Log.FastLog("Link-Factory", $"Endpoint with name '{CurrentConfig.MachineChannelLinks[channelLink.ChannelID].Name}' has already connected, new connection came from {(socket.RemoteEndPoint as IPEndPoint).Address}, closing connection", xLogSeverity.Alert);

                            EndConnection(ref random);

                            continue;
                        }

                        if (!EndpointHasValidGuid(ref channelLink))
                        {
                            EndConnection(ref random);

                            continue;
                        }

                        if (!WorkerThreads.LinkFactory_WasCanceled)
                        {
                            RegisterNewMachineLink(ref channelLink, out Thread newLinkWorker);
                            newLinkWorker.Start();

                            Log.FastLog("Link-Factory", $"Successfully registered and started new link worker with name '{CurrentConfig.MachineChannelLinks[channelLink.ChannelID].Name}' ({(socket.RemoteEndPoint as IPEndPoint).Address})", xLogSeverity.Info);
                        }
                    }

                    Log.FastLog("Link-Factory", "Received shutdown signal", xLogSeverity.Info);

                    return;
                }
                catch (Exception ex)
                {
                    if (retries == 4)
                    {
                        Log.FastLog("Link-Factory", $"At least 5 total errors occurred in in the link factory worker, shutting down service", xLogSeverity.Critical);

                        Control.Shutdown.ServiceComponents();
                    }

                    Log.FastLog("Link-Factory", $"An error occurred in the link factory worker, this was the {retries + 1} out of 5 allowed errors, the error message was:\n" +
                        $"{ex.Message}\n\nSource: {ex.Source}\n\nStackTrace: {ex.StackTrace}\n\n\t=> continuing", xLogSeverity.Error);
                }
            }
        }

        // # # # # #

        private static void BindPort()
        {
            IPAddress ipAddress = CurrentConfig.TcpListenerIP;
            IPEndPoint localEndPoint = new(ipAddress, (Int32)CurrentConfig.TcpListenerPort);

            listener = new(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
            }
            catch
            {
                Log.FastLog("Link-Factory", $"Unable to bind port [{CurrentConfig.TcpListenerPort}] to [{CurrentConfig.TcpListenerIP}], terminating in 5120ms", xLogSeverity.Critical);

                Control.Shutdown.ServiceComponents();
            }

            Log.FastLog("Link-Factory", $"Successfully bound to [{CurrentConfig.TcpListenerIP}:{CurrentConfig.TcpListenerPort}], entering listening state", xLogSeverity.Info);
        }

        private static Boolean InterruptibleAccept()
        {
            Thread accepter = new(() =>
            {
                try
                {
                    socket = listener.Accept();

                    socket.Blocking = true;
                    socket.NoDelay = true;
                    socket.ReceiveTimeout = 5120;
                    socket.SendTimeout = 5120;
                }
                catch { }
            });

            accepter.Name = "New link connection accepter";
            accepter.Start();

            while (!WorkerThreads.LinkFactory_WasCanceled && accepter.IsAlive)
            {
                Task.Delay(256).Wait();
            }

            if (WorkerThreads.LinkFactory_WasCanceled)
            {
                listener.Close(0);

                for (Byte b = 0; b < 16 && accepter.ThreadState != ThreadState.Stopped; ++b)
                {
                    Task.Delay(64).Wait();
                }

                return true;
            }

            return false;
        }

        private static void EndConnection(ref FipsSecureRandom random)
        {
            try
            {
                socket.Disconnect(false);
            }
            catch { }

            try
            {
                socket.Close(0);
            }
            catch { }

            Task.Delay(random.Next(5120)).Wait();
        }
    }
}