using System;

namespace MailClient.DataModels.NavigationControlItems
{
    public interface INavigationControlItem : IComparable<INavigationControlItem>
    {
        public string Text { get; set; }

        public string Glyph { get; set; }

        public string Tag { get; set; }

        public bool SelectOnInvoked { get; set; }

        public NavigationControlItemType ItemType { get; set; }
    }

    public enum NavigationControlItemType
    {
        NavItem,
        NavList,
        NavEmpty,
        NavAccount
    }
}