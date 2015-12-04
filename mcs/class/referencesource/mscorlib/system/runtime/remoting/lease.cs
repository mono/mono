// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//+----------------------------------------------------------------------------
//
// Microsoft Windows
// File:        Lease.cs
//
// Contents:    Lease class
//
// History:     1/5/00   <EMAIL>[....]</EMAIL>        Created
//
//+----------------------------------------------------------------------------

namespace System.Runtime.Remoting.Lifetime
{
    using System;
    using System.Security;
    using System.Security.Permissions;
    using System.Collections;
    using System.Threading;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    using System.Globalization;

    internal class Lease : MarshalByRefObject, ILease
    {
        internal int id = 0;
        
        // Lease Time
        internal DateTime leaseTime;
        internal TimeSpan initialLeaseTime;
        
        // Renewal Policies
        internal TimeSpan renewOnCallTime;
        internal TimeSpan sponsorshipTimeout;

        // Sponsors
        internal Hashtable sponsorTable;
        internal int sponsorCallThread;

        // Links to leasemanager and managed object
        internal LeaseManager leaseManager;
        internal MarshalByRefObject managedObject;

        // State
        internal LeaseState state;

        internal static volatile int nextId = 0;        


        internal Lease(TimeSpan initialLeaseTime,
                       TimeSpan renewOnCallTime,                       
                       TimeSpan sponsorshipTimeout,
                       MarshalByRefObject managedObject
                      )
        {
            id = nextId++;
            BCLDebug.Trace("REMOTE", "Lease Constructor ",managedObject," initialLeaseTime "+initialLeaseTime+" renewOnCall "+renewOnCallTime+" sponsorshipTimeout ",sponsorshipTimeout);

            // Set Policy            
            this.renewOnCallTime = renewOnCallTime;
            this.sponsorshipTimeout = sponsorshipTimeout;
            this.initialLeaseTime = initialLeaseTime;
            this.managedObject = managedObject;

            //Add lease to leaseManager
            leaseManager = LeaseManager.GetLeaseManager();

            // Initialize tables
            sponsorTable = new Hashtable(10);
            state = LeaseState.Initial;
        }

        internal void ActivateLease()
        {
            // Set leaseTime
            leaseTime = DateTime.UtcNow.Add(initialLeaseTime);
            state = LeaseState.Active;
            leaseManager.ActivateLease(this);
        }

        // Override MarshalByRefObject InitializeLifetimeService
        // Don't want a lease on a lease therefore returns null
        [System.Security.SecurityCritical]  // auto-generated
        public override Object InitializeLifetimeService()
        {
            BCLDebug.Trace("REMOTE", "Lease ",id," InitializeLifetimeService, lease Marshalled");
            return null;
        }

        // ILease Property and Methods

        public TimeSpan RenewOnCallTime
        {
            [System.Security.SecurityCritical]  // auto-generated
            get { return renewOnCallTime; }
            [System.Security.SecurityCritical]  // auto-generated
            [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
            set
            {
                if (state == LeaseState.Initial)
                {
                    renewOnCallTime = value;
                    BCLDebug.Trace("REMOTE", "Lease Set RenewOnCallProperty ",managedObject," "+renewOnCallTime);
                }
                else
                    throw new RemotingException(Environment.GetResourceString("Remoting_Lifetime_InitialStateRenewOnCall", ((Enum)state).ToString()));                    
            }
        }

        public TimeSpan SponsorshipTimeout
        {
            [System.Security.SecurityCritical]  // auto-generated
            get { return sponsorshipTimeout; }
            [System.Security.SecurityCritical]  // auto-generated
            [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
            set
            {
                if (state == LeaseState.Initial)
                {
                    sponsorshipTimeout = value;
                    BCLDebug.Trace("REMOTE", "Lease Set SponsorshipTimeout Property ",managedObject," "+sponsorshipTimeout);                    
                }
                else
                    throw new RemotingException(Environment.GetResourceString("Remoting_Lifetime_InitialStateSponsorshipTimeout", ((Enum)state).ToString()));                                        
            }
        }

        public TimeSpan InitialLeaseTime
        {
            [System.Security.SecurityCritical]  // auto-generated
            get { return initialLeaseTime; }

            [System.Security.SecurityCritical]  // auto-generated
            [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
            set
            {
                if (state == LeaseState.Initial)
                {
                    initialLeaseTime = value;
                    if (TimeSpan.Zero.CompareTo(value) >= 0)
                        state = LeaseState.Null;
                    BCLDebug.Trace("REMOTE", "Lease Set InitialLeaseTime Property ",managedObject,"  "+InitialLeaseTime+", current state "+((Enum)state).ToString());                                                            
                }
                else
                    throw new RemotingException(Environment.GetResourceString("Remoting_Lifetime_InitialStateInitialLeaseTime", ((Enum)state).ToString()));                                                            
            }
        }

        public TimeSpan CurrentLeaseTime
        {
            [System.Security.SecurityCritical]  // auto-generated
            get { return leaseTime.Subtract(DateTime.UtcNow); }
        }

        public LeaseState CurrentState
        {
            [System.Security.SecurityCritical]  // auto-generated
            get { return state;}
        }        


        [System.Security.SecurityCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public void Register(ISponsor obj)
        {
            Register(obj, TimeSpan.Zero);
        }
        
        [System.Security.SecurityCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public void Register(ISponsor obj, TimeSpan renewalTime)
        {
            lock(this)
            {
                BCLDebug.Trace("REMOTE", "Lease "+id+" Register Sponsor  renewalTime ",renewalTime," state ",((Enum)state).ToString());
                if (state == LeaseState.Expired || sponsorshipTimeout == TimeSpan.Zero)
                    return;

                Object sponsorId = GetSponsorId(obj);
                lock(sponsorTable)
                {
                    if (renewalTime > TimeSpan.Zero)
                        AddTime(renewalTime);
                    if (!sponsorTable.ContainsKey(sponsorId))
                    {
                        // Place in tables
                        sponsorTable[sponsorId] = new SponsorStateInfo(renewalTime, SponsorState.Initial);
                    }
                }
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public void Unregister(ISponsor sponsor)
        {
            lock(this)
            {
                BCLDebug.Trace("REMOTE", "Lease",id," Unregister  state ",((Enum)state).ToString());
                if (state == LeaseState.Expired)
                    return;

                Object sponsorId = GetSponsorId(sponsor);
                lock(sponsorTable)
                {
                    if (sponsorId != null)
                    {
                        leaseManager.DeleteSponsor(sponsorId);                
                        SponsorStateInfo sponsorStateInfo = (SponsorStateInfo)sponsorTable[sponsorId];
                        sponsorTable.Remove(sponsorId);
                    }
                }
            }
        }

        // Get the local representative of the sponsor to prevent a remote access when placing
        // in a hash table.
        [System.Security.SecurityCritical]  // auto-generated
        private Object GetSponsorId(ISponsor obj)
        {
            Object sponsorId = null;
            if (obj != null)
            {
                if (RemotingServices.IsTransparentProxy(obj))
                    sponsorId = RemotingServices.GetRealProxy(obj);
                else
                    sponsorId = obj;
            }
            return sponsorId;
        }

        // Convert from the local representative of the sponsor to either the MarshalByRefObject or local object
        [System.Security.SecurityCritical]  // auto-generated
        private ISponsor GetSponsorFromId(Object sponsorId)
        {
            Object sponsor = null;
            RealProxy rp = sponsorId as RealProxy;
            if (null != rp)
                sponsor = rp.GetTransparentProxy();
            else
                sponsor = sponsorId;
            return (ISponsor)sponsor;
        }
        
        [System.Security.SecurityCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
        public TimeSpan Renew(TimeSpan renewalTime)
        {
            return RenewInternal(renewalTime);
        }

        // We will call this internally within the server domain
        internal TimeSpan RenewInternal(TimeSpan renewalTime)
        {
            lock(this)
            {
                BCLDebug.Trace("REMOTE","Lease ",id," Renew ",renewalTime," state ",((Enum)state).ToString());
                if (state == LeaseState.Expired)
                    return TimeSpan.Zero;
                AddTime(renewalTime);
                return leaseTime.Subtract(DateTime.UtcNow);
            }
        }

        // Used for a lease which has been created, but will not be used
        internal void Remove()
        {
            BCLDebug.Trace("REMOTE","Lease ",id," Remove state ",((Enum)state).ToString());
            if (state == LeaseState.Expired)
                return;
            state = LeaseState.Expired;            
            leaseManager.DeleteLease(this);
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal void Cancel()
        {
            lock(this)
            {                        
                BCLDebug.Trace("REMOTE","Lease ",id," Cancel Managed Object ",managedObject," state ",((Enum)state).ToString());

                if (state == LeaseState.Expired)
                    return;

                Remove();
                // Disconnect the object ... 
                // We use the internal version of Disconnect passing "false"
                // for the bResetURI flag. This allows the object to keep its 
                // old URI in case its lease gets reactivated later.
                RemotingServices.Disconnect(managedObject, false);

                // Disconnect the lease for the object.
                RemotingServices.Disconnect(this);
            }
        }



#if _DEBUG
        ~Lease()
        {
            BCLDebug.Trace("REMOTE","Lease ",id," Finalize");
        }
#endif

        internal void RenewOnCall()
        {
            lock(this)
            {
                //BCLDebug.Trace("REMOTE","Lease ",id," RenewOnCall state ",((Enum)state).ToString());
                if (state == LeaseState.Initial || state == LeaseState.Expired)
                    return;            
                AddTime(renewOnCallTime);
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal void LeaseExpired(DateTime now)
        {
            lock(this)
            {
                BCLDebug.Trace("REMOTE","Lease ",id," LeaseExpired state ",((Enum)state).ToString());
                if (state == LeaseState.Expired)
                    return;

                // There is a small window between the time the leaseManager
                // thread examines all the leases and tests for expiry and 
                // when an indivisual lease is locked for expiry. The object 
                // could get marshal-ed in this time which would reset its lease
                // Therefore we check again to see if we should indeed proceed
                // with the expire code (using the same value of 'now' as used
                // by the leaseManager thread)
                if (leaseTime.CompareTo(now) < 0)
                    ProcessNextSponsor();
            }
        }

        internal delegate TimeSpan AsyncRenewal(ILease lease);
        
        [System.Security.SecurityCritical]  // auto-generated
        internal void SponsorCall(ISponsor sponsor)
        {
            BCLDebug.Trace("REMOTE","Lease ",id," SponsorCall state ",((Enum)state).ToString());
            bool exceptionOccurred = false;
            if (state == LeaseState.Expired)
                return;

            lock(sponsorTable)
            {
                try
                {
                    Object sponsorId = GetSponsorId(sponsor);            
                    sponsorCallThread = Thread.CurrentThread.GetHashCode();
                    AsyncRenewal ar = new AsyncRenewal(sponsor.Renewal);
                    SponsorStateInfo sponsorStateInfo = (SponsorStateInfo)sponsorTable[sponsorId];            
                    sponsorStateInfo.sponsorState = SponsorState.Waiting;

                    // The first parameter should be the lease we are trying to renew.
                    IAsyncResult iar = ar.BeginInvoke(this, new AsyncCallback(this.SponsorCallback), null);
                    if ((sponsorStateInfo.sponsorState == SponsorState.Waiting) && (state != LeaseState.Expired))
                    {
                        //   Even if we get here, the operation could still complete before
                        //   we call the the line below. This seems to be a ----.
                        
                        // Sponsor could have completed before statement is reached, so only execute
                        // if the sponsor state is still waiting
                        leaseManager.RegisterSponsorCall(this, sponsorId, sponsorshipTimeout);
                    }
                    sponsorCallThread = 0;
                }catch(Exception)
                {
                    // Sponsor not avaiable
                    exceptionOccurred = true;

                    sponsorCallThread = 0;
                }
            }

            if (exceptionOccurred)
            {
                BCLDebug.Trace("REMOTE","Lease ",id," SponsorCall Sponsor Exception ");
                Unregister(sponsor);
                ProcessNextSponsor();
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal void SponsorTimeout(Object sponsorId)
        {
            lock (this)
            {
                if (!sponsorTable.ContainsKey(sponsorId))
                    return;
                lock(sponsorTable)
                {
                    SponsorStateInfo sponsorStateInfo = (SponsorStateInfo)sponsorTable[sponsorId];
                    BCLDebug.Trace("REMOTE","Lease ",id," SponsorTimeout  sponsorState ",((Enum)sponsorStateInfo.sponsorState).ToString());
                    if (sponsorStateInfo.sponsorState == SponsorState.Waiting)
                    {
                        Unregister(GetSponsorFromId(sponsorId));
                        ProcessNextSponsor();
                    }
                }
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        private void ProcessNextSponsor()
        {
            BCLDebug.Trace("REMOTE","Lease ",id," ProcessNextSponsor");

            Object largestSponsor = null;
            TimeSpan largestRenewalTime = TimeSpan.Zero;
            
            
            lock(sponsorTable)
            {
                IDictionaryEnumerator e = sponsorTable.GetEnumerator();
                // Find sponsor with largest previous renewal value
                while(e.MoveNext())
                {
                    Object sponsorId = e.Key;
                    SponsorStateInfo sponsorStateInfo = (SponsorStateInfo)e.Value;
                    if ((sponsorStateInfo.sponsorState == SponsorState.Initial) && (largestRenewalTime == TimeSpan.Zero))
                    {
                        largestRenewalTime = sponsorStateInfo.renewalTime;
                        largestSponsor = sponsorId;                        
                    }
                    else if (sponsorStateInfo.renewalTime > largestRenewalTime)
                    {
                        largestRenewalTime = sponsorStateInfo.renewalTime;
                        largestSponsor = sponsorId;
                    }
                }
            }

            if (largestSponsor != null)
                SponsorCall(GetSponsorFromId(largestSponsor));
            else
            {
                // No more sponsors to try, Cancel
                BCLDebug.Trace("REMOTE","Lease ",id," ProcessNextSponsor no more sponsors");                
                Cancel();
            }
        }


        // This gets called when we explicitly transfer the call back from the 
        // called function to a threadpool thread.
        [System.Security.SecurityCritical]  // auto-generated
        internal void SponsorCallback(Object obj)
        {
            SponsorCallback((IAsyncResult)obj);
        }

        // On another thread
        [System.Security.SecurityCritical]  // auto-generated
        internal void SponsorCallback(IAsyncResult iar)
        {
            BCLDebug.Trace("REMOTE","Lease ",id," SponsorCallback IAsyncResult ",iar," state ",((Enum)state).ToString());
            if (state == LeaseState.Expired)
            {
                return;
            }

            int thisThread = Thread.CurrentThread.GetHashCode();
            if (thisThread == sponsorCallThread)
            {
                // Looks like something went wrong and the thread that
                // did the AsyncRenewal::BeginInvoke is executing the callback
                // We will queue the work to the thread pool (otherwise there
                // is a possibility of stack overflow if all sponsors are down)
                WaitCallback threadFunc = new WaitCallback(this.SponsorCallback);
                ThreadPool.QueueUserWorkItem(threadFunc, iar);
                return;
            }

            AsyncResult asyncResult = (AsyncResult)iar;
            AsyncRenewal ar = (AsyncRenewal)asyncResult.AsyncDelegate;
            ISponsor sponsor = (ISponsor)ar.Target;
            SponsorStateInfo sponsorStateInfo = null;
            if (iar.IsCompleted)
            {
                // Sponsor came back with renewal
                BCLDebug.Trace("REMOTE","Lease ",id," SponsorCallback sponsor completed");
                bool exceptionOccurred = false;
                TimeSpan renewalTime = TimeSpan.Zero;
                try
                {
                    renewalTime = (TimeSpan)ar.EndInvoke(iar);
                }catch(Exception)
                {
                    // Sponsor not avaiable
                    exceptionOccurred = true;
                }
                if (exceptionOccurred)
                {
                    BCLDebug.Trace("REMOTE","Lease ",id," SponsorCallback Sponsor Exception ");
                    Unregister(sponsor);
                    ProcessNextSponsor();
                }
                else
                {
                    Object sponsorId = GetSponsorId(sponsor);
                    lock(sponsorTable)
                    {
                        if (sponsorTable.ContainsKey(sponsorId))
                        {
                            sponsorStateInfo = (SponsorStateInfo)sponsorTable[sponsorId];
                            sponsorStateInfo.sponsorState = SponsorState.Completed;
                            sponsorStateInfo.renewalTime = renewalTime;
                        }
                        else
                        {
                            // Sponsor was deleted, possibly from a sponsor time out
                        }
                    }

                    if (sponsorStateInfo == null)
                    {
                        // Sponsor was deleted
                        ProcessNextSponsor();
                    }
                    else if (sponsorStateInfo.renewalTime == TimeSpan.Zero)
                    {
                        BCLDebug.Trace("REMOTE","Lease ",id," SponsorCallback sponsor did not renew ");                                            
                        Unregister(sponsor);
                        ProcessNextSponsor();
                    }
                    else
                        RenewInternal(sponsorStateInfo.renewalTime);
                }
            }
            else
            {
                // Sponsor timed out
                // Note time outs should be handled by the LeaseManager
                BCLDebug.Trace("REMOTE","Lease ",id," SponsorCallback sponsor did not complete, timed out");
                Unregister(sponsor);                    
                ProcessNextSponsor();
            }
        }

        

        private void AddTime(TimeSpan renewalSpan)
        {
            if (state == LeaseState.Expired)
                return;

            DateTime now = DateTime.UtcNow;
            DateTime oldLeaseTime = leaseTime;
            DateTime renewTime = now.Add(renewalSpan);
            if (leaseTime.CompareTo(renewTime) < 0)
            {
                leaseManager.ChangedLeaseTime(this, renewTime);
                leaseTime = renewTime;
                state = LeaseState.Active;
            }
            //BCLDebug.Trace("REMOTE","Lease ",id," AddTime renewalSpan ",renewalSpan," current Time ",now," old leaseTime ",oldLeaseTime," new leaseTime ",leaseTime," state ",((Enum)state).ToString());            
        }



        [Serializable]
        internal enum SponsorState
        {
            Initial = 0,
            Waiting = 1,
            Completed = 2
        }

        internal sealed class SponsorStateInfo
        {
            internal TimeSpan renewalTime;
            internal SponsorState sponsorState;

            internal SponsorStateInfo(TimeSpan renewalTime, SponsorState sponsorState)
            {
                this.renewalTime = renewalTime;
                this.sponsorState = sponsorState;
            }
        }
    }

    internal class LeaseSink : IMessageSink
    {
        Lease lease = null;
        IMessageSink nextSink = null;

        public LeaseSink(Lease lease, IMessageSink nextSink)
        {
            this.lease = lease;
            this.nextSink = nextSink;
        }
        
        //IMessageSink methods
        [System.Security.SecurityCritical]  // auto-generated
        public IMessage SyncProcessMessage(IMessage msg)
        {
            //BCLDebug.Trace("REMOTE","Lease ",id," SyncProcessMessage");
            lease.RenewOnCall();
            return nextSink.SyncProcessMessage(msg);
        }

        [System.Security.SecurityCritical]  // auto-generated
        public IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
        {
            //BCLDebug.Trace("REMOTE","Lease ",id," AsyncProcessMessage");
            lease.RenewOnCall();
            return nextSink.AsyncProcessMessage(msg, replySink);        
        }

        public IMessageSink NextSink
        {
            [System.Security.SecurityCritical]  // auto-generated
            get
            {
                //BCLDebug.Trace("REMOTE","Lease ",id," NextSink");        
                return nextSink;
            }
        }
    }
} 
