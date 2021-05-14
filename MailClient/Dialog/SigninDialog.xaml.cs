using MailClientDatabase;
using Emeow.ControlItems;
using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Emeow.Dialog
{
    public sealed partial class SigninDialog : ContentDialog
    {
        public NavAccountItem Account
        {
            get => (NavAccountItem)GetValue(account);
            set => SetValue(account, value);
        }
        public static readonly DependencyProperty account =
            DependencyProperty.Register("Username", typeof(NavAccountItem), typeof(UserControl), new PropertyMetadata(null));

        // Interesting things about IMAP :D
        List<string> interestingIMAP = new List<string>
        {
            "💡 Do you know: IMAP was originally created by Mark Crispin at Stanford in the 1980s.",
            "💡 Do you know: IMAP has a total of 4 versions. The latest version is IMAP4.",
            "📮📮📮 S: * OK IMAP4rev1 Service Ready.",
            "💡 Do you know: IMAP stands for Internet Message Access Protocol. Interesting! Right? 😍",

        };

        public SigninDialog()
        {
            this.InitializeComponent();

            Random random = new Random();
            StatusTbl.Text = interestingIMAP[random.Next(interestingIMAP.Count)];
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private async void SignInBtn_Click(object sender, RoutedEventArgs e)
        {

            StatusTbl.Foreground = new SolidColorBrush { Color = Color.FromArgb(255, 255, 95, 95) };

            if (EmailAddrTbx.Text == string.Empty || PasswordPwb.Password == string.Empty)
            {
                StatusTbl.Text = "❗❗❗ Please don't leave a blank username or password.";
                return;
            }

            string[] email = EmailAddrTbx.Text.Split('@');

            if (email.Length != 2)
            {
                StatusTbl.Text = "❗❗❗ Please input a valid email address. Ex: hellothere@imap.com.";
                return;
            }

            if (email[0] == string.Empty || email[1] == string.Empty)
            {
                StatusTbl.Text = "❗❗❗ Please input a valid email address. Ex: hellothere@imap.com.";
                return;
            }

            List<string> data = await Database.SearchAccountData(email[0], "'" + email[1] + "'", PasswordPwb.Password);

            if (data.Count == 0)
            {
                StatusTbl.Text = "❗❗❗ Wrong username or password.";
                return;
            }

            Account = new NavAccountItem()
            {
                Address = data[0] + "@" + data[1],
                Content = data[2],
                Glyph = data[4]
            };
            this.Hide();
        }

        private void SignIn_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SignInBtn_Click(null, null);
            }
        }
    }
}
