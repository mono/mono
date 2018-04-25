//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Xml;
    using System.ServiceModel.Channels;
    using System.ServiceModel;

    abstract class DelegatingHeader : MessageHeader
    {
        MessageHeader innerHeader;

        protected DelegatingHeader(MessageHeader innerHeader)
        {
            if (innerHeader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("innerHeader");
            }
            this.innerHeader = innerHeader;
        }

        public override bool MustUnderstand
        {
            get
            {
                return this.innerHeader.MustUnderstand;
            }
        }

        public override string Name
        {
            get
            {
                return this.innerHeader.Name;
            }
        }

        public override string Namespace
        {
            get
            {
                return this.innerHeader.Namespace;
            }
        }

        public override bool Relay
        {
            get
            {
                return this.innerHeader.Relay;
            }
        }

        public override string Actor
        {
            get
            {
                return this.innerHeader.Actor;
            }
        }

        protected MessageHeader InnerHeader
        {
            get
            {
                return this.innerHeader;
            }
        }

        protected override void OnWriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            this.innerHeader.WriteStartHeader(writer, messageVersion);
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            this.innerHeader.WriteHeaderContents(writer, messageVersion);
        }
    }
}
