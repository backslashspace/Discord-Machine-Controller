using System;
using System.Net.Sockets;

namespace VMLink_Slave
{
    internal static class AES_FastSocket
    {
        internal static void SendTCP(ref Socket socket, Byte[] data, Byte[] key, Byte[] hmac_key)
        {
            FastSocket.SendTCP(ref socket, xAES.Pack(ref data, ref key, ref hmac_key));
        }

        internal static Byte[] ReceiveTCP(ref Socket socket, Byte[] key, Byte[] hmac_key)
        {
            return xAES.Unpack(FastSocket.ReceiveTCP(ref socket).Item1, ref key, ref hmac_key);
        }
    }
}