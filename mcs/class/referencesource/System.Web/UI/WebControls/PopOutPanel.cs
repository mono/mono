//------------------------------------------------------------------------------
// <copyright file="PopOutPanel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.ComponentModel;

    /// <devdoc>
    ///    Constructs a panel with automatic scrolling
    ///     and relative positioning for use with the Menu control.
    /// </devdoc>
    internal sealed class PopOutPanel : Panel {

        private Menu _owner;
        private String _scrollerClass;
        private Style _scrollerStyle;
        private Style _style;
        private PopOutPanelStyle _emptyPopOutPanelStyle;

        public PopOutPanel(Menu owner, Style style) {
            _owner = owner;
            _style = style;
            _emptyPopOutPanelStyle = new PopOutPanelStyle(null);
        }

        public override ScrollBars ScrollBars {
            get {
                return ScrollBars.None;
            }
        }

        internal String ScrollerClass {
            get {
                return _scrollerClass;
            }
            set {
                _scrollerClass = value;
            }
        }

        internal Style ScrollerStyle {
            get {
                return _scrollerStyle;
            }
            set {
                _scrollerStyle = value;
            }
        }

        /// <devdoc>
        ///    Add positioning custom attributes.
        /// </devdoc>
        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            string oldCssClass = CssClass;
            Style oldStyle = _style;
            CssClass = String.Empty;
            _style = null;
            ControlStyle.Reset();

            base.AddAttributesToRender(writer);

            CssClass = oldCssClass;
            _style = oldStyle;

            // Non-configurable style attributes
            RenderStyleAttributes(writer);
        }

        internal PopOutPanelStyle GetEmptyPopOutPanelStyle() {
            return _emptyPopOutPanelStyle;
        }

        public override void RenderEndTag(HtmlTextWriter writer) {
            if (!_owner.DesignMode) {
                RenderScrollerAttributes(writer);
                writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID + "Up");
                writer.AddAttribute("onmouseover", "PopOut_Up(this)");
                writer.AddAttribute("onmouseout", "PopOut_Stop(this)");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                string scrollUpImageUrl = _owner.ScrollUpImageUrl;
                if (scrollUpImageUrl.Length != 0) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, _owner.ResolveClientUrl(scrollUpImageUrl));
                }
                else {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, _owner.GetImageUrl(Menu.ScrollUpImageIndex));
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, _owner.ScrollUpText);
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
                writer.RenderEndTag();
                RenderScrollerAttributes(writer);
                writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID + "Dn");
                writer.AddAttribute("onmouseover", "PopOut_Down(this)");
                writer.AddAttribute("onmouseout", "PopOut_Stop(this)");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                string scrollDownImageUrl = _owner.ScrollDownImageUrl;
                if (scrollDownImageUrl.Length != 0) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, _owner.ResolveClientUrl(scrollDownImageUrl));
                }
                else {
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, _owner.GetImageUrl(Menu.ScrollDownImageIndex));
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Alt, _owner.ScrollDownText);
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
                writer.RenderEndTag();
            }
            base.RenderEndTag(writer);
        }

        private void RenderScrollerAttributes(HtmlTextWriter writer) {
            if ((Page != null) && Page.SupportsStyleSheets) {
                if (!String.IsNullOrEmpty(ScrollerClass)) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, ScrollerClass + ' ' +
                        GetEmptyPopOutPanelStyle().RegisteredCssClass);
                }
                else {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, GetEmptyPopOutPanelStyle().RegisteredCssClass);
                }
            }
            else {
                if ((ScrollerStyle != null) && !ScrollerStyle.IsEmpty) {
                    ScrollerStyle.AddAttributesToRender(writer);
                }
                if (ScrollerStyle.BackColor.IsEmpty) {
                    writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, "white");
                }
                writer.AddStyleAttribute(HtmlTextWriterStyle.Visibility, "hidden");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "absolute");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Left, "0px");
                writer.AddStyleAttribute(HtmlTextWriterStyle.Top, "0px");
            }
            writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "center");
        }

        private void RenderStyleAttributes(HtmlTextWriter writer) {
            if (_style == null) {
                if (!String.IsNullOrEmpty(CssClass)) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClass);
                }
                else {
                    if (BackColor.IsEmpty) {
                        writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, "white");
                    }
                    else {
                        writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, ColorTranslator.ToHtml(BackColor));
                    }
                    if (!_owner.DesignMode) {
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Visibility, "hidden");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Display, "none");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "absolute");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Left, "0px");
                        writer.AddStyleAttribute(HtmlTextWriterStyle.Top, "0px");
                    }
                }
            }
            else {
                if ((Page != null) && Page.SupportsStyleSheets) {
                    string styleClass = _style.RegisteredCssClass;
                    if (styleClass.Trim().Length > 0) {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class,
                            (!String.IsNullOrEmpty(CssClass)) ?
                            styleClass + ' ' + CssClass:
                            styleClass);
                        return;
                    }
                }
                if (!String.IsNullOrEmpty(CssClass)) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClass);
                }
                else {
                    _style.AddAttributesToRender(writer);
                }
            }
        }

        internal void SetInternalStyle(Style style) {
            _style = style;
        }

        internal sealed class PopOutPanelStyle : SubMenuStyle {
            private PopOutPanel _owner;

            public PopOutPanelStyle(PopOutPanel owner) {
                _owner = owner;
            }

            protected override void FillStyleAttributes(CssStyleCollection attributes, IUrlResolutionService urlResolver) {
                if (BackColor.IsEmpty && ((_owner == null) || _owner.BackColor.IsEmpty)) {
                    attributes.Add(HtmlTextWriterStyle.BackgroundColor, "white");
                }
                attributes.Add(HtmlTextWriterStyle.Visibility, "hidden");
                attributes.Add(HtmlTextWriterStyle.Display, "none");
                attributes.Add(HtmlTextWriterStyle.Position, "absolute");
                attributes.Add(HtmlTextWriterStyle.Left, "0px");
                attributes.Add(HtmlTextWriterStyle.Top, "0px");
                base.FillStyleAttributes(attributes, urlResolver);
            }
        }
    }
}
