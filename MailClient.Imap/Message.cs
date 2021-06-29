using MailClient.Imap.Common;
using MailClient.Imap.Enums;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MailClient.Imap
{
    public class Message : BindableBase
    {
        private long uid = 0;

        public long Uid
        {
            get => uid;
            set => this.SetProperty(ref uid, value);
        }

        private string from = string.Empty;

        public string From
        {
            get => from;
            set => SetProperty(ref from, value);
        }

        private string to = string.Empty;

        public string To
        {
            get => to;
            set => SetProperty(ref to, value);
        }

        private string subject = string.Empty;

        public string Subject
        {
            get => subject;
            set => SetProperty(ref subject, value);
        }

        private string senderName = string.Empty;

        public string SenderName
        {
            get => senderName;
            set => SetProperty(ref senderName, value);
        }

        public ObservableCollection<MessageFlag> Flags { get; set; }

        private DateTime dateTime;

        public DateTime DateTime
        {
            get => dateTime;
            set => SetProperty(ref dateTime, value);
        }

        private Body body;

        public Body Body
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

        //private string displayBody = string.Empty;

        public string DisplayBody
        {
            get
            {
                if (Body.Parts.Count == 0)
                    return string.Empty;

                if (Body.ContentType == ContentType.Text)
                    return Body.Parts[0].Content;

                if (Body.ContentType == ContentType.Multipart)
                    return Body.Parts[0].Content;

                return string.Empty;
            }
        }

        public bool IsSeen
        {
            get => Flags.Any(x => x == MessageFlag.Seen);
            set
            {
                if (value)
                {
                    Flags.Add(MessageFlag.Seen);
                }
                else
                {
                    Flags.Remove(MessageFlag.Seen);
                }

                OnPropertyChanged();
            }
        }

        public bool IsFlagged
        {
            get => Flags.Any(x => x == MessageFlag.Flagged);
            set
            {
                if (value)
                {
                    Flags.Add(MessageFlag.Flagged);
                }
                else
                {
                    Flags.Remove(MessageFlag.Flagged);
                }

                OnPropertyChanged();
            }
        }

        public Message()
        {
        }

        public static Message InstanceFromDatabase(string[] data)
        {
            Message message = new();

            message.Uid = long.Parse(data[0]);
            message.From = data[1];
            message.To = data[2];
            message.Subject = data[3];
            message.DateTime = DateTime.Parse(data[4], System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

            // Set cont
            ContentType type = ConvertToEnum.ToContentType(data[5]);
            if (type == ContentType.Text)
            {
                message.Body = new Body { Parts = new() { new BodyPart() { Content = data[6], ContentType = type } } };
                message.BodyPreview = message.Body.Parts[0].Content[0..(80 < data[6].Length ? 80 : data[6].Length)].Replace("\r\n", " ").Replace("\n\r", " ");
            }
            else if (type == ContentType.Multipart)
            {
                message.Body = new Body
                {
                    Parts = new() { new BodyPart() { Content = data[6], ContentType = ContentType.Text }, new BodyPart { Content = data[7], ContentType = ContentType.Text } },
                };
                message.BodyPreview = message.Body.Parts[0].Content[0..(80 < data[6].Length ? 80 : data[6].Length)].Replace("\r\n", " ").Replace("\n\r", " ");
            }
            else
            {
                message.BodyPreview = string.Empty;
            }

            message.Body.ContentType = type;

            message.Flags = new ObservableCollection<MessageFlag>(data[9].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(x => (MessageFlag)Enum.Parse(typeof(MessageFlag), x, true)));

            return message;
        }

        public void SetBodyPreview(Body body)
        {
            int length = body.Parts[0].Content.Length;
            if (body.ContentType == ContentType.Text)
            {
                BodyPreview = body.Parts[0].Content[0..(80 < length ? 80 : length)].Replace("\r\n", " ").Replace("\n\r", " ");
            }
            else if (body.ContentType == ContentType.Multipart)
            {
                BodyPreview = body.Parts[0].Content[0..(80 < length ? 80 : length)].Replace("\r\n", " ").Replace("\n\r", " ");
            }
            else
            {
                BodyPreview = string.Empty;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Message message)
            {
                return base.Equals(message) && this.Flags.Except(message.Flags).ToArray().Length == 0;
            }
            return false;
        }
    }
}