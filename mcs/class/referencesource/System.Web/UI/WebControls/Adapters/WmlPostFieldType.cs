//------------------------------------------------------------------------------
// <copyright file="WmlPostFieldType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.Adapters {
    using System.Web.UI.WebControls;

    public enum WmlPostFieldType {
        Normal,
        Submit,
        Variable,
        Raw
    }
}

#endif 
