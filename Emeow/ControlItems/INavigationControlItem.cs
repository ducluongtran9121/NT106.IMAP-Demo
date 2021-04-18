using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emeow.ControlItems
{
    public interface INavigationControlItem : IComparable<INavigationControlItem>
    {
        public string Glyph { get; set; }

        public string Content { get; set; }

        public string Tag { get; set; }

        public NavigationControlItemType ItemType { get; set; }
    }
    public enum NavigationControlItemType
    {
        NewMail,
        Account,
        SpaceSeparator,
    }
}
