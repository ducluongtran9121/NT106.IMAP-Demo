using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace MailClient.Helpers
{
    public static class NavigationHelper
    {
        private const string IsLeftModeKey = "IsNavigationOnLeftMode";

        public static bool IsLeftMode
        {
            get
            {
                object valueFromSettings = ApplicationData.Current.LocalSettings.Values[IsLeftModeKey];
                if (valueFromSettings == null)
                {
                    ApplicationData.Current.LocalSettings.Values[IsLeftModeKey] = true;
                    valueFromSettings = true;
                }
                return (bool)valueFromSettings;
            }

            set
            {
                UpdateTitleBar(value);
                ApplicationData.Current.LocalSettings.Values[IsLeftModeKey] = value;
            }
        }

        public static double TitlebarHeight = 0;

        private const string SelectedAccountIndexKey = "SelectedAccountIndex";

        public static int SelectedAccountIndex
        {
            get
            {
                object valueFromSettings = ApplicationData.Current.LocalSettings.Values[SelectedAccountIndexKey];
                if (valueFromSettings == null)
                {
                    ApplicationData.Current.LocalSettings.Values[SelectedAccountIndexKey] = 0;
                    valueFromSettings = 0;
                }
                return (int)valueFromSettings;
            }

            set => ApplicationData.Current.LocalSettings.Values[SelectedAccountIndexKey] = value;
        }

        public static void UpdateTitleBar(bool isLeftMode)
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = isLeftMode;

            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

            if (isLeftMode)
            {
                Views.MainPage.Current.PaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.Auto;
            }
            else
            {
                Views.MainPage.Current.PaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top;
            }

            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }
    }
}