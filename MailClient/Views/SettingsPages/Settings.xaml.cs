using MailClient.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MailClient.Views.SettingsPages
{
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            this.InitializeComponent();

            TLSToggleButton.IsOn = SettingsHelper.IsUseTLS;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ThemeChooser.SelectedIndex = (int)ThemeHelper.RootTheme;
        }

        private void ThemeChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ThemeHelper.RootTheme = (ElementTheme)(sender as ComboBox).SelectedIndex;
        }

        private void TLSToggleButton_Toggled(object sender, RoutedEventArgs e)
        {
            SettingsHelper.IsUseTLS = TLSToggleButton.IsOn;
        }
    }
}