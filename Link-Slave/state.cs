using System;
using System.Net;
using System.Threading;

namespace Link_Slave
{
    internal struct WorkerThreads
    {
        internal volatile static Boolean LocalConsoleLogWorker_WasCanceled = false;
        internal static Thread LocalConsoleLogWorker = null;

        internal volatile static Boolean DiscordLogWorker_WasCanceled = false;
        internal static Thread DiscordLogWorker = null;

        internal static Thread LinkFactory = null;
        internal volatile static Boolean LinkFactory_WasCanceled = false;
    }

    internal struct CurrentConfig
    {
        internal readonly static Byte[] HMAC_Key = new Byte[64];
        internal readonly static Byte[] AES_Key = new Byte[32];

        internal static UInt16 TcpPort = 0;
        internal static IPAddress ServerIP = null;

        internal static UInt64 ChannelID = 0;
        internal static Guid Guid = Guid.Empty;

        internal static String ScriptDirectory = null;
    }
}