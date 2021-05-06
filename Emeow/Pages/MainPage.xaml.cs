using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;


namespace Emeow.Pages
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            InitializeTitleBar();
        }

        private void InitializeTitleBar()
        {
            var titleBarCore = CoreApplication.GetCurrentView().TitleBar;
            titleBarCore.ExtendViewIntoTitleBar = true;
            titleBarCore.LayoutMetricsChanged += Titlebar_LayoutMetricsChanged;
            Window.Current.SetTitleBar(AppTitleBar);

            var titleBarView = ApplicationView.GetForCurrentView().TitleBar;
            titleBarView.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;
            titleBarView.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
        }

        private void Titlebar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            AppTitleBar.Height = sender.Height;
        }
    }
}
