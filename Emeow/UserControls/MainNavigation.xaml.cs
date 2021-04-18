using Emeow.ControlItems;
using Emeow.User;
using Emeow.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Emeow.UserControls
{
    public sealed partial class MainNavigation : UserControl
    {
        public ObservableCollection<INavigationControlItem> Items { get; private set; } = new ObservableCollection<INavigationControlItem>();

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
            CompactWidth = MainNavigationView.CompactPaneLength.ToString();
            Items.Add(new MailItem { Content = "New Mail", Glyph = "\xE948", ItemType = NavigationControlItemType.NewMail, Tag = "Nav_NewMail" });
            Items.Add(new SpaceSeparatorItem { Content = "", Glyph = "", ItemType = NavigationControlItemType.SpaceSeparator, Tag = "nfu" });
            Items.Add(new AccountItem { Content = "Account", Glyph = "\xE77B", ItemType = NavigationControlItemType.Account, Tag = "Nav_Account", 
                Child = new ObservableCollection<INavigationControlItem>() { new MailItem { Content = "ng8", Glyph = "\xE77B", ItemType = NavigationControlItemType.NewMail } }
            });;
        }

        private void MainNavigationView_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
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
                            ContentFrame.Navigate(typeof(NewMailPage));
                            break;
                        case "Nav_Account":
                            break;
                    }
                }

            }
        }
    }
    public class NavigationItemDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NewMailNavigationItemTemplate { get; set; }
        public DataTemplate AccountNavigationItemTemplate { get; set; }
        public DataTemplate SpaceSeparatorNavigationItemTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item != null && item is INavigationControlItem)
            {
                INavigationControlItem navigationControlItem = item as INavigationControlItem;

                switch (navigationControlItem.ItemType)
                {
                    case NavigationControlItemType.NewMail:
                        return NewMailNavigationItemTemplate;

                    case NavigationControlItemType.Account:
                        return AccountNavigationItemTemplate;

                    case NavigationControlItemType.SpaceSeparator:
                        return SpaceSeparatorNavigationItemTemplate;
                }
            }
            return null;
        }
    }
}
