//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using Microsoft.Win32;


    delegate void NeighborClosedHandler(IPeerNeighbor neighbor);
    delegate void NeighborConnectedHandler(IPeerNeighbor neighbor);
    delegate void MaintainerClosedHandler();
    delegate void ReferralsAddedHandler(IList<Referral> referrals, IPeerNeighbor neighbor);

    interface IPeerMaintainer
    {
        event NeighborClosedHandler NeighborClosed;
        event NeighborConnectedHandler NeighborConnected;
        event MaintainerClosedHandler MaintainerClosed;
        event ReferralsAddedHandler ReferralsAdded;

        int ConnectedNeighborCount { get; }
        int NonClosingNeighborCount { get; }
        bool IsOpen { get; }

        IAsyncResult BeginOpenNeighbor(PeerNodeAddress to, TimeSpan timeout, AsyncCallback callback, object asyncState);
        IPeerNeighbor EndOpenNeighbor(IAsyncResult result);

        void CloseNeighbor(IPeerNeighbor neighbor, PeerCloseReason closeReason);

        IPeerNeighbor FindDuplicateNeighbor(PeerNodeAddress address);
        PeerNodeAddress GetListenAddress();
        IPeerNeighbor GetLeastUsefulNeighbor();
    }

    interface IConnectAlgorithms : IDisposable
    {
        void Connect(TimeSpan timeout);
        void Initialize(IPeerMaintainer maintainer, PeerNodeConfig config, int wantedConnectedNeighbors, Dictionary<EndpointAddress, Referral> referralCache);
        void PruneConnections();
        void UpdateEndpointsCollection(ICollection<PeerNodeAddress> src);
    }

    class PeerMaintainerBase<TConnectAlgorithms> : IPeerMaintainer where TConnectAlgorithms : IConnectAlgorithms, new()
    {
        public delegate void ConnectCallback(Exception e);

        ConnectCallback connectCallback;

        PeerNodeConfig config;
        PeerFlooder flooder;
        PeerNeighborManager neighborManager;
        Dictionary<EndpointAddress, Referral> referralCache;
        object thisLock;
        PeerNodeTraceRecord traceRecord;

        // Double-checked locking pattern requires volatile for read/write synchronization
        volatile bool isRunningMaintenance = false;                    // true indicates performing connection Maintenance
        volatile bool isOpen = false;
        IOThreadTimer maintainerTimer;
        public event ReferralsAddedHandler ReferralsAdded;

        object ThisLock
        {
            get { return thisLock; }
        }

        public PeerMaintainerBase(PeerNodeConfig config, PeerNeighborManager neighborManager, PeerFlooder flooder)
        {
            this.neighborManager = neighborManager;
            this.flooder = flooder;
            this.config = config;
            thisLock = new object();

            referralCache = new Dictionary<EndpointAddress, Referral>();
            maintainerTimer = new IOThreadTimer(new Action<object>(OnMaintainerTimer), this, false);
        }

        // Maintainer is expected to validate and accept the contents of referrals
        // and to determine how many referrals it will accept from the array.
        // Neighbor reference is passed in case the Maintainer decided to reject a referral 
        // based on invalid content and close the neighbor.
        public bool AddReferrals(IList<Referral> referrals, IPeerNeighbor neighbor)
        {
            Fx.Assert(null != config.Resolver, "");

            bool valid = true;
            bool canShareReferrals = false;
            try
            {
                canShareReferrals = config.Resolver.CanShareReferrals;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(SR.GetString(SR.ResolverException), e);
            }
            if (referrals != null && canShareReferrals)
            {
                foreach (Referral referral in referrals)
                {
                    // If any referral is invalid then the connection is bad so don't accept any referals from this neighbor.
                    if (referral == null
                    || referral.NodeId == PeerTransportConstants.InvalidNodeId
                    || !PeerValidateHelper.ValidNodeAddress(referral.Address)
                    || !PeerValidateHelper.ValidReferralNodeAddress(referral.Address))
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid)
                {
                    lock (ThisLock)
                    {
                        foreach (Referral referral in referrals)
                        {
                            EndpointAddress key = referral.Address.EndpointAddress;
                            if (referralCache.Count <= this.config.MaxReferralCacheSize && !referralCache.ContainsKey(key))
                            {
                                referralCache.Add(key, referral);
                            }
                        }
                    }

                    // Invoke any handler that is interested in Referrals being added.
                    ReferralsAddedHandler handler = ReferralsAdded;
                    if (handler != null)
                    {
                        ReferralsAdded(referrals, neighbor);
                    }
                }
            }
            return valid;
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        public void Close()
        {
            lock (ThisLock)
            {
                isOpen = false;
            }
            maintainerTimer.Cancel();                        // No reconnect while closed
            SystemEvents.PowerModeChanged -= new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
            MaintainerClosedHandler handler = MaintainerClosed;
            if (handler != null)
            {
                handler();
            }
        }

        void InitialConnection(object dummy)
        {
            // Are we open and is any maintenance activity occuring
            if (isOpen)
            {
                bool continueMaintenance = false;
                if (!isRunningMaintenance)
                {
                    lock (ThisLock)
                    {
                        if (!isRunningMaintenance)
                        {
                            isRunningMaintenance = true;
                            continueMaintenance = true;
                        }
                    }
                }
                if (continueMaintenance)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        PeerMaintainerTraceRecord record = new PeerMaintainerTraceRecord(SR.GetString(SR.PeerMaintainerInitialConnect, this.config.MeshId));
                        TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PeerMaintainerActivity, SR.GetString(SR.TraceCodePeerMaintainerActivity),
                            record, this, null);
                    }

                    TimeoutHelper timeoutHelper = new TimeoutHelper(config.MaintainerTimeout);
                    Exception exception = null;
                    // The connection algorithm may be pluggable if we provide an api or metadata to enable it.
                    // I am sure that research would be interested in doing such a thing.
                    try
                    {
                        maintainerTimer.Cancel();                   // No reconnect until after connect has succeeded

                        using (IConnectAlgorithms connectAlgorithm = (IConnectAlgorithms)new TConnectAlgorithms())
                        {
                            connectAlgorithm.Initialize(this, config, config.MinNeighbors, referralCache);
                            if (referralCache.Count == 0)
                            {
                                ReadOnlyCollection<PeerNodeAddress> addresses = ResolveNewAddresses(timeoutHelper.RemainingTime(), false);
                                connectAlgorithm.UpdateEndpointsCollection(addresses);
                            }
                            if (isOpen)
                            {
                                connectAlgorithm.Connect(timeoutHelper.RemainingTime());
                            }
                        }
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        exception = e;                              // Exeption is saved and transferred
                    }
                    if (isOpen)
                    {
                        try
                        {
                            lock (ThisLock)
                            {
                                if (isOpen)
                                {
                                    // No reconnect until after connect has succeeded
                                    if (neighborManager.ConnectedNeighborCount < 1)
                                    {
                                        maintainerTimer.Set(config.MaintainerRetryInterval);
                                    }
                                    else
                                    {
                                        maintainerTimer.Set(config.MaintainerInterval);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e)) throw;
                            DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                            if (exception == null) exception = e;                // Exeption is saved and transferred via callback
                        }
                    }
                    lock (ThisLock)
                    {
                        isRunningMaintenance = false;
                    }
                    if (connectCallback != null)
                    {
                        connectCallback(exception);
                    }
                }
            }
        }

        // This activity maintains the connected nodes
        void MaintainConnections(object dummy)
        {
            // Are we open and is any maintenance activity occuring
            if (isOpen)
            {
                bool continueMaintenance = false;
                if (!isRunningMaintenance)
                {
                    lock (ThisLock)
                    {
                        if (!isRunningMaintenance)
                        {
                            isRunningMaintenance = true;
                            continueMaintenance = true;
                        }
                    }
                }
                if (continueMaintenance)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        PeerMaintainerTraceRecord record = new PeerMaintainerTraceRecord(SR.GetString(SR.PeerMaintainerStarting, this.config.MeshId));
                        TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PeerMaintainerActivity, SR.GetString(SR.TraceCodePeerMaintainerActivity),
                            record, this, null);
                    }

                    TimeoutHelper timeoutHelper = new TimeoutHelper(config.MaintainerTimeout);
                    try
                    {
                        maintainerTimer.Cancel();                               // No reconnect until after connect has succeeded

                        int currentlyConnected = neighborManager.ConnectedNeighborCount;
                        if (currentlyConnected != config.IdealNeighbors)        // Already at ideal no work to do
                        {
                            using (IConnectAlgorithms connectAlgorithm = (IConnectAlgorithms)new TConnectAlgorithms())
                            {
                                connectAlgorithm.Initialize(this, config, config.IdealNeighbors, referralCache);
                                if (currentlyConnected > config.IdealNeighbors)
                                {
                                    if (DiagnosticUtility.ShouldTraceInformation)
                                    {
                                        PeerMaintainerTraceRecord record = new PeerMaintainerTraceRecord(SR.GetString(SR.PeerMaintainerPruneMode, this.config.MeshId));
                                        TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PeerMaintainerActivity, SR.GetString(SR.TraceCodePeerMaintainerActivity),
                                            record, this, null);
                                    }
                                    connectAlgorithm.PruneConnections();
                                }

                                // During Prune some other neighbor may have gone away which leaves us below Ideal
                                // So try to reconnect
                                currentlyConnected = neighborManager.ConnectedNeighborCount;
                                if (currentlyConnected < config.IdealNeighbors)
                                {
                                    if (referralCache.Count == 0)
                                    {
                                        ReadOnlyCollection<PeerNodeAddress> addresses = ResolveNewAddresses(timeoutHelper.RemainingTime(), true);
                                        connectAlgorithm.UpdateEndpointsCollection(addresses);
                                    }
                                    if (DiagnosticUtility.ShouldTraceInformation)
                                    {
                                        PeerMaintainerTraceRecord record = new PeerMaintainerTraceRecord(SR.GetString(SR.PeerMaintainerConnectMode, this.config.MeshId));
                                        TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PeerMaintainerActivity, SR.GetString(SR.TraceCodePeerMaintainerActivity),
                                            record, this, null);
                                    }
                                    connectAlgorithm.Connect(timeoutHelper.RemainingTime());
                                }
                            }
                        }
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        // We ---- all non Fatal exceptions because this is a worker thread, with no user code waiting
                    }
                    finally
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            PeerMaintainerTraceRecord record = new PeerMaintainerTraceRecord("Maintainer cycle finish");
                            TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PeerMaintainerActivity, SR.GetString(SR.TraceCodePeerMaintainerActivity),
                                record, this, null);
                        }
                    }
                    ResetMaintenance();
                }
            }
        }

        void OnMaintainerTimer(object state)
        {
            ActionItem.Schedule(new Action<object>(MaintainConnections), null);
        }

        public void RefreshConnection()
        {
            // Are we open and is any maintenance activity occuring
            if (isOpen)
            {
                bool continueMaintenance = false;
                if (!isRunningMaintenance)
                {
                    lock (ThisLock)
                    {
                        if (!isRunningMaintenance)
                        {
                            isRunningMaintenance = true;
                            continueMaintenance = true;
                        }
                    }
                }
                if (continueMaintenance)
                {
                    try
                    {
                        TimeoutHelper timeoutHelper = new TimeoutHelper(config.MaintainerTimeout);
                        maintainerTimer.Cancel();                   // No maintainer until after connect has succeeded

                        using (IConnectAlgorithms connectAlgorithm = (IConnectAlgorithms)new TConnectAlgorithms())
                        {
                            // Always go to the resolver for RefreshConnection
                            ReadOnlyCollection<PeerNodeAddress> addresses = ResolveNewAddresses(timeoutHelper.RemainingTime(), true);
                            connectAlgorithm.Initialize(this, config, neighborManager.ConnectedNeighborCount + 1, new Dictionary<EndpointAddress, Referral>());
                            if (addresses.Count > 0)
                            {
                                if (isOpen)
                                {
                                    connectAlgorithm.UpdateEndpointsCollection(addresses);
                                    connectAlgorithm.Connect(timeoutHelper.RemainingTime());
                                }
                            }
                        }
                    }
                    finally
                    {
                        ResetMaintenance();
                    }
                }
            }
        }

        void ResetMaintenance()
        {
            if (isOpen)
            {
                lock (ThisLock)
                {
                    if (isOpen)
                    {
                        try
                        {
                            maintainerTimer.Set(config.MaintainerInterval);                 // No reconnect until after connect has succeeded
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e)) throw;
                            DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                            // We ---- all non Fatal exceptions because this is a worker thread, with no user code waiting
                        }
                    }
                }
            }
            lock (ThisLock)
            {
                isRunningMaintenance = false;
            }
        }

        public void ScheduleConnect(ConnectCallback connectCallback)
        {
            this.connectCallback = connectCallback;
            ActionItem.Schedule(new Action<object>(InitialConnection), null);
        }

        public Referral[] GetReferrals()
        {
            Fx.Assert(null != config.Resolver, "");

            Referral[] referrals = null;
            bool canShareReferrals = false;
            try
            {
                canShareReferrals = config.Resolver.CanShareReferrals;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(SR.GetString(SR.ResolverException), e);
            }

            if (canShareReferrals)
            {
                List<IPeerNeighbor> neighbors = this.neighborManager.GetConnectedNeighbors();
                int count = Math.Min(this.config.MaxReferrals, neighbors.Count);
                referrals = new Referral[count];
                for (int i = 0; i < count; i++)
                {
                    referrals[i] = new Referral(neighbors[i].NodeId, neighbors[i].ListenAddress);
                }
            }
            else
            {
                referrals = new Referral[0];
            }
            return referrals;
        }

        // Notify whoever is interested in NeighborClosed, and start the Maintenance algorithms at threshold
        public virtual void OnNeighborClosed(IPeerNeighbor neighbor)
        {
            if (isOpen)
            {
                lock (ThisLock)
                {
                    if (neighbor != null && neighbor.ListenAddress != null)
                    {
                        EndpointAddress key = neighbor.ListenAddress.EndpointAddress;
                    }

                    if (isOpen && !isRunningMaintenance && neighborManager.ConnectedNeighborCount < config.MinNeighbors)
                    {
                        maintainerTimer.Set(0);
                    }
                }
            }

            NeighborClosedHandler handler = NeighborClosed;
            if (handler != null)
            {
                handler(neighbor);
            }
        }

        public virtual void OnNeighborConnected(IPeerNeighbor neighbor)
        {
            NeighborConnectedHandler handler = NeighborConnected;
            if (handler != null)
            {
                handler(neighbor);
            }
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        public void Open()
        {
            traceRecord = new PeerNodeTraceRecord(config.NodeId);

            if (isRunningMaintenance)
            {
                return;
            }
            lock (ThisLock)
            {
                SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);
                isOpen = true;
            }
        }

        // Get some addresses and make sure they are not in my neighborlist
        ReadOnlyCollection<PeerNodeAddress> ResolveNewAddresses(TimeSpan timeLeft, bool retryResolve)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeLeft);
            Dictionary<string, PeerNodeAddress> alreadySeen = new Dictionary<string, PeerNodeAddress>();
            List<PeerNodeAddress> reply = new List<PeerNodeAddress>();

            // Is this address me
            PeerNodeAddress lclNodeAddress = config.GetListenAddress(true);
            alreadySeen.Add(lclNodeAddress.ServicePath, lclNodeAddress);

            // Maximum of 2 resolves to get new addresses - if the resolver doesn't return us good addresses in 2 goes (8 randomly returned addresses)
            // it is probably messing with us
            int nresolves = (retryResolve) ? 2 : 1;
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                PeerMaintainerTraceRecord record = new PeerMaintainerTraceRecord("Resolving");
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PeerMaintainerActivity, SR.GetString(SR.TraceCodePeerMaintainerActivity),
                    record, this, null);
            }

            for (int i = 0; i < nresolves && reply.Count < config.MaxResolveAddresses && isOpen && timeoutHelper.RemainingTime() > TimeSpan.Zero; i++)
            {
                ReadOnlyCollection<PeerNodeAddress> addresses;
                try
                {
                    addresses = config.Resolver.Resolve(config.MeshId, config.MaxResolveAddresses, timeoutHelper.RemainingTime());
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e)) throw;
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        PeerMaintainerTraceRecord record = new PeerMaintainerTraceRecord("Resolve exception " + e.Message);
                        TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PeerMaintainerActivity, SR.GetString(SR.TraceCodePeerMaintainerActivity),
                            record, this, null);
                    }

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(SR.GetString(SR.ResolverException), e));
                }

                if (addresses != null)
                {
                    foreach (PeerNodeAddress address in addresses)
                    {
                        if (!alreadySeen.ContainsKey(address.ServicePath))
                        {
                            alreadySeen.Add(address.ServicePath, address);
                            if (((IPeerMaintainer)this).FindDuplicateNeighbor(address) == null)
                            {
                                reply.Add(address);
                            }
                        }
                    }
                }
            }
            return (new ReadOnlyCollection<PeerNodeAddress>(reply));
        }

        // interface IPeerMaintainer implementation
        public event NeighborClosedHandler NeighborClosed;
        public event NeighborConnectedHandler NeighborConnected;
        public event MaintainerClosedHandler MaintainerClosed;

        void IPeerMaintainer.CloseNeighbor(IPeerNeighbor neighbor, PeerCloseReason closeReason)
        {
            neighborManager.CloseNeighbor(neighbor, closeReason, PeerCloseInitiator.LocalNode);
        }

        IPeerNeighbor IPeerMaintainer.FindDuplicateNeighbor(PeerNodeAddress address)
        {
            return neighborManager.FindDuplicateNeighbor(address);
        }

        PeerNodeAddress IPeerMaintainer.GetListenAddress()
        {
            return config.GetListenAddress(true);
        }

        IPeerNeighbor IPeerMaintainer.GetLeastUsefulNeighbor()
        {
            IPeerNeighbor leastUsefulNeighbor = null;
            uint minUtility = UInt32.MaxValue;

            foreach (IPeerNeighbor neighbor in this.neighborManager.GetConnectedNeighbors())
            {
                UtilityExtension utilityExtension = neighbor.Extensions.Find<UtilityExtension>();
                if (utilityExtension != null && utilityExtension.IsAccurate && utilityExtension.LinkUtility < minUtility && !neighbor.IsClosing)
                {
                    minUtility = utilityExtension.LinkUtility;
                    leastUsefulNeighbor = neighbor;
                }
            }
            return leastUsefulNeighbor;
        }

        IAsyncResult IPeerMaintainer.BeginOpenNeighbor(PeerNodeAddress address, TimeSpan timeout, AsyncCallback callback, object asyncState)
        {
            lock (ThisLock)
            {
                EndpointAddress key = address.EndpointAddress;
                if (referralCache.ContainsKey(key))
                {
                    referralCache.Remove(key);
                }
            }

            return neighborManager.BeginOpenNeighbor(address, timeout, callback, asyncState);
        }

        IPeerNeighbor IPeerMaintainer.EndOpenNeighbor(IAsyncResult result)
        {
            return neighborManager.EndOpenNeighbor(result);
        }

        int IPeerMaintainer.ConnectedNeighborCount
        {
            get { return neighborManager.ConnectedNeighborCount; }
        }

        int IPeerMaintainer.NonClosingNeighborCount
        {
            get { return neighborManager.NonClosingNeighborCount; }
        }

        bool IPeerMaintainer.IsOpen
        {
            get { return isOpen; }
        }

        public void PingConnections()
        {
            neighborManager.PingNeighbors();
        }

        public void PingAndRefresh(object state)
        {
            PingConnections();
            if (this.neighborManager.ConnectedNeighborCount < this.config.IdealNeighbors)
                MaintainConnections(null);
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode != PowerModes.Resume)
                return;
            if (!isOpen)
                return;
            ActionItem.Schedule(new Action<object>(PingAndRefresh), null);
        }

    }

    partial class PeerMaintainer : PeerMaintainerBase<ConnectAlgorithms>
    {
        public PeerMaintainer(PeerNodeConfig config, PeerNeighborManager neighborManager, PeerFlooder flooder)
            : base(config, neighborManager, flooder)
        {
        }
    }
}
