//------------------------------------------------------------------------------
// <copyright file="MenuRendererStandards.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System.Collections;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Web.Util;

    public partial class Menu {
        /// <devdoc>The standards-compliant Menu renderer</devdoc>
        internal class MenuRendererStandards : MenuRenderer {
            private string _dynamicPopOutUrl;
            private string _staticPopOutUrl;

            public MenuRendererStandards(Menu menu) : base(menu) { }

            private string DynamicPopOutUrl {
                get {
                    if (_dynamicPopOutUrl == null) {
                        _dynamicPopOutUrl = GetDynamicPopOutImageUrl();
                    }
                    return _dynamicPopOutUrl;
                }
            }

            protected virtual string SpacerImageUrl {
                get {
                    return Menu.SpacerImageUrl;
                }
            }

            private string StaticPopOutUrl {
                get {
                    if (_staticPopOutUrl == null) {
                        _staticPopOutUrl = GetStaticPopOutImageUrl();
                    }
                    return _staticPopOutUrl;
                }
            }

            private void AddScriptReference() {
                string key = "_registerMenu_" + Menu.ClientID;
                string initScript = String.Format(CultureInfo.InvariantCulture,
                    "<script type='text/javascript'>" +
                    "new Sys.WebForms.Menu({{ element: '{0}', disappearAfter: {1}, orientation: '{2}', tabIndex: {3}, disabled: {4} }});" +
                    "</script>",
                    Menu.ClientID,
                    Menu.DisappearAfter,
                    Menu.Orientation.ToString().ToLowerInvariant(),
                    Menu.TabIndex,
                    (!Menu.IsEnabled).ToString().ToLowerInvariant());

                if (Menu.Page.ScriptManager != null) {
                    Menu.Page.ScriptManager.RegisterClientScriptResource(Menu.Page, typeof(Menu), "MenuStandards.js");
                    Menu.Page.ScriptManager.RegisterStartupScript(Menu, typeof(MenuRendererStandards), key, initScript, false);
                }
                else {
                    Menu.Page.ClientScript.RegisterClientScriptResource(Menu.Page, typeof(Menu), "MenuStandards.js");
                    Menu.Page.ClientScript.RegisterStartupScript(typeof(MenuRendererStandards), key, initScript);
                }
            }

            private void AddStyleBlock() {
                if (Menu.IncludeStyleBlock) {
                    Menu.Page.Header.Controls.Add(CreateStyleBlock());
                }
            }

            private StyleBlock CreateStyleBlock() {
                StyleBlock styleBlock = new StyleBlock();
                Style rootMenuItemStyle = Menu.RootMenuItemStyle;

                // drop the font and forecolor from the control style, those are applied
                // to the anchors directly with rootMenuItemStyle.
                Style menuStyle = null;
                if (!Menu.ControlStyle.IsEmpty) {
                    menuStyle = new Style();
                    menuStyle.CopyFrom(Menu.ControlStyle);
                    // Relative sizes should not be multiplied (VSWhidbey 457610)
                    menuStyle.Font.Reset();
                    menuStyle.ForeColor = Color.Empty;
                }

                // Menu surrounding DIV style -- without ForeColor or Font,
                // those are applied directly to the '#Menu a' selector.

                styleBlock.AddStyleDefinition("#{0}", Menu.ClientID)
                          .AddStyles(menuStyle);

                // Image styles

                styleBlock.AddStyleDefinition("#{0} img.icon", Menu.ClientID)
                          .AddStyle(HtmlTextWriterStyle.BorderStyle, "none")
                          .AddStyle(HtmlTextWriterStyle.VerticalAlign, "middle");

                styleBlock.AddStyleDefinition("#{0} img.separator", Menu.ClientID)
                          .AddStyle(HtmlTextWriterStyle.BorderStyle, "none")
                          .AddStyle(HtmlTextWriterStyle.Display, "block");

                if (Menu.Orientation == Orientation.Horizontal) {
                    styleBlock.AddStyleDefinition("#{0} img.horizontal-separator", Menu.ClientID)
                              .AddStyle(HtmlTextWriterStyle.BorderStyle, "none")
                              .AddStyle(HtmlTextWriterStyle.VerticalAlign, "middle");
                }

                // Menu styles

                styleBlock.AddStyleDefinition("#{0} ul", Menu.ClientID)
                          .AddStyle("list-style", "none")
                          .AddStyle(HtmlTextWriterStyle.Margin, "0")
                          .AddStyle(HtmlTextWriterStyle.Padding, "0")
                          .AddStyle(HtmlTextWriterStyle.Width, "auto");

                styleBlock.AddStyleDefinition("#{0} ul.static", Menu.ClientID)
                          .AddStyles(Menu._staticMenuStyle);

                var ulDynamic = styleBlock.AddStyleDefinition("#{0} ul.dynamic", Menu.ClientID)
                                          .AddStyles(Menu._dynamicMenuStyle)
                                          .AddStyle(HtmlTextWriterStyle.ZIndex, "1");

                if (Menu.DynamicHorizontalOffset != 0) {
                    ulDynamic.AddStyle(HtmlTextWriterStyle.MarginLeft, Menu.DynamicHorizontalOffset.ToString(CultureInfo.InvariantCulture) + "px");
                }

                if (Menu.DynamicVerticalOffset != 0) {
                    ulDynamic.AddStyle(HtmlTextWriterStyle.MarginTop, Menu.DynamicVerticalOffset.ToString(CultureInfo.InvariantCulture) + "px");
                }

                if (Menu._levelStyles != null) {
                    int index = 1;

                    foreach (SubMenuStyle style in Menu._levelStyles) {
                        styleBlock.AddStyleDefinition("#{0} ul.level{1}", Menu.ClientID, index++)
                                  .AddStyles(style);
                    }
                }

                // Menu item styles

                // MenuItems have a 14px default right padding.
                // This is necessary to prevent the default (and the typical) popout image from going under
                // the menu item text when it is one of the longer items in the parent menu.
                // It is 'px' based since its based on the image size not font size.
                styleBlock.AddStyleDefinition("#{0} a", Menu.ClientID)
                          .AddStyle(HtmlTextWriterStyle.WhiteSpace, "nowrap")
                          .AddStyle(HtmlTextWriterStyle.Display, "block")
                          .AddStyles(rootMenuItemStyle);

                var menuItemStatic = styleBlock.AddStyleDefinition("#{0} a.static", Menu.ClientID);
                if ((Menu.Orientation == Orientation.Horizontal) &&
                    ((Menu._staticItemStyle == null) || (Menu._staticItemStyle.HorizontalPadding.IsEmpty))) {
                    menuItemStatic.AddStyle(HtmlTextWriterStyle.PaddingLeft, "0.15em")
                                  .AddStyle(HtmlTextWriterStyle.PaddingRight, "0.15em");
                }
                menuItemStatic.AddStyles(Menu._staticItemStyle);

                if (Menu._staticItemStyle != null) {
                    menuItemStatic.AddStyles(Menu._staticItemStyle.HyperLinkStyle);
                }

                if (!String.IsNullOrEmpty(StaticPopOutUrl)) {
                    styleBlock.AddStyleDefinition("#{0} a.popout", Menu.ClientID)
                        .AddStyle("background-image", "url(\"" + Menu.ResolveClientUrl(StaticPopOutUrl).Replace("\"", "\\\"") + "\")")
                        .AddStyle("background-repeat", "no-repeat")
                        .AddStyle("background-position", "right center")
                        .AddStyle(HtmlTextWriterStyle.PaddingRight, "14px");
                }

                if (!String.IsNullOrEmpty(DynamicPopOutUrl)) {
                    // Check if dynamic popout is the same as the static one, so there's no need for a separate rule
                    if (DynamicPopOutUrl != StaticPopOutUrl) {
                        styleBlock.AddStyleDefinition("#{0} a.popout-dynamic", Menu.ClientID)
                            .AddStyle("background", "url(\"" + Menu.ResolveClientUrl(DynamicPopOutUrl).Replace("\"", "\\\"") + "\") no-repeat right center")
                            .AddStyle(HtmlTextWriterStyle.PaddingRight, "14px");
                    }
                }

                var styleBlockStyles = styleBlock.AddStyleDefinition("#{0} a.dynamic", Menu.ClientID)
                    .AddStyles(Menu._dynamicItemStyle);
                if (Menu._dynamicItemStyle != null) {
                    styleBlockStyles.AddStyles(Menu._dynamicItemStyle.HyperLinkStyle);
                }

                if (Menu._levelMenuItemStyles != null || Menu.StaticDisplayLevels > 1) {
                    int lastIndex = Menu.StaticDisplayLevels;
                    if (Menu._levelMenuItemStyles != null) {
                        lastIndex = Math.Max(lastIndex, Menu._levelMenuItemStyles.Count);
                    }

                    for (int index = 0; index < lastIndex; ++index) {
                        var style = styleBlock.AddStyleDefinition("#{0} a.level{1}", Menu.ClientID, index + 1);

                        if (index > 0 && index < Menu.StaticDisplayLevels) {
                            Unit indent = Menu.StaticSubMenuIndent;
                            
                            // The default value of Menu.StaticSubMenuIndent is Unit.Empty, and the effective default value
                            // for list rendering is either 1em (vertical) or empty (horizontal).
                            if (indent.IsEmpty && Menu.Orientation == Orientation.Vertical) {
                                indent = new Unit(1, UnitType.Em);
                            }

                            if (!indent.IsEmpty && indent.Value != 0) {
                                double indentValue = indent.Value * index;
                                if (indentValue < Unit.MaxValue) {
                                    indent = new Unit(indentValue, indent.Type);
                                }
                                else {
                                    indent = new Unit(Unit.MaxValue, indent.Type);
                                }

                                style.AddStyle(HtmlTextWriterStyle.PaddingLeft, indent.ToString(CultureInfo.InvariantCulture));
                            }
                        }

                        if (Menu._levelMenuItemStyles != null && index < Menu._levelMenuItemStyles.Count) {
                            var levelItemStyle = Menu._levelMenuItemStyles[index];
                            style.AddStyles(levelItemStyle).AddStyles(levelItemStyle.HyperLinkStyle);
                        }
                    }
                }

                styleBlockStyles = styleBlock.AddStyleDefinition("#{0} a.static.selected", Menu.ClientID)
                          .AddStyles(Menu._staticSelectedStyle);
                if (Menu._staticSelectedStyle != null) {
                    styleBlockStyles.AddStyles(Menu._staticSelectedStyle.HyperLinkStyle);
                }

                styleBlockStyles = styleBlock.AddStyleDefinition("#{0} a.dynamic.selected", Menu.ClientID)
                          .AddStyles(Menu._dynamicSelectedStyle);
                if (Menu._dynamicSelectedStyle != null) {
                    styleBlockStyles.AddStyles(Menu._dynamicSelectedStyle.HyperLinkStyle);
                }

                styleBlock.AddStyleDefinition("#{0} a.static.highlighted", Menu.ClientID)
                          .AddStyles(Menu._staticHoverStyle);

                styleBlock.AddStyleDefinition("#{0} a.dynamic.highlighted", Menu.ClientID)
                          .AddStyles(Menu._dynamicHoverStyle);

                if (Menu._levelSelectedStyles != null) {
                    int index = 1;

                    foreach (MenuItemStyle style in Menu._levelSelectedStyles) {
                        styleBlock.AddStyleDefinition("#{0} a.selected.level{1}", Menu.ClientID, index++)
                                  .AddStyles(style).AddStyles(style.HyperLinkStyle);
                    }
                }

                return styleBlock;
            }

            private string GetCssClass(int level, Style staticStyle, Style dynamicStyle, IList levelStyles) {
                string result = "level" + level;
                Style style;

                if (level > Menu.StaticDisplayLevels) {
                    style = dynamicStyle;
                }
                else {
                    if (Menu.DesignMode) {
                        result += " static";
                        if (Menu.Orientation == Orientation.Horizontal) {
                            result += " horizontal";
                        }
                    }

                    style = staticStyle;
                }

                if (style != null && !String.IsNullOrEmpty(style.CssClass)) {
                    result += " " + style.CssClass;
                }

                if (levelStyles != null && levelStyles.Count >= level) {
                    Style levelStyle = (Style)levelStyles[level - 1];

                    if (levelStyle != null && !String.IsNullOrEmpty(levelStyle.CssClass)) {
                        result += " " + levelStyle.CssClass;
                    }
                }

                return result;
            }

            protected virtual string GetDynamicPopOutImageUrl() {
                string url = Menu.DynamicPopOutImageUrl;
                if (String.IsNullOrEmpty(url) && Menu.DynamicEnableDefaultPopOutImage) {
                    url = Menu.GetImageUrl(Menu.PopOutImageIndex);
                }
                return url;
            }

            protected virtual string GetStaticPopOutImageUrl() {
                string url = Menu.StaticPopOutImageUrl;
                if (String.IsNullOrEmpty(url) && Menu.StaticEnableDefaultPopOutImage) {
                    url = Menu.GetImageUrl(Menu.PopOutImageIndex);
                }
                return url;
            }

            private string GetMenuCssClass(int level) {
                return GetCssClass(level, Menu.StaticMenuStyle, Menu.DynamicMenuStyle, Menu._levelStyles);
            }

            private string GetMenuItemCssClass(MenuItem item, int level) {
                // give the A the proper popout class
                string cssClass = null;
                if (ShouldHavePopOutImage(item)) {
                    if (level > Menu.StaticDisplayLevels) {
                        if (!String.IsNullOrEmpty(DynamicPopOutUrl)) {
                            cssClass = (DynamicPopOutUrl == StaticPopOutUrl) ? "popout" : "popout-dynamic";
                        }
                    }
                    else if (!String.IsNullOrEmpty(StaticPopOutUrl)) {
                        cssClass = "popout";
                    }
                }
                string levelCssClass = GetCssClass(level, Menu.StaticMenuItemStyle, Menu.DynamicMenuItemStyle, Menu._levelMenuItemStyles);
                if (!String.IsNullOrEmpty(cssClass)) {
                    return cssClass + " " + levelCssClass;
                }
                else {
                    return levelCssClass;
                }
            }

            protected virtual string GetPostBackEventReference(MenuItem item) {
                return Menu.Page.ClientScript.GetPostBackEventReference(Menu, item.InternalValuePath, true);
            }

            private bool IsChildPastMaximumDepth(MenuItem item) {
                return (item.Depth + 1 >= Menu.MaximumDepth);
            }

            private bool IsChildDepthDynamic(MenuItem item) {
                return (item.Depth + 1 >= Menu.StaticDisplayLevels);
            }

            private bool IsDepthDynamic(MenuItem item) {
                // Depth is 0 based. StaticDisplayLevels is 1 based because it is a counter
                // (1 means show "one level" statically -- so, show Depth 0 statically but not Depth 1).
                // Therefore, it is dynamic if the item's depth is greater than or equal to the static levels.
                // Depth = 2 (3rd level), StaticDisplayLevels = 3 ==> Static
                // Depth = 2 (3rd level), StaticDisplayLevels = 2 ==> Dynamic
                return (item.Depth >= Menu.StaticDisplayLevels);
            }

            private bool IsDepthStatic(MenuItem item) {
                return !IsDepthDynamic(item);
            }

            public override void PreRender(bool registerScript) {
                if (Menu.DesignMode || Menu.Page == null) {
                    return;
                }

                if (Menu.IncludeStyleBlock && Menu.Page.Header == null) {
                    throw new InvalidOperationException(SR.GetString(SR.NeedHeader, "Menu.IncludeStyleBlock"));
                }

                AddScriptReference();  // We always need our script, even if we're disabled, because the script sets our styles
                AddStyleBlock();
            }

            public override void RenderBeginTag(HtmlTextWriter writer, bool staticOnly) {
                ControlRenderingHelper.WriteSkipLinkStart(writer, Menu.RenderingCompatibility, Menu.DesignMode, Menu.SkipLinkText, SpacerImageUrl, Menu.ClientID);

                if (Menu.DesignMode && Menu.IncludeStyleBlock) {
                    // Need to render style block in design mode, since it won't be present
                    CreateStyleBlock().Render(writer);
                }

                // Add expando attributes
                if (Menu.HasAttributes) {
                    foreach (string key in Menu.Attributes.Keys) {
                        writer.AddAttribute(key, Menu.Attributes[key]);
                    }
                }

                // CSS class, including disabled class if it's set
                string cssClass = Menu.CssClass ?? "";
                if (!Menu.Enabled) {
                    cssClass = (cssClass + " " + DisabledCssClass).Trim();
                }
                if (!String.IsNullOrEmpty(cssClass)) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, cssClass);
                }

                // Need to simulate the float done by Javascript when we're in design mode
                if (Menu.DesignMode) {
                    writer.AddStyleAttribute("float", "left");
                }

                writer.AddAttribute(HtmlTextWriterAttribute.Id, Menu.ClientID);
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
            }

            public override void RenderContents(HtmlTextWriter writer, bool staticOnly) {
                RenderItems(writer, staticOnly || Menu.DesignMode || !Menu.Enabled, Menu.Items, 1, !String.IsNullOrEmpty(Menu.AccessKey));
            }

            public override void RenderEndTag(HtmlTextWriter writer, bool staticOnly) {
                writer.RenderEndTag();

                // Need to simulate the clear done by Javascript when we're in design mode
                if (Menu.DesignMode) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Style, "clear: left");
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    writer.RenderEndTag();
                }

                ControlRenderingHelper.WriteSkipLinkEnd(writer, Menu.DesignMode, Menu.SkipLinkText, Menu.ClientID);
            }

            private bool RenderItem(HtmlTextWriter writer, MenuItem item, int level, string cssClass, bool needsAccessKey) {
                RenderItemPreSeparator(writer, item);

                if (Menu.DesignMode && Menu.Orientation == Orientation.Horizontal) {
                    writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
                }
                needsAccessKey = RenderItemLinkAttributes(writer, item, level, cssClass, needsAccessKey);
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                RenderItemIcon(writer, item);
                item.RenderText(writer);
                // popout image is in the A's background css
                writer.RenderEndTag();  // </a>

                RenderItemPostSeparator(writer, item);

                return needsAccessKey;
            }

            private void RenderItemIcon(HtmlTextWriter writer, MenuItem item) {
                if (String.IsNullOrEmpty(item.ImageUrl) || !item.NotTemplated()) {
                    return;
                }

                writer.AddAttribute(HtmlTextWriterAttribute.Src, Menu.ResolveClientUrl(item.ImageUrl));
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, item.ToolTip);
                writer.AddAttribute(HtmlTextWriterAttribute.Title, item.ToolTip);
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "icon");
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
            }

            private bool RenderItemLinkAttributes(HtmlTextWriter writer, MenuItem item, int level, string cssClass, bool needsAccessKey) {
                if (!String.IsNullOrEmpty(item.ToolTip)) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, item.ToolTip);
                }

                // Bail out for for disabled or non-selectable menu items
                if (!item.Enabled || !Menu.Enabled) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, cssClass + " " + DisabledCssClass);
                    return needsAccessKey;
                }
                if (!item.Selectable) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, cssClass);
                    return needsAccessKey;
                }

                // Selected
                if (item.Selected) {
                    cssClass += " selected";
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Class, cssClass);

                // Attach the access key to the first link we render
                if (needsAccessKey) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, Menu.AccessKey);
                }

                // Postback...
                if (String.IsNullOrEmpty(item.NavigateUrl)) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, "#");
                    writer.AddAttribute(HtmlTextWriterAttribute.Onclick, GetPostBackEventReference(item));
                }
                // ...or direct link
                else {
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, Menu.ResolveClientUrl(item.NavigateUrl));

                    string target = item.Target;
                    if (String.IsNullOrEmpty(target)) {
                        target = Menu.Target;
                    }
                    if (!String.IsNullOrEmpty(target)) {
                        writer.AddAttribute(HtmlTextWriterAttribute.Target, target);
                    }
                }

                return false;
            }

            private void RenderItemPostSeparator(HtmlTextWriter writer, MenuItem item) {
                string separatorImageUrl = item.SeparatorImageUrl;
                if (String.IsNullOrEmpty(separatorImageUrl)) {
                    separatorImageUrl = IsDepthStatic(item)
                                       ? Menu.StaticBottomSeparatorImageUrl
                                       : Menu.DynamicBottomSeparatorImageUrl;
                }

                if (!String.IsNullOrEmpty(separatorImageUrl)) {
                    RenderItemSeparatorImage(writer, item, separatorImageUrl);
                }
            }

            private void RenderItemPreSeparator(HtmlTextWriter writer, MenuItem item) {
                string separatorImageUrl = IsDepthStatic(item)
                                   ? Menu.StaticTopSeparatorImageUrl
                                   : Menu.DynamicTopSeparatorImageUrl;

                if (!String.IsNullOrEmpty(separatorImageUrl)) {
                    RenderItemSeparatorImage(writer, item, separatorImageUrl);
                }
            }

            private void RenderItemSeparatorImage(HtmlTextWriter writer, MenuItem item, string separatorImageUrl) {
                if (Menu.RenderingCompatibility >= VersionUtil.Framework45) {
                    // Dev10 #867750, Dev11 #436206: We need to be consistent with other controls by calling ResolveClientUrl,
                    // but we need to hide this behavior behind a compat switch so that upgrading to 4.5 doesn't break
                    // customers who have implemented their own manual workaround.
                    separatorImageUrl = Menu.ResolveClientUrl(separatorImageUrl);
                }

                writer.AddAttribute(HtmlTextWriterAttribute.Src, separatorImageUrl);
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, String.Empty);
                writer.AddAttribute(HtmlTextWriterAttribute.Class,
                                    IsDepthStatic(item) && Menu.Orientation == Orientation.Horizontal ? "horizontal-separator" : "separator");
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
            }

            private void RenderItems(HtmlTextWriter writer, bool staticOnly, MenuItemCollection items, int level, bool needsAccessKey) {
                if (level == 1 || level > Menu.StaticDisplayLevels) {  // Render a <UL> to start, and for all dynamic descendents
                    if (Menu.DesignMode && Menu.Orientation == Orientation.Horizontal) {
                        writer.AddStyleAttribute("float", "left");
                    }
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, GetMenuCssClass(level));
                    writer.RenderBeginTag(HtmlTextWriterTag.Ul);
                }

                foreach (MenuItem item in items) {
                    if (Menu.DesignMode && Menu.Orientation == Orientation.Horizontal) {
                        writer.AddStyleAttribute("float", "left");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
                    }
                    writer.RenderBeginTag(HtmlTextWriterTag.Li);

                    needsAccessKey = RenderItem(writer, item, level, GetMenuItemCssClass(item, level), needsAccessKey);

                    if (level < Menu.StaticDisplayLevels) {  // Close off <LI> if we (and our direct descendents) are static
                        writer.RenderEndTag();
                    }

                    if (item.ChildItems.Count > 0 && !IsChildPastMaximumDepth(item) && item.Enabled) {
                        if (level < Menu.StaticDisplayLevels || !staticOnly) {
                            RenderItems(writer, staticOnly, item.ChildItems, level + 1, needsAccessKey);
                        }
                    }

                    if (level >= Menu.StaticDisplayLevels) {  // Close off <LI> if we (or our direct descendents) are dynamic
                        writer.RenderEndTag();
                    }
                }

                if (level == 1 || level > Menu.StaticDisplayLevels) {
                    writer.RenderEndTag();
                }
            }

            private bool ShouldHavePopOutImage(MenuItem item) {
                return (item.ChildItems.Count > 0) && IsChildDepthDynamic(item) && !IsChildPastMaximumDepth(item);
            }
        }
    }

    internal class StyleBlock : Control {
        List<StyleBlockStyles> _styles = new List<StyleBlockStyles>();

        public StyleBlockStyles AddStyleDefinition(string selector) {
            StyleBlockStyles result = new StyleBlockStyles(selector, this);
            _styles.Add(result);
            return result;
        }

        public StyleBlockStyles AddStyleDefinition(string selectorFormat, params object[] args) {
            return AddStyleDefinition(String.Format(CultureInfo.InvariantCulture, selectorFormat, args));
        }

        protected internal override void Render(HtmlTextWriter writer) {
            if (_styles.Any(s => !s.Empty)) {
                writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                writer.RenderBeginTag(HtmlTextWriterTag.Style);
                writer.WriteLine("/* <![CDATA[ */");

                foreach (var style in _styles.Where(s => !s.Empty)) {
                    style.Render(writer);
                }

                writer.Write("/* ]]> */");
                writer.RenderEndTag();
            }
        }
    }

    internal class StyleBlockStyles {
        private string _selector;
        private StyleBlock _styleControl;
        private CssStyleCollection _styles = new CssStyleCollection();

        public StyleBlockStyles(string selector, StyleBlock styleControl) {
            _selector = selector;
            _styleControl = styleControl;
        }

        public bool Empty {
            get { return _styles.Count == 0; }
        }

        public StyleBlockStyles AddStyle(HtmlTextWriterStyle styleName, string value) {
            _styles.Add(styleName, value);
            return this;
        }

        public StyleBlockStyles AddStyle(string styleName, string value) {
            _styles.Add(styleName, value);
            return this;
        }

        public StyleBlockStyles AddStyles(Style style) {
            if (style != null) {
                AddStyles(style.GetStyleAttributes(_styleControl));
            }
            return this;
        }

        public StyleBlockStyles AddStyles(CssStyleCollection styles) {
            if (styles != null) {
                foreach (string key in styles.Keys) {
                    _styles.Add(key, styles[key]);
                }
            }
            return this;
        }

        public void Render(HtmlTextWriter writer) {
            writer.WriteLine("{0} {{ {1} }}", _selector, _styles.Value);
        }
    }
}

