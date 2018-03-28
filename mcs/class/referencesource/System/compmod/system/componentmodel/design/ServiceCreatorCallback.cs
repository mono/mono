//------------------------------------------------------------------------------
// <copyright file="ServiceCreatorCallback.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {
    using System;
    using System.Security.Permissions;

    /// <devdoc>
    ///     Declares a callback function to create an instance of a service on demand.
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public delegate object ServiceCreatorCallback(IServiceContainer container, Type serviceType);

}
