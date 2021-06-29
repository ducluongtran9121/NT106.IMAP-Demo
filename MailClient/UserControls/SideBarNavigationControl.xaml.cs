using MailClient.DataModels.Imap;
using MailClient.DataModels.NavigationControlItems;
using MailClient.Dialogs;
using MailClient.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using muxc = Microsoft.UI.Xaml.Controls;

namespace MailClient.UserControls
{
    public sealed partial class SideBarNavigationControl : muxc.NavigationView
    {
        public ObservableCollection<INavigationControlItem> NavigationItems { get; set; }

        public NavigationListItem AccountItems { get; set; }

        private string SelectedItemTagSaved;

        public delegate void EvenHandler(object sender, EventArgs e);

        public event EventHandler OnAccountSignedIn;

        public event EventHandler OnChangedAccount;

        public SideBarNavigationControl()
        {
            this.InitializeComponent();

            InitializeItem();
        }

        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            (SettingsItem as muxc.NavigationViewItem).SelectsOnInvoked = false;
        }

        // Remember to call this function before root page loaded.
        public void SetAccountItems(Account[] accounts)
        {
            int i = 0;
            foreach (Account account in accounts)
            {
                AccountItems.Child.Insert(0 + i, new NavigationAccountItem
                {
                    Address = account.Address,
                    Text = account.Name,
                    Glyph = account.Glyph,
                    SelectOnInvoked = true,
                    Tag = $"Nav_Account_{i}"
                });
                i++;
            }

            SelectedItemTagSaved = "Nav_Account_0";
            SelectedItem = AccountItems.Child[0];
        }

        private void InitializeItem()
        {
            AccountItems = new NavigationListItem("Accounts", "\xE716", "Nav_Accounts", false,
                new ObservableCollection<INavigationControlItem>()
                {
                    new NavigationItem("Add account", "\xE8FA", "Nav_Add_Account", false)
                });

            NavigationItems = new ObservableCollection<INavigationControlItem>
            {
                new NavigationEmptyItem(),
                new NavigationItem("New Mail", "\xE948", "Nav_NewMail", false),
                new NavigationEmptyItem(),
                AccountItems
            };
        }

        private async void NavigationView_ItemInvoked(muxc.NavigationView sender, muxc.NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                await new SettingsDialog().ShowAsync();
                return;
            }

            var item = args.InvokedItemContainer;
            if (item != null)
            {
                if (item.Tag.ToString().Contains("Nav_Account_"))
                {
                    if (SelectedItemTagSaved == item.Tag.ToString())
                        return;

                    int i = int.Parse(item.Tag.ToString().Replace("Nav_Account_", string.Empty));
                    NavigationAccountItem accountItem = AccountItems.Child[i] as NavigationAccountItem;

                    AccountHelper.CurrentAccount = new Account { Address = accountItem.Address, Glyph = accountItem.Glyph };
                    DatabaseHelper.CurrentDatabaseName = $"{AccountHelper.CurrentAccount.Address}.db";

                    OnChangedAccount(null, null);

                    return;
                }

                switch (item.Tag.ToString())
                {
                    case "Nav_NewMail":
                        //OnNavigatePage(typeof(Views.MailEditorPage), null);
                        break;

                    case "Nav_Add_Account":
                        SigninDialog signinDialog = new();
                        _ = await signinDialog.ShowAsync();

                        if (signinDialog.IsLoggedIn == false)
                            return;

                        NavigationAccountItem accountItem = new NavigationAccountItem
                        {
                            Address = AccountHelper.CurrentAccount.Address,
                            Glyph = AccountHelper.CurrentAccount.Glyph,
                            Tag = $"Nav_Account_{AccountItems.Child.Count - 1}"
                        };

                        AccountItems.Child.Insert(AccountItems.Child.Count - 1, accountItem);

                        SelectedItem = AccountItems.Child[^2];

                        OnAccountSignedIn(null, null);

                        break;

                    default:
                        break;
                }
            }
        }

        private void NavigationView_SelectionChanged(muxc.NavigationView sender, muxc.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
                SelectedItemTagSaved = args.SelectedItemContainer.Tag.ToString();
        }
    }

    public class NavigationItemDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NavigationItemTemplate { get; set; }
        public DataTemplate NavigationListItemTemplate { get; set; }
        public DataTemplate NavigationAccountItemTemplate { get; set; }
        public DataTemplate NavigationEmptyItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item != null && item is INavigationControlItem)
            {
                if (item is INavigationControlItem navigationControlItem)
                {
                    switch (navigationControlItem.ItemType)
                    {
                        case NavigationControlItemType.NavItem:
                            return NavigationItemTemplate;

                        case NavigationControlItemType.NavList:
                            return NavigationListItemTemplate;

                        case NavigationControlItemType.NavEmpty:
                            return NavigationEmptyItemTemplate;

                        case NavigationControlItemType.NavAccount:
                            return NavigationAccountItemTemplate;
                    }
                }
            }
            return NavigationItemTemplate;
        }
    }
}