using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Link_Master.Worker
{
    internal struct Client
    {
        internal static DiscordSocketClient Discord;
        internal static Boolean IsConnected = false;
        internal static Boolean WasReadyOnce = false;
        internal static Boolean BlockNew = false;
    }

    internal struct WorkerThreads
    {
        internal static Thread LogWorker = null;
        internal static Thread LocalConsoleLogWorker = null;

        internal static Thread LinkManager = null;
        internal static List<Thread> Links = null;
    }

    internal struct CurrentConfig
    {
        internal static String __MESSAGE_no_perm_hint_admin = null;

        internal static String TokenPath = null;

        internal static String DiscordAdmin = null;
        internal static UInt64? DiscordAdminID = null;

        internal static UInt16? TcpListenerPort = null;
        internal static IPAddress TcpListenerIP = null;

        internal static UInt64? GuildID = null;

        internal static Boolean? CmdRegisterOnBoot = null;

        internal static SocketTextChannel LogChannel = null;

        internal static Dictionary<UInt64?, ChannelLink> MachineChannelLink = new();
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