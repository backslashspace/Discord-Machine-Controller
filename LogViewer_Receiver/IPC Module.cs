using System;
using System.Runtime.Serialization;

namespace LogViewer
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

    internal enum LogSeverity
    {
        Critical = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Verbose = 4,
        Debug = 5
    }
}