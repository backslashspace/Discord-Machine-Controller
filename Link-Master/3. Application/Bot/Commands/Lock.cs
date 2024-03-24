using Discord.WebSocket;
using Discord;
using System;
using System.Threading.Tasks;

namespace Link_Master.Worker
{
    internal static partial class Bot
    {
        private static async Task Lock(ChannelLink channelLink, SocketSlashCommand command)
        {
            if (command.User.Id != CurrentConfig.DiscordAdminID)
            {
                await FormattedResponseAsync(command, ResponseStrings.No_Permission);
                Log.FastLog("Discord-CMD", $"User '{command.User.Username}' issued /{command.CommandName} in #{command.Channel.Name} but got rejected (no permissions)", LogSeverity.Info);

                return;
            }

            if (channelLink.IsLocked)
            {
                await FormattedResponseAsync(command, "This channel is already locked", Color.Orange);

                return;
            }

            SetChannelLockState(ref channelLink, true);

            Log.FastLog("Discord-CMD", $"User '{command.User.Username}' locked #{command.Channel.Name}", LogSeverity.Info);

            await FormattedResponseAsync(command, "Locked channel", Color.Green);
        }

        private static async Task Unlock(ChannelLink channelLink, SocketSlashCommand command)
        {
            if (command.User.Id != CurrentConfig.DiscordAdminID)
            {
                await FormattedResponseAsync(command, ResponseStrings.No_Permission);
                Log.FastLog("Discord-CMD", $"User '{command.User.Username}' issued /{command.CommandName} in #{command.Channel.Name} but got rejected (no permissions)", LogSeverity.Info);

                return;
            }

            if (!channelLink.IsLocked)
            {
                await FormattedResponseAsync(command, "This channel is not locked", Color.Orange);

                return;
            }

            SetChannelLockState(ref channelLink, false);

            Log.FastLog("Discord-CMD", $"User '{command.User.Username}' unlocked #{command.Channel.Name}", LogSeverity.Info);

            await FormattedResponseAsync(command, "Unlocked channel", Color.Green);
        }

        //
        private static void SetChannelLockState(ref ChannelLink channelLink, Boolean newState)
        {
            ChannelLink newLink = CurrentConfig.MachineChannelLinks[channelLink.ChannelID];
            newLink.IsLocked = newState;
            CurrentConfig.MachineChannelLinks[channelLink.ChannelID] = newLink;
        }
    }
}