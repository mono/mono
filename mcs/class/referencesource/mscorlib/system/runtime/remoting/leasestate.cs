// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//+----------------------------------------------------------------------------
//
// Microsoft Windows
// File:        LeaseState.cs
//
// Contents:    Lease States
//
// History:     1/5/00   <EMAIL>[....]</EMAIL>        Created
//
//+----------------------------------------------------------------------------

namespace System.Runtime.Remoting.Lifetime
{
    using System;

    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public enum LeaseState
    {
        Null = 0,
        Initial = 1,
        Active = 2,
        Renewing = 3,
        Expired = 4,
    }
} 
