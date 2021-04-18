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
