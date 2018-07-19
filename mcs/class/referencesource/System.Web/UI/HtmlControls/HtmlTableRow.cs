//------------------------------------------------------------------------------
// <copyright file="HtmlTableRow.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.HtmlControls {
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Security.Permissions;


    /// <devdoc>
    ///    <para>
    ///       The <see langword='HtmlTableRow'/>
    ///       class defines the properties, methods, and events for the HtmlTableRow control.
    ///       This class allows programmatic access on the server to individual HTML
    ///       &lt;tr&gt; elements enclosed within an <see cref='System.Web.UI.HtmlControls.HtmlTable'/> control.
    ///    </para>
    /// </devdoc>
    [
    ParseChildren(true, "Cells")
    ]
    public class HtmlTableRow : HtmlContainerControl {
        HtmlTableCellCollection cells;


        public HtmlTableRow() : base("tr") {
        }



        /// <devdoc>
        ///    <para>
        ///       Gets or sets the horizontal alignment of the cells contained in an
        ///    <see langword='HtmlTableRow'/> control.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string Align {
            get {
                string s = Attributes["align"];
                return((s != null) ? s : String.Empty);
            }

            set {
                Attributes["align"] = MapStringAttributeToString(value);
            }
        }

        /*
         * Collection of child TableCells.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the group of table cells contained within an
        ///    <see langword='HtmlTableRow'/> control.
        ///    </para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public virtual HtmlTableCellCollection Cells {
            get {
                if (cells == null)
                    cells = new HtmlTableCellCollection(this);

                return cells;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the background color of an <see langword='HtmlTableRow'/>
        ///       control.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string BgColor {
            get {
                string s = Attributes["bgcolor"];
                return((s != null) ? s : String.Empty);
            }

            set {
                Attributes["bgcolor"] = MapStringAttributeToString(value);
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the border color of an <see langword='HtmlTableRow'/> control.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string BorderColor {
            get {
                string s = Attributes["bordercolor"];
                return((s != null) ? s : String.Empty);
            }

            set {
                Attributes["bordercolor"] = MapStringAttributeToString(value);
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the height of an <see langword='HtmlTableRow'/> control.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string Height {
            get {
                string s = Attributes["height"];
                return((s != null) ? s : String.Empty);
            }

            set {
                Attributes["height"] = MapStringAttributeToString(value);
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override string InnerHtml {
            get {
                throw new NotSupportedException(SR.GetString(SR.InnerHtml_not_supported, this.GetType().Name));
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.InnerHtml_not_supported, this.GetType().Name));
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override string InnerText {
            get {
                throw new NotSupportedException(SR.GetString(SR.InnerText_not_supported, this.GetType().Name));
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.InnerText_not_supported, this.GetType().Name));
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the vertical alignment of of the cells contained in an
        ///    <see langword='HtmlTableRow'/> control.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string VAlign {
            get {
                string s = Attributes["valign"];
                return((s != null) ? s : String.Empty);
            }

            set {
                Attributes["valign"] = MapStringAttributeToString(value);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected internal override void RenderChildren(HtmlTextWriter writer) {
            writer.WriteLine();
            writer.Indent++;
            base.RenderChildren(writer);

            writer.Indent--;
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override void RenderEndTag(HtmlTextWriter writer) {
            base.RenderEndTag(writer);
            writer.WriteLine();
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override ControlCollection CreateControlCollection() {
            return new HtmlTableCellControlCollection(this);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected class HtmlTableCellControlCollection : ControlCollection {

            internal HtmlTableCellControlCollection (Control owner) : base(owner) {
            }


            /// <devdoc>
            /// <para>Adds the specified <see cref='System.Web.UI.Control'/> object to the collection. The new control is added
            ///    to the end of the array.</para>
            /// </devdoc>
            public override void Add(Control child) {
                if (child is HtmlTableCell)
                    base.Add(child);
                else
                    throw new ArgumentException(SR.GetString(SR.Cannot_Have_Children_Of_Type, "HtmlTableRow", child.GetType().Name.ToString(CultureInfo.InvariantCulture))); // throw an exception here
            }
            

            /// <devdoc>
            /// <para>Adds the specified <see cref='System.Web.UI.Control'/> object to the collection. The new control is added
            ///    to the array at the specified index location.</para>
            /// </devdoc>
            public override void AddAt(int index, Control child) {
                if (child is HtmlTableCell)
                    base.AddAt(index, child);
                else
                    throw new ArgumentException(SR.GetString(SR.Cannot_Have_Children_Of_Type, "HtmlTableRow", child.GetType().Name.ToString(CultureInfo.InvariantCulture))); // throw an exception here
            }
        }
    }
}
