//------------------------------------------------------------------------------
// <copyright file="DataSourceOperation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;


    /// <devdoc>
    /// Specifies a DataSource operation.
    /// </devdoc>
    public enum DataSourceOperation {

        Delete = 0,

        Insert = 1,

        Select = 2,

        Update = 3,

        SelectCount = 4
    }
}

