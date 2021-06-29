using MailClient.Imap;
using MailClient.Imap.Enums;
using MailClient.UserControls;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MailClient.Views.Pages
{
    public sealed partial class MailViewerPage : Page
    {
        public Message Message { get; set; }

        public delegate void EvenHandler(object sender, EventArgs e);

        public static event EventHandler OnMessagePropertyChanged;

        public MailViewerPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Message message)
            {
                Message = message;
                if (message.IsSeen)
                    MarkAsReadButton.Label = "Mark as unread";
                else
                    MarkAsReadButton.Label = "Mark as read";

                if (message.IsFlagged)
                    SetFlagButton.Label = "Clear flag";
                else
                    SetFlagButton.Label = "Set flag";
            }
        }

        private void SetFlagButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (Message.IsFlagged)
            {
                SetFlagButton.Label = "Set flag";
                Message.IsFlagged = false;
                OnMessagePropertyChanged(MessageFlag.Flagged, null);
            }
            else
            {
                SetFlagButton.Label = "Clear flag";
                Message.IsFlagged = true;
                OnMessagePropertyChanged(MessageFlag.Flagged, null);
            }
        }

        private void MarkAsReadButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (Message.IsSeen)
            {
                MarkAsReadButton.Label = "Mark as read";
                Message.IsSeen = false;
                OnMessagePropertyChanged(MessageFlag.Seen, null);
            }
            else
            {
                MarkAsReadButton.Label = "Mark as unread";
                Message.IsSeen = true;
                OnMessagePropertyChanged(MessageFlag.Seen, null);
            }
        }
    }
}