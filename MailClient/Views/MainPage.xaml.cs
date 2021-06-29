using MailClient.DataModels.Imap;
using MailClient.Helpers;
using MailClient.Imap;
using MailClient.Imap.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using muxc = Microsoft.UI.Xaml.Controls;

namespace MailClient.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            // Set Titlebar
            Window.Current.SetTitleBar(AppTitleBar);

            SideBarNavigation.PaneDisplayMode = muxc.NavigationViewPaneDisplayMode.Left;

            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += (s, e) =>
            {
                SideBarNavigation.PaneDisplayMode = muxc.NavigationViewPaneDisplayMode.Auto;
                UpdateAppTitle(s);
            };
        }

        private void UpdateAppTitle(CoreApplicationViewTitleBar coreTitleBar)
        {
            // Ensure the custom title bar does not overlap window caption controls
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, coreTitleBar.SystemOverlayRightInset, currMargin.Bottom);
        }

        private void SideBarNavigation_DisplayModeChanged(muxc.NavigationView sender, muxc.NavigationViewDisplayModeChangedEventArgs args)
        {
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(sender.CompactPaneLength, currMargin.Top, currMargin.Right, currMargin.Bottom);
            UpdateAppTitleMargin(sender);
        }

        private void SideBarNavigation_PaneClosing(muxc.NavigationView sender, muxc.NavigationViewPaneClosingEventArgs args)
        {
            UpdateAppTitleMargin(sender);
        }

        private void SideBarNavigation_PaneOpening(muxc.NavigationView sender, object args)
        {
            UpdateAppTitleMargin(sender);
        }

        // Titlebar animation
        private void UpdateAppTitleMargin(muxc.NavigationView sender)
        {
            const int smallLeftIndent = 4, largeLeftIndent = 24;

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
            {
                AppTitle.TranslationTransition = new Vector3Transition();

                AppTitle.Translation = (sender.DisplayMode == muxc.NavigationViewDisplayMode.Expanded && sender.IsPaneOpen) ||
                         sender.DisplayMode == muxc.NavigationViewDisplayMode.Minimal
                    ? new System.Numerics.Vector3(smallLeftIndent, 0, 0)
                    : new System.Numerics.Vector3(largeLeftIndent, 0, 0);
            }
            else
            {
                Thickness currMargin = AppTitle.Margin;

                AppTitle.Margin = (sender.DisplayMode == muxc.NavigationViewDisplayMode.Expanded && sender.IsPaneOpen) ||
                         sender.DisplayMode == muxc.NavigationViewDisplayMode.Minimal
                    ? new Thickness(smallLeftIndent, currMargin.Top, currMargin.Right, currMargin.Bottom)
                    : new Thickness(largeLeftIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
        }

        private async void Page_Loading(FrameworkElement sender, object args)
        {
            try
            {
                // Load list of account from database
                List<string[]> accounts =
                    await DatabaseHelper.GetTableDataAsync(DatabaseHelper.AccountsDatabaseName, DatabaseHelper.AccountTableName);

                if (accounts == null)
                    return;

                // Load accounts info from database
                foreach (string[] account in accounts)
                    AccountHelper.Accounts.Add(Account.InstanceFromDatabase(account));

                AccountHelper.CurrentAccount = AccountHelper.Accounts[0];

                // Pass accounts data to SideNavigation control
                SideBarNavigation.SetAccountItems(AccountHelper.Accounts.ToArray());

                // Set default database to the first account
                DatabaseHelper.CurrentDatabaseName = $"{AccountHelper.CurrentAccount.Address}.db";

                // Get all folder name
                await MailNavigation.UpdateFolderItems(await DatabaseHelper.GetTableNamesAsync(DatabaseHelper.CurrentDatabaseName));

                // Check folder has mail?
                if (await DatabaseHelper.CountRows(DatabaseHelper.CurrentDatabaseName, "INBOX") > 0)
                {
                    List<string[]> messages = await DatabaseHelper.GetTableDataAsync(DatabaseHelper.CurrentDatabaseName, "INBOX");

                    foreach (string[] message in messages)
                        MailNavigation.MessageItems.Add(Message.InstanceFromDatabase(message));
                }

                LoadingControl.IsLoading = false;
            }
            catch (Exception)
            {
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ImapHelper.IsBusy = true;

            Message[] messages = await InitialFetchingMail("INBOX");

            if (messages != null)
            {
                await UpdateMessageData(messages, "INBOX");
            }

            ImapHelper.IsBusy = false;

            MailNavigation.IsLoadingBarRun = false;
        }

        private async Task<Message[]> InitialFetchingMail(string folderName)
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
                        return null;
                }

                if (!client.IsAuthenticated)
                {
                    string[] selected = await DatabaseHelper.SelectDataAsync(
                        DatabaseHelper.AccountsDatabaseName, DatabaseHelper.AccountTableName, new string[] { "Password" },
                        new (string, string)[] { ("Address", AccountHelper.CurrentAccount.Address) });

                    if (!await client.AuthenticateAsync(AccountHelper.CurrentAccount.Address, AES.DecryptToString(selected[0], ImapHelper.Key, ImapHelper.Iv)))
                        return null;
                }

                Folder folder = new Folder(folderName, ImapHelper.Client);

                List<Message> messages = new();

                if (!await folder.OpenAsync())
                    throw new Exception();

                ImapHelper.CurrentFolder = folder;

                return await folder.GetMessagesAsync();
            }
            catch (Exception)
            {
                ImapHelper.Client?.Dispose();
                return null;
            }
        }

        private async Task<bool> UpdateMessageData(Message[] messages, string folderName)
        {
            try
            {
                var mess = MailNavigation.MessageItems;
                var folders = MailNavigation.FolderItems;

                string[] updatedFolders = await ImapHelper.Client.GetListFolderWithoutFlagAsync();

                foreach (string i in folders.ToArray())
                {
                    if (!updatedFolders.Any(x => x == i))
                    {
                        await DatabaseHelper.DropTableAsync(DatabaseHelper.CurrentDatabaseName, i);
                    }
                }

                foreach (string i in updatedFolders)
                {
                    if (!folders.Any(x => x == i))
                    {
                        await DatabaseHelper.CreateAccountMailboxAsync(DatabaseHelper.CurrentDatabaseName, i);
                    }
                }

                // Update folder
                await MailNavigation.UpdateFolderItems(updatedFolders);

                // Update message in current mailbox (inbox)
                foreach (Message i in mess.ToArray())
                {
                    if (!messages.Any(x => x.Uid == i.Uid))
                    {
                        _ = mess.Remove(i);
                        await DatabaseHelper.DeleteRowsAsync(DatabaseHelper.CurrentDatabaseName, folderName,
                            new (string, string)[] { ("UID", i.Uid.ToString()) });
                    };
                }

                foreach (Message i in messages)
                {
                    if (!mess.Any(x => x.Uid == i.Uid))
                    {
                        mess.Add(i);
                        // Temp save to database, will be fixed
                        await DatabaseHelper.InsertDataAsync(DatabaseHelper.CurrentDatabaseName, folderName,
                            new string[] { i.Uid.ToString(), i.From, i.To, i.Subject, i.DateTime.ToString(), i.Body.ContentType.ToString(), i.Body.Parts[0].Content, "", "", string.Join(' ', i.Flags) });
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void MailNavigation_OnMessageSelected(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            ContentFrame.BackStack.Clear();

            if (MailNavigation.SelectionMode == ListViewSelectionMode.Multiple)
            {
                ContentFrame.Visibility = Visibility.Collapsed;
                ContentFrame.Content = null;
                return;
            }

            ContentFrame.Visibility = Visibility.Visible;
            ContentFrame.Navigate(typeof(Pages.MailViewerPage), sender);
        }

        private async void MailNavigation_OnSyncButtonPressed(object sender, EventArgs e)
        {
            try
            {
                if (ImapHelper.IsBusy)
                    return;

                ImapHelper.IsBusy = true;
                MailNavigation.IsLoadingBarRun = true;
                string name = sender as string;

                Message[] messages = await InitialFetchingMail(name);

                if (messages == null)
                    throw new Exception();

                if (messages != null)
                {
                    if (!await UpdateMessageData(messages, name))
                        throw new Exception();
                }

                ImapHelper.IsBusy = false;
                MailNavigation.IsLoadingBarRun = false;
            }
            catch (Exception)
            {
                ContentDialog dialog = new();
                dialog.PrimaryButtonText = "OK";
                dialog.Content = "Failed to sync your mail! Please try again...";
                _ = dialog.ShowAsync();
                MailNavigation.IsLoadingBarRun = false;
                ImapHelper.IsBusy = false;
            }
        }

        private async void MailNavigation_OnFolderSelected(object sender, EventArgs e)
        {
            try
            {
                if (ImapHelper.IsBusy)
                    return;

                ImapHelper.IsBusy = true;

                string name = sender as string;

                MailNavigation.IsLoadingBarRun = true;

                foreach (Message i in MailNavigation.FilteredMessageItems.ToArray())
                    MailNavigation.MessageItems.Remove(i);

                foreach (Message i in MailNavigation.MessageItems.ToArray())
                    MailNavigation.MessageItems.Remove(i);

                // Check folder has mail?
                if (await DatabaseHelper.CountRows(DatabaseHelper.CurrentDatabaseName, name) > 0)
                {
                    List<string[]> messagesDatabase = await DatabaseHelper.GetTableDataAsync(DatabaseHelper.CurrentDatabaseName, name);

                    foreach (string[] message in messagesDatabase)
                        MailNavigation.MessageItems.Add(Message.InstanceFromDatabase(message));
                }

                Message[] messages = await InitialFetchingMail(name);

                if (messages == null)
                    throw new Exception();

                if (messages != null)
                {
                    if (!await UpdateMessageData(messages, name))
                        throw new Exception();
                }

                ImapHelper.IsBusy = false;
                MailNavigation.IsLoadingBarRun = false;
            }
            catch (Exception)
            {
                ImapHelper.Client?.Dispose();
                ContentDialog dialog = new();
                dialog.PrimaryButtonText = "OK";
                dialog.Content = "Failed to sync your mail! Please try again...";
                _ = dialog.ShowAsync();
                MailNavigation.IsLoadingBarRun = false;
                ImapHelper.IsBusy = false;
            }
        }

        private async void SideBarNavigation_OnAccountSignedIn(object sender, EventArgs e)
        {
            try
            {
                if (ImapHelper.IsBusy)
                    return;

                MailNavigation.IsLoadingBarRun = true;
                ImapHelper.IsBusy = true;

                foreach (Message i in MailNavigation.FilteredMessageItems.ToArray())
                    MailNavigation.MessageItems.Remove(i);

                foreach (Message i in MailNavigation.MessageItems.ToArray())
                    MailNavigation.MessageItems.Remove(i);

                ContentFrame.Content = null;

                Message[] messages = await InitialFetchingMail("INBOX");

                if (messages == null)
                    throw new Exception();

                if (messages != null)
                {
                    if (!await UpdateMessageData(messages, "INBOX"))
                        throw new Exception();
                }

                ImapHelper.IsBusy = false;
                MailNavigation.IsLoadingBarRun = false;
            }
            catch (Exception)
            {
                ContentDialog dialog = new();
                dialog.PrimaryButtonText = "OK";
                dialog.Content = "Failed to sync your mail! Please try again...";
                _ = dialog.ShowAsync();

                ImapHelper.IsBusy = false;
                MailNavigation.IsLoadingBarRun = false;
            }
        }

        private async void SideBarNavigation_OnChangedAccount(object sender, EventArgs e)
        {
            try
            {
                if (ImapHelper.IsBusy)
                    return;

                ImapHelper.Client?.Dispose();

                ImapHelper.IsBusy = true;
                MailNavigation.IsLoadingBarRun = true;

                foreach (Message i in MailNavigation.FilteredMessageItems.ToArray())
                    MailNavigation.MessageItems.Remove(i);

                foreach (Message i in MailNavigation.MessageItems.ToArray())
                    MailNavigation.MessageItems.Remove(i);

                Message[] messages = await InitialFetchingMail("INBOX");

                if (messages == null)
                    throw new Exception();

                if (messages != null)
                {
                    if (!await UpdateMessageData(messages, "INBOX"))
                        throw new Exception();
                }

                ImapHelper.IsBusy = false;
                MailNavigation.IsLoadingBarRun = false;
            }

            catch (Exception)
            {
                ContentDialog dialog = new();
                dialog.PrimaryButtonText = "OK";
                dialog.Content = "Failed to sync your mail! Please try again...";
                _ = dialog.ShowAsync();

                ImapHelper.IsBusy = false;
                MailNavigation.IsLoadingBarRun = false;
            }
        }
    }
}