using LinkSlave;
using LinkSlave.Win;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace VMLink_Slave
{
    internal static partial class Config
    {
        internal static void Load()
        {
            Client.assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Log.Print($"Starting version v{Program.Version}", LogSeverity.Info);
            Log.Print("Loading config", LogSeverity.Info);

            List<String> configLines = ReadConfigFile();

            if (configLines.Count > 128)
            {
                Log.Print("Found more than 128 lines in the config file, consider storing any non-config data in a separate file, terminating.", LogSeverity.Error);

                Client.Exit();
            }

            ParseConfig(ref configLines);

            Verify();

            Log.Print("Successfully loaded config", LogSeverity.Info);
        }

        //# # # # # # # # # # # # # # # # # # # # # # # # #

        private static List<String> ReadConfigFile()
        {
            List<String> configLines = new();

            try
            {
                if (!File.Exists($"{Client.assemblyPath}\\config.txt"))
                {
                    Log.Print($"No config file was found, tell your administrator to create one, terminating.", LogSeverity.Critical);

                    Client.Exit();
                }

                using (StreamReader streamReader = new($"{Client.assemblyPath}\\config.txt", Encoding.UTF8))
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
                Log.Print($"An unknown error occurred while reading the config file\n\n{ex.Message}\n\n{ex.StackTrace}\n\n, terminating.", LogSeverity.Critical);

                Client.Exit();
            }

            return configLines;
        }

        private static void ParseConfig(ref List<String> configLines)
        {
            try
            {
                Boolean gotKeys = false;

                for (Byte b = 0; b < configLines.Count; ++b)
                {
                    if (configLines[b] == "" || configLines[b][0] == '#')
                    {
                        continue;
                    }

                    if (CurrentConfig.scriptDirectory == null)
                    {
                        Match match = Regex.Match(configLines[b], Pattern.scriptDirectory, RegexOptions.IgnoreCase);

                        if (match.Success)
                        {
                            try
                            {
                                if (Directory.Exists(match.Groups[1].Value))

                                CurrentConfig.scriptDirectory = match.Groups[1].Value;

                                continue;
                            }
                            catch
                            {
                                Log.Print("Unable to parse or find script directory on disk make sure the folder exists and is accessible, terminating.", LogSeverity.Critical);
                            }
                        }
                    }

                    if (CurrentConfig.tcpPort == 0)
                    {
                        Match match = Regex.Match(configLines[b], Pattern.tcpPort, RegexOptions.IgnoreCase);

                        if (match.Success)
                        {
                            try
                            {
                                CurrentConfig.tcpPort = UInt16.Parse(match.Groups[1].Value);

                                continue;
                            }
                            catch
                            {
                                Log.Print("Unable to parse server port, terminating.", LogSeverity.Critical);
                            }
                        }
                    }

                    if (CurrentConfig.serverIP == null)
                    {
                        Match match = Regex.Match(configLines[b], Pattern.serverIP, RegexOptions.IgnoreCase);

                        if (match.Success)
                        {
                            try
                            {
                                CurrentConfig.serverIP = IPAddress.Parse(match.Groups[1].Value);

                                continue;
                            }
                            catch
                            {
                                Log.Print("Unable to parse server IP, terminating.", LogSeverity.Critical);
                            }
                        }
                    }

                    if (CurrentConfig.channelID == 0)
                    {
                        Match match = Regex.Match(configLines[b], Pattern.channelID, RegexOptions.IgnoreCase);

                        if (match.Success)
                        {
                            try
                            {
                                CurrentConfig.channelID = UInt64.Parse(match.Groups[1].Value);

                                continue;
                            }
                            catch
                            {
                                Log.Print("Unable to parse linked channel ID, terminating.", LogSeverity.Critical);
                            }
                        }
                    }

                    if (CurrentConfig.guid == Guid.Empty)
                    {
                        Match match = Regex.Match(configLines[b], Pattern.guid, RegexOptions.IgnoreCase);

                        if (match.Success)
                        {
                            try
                            {
                                CurrentConfig.guid = Guid.Parse(match.Groups[1].Value);

                                continue;
                            }
                            catch
                            {
                                Log.Print("Unable to parse client guid, terminating.", LogSeverity.Critical);
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
                                Span<Byte> keys = stackalloc Byte[96];
                                keys = Convert.FromBase64String(match.Groups[1].Value);

                                CurrentConfig.HMAC_Key = keys.Slice(0, 64).ToArray();
                                CurrentConfig.AES_Key = keys.Slice(64, 32).ToArray();

                                gotKeys = true;

                                continue;
                            }
                            catch
                            {
                                Log.Print($"No config file was found, tell your administrator to create one, terminating.", LogSeverity.Critical);

                                throw;
                            }
                        }
                    }
                }

                if (!gotKeys)
                {
                    Log.Print("Unable to find encryption keys in config, terminating.", LogSeverity.Critical);
                }
            }
            catch
            {
                Client.Exit();
            }
        }

        private static void Verify()
        {
            Boolean iniHasComeToAnEnd = false;

            if (CurrentConfig.scriptDirectory == null)
            {
                Log.Print("Unable to find script directory in config.", LogSeverity.Critical);

                iniHasComeToAnEnd = true;
            }

            if (CurrentConfig.tcpPort == 0)
            {
                Log.Print("Unable to find server port in config.", LogSeverity.Critical);

                iniHasComeToAnEnd = true;
            }

            if (CurrentConfig.serverIP == null)
            {
                Log.Print("Unable to find server IP in config.", LogSeverity.Critical);

                iniHasComeToAnEnd = true;
            }

            if (CurrentConfig.channelID == 0)
            {
                Log.Print("Unable to find linked channel ID in config.", LogSeverity.Critical);

                iniHasComeToAnEnd = true;
            }

            if (CurrentConfig.guid == Guid.Empty)
            {
                Log.Print("Unable to find client guid in config.", LogSeverity.Critical);

                iniHasComeToAnEnd = true;
            }

            if (iniHasComeToAnEnd)
            {
                Log.Print("Invalid configuration, terminating.", LogSeverity.Verbose);

                Client.Exit();
            }
        }
    }
}