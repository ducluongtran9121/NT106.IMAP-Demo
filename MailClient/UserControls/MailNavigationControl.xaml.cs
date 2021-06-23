using MailClient.DataModels.Mail;
using MailClient.Helpers;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Diagnostics;

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

            Loaded += MailNavigationControl_Loaded;
        }

        private void MailNavigationControl_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            MailMessageItems = new(AccountHelper.CurretMailBoxMessages);

            SearchBox_TextChanged(null, null);
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

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (MainListView.ItemsSource != FilteredMailMessageItems)
                MainListView.ItemsSource = FilteredMailMessageItems;

            if (MainListView.ItemsSource == FilteredMailMessageItems)
            {
                var filtered = MailMessageItems.Where(x => Filter(x, SearchBox.Text)).ToArray();
                foreach (MailMessage i in FilteredMailMessageItems.ToArray())
                {
                    if (!filtered.Any(x => i.From == x.From && i.Subject == x.Subject))
                        FilteredMailMessageItems.Remove(i);
                }

                for (int i = 0; i < filtered.Length; i++)
                {
                    if (!FilteredMailMessageItems.Any(x => filtered[i].From == x.From && filtered[i].Subject == x.Subject))
                        FilteredMailMessageItems.Insert(i > FilteredMailMessageItems.Count ? FilteredMailMessageItems.Count : i, filtered[i]);
                }
            }
        }

        private bool Filter(MailMessage mailMessage, string value)
        {
            if (value == string.Empty) return true;
            return mailMessage.Subject.Contains(value) || mailMessage.From.Contains(value) || mailMessage.GetToString().Contains(value);
        }
    }
}