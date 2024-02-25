using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.IO;
using Discord;

namespace DC_SRV_VM_LINK.Bot
{
    internal static partial class SRV
    {
        private static async Task SlashCommandHandler(SocketSlashCommand command)
        {
            UInt64 originChannel = await ValidateOrigin(command);

            if (originChannel == 0)
            {
                return;
            }

            ChannelLink channelLink = CurrentConfig.vmChannelLink[originChannel];

            //test if command was lock
            switch (command.Data.Name)
            {
                case "lock":
                    await Lock(channelLink, command);
                    return;

                case "unlock":
                    await Unlock(channelLink, command);
                    return;
            }

            if (CurrentConfig.vmChannelLink[channelLink.ChannelID].IsLocked)
            {
                await FormattedResponseAsync(command, "You are currently not allowed to enqueue commands, channel is locked", Color.Orange);

                await FastLog("Discord-CMD", $"User '{command.User.Username}' issued /{command.CommandName} in #{command.Channel.Name} but got rejected, channel locked", LogSeverity.Info);

                return;
            }

            if (!VMIsUp(channelLink.ChannelID))
            {
                await ErrorRespondAsync(command, "Endpoint is not connected");

                await FastLog("Discord-CMD", $"User '{command.User.Username}' issued /{command.CommandName} in #{command.Channel.Name} but got rejected, endpoint not connected", LogSeverity.Info);

                return;
            }

            await FastLog("Discord-CMD", $"User '{command.User.Username}' issued /{command.CommandName} in #{command.Channel.Name}", LogSeverity.Info);

            switch (command.Data.Name)
            {
                case "help":
                    await Help(command);
                    break;

                case "upload":
                    await Upload(channelLink, command);
                    break;

                case "lock":
                    await Lock(channelLink, command);
                    break;

                case "unlock":
                    await Unlock(channelLink, command);
                    break;

                case "list-scripts":
                    await ListScripts(channelLink, command);
                    break;

                case "execute-scripts":
                    await ExecuteScript(channelLink, command);
                    break;

                case "vdebug":
                    await Debug(channelLink, command);
                    break;
            }
        }

        private static async Task<UInt64> ValidateOrigin(SocketSlashCommand command)
        {
            if (command.GuildId != CurrentConfig.guildID)
            {
                await ErrorRespondAsync(command, "lebru\ni shloudnt b hre");
                return 0;
            }

            try
            {
                if (!CurrentConfig.vmChannelLink.TryGetValue(command.ChannelId, out _))
                {
                    throw new InvalidDataException();
                }

                return (UInt64)command.ChannelId;
            }
            catch
            {
                return 0;
            }
        }

        private static Boolean VMIsUp(UInt64 channelID)
        {
            try
            {
                if (!VMLink.linkedVMs.TryGetValue(channelID, out _))
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}