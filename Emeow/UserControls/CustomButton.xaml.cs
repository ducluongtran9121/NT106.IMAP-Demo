using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Emeow.UserControls
{
    public sealed partial class CustomButton : UserControl
    {
        public string Glyph
        {
            get => (string)GetValue(glyph);
            set => SetValue(glyph, value);
        }
        public static readonly DependencyProperty glyph =
            DependencyProperty.Register("Glyph", typeof(string), typeof(UserControl), new PropertyMetadata(string.Empty));

        public string Text
        {
            get => (string)GetValue(text);
            set => SetValue(text, value);
        }
        public static readonly DependencyProperty text =
            DependencyProperty.Register("Text", typeof(string), typeof(UserControl), new PropertyMetadata(string.Empty));

        public event RoutedEventHandler Click
        {
            add
            {
                MainButton.Click += value;
            }
            remove
            {
                MainButton.Click += value;
            }
        }

        public CustomButton()
        {
            this.InitializeComponent();
        }
    }
}
