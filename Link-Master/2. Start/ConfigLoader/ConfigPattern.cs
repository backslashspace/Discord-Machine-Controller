using System;

namespace Link_Master.Control
{
    internal static partial class ConfigLoader
    {
        internal struct Pattern
        {
            internal const String TokenPath = "tokenPath:\\s*\"(.+?)\"";

            internal const String DiscordAdminUserID = "discordAdminUserID:\\s*\"(.+?)\"";

            internal const String CmdRegisterOnBoot = "cmdRegisterOnBoot:\\s*\"(.+?)\"";

            internal const String TcpListenerPort = "tcpListenerPort:\\s*\"(.+?)\"";

            internal const String TcpListenerIP = "tcpListenerIP:\\s*\"(.+?)\"";
            internal const String DiscordAnnounceEndpointConnect = "discordAnnounceEndpointConnect:\\s*\"(.+?)\"";

            internal const String GuildID = "guildID:\\s*\"(.+?)\"";
            internal const String DiscordGatewayDebugMode = "discordGatewayDebugMode:\\s*\"(.+?)\"";

            internal const String LogChannelID = "logChannelID:\\s*\"(.+?)\"";

            internal const String VMChannelLink = "vmChannelLink:\\s*\"(.+?):(.+?):(.+?):(.+?)\"";
        }
    }
}