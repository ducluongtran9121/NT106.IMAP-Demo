using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.ApplicationModel.Core;
using Windows.UI.Text;

namespace Emeow.Pages
{
    public sealed partial class NewMailPage : Page , INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _titleBarHeight;
        public string TitleBarHeight
        {
            get { return _titleBarHeight; }
            set
            {
                _titleBarHeight = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TitleBarHeight"));
            }
        }

        private bool _isEdittingRichEditBoxText;
        public bool IsEdittingRichEditBoxText
        {
            get { return _isEdittingRichEditBoxText; }
            set
            {
                _isEdittingRichEditBoxText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEdittingRichEditBoxText"));
            }
        }

        private bool _canUndo;
        public bool CanUndo
        {
            get { return _canUndo; }
            set
            {
                _canUndo = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CanUndo"));
            }
        }

        private bool _canRedo;
        public bool CanRedo
        {
            get { return _canRedo; }
            set
            {
                _canRedo = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CanRedo"));
            }
        }

        public NewMailPage()
        {
            this.InitializeComponent();
            TitleBarHeight = (CoreApplication.GetCurrentView().TitleBar.Height + 10).ToString();
            IsEdittingRichEditBoxText = false;
        }

        private void BoldButton_Click(object sender, RoutedEventArgs e)
        {
            ITextSelection selectedText = Editor.Document.Selection;
            if (selectedText != null)
            {
                ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
                charFormatting.Bold = FormatEffect.Toggle;
                selectedText.CharacterFormat = charFormatting;
            }
            Editor.Focus(FocusState.Programmatic);
        }

        private void ItalicButton_Click(object sender, RoutedEventArgs e)
        {
            ITextSelection selectedText = Editor.Document.Selection;
            if (selectedText != null)
            {
                ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
                charFormatting.Italic = FormatEffect.Toggle;
                selectedText.CharacterFormat = charFormatting;
            }
            Editor.Focus(FocusState.Programmatic);
        }

        private void UnderlineButton_Click(object sender, RoutedEventArgs e)
        {
            ITextSelection selectedText = Editor.Document.Selection;
            if (selectedText != null)
            {
                Windows.UI.Text.ITextCharacterFormat charFormatting = selectedText.CharacterFormat;
                if (charFormatting.Underline == UnderlineType.None)
                {
                    charFormatting.Underline = UnderlineType.Single;
                }
                else
                {
                    charFormatting.Underline = UnderlineType.None;
                }
                selectedText.CharacterFormat = charFormatting;
            }
            Editor.Focus(FocusState.Programmatic);
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            if (Editor.Document.CanUndo())
            {
                Editor.Document.Undo();
                Editor.Focus(FocusState.Programmatic);
            }
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            if (Editor.Document.CanRedo())
            {
                Editor.Document.Redo();
                Editor.Focus(FocusState.Programmatic);
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            IsEdittingRichEditBoxText = false;
            CanUndo = false;
            CanRedo = false;
        }

        private void Editor_GotFocus(object sender, RoutedEventArgs e)
        {
            IsEdittingRichEditBoxText = true;
            CanUndo = Editor.TextDocument.CanUndo();
            CanRedo = Editor.TextDocument.CanRedo();
        }

        private void Editor_TextChanged(object sender, RoutedEventArgs e)
        {
            CanUndo = Editor.TextDocument.CanUndo();
            CanRedo = Editor.TextDocument.CanRedo();
        }

        private void Editor_SelectionChanging(RichEditBox sender, RichEditBoxSelectionChangingEventArgs args)
        {
            ITextSelection selection = Editor.Document.Selection;
            if (selection != null)
            {
                ITextCharacterFormat textCharacterFormat = selection.CharacterFormat;
                if (textCharacterFormat.Bold == FormatEffect.On)
                {
                    BoldButton.IsChecked = true;
                    BoldButton.Background = (SolidColorBrush)App.Current.Resources["ButtonBackgroundPointerOver"];
                }
                else
                {
                    BoldButton.IsChecked = false;
                    BoldButton.Background = (SolidColorBrush)App.Current.Resources["ControlColorLowBrush"];
                }

                if (textCharacterFormat.Italic == FormatEffect.On)
                {
                    ItalicButton.IsChecked = true;
                    ItalicButton.Background = (SolidColorBrush)App.Current.Resources["ButtonBackgroundPointerOver"];
                }
                else
                {
                    ItalicButton.IsChecked = false;
                    ItalicButton.Background = (SolidColorBrush)App.Current.Resources["ControlColorLowBrush"];
                }

                if (textCharacterFormat.Underline == UnderlineType.Single)
                {
                    UnderlineButton.IsChecked = true;
                    UnderlineButton.Background = (SolidColorBrush)App.Current.Resources["ButtonBackgroundPointerOver"];
                }
                else
                {
                    UnderlineButton.IsChecked = false;
                    UnderlineButton.Background = (SolidColorBrush)App.Current.Resources["ControlColorLowBrush"];
                }
            }
        }
    }
}
