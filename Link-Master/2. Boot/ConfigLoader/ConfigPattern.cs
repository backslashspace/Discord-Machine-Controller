using System;

namespace Link_Master.Control
{
    internal static partial class ConfigLoader
    {
        internal struct Pattern
        {
            internal const String tokenPath = "tokenPath:\\s*\"(.+?)\"";

            internal const String discordAdminUserID = "discordAdminUserID:\\s*\"(.+?)\"";

            internal const String cmdRegisterOnBoot = "cmdRegisterOnBoot:\\s*\"(.+?)\"";

            internal const String tcpListenerPort = "tcpListenerPort:\\s*\"(.+?)\"";

            internal const String tcpListenerIP = "tcpListenerIP:\\s*\"(.+?)\"";

            internal const String guildID = "guildID:\\s*\"(.+?)\"";

            internal const String logChannelID = "logChannelID:\\s*\"(.+?)\"";

            internal const String vmChannelLink = "vmChannelLink:\\s*\"(.+?):(.+?):(.+?):(.+?)\"";
        }
    }
}