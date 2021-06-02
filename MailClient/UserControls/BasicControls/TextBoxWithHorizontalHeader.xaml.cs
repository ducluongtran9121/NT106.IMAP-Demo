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

namespace MailClient.UserControls.BasicControls
{
    public sealed partial class TextBoxWithHorizontalHeader : UserControl
    {
        public string HorizontalHeader
        {
            get => (string)GetValue(horizontalHeader);
            set => SetValue(horizontalHeader, value);
        }
        public static readonly DependencyProperty horizontalHeader =
            DependencyProperty.Register("HorizontalHeader", typeof(string), typeof(UserControl), new PropertyMetadata(string.Empty));

        public string PlaceHolderText
        {
            get => (string)GetValue(placeHolderText);
            set => SetValue(placeHolderText, value);
        }
        public static readonly DependencyProperty placeHolderText =
            DependencyProperty.Register("PlaceHolderText", typeof(string), typeof(UserControl), new PropertyMetadata(string.Empty));

        public TextBoxWithHorizontalHeader()
        {
            this.InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SpaceSeparator.Width = string.IsNullOrEmpty(HorizontalHeader) ? 0 : 10;
        }
    }
}
