using MailClient.DataModels.Mail;
using Windows.UI.Xaml.Controls;

namespace MailClient.UserControls
{
    public sealed partial class MailNavigationControl : UserControl
    {
        public MailNavigationControl()
        {
            this.InitializeComponent();
        }

        public void SetMailMessageItems(MailMessage[] mailMessages)
        {
            foreach (MailMessage mailMessage in mailMessages)
            {
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}