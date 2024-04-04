using System;

namespace Link_Slave.Worker
{
    internal static partial class Client
    {
        private static void NotImplemented(ref Byte requestType)
        {
            Color responseColor = Color.Purple;
            String responseMessage = $"Endpoint message: Unknown server request ({requestType:X}), version mismatch?";

            Byte[] rawResponse = ServerResponseBuilder(ref responseMessage, ref responseColor);

            AES_TCP.Send(ref socket, ref rawResponse, CurrentConfig.AES_Key, CurrentConfig.HMAC_Key);
        }
    }
}