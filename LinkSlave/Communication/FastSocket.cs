using System;
using System.IO;
using System.Net.Sockets;

namespace VMLink_Slave
{
    internal static class FastSocket
    {
        internal static Boolean SendTCP(ref Socket socket, Byte[] data, Boolean throwOnError = true)
        {
            try
            {
                Byte[] dataBufferSize = new Byte[4];
                dataBufferSize = BitConverter.GetBytes(data.Length);

                socket.Send(dataBufferSize);

                Byte[] confirmBuffer = new Byte[1];
                socket.Receive(dataBufferSize);
                if (dataBufferSize[0] != 0b01010101)
                {
                    throw new InvalidDataException("Received invalid sync response");
                }

                socket.Send(data);

                return true;
            }
            catch
            {
                if (!throwOnError)
                {
                    return false;
                }

                throw;
            }
        }

        internal static (Byte[], Boolean) ReceiveTCP(ref Socket socket, Boolean throwOnError = true)
        {
            try
            {
                Byte[] rawDataBufferSize = new Byte[4];
                socket.Receive(rawDataBufferSize);
                Int32 dataBufferSize = BitConverter.ToInt32(rawDataBufferSize, 0);

                socket.Send(new Byte[] { 0b01010101 });

                Byte[] data = new Byte[dataBufferSize];
                socket.Receive(data);

                return (data, true);
            }
            catch
            {
                if (!throwOnError)
                {
                    return (new Byte[0], false);
                }

                throw;
            }
        }
    }
}