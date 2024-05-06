using System;
using System.Net;
using System.Threading;

namespace Link_Slave
{
    internal struct WorkerThread
    {
        internal volatile static Boolean Worker_WasCanceled = false;
        internal static Thread Worker = null;
    }

    internal struct CurrentConfig
    {
        internal readonly static Byte[] HMAC_Key = new Byte[64];
        internal readonly static Byte[] AES_Key = new Byte[32];

        internal static UInt16 TcpPort = 0;
        internal static IPAddress ServerIP = null;

        internal static UInt64 ChannelID = 0;
        internal static Guid Guid = Guid.Empty;

        internal static String Name = null;
        internal static xVersion ServerVersion;

        internal static String ScriptDirectory = null;
    }
}