using Discord;
using System;
//
using static Link_Master.Worker.Bot;

namespace Link_Master.Worker
{
    internal static partial class LinkWorker
    {
        private static void AnnounceConnect(ref ChannelLink channelLink)
        {
            if ((Boolean)CurrentConfig.AnnounceEndpointConnect && !Client.BlockNew)
            {
                try
                {
                    EmbedBuilder formattedResponse = new()
                    {
                        Color = Color.Green,
                        Description = "Endpoint connected",
                    };
                    formattedResponse.WithFooter($"Endpoint version: {ActiveMachineLinks[channelLink.ChannelID].Version}");

                    Client.Discord.GetGuild((UInt64)CurrentConfig.GuildID).GetTextChannel(channelLink.ChannelID).SendMessageAsync(embed: formattedResponse.Build());
                }
                catch { }
            }
        }

        private static void AnnounceDisconnect(ref ChannelLink channelLink, Boolean error)
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
                    EmbedBuilder formattedResponse = new()
                    {
                        Color = color,
                        Description = "Endpoint disconnected",
                    };

                    Client.Discord.GetGuild((UInt64)CurrentConfig.GuildID).GetTextChannel(channelLink.ChannelID).SendMessageAsync(embed: formattedResponse.Build());
                }
                catch { }
            }
        }
    }
}