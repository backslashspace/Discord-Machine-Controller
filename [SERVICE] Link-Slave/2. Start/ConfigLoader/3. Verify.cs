using System;

namespace Link_Slave.Control
{
    internal static partial class ConfigLoader
    {
        private static void VerifyMandatorySettings()
        {
            if (CurrentConfig.ScriptDirectory == null)
            {
                Error("Unable to find script directory in config, terminating");
            }

            if (CurrentConfig.TcpPort == 0)
            {
                Error("Unable to find server port in config, terminating");
            }

            if (CurrentConfig.ServerIP == null)
            {
                Error("Unable to find server IP in config, terminating");
            }

            if (CurrentConfig.ChannelID == 0)
            {
                Error("Unable to find linked channel ID in config, terminating");
            }

            if (CurrentConfig.Guid == Guid.Empty)
            {
                Error("Unable to find client guid in config, terminating");
            }

            if (CurrentConfig.Name == null)
            {
                Error("Unable to find client name in config, terminating");
            }
        }
    }
}