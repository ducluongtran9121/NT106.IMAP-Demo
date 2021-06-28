using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MailClient.UserControls
{
    public sealed partial class SearchBoxControl : TextBox
    {
        public delegate void EvenHandler(object sender, EventArgs e);

        public event EventHandler OnSearchButtonClick;

        public SearchBoxControl()
        {
            this.InitializeComponent();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            this.Focus(FocusState.Programmatic);
            OnSearchButtonClick(sender, null);
        }
    }
}