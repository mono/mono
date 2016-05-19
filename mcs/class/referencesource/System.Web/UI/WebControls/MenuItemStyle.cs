//------------------------------------------------------------------------------
// <copyright file="MenuItemStyle.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Text;
    using System.Web.UI;


    /// <devdoc>
    ///    Specifies the style of a MenuItem.
    /// </devdoc>
    public sealed class MenuItemStyle : Style {
        private const int PROP_VPADDING = 0x00010000;
        private const int PROP_HPADDING = 0x00020000;
        private const int PROP_ITEMSPACING = 0x00040000;

        private HyperLinkStyle _hyperLinkStyle;

        public MenuItemStyle() : base() {
        }

        public MenuItemStyle(StateBag bag) : base(bag) {
        }

        /// <devdoc>
        /// Gets and sets the horizontal padding around the node text
        /// </devdoc>
        [
        DefaultValue(typeof(Unit), ""),
        WebCategory("Layout"),
        NotifyParentProperty(true),
        WebSysDescription(SR.MenuItemStyle_HorizontalPadding)
        ]
        public Unit HorizontalPadding {
            get {
                if (IsSet(PROP_HPADDING)) {
                    return (Unit)(ViewState["HorizontalPadding"]);
                }
                return Unit.Empty;
            }
            set {
                if ((value.Type == UnitType.Percentage) || (value.Value < 0)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["HorizontalPadding"] = value;
                SetBit(PROP_HPADDING);
            }
        }

        internal HyperLinkStyle HyperLinkStyle {
            get {
                if (_hyperLinkStyle == null) {
                    _hyperLinkStyle = new HyperLinkStyle(this);
                }

                return _hyperLinkStyle;
            }
        }


        /// <devdoc>
        /// Gets and sets the vertical spacing between nodes
        /// </devdoc>
        [
        DefaultValue(typeof(Unit), ""),
        WebCategory("Layout"),
        NotifyParentProperty(true),
        WebSysDescription(SR.MenuItemStyle_ItemSpacing)
        ]
        public Unit ItemSpacing {
            get {
                if (IsSet(PROP_ITEMSPACING)) {
                    return (Unit)(ViewState["ItemSpacing"]);
                }
                return Unit.Empty;
            }
            set {
                if ((value.Type == UnitType.Percentage) || (value.Value < 0)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["ItemSpacing"] = value;
                SetBit(PROP_ITEMSPACING);
            }
        }


        /// <devdoc>
        /// Gets and sets the vertical padding around the node text
        /// </devdoc>
        [
        DefaultValue(typeof(Unit), ""),
        WebCategory("Layout"),
        NotifyParentProperty(true),
        WebSysDescription(SR.MenuItemStyle_VerticalPadding)
        ]
        public Unit VerticalPadding {
            get {
                if (IsSet(PROP_VPADDING)) {
                    return (Unit)(ViewState["VerticalPadding"]);
                }
                return Unit.Empty;
            }
            set {
                if ((value.Type == UnitType.Percentage) || (value.Value < 0)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["VerticalPadding"] = value;
                SetBit(PROP_VPADDING);
            }
        }


        /// <devdoc>
        ///    Copies non-blank elements from the specified style, overwriting existing
        ///    style elements if necessary.
        /// </devdoc>
        public override void CopyFrom(Style s) {
            if (s != null) {
                base.CopyFrom(s);

                MenuItemStyle mis = s as MenuItemStyle;
                if (mis != null && !mis.IsEmpty) {
                    // Only copy the Css attributes if they aren't included in the source Style's registered CSS class
                    if (s.RegisteredCssClass.Length != 0) {
                        if (mis.IsSet(PROP_VPADDING)) {
                            ViewState.Remove("VerticalPadding");
                            ClearBit(PROP_VPADDING);
                        }

                        if (mis.IsSet(PROP_HPADDING)) {
                            ViewState.Remove("HorizontalPadding");
                            ClearBit(PROP_HPADDING);
                        }
                    }
                    else {
                        if (mis.IsSet(PROP_VPADDING)) {
                            this.VerticalPadding = mis.VerticalPadding;
                        }

                        if (mis.IsSet(PROP_HPADDING)) {
                            this.HorizontalPadding = mis.HorizontalPadding;
                        }
                    }

                    // Item spacing is not rendered in the registered css, so we always copy it
                    if (mis.IsSet(PROP_ITEMSPACING)) {
                        this.ItemSpacing = mis.ItemSpacing;
                    }
                }
            }
        }


        protected override void FillStyleAttributes(CssStyleCollection attributes, IUrlResolutionService urlResolver) {
            // The main style will be rendered on the container element, that does not contain the text.
            // The Hyperlink style will render the text styles
            // Copying the code from the base class, except for the part that deals with Font and ForeColor.
            StateBag viewState = ViewState;
            Color c;

            // BackColor
            if (IsSet(PROP_BACKCOLOR)) {
                c = (Color)viewState["BackColor"];
                if (!c.IsEmpty) {
                    attributes.Add(HtmlTextWriterStyle.BackgroundColor, ColorTranslator.ToHtml(c));
                }
            }

            // BorderColor
            if (IsSet(PROP_BORDERCOLOR)) {
                c = (Color)viewState["BorderColor"];
                if (!c.IsEmpty) {
                    attributes.Add(HtmlTextWriterStyle.BorderColor, ColorTranslator.ToHtml(c));
                }
            }

            BorderStyle bs = this.BorderStyle;
            Unit bu = this.BorderWidth;
            if (!bu.IsEmpty) {
                attributes.Add(HtmlTextWriterStyle.BorderWidth, bu.ToString(CultureInfo.InvariantCulture));
                if (bs == BorderStyle.NotSet) {
                    if (bu.Value != 0.0) {
                        attributes.Add(HtmlTextWriterStyle.BorderStyle, "solid");
                    }
                }
                else {
                    attributes.Add(HtmlTextWriterStyle.BorderStyle, borderStyles[(int)bs]);
                }
            }
            else {
                if (bs != BorderStyle.NotSet) {
                    attributes.Add(HtmlTextWriterStyle.BorderStyle, borderStyles[(int)bs]);
                }
            }

            Unit u;

            // Height
            if (IsSet(PROP_HEIGHT)) {
                u = (Unit)viewState["Height"];
                if (!u.IsEmpty) {
                    attributes.Add(HtmlTextWriterStyle.Height, u.ToString(CultureInfo.InvariantCulture));
                }
            }

            // Width
            if (IsSet(PROP_WIDTH)) {
                u = (Unit)viewState["Width"];
                if (!u.IsEmpty) {
                    attributes.Add(HtmlTextWriterStyle.Width, u.ToString(CultureInfo.InvariantCulture));
                }
            }

            if (!HorizontalPadding.IsEmpty || !VerticalPadding.IsEmpty) {
                // DevDiv Bugs 84107: Must pass InvariantCulture to Unit.ToString since Unit is not IFormattable.
                Unit verticalPadding = VerticalPadding.IsEmpty ? Unit.Pixel(0) : VerticalPadding;
                Unit horizontalPadding = HorizontalPadding.IsEmpty ? Unit.Pixel(0) : HorizontalPadding;
                attributes.Add(HtmlTextWriterStyle.Padding, string.Format(CultureInfo.InvariantCulture,
                    "{0} {1} {0} {1}",
                    verticalPadding.ToString(CultureInfo.InvariantCulture),
                    horizontalPadding.ToString(CultureInfo.InvariantCulture)));
            }
        }

        public override void MergeWith(Style s) {
            if (s != null) {
                if (IsEmpty) {
                    // Merging with an empty style is equivalent to copying,
                    // which is more efficient.
                    CopyFrom(s);
                    return;
                }

                base.MergeWith(s);

                MenuItemStyle mis = s as MenuItemStyle;
                if (mis != null && !mis.IsEmpty) {
                    // Since we're already copying the registered CSS class in base.MergeWith, we don't
                    // need to any attributes that would be included in that class.
                    if (s.RegisteredCssClass.Length == 0) {
                        if (mis.IsSet(PROP_VPADDING) && !this.IsSet(PROP_VPADDING)) {
                            this.VerticalPadding = mis.VerticalPadding;
                        }

                        if (mis.IsSet(PROP_HPADDING) && !this.IsSet(PROP_HPADDING)) {
                            this.HorizontalPadding = mis.HorizontalPadding;
                        }
                    }

                    if (mis.IsSet(PROP_ITEMSPACING) && !this.IsSet(PROP_ITEMSPACING)) {
                        this.ItemSpacing = mis.ItemSpacing;
                    }
                }
            }
        }


        /// <devdoc>
        ///    Clears out any defined style elements from the state bag.
        /// </devdoc>
        public override void Reset() {
            if (IsSet(PROP_VPADDING))
                ViewState.Remove("VerticalPadding");
            if (IsSet(PROP_HPADDING))
                ViewState.Remove("HorizontalPadding");
            if (IsSet(PROP_ITEMSPACING))
                ViewState.Remove("ItemSpacing");
            ResetCachedStyles();

            base.Reset();
        }

        internal void ResetCachedStyles() {
            _hyperLinkStyle = null;
        }
    }
}
