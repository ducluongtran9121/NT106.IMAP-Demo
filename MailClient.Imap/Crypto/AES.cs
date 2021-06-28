using MailClient.Imap.Common;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MailClient.Imap.Crypto
{
    public static class AES
    {
        private static readonly Aes aes = Aes.Create();

        public static byte[] Key
        {
            get => aes.Key;
            set => aes.Key = value;
        }

        public static byte[] IV
        {
            get => aes.IV;
            set => aes.IV = value;
        }

        public static PaddingMode Padding
        {
            get => aes.Padding;
            set => aes.Padding = value;
        }

        public static CipherMode Mode
        {
            get => aes.Mode;
            set => aes.Mode = value;
        }

        public static byte[] Encrypt(byte[] data)
        {
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            return PerformCrypto(data, encryptor);
        }

        public static byte[] Encrypt(string plainText) =>
            Encrypt(Encoding.UTF8.GetBytes(plainText));

        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            aes.Key = key;
            aes.IV = iv;

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            return PerformCrypto(data, encryptor);
        }

        public static byte[] Encrypt(string plainText, byte[] key, byte[] iv) =>
            Encrypt(Encoding.UTF8.GetBytes(plainText), key, iv);

        public static string EncryptToHex(byte[] data) =>
            BitConverter.ToString(Encrypt(data)).Replace("-", string.Empty);

        public static string EncryptToHex(string plainText) =>
            BitConverter.ToString(Encrypt(Encoding.UTF8.GetBytes(plainText))).Replace("-", string.Empty);

        public static string EncryptToHex(byte[] data, byte[] key, byte[] iv) =>
            BitConverter.ToString(Encrypt(data, key, iv)).Replace("-", string.Empty);

        public static string EncryptToHex(string plainText, byte[] key, byte[] iv) =>
            BitConverter.ToString(Encrypt(Encoding.UTF8.GetBytes(plainText), key, iv)).Replace("-", string.Empty);

        public static byte[] Decrypt(byte[] data)
        {
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            return PerformCrypto(data, decryptor);
        }

        public static byte[] Decrypt(string hexText) =>
            Decrypt(HexUtil.ToBytes(hexText));

        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            aes.Key = key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            return PerformCrypto(data, decryptor);
        }

        public static byte[] Decrypt(string hexText, byte[] key, byte[] iv) =>
           Decrypt(HexUtil.ToBytes(hexText), key, iv);

        public static string DecryptToString(byte[] data) =>
            Encoding.UTF8.GetString(Decrypt(data));

        public static string DecryptToString(string hexText) =>
            Encoding.UTF8.GetString(Decrypt(hexText));

        public static string DecryptToString(byte[] data, byte[] key, byte[] iv) =>
            Encoding.UTF8.GetString(Decrypt(data, key, iv));

        public static string DecryptToString(string hexText, byte[] key, byte[] iv) =>
            Encoding.UTF8.GetString(Decrypt(hexText, key, iv));

        private static byte[] PerformCrypto(byte[] data, ICryptoTransform cryptoTransform)
        {
            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, cryptoTransform, CryptoStreamMode.Write);

            cryptoStream.Write(data, 0, data.Length);
            cryptoStream.FlushFinalBlock();
            return memoryStream.ToArray();
        }
    }
}