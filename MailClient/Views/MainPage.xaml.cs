using MailClient.DataModels.Mail;
using MailClient.Helpers;
using MailClient.IMAP;
using MailClient.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using muxc = Microsoft.UI.Xaml.Controls;

namespace MailClient.Views
{
    public sealed partial class MainPage : Page
    {
        public static SideNavigationControl Current;

        public MainPage()
        {
            this.InitializeComponent();

            SideNavigation.PaneDisplayMode = muxc.NavigationViewPaneDisplayMode.Left;

            Current = SideNavigation;

            // Set titlebar
            Window.Current.SetTitleBar(AppTitleBar);

            // If this page is loaded from Welcome page, LayoutMetricsChanged event in this page
            // isn't triggerged
            // Set padding ContentContainer
            ContentContainer.Padding = new Thickness(24, NavigationHelper.TitlebarHeight + 12, 24, 12);

            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += (s, e) =>
            {
                UpdateAppTitle(s);

                // Set padding ContentContainer
                ContentContainer.Padding = new Thickness(24, s.Height + 12, 24, 12);
            };
        }

        // Load accounts and mailboxes from database
        private async void Page_Loading(FrameworkElement sender, object args)
        {
            // Load list of account from database
            List<List<string>> accounts =
                await DatabaseHelper.GetTableDataAsync(DatabaseHelper.AccountsDatabaseName, DatabaseHelper.AccountTableName);

            if (accounts == null)
                return;

            foreach (List<string> account in accounts)
            {
                AccountHelper.Accounts.Add(new Account
                {
                    Address = account[0],
                    Name = account[1],
                    Glyph = account[3],
                    MailBoxes = new ObservableCollection<MailBox>()
                    {
                        new MailBox() { Name = "Inbox"}
                    }
                });
            }

            // Pass accounts data to SideNavigation control
            SideNavigation.SetAccountItems(AccountHelper.Accounts.ToArray());

            // Set default account to the first account
            AccountHelper.CurrentAccount = AccountHelper.Accounts[0];

            LoadingControl.IsLoading = false;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationHelper.UpdateTitleBar(NavigationHelper.IsLeftMode);

            var client = ConnectionHelper.CurrentClient;
            if (client == null)
            {
                client = new Client();
            }

            if (!client.IsConnected)
            {
                if (!await client.InitiallizeConnectionAsync())
                    return;
            }

            if (!client.IsLoggedIn)
            {
                string[] selected = await DatabaseHelper.SelectDataAsync(AccountHelper.CurrentAccount.Address, "Inbox", new string[] { "Password" },
                    new Tuple<string, string>[]
                    {
                        new Tuple<string, string>("Address", AccountHelper.CurrentAccount.Address)
                    });
                if (!await client.LoginAsync(AccountHelper.CurrentAccount.Address, selected[0]))
                    return;
            }

            if (!await client.SelectMailBoxAsync("Inbox"))
                return;

            ObservableCollection<MailMessage> messages = new();

            for (int i = 1; i <= 3; i++)
            {
                string[] header = await client.GetMailHeaderAsync(i);
                if (header == null)
                    return;

                string text = await client.GetMailBodyAsync(i);
                if (text == null)
                    return;

                messages.Add(new MailMessage(header, text));
            }

            var mess = AccountHelper.CurrentAccount.MailBoxes[0].Messages;
            foreach (MailMessage i in mess)
            {
                if (!messages.Any(x => x.From == i.From && x.Subject == i.Subject))
                {
                    _ = mess.Remove(i);
                    await DatabaseHelper.DeleteRowsAsync($"{AccountHelper.CurrentAccount.Address}.db", "Inbox",
                        new Tuple<string, string>[]
                        {
                            new Tuple<string, string>("Subject", i.Subject)
                        });
                };
            }

            int j = 0;
            
            foreach (MailMessage i in messages)
            {
                if (!mess.Any(x => x.From == i.From && x.Subject == i.Subject))
                {
                    mess.Add(i);
                    await DatabaseHelper.InsertDataAsync($"{AccountHelper.CurrentAccount.Address}.db", "Inbox",
                        new string[] { j.ToString(), i.From, i.GetToString(), i.Subject, "123", i.Body, "123", "0", "0", "0", "0", "0", "0" });
                }
                j += 1;
            }
        }

        private void UpdateAppTitle(CoreApplicationViewTitleBar coreTitleBar)
        {
            // Ensure the custom title bar does not overlap window caption controls
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(currMargin.Left, currMargin.Top, coreTitleBar.SystemOverlayRightInset, currMargin.Bottom);
        }

        private void SideNavigation_PaneOpening(muxc.NavigationView sender, object args)
        {
            UpdateAppTitleMargin(sender);
        }

        private void SideNavigation_PaneClosing(muxc.NavigationView sender, muxc.NavigationViewPaneClosingEventArgs args)
        {
            UpdateAppTitleMargin(sender);
        }

        private void SideNavigation_DisplayModeChanged(muxc.NavigationView sender, muxc.NavigationViewDisplayModeChangedEventArgs args)
        {
            Thickness currMargin = AppTitleBar.Margin;
            AppTitleBar.Margin = new Thickness(sender.CompactPaneLength, currMargin.Top, currMargin.Right, currMargin.Bottom);
            UpdateAppTitleMargin(sender);
        }

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
    }
}