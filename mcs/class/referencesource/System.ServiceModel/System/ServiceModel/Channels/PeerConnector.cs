//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Threading;

    // Connector is responsible for transitioning neighbors to connected state. 
    class PeerConnector : IPeerConnectorContract
    {
        enum State
        {
            Created,
            Opened,
            Closed,
            Closing
        }

        PeerNodeConfig config;
        PeerMaintainer maintainer;
        PeerNeighborManager neighborManager;
        State state;
        object thisLock;

        // TypedMessageConverters:
        TypedMessageConverter connectInfoMessageConverter;
        TypedMessageConverter disconnectInfoMessageConverter;
        TypedMessageConverter refuseInfoMessageConverter;
        TypedMessageConverter welcomeInfoMessageConverter;

        // To keep track of timers to transition neighbors to connected state
        Dictionary<IPeerNeighbor, IOThreadTimer> timerTable;

        public PeerConnector(PeerNodeConfig config, PeerNeighborManager neighborManager,
            PeerMaintainer maintainer)
        {
            Fx.Assert(config != null, "Config is expected to non-null");
            Fx.Assert(neighborManager != null, "NeighborManager is expected to be non-null");
            Fx.Assert(maintainer != null, "Maintainer is expected to be non-null");
            Fx.Assert(config.NodeId != PeerTransportConstants.InvalidNodeId, "Invalid NodeId");
            Fx.Assert(config.MaxNeighbors > 0, "MaxNeighbors is expected to be non-zero positive value");
            Fx.Assert(config.ConnectTimeout > 0, "ConnectTimeout is expected to be non-zero positive value");

            this.thisLock = new object();
            this.config = config;
            this.neighborManager = neighborManager;
            this.maintainer = maintainer;
            this.timerTable = new Dictionary<IPeerNeighbor, IOThreadTimer>();
            this.state = State.Created;
        }

        object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }


        internal TypedMessageConverter ConnectInfoMessageConverter
        {
            get
            {
                if (connectInfoMessageConverter == null)
                {
                    connectInfoMessageConverter = TypedMessageConverter.Create(typeof(ConnectInfo), PeerStrings.ConnectAction);
                }
                return connectInfoMessageConverter;
            }
        }

        internal TypedMessageConverter DisconnectInfoMessageConverter
        {
            get
            {
                if (disconnectInfoMessageConverter == null)
                {
                    disconnectInfoMessageConverter = TypedMessageConverter.Create(typeof(DisconnectInfo), PeerStrings.DisconnectAction);
                }
                return disconnectInfoMessageConverter;
            }
        }

        internal TypedMessageConverter RefuseInfoMessageConverter
        {
            get
            {
                if (refuseInfoMessageConverter == null)
                {
                    refuseInfoMessageConverter = TypedMessageConverter.Create(typeof(RefuseInfo), PeerStrings.RefuseAction);
                }
                return refuseInfoMessageConverter;
            }
        }

        internal TypedMessageConverter WelcomeInfoMessageConverter
        {
            get
            {
                if (welcomeInfoMessageConverter == null)
                {
                    welcomeInfoMessageConverter = TypedMessageConverter.Create(typeof(WelcomeInfo), PeerStrings.WelcomeAction);
                }
                return welcomeInfoMessageConverter;
            }
        }

        // Add a timer for the specified neighbor to the timer table. The timer is only added
        // if Connector is open and the neighbor is in Connecting state.
        bool AddTimer(IPeerNeighbor neighbor)
        {
            bool added = false;

            lock (ThisLock)
            {
                if (state == State.Opened && neighbor.State == PeerNeighborState.Connecting)
                {
                    IOThreadTimer timer = new IOThreadTimer(new Action<object>(OnConnectTimeout), neighbor, true);
                    timer.Set(this.config.ConnectTimeout);
                    this.timerTable.Add(neighbor, timer);
                    added = true;
                }
            }

            return added;
        }

        //this method takes care of closing the message.
        void SendMessageToNeighbor(IPeerNeighbor neighbor, Message message, PeerMessageHelpers.CleanupCallback cleanupCallback)
        {
            bool fatal = false;
            try
            {
                neighbor.Send(message);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    fatal = true;
                    throw;
                }
                if (e is CommunicationException ||
                    e is QuotaExceededException ||
                    e is ObjectDisposedException ||
                    e is TimeoutException)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    // Message failed to transmit due to quota exceeding or channel failure
                    if (cleanupCallback != null)
                    {
                        cleanupCallback(neighbor, PeerCloseReason.InternalFailure, e);
                    }
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (!fatal)
                    message.Close();
            }
        }

        // If neighbor cannot transition to connected state, this method cleans up the timer and 
        // closes the neighbor
        void CleanupOnConnectFailure(IPeerNeighbor neighbor, PeerCloseReason reason,
            Exception exception)
        {
            // timer will not be found if neighbor is already closed or connected.
            if (RemoveTimer(neighbor))
            {
                this.neighborManager.CloseNeighbor(neighbor, reason,
                    PeerCloseInitiator.LocalNode, exception);
            }
        }

        public void Close()
        {
            Dictionary<IPeerNeighbor, IOThreadTimer> table;

            lock (ThisLock)
            {
                table = this.timerTable;
                this.timerTable = null;
                this.state = State.Closed;

            }

            // Cancel each timer
            if (table != null)
            {
                foreach (IOThreadTimer timer in table.Values)
                    timer.Cancel();
            }
        }

        public void Closing()
        {
            lock (ThisLock)
            {
                this.state = State.Closing;
            }
        }

        // Complete processing of Disconnect or Refuse message from the neighbor
        void CompleteTerminateMessageProcessing(IPeerNeighbor neighbor,
            PeerCloseReason closeReason, IList<Referral> referrals)
        {
            // Close the neighbor after setting the neighbor state to Disconnected.
            // The set can fail if the neighbor is already being closed and that is ok.
            if (neighbor.TrySetState(PeerNeighborState.Disconnected))
                this.neighborManager.CloseNeighbor(neighbor, closeReason, PeerCloseInitiator.RemoteNode);
            else
                if (!(neighbor.State >= PeerNeighborState.Disconnected))
                {
                    throw Fx.AssertAndThrow("Unexpected neighbor state");
                }

            // Hand over the referrals to maintainer
            this.maintainer.AddReferrals(referrals, neighbor);
        }

        void OnConnectFailure(IPeerNeighbor neighbor, PeerCloseReason reason,
            Exception exception)
        {
            CleanupOnConnectFailure(neighbor, reason, exception);
        }

        void OnConnectTimeout(object asyncState)
        {
            CleanupOnConnectFailure((IPeerNeighbor)asyncState, PeerCloseReason.ConnectTimedOut, null);
        }

        // Process neighbor closed notification.
        public void OnNeighborClosed(IPeerNeighbor neighbor)
        {
            // If the neighbor is closed abruptly by the remote node, OnNeighborClosing will 
            // not be invoked. Remove neighbor's timer from the table.
            RemoveTimer(neighbor);
        }

        // Process neighbor closing notification.
        public void OnNeighborClosing(IPeerNeighbor neighbor, PeerCloseReason closeReason)
        {
            // Send Disconnect message to a Connected neighbor
            if (neighbor.IsConnected)
                SendTerminatingMessage(neighbor, PeerStrings.DisconnectAction, closeReason);
        }

        // Process neighbor authenticated notification
        public void OnNeighborAuthenticated(IPeerNeighbor neighbor)
        {
            if (!(this.state != State.Created))
            {
                throw Fx.AssertAndThrow("Connector not expected to be in Created state");
            }

            if (!(PeerNeighborStateHelper.IsAuthenticatedOrClosed(neighbor.State)))
            {
                throw Fx.AssertAndThrow(string.Format(CultureInfo.InvariantCulture, "Neighbor state expected to be Authenticated or Closed, actual state: {0}", neighbor.State));
            }

            // setting the state fails if neighbor is already closed or closing
            // If so, we have nothing to do.
            if (!neighbor.TrySetState(PeerNeighborState.Connecting))
            {
                if (!(neighbor.State >= PeerNeighborState.Faulted))
                {
                    throw Fx.AssertAndThrow(string.Format(CultureInfo.InvariantCulture, "Neighbor state expected to be Faulted or Closed, actual state: {0}", neighbor.State));
                }
                return;
            }

            // Add a timer to timer table to transition the neighbor to connected state
            // within finite duration. The neighbor is closed if the timer fires and the
            // neighbor has not reached connected state.
            // The timer is not added if neighbor or connector are closed
            if (AddTimer(neighbor))
            {
                // Need to send connect message if the neighbor is the initiator
                if (neighbor.IsInitiator)
                {
                    if (this.neighborManager.ConnectedNeighborCount < this.config.MaxNeighbors)
                        SendConnect(neighbor);
                    else
                    {
                        // We have max connected neighbors already. So close this one.
                        this.neighborManager.CloseNeighbor(neighbor, PeerCloseReason.NodeBusy,
                            PeerCloseInitiator.LocalNode);
                    }
                }
            }
        }

        public void Open()
        {
            lock (ThisLock)
            {
                if (!(this.state == State.Created))
                {
                    throw Fx.AssertAndThrow("Connector expected to be in Created state");
                }
                this.state = State.Opened;
            }
        }

        //<Implementation of PeerConnector.IPeerConnectorContract>
        // Process Connect from the neighbor
        public void Connect(IPeerNeighbor neighbor, ConnectInfo connectInfo)
        {
            // Don't bother processing the message if Connector has closed
            if (this.state != State.Opened)
                return;

            PeerCloseReason closeReason = PeerCloseReason.None;

            // A connect message should only be received by a responder neighbor that is
            // in Connecting state. If not, we close the neighbor without bothering 
            // to send a Refuse message
            // A malicious neighbor can format a message with a null connectInfo as an argument
            if (neighbor.IsInitiator || !connectInfo.HasBody() || (neighbor.State != PeerNeighborState.Connecting &&
                neighbor.State != PeerNeighborState.Closed))
            {
                closeReason = PeerCloseReason.InvalidNeighbor;
            }

            // Remove the timer from the timer table for this neighbor. If the timer is not
            // present, the neighbor is already being closed and the Connect message should 
            // be ignored.
            else if (RemoveTimer(neighbor))
            {
                // Determine if Welcome or Refuse should be sent

                // Refuse if node has maximum allowed connected neighbors?
                if (this.neighborManager.ConnectedNeighborCount >= this.config.MaxNeighbors)
                    closeReason = PeerCloseReason.NodeBusy;
                else
                {
                    // Deserialization failed or connect info is invalid?
                    if (!PeerValidateHelper.ValidNodeAddress(connectInfo.Address))
                    {
                        closeReason = PeerCloseReason.InvalidNeighbor;
                    }
                    else
                    {
                        // Determine if neighbor should be accepted.
                        PeerCloseReason closeReason2;
                        IPeerNeighbor neighborToClose;
                        string action = PeerStrings.RefuseAction;
                        ValidateNeighbor(neighbor, connectInfo.NodeId, out neighborToClose, out closeReason2, out action);

                        if (neighbor != neighborToClose)    // new neighbor should be accepted
                        {
                            SendWelcome(neighbor);
                            try
                            {
                                neighbor.ListenAddress = connectInfo.Address;
                            }
                            catch (ObjectDisposedException e)
                            {
                                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                            }

                            if (!neighbor.TrySetState(PeerNeighborState.Connected))
                                if (!(neighbor.State >= PeerNeighborState.Disconnecting))
                                {
                                    throw Fx.AssertAndThrow("Neighbor state expected to be >= Disconnecting; it is " + neighbor.State.ToString());
                                }

                            if (neighborToClose != null)
                            {
                                // The other neighbor should be closed
                                SendTerminatingMessage(neighborToClose, action, closeReason2);
                                this.neighborManager.CloseNeighbor(neighborToClose, closeReason2, PeerCloseInitiator.LocalNode);
                            }
                        }
                        else
                            closeReason = closeReason2;
                    }
                }
            }

            if (closeReason != PeerCloseReason.None)
            {
                SendTerminatingMessage(neighbor, PeerStrings.RefuseAction, closeReason);
                this.neighborManager.CloseNeighbor(neighbor, closeReason, PeerCloseInitiator.LocalNode);
            }
        }

        // Process Disconnect message from the neighbor
        public void Disconnect(IPeerNeighbor neighbor, DisconnectInfo disconnectInfo)
        {
            // Don't bother processing the message if Connector has closed
            if (this.state != State.Opened)
                return;

            PeerCloseReason closeReason = PeerCloseReason.InvalidNeighbor;
            IList<Referral> referrals = null;

            if (disconnectInfo.HasBody())
            {
                // We should only receive Disconnect message after the neighbor has transitioned
                // to connected state.
                if (neighbor.State >= PeerNeighborState.Connected)
                {
                    if (PeerConnectorHelper.IsDefined(disconnectInfo.Reason))
                    {
                        closeReason = (PeerCloseReason)disconnectInfo.Reason;
                        referrals = disconnectInfo.Referrals;
                    }
                }
            }

            // Complete processing of disconnect message
            CompleteTerminateMessageProcessing(neighbor, closeReason, referrals);
        }


        // Process Refuse message from the neighbor
        public void Refuse(IPeerNeighbor neighbor, RefuseInfo refuseInfo)
        {
            // Don't bother processing the message if Connector has closed
            if (this.state != State.Opened)
                return;

            PeerCloseReason closeReason = PeerCloseReason.InvalidNeighbor;
            IList<Referral> referrals = null;

            if (refuseInfo.HasBody())
            {
                // Refuse message should only be received when neighbor is the initiator
                // and is in connecting state --we accept in closed state to account for
                // timeouts.
                if (neighbor.IsInitiator && (neighbor.State == PeerNeighborState.Connecting ||
                    neighbor.State == PeerNeighborState.Closed))
                {
                    // Remove the entry from timer table for this neighbor
                    RemoveTimer(neighbor);

                    if (PeerConnectorHelper.IsDefined(refuseInfo.Reason))
                    {
                        closeReason = (PeerCloseReason)refuseInfo.Reason;
                        referrals = refuseInfo.Referrals;
                    }
                }
            }
            // Complete processing of refuse message
            CompleteTerminateMessageProcessing(neighbor, closeReason, referrals);
        }

        // Process Welcome message from the neighbor
        public void Welcome(IPeerNeighbor neighbor, WelcomeInfo welcomeInfo)
        {
            // Don't bother processing the message if Connector has closed
            if (this.state != State.Opened)
                return;

            PeerCloseReason closeReason = PeerCloseReason.None;

            // Welcome message should only be received when neighbor is the initiator
            // and is in connecting state --we accept in closed state to account for
            // timeouts.
            if (!neighbor.IsInitiator || !welcomeInfo.HasBody() || (neighbor.State != PeerNeighborState.Connecting &&
                neighbor.State != PeerNeighborState.Closed))
            {
                closeReason = PeerCloseReason.InvalidNeighbor;
            }
            // Remove the entry from timer table for this neighbor. If entry is still present,
            // RemoveTimer returns true. Otherwise, neighbor is already being closed and 
            // welcome message will be ignored.
            else if (RemoveTimer(neighbor))
            {
                // It is allowed for a node to have more than MaxNeighbours when processing a welcome message
                // Determine if neighbor should be accepted.
                PeerCloseReason closeReason2;
                IPeerNeighbor neighborToClose;
                string action = PeerStrings.RefuseAction;
                ValidateNeighbor(neighbor, welcomeInfo.NodeId, out neighborToClose, out closeReason2, out action);

                if (neighbor != neighborToClose)
                {
                    // Neighbor should be accepted AddReferrals validates the referrals, 
                    // if they are valid then the neighbor is accepted.
                    if (this.maintainer.AddReferrals(welcomeInfo.Referrals, neighbor))
                    {
                        if (!neighbor.TrySetState(PeerNeighborState.Connected))
                        {
                            if (!(neighbor.State >= PeerNeighborState.Faulted))
                            {
                                throw Fx.AssertAndThrow("Neighbor state expected to be >= Faulted; it is " + neighbor.State.ToString());
                            }
                        }

                        if (neighborToClose != null)
                        {
                            // The other neighbor should be closed
                            SendTerminatingMessage(neighborToClose, action, closeReason2);
                            this.neighborManager.CloseNeighbor(neighborToClose, closeReason2, PeerCloseInitiator.LocalNode);
                        }
                    }
                    else
                    {
                        // Referrals were invalid this node is suspicous
                        closeReason = PeerCloseReason.InvalidNeighbor;
                    }
                }
                else
                {
                    closeReason = closeReason2;
                }
            }

            if (closeReason != PeerCloseReason.None)
            {
                SendTerminatingMessage(neighbor, PeerStrings.DisconnectAction, closeReason);
                this.neighborManager.CloseNeighbor(neighbor, closeReason, PeerCloseInitiator.LocalNode);
            }
        }

        bool RemoveTimer(IPeerNeighbor neighbor)
        {
            IOThreadTimer timer = null;
            bool removed = false;

            // Remove the timer from the table and cancel it. Do this if Connector is
            // still open. Otherwise, Close method will have already cancelled the timers.
            lock (ThisLock)
            {
                if (this.state == State.Opened &&
                    this.timerTable.TryGetValue(neighbor, out timer))
                {
                    removed = this.timerTable.Remove(neighbor);
                }
            }
            if (timer != null)
            {
                timer.Cancel();
                if (!removed)
                {
                    throw Fx.AssertAndThrow("Neighbor key should have beeen removed from the table");
                }
            }

            return removed;
        }

        void SendConnect(IPeerNeighbor neighbor)
        {
            // We do not attempt to send the message if PeerConnector is not open
            if (neighbor.State == PeerNeighborState.Connecting && this.state == State.Opened)
            {
                // Retrieve the local address. The retrieved address may be null if the node 
                // is shutdown. In that case, don't bother to send connect message since the 
                // node is closing...
                PeerNodeAddress listenAddress = this.config.GetListenAddress(true);
                if (listenAddress != null)
                {
                    ConnectInfo connectInfo = new ConnectInfo(this.config.NodeId, listenAddress);
                    Message message = ConnectInfoMessageConverter.ToMessage(connectInfo, MessageVersion.Soap12WSAddressing10);
                    SendMessageToNeighbor(neighbor, message, OnConnectFailure);
                }
            }
        }

        // Send Disconnect or Refuse message
        void SendTerminatingMessage(IPeerNeighbor neighbor, string action, PeerCloseReason closeReason)
        {
            // We do not attempt to send the message if Connector is not open
            // or if the close reason is InvalidNeighbor.
            if (this.state != State.Opened || closeReason == PeerCloseReason.InvalidNeighbor)
                return;

            // Set the neighbor state to disconnecting. TrySetState can fail if the 
            // neighbor is already being closed. Disconnect/Refuse msg not sent in that case.
            if (neighbor.TrySetState(PeerNeighborState.Disconnecting))
            {
                // Get referrals from the maintainer
                Referral[] referrals = maintainer.GetReferrals();

                // Build and send the message
                Message message;
                if (action == PeerStrings.DisconnectAction)
                {
                    DisconnectInfo disconnectInfo = new DisconnectInfo((DisconnectReason)closeReason, referrals);
                    message = DisconnectInfoMessageConverter.ToMessage(disconnectInfo, MessageVersion.Soap12WSAddressing10);
                }
                else
                {
                    RefuseInfo refuseInfo = new RefuseInfo((RefuseReason)closeReason, referrals);
                    message = RefuseInfoMessageConverter.ToMessage(refuseInfo, MessageVersion.Soap12WSAddressing10);
                }
                SendMessageToNeighbor(neighbor, message, null);
            }
            else
                if (!(neighbor.State >= PeerNeighborState.Disconnecting))
                {
                    throw Fx.AssertAndThrow("Neighbor state expected to be >= Disconnecting; it is " + neighbor.State.ToString());
                }
        }

        void SendWelcome(IPeerNeighbor neighbor)
        {
            // We do not attempt to send the message if PeerConnector is not open
            if (state == State.Opened)
            {
                // Get referrals from the maintainer
                Referral[] referrals = maintainer.GetReferrals();

                WelcomeInfo welcomeInfo = new WelcomeInfo(this.config.NodeId, referrals);
                Message message = WelcomeInfoMessageConverter.ToMessage(welcomeInfo, MessageVersion.Soap12WSAddressing10);
                SendMessageToNeighbor(neighbor, message, OnConnectFailure);
            }
        }

        // Validates the new neighbor based on its node ID. If it detects duplicate neighbor condition,
        // it will return reference to the neighbor that should be closed.
        void ValidateNeighbor(IPeerNeighbor neighbor, ulong neighborNodeId,
            out IPeerNeighbor neighborToClose, out PeerCloseReason closeReason, out string action)
        {
            neighborToClose = null;
            closeReason = PeerCloseReason.None;
            action = null;

            // Invalid neighbor node Id?
            if (neighborNodeId == PeerTransportConstants.InvalidNodeId)
            {
                neighborToClose = neighbor;
                closeReason = PeerCloseReason.InvalidNeighbor;
            }
            // Neighbor's node ID matches local node Id?
            else if (neighborNodeId == this.config.NodeId)
            {
                neighborToClose = neighbor;
                closeReason = PeerCloseReason.DuplicateNodeId;
            }
            else
            {
                // Check for duplicate neighbors (i.e., if another neighbor has the
                // same node Id as the new neighbor).
                // Set neighbor's node Id prior to calling FindDuplicateNeighbor.
                try
                {
                    neighbor.NodeId = neighborNodeId;
                }
                catch (ObjectDisposedException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    return;
                }

                IPeerNeighbor duplicateNeighbor =
                    this.neighborManager.FindDuplicateNeighbor(neighborNodeId, neighbor);
                if (duplicateNeighbor != null && this.neighborManager.PingNeighbor(duplicateNeighbor))
                {
                    // We have a duplicate neighbor. Determine which one should be closed
                    closeReason = PeerCloseReason.DuplicateNeighbor;

                    // In the corner case where both neighbors are initiated by the same node, 
                    // close the new neighbor -- Maintainer is expected to check if there is 
                    // already a connection to a node prior to initiating a new connection.
                    if (neighbor.IsInitiator == duplicateNeighbor.IsInitiator)
                        neighborToClose = neighbor;

                    // Otherwise, close the neighbor that was initiated by the node with the 
                    // larger node ID -- this ensures that both nodes tear down the same link.
                    else if (this.config.NodeId > neighborNodeId)
                        neighborToClose = (neighbor.IsInitiator ? neighbor : duplicateNeighbor);
                    else
                        neighborToClose = (neighbor.IsInitiator ? duplicateNeighbor : neighbor);
                }
            }

            if (neighborToClose != null)
            {
                // If we decided to close the other neighbor, go ahead and do it.
                if (neighborToClose != neighbor)
                {
                    // Send Disconnect or Refuse message depending on its state
                    if (neighborToClose.State == PeerNeighborState.Connected)
                    {
                        action = PeerStrings.DisconnectAction;
                    }
                    else if (!neighborToClose.IsInitiator &&
                        neighborToClose.State == PeerNeighborState.Connecting)
                    {
                        action = PeerStrings.RefuseAction;
                    }
                }
            }
        }
    }
}
