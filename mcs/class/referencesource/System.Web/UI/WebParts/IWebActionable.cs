//------------------------------------------------------------------------------
// <copyright file="IWebActionable.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;

    public interface IWebActionable {

        WebPartVerbCollection Verbs { get; }
    }
}
