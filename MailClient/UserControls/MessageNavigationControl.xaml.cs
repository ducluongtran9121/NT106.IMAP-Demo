using MailClient.Helpers;
using MailClient.Imap;
using MailClient.Imap.Crypto;
using MailClient.Imap.Enums;
using MailClient.Views.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        public ObservableCollection<Message> MessageItems { get; set; } = new();

        public ObservableCollection<Message> FilteredMessageItems { get; set; } = new();

        public ObservableCollection<string> FolderItems { get; set; } = new();

        private Message CurrentMessageItem;

        private bool IsFirstTimeSelectFolder { get; set; } = true;

        public delegate void EvenHandler(object sender, EventArgs e);

        public event EventHandler OnMessageSelected;

        public event EvenHandler OnFolderSelected;

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

            MailViewerPage.OnMessagePropertyChanged += MailViewerPage_OnMessagePropertyChanged;
        }

        private async void MailViewerPage_OnMessagePropertyChanged(object sender, EventArgs e)
        {
            try
            {
                if (ImapHelper.Client == null)
                {
                    ImapHelper.Client = new ImapClient();
                }

                if (ImapHelper.Client.IsEncrypt != SettingsHelper.IsUseTLS)
                {
                    ImapHelper.Client.Dispose();
                    ImapHelper.Client = new ImapClient();
                }

                var client = ImapHelper.Client;

                if (!client.IsConnected)
                {
                    if (!await client.ConnectAsync(ImapHelper.IPEndPoint, SettingsHelper.IsUseTLS))
                        return;
                }

                if (!client.IsAuthenticated)
                {
                    string[] selected = await DatabaseHelper.SelectDataAsync(
                        DatabaseHelper.AccountsDatabaseName, DatabaseHelper.AccountTableName, new string[] { "Password" },
                        new (string, string)[] { ("Address", AccountHelper.CurrentAccount.Address) });

                    if (!await client.AuthenticateAsync(AccountHelper.CurrentAccount.Address, AES.DecryptToString(selected[0], ImapHelper.Key, ImapHelper.Iv)))
                        return;
                }

                if (ImapHelper.CurrentFolder == null)
                {
                    if (FoldersComboBox.SelectedIndex == -1)
                        return;

                    Folder folder = new Folder(FolderItems[FoldersComboBox.SelectedIndex], ImapHelper.Client);

                    await folder.OpenAsync();

                    if (!await folder.OpenAsync())
                        return;

                    ImapHelper.CurrentFolder = folder;
                }

                MessageFlag flag = (MessageFlag)sender;

                List<MessageFlag[]> updatedFlags = new();

                if (flag == MessageFlag.Seen)
                {
                    updatedFlags = await ImapHelper.CurrentFolder.SetMessageFlag(CurrentMessageItem.Uid, CurrentMessageItem.IsSeen, flag);
                }
                else if (flag == MessageFlag.Flagged)
                {
                    updatedFlags = await ImapHelper.CurrentFolder.SetMessageFlag(CurrentMessageItem.Uid, CurrentMessageItem.IsFlagged, flag);
                }

                if (updatedFlags == null)
                    await DatabaseHelper.UpdateCellAsync(DatabaseHelper.CurrentDatabaseName, ImapHelper.CurrentFolder.Name, "Flag", "", new (string, string)[] { ("UID", CurrentMessageItem.Uid.ToString()) });
                else
                    await DatabaseHelper.UpdateCellAsync(DatabaseHelper.CurrentDatabaseName, ImapHelper.CurrentFolder.Name, "Flag", string.Join(" ", updatedFlags[0]), new (string, string)[] { ("UID", CurrentMessageItem.Uid.ToString()) });
            }
            catch (Exception)
            {

            }
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

            if (FolderItems.Count > 0 && FoldersComboBox.SelectedItem == null)
            {
                FoldersComboBox.SelectedItem = FolderItems[0];
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

        private async void CurrentMessageItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        { 

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
            if (FoldersComboBox.SelectedIndex != - 1)
            OnSyncButtonPressed(FolderItems[FoldersComboBox.SelectedIndex], null);
        }

        private void FoldersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combobox = sender as ComboBox;

            if (IsFirstTimeSelectFolder)
            {
                IsFirstTimeSelectFolder = false;
                return;
            }

            if (combobox.SelectedIndex != -1)
                OnFolderSelected(FolderItems[combobox.SelectedIndex], null);
        }
    }
}