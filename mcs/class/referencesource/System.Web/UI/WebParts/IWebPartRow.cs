//------------------------------------------------------------------------------
// <copyright file="IWebPartRow.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.ComponentModel;

    public interface IWebPartRow {
        PropertyDescriptorCollection Schema { get; }
        void GetRowData(RowCallback callback);
    }
}
