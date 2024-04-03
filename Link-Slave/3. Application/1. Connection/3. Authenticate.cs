using System;
using System.Net.Sockets;
using System.Text;
using System.IO;

namespace Link_Slave.Worker
{
    internal static partial class Client
    {
        private static Boolean Authenticate()
        {
            //send name length in plain
            //send id and name encrypted

            Byte[] buffer = new Byte[] { (Byte)CurrentConfig.Name.Length };

            try
            {
                //send name length
                Int32 sendBytes = socket.Receive(buffer, 0, 1, SocketFlags.None);

                if (sendBytes != 1)
                {
                    Log.FastLog("Connection", $"Failed to send name length during authentication", xLogSeverity.Error);

                    return false;
                }

                //send id + name
                buffer = new Byte[308];
                Buffer.BlockCopy(BitConverter.GetBytes(CurrentConfig.ChannelID), 0, buffer, 0, 4);
                Buffer.BlockCopy(Encoding.UTF8.GetBytes(CurrentConfig.Name), 0, buffer, 4, 304);

                if (socket.Receive(buffer, 0, 308, SocketFlags.None) != 308)
                {
                    Log.FastLog("Connection", $"Failed to send id and name during authentication, not all bytes were send", xLogSeverity.Error);

                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                if (ex is SocketException e)
                {
                    Log.FastLog("Connection", $"Lost connection during authentication: {e.SocketErrorCode}", xLogSeverity.Warning);
                }
                else if (ex is InvalidDataException ea)
                {
                    Log.FastLog("Connection", $"A data error occurred during authentication: {ea.Message}", xLogSeverity.Error);
                }
                else
                {
                    Log.FastLog("Connection", $"An unknown Error occurred during authentication: {ex.Message}", xLogSeverity.Error);
                }

                return false;
            }
        }
    }
}