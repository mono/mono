//------------------------------------------------------------------------------
// <copyright file="DataControlCellType.cs" company="Microsoft">
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
    public enum DataControlCellType {
        

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
        DataCell = 2
    }
}

