//------------------------------------------------------------------------------
// <copyright file="Table.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web;
    using System.Web.Util;

    /// <devdoc>
    ///    <para>Constructs a table and defines its properties.</para>
    /// </devdoc>
    [
    DefaultProperty("Rows"),
    ParseChildren(true, "Rows"),
    Designer("System.Web.UI.Design.WebControls.TableDesigner, " + AssemblyRef.SystemDesign),
    SupportsEventValidation,
    ]
    public class Table : WebControl, IPostBackEventHandler {
        private TableRowCollection _rows;
        private bool _hasRowSections;


        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.UI.WebControls.Table'/> class.
        ///    </para>
        /// </devdoc>
        public Table() : base(HtmlTextWriterTag.Table) {
        }



        /// <devdoc>
        ///    <para>Indicates the URL of the background image to display
        ///       behind the table. The image will be tiled if it is smaller than the table.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        Editor("System.Web.UI.Design.ImageUrlEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        UrlProperty(),
        WebSysDescription(SR.Table_BackImageUrl)
        ]
        public virtual string BackImageUrl {
            get {
                if (ControlStyleCreated == false) {
                    return String.Empty;
                }
                return((TableStyle)ControlStyle).BackImageUrl;
            }
            set {
                ((TableStyle)ControlStyle).BackImageUrl = value;
            }
        }


        [
        DefaultValue(""),
        Localizable(true),
        WebCategory("Accessibility"),
        WebSysDescription(SR.Table_Caption)
        ]
        public virtual string Caption {
            get {
                string s = (string)ViewState["Caption"];
                return (s != null) ? s : String.Empty;
            }
            set {
                ViewState["Caption"] = value;
            }
        }


        [
        DefaultValue(TableCaptionAlign.NotSet),
        WebCategory("Accessibility"),
        WebSysDescription(SR.WebControl_CaptionAlign)
        ]
        public virtual TableCaptionAlign CaptionAlign {
            get {
                object o = ViewState["CaptionAlign"];
                return (o != null) ? (TableCaptionAlign)o : TableCaptionAlign.NotSet;
            }
            set {
                if ((value < TableCaptionAlign.NotSet) ||
                    (value > TableCaptionAlign.Right)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["CaptionAlign"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets
        ///       the distance (in pixels) between the border and
        ///       the contents of the table cell.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(-1),
        WebSysDescription(SR.Table_CellPadding)
        ]
        public virtual int CellPadding {
            get {
                if (ControlStyleCreated == false) {
                    return -1;
                }
                return((TableStyle)ControlStyle).CellPadding;
            }
            set {
                ((TableStyle)ControlStyle).CellPadding = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or
        ///       sets
        ///       the distance (in pixels) between table cells.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(-1),
        WebSysDescription(SR.Table_CellSpacing)
        ]
        public virtual int CellSpacing {
            get {
                if (ControlStyleCreated == false) {
                    return -1;
                }
                return((TableStyle)ControlStyle).CellSpacing;
            }
            set {
                ((TableStyle)ControlStyle).CellSpacing = value;
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the gridlines property of the <see cref='System.Web.UI.WebControls.Table'/>
        /// class.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(GridLines.None),
        WebSysDescription(SR.Table_GridLines)
        ]
        public virtual GridLines GridLines {
            get {
                if (ControlStyleCreated == false) {
                    return GridLines.None;
                }
                return((TableStyle)ControlStyle).GridLines;
            }
            set {
                ((TableStyle)ControlStyle).GridLines = value;
            }
        }

        internal bool HasRowSections {
            get {
                return _hasRowSections;
            }
            set {
                _hasRowSections = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the horizontal alignment of the table within the page.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(HorizontalAlign.NotSet),
        WebSysDescription(SR.Table_HorizontalAlign)
        ]
        public virtual HorizontalAlign HorizontalAlign {
            get {
                if (ControlStyleCreated == false) {
                    return HorizontalAlign.NotSet;
                }
                return((TableStyle)ControlStyle).HorizontalAlign;
            }
            set {
                ((TableStyle)ControlStyle).HorizontalAlign = value;
            }
        }

        public override bool SupportsDisabledAttribute {
            get {
                return RenderingCompatibility < VersionUtil.Framework40;
            }
        }

        /// <devdoc>
        ///    <para> Gets the collection of rows within
        ///       the table.</para>
        /// </devdoc>
        [
        MergableProperty(false),
        WebSysDescription(SR.Table_Rows),
        PersistenceMode(PersistenceMode.InnerDefaultProperty)
        ]
        public virtual TableRowCollection Rows {
            get {
                if (_rows == null)
                    _rows = new TableRowCollection(this);
                return _rows;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>A protected method. Adds information about the border
        ///       color and border width HTML attributes to the list of attributes to render.</para>
        /// </devdoc>
        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            base.AddAttributesToRender(writer);
            if (ControlStyleCreated) {
                if (EnableLegacyRendering || writer is Html32TextWriter) {
                    // Must render bordercolor attribute to affect cell borders.
                    Color borderColor = BorderColor;
                    if (!borderColor.IsEmpty) {
                        writer.AddAttribute(HtmlTextWriterAttribute.Bordercolor, ColorTranslator.ToHtml(borderColor));
                    }
                }
            }

            string borderWidthString = "0";
            bool renderBorder = false;
            if (ControlStyleCreated) {
                // GridLines property controls whether we render the "border" attribute, as "border" controls
                // whether gridlines appear in HTML 3.2. Always render a value for the border attribute.
                Unit borderWidth = BorderWidth;
                GridLines gridLines = GridLines;
                if (gridLines != GridLines.None) {
                    if (borderWidth.IsEmpty || borderWidth.Type != UnitType.Pixel) {
                        borderWidthString = "1";
                        renderBorder = true;
                    }
                    else {
                        borderWidthString = ((int)borderWidth.Value).ToString(NumberFormatInfo.InvariantInfo);
                    }
                }
            }
            if ((RenderingCompatibility < VersionUtil.Framework40) || renderBorder) {
                writer.AddAttribute(HtmlTextWriterAttribute.Border, borderWidthString);
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override ControlCollection CreateControlCollection() {
            return new RowControlCollection(this);
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>A protected method. Creates a table control style.</para>
        /// </devdoc>
        protected override Style CreateControlStyle() {
            return new TableStyle(ViewState);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual void RaisePostBackEvent(string argument) {
            ValidateEvent(UniqueID, argument);

            if (AdapterInternal != null) {
                IPostBackEventHandler pbeh = AdapterInternal as IPostBackEventHandler;
                if (pbeh != null) {
                    pbeh.RaisePostBackEvent(argument);
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///   Renders out the caption of the table if needed, before any rows get rendered.
        /// </devdoc>
        public override void RenderBeginTag(HtmlTextWriter writer) {
            base.RenderBeginTag(writer);

            string caption = Caption;
            if (caption.Length != 0) {
                TableCaptionAlign alignment = CaptionAlign;

                if (alignment != TableCaptionAlign.NotSet) {
                    string alignValue = "Right";

                    switch (alignment) {
                        case TableCaptionAlign.Top:
                            alignValue = "Top";
                            break;
                        case TableCaptionAlign.Bottom:
                            alignValue = "Bottom";
                            break;
                        case TableCaptionAlign.Left:
                            alignValue = "Left";
                            break;
                    }
                    writer.AddAttribute(HtmlTextWriterAttribute.Align, alignValue);
                }
                writer.RenderBeginTag(HtmlTextWriterTag.Caption);
                writer.Write(caption);
                writer.RenderEndTag();
            }
        }


        /// <devdoc>
        ///    Render the table rows.
        /// </devdoc>
        protected internal override void RenderContents(HtmlTextWriter writer) {
            TableRowCollection rows = Rows;
            int rowCount = rows.Count;
            if (rowCount > 0) {
                if (HasRowSections) {
                    TableRowSection currentSection = TableRowSection.TableHeader;
                    bool openedTag = false;
                    foreach (TableRow row in rows) {
                        if (row.TableSection < currentSection) {
                            // throw if table sections aren't in order
                            throw new HttpException(SR.GetString(SR.Table_SectionsMustBeInOrder, ID));
                        }
                        if (currentSection < row.TableSection || (row.TableSection == TableRowSection.TableHeader && !openedTag)) {
                            if (openedTag) {
                                writer.RenderEndTag();
                            }
                            currentSection = row.TableSection;
                            openedTag = true;
                            switch (currentSection) {
                                case TableRowSection.TableHeader:
                                    writer.RenderBeginTag(HtmlTextWriterTag.Thead);
                                    break;
                                case TableRowSection.TableBody:
                                    writer.RenderBeginTag(HtmlTextWriterTag.Tbody);
                                    break;
                                case TableRowSection.TableFooter:
                                    writer.RenderBeginTag(HtmlTextWriterTag.Tfoot);
                                    break;
                            }
                        }
                        row.RenderControl(writer);
                    }
                    writer.RenderEndTag();
                }
                else {
                    foreach (TableRow row in rows) {
                        row.RenderControl(writer);
                    }
                }
            }
        }

        void IPostBackEventHandler.RaisePostBackEvent(string eventArgument) {
            RaisePostBackEvent(eventArgument);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected class RowControlCollection : ControlCollection {

            internal RowControlCollection (Control owner) : base(owner) {
            }


            /// <devdoc>
            /// <para>Adds the specified <see cref='System.Web.UI.Control'/> object to the collection. The new control is added
            ///    to the end of the array.</para>
            /// </devdoc>
            public override void Add(Control child) {
                if (child is TableRow)
                    base.Add(child);
                else
                    throw new ArgumentException(SR.GetString(SR.Cannot_Have_Children_Of_Type, "Table", child.GetType().Name.ToString(CultureInfo.InvariantCulture))); // throw an exception here
            }


            /// <devdoc>
            /// <para>Adds the specified <see cref='System.Web.UI.Control'/> object to the collection. The new control is added
            ///    to the array at the specified index location.</para>
            /// </devdoc>
            public override void AddAt(int index, Control child) {
                if (child is TableRow)
                    base.AddAt(index, child);
                else
                    throw new ArgumentException(SR.GetString(SR.Cannot_Have_Children_Of_Type, "Table", child.GetType().Name.ToString(CultureInfo.InvariantCulture))); // throw an exception here
            }
        } // class RowControlCollection
    } // class Table
}

