using MailClient.DataModels.Mail;
using MailClient.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace MailClient.UserControls
{
    public sealed partial class MailNavigationControl : UserControl
    {
        private ObservableCollection<MailMessage> MailMessageItems;

        private ObservableCollection<MailMessage> FilteredMailMessageItems = new();

        public delegate void EvenHandler(object sender, EventArgs e);

        public event EventHandler OnMailMessageSelected;

        public MailNavigationControl()
        {
            this.InitializeComponent();
        }

        private void MailNavigationControl_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            MailMessageItems = new(AccountHelper.CurretMailBoxMessages);

            MainListView.ItemsSource = FilteredMailMessageItems;

            MailMessageItems.CollectionChanged += MailMessageItems_CollectionChanged;

            UpdateListviewItem(null, null);
        }

        private void MailMessageItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateListviewItem(null, null);
        }

        public bool IsLoadingBarRun
        {
            get => LoadingBar.IsIndeterminate;

            set => LoadingBar.IsIndeterminate = value;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedIndex != -1)
            {
                OnMailMessageSelected(AccountHelper.CurretMailBoxMessages[listView.SelectedIndex], null);
            }
        }

        private async void UpdateListviewItem(object sender, TextChangedEventArgs e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var filtered = MailMessageItems.Where(x => Filter(x, SearchBox.Text)).ToArray();
                foreach (MailMessage i in FilteredMailMessageItems.ToArray())
                {
                    if (!filtered.Any(x => x.Equals(i)))
                        _ = FilteredMailMessageItems.Remove(i);
                }

                for (int i = 0; i < filtered.Length; i++)
                {
                    if (!FilteredMailMessageItems.Any(x => x.Equals(filtered[i])))
                        FilteredMailMessageItems.Insert(i > FilteredMailMessageItems.Count ? FilteredMailMessageItems.Count : i, filtered[i]);
                }
            });
        }

        private bool Filter(MailMessage mailMessage, string value)
        {
            if (value == string.Empty) return true;
            return mailMessage.Subject.Contains(value) || mailMessage.From.Contains(value) || mailMessage.GetToString().Contains(value);
        }
    }
}