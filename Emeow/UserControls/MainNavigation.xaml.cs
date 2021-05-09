using Emeow.ControlItems;
using Emeow.Pages;
using Emeow.Dialog;
using EmeowDatabase;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Diagnostics;

namespace Emeow.UserControls
{
    public sealed partial class MainNavigation : UserControl
    {
        public ObservableCollection<INavigationControlItem> Items { get; set; } = new ObservableCollection<INavigationControlItem>();

        public Microsoft.UI.Xaml.Controls.NavigationViewItem itemRightClickSelected { get; set; }

        public string CompactWidth
        {
            get => (string)GetValue(compactWidth);
            set => SetValue(compactWidth, value);
        }
        public static readonly DependencyProperty compactWidth =
            DependencyProperty.Register("CompactWidth", typeof(double), typeof(UserControl), new PropertyMetadata(0.0));

        public MainNavigation()
        {
            this.InitializeComponent();

            InitializeNavItems();

            CompactWidth = MainNavigationView.CompactPaneLength.ToString();
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
                Glyph = "\xE77B",
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

        private async void MainNavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            List<List<string>> data = await Database.GetTableData(Database.Table.Accounts);

            try
            {
                NavListItem accounts = Items[2] as NavListItem;

                foreach (List<string> i in data)
                {
                    accounts.Child.Insert(accounts.Child.Count - 1, new NavAccountItem
                    {
                        Address = i[0] + "@" + i[1],
                        Content = i[2],
                        Tag = "Nav_Account_" + (accounts.Child.Count - 1).ToString(),
                        Glyph = i[4],
                    });
                }
            }
            catch(Exception) { }
        }

        private async void MainNavigationView_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
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
                            if (ContentFrame.CurrentSourcePageType != typeof(NewMailPage))
                            {
                                ContentFrame.Navigate(typeof(NewMailPage));
                            }
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

        private void NavAccountItem_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            Microsoft.UI.Xaml.Controls.NavigationViewItem item = sender as Microsoft.UI.Xaml.Controls.NavigationViewItem;

            itemRightClickSelected = item;

            item.ContextFlyout = NavAccountItemFlyout;
            item.ContextFlyout.ShowAt(item);
        }

        private void RemovemenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menuFlyoutItem = sender as MenuFlyoutItem;

            NavListItem list = Items[2] as NavListItem;

            list.Child.Remove(list.Child[itemRightClickSelected.Tag.ToString()[itemRightClickSelected.Tag.ToString().Length - 1] - 48]);

            for (int i = 0; i < list.Child.Count - 1; i++)
            {
                list.Child[i].Tag = "Nav_Account_" + i.ToString();
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
