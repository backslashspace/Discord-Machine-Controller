using System;

namespace Link_Slave.Worker
{
    internal static partial class Client
    {
        private static void KeepAlive(ref Byte[] buffer, ref Byte errorCode)
        {
            buffer = new Byte[] { (Byte)RequestTypes.YuesAmIAlive };

            AES_TCP.RefSend(ref errorCode, ref socket, ref buffer, CurrentConfig.AES_Key, CurrentConfig.HMAC_Key);
        }
    }
}