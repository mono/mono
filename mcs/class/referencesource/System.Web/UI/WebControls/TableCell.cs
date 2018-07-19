//------------------------------------------------------------------------------
// <copyright file="TableCell.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;


    /// <devdoc>
    /// <para>Interacts with the parser to build a <see cref='System.Web.UI.WebControls.TableCell'/> control.</para>
    /// </devdoc>
    public class TableCellControlBuilder : ControlBuilder {


        /// <internalonly/>
        /// <devdoc>
        ///    Specifies whether white space literals are allowed.
        /// </devdoc>
        public override bool AllowWhitespaceLiterals() {
            return false;
        }
    }



    /// <devdoc>
    ///    <para>Encapsulates a cell within a table.</para>
    /// </devdoc>
    [
    Bindable(false),
    ControlBuilderAttribute(typeof(TableCellControlBuilder)),
    DefaultProperty("Text"),
    ParseChildren(false),
    ToolboxItem(false)
    ]
    [Designer("System.Web.UI.Design.WebControls.PreviewControlDesigner, " + AssemblyRef.SystemDesign)]
    public class TableCell : WebControl {

        private bool _textSetByAddParsedSubObject = false;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.UI.WebControls.TableCell'/> class.
        ///    </para>
        /// </devdoc>
        public TableCell() : this(HtmlTextWriterTag.Td) {
        }


        /// <devdoc>
        /// </devdoc>
        internal TableCell(HtmlTextWriterTag tagKey) : base(tagKey) {
            PreventAutoID();
        }


        /// <devdoc>
        ///    <para>
        ///     Contains a list of categories associated with the table header (read by screen readers). The categories can be any string values. The categories are rendered as a comma delimited list using the HTML axis attribute.
        ///    </para>
        /// </devdoc>
        [
        DefaultValue(null),
        TypeConverterAttribute(typeof(StringArrayConverter)),
        WebCategory("Accessibility"),
        WebSysDescription(SR.TableCell_AssociatedHeaderCellID)
        ]
        public virtual string[] AssociatedHeaderCellID {
            get {
                object x = ViewState["AssociatedHeaderCellID"];
                return (x != null) ? (string[])((string[])x).Clone() : new string[0];
            }
            set {
                if (value != null) {
                    ViewState["AssociatedHeaderCellID"] = (string[])value.Clone();
                }
                else {
                    ViewState["AssociatedHeaderCellID"] = null;
                }
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the number
        ///       of columns this table cell stretches horizontally.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(0),
        WebSysDescription(SR.TableCell_ColumnSpan)
        ]
        public virtual int ColumnSpan {
            get {
                object o = ViewState["ColumnSpan"];
                return((o == null) ? 0 : (int)o);
            }
            set {
                if (value < 0) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["ColumnSpan"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets
        ///       the horizontal alignment of the content within the cell.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(HorizontalAlign.NotSet),
        WebSysDescription(SR.TableItem_HorizontalAlign)
        ]
        public virtual HorizontalAlign HorizontalAlign {
            get {
                if (ControlStyleCreated == false) {
                    return HorizontalAlign.NotSet;
                }
                return ((TableItemStyle)ControlStyle).HorizontalAlign;
            }
            set {
                ((TableItemStyle)ControlStyle).HorizontalAlign = value;
            }
        }

        public override bool SupportsDisabledAttribute {
            get {
                return RenderingCompatibility < VersionUtil.Framework40;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the
        ///       number of rows this table cell stretches vertically.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(0),
        WebSysDescription(SR.TableCell_RowSpan)
        ]
        public virtual int RowSpan {
            get {
                object o = ViewState["RowSpan"];
                return((o == null) ? 0 : (int)o);
            }
            set {
                if (value < 0) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["RowSpan"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets
        ///       or sets the text contained in the cell.</para>
        /// </devdoc>
        [
        Localizable(true),
        WebCategory("Appearance"),
        DefaultValue(""),
        PersistenceMode(PersistenceMode.InnerDefaultProperty),
        WebSysDescription(SR.TableCell_Text)
        ]
        public virtual string Text {
            get {
                object o = ViewState["Text"];
                return((o == null) ? String.Empty : (string)o);
            }
            set {
                if (HasControls()) {
                    Controls.Clear();
                }
                ViewState["Text"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the vertical alignment of the content within the cell.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(VerticalAlign.NotSet),
        WebSysDescription(SR.TableItem_VerticalAlign)
        ]
        public virtual VerticalAlign VerticalAlign {
            get {
                if (ControlStyleCreated == false) {
                    return VerticalAlign.NotSet;
                }
                return ((TableItemStyle)ControlStyle).VerticalAlign;
            }
            set {
                ((TableItemStyle)ControlStyle).VerticalAlign = value;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets
        ///       whether the cell content wraps within the cell.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(true),
        WebSysDescription(SR.TableCell_Wrap)
        ]
        public virtual bool Wrap {
            get {
                if (ControlStyleCreated == false) {
                    return true;
                }
                return ((TableItemStyle)ControlStyle).Wrap;
            }
            set {
                ((TableItemStyle)ControlStyle).Wrap = value;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>A protected method. Adds information about the column
        ///       span and row span to the list of attributes to render.</para>
        /// </devdoc>
        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            base.AddAttributesToRender(writer);

            int span = ColumnSpan;
            if (span > 0)
                writer.AddAttribute(HtmlTextWriterAttribute.Colspan, span.ToString(NumberFormatInfo.InvariantInfo));

            span = RowSpan;
            if (span > 0)
                writer.AddAttribute(HtmlTextWriterAttribute.Rowspan, span.ToString(NumberFormatInfo.InvariantInfo));

            string[] arr = AssociatedHeaderCellID;
            if (arr.Length > 0) {
                bool first = true;
                StringBuilder builder = new StringBuilder();
                Control namingContainer = NamingContainer;
                foreach (string id in arr) {
                    TableHeaderCell headerCell = namingContainer.FindControl(id) as TableHeaderCell;
                    if (headerCell == null) {
                        throw new HttpException(SR.GetString(SR.TableCell_AssociatedHeaderCellNotFound, id));
                    }
                    if (first) {
                        first = false;
                    }
                    else {
                        // AssociatedHeaderCellID was seperated by "," instead of " ". (DevDiv Bugs 159670)
                        builder.Append(" ");
                    }
                    builder.Append(headerCell.ClientID);
                }
                string val = builder.ToString();
                if (!String.IsNullOrEmpty(val)) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Headers, val);
                }
            }
        }


        /// <devdoc>
        /// </devdoc>
        protected override void AddParsedSubObject(object obj) {
            if (HasControls()) {
                base.AddParsedSubObject(obj);
            }
            else {
                if (obj is LiteralControl) {
                    // If we have multiple LiteralControls added here (as would happen if there were a code block
                    // at design time) we don't want to overwrite Text with the last LiteralControl's Text.  Just
                    // concatenate their content.  However, if Text was set by the property or was in ViewState,
                    // we want to overwrite it.
                    if (_textSetByAddParsedSubObject) {
                        Text += ((LiteralControl)obj).Text;
                    }
                    else {
                        Text = ((LiteralControl)obj).Text;
                    }
                    _textSetByAddParsedSubObject = true;
                }
                else {
                    string currentText = Text;
                    if (currentText.Length != 0) {
                        Text = String.Empty;
                        base.AddParsedSubObject(new LiteralControl(currentText));
                    }
                    base.AddParsedSubObject(obj);
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>A protected
        ///       method. Creates a table item control
        ///       style.</para>
        /// </devdoc>
        protected override Style CreateControlStyle() {
            return new TableItemStyle(ViewState);
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>A protected method.</para>
        /// </devdoc>
        protected internal override void RenderContents(HtmlTextWriter writer) {
            // We can't use HasControls() here, because we may have a compiled render method (ASURT 94127)
            if (HasRenderingData()) {
                base.RenderContents(writer);
            }
            else {
                writer.Write(Text);
            }
        }
    }
}

