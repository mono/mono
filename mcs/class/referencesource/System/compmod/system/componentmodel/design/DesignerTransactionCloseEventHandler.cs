//------------------------------------------------------------------------------
// <copyright file="DesignerTransactionCloseEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    [System.Runtime.InteropServices.ComVisible(true)]
    public delegate void DesignerTransactionCloseEventHandler(object sender, DesignerTransactionCloseEventArgs e);
}
