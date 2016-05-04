//------------------------------------------------------------------------------
// <copyright file="MenuAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.Adapters {

    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    public class MenuAdapter : WebControlAdapter, IPostBackEventHandler {
        
        private string _path;
        private Panel _menuPanel;
        private int _currentAccessKey = 0;
        private MenuItem _titleItem;

        protected new Menu Control  {
            get  {
                return (Menu) base.Control;
            }
        }

        protected internal override void LoadAdapterControlState(Object state) {
            if (state != null) {
                Pair pairState = state as Pair;
                if (pairState != null) {
                    base.LoadAdapterViewState(pairState.First);
                    _path = (string)pairState.Second;
                }
                else {
                    base.LoadAdapterViewState(null);
                    _path = state as string;
                }
            }
        }

        private string Escape(string path) {
            // This function escapes \\ so that they don't get replaced because of
            // a Netscape 4 bug. Other escapable characters will be escaped by .
            // _ becomes __ and \\ becomes \_\
            StringBuilder b = null;

            if (String.IsNullOrEmpty(path)) {
                return String.Empty;
            }

            int startIndex = 0;
            int count = 0;
            for (int i = 0; i < path.Length; i++) {
                switch (path[i]) {
                    case '\\':

                        if (i + 1 < path.Length && path[i + 1] == '\\') {
                            if (b == null) {
                                b = new StringBuilder(path.Length + 5);
                            }
                            if (count > 0) {
                                b.Append(path, startIndex, count);
                            }
                            b.Append(@"\_\");
                            i++;
                            startIndex = i + 1;
                            count = 0;
                        }
                        else {
                            count++;
                        }
                        break;
                    case '_':
                        if (b == null) {
                            b = new StringBuilder(path.Length + 5);
                        }

                        if (count > 0) {
                            b.Append(path, startIndex, count);
                        }

                        b.Append("__");

                        startIndex = i + 1;
                        count = 0;
                        break;
                    default:
                        count++;
                        break;
                }
            }

            if (b == null) {
                return path;
            }

            if (count > 0) {
                b.Append(path, startIndex, count);
            }

            return b.ToString();
        }

        private string UnEscape(string path) {
            return path.Replace(@"\\", @"\").Replace(@"\_\", @"\\").Replace("__", "_");
        }

        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);
            Control.Page.RegisterRequiresControlState(Control);
        }

        protected internal override void OnPreRender(EventArgs e) {
            Control.OnPreRender(e, false);
        }

        protected internal override object SaveAdapterControlState() {
            object baseState = base.SaveAdapterViewState();

            if (baseState == null) {
                if (_path != null) {
                    return _path;
                }
                else {
                    return null;
                }
            }
            else {
                return new Pair(baseState, _path);
            }
        }

        private void RenderBreak(HtmlTextWriter writer) {
            if (Control.Orientation == Orientation.Vertical) {
                writer.WriteBreak();
            }
            else {
                writer.Write(' ');
            }
        }

        protected override void RenderBeginTag(HtmlTextWriter writer) {
            Menu owner = Control;
            // skip link
            if (owner.SkipLinkText.Length != 0) {
                HyperLink skipLink = new HyperLink();
                skipLink.NavigateUrl = '#' + owner.ClientID + "_SkipLink";
                skipLink.ImageUrl = owner.SpacerImageUrl;
                skipLink.Text = owner.SkipLinkText;
                skipLink.Height = Unit.Pixel(1);
                skipLink.Width = Unit.Pixel(1);
                skipLink.Page = Page;
                skipLink.RenderControl(writer);
            }
            _menuPanel = new Panel();
            _menuPanel.ID = owner.UniqueID;
            _menuPanel.Page = Page;
            // Determine root menu style
            MenuItem titleItem;
            if (_path != null) {
                titleItem = owner.Items.FindItem(_path.Split(TreeView.InternalPathSeparator), 0);
                _titleItem = titleItem;
            }
            else {
                titleItem = owner.RootItem;
            }
            SubMenuStyle rootMenuStyle = owner.GetSubMenuStyle(titleItem);
            if (!rootMenuStyle.IsEmpty) {
                if (Page != null && Page.SupportsStyleSheets) {
                    string styleClass = owner.GetSubMenuCssClassName(titleItem);
                    if (styleClass.Trim().Length > 0) {
                        _menuPanel.CssClass = styleClass;
                    }
                }
                else {
                    _menuPanel.ApplyStyle(rootMenuStyle);
                }
            }
            _menuPanel.Width = owner.Width;
            _menuPanel.Height = owner.Height;
            _menuPanel.Enabled = owner.IsEnabled;
            _menuPanel.RenderBeginTag(writer);
        }

        protected override void RenderContents(HtmlTextWriter writer) {
            Menu owner = Control;
            int position = 0;
            if (_titleItem != null) {
                if (_titleItem.Depth + 1 >= owner.MaximumDepth) {
                    throw new InvalidOperationException(SR.GetString(SR.Menu_InvalidDepth));
                }
                if (!_titleItem.IsEnabled) {
                    throw new InvalidOperationException(SR.GetString(SR.Menu_InvalidNavigation));
                }
                RenderItem(writer, _titleItem, position++);
                foreach (MenuItem child in _titleItem.ChildItems) {
                    RenderItem(writer, child, position++);
                }
                if (PageAdapter != null) {
                    PageAdapter.RenderPostBackEvent(writer,
                                                    owner.UniqueID,
                                                    "u",
                                                    SR.GetString(SR.MenuAdapter_Up),
                                                    SR.GetString(SR.MenuAdapter_UpOneLevel));
                }
                else {
                    HyperLink link = new HyperLink();
                    link.NavigateUrl = Page.ClientScript.GetPostBackClientHyperlink(owner, "u");
                    link.Text = SR.GetString(SR.MenuAdapter_UpOneLevel);
                    link.Page = Page;
                    link.RenderControl(writer);
                }
                return;
            }
            else {
                position = 1;
            }
            _path = null;
            foreach(MenuItem child in owner.Items) {
                RenderItem(writer, child, position++);
                if (owner.StaticDisplayLevels > 1 && child.ChildItems.Count > 0) {
                    RenderContentsRecursive(writer, child, 1, owner.StaticDisplayLevels);
                }
            }
        }

        private void RenderContentsRecursive(HtmlTextWriter writer, MenuItem parentItem, int depth, int maxDepth) {
            int position = 1;
            foreach(MenuItem child in parentItem.ChildItems) {
                RenderItem(writer, child, position++);
                if (depth + 1 < maxDepth && child.ChildItems.Count > 0) {
                    RenderContentsRecursive(writer, child, depth + 1, maxDepth);
                }
            }
        }

        protected override void RenderEndTag(HtmlTextWriter writer) {
            _menuPanel.RenderEndTag(writer);
            // skip link
            if (Control.SkipLinkText.Length != 0) {
                HtmlAnchor skipAnchor = new HtmlAnchor();
                skipAnchor.Name = Control.ClientID + "_SkipLink";
                skipAnchor.Page = Page;
                skipAnchor.RenderControl(writer);
            }
        }

        private void RenderExpand(HtmlTextWriter writer, MenuItem item, Menu owner) {
            string expandImageUrl = item.GetExpandImageUrl();
            if (expandImageUrl.Length > 0) {
                Image expandImage = new Image();
                expandImage.ImageUrl = expandImageUrl;
                expandImage.GenerateEmptyAlternateText = true;
                if (item.Depth < owner.StaticDisplayLevels) {
                    
                    expandImage.AlternateText = String.Format(
                        CultureInfo.CurrentCulture,
                        owner.StaticPopOutImageTextFormatString,
                        item.Text);
                }
                else {
                    
                    expandImage.AlternateText = String.Format(
                        CultureInfo.CurrentCulture,
                        owner.DynamicPopOutImageTextFormatString,
                        item.Text);
                }
                // expandImage.ImageAlign = ImageAlign.Right;
                expandImage.ImageAlign = ImageAlign.AbsMiddle;
                expandImage.Page = Page;
                expandImage.RenderControl(writer);
            }
            else {
                writer.Write(' ');
                if (item.Depth < owner.StaticDisplayLevels &&
                    owner.StaticPopOutImageTextFormatString.Length != 0) {
                    
                    writer.Write(HttpUtility.HtmlEncode(String.Format(
                        CultureInfo.CurrentCulture,
                        owner.StaticPopOutImageTextFormatString,
                        item.Text)));
                }
                else if (item.Depth >= owner.StaticDisplayLevels &&
                    owner.DynamicPopOutImageTextFormatString.Length != 0) {
                    
                    writer.Write(HttpUtility.HtmlEncode(String.Format(
                        CultureInfo.CurrentCulture,
                        owner.DynamicPopOutImageTextFormatString,
                        item.Text)));
                }
                else {
                    writer.Write(HttpUtility.HtmlEncode(SR.GetString(SR.MenuAdapter_Expand, item.Text)));
                }
            }
        }

        protected virtual internal void RenderItem(HtmlTextWriter writer, MenuItem item, int position) {
            Menu owner = Control;
            MenuItemStyle mergedStyle = owner.GetMenuItemStyle(item);

            string imageUrl = item.ImageUrl;
            int depth = item.Depth;
            int depthPlusOne = depth + 1;
            string toolTip = item.ToolTip;
            string navigateUrl = item.NavigateUrl;
            string text = item.Text;
            bool enabled = item.IsEnabled;
            bool selectable = item.Selectable;
            MenuItemCollection childItems = item.ChildItems;

            // Top separator
            string topSeparatorUrl = null;
            if (depth < owner.StaticDisplayLevels && owner.StaticTopSeparatorImageUrl.Length != 0) {
                topSeparatorUrl = owner.StaticTopSeparatorImageUrl;
            }
            else if (depth >= owner.StaticDisplayLevels && owner.DynamicTopSeparatorImageUrl.Length != 0) {
                topSeparatorUrl = owner.DynamicTopSeparatorImageUrl;
            }
            if (topSeparatorUrl != null) {
                Image separatorImage = new Image();
                separatorImage.ImageUrl = topSeparatorUrl;
                separatorImage.GenerateEmptyAlternateText = true; // XHtml compliance
                separatorImage.Page = Page;
                separatorImage.RenderControl(writer);
                RenderBreak(writer);
            }

            // Don't render the top spacing if this is the first root item
            if ((mergedStyle != null) && !mergedStyle.ItemSpacing.IsEmpty &&
                ((_titleItem != null) || (position != 0))) {
                RenderSpace(writer, mergedStyle.ItemSpacing, owner.Orientation);
            }

            // Item span
            Panel itemPanel = new SpanPanel();
            itemPanel.Enabled = enabled;
            itemPanel.Page = Page;

            // Apply styles
            if (Page != null && Page.SupportsStyleSheets) {
                string styleClass = owner.GetCssClassName(item, false);
                if (styleClass.Trim().Length > 0) {
                    itemPanel.CssClass = styleClass;
                }
            }
            else if (mergedStyle != null) {
                itemPanel.ApplyStyle(mergedStyle);
            }

            // Tooltip
            if (item.ToolTip.Length != 0) {
                itemPanel.ToolTip = item.ToolTip;
            }

            // Render item begin tag
            itemPanel.RenderBeginTag(writer);

            // If there is a navigation url on this item, set up the navigation stuff if:
            // - the item is the current title item for this level
            // - the item has no children
            // - the item is a static item of depth + 1 < StaticDisplayLevels
            bool clickOpensThisNode = !((position == 0) || 
                                        (childItems.Count == 0) ||
                                        (depthPlusOne < owner.StaticDisplayLevels) || 
                                        (depthPlusOne >= owner.MaximumDepth));
           
            // Indent
            if (position != 0 &&
                depth > 0 &&
                owner.StaticSubMenuIndent != Unit.Pixel(0) &&
                depth < owner.StaticDisplayLevels) {
                Image spacerImage = new Image();
                spacerImage.ImageUrl = owner.SpacerImageUrl;
                spacerImage.GenerateEmptyAlternateText = true; // XHtml compliance
                double indent = owner.StaticSubMenuIndent.Value * depth;
                if (indent < Unit.MaxValue) {
                    spacerImage.Width = new Unit(indent, owner.StaticSubMenuIndent.Type);
                }
                else {
                    spacerImage.Width = new Unit(Unit.MaxValue, owner.StaticSubMenuIndent.Type);;
                }
                spacerImage.Height = Unit.Pixel(1);
                spacerImage.Page = Page;
                spacerImage.RenderControl(writer);
            }

            // Render out the item icon, if it is set and if the item is not templated (VSWhidbey 501618)
            if (imageUrl.Length > 0 && item.NotTemplated()) {
                Image newImage = new Image();
                newImage.ImageUrl = imageUrl;
                if (toolTip.Length != 0) {
                    newImage.AlternateText = toolTip;
                }
                else {
                    newImage.GenerateEmptyAlternateText = true; // XHtml compliance
                }
                newImage.Page = Page;
                newImage.RenderControl(writer);
                writer.Write(' ');
            }

            bool applyInlineBorder;
            string linkClass;
            if (Page != null && Page.SupportsStyleSheets) {
                linkClass = owner.GetCssClassName(item, true, out applyInlineBorder);
            }
            else {
                linkClass = String.Empty;
                applyInlineBorder = false;
            }
            if (enabled && (clickOpensThisNode || selectable)) {
                string accessKey = owner.AccessKey;
                string itemAccessKey = ((position == 0 || (position == 1 && depth == 0)) && accessKey.Length != 0) ?
                    accessKey :
                    null;
                if (navigateUrl.Length > 0 && !clickOpensThisNode) {
                    if (PageAdapter != null) {
                        PageAdapter.RenderBeginHyperlink(writer,
                                                         owner.ResolveClientUrl(navigateUrl),
                                                         true,
                                                         SR.GetString(SR.Adapter_GoLabel),
                                                         itemAccessKey != null ?
                                                          itemAccessKey :
                                                          (_currentAccessKey < 10 ?
                                                            (_currentAccessKey++).ToString(CultureInfo.InvariantCulture) :
                                                            null));
                        writer.Write(HttpUtility.HtmlEncode(item.FormattedText));
                        PageAdapter.RenderEndHyperlink(writer);
                    }
                    else {
                        HyperLink link = new HyperLink();
                        link.NavigateUrl = owner.ResolveClientUrl(navigateUrl);
                        string target = item.Target;
                        if (String.IsNullOrEmpty(target)) {
                            target = owner.Target;
                        }
                        if (!String.IsNullOrEmpty(target)) {
                            link.Target = target;
                        }
                        link.AccessKey = itemAccessKey;
                        link.Page = Page;
                        if (writer is Html32TextWriter) {
                            link.RenderBeginTag(writer);
                            SpanPanel lbl = new SpanPanel();
                            lbl.Page = Page;
                            RenderStyle(writer, lbl, linkClass, mergedStyle, applyInlineBorder);
                            lbl.RenderBeginTag(writer);
                            item.RenderText(writer);
                            lbl.RenderEndTag(writer);
                            link.RenderEndTag(writer);
                        }
                        else {
                            RenderStyle(writer, link, linkClass, mergedStyle, applyInlineBorder);
                            link.RenderBeginTag(writer);
                            item.RenderText(writer);
                            link.RenderEndTag(writer);
                        }
                    }
                }
                // Otherwise, write out a postback that will open or select the item
                else {
                    if (PageAdapter != null) {
                        PageAdapter.RenderPostBackEvent(writer,
                                                        owner.UniqueID,
                                                        (clickOpensThisNode ? 'o' : 'b') +
                                                            Escape(item.InternalValuePath),
                                                        SR.GetString(SR.Adapter_OKLabel),
                                                        item.FormattedText,
                                                        null,
                                                        itemAccessKey != null ?
                                                          itemAccessKey :
                                                          (_currentAccessKey < 10 ?
                                                         (_currentAccessKey++).ToString(CultureInfo.InvariantCulture) :
                                                         null));

                        // Expand image
                        if (clickOpensThisNode) {
                            RenderExpand(writer, item, owner);
                        }
                    }
                    else {
                        HyperLink link = new HyperLink();
                        link.NavigateUrl = Page.ClientScript.GetPostBackClientHyperlink(owner,
                            (clickOpensThisNode ? 'o' : 'b') + Escape(item.InternalValuePath), true);
                        link.AccessKey = itemAccessKey;
                        link.Page = Page;
                        if (writer is Html32TextWriter) {
                            link.RenderBeginTag(writer);
                            SpanPanel lbl = new SpanPanel();
                            lbl.Page = Page;
                            RenderStyle(writer, lbl, linkClass, mergedStyle, applyInlineBorder);
                            lbl.RenderBeginTag(writer);
                            item.RenderText(writer);
                            if (clickOpensThisNode) {
                                RenderExpand(writer, item, owner);
                            }
                            lbl.RenderEndTag(writer);
                            link.RenderEndTag(writer);
                        }
                        else {
                            RenderStyle(writer, link, linkClass, mergedStyle, applyInlineBorder);
                            link.RenderBeginTag(writer);
                            item.RenderText(writer);
                            if (clickOpensThisNode) {
                                RenderExpand(writer, item, owner);
                            }
                            link.RenderEndTag(writer);
                        }
                    }
                }
            }
            else {
                item.RenderText(writer);
            }
            itemPanel.RenderEndTag(writer);

            // Bottom (or right) item spacing
            RenderBreak(writer);
            if ((mergedStyle != null) && !mergedStyle.ItemSpacing.IsEmpty) {
                RenderSpace(writer, mergedStyle.ItemSpacing, owner.Orientation);
            }

            // Bottom separator
            string bottomSeparatorUrl = null;
            if (item.SeparatorImageUrl.Length != 0) {
                bottomSeparatorUrl = item.SeparatorImageUrl;
            }
            else if ((depth < owner.StaticDisplayLevels) && (owner.StaticBottomSeparatorImageUrl.Length != 0)) {
                bottomSeparatorUrl = owner.StaticBottomSeparatorImageUrl;
            }
            else if ((depth >= owner.StaticDisplayLevels) && (owner.DynamicBottomSeparatorImageUrl.Length != 0)) {
                bottomSeparatorUrl = owner.DynamicBottomSeparatorImageUrl;
            }
            if (bottomSeparatorUrl != null) {
                Image separatorImage = new Image();
                separatorImage.ImageUrl = bottomSeparatorUrl;
                separatorImage.GenerateEmptyAlternateText = true; // XHtml compliance
                separatorImage.Page = Page;
                separatorImage.RenderControl(writer);
                RenderBreak(writer);
            }
        }
        
        private void RenderSpace(HtmlTextWriter writer, Unit space, Orientation orientation) {
            Image spacerImage = new Image();
            spacerImage.ImageUrl = Control.SpacerImageUrl;
            spacerImage.GenerateEmptyAlternateText = true; // XHtml compliance
            spacerImage.Page = Page;
            if (orientation == Orientation.Vertical) {
                spacerImage.Height = space;
                spacerImage.Width = Unit.Pixel(1);
                spacerImage.RenderControl(writer);
                writer.WriteBreak();
            }
            else {
                spacerImage.Width = space;
                spacerImage.Height = Unit.Pixel(1);
                spacerImage.RenderControl(writer);
            }
        }

        private void RenderStyle(HtmlTextWriter writer, WebControl control, string className, MenuItemStyle style, bool applyInlineBorder) {
            if (!String.IsNullOrEmpty(className)) {
                control.CssClass = className;
                if (applyInlineBorder) {
                    // Add inline style to force the border to none to override any CssClass (VSWhidbey 336610)
                    writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "none");
                    // And an inline font-size of 1em to avoid squaring relative font sizes by applying them twice
                    writer.AddStyleAttribute(HtmlTextWriterStyle.FontSize, "1em");
                }
            }
            else if (style != null) {
                control.ApplyStyle(style);
            }
        }

        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
            RaisePostBackEvent(eventArgument);
        }

        protected virtual void RaisePostBackEvent(string eventArgument) {
            if (eventArgument.Length == 0) {
                return;
            }

            // On postback, see what kind of event we received by checking the first character
            char eventType = eventArgument[0];

            switch (eventType) {
                case 'o': {
                    // 'o' means that we're opening the secondary navigation for this node
                    // Get the path of the item specified in the eventArgument
                    // The replace is to correct a Netscape 4 bug
                    string newPath = UnEscape(HttpUtility.UrlDecode(eventArgument.Substring(1)));
                    // Check the number of separator characters in the argument (should not be more than the max depth)
                    int matches = 0;
                    for (int i = 0; i < newPath.Length; i++) {
                        if (newPath[i] == TreeView.InternalPathSeparator) {
                            if (++matches >= Control.MaximumDepth) {
                                throw new InvalidOperationException(SR.GetString(SR.Menu_InvalidDepth));
                            }
                        }
                    }
                    // Check this item does have subitem
                    // (otherwise, it should just be selected, not opened)
                    MenuItem item = Control.Items.FindItem(newPath.Split(TreeView.InternalPathSeparator), 0);
                    if (item != null) {
                        if (item.ChildItems.Count > 0) {
                            _path = newPath;
                        }
                        else {
                            Control.InternalRaisePostBackEvent(newPath);
                        }
                    }
                    break;
                }
                case 'u' : 
                    // 'u' means go up a level
                    if (_path != null) {
                        // Find that item in the tree
                        MenuItem item = Control.Items.FindItem(_path.Split(TreeView.InternalPathSeparator), 0);
                        if (item != null) {
                            MenuItem parentItem = item.Parent;
                            if (parentItem != null && item.Depth + 1 > Control.StaticDisplayLevels) {
                                _path = parentItem.InternalValuePath;
                            }
                            else {
                                _path = null;
                            }
                        }
                    }
                    break;

                case 'b' : 
                    // 'b' means bubble for to the control to handle
                    // The replace is to correct a Netscape 4 bug
                    Control.InternalRaisePostBackEvent(
                        UnEscape(HttpUtility.UrlDecode(eventArgument.Substring(1))));
                    break;
            }
        }

        internal void SetPath(string path) {
            _path = path;
        }

        private class SpanPanel : Panel {
            protected override HtmlTextWriterTag TagKey {
                get {
                    return HtmlTextWriterTag.Span;
                }
            }
        }
    }
}
