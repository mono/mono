//------------------------------------------------------------------------------
// <copyright file="PropertyChangingEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {

    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>Represents the method that will handle the
    ///    <see langword='PropertyChanging'/> event raised when a
    ///       property is changing on a component.</para>
    /// </devdoc>
    [SuppressMessage("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances")]
#if !SILVERLIGHT
    [HostProtection(SharedState = true)]
#endif
    public delegate void PropertyChangingEventHandler(object sender, PropertyChangingEventArgs e);
}
