namespace Link_Master.Worker.Control
{
    internal static partial class ConfigLoader
    {
        private static void VerifyMandatorySettings()
        {
            if (CurrentConfig.tokenPath == null)
            {
                Error("Unable to find tokenPath variable in config, terminating");
            }

            if (CurrentConfig.discordAdmin == null)
            {
                Error("Unable to find discordAdminUserID variable in config, terminating");
            }

            if (CurrentConfig.guildID == null)
            {
                Error("Unable to find guildID in config, terminating");
            }

            if (CurrentConfig.tcpListenerPort == null)
            {
                Error("Unable to find tcpListenPort in config, terminating");
            }

            if (CurrentConfig.tcpListenerIP == null)
            {
                Error("Unable to find tcpListenIP in config, terminating");
            }
        }
    }
}