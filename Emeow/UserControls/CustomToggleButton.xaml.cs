using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Emeow.UserControls
{
    public sealed partial class CustomToggleButton : UserControl
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

        public bool IsChecked
        {
            get => (bool)GetValue(isChecked);
            set => SetValue(isChecked, value);
        }
        public static readonly DependencyProperty isChecked =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(UserControl), new PropertyMetadata(false));

        public event RoutedEventHandler Click
        {
            add
            {
                MainToggleButton.Click += value;
            }
            remove
            {
                MainToggleButton.Click -= value;
            }
        }

        public CustomToggleButton()
        {
            this.InitializeComponent();
        }
    }
}
