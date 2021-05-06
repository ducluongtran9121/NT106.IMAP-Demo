using System;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Emeow.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SignInPage : Page
    {
        public SignInPage()
        {
            this.InitializeComponent();
            Background = new AcrylicBrush()
            {
                BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                TintColor = (Color)App.Current.Resources["SolidBackgroundAcrylic"],
                FallbackColor = (Color)App.Current.Resources["SolidFallbackBackground"],
                TintOpacity = 0.7,
                TintLuminosityOpacity = 0.85
            };
        }
    }
}
