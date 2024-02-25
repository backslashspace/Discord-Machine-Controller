using Pcg;
using System;
using System.IO;
using System.Security.Cryptography;

namespace DC_SRV_VM_LINK.Bot
{
    internal static class xAES
    {
        internal static Byte[] Pack(ref Byte[] data, ref Byte[] key, ref Byte[] hmac_key)
        {
            VerifyKeys(ref key, ref hmac_key);

            Byte[] cipherData = Encrypt(ref data, ref key);
            Byte[] hmac = HMAC(cipherData, ref hmac_key);

            Byte[] packedData = new Byte[cipherData.Length + 64];

            cipherData.CopyTo(packedData, 0);
            hmac.CopyTo(packedData, cipherData.Length);

            return packedData;
        }

        private static Byte[] Encrypt(ref Byte[] plainBytes, ref Byte[] key)
        {
            Byte[] IV = new Byte[16];
            Random random = new PcgRandom();
            random.NextBytes(IV);

            using Aes aesAlg = Aes.Create();
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.KeySize = 256;
            aesAlg.Key = key;
            aesAlg.IV = IV;
            aesAlg.Padding = plainBytes.Length % 16 == 0 ? PaddingMode.None : PaddingMode.PKCS7;

            ICryptoTransform transformer = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, transformer, CryptoStreamMode.Write);
            using (BinaryWriter binaryWriter = new(cryptoStream))
            {
                binaryWriter.Write(plainBytes);
            }

            Byte[] cipherText = memoryStream.ToArray();
            Byte[] output = new Byte[cipherText.Length + 17];

            output[0] = aesAlg.Padding == PaddingMode.None ? (Byte)0 : (Byte)1;
            cipherText.CopyTo(output, 1);
            IV.CopyTo(output, output.Length - 16);

            return output;
        }

        //

        internal static Byte[] Unpack(Byte[] packedCipherData, ref Byte[] key, ref Byte[] hmac_key)
        {
            if (packedCipherData.Length == 0)
            {
                throw new InvalidDataException("Received data length was 0");
            }

            VerifyKeys(ref key, ref hmac_key);

            Span<Byte> packedHmac = stackalloc Byte[64];
            Span<Byte> cipherData = packedCipherData.Length < 1089 ? stackalloc Byte[packedCipherData.Length - 64] : new Byte[packedCipherData.Length - 64];

            cipherData = packedCipherData.AsSpan().Slice(0, packedCipherData.Length - 64);
            packedHmac = packedCipherData.AsSpan().Slice(packedCipherData.Length - 64, 64);

            Byte[] hmac = HMAC(cipherData.ToArray(), ref hmac_key);

            CompareHmac(ref packedHmac, ref hmac);

            return Decrypt(ref cipherData, ref key);
        }

        private static void CompareHmac(ref Span<Byte> packedHmac, ref Byte[] hmac)
        {
            if (packedHmac.Length != hmac.Length)
            {
                throw new InvalidDataException($"HMAC length mismatch, packed length: [{packedHmac.Length}], data length: [{hmac.Length}]");
            }

            for (Byte b = 0; b < packedHmac.Length; ++b)
            {
                if (packedHmac[b] != hmac[b])
                {
                    throw new InvalidDataException($"HMAC mismatch at index [{b}], packed hmac: {packedHmac[b]}, data hmac: {hmac[b]}");
                }
            }
        }

        private static Byte[] Decrypt(ref Span<Byte> cipherData, ref Byte[] key)
        {
            Span<Byte> padding = cipherData.Slice(0, 1);
            Span<Byte> cipherText = cipherData.Slice(1, cipherData.Length - 17);
            Span<Byte> IV = cipherData.Slice(cipherData.Length - 16, 16);

            using Aes aesAlg = Aes.Create();
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.KeySize = 256;
            aesAlg.Key = key;
            aesAlg.IV = IV.ToArray();
            aesAlg.Padding = padding[0] == 0 ? PaddingMode.None : PaddingMode.PKCS7;

            ICryptoTransform transformer = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using MemoryStream memoryStream = new(cipherText.ToArray());
            using CryptoStream cryptoStream = new(memoryStream, transformer, CryptoStreamMode.Read);
            using BinaryReader binaryReader = new(cryptoStream);

            return binaryReader.ReadBytes(cipherText.Length);
        }

        //

        private static Byte[] HMAC(Byte[] data, ref Byte[] key)
        {
            using HMACSHA512 hmac = new(key);

            Byte[] hmac_Bytes = hmac.ComputeHash(data);

            return hmac_Bytes;
        }

        //

        private static void VerifyKeys(ref Byte[] key, ref Byte[] hmac_key)
        {
            if (key.Length != 32)
            {
                throw new InvalidDataException($"insufficient key length, was [{key.Length}] must be [32]");
            }

            if (hmac_key.Length != 64)
            {
                throw new InvalidDataException($"insufficient key length, was [{hmac_key.Length}] must be [64]");
            }
        }
    }
}