using System;
using System.Net.Sockets;
//
using BSS.Encryption.Fips;
using Org.BouncyCastle.Crypto.Fips;
using System.Security;

namespace Link_Master.Worker
{
    internal static class AES_TCP
    {
        internal static void Send(ref Socket socket, ref Byte[] data, Byte[] key, Byte[] hmac_key)
        {
            Byte[] cipherData = Pack(ref data, ref key, ref hmac_key);

            xSocket.TCP_Send(ref socket, ref cipherData);
        }

        internal static Byte[] Receive(ref Socket socket, Byte[] key, Byte[] hmac_key)
        {
            xSocket.TCP_Receive(ref socket, out Byte[] cipherData);

            try
            {
                return UnPack(ref cipherData, ref key, ref hmac_key);
            }
            catch (SecurityException ex)
            {
                Log.FastLog("TCP-AES", "Security violation! failed to verify data integrity / authenticity, Error was: " + ex.Message, xLogSeverity.Alert);

                throw;
            }
        }

        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        internal static Byte[] Pack(ref Byte[] plainBytes, ref Byte[] key, ref Byte[] hmac_key)
        {
            xFips.SetApprovedOnlyMode(true);

            FipsSecureRandom random = xFips.GenerateSecureRandom();
            Byte[] iv = new Byte[16];
            random.NextBytes(iv);

            Byte[] cipherText = xAES_Fips.CTR.Encrypt(plainBytes, ref key,ref iv);
            Byte[] hmac = xHMAC_Fips.ComputeHMAC_512(ref cipherText, ref hmac_key);

            Byte[] transportableBytes = new Byte[hmac.Length + cipherText.Length];

            Buffer.BlockCopy(hmac, 0, transportableBytes, 0, hmac.Length);
            Buffer.BlockCopy(cipherText, 0, transportableBytes, hmac.Length, cipherText.Length);

            return transportableBytes;
        }

        internal static Byte[] UnPack(ref Byte[] cipherData, ref Byte[] key, ref Byte[] hmac_key)
        {
            xFips.SetApprovedOnlyMode(true);

            Byte[] packedHMAC = new Byte[64];
            Byte[] cipherText = new Byte[cipherData.Length - 64];

            Buffer.BlockCopy(cipherData, 0, packedHMAC, 0, 64);
            Buffer.BlockCopy(cipherData, 64, cipherText, 0, cipherText.Length);

            Byte[] cipherTextHMAC = xHMAC_Fips.ComputeHMAC_512(ref cipherText, ref hmac_key);

            for (Byte b = 0; b < packedHMAC.Length; ++b)
            {
                if (packedHMAC[b] != cipherTextHMAC[b])
                {
                    throw new SecurityException($"Received data HMAC did not match! Mismatch at position: {b}");
                }
            }

            return xAES_Fips.CTR.Decrypt(ref cipherText, ref key);
        }
    }
}