using Emeow.ControlItems;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emeow.User
{
    class AccountItem : INavigationControlItem
    {
        public string Content { get; set; }

        public string EmailAddr { get; set; }

        public string Glyph { get; set; }

        public string Tag { get; set; }

        public NavigationControlItemType ItemType { get; set; }

        public ObservableCollection<INavigationControlItem> Child { get; set; }

        public int CompareTo(INavigationControlItem other) => Content.CompareTo(other.Content);
    }
}
