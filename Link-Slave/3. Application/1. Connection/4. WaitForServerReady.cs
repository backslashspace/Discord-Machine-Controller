using System;

namespace Link_Slave.Worker
{
    internal static partial class Client
    {
        private static Boolean WaitForReady()
        {
            try
            {
                Byte[] ready = AES_TCP.Receive(ref socket, CurrentConfig.AES_Key, CurrentConfig.HMAC_Key);

                if (ready.Length == 2 && ready[0] == 0b1010_1010 && ready[1] == 0b0101_0101)
                {
                    return true;
                }
            }
            catch
            {
                Log.FastLog("Connection", "Failed to receive 'Ready' packet, closing connection", xLogSeverity.Error);

                return false;
            }

            Log.FastLog("Connection", "Received invalid 'Ready' data, closing connection", xLogSeverity.Warning);

            return false;
        }
    }
}