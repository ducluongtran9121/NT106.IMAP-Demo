using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MailClient.IMAP
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

        public static byte[] Encrypt(string plainText)
        {
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            return PerformCrypto(Encoding.UTF8.GetBytes(plainText), encryptor);
        }

        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            aes.Key = key;
            aes.IV = iv;

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            return PerformCrypto(data, encryptor);
        }

        public static byte[] Encrypt(string plainText, byte[] key, byte[] iv)
        {
            aes.Key = key;
            aes.IV = iv;

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            return PerformCrypto(Encoding.UTF8.GetBytes(plainText), encryptor);
        }

        public static string EncryptToHex(byte[] data)
        {
            return BitConverter.ToString(Encrypt(data)).Replace("-", string.Empty);
        }

        public static string EncryptToHex(string plainText)
        {
            return BitConverter.ToString(Encrypt(Encoding.UTF8.GetBytes(plainText))).Replace("-", string.Empty);
        }

        public static string EncryptToHex(string plainText, byte[] key, byte[] iv)
        {
            return BitConverter.ToString(Encrypt(Encoding.UTF8.GetBytes(plainText), key, iv)).Replace("-", string.Empty);
        }

        public static byte[] Decrypt(byte[] data)
        {
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            return PerformCrypto(data, decryptor);
        }

        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            aes.Key = key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            return PerformCrypto(data, decryptor);
        }

        public static string DecryptToString(byte[] data)
        {
            return Encoding.UTF8.GetString(Decrypt(data));
        }

        public static string DecryptToString(byte[] data, byte[] key, byte[] iv)
        {
            return Encoding.UTF8.GetString(Decrypt(data, key, iv));
        }

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