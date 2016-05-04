//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IdentityModel.Claims;
    using System.Net;
    using System.Runtime.Diagnostics;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    class PeerNodeTraceRecord : TraceRecord
    {
        ulong id;
        string meshId;
        PeerNodeAddress address;

        public PeerNodeTraceRecord(ulong id)
        {
            this.id = id;
        }

        public PeerNodeTraceRecord(ulong id, string meshId)
        {
            this.id = id;
            this.meshId = meshId;
        }

        public PeerNodeTraceRecord(ulong id, string meshId, PeerNodeAddress address)
        {
            this.id = id;
            this.meshId = meshId;
            this.address = address;
        }

        internal override string EventId
        {
            get
            {
                return TraceRecord.EventIdBase + "PeerNode" + TraceRecord.NamespaceSuffix;
            }
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteElementString("NodeId", this.id.ToString(CultureInfo.InvariantCulture));
            if (this.meshId != null)
            {
                writer.WriteElementString("MeshId", this.meshId);
            }
            if (this.address != null)
            {
                this.address.EndpointAddress.WriteTo(AddressingVersion.WSAddressing10, writer, "LocalAddress", "");
                foreach (IPAddress address in this.address.IPAddresses)
                {
                    writer.WriteElementString("IPAddress", address.ToString());
                }
            }
        }
    }

    class PeerNeighborTraceRecord : TraceRecord
    {
        int hashCode;
        bool initiator;
        PeerNodeAddress listenAddress;
        IPAddress connectIPAddress;
        ulong localNodeId;
        ulong remoteNodeId;
        string state;
        string previousState;
        string attemptedState;
        string action;

        public PeerNeighborTraceRecord(ulong remoteNodeId, ulong localNodeId,
            PeerNodeAddress listenAddress, IPAddress connectIPAddress, int hashCode, bool initiator, string state,
            string previousState, string attemptedState, string action)
        {
            this.localNodeId = localNodeId;
            this.remoteNodeId = remoteNodeId;
            this.listenAddress = listenAddress;
            this.connectIPAddress = connectIPAddress;
            this.hashCode = hashCode;
            this.initiator = initiator;
            this.state = state;
            this.previousState = previousState;
            this.attemptedState = attemptedState;
            this.action = action;
        }

        internal override string EventId
        {
            get
            {
                return TraceRecord.EventIdBase + "PeerNeighbor" + TraceRecord.NamespaceSuffix;
            }
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteStartElement("HashCode");
            writer.WriteValue(this.hashCode);
            writer.WriteEndElement();
            if (this.remoteNodeId != 0)
                writer.WriteElementString("RemoteNodeId", this.remoteNodeId.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("LocalNodeId", this.localNodeId.ToString(CultureInfo.InvariantCulture));
            if (this.listenAddress != null)
            {
                this.listenAddress.EndpointAddress.WriteTo(AddressingVersion.WSAddressing10, writer, "ListenAddress", "");
                foreach (IPAddress address in this.listenAddress.IPAddresses)
                {
                    writer.WriteElementString("IPAddress", address.ToString());
                }
            }
            if (this.connectIPAddress != null)
            {
                writer.WriteElementString("ConnectIPAddress", this.connectIPAddress.ToString());
            }

            writer.WriteElementString("State", this.state);
            if (this.previousState != null)
            {
                writer.WriteElementString("PreviousState", this.previousState);
            }
            if (this.attemptedState != null)
            {
                writer.WriteElementString("AttemptedState", this.attemptedState);
            }
            writer.WriteStartElement("Initiator");
            writer.WriteValue(this.initiator);
            writer.WriteEndElement();
            if (this.action != null)
            {
                writer.WriteElementString("Action", this.action);
            }
        }
    }

    class PeerNeighborCloseTraceRecord : PeerNeighborTraceRecord
    {
        string closeInitiator;
        string closeReason;

        public PeerNeighborCloseTraceRecord(ulong remoteNodeId, ulong localNodeId,
            PeerNodeAddress listenAddress, IPAddress connectIPAddress, int hashCode, bool initiator,
            string state, string previousState, string attemptedState,
            string closeInitiator, string closeReason)
            : base(remoteNodeId, localNodeId, listenAddress, connectIPAddress, hashCode, initiator,
                    state, previousState, attemptedState, null)
        {
            this.closeInitiator = closeInitiator;
            this.closeReason = closeReason;
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteElementString("CloseReason", this.closeReason);
            writer.WriteElementString("CloseInitiator", this.closeInitiator);
        }
    }

    class PnrpPeerResolverTraceRecord : TraceRecord
    {
        string meshId;
        List<PeerNodeAddress> addresses;

        public PnrpPeerResolverTraceRecord(string meshId, List<PeerNodeAddress> addresses)
        {
            this.meshId = meshId;
            this.addresses = addresses;
        }

        internal override string EventId
        {
            get
            {
                return TraceRecord.EventIdBase + "PnrpPeerResolver" + TraceRecord.NamespaceSuffix;
            }
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteElementString("MeshId", meshId);
            if (this.addresses != null)
            {
                foreach (PeerNodeAddress nodeAddress in addresses)
                {
                    nodeAddress.EndpointAddress.WriteTo(AddressingVersion.WSAddressing10, writer, "Address", "");
                    foreach (IPAddress ipAddress in nodeAddress.IPAddresses)
                    {
                        writer.WriteElementString("IPAddress", ipAddress.ToString());
                    }
                }
            }
        }
    }

    class PeerSecurityTraceRecord : TraceRecord
    {
        protected string meshId;
        protected string remoteAddress;
        protected ClaimSet claimSet;
        Exception exception;

        protected PeerSecurityTraceRecord(string meshId, string remoteAddress, ClaimSet claimSet, Exception exception)
        {
            this.meshId = meshId;
            this.remoteAddress = remoteAddress;
            this.claimSet = claimSet;
            this.exception = exception;
        }

        protected PeerSecurityTraceRecord(string meshId, string remoteAddress) : this(meshId, remoteAddress, null, null) { }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteElementString("MeshId", meshId);
            writer.WriteElementString("RemoteAddress", remoteAddress);
            WriteClaimSet(writer, claimSet);
            if (exception != null)
                writer.WriteElementString("Exception", exception.GetType().ToString() + ":" + exception.Message);
        }

        static internal void WriteClaimSet(XmlWriter writer, ClaimSet claimSet)
        {
            writer.WriteStartElement("NeighborCredentials");
            if (claimSet != null)
            {
                foreach (Claim claim in claimSet)
                {
                    if (claim.ClaimType == ClaimTypes.Name)
                        writer.WriteElementString("Name", claim.Resource.ToString());
                    else if (claim.ClaimType == ClaimTypes.X500DistinguishedName)
                    {
                        writer.WriteElementString("X500DistinguishedName", (claim.Resource as X500DistinguishedName).Name.ToString());
                    }
                    else if (claim.ClaimType == ClaimTypes.Thumbprint)
                    {
                        writer.WriteElementString("Thumbprint", Convert.ToBase64String(claim.Resource as byte[]));
                    }
                }
            }
            writer.WriteEndElement(); //"NeighborCredentials"
        }

    }

    class PeerAuthenticationFailureTraceRecord : PeerSecurityTraceRecord
    {

        public PeerAuthenticationFailureTraceRecord(string meshId, string remoteAddress, ClaimSet claimSet, Exception e)
            : base(meshId, remoteAddress, claimSet, e) { }

        public PeerAuthenticationFailureTraceRecord(string meshId, string remoteAddress) : base(meshId, remoteAddress, null, null) { }

        internal override string EventId
        {
            get
            {
                return TraceRecord.EventIdBase + "PeerAuthentication" + TraceRecord.NamespaceSuffix;
            }
        }
    }

    class PeerSignatureFailureTraceRecord : PeerSecurityTraceRecord
    {
        Uri via;
        public PeerSignatureFailureTraceRecord(string meshId, Uri via, ClaimSet claimSet, Exception exception)
            : base(meshId, null, claimSet, exception)
        {
            this.via = via;
        }

        internal override string EventId
        {
            get
            {
                return TraceRecord.EventIdBase + "PeerSignatureFailure" + TraceRecord.NamespaceSuffix;
            }
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteElementString("Via", via.ToString());
        }
    }

    class PeerFlooderTraceRecord : TraceRecord
    {
        string meshId;
        Uri from;
        Exception exception;

        public PeerFlooderTraceRecord(string meshId, PeerNodeAddress fromAddress, Exception e)
        {
            this.from = fromAddress != null ? fromAddress.EndpointAddress.Uri : new Uri("net.p2p://");
            this.meshId = meshId;
            this.exception = e;
        }

        internal override string EventId
        {
            get
            {
                return TraceRecord.EventIdBase + "PeerFlooderQuotaExceeded" + TraceRecord.NamespaceSuffix;
            }
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteElementString("MeshId", meshId.ToString());
            writer.WriteElementString("MessageSource", from.ToString());
            writer.WriteElementString("Exception", exception.Message);
        }
    }

    class PeerThrottleTraceRecord : TraceRecord
    {
        string meshId;
        string message;

        public PeerThrottleTraceRecord(string meshId, string message)
        {
            this.meshId = meshId;
            this.message = message;
        }

        internal override string EventId
        {
            get
            {
                return TraceRecord.EventIdBase + "PeerFlooderQuotaExceeded" + TraceRecord.NamespaceSuffix;
            }
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteElementString("MeshId", meshId.ToString());
            writer.WriteElementString("Activity", message);
        }
    }

    class PnrpRegisterTraceRecord : TraceRecord
    {
        string meshId;
        IEnumerable<PnrpPeerResolver.PnrpRegistration> siteEntries;
        IEnumerable<PnrpPeerResolver.PnrpRegistration> linkEntries;
        PnrpPeerResolver.PnrpRegistration global;
        public PnrpRegisterTraceRecord(string meshId, PnrpPeerResolver.PnrpRegistration global, IEnumerable<PnrpPeerResolver.PnrpRegistration> siteEntries, IEnumerable<PnrpPeerResolver.PnrpRegistration> linkEntries)
        {
            this.meshId = meshId;
            this.siteEntries = siteEntries;
            this.linkEntries = linkEntries;
            this.global = global;
        }

        internal override string EventId
        {
            get
            {
                return TraceRecord.EventIdBase + "PnrpRegistration" + TraceRecord.NamespaceSuffix;
            }
        }

        void WriteEntry(XmlWriter writer, PnrpPeerResolver.PnrpRegistration entry)
        {
            if (entry == null)
                return;
            writer.WriteStartElement("Registration");
            writer.WriteAttributeString("CloudName", entry.CloudName);
            foreach (IPEndPoint address in entry.Addresses)
            {
                writer.WriteElementString("Address", address.ToString());
            }
            writer.WriteEndElement();
        }

        void WriteEntries(XmlWriter writer, IEnumerable<PnrpPeerResolver.PnrpRegistration> entries)
        {
            if (entries == null)
                return;
            foreach (PnrpPeerResolver.PnrpRegistration reg in entries)
            {
                WriteEntry(writer, reg);
            }
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteElementString("MeshId", meshId.ToString());
            writer.WriteStartElement("Registrations");
            WriteEntry(writer, global);
            WriteEntries(writer, siteEntries);
            WriteEntries(writer, linkEntries);
            writer.WriteEndElement();
        }
    }

    class PeerMaintainerTraceRecord : TraceRecord
    {
        string activity;
        public PeerMaintainerTraceRecord(string activity)
        {
            this.activity = activity;
        }

        internal override string EventId
        {
            get
            {
                return TraceRecord.EventIdBase + "PeerMaintainerActivity" + TraceRecord.NamespaceSuffix;
            }
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteElementString("Activity", activity);
        }
    }

    class PnrpResolveExceptionTraceRecord : TraceRecord
    {

        string peerName;
        string cloudName;
        Exception exception;

        public PnrpResolveExceptionTraceRecord(string peerName, string cloudName, Exception exception)
        {
            this.peerName = peerName;
            this.cloudName = cloudName;
            this.exception = exception;
        }
        internal override string EventId
        {
            get
            {
                return TraceRecord.EventIdBase + "PnrpResolveException" + TraceRecord.NamespaceSuffix;
            }
        }
        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteElementString("PeerName", peerName);
            writer.WriteElementString("CloudName", cloudName);
            writer.WriteElementString("Exception", exception.ToString());
        }

    }


}
