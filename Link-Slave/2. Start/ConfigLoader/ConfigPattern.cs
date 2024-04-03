using System;

namespace Link_Slave.Control
{
    internal static partial class ConfigLoader
    {
        private struct Pattern
        {
            internal const String scriptDirectory = "scriptDirectory:\\s*\"(.+?)\"";

            internal const String tcpPort = "serverPort:\\s*\"(.+?)\"";

            internal const String serverIP = "serverIP:\\s*\"(.+?)\"";

            internal const String channelID = "channelID:\\s*\"(.+?)\"";

            internal const String guid = "guid:\\s*\"(.+?)\"";

            internal const String name = "name:\\s*\"(.+?)\"";

            internal const String keys = "keys:\\s*\"(.+?)\"";
        }
    }
}