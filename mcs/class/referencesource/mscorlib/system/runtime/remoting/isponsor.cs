// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//+----------------------------------------------------------------------------
//
// Microsoft Windows
// File:        ISponsor.cs
//
// Contents:    Interface for Sponsors
//
// History:     1/5/00   <EMAIL>[....]</EMAIL>        Created
//
//+----------------------------------------------------------------------------

namespace System.Runtime.Remoting.Lifetime
{
    using System;
    using System.Security.Permissions;

    [System.Runtime.InteropServices.ComVisible(true)]
    public interface ISponsor
    {
        [System.Security.SecurityCritical]  // auto-generated_required
        TimeSpan Renewal(ILease lease);
    }
} 
