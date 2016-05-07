//------------------------------------------------------------------------------
// <copyright file="CancelEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>Represents the method that will handle the event raised when canceling an
    ///       event.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public delegate void CancelEventHandler(object sender, CancelEventArgs e);
}
