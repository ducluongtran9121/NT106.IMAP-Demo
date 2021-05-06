using System;
using System.Collections.ObjectModel;

namespace Emeow.ControlItems
{
    public class NavListItem : INavigationControlItem
    {
        public string Content { get; set; }

        public string Glyph { get; set; }

        public string Tag { get; set; }

        public bool SelectOnInvoked { get; set; }

        public NavigationControlItemType ItemType { get; set; }

        public ObservableCollection<INavigationControlItem> Child { get; set; }

        public int CompareTo(INavigationControlItem other) => Content.CompareTo(other.Content);

        public NavListItem()
        {
            Content = string.Empty;
            Glyph = string.Empty;
            Tag = string.Empty;
            SelectOnInvoked = false;
            ItemType = NavigationControlItemType.NavList;
            Child = null;
        }
    }
}
