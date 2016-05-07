//------------------------------------------------------------------------------
// <copyright file="RunWorkerCompletedEventHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.ComponentModel 
{
    using System.Security.Permissions;

    [HostProtection(SharedState = true)]
    public delegate void RunWorkerCompletedEventHandler(object sender,
                                                        RunWorkerCompletedEventArgs e);
}
