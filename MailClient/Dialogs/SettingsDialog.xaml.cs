using MailClient.Views.SettingsPages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MailClient.Dialogs
{
    public sealed partial class SettingsDialog : ContentDialog
    {
        private FrameworkElement RootAppElement => Window.Current.Content as FrameworkElement;

        public SettingsDialog()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void NavigationView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = args.SelectedItem as Microsoft.UI.Xaml.Controls.NavigationViewItem;

            string tag = selectedItem.Tag.ToString();

            _ = tag switch
            {
                "Nav_About" => SettingsContentFrame.Navigate(typeof(About)),
                "Nav_Appearances" => SettingsContentFrame.Navigate(typeof(Appearances)),
                _ => SettingsContentFrame.Navigate(typeof(About))
            };
        }

        private void SettingNavigation_Loaded(object sender, RoutedEventArgs e)
        {
            SettingNavigation.SelectedItem = SettingNavigation.MenuItems[1];
        }
    }
}