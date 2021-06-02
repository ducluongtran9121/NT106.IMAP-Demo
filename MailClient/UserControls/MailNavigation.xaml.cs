using MailClientIMAP;
using MailClient.DataModels;
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
using System.Threading.Tasks;

namespace MailClient.UserControls
{
    public sealed partial class MailNavigation : UserControl
    {
        public ObservableCollection<MailMessage> MailItems { get; set; }

        public ImapClient Client { get; set; }

        public string CurrentMailBox { get; set; }

        public delegate void EvenHandler(object sender, EventArgs e);

        public event EventHandler OnMailItemSelected;

        private bool InitialLoad = true;

        public MailNavigation()
        {
            this.InitializeComponent();

            MailItems = new ObservableCollection<MailMessage>();

            CurrentMailBox = "Inbox";
        }

        private async Task<string> LoginAsync()
        {
            try
            {
                if (Client == null)
                    Client = new ImapClient();

                var check = await Client.InitiallizeConnection();
                if (!check)
                    throw new Exception("❗❗❗ Can't connect to server! Please check your connection and try again!");

                check = await Client.Login();
                if (!check)
                    throw new Exception("❗❗❗ Can't log into your account! Please check your connection and try again!");

                return string.Empty;
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }

        private async Task MailSyncAsync()
        {
            try
            {
                SyncButton.IsEnabled = false;
                LoadingBar.IsIndeterminate = true;

                string loginReturn = string.Empty;
                if (Client == null || (Client != null & !Client.IsConnected()))
                    loginReturn = await LoginAsync();

                if (!string.IsNullOrEmpty(loginReturn))
                    throw new Exception(loginReturn);

                var check = await Client.SelectMailBox(CurrentMailBox);
                if (!check)
                    throw new Exception("❗❗❗ Can't select this mailbox! Please check your connection and try again!");

                ObservableCollection<MailMessage> mails = new ObservableCollection<MailMessage>();

                for (int i = 1; i <= 3; i++)
                {
                    List<string> header = await Client.GetMailHeader(i);
                    if (header == null)
                        throw new Exception("❗❗❗ Error while fetching your mails! Please check your connection and try again!");

                    string text = await Client.GetMailBody(i);
                    if (text == null)
                        throw new Exception("❗❗❗ Error while fetching your mails! Please check your connection and try again!");

                    mails.Add(new MailMessage(header, text));
                }

                foreach (MailMessage i in MailItems.ToArray())
                {
                    if (!mails.Any(x => x.From == i.From && x.Subject == i.Subject))
                        MailItems.Remove(i);
                }

                foreach (MailMessage i in mails)
                {
                    if (!MailItems.Any(x => x.From == i.From && x.Subject == i.Subject))
                        MailItems.Add(i);
                }

                SyncButton.IsEnabled = true;
                LoadingBar.IsIndeterminate = false;
            }
            catch(Exception ex)
            {
                Client = null;
                SyncButton.IsEnabled = true;
                LoadingBar.IsIndeterminate = false;

                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Error";
                dialog.CloseButtonText = "OK";
                dialog.Content = ex.Message;
                await dialog.ShowAsync();
            }
        }

        private async void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (InitialLoad)
            {
                await MailSyncAsync();
                InitialLoad = false;
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            if (listView.SelectedItem != null)
                OnMailItemSelected(MailItems[listView.Items.IndexOf(listView.SelectedItem)], null) ;
        }

        private async void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            await MailSyncAsync();
        }
    }
}
