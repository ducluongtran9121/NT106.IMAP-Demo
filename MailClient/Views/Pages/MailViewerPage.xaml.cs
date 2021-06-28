using MailClient.Imap;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MailClient.Views.Pages
{
    public sealed partial class MailViewerPage : Page
    {
        public Message Message { get; set; }

        public MailViewerPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Message message = e.Parameter as Message;
            if (message != null)
            {
                Message = message;
            }
        }
    }
}