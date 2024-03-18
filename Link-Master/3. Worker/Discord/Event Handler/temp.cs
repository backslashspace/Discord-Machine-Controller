using Discord.Net;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Link_Master.Worker.Control;
using System.Threading;

namespace Link_Master.Worker
{
    internal static partial class Bot
    {
        private static Task SlashCommandHandler(SocketSlashCommand command)
        {
            return Task.CompletedTask;
        }

        private static async Task OnReady()
        {
            if (Client.WasReadyOnce)
            {
                return;
            }

            Log.FastLog("Initiator", "Attempting verify discord related configs", LogSeverity.Info);
            VerifyConfig();
            Log.FastLog("Initiator", "Successfully loaded config", LogSeverity.Info);

            if (CurrentConfig.CmdRegisterOnBoot == true)
            {
                await RegisterCommands();
            }
 
            Log.FastLog("Initiator", "Service fully loaded", LogSeverity.Verbose);

            Client.WasReadyOnce = true;
        }

        private static Task GatewayDisconnected(Exception exception)
        {
            Client.IsConnected = false;

            return Task.CompletedTask;
        }

        private static Task GatewayConnected()
        {
            Client.IsConnected = true;

            return Task.CompletedTask;
        }













        private static async Task RegisterCommands()
        {
            Log.FastLog("Initiator", "Initializing", LogSeverity.Info);

            SocketGuild guild = Client.Discord.GetGuild((UInt64)CurrentConfig.GuildID);

            Log.FastLog("Initiator", "Deregistering commands", LogSeverity.Info);
            await guild.DeleteApplicationCommandsAsync();

            Log.FastLog("Initiator", "Registering commands", LogSeverity.Info);

            SlashCommandBuilder[] guildCommand = new SlashCommandBuilder[7];
            for (Byte b = 0; b < guildCommand.Length; ++b)
            {
                guildCommand[b] = new SlashCommandBuilder();
            }

            guildCommand[0].WithName("lock");
            guildCommand[0].WithDescription("Deactivates commands for normal users.");

            guildCommand[1].WithName("unlock");
            guildCommand[1].WithDescription("Reactivates all commands for normal users.");

            guildCommand[2].WithName("upload");
            guildCommand[2].WithDescription("Allows you to upload files to your configured location.");
            guildCommand[2].AddOption("attachment", ApplicationCommandOptionType.Attachment, "file to be uploaded into the configured machine directory", isRequired: true);

            guildCommand[3].WithName("help");
            guildCommand[3].WithDescription("me when reading tis diskrypschn: ＼(ﾟｰﾟ＼)");

            guildCommand[4].WithName("list-scripts");
            guildCommand[4].WithDescription("Shows a list containing all files in the configured script directory.");

            guildCommand[5].WithName("execute-scripts");
            guildCommand[5].WithDescription("Executes a given script.");
            guildCommand[5].AddOption("filename", ApplicationCommandOptionType.String, "Name of the script to be executed.", isRequired: true);

            guildCommand[6].WithName("vdebug");
            guildCommand[6].WithDescription("lédebuugè");

            try
            {
                foreach (SlashCommandBuilder command in guildCommand)
                {
                    await guild.CreateApplicationCommandAsync(command.Build());
                }
            }
            catch (HttpException exception)
            {
                String json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

                Log.FastLog("Initiator", json, LogSeverity.Critical);
            }
        }
    }
}