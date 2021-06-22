using MailClient.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MailClient.Views.SettingsPages
{
    public sealed partial class Appearances : Page
    {
        public Appearances()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ThemeChooser.SelectedIndex = (int)ThemeHelper.RootTheme;
        }

        private void ThemeChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ThemeHelper.RootTheme = (ElementTheme)(sender as ComboBox).SelectedIndex;
        }
    }
}