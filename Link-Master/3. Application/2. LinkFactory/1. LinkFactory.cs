using System;
using System.Net.Sockets;
using System.Net;
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
                            CloseConnection(ref random);

                            continue;
                        }

                        if (ActiveMachineLinks.ContainsKey(channelLink.ChannelID))
                        {
                            Log.FastLog("Link-Factory", $"Endpoint with name '{CurrentConfig.MachineChannelLinks[channelLink.ChannelID].Name}' has already connected, new connection came from {(socket.RemoteEndPoint as IPEndPoint).Address}, closing connection", xLogSeverity.Alert);

                            CloseConnection(ref random);

                            continue;
                        }

                        if (!EndpointHasValidGuid(ref channelLink))
                        {
                            CloseConnection(ref random);

                            continue;
                        }

                        if (!WorkerThreads.LinkFactory_WasCanceled)
                        {
                            RegisterNewMachineLink(channelLink, out Thread newLinkWorker);
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

        private static void RegisterNewMachineLink(ChannelLink channelLink, out Thread newLinkWorker)
        {
            Byte[] serverVersion = xVersion.GetBytes(ref Program.version);
            AES_TCP.Send(ref socket, ref serverVersion, channelLink.AES_Key, channelLink.HMAC_Key);

            Byte[] rawEndpointXVersion = AES_TCP.Receive(ref socket, channelLink.AES_Key, channelLink.HMAC_Key);
            xVersion endpointXVersion = xVersion.GetXVersion(ref rawEndpointXVersion);

            Log.FastLog("Link-Factory", $"Endpoint version v{endpointXVersion} with name '{CurrentConfig.MachineChannelLinks[channelLink.ChannelID].Name}' successfully authenticated", xLogSeverity.Info);

            //register and start new link
            Machine machine = new(channelLink.ChannelID, (socket.RemoteEndPoint as IPEndPoint).Address, ref endpointXVersion);
            ActiveMachineLinks.TryAdd(channelLink.ChannelID, machine);

            CancellationTokenSource tokenSource = new();

            newLinkWorker = new(() => LinkWorker.Worker(tokenSource.Token, channelLink, socket));
            newLinkWorker.Name = $"{channelLink.Name} - {(socket.RemoteEndPoint as IPEndPoint).Address}";

            Link link = new(ref newLinkWorker, ref tokenSource);

            WorkerThreads.Links.Add(link);
        }
    }
}