namespace Link_Master.Worker.Control
{
    internal static partial class ConfigLoader
    {
        private static void VerifyMandatorySettings()
        {
            if (CurrentConfig.TokenPath == null)
            {
                Error("Unable to find tokenPath variable in config, terminating");
            }

            if (CurrentConfig.DiscordAdmin == null)
            {
                Error("Unable to find discordAdminUserID variable in config, terminating");
            }

            if (CurrentConfig.GuildID == null)
            {
                Error("Unable to find guildID in config, terminating");
            }

            if (CurrentConfig.TcpListenerPort == null)
            {
                Error("Unable to find tcpListenPort in config, terminating");
            }

            if (CurrentConfig.TcpListenerIP == null)
            {
                Error("Unable to find tcpListenIP in config, terminating");
            }
        }
    }
}