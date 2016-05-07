// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//+----------------------------------------------------------------------------
//
// Microsoft Windows
// File:        LeaseManager.cs
//
// Contents:    Administers the leases in an appdomain
//
// History:     1/5/00   <EMAIL>[....]</EMAIL>        Created
//
//+----------------------------------------------------------------------------

namespace System.Runtime.Remoting.Lifetime
{
    using System;
    using System.Collections;
    using System.Threading;

    internal class LeaseManager
    {

        // Lease Lists
        private Hashtable leaseToTimeTable = new Hashtable();

        // Async Sponsor Calls
        //private SortedList sponsorCallList = new SortedList();
        private Hashtable sponsorTable = new Hashtable();


        // LeaseTimeAnalyzer thread
        private TimeSpan pollTime;
        AutoResetEvent waitHandle;
        TimerCallback leaseTimeAnalyzerDelegate;
        private volatile Timer leaseTimer;


        internal static bool IsInitialized()
        {
            DomainSpecificRemotingData remotingData = Thread.GetDomain().RemotingData;
            LeaseManager leaseManager = remotingData.LeaseManager;
            return leaseManager != null;
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal static LeaseManager GetLeaseManager(TimeSpan pollTime)
        {
            DomainSpecificRemotingData remotingData = Thread.GetDomain().RemotingData;
            LeaseManager leaseManager = remotingData.LeaseManager;
            if (leaseManager == null)
            {
                lock (remotingData)
                {
                    if (remotingData.LeaseManager == null)
                    {
                        remotingData.LeaseManager = new LeaseManager(pollTime);
                    }
                    leaseManager = remotingData.LeaseManager;
                }
            }

            return leaseManager;
        }

        internal static LeaseManager GetLeaseManager()
        {
            DomainSpecificRemotingData remotingData = Thread.GetDomain().RemotingData;
            LeaseManager leaseManager = remotingData.LeaseManager;          
            BCLDebug.Assert(leaseManager != null, "[LeaseManager.GetLeaseManager()]leaseManager !=null");
            return leaseManager;
        }


        [System.Security.SecurityCritical]  // auto-generated
        private LeaseManager(TimeSpan pollTime)
        {
            BCLDebug.Trace("REMOTE","LeaseManager Constructor");            
            this.pollTime = pollTime;

            leaseTimeAnalyzerDelegate = new TimerCallback(this.LeaseTimeAnalyzer);
            waitHandle = new AutoResetEvent(false);
            // We need to create a Timer with Infinite dueTime to ensure that
            // leaseTimeAnalyzerDelegate doesnt get invoked before leaseTimer is initialized
            // Once initialized we can change it to the appropriate dueTime
            leaseTimer = new Timer(leaseTimeAnalyzerDelegate, null, Timeout.Infinite, Timeout.Infinite);
            leaseTimer.Change((int)pollTime.TotalMilliseconds, Timeout.Infinite);
        }


        internal void ChangePollTime(TimeSpan pollTime)
        {
            BCLDebug.Trace("REMOTE","LeaseManager ChangePollTime ", pollTime);
            this.pollTime = pollTime;
        }


        internal void ActivateLease(Lease lease)
        {
            BCLDebug.Trace("REMOTE","LeaseManager AddLease ",lease.id," ",lease.managedObject);
            lock(leaseToTimeTable)
            {
                leaseToTimeTable[lease] = lease.leaseTime;
            }
        }       

        internal void DeleteLease(Lease lease)
        {
            BCLDebug.Trace("REMOTE","LeaseManager DeleteLease ",lease.id);
            lock(leaseToTimeTable)
            {
                leaseToTimeTable.Remove(lease);
            }
        }

        [System.Diagnostics.Conditional("_LOGGING")]
        internal void DumpLeases(Lease[] leases)
        {
            for (int i=0; i<leases.Length; i++)
            {
                BCLDebug.Trace("REMOTE","LeaseManager DumpLease ",leases[i].managedObject);                                         
            }
        }


        internal ILease GetLease(MarshalByRefObject obj)
        {
            BCLDebug.Trace("REMOTE","LeaseManager GetLease ",obj);
            bool fServer = true;
            Identity idObj = MarshalByRefObject.GetIdentity(obj, out fServer);
            if (idObj == null)
                return null;
            else
                return idObj.Lease;
        }

        internal void ChangedLeaseTime(Lease lease, DateTime newTime)
        {
            BCLDebug.Trace("REMOTE","LeaseManager ChangedLeaseTime ",lease.id," ",lease.managedObject," newTime ",newTime," currentTime ", DateTime.UtcNow);
            lock(leaseToTimeTable)
            {
                leaseToTimeTable[lease] = newTime;
            }
        }

        internal class SponsorInfo
        {
            internal Lease lease;
            internal Object sponsorId;
            internal DateTime sponsorWaitTime;

            internal SponsorInfo(Lease lease, Object sponsorId, DateTime sponsorWaitTime)
            {
                this.lease = lease;
                this.sponsorId = sponsorId;
                this.sponsorWaitTime = sponsorWaitTime;
            }
        }

        internal void RegisterSponsorCall(Lease lease, Object sponsorId, TimeSpan sponsorshipTimeOut)
        {
            BCLDebug.Trace("REMOTE","LeaseManager RegisterSponsorCall Lease ",lease," sponsorshipTimeOut ",sponsorshipTimeOut);

            lock(sponsorTable)
            {
                DateTime sponsorWaitTime = DateTime.UtcNow.Add(sponsorshipTimeOut);
                sponsorTable[sponsorId] = new SponsorInfo(lease, sponsorId, sponsorWaitTime);
            }
        }

        internal void DeleteSponsor(Object sponsorId)
        {
            lock(sponsorTable)
            {
                sponsorTable.Remove(sponsorId);
            }
        }

        ArrayList tempObjects = new ArrayList(10);

        // Thread Loop
        [System.Security.SecurityCritical]  // auto-generated
        private void LeaseTimeAnalyzer(Object state)
        {
            //BCLDebug.Trace("REMOTE","LeaseManager LeaseTimeAnalyzer Entry ",state);

            // Find expired leases
            DateTime now = DateTime.UtcNow;
            lock(leaseToTimeTable)
            {
                IDictionaryEnumerator e = leaseToTimeTable.GetEnumerator();

                while (e.MoveNext())
                {
                    DateTime time = (DateTime)e.Value;
                    Lease lease = (Lease)e.Key;
                    //BCLDebug.Trace("REMOTE","LeaseManager LeaseTimeAnalyzer lease ",lease.id, " lease time ", time, " now ", now);
                    if (time.CompareTo(now) < 0)
                    {
                        // lease expired
                        tempObjects.Add(lease);
                    }
                }
                for (int i=0; i<tempObjects.Count; i++)
                {
                    Lease lease = (Lease)tempObjects[i];
                    //BCLDebug.Trace("REMOTE","LeaseManager LeaseTimeAnalyzer lease Expired remove from leaseToTimeTable ",lease.id);
                    leaseToTimeTable.Remove(lease);
                }

            }

            // Need to run this without lock on leaseToTimeTable to avoid deadlock
            for (int i=0; i<tempObjects.Count; i++)
            {
                Lease lease = (Lease)tempObjects[i];
                //BCLDebug.Trace("REMOTE","LeaseManager LeaseTimeAnalyzer lease Expired ",lease.id);
                if (lease != null) // Lease could be deleted if there is more then one reference to the lease
                    lease.LeaseExpired(now);
            }

            tempObjects.Clear();                

            lock(sponsorTable)
            {
                IDictionaryEnumerator e = sponsorTable.GetEnumerator();

                while (e.MoveNext())
                {
                    // Check for SponshipTimeOuts
                    Object sponsorId = e.Key;
                    SponsorInfo sponsorInfo = (SponsorInfo)e.Value;
                    //BCLDebug.Trace("REMOTE","LeaseManager LeaseTimeAnalyzer sponsor time ", sponsorInfo.sponsorWaitTime, " now ", now);                    
                    if (sponsorInfo.sponsorWaitTime.CompareTo(now) < 0)
                    {
                        // Sponsortimeout expired expired
                        tempObjects.Add(sponsorInfo);
                    }
                }

                // Process the timed out sponsors
                for (int i=0; i<tempObjects.Count; i++)
                {
                    SponsorInfo sponsorInfo = (SponsorInfo)tempObjects[i];
                    //BCLDebug.Trace("REMOTE","LeaseManager LeaseTimeAnalyzer sponsor Expired remove from spansorTable", sponsorInfo.sponsorId);                    
                    sponsorTable.Remove(sponsorInfo.sponsorId);
                }
            }

            // Process the timed out sponsors
            // Need to run this without lock on sponsorTable to avoid deadlock
            for (int i=0; i<tempObjects.Count; i++)
            {
                SponsorInfo sponsorInfo = (SponsorInfo)tempObjects[i];
                //BCLDebug.Trace("REMOTE","LeaseManager LeaseTimeAnalyzer sponsor Expired ", sponsorInfo.sponsorId);                    
                if (sponsorInfo != null && sponsorInfo.lease != null){
                    sponsorInfo.lease.SponsorTimeout(sponsorInfo.sponsorId);
                    tempObjects[i] = null;
                }
            }

            tempObjects.Clear();
            leaseTimer.Change((int)pollTime.TotalMilliseconds, Timeout.Infinite);

            //BCLDebug.Trace("REMOTE","LeaseManager LeaseTimeAnalyzer Exit");
        }

    }
}




