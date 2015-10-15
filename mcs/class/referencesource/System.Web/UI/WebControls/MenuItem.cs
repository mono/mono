//------------------------------------------------------------------------------
// <copyright file="MenuItem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web.UI;
    using System.Web.Util;


    /// <devdoc>
    ///     Provides a hierarchical menu item for use in the Menu class
    /// </devdoc>
    [ParseChildren(true, "ChildItems")]
    public sealed class MenuItem : IStateManager, ICloneable {
        
        private static readonly Unit HorizontalDefaultSpacing = Unit.Pixel(3);

        private bool _isTrackingViewState;
        private StateBag _viewState;

        private MenuItemCollection _childItems;
        private Menu _owner;
        private MenuItem _parent;

        private int _selectDesired;

        private object _dataItem;
        private MenuItemTemplateContainer _container;

        private int _index;
        internal string _id = string.Empty;

        private string _valuePath;
        private string _internalValuePath;
        private int _depth = -2;

        private bool _isRoot;


        /// <devdoc>
        ///     Constructs a new MenuItem without a text or value
        /// </devdoc>
        public MenuItem() {
            _selectDesired = 0;
        }


        /// <devdoc>
        ///     Constructs a new MenuItem with the specified owner Menu
        /// </devdoc>
        internal MenuItem(Menu owner, bool isRoot)
            : this() {
            _owner = owner;
            _isRoot = isRoot;
        }


        /// <devdoc>
        ///     Constructs a new MenuItem with the specified text
        /// </devdoc>
        public MenuItem(string text)
            : this(text, null, null, null, null) {
        }


        /// <devdoc>
        ///     Constructs a new MenuItem with the specified text, and value
        /// </devdoc>
        public MenuItem(string text, string value)
            : this(text, value, null, null, null) {
        }


        /// <devdoc>
        ///     Constructs a new MenuItem with the specified text, value, and image URL
        /// </devdoc>
        public MenuItem(string text, string value, string imageUrl)
            : this(text, value, imageUrl, null, null) {
        }


        /// <devdoc>
        ///     Constructs a new MenuItem with the specified text, value, image URL and navigateUrl
        /// </devdoc>
        public MenuItem(string text, string value, string imageUrl, string navigateUrl)
            : this(text, value, imageUrl, navigateUrl, null) {
        }


        /// <devdoc>
        ///     Constructs a new MenuItem with the specified text, value, image URL, navigation URL, and target.
        /// </devdoc>
        public MenuItem(string text, string value, string imageUrl, string navigateUrl, string target)
            : this() {
            if (text != null) {
                Text = text;
            }

            if (value != null) {
                Value = value;
            }

            if (!String.IsNullOrEmpty(imageUrl)) {
                ImageUrl = imageUrl;
            }

            if (!String.IsNullOrEmpty(navigateUrl)) {
                NavigateUrl = navigateUrl;
            }

            if (!String.IsNullOrEmpty(target)) {
                Target = target;
            }
        }


        /// <devdoc>
        ///     Gets the collection of children items parented to this MenuItem
        /// </devdoc>
        [Browsable(false)]
        [MergableProperty(false)]
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)]
        public MenuItemCollection ChildItems {
            get {
                if (_childItems == null) {
                    _childItems = new MenuItemCollection(this);
                }
                return _childItems;
            }
        }

        internal MenuItemTemplateContainer Container {
            get {
                return _container;
            }
            set {
                _container = value;
            }
        }


        /// <devdoc>
        ///     Gets whether this item was created through databinding
        /// </devdoc>
        [Browsable(false)]
        [DefaultValue(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DataBound {
            get {
                object o = ViewState["DataBound"];
                if (o == null) {
                    return false;
                }
                return (bool)o;
            }
        }


        /// <devdoc>
        ///     Gets path to the data to which this item is bound.
        /// </devdoc>
        [Browsable(false)]
        [DefaultValue("")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DataPath {
            get {
                object s = ViewState["DataPath"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
        }


        /// <devdoc>
        ///     Gets the depth of the menu item.
        /// </devdoc>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Depth {
            get {
                // -2 means not set yet, -1 means root
                if (_depth == -2) {
                    if (_isRoot) {
                        return -1;
                    }

                    if (Parent != null) {
                        _depth = Parent.Depth + 1;
                    }
                    else {
                        return 0;
                    }
                }
                return _depth;
            }
        }


        /// <devdoc>
        ///     Gets the data item for the menu item.
        /// </devdoc>
        [Browsable(false)]
        [DefaultValue(null)]
        public object DataItem {
            get {
                return _dataItem;
            }
        }

        [Browsable(true)]
        [DefaultValue(true)]
        [WebSysDescription(SR.MenuItem_Enabled)]
        public bool Enabled {
            get {
                object o = ViewState["Enabled"];
                return (o == null ? true : (bool)o);
            }
            set {
                ViewState["Enabled"] = value;
            }
        }

        internal string FormattedText {
            get {
                if (_owner.StaticItemFormatString.Length > 0 && Depth < _owner.StaticDisplayLevels) {
                    return String.Format(CultureInfo.CurrentCulture, _owner.StaticItemFormatString, Text);
                }
                else if (_owner.DynamicItemFormatString.Length > 0 && Depth >= _owner.StaticDisplayLevels) {
                    return String.Format(CultureInfo.CurrentCulture, _owner.DynamicItemFormatString, Text);
                }
                else {
                    return Text;
                }
            }
        }

        internal string Id {
            get {
                if (_id.Length == 0) {
                    Index = _owner.CreateItemIndex();
                    _id = _owner.ClientID + 'n' + Index;
                }
                return _id;
            }
        }


        /// <devdoc>
        ///     Gets and sets the image URl to be rendered for this item
        /// </devdoc>
        [DefaultValue("")]
        [Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [UrlProperty()]
        [WebSysDescription(SR.MenuItem_ImageUrl)]
        public string ImageUrl {
            get {
                object s = ViewState["ImageUrl"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["ImageUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the unique index for the menu item
        /// </devdoc>
        internal int Index {
            get {
                return _index;
            }
            set {
                _index = value;
            }
        }

        internal string InternalValuePath {
            get {
                if (_internalValuePath != null) {
                    return _internalValuePath;
                }
                if (_parent != null) {
                    // StringBuilder.Insert is expensive, but we need to build starting from the end.
                    // First build a list, then build the string starting from the end of the list.
                    List<string> pathParts = new List<string>();
                    pathParts.Add(TreeView.Escape(Value));
                    MenuItem parent = _parent;
                    while ((parent != null) && !parent._isRoot) {
                        if (parent._internalValuePath != null) {
                            pathParts.Add(parent._internalValuePath);
                            break;
                        }
                        else {
                            pathParts.Add(TreeView.Escape(parent.Value));
                        }
                        parent = parent._parent;
                    }
                    pathParts.Reverse();
                    _internalValuePath = String.Join(TreeView.InternalPathSeparator.ToString(), pathParts.ToArray());
                    return _internalValuePath;
                }
                else {
                    return String.Empty;
                }
            }
        }

        internal bool IsEnabled {
            get {
                return IsEnabledNoOwner && Owner.IsEnabled;
            }
        }

        internal bool IsEnabledNoOwner {
            get {
                MenuItem current = this;
                while (current != null) {
                    if (!current.Enabled) {
                        return false;
                    }

                    current = current.Parent;
                }

                return true;
            }
        }


        /// <devdoc>
        ///     Gets and sets the URL to navigate to when the item is clicked
        /// </devdoc>
        [DefaultValue("")]
        [Editor("System.Web.UI.Design.UrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [UrlProperty()]
        [WebSysDescription(SR.MenuItem_NavigateUrl)]
        public string NavigateUrl {
            get {
                object s = ViewState["NavigateUrl"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["NavigateUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets the owner Menu for this MenuItem, if there is one
        /// </devdoc>
        internal Menu Owner {
            get {
                return _owner;
            }
        }


        /// <devdoc>
        ///     Gets the parent MenuItem
        /// </devdoc>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MenuItem Parent {
            get {
                if ((_parent == null) || _parent._isRoot) {
                    return null;
                }

                return _parent;
            }

        }


        /// <devdoc>
        ///     Gets and sets the image URl to be rendered as a pop-out icon for this item if it has children
        /// </devdoc>
        [DefaultValue("")]
        [Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [UrlProperty()]
        [WebSysDescription(SR.MenuItem_PopOutImageUrl)]
        public string PopOutImageUrl {
            get {
                object s = ViewState["PopOutImageUrl"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["PopOutImageUrl"] = value;
            }
        }

        [Browsable(true)]
        [DefaultValue(true)]
        [WebSysDescription(SR.MenuItem_Selectable)]
        public bool Selectable {
            get {
                object o = ViewState["Selectable"];
                return (o == null ? true : (bool)o);
            }
            set {
                ViewState["Selectable"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the selected state
        /// </devdoc>
        [Browsable(true)]
        [DefaultValue(false)]
        [WebSysDescription(SR.MenuItem_Selected)]
        public bool Selected {
            get {
                object o = ViewState["Selected"];
                if (o == null) {
                    return false;
                }
                return (bool)o;
            }
            set {
                SetSelected(value);
                NotifyOwnerSelected();
            }
        }


        /// <devdoc>
        ///     Gets and sets the image URl to be rendered as a separator for this item
        /// </devdoc>
        [DefaultValue("")]
        [Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [UrlProperty()]
        [WebSysDescription(SR.MenuItem_SeparatorImageUrl)]
        public string SeparatorImageUrl {
            get {
                object s = ViewState["SeparatorImageUrl"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["SeparatorImageUrl"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the target window that the MenuItem will browse to if selected
        /// </devdoc>
        [DefaultValue("")]
        [WebSysDescription(SR.MenuItem_Target)]
        public string Target {
            get {
                object s = ViewState["Target"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["Target"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the display text
        /// </devdoc>
        [DefaultValue("")]
        [Localizable(true)]
        [WebSysDescription(SR.MenuItem_Text)]
        public string Text {
            get {
                object s = ViewState["Text"];
                if (s == null) {
                    s = ViewState["Value"];
                    if (s == null) {
                        return String.Empty;
                    }
                }
                return (string)s;
            }
            set {
                ViewState["Text"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the MenuItem tooltip
        /// </devdoc>
        [DefaultValue("")]
        [Localizable(true)]
        [WebSysDescription(SR.MenuItem_ToolTip)]
        public string ToolTip {
            get {
                object s = ViewState["ToolTip"];
                if (s == null) {
                    return String.Empty;
                }
                return (string)s;
            }
            set {
                ViewState["ToolTip"] = value;
            }
        }


        /// <devdoc>
        ///     Gets and sets the value
        /// </devdoc>
        [DefaultValue("")]
        [Localizable(true)]
        [WebSysDescription(SR.MenuItem_Value)]
        public string Value {
            get {
                object s = ViewState["Value"];
                if (s == null) {
                    s = ViewState["Text"];
                    if (s == null) {
                        return String.Empty;
                    }
                }
                return (string)s;
            }
            set {
                ViewState["Value"] = value;
                ResetValuePathRecursive();
            }
        }


        /// <devdoc>
        ///     Gets the full path of the MenuItem
        /// </devdoc>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ValuePath {
            get {
                if (_valuePath != null) {
                    return _valuePath;
                }

                if (_parent != null) {
                    string parentPath = _parent.ValuePath;
                    _valuePath = ((parentPath.Length == 0) && (_parent.Depth == -1)) ?
                        Value : parentPath + _owner.PathSeparator + Value;
                    return _valuePath;
                }
                else {
                    return String.Empty;
                }
            }
        }


        /// <devdoc>
        ///     The state for this MenuItem
        /// </devdoc>
        private StateBag ViewState {
            get {
                if (_viewState == null) {
                    _viewState = new StateBag();
                    if (_isTrackingViewState) {
                        ((IStateManager)_viewState).TrackViewState();
                    }
                }
                return _viewState;
            }
        }

        internal string GetExpandImageUrl() {
            if (ChildItems.Count > 0) {
                if (PopOutImageUrl.Length != 0) {
                    return _owner.ResolveClientUrl(PopOutImageUrl);
                }
                else {
                    if (Depth < _owner.StaticDisplayLevels) {
                        if (_owner.StaticPopOutImageUrl.Length != 0) {
                            return _owner.ResolveClientUrl(_owner.StaticPopOutImageUrl);
                        }
                        else if (_owner.StaticEnableDefaultPopOutImage) {
                            return _owner.GetImageUrl(Menu.PopOutImageIndex);
                        }
                    }
                    else {
                        if (_owner.DynamicPopOutImageUrl.Length != 0) {
                            return _owner.ResolveClientUrl(_owner.DynamicPopOutImageUrl);
                        }
                        else if (_owner.DynamicEnableDefaultPopOutImage) {
                            return _owner.GetImageUrl(Menu.PopOutImageIndex);
                        }
                    }
                }
            }
            return String.Empty;
        }

        internal bool NotTemplated() {
            return ((_owner.StaticItemTemplate == null || Depth >= _owner.StaticDisplayLevels) &&
                    (_owner.DynamicItemTemplate == null || Depth < _owner.StaticDisplayLevels));
        }

        private void NotifyOwnerSelected() {
            object o = ViewState["Selected"];
            bool value = (o == null ? false : (bool)o);
            // If the owner hasn't been set, remember that we want to select this item
            // when the owner is determined
            if (_owner == null) {
                _selectDesired = (value ? +1 : -1);
                return;
            }
            else if (value) {
                // Set the Menu's selected item to this one
                _owner.SetSelectedItem(this);
            }
            else if (this == _owner.SelectedItem) {
                _owner.SetSelectedItem(null);
            }
        }


        /// <devdoc>
        ///     Renders the contents of the item and its children.
        /// </devdoc>
        internal void Render(HtmlTextWriter writer, bool enabled, bool staticOnly) {
            Render(writer, enabled, staticOnly, true);
        }


        /// <devdoc>
        ///     Renders the contents of the item and its children.
        /// </devdoc>
        internal void Render(HtmlTextWriter writer, bool enabled, bool staticOnly, bool recursive) {
            enabled = enabled && Enabled;
            // If children exist, maybe render them
            int nextDepth = Depth + 1;
            if (ChildItems.Count > 0 && nextDepth < _owner.MaximumDepth) {
                // <table cellpadding="0" cellspacing="0" border="0">
                // Find the right style:
                SubMenuStyle subMenuStyle = _owner.GetSubMenuStyle(this);
                string styleClass = null;
                if (_owner.Page != null && _owner.Page.SupportsStyleSheets) {
                    styleClass = _owner.GetSubMenuCssClassName(this);
                }

                if (nextDepth >= _owner.StaticDisplayLevels) {
                    // The submenu is dynamic
                    if (!staticOnly && enabled && !(_owner.DesignMode && recursive)) {
                        // Not recreating a panel each time: panel is created and configured only once from Menu.Panel.
                        PopOutPanel panel = _owner.Panel;
                        if (_owner.Page != null && _owner.Page.SupportsStyleSheets) {
                            panel.ScrollerClass = _owner.GetCssClassName(ChildItems[0], false);
                            panel.ScrollerStyle = null;
                        }
                        else {
                            panel.ScrollerClass = null;
                            panel.ScrollerStyle = _owner.GetMenuItemStyle(ChildItems[0]);
                        }
                        if (_owner.Page != null && _owner.Page.SupportsStyleSheets) {
                            panel.CssClass = styleClass;
                            panel.SetInternalStyle(null);
                        }
                        else if (!subMenuStyle.IsEmpty) {
                            panel.CssClass = String.Empty;
                            panel.SetInternalStyle(subMenuStyle);
                        }
                        else {
                            panel.CssClass = String.Empty;
                            panel.SetInternalStyle(null);
                            panel.BackColor = Color.Empty;
                        }
                        panel.ID = Id + "Items";
                        panel.RenderBeginTag(writer);
                        writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                        writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
                        writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                        writer.RenderBeginTag(HtmlTextWriterTag.Table);
                        for (int i = 0; i < ChildItems.Count; i++) {
                            ChildItems[i].RenderItem(writer, i, enabled, Orientation.Vertical);
                        }
                        writer.RenderEndTag(); // Table
                        panel.RenderEndTag(writer);

                        if (recursive) {
                            for (int i = 0; i < ChildItems.Count; i++) {
                                ChildItems[i].Render(writer, enabled, false);
                            }
                        }
                    }
                }
                else {
                    // The submenu is static
                    writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                    writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
                    writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
                    writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                    if (_owner.Page != null && _owner.Page.SupportsStyleSheets) {
                        if (styleClass != null && styleClass.Length > 0) {
                            writer.AddAttribute(HtmlTextWriterAttribute.Class, styleClass);
                        }
                    }
                    else {
                        subMenuStyle.AddAttributesToRender(writer);
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Table);
                    if (_owner.Orientation == Orientation.Horizontal) {
                        // Render one global tr as items won't render any
                        writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    }
                    bool isNextStatic = (nextDepth + 1 < _owner.StaticDisplayLevels);
                    bool isNextInRange = (nextDepth + 1 < _owner.MaximumDepth);
                    for (int i = 0; i < ChildItems.Count; i++) {
                        if (recursive
                            && ChildItems[i].ChildItems.Count != 0
                            && ((enabled && ChildItems[i].Enabled) || isNextStatic)
                            && isNextInRange) {

                            // If the next items are dynamic, we want to render the div inside the item's
                            // td so that we don't generate a tr that contains only absolute positioned
                            // divs, which would cause a gap to appear (VSWhidbey 354884)
                            if (isNextStatic) {
                                ChildItems[i].RenderItem(writer, i, enabled, _owner.Orientation);
                                if (_owner.Orientation == Orientation.Vertical) {
                                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                                    ChildItems[i].Render(writer, enabled, staticOnly);
                                    writer.RenderEndTag(); //td
                                    writer.RenderEndTag(); //tr
                                }
                                else {
                                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                                    ChildItems[i].Render(writer, enabled, staticOnly);
                                    writer.RenderEndTag(); //td
                                }
                            }
                            else {
                                ChildItems[i].RenderItem(writer, i, enabled, _owner.Orientation, staticOnly);
                            }
                        }
                        else {
                            ChildItems[i].RenderItem(writer, i, enabled, _owner.Orientation);
                        }
                    }
                    if (_owner.Orientation == Orientation.Horizontal) {
                        // Render global /tr
                        writer.RenderEndTag();
                    }
                    writer.RenderEndTag(); //table
                    if (!isNextStatic && !staticOnly && recursive && isNextInRange) {
                        for (int i = 0; i < ChildItems.Count; i++) {
                            if (ChildItems[i].ChildItems.Count != 0
                                && ((enabled && ChildItems[i].Enabled))) {

                                // The next items are dynamic, so we want to render the div outside the menu table
                                // so that we don't generate a tr that contains only absolute positioned
                                // divs, which would cause a gap to appear (VSWhidbey 354884)
                                ChildItems[i].Render(writer, enabled, false, true);
                            }
                        }
                    }
                }
            }
        }


        /// <devdoc>
        ///     Renders the contents of the item but not its children.
        /// </devdoc>
        internal void RenderItem(HtmlTextWriter writer, int position, bool enabled, Orientation orientation) {
            RenderItem(writer, position, enabled, orientation, false);
        }

        internal void RenderItem(HtmlTextWriter writer, int position, bool enabled, Orientation orientation, bool staticOnly) {
            enabled = enabled && Enabled;
            int depth = Depth;
            MenuItemStyle mergedStyle = _owner.GetMenuItemStyle(this);

            int depthPlusOne = Depth + 1;

            bool staticTopSeparator = (depth < _owner.StaticDisplayLevels) && (_owner.StaticTopSeparatorImageUrl.Length != 0);
            bool dynamicTopSeparator = (depth >= _owner.StaticDisplayLevels) && (_owner.DynamicTopSeparatorImageUrl.Length != 0);
            // The separator is in a separate td in the vertical case, in a separate tr otherwise.
            if (staticTopSeparator || dynamicTopSeparator) {
                if (orientation == Orientation.Vertical) {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                if (staticTopSeparator) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, _owner.ResolveClientUrl(_owner.StaticTopSeparatorImageUrl));
                }
                else {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, _owner.ResolveClientUrl(_owner.DynamicTopSeparatorImageUrl));
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, String.Empty);
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag(); // Img
                writer.RenderEndTag(); // Td
                if (orientation == Orientation.Vertical) {
                    writer.RenderEndTag(); // Tr
                }
            }

            // Top spacing
            if ((mergedStyle != null) && !mergedStyle.ItemSpacing.IsEmpty && ((depth != 0) || (position != 0))) {
                RenderItemSpacing(writer, mergedStyle.ItemSpacing, orientation);
            }

            if (!staticOnly && _owner.Enabled) {
                if (depthPlusOne > _owner.StaticDisplayLevels) {
                    // Only the last static level and dynamic levels need hover behavior.
                    // And then only if they are selectable or have children.
                    if ((Selectable && Enabled) || ChildItems.Count != 0) {
                        writer.AddAttribute("onmouseover", "Menu_HoverDynamic(this)");
                        RenderItemEvents(writer);
                    }
                    else {
                        // dynamic disabled or unselectable items without children still need to maintain the menu open
                        writer.AddAttribute("onmouseover", "Menu_HoverDisabled(this)");
                        writer.AddAttribute("onmouseout", "Menu_Unhover(this)");
                    }
                }
                else if (depthPlusOne == _owner.StaticDisplayLevels) {
                    // Here's for the last static level
                    if ((Selectable && Enabled) || ChildItems.Count != 0) {
                        writer.AddAttribute("onmouseover", "Menu_HoverStatic(this)");
                        RenderItemEvents(writer);
                    }
                }
                else if (Selectable && Enabled) {
                    // Other nodes need hover styles but no expand
                    writer.AddAttribute("onmouseover", "Menu_HoverRoot(this)");
                    RenderItemEvents(writer);
                }
            }
            // Tooltip
            if (ToolTip.Length != 0) {
                writer.AddAttribute(HtmlTextWriterAttribute.Title, ToolTip);
            }
            // Set the id
            writer.AddAttribute(HtmlTextWriterAttribute.Id, Id);
            if (orientation == Orientation.Vertical) {
                // <tr>
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            // Set the style
            if (_owner.Page != null && _owner.Page.SupportsStyleSheets) {
                string styleClass = _owner.GetCssClassName(this, false);
                if (styleClass.Trim().Length > 0) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, styleClass);
                }
            }
            else if (mergedStyle != null) {
                mergedStyle.AddAttributesToRender(writer);
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            if (!_owner.ItemWrap) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
            }
            if (orientation == Orientation.Vertical) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "100%");
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            if (_owner.Page != null && _owner.Page.SupportsStyleSheets) {
                bool applyInlineBorder;
                string styleClass = _owner.GetCssClassName(this, true, out applyInlineBorder);
                if (styleClass.Trim().Length > 0) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, styleClass);
                    if (applyInlineBorder) {
                        // Add inline style to force the border to none to override any CssClass (VSWhidbey 336610)
                        writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "none");
                        // And an inline font-size of 1em to avoid squaring relative font sizes by applying them twice
                        writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "1em");
                    }
                }
            }
            else {
                if (mergedStyle != null) {
                    mergedStyle.HyperLinkStyle.AddAttributesToRender(writer);
                }
            }

            string accessKey = _owner.AccessKey;
            if (enabled && Selectable) {
                // If there is a navigation url on this item, set up the navigation stuff
                if (NavigateUrl.Length > 0) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, _owner.ResolveClientUrl(NavigateUrl));

                    // Use the MenuItem Target if it has one, the Menu's if it doesn't
                    string target = ViewState["Target"] as string;
                    if (target == null) {
                        target = _owner.Target;
                    }

                    if (target.Length > 0) {
                        writer.AddAttribute(HtmlTextWriterAttribute.Target, target);
                    }
                }
                // Otherwise, write out a postback that will select the item
                else {
                    writer.AddAttribute(HtmlTextWriterAttribute.Href,
                        _owner.Page.ClientScript.GetPostBackClientHyperlink(_owner, InternalValuePath, true, true));
                }
                // AccessKey
                if (!_owner.AccessKeyRendered && (accessKey.Length != 0)) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, accessKey, true);
                    _owner.AccessKeyRendered = true;
                }

            }
            else {
                // Span if disabled or not selectable
                if (!enabled) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "true");
                }
                else if ((ChildItems.Count != 0) && (depthPlusOne >= _owner.StaticDisplayLevels)) {
                    // For accessibility reasons, we want the a to have an href even if it's not selectable
                    // because we want it to be focusable if it has dynamic children.
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, "#");
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Cursor, "text");
                    // AccessKey
                    if (!_owner.AccessKeyRendered && (accessKey.Length != 0)) {
                        writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, accessKey, true);
                        _owner.AccessKeyRendered = true;
                    }
                }
            }
            if (depth != 0 && depth < _owner.StaticDisplayLevels) {
                Unit indent = _owner.StaticSubMenuIndent;

                // In 4.0, the default value of Menu.StaticSubMenuIndent was changed from 16px to Unit.Empty,
                // since the table and list rendering modes need to have different effective default values.
                // To maintain back compat, the effective default value for table rendering is 16px.
                // Dev10 
                if (indent.IsEmpty) {
                    indent = Unit.Pixel(16);
                }

                if (indent.Value != 0) {
                    double indentValue = indent.Value * depth;
                    if (indentValue <  Unit.MaxValue) {
                        indent = new Unit(indentValue, indent.Type);
                    }
                    else {
                        indent = new Unit(Unit.MaxValue, indent.Type);
                    }
                    writer.AddStyleAttribute("margin-left", indent.ToString(CultureInfo.InvariantCulture));
                }
            }
            // <a href=href>
            // We're rendering an A tag in all cases so that the client script can always find the items by tag name
            writer.RenderBeginTag(HtmlTextWriterTag.A);

            // Render out the item icon, if it is set and if the item is not templated
            if (ImageUrl.Length > 0 && NotTemplated()) {
                // <img>
                writer.AddAttribute(HtmlTextWriterAttribute.Src, _owner.ResolveClientUrl(ImageUrl));
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, ToolTip);
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "none");
                writer.AddStyleAttribute("vertical-align", "middle");
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
            }

            // Item text
            RenderText(writer);

            // </a> or </span>
            writer.RenderEndTag();

            bool shouldRenderExpandImage = ((depthPlusOne >= _owner.StaticDisplayLevels) &&
                (depthPlusOne < _owner.MaximumDepth));
            string expandImageUrl = (shouldRenderExpandImage ? GetExpandImageUrl() : String.Empty);

            // Render some space if horizontal
            bool needsSpace = false;
            if ((orientation == Orientation.Horizontal) &&
                (depth < _owner.StaticDisplayLevels) &&
                (!shouldRenderExpandImage || (expandImageUrl.Length == 0)) &&
                ((mergedStyle == null) || mergedStyle.ItemSpacing.IsEmpty)) {


                if (((Depth + 1) < _owner.StaticDisplayLevels) && (ChildItems.Count != 0)) {
                    // next level is static too and exists, so we're not the last static
                    needsSpace = true;
                }
                else {
                    // Static item with no static children.
                    // We need to check if there are any static items at any level after this one.
                    // This is done by checking if any ancestor is not last.
                    // This walk should be marginally executed as multiple static display levels
                    // don't make sense on a horizontal menu.
                    MenuItem parent = this;
                    while (parent != null) {
                        if (((parent.Parent == null) &&
                             (_owner.Items.Count != 0) &&
                             (parent != _owner.Items[_owner.Items.Count - 1])) ||
                            ((parent.Parent != null) &&
                             (parent.Parent.ChildItems.Count != 0) &&
                             (parent != parent.Parent.ChildItems[parent.Parent.ChildItems.Count - 1]))) {

                            needsSpace = true;
                            break;
                        }
                        parent = parent.Parent;
                    }
                }
            }

            // </td>
            writer.RenderEndTag();

            if (shouldRenderExpandImage && expandImageUrl.Length > 0) {
                // <td>
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "0");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                // <img>
                writer.AddAttribute(HtmlTextWriterAttribute.Src, expandImageUrl);
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "none");
                writer.AddStyleAttribute(HtmlTextWriterStyle.VerticalAlign, "middle");
                if (depth < _owner.StaticDisplayLevels) {

                    writer.AddAttribute(HtmlTextWriterAttribute.Alt,
                                        String.Format(CultureInfo.CurrentCulture, _owner.StaticPopOutImageTextFormatString, Text));
                }
                else if (depth >= _owner.StaticDisplayLevels) {

                    writer.AddAttribute(HtmlTextWriterAttribute.Alt,
                                        String.Format(CultureInfo.CurrentCulture, _owner.DynamicPopOutImageTextFormatString, Text));
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
                // </td>
                writer.RenderEndTag();
            }

            
            writer.RenderEndTag(); // </tr>
            writer.RenderEndTag(); // </table>

            writer.RenderEndTag(); // </td>
            if (orientation == Orientation.Vertical) {
                writer.RenderEndTag(); // </tr>
            }
            // Bottom (or right) item spacing
            // We do not render the spacing on the last static item or on the last item of a dynamic submenu
            if ((mergedStyle != null) && !mergedStyle.ItemSpacing.IsEmpty) {
                RenderItemSpacing(writer, mergedStyle.ItemSpacing, orientation);
            }
            else if (needsSpace) {
                RenderItemSpacing(writer, HorizontalDefaultSpacing, orientation);
            }
            // Bottom separator
            bool separator = (SeparatorImageUrl.Length != 0);
            bool staticBottomSeparator = (depth < _owner.StaticDisplayLevels) && (_owner.StaticBottomSeparatorImageUrl.Length != 0);
            bool dynamicBottomSeparator = (depth >= _owner.StaticDisplayLevels) && (_owner.DynamicBottomSeparatorImageUrl.Length != 0);
            if (separator || staticBottomSeparator || dynamicBottomSeparator) {
                if (orientation == Orientation.Vertical) {
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                if (separator) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, _owner.ResolveClientUrl(SeparatorImageUrl));
                }
                else if (staticBottomSeparator) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, _owner.ResolveClientUrl(_owner.StaticBottomSeparatorImageUrl));
                }
                else {
                writer.AddAttribute(HtmlTextWriterAttribute.Src, _owner.ResolveClientUrl(_owner.DynamicBottomSeparatorImageUrl));
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, String.Empty);
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag(); // Img
                writer.RenderEndTag(); // Td
                if (orientation == Orientation.Vertical) {
                    writer.RenderEndTag(); // Tr
                }
            }
        }

        private void RenderItemEvents(HtmlTextWriter writer) {
            writer.AddAttribute("onmouseout", "Menu_Unhover(this)");
            if (_owner.IsNotIE) {
                writer.AddAttribute("onkeyup", "Menu_Key(event)");
            }
            else {
                writer.AddAttribute("onkeyup", "Menu_Key(this)");
            }
        }

        private void RenderItemSpacing(HtmlTextWriter writer, Unit spacing, Orientation orientation) {
            if (orientation == Orientation.Vertical) {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height,
                    spacing.ToString(CultureInfo.InvariantCulture));
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
            else {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width,
                    spacing.ToString(CultureInfo.InvariantCulture));
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.RenderEndTag();
            }
        }

        internal void RenderText(HtmlTextWriter writer) {
            if (Container != null &&
                ((_owner.StaticItemTemplate != null && Depth < _owner.StaticDisplayLevels) ||
                (_owner.DynamicItemTemplate != null && Depth >= _owner.StaticDisplayLevels))) {

                Container.RenderControl(writer);
            }
            else {
                writer.Write(FormattedText);
            }
        }

        internal void ResetValuePathRecursive() {
            if (_valuePath != null) {
                _valuePath = null;
                foreach (MenuItem child in ChildItems) {
                    child.ResetValuePathRecursive();
                }
            }
        }


        /// <devdoc>
        ///     Marks this item as a databound item
        /// </devdoc>
        internal void SetDataBound(bool dataBound) {
            ViewState["DataBound"] = dataBound;
        }


        /// <devdoc>
        ///     Sets the data item for use by the user in databinding
        /// </devdoc>
        internal void SetDataItem(object dataItem) {
            _dataItem = dataItem;
        }

        /// <devdoc>
        ///     Sets the data path for use by the Menu in databinding
        /// </devdoc>
        internal void SetDataPath(string dataPath) {
            ViewState["DataPath"] = dataPath;
        }

        internal void SetDepth(int depth) {
            _depth = depth;
        }

        internal void SetDirty() {
            ViewState.SetDirty(true);

            if (ChildItems.Count > 0) {
                ChildItems.SetDirty();
            }
        }


        /// <devdoc>
        ///     Sets the owner Menu of this item.
        /// </devdoc>
        internal void SetOwner(Menu owner) {
            _owner = owner;

            if (_selectDesired == +1) {
                _selectDesired = 0;
                Selected = true;
            }
            else if (_selectDesired == -1) {
                _selectDesired = 0;
                Selected = false;
            }

            foreach (MenuItem item in ChildItems) {
                item.SetOwner(_owner);
            }
        }


        /// <devdoc>
        ///     Sets the parent MenuItem of the item
        /// </devdoc>
        internal void SetParent(MenuItem parent) {
            _parent = parent;
            SetPath(null);
        }

        internal void SetPath(string newPath) {
            _internalValuePath = newPath;
            _depth = -2;
        }

        internal void SetSelected(bool value) {
            ViewState["Selected"] = value;

            // If the owner hasn't been set, remember that we want to select this node
            // when the owner is determined
            if (_owner == null) {
                _selectDesired = (value ? +1 : -1);
            }
        }

            #region IStateManager implementation

        /// <internalonly/>
        bool IStateManager.IsTrackingViewState {
            get {
                return _isTrackingViewState;
            }
        }


        /// <internalonly/>
        void IStateManager.LoadViewState(object state) {
            object[] itemState = (object[])state;

            if (itemState != null) {
                if (itemState[0] != null) {
                    ((IStateManager)ViewState).LoadViewState(itemState[0]);
                }
                // We need to call the selected setter so that the owner can be notified
                // N.B. The treeview does not need to do that
                // because it's loading the state of its node differently because of the richer client-side behavior.
                NotifyOwnerSelected();

                if (itemState[1] != null) {
                    ((IStateManager)ChildItems).LoadViewState(itemState[1]);
                }
            }
        }


        /// <internalonly/>
        object IStateManager.SaveViewState() {
            object[] state = new object[2];
            if (_viewState != null) {
                state[0] = ((IStateManager)_viewState).SaveViewState();
            }

            if (_childItems != null) {
                state[1] = ((IStateManager)_childItems).SaveViewState();
            }

            if ((state[0] == null) && (state[1] == null)) {
                return null;
            }

            return state;
        }


        /// <internalonly/>
        void IStateManager.TrackViewState() {
            _isTrackingViewState = true;

            if (_viewState != null) {
                ((IStateManager)_viewState).TrackViewState();
            }

            if (_childItems != null) {
                ((IStateManager)_childItems).TrackViewState();
            }
        }
            #endregion

            #region ICloneable implementation

        /// <internalonly/>
        object ICloneable.Clone() {
            MenuItem newItem = new MenuItem();
            newItem.Enabled = Enabled;
            newItem.ImageUrl = ImageUrl;
            newItem.NavigateUrl = NavigateUrl;
            newItem.PopOutImageUrl = PopOutImageUrl;
            newItem.Selectable = Selectable;
            newItem.Selected = Selected;
            newItem.SeparatorImageUrl = SeparatorImageUrl;
            newItem.Target = Target;
            newItem.Text = Text;
            newItem.ToolTip = ToolTip;
            newItem.Value = Value;

            return newItem;
        }
            #endregion
    }

    public sealed class MenuItemTemplateContainer : Control, IDataItemContainer {

        private int _itemIndex;
        private object _dataItem;

        public MenuItemTemplateContainer(int itemIndex, MenuItem dataItem) {
            _itemIndex = itemIndex;
            _dataItem = dataItem;
        }

        public object DataItem {
            get {
                return _dataItem;
            }
            set {
                _dataItem = value;
            }
        }

        public int ItemIndex {
            get {
                return _itemIndex;
            }
        }

        protected override bool OnBubbleEvent(object source, EventArgs e) {
            CommandEventArgs ce = e as CommandEventArgs;
            if (ce != null) {
                if (ce is MenuEventArgs) {
                    RaiseBubbleEvent(this, ce);
                }
                else {
                    MenuEventArgs args = new MenuEventArgs((MenuItem)_dataItem, source, ce);
                    RaiseBubbleEvent(this, args);
                }
                return true;
            }
            return false;
        }

        object IDataItemContainer.DataItem {
            get {
                return _dataItem;
            }
        }

        int IDataItemContainer.DataItemIndex {
            get {
                return ItemIndex;
            }
        }

        int IDataItemContainer.DisplayIndex {
            get {
                return ItemIndex;
            }
        }
    }
}
