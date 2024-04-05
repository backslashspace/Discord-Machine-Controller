using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Link_Master
{
    internal struct Client
    {
        internal static DiscordSocketClient Discord;
        internal volatile static Boolean IsConnected = false;
        internal volatile static Boolean WasReadyAtLeastOnce = false;
        internal volatile static Boolean BlockNew = false;
    }

    internal struct WorkerThreads
    {
        internal volatile static Boolean LocalConsoleLogWorker_WasCanceled = false;
        internal static Thread LocalConsoleLogWorker = null;

        internal volatile static Boolean DiscordLogWorker_WasCanceled = false;
        internal static Thread DiscordLogWorker = null;

        internal static Thread LinkFactory = null;
        internal volatile static Boolean LinkFactory_WasCanceled = false;

        internal readonly static ConcurrentDictionary<UInt64 ,Link> Links = new();
    }

    internal readonly struct Link
    {
        internal Link(ref Thread thread, ref CancellationTokenSource tokenSource, ref Socket socket)
        {
            Worker = thread;
            CancelToken = tokenSource;
            Socket = socket;
        }

        internal readonly Thread Worker;
        internal readonly CancellationTokenSource CancelToken;
        internal readonly Socket Socket;
    }

    internal struct CurrentConfig
    {
        internal static String __MESSAGE_no_perm_hint_admin = null;

        internal static String TokenPath = null;

        internal static Boolean? AnnounceEndpointConnect = null;

        internal static String DiscordAdmin = null;
        internal static UInt64? DiscordAdminID = null;

        internal static UInt16? TcpListenerPort = null;
        internal static IPAddress TcpListenerIP = null;

        internal static UInt64? GuildID = null;

        internal static Boolean? CmdRegisterOnBoot = null;

        internal static SocketTextChannel LogChannel = null;

        internal static ConcurrentDictionary<UInt64, ChannelLink> MachineChannelLinks = new();
    }

    internal struct ChannelLink
    {
        internal ChannelLink(ref String name, ref Guid guid, ref UInt64 channelID, ref Byte[] aesKey, ref Byte[] hmacKey)
        {
            Name = name;
            Guid = guid;
            ChannelID = channelID;
            IsLocked = false;

            AES_Key = aesKey;
            HMAC_Key = hmacKey;
        }

        internal readonly String Name;
        internal readonly Guid Guid;
        internal readonly UInt64 ChannelID;
        internal volatile Boolean IsLocked;

        internal readonly Byte[] HMAC_Key;
        internal readonly Byte[] AES_Key;
    }
}