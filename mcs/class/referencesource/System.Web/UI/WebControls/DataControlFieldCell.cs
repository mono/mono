//------------------------------------------------------------------------------
// <copyright file="DataControlFieldCell.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Web.UI.WebControls;


    /// <devdoc>
    /// <para>Creates a special cell that is contained within a DataControlField.</para>
    /// </devdoc>
    public class DataControlFieldCell : TableCell {
        DataControlField _containingField;


        public DataControlFieldCell(DataControlField containingField) {
            _containingField = containingField;
        }


        protected DataControlFieldCell(HtmlTextWriterTag tagKey, DataControlField containingField) : base(tagKey) {
            _containingField = containingField;
        }


        public DataControlField ContainingField {
            get {
                return _containingField;
            }
        }

        /// <summary>
        /// <see cref='System.Web.UI.WebControls.DataControlFieldCell'/> gets the value of ValidateRequestMode from it's <see cref='System.Web.UI.WebControls.DataControlFieldCell.ContainingField'/>. 
        /// The ValidateRequestMode property should not be set directly on <see cref='System.Web.UI.WebControls.DataControlFieldCell'/>.
        /// </summary>
        public override ValidateRequestMode ValidateRequestMode {
            get {
                return _containingField.ValidateRequestMode;
            }
            set {
                throw new InvalidOperationException(SR.GetString(SR.DataControlFieldCell_ShouldNotSetValidateRequestMode));
            }
        }
    }
}


