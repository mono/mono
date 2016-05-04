//------------------------------------------------------------------------------
// <copyright file="WebApplicationLevel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {

    // Represents the path level of a configuration file for a web app.
    public enum WebApplicationLevel {
        AboveApplication     = 10,
        AtApplication        = 20,
        BelowApplication     = 30
    }
}
