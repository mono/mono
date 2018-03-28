//------------------------------------------------------------------------------
// <copyright file="BuildProviderAppliesTo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Compilation {

    [Flags]
    public enum BuildProviderAppliesTo {
        Web             = 0x1,
        Code            = 0x2,
        Resources       = 0x4,
        All             = 0x7,
    }
}
