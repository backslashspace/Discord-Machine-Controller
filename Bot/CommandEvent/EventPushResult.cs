using Discord;
using Discord.WebSocket;
using System;
using System.Text;
using System.Threading.Tasks;

namespace DC_SRV_VM_LINK.Bot
{
    internal static partial class SRV
    {
        private static Boolean ResultIsPresent_ThrowOnMissingQueue(ref ChannelLink channelLink, Byte iD)
        {
            if (VMLink.linkedVMs[channelLink.ChannelID].Results.IsEmpty)
            {
                return true;
            }

            foreach (VMLink.Result result in VMLink.linkedVMs[channelLink.ChannelID].Results)
            {
                if (result.ID == iD)
                {
                    return true;
                }
            }

            return false;
        }

        //

        private static void WaitForResultAndDisplay(ChannelLink channelLink, SocketSlashCommand command, VMLink.Command remoteCommand)
        {
            try
            {
                while (ResultIsPresent_ThrowOnMissingQueue(ref channelLink, remoteCommand.ID))
                {
                    if (VMLink.linkedVMs[channelLink.ChannelID].Results.TryPeek(out VMLink.Result result))
                    {
                        if (result.ID == remoteCommand.ID)
                        {
                            VMLink.linkedVMs[channelLink.ChannelID].Results.TryDequeue(out _);

                            ResponseDisplay(ref channelLink, ref command, result.ResultData);

                            return;
                        }
                    }

                    Task.Delay(512).Wait();
                }
            }
            catch
            {
                FormattedMessageAsync(command, "Link error, endpoint disconnected", Color.Red).Wait();

                return;
            }
        }

        //########################################################################################################################################################

        private static void ResponseDisplay(ref ChannelLink channelLink, ref SocketSlashCommand command, Byte[] rawResponse)
        {
            try
            {
                (String response, Color color) = ResponseDeconstructer(rawResponse.AsSpan());

                if (response.Length > 4069)
                {
                    FormattedMessageAsync(command, "Response string to long, cutting output..", Color.DarkOrange).Wait();

                    FastLog("Discord-CMD", $"Endpoint '{channelLink.Name}' for User '{command.User.Username}' send to long response, cutting output..", LogSeverity.Warning).Wait();

                    response = response.Substring(0, 4069);
                }
                
                FormattedMessageAsync(command, response, color).Wait();
                
                FastLog("Discord-CMD", $"Successfully received and displayed result from endpoint '{channelLink.Name}' for User '{command.User.Username}'", LogSeverity.Info).Wait();
            }
            catch
            {
                try
                {
                    FormattedMessageAsync(command, "Error, unable to convert response to message", Color.Red).Wait();
                    FastLog("Discord-CMD", $"Unable to parse response from endpoint for user '{command.User.Username}' in #{command.Channel.Name} (Byte[] => String)", LogSeverity.Error).Wait();
                }
                catch
                {
                    FastLog("Discord-CMD", $"Unable to push response from endpoint for user '{command.User.Username}' to Discord channel #{command.Channel.Name} (no message perms?)", LogSeverity.Error).Wait();
                }
            }
        }

        private static (String response, Color color) ResponseDeconstructer(Span<Byte> rawResponse)
        {
            UInt32 rawColor = BitConverter.ToUInt32(rawResponse.Slice(0, 4).ToArray(), 0);
            Color color = new(rawColor);


            String response = Encoding.UTF8.GetString(rawResponse.Slice(4, rawResponse.Length - 4).ToArray());

            return (response, color);
        }
    }
}