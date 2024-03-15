using System;
using System.Collections.Generic;
using System.IO.Pipes;

namespace LogViewer
{
    internal static partial class Program
    {
        private static List<IPCLogMessage> GetLatestLog(ref NamedPipeClientStream pipeClient)
        {
            pipeClient.WriteByte((Byte)RequestType.SendPastLog);
            pipeClient.WaitForPipeDrain();

            Byte[] bufferSize = new Byte[4];

            pipeClient.Read(bufferSize, 0, bufferSize.Length);

            Byte[] buffer = new Byte[BitConverter.ToInt32(bufferSize, 0)];
            pipeClient.Read(buffer, 0, buffer.Length);

            return (List<IPCLogMessage>)Deserialize(buffer, typeof(List<IPCLogMessage>));
        }
    }
}