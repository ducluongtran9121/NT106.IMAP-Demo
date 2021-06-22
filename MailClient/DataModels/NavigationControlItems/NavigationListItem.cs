using MailClient.Common;
using System.Collections.ObjectModel;

namespace MailClient.DataModels.NavigationControlItems
{
    public class NavigationListItem : BindableBase, INavigationControlItem
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

        public ObservableCollection<INavigationControlItem> Child;

        private NavigationControlItemType itemType;

        public NavigationControlItemType ItemType
        {
            get => itemType;
            set => SetProperty(ref itemType, value);
        }

        public int CompareTo(INavigationControlItem other) => Text.CompareTo(other.Tag);

        public NavigationListItem()
        {
            Text = string.Empty;
            Glyph = string.Empty;
            Tag = string.Empty;
            SelectOnInvoked = false;
            ItemType = NavigationControlItemType.NavList;
        }

        public NavigationListItem(string content, string glyph, string tag, bool selectOnInvoked, ObservableCollection<INavigationControlItem> child)
        {
            Text = content;
            Glyph = glyph;
            Tag = tag;
            SelectOnInvoked = selectOnInvoked;
            Child = child;
            ItemType = NavigationControlItemType.NavList;
        }
    }
}