//------------------------------------------------------------------------------
// <copyright file="DocumentEventHandler.cs" company="Microsoft">
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
    /// <para>Represents the method that will handle the System.ComponentModel.Design.IDesignerEventService.DesignerEvent
    /// event raised when a document is created or disposed.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public delegate void DesignerEventHandler(object sender, DesignerEventArgs e);
}

