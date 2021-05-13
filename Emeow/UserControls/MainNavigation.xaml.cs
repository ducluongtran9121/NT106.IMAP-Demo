using Emeow.ControlItems;
using Emeow.Pages;
using Emeow.Dialog;
using EmeowDatabase;
using Emeow.Common;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;

namespace Emeow.UserControls
{
    public sealed partial class MainNavigation : Microsoft.UI.Xaml.Controls.NavigationView
    {
        public ObservableCollection<INavigationControlItem> Items { get; private set; } = new ObservableCollection<INavigationControlItem>();

        public Microsoft.UI.Xaml.Controls.NavigationViewItem itemRightClickSelected { get; set; }

        public delegate void EvenHandler(object sender, EventArgs e);

        public event EventHandler OnNavigatePage;

        public MainNavigation()
        {
            this.InitializeComponent();

            InitializeNavItems();
        }

        private void InitializeNavItems()
        {
            Items.Add(new NavItem()
            {
                Content = "New Mail",
                Glyph = "\xE948",
                Tag = "Nav_NewMail",
                SelectOnInvoked = false,
            });
            Items.Add(new NavSeparatorItem());
            Items.Add(new NavListItem()
            {
                Content = "Accounts",
                Glyph = "\xE716",
                Tag = "Nav_Account",
                Child = new ObservableCollection<INavigationControlItem>()
                {
                    new NavItem
                    {
                        Content = "Add account",
                        Glyph = "\xE8FA",
                        Tag = "Nav_Add_Account",
                        SelectOnInvoked = false
                    }
                }
            });
        }

        private void ResizePaneBar_ManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
        {
            if (IsPaneOpen)
            {
                if (OpenPaneLength >= 200)
                    OpenPaneLength += e.Delta.Translation.X;
                if (OpenPaneLength < 200)
                    OpenPaneLength = 200;
                if (OpenPaneLength > 350)
                    OpenPaneLength = 350;
            }
        }

        private void ResizePaneBar_PointerCanceled(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
        }

        private void ResizePaneBar_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (IsPaneOpen)
            {
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeWestEast, 0);
            }
        }

        private void ResizePaneBar_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
        }

        private async void NavigationView_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {

            }
            else
            {
                var item = args.InvokedItemContainer;
                if (item != null)
                {
                    switch (item.Tag.ToString())
                    {
                        case "Nav_NewMail":
                            OnNavigatePage(typeof(NewMailPage), null);
                            break;

                        case "Nav_Account":
                            break;

                        case "Nav_Add_Account":
                            SigninDialog signinDialog = new SigninDialog();
                            await signinDialog.ShowAsync();

                            if (signinDialog.Account != null)
                            {
                                NavListItem accounts = Items[2] as NavListItem;

                                signinDialog.Account.Tag = "Nav_Account_" + accounts.Child.Count.ToString();

                                accounts.Child.Insert(accounts.Child.Count - 1, signinDialog.Account);
                            }
                            break;

                        default:
                            break;
                    }
                }
            }
        }
    }

    public class NavigationItemDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NavigationItemTemplate { get; set; }
        public DataTemplate NavigationListItemTemplate { get; set; }
        public DataTemplate NavigationAccountItemTemplate { get; set; }
        public DataTemplate NavigationSeparatorItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item != null && item is INavigationControlItem)
            {
                INavigationControlItem navigationControlItem = item as INavigationControlItem;

                switch (navigationControlItem.ItemType)
                {
                    case NavigationControlItemType.NavItem:
                        return NavigationItemTemplate;

                    case NavigationControlItemType.NavList:
                        return NavigationListItemTemplate;

                    case NavigationControlItemType.NavSeparator:
                        return NavigationSeparatorItemTemplate;

                    case NavigationControlItemType.NavAccount:
                        return NavigationAccountItemTemplate;
                }
            }
            return null;
        }
    }
}
