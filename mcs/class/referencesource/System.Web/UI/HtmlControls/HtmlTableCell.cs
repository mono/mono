//------------------------------------------------------------------------------
// <copyright file="HtmlTableCell.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * HtmlTableCell.cs
 *
 * Copyright (c) 2000 Microsoft Corporation
 */

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
///       The <see langword='HtmlTableCell'/>
///       class defines the properties, methods, and events for the HtmlTableCell control.
///       This class allows programmatic access on the server to individual HTML
///       &lt;td&gt; and &lt;th&gt; elements enclosed within an
///    <see langword='HtmlTableRow'/>
///    control.
/// </para>
/// </devdoc>
    [ConstructorNeedsTag(true)]
    public class HtmlTableCell : HtmlContainerControl {

        /// <devdoc>
        /// </devdoc>
        public HtmlTableCell() : base("td") {
        }


        /// <devdoc>
        /// </devdoc>
        public HtmlTableCell(string tagName) : base(tagName) {
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the horizontal alignment of content within an
        ///    <see langword='HtmlTableCell'/> control.
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


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the background color of an <see langword='HtmlTableCell'/>
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
        ///       Gets or sets the border color of an <see langword='HtmlTableCell'/>
        ///       control.
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

        /*
         * Number of columns that this cell spans.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the number of columns that the HtmlTableCell control spans.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int ColSpan {
            get {
                string s = Attributes["colspan"];
                return((s != null) ? Int32.Parse(s, CultureInfo.InvariantCulture) : -1);
            }
            set {
                Attributes["colspan"] = MapIntegerAttributeToString(value);
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the height, in pixels, of an <see langword='HtmlTableCell'/>
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

        /*
         * Suppresses wrapping.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating whether text within an
        ///    <see langword='HtmlTableCell'/> control
        ///       should be wrapped.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        TypeConverter(typeof(MinimizableAttributeTypeConverter))
        ]
        public bool NoWrap {
            get {
                string s = Attributes["nowrap"];
                return((s != null) ? (s.Equals("nowrap")) : false);
            }

            set {
                if (value) 
                    Attributes["nowrap"] = "nowrap";
                else 
                    Attributes["nowrap"] = null;
            }
        }

        /*
         * Number of rows that this cell spans.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the number of rows an <see langword='HtmlTableCell'/> control
        ///       spans.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int RowSpan {
            get {
                string s = Attributes["rowspan"];
                return((s != null) ? Int32.Parse(s, CultureInfo.InvariantCulture) : -1);
            }
            set {
                Attributes["rowspan"] = MapIntegerAttributeToString(value);
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the vertical alignment for text within an
        ///    <see langword='HtmlTableCell'/> control.
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


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the width, in pixels, of an <see langword='HtmlTableCell'/>
        ///       control.
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


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override void RenderEndTag(HtmlTextWriter writer) {
            base.RenderEndTag(writer);
            writer.WriteLine();
        }

    }
}
