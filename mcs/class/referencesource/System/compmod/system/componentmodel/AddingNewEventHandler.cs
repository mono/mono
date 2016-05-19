//------------------------------------------------------------------------------
// <copyright file="AddingNewEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.ComponentModel {
    using System;
    using System.ComponentModel;
    using System.Security.Permissions;

    /// <devdoc>
    ///     Represents the method that will handle the AddingNew event on a list,
    ///     and provide the new object to be added to the list.
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public delegate void AddingNewEventHandler(object sender, AddingNewEventArgs e);
}
