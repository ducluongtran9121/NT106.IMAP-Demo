using MailClient.Common;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace MailClient.DataModels.Mail
{
    public class MailMessage : BindableBase
    {
        private const string MailRegexPattern = @"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)";

        private string from = string.Empty;

        public string From
        {
            get => from;
            set => SetProperty(ref from, value);
        }

        public ObservableCollection<string> To { get; set; }

        public ObservableCollection<object> Attachments { get; set; }

        private bool seen;

        public bool Seen
        {
            get => seen;
            set => SetProperty(ref seen, value);
        }

        private bool answered;

        public bool Answered
        {
            get => answered;
            set => SetProperty(ref answered, value);
        }

        private bool flagged;

        public bool Flagged
        {
            get => flagged;
            set => SetProperty(ref flagged, value);
        }

        private bool deleted;

        public bool Deleted
        {
            get => deleted;
            set => SetProperty(ref deleted, value);
        }

        private bool draft;

        public bool Draft
        {
            get => draft;
            set => SetProperty(ref draft, value);
        }

        private bool recent;

        public bool Recent
        {
            get => recent;
            set => SetProperty(ref recent, value);
        }

        private string name = string.Empty;

        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        private string subject = string.Empty;

        public string Subject
        {
            get => subject;
            set => SetProperty(ref subject, value);
        }

        private string body = string.Empty;

        public string Body
        {
            get => body;
            set => SetProperty(ref body, value);
        }

        private string bodyPreview = string.Empty;

        public string BodyPreview
        {
            get => bodyPreview;
            set => SetProperty(ref bodyPreview, value);
        }

        public MailMessage()
        {
            From = string.Empty;
            Name = string.Empty;
            Subject = string.Empty;
            Body = string.Empty;
            BodyPreview = string.Empty;
        }

        public MailMessage(string[] header, string body)
        {
            To = new ObservableCollection<string>();

            Body = body;

            BodyPreview = body.Replace('\n', ' ').Substring(0, body.Length >= 80 ? 80 : body.Length);

            GetInfoFromHeader(header);
        }

        public MailMessage(string[] row)
        {
            From = row[1];
            To = new ObservableCollection<string>(row[2].Split(';'));
            Subject = row[3];
            Body = row[5];
            BodyPreview = row[5].Replace('\n', ' ').Substring(0, body.Length >= 80 ? 80 : body.Length);
        }

        public static bool IsValidAddress(string address)
        {
            return new Regex(MailRegexPattern).IsMatch(address);
        }

        public void GetInfoFromHeader(string[] header)
        {
            Regex regex = new(MailRegexPattern);

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
}