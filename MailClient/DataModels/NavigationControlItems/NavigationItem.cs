using MailClient.Common;

namespace MailClient.DataModels.NavigationControlItems
{
    public class NavigationItem : BindableBase, INavigationControlItem
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

        private string tag = string.Empty;

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

        public NavigationItem()
        {
            Text = string.Empty;
            Glyph = string.Empty;
            Tag = string.Empty;
            SelectOnInvoked = true;
            ItemType = NavigationControlItemType.NavItem;
        }

        public NavigationItem(string text, string glyph, string tag, bool selectOnInvoked)
        {
            Text = text;
            Glyph = glyph;
            Tag = tag;
            SelectOnInvoked = selectOnInvoked;
            ItemType = NavigationControlItemType.NavItem;
        }
    }
}