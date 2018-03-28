//------------------------------------------------------------------------------
// <copyright file="ValidateStringCallback.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Web.Util;

    // Delegate to a function that can validate a single string from a collection
    internal delegate void ValidateStringCallback(string key, string value);

}
