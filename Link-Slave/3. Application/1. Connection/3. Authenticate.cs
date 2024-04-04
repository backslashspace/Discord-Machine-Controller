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
            Byte[] buffer = new Byte[] { (Byte)CurrentConfig.Name.Length };

            //socket.SendTimeout = 0;
            //socket.ReceiveTimeout = 0;

            try
            {
                if (!SendNameLength(ref buffer))
                {
                    return false;
                }

                if (!SendID_Name(ref buffer))
                {
                    return false;
                }

                if (!SendGuid(ref buffer))
                {
                    return false;
                }

                if (!VersionExchange(ref buffer))
                {
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

        //

        private static Boolean SendNameLength(ref Byte[] buffer)
        {
            if (socket.Send(buffer, 0, 1, SocketFlags.None) != 1)
            {
                Log.FastLog("Connection", $"Failed to send name length during authentication", xLogSeverity.Error);

                return false;
            }

            return true;
        }

        private static Boolean SendID_Name(ref Byte[] buffer)
        {
            buffer = new Byte[312];
            Buffer.BlockCopy(BitConverter.GetBytes(CurrentConfig.ChannelID), 0, buffer, 0, 8);
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(CurrentConfig.Name), 0, buffer, 8, CurrentConfig.Name.Length);

            try
            {
                AES_TCP.Send(ref socket, ref buffer, CurrentConfig.AES_Key, CurrentConfig.HMAC_Key);
            }
            catch (Exception ex)
            {
                Log.FastLog("Connection", $"Failed to send id and name during authentication, {ex.Message}", xLogSeverity.Error);

                return false;
            }

            return true;
        }

        private static Boolean SendGuid(ref Byte[] buffer)
        {
            buffer = CurrentConfig.Guid.ToByteArray();

            try
            {
                AES_TCP.Send(ref socket, ref buffer, CurrentConfig.AES_Key, CurrentConfig.HMAC_Key);
            }
            catch (Exception ex)
            {
                Log.FastLog("Connection", $"Failed to send guid during authentication, {ex.Message}", xLogSeverity.Error);

                return false;
            }

            return true;
        }

        private static Boolean VersionExchange(ref Byte[] buffer)
        {
            try
            {
                buffer = AES_TCP.Receive(ref socket, CurrentConfig.AES_Key, CurrentConfig.HMAC_Key);
            }
            catch (Exception ex)
            {
                Log.FastLog("Connection", $"Failed to receive server version during authentication, {ex.Message}", xLogSeverity.Error);

                return false;
            }

            CurrentConfig.ServerVersion = xVersion.GetXVersion(ref buffer);
            Log.FastLog("Connection", $"Server version: {CurrentConfig.ServerVersion}", xLogSeverity.Info);

            buffer = xVersion.GetBytes(ref Program.Version);

            try
            {
                AES_TCP.Send(ref socket, ref buffer, CurrentConfig.AES_Key, CurrentConfig.HMAC_Key);
            }
            catch (Exception ex)
            {
                Log.FastLog("Connection", $"Failed to send version during authentication, {ex.Message}", xLogSeverity.Error);

                return false;
            }

            return true;
        }
    }
}