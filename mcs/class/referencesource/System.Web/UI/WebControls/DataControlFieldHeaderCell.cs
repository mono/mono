//------------------------------------------------------------------------------
// <copyright file="DataControlFieldHeaderCell.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Globalization;

    /// <devdoc>
    /// <para>Creates a special header cell that is contained within a DataControlField.</para>
    /// </devdoc>
    public class DataControlFieldHeaderCell : DataControlFieldCell {


        public DataControlFieldHeaderCell(DataControlField containingField) : base(HtmlTextWriterTag.Th, containingField) {
        }


        /// <devdoc>
        /// <para>
        /// Sets the abbreviated text for a header cell. The abbreviated text 
        /// is rendered with the HTML ABBR attribute. The ABBR attribute is important 
        /// for screen readers since it allows them to read a shortened version of a header for each cell in the table.
        /// </para>
        /// </devdoc>
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
        /// <para>
        /// Represents the cells that the header applies to. Renders the HTML scope attribute. Possible values are from the TableHeaderScope enumeration: Column and Row.
        /// </para>
        /// </devdoc>
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
        /// <para>Adds header cell attributes to the list of attributes to render.</para>
        /// </devdoc>
        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            base.AddAttributesToRender(writer);

            TableHeaderScope scope = Scope;
            if (scope != TableHeaderScope.NotSet) {
                if (scope == TableHeaderScope.Column) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Scope, "col");
                }
                else {
                    writer.AddAttribute(HtmlTextWriterAttribute.Scope, "row");
                }
            }

            String abbr = AbbreviatedText;
            if (!String.IsNullOrEmpty(abbr)) {
                writer.AddAttribute(HtmlTextWriterAttribute.Abbr, abbr);
            }
        }
    }
}


