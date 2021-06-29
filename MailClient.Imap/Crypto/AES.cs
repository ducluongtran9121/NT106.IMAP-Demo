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

        // mã hóa (AES)
        static public byte[] EncryptWithAES(string msg, string keyAES, string ivAES)
        {
            byte[] Key = Encoding.UTF8.GetBytes(keyAES);
            byte[] IV = Encoding.UTF8.GetBytes(ivAES);
            byte[] encResponse;
            using (Aes myAes = Aes.Create())
            {
                // Encrypt the string to an array of bytes.
                myAes.Key = Key;
                myAes.IV = IV;
                encResponse = EncryptStringToBytes_Aes(msg, myAes.Key, myAes.IV);
            }
            return encResponse;
        }

        internal static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        // giải mã (AES)
        public static string DecryptWithAES(byte[] cipher, string keyAES, string ivAES)
        {
            byte[] Key = Encoding.UTF8.GetBytes(keyAES);
            byte[] IV = Encoding.UTF8.GetBytes(ivAES);
            string decryptedMess;
            using (Aes myAes = Aes.Create())
            {
                // Decrypt the bytes to a string.
                myAes.Key = Key;
                myAes.IV = IV;
                decryptedMess = DecryptStringFromBytes_Aes(cipher, myAes.Key, myAes.IV);
            }
            return decryptedMess;
        }
        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
    }
}