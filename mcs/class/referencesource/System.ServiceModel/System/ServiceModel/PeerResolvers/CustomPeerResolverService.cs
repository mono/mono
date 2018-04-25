//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.PeerResolvers
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;


    [ObsoleteAttribute ("PeerChannel feature is obsolete and will be removed in the future.", false)]
    [ServiceBehavior(UseSynchronizationContext = false, InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class CustomPeerResolverService : IPeerResolverContract
    {
        internal enum RegistrationState
        {
            OK, Deleted
        }

        internal class RegistrationEntry
        {
            Guid clientId;
            Guid registrationId;
            string meshId;
            DateTime expires;
            PeerNodeAddress address;
            RegistrationState state;


            public RegistrationEntry(Guid clientId, Guid registrationId, string meshId, DateTime expires, PeerNodeAddress address)
            {
                this.ClientId = clientId;
                this.RegistrationId = registrationId;
                this.MeshId = meshId;
                this.Expires = expires;
                this.Address = address;
                this.State = RegistrationState.OK;
            }

            public Guid ClientId
            {
                get { return clientId; }
                set { clientId = value; }
            }

            public Guid RegistrationId
            {
                get { return registrationId; }
                set { registrationId = value; }
            }

            public string MeshId
            {
                get { return meshId; }
                set { meshId = value; }
            }

            public DateTime Expires
            {
                get { return expires; }
                set { expires = value; }
            }

            public PeerNodeAddress Address
            {
                get { return address; }
                set { address = value; }
            }

            public RegistrationState State
            {
                get { return state; }
                set { state = value; }
            }

        }

        internal class LiteLock
        {
            bool forWrite;
            bool upgraded;
            ReaderWriterLock locker;
            TimeSpan timeout = TimeSpan.FromMinutes(1);
            LockCookie lc;

            LiteLock(ReaderWriterLock locker, bool forWrite)
            {
                this.locker = locker;
                this.forWrite = forWrite;
            }

            public static void Acquire(out LiteLock liteLock, ReaderWriterLock locker)
            {
                Acquire(out liteLock, locker, false);
            }

            public static void Acquire(out LiteLock liteLock, ReaderWriterLock locker, bool forWrite)
            {
                LiteLock theLock = new LiteLock(locker, forWrite);
                try { }
                finally
                {
                    if (forWrite)
                    {
                        locker.AcquireWriterLock(theLock.timeout);
                    }
                    else
                    {
                        locker.AcquireReaderLock(theLock.timeout);
                    }
                    liteLock = theLock;
                }
            }

            public static void Release(LiteLock liteLock)
            {
                if (liteLock == null)
                {
                    return;
                }

                if (liteLock.forWrite)
                {
                    liteLock.locker.ReleaseWriterLock();
                }
                else
                {
                    Fx.Assert(!liteLock.upgraded, "Can't release while upgraded!");
                    liteLock.locker.ReleaseReaderLock();
                }
            }
            public void UpgradeToWriterLock()
            {
                Fx.Assert(!forWrite, "Invalid call to Upgrade!!");
                Fx.Assert(!upgraded, "Already upgraded!");
                try { }
                finally
                {
                    lc = locker.UpgradeToWriterLock(timeout);
                    upgraded = true;
                }
            }
            public void DowngradeFromWriterLock()
            {
                Fx.Assert(!forWrite, "Invalid call to Downgrade!!");
                if (upgraded)
                {
                    locker.DowngradeFromWriterLock(ref lc);
                    upgraded = false;
                }
            }
        }

        internal class MeshEntry
        {
            Dictionary<Guid, RegistrationEntry> entryTable;
            Dictionary<string, RegistrationEntry> service2EntryTable;
            List<RegistrationEntry> entryList;
            ReaderWriterLock gate;

            internal MeshEntry()
            {
                EntryTable = new Dictionary<Guid, RegistrationEntry>();
                Service2EntryTable = new Dictionary<string, RegistrationEntry>();
                EntryList = new List<RegistrationEntry>();
                Gate = new ReaderWriterLock();
            }

            public Dictionary<Guid, RegistrationEntry> EntryTable
            {
                get { return entryTable; }
                set { entryTable = value; }
            }

            public Dictionary<string, RegistrationEntry> Service2EntryTable
            {
                get { return service2EntryTable; }
                set { service2EntryTable = value; }
            }

            public List<RegistrationEntry> EntryList
            {
                get { return entryList; }
                set { entryList = value; }
            }

            public ReaderWriterLock Gate
            {
                get { return gate; }
                set { gate = value; }
            }
        }

        Dictionary<string, MeshEntry> meshId2Entry = new Dictionary<string, MeshEntry>();
        ReaderWriterLock gate;

        TimeSpan timeout = TimeSpan.FromMinutes(1);
        TimeSpan cleanupInterval = TimeSpan.FromMinutes(1);
        TimeSpan refreshInterval = TimeSpan.FromMinutes(10);
        bool controlShape;
        bool isCleaning;
        IOThreadTimer timer;
        object thisLock = new object();
        bool opened;
        TimeSpan LockWait = TimeSpan.FromSeconds(5);

        public CustomPeerResolverService()
        {
            isCleaning = false;
            gate = new ReaderWriterLock();
        }

        public TimeSpan CleanupInterval
        {
            get
            {
                return cleanupInterval;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                lock (ThisLock)
                {
                    ThrowIfOpened("Set CleanupInterval");
                    this.cleanupInterval = value;
                }
            }
        }

        public TimeSpan RefreshInterval
        {
            get
            {
                return refreshInterval;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }

                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                lock (ThisLock)
                {
                    ThrowIfOpened("Set RefreshInterval");
                    this.refreshInterval = value;
                }
            }
        }

        public bool ControlShape
        {
            get
            {
                return this.controlShape;
            }
            set
            {
                lock (ThisLock)
                {
                    ThrowIfOpened("Set ControlShape");
                    this.controlShape = value;
                }
            }
        }

        MeshEntry GetMeshEntry(string meshId) { return GetMeshEntry(meshId, true); }
        MeshEntry GetMeshEntry(string meshId, bool createIfNotExists)
        {
            MeshEntry meshEntry = null;
            LiteLock ll = null;
            try
            {
                LiteLock.Acquire(out ll, gate);
                if (!this.meshId2Entry.TryGetValue(meshId, out meshEntry) && createIfNotExists)
                {
                    meshEntry = new MeshEntry();
                    try
                    {
                        ll.UpgradeToWriterLock();
                        meshId2Entry.Add(meshId, meshEntry);
                    }
                    finally
                    {
                        ll.DowngradeFromWriterLock();
                    }
                }
            }
            finally
            {
                LiteLock.Release(ll);
            }
            Fx.Assert(meshEntry != null || !createIfNotExists, "GetMeshEntry failed to get an entry!");
            return meshEntry;
        }

        public virtual RegisterResponseInfo Register(Guid clientId, string meshId, PeerNodeAddress address)
        {
            Guid registrationId = Guid.NewGuid();
            DateTime expiry = DateTime.UtcNow + RefreshInterval;

            RegistrationEntry entry = null;
            MeshEntry meshEntry = null;

            lock (ThisLock)
            {
                entry = new RegistrationEntry(clientId, registrationId, meshId, expiry, address);
                meshEntry = GetMeshEntry(meshId);
                if (meshEntry.Service2EntryTable.ContainsKey(address.ServicePath))
                    PeerExceptionHelper.ThrowInvalidOperation_DuplicatePeerRegistration(address.ServicePath);
                LiteLock ll = null;

                try
                {
                    // meshEntry.gate can be held by this thread for write if this is coming from update
                    // else MUST not be held at all.
                    if (!meshEntry.Gate.IsWriterLockHeld)
                    {
                        LiteLock.Acquire(out ll, meshEntry.Gate, true);
                    }
                    meshEntry.EntryTable.Add(registrationId, entry);
                    meshEntry.EntryList.Add(entry);
                    meshEntry.Service2EntryTable.Add(address.ServicePath, entry);
                }
                finally
                {
                    if (ll != null)
                        LiteLock.Release(ll);
                }
            }
            return new RegisterResponseInfo(registrationId, RefreshInterval);
        }

        public virtual RegisterResponseInfo Register(RegisterInfo registerInfo)
        {
            if (registerInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("registerInfo", SR.GetString(SR.PeerNullRegistrationInfo));
            }

            ThrowIfClosed("Register");

            if (!registerInfo.HasBody() || String.IsNullOrEmpty(registerInfo.MeshId))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("registerInfo", SR.GetString(SR.PeerInvalidMessageBody, registerInfo));
            }
            return Register(registerInfo.ClientId, registerInfo.MeshId, registerInfo.NodeAddress);
        }

        public virtual RegisterResponseInfo Update(UpdateInfo updateInfo)
        {
            if (updateInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("updateInfo", SR.GetString(SR.PeerNullRegistrationInfo));
            }

            ThrowIfClosed("Update");

            if (!updateInfo.HasBody() || String.IsNullOrEmpty(updateInfo.MeshId) || updateInfo.NodeAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("updateInfo", SR.GetString(SR.PeerInvalidMessageBody, updateInfo));
            }

            Guid registrationId = updateInfo.RegistrationId;
            RegistrationEntry entry;

            MeshEntry meshEntry = GetMeshEntry(updateInfo.MeshId);
            LiteLock ll = null;

            //handle cases when Update ----s with Register.
            if (updateInfo.RegistrationId == Guid.Empty || meshEntry == null)
                return Register(updateInfo.ClientId, updateInfo.MeshId, updateInfo.NodeAddress);
            //
            // preserve locking order between ThisLock and the LiteLock.
            lock (ThisLock)
            {
                try
                {
                    LiteLock.Acquire(out ll, meshEntry.Gate);
                    if (!meshEntry.EntryTable.TryGetValue(updateInfo.RegistrationId, out entry))
                    {
                        try
                        {
                            // upgrade to writer lock
                            ll.UpgradeToWriterLock();
                            return Register(updateInfo.ClientId, updateInfo.MeshId, updateInfo.NodeAddress);
                        }
                        finally
                        {
                            ll.DowngradeFromWriterLock();
                        }
                    }
                    lock (entry)
                    {
                        entry.Address = updateInfo.NodeAddress;
                        entry.Expires = DateTime.UtcNow + this.RefreshInterval;
                    }
                }
                finally
                {
                    LiteLock.Release(ll);
                }
            }
            return new RegisterResponseInfo(registrationId, RefreshInterval);
        }

        public virtual ResolveResponseInfo Resolve(ResolveInfo resolveInfo)
        {
            if (resolveInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("resolveInfo", SR.GetString(SR.PeerNullResolveInfo));
            }

            ThrowIfClosed("Resolve");

            if (!resolveInfo.HasBody())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("resolveInfo", SR.GetString(SR.PeerInvalidMessageBody, resolveInfo));
            }

            int currentCount = 0;
            int index = 0;
            int maxEntries = resolveInfo.MaxAddresses;
            ResolveResponseInfo response = new ResolveResponseInfo();
            List<PeerNodeAddress> results = new List<PeerNodeAddress>();
            List<RegistrationEntry> entries = null;
            PeerNodeAddress address;
            RegistrationEntry entry;
            MeshEntry meshEntry = GetMeshEntry(resolveInfo.MeshId, false);
            if (meshEntry != null)
            {
                LiteLock ll = null;
                try
                {
                    LiteLock.Acquire(out ll, meshEntry.Gate);
                    entries = meshEntry.EntryList;
                    if (entries.Count <= maxEntries)
                    {
                        foreach (RegistrationEntry e in entries)
                        {
                            results.Add(e.Address);
                        }
                    }
                    else
                    {
                        Random random = new Random();
                        while (currentCount < maxEntries)
                        {
                            index = random.Next(entries.Count);
                            entry = entries[index];
                            Fx.Assert(entry.State == RegistrationState.OK, "A deleted registration is still around!");
                            address = entry.Address;
                            if (!results.Contains(address))
                                results.Add(address);
                            currentCount++;
                        }
                    }
                }
                finally
                {
                    LiteLock.Release(ll);
                }
            }
            response.Addresses = results.ToArray();
            return response;
        }

        public virtual void Unregister(UnregisterInfo unregisterInfo)
        {
            if (unregisterInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("unregisterinfo", SR.GetString(SR.PeerNullRegistrationInfo));
            }

            ThrowIfClosed("Unregister");

            if (!unregisterInfo.HasBody() || String.IsNullOrEmpty(unregisterInfo.MeshId) || unregisterInfo.RegistrationId == Guid.Empty)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("unregisterInfo", SR.GetString(SR.PeerInvalidMessageBody, unregisterInfo));
            }

            RegistrationEntry registration = null;
            MeshEntry meshEntry = GetMeshEntry(unregisterInfo.MeshId, false);
            //there could be a ---- that two different threads could be working on the same entry
            //we wont optimize for that case.
            LiteLock ll = null;
            try
            {
                LiteLock.Acquire(out ll, meshEntry.Gate, true);
                if (!meshEntry.EntryTable.TryGetValue(unregisterInfo.RegistrationId, out registration))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("unregisterInfo", SR.GetString(SR.PeerInvalidMessageBody, unregisterInfo));
                meshEntry.EntryTable.Remove(unregisterInfo.RegistrationId);
                meshEntry.EntryList.Remove(registration);
                meshEntry.Service2EntryTable.Remove(registration.Address.ServicePath);
                registration.State = RegistrationState.Deleted;
            }
            finally
            {
                LiteLock.Release(ll);
            }
        }

        public virtual RefreshResponseInfo Refresh(RefreshInfo refreshInfo)
        {
            if (refreshInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("refreshInfo", SR.GetString(SR.PeerNullRefreshInfo));
            }

            ThrowIfClosed("Refresh");

            if (!refreshInfo.HasBody() || String.IsNullOrEmpty(refreshInfo.MeshId) || refreshInfo.RegistrationId == Guid.Empty)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("refreshInfo", SR.GetString(SR.PeerInvalidMessageBody, refreshInfo));
            }
            RefreshResult result = RefreshResult.RegistrationNotFound;
            RegistrationEntry entry = null;
            MeshEntry meshEntry = GetMeshEntry(refreshInfo.MeshId, false);
            LiteLock ll = null;

            if (meshEntry != null)
            {
                try
                {
                    LiteLock.Acquire(out ll, meshEntry.Gate);
                    if (!meshEntry.EntryTable.TryGetValue(refreshInfo.RegistrationId, out entry))
                        return new RefreshResponseInfo(RefreshInterval, result);
                    lock (entry)
                    {
                        if (entry.State == RegistrationState.OK)
                        {
                            entry.Expires = DateTime.UtcNow + RefreshInterval;
                            result = RefreshResult.Success;
                        }
                    }
                }
                finally
                {
                    LiteLock.Release(ll);
                }
            }
            return new RefreshResponseInfo(RefreshInterval, result);
        }

        public virtual ServiceSettingsResponseInfo GetServiceSettings()
        {
            ThrowIfClosed("GetServiceSettings");
            ServiceSettingsResponseInfo info = new ServiceSettingsResponseInfo(this.ControlShape);
            return info;
        }

        public virtual void Open()
        {
            ThrowIfOpened("Open");
            if (this.refreshInterval <= TimeSpan.Zero)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("RefreshInterval", SR.GetString(SR.RefreshIntervalMustBeGreaterThanZero, this.refreshInterval));
            if (this.CleanupInterval <= TimeSpan.Zero)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("CleanupInterval", SR.GetString(SR.CleanupIntervalMustBeGreaterThanZero, this.cleanupInterval));

            //check that we are good to open
            timer = new IOThreadTimer(new Action<object>(CleanupActivity), null, false);
            timer.Set(CleanupInterval);
            opened = true;
        }

        public virtual void Close()
        {
            ThrowIfClosed("Close");
            timer.Cancel();
            opened = false;
        }

        internal virtual void CleanupActivity(object state)
        {
            if (!opened)
                return;

            if (!isCleaning)
            {
                lock (ThisLock)
                {
                    if (!isCleaning)
                    {
                        isCleaning = true;
                        try
                        {
                            MeshEntry meshEntry = null;
                            //acquire a write lock.  from the reader/writer lock can we postpone until no contention?
                            ICollection<string> keys = null;
                            LiteLock ll = null;
                            try
                            {
                                LiteLock.Acquire(out ll, gate);
                                keys = meshId2Entry.Keys;
                            }
                            finally
                            {
                                LiteLock.Release(ll);
                            }
                            foreach (string meshId in keys)
                            {
                                meshEntry = GetMeshEntry(meshId);
                                CleanupMeshEntry(meshEntry);
                            }
                        }
                        finally
                        {
                            isCleaning = false;
                            if (opened)
                                timer.Set(this.CleanupInterval);
                        }
                    }
                }
            }
        }

        //always call this from a readlock
        void CleanupMeshEntry(MeshEntry meshEntry)
        {
            List<Guid> remove = new List<Guid>();
            if (!opened)
                return;
            LiteLock ll = null;
            try
            {
                LiteLock.Acquire(out ll, meshEntry.Gate, true);
                foreach (KeyValuePair<Guid, RegistrationEntry> item in meshEntry.EntryTable)
                {
                    if ((item.Value.Expires <= DateTime.UtcNow) || (item.Value.State == RegistrationState.Deleted))
                    {
                        remove.Add(item.Key);
                        meshEntry.EntryList.Remove(item.Value);
                        meshEntry.Service2EntryTable.Remove(item.Value.Address.ServicePath);
                    }
                }
                foreach (Guid id in remove)
                {
                    meshEntry.EntryTable.Remove(id);
                }
            }
            finally
            {
                LiteLock.Release(ll);
            }
        }

        object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }
        void ThrowIfOpened(string operation)
        {
            if (opened)
                PeerExceptionHelper.ThrowInvalidOperation_NotValidWhenOpen(operation);
        }
        void ThrowIfClosed(string operation)
        {
            if (!opened)
                PeerExceptionHelper.ThrowInvalidOperation_NotValidWhenClosed(operation);

        }
    }
}

