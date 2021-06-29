using MailClient.Imap.Commands;
using MailClient.Imap.Crypto;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MailClient.Imap
{
    internal class CoreClient : IDisposable
    {
        private bool _disposed = false;

        private TcpClient TcpClient { get; set; }

        private NetworkStream NetworkStream { get; set; }

        public bool IsEncrypt { get; set; }

        private string Key ="12345678123456781234567812345678";

        private string Iv = "1122334455667788";

        internal bool IsConnected => TcpClient != null && TcpClient.Connected;

        public int Tag { get; set; }

        internal CoreClient()
        {
        }

        internal async Task<bool> InitializeConnection(IPEndPoint endPoint)
        {
            try
            {
                TcpClient = new TcpClient();

                await TcpClient.ConnectAsync(endPoint.Address, endPoint.Port);

                NetworkStream = TcpClient.GetStream();

                TcpClient.SendTimeout = 10000;

                return true;
            }
            catch (Exception)
            {
                Dispose();
                return false;
            }
        }

        internal async Task<bool> WriteDataAsync(string data)
        {
            return await WriteDataAsync(Encoding.ASCII.GetBytes(data));
        }

        internal async Task<bool> WriteDataAsync(byte[] data)
        {
            try
            {
                if (IsConnected)
                {
                    if (IsEncrypt)
                    {
                        data = AES.EncryptWithAES(Encoding.UTF8.GetString(data), Key, Iv);

                        byte[] lengthEcnrypted = AES.EncryptWithAES(data.Length.ToString(), Key, Iv);
                        await NetworkStream.WriteAsync(lengthEcnrypted, 0, lengthEcnrypted.Length);
                        await NetworkStream.FlushAsync();
                    }

                    await NetworkStream.WriteAsync(data, 0, data.Length);
                    await NetworkStream.FlushAsync();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal async Task<byte[]> ReadDataAsync()
        {
            try
            {
                if (IsConnected)
                {
                    byte[] buffer = new byte[1024];

                    int numByteRead = 0;

                    using MemoryStream memoryStream = new();
                    while (!NetworkStream.DataAvailable) ;

                    if (IsEncrypt)
                    {
                        byte[] dataLength = new byte[16];
                        long messageLength = 0;
                        while (IsConnected)
                        {
                            if (NetworkStream.DataAvailable)
                            {
                                await NetworkStream.ReadAsync(dataLength, 0, dataLength.Length);
                                if (!long.TryParse(AES.DecryptWithAES(dataLength, Key, Iv), out messageLength)) throw new Exception();
                                break;
                            }
                        }

                        int byteRead = 0;
                        while(IsConnected && byteRead < messageLength)
                        {
                            numByteRead = await NetworkStream.ReadAsync(buffer, 0, buffer.Length);
                            await memoryStream.WriteAsync(buffer, 0, numByteRead);
                            byteRead += numByteRead;
                        }
                        return Encoding.UTF8.GetBytes(AES.DecryptWithAES(memoryStream.ToArray(), Key, Iv));
                    }
                    else
                    {
                        while (!FragmentCommand.IsEnd(buffer) && IsConnected)
                        {
                            if (NetworkStream.DataAvailable)
                            {
                                numByteRead = await NetworkStream.ReadAsync(buffer, 0, buffer.Length);
                                await memoryStream.WriteAsync(buffer, 0, numByteRead);
                            }
                        }
                    }

                    return memoryStream.ToArray();
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            if (IsConnected)
                TcpClient.Close();

            TcpClient?.Dispose();

            NetworkStream?.Close();
            NetworkStream?.Dispose();
        }
    }
}