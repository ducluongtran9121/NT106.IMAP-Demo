using MailClient.DataModels.Imap;
using MailClient.Helpers;
using MailClient.Imap;
using MailClient.Imap.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace MailClient.Dialogs
{
    public sealed partial class SigninDialog : ContentDialog
    {
        private const string MailAddressPattern = @"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)";

        public bool IsLoggedIn { get; set; }

        public Account Account { get; set; }

        public SigninDialog()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void SetEnabledControl(bool value)
        {
            SigninButton.IsEnabled = value;
            UsernameTextbox.IsEnabled = value;
            PasswordPbox.IsEnabled = value;
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

                // Logged in, create this account database, and add logged account into account database
                AccountHelper.CurrentAccount =
                    new Account { Address = UsernameTextbox.Text, Glyph = "\xED56" };


                SettingsHelper.IsFirstTimeLogin = true;

                var account = AccountHelper.CurrentAccount;
                DatabaseHelper.CurrentDatabaseName = $"{AccountHelper.CurrentAccount.Address}.db";

                await DatabaseHelper.InitializeAccountDatabaseAsync(DatabaseHelper.CurrentDatabaseName);
                await DatabaseHelper.InsertDataAsync(DatabaseHelper.AccountsDatabaseName, DatabaseHelper.AccountTableName,
                    new string[] { account.Address, account.Name, AES.EncryptToHex(PasswordPbox.Password, ImapHelper.Key, ImapHelper.Iv), account.Glyph, "" });

                IsLoggedIn = true;
                Account = new Account { Address = UsernameTextbox.Text, Glyph = "\xED56" };
                this.Hide();

            }
            catch (ConnectionException)
            {
                SigninStatusTextblock.Text = "Failed to connect to the server. Please check your connection and try again!";
                ImapHelper.Client?.Dispose();
            }
            catch (AuthenticationException)
            {
                SigninStatusTextblock.Text = "Wrong Username or Password. Please try again!";
                ImapHelper.Client?.Dispose();
            }
            catch (Exception)
            {
                SigninStatusTextblock.Text = "An error occurred Please try again!";
                ImapHelper.Client?.Dispose();

                if (File.Exists(Path.Combine(ApplicationData.Current.LocalFolder.Path, DatabaseHelper.CurrentDatabaseName)))
                {
                    File.Delete(Path.Combine(ApplicationData.Current.LocalFolder.Path, DatabaseHelper.CurrentDatabaseName));
                }

                ImapHelper.Client?.Dispose();

                ImapHelper.Client?.Dispose();
            }
            finally
            {
                SetEnabledControl(true);
                Processbar.ShowError = true;
            }
        }
    }
}
