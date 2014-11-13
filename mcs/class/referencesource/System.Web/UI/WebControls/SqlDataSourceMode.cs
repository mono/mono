//------------------------------------------------------------------------------
// <copyright file="SqlDataSourceMode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;


    /// <devdoc>
    /// Specifies the behavior of the SqlDataSource.
    /// </devdoc>
    public enum SqlDataSourceMode {

        /// <devdoc>
        /// The SqlDataSource uses a DataReader, which does not allow sorting or paging.
        /// </devdoc>
        DataReader = 0,


        /// <devdoc>
        /// The SqlDataSource uses a DataSet, which allows sorting and paging.
        /// </devdoc>
        DataSet = 1,
    }
}

