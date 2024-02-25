using System;
using System.Net;

namespace VMLink_Slave
{
    internal struct CurrentConfig
    {
        internal static Byte[] HMAC_Key = new Byte[64];
        internal static Byte[] AES_Key = new Byte[32];

        internal static UInt16 tcpPort = 0;
        internal static IPAddress serverIP = null;

        internal static UInt64 channelID = 0;
        internal static Guid guid = Guid.Empty;

        internal static String scriptDirectory = null;
    }
}