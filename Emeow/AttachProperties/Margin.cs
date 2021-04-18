using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Emeow.AttachProperties
{
    class Margin
    {
        public static readonly DependencyProperty MarginLeftProperty = DependencyProperty.Register(
            "MarginLeft",
            typeof(string),
            typeof(Margin),
            new PropertyMetadata(false, OnMarginLeftPropertyChanged)
        );

        public static string GetMarginLeft(FrameworkElement element)
        {
            return (string)element.GetValue(MarginLeftProperty);
        }

        public static void SetMarginLeft(FrameworkElement element, string value)
        {
            element.SetValue(MarginLeftProperty, value);
        }

        private static void OnMarginLeftPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is FrameworkElement element)
            {
                if (int.TryParse((string)args.NewValue, out int value))
                {
                    var margin = element.Margin;
                    margin.Left = value;
                    element.Margin = margin;
                }
            }
        }
    }
}
