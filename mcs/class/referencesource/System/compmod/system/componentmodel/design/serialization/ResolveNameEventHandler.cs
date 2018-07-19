//------------------------------------------------------------------------------
// <copyright file="ResolveNameEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel.Design.Serialization {
    using System.Security.Permissions;
    /// <devdoc>
    ///     This delegate is used to resolve object names when performing
    ///     serialization and deserialization.
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public delegate void ResolveNameEventHandler(object sender, ResolveNameEventArgs e);
}

