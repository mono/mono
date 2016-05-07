//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.ObjectModel;
    using System.ServiceModel;

    // Neighbor interface
    interface IPeerNeighbor : IExtensibleObject<IPeerNeighbor>
    {
        bool IsConnected { get; }       // True if the neighbor is connected
        PeerNodeAddress ListenAddress { get; set; }   // Neighbor's listen address
        bool IsInitiator { get; }
        ulong NodeId { get; set; }      // NodeID of the neighboring node
        PeerNeighborState State { get; set; }
        bool IsClosing { get; }
        IAsyncResult BeginSend(Message message, AsyncCallback callback, object state);
        IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state);
        void EndSend(IAsyncResult result);
        void Send(Message message);
        bool TrySetState(PeerNeighborState state);
        void Abort(PeerCloseReason reason, PeerCloseInitiator initiator);
        Message RequestSecurityToken(Message request);
        void Ping(Message request);
        UtilityExtension Utility { get; }
    }

    // Neighbor states
    // If add new states, carefully consider where they should occur in state transition and make
    // appropriate changes to PeerNeighbor implementation.
    enum PeerNeighborState
    {
        Created,
        Opened,
        Authenticated,
        Connecting,
        Connected,
        Disconnecting,
        Disconnected,
        Faulted,
        Closed,
    }

    static class PeerNeighborStateHelper
    {
        // Returns true if the specified state can be set for the neighbor
        public static bool IsSettable(PeerNeighborState state)
        {
            return (
                    (state == PeerNeighborState.Authenticated) ||
                    (state == PeerNeighborState.Connecting) ||
                    (state == PeerNeighborState.Connected) ||
                    (state == PeerNeighborState.Disconnecting) ||
                    (state == PeerNeighborState.Disconnected));
        }

        // Returns true if the specified state is a "connected" state
        public static bool IsConnected(PeerNeighborState state)
        {
            return ((state == PeerNeighborState.Connected));
        }

        // Returns true if the specified state is either authenticated or closing
        public static bool IsAuthenticatedOrClosed(PeerNeighborState state)
        {
            return ((state == PeerNeighborState.Authenticated) ||
                     (state == PeerNeighborState.Faulted) ||
                     (state == PeerNeighborState.Closed));
        }

    }
}
