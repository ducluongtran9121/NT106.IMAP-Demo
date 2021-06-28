using MailClient.Imap.Commands;
using System;
using System.Net;
using System.Threading.Tasks;

namespace MailClient.Imap
{
    public class ImapClient : IDisposable
    {
        private bool _disposed = false;

        public string Address { get; private set; }

        private CoreClient CoreClient { get; set; }

        public Folder Inbox { get; private set; }

        public bool IsConnected => CoreClient != null && CoreClient.IsConnected;

        public bool IsAuthenticated { get; private set; }

        public ImapClient()
        {
        }

        public async Task<bool> ConnectAsync(string host, int port)
        {
            return await ConnectAsync(new IPEndPoint(IPAddress.Parse(host), port));
        }

        public async Task<bool> ConnectAsync(IPEndPoint endPoint)
        {
            CoreClient = new();

            if (!await CoreClient.InitializeConnection(endPoint))
                throw new ConnectionException($"Failed to connect to {endPoint.Address}:{endPoint.Port}");

            byte[] data = await CoreClient.ReadDataAsync();

            if (data == null) throw new ReadDataException("Failed to read server connected message");

            return FragmentCommand.Connect(data);
        }

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            if (CoreClient == null)
                throw new NullReferenceException("CoreClient not set to an instance of an object");

            if (!CoreClient.IsConnected)
                throw new ConnectionException("CoreClient is not connected");

            if (!await CoreClient.WriteDataAsync(Command.Authenticate(++CoreClient.Tag, username, password)))
                throw new WriteDataException("Failed to send authentication command");

            byte[] data = await CoreClient.ReadDataAsync();

            if (data == null) throw new ReadDataException("Failed to read data");

            if (FragmentCommand.Authenticate(data))
            {
                Inbox = new Folder("INBOX", CoreClient);
                Address = username;
                IsAuthenticated = true;
                return true;
            }
            return false;
        }

        public async Task<string[]> GetListFolderAsync()
        {
            if (CoreClient == null)
                throw new NullReferenceException("CoreClient not set to an instance of an object");

            if (!CoreClient.IsConnected)
                throw new ConnectionException("CoreClient is not connected");

            if (!await CoreClient.WriteDataAsync(Command.List(CoreClient.Tag, "", "*")))
                throw new WriteDataException("Failed to send authentication command");

            byte[] data = await CoreClient.ReadDataAsync();

            if (data == null) throw new ReadDataException();

            return FragmentCommand.List(data);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            CoreClient?.Dispose();
        }
    }
}