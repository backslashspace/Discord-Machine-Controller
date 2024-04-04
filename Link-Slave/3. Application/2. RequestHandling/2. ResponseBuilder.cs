using System;
using System.Text;

namespace Link_Slave.Worker
{
    internal static partial class Client
    {
        private static Byte[] ServerResponseBuilder(ref String text, ref Color color)
        {
            Byte[] response = new Byte[text.Length + 4];

            Buffer.BlockCopy(BitConverter.GetBytes((UInt32)color), 0, response, 0, 4);
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(text), 0, response, 4, response.Length - 4);

            return response;
        }
    }
}