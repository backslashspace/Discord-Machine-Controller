using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Link_Master.Worker.Control
{
    internal static partial class ConfigLoader
    {
        private static List<String> postLoadConfigLines = new();

        private static UInt64? _logChannelID = null;

        //

        internal static void PostLoad()
        {
            for (Byte b = 0; b < postLoadConfigLines.Count; ++b)
            {
                if (CurrentConfig.DiscordAdmin == "")
                {
                    Match match = Regex.Match(postLoadConfigLines[b], Pattern.discordAdminUserID, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        try
                        {
                            UInt64 id = UInt64.Parse(match.Groups[1].Value);

                            IUser user = Client.Current.GetUserAsync(id).Result;

                            CurrentConfig.DiscordAdminID = id;
                            CurrentConfig.DiscordAdmin = user.Username;
                            CurrentConfig.__MESSAGE_no_perm_hint_admin = $"You have no permissions to execute this command, you may ask (spam ping) `{user.Username}` to execute this command for you.";
                            postLoadConfigLines.RemoveAt(b);
                        }
                        catch
                        {
                            Error("Invalid discordAdminID, terminating");
                        }
                    }
                }

                if (CurrentConfig.GuildID == 0)
                {
                    Match match = Regex.Match(postLoadConfigLines[b], Pattern.guildID, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        try
                        {
                            CurrentConfig.GuildID = UInt64.Parse(match.Groups[1].Value);

                            if (Client.Current.GetGuild((UInt64)CurrentConfig.GuildID) == null)
                            {
                                throw new InvalidDataException();
                            }

                            postLoadConfigLines.RemoveAt(b);
                        }
                        catch
                        {
                            Error("invalid guildID in config, terminating");
                        }
                    }
                }

                if (_logChannelID == 0)
                {
                    Match match = Regex.Match(postLoadConfigLines[b], Pattern.logChannelID, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        try
                        {
                            _logChannelID = UInt64.Parse(match.Groups[1].Value);

                            postLoadConfigLines.RemoveAt(b);
                        }
                        catch
                        {
                            Error("Failed parse logChannelID from config, terminating");
                        }
                    }
                }

                //

                if (_logChannelID != null)
                {
                    SetLogChannelState();
                }
            }
        }

        //

        private static void SetLogChannelState()
        {
            try
            {
                if (_logChannelID != 0)
                {
                    CurrentConfig.LogChannel = Client.Current.GetGuild((UInt64)CurrentConfig.GuildID).GetTextChannel((UInt64)_logChannelID);
                }
            }
            catch
            {
                Error("Unable to find specified channel, terminating");
            }
        }
    }
}