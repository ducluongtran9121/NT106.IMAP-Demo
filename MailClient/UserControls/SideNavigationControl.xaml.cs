using MailClient.DataModels.NavigationControlItems;
using MailClient.DataModels.Mail;
using MailClient.Helpers;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using muxc = Microsoft.UI.Xaml.Controls;

namespace MailClient.UserControls
{
    public sealed partial class SideNavigationControl : muxc.NavigationView
    {
        public ObservableCollection<INavigationControlItem> NavigationItems { get; set; }

        public NavigationListItem AccountItems { get; set; }

        public delegate void EvenHandler(object sender, EventArgs e);

        public event EventHandler OnNavigatePage;

        public SideNavigationControl()
        {
            this.InitializeComponent();

            InitializeItem();

            Loaded += (s, e) => (SettingsItem as muxc.NavigationViewItem).SelectsOnInvoked = false;
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

        // Remember to call this function before rootpage loaded.
        public void SetAccountItems(Account[] accounts)
        {
            int i = 0;
            foreach (Account account in accounts)
            {
                AccountItems.Child.Insert(0, new NavigationAccountItem
                {
                    Address = account.Address,
                    Text = account.Name,
                    Glyph = account.Glyph,
                    SelectOnInvoked = true,
                    Tag = $"Nav_Account_{i}"
                });
                i++;
            }
            SelectedItem = AccountItems.Child[0];
        }

        private void ResizePaneBar_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (IsPaneOpen)
            {
                if (OpenPaneLength >= 200)
                {
                    OpenPaneLength += e.Delta.Translation.X;
                }

                if (OpenPaneLength < 200)
                {
                    OpenPaneLength = 200;
                }

                if (OpenPaneLength > 350)
                {
                    OpenPaneLength = 350;
                }
            }
        }

        private void ResizePaneBar_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (IsPaneOpen)
            {
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeWestEast, 0);
            }
        }

        private void ResizePaneBar_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
        }

        private void NavigationView_ItemInvoked(muxc.NavigationView sender, muxc.NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                return;
            }

            var item = args.InvokedItemContainer;
            if (item != null)
            {
                switch (item.Tag.ToString())
                {
                    case "Nav_NewMail":
                        //OnNavigatePage(typeof(Views.MailEditorPage), null);
                        break;

                    default:
                        break;
                }
            }
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