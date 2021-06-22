using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MailClient.UserControls.BasicControls
{
    public sealed partial class ButtonWithTextAndGlyph : Button
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

        public ButtonWithTextAndGlyph()
        {
            this.InitializeComponent();
        }

        private void Button_Loaded(object sender, RoutedEventArgs e)
        {
            SpaceSeparator.Width = (string.IsNullOrEmpty(Glyph) || string.IsNullOrEmpty(Text)) ? 0 : 10;
        }
    }
}