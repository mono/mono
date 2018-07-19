//------------------------------------------------------------------------------
// <copyright file="ComponentEventHandler.cs" company="Microsoft">
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
    /// <para>Represents the method that will handle the <see cref='System.ComponentModel.Design.IComponentChangeService.ComponentAdding'/> , <see cref='System.ComponentModel.Design.IComponentChangeService.ComponentAdded'/>, <see cref='System.ComponentModel.Design.IComponentChangeService.ComponentRemoving'/>, and 
    /// <see cref='System.ComponentModel.Design.IComponentChangeService.ComponentRemoved'/> event raised
    ///    for component-level events.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public delegate void ComponentEventHandler(object sender, ComponentEventArgs e);
}
