//------------------------------------------------------------------------------
// <copyright file="TableStyle.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Web;
    using System.Web.UI;

    /// <devdoc>
    ///    <para>Specifies the style of the table.</para>
    /// </devdoc>
    public class TableStyle : Style {

        // The following are marked internal so that WebPartMenuStyle (or other derived types) can access IsSet on them
        internal const int PROP_BACKIMAGEURL = 0x00010000;
        internal const int PROP_CELLPADDING = 0x00020000;
        internal const int PROP_CELLSPACING = 0x00040000;
        internal const int PROP_GRIDLINES = 0x00080000;
        internal const int PROP_HORZALIGN = 0x00100000;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.TableStyle'/> class.</para>
        /// </devdoc>
        public TableStyle() : base() {
        }


        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.UI.WebControls.TableStyle'/> class with the
        ///       specified state bag information.
        ///    </para>
        /// </devdoc>
        public TableStyle(StateBag bag) : base(bag) {
        }


        /// <devdoc>
        ///    <para>Gets or sets the URL of the background image for the
        ///       table. The image will be tiled if it is smaller than the table.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        UrlProperty(),
        WebSysDescription(SR.TableStyle_BackImageUrl),
        NotifyParentProperty(true)
        ]
        public virtual string BackImageUrl {
            get {
                if (IsSet(PROP_BACKIMAGEURL)) {
                    return(string)(ViewState["BackImageUrl"]);
                }
                return String.Empty;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                ViewState["BackImageUrl"] = value;
                SetBit(PROP_BACKIMAGEURL);
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the distance between the border and the
        ///       contents of the table cell.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(-1),
        WebSysDescription(SR.TableStyle_CellPadding),
        NotifyParentProperty(true)
        ]
        public virtual int CellPadding {
            get {
                if (IsSet(PROP_CELLPADDING)) {
                    return(int)(ViewState["CellPadding"]);
                }
                return -1;
            }
            set {
                if (value < -1) {
                    throw new ArgumentOutOfRangeException("value", SR.GetString(SR.TableStyle_InvalidCellPadding));
                }
                ViewState["CellPadding"] = value;
                SetBit(PROP_CELLPADDING);
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the distance between table cells.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(-1),
        WebSysDescription(SR.TableStyle_CellSpacing),
        NotifyParentProperty(true)
        ]
        public virtual int CellSpacing {
            get {
                if (IsSet(PROP_CELLSPACING)) {
                    return(int)(ViewState["CellSpacing"]);
                }
                return -1;
            }
            set {
                if (value < -1) {
                    throw new ArgumentOutOfRangeException("value", SR.GetString(SR.TableStyle_InvalidCellSpacing));
                }
                ViewState["CellSpacing"] = value;
                SetBit(PROP_CELLSPACING);
            }
        }


        /// <devdoc>
        ///    Gets or sets the gridlines property of the table.
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(GridLines.None),
        WebSysDescription(SR.TableStyle_GridLines),
        NotifyParentProperty(true)
        ]
        public virtual GridLines GridLines {
            get {
                if (IsSet(PROP_GRIDLINES)) {
                    return(GridLines)(ViewState["GridLines"]);
                }
                return GridLines.None;
            }
            set {
                if (value < GridLines.None || value > GridLines.Both) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["GridLines"] = value;
                SetBit(PROP_GRIDLINES);
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the horizontal alignment of the table within the page.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(HorizontalAlign.NotSet),
        WebSysDescription(SR.TableStyle_HorizontalAlign),
        NotifyParentProperty(true)
        ]
        public virtual HorizontalAlign HorizontalAlign {
            get {
                if (IsSet(PROP_HORZALIGN)) {
                    return(HorizontalAlign)(ViewState["HorizontalAlign"]);
                }
                return HorizontalAlign.NotSet;
            }
            set {
                if (value < HorizontalAlign.NotSet || value > HorizontalAlign.Justify) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["HorizontalAlign"] = value;
                SetBit(PROP_HORZALIGN);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para> Adds information about the background
        ///       image, callspacing, cellpadding, gridlines, and alignment to the list of attributes
        ///       to render.</para>
        /// </devdoc>
        public override void AddAttributesToRender(HtmlTextWriter writer, WebControl owner) {
            base.AddAttributesToRender(writer, owner);

            int n = CellSpacing;
            if (n >= 0) {
                writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, n.ToString(NumberFormatInfo.InvariantInfo));
                if (n == 0) {
                    writer.AddStyleAttribute(HtmlTextWriterStyle.BorderCollapse, "collapse");
                }
            }

            n = CellPadding;
            if (n >= 0) {
                writer.AddAttribute(HtmlTextWriterAttribute.Cellpadding, n.ToString(NumberFormatInfo.InvariantInfo));
            }

            HorizontalAlign align = HorizontalAlign;
            if (align != HorizontalAlign.NotSet) {
                string alignValue = "Justify";

                switch (align) {
                    case HorizontalAlign.Left:
                        alignValue = "Left";
                        break;
                    case HorizontalAlign.Center:
                        alignValue = "Center";
                        break;
                    case HorizontalAlign.Right:
                        alignValue = "Right";
                        break;
                    case HorizontalAlign.Justify:
                        alignValue = "Justify";
                        break;
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Align, alignValue);
            }

            GridLines gridLines = GridLines;
            if (gridLines != GridLines.None) {
                string rules = String.Empty;
                switch (GridLines) {
                    case GridLines.Both:
                        rules = "all";
                        break;
                    case GridLines.Horizontal:
                        rules = "rows";
                        break;
                    case GridLines.Vertical:
                        rules = "cols";
                        break;
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Rules, rules);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Copies non-blank elements from the specified style, overwriting existing
        ///       style elements if necessary.</para>
        /// </devdoc>
        public override void CopyFrom(Style s) {
            if (s != null && !s.IsEmpty) {
                base.CopyFrom(s);

                TableStyle ts = s as TableStyle;
                if (ts != null) {

                    // Only copy the BackImageUrl if it isn't in the source Style's registered CSS class
                    if (s.RegisteredCssClass.Length != 0) {
                        if (ts.IsSet(PROP_BACKIMAGEURL)) {
                            ViewState.Remove("BackImageUrl");
                            ClearBit(PROP_BACKIMAGEURL);
                        }
                    }
                    else {
                        if (ts.IsSet(PROP_BACKIMAGEURL))
                            this.BackImageUrl = ts.BackImageUrl;
                    }

                    if (ts.IsSet(PROP_CELLPADDING))
                        this.CellPadding = ts.CellPadding;
                    if (ts.IsSet(PROP_CELLSPACING))
                        this.CellSpacing = ts.CellSpacing;
                    if (ts.IsSet(PROP_GRIDLINES))
                        this.GridLines = ts.GridLines;
                    if (ts.IsSet(PROP_HORZALIGN))
                        this.HorizontalAlign = ts.HorizontalAlign;
                }
            }
        }


        /// <internalonly/>
        protected override void FillStyleAttributes(CssStyleCollection attributes, IUrlResolutionService urlResolver) {
            base.FillStyleAttributes(attributes, urlResolver);

            string s = BackImageUrl;
            if (s.Length != 0) {
                if (urlResolver != null) {
                    s = urlResolver.ResolveClientUrl(s);
                }
                attributes.Add(HtmlTextWriterStyle.BackgroundImage, s);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Copies non-blank elements from the specified style, but will not overwrite
        ///       any existing style elements.</para>
        /// </devdoc>
        public override void MergeWith(Style s) {
            if (s != null && !s.IsEmpty) {
                if (IsEmpty) {
                    // merge into an empty style is equivalent to a copy,
                    // which is more efficient
                    CopyFrom(s);
                    return;
                }

                base.MergeWith(s);

                TableStyle ts = s as TableStyle;
                if (ts != null) {

                    // Since we're already copying the registered CSS class in base.MergeWith, we don't
                    // need to any attributes that would be included in that class.
                    if (s.RegisteredCssClass.Length == 0) {
                        if (ts.IsSet(PROP_BACKIMAGEURL) && !this.IsSet(PROP_BACKIMAGEURL))
                            this.BackImageUrl = ts.BackImageUrl;
                    }

                    if (ts.IsSet(PROP_CELLPADDING) && !this.IsSet(PROP_CELLPADDING))
                        this.CellPadding = ts.CellPadding;
                    if (ts.IsSet(PROP_CELLSPACING) && !this.IsSet(PROP_CELLSPACING))
                        this.CellSpacing = ts.CellSpacing;
                    if (ts.IsSet(PROP_GRIDLINES) && !this.IsSet(PROP_GRIDLINES))
                        this.GridLines = ts.GridLines;
                    if (ts.IsSet(PROP_HORZALIGN) && !this.IsSet(PROP_HORZALIGN))
                        this.HorizontalAlign = ts.HorizontalAlign;
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Clears out any defined style elements from the state bag.</para>
        /// </devdoc>
        public override void Reset() {
            if (IsSet(PROP_BACKIMAGEURL))
                ViewState.Remove("BackImageUrl");
            if (IsSet(PROP_CELLPADDING))
                ViewState.Remove("CellPadding");
            if (IsSet(PROP_CELLSPACING))
                ViewState.Remove("CellSpacing");
            if (IsSet(PROP_GRIDLINES))
                ViewState.Remove("GridLines");
            if (IsSet(PROP_HORZALIGN))
                ViewState.Remove("HorizontalAlign");

            base.Reset();
        }
    }
}
