using Discord;
using System;
using System.IO;
using System.Runtime.Serialization;

namespace Link_Master.Logging
{
    internal static partial class LogConsole
    {
        [DataContract(Name = "ConsoleMessage", Namespace = "")]
        internal readonly struct ConsoleMessage
        {
            internal ConsoleMessage(String source, String message, xLogSeverity severity, DateTime timeStamp)
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
            internal readonly xLogSeverity Severity;

            [DataMember]
            internal readonly DateTime TimeStamp;
        }

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