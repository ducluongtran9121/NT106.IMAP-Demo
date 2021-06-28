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

                    do
                    {
                        numByteRead = await NetworkStream.ReadAsync(buffer, 0, buffer.Length);
                        await memoryStream.WriteAsync(buffer, 0, numByteRead);
                        await Task.Delay(100);
                    }
                    while (NetworkStream.DataAvailable);

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