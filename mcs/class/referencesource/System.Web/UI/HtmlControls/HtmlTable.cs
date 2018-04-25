//------------------------------------------------------------------------------
// <copyright file="HtmlTable.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.HtmlControls {
    using System;
    using System.Reflection;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Security.Permissions;



    /// <devdoc>
    ///    <para>Defines the properties, methods, and events for the 
    ///    <see cref='System.Web.UI.HtmlControls.HtmlTable'/> 
    ///    control that allows programmatic access on the
    ///    server to the HTML &lt;table&gt; element.</para>
    /// </devdoc>
    [
    ParseChildren(true, "Rows")
    ]
    public class HtmlTable : HtmlContainerControl {
        HtmlTableRowCollection rows;


        /// <devdoc>
        ///    Initializes a new instance of the <see cref='System.Web.UI.HtmlControls.HtmlTable'/> class.
        /// </devdoc>
        public HtmlTable() : base("table") {
        }


        /// <devdoc>
        /// <para>Gets or sets the alignment of content within the <see cref='System.Web.UI.HtmlControls.HtmlTable'/> 
        /// control.</para>
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


        /// <devdoc>
        /// <para>Gets or sets the background color of an <see cref='System.Web.UI.HtmlControls.HtmlTable'/>
        /// control.</para>
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
        ///    <para>Gets or sets the width of the border, in pixels, of an 
        ///    <see cref='System.Web.UI.HtmlControls.HtmlTable'/> control.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(-1),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int Border {
            get {
                string s = Attributes["border"];
                return((s != null) ? Int32.Parse(s, CultureInfo.InvariantCulture) : -1);
            }

            set {
                Attributes["border"] = MapIntegerAttributeToString(value);
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the border color of an <see cref='System.Web.UI.HtmlControls.HtmlTable'/> control.</para>
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
        ///       Gets or sets the cell padding, in pixels, for an <see langword='HtmlTable'/> control.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int CellPadding {
            get {
                string s = Attributes["cellpadding"];
                return((s != null) ? Int32.Parse(s, CultureInfo.InvariantCulture) : -1);
            }
            set {
                Attributes["cellpadding"] = MapIntegerAttributeToString(value);
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the cell spacing, in pixels, for an <see langword='HtmlTable'/> control.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int CellSpacing {
            get {
                string s = Attributes["cellspacing"];
                return((s != null) ? Int32.Parse(s, CultureInfo.InvariantCulture) : -1);
            }
            set {
                Attributes["cellspacing"] = MapIntegerAttributeToString(value);
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
        ///       Gets or sets the height of an <see langword='HtmlTable'/>
        ///       control.
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
        ///    <para>
        ///       Gets or sets the width of an <see langword='HtmlTable'/> control.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string Width {
            get {
                string s = Attributes["width"];
                return((s != null) ? s : String.Empty);
            }

            set {
                Attributes["width"] = MapStringAttributeToString(value);
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets a collection that contains all of the rows in an
        ///    <see langword='HtmlTable'/> control. An empty collection is returned if no 
        ///       &lt;tr&gt; elements are contained within the control.
        ///    </para>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        IgnoreUnknownContent()
        ]
        public virtual HtmlTableRowCollection Rows {
            get {
                if (rows == null)
                    rows = new HtmlTableRowCollection(this);

                return rows;
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
            return new HtmlTableRowControlCollection(this);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected class HtmlTableRowControlCollection : ControlCollection {

            internal HtmlTableRowControlCollection (Control owner) : base(owner) {
            }


            /// <devdoc>
            /// <para>Adds the specified <see cref='System.Web.UI.Control'/> object to the collection. The new control is added
            ///    to the end of the array.</para>
            /// </devdoc>
            public override void Add(Control child) {
                if (child is HtmlTableRow)
                    base.Add(child);
                else
                    throw new ArgumentException(SR.GetString(SR.Cannot_Have_Children_Of_Type, "HtmlTable", child.GetType().Name.ToString(CultureInfo.InvariantCulture))); // throw an exception here
            }


            /// <devdoc>
            /// <para>Adds the specified <see cref='System.Web.UI.Control'/> object to the collection. The new control is added
            ///    to the array at the specified index location.</para>
            /// </devdoc>
            public override void AddAt(int index, Control child) {
                if (child is HtmlTableRow)
                    base.AddAt(index, child);
                else
                    throw new ArgumentException(SR.GetString(SR.Cannot_Have_Children_Of_Type, "HtmlTable", child.GetType().Name.ToString(CultureInfo.InvariantCulture))); // throw an exception here
            }
        } // class HtmlTableRowControlCollection 

    }
}
