//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.IO;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Selectors;
    using System.Security.Cryptography;
    using System.Xml;

    using DictionaryManager = System.IdentityModel.DictionaryManager;
    using ISecurityElement = System.IdentityModel.ISecurityElement;

    sealed class EncryptedHeaderXml
    {
        internal static readonly XmlDictionaryString ElementName = XD.SecurityXXX2005Dictionary.EncryptedHeader;
        internal static readonly XmlDictionaryString NamespaceUri = XD.SecurityXXX2005Dictionary.Namespace;
        const string Prefix = SecurityXXX2005Strings.Prefix;

        string id;
        bool mustUnderstand;
        bool relay;
        string actor;
        MessageVersion version;
        EncryptedData encryptedData;

        public EncryptedHeaderXml(MessageVersion version, bool shouldReadXmlReferenceKeyInfoClause)
        {
            this.version = version;
            encryptedData = new EncryptedData();
            
            // This is for the case when the service send an EncryptedHeader to the client where the KeyInfo clause contains referenceXml clause.
            encryptedData.ShouldReadXmlReferenceKeyInfoClause = shouldReadXmlReferenceKeyInfoClause;
        }

        public string Actor
        {
            get
            {
                return this.actor;
            }
            set
            {
                this.actor = value;
            }
        }

        public string EncryptionMethod
        {
            get
            {
                return encryptedData.EncryptionMethod;
            }
            set
            {
                encryptedData.EncryptionMethod = value;
            }
        }

        public XmlDictionaryString EncryptionMethodDictionaryString
        {
            get
            {
                return encryptedData.EncryptionMethodDictionaryString;
            }
            set
            {
                encryptedData.EncryptionMethodDictionaryString = value;
            }
        }

        public bool HasId
        {
            get
            {
                return true;
            }
        }

        public string Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public SecurityKeyIdentifier KeyIdentifier
        {
            get
            {
                return encryptedData.KeyIdentifier;
            }
            set
            {
                encryptedData.KeyIdentifier = value;
            }
        }

        public bool MustUnderstand
        {
            get
            {
                return this.mustUnderstand;
            }
            set
            {
                this.mustUnderstand = value;
            }
        }

        public bool Relay
        {
            get
            {
                return this.relay;
            }
            set
            {
                this.relay = value;
            }
        }

        public SecurityTokenSerializer SecurityTokenSerializer
        {
            get
            {
                return encryptedData.SecurityTokenSerializer;
            }
            set
            {
                encryptedData.SecurityTokenSerializer = value;
            }
        }

        public byte[] GetDecryptedBuffer()
        {
            return encryptedData.GetDecryptedBuffer();
        }

        public void ReadFrom(XmlDictionaryReader reader, long maxBufferSize)
        {
            reader.MoveToStartElement(ElementName, NamespaceUri);
            bool isReferenceParameter;
            MessageHeader.GetHeaderAttributes(reader, version, out this.actor, out this.mustUnderstand, out this.relay, out isReferenceParameter);
            this.id = reader.GetAttribute(XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace);

            reader.ReadStartElement();
            encryptedData.ReadFrom(reader, maxBufferSize);
            reader.ReadEndElement();
        }

        public void SetUpDecryption(SymmetricAlgorithm algorithm)
        {
            encryptedData.SetUpDecryption(algorithm);
        }

        public void SetUpEncryption(SymmetricAlgorithm algorithm, MemoryStream source)
        {
            encryptedData.SetUpEncryption(algorithm, new ArraySegment<byte>(source.GetBuffer(), 0, (int) source.Length));
        }

        public void WriteHeaderElement(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement(Prefix, ElementName, NamespaceUri);
        }

        public void WriteHeaderId(XmlDictionaryWriter writer)
        {
            writer.WriteAttributeString(XD.UtilityDictionary.Prefix.Value, XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace, id);
        }

        public void WriteHeaderContents(XmlDictionaryWriter writer)
        {
            this.encryptedData.WriteTo(writer, ServiceModelDictionaryManager.Instance);
        }
    }
}
