//------------------------------------------------------------------------------
// <copyright file="PropertyChangedEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {

    using System;
    using System.Diagnostics;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>Represents the method that will handle the
    ///    <see langword='PropertyChanged'/> event raised when a
    ///       property is changed on a component.</para>
    /// </devdoc>
#if !SILVERLIGHT
    [HostProtection(SharedState = true)]
#endif
    public delegate void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e);
}
