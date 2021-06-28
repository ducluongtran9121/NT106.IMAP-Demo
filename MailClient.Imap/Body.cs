using MailClient.Imap.Common;
using MailClient.Imap.Enums;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace MailClient.Imap
{
    public class Body : BindableBase
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

        public ObservableCollection<BodyPart> Parts { get; set; }

        public Body()
        {
            Parts = new();
        }

        public static Body MultiPartBody(string body, string contentType, int contentTypeLength)
        {
            Body body1 = new();

            string pattern = contentType.Split(';')[1][9..].Replace("\"", string.Empty);

            string[] partsContent = Regex.Split(body[contentTypeLength..], "--" + pattern[0..(pattern.Length - 2)] + @".*\r\n");

            foreach (string i in partsContent[1..(partsContent.Length - 1)])
            {
                body1.Parts.Add(BodyPart.DontKnowTypeInfoSize(i));
            }

            body1.ContentType = ConvertToEnum.ToContentType(contentType);
            body1.Extension = ConvertToEnum.ToFileExtensionType(contentType);

            return body1;
        }

        public static Body SinglePartBody(string body, string contentType, int contentTypeLength)
        {
            Match match;

            int typeInfoSize = contentTypeLength;

            string type = contentType.Split(';')[0].Replace("Content-Type: ", string.Empty);
            string charset = contentType.Split(';')[1].Replace("charset=", string.Empty).Replace("\"", string.Empty).Trim();

            match = Regex.Match(body, "Content-Transfer-Encoding.*\r\n");
            string encoding = match.Value.Replace("Content-Transfer-Encoding: ", string.Empty).Trim();

            typeInfoSize += match.Value.Length;

            Body body1 = new();
            BodyPart bodyPart = BodyPart.KnowTypeInfoSize(body, type, charset, encoding, typeInfoSize + 2);
            body1.ContentType = bodyPart.ContentType;
            body1.Extension = bodyPart.Extension;
            body1.Parts.Add(bodyPart);
            return body1;
        }
    }
}