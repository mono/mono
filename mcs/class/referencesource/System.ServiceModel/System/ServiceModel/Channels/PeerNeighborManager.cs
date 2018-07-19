//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Net;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Threading;

    // Neighbor Manager is responsible for managing a set of neighbors for a node.
    class PeerNeighborManager
    {
        public event EventHandler<PeerNeighborCloseEventArgs> NeighborClosed;
        public event EventHandler<PeerNeighborCloseEventArgs> NeighborClosing;
        public event EventHandler NeighborConnected;
        public event EventHandler NeighborOpened;
        public event EventHandler Offline;
        public event EventHandler Online;

        // Delegate to determine if neighbor manager is closing or closed
        delegate bool ClosedCallback();

        enum State
        {
            Created,
            Opened,
            ShuttingDown,
            Shutdown,
            Closed,
        }

        PeerNodeConfig config;

        //
        // Contains the neighbors in connected state
        // We maintain a connectedNeighborList in addition to neighborList for two reasons:
        // (a) Several operations are on connected neighbors 
        // (b) To correctly handle online/offline conditions
        //
        List<IPeerNeighbor> connectedNeighborList;

        bool isOnline;
        PeerIPHelper ipHelper;
        List<PeerNeighbor> neighborList;    // contains all the neighbors known to neighbor manager
        ManualResetEvent shutdownEvent;
        State state;
        object thisLock;
        PeerNodeTraceRecord traceRecord;
        PeerService service;
        Binding serviceBinding;
        IPeerNodeMessageHandling messageHandler;

        public PeerNeighborManager(PeerIPHelper ipHelper, PeerNodeConfig config)
            :
            this(ipHelper, config, null) { }
        public PeerNeighborManager(PeerIPHelper ipHelper, PeerNodeConfig config, IPeerNodeMessageHandling messageHandler)
        {
            Fx.Assert(ipHelper != null, "Non-null ipHelper is expected");
            Fx.Assert(config != null, "Non-null Config is expected");

            this.neighborList = new List<PeerNeighbor>();
            this.connectedNeighborList = new List<IPeerNeighbor>();
            this.ipHelper = ipHelper;
            this.messageHandler = messageHandler;
            this.config = config;
            this.thisLock = new object();
            this.traceRecord = new PeerNodeTraceRecord(config.NodeId);
            this.state = State.Created;
        }

        // Returns the count of connected neighbors
        public int ConnectedNeighborCount
        {
            get
            {
                return this.connectedNeighborList.Count;
            }
        }

        public int NonClosingNeighborCount
        {
            get
            {
                int count = 0;
                foreach (PeerNeighbor neighbor in this.connectedNeighborList)
                {
                    if (!neighbor.IsClosing) count++;
                }
                return count;
            }
        }

        // Returns true if Neighbor Manager is online 
        // (i.e., has one or more connected neighbors)
        public bool IsOnline
        {
            get
            {
                return this.isOnline;
            }
        }

        // Returns the count of connected neighbors
        public int NeighborCount
        {
            get
            {
                return this.neighborList.Count;
            }
        }

        object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        // Ungracefully shutdown the neighbor manager
        void Abort(PeerNeighbor[] neighbors)
        {
            foreach (PeerNeighbor neighbor in neighbors)
                neighbor.Abort(PeerCloseReason.LeavingMesh, PeerCloseInitiator.LocalNode);
        }

        public IAsyncResult BeginOpenNeighbor(PeerNodeAddress remoteAddress, TimeSpan timeout, AsyncCallback callback, object asyncState)
        {
            ThrowIfNotOpen();

            // It's okay if neighbor manager is shutdown and closed after the above check 
            // because the new neighbor is only added to neighborList in NeighborOpened 
            // handler if the neighbor manager is still open.

            // Sort the IP addresses
            ReadOnlyCollection<IPAddress> sortedAddresses = this.ipHelper.SortAddresses(remoteAddress.IPAddresses);
            PeerNodeAddress address = new PeerNodeAddress(remoteAddress.EndpointAddress, sortedAddresses);
            return BeginOpenNeighborInternal(address, timeout, callback, asyncState);
        }

        internal IAsyncResult BeginOpenNeighborInternal(PeerNodeAddress remoteAddress, TimeSpan timeout, AsyncCallback callback, object asyncState)
        {
            PeerNeighbor neighbor = new PeerNeighbor(this.config, this.messageHandler);
            RegisterForNeighborEvents(neighbor);

            return new NeighborOpenAsyncResult(neighbor, remoteAddress, this.serviceBinding, this.service,
                new ClosedCallback(Closed), timeout, callback, asyncState);
        }

        // Cleanup after shutdown
        void Cleanup(bool graceful)
        {
            lock (ThisLock)
            {
                // In case of g----ful shutdown, we wait for neighbor list to become empty. connectedNeighborList should become
                // empty as well.
                if (graceful)
                {
                    Fx.Assert(this.neighborList.Count == 0, "neighbor count should be 0");
                    Fx.Assert(this.connectedNeighborList.Count == 0, "Connected neighbor count should be 0");

                    // shutdownEvent is only relevant for a g----ful close. And should be closed by the thread
                    // performing g----ful close
                    if (this.shutdownEvent != null)
                        this.shutdownEvent.Close();
                }
                this.state = State.Shutdown;
            }
        }

        // To clear the neighbor lists in case of unexpected exceptions during shutdown
        void ClearNeighborList()
        {
            lock (ThisLock)
            {
                this.neighborList.Clear();
                this.connectedNeighborList.Clear();
            }
        }

        // Close the neighbor manager. It should be called after Shutdown(). 
        // Can also be called before Open.
        public void Close()
        {
            lock (ThisLock)
            {
                this.state = State.Closed;
            }
        }

        // Returns true if neighbor manager is closing or closed
        bool Closed()
        {
            return this.state != State.Opened;
        }

        //
        // Close the specified neighbor. Ok to call multiple times, but NeighborClosing 
        // and NeighborClosed events are fired just once.
        // If the closeReason specified is InvalidNeighbor, it will be closed ungracefully
        //
        public void CloseNeighbor(IPeerNeighbor neighbor, PeerCloseReason closeReason,
            PeerCloseInitiator closeInitiator)
        {
            CloseNeighbor(neighbor, closeReason, closeInitiator, null);
        }

        public void CloseNeighbor(IPeerNeighbor neighbor, PeerCloseReason closeReason,
            PeerCloseInitiator closeInitiator, Exception closeException)
        {
            PeerNeighbor nbr = (PeerNeighbor)neighbor;

            lock (ThisLock)
            {
                if (!(this.state != State.Created))
                {
                    throw Fx.AssertAndThrow("Neighbor Manager is not expected to be in Created state");
                }

                // Check that the neighbor is known to neighbor manager
                if (!this.neighborList.Contains(nbr))
                    return;
            }

            // initiate closing of the neighbor
            if (closeReason != PeerCloseReason.InvalidNeighbor)
            {
                if (!nbr.IsClosing)
                    InvokeAsyncNeighborClose(nbr, closeReason, closeInitiator, closeException, null);
            }
            else    // Call abort even if neighbor is already closing
            {
                nbr.Abort(closeReason, closeInitiator);
            }
        }

        public IPeerNeighbor EndOpenNeighbor(IAsyncResult result)
        {
            return NeighborOpenAsyncResult.End(result);
        }

        static void FireEvent(EventHandler handler, PeerNeighborManager manager)
        {
            if (handler != null)
                handler(manager, EventArgs.Empty);
        }

        static void FireEvent(EventHandler handler, PeerNeighbor neighbor)
        {
            if (handler != null)
                handler(neighbor, EventArgs.Empty);
        }

        static void FireEvent(EventHandler<PeerNeighborCloseEventArgs> handler,
            PeerNeighbor neighbor, PeerCloseReason closeReason,
            PeerCloseInitiator closeInitiator, Exception closeException)
        {
            if (handler != null)
            {
                PeerNeighborCloseEventArgs args = new PeerNeighborCloseEventArgs(
                    closeReason, closeInitiator, closeException);
                handler(neighbor, args);
            }
        }

        // Find a duplicate neighbor matching the nodeId
        public IPeerNeighbor FindDuplicateNeighbor(ulong nodeId)
        {
            return FindDuplicateNeighbor(nodeId, null);
        }

        // Find a duplicate neighbor (excluding skipNeighbor) matching the nodeID.
        public IPeerNeighbor FindDuplicateNeighbor(ulong nodeId, IPeerNeighbor skipNeighbor)
        {
            PeerNeighbor duplicateNeighbor = null;

            if (nodeId != PeerTransportConstants.InvalidNodeId)
            {
                lock (ThisLock)
                {
                    foreach (PeerNeighbor neighbor in this.neighborList)
                    {
                        // We restrict search to neighbors that are not yet closing.
                        if (neighbor != (PeerNeighbor)skipNeighbor && neighbor.NodeId == nodeId &&
                            !neighbor.IsClosing &&
                            neighbor.State < PeerNeighborState.Disconnecting)
                        {
                            duplicateNeighbor = neighbor;
                            break;
                        }
                    }
                }
            }
            return duplicateNeighbor;
        }

        public bool PingNeighbor(IPeerNeighbor peer)
        {
            bool result = true;
            Message message = Message.CreateMessage(MessageVersion.Soap12WSAddressing10, PeerStrings.PingAction);
            try
            {
                peer.Ping(message);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                peer.Abort(PeerCloseReason.InternalFailure, PeerCloseInitiator.LocalNode);
                result = false;
            }
            return result;

        }

        public void PingNeighbors()
        {
            List<IPeerNeighbor> neighbors = GetConnectedNeighbors();
            foreach (IPeerNeighbor neighbor in neighbors)
            {
                PingNeighbor(neighbor);
            }
        }

        // Find a duplicate neighbor matching the address
        public IPeerNeighbor FindDuplicateNeighbor(PeerNodeAddress address)
        {
            return FindDuplicateNeighbor(address, null);
        }

        // Find a duplicate neighbor (excluding skipNeighbor) matching the address.
        public IPeerNeighbor FindDuplicateNeighbor(PeerNodeAddress address, IPeerNeighbor skipNeighbor)
        {
            PeerNeighbor duplicateNeighbor = null;

            lock (ThisLock)
            {
                foreach (PeerNeighbor neighbor in this.neighborList)
                {
                    // We restrict search to neighbors that are not yet closing.
                    if (neighbor != (PeerNeighbor)skipNeighbor &&
                        neighbor.ListenAddress != null &&
                        neighbor.ListenAddress.ServicePath == address.ServicePath &&
                        !neighbor.IsClosing &&
                        neighbor.State < PeerNeighborState.Disconnecting)
                    {
                        duplicateNeighbor = neighbor;
                        break;
                    }
                }
            }
            return duplicateNeighbor;
        }

        // Returns a copy of the list of connected neighbors.
        public List<IPeerNeighbor> GetConnectedNeighbors()
        {
            lock (ThisLock)
            {
                return new List<IPeerNeighbor>(this.connectedNeighborList);
            }
        }

        // Used to retrieve a neighbor given the proxy.
        // Maps the proxy from the incoming message's service context to a neighbor instance.
        public IPeerNeighbor GetNeighborFromProxy(IPeerProxy proxy)
        {
            PeerNeighbor neighbor = null;

            lock (ThisLock)
            {
                if (state == State.Opened)
                {
                    // Find the neighbor containing the specified proxy.
                    foreach (PeerNeighbor nbr in this.neighborList)
                    {
                        if (nbr.Proxy == proxy)
                        {
                            neighbor = nbr;
                            break;
                        }
                    }
                }
            }

            return neighbor;
        }

        // Calls neighbor.BeginClose or EndClose and catches appropriate exceptions for any cleanup.
        // We use a single method for both BeginClose and EndClose processing since exception handling
        // is very similar in both cases.
        void InvokeAsyncNeighborClose(PeerNeighbor neighbor, PeerCloseReason closeReason,
            PeerCloseInitiator closeInitiator, Exception closeException, IAsyncResult endResult)
        {
            // initiate invoking BeginClose or EndClose
            try
            {
                if (endResult == null)
                {
                    IAsyncResult beginResult = neighbor.BeginClose(closeReason, closeInitiator,
                                        closeException, Fx.ThunkCallback(new AsyncCallback(OnNeighborClosedCallback)), neighbor);
                    if (beginResult.CompletedSynchronously)
                        neighbor.EndClose(beginResult);
                }
                else
                {
                    neighbor.EndClose(endResult);
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e)) throw;
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);

                neighbor.TraceEventHelper(TraceEventType.Warning, TraceCode.PeerNeighborCloseFailed, SR.GetString(SR.TraceCodePeerNeighborCloseFailed), e);
                // May get InvalidOperationException or ObjectDisposedException due to simultaneous close from both sides (and autoclose is enabled)
                if (e is InvalidOperationException || e is CommunicationException || e is TimeoutException)
                {
                    neighbor.Abort();
                }
                else
                {
                    throw;
                }
            }
        }

        //
        // Handler for processing neighbor closed event.
        //
        // We should allow this event to be processed even if the neighbor manager is shutting
        // down because neighbor manager will be waiting on the shutdown event which is set in 
        // this handler once the last neighbor's close event is processed.
        //
        void OnNeighborClosed(object source, EventArgs args)
        {
            RemoveNeighbor((PeerNeighbor)source);
        }

        // Callback that is invoked when BeginClose completes.
        void OnNeighborClosedCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                // Call neighbor.EndClose -- PeerCloseReason and PeerCloseInitiator are dummy values
                InvokeAsyncNeighborClose((PeerNeighbor)result.AsyncState, PeerCloseReason.None,
                    PeerCloseInitiator.LocalNode, null, result);
            }
        }

        // Handles neighbor disconnecting or disconnected events
        void OnNeighborClosing(object source, EventArgs args)
        {
            //
            // Remove the neighbor from connected list. But closed and offline events are
            // fired upon processing closed event. If, due to thread scheduling issues,
            // closed handler executes before this handler, it will have already done the
            // work and Remove() below is a NOP.
            //
            lock (ThisLock)
            {
                this.connectedNeighborList.Remove((IPeerNeighbor)source);
            }
        }

        // handler to process neighbor connected event
        void OnNeighborConnected(object source, EventArgs args)
        {
            PeerNeighbor neighbor = (PeerNeighbor)source;
            bool fireConnected = false;
            bool fireOnline = false;

            // we may get this event after the neighbor has been closed. So, we check to see if
            // the neighbor exists in the neighbor list before processing the event.
            lock (ThisLock)
            {
                if (this.neighborList.Contains(neighbor))
                {
                    fireConnected = true;

                    // Add the neighbor to connected list and determine if online should be fired
                    this.connectedNeighborList.Add(neighbor);
                    if (!this.isOnline)
                    {
                        // Fire online event
                        this.isOnline = true;
                        fireOnline = true;
                    }
                }
            }

            if (fireConnected)
            {
                FireEvent(NeighborConnected, neighbor);
            }
            else
                neighbor.TraceEventHelper(TraceEventType.Warning, TraceCode.PeerNeighborNotFound, SR.GetString(SR.TraceCodePeerNeighborNotFound));

            if (fireOnline)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PeerNeighborManagerOnline,
                        SR.GetString(SR.TraceCodePeerNeighborManagerOnline), this.traceRecord, this, null);
                }
                FireEvent(Online, this);
            }
        }

        // handler to process neighbor opened event
        void OnNeighborOpened(object source, EventArgs args)
        {
            PeerNeighbor neighbor = (PeerNeighbor)source;
            bool accept = false;

            lock (ThisLock)
            {
                // Add the neighbor to neighborList if neighbor manager is still open
                if (this.state == State.Opened)
                {
                    // StateManager assures that neighbor Opened and Closed events are 
                    // serialized. So, we should never have to process a closed event handler 
                    // before opened is complete.
                    if (!(neighbor.State == PeerNeighborState.Opened))
                    {
                        throw Fx.AssertAndThrow("Neighbor expected to be in Opened state");
                    }
                    this.neighborList.Add(neighbor);
                    accept = true;
                }
            }
            if (accept)
            {
                FireEvent(NeighborOpened, neighbor);
            }
            else                // close the neighbor ungracefully
            {
                neighbor.Abort();
                neighbor.TraceEventHelper(TraceEventType.Warning, TraceCode.PeerNeighborNotAccepted, SR.GetString(SR.TraceCodePeerNeighborNotAccepted));
            }
        }

        // Opens the neighbor manager. When this method returns the neighbor manager is ready 
        // to accept incoming neighbor requests and to establish outgoing neighbors.
        public void Open(Binding serviceBinding, PeerService service)
        {
            Fx.Assert(serviceBinding != null, "serviceBinding must not be null");
            Fx.Assert(service != null, "service must not be null");

            lock (ThisLock)
            {
                this.service = service;
                this.serviceBinding = serviceBinding;
                if (!(this.state == State.Created))
                {
                    throw Fx.AssertAndThrow("Neighbor Manager is expected to be in Created state");
                }
                this.state = State.Opened;
            }
        }

        // Process an inbound channel 
        public bool ProcessIncomingChannel(IClientChannel channel)
        {
            bool accepted = false;
            IPeerProxy proxy = (IPeerProxy)channel;

            Fx.Assert(GetNeighborFromProxy(proxy) == null, "Channel should not map to an existing neighbor");
            if (this.state == State.Opened)
            {
                // It is okay if neighbor manager is closed after the above check because the 
                // new neighbor is only added to neighborList in neighbor Opened handler if the 
                // neighbor manager is still open.
                PeerNeighbor neighbor = new PeerNeighbor(this.config, this.messageHandler);
                RegisterForNeighborEvents(neighbor);
                neighbor.Open(proxy);
                accepted = true;
            }
            else
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.PeerNeighborNotAccepted,
                        SR.GetString(SR.TraceCodePeerNeighborNotAccepted), this.traceRecord, this, null);
                }
            }

            return accepted;
        }

        void RegisterForNeighborEvents(PeerNeighbor neighbor)
        {
            neighbor.Opened += OnNeighborOpened;
            neighbor.Connected += OnNeighborConnected;
            neighbor.Closed += OnNeighborClosed;

            // We want the neighbor to call Closing handlers directly, so we delegate
            neighbor.Closing += this.NeighborClosing;

            // Disconnecting and Disconnected are treated the same way
            neighbor.Disconnecting += OnNeighborClosing;
            neighbor.Disconnected += OnNeighborClosing;
        }

        // Remove neighbor from the list and fire relevant events
        void RemoveNeighbor(PeerNeighbor neighbor)
        {
            bool fireClosed = false;
            bool fireOffline = false;

            lock (ThisLock)
            {
                if (this.neighborList.Contains(neighbor))
                {
                    fireClosed = true;

                    // Remove neighbor from our lists and determine if offline should be fired.
                    this.neighborList.Remove(neighbor);
                    this.connectedNeighborList.Remove(neighbor);
                    if (this.isOnline && this.connectedNeighborList.Count == 0)
                    {
                        this.isOnline = false;
                        fireOffline = true;
                    }
                    // If in the process of shutting down neighbor manager, signal completion 
                    // upon closing of the last remaining neighbor
                    if (this.neighborList.Count == 0 && this.shutdownEvent != null)
                    {
                        this.shutdownEvent.Set();
                    }
                }
            }

            // Fire events
            if (fireClosed)
            {
                FireEvent(NeighborClosed, neighbor, neighbor.CloseReason,
                        neighbor.CloseInitiator, neighbor.CloseException);
            }
            else
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    neighbor.TraceEventHelper(TraceEventType.Warning, TraceCode.PeerNeighborNotFound, SR.GetString(SR.TraceCodePeerNeighborNotFound));
                }
            }
            if (fireOffline)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.PeerNeighborManagerOffline,
                        SR.GetString(SR.TraceCodePeerNeighborManagerOffline), this.traceRecord, this, null);
                }
                FireEvent(Offline, this);
            }
        }

        //
        // Shutdown the neighbor manager. Shutdown should be called prior to Close(). It stops
        // processing inbound neighbor sessions and closes all the neighbors. Outbound neighbor
        // sessions are also disabled as a result of setting the state to ShuttingDown
        // (and then Shutdown).
        //
        public void Shutdown(bool graceful, TimeSpan timeout)
        {
            PeerNeighbor[] neighbors = null;
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

            try
            {
                lock (ThisLock)
                {
                    if (this.state == State.Shutdown || this.state == State.Closed)
                        return;
                    this.state = State.ShuttingDown;

                    // Create a copy of neighbor list in order to close neighbors outside lock.
                    neighbors = this.neighborList.ToArray();

                    // In case of g----ful shutdown, if there are any neighbors to close, create an event 
                    // to wait until they are closed
                    if (graceful && neighbors.Length > 0)
                        this.shutdownEvent = new ManualResetEvent(false);
                }

                // Close each neighbor. Do this outside the lock due to Closing and Closed event handlers being invoked
                if (graceful)
                    Shutdown(neighbors, timeoutHelper.RemainingTime());
                else
                    Abort(neighbors);
            }
            catch (Exception e)
            {
                // Purge neighbor list in case of unexpected exceptions
                if (Fx.IsFatal(e)) throw;
                try
                {
                    ClearNeighborList();
                }
                catch (Exception ee)
                {
                    if (Fx.IsFatal(ee)) throw;
                    DiagnosticUtility.TraceHandledException(ee, TraceEventType.Information);
                }
                throw;
            }
            finally
            {
                Cleanup(graceful);
            }
        }

        // G----ful shutdown
        void Shutdown(PeerNeighbor[] neighbors, TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            foreach (PeerNeighbor neighbor in neighbors)
                CloseNeighbor(neighbor, PeerCloseReason.LeavingMesh, PeerCloseInitiator.LocalNode, null);

            // Wait for all the neighbors to close (the event object is set when the last 
            // neighbor is closed). Specify a timeout for wait event handle in case event.Set
            // fails for some reason (it doesn't throw exception). Bail out of the loop when 
            // the neighbor count reaches 0. This ensures that Shutdown() doesn't hang.
            if (neighbors.Length > 0)
            {
                if (!TimeoutHelper.WaitOne(this.shutdownEvent, timeoutHelper.RemainingTime()))
                {
                    Abort(neighbors);   // abort neighbors that haven't been closed yet
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException());
                }
            }
        }

        void ThrowIfNotOpen()
        {
            if (!(this.state != State.Created))
            {
                throw Fx.AssertAndThrow("Neighbor manager not expected to be in Created state");
            }
            if (Closed())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.ToString()));
            }
        }

        class PeerNeighbor : IPeerNeighbor, IInputSessionShutdown
        {
            public event EventHandler Closed;
            public event EventHandler<PeerNeighborCloseEventArgs> Closing;
            public event EventHandler Connected;
            public event EventHandler Disconnected;
            public event EventHandler Disconnecting;
            public event EventHandler Opened;

            ChannelFactory<IPeerProxy> channelFactory;

            // Used after closing the neighbor to find details of the close reason and who 
            // initiated closing.
            Exception closeException;
            PeerCloseInitiator closeInitiator;
            PeerCloseReason closeReason;

            PeerNodeConfig config;
            IPAddress connectIPAddress;         // relevant for initiator neighbor. Indicates the IP address used for connection
            ExtensionCollection<IPeerNeighbor> extensions;
            bool initiator;
            bool isClosing;                     // If true, the neighbor is being closed or already closed
            PeerNodeAddress listenAddress;      // The address that the remote endpoint is listening on
            ulong nodeId;                       // The nodeID of the remote endpoint
            IPeerProxy proxy;                   // Proxy channel to talk to the remote endpoint
            IClientChannel proxyChannel;        // To access inner Channel from proxy w/o casting
            PeerNeighborState state;            // Current state of the neighbor
            object thisLock = new object();
            IPeerNodeMessageHandling messageHandler;
            UtilityExtension utility;

            // Dictates if attempt to set neighbor state should throw exception on failure.
            enum SetStateBehavior
            {
                ThrowException,
                TrySet,
            }

            public PeerNeighbor(PeerNodeConfig config,
                                IPeerNodeMessageHandling messageHandler)
            {
                this.closeReason = PeerCloseReason.None;
                this.closeInitiator = PeerCloseInitiator.LocalNode;
                this.config = config;
                this.state = PeerNeighborState.Created;
                this.extensions = new ExtensionCollection<IPeerNeighbor>(this, thisLock);
                this.messageHandler = messageHandler;
            }

            public IPAddress ConnectIPAddress
            {
                get
                {
                    return this.connectIPAddress;
                }
                set
                {
                    this.connectIPAddress = value;
                }
            }

            // To retrieve reason for closing the neighbor
            public PeerCloseReason CloseReason
            {
                get
                {
                    return this.closeReason;
                }
            }

            // Indicates if close was initiated by local or remote node
            public PeerCloseInitiator CloseInitiator
            {
                get
                {
                    return this.closeInitiator;
                }
            }

            // If an exception during processing caused the neighbor to be closed
            public Exception CloseException
            {
                get
                {
                    return this.closeException;
                }
            }

            public IExtensionCollection<IPeerNeighbor> Extensions
            {
                get
                {
                    return extensions;
                }
            }

            // Returns true if the neighbor is currently closing or already closed
            public bool IsClosing
            {
                get
                {
                    return isClosing;
                }
            }

            // Returns true if neighbor is in connected, synchronizing, or synchronized states
            public bool IsConnected
            {
                get
                {
                    return PeerNeighborStateHelper.IsConnected(this.state);
                }
            }

            // NOTE: If the property is accessed before the neighbor transitions to connected 
            // state, the returned listen address may be null for the accepting neighbor.
            public PeerNodeAddress ListenAddress
            {
                get
                {
                    // Return a copy since the scope ID is settable
                    PeerNodeAddress address = this.listenAddress;
                    if (address != null)
                        return new PeerNodeAddress(address.EndpointAddress, PeerIPHelper.CloneAddresses(address.IPAddresses, true));
                    else
                        return address;
                }
                set
                {

                    lock (ThisLock)
                    {
                        if (!(!this.initiator))
                        {
                            throw Fx.AssertAndThrow("Cannot be set for initiator neighbors");
                        }
                        ThrowIfClosed();

                        if (value != null)
                        {
                            this.listenAddress = value;
                        }
                    }
                }
            }

            // Returns true if the neighbor is an initiator
            public bool IsInitiator
            {
                get
                {
                    return this.initiator;
                }
            }

            // Returns the node ID of the neighbor. If this property is accessed before the 
            // neighbor transitions to connected state, the returned node ID may be 0.
            public ulong NodeId
            {
                get
                {
                    return this.nodeId;
                }

                set
                {
                    lock (ThisLock)
                    {
                        ThrowIfClosed();
                        this.nodeId = value;
                    }
                }
            }

            // Returns the proxy for the neighbor (i.e., the channel that SFx maintains to the
            // remote node associated with this neighbor instance).
            public IPeerProxy Proxy
            {
                get
                {
                    return this.proxy;
                }
                set
                {
                    this.proxy = value;
                    this.proxyChannel = (IClientChannel)this.proxy;
                    RegisterForChannelEvents();
                }
            }

            // The only states that are settable are connecting, connected, synchronizing, 
            // synchronized, disconnecting, and disconnected.
            public PeerNeighborState State
            {
                get
                {
                    return this.state;
                }

                set
                {
                    if (!(PeerNeighborStateHelper.IsSettable(value)))
                    {
                        throw Fx.AssertAndThrow("A valid settable state is expected");
                    }
                    SetState(value, SetStateBehavior.ThrowException);
                }
            }

            object ThisLock
            {
                get
                {
                    return this.thisLock;
                }
            }

            // NOTE: Closing handlers not invoked when a neighbor is aborted; but Closed handlers are.
            public void Abort(PeerCloseReason reason, PeerCloseInitiator closeInit)
            {
                lock (ThisLock)
                {
                    // Set close reason etc. if they are not already set.
                    if (!this.isClosing)
                    {
                        this.isClosing = true;
                        this.closeReason = reason;
                        this.closeInitiator = closeInit;
                    }
                }
                Abort();
            }

            public void Abort()
            {
                if (this.channelFactory != null)
                    this.channelFactory.Abort();
                else
                    this.proxyChannel.Abort();
            }

            // Close a neighbor gracefully
            public IAsyncResult BeginClose(PeerCloseReason reason,
                PeerCloseInitiator closeInit, Exception exception,
                AsyncCallback callback, object asyncState)
            {
                bool callClosing = false;

                lock (ThisLock)
                {
                    // Set close reason etc. if they are not already set.
                    if (!this.isClosing)
                    {
                        callClosing = true;
                        this.isClosing = true;
                        this.closeReason = reason;
                        this.closeInitiator = closeInit;
                        this.closeException = exception;
                    }
                }

                // Initiate close, if another thread has not already done so....
                // NOTE: NeighborClosing handlers should not throw any catchable exceptions.
                if (callClosing)
                {
                    EventHandler<PeerNeighborCloseEventArgs> handler = this.Closing;
                    if (handler != null)
                    {
                        try
                        {
                            PeerNeighborCloseEventArgs args = new PeerNeighborCloseEventArgs(
                                reason, closeInitiator, exception);
                            handler(this, args);
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e)) throw;
                            Abort();
                            throw;
                        }
                    }
                }

                if (this.channelFactory != null)
                    return this.channelFactory.BeginClose(callback, asyncState);
                else
                    return this.proxyChannel.BeginClose(callback, asyncState);
            }

            // Begin opening of a neighbor channel to 'to'. instanceContext is where the remote 
            // endpoint should send messages to (it will be a reference to PeerNeighborManager).
            public IAsyncResult BeginOpen(PeerNodeAddress remoteAddress, Binding binding,
                PeerService service, ClosedCallback closedCallback, TimeSpan timeout,
                AsyncCallback callback, object asyncState)
            {
                this.initiator = true;
                this.listenAddress = remoteAddress;
                OpenAsyncResult result = new OpenAsyncResult(this, remoteAddress, binding, service,
                    closedCallback, timeout, callback, state);
                return result;
            }

            // Called by OpenAsyncResult
            public IAsyncResult BeginOpenProxy(EndpointAddress remoteAddress, Binding binding,
                InstanceContext instanceContext, TimeSpan timeout, AsyncCallback callback, object state)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                if (this.channelFactory != null)
                    Abort();    // to close previously created factory, if any

                EndpointAddressBuilder meshEprBuilder = new EndpointAddressBuilder(remoteAddress);
                meshEprBuilder.Uri = config.GetMeshUri();
                this.channelFactory = new DuplexChannelFactory<IPeerProxy>(instanceContext, binding, meshEprBuilder.ToEndpointAddress());
                this.channelFactory.Endpoint.Behaviors.Add(new ClientViaBehavior(remoteAddress.Uri));
                this.channelFactory.Endpoint.Behaviors.Add(new PeerNeighborBehavior(this));
                this.channelFactory.Endpoint.Contract.Behaviors.Add(new PeerOperationSelectorBehavior(this.messageHandler));
                this.config.SecurityManager.ApplyClientSecurity(channelFactory);
                this.channelFactory.Open(timeoutHelper.RemainingTime());
                this.Proxy = this.channelFactory.CreateChannel();

                IAsyncResult result = this.proxyChannel.BeginOpen(timeoutHelper.RemainingTime(), callback, state);
                if (result.CompletedSynchronously)
                    this.proxyChannel.EndOpen(result);

                return result;
            }

            public IAsyncResult BeginSend(Message message,
                AsyncCallback callback, object asyncState)
            {
                return this.proxy.BeginSend(message, callback, asyncState);
            }

            public IAsyncResult BeginSend(Message message,
                TimeSpan timeout, AsyncCallback callback, object asyncState)
            {
                return this.proxy.BeginSend(message, timeout, callback, asyncState);
            }

            public void Send(Message message)
            {
                this.proxy.Send(message);
            }

            // Called to Abort channelFactory in case BeginOpenProxy or EndOpenProxy throw
            public void CleanupProxy()
            {
                this.channelFactory.Abort();
            }

            public void EndClose(IAsyncResult result)
            {
                if (this.channelFactory != null)
                    this.channelFactory.EndClose(result);
                else
                    this.proxyChannel.EndClose(result);
            }

            public void EndOpen(IAsyncResult result)
            {
                OpenAsyncResult.End(result);
            }

            // Called by OpenAsyncResult
            public void EndOpenProxy(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                    this.proxyChannel.EndOpen(result);
            }

            public void EndSend(IAsyncResult result)
            {
                this.proxy.EndSend(result);
            }


            public Message RequestSecurityToken(Message request)
            {
                return this.proxy.ProcessRequestSecurityToken(request);
            }

            public void Ping(Message request)
            {
                this.proxy.Ping(request);
            }

            // Service channel closed event handler.
            void OnChannelClosed(object source, EventArgs args)
            {
                if (this.state < PeerNeighborState.Closed)
                    OnChannelClosedOrFaulted(PeerCloseReason.Closed);

                // If the other side closed the channel, abort the factory (if one exists)
                if (this.closeInitiator != PeerCloseInitiator.LocalNode && this.channelFactory != null)
                    this.channelFactory.Abort();
            }

            // Does heavy-lifting of processing closed/faulted events
            void OnChannelClosedOrFaulted(PeerCloseReason reason)
            {
                PeerNeighborState oldState;

                lock (ThisLock)
                {
                    // We don't call SetState here because it should not be called inside lock,
                    // and to avoid race conditions, we need to set the state before the lock 
                    // can be released.
                    oldState = this.state;
                    this.state = PeerNeighborState.Closed;

                    // Set close reason etc. if they are not already set (as a result of local 
                    // node initiating Close)
                    if (!this.isClosing)
                    {
                        this.isClosing = true;
                        this.closeReason = reason;
                        this.closeInitiator = PeerCloseInitiator.RemoteNode;
                    }
                    TraceClosedEvent(oldState);
                }

                // Update traces and counters and notify interested parties
                OnStateChanged(PeerNeighborState.Closed);
            }

            // Service channel faulted event handler.
            void OnChannelFaulted(object source, EventArgs args)
            {
                try
                {
                    OnChannelClosedOrFaulted(PeerCloseReason.Faulted);
                }
                finally
                {
                    Abort();
                }
            }

            // Service channel opened event handler.
            void OnChannelOpened(object source, EventArgs args)
            {
                // TrySetState is not used because it asserts for a settable state
                // and is meant for use by upper layers. Only PeerNeighbor can set
                // the state to Opened. So, it calls SetState directly.
                SetState(PeerNeighborState.Opened, SetStateBehavior.TrySet);
            }

            //
            // Invokes the appropriate state changed event handler.
            // WARNING: This method should not be called within lock.
            //
            void OnStateChanged(PeerNeighborState newState)
            {
                EventHandler handler = null;
                switch (newState)
                {
                    case PeerNeighborState.Opened:
                        handler = this.Opened;
                        break;
                    case PeerNeighborState.Closed:
                        handler = this.Closed;
                        break;
                    case PeerNeighborState.Connected:
                        handler = this.Connected;
                        break;
                    case PeerNeighborState.Disconnecting:
                        handler = this.Disconnecting;
                        break;
                    case PeerNeighborState.Disconnected:
                        handler = this.Disconnected;
                        break;
                }
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }

            // Open an accepting (incoming) neighbor. callbackInstance is where msgs meant for
            // remote endpoint should be sent.
            public void Open(IPeerProxy callbackInstance)
            {
                this.initiator = false;
                this.Proxy = callbackInstance;
            }

            // Register for channel events
            void RegisterForChannelEvents()
            {
                this.state = PeerNeighborState.Created;     // reset state if the previous proxy failed
                this.proxyChannel.Opened += OnChannelOpened;
                this.proxyChannel.Closed += OnChannelClosed;
                this.proxyChannel.Faulted += OnChannelFaulted;
            }

            // WARNING: This method should not be called within the lock -- it may invoke state 
            // changed event handlers
            bool SetState(PeerNeighborState newState, SetStateBehavior behavior)
            {
                bool stateChanged = false;
                PeerNeighborState oldState;

                // Attempt to set the state
                lock (ThisLock)
                {
                    oldState = this.State;
                    if (behavior == SetStateBehavior.ThrowException)
                        ThrowIfInvalidState(newState);
                    if (newState > this.state)
                    {
                        this.state = newState;
                        stateChanged = true;
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            TraceEventHelper(TraceEventType.Information, TraceCode.PeerNeighborStateChanged, SR.GetString(SR.TraceCodePeerNeighborStateChanged), null, null, newState, oldState);
                        }
                    }
                    else
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            TraceEventHelper(TraceEventType.Information, TraceCode.PeerNeighborStateChangeFailed, SR.GetString(SR.TraceCodePeerNeighborStateChangeFailed), null, null, oldState, newState);
                        }
                    }
                }

                if (stateChanged)
                {
                    // Pass state change notification on to interested subscribers.
                    OnStateChanged(newState);
                }

                return stateChanged;
            }

            // Attempts to set to specified state.
            // Returns true if succeed and false otherwise.
            public bool TrySetState(PeerNeighborState newState)
            {
                if (!(PeerNeighborStateHelper.IsSettable(newState)))
                {
                    throw Fx.AssertAndThrow("A valid settable state is expected");
                }
                return SetState(newState, SetStateBehavior.TrySet);
            }

            public void ThrowIfClosed()
            {
                if (this.state == PeerNeighborState.Closed)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(
                        this.ToString()));
                }
            }

            // Throws if the new state being set on the neighbor is invalid compared to the 
            // current state (such as setting state to connecting when it is already in
            // disconnected state). Also throws if neighbor is already closed.
            // NOTE: This method should be called within the lock.
            void ThrowIfInvalidState(PeerNeighborState newState)
            {
                if (this.state == PeerNeighborState.Closed)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(
                        this.ToString()));
                }
                if (this.state >= newState)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.PeerNeighborInvalidState, this.state.ToString(),
                        newState.ToString())));
                }
            }

            public void TraceClosedEvent(PeerNeighborState previousState)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceEventType severity = TraceEventType.Information;

                    // Override tracing severity based on close reason
                    switch (this.closeReason)
                    {
                        case PeerCloseReason.InvalidNeighbor:
                        case PeerCloseReason.DuplicateNodeId:
                            severity = TraceEventType.Error;
                            break;

                        case PeerCloseReason.ConnectTimedOut:
                        case PeerCloseReason.InternalFailure:
                        case PeerCloseReason.Faulted:
                            severity = TraceEventType.Warning;
                            break;
                    }

                    PeerNeighborCloseTraceRecord record = new PeerNeighborCloseTraceRecord(
                        this.nodeId, this.config.NodeId, null, null,
                        this.GetHashCode(), this.initiator,
                        PeerNeighborState.Closed.ToString(), previousState.ToString(), null,
                        this.closeInitiator.ToString(), this.closeReason.ToString()
                    );

                    TraceUtility.TraceEvent(severity, TraceCode.PeerNeighborStateChanged,
                        SR.GetString(SR.TraceCodePeerNeighborStateChanged), record, this, this.closeException);
                }
            }

            public void TraceEventHelper(TraceEventType severity, int traceCode, string traceDescription)
            {
                PeerNeighborState nbrState = this.state;
                this.TraceEventHelper(severity, traceCode, traceDescription, null, null, nbrState, nbrState);
            }

            public void TraceEventHelper(TraceEventType severity, int traceCode, string traceDescription, Exception e)
            {
                PeerNeighborState nbrState = this.state;
                this.TraceEventHelper(severity, traceCode, traceDescription, e, null, nbrState, nbrState);
            }

            public void TraceEventHelper(TraceEventType severity, int traceCode, string traceDescription, Exception e,
                string action, PeerNeighborState nbrState, PeerNeighborState previousOrAttemptedState)
            {
                if (DiagnosticUtility.ShouldTrace(severity))
                {
                    string attemptedState = null;
                    string previousState = null;
                    PeerNodeAddress listenAddr = null;
                    IPAddress connectIPAddr = null;

                    if (nbrState >= PeerNeighborState.Opened && nbrState <= PeerNeighborState.Connected)
                    {
                        listenAddr = this.ListenAddress;
                        connectIPAddr = this.ConnectIPAddress;
                    }

                    if (traceCode == TraceCode.PeerNeighborStateChangeFailed)
                        attemptedState = previousOrAttemptedState.ToString();
                    else if (traceCode == TraceCode.PeerNeighborStateChanged)
                        previousState = previousOrAttemptedState.ToString();

                    PeerNeighborTraceRecord record = new PeerNeighborTraceRecord(this.nodeId,
                        this.config.NodeId, listenAddr, connectIPAddr, this.GetHashCode(),
                        this.initiator, nbrState.ToString(), previousState, attemptedState, action);

                    if (severity == TraceEventType.Verbose && e != null)
                        severity = TraceEventType.Information; // need to be >= info for exceptions

                    TraceUtility.TraceEvent(severity, traceCode, traceDescription, record, this, e);
                }
            }

            // Helper class to implement PeerNeighbor's AsyncOpen by iterating over the IPAddress array
            class OpenAsyncResult : AsyncResult
            {
                bool completedSynchronously;
                ClosedCallback closed;
                int currentIndex;           // index into the ipAddress array
                PeerNeighbor neighbor;
                PeerNodeAddress remoteAddress;
                Binding binding;
                PeerService service;
                AsyncCallback onOpen;
                Exception lastException;
                TimeoutHelper timeoutHelper;

                public OpenAsyncResult(PeerNeighbor neighbor, PeerNodeAddress remoteAddress, Binding binding,
                    PeerService service, ClosedCallback closedCallback, TimeSpan timeout,
                    AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    Fx.Assert(remoteAddress != null && remoteAddress.IPAddresses.Count > 0, "Non-empty IPAddress collection expected");

                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.neighbor = neighbor;
                    this.currentIndex = 0;
                    this.completedSynchronously = true;         // initially
                    this.remoteAddress = remoteAddress;
                    this.service = service;
                    this.binding = binding;
                    this.onOpen = Fx.ThunkCallback(new AsyncCallback(OnOpen));
                    this.closed = closedCallback;
                    BeginOpen();
                }

                void BeginOpen()
                {
                    bool success = false;

                    try
                    {
                        while (this.currentIndex < this.remoteAddress.IPAddresses.Count)
                        {
                            EndpointAddress remoteAddress = PeerIPHelper.GetIPEndpointAddress(
                                        this.remoteAddress.EndpointAddress, this.remoteAddress.IPAddresses[this.currentIndex]);
                            if (this.closed())
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().ToString()));
                            }
                            try
                            {
                                this.neighbor.ConnectIPAddress = this.remoteAddress.IPAddresses[this.currentIndex];
                                IAsyncResult result = this.neighbor.BeginOpenProxy(remoteAddress, binding, new InstanceContext(null, service, false), this.timeoutHelper.RemainingTime(), onOpen, null);
                                if (!result.CompletedSynchronously)
                                {
                                    return;
                                }

                                this.neighbor.EndOpenProxy(result);
                                this.lastException = null;
                                success = true;
                                neighbor.isClosing = false;
                                break;
                            }
#pragma warning suppress 56500 // covered by FxCOP
                            catch (Exception e)
                            {
                                if (Fx.IsFatal(e)) throw;
                                try
                                {
                                    this.neighbor.CleanupProxy();
                                }
                                catch (Exception ee)
                                {
                                    if (Fx.IsFatal(ee)) throw;
                                    DiagnosticUtility.TraceHandledException(ee, TraceEventType.Information);
                                }
                                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                                if (!ContinuableException(e)) throw;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        this.lastException = e;
                    }

                    // Indicate completion to the caller
                    if (success)
                    {
                        Fx.Assert(this.lastException == null, "lastException expected to be null");
                    }
                    else
                    {
                        Fx.Assert(this.lastException != null, "lastException expected to be non-null");
                    }
                    base.Complete(this.completedSynchronously, this.lastException);
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<OpenAsyncResult>(result);
                }

                // Checks if the exception can be handled
                bool ContinuableException(Exception exception)
                {
                    if (
                            (
                                exception is EndpointNotFoundException
                                || exception is TimeoutException
                            )
                            && timeoutHelper.RemainingTime() > TimeSpan.Zero
                        )
                    {
                        this.lastException = exception;
                        this.currentIndex++;
                        return true;
                    }
                    return false;
                }

                // Open completion callback. If open failed, reattempts with the next IP address in the list
                void OnOpen(IAsyncResult result)
                {
                    Exception exception = null;
                    bool completed = false;

                    if (!result.CompletedSynchronously)
                    {
                        this.completedSynchronously = false;
                        try
                        {
                            this.neighbor.EndOpenProxy(result);
                            completed = true;
                            neighbor.isClosing = false;
                        }
#pragma warning suppress 56500 // covered by FxCOP
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e)) throw;
                            try
                            {
                                this.neighbor.CleanupProxy();
                            }
                            catch (Exception ee)
                            {
                                if (Fx.IsFatal(ee)) throw;
                                DiagnosticUtility.TraceHandledException(ee, TraceEventType.Information);
                            }
                            exception = e;
                            if (ContinuableException(exception))
                            {
                                // attempt connection with the next IP address
                                try
                                {
                                    BeginOpen();
                                }
                                catch (Exception ee)
                                {
                                    if (Fx.IsFatal(ee)) throw;
                                    DiagnosticUtility.TraceHandledException(ee, TraceEventType.Information);
                                }
                            }
                            else
                            {
                                completed = true;
                            }
                        }
                    }

                    if (completed)
                        base.Complete(this.completedSynchronously, exception);
                }
            }

            #region IInputSessionShutdown Members

            void IInputSessionShutdown.ChannelFaulted(IDuplexContextChannel channel)
            {
                //Noop
            }

            void IInputSessionShutdown.DoneReceiving(IDuplexContextChannel channel)
            {
                //Close it if the neighbor it was connected to has disconnected
                if (channel.State == CommunicationState.Opened)
                {
                    channel.Close();
                }
            }

            #endregion

            public UtilityExtension Utility
            {
                get
                {
                    if (this.utility == null)
                    {
                        this.utility = this.Extensions.Find<UtilityExtension>();
                    }
                    return this.utility;
                }
            }
        }

        // Helper class to implement PeerNeighborManager's async neighbor open
        class NeighborOpenAsyncResult : AsyncResult
        {
            PeerNeighbor neighbor;

            // ClosedCallback is a delegate to determine if caller has closed. If so, we bail out of open operation
            public NeighborOpenAsyncResult(PeerNeighbor neighbor, PeerNodeAddress remoteAddress, Binding binding,
                PeerService service, ClosedCallback closedCallback, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.neighbor = neighbor;

                IAsyncResult result = null;
                try
                {
                    result = neighbor.BeginOpen(remoteAddress, binding, service, closedCallback, timeout,
                        Fx.ThunkCallback(new AsyncCallback(OnOpen)), null);
                    if (result.CompletedSynchronously)
                    {
                        neighbor.EndOpen(result);
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e)) throw;
                    neighbor.TraceEventHelper(TraceEventType.Warning, TraceCode.PeerNeighborOpenFailed, SR.GetString(SR.TraceCodePeerNeighborOpenFailed));
                    throw;
                }

                // Indicate sync completion to the caller
                if (result.CompletedSynchronously)
                    base.Complete(true);
            }

            void OnOpen(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    Exception exception = null;

                    try
                    {
                        this.neighbor.EndOpen(result);
                    }
#pragma warning suppress 56500 // covered by FxCOP
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e)) throw;
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        neighbor.TraceEventHelper(TraceEventType.Warning, TraceCode.PeerNeighborOpenFailed, SR.GetString(SR.TraceCodePeerNeighborOpenFailed));
                        exception = e;
                    }

                    base.Complete(result.CompletedSynchronously, exception);
                }
            }

            public static IPeerNeighbor End(IAsyncResult result)
            {
                NeighborOpenAsyncResult asyncResult = AsyncResult.End<NeighborOpenAsyncResult>(result);
                return asyncResult.neighbor;
            }
        }
        class PeerNeighborBehavior : IEndpointBehavior
        {
            PeerNeighbor neighbor;

            public PeerNeighborBehavior(PeerNeighbor neighbor)
            {
                this.neighbor = neighbor;
            }

            #region IEndpointBehavior Members

            public void Validate(ServiceEndpoint serviceEndpoint)
            {
            }

            public void AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection bindingParameters)
            {
            }

            public void ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
            {
            }

            public void ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
            {
                behavior.DispatchRuntime.InputSessionShutdownHandlers.Add(this.neighbor);
            }

            #endregion
        }

        public IPeerNeighbor SlowestNeighbor()
        {
            List<IPeerNeighbor> neighbors = this.GetConnectedNeighbors();
            IPeerNeighbor slowNeighbor = null;
            UtilityExtension utility = null;
            //if the neighbor has below this number, we wont consider for pruning
            int pending = PeerTransportConstants.MessageThreshold;
            foreach (IPeerNeighbor peer in neighbors)
            {
                utility = peer.Utility;
                if (utility == null || !peer.IsConnected)
                    continue;
                if (utility.PendingMessages > pending)
                {
                    slowNeighbor = peer;
                    pending = utility.PendingMessages;
                }
            }
            return slowNeighbor;
        }

    }
}
