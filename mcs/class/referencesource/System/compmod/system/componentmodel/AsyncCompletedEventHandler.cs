//------------------------------------------------------------------------------
// <copyright file="AsyncCompletedEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.ComponentModel
{
    using System.Security.Permissions;

    [HostProtection(SharedState = true)]
    public delegate void AsyncCompletedEventHandler(object sender, AsyncCompletedEventArgs e);
}
