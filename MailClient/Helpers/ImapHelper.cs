using MailClient.Imap;
using System.Net;
using System.Text;

namespace MailClient.Helpers
{
    public static class ImapHelper
    {
        public static ImapClient Client;

        public static Folder CurrentFolder = new();

        public static bool IsBusy;

        public static IPEndPoint IPEndPoint = new(IPAddress.Parse("1.54.83.178"), 1580);

        internal static byte[] Key = Encoding.ASCII.GetBytes("12345678123456781234567812345678");

        internal static byte[] Iv = Encoding.ASCII.GetBytes("1122334455667788");
    }
}