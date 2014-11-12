// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//+----------------------------------------------------------------------------
//
// Microsoft Windows
// File:        ILease.cs
//
// Contents:    Interface for Lease
//
// History:     1/5/00   <EMAIL>[....]</EMAIL>        Created
//
//+----------------------------------------------------------------------------

namespace System.Runtime.Remoting.Lifetime
{
    using System;
    using System.Security.Permissions;

[System.Runtime.InteropServices.ComVisible(true)]
    public interface ILease
    {
        [System.Security.SecurityCritical]  // auto-generated_required
        void Register(ISponsor obj, TimeSpan renewalTime);
        
        [System.Security.SecurityCritical]  // auto-generated_required
        void Register(ISponsor obj);

        [System.Security.SecurityCritical]  // auto-generated_required
        void Unregister(ISponsor obj);

        [System.Security.SecurityCritical]  // auto-generated_required
        TimeSpan Renew(TimeSpan renewalTime);

        TimeSpan RenewOnCallTime 
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
            [System.Security.SecurityCritical]  // auto-generated_required
            set;
        }

        TimeSpan SponsorshipTimeout
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
            [System.Security.SecurityCritical]  // auto-generated_required
            set;
        }

        TimeSpan InitialLeaseTime
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
            [System.Security.SecurityCritical]  // auto-generated_required
            set;
        }

        TimeSpan CurrentLeaseTime 
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
        }  

        LeaseState CurrentState 
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get;
        }
    }
}


