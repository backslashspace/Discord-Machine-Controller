using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Net;

namespace DC_SRV_VM_LINK.Bot
{
    internal struct CurrentConfig
    {
        internal static String tokenPath = null;

        internal static String consoleUser = null;

        internal static String discordAdmin = null;
        internal static UInt64? discordAdminID = null;

        internal static UInt16? tcpListenerPort = null;
        internal static IPAddress tcpListenerIP = null;

        internal static UInt64? guildID = null;

        internal static Boolean? cmdRegisterOnBoot = null;

        internal static SocketTextChannel logChannel = null;

        internal static Dictionary<UInt64?, ChannelLink> vmChannelLink = new();
    }

    internal struct ChannelLink
    {
        internal ChannelLink(ref String name, ref Guid guid, ref UInt64 channelID, ref Span<Byte> keys)
        {
            Name = name;
            Guid = guid;
            ChannelID = channelID;
            IsLocked = false;

            HMAC_Key = keys.Slice(0, 64).ToArray();
            AES_Key = keys.Slice(64, 32).ToArray();
        }

        internal readonly String Name;
        internal readonly Guid Guid;
        internal readonly UInt64 ChannelID;
        internal Boolean IsLocked;

        internal readonly Byte[] HMAC_Key;
        internal readonly Byte[] AES_Key;
    }
}