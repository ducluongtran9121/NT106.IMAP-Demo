using System;

namespace MailClient.DataModels.NavigationControlItems
{
    public class NavigationEmptyItem : INavigationControlItem
    {
        public string Text { get; set; }

        public string Glyph { get; set; }

        public string Tag { get; set; }

        public bool SelectOnInvoked { get; set; }

        public NavigationControlItemType ItemType { get; set; }

        public int CompareTo(INavigationControlItem other) => Text.CompareTo(other.Tag);

        public NavigationEmptyItem()
        {
            Text = string.Empty;
            Glyph = string.Empty;
            Tag = string.Empty;
            SelectOnInvoked = false;
            ItemType = NavigationControlItemType.NavEmpty;
        }
    }
}