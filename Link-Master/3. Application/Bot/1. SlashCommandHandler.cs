using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Link_Master.Worker
{
    internal static partial class Bot
    {
        private static async Task SlashCommandHandler(SocketSlashCommand command)
        {
            if (command.ChannelId == null)
            {
                return;
            }

            UInt64 originChannel = (UInt64)command.ChannelId;

            if (!IsMachineControlChannel(ref originChannel))
            {
                Log.FastLog("Discord-CMD", $"User '{command.User.Username}' ({command.User.Id}) issued /{command.Data.Name} in #{command.Channel.Name} ({command.Channel.Id}), but got rejected (not a machine control channel)", xLogSeverity.Info);
                await Respond_NotAControlChannel(command);

                return;
            }

            ChannelLink channelLink = CurrentConfig.MachineChannelLinks[originChannel];

            switch (command.Data.Name)
            {
                case "lock":
                    await Lock(channelLink, command);
                    return;

                case "unlock":
                    await Unlock(channelLink, command);
                    return;
            }

            if (CurrentConfig.MachineChannelLinks[originChannel].IsLocked)
            {
                Log.FastLog("Discord-CMD", $"User '{command.User.Username}' issued /{command.CommandName} in #{command.Channel.Name} but got rejected (channel locked)", xLogSeverity.Info);
                await FormattedResponseAsync(command, "You are currently not allowed to enqueue commands, channel is locked", Color.Orange);

                return;
            }

            if (!ChannelEndpointIsConnected(ref channelLink))
            {
                Log.FastLog("Discord-CMD", $"User '{command.User.Username}' ({command.User.Id}) issued /{command.CommandName} in #{command.Channel.Name} ({command.Channel.Id}), but got rejected (endpoint not connected)", xLogSeverity.Info);
                await FormattedErrorRespondAsync(command, "Endpoint is not connected");

                return;
            }

            Log.FastLog("Discord-CMD", $"User '{command.User.Username}' ({command.User.Id}) issued /{command.CommandName} in #{command.Channel.Name} ({command.Channel.Id})", xLogSeverity.Info);

            switch (command.Data.Name)
            {
                case "help":
                    await Help(command);
                    break;

                case "upload":
                    await Upload(channelLink, command);
                    break;

                case "list-scripts":
                    await ListScripts(channelLink, command);
                    break;

                case "execute-scripts":
                    await ExecuteScript(channelLink, command);
                    break;

                case "vdebug":
                    await Debug(command);
                    break;
            }

            return;
        }

        //

        private static Boolean IsMachineControlChannel(ref UInt64 channelID)
        {
            try
            {
                if (CurrentConfig.MachineChannelLinks.TryGetValue(channelID, out _))
                {
                    return true;
                }
            }
            catch { }

            return false;
        }

        private static async Task Respond_NotAControlChannel(SocketSlashCommand command)
        {
            EmbedBuilder response = new()
            {
                Color = Color.DarkerGrey,
                Description = $"**This is not a machine control channel**"
            };

            try
            {
                await command.RespondAsync(embed: response.Build());
            }
            catch { }
        }

        private static Boolean ChannelEndpointIsConnected(ref ChannelLink channelLink)
        {
            try
            {
                if (ActiveMachineLinks.TryGetValue(channelLink.ChannelID, out _))
                {
                    return true;
                }
            }
            catch { }

            return false;
        }
    }
}