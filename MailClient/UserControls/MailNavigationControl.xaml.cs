using MailClient.Helpers;
using System;
using Windows.UI.Xaml.Controls;

namespace MailClient.UserControls
{
    public sealed partial class MailNavigationControl : UserControl
    {
        public delegate void EvenHandler(object sender, EventArgs e);

        public event EventHandler OnMailMessageSelected;

        public MailNavigationControl()
        {
            this.InitializeComponent();
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
    }
}