using MailClient.DataModels.Mail;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MailClient.Views.Pages
{
    public sealed partial class MailViewerPage : Page
    {
        public MailMessage Message { get; set; }

        public MailViewerPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            MailMessage message = e.Parameter as MailMessage;
            if (message != null)
            {
                Message = message;
            }
        }
    }
}