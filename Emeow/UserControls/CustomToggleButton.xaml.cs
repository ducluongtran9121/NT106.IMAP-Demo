using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

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
