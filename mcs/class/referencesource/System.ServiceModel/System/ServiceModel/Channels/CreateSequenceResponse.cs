//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.Xml;

    sealed class CreateSequenceResponse : BodyWriter
    {
        EndpointAddress acceptAcksTo;
        AddressingVersion addressingVersion;
        Nullable<TimeSpan> expires;
        UniqueId identifier;
        bool ordered;
        ReliableMessagingVersion reliableMessagingVersion;

        CreateSequenceResponse()
            : base(true)
        {
        }

        public CreateSequenceResponse(AddressingVersion addressingVersion,
            ReliableMessagingVersion reliableMessagingVersion)
            : base(true)
        {
            this.addressingVersion = addressingVersion;
            this.reliableMessagingVersion = reliableMessagingVersion;
        }

        public EndpointAddress AcceptAcksTo
        {
            get
            {
                return this.acceptAcksTo;
            }
            set
            {
                this.acceptAcksTo = value;
            }
        }

        public Nullable<TimeSpan> Expires
        {
            get
            {
                return this.expires;
            }
            set
            {
                this.expires = value;
            }
        }

        public UniqueId Identifier
        {
            get
            {
                return this.identifier;
            }
            set
            {
                this.identifier = value;
            }
        }

        public bool Ordered
        {
            get
            {
                return this.ordered;
            }
            set
            {
                this.ordered = value;
            }
        }

        public static CreateSequenceResponseInfo Create(AddressingVersion addressingVersion,
            ReliableMessagingVersion reliableMessagingVersion, XmlDictionaryReader reader)
        {
            if (reader == null)
            {
                Fx.Assert("Argument reader cannot be null.");
            }

            CreateSequenceResponseInfo createSequenceResponse = new CreateSequenceResponseInfo();
            WsrmFeb2005Dictionary wsrmFeb2005Dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString wsrmNs = WsrmIndex.GetNamespace(reliableMessagingVersion);

            reader.ReadStartElement(wsrmFeb2005Dictionary.CreateSequenceResponse, wsrmNs);

            reader.ReadStartElement(wsrmFeb2005Dictionary.Identifier, wsrmNs);
            createSequenceResponse.Identifier = reader.ReadContentAsUniqueId();
            reader.ReadEndElement();

            if (reader.IsStartElement(wsrmFeb2005Dictionary.Expires, wsrmNs))
            {
                reader.ReadElementContentAsTimeSpan();
            }

            if (reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                if (reader.IsStartElement(DXD.Wsrm11Dictionary.IncompleteSequenceBehavior, wsrmNs))
                {
                    string incompleteSequenceBehavior = reader.ReadElementContentAsString();

                    if ((incompleteSequenceBehavior != Wsrm11Strings.DiscardEntireSequence)
                        && (incompleteSequenceBehavior != Wsrm11Strings.DiscardFollowingFirstGap)
                        && (incompleteSequenceBehavior != Wsrm11Strings.NoDiscard))
                    {
                        string reason = SR.GetString(SR.CSResponseWithInvalidIncompleteSequenceBehavior);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(reason));
                    }

                    // Otherwise ignore the value.
                }
            }

            if (reader.IsStartElement(wsrmFeb2005Dictionary.Accept, wsrmNs))
            {
                reader.ReadStartElement();
                createSequenceResponse.AcceptAcksTo = EndpointAddress.ReadFrom(addressingVersion, reader,
                    wsrmFeb2005Dictionary.AcksTo, wsrmNs);
                while (reader.IsStartElement())
                {
                    reader.Skip();
                }
                reader.ReadEndElement();
            }

            while (reader.IsStartElement())
            {
                reader.Skip();
            }

            reader.ReadEndElement();

            return createSequenceResponse;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            WsrmFeb2005Dictionary wsrmFeb2005Dictionary = XD.WsrmFeb2005Dictionary;
            XmlDictionaryString wsrmNs = WsrmIndex.GetNamespace(this.reliableMessagingVersion);
            writer.WriteStartElement(wsrmFeb2005Dictionary.CreateSequenceResponse, wsrmNs);

            writer.WriteStartElement(wsrmFeb2005Dictionary.Identifier, wsrmNs);
            writer.WriteValue(this.identifier);
            writer.WriteEndElement();

            if (this.expires.HasValue)
            {
                writer.WriteStartElement(wsrmFeb2005Dictionary.Expires, wsrmNs);
                writer.WriteValue(this.expires.Value);
                writer.WriteEndElement();
            }

            if (this.reliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11)
            {
                Wsrm11Dictionary wsrm11Dictionary = DXD.Wsrm11Dictionary;
                writer.WriteStartElement(wsrm11Dictionary.IncompleteSequenceBehavior, wsrmNs);
                writer.WriteValue(
                    this.ordered ? wsrm11Dictionary.DiscardFollowingFirstGap : wsrm11Dictionary.NoDiscard);
                writer.WriteEndElement();
            }

            if (this.acceptAcksTo != null)
            {
                writer.WriteStartElement(wsrmFeb2005Dictionary.Accept, wsrmNs);
                this.acceptAcksTo.WriteTo(this.addressingVersion, writer, wsrmFeb2005Dictionary.AcksTo, wsrmNs);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }
    }
}
