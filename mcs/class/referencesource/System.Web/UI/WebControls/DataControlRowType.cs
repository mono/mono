//------------------------------------------------------------------------------
// <copyright file="DataControlRowType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {

    using System;
    

    /// <devdoc>
    ///    <para>Specifies the type of the item in a list.</para>
    /// </devdoc>
    public enum DataControlRowType {
        

        /// <devdoc>
        ///    <para> 
        ///       A header. It is not databound.</para>
        /// </devdoc>
        Header = 0,


        /// <devdoc>
        ///    <para> 
        ///       A footer. It is not databound.</para>
        /// </devdoc>
        Footer = 1,
        

        /// <devdoc>
        ///    An item. It is databound.
        /// </devdoc>
        DataRow = 2,


        /// <devdoc>
        ///    <para> A separator. It is not databound.</para>
        /// </devdoc>
        Separator = 3,


        /// <devdoc>
        ///    <para> A pager. It is used for rendering paging (page accessing) UI associated 
        ///       with the <see cref='System.Web.UI.WebControls.DataGrid'/> control and is not
        ///       databound.</para>
        /// </devdoc>
        Pager = 4,


        /// <devdoc>
        /// <para> An empty data row. It is used for rendering the UI associated with an empty row and is not
        /// databound.</para>
        /// </devdoc>
        EmptyDataRow = 5
    }
}

