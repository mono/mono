//------------------------------------------------------------------------------
// <copyright file="ProcessModelComImpersonationLevel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {

    public enum ProcessModelComImpersonationLevel {
        Default = 0,
        Anonymous = 1,
        Delegate = 2,
        Identify = 3,
        Impersonate = 4
    }
}
