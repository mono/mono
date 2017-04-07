// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//+----------------------------------------------------------------------------
//
// File:        ClientSponsor.cs
//
// Contents:    Agent for keeping Server Object's lifetime in sync with a client's lifetime
//
// History:     8/9/00   <EMAIL>Microsoft</EMAIL>        Created
//
//+----------------------------------------------------------------------------

namespace System.Runtime.Remoting.Lifetime
{
    using System;
    using System.Collections;
    using System.Security.Permissions;

    [System.Security.SecurityCritical]  // auto-generated_required
    [SecurityPermissionAttribute(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]    
    [System.Runtime.InteropServices.ComVisible(true)]
    public class ClientSponsor : MarshalByRefObject, ISponsor
    {
        private Hashtable sponsorTable = new Hashtable(10);
        private TimeSpan m_renewalTime = TimeSpan.FromMinutes(2);

        public ClientSponsor()
        {
        }

        public ClientSponsor(TimeSpan renewalTime)
        {
            this.m_renewalTime = renewalTime;
        }

        public TimeSpan RenewalTime
        {
            get{ return m_renewalTime;}
            set{ m_renewalTime = value;}
        }
            
        [System.Security.SecurityCritical]  // auto-generated
        public bool Register(MarshalByRefObject obj)
        {
            BCLDebug.Trace("REMOTE", "ClientSponsor Register "+obj);
            ILease lease = (ILease)obj.GetLifetimeService();
            if (lease == null)
                return false;

            lease.Register(this);
            lock(sponsorTable)
            {
                sponsorTable[obj] = lease;
            }
            return true;
        }

        [System.Security.SecurityCritical]  // auto-generated
        public void Unregister(MarshalByRefObject obj)
        {
            BCLDebug.Trace("REMOTE", "ClientSponsor Unregister "+obj);

            ILease lease = null;
            lock(sponsorTable)
            {
                lease = (ILease)sponsorTable[obj];
            }
            if (lease != null)
                lease.Unregister(this);
        }

        // ISponsor method
        [System.Security.SecurityCritical]
        public TimeSpan Renewal(ILease lease)
        {
            BCLDebug.Trace("REMOTE", "ClientSponsor Renewal "+m_renewalTime);
            return m_renewalTime;
        }

        [System.Security.SecurityCritical]  // auto-generated
        public void Close()
        {
            BCLDebug.Trace("REMOTE","ClientSponsor Close");
            lock(sponsorTable)
            {
                IDictionaryEnumerator e = sponsorTable.GetEnumerator();
                while(e.MoveNext())
                    ((ILease)e.Value).Unregister(this);
                sponsorTable.Clear();
            }
        }

        // Don't create a lease on the sponsor
        [System.Security.SecurityCritical]
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        [System.Security.SecuritySafeCritical] // finalizers should be treated as safe
        ~ClientSponsor()
        {
            BCLDebug.Trace("REMOTE","ClientSponsor Finalize");
        }
    }
}
