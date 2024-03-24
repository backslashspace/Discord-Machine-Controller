using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Link_Master.Worker
{
    internal static partial class Bot
    {
        private static async Task Upload(ChannelLink channelLink, SocketSlashCommand slashCommand)
        {
            if (slashCommand.User.Id != CurrentConfig.DiscordAdminID)
            {
                Log.FastLog("Discord-CMD", $"User '{slashCommand.User.Username}' issued /{slashCommand.CommandName} in #{slashCommand.Channel.Name} but got rejected (no permissions)", LogSeverity.Info);
                await FormattedResponseAsync(slashCommand, CurrentConfig.__MESSAGE_no_perm_hint_admin);

                return;
            }

            Byte[] attachment;

            try
            {
                IReadOnlyCollection<SocketSlashCommandDataOption> unpacked_command_data = slashCommand.Data.Options;

                Attachment attachment_data = (Attachment)unpacked_command_data.First().Value;
                attachment = Encoding.UTF8.GetBytes($"{attachment_data.Url}§{attachment_data.Filename}");
            }
            catch (Exception ex)
            {
                Log.FastLog("/upload", $"Attachment data error\n\n{ex.Message}", LogSeverity.Error);
                await FormattedErrorRespondAsync(slashCommand, "Encountered an error while parsing the attachment, please contact your administrator providing this useless error message: 22_ERROR_NOT_DOS_DISK`");

                return;
            }

            //

            Command remoteCommand = await TryEnqueue(channelLink, CommandAction.RemoteDownload, slashCommand, attachment);

            await FormattedResponseAsync(slashCommand, "Successfully enqueued request", Color.Green);

            Thread thread = new(() => AwaitCommandProcessing(channelLink, remoteCommand, slashCommand))
            {
                Name = $"Endpoint response awaiter ID: {remoteCommand.ID}"
            };

            thread.Start();
        }
    }
}