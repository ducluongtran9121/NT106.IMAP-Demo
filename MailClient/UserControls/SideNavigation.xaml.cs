using MailClient.DataModels.NavigationControlItems;
using MailClient.Views.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MailClient.UserControls
{
    public sealed partial class SideNavigation : Microsoft.UI.Xaml.Controls.NavigationView
    {
        public ObservableCollection<INavigationControlItem> NavigationItems { get; set; }

        public delegate void EvenHandler(object sender, EventArgs e);

        public event EventHandler OnNavigatePage;

        public SideNavigation()
        {
            this.InitializeComponent();

            NavigationItems = new ObservableCollection<INavigationControlItem>();

            NavigationItems.Add(new NavigationItem("New Mail", "\xE948", "Nav_NewMail", false));
            NavigationItems.Add(new NavigationEmptyItem());
            NavigationItems.Add(new NavigationListItem("Accounts", "\xE716", "Nav_Accounts", false,
                new ObservableCollection<INavigationControlItem>()
                {
                    new NavigationAccountItem("Huỳnh Thái Thi", "19522256@thi123.com", "\xED56", "Nav_Account_0", true),
                    new NavigationItem("Add account", "\xE8FA", "Nav_Add_Account", false)
                }));

            SelectedItem = (NavigationItems[2] as NavigationListItem).Child[0];
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

        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            (SettingsItem as Microsoft.UI.Xaml.Controls.NavigationViewItem).SelectsOnInvoked = false;

            
        }

        private void NavigationView_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                App.NavigateToPage(typeof(Views.Settings));
                return;
            }

            var item = args.InvokedItemContainer;
            if (item != null)
            {
                switch(item.Tag.ToString())
                {
                    case "Nav_NewMail":
                        OnNavigatePage(typeof(MailEditPage), null);
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
