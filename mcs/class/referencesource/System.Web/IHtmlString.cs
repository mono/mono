//------------------------------------------------------------------------------
// <copyright file="IHtmlString.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

namespace System.Web {
    // Marker interface implemented by objects that should NOT be HTML encoded when <%: o %> is used
    public interface IHtmlString {
        string ToHtmlString();
    }
}
