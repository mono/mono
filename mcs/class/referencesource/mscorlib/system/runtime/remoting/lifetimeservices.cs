// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//+----------------------------------------------------------------------------
//
// Microsoft Windows
// File:        LifetimeServices.cs
//
// Contents:    Used to obtain a lease <



//
//+----------------------------------------------------------------------------

namespace System.Runtime.Remoting.Lifetime

{
    using System;
    using System.Threading;
    using System.Security;
    using System.Security.Permissions;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Messaging;
    using System.Globalization;

        //   access needs to be restricted    
        [System.Security.SecurityCritical]  // auto-generated_required
        [System.Runtime.InteropServices.ComVisible(true)]
        public sealed class LifetimeServices
        {   
            // Set once boolean
            private static bool s_isLeaseTime = false;
            private static bool s_isRenewOnCallTime = false;
            private static bool s_isSponsorshipTimeout = false;
        
            // Default values
            private static long s_leaseTimeTicks = TimeSpan.FromMinutes(5).Ticks;
            private static long s_renewOnCallTimeTicks = TimeSpan.FromMinutes(2).Ticks;
            private static long s_sponsorshipTimeoutTicks = TimeSpan.FromMinutes(2).Ticks;
            private static long s_pollTimeTicks = TimeSpan.FromMilliseconds(10000).Ticks;

            private static TimeSpan GetTimeSpan(ref long ticks)
            {
                return TimeSpan.FromTicks(Volatile.Read(ref ticks));
            }

            private static void SetTimeSpan(ref long ticks, TimeSpan value)
            {
                Volatile.Write(ref ticks, value.Ticks);
            }

            // Testing values
            //private static TimeSpan s_leaseTimeTicks = TimeSpan.FromSeconds(20).Ticks;
            //private static TimeSpan s_renewOnCallTimeTicks = TimeSpan.FromSeconds(20).Ticks;
            //private static TimeSpan s_sponsorshipTimeoutTicks = TimeSpan.FromSeconds(20).Ticks;
            //private static TimeSpan s_pollTimeTicks = TimeSpan.FromMilliseconds(10000).Ticks;

            private static Object s_LifetimeSyncObject = null;

            private static Object LifetimeSyncObject
            {
                get
                {
                    if (s_LifetimeSyncObject == null)
                    {
                        Object o = new Object();
                        Interlocked.CompareExchange(ref s_LifetimeSyncObject, o, null);
                    }
                    return s_LifetimeSyncObject;
                }
            }

            // This should have been a static class, but wasn't as of v3.5.  Clearly, this is
            // broken.  We'll keep this in V4 for binary compat, but marked obsolete as error
            // so migrated source code gets fixed.
            [Obsolete("Do not create instances of the LifetimeServices class.  Call the static methods directly on this type instead", true)]
            public LifetimeServices()
            {
                // Should be a static type - this exists in V4 for binary compatiblity.
            }

            // Initial Lease Time span for appdomain
            public static TimeSpan LeaseTime
            {
                get{ return GetTimeSpan(ref s_leaseTimeTicks); }

                [System.Security.SecurityCritical]  // auto-generated
                [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
                set
                    {
                        lock(LifetimeSyncObject)
                            {
                                if (s_isLeaseTime)
                                    throw new RemotingException(Environment.GetResourceString("Remoting_Lifetime_SetOnce", "LeaseTime"));


                                SetTimeSpan(ref s_leaseTimeTicks, value);
                                s_isLeaseTime = true;
                            }
                    }

            }

            // Initial renew on call time span for appdomain
            public static TimeSpan RenewOnCallTime
            {
                get{ return GetTimeSpan(ref s_renewOnCallTimeTicks); }
                [System.Security.SecurityCritical]  // auto-generated
                [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
                set
                    {
                        lock(LifetimeSyncObject)
                            {
                                if (s_isRenewOnCallTime)
                                    throw new RemotingException(Environment.GetResourceString("Remoting_Lifetime_SetOnce", "RenewOnCallTime"));                        


                                SetTimeSpan(ref s_renewOnCallTimeTicks, value);
                                s_isRenewOnCallTime = true;
                            }
                    }

            }


            // Initial sponsorshiptimeout for appdomain
            public static TimeSpan SponsorshipTimeout

            {
                get{ return GetTimeSpan(ref s_sponsorshipTimeoutTicks); }
                [System.Security.SecurityCritical]  // auto-generated
                [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
                set
                    {
                        lock(LifetimeSyncObject)
                            {
                                if (s_isSponsorshipTimeout)
                                    throw new RemotingException(Environment.GetResourceString("Remoting_Lifetime_SetOnce", "SponsorshipTimeout"));                        
                                SetTimeSpan(ref s_sponsorshipTimeoutTicks, value);
                                s_isSponsorshipTimeout = true;
                            }
                    }

            }


            // Initial sponsorshiptimeout for appdomain
            public static TimeSpan LeaseManagerPollTime

            {
                get{ return GetTimeSpan(ref s_pollTimeTicks); }
                [System.Security.SecurityCritical]  // auto-generated
                [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.RemotingConfiguration)]
                set
                    {
                        lock(LifetimeSyncObject)
                            {
                                SetTimeSpan(ref s_pollTimeTicks, value);
                                if (LeaseManager.IsInitialized())
                                    LeaseManager.GetLeaseManager().ChangePollTime(value);
                            }
                    }

            }

        

            [System.Security.SecurityCritical]  // auto-generated
            internal static ILease GetLeaseInitial(MarshalByRefObject obj)

            {
                ILease lease = null;
                LeaseManager leaseManager = LeaseManager.GetLeaseManager(LeaseManagerPollTime);
                lease = (ILease)leaseManager.GetLease(obj);
                if (lease == null)
                    lease = CreateLease(obj);
                return lease;

            }


            [System.Security.SecurityCritical]  // auto-generated
            internal static ILease GetLease(MarshalByRefObject obj)

            {
                ILease lease = null;
                LeaseManager leaseManager = LeaseManager.GetLeaseManager(LeaseManagerPollTime);
                lease = (ILease)leaseManager.GetLease(obj);
                return lease;            

            }

        


            //internal static ILease CreateLease(MarshalByRefObject obj, IMessageSink nextSink)

            [System.Security.SecurityCritical]  // auto-generated
            internal static ILease CreateLease(MarshalByRefObject obj)        

            {
                return CreateLease(LeaseTime, RenewOnCallTime, SponsorshipTimeout, obj);

            }


            [System.Security.SecurityCritical]  // auto-generated
            internal static ILease CreateLease(TimeSpan leaseTime,
                                               TimeSpan renewOnCallTime,                       
                                               TimeSpan sponsorshipTimeout,
                                               MarshalByRefObject obj
                                               )

            {
                // Will create leaseManager if not already created.
                LeaseManager.GetLeaseManager(LeaseManagerPollTime);            
                return (ILease)(new Lease(leaseTime, renewOnCallTime, sponsorshipTimeout, obj));

            }

        }


    [Serializable]
    internal class LeaseLifeTimeServiceProperty : IContextProperty, IContributeObjectSink    

    {

        public String Name

        {
            [System.Security.SecurityCritical]  // auto-generated
            get {return "LeaseLifeTimeServiceProperty";}

        }


        [System.Security.SecurityCritical]  // auto-generated
        public bool IsNewContextOK(Context newCtx)

        {
            return true;

        }


        [System.Security.SecurityCritical]  // auto-generated
        public void Freeze(Context newContext)

        {

        }


        // Initiates the creation of a lease

        // Creates a sink for invoking a renew on call when an object is created.

        [System.Security.SecurityCritical]  // auto-generated
        public IMessageSink GetObjectSink(MarshalByRefObject obj, 
                                          IMessageSink nextSink)

        {
            bool fServer;
            ServerIdentity identity = (ServerIdentity)MarshalByRefObject.GetIdentity(obj, out fServer);
            BCLDebug.Assert(identity != null, "[LifetimeServices.GetObjectSink] identity != null");

            // NOTE: Single Call objects do not have a lease associated with it because they last 
            // only for the duration of the call. 
            // Singleton objects on the other hand do have leases associated with them and they can 
            // be garbage collected.
            if (identity.IsSingleCall())
            {
                BCLDebug.Trace("REMOTE", "LeaseLifeTimeServiceProperty.GetObjectSink, no lease SingleCall",obj,", NextSink "+nextSink);                
                return nextSink;
            }
    


            // Create lease. InitializeLifetimeService is a virtual method which can be overridded so that a lease with
            // object specific properties can be created.
            Object leaseObj = obj.InitializeLifetimeService();


            BCLDebug.Trace("REMOTE", "LeaseLifeTimeServiceProperty.GetObjectSink, return from InitializeLifetimeService obj ",obj,", lease ",leaseObj);


            // InitializeLifetimeService can return a lease in one of conditions:
            // 1) the lease has a null state which specifies that no lease is to be created.
            // 2) the lease has an initial state which specifies that InitializeLifeTimeService has created a new lease.
            // 3) the lease has another state which indicates that the lease has already been created and registered.


            if (leaseObj == null)
                {
                    BCLDebug.Trace("REMOTE", "LeaseLifeTimeServiceProperty.GetObjectSink, no lease ",obj,", NextSink "+nextSink);
                    return nextSink;
                }

            if (!(leaseObj is System.Runtime.Remoting.Lifetime.ILease))
                throw new RemotingException(Environment.GetResourceString("Remoting_Lifetime_ILeaseReturn", leaseObj));

            ILease ilease = (ILease)leaseObj;
    
            if (ilease.InitialLeaseTime.CompareTo(TimeSpan.Zero) <= 0)
                {
                    // No lease
                    {
                        BCLDebug.Trace("REMOTE", "LeaseLifeTimeServiceProperty.GetObjectSink, no lease because InitialLeaseTime is Zero ",obj);
                        if (ilease is System.Runtime.Remoting.Lifetime.Lease)
                            {
                                ((Lease)ilease).Remove();
                            }
                        return nextSink;
                    }
                }


            Lease lease = null;
            lock(identity)
                {
                    if (identity.Lease != null)
                        {
                            // Lease already exists for object, object is being marsalled again
                            BCLDebug.Trace("REMOTE", "LeaseLifeTimeServiceProperty.GetObjectSink, Lease already exists for object ",obj);                    
                            lease = (Lease)identity.Lease;
                            lease.Renew(lease.InitialLeaseTime); // Reset initial lease time
                        }
                    else
                        {
                            // New lease
                            if (!(ilease is System.Runtime.Remoting.Lifetime.Lease))
                                {
                                    // InitializeLifetimeService created its own ILease object
                                    // Need to create a System.Runtime.Remoting.Lease object
                                    BCLDebug.Trace("REMOTE", "LeaseLifeTimeServiceProperty.GetObjectSink, New Lease, lease not of type Lease  ",obj);                                            
                                    lease = (Lease)LifetimeServices.GetLeaseInitial(obj);
                                    if (lease.CurrentState == LeaseState.Initial)
                                        {
                                            lease.InitialLeaseTime = ilease.InitialLeaseTime;
                                            lease.RenewOnCallTime = ilease.RenewOnCallTime;
                                            lease.SponsorshipTimeout = ilease.SponsorshipTimeout;
                                        }
                                }
                            else
                                {
                                    // An object of Type Lease was created
                                    BCLDebug.Trace("REMOTE", "LeaseLifeTimeServiceProperty.GetObjectSink, New Lease, lease is type Lease  ",obj);                                                                    
                                    lease = (Lease)ilease;
                                }

                            // Put lease in active state
                            // Creation phase of lease is over, properties can no longer be set on lease.
                            identity.Lease = lease; // Place lease into identity for object
                            // If the object has been marshaled activate 
                            // the lease
                            if (identity.ObjectRef != null)
                            {
                                lease.ActivateLease();
                            }
                        }
                }


            if (lease.RenewOnCallTime > TimeSpan.Zero)
                {
                    // RenewOnCall create sink
                    BCLDebug.Trace("REMOTE", "LeaseLifeTimeServiceProperty.GetObjectSink, lease created ",obj);                
                    return new LeaseSink(lease, nextSink);
                }
            else
                {
                    // No RenewOnCall so no sink created
                    BCLDebug.Trace("REMOTE", "LeaseLifeTimeServiceProperty.GetObjectSink, No RenewOnCall so no sink created ",obj);                                
                    return nextSink;
                }

        }

    }

} 

