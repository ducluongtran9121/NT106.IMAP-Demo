using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Emeow.UserControls
{
    public sealed partial class HorizontalHeaderTextBox : UserControl
    {
        public string Header
        {
            get => (string)GetValue(header);
            set => SetValue(header, value);
        }
        public static readonly DependencyProperty header =
            DependencyProperty.Register("Header", typeof(string), typeof(UserControl), new PropertyMetadata(string.Empty));

        public string Text
        {
            get => (string)GetValue(text);
            set => SetValue(text, value);
        }
        public static readonly DependencyProperty text =
            DependencyProperty.Register("Text", typeof(string), typeof(UserControl), new PropertyMetadata(string.Empty));

        public HorizontalHeaderTextBox()
        {
            this.InitializeComponent();
        }
    }
}
