//------------------------------------------------------------------------------
// <copyright file="AuthenticationMode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {

    public enum AuthenticationMode {
        None,
        Windows,
        [Obsolete("This field is obsolete. The Passport authentication product is no longer supported and has been superseded by Live ID.")]
        Passport,
        Forms
    }
}
