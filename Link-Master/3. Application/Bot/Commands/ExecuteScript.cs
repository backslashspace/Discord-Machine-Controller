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
        private static async Task ExecuteScript(ChannelLink channelLink, SocketSlashCommand slashCommand)
        {
            Byte[] rawScriptName;

            try
            {
                IReadOnlyCollection<SocketSlashCommandDataOption> unpacked_command_data = slashCommand.Data.Options;

                rawScriptName = Encoding.UTF8.GetBytes((String)unpacked_command_data.FirstOrDefault().Value);
            }
            catch (Exception ex)
            {
                Log.FastLog("/execute-scripts", $"Failed to parse command parameter (fileName)\n\n{ex.Message}", xLogSeverity.Error);

                if (!Client.BlockNew)
                {
                    await FormattedErrorRespondAsync(slashCommand, "Failed to parse command parameter (fileName)");
                }

                return;
            }

            Command remoteCommand = await TryEnqueue(channelLink, CommandAction.ExecuteScript, slashCommand, rawScriptName);

            await FormattedResponseAsync(slashCommand, "Successfully enqueued request", Color.Green);

            Thread thread = new(() => AwaitCommandProcessing(channelLink, remoteCommand, slashCommand));            
            thread.Name = $"Endpoint response awaiter ID: {remoteCommand.ID}";
            thread.Start();
        }
    }
}