//------------------------------------------------------------------------------
// <copyright file="ComponentChangedEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Security.Permissions;

    /// <devdoc>
    /// <para>Represents the method that will handle a <see cref='System.ComponentModel.Design.IComponentChangeService.ComponentChanged'/> event.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public delegate void ComponentChangedEventHandler(object sender, ComponentChangedEventArgs e);

}
