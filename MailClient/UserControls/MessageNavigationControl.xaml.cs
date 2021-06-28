using MailClient.Imap;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MailClient.UserControls
{
    public sealed partial class MessageNavigationControl : UserControl
    {
        public ObservableCollection<Message> MessageItems = new();

        private ObservableCollection<Message> FilteredMessageItems { get; set; } = new();

        public ObservableCollection<string> FolderItems { get; set; } = new();

        private Message CurrentMessageItem { get; set; }

        private string CurrentFolderName { get; set; }

        public delegate void EvenHandler(object sender, EventArgs e);

        public event EventHandler OnMessageSelected;

        public event EvenHandler OnSyncButtonPressed;

        public bool IsLoadingBarRun
        {
            get => LoadingBar.IsIndeterminate;

            set => LoadingBar.IsIndeterminate = value;
        }

        public ListViewSelectionMode SelectionMode
        {
            get => MainListView.SelectionMode;

            set => MainListView.SelectionMode = value;
        }

        public MessageNavigationControl()
        {
            this.InitializeComponent();

            MessageItems.CollectionChanged += MessageItems_CollectionChanged;
        }

        private void SearchBox_OnSearchButtonClick(object sender, EventArgs e)
        {
        }

        private void MainListView_Loaded(object sender, RoutedEventArgs e)
        {
            MainListView.ItemsSource = FilteredMessageItems;
        }

        public async Task UpdateFolderItems(string[] names)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                foreach (string i in FolderItems.ToArray())
                {
                    if (!names.AsParallel().Any(x => x.Equals(i)))
                        _ = FolderItems.Remove(i);
                }

                foreach (string i in names)
                {
                    if (!FolderItems.AsParallel().Any(x => x.Equals(i)))
                        FolderItems.Add(i);
                }
            });

            FolderItems.Add("New mailbox...");

            if (FolderItems.Count > 1 && FoldersComboBox.SelectedItem == null)
            {
                FoldersComboBox.SelectedItem = FolderItems[0];
                CurrentFolderName = FolderItems[0];
            }
        }

        private async void UpdateListviewItem(object sender, TextChangedEventArgs e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var filtered = MessageItems.Where(x => Filter(x, SearchBox.Text)).ToArray();
                foreach (Message i in FilteredMessageItems.ToArray())
                {
                    if (!filtered.AsParallel().Any(x => x.Equals(i)))
                        _ = FilteredMessageItems.Remove(i);
                }

                for (int i = 0; i < filtered.Length; i++)
                {
                    if (!FilteredMessageItems.AsParallel().Any(x => x.Equals(filtered[i])))
                        FilteredMessageItems.Insert(i > FilteredMessageItems.Count ? FilteredMessageItems.Count : i, filtered[i]);
                }
            });
        }

        private void MessageItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateListviewItem(null, null);
        }

        private void MainListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListView listView)
                return;

            if (listView.SelectionMode == ListViewSelectionMode.Single && listView.SelectedIndex != -1)
            {
                CurrentMessageItem = MessageItems[listView.SelectedIndex];
            }

            OnMessageSelected(CurrentMessageItem, null);
        }

        private bool Filter(Message Message, string value)
        {
            if (value == string.Empty) return true;
            return Message.Subject.ToLower().Contains(value.ToLower()) || Message.From.ToLower().Contains(value.ToLower());
        }

        private void MultiSelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainListView.SelectionMode == ListViewSelectionMode.Single)
            {
                CurrentMessageItem = (Message)MainListView.SelectedItem;

                MainListView.SelectionMode = ListViewSelectionMode.Multiple;

                MainListView.SelectedItem = CurrentMessageItem;
            }
            else
            {
                MainListView.SelectionMode = ListViewSelectionMode.Single;

                MainListView.SelectedItem = CurrentMessageItem;
            }
        }

        private void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            OnSyncButtonPressed(null, null);
        }
    }
}