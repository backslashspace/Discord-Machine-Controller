using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using BSS.Encryption.Fips;
using Org.BouncyCastle.Crypto.Fips;
//
using static Link_Master.Worker.Bot;
using System.Threading.Tasks;

namespace Link_Master.Worker
{
    internal static partial class LinkFactory
    {
        private static Socket socket;
        private static Socket listener;

        internal static void Worker()
        {
            if (!Client.IsConnected)
            {
                Log.FastLog("Link-Factory", "Waiting for connection to discord", xLogSeverity.Info);

                while (!Client.IsConnected)
                {
                    Task.Delay(512).Wait();
                }
            }

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
                            continue;   //cancel was requested while socket was in Accept()
                        }

                        (Boolean endpointIsValid, ChannelLink channelLink, Machine machine) = AuthenticateEndpoint();

                        if (!endpointIsValid)
                        {
                            CloseConnection(ref random);

                            continue;
                        }

                        if (ActiveMachineLinks.ContainsKey(channelLink.ChannelID) || WorkerThreads.Links.ContainsKey(channelLink.ChannelID))
                        {
                            Log.FastLog("Link-Factory", $"Endpoint '{CurrentConfig.MachineChannelLinks[channelLink.ChannelID].Name}' has already connected, new connection came from {(socket.RemoteEndPoint as IPEndPoint).Address}, closing connection", xLogSeverity.Alert);

                            CloseConnection(ref random);

                            continue;
                        }

                        if (!WorkerThreads.LinkFactory_WasCanceled)
                        {
                            ActiveMachineLinks.TryAdd(channelLink.ChannelID, machine);

                            CancellationTokenSource tokenSource = new();

                            Thread newLinkWorker = new(() => LinkWorker.Worker(tokenSource.Token, channelLink, socket));
                            newLinkWorker.Name = $"{channelLink.Name} - {(socket.RemoteEndPoint as IPEndPoint).Address}";

                            Link link = new(ref newLinkWorker, ref tokenSource, ref socket);

                            WorkerThreads.Links.TryAdd(channelLink.ChannelID, link);

                            newLinkWorker.Start();

                            Log.FastLog("Link-Factory", $"Successfully registered and started new link ({CurrentConfig.MachineChannelLinks[channelLink.ChannelID].Name})", xLogSeverity.Info);
                        }
                    }

                    return;     //canceled
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
    }
}