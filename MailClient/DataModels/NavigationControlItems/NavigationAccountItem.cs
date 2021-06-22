using MailClient.Common;

namespace MailClient.DataModels.NavigationControlItems
{
    public class NavigationAccountItem : BindableBase, INavigationControlItem
    {
        private string text = string.Empty;

        public string Text
        {
            get => text;
            set => SetProperty(ref text, value);
        }

        private string glyph = string.Empty;

        public string Glyph
        {
            get => glyph;
            set => SetProperty(ref glyph, value);
        }

        private string address;

        public string Address
        {
            get => address;
            set => SetProperty(ref address, value);
        }

        private string tag;

        public string Tag
        {
            get => tag;
            set => SetProperty(ref tag, value);
        }

        private bool selectOnInvoked;

        public bool SelectOnInvoked
        {
            get => selectOnInvoked;
            set => SetProperty(ref selectOnInvoked, value);
        }

        private NavigationControlItemType itemType;

        public NavigationControlItemType ItemType
        {
            get => itemType;
            set => SetProperty(ref itemType, value);
        }

        public int CompareTo(INavigationControlItem other) => Text.CompareTo(other.Tag);

        public NavigationAccountItem()
        {
            Text = string.Empty;
            Address = string.Empty;
            Glyph = string.Empty;
            Tag = string.Empty;
            SelectOnInvoked = true;
            ItemType = NavigationControlItemType.NavAccount;
        }

        public NavigationAccountItem(string text, string address, string glyph, string tag, bool selectOnInvoked)
        {
            Text = text;
            Address = address;
            Glyph = glyph;
            Tag = tag;
            SelectOnInvoked = selectOnInvoked;
            ItemType = NavigationControlItemType.NavAccount;
        }
    }
}