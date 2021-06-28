using MailClient.Imap;
using System.Net;
using System.Text;

namespace MailClient.Helpers
{
    public static class ImapHelper
    {
        public static ImapClient Client;

        public static bool IsBusy;

        public static IPEndPoint IPEndPoint = new(IPAddress.Parse("127.0.0.1"), 1578);

        internal static byte[] Key = Encoding.ASCII.GetBytes("12345678123456781234567812345678");

        internal static byte[] Iv = Encoding.ASCII.GetBytes("8765432187654321");
    }
}