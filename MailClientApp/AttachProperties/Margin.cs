using System;
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


        public static readonly DependencyProperty MarginTopProperty = DependencyProperty.Register(
            "MarginTop",
            typeof(string),
            typeof(Margin),
            new PropertyMetadata(false, OnMarginTopPropertyChanged)
        );

        public static string GetMarginTop(FrameworkElement element)
        {
            return (string)element.GetValue(MarginTopProperty);
        }

        public static void SetMarginTop(FrameworkElement element, string value)
        {
            element.SetValue(MarginTopProperty, value);
        }

        private static void OnMarginTopPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is FrameworkElement element)
            {
                if (int.TryParse((string)args.NewValue, out int value))
                {
                    var margin = element.Margin;
                    margin.Top = value;
                    element.Margin = margin;
                }
            }
        }
    }
}
