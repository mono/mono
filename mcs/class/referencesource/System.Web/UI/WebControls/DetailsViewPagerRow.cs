//------------------------------------------------------------------------------
// <copyright file="DetailsViewPagerRow.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;

    /// <devdoc>
    /// <para>Represents an individual row in the <see cref='System.Web.UI.WebControls.DetailsView'/>.</para>
    /// </devdoc>
    public class DetailsViewPagerRow : DetailsViewRow, INonBindingContainer {
        
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.DetailsViewRow'/> class.</para>
        /// </devdoc>
        public DetailsViewPagerRow(int rowIndex, DataControlRowType rowType, DataControlRowState rowState) : base(rowIndex, rowType, rowState) {
        }
    }
}

