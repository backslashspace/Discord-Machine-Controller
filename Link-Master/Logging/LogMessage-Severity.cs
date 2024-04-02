using System;

namespace Link_Master
{
    internal struct xLogMessage
    {
        internal xLogMessage(xLogSeverity severity, String source, String message, Exception exception = null)
        {
            Severity = severity;
            Source = source;
            Message = message;
            Exception = exception;
        }

        internal xLogSeverity Severity;

        internal String Source;
        internal String Message;

        internal Exception Exception;
    }

    public enum xLogSeverity
    {
        Critical = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Verbose = 4,
        Debug = 5,
        Alert = 6,
    }
}