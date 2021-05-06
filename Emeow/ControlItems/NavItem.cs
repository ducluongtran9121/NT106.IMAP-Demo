using System;

namespace Emeow.ControlItems
{
    public class NavItem : INavigationControlItem
    {
        public string Content { get; set; }

        public string Glyph { get; set; }

        public string Tag { get; set; }

        public bool SelectOnInvoked { get; set; }

        public NavigationControlItemType ItemType { get; set; }

        public int CompareTo(INavigationControlItem other) => Content.CompareTo(other.Content);

        public NavItem()
        {
            Content = string.Empty;
            Glyph = string.Empty;
            Tag = string.Empty;
            SelectOnInvoked = true;
            ItemType = NavigationControlItemType.NavItem;
        }
    }
}
