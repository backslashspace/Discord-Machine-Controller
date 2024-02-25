using System;

namespace DC_SRV_VM_LINK.Bot
{
    internal struct Lang
    {
        //discord
        internal const String no_perm = "You have no permissions to execute this command.";
        internal static String no_perm_hint_admin = null;

        internal const String cmd_execution_error = "Failed to execute";

        internal const String cmd_help_no = "no";

        internal const String cmd_lock_description = "Deactivates all commands for all users except 263738287364112384.";

        internal const String cmd_unlock_description = "Reactivates all commands for all users.";

        internal const String cmd_upload_description = "Allows you to upload a referenced file to the configured location.";
        internal const String cmd_upload_parameter_description = "file to be uploaded into the configured vm directory ";
        internal const String cmd_upload_error = "Encountered an error while parsing the attachment, please contact your administrator providing this useless error message: 22_ERROR_NOT_DOS_DISK`";

        internal const String cmd_help_description = "me when reading tis diskrypschn: ＼(ﾟｰﾟ＼)";

        internal const String cmd_list_scripts_description = "Shows a list containing all files in the configured script directory.";
        internal const String cmd_execute_scripts_description = "Executes a given script.";

        //console
        internal const String log_verbose_tell_config_was_created = "Missing config file, created a template in assembly file location, terminating in";

        internal const String log_critical_config_read_or_create_error = "Failed to read config file, terminating in";

        internal const String log_critical_config_read_invalid_vmChannelLink = "Failed to parse config (vmChannelLink), terminating in";

        internal const String log_critical_config_read_guild_not_specified = "Unable to find guildID in config, terminating in";

        internal const String log_critical_config_read_guild_invalid = "invalid guildID in config, terminating in";

        internal const String log_critical_config_read_parse_guildID = "Failed parse guildID from config, terminating in";
        internal const String log_critical_config_read_parse_logChannelID = "Failed parse logChannelID from config, terminating in";
        internal const String config_critical_unable_find_logChannel = "Unable to find specified channel, terminating in";

        internal const String log_critical_config_read_invalid_username = "Invalid console viewer user, terminating in";

        internal const String log_critical_config_read_invalid_cmdRegisterOnBoot_Value = "Invalid cmdRegisterOnBoot value, terminating in";

        internal const String log_critical_config_read_invalid_discordAdminID = "Invalid discordAdminID, terminating in";

        internal const String log_critical_config_read_discordAdminID_not_found = "Unable to find discordAdminUserID variable in config, terminating in";

        internal const String log_critical_config_read_tokenPath_not_found = "Unable to find tokenPath variable in config, terminating in";
        internal const String log_critical_config_read_allowedConsoleUser_not_found = "Unable to find allowedConsoleUser variable in config, terminating in";

        internal const String log_critical_cannot_create_or_access_log_dir = "Unable to access or create log directory, terminating in";

        internal const String log_critical_config_file_too_big_tell_shrink = "Consider storing config non-mandatory data in a different file, terminating in";

        internal const String log_critical_no_token = "Failed to load token from disk, terminating in";

        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # 






    }
}