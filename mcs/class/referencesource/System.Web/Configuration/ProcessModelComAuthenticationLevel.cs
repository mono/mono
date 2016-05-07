//------------------------------------------------------------------------------
// <copyright file="ProcessModelComAuthenticationLevel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {

    public enum ProcessModelComAuthenticationLevel {
        None = 0,
        Call = 1,
        Connect = 2,
        Default = 3,
        Pkt = 4,
        PktIntegrity = 5,
        PktPrivacy = 6
    }
}
