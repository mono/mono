//------------------------------------------------------------------------------
// <copyright file="TableRow.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    /// <devdoc>
    ///    <para> Encapsulates a row
    ///       within a table.</para>
    /// </devdoc>
    [
    Bindable(false),
    DefaultProperty("Cells"),
    ParseChildren(true, "Cells"),
    ToolboxItem(false)
    ]
    [Designer("System.Web.UI.Design.WebControls.PreviewControlDesigner, " + AssemblyRef.SystemDesign)]
    public class TableRow : WebControl {

        TableCellCollection cells;


        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.UI.WebControls.TableRow'/> class.
        ///    </para>
        /// </devdoc>
        public TableRow() : base(HtmlTextWriterTag.Tr) {
            PreventAutoID();
        }


        /// <devdoc>
        ///    <para> Indicates the table cell collection of the table
        ///       row. This property is read-only.</para>
        /// </devdoc>
        [
        MergableProperty(false),
        WebSysDescription(SR.TableRow_Cells),
        PersistenceMode(PersistenceMode.InnerDefaultProperty)
        ]
        public virtual TableCellCollection Cells {
            get {
                if (cells == null)
                    cells = new TableCellCollection(this);
                return cells;
            }
        }


        /// <devdoc>
        ///    <para> Indicates the horizontal alignment of the content within the table cells.</para>
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

        [
        WebCategory("Accessibility"),
        DefaultValue(TableRowSection.TableBody),
        WebSysDescription(SR.TableRow_TableSection)
        ]
        public virtual TableRowSection TableSection {
            get {
                object o = ViewState["TableSection"];
                return((o == null) ? TableRowSection.TableBody : (TableRowSection)o);
            }
            set {
                if (value < TableRowSection.TableHeader || value > TableRowSection.TableFooter) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["TableSection"] = value;
                if (value != TableRowSection.TableBody) {
                    Control parent = Parent;
                    if (parent != null) {
                        Table parentTable = parent as Table;
                        if (parentTable != null) {
                            parentTable.HasRowSections = true;
                        }
                    }
                }
            }
        }


        /// <devdoc>
        ///    <para>Indicates the vertical alignment of the content within the cell.</para>
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


        /// <internalonly/>
        /// <devdoc>
        ///    <para>A protected method. Creates a table item control style.</para>
        /// </devdoc>
        protected override Style CreateControlStyle() {
            return new TableItemStyle(ViewState);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override ControlCollection CreateControlCollection() {
            return new CellControlCollection(this);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected class CellControlCollection : ControlCollection {

            internal CellControlCollection (Control owner) : base(owner) {
            }


            /// <devdoc>
            /// <para>Adds the specified <see cref='System.Web.UI.Control'/> object to the collection. The new control is added
            ///    to the end of the array.</para>
            /// </devdoc>
            public override void Add(Control child) {
                if (child is TableCell)
                    base.Add(child);
                else
                    throw new ArgumentException(SR.GetString(SR.Cannot_Have_Children_Of_Type, "TableRow", child.GetType().Name.ToString(CultureInfo.InvariantCulture))); // throw an exception here
            }


            /// <devdoc>
            /// <para>Adds the specified <see cref='System.Web.UI.Control'/> object to the collection. The new control is added
            ///    to the array at the specified index location.</para>
            /// </devdoc>
            public override void AddAt(int index, Control child) {
                if (child is TableCell)
                    base.AddAt(index, child);
                else
                    throw new ArgumentException(SR.GetString(SR.Cannot_Have_Children_Of_Type, "TableRow", child.GetType().Name.ToString(CultureInfo.InvariantCulture))); // throw an exception here
            }
        } // class CellControlCollection

    }
}

