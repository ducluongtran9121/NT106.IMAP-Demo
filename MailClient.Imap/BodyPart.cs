using MailClient.Imap.Common;
using MailClient.Imap.Enums;
using System.Text.RegularExpressions;

namespace MailClient.Imap
{
    public class BodyPart : BindableBase
    {
        private ContentType contentType;

        public ContentType ContentType
        {
            get => contentType;
            set => SetProperty(ref contentType, value);
        }

        private FileExtensionType extension;

        public FileExtensionType Extension
        {
            get => extension;
            set => SetProperty(ref extension, value);
        }

        private string content = string.Empty;

        public string Content
        {
            get => content;
            set => SetProperty(ref content, value);
        }

        public BodyPart()
        {
        }

        public static BodyPart KnowTypeInfoSize(string body, string type, string charset, string encoding, int size)
        {
            body = body[size..(body.Length - 2)];

            BodyPart bodyPart = new();

            bodyPart.ContentType = ConvertToEnum.ToContentType(type);
            bodyPart.Extension = ConvertToEnum.ToFileExtensionType(type);
            bodyPart.Content = ConvertToString.FromString(body, encoding, charset);

            return bodyPart;
        }

        public static BodyPart DontKnowTypeInfoSize(string body)
        {
            string contentType;

            Match match;

            if ((match = Regex.Match(body, @"Content-Type.*\r\n\t.*\r\n")).Success)
            {
                contentType = match.Value.Replace("\r\n\t", string.Empty);
            }
            else
            {
                match = Regex.Match(body, @"Content-Type.*\r\n");
                contentType = match.Value.Replace(" ", string.Empty);
            }

            int typeInfoSize = match.Value.Length;

            string type = contentType.Split(';')[0].Replace("Content-Type: ", string.Empty);
            string charset = contentType.Split(';')[1].Replace("charset=", string.Empty).Replace("\"", string.Empty).Trim();

            match = Regex.Match(body, "Content-Transfer-Encoding.*\r\n");
            string encoding = match.Value.Replace("Content-Transfer-Encoding: ", string.Empty).Trim();

            typeInfoSize += match.Value.Length;

            return KnowTypeInfoSize(body, type, charset, encoding, typeInfoSize + 2);
        }
    }
}