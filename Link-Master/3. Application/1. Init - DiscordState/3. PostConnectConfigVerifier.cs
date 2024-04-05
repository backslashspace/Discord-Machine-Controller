using Discord;
using System;
using System.IO;

namespace Link_Master.Worker
{
    internal static partial class Bot
    {
        internal static UInt64? LogChannelID = null;

        //

        internal static void VerifyConfig()
        {
            try
            {
                IUser user = Client.Discord.GetUserAsync((UInt64)CurrentConfig.DiscordAdminID).Result;

                CurrentConfig.DiscordAdmin = user.Username;
                CurrentConfig.__MESSAGE_no_perm_hint_admin = $"You have no permissions to execute this command, you may ask (spam ping) `{user.Username}` to execute this command for you.";
            }
            catch
            {
                Log.FastLog("Post-Connect", "discordAdminID was invalid, make sure the config contains a valid user id", xLogSeverity.Critical);

                Control.Shutdown.ServiceComponents();
            }

            try
            {

                if (Client.Discord.GetGuild((UInt64)CurrentConfig.GuildID) == null)
                {
                    throw new InvalidDataException();
                }
            }
            catch
            {
                Log.FastLog("Post-Connect", "guildID was invalid, make sure the config contains a valid server id / make sure the bot is on said server", xLogSeverity.Critical);

                Control.Shutdown.ServiceComponents();
            }

            if (Bot.LogChannelID != null)
            {
                try
                {
                    CurrentConfig.LogChannel = Client.Discord.GetGuild((UInt64)CurrentConfig.GuildID).GetTextChannel((UInt64)Bot.LogChannelID);

                    if (CurrentConfig.LogChannel == null)
                    {
                        Log.FastLog("Post-Connect", $"Unable to find channel with ID: '{Bot.LogChannelID}', " +
                        $"discord reports channel not found", xLogSeverity.Critical);

                        Control.Shutdown.ServiceComponents();
                    }

                    EmbedBuilder sendHello = new()
                    {
                        Color = Color.Green,
                        Description = "Service connected"
                    };

                    if (!Client.BlockNew)
                    { 
                        CurrentConfig.LogChannel.SendMessageAsync(embed: sendHello.Build()).Wait();
                    }
                }
                catch
                {
                    Log.FastLog("Post-Connect", $"Failed to push message to the configured log channel #{CurrentConfig.LogChannel.Name}, " +
                        $"make sure the bot has proper access " +
                        $"or deactivate logging to discord by removing / commenting out the config line", xLogSeverity.Critical);

                    Control.Shutdown.ServiceComponents();
                }

                LogChannelID = null;
            }
        }
    }
}