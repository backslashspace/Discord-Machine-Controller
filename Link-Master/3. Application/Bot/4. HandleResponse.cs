using Discord;
using Discord.WebSocket;
using System;
using System.Text;

namespace Link_Master.Worker
{
    internal static partial class Bot
    {
        private static async void DisplayResultDiscord(ChannelLink channelLink, SocketSlashCommand slashCommand, Result result)
        {
            String response;
            Color color;

            try
            {
                (response, color) = ParseResponse(ref result.ResultData);
            }
            catch
            {
                Log.FastLog("Discord-CMD", $"Unable to parse response from endpoint '{channelLink.Name}' for user '{slashCommand.User.Username}' ({slashCommand.User.Id}) in #{slashCommand.Channel.Name} (Byte[] => String, Color)", xLogSeverity.Error);
                await FormattedMessageAsync(slashCommand, "Error, unable to parse response from endpoint", Color.Red);
                
                return;
            }
            
            if (response.Length > 4064)
            {
                Log.FastLog("Discord-CMD", $"Response from endpoint '{channelLink.Name}' for user '{slashCommand.User.Username}' ({slashCommand.User.Id}) was too long, cutting output..", xLogSeverity.Warning);
                await FormattedMessageAsync(slashCommand, "Response string to long, cutting output..", Color.DarkOrange);

                response = response.Substring(0, 4064);
            }

            await FormattedMessageAsync(slashCommand, response, color);
            Log.FastLog("Discord-CMD", $"Successfully received and displayed result from endpoint '{channelLink.Name}' for user '{slashCommand.User.Username}' ({slashCommand.User.Id}) in channel #{slashCommand.Channel.Name}", xLogSeverity.Info);
        }

        //

        private static (String response, Color color) ParseResponse(ref byte[] rawResponse)
        {
            Color color = new(BitConverter.ToUInt32(rawResponse, 0));

            String response = Encoding.UTF8.GetString(rawResponse, 4, rawResponse.Length - 4);

            return (response, color);
        }
    }
}