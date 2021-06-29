using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace MailClient.Converters
{
    public class BoolToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value as bool?) ?? false ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ((value as Visibility?) ?? Visibility.Visible) == Visibility.Collapsed;
        }

    }

    public class BoolToVisibilityy : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value as bool?) ?? false ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ((value as Visibility?) ?? Visibility.Collapsed) == Visibility.Visible;
        }

    }
}
