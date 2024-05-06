using Discord;
using Discord.WebSocket;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

//
using static Link_Master.Worker.Bot;

namespace Link_Master.Worker
{
    internal static partial class LinkWorker
    {
        private static void AnnounceConnect(ref ChannelLink channelLink)
        {
            if (!(Boolean)CurrentConfig.AnnounceEndpointConnect)
            {
                return;
            }

            while (!Client.IsConnected)
            {
                Task.Delay(512).Wait();
            }

            if (!Client.BlockNew)
            {
                try
                {
                    EmbedBuilder formattedResponse = new()
                    {
                        Color = Color.Green,
                        Description = "Endpoint connected",
                    };
                    formattedResponse.WithFooter($"Client version: {ActiveMachineLinks[channelLink.ChannelID].Version}");

                    SocketGuild socketGuild = Client.Discord.GetGuild((UInt64)CurrentConfig.GuildID);

                    SocketTextChannel socketTextChannel = socketGuild.GetTextChannel(channelLink.ChannelID);

                    if (!Client.BlockNew)
                    {
                        socketTextChannel.SendMessageAsync(embed: formattedResponse.Build());
                    }
                }
                catch (Exception ex)
                {
                    Log.FastLog("Machine-Link", $"An error occurred in '{channelLink.Name}', error was: ({ex.InnerException.GetType().Name}) => {ex.InnerException.Message}", xLogSeverity.Error);
                }
            }
        }

        private static void AnnounceDisconnect(ref ChannelLink channelLink, Boolean error, ref CancellationToken cancellationToken)
        {
            if ((Boolean)CurrentConfig.AnnounceEndpointConnect && !Client.BlockNew)
            {
                Color color;

                if (error)
                {
                    color = Color.Red;
                }
                else
                {
                    color = Color.Orange;
                }

                try
                {
                    EmbedBuilder formattedResponse;

                    if (cancellationToken.IsCancellationRequested)
                    {
                        formattedResponse = new()
                        {
                            Color = new Color(0u),
                            Description = "Endpoint disconnected, master shutting down",
                        };
                    }
                    else
                    {
                        formattedResponse = new()
                        {
                            Color = color,
                            Description = "Endpoint disconnected",
                        };
                    }

                    if (!Client.BlockNew)
                    {
                        Client.Discord.GetGuild((UInt64)CurrentConfig.GuildID).GetTextChannel(channelLink.ChannelID).SendMessageAsync(embed: formattedResponse.Build());
                    }
                }
                catch (Exception ex)
                {
                    Log.FastLog("Machine-Link", $"An error occurred in '{channelLink.Name}', error was: ({ex.InnerException.GetType().Name}) => {ex.InnerException.Message}", xLogSeverity.Error);
                }
            }
        }
    }
}