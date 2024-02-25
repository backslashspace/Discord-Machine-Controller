using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace DC_SRV_VM_LINK.Bot
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
                if (CurrentConfig.discordAdmin == "")
                {
                    Match match = Regex.Match(postLoadConfigLines[b], Pattern.discordAdminUserID, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        try
                        {
                            UInt64 id = UInt64.Parse(match.Groups[1].Value);

                            IUser user = SRV.dc_client.GetUserAsync(id).Result;

                            CurrentConfig.discordAdminID = id;
                            CurrentConfig.discordAdmin = user.Username;
                            Lang.no_perm_hint_admin = $"You have no permissions to execute this command, you may ask (spam ping) `{user.Username}` to execute this command for you.";
                            postLoadConfigLines.RemoveAt(b);
                        }
                        catch
                        {
                            Error(Lang.log_critical_config_read_invalid_discordAdminID);
                        }
                    }
                }

                if (CurrentConfig.guildID == 0)
                {
                    Match match = Regex.Match(postLoadConfigLines[b], Pattern.guildID, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        try
                        {
                            CurrentConfig.guildID = UInt64.Parse(match.Groups[1].Value);

                            if (SRV.dc_client.GetGuild((UInt64)CurrentConfig.guildID) == null)
                            {
                                throw new InvalidDataException();
                            }

                            postLoadConfigLines.RemoveAt(b);
                        }
                        catch
                        {
                            Error(Lang.log_critical_config_read_guild_invalid);
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
                            Error(Lang.log_critical_config_read_parse_logChannelID);
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
                    CurrentConfig.logChannel = SRV.dc_client.GetGuild((UInt64)CurrentConfig.guildID).GetTextChannel((UInt64)_logChannelID);
                }
            }
            catch
            {
                Error(Lang.config_critical_unable_find_logChannel);
            }
        }
    }
}