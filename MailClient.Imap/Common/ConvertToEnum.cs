using MailClient.Imap.Enums;

namespace MailClient.Imap.Common
{
    public static class ConvertToEnum
    {
        public static ContentType ToContentType(string input)
        {
            if (input.ToLower().Contains("text"))
                return ContentType.Text;

            if (input.ToLower().Contains("multipart"))
                return ContentType.Multipart;

            return ContentType.Text;
        }

        public static FileExtensionType ToFileExtensionType(string input)
        {
            if (input.Contains("plain"))
                return FileExtensionType.txt;

            if (input.Contains("html"))
                return FileExtensionType.html;

            if (input.Contains("alternative"))
                return FileExtensionType.alternative;

            return FileExtensionType.txt;
        }
    }
}