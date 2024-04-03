using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Link_Slave.Control
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

                Boolean gotKeys = false;

                if (CurrentConfig.ScriptDirectory == null)
                {
                    Match match = Regex.Match(configLines[b], Pattern.scriptDirectory, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        try
                        {
                            if (Directory.Exists(match.Groups[1].Value))
                            {
                                CurrentConfig.ScriptDirectory = match.Groups[1].Value;

                                continue;
                            }
                        }
                        catch
                        {
                            Error("Unable to parse or find script directory on disk, make sure the folder exists and is accessible, terminating");
                        }
                    }
                }

                if (CurrentConfig.TcpPort == 0)
                {
                    Match match = Regex.Match(configLines[b], Pattern.tcpPort, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        try
                        {
                            CurrentConfig.TcpPort = UInt16.Parse(match.Groups[1].Value);

                            continue;
                        }
                        catch
                        {
                            Error("Unable to parse server port, terminating");
                        }
                    }
                }

                if (CurrentConfig.ServerIP == null)
                {
                    Match match = Regex.Match(configLines[b], Pattern.serverIP, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        try
                        {
                            CurrentConfig.ServerIP = IPAddress.Parse(match.Groups[1].Value);

                            continue;
                        }
                        catch
                        {
                            Error("Unable to parse server IP, terminating");
                        }
                    }
                }

                if (CurrentConfig.ChannelID == 0)
                {
                    Match match = Regex.Match(configLines[b], Pattern.channelID, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        try
                        {
                            CurrentConfig.ChannelID = UInt64.Parse(match.Groups[1].Value);

                            continue;
                        }
                        catch
                        {
                            Error("Unable to parse linked channel ID, terminating");
                        }
                    }
                }

                if (CurrentConfig.Guid == Guid.Empty)
                {
                    Match match = Regex.Match(configLines[b], Pattern.guid, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        try
                        {
                            CurrentConfig.Guid = Guid.Parse(match.Groups[1].Value);

                            continue;
                        }
                        catch
                        {
                            Error("Unable to parse client guid, terminating");
                        }
                    }
                }

                if (CurrentConfig.Name == null)
                {
                    Match match = Regex.Match(configLines[b], Pattern.name, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        try
                        {
                            CurrentConfig.Name = match.Groups[1].Value;

                            if (CurrentConfig.Name.Length > 255)
                            {
                                Error("name variable is too long, terminating");
                            }

                            continue;
                        }
                        catch
                        {
                            Error("Unable to parse name length, terminating");
                        }
                    }
                }

                if (!gotKeys)
                {
                    Match match = Regex.Match(configLines[b], Pattern.keys, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        try
                        {
                            Byte[] keys = Convert.FromBase64String(match.Groups[1].Value);

                            Buffer.BlockCopy(keys, 0, CurrentConfig.HMAC_Key, 0, 64);
                            Buffer.BlockCopy(keys, 63, CurrentConfig.AES_Key, 0, 32);

                            gotKeys = true;

                            continue;
                        }
                        catch
                        {
                            Error($"Unable to parse connection keys, terminating");

                            throw;
                        }
                    }
                }
            }
        }
    }
}