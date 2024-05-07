using System;
using System.Net;
using System.Reflection;

namespace Configurator
{
    internal static class State
    {
        internal static readonly Assembly Assembly = Assembly.GetExecutingAssembly();

        internal static Boolean InstallDirectoryIsPresent = false;

        internal static Boolean ServiceIsPresent = false;

        internal static Boolean MMCServiceIsPresent = false;

        internal static Boolean ServiceIsRunning = false;

        //

        internal static String RepoPath = Config.DefaultScriptRepo;
        internal static UInt32 ScriptTimeout = Config.DefaultScriptTimeout;
        internal static IPAddress ServerIP;
        internal static UInt16 ServerPort = Config.DefaultServerPort;
        internal static UInt64 ChannelID;
        internal static String Name = Environment.MachineName;
        internal static Guid Guid;
        internal static Byte[] Keys = new Byte[96];
    }
}