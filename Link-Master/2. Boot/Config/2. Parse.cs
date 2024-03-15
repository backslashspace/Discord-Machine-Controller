using Discord;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace Link_Master.Worker.Control
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

                if (CurrentConfig.tokenPath == null)
                {
                    Match match = Regex.Match(configLines[b], Pattern.tokenPath, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        CurrentConfig.tokenPath = match.Groups[1].Value;
                    }
                }

                if (CurrentConfig.cmdRegisterOnBoot == null)
                {
                    Match match = Regex.Match(configLines[b], Pattern.cmdRegisterOnBoot, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        try
                        {
                            CurrentConfig.cmdRegisterOnBoot = Boolean.Parse(match.Groups[1].Value);
                        }
                        catch
                        {
                            Error("Invalid cmdRegisterOnBoot value, terminating");
                        }
                    }
                }

                if (CurrentConfig.tcpListenerPort == null)
                {
                    Match match = Regex.Match(configLines[b], Pattern.tcpListenerPort, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        try
                        {
                            CurrentConfig.tcpListenerPort = UInt16.Parse(match.Groups[1].Value);
                        }
                        catch
                        {
                            Error("Unable to parse tcpListenPort, terminating");
                        }
                    }
                }

                if (CurrentConfig.tcpListenerIP == null)
                {
                    Match match = Regex.Match(configLines[b], Pattern.tcpListenerIP, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        try
                        {
                            CurrentConfig.tcpListenerIP = IPAddress.Parse(match.Groups[1].Value);
                        }
                        catch
                        {
                            Error("Unable to parse tcpListenIP, terminating");
                        }
                    }
                }

                //post load
                if (CurrentConfig.discordAdmin == null)
                {
                    Match match = Regex.Match(configLines[b], Pattern.discordAdminUserID, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        CurrentConfig.discordAdmin = "";

                        postLoadConfigLines.Add(configLines[b]);
                    }
                }

                if (CurrentConfig.guildID == null)
                {
                    Match match = Regex.Match(configLines[b], Pattern.guildID, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        CurrentConfig.guildID = 0;

                        postLoadConfigLines.Add(configLines[b]);
                    }
                }

                if (_logChannelID == null)
                {
                    Match match = Regex.Match(configLines[b], Pattern.logChannelID, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        _logChannelID = 0;

                        postLoadConfigLines.Add(configLines[b]);
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
                        CurrentConfig.vmChannelLink.Add(channelID, new ChannelLink(ref name, ref guid, ref channelID, ref keys));
                    }
                    catch
                    {
                        Log.FastLog("Initiator", $"Invalid configuration for '{name}', only one endpoint per channel allowed, skipping '{name}'", LogSeverity.Error);
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