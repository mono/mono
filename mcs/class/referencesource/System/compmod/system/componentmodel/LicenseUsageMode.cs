//------------------------------------------------------------------------------
// <copyright file="LicenseUsageMode.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    

    using System.Diagnostics;
    using System;

    /// <devdoc>
    ///    <para>Specifies when the license can be used.</para>
    /// </devdoc>
    public enum LicenseUsageMode {

        /// <devdoc>
        ///    <para>
        ///       Used during runtime.
        ///    </para>
        /// </devdoc>
        Runtime,

        /// <devdoc>
        ///    <para>
        ///       Used during design time by a visual designer or the compiler.
        ///    </para>
        /// </devdoc>
        Designtime,
    }
}
