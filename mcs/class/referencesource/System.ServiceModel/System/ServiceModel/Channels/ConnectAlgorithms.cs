//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using System.ServiceModel.Diagnostics.Application;

    // Graph maintainence algorithms.
    sealed class ConnectAlgorithms : IConnectAlgorithms
    {
        static Random random = new Random();

        int wantedConnectionCount = 0;
        EventWaitHandle addNeighbor = new EventWaitHandle(true, EventResetMode.ManualReset);
        EventWaitHandle maintainerClosed = new EventWaitHandle(false, EventResetMode.ManualReset);
        EventWaitHandle welcomeReceived = new EventWaitHandle(false, EventResetMode.ManualReset);

        Dictionary<Uri, PeerNodeAddress> nodeAddresses = new Dictionary<Uri, PeerNodeAddress>();
        PeerNodeConfig config;
        Dictionary<Uri, PeerNodeAddress> pendingConnectedNeighbor = new Dictionary<Uri, PeerNodeAddress>();
        object thisLock = new object();
        IPeerMaintainer maintainer = null;
        bool disposed = false;

        public void Initialize(IPeerMaintainer maintainer, PeerNodeConfig config, int wantedConnectionCount, Dictionary<EndpointAddress, Referral> referralCache)
        {
            this.maintainer = maintainer;
            this.config = config;
            this.wantedConnectionCount = wantedConnectionCount;
            UpdateEndpointsCollection(referralCache.Values);        // Add to the endpoints connection anything in the referralsCache

            // Hook up the event handlers
            maintainer.NeighborClosed += OnNeighborClosed;
            maintainer.NeighborConnected += OnNeighborConnected;
            maintainer.MaintainerClosed += OnMaintainerClosed;
            maintainer.ReferralsAdded += OnReferralsAdded;
        }

        // instance lock
        object ThisLock
        {
            get { return thisLock; }
        }

        public void Connect(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            addNeighbor.Set();              // We are trying to add a neighbor

            List<IAsyncResult> results = new List<IAsyncResult>();
            List<WaitHandle> handles = new List<WaitHandle>();

            // While we have more to endpoints try and we have connections pending and we are not connected upto ideal yet, and the maintainer is still open
            while (results.Count != 0
                || (((nodeAddresses.Count != 0 || pendingConnectedNeighbor.Count != 0) && maintainer.IsOpen)
                && maintainer.ConnectedNeighborCount < wantedConnectionCount))
            {
                try
                {
                    handles.Clear();
                    foreach (IAsyncResult iar in results)
                    {
                        handles.Add(iar.AsyncWaitHandle);
                    }
                    handles.Add(welcomeReceived);                               // One of our connect requests resulted in a welcome or neighborManager was shutting down
                    handles.Add(maintainerClosed);                              // One of our connect requests resulted in a welcome or neighborManager was shutting down
                    handles.Add(addNeighbor);                                   // Make the last waithandle the add a neighbor signal

                    int index = WaitHandle.WaitAny(handles.ToArray(), config.ConnectTimeout, false);
                    if (index == results.Count)                                 // welcomeReceived was signalled
                    {
                        welcomeReceived.Reset();
                    }
                    else if (index == results.Count + 1)                        // maintainerClosed was signalled
                    {
                        maintainerClosed.Reset();
                        lock (ThisLock)
                        {
                            nodeAddresses.Clear();
                        }
                    }
                    else if (index == results.Count + 2)                        // addNeighbor was signalled
                    {
                        // We need to open a new neighbor
                        if (nodeAddresses.Count > 0)
                        {
                            if (pendingConnectedNeighbor.Count + maintainer.ConnectedNeighborCount < wantedConnectionCount)
                            {
                                PeerNodeAddress epr = null;
                                lock (ThisLock)
                                {
                                    if (nodeAddresses.Count == 0 || !maintainer.IsOpen)   // nodeAddresses or maintainer is closed got updated better cycle
                                    {
                                        addNeighbor.Reset();
                                        continue;
                                    }
                                    int index2 = random.Next() % nodeAddresses.Count;
                                    ICollection<Uri> keys = nodeAddresses.Keys;
                                    int i = 0;
                                    Uri key = null;
                                    foreach (Uri uri in keys)
                                    {
                                        if (i++ == index2)
                                        {
                                            key = uri;
                                            break;
                                        }
                                    }
                                    Fx.Assert(key != null, "key cannot be null here");
                                    epr = nodeAddresses[key];
                                    Fx.Assert(epr != null, "epr cannot be null here");
                                    nodeAddresses.Remove(key);
                                }
                                if (maintainer.FindDuplicateNeighbor(epr) == null
                                && pendingConnectedNeighbor.ContainsKey(GetEndpointUri(epr)) == false)
                                {
                                    lock (ThisLock)
                                    {
                                        pendingConnectedNeighbor.Add(GetEndpointUri(epr), epr);
                                    }

                                    // If the neighborManager is not open this call is going to throw.
                                    // It throws ObjectDisposed exception.
                                    // This check merely eliminates the perf hit, this check is not strictly necessary
                                    // but cuts down the window for the ---- that will result in a throw to a miniscule level
                                    // We ---- the throw because we are closing down
                                    try
                                    {
                                        if (maintainer.IsOpen)
                                        {
                                            if (DiagnosticUtility.ShouldTraceInformation)
                                            {
                                                PeerMaintainerTraceRecord record = new PeerMaintainerTraceRecord(SR.GetString(SR.PeerMaintainerConnect, epr, this.config.MeshId));
                                                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PeerMaintainerActivity, SR.GetString(SR.TraceCodePeerMaintainerActivity),
                                                    record, this, null);
                                            }
                                            IAsyncResult iar = maintainer.BeginOpenNeighbor(epr, timeoutHelper.RemainingTime(), null, epr);
                                            results.Add(iar);
                                        }

                                    }
                                    catch (Exception e)
                                    {
                                        if (Fx.IsFatal(e)) throw;
                                        if (DiagnosticUtility.ShouldTraceInformation)
                                        {
                                            PeerMaintainerTraceRecord record = new PeerMaintainerTraceRecord(SR.GetString(SR.PeerMaintainerConnectFailure, epr, this.config.MeshId, e.Message));
                                            TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PeerMaintainerActivity, SR.GetString(SR.TraceCodePeerMaintainerActivity),
                                                record, this, null);
                                        }

                                        // I need to remove the epr just began because the BeginOpen threw.
                                        // However Object Disposed can arise as a result of a ---- between PeerNode.Close()
                                        // and Connect trying to reconnect nodes.
                                        pendingConnectedNeighbor.Remove(GetEndpointUri(epr));
                                        if (!(e is ObjectDisposedException)) throw;

                                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                                    }
                                }
                            }
                        }

                        if (nodeAddresses.Count == 0 || pendingConnectedNeighbor.Count + maintainer.ConnectedNeighborCount == wantedConnectionCount)
                        {
                            addNeighbor.Reset();
                        }
                    }
                    else if (index != WaitHandle.WaitTimeout)
                    {
                        // We have completed this thing remove it from results
                        IAsyncResult iar = results[index];
                        results.RemoveAt(index);
                        IPeerNeighbor neighbor = null;
                        try
                        {
                            // Get opened neighbor and fire NeighborOpened notification
                            neighbor = maintainer.EndOpenNeighbor(iar);
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e)) throw;
                            pendingConnectedNeighbor.Remove(GetEndpointUri((PeerNodeAddress)iar.AsyncState));
                            throw;
                        }
                    }
                    else
                    {
                        //A timeout occured no connections progressed, try some more connections
                        //This may result in more than wantedConnectionCount connections if the timeout connections were 
                        // merely being slow
                        pendingConnectedNeighbor.Clear();
                        results.Clear();
                        addNeighbor.Set();
                    }
                }
                catch (CommunicationException e)
                {
                    // mostly likely the endpoint could not be reached, but any channel exception means we should try another node
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    addNeighbor.Set();
                }
                catch (TimeoutException e)
                {
                    if (TD.OpenTimeoutIsEnabled())
                    {
                        TD.OpenTimeout(e.Message);
                    }
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    addNeighbor.Set();
                }
            }
        }


        void IDisposable.Dispose()
        {
            if (!disposed)
            {
                lock (ThisLock)
                {
                    if (!disposed)
                    {
                        disposed = true;
                        maintainer.ReferralsAdded -= OnReferralsAdded;
                        maintainer.MaintainerClosed -= OnMaintainerClosed;
                        maintainer.NeighborClosed -= OnNeighborClosed;
                        maintainer.NeighborConnected -= OnNeighborConnected;

                        addNeighbor.Close();
                        maintainerClosed.Close();
                        welcomeReceived.Close();
                    }
                }
            }
        }

        // This method exists to minimize code churn if PeerNodeAddress is refactored later to derive from EndpointAddress
        static Uri GetEndpointUri(PeerNodeAddress address)
        {
            return address.EndpointAddress.Uri;
        }

        // Algorithm to prune connections
        // This implementation will reduce the number of connections to config.IdealNeighbors
        // by examining LinkUtility and selecting the neighbor with the lowest and then disconnecting it
        public void PruneConnections()
        {
            while (maintainer.NonClosingNeighborCount > config.IdealNeighbors && maintainer.IsOpen)
            {
                IPeerNeighbor leastUseful = maintainer.GetLeastUsefulNeighbor();
                if (leastUseful == null)
                    break;
                maintainer.CloseNeighbor(leastUseful, PeerCloseReason.NotUsefulNeighbor);
            }
        }

        // Helper method for updating the end points list
        public void UpdateEndpointsCollection(ICollection<PeerNodeAddress> src)
        {
            if (src != null)
            {
                lock (ThisLock)
                {
                    foreach (PeerNodeAddress address in src)
                    {
                        UpdateEndpointsCollection(address);
                    }
                }
            }
        }

        public void UpdateEndpointsCollection(ICollection<Referral> src)
        {
            if (src != null)
            {
                lock (ThisLock)
                {
                    foreach (Referral referral in src)
                    {
                        UpdateEndpointsCollection(referral.Address);
                    }
                }
            }
        }

        void UpdateEndpointsCollection(PeerNodeAddress address)
        {
            // Don't accept invalid addresses
            if (PeerValidateHelper.ValidNodeAddress(address))
            {
                Uri key = GetEndpointUri(address);
                if (!nodeAddresses.ContainsKey(key) && key != GetEndpointUri(maintainer.GetListenAddress()))
                {
                    nodeAddresses[key] = address;
                }
            }
        }

        // When a connection occurs remove it from the list to look at
        void OnNeighborClosed(IPeerNeighbor neighbor)
        {
            if (neighbor.ListenAddress != null)
            {
                Uri address = GetEndpointUri(neighbor.ListenAddress);

                if (!disposed)
                {
                    lock (ThisLock)
                    {
                        if (!disposed)
                        {
                            if (address != null && pendingConnectedNeighbor.ContainsKey(address))
                            {
                                pendingConnectedNeighbor.Remove(address);
                                addNeighbor.Set();
                            }
                        }
                    }
                }
            }
        }

        // When a connection occurs remove it from the list to look at
        void OnNeighborConnected(IPeerNeighbor neighbor)
        {
            Uri address = GetEndpointUri(neighbor.ListenAddress);

            if (!disposed)
            {
                lock (ThisLock)
                {
                    if (!disposed)
                    {
                        if (address != null && pendingConnectedNeighbor.ContainsKey(address))
                        {
                            pendingConnectedNeighbor.Remove(address);
                        }
                        welcomeReceived.Set();
                    }
                }
            }
        }

        void OnMaintainerClosed()
        {
            if (!disposed)
            {
                lock (ThisLock)
                {
                    if (!disposed)
                    {
                        maintainerClosed.Set();
                    }
                }
            }
        }

        // When a connection occurs add those to the group I look at
        void OnReferralsAdded(IList<Referral> referrals, IPeerNeighbor neighbor)
        {
            bool added = false;

            // Do some stuff here
            foreach (Referral referral in referrals)
            {
                if (!disposed)
                {
                    lock (ThisLock)
                    {
                        if (!disposed)
                        {
                            if (!maintainer.IsOpen)
                                return;

                            Uri key = GetEndpointUri(referral.Address);
                            if (key != GetEndpointUri(maintainer.GetListenAddress()))   // make sure the referral is not mine
                            {
                                if (!nodeAddresses.ContainsKey(key)
                                && !pendingConnectedNeighbor.ContainsKey(key)
                                && maintainer.FindDuplicateNeighbor(referral.Address) == null)
                                {
                                    nodeAddresses[key] = referral.Address;
                                    added = true;
                                }
                            }
                        }
                    }
                }
            }

            if (added)
            {
                if (maintainer.ConnectedNeighborCount < wantedConnectionCount)
                {
                    addNeighbor.Set();
                }
            }
        }
    }
}
