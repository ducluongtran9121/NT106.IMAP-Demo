using System;
using System.ComponentModel;

namespace Emeow.ControlItems
{
    public interface INavigationControlItem : IComparable<INavigationControlItem>
    {
        public string Content { get; set; }

        public string Glyph { get; set; }

        public string Tag { get; set; }

        public bool SelectOnInvoked { get; set; }

        public NavigationControlItemType ItemType { get; set; }
    }
    public enum NavigationControlItemType
    {
        NavItem,
        NavList,
        NavSeparator,
        NavAccount
    }
}
