using DC_SRV_VM_LINK.Service;
using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;

namespace DC_SRV_VM_LINK.Bot
{
    internal static partial class ConfigLoader
    {
        internal static void Load()
        {
            SRV.FastLog("Initiator", "Loading config", LogSeverity.Info).Wait();

            List<String> configLines = ReadConfigFile();

            if (configLines.Count > 128)
            {
                SRV.FastLog("Initiator", $"{Lang.log_critical_config_file_too_big_tell_shrink} 5120ms", LogSeverity.Critical).Wait();

                Exit.Service();
            }

            try
            {
                ParseSettings(ref configLines);

                VerifyMandatorySettings();
            }
            catch (Exception ex)
            {
                SRV.FastLog("Initiator", $"Unknow error while loading config, terminating in 5120ms", LogSeverity.Critical).Wait();
                SRV.FastLog("Initiator", $"Message: {ex.Message}\n\nSource: {ex.Source}\n\nStackTrace: {ex.StackTrace}", LogSeverity.Verbose).Wait();

                Exit.Service();
            }
        }

        //# # # # # # # # # # # # # # # # # # #

        private static List<String> ReadConfigFile()
        {
            List<String> configLines = new();

            try
            {
                if (!File.Exists($"{Program.assemblyPath}\\config.txt"))
                {
                    CreateConfigTemplate();

                    SRV.FastLog("Initiator", $"{Lang.log_verbose_tell_config_was_created} 5120ms", LogSeverity.Verbose).Wait();

                    Exit.Service();
                }

                using (StreamReader streamReader = new($"{Program.assemblyPath}\\config.txt", Encoding.UTF8))
                {
                    String item;

                    while ((item = streamReader.ReadLine()) != null)
                    {
                        configLines.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                SRV.FastLog("Initiator", $"{Lang.log_critical_config_read_or_create_error} 5120ms", LogSeverity.Critical).Wait();
                SRV.FastLog("Initiator", ex.Message, LogSeverity.Verbose).Wait();

                Exit.Service();
            }

            return configLines;
        }

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

                if (CurrentConfig.consoleUser == null)
                {
                    Match match = Regex.Match(configLines[b], Pattern.consoleUser, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        try
                        {
                            PipeSecurity pipeSecurity = new();
                            pipeSecurity.SetAccessRule(new PipeAccessRule(new NTAccount(Environment.MachineName, match.Groups[1].Value), PipeAccessRights.FullControl, AccessControlType.Allow));

                            CurrentConfig.consoleUser = match.Groups[1].Value;
                        }
                        catch
                        {
                            Error(Lang.log_critical_config_read_invalid_username);
                        }
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
                            Error(Lang.log_critical_config_read_invalid_cmdRegisterOnBoot_Value);
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
                            Error("Unable to parse tcpListenPort, terminating in");
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
                            Error("Unable to parse tcpListenIP, terminating in");
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

        private static void VerifyMandatorySettings()
        {
            if (CurrentConfig.tokenPath == null)
            {
                Error(Lang.log_critical_config_read_tokenPath_not_found);
            }

            if (CurrentConfig.consoleUser == null)
            {
                Error(Lang.log_critical_config_read_allowedConsoleUser_not_found);
            }

            if (CurrentConfig.discordAdmin == null)
            {
                Error(Lang.log_critical_config_read_discordAdminID_not_found);
            }

            if (CurrentConfig.guildID == null)
            {
                Error(Lang.log_critical_config_read_guild_not_specified);
            }

            if (CurrentConfig.tcpListenerPort == null)
            {
                Error("Unable to find tcpListenPort in config, terminating in");
            }

            if (CurrentConfig.tcpListenerIP == null)
            {
                Error("Unable to find tcpListenIP in config, terminating in");
            }
        }

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
                        SRV.FastLog("Initiator", $"Invalid configuration for '{name}', only one endpoint per channel allowed, skipping '{name}'", LogSeverity.Error).Wait();
                    }
                }
                catch
                {
                    Error(Lang.log_critical_config_read_invalid_vmChannelLink);
                }
            }
        }

        //# # # # # # # # # # # # # # # # # # #

        private static void Error(String msg)
        {
            SRV.FastLog("Initiator", $"{msg} 5120ms", LogSeverity.Critical).Wait();

            Exit.Service();
        }
    }
}