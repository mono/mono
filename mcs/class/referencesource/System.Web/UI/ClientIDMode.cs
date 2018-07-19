//------------------------------------------------------------------------------
// <copyright file="ClientIDModes.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System;
    using System.Diagnostics.CodeAnalysis;


    [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase")]
    public enum ClientIDMode {

        Inherit = 0,

        [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase")]
        AutoID = 1,

        Predictable = 2,

        Static = 3,
    }
}
