//------------------------------------------------------------------------------
// <copyright file="TreeNode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Text;
    using System.Web.UI;

    /// <devdoc>
    ///    Specifies the style of a TreeNode.
    /// </devdoc>
    public sealed class TreeNodeStyle : Style {
        private const int PROP_VPADDING = 0x00010000;
        private const int PROP_HPADDING = 0x00020000;
        private const int PROP_NODESPACING = 0x00040000;
        private const int PROP_CHILDNODESPADDING = 0x00080000;
        private const int PROP_IMAGEURL = 0x00100000;

        private HyperLinkStyle _hyperLinkStyle;

        public TreeNodeStyle() : base() {
        }

        public TreeNodeStyle(StateBag bag) : base(bag) {
        }

        /// <devdoc>
        /// Gets and sets the vertical spacing between the node and its child nodes
        /// </devdoc>
        [
        DefaultValue(typeof(Unit), ""),
        WebSysDescription(SR.TreeNodeStyle_ChildNodesPadding),
        WebCategory("Layout"),
        NotifyParentProperty(true)
        ]
        public Unit ChildNodesPadding {
            get {
                if (IsSet(PROP_CHILDNODESPADDING)) {
                    return (Unit)(ViewState["ChildNodesPadding"]);
                }
                return Unit.Empty;
            }
            set {
                if ((value.Type == UnitType.Percentage) || (value.Value < 0)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["ChildNodesPadding"] = value;
                SetBit(PROP_CHILDNODESPADDING);
            }
        }

        /// <devdoc>
        /// Gets and sets the horizontal padding around the node text
        /// </devdoc>
        [
        DefaultValue(typeof(Unit), ""),
        WebSysDescription(SR.TreeNodeStyle_HorizontalPadding),
        WebCategory("Layout"),
        NotifyParentProperty(true)
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

        [DefaultValue("")]
        [Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
        [NotifyParentProperty(true)]
        [UrlProperty()]
        [WebCategory("Appearance")]
        [WebSysDescription(SR.TreeNodeStyle_ImageUrl)]
        public string ImageUrl {
            get {
                if (IsSet(PROP_IMAGEURL)) {
                    return (string)(ViewState["ImageUrl"]);
                }
                return String.Empty;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                ViewState["ImageUrl"] = value;
                SetBit(PROP_IMAGEURL);
            }
        }

        /// <devdoc>
        /// Gets and sets the vertical spacing between nodes
        /// </devdoc>
        [
        DefaultValue(typeof(Unit), ""),
        WebSysDescription(SR.TreeNodeStyle_NodeSpacing),
        WebCategory("Layout"),
        NotifyParentProperty(true)
        ]
        public Unit NodeSpacing {
            get {
                if (IsSet(PROP_NODESPACING)) {
                    return (Unit)(ViewState["NodeSpacing"]);
                }
                return Unit.Empty;
            }
            set {
                if ((value.Type == UnitType.Percentage) || (value.Value < 0)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["NodeSpacing"] = value;
                SetBit(PROP_NODESPACING);
            }
        }

        /// <devdoc>
        /// Gets and sets the vertical padding around the node text
        /// </devdoc>
        [
        DefaultValue(typeof(Unit), ""),
        WebSysDescription(SR.TreeNodeStyle_VerticalPadding),
        WebCategory("Layout"),
        NotifyParentProperty(true)
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

                TreeNodeStyle tns = s as TreeNodeStyle;
                if (tns != null && !tns.IsEmpty) {
                    // Only copy the paddings if they aren't in the source Style's registered CSS class
                    if (s.RegisteredCssClass.Length != 0) {
                        if (tns.IsSet(PROP_VPADDING)) {
                            ViewState.Remove("VerticalPadding");
                            ClearBit(PROP_VPADDING);
                        }

                        if (tns.IsSet(PROP_HPADDING)) {
                            ViewState.Remove("HorizontalPadding");
                            ClearBit(PROP_HPADDING);
                        }
                    }
                    else {
                        if (tns.IsSet(PROP_VPADDING)) {
                            this.VerticalPadding = tns.VerticalPadding;
                        }

                        if (tns.IsSet(PROP_HPADDING)) {
                            this.HorizontalPadding = tns.HorizontalPadding;
                        }
                    }

                    if (tns.IsSet(PROP_NODESPACING)) {
                        this.NodeSpacing = tns.NodeSpacing;
                    }

                    if (tns.IsSet(PROP_CHILDNODESPADDING)) {
                        this.ChildNodesPadding = tns.ChildNodesPadding;
                    }

                    if (tns.IsSet(PROP_IMAGEURL)) {
                        this.ImageUrl = tns.ImageUrl;
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
                attributes.Add(HtmlTextWriterStyle.Padding, string.Format(CultureInfo.InvariantCulture,
                    "{0} {1} {0} {1}",
                    VerticalPadding.IsEmpty ? Unit.Pixel(0) : VerticalPadding,
                    HorizontalPadding.IsEmpty ? Unit.Pixel(0) : HorizontalPadding));
            }
        }


        /// <devdoc>
        ///    Copies non-blank elements from the specified style, but will not overwrite
        ///    any existing style elements.
        /// </devdoc>
        public override void MergeWith(Style s) {
            if (s != null) {
                if (IsEmpty) {
                    // Merging with an empty style is equivalent to copying,
                    // which is more efficient.
                    CopyFrom(s);
                    return;
                }

                base.MergeWith(s);

                TreeNodeStyle tns = s as TreeNodeStyle;
                if (tns != null && !tns.IsEmpty) {
                    // Since we're already copying the registered CSS class in base.MergeWith, we don't
                    // need to any attributes that would be included in that class.
                    if (s.RegisteredCssClass.Length == 0) {
                        if (tns.IsSet(PROP_VPADDING) && !this.IsSet(PROP_VPADDING)) {
                            this.VerticalPadding = tns.VerticalPadding;
                        }

                        if (tns.IsSet(PROP_HPADDING) && !this.IsSet(PROP_HPADDING)) {
                            this.HorizontalPadding = tns.HorizontalPadding;
                        }
                    }

                    if (tns.IsSet(PROP_NODESPACING) && !this.IsSet(PROP_NODESPACING)) {
                        this.NodeSpacing = tns.NodeSpacing;
                    }

                    if (tns.IsSet(PROP_CHILDNODESPADDING) && !this.IsSet(PROP_CHILDNODESPADDING)) {
                        this.ChildNodesPadding = tns.ChildNodesPadding;
                    }

                    if (tns.IsSet(PROP_IMAGEURL) && !this.IsSet(PROP_IMAGEURL)) {
                        this.ImageUrl = tns.ImageUrl;
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
            if (IsSet(PROP_NODESPACING))
                ViewState.Remove("NodeSpacing");
            if (IsSet(PROP_CHILDNODESPADDING))
                ViewState.Remove("ChildNodesPadding");
            ResetCachedStyles();

            base.Reset();
        }

        internal void ResetCachedStyles() {
            _hyperLinkStyle = null;
        }
    }
}
