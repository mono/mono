//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;
    using System.ServiceModel;
    using System.Xml;

    abstract class AddressingHeader : DictionaryHeader, IMessageHeaderWithSharedNamespace
    {
        AddressingVersion version;

        protected AddressingHeader(AddressingVersion version)
        {
            this.version = version;
        }

        internal AddressingVersion Version
        {
            get { return this.version; }
        }

        XmlDictionaryString IMessageHeaderWithSharedNamespace.SharedPrefix
        {
            get { return XD.AddressingDictionary.Prefix; }
        }

        XmlDictionaryString IMessageHeaderWithSharedNamespace.SharedNamespace
        {
            get { return this.version.DictionaryNamespace; }
        }

        public override XmlDictionaryString DictionaryNamespace
        {
            get { return this.version.DictionaryNamespace; }
        }
    }

    class ActionHeader : AddressingHeader
    {
        string action;
        const bool mustUnderstandValue = true;

        ActionHeader(string action, AddressingVersion version)
            : base(version)
        {
            this.action = action;
        }

        public string Action
        {
            get { return action; }
        }

        public override bool MustUnderstand
        {
            get { return mustUnderstandValue; }
        }

        public override XmlDictionaryString DictionaryName
        {
            get { return XD.AddressingDictionary.Action; }
        }

        public static ActionHeader Create(string action, AddressingVersion addressingVersion)
        {
            if (action == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("action"));
            if (addressingVersion == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("addressingVersion");
            return new ActionHeader(action, addressingVersion);
        }

        public static ActionHeader Create(XmlDictionaryString dictionaryAction, AddressingVersion addressingVersion)
        {
            if (dictionaryAction == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("action"));
            if (addressingVersion == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("addressingVersion");
            return new DictionaryActionHeader(dictionaryAction, addressingVersion);
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteString(action);
        }

        public static string ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion addressingVersion)
        {
            Fx.Assert(reader.IsStartElement(XD.AddressingDictionary.Action, addressingVersion.DictionaryNamespace), "");
            string act = reader.ReadElementContentAsString();

            if (act.Length > 0 && (act[0] <= 32 || act[act.Length - 1] <= 32))
                act = XmlUtil.Trim(act);

            return act;
        }

        public static ActionHeader ReadHeader(XmlDictionaryReader reader, AddressingVersion version,
            string actor, bool mustUnderstand, bool relay)
        {
            string action = ReadHeaderValue(reader, version);

            if (actor.Length == 0 && mustUnderstand == mustUnderstandValue && !relay)
            {
                return new ActionHeader(action, version);
            }
            else
            {
                return new FullActionHeader(action, actor, mustUnderstand, relay, version);
            }
        }

        class DictionaryActionHeader : ActionHeader
        {
            XmlDictionaryString dictionaryAction;

            public DictionaryActionHeader(XmlDictionaryString dictionaryAction, AddressingVersion version)
                : base(dictionaryAction.Value, version)
            {
                this.dictionaryAction = dictionaryAction;
            }

            protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                writer.WriteString(dictionaryAction);
            }
        }

        class FullActionHeader : ActionHeader
        {
            string actor;
            bool mustUnderstand;
            bool relay;

            public FullActionHeader(string action, string actor, bool mustUnderstand, bool relay, AddressingVersion version)
                : base(action, version)
            {
                this.actor = actor;
                this.mustUnderstand = mustUnderstand;
                this.relay = relay;
            }

            public override string Actor
            {
                get { return actor; }
            }

            public override bool MustUnderstand
            {
                get { return mustUnderstand; }
            }

            public override bool Relay
            {
                get { return relay; }
            }
        }
    }

    class FromHeader : AddressingHeader
    {
        EndpointAddress from;
        const bool mustUnderstandValue = false;

        FromHeader(EndpointAddress from, AddressingVersion version)
            : base(version)
        {
            this.from = from;
        }

        public EndpointAddress From
        {
            get { return from; }
        }

        public override XmlDictionaryString DictionaryName
        {
            get { return XD.AddressingDictionary.From; }
        }

        public override bool MustUnderstand
        {
            get { return mustUnderstandValue; }
        }

        public static FromHeader Create(EndpointAddress from, AddressingVersion addressingVersion)
        {
            if (from == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("from"));
            if (addressingVersion == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("addressingVersion");
            return new FromHeader(from, addressingVersion);
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            from.WriteContentsTo(this.Version, writer);
        }

        public static FromHeader ReadHeader(XmlDictionaryReader reader, AddressingVersion version,
            string actor, bool mustUnderstand, bool relay)
        {
            EndpointAddress from = ReadHeaderValue(reader, version);

            if (actor.Length == 0 && mustUnderstand == mustUnderstandValue && !relay)
            {
                return new FromHeader(from, version);
            }
            else
            {
                return new FullFromHeader(from, actor, mustUnderstand, relay, version);
            }
        }

        public static EndpointAddress ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion addressingVersion)
        {
            Fx.Assert(reader.IsStartElement(XD.AddressingDictionary.From, addressingVersion.DictionaryNamespace), "");
            return EndpointAddress.ReadFrom(addressingVersion, reader);
        }

        class FullFromHeader : FromHeader
        {
            string actor;
            bool mustUnderstand;
            bool relay;

            public FullFromHeader(EndpointAddress from, string actor, bool mustUnderstand, bool relay, AddressingVersion version)
                : base(from, version)
            {
                this.actor = actor;
                this.mustUnderstand = mustUnderstand;
                this.relay = relay;
            }

            public override string Actor
            {
                get { return actor; }
            }

            public override bool MustUnderstand
            {
                get { return mustUnderstand; }
            }

            public override bool Relay
            {
                get { return relay; }
            }
        }
    }

    class FaultToHeader : AddressingHeader
    {
        EndpointAddress faultTo;
        const bool mustUnderstandValue = false;

        FaultToHeader(EndpointAddress faultTo, AddressingVersion version)
            : base(version)
        {
            this.faultTo = faultTo;
        }

        public EndpointAddress FaultTo
        {
            get { return faultTo; }
        }

        public override XmlDictionaryString DictionaryName
        {
            get { return XD.AddressingDictionary.FaultTo; }
        }

        public override bool MustUnderstand
        {
            get { return mustUnderstandValue; }
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            faultTo.WriteContentsTo(this.Version, writer);
        }

        public static FaultToHeader Create(EndpointAddress faultTo, AddressingVersion addressingVersion)
        {
            if (faultTo == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("faultTo"));
            if (addressingVersion == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("addressingVersion");
            return new FaultToHeader(faultTo, addressingVersion);
        }

        public static FaultToHeader ReadHeader(XmlDictionaryReader reader, AddressingVersion version,
            string actor, bool mustUnderstand, bool relay)
        {
            EndpointAddress faultTo = ReadHeaderValue(reader, version);

            if (actor.Length == 0 && mustUnderstand == mustUnderstandValue && !relay)
            {
                return new FaultToHeader(faultTo, version);
            }
            else
            {
                return new FullFaultToHeader(faultTo, actor, mustUnderstand, relay, version);
            }
        }

        public static EndpointAddress ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion version)
        {
            Fx.Assert(reader.IsStartElement(XD.AddressingDictionary.FaultTo, version.DictionaryNamespace), "");
            return EndpointAddress.ReadFrom(version, reader);
        }

        class FullFaultToHeader : FaultToHeader
        {
            string actor;
            bool mustUnderstand;
            bool relay;

            public FullFaultToHeader(EndpointAddress faultTo, string actor, bool mustUnderstand, bool relay, AddressingVersion version)
                : base(faultTo, version)
            {
                this.actor = actor;
                this.mustUnderstand = mustUnderstand;
                this.relay = relay;
            }

            public override string Actor
            {
                get { return actor; }
            }

            public override bool MustUnderstand
            {
                get { return mustUnderstand; }
            }

            public override bool Relay
            {
                get { return relay; }
            }
        }
    }

    class ToHeader : AddressingHeader
    {
        Uri to;
        const bool mustUnderstandValue = true;

        static ToHeader anonymousToHeader10;
        static ToHeader anonymousToHeader200408;

        protected ToHeader(Uri to, AddressingVersion version)
            : base(version)
        {
            this.to = to;
        }

        static ToHeader AnonymousTo10
        {
            get
            {
                if (anonymousToHeader10 == null)
                    anonymousToHeader10 = new AnonymousToHeader(AddressingVersion.WSAddressing10);
                return anonymousToHeader10;
            }
        }

        static ToHeader AnonymousTo200408
        {
            get
            {
                if (anonymousToHeader200408 == null)
                    anonymousToHeader200408 = new AnonymousToHeader(AddressingVersion.WSAddressingAugust2004);
                return anonymousToHeader200408;
            }
        }

        public override XmlDictionaryString DictionaryName
        {
            get { return XD.AddressingDictionary.To; }
        }

        public override bool MustUnderstand
        {
            get { return mustUnderstandValue; }
        }

        public Uri To
        {
            get { return to; }
        }

        public static ToHeader Create(Uri toUri, XmlDictionaryString dictionaryTo, AddressingVersion addressingVersion)
        {
            if (addressingVersion == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("addressingVersion");

            if (((object)toUri == (object)addressingVersion.AnonymousUri))
            {
                if (addressingVersion == AddressingVersion.WSAddressing10)
                    return AnonymousTo10;
                else
                    return AnonymousTo200408;
            }
            else
            {
                return new DictionaryToHeader(toUri, dictionaryTo, addressingVersion);
            }
        }

        public static ToHeader Create(Uri to, AddressingVersion addressingVersion)
        {
            if ((object)to == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("to"));
            }
            else if ((object)to == (object)addressingVersion.AnonymousUri)
            {
                if (addressingVersion == AddressingVersion.WSAddressing10)
                    return AnonymousTo10;
                else
                    return AnonymousTo200408;
            }
            else
            {
                return new ToHeader(to, addressingVersion);
            }
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteString(to.AbsoluteUri);
        }

        public static Uri ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion version)
        {
            return ReadHeaderValue(reader, version, null);
        }

        public static Uri ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion version, UriCache uriCache)
        {
            Fx.Assert(reader.IsStartElement(XD.AddressingDictionary.To, version.DictionaryNamespace), "");

            string toString = reader.ReadElementContentAsString();

            if ((object)toString == (object)version.Anonymous)
            {
                return version.AnonymousUri;
            }

            if (uriCache == null)
            {
                return new Uri(toString);
            }

            return uriCache.CreateUri(toString);
        }

        public static ToHeader ReadHeader(XmlDictionaryReader reader, AddressingVersion version, UriCache uriCache,
            string actor, bool mustUnderstand, bool relay)
        {
            Uri to = ReadHeaderValue(reader, version, uriCache);

            if (actor.Length == 0 && mustUnderstand == mustUnderstandValue && !relay)
            {
                if ((object)to == (object)version.Anonymous)
                {
                    if (version == AddressingVersion.WSAddressing10)
                        return AnonymousTo10;
                    else
                        return AnonymousTo200408;
                }
                else
                {
                    return new ToHeader(to, version);
                }
            }
            else
            {
                return new FullToHeader(to, actor, mustUnderstand, relay, version);
            }
        }

        class AnonymousToHeader : ToHeader
        {
            public AnonymousToHeader(AddressingVersion version)
                : base(version.AnonymousUri, version)
            {
            }

            protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                writer.WriteString(this.Version.DictionaryAnonymous);
            }
        }

        class DictionaryToHeader : ToHeader
        {
            XmlDictionaryString dictionaryTo;

            public DictionaryToHeader(Uri to, XmlDictionaryString dictionaryTo, AddressingVersion version)
                : base(to, version)
            {
                this.dictionaryTo = dictionaryTo;
            }

            protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                writer.WriteString(dictionaryTo);
            }
        }

        class FullToHeader : ToHeader
        {
            string actor;
            bool mustUnderstand;
            bool relay;

            public FullToHeader(Uri to, string actor, bool mustUnderstand, bool relay, AddressingVersion version)
                : base(to, version)
            {
                this.actor = actor;
                this.mustUnderstand = mustUnderstand;
                this.relay = relay;
            }

            public override string Actor
            {
                get { return actor; }
            }

            public override bool MustUnderstand
            {
                get { return mustUnderstand; }
            }

            public override bool Relay
            {
                get { return relay; }
            }
        }
    }

    class ReplyToHeader : AddressingHeader
    {
        EndpointAddress replyTo;
        const bool mustUnderstandValue = false;
        static ReplyToHeader anonymousReplyToHeader10;
        static ReplyToHeader anonymousReplyToHeader200408;

        ReplyToHeader(EndpointAddress replyTo, AddressingVersion version)
            : base(version)
        {
            this.replyTo = replyTo;
        }

        public EndpointAddress ReplyTo
        {
            get { return replyTo; }
        }

        public override XmlDictionaryString DictionaryName
        {
            get { return XD.AddressingDictionary.ReplyTo; }
        }

        public override bool MustUnderstand
        {
            get { return mustUnderstandValue; }
        }

        public static ReplyToHeader AnonymousReplyTo10
        {
            get
            {
                if (anonymousReplyToHeader10 == null)
                    anonymousReplyToHeader10 = new ReplyToHeader(EndpointAddress.AnonymousAddress, AddressingVersion.WSAddressing10);
                return anonymousReplyToHeader10;
            }
        }

        public static ReplyToHeader AnonymousReplyTo200408
        {
            get
            {
                if (anonymousReplyToHeader200408 == null)
                    anonymousReplyToHeader200408 = new ReplyToHeader(EndpointAddress.AnonymousAddress, AddressingVersion.WSAddressingAugust2004);
                return anonymousReplyToHeader200408;
            }
        }

        public static ReplyToHeader Create(EndpointAddress replyTo, AddressingVersion addressingVersion)
        {
            if (replyTo == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("replyTo"));
            if (addressingVersion == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("addressingVersion"));
            return new ReplyToHeader(replyTo, addressingVersion);
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            replyTo.WriteContentsTo(this.Version, writer);
        }

        public static ReplyToHeader ReadHeader(XmlDictionaryReader reader, AddressingVersion version,
            string actor, bool mustUnderstand, bool relay)
        {
            EndpointAddress replyTo = ReadHeaderValue(reader, version);

            if (actor.Length == 0 && mustUnderstand == mustUnderstandValue && !relay)
            {
                if ((object)replyTo == (object)EndpointAddress.AnonymousAddress)
                {
                    if (version == AddressingVersion.WSAddressing10)
                        return AnonymousReplyTo10;
                    else
                        return AnonymousReplyTo200408;
                }
                return new ReplyToHeader(replyTo, version);
            }
            else
            {
                return new FullReplyToHeader(replyTo, actor, mustUnderstand, relay, version);
            }
        }

        public static EndpointAddress ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion version)
        {
            Fx.Assert(reader.IsStartElement(XD.AddressingDictionary.ReplyTo, version.DictionaryNamespace), "");
            return EndpointAddress.ReadFrom(version, reader);
        }

        class FullReplyToHeader : ReplyToHeader
        {
            string actor;
            bool mustUnderstand;
            bool relay;

            public FullReplyToHeader(EndpointAddress replyTo, string actor, bool mustUnderstand, bool relay, AddressingVersion version)
                : base(replyTo, version)
            {
                this.actor = actor;
                this.mustUnderstand = mustUnderstand;
                this.relay = relay;
            }

            public override string Actor
            {
                get { return actor; }
            }

            public override bool MustUnderstand
            {
                get { return mustUnderstand; }
            }

            public override bool Relay
            {
                get { return relay; }
            }
        }
    }

    class MessageIDHeader : AddressingHeader
    {
        UniqueId messageId;
        const bool mustUnderstandValue = false;

        MessageIDHeader(UniqueId messageId, AddressingVersion version)
            : base(version)
        {
            this.messageId = messageId;
        }

        public override XmlDictionaryString DictionaryName
        {
            get { return XD.AddressingDictionary.MessageId; }
        }

        public UniqueId MessageId
        {
            get { return messageId; }
        }

        public override bool MustUnderstand
        {
            get { return mustUnderstandValue; }
        }

        public static MessageIDHeader Create(UniqueId messageId, AddressingVersion addressingVersion)
        {
            if (object.ReferenceEquals(messageId, null))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("messageId"));
            if (addressingVersion == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("addressingVersion"));
            return new MessageIDHeader(messageId, addressingVersion);
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteValue(messageId);
        }

        public static UniqueId ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion version)
        {
            Fx.Assert(reader.IsStartElement(XD.AddressingDictionary.MessageId, version.DictionaryNamespace), "");
            return reader.ReadElementContentAsUniqueId();
        }

        public static MessageIDHeader ReadHeader(XmlDictionaryReader reader, AddressingVersion version,
            string actor, bool mustUnderstand, bool relay)
        {
            UniqueId messageId = ReadHeaderValue(reader, version);

            if (actor.Length == 0 && mustUnderstand == mustUnderstandValue && !relay)
            {
                return new MessageIDHeader(messageId, version);
            }
            else
            {
                return new FullMessageIDHeader(messageId, actor, mustUnderstand, relay, version);
            }
        }

        class FullMessageIDHeader : MessageIDHeader
        {
            string actor;
            bool mustUnderstand;
            bool relay;

            public FullMessageIDHeader(UniqueId messageId, string actor, bool mustUnderstand, bool relay, AddressingVersion version)
                : base(messageId, version)
            {
                this.actor = actor;
                this.mustUnderstand = mustUnderstand;
                this.relay = relay;
            }

            public override string Actor
            {
                get { return actor; }
            }

            public override bool MustUnderstand
            {
                get { return mustUnderstand; }
            }

            public override bool Relay
            {
                get { return relay; }
            }
        }
    }

    class RelatesToHeader : AddressingHeader
    {
        UniqueId messageId;
        const bool mustUnderstandValue = false;
        internal static readonly Uri ReplyRelationshipType = new Uri(Addressing10Strings.ReplyRelationship);

        RelatesToHeader(UniqueId messageId, AddressingVersion version)
            : base(version)
        {
            this.messageId = messageId;
        }

        public override XmlDictionaryString DictionaryName
        {
            get { return XD.AddressingDictionary.RelatesTo; }
        }

        public UniqueId UniqueId
        {
            get { return messageId; }
        }

        public override bool MustUnderstand
        {
            get { return mustUnderstandValue; }
        }

        public virtual Uri RelationshipType
        {
            get { return ReplyRelationshipType; }
        }

        public static RelatesToHeader Create(UniqueId messageId, AddressingVersion addressingVersion)
        {
            if (object.ReferenceEquals(messageId, null))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("messageId"));
            if (addressingVersion == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("addressingVersion"));
            return new RelatesToHeader(messageId, addressingVersion);
        }

        public static RelatesToHeader Create(UniqueId messageId, AddressingVersion addressingVersion, Uri relationshipType)
        {
            if (object.ReferenceEquals(messageId, null))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("messageId"));
            if (addressingVersion == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("addressingVersion"));
            if (relationshipType == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("relationshipType"));
            if (relationshipType == ReplyRelationshipType)
            {
                return new RelatesToHeader(messageId, addressingVersion);
            }
            else
            {
                return new FullRelatesToHeader(messageId, "", false, false, addressingVersion);
            }
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteValue(messageId);
        }

        public static void ReadHeaderValue(XmlDictionaryReader reader, AddressingVersion version, out Uri relationshipType, out UniqueId messageId)
        {
            AddressingDictionary addressingDictionary = XD.AddressingDictionary;

            // The RelationshipType attribute has no namespace.
            relationshipType = ReplyRelationshipType;
            /*
            string relation = reader.GetAttribute(addressingDictionary.RelationshipType, addressingDictionary.Empty);
            if (relation == null)
            {
                relationshipType = ReplyRelationshipType;
            }
            else
            {
                relationshipType = new Uri(relation);
            }
            */
            Fx.Assert(reader.IsStartElement(addressingDictionary.RelatesTo, version.DictionaryNamespace), "");
            messageId = reader.ReadElementContentAsUniqueId();
        }

        public static RelatesToHeader ReadHeader(XmlDictionaryReader reader, AddressingVersion version,
            string actor, bool mustUnderstand, bool relay)
        {
            UniqueId messageId;
            Uri relationship;
            ReadHeaderValue(reader, version, out relationship, out messageId);

            if (actor.Length == 0 && mustUnderstand == mustUnderstandValue && !relay && (object)relationship == (object)ReplyRelationshipType)
            {
                return new RelatesToHeader(messageId, version);
            }
            else
            {
                return new FullRelatesToHeader(messageId, actor, mustUnderstand, relay, version);
            }
        }

        class FullRelatesToHeader : RelatesToHeader
        {
            string actor;
            bool mustUnderstand;
            bool relay;
            //Uri relationship;

            public FullRelatesToHeader(UniqueId messageId, string actor, bool mustUnderstand, bool relay, AddressingVersion version)
                : base(messageId, version)
            {
                //this.relationship = relationship;
                this.actor = actor;
                this.mustUnderstand = mustUnderstand;
                this.relay = relay;
            }

            public override string Actor
            {
                get { return actor; }
            }

            public override bool MustUnderstand
            {
                get { return mustUnderstand; }
            }

            /*
            public override Uri RelationshipType
            {
                get { return relationship; }
            }
            */

            public override bool Relay
            {
                get { return relay; }
            }

            protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
            {
                /*
                if ((object)relationship != (object)ReplyRelationshipType)
                {
                    // The RelationshipType attribute has no namespace.
                    writer.WriteStartAttribute(AddressingStrings.RelationshipType, AddressingStrings.Empty);
                    writer.WriteString(relationship.AbsoluteUri);
                    writer.WriteEndAttribute();
                }
                */
                writer.WriteValue(messageId);
            }
        }
    }
}
