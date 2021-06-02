using System;

namespace MailClient.DataModels.NavigationControlItems
{
    public class NavigationEmptyItem : INavigationControlItem
    {
        public string Content { get; set; }

        public string Glyph { get; set; }

        public string Tag { get; set; }

        public bool SelectOnInvoked { get; set; }

        public NavigationControlItemType ItemType { get; set; }

        public int CompareTo(INavigationControlItem other) => Content.CompareTo(other.Tag);

        public NavigationEmptyItem()
        {
            Content = string.Empty;
            Glyph = string.Empty;
            Tag = string.Empty;
            SelectOnInvoked = false;
            ItemType = NavigationControlItemType.NavEmpty;
        }
    }
}
