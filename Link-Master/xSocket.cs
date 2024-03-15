using System;
using System.IO;
using System.Net.Sockets;

namespace Link_Master
{
    internal static class xSocket
    {
        internal static void TCP_Send(ref Socket socket, ref Byte[] data, Boolean throwOnError = true)
        {
            try
            {
                Byte[] bufferSize = BitConverter.GetBytes(data.Length);
                socket.Send(bufferSize);

                Int32 pushedBytes = socket.Send(data);

                if (pushedBytes != data.Length)
                {
                    throw new InvalidDataException($"Not all bytes were transmitted ({pushedBytes}/{data.Length})\nSocket.SendTimeout was: {socket.SendTimeout}\nSocket.ReceiveTimeout was: {socket.ReceiveTimeout}\n");
                }
            }
            catch
            {
                if (throwOnError)
                {
                    throw;
                }
            }
        }

        internal static void TCP_Receive(ref Socket socket, out Byte[] buffer, Int32 workingBufferSize = 262144, Boolean throwOnError = true)
        {
            try
            {
                Byte[] bufferSize = new Byte[4];
                socket.Receive(bufferSize);

                Int32 remainingBuffer = BitConverter.ToInt32(bufferSize, 0);
                buffer = new Byte[remainingBuffer];

                //

                Int32 receivedBytes = 0;

                Byte[] workingBuffer = new Byte[workingBufferSize];

                if (remainingBuffer < workingBufferSize)
                {
                    receivedBytes = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);

                    remainingBuffer -= receivedBytes;
                }
                else
                {
                    while (remainingBuffer > 0)
                    {
                        if (remainingBuffer < workingBufferSize)
                        {
                            workingBuffer = new Byte[remainingBuffer];
                        }

                        receivedBytes = socket.Receive(workingBuffer, 0, workingBuffer.Length, SocketFlags.None);

                        Buffer.BlockCopy(workingBuffer, 0, buffer, buffer.Length - remainingBuffer, workingBuffer.Length);

                        remainingBuffer -= receivedBytes;
                    }
                }

                if (remainingBuffer != 0)
                {
                    if (buffer.Length > 1073741796)
                    {
                        throw new InvalidDataException($"Not all bytes were received ({remainingBuffer - buffer.Length}/{buffer.Length})\nSocket.SendTimeout was: {socket.SendTimeout}\nSocket.ReceiveTimeout was: {socket.ReceiveTimeout}\nNot enough RAM?\n\n");
                    }
                    else
                    {
                        throw new InvalidDataException($"Not all bytes were received ({remainingBuffer - buffer.Length}/{buffer.Length})\nSocket.SendTimeout was: {socket.SendTimeout}\nSocket.ReceiveTimeout was: {socket.ReceiveTimeout}\n\n");
                    }
                }
            }
            catch
            {
                if (throwOnError)
                {
                    throw;
                }

                buffer = new Byte[0];
            }
        }
    }
}
