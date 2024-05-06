namespace Link_Master.Control
{
    internal static partial class ConfigLoader
    {
        private static void VerifyMandatorySettings()
        {
            if (CurrentConfig.TokenPath == null)
            {
                Error("Unable to find tokenPath variable in config, terminating");
            }

            if (CurrentConfig.DiscordAdminID == null)
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

            //defaults
            CurrentConfig.AnnounceEndpointConnect??= true;
            CurrentConfig.GatewayDebug ??= false;

            //notify
            if (Worker.Bot.LogChannelID == null)
            {
                Log.FastLog("Initiator", "Discord log channel not specified, stopping worker", xLogSeverity.Info);
                WorkerThreads.DiscordLogWorker_WasCanceled = true;
            }
        }
    }
}