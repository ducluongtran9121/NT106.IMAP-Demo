using MailClient.DataModels.Mail;
using MailClient.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
