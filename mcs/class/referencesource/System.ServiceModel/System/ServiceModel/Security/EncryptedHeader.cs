//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Diagnostics;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.Globalization;    
    using System.Xml;
    using System.IO;

    using ISecurityElement = System.IdentityModel.ISecurityElement;

    sealed class EncryptedHeader : DelegatingHeader
    {
        EncryptedHeaderXml headerXml;
        string name;
        string namespaceUri;
        MessageVersion version;

        public EncryptedHeader(MessageHeader plainTextHeader, EncryptedHeaderXml headerXml, string name, string namespaceUri, MessageVersion version)
            : base(plainTextHeader)
        {
            if (!headerXml.HasId || headerXml.Id == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.EncryptedHeaderXmlMustHaveId)));
            }
            this.headerXml = headerXml;
            this.name = name;
            this.namespaceUri = namespaceUri;
            this.version = version;
        }

        public string Id
        {
            get { return this.headerXml.Id; }
        }

        public override string Name
        {
            get { return this.name; }
        }

        public override string Namespace
        {
            get { return this.namespaceUri; }
        }

        public override string Actor
        {
            get
            {
                return this.headerXml.Actor;
            }
        }

        public override bool MustUnderstand
        {
            get
            {
                return this.headerXml.MustUnderstand;
            }
        }

        public override bool Relay
        {
            get
            {
                return this.headerXml.Relay;
            }
        }

        internal MessageHeader OriginalHeader
        {
            get { return this.InnerHeader; }
        }

        public override bool IsMessageVersionSupported(MessageVersion messageVersion)
        {
            return this.version.Equals( messageVersion );
        }

        protected override void OnWriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            if (!IsMessageVersionSupported(messageVersion))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.MessageHeaderVersionNotSupported, String.Format(CultureInfo.InvariantCulture, "{0}:{1}", this.Namespace, this.Name), version.ToString()), "version"));
            }

            this.headerXml.WriteHeaderElement(writer);
            WriteHeaderAttributes(writer, messageVersion);
            this.headerXml.WriteHeaderId(writer);
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            this.headerXml.WriteHeaderContents(writer);
        }
    }
}
