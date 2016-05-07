//------------------------------------------------------------------------------
// <copyright file="DataSourceOperationDelegates.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System.Collections;

    public delegate void DataSourceViewSelectCallback(IEnumerable data);

    // returns whether the exception was handled
    public delegate bool DataSourceViewOperationCallback(int affectedRecords, Exception ex);
}
