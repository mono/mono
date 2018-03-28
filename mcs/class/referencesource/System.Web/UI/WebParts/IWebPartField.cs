//------------------------------------------------------------------------------
// <copyright file="IWebPartField.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.ComponentModel;

    public interface IWebPartField {
        PropertyDescriptor Schema { get; }
        void GetFieldValue(FieldCallback callback);
    }
}
