using System;
using System.Text;

namespace MailClient.Imap.Common
{
    public static class ConvertToString
    {
        public static string FromString(string input, string encoding, string charset)
        {
            if (encoding == "quoted-printable")
                return input;

            if (encoding == "7bit")
                return input;

            byte[] data = null;

            if (encoding == "base64" || encoding == "B")
            {
                data = Convert.FromBase64String(input);
            }

            // Charset
            if (charset == "utf-8")
                return Encoding.UTF8.GetString(data);

            if (charset == "us-ascii")
                return Encoding.ASCII.GetString(data);

            return string.Empty;
        }
    }
}