using MailClient.Common;
using System;


namespace MailClient.DataModels.NavigationControlItems
{
    public class NavigationItem: BindableBase, INavigationControlItem
    {
        private string content = string.Empty;
        public string Content
        {
            get => content;
            set => SetProperty(ref content, value);
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

        public int CompareTo(INavigationControlItem other) => Content.CompareTo(other.Tag);

        public NavigationItem()
        {
            Content = string.Empty;
            Glyph = string.Empty;
            Tag = string.Empty;
            SelectOnInvoked = true;
            ItemType = NavigationControlItemType.NavItem;
        }

        public NavigationItem(string content, string glyph, string tag, bool selectOnInvoked)
        {
            Content = content;
            Glyph = glyph;
            Tag = tag;
            SelectOnInvoked = selectOnInvoked;
            ItemType = NavigationControlItemType.NavItem;
        }
    }
}
