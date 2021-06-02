using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MailClient.DataModels
{
    public class MailMessage
    {
        public ObservableCollection<Flag> flags;

        public string From { get; set; }

        public ObservableCollection<string> To { get; set; }

        public string Name { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public string BodyPreview { get; set; }

        public ObservableCollection<object> Attachments { get; set; }

        public MailMessage()
        {
            From = string.Empty;
            Name = string.Empty;
            Subject = string.Empty;
            Body = string.Empty;
            BodyPreview = string.Empty;
        }

        public MailMessage(List<string> header, string body)
        {
            To = new ObservableCollection<string>();

            Body = body;

            BodyPreview = body.Replace('\n', ' ').Substring(0, body.Length >= 80 ? 80 : body.Length);

            GetInfoFromHeader(header);
        }

        public void GetInfoFromHeader(List<string> header)
        {
            Regex regex = new Regex(@"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)");

            Match match;

            match = regex.Match(header[0]);
            if (match.Success)
            {
                From = match.Value;
            }

            match = regex.Match(header[1]);
            if (match.Success)
            {
                To.Add(match.Value);
            }

            Subject = header[2].Replace(header[2].Substring(0, 9), "");
        }

        public string GetToString()
        {
            if (To != null)
                return string.Join(';', To);
            return string.Empty;
        }
    }

    public enum Flag
    {
        Seen,
        Answered,
        Flagged,
        Deleted,
        Draft,
        Recent,
    }
}
