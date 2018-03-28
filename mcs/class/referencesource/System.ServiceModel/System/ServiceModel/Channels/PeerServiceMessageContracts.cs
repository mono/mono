//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.ServiceModel.Diagnostics;


    [MessageContract(IsWrapped = false)]
    class ConnectInfo
    {
        [DataContract(Name = "ConnectInfo", Namespace = PeerStrings.Namespace)]
        class ConnectInfoDC
        {
            [DataMember(Name = "NodeId")]
            public ulong nodeId;

            [DataMember(Name = "Address")]
            public PeerNodeAddress address;

            public ConnectInfoDC() { }
            public ConnectInfoDC(ulong nodeId, PeerNodeAddress address)
            {
                this.nodeId = nodeId;
                this.address = address;
            }
        }

        [MessageBodyMember(Name = "Connect", Namespace = PeerStrings.Namespace)]
        ConnectInfoDC body;

        public ConnectInfo()
        {
            this.body = new ConnectInfoDC();
        }

        public ConnectInfo(ulong nodeId, PeerNodeAddress address)
        {
            this.body = new ConnectInfoDC(nodeId, address);
        }

        public PeerNodeAddress Address
        {
            get { return this.body.address; }
        }

        public ulong NodeId
        {
            get { return this.body.nodeId; }
        }

        public bool HasBody()
        {
            return body != null;
        }
    }

    [MessageContract(IsWrapped = false)]
    class DisconnectInfo
    {
        [DataContract(Name = "DisconnectInfo", Namespace = PeerStrings.Namespace)]
        class DisconnectInfoDC
        {
            [DataMember(Name = "Reason")]
            public DisconnectReason reason;

            [DataMember(Name = "Referrals")]
            public Referral[] referrals;

            public DisconnectInfoDC() { }

            public DisconnectInfoDC(DisconnectReason reason, Referral[] referrals)
            {
                this.reason = reason;
                this.referrals = referrals;
            }
        }

        [MessageBodyMember(Name = "Disconnect", Namespace = PeerStrings.Namespace)]
        DisconnectInfoDC body;

        public DisconnectInfo()
        {
            body = new DisconnectInfoDC();
        }

        public DisconnectInfo(DisconnectReason reason, Referral[] referrals)
        {
            this.body = new DisconnectInfoDC(reason, referrals);
        }

        public DisconnectReason Reason
        {
            get { return this.body.reason; }
        }

        public IList<Referral> Referrals
        {
            get
            {
                return this.body.referrals != null ? Array.AsReadOnly<Referral>(this.body.referrals) : null;
            }
        }

        public bool HasBody()
        {
            return body != null;
        }
    }

    // Reasons for sending a Disconnect message
    enum DisconnectReason
    {
        LeavingMesh = PeerCloseReason.LeavingMesh,
        NotUsefulNeighbor = PeerCloseReason.NotUsefulNeighbor,
        DuplicateNeighbor = PeerCloseReason.DuplicateNeighbor,
        DuplicateNodeId = PeerCloseReason.DuplicateNodeId,
        NodeBusy = PeerCloseReason.NodeBusy,
        InternalFailure = PeerCloseReason.InternalFailure,
    }


    //
    // Service contract used for neighbor-to-neighbor communication
    // Sending messages is asynchronous and processing incoming messages is synchronous.
    // Used for Service implementation
    //
    [ServiceContract(Name = PeerStrings.ServiceContractName,
                 Namespace = PeerStrings.Namespace,
                 SessionMode = SessionMode.Required,
                 CallbackContract = typeof(IPeerServiceContract))]
    interface IPeerServiceContract
    {
        [OperationContract(IsOneWay = true, Action = PeerStrings.ConnectAction)]
        void Connect(ConnectInfo connectInfo);

        [OperationContract(IsOneWay = true, Action = PeerStrings.DisconnectAction)]
        void Disconnect(DisconnectInfo disconnectInfo);

        [OperationContract(IsOneWay = true, Action = PeerStrings.RefuseAction)]
        void Refuse(RefuseInfo refuseInfo);

        [OperationContract(IsOneWay = true, Action = PeerStrings.WelcomeAction)]
        void Welcome(WelcomeInfo welcomeInfo);

        [OperationContract(IsOneWay = true, Action = PeerStrings.FloodAction, AsyncPattern = true)]
        IAsyncResult BeginFloodMessage(Message floodedInfo, AsyncCallback callback, object state);
        void EndFloodMessage(IAsyncResult result);

        [OperationContract(IsOneWay = true, Action = PeerStrings.LinkUtilityAction)]
        void LinkUtility(UtilityInfo utilityInfo);

        [OperationContract(
            Action = TrustFeb2005Strings.RequestSecurityToken,
            ReplyAction = TrustFeb2005Strings.RequestSecurityTokenResponse)]
        Message ProcessRequestSecurityToken(Message message);

        [OperationContract(IsOneWay = true, Action = PeerStrings.PingAction)]
        void Ping(Message message);

        [OperationContract(IsOneWay = true, Action = Addressing10Strings.FaultAction)]
        void Fault(Message message);

    }

    [ServiceContract(Name = PeerStrings.ServiceContractName,
                 Namespace = PeerStrings.Namespace,
                 SessionMode = SessionMode.Required,
                 CallbackContract = typeof(IPeerService))]
    interface IPeerProxy : IPeerServiceContract, IOutputChannel
    {
    }

    [ServiceContract(Name = PeerStrings.ServiceContractName,
                     Namespace = PeerStrings.Namespace,
                     SessionMode = SessionMode.Required,
                     CallbackContract = typeof(IPeerProxy))]
    interface IPeerService : IPeerServiceContract
    {
    }

    static class PeerConnectorHelper
    {
        public static bool IsDefined(DisconnectReason value)
        {
            return ((value == DisconnectReason.LeavingMesh) ||
                    (value == DisconnectReason.NotUsefulNeighbor) ||
                    (value == DisconnectReason.DuplicateNeighbor) ||
                    (value == DisconnectReason.DuplicateNodeId) ||
                    (value == DisconnectReason.NodeBusy) ||
                    (value == DisconnectReason.InternalFailure));
        }

        public static bool IsDefined(RefuseReason value)
        {
            return ((value == RefuseReason.DuplicateNodeId) ||
                    (value == RefuseReason.DuplicateNeighbor) ||
                    (value == RefuseReason.NodeBusy));
        }
    }

    [DataContract(Name = "Referral", Namespace = PeerStrings.Namespace)]
    class Referral
    {
        [DataMember(Name = "NodeId")]
        ulong nodeId;               // Referral NodeId

        [DataMember(Name = "Address")]
        PeerNodeAddress address;    // Referral address

        public Referral(ulong nodeId, PeerNodeAddress address)
        {
            this.nodeId = nodeId;
            this.address = address;
        }

        public PeerNodeAddress Address
        {
            get { return this.address; }
            set { this.address = value; }
        }

        public ulong NodeId
        {
            get { return this.nodeId; }
            set { this.nodeId = value; }
        }
    }

    [MessageContract(IsWrapped = false)]
    class RefuseInfo
    {
        [DataContract(Name = "RefuseInfo", Namespace = PeerStrings.Namespace)]
        class RefuseInfoDC
        {
            [DataMember(Name = "Reason")]
            public RefuseReason reason;

            [DataMember(Name = "Referrals")]
            public Referral[] referrals;

            public RefuseInfoDC() { }
            public RefuseInfoDC(RefuseReason reason, Referral[] referrals)
            {
                this.reason = reason;
                this.referrals = referrals;
            }
        }

        public RefuseInfo()
        {
            this.body = new RefuseInfoDC();
        }

        public RefuseInfo(RefuseReason reason, Referral[] referrals)
        {
            this.body = new RefuseInfoDC(reason, referrals);
        }

        [MessageBodyMember(Name = "Refuse", Namespace = PeerStrings.Namespace)]
        RefuseInfoDC body;

        public RefuseReason Reason
        {
            get { return this.body.reason; }
        }

        public IList<Referral> Referrals
        {
            get { return this.body.referrals != null ? Array.AsReadOnly<Referral>(this.body.referrals) : null; }
        }

        public bool HasBody()
        {
            return body != null;
        }
    }

    // Reasons for sending a Refuse message
    enum RefuseReason
    {
        DuplicateNeighbor = PeerCloseReason.DuplicateNeighbor,
        DuplicateNodeId = PeerCloseReason.DuplicateNodeId,
        NodeBusy = PeerCloseReason.NodeBusy,
    }

    [MessageContract(IsWrapped = false)]
    class UtilityInfo
    {
        [DataContract(Name = "LinkUtilityInfo", Namespace = PeerStrings.Namespace)]
        class UtilityInfoDC
        {
            [DataMember(Name = "Useful")]
            public uint useful;

            [DataMember(Name = "Total")]
            public uint total;

            public UtilityInfoDC() { }

            public UtilityInfoDC(uint useful, uint total)
            {
                this.useful = useful;
                this.total = total;
            }
        }

        public UtilityInfo()
        {
            this.body = new UtilityInfoDC();
        }

        public UtilityInfo(uint useful, uint total)
        {
            this.body = new UtilityInfoDC(useful, total);
        }

        [MessageBodyMember(Name = "LinkUtility", Namespace = PeerStrings.Namespace)]
        UtilityInfoDC body;

        public uint Useful
        {
            get { return body.useful; }
        }

        public uint Total
        {
            get { return body.total; }
        }

        public bool HasBody()
        {
            return body != null;
        }
    }

    [MessageContract(IsWrapped = false)]
    class WelcomeInfo
    {
        [DataContract(Name = "WelcomeInfo", Namespace = PeerStrings.Namespace)]
        class WelcomeInfoDC
        {
            [DataMember(Name = "NodeId")]
            public ulong nodeId;

            [DataMember(Name = "Referrals")]
            public Referral[] referrals;

            public WelcomeInfoDC() { }
            public WelcomeInfoDC(ulong nodeId, Referral[] referrals)
            {
                this.nodeId = nodeId;
                this.referrals = referrals;
            }
        }

        public WelcomeInfo()
        {
            this.body = new WelcomeInfoDC();
        }
        public WelcomeInfo(ulong nodeId, Referral[] referrals)
        {
            this.body = new WelcomeInfoDC(nodeId, referrals);
        }

        [MessageBodyMember(Name = "Welcome", Namespace = PeerStrings.Namespace)]
        WelcomeInfoDC body;

        public ulong NodeId
        {
            get { return this.body.nodeId; }
        }

        public IList<Referral> Referrals
        {
            get { return this.body.referrals != null ? Array.AsReadOnly<Referral>(this.body.referrals) : null; }
        }

        public bool HasBody()
        {
            return body != null;
        }
    }
}

