using Discord.Net;
using Discord.WebSocket;
using Discord;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace DC_SRV_VM_LINK.Bot
{
    internal static partial class SRV
    {
        private static async Task RegisterCommands()
        {
            await FastLog("Initiator", "Initializing", LogSeverity.Info);

            SocketGuild guild = dc_client.GetGuild((UInt64)CurrentConfig.guildID);

            await FastLog("Initiator", "Deregistering commands", LogSeverity.Info);
            await guild.DeleteApplicationCommandsAsync();

            await FastLog("Initiator", "Registering commands", LogSeverity.Info);

            SlashCommandBuilder[] guildCommand = new SlashCommandBuilder[7];
            for (Byte b = 0; b < guildCommand.Length; ++b)
            {
                guildCommand[b] = new SlashCommandBuilder();
            }

            guildCommand[0].WithName("lock");
            guildCommand[0].WithDescription(Lang.cmd_lock_description);

            guildCommand[1].WithName("unlock");
            guildCommand[1].WithDescription(Lang.cmd_unlock_description);

            guildCommand[2].WithName("upload");
            guildCommand[2].WithDescription(Lang.cmd_upload_description);
            guildCommand[2].AddOption("attachment", ApplicationCommandOptionType.Attachment, Lang.cmd_upload_parameter_description, isRequired: true);

            guildCommand[3].WithName("help");
            guildCommand[3].WithDescription(Lang.cmd_help_description);

            guildCommand[4].WithName("list-scripts");
            guildCommand[4].WithDescription(Lang.cmd_list_scripts_description);

            guildCommand[5].WithName("execute-scripts");
            guildCommand[5].WithDescription(Lang.cmd_execute_scripts_description);
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

                await FastLog("Initiator", json, LogSeverity.Critical);
            }
        }
    }
}