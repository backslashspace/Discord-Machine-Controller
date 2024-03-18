using Discord;
using Link_Master.Worker.Control;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

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
                Log.FastLog("Post-Connect", "discordAdminID was invalid, make sure the config contains a valid user id", LogSeverity.Critical);

                Shutdown.ServiceComponents();
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
                Log.FastLog("Post-Connect", "guildID was invalid, make sure the config contains a valid server id", LogSeverity.Critical);

                Shutdown.ServiceComponents();
            }

            if (Bot.LogChannelID != null)
            {
                try
                {
                    CurrentConfig.LogChannel = Client.Discord.GetGuild((UInt64)CurrentConfig.GuildID).GetTextChannel((UInt64)Bot.LogChannelID);

                    if (CurrentConfig.LogChannel == null)
                    {
                        Log.FastLog("Post-Connect", $"Failed to push message to the configured log channel with the following configured id: '{Bot.LogChannelID}', " +
                        $"discord says the channel does not exists", LogSeverity.Critical);

                        Shutdown.ServiceComponents();
                    }

                    EmbedBuilder sendHello = new()
                    {
                        Color = Color.Green,
                        Description = "Service connected"
                    };

                    CurrentConfig.LogChannel.SendMessageAsync(embed: sendHello.Build()).Wait();
                }
                catch
                {
                    Log.FastLog("Post-Connect", $"Failed to push message to the configured log channel with the following id: '{CurrentConfig.LogChannel}', " +
                        $"you may do one of the following things: verify that the channel exists, make sure the bot has proper access to said channel, " +
                        $"or deactivate logging to discord by removing / commenting out the config line", LogSeverity.Critical);

                    Shutdown.ServiceComponents();
                }

                LogChannelID = null;
            }
        }
    }
}