//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Xml;
    using System.ServiceModel.Channels;
    using System.ServiceModel;

    sealed class DecryptedHeader : ReadableMessageHeader
    {
        XmlDictionaryReader cachedReader;
        readonly byte[] decryptedBuffer;
        readonly string id;
        readonly string name;
        readonly string namespaceUri;
        readonly string actor;
        readonly bool mustUnderstand;
        readonly bool relay;
        readonly bool isRefParam;
        readonly MessageVersion version;
        readonly XmlAttributeHolder[] envelopeAttributes;
        readonly XmlAttributeHolder[] headerAttributes;
        readonly XmlDictionaryReaderQuotas quotas;

        public DecryptedHeader(byte[] decryptedBuffer,
            XmlAttributeHolder[] envelopeAttributes, XmlAttributeHolder[] headerAttributes,
            MessageVersion version, SignatureTargetIdManager idManager, XmlDictionaryReaderQuotas quotas)
        {
            if (quotas == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("quotas");

            this.decryptedBuffer = decryptedBuffer;
            this.version = version;
            this.envelopeAttributes = envelopeAttributes;
            this.headerAttributes = headerAttributes;
            this.quotas = quotas;

            XmlDictionaryReader reader = CreateReader();
            reader.MoveToStartElement();

            this.name = reader.LocalName;
            this.namespaceUri = reader.NamespaceURI;
            MessageHeader.GetHeaderAttributes(reader, version, out this.actor, out this.mustUnderstand, out this.relay, out this.isRefParam);
            this.id = idManager.ExtractId(reader);

            this.cachedReader = reader;
        }

        public override string Actor
        {
            get
            {
                return this.actor;
            }
        }

        public string Id
        {
            get
            {
                return this.id;
            }
        }

        public override bool IsReferenceParameter
        {
            get
            {
                return this.isRefParam;
            }
        }
        
        public override bool MustUnderstand
        {
            get
            {
                return this.mustUnderstand;
            }
        }

        public override string Name
        {
            get
            {
                return this.name;
            }
        }

        public override string Namespace
        {
            get
            {
                return this.namespaceUri;
            }
        }

        public override bool Relay
        {
            get
            {
                return this.relay;
            }
        }

        XmlDictionaryReader CreateReader()
        {
            return ContextImportHelper.CreateSplicedReader(
                this.decryptedBuffer,
                this.envelopeAttributes,
                this.headerAttributes, null, this.quotas);
        }

        public override XmlDictionaryReader GetHeaderReader()
        {
            if (this.cachedReader != null)
            {
                XmlDictionaryReader cachedReader = this.cachedReader;
                this.cachedReader = null;
                return cachedReader;
            }
            XmlDictionaryReader reader = CreateReader();
            reader.MoveToContent();
            return reader;
        }

        public override bool IsMessageVersionSupported(MessageVersion messageVersion)
        {
            return this.version.Equals( messageVersion );
        }
    }
}
