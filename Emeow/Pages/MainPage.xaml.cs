using EmeowIMAP;
using Emeow.User;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media.Animation;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Emeow.Pages
{
    public sealed partial class MainPage : Page
    {
        public MailClient Client { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            InitializeTitleBar();

            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ListMailControls.StartLoadingAnimation();

                Client = new MailClient();

                if (!Client.InitiallizeConnection())
                    throw new Exception("❗❗❗ Can't connect to server! Please check your connection and try again!");

                var check = await Client.Login();
                if (check == false)
                    throw new Exception("❗❗❗ Can't log into your account! Please check your connection and try again!");

                check = await Client.SelectMailBox("inbox");
                if(check == false)
                    throw new Exception("❗❗❗ Can't select this mailbox! Please check your connection and try again!");

                ObservableCollection<Mail> temp = new ObservableCollection<Mail>();

                for (int i = 1; i <= 2; i++)
                {
                    List<string> a = await Client.GetMailHeader(i);
                    if (a == null)
                        throw new Exception("❗❗❗ Error while fetching your mails! Please check your connection and try again!");

                    string mess = await Client.GetMailText(i);
                    if (mess == null)
                        throw new Exception("❗❗❗ Error while fetching your mails! Please check your connection and try again!");

                    Mail c = new Mail(a, mess);

                    temp.Add(c);
                }

                foreach (Mail i in temp)
                {
                    ListMailControls.MailItems.Add(i);
                }

                ListMailControls.StopLoadingAnimation();
            }
            catch (Exception ex) 
            {
                Client = null;
                ListMailControls.StopLoadingAnimation();

                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Error";
                dialog.CloseButtonText = "OK";
                dialog.Content = ex.Message;
                await dialog.ShowAsync();
            }
        }

        private void ListMailControls_OnMailItemSelected(object sender, EventArgs e)
        {
            ContentFrame.Navigate(typeof(MailPage), sender as Mail);
        }

        private async void ListMailControls_OnMailSync(object sender, EventArgs e)
        {
            try
            {
                ListMailControls.StartLoadingAnimation();

                if (Client == null)
                {
                    Client = new MailClient();

                    if (!Client.InitiallizeConnection())
                        throw new Exception("❗❗❗ Can't connect to server! Please check your connection and try again!");

                    var check = await Client.Login();
                    if (check == false)
                        throw new Exception("❗❗❗ Can't log into your account! Please check your connection and try again!");

                    check = await Client.SelectMailBox("inbox");
                    if (check == false)
                        throw new Exception("❗❗❗ Can't select this mailbox! Please check your connection and try again!");
                }

                ObservableCollection<Mail> temp = new ObservableCollection<Mail>();

                for (int i = 1; i <= 3; i++)
                {
                    List<string> a = await Client.GetMailHeader(i);
                    if (a == null)
                        throw new Exception("❗❗❗ Error while fetching your mails! Please check your connection and try again!");

                    string mess = await Client.GetMailText(i);
                    if (mess == null)
                        throw new Exception("❗❗❗ Error while fetching your mails! Please check your connection and try again!");

                    Mail c = new Mail(a, mess);

                    temp.Add(c);
                }

                foreach (Mail i in ListMailControls.MailItems.ToArray())
                {
                    if (!temp.Any(x => x.AddressFrom == i.AddressFrom && x.Subject == i.Subject))
                        ListMailControls.MailItems.Remove(i);
                }

                foreach (Mail i in temp)
                {
                    if (!ListMailControls.MailItems.Any(x => x.AddressFrom == i.AddressFrom && x.Subject == i.Subject))
                        ListMailControls.MailItems.Add(i);
                }

                ListMailControls.StopLoadingAnimation();
            }
            catch(Exception ex)
            {
                Client = null;
                ListMailControls.StopLoadingAnimation();

                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Error";
                dialog.CloseButtonText = "OK";
                dialog.Content = ex.Message;
                await dialog.ShowAsync();
            }
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
            ListMailControls.Margin = new Thickness() { Right = 0, Bottom = 0, Left = 0, Top = sender.Height };
            FrameGrid.Margin = new Thickness() { Right = 0, Bottom = 0, Left = 0, Top = sender.Height };          
        }

        private void MainNavigationView_PaneClosing(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewPaneClosingEventArgs args)
        {
            TitleBarStoryBoard.Children[0].SetValue(DoubleAnimation.FromProperty, TitlebarTransform.X);
            TitleBarStoryBoard.Children[0].SetValue(DoubleAnimation.ToProperty, TitlebarTransform.X + 12);
            TitleBarStoryBoard.Begin();
        }

        private void MainNavigationView_PaneOpening(Microsoft.UI.Xaml.Controls.NavigationView sender, object args)
        {
            TitleBarStoryBoard.Children[0].SetValue(DoubleAnimation.FromProperty, TitlebarTransform.X);
            TitleBarStoryBoard.Children[0].SetValue(DoubleAnimation.ToProperty, TitlebarTransform.X - 12);
            TitleBarStoryBoard.Begin();
        }

        private void MainNavigationView_OnNavigatePage(object sender, EventArgs e)
        {
            if (ContentFrame.CurrentSourcePageType != sender as Type)
                ContentFrame.Navigate(sender as Type);
        }
    }
}
