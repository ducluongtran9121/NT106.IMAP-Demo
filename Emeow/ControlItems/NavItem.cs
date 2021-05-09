﻿using Emeow.Common;
using System;

namespace Emeow.ControlItems
{
    public class NavItem : BindableBase, INavigationControlItem
    {
        private string content;

        private string glyph;

        private string tag;

        private bool selectOnInvoked;

        private NavigationControlItemType itemType;

        public string Content
        {
            get { return this.content; }
            set { this.SetProperty(ref this.content, value); }
        }

        public string Glyph 
        {
            get { return this.glyph; }
            set { this.SetProperty(ref this.glyph, value); }
        }

        public string Tag
        {
            get { return this.tag; }
            set { this.SetProperty(ref this.tag, value); }
        }

        public bool SelectOnInvoked 
        {
            get { return this.selectOnInvoked; }
            set { this.SetProperty(ref this.selectOnInvoked, value); }
        }

        public NavigationControlItemType ItemType 
        {
            get { return this.itemType; }
            set { this.SetProperty(ref this.itemType, value); }
        }

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
