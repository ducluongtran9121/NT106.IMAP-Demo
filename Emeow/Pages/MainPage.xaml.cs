using System;
using System.Collections.Generic;
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
