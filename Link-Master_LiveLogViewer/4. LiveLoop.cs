using System;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace LogViewer
{
    internal static partial class Program
    {
        private static void UpdateLoop(ref NamedPipeClientStream pipeClient)
        {
            pipeClient.WriteByte((Byte)RequestType.SendUpdates);

            IPCLogMessage log;

            Byte[] bufferSize = new Byte[4];
            Byte[] buffer;

            while (true)
            {
                switch (pipeClient.ReadByte())
                {
                    case (Byte)RequestType.DataAvailable:
                        break;

                    case (Byte)RequestType.DataNotAvailable:
                        Task.Delay(256).Wait();
                        continue;

                    case (Byte)RequestType.Goodbye:
                        break;

                    default:
                        throw new Exception();
                }

                pipeClient.Read(bufferSize, 0, 4);

                buffer = new Byte[BitConverter.ToInt32(bufferSize, 0)];
                pipeClient.Read(buffer, 0, buffer.Length);

                log = (IPCLogMessage)Deserialize(buffer, typeof(IPCLogMessage));

                PrintLog(log);
            }
        }
    }
}