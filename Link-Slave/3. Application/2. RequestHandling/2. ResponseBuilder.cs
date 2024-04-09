using System;
using System.Text;

namespace Link_Slave.Worker
{
    internal static partial class Client
    {
        private static Byte[] ServerResponseBuilder(ref String text, ref Color color)
        {
            Byte[] rawText = Encoding.UTF8.GetBytes(text);

            Byte[] response = new Byte[rawText.Length + 4];

            Buffer.BlockCopy(BitConverter.GetBytes((UInt32)color), 0, response, 0, 4);
            Buffer.BlockCopy(rawText, 0, response, 4, rawText.Length);

            return response;
        }
    }
}