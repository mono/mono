//------------------------------------------------------------------------------
// <copyright file="TableHeaderCell.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

// TableHeaderCell.cs
//

namespace System.Web.UI.WebControls {

    using System;
    using System.Text;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    /// <devdoc>
    ///    <para> Encapsulates
    ///       a header cell within a table.</para>
    /// </devdoc>
    public class TableHeaderCell : TableCell {


        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.UI.WebControls.TableHeaderCell'/> class.
        ///    </para>
        /// </devdoc>
        public TableHeaderCell() : base(HtmlTextWriterTag.Th) {
        }
        

        /// <devdoc>
        ///    <para>
        ///     Sets the abbreviated text for a header cell. The abbreviated text 
        ///     is rendered with the HTML ABBR attribute. The ABBR attribute is important 
        ///     for screen readers since it allows them to read a shortened version of a header for each cell in the table.
        ///     </para>
        /// </devdoc>
        [
        WebCategory("Accessibility"),
        DefaultValue(""),
        WebSysDescription(SR.TableHeaderCell_AbbreviatedText)
        ]
        public virtual string AbbreviatedText {
            get {
                object x = ViewState["AbbrText"];
                return((x == null) ? String.Empty : (string)x);
            }
            set {
                ViewState["AbbrText"] = value;
            }
        }


        /// <devdoc>
        ///    <para>
        ///     Represents the cells that the header applies to. Renders the HTML scope attribute. Possible values are from the TableHeaderScope enumeration: Column and Row.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Accessibility"),
        DefaultValue(TableHeaderScope.NotSet),
        WebSysDescription(SR.TableHeaderCell_Scope)
        ]
        public virtual TableHeaderScope Scope {
            get {
                object x = ViewState["Scope"];
                return((x == null) ? TableHeaderScope.NotSet : (TableHeaderScope)x);
            }
            set {
                ViewState["Scope"] = value;
            }
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
        WebSysDescription(SR.TableHeaderCell_CategoryText)
        ]
        public virtual string[] CategoryText {
            get {
                object x = ViewState["CategoryText"];
                return (x != null) ? (string[])((string[])x).Clone() : new string[0];
            }
            set {
                if (value != null) {
                    ViewState["CategoryText"] = (string[])value.Clone();
                } 
                else {
                    ViewState["CategoryText"] = null;
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Adds header cell attributes to the list of attributes to render.</para>
        /// </devdoc>
        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            base.AddAttributesToRender(writer);
            TableHeaderScope scope = Scope;
            if (scope != TableHeaderScope.NotSet) {
                writer.AddAttribute(HtmlTextWriterAttribute.Scope, scope.ToString().ToLowerInvariant());
            }

            String abbr = AbbreviatedText;
            if (!String.IsNullOrEmpty(abbr)) {
                writer.AddAttribute(HtmlTextWriterAttribute.Abbr, abbr);
            }

            string[] arr = CategoryText;
            if (arr.Length > 0) {
                bool first = true;
                StringBuilder builder = new StringBuilder();
                foreach (string s in arr) {
                    if (first) {
                        first = false;
                    }
                    else {
                        builder.Append(",");
                    }
                    builder.Append(s);
                }
                string val = builder.ToString();
                if (!String.IsNullOrEmpty(val)) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Axis, val);
                }
            }
        }
    }
}
