using Discord;
using Discord.WebSocket;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

namespace DC_SRV_VM_LINK.Bot
{
    internal static partial class SRV
    {
        private static readonly Object configLock = new();

#pragma warning disable CS4014
        private static async Task Debug(ChannelLink channelLink, SocketSlashCommand command)
        {
            if (command.User.Id != CurrentConfig.discordAdminID)
            {
                await FormattedResponseAsync(command, Lang.no_perm);

                return;
            }

            try
            {
                VMLink.Command remoteCommand = await TryEnqueue(channelLink, VMLink.CommandAction.EnumScripts, command);

                await FormattedResponseAsync(command, "Successfully enqueued request", Color.Green);

                Task.Run(() => WaitForResultAndDisplay(channelLink, command, remoteCommand));
            }
            catch { }
        }

        private static async Task ListScripts(ChannelLink channelLink, SocketSlashCommand command)
        {
            try
            {
                VMLink.Command remoteCommand = await TryEnqueue(channelLink, VMLink.CommandAction.EnumScripts, command);

                await FormattedResponseAsync(command, "Successfully enqueued request", Color.Green);

                Task.Run(() => WaitForResultAndDisplay(channelLink, command, remoteCommand));
            }
            catch { }
        }

        private static async Task ExecuteScript(ChannelLink channelLink, SocketSlashCommand command)
        {
            Byte[] rawScriptName;

            try
            {
                IReadOnlyCollection<SocketSlashCommandDataOption> unpacked_command_data = command.Data.Options;

                rawScriptName = Encoding.UTF8.GetBytes((String)unpacked_command_data.First().Value);
            }
            catch (Exception ex)
            {
                await FastLog("/execute-scripts", $"Failed to parse command parameter (fileName)\n\n{ex.Message}", LogSeverity.Error);

                await ErrorRespondAsync(command, "Failed to parse command parameter (fileName)");

                return;
            }

            try
            {
                VMLink.Command remoteCommand = await TryEnqueue(channelLink, VMLink.CommandAction.ExecuteScript, command, rawScriptName);

                await FormattedResponseAsync(command, "Successfully enqueued request", Color.Green);

                Task.Run(() => WaitForResultAndDisplay(channelLink, command, remoteCommand));
            }
            catch { }
        }

        private static async Task Help(SocketSlashCommand command)
        {
            EmbedBuilder formattedResponse = new()
            {
                Color = Color.Blue,
                Title = $"/{command.Data.Name}"
            };
            formattedResponse.WithFooter(Lang.cmd_help_no);

            await command.RespondAsync(embed: formattedResponse.Build());
        }

        private static async Task Upload(ChannelLink channelLink, SocketSlashCommand command)
        {
            if (command.User.Id != CurrentConfig.discordAdminID)
            {
                await FormattedResponseAsync(command, Lang.no_perm_hint_admin);

                return;
            }

            Byte[] attachment;

            try
            {
                IReadOnlyCollection<SocketSlashCommandDataOption> unpacked_command_data = command.Data.Options;

                Attachment attachment_data = (Attachment)unpacked_command_data.First().Value;
                attachment = Encoding.UTF8.GetBytes($"{attachment_data.Url}§{attachment_data.Filename}");
            }
            catch (Exception ex)
            {
                await FastLog("/upload", $"attachment data error\n\n{ex.Message}", LogSeverity.Error);

                await ErrorRespondAsync(command, Lang.cmd_upload_error);

                return;
            }

            //

            try
            {
                VMLink.Command remoteCommand = await TryEnqueue(channelLink, VMLink.CommandAction.RemoteDownload, command, attachment);

                await FormattedResponseAsync(command, "Successfully enqueued request", Color.Green);

                Task.Run(() => WaitForResultAndDisplay(channelLink, command, remoteCommand));
            }
            catch { }
        }

        private static async Task Lock(ChannelLink channelLink, SocketSlashCommand command)
        {
            if (command.User.Id != CurrentConfig.discordAdminID)
            {
                await FormattedResponseAsync(command, Lang.no_perm);

                return;
            }

            ChannelLink link = CurrentConfig.vmChannelLink[channelLink.ChannelID];

            if (link.IsLocked)
            {
                await FormattedResponseAsync(command, "This channel is already locked", Color.Orange);

                return;
            }

            link.IsLocked = true;

            lock (configLock)
            {
                CurrentConfig.vmChannelLink[channelLink.ChannelID] = link;
            }

            await FastLog("Discord-CMD", $"User '{command.User.Username}' locked #{command.Channel.Name}", LogSeverity.Info);

            await FormattedResponseAsync(command, "Locked channel", Color.Green);
        }

        private static async Task Unlock(ChannelLink channelLink, SocketSlashCommand command)
        {
            if (command.User.Id != CurrentConfig.discordAdminID)
            {
                await FormattedResponseAsync(command, Lang.no_perm);

                return;
            }

            ChannelLink link = CurrentConfig.vmChannelLink[channelLink.ChannelID];

            if (!link.IsLocked)
            {
                await FormattedResponseAsync(command, "This channel is not locked", Color.Orange);

                return;
            }

            link.IsLocked = false;

            lock (configLock)
            {
                CurrentConfig.vmChannelLink[channelLink.ChannelID] = link;
            }

            await FastLog("Discord-CMD", $"User '{command.User.Username}' unlocked #{command.Channel.Name}", LogSeverity.Info);

            await FormattedResponseAsync(command, "Unlocked channel", Color.Green);
        }
#pragma warning restore CS4014

        //

        private static async Task<VMLink.Command> TryEnqueue(ChannelLink channelLink, VMLink.CommandAction action, SocketSlashCommand command, Byte[] cmdData = null)
        {
            Byte cmdID;

            try
            {
                cmdID = (Byte)VMLink.linkedVMs[channelLink.ChannelID].CommandQueue.Count;
            }
            catch
            {
                await ErrorRespondAsync(command, "Unable to access command queue for link, endpoint not connected? || too many items in command queue for link ( queue > 255 )");

                throw;
            }

            VMLink.Command remoteCommand = new((Byte)(cmdID + 1), action, cmdData);

            try
            {
                VMLink.linkedVMs[channelLink.ChannelID].CommandQueue.Enqueue(remoteCommand);
            }
            catch
            {
                await ErrorRespondAsync(command, "Unable to enqueue request, link down?");

                throw;
            }

            return remoteCommand;
        }
    }
}