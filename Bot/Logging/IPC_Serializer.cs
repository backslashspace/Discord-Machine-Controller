using Discord;
using System;
using System.IO;
using System.Runtime.Serialization;

namespace DC_SRV_VM_LINK.Bot
{
    internal static class IPCData
    {
        [DataContract(Name = "IPCData", Namespace = "")]
        internal readonly struct ExtendedLogMessage
        {
            internal ExtendedLogMessage(String source, String message, LogSeverity severity, DateTime timeStamp)
            {
                Source = source;
                Message = message;
                Severity = severity;
                TimeStamp = timeStamp;
            }

            [DataMember]
            internal readonly String Source;

            [DataMember]
            internal readonly String Message;

            [DataMember]
            internal readonly LogSeverity Severity;

            [DataMember]
            internal readonly DateTime TimeStamp;
        }
    }

    internal static partial class IPCAdapter
    {
        private static Byte[] Serialize(Object data)
        {
            using MemoryStream memoryStream = new();
            using StreamReader streamReader = new(memoryStream);

            DataContractSerializer serializer = new(data.GetType());

            serializer.WriteObject(memoryStream, data);

            memoryStream.Position = 0;

            Byte[] output = new Byte[memoryStream.Length];

            memoryStream.Read(output, 0, (Int32)memoryStream.Length);

            return output;
        }
    }
}