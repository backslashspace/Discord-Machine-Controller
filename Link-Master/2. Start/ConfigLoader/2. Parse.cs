using Discord;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace Link_Master.Control
{
    internal static partial class ConfigLoader
    {
        private static void ParseSettings(ref List<String> configLines)
        {
            for (Byte b = 0; b < configLines.Count; ++b)
            {
                if (configLines[b] == "" || configLines[b][0] == '#')
                {
                    continue;
                }

                TryGetChannelPerm(ref configLines, ref b);

                if (CurrentConfig.TokenPath == null)
                {
                    Match match = Regex.Match(configLines[b], Pattern.tokenPath, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        CurrentConfig.TokenPath = match.Groups[1].Value;
                    }
                }

                if (CurrentConfig.CmdRegisterOnBoot == null)
                {
                    Match match = Regex.Match(configLines[b], Pattern.cmdRegisterOnBoot, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        try
                        {
                            CurrentConfig.CmdRegisterOnBoot = Boolean.Parse(match.Groups[1].Value);
                        }
                        catch
                        {
                            Error("Invalid cmdRegisterOnBoot value, terminating");
                        }
                    }
                }

                if (CurrentConfig.TcpListenerPort == null)
                {
                    Match match = Regex.Match(configLines[b], Pattern.tcpListenerPort, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        try
                        {
                            CurrentConfig.TcpListenerPort = UInt16.Parse(match.Groups[1].Value);
                        }
                        catch
                        {
                            Error("Unable to parse tcpListenPort, terminating");
                        }
                    }
                }

                if (CurrentConfig.TcpListenerIP == null)
                {
                    Match match = Regex.Match(configLines[b], Pattern.tcpListenerIP, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        try
                        {
                            CurrentConfig.TcpListenerIP = IPAddress.Parse(match.Groups[1].Value);
                        }
                        catch
                        {
                            Error("Unable to parse tcpListenIP, terminating");
                        }
                    }
                }

                //will be verified after Bot.Connect()

                if (CurrentConfig.DiscordAdmin == null)
                {
                    Match match = Regex.Match(configLines[b], Pattern.discordAdminUserID, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        try
                        {
                            CurrentConfig.DiscordAdminID = UInt64.Parse(match.Groups[1].Value);
                        }
                        catch
                        {
                            Error("Invalid discordAdminID, terminating");
                        }
                    }
                }

                if (CurrentConfig.GuildID == null)
                {
                    Match match = Regex.Match(configLines[b], Pattern.guildID, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        try
                        {
                            CurrentConfig.GuildID = UInt64.Parse(match.Groups[1].Value);
                        }
                        catch
                        {
                            Error("Failed to parse guildID from config, terminating");
                        }
                    }
                }

                if (Worker.Bot.LogChannelID == null)
                {
                    Match match = Regex.Match(configLines[b], Pattern.logChannelID, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        try
                        {
                            Worker.Bot.LogChannelID = UInt64.Parse(match.Groups[1].Value);
                        }
                        catch
                        {
                            Error("Failed to parse logChannelID from config, terminating");
                        }
                    }
                }
            }
        }

        //

        private static void TryGetChannelPerm(ref List<String> configLines, ref Byte b)
        {
            Match match = Regex.Match(configLines[b], Pattern.vmChannelLink, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                try
                {
                    String name = match.Groups[1].Value;
                    Guid guid = Guid.Parse(match.Groups[2].Value);
                    UInt64 channelID = UInt64.Parse(match.Groups[3].Value);

                    Span<Byte> keys = stackalloc Byte[96];
                    keys = Convert.FromBase64String(match.Groups[4].Value);

                    try
                    {
                        CurrentConfig.MachineChannelLinks.TryAdd(channelID, new ChannelLink(ref name, ref guid, ref channelID, ref keys));
                    }
                    catch
                    {
                        Log.FastLog("Initiator", $"Invalid configuration for '{name}', only one endpoint per channel allowed, skipping '{name}'", xLogSeverity.Error);
                    }
                }
                catch
                {
                    Error("Failed to parse config (vmChannelLink), terminating");
                }
            }
        }
    }
}