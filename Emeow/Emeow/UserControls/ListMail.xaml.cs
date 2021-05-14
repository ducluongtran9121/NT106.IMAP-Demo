using Emeow.User;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;

namespace Emeow.UserControls
{
    public sealed partial class ListMail : UserControl
    {

        public ObservableCollection<Mail> MailItems { get; set; } = new ObservableCollection<Mail>();

        public string CurrentMailBox { get; set; }

        public delegate void EvenHandler(object sender, EventArgs e);

        public event EventHandler OnMailItemSelected;

        public event EvenHandler OnMailSync;

        public ListMail()
        {
            this.InitializeComponent();

            CurrentMailBox = MailBoxDropBtn.Content.ToString().ToLower();
        }

        public void StartLoadingAnimation()
        {
            LoadingBar.IsIndeterminate = true;
        }

        public void StopLoadingAnimation()
        {
            LoadingBar.IsIndeterminate = false;
        }

        public void RemoveAllItems()
        {
            for (int i = 0; i < MailItems.Count; i++)
                MailItems.RemoveAt(i);
        }

        private void FlyoutInboxItem_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            MailBoxDropBtn.Content = (sender as MenuFlyoutItem).Text;
        }

        private void FlyoutSentItem_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            MailBoxDropBtn.Content = (sender as MenuFlyoutItem).Text;
        }

        private void FlyoutDraftItem_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            MailBoxDropBtn.Content = (sender as MenuFlyoutItem).Text;
        }

        private void MainListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainListView.SelectedItem != null)
                OnMailItemSelected(MailItems[MainListView.Items.IndexOf(MainListView.SelectedItem)], null);
        }

        private void SyncButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            OnMailSync(null, null);
        }
    }
}
