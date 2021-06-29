using MailClient.Helpers;
using MailClient.Imap;
using MailClient.Imap.Crypto;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
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

            this.InitializeComponent();

            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;

            TLSToggleButton.IsOn = SettingsHelper.IsUseTLS;
        }

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            TitlebarHelper.TitlebarHeight = sender.Height;
            CoreApplication.GetCurrentView().TitleBar.LayoutMetricsChanged -= TitleBar_LayoutMetricsChanged;
        }

        private void SetEnabledControl(bool value)
        {
            SigninButton.IsEnabled = value;
            UsernameTextbox.IsEnabled = value;
            PasswordPbox.IsEnabled = value;
        }

        private void TLSToggleButton_Toggled(object sender, RoutedEventArgs e)
        {
            SettingsHelper.IsUseTLS = TLSToggleButton.IsOn;
        }

        private async void SigninButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.
                ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
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

                SigninStatusTextblock.Foreground = (SolidColorBrush)Application.Current.Resources["ButtonForeground"];
                SigninStatusTextblock.Text = "Signing you in... ";
                Processbar.IsIndeterminate = true;
                Processbar.ShowError = false;

                try
                {
                    ImapHelper.Client = new ImapClient();

                    if (!await ImapHelper.Client.ConnectAsync(ImapHelper.IPEndPoint, SettingsHelper.IsUseTLS))
                        throw new ConnectionException();

                    if (!await ImapHelper.Client.AuthenticateAsync(UsernameTextbox.Text, PasswordPbox.Password))
                        throw new AuthenticationException();

                    Processbar.IsIndeterminate = false;
                    SigninStatusTextblock.Text = "Signed in! Please wait...";
                    await Task.Delay(1000);

                    LoadingControl.IsLoading = true;

                    // Logged in, create this account database, and add logged account into account database
                    AccountHelper.CurrentAccount =
                        new DataModels.Imap.Account { Address = UsernameTextbox.Text, Name = "Huỳnh Thái Thi", Glyph = "\xED56" };

                    SettingsHelper.IsFirstTimeLogin = true;

                    var account = AccountHelper.CurrentAccount;
                    DatabaseHelper.CurrentDatabaseName = $"{AccountHelper.CurrentAccount.Address}.db";

                    await DatabaseHelper.InitializeAccountDatabaseAsync(DatabaseHelper.CurrentDatabaseName);
                    await DatabaseHelper.InsertDataAsync(DatabaseHelper.AccountsDatabaseName, DatabaseHelper.AccountTableName,
                        new string[] { account.Address, account.Name, AES.EncryptToHex(PasswordPbox.Password, ImapHelper.Key, ImapHelper.Iv), account.Glyph, "" });

                    // Show beautiful loading control longer :D
                    await Task.Delay(1000);

                    // Navigate to MainPage
                    _ = (Window.Current.Content as Frame).Navigate(typeof(MainPage));
                }
                catch (ConnectionException)
                {
                    ContentDialog dialog = new();
                    dialog.PrimaryButtonText = "OK";
                    dialog.Content = "Failed to connect to the server. Please check your connection and try again!";
                    _ = await dialog.ShowAsync();
                    ImapHelper.Client?.Dispose();
                }
                catch (AuthenticationException)
                {
                    ContentDialog dialog = new();
                    dialog.PrimaryButtonText = "OK";
                    dialog.Content = "Wrong Username or Password. Please try again!";
                    _ = await dialog.ShowAsync();
                    ImapHelper.Client?.Dispose();
                }
                catch (Exception)
                {
                    ContentDialog dialog = new();
                    dialog.PrimaryButtonText = "OK";
                    dialog.Content = "An error occurred Please try again!";
                    _ = await  dialog.ShowAsync();

                    ImapHelper.Client?.Dispose();
                }
                finally
                {
                    SetEnabledControl(true);
                    LoadingControl.IsLoading = false;
                    Processbar.ShowError = true;
                    SigninStatusTextblock.Visibility = Visibility.Collapsed;
                }
            });
        }
    }
}