using MailClient.DataModels;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MailClient.Views.Pages
{
    public sealed partial class MailInfoPage : Page
    {
        public MailMessage Message { get; set; }

        public MailInfoPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //base.OnNavigatedTo(e);
            Message = e.Parameter as MailMessage;
        }
    }
}
