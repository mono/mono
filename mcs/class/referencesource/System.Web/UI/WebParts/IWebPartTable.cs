//------------------------------------------------------------------------------
// <copyright file="IWebPartTable.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.ComponentModel;

    public interface IWebPartTable {
        PropertyDescriptorCollection Schema { get; }
        void GetTableData(TableCallback callback);
    }
}
