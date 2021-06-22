using MailClient.Helpers;
using MailClient.IMAP;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace MailClient.Views
{
    public sealed partial class WelcomePage : Page
    {
        private const string MailAddressPattern = @"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)";

        public WelcomePage()
        {
            this.InitializeComponent();

            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
        }

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            NavigationHelper.TitlebarHeight = sender.Height;
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged -= TitleBar_LayoutMetricsChanged;
        }

        private void SetEnabledControl(bool value)
        {
            SigninButton.IsEnabled = value;
            UsernameTextbox.IsEnabled = value;
            PasswordPbox.IsEnabled = value;
        }

        private void Textbox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SigninButton_Click(null, null);
            }
        }

        private async void SigninButton_Click(object sender, RoutedEventArgs e)
        {
            SetEnabledControl(false);

            SigninStatusTextblock.Visibility = Visibility.Visible;

            if (UsernameTextbox.Text == string.Empty || PasswordPbox.Password == string.Empty)
            {
                SigninStatusTextblock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 95, 95));
                SigninStatusTextblock.Text = "Please do not empty Username or Password! 😡";
                SetEnabledControl(true);
                return;
            }

            if (!Regex.IsMatch(UsernameTextbox.Text, MailAddressPattern))
            {
                SigninStatusTextblock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 95, 95));
                SigninStatusTextblock.Text = "Please use a valid address! 😡";
                SetEnabledControl(true);
                return;
            }

            SigninStatusTextblock.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            SigninStatusTextblock.Text = "Signing you in... ";
            Processbar.IsIndeterminate = true;
            Processbar.ShowError = false;

            try
            {
                ConnectionHelper.CurrentClient = new Client();

                if (!await ConnectionHelper.CurrentClient.InitiallizeConnectionAsync())
                {
                    throw new Exception("Failed to connect to the server. Please check your connection and try again!");
                }

                if (!await ConnectionHelper.CurrentClient.LoginAsync(UsernameTextbox.Text, PasswordPbox.Password))
                {
                    if (!ConnectionHelper.CurrentClient.IsConnected)
                    {
                        throw new Exception("Failed to connect to the server. Please check your connection and try again!");
                    }
                    else
                    {
                        throw new Exception("Wrong Username or Password. Please try again!");
                    }
                }
                else
                {
                    Processbar.IsIndeterminate = false;
                    SigninStatusTextblock.Text = "Signed in! Please wait...";
                    await Task.Delay(1000);

                    LoadingControl.IsLoading = true;

                    // Logged in, create this account database, and add logged account into account database
                    AccountHelper.CurrentAccount =
                        new DataModels.Mail.Account { Address = UsernameTextbox.Text, Name = "Huỳnh Thái Thi", Glyph = "\xED56" };

                    SettingsHelper.IsFirstTimeLogin = true;

                    var account = AccountHelper.CurrentAccount;
                    DatabaseHelper.CurrentDatabaseName = $"{AccountHelper.CurrentAccount.Address}.db";

                    await DatabaseHelper.InitializeAccountDatabaseAsync(DatabaseHelper.CurrentDatabaseName);
                    await DatabaseHelper.InsertDataAsync(DatabaseHelper.AccountsDatabaseName, DatabaseHelper.AccountTableName,
                        new string[] { account.Address, account.Name, PasswordPbox.Password, account.Glyph });

                    // Show beautiful loading control longer :D
                    await Task.Delay(1000);

                    // Navigate to MainPage
                    _ = (Window.Current.Content as Frame).Navigate(typeof(MainPage));
                }
            }
            catch (Exception ex)
            {
                ContentDialog dialog = new();
                dialog.PrimaryButtonText = "OK";
                dialog.Content = ex.Message;
                _ = await dialog.ShowAsync();

                SetEnabledControl(true);
                LoadingControl.IsLoading = false;
                Processbar.ShowError = true;
                SigninStatusTextblock.Visibility = Visibility.Collapsed;

                ConnectionHelper.CurrentClient.Dispose();
            }
        }
    }
}