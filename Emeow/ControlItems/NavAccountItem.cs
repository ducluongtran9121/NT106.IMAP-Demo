using System;

namespace Emeow.ControlItems
{
    public class NavAccountItem : INavigationControlItem
    {
        public string Content { get; set; }

        public string Address { get; set; }

        public string Glyph { get; set; }

        public string Tag { get; set; }

        public bool SelectOnInvoked { get; set; }

        public NavigationControlItemType ItemType { get; set; }

        public int CompareTo(INavigationControlItem other) => Content.CompareTo(other.Content);

        public NavAccountItem()
        {
            Content = string.Empty;
            Address = string.Empty;
            Glyph = string.Empty;
            Tag = string.Empty;
            SelectOnInvoked = true;
            ItemType = NavigationControlItemType.NavAccount;
        }
    }
}
