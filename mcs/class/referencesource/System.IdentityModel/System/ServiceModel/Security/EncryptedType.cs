//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.IdentityModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Xml;
    using DictionaryManager = System.IdentityModel.DictionaryManager;
    using ISecurityElement = System.IdentityModel.ISecurityElement;

    [TypeForwardedFrom("System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    abstract class EncryptedType : ISecurityElement
    {
        internal static readonly XmlDictionaryString NamespaceUri = XD.XmlEncryptionDictionary.Namespace;
        internal static readonly XmlDictionaryString EncodingAttribute = XD.XmlEncryptionDictionary.Encoding;
        internal static readonly XmlDictionaryString MimeTypeAttribute = XD.XmlEncryptionDictionary.MimeType;
        internal static readonly XmlDictionaryString TypeAttribute = XD.XmlEncryptionDictionary.Type;
        internal static readonly XmlDictionaryString CipherDataElementName = XD.XmlEncryptionDictionary.CipherData;
        internal static readonly XmlDictionaryString CipherValueElementName = XD.XmlEncryptionDictionary.CipherValue;

        string encoding;
        EncryptionMethodElement encryptionMethod;
        string id;
        string wsuId;
        SecurityKeyIdentifier keyIdentifier;
        string mimeType;
        EncryptionState state;
        string type;
        SecurityTokenSerializer tokenSerializer;
        bool shouldReadXmlReferenceKeyInfoClause;

        protected EncryptedType()
        {
            this.encryptionMethod.Init();
            this.state = EncryptionState.New;
            this.tokenSerializer = new KeyInfoSerializer(false);
        }

        public string Encoding
        {
            get
            {
                return this.encoding;
            }
            set
            {
                this.encoding = value;
            }
        }

        public string EncryptionMethod
        {
            get
            {
                return this.encryptionMethod.algorithm;
            }
            set
            {
                this.encryptionMethod.algorithm = value;
            }
        }

        public XmlDictionaryString EncryptionMethodDictionaryString
        {
            get
            {
                return this.encryptionMethod.algorithmDictionaryString;
            }
            set
            {
                this.encryptionMethod.algorithmDictionaryString = value;
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
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        // This is set to true on the client side. And this means that when this knob is set to true and the default serializers on the client side fail 
        // to read the KeyInfo clause from the incoming response message from a service; then the ckient should 
        // try to read the keyInfo clause as GenericXmlSecurityKeyIdentifierClause before throwing.
        public bool ShouldReadXmlReferenceKeyInfoClause
        {
            get
            {
                return this.shouldReadXmlReferenceKeyInfoClause;
            }
            set
            {
                this.shouldReadXmlReferenceKeyInfoClause = value;
            }
        }

        public string WsuId
        {
            get
            {
                return this.wsuId;
            }
            set
            {
                this.wsuId = value;
            }
        }

        public SecurityKeyIdentifier KeyIdentifier
        {
            get
            {
                return this.keyIdentifier;
            }
            set
            {
                this.keyIdentifier = value;
            }
        }

        public string MimeType
        {
            get
            {
                return this.mimeType;
            }
            set
            {
                this.mimeType = value;
            }
        }

        public string Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        protected abstract XmlDictionaryString OpeningElementName
        {
            get;
        }

        protected EncryptionState State
        {
            get
            {
                return this.state;
            }
            set
            {
                this.state = value;
            }
        }

        public SecurityTokenSerializer SecurityTokenSerializer
        {
            get
            {
                return this.tokenSerializer;
            }
            set
            {
                this.tokenSerializer = value ?? new KeyInfoSerializer(false);
            }
        }

        protected abstract void ForceEncryption();

        protected virtual void ReadAdditionalAttributes(XmlDictionaryReader reader)
        {
        }

        protected virtual void ReadAdditionalElements(XmlDictionaryReader reader)
        {
        }

        protected abstract void ReadCipherData(XmlDictionaryReader reader);
        protected abstract void ReadCipherData(XmlDictionaryReader reader, long maxBufferSize);

        public void ReadFrom(XmlReader reader)
        {
            ReadFrom(reader, 0);
        }

        public void ReadFrom(XmlDictionaryReader reader)
        {
            ReadFrom(reader, 0);
        }

        public void ReadFrom(XmlReader reader, long maxBufferSize)
        {
            ReadFrom(XmlDictionaryReader.CreateDictionaryReader(reader), maxBufferSize);
        }

        public void ReadFrom(XmlDictionaryReader reader, long maxBufferSize)
        {
            ValidateReadState();
            reader.MoveToStartElement(OpeningElementName, NamespaceUri);
            this.encoding = reader.GetAttribute(EncodingAttribute, null);
            this.id = reader.GetAttribute(XD.XmlEncryptionDictionary.Id, null) ?? SecurityUniqueId.Create().Value;
            this.wsuId = reader.GetAttribute(XD.XmlEncryptionDictionary.Id, XD.UtilityDictionary.Namespace) ?? SecurityUniqueId.Create().Value;
            this.mimeType = reader.GetAttribute(MimeTypeAttribute, null);
            this.type = reader.GetAttribute(TypeAttribute, null);
            ReadAdditionalAttributes(reader);
            reader.Read();

            if (reader.IsStartElement(EncryptionMethodElement.ElementName, NamespaceUri))
            {
                this.encryptionMethod.ReadFrom(reader);
            }

            if (this.tokenSerializer.CanReadKeyIdentifier(reader))
            {
                XmlElement xml = null;
                XmlDictionaryReader localReader;

                if (this.ShouldReadXmlReferenceKeyInfoClause)
                {
                    // We create the dom only when needed to not affect perf.
                    XmlDocument doc = new XmlDocument();
                    xml = (doc.ReadNode(reader) as XmlElement);
                    localReader = XmlDictionaryReader.CreateDictionaryReader(new XmlNodeReader(xml));
                }
                else
                {
                    localReader = reader;
                }

                try
                {
                    this.KeyIdentifier = this.tokenSerializer.ReadKeyIdentifier(localReader);
                }
                catch (Exception e)
                {
                    // In case when the issued token ( custom token) is used as an initiator token; we will fail 
                    // to read the keyIdentifierClause using the plugged in default serializer. So We need to try to read it as an XmlReferencekeyIdentifierClause 
                    // if it is the client side.

                    if (Fx.IsFatal(e) || !this.ShouldReadXmlReferenceKeyInfoClause)
                    {
                        throw;
                    }

                    this.keyIdentifier = ReadGenericXmlSecurityKeyIdentifier( XmlDictionaryReader.CreateDictionaryReader( new XmlNodeReader(xml)), e);
                }
            }

            reader.ReadStartElement(CipherDataElementName, EncryptedType.NamespaceUri);
            reader.ReadStartElement(CipherValueElementName, EncryptedType.NamespaceUri);
            if (maxBufferSize == 0)
                ReadCipherData(reader);
            else
                ReadCipherData(reader, maxBufferSize);
            reader.ReadEndElement(); // CipherValue
            reader.ReadEndElement(); // CipherData

            ReadAdditionalElements(reader);
            reader.ReadEndElement(); // OpeningElementName
            this.State = EncryptionState.Read;
        }

        private SecurityKeyIdentifier ReadGenericXmlSecurityKeyIdentifier(XmlDictionaryReader localReader, Exception previousException)
        {
            if (!localReader.IsStartElement(XD.XmlSignatureDictionary.KeyInfo, XD.XmlSignatureDictionary.Namespace))
            {
                return null;
            }

            localReader.ReadStartElement(XD.XmlSignatureDictionary.KeyInfo, XD.XmlSignatureDictionary.Namespace);
            SecurityKeyIdentifier keyIdentifier = new SecurityKeyIdentifier();
          
            if (localReader.IsStartElement())
            {
                SecurityKeyIdentifierClause clause = null;
                string strId = localReader.GetAttribute(XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace);
                XmlDocument doc = new XmlDocument();
                XmlElement keyIdentifierReferenceXml = (doc.ReadNode(localReader) as XmlElement);
                clause = new GenericXmlSecurityKeyIdentifierClause(keyIdentifierReferenceXml);
                if (!string.IsNullOrEmpty(strId))
                    clause.Id = strId;
                keyIdentifier.Add(clause);
            }

            if (keyIdentifier.Count == 0)
                throw previousException;

            localReader.ReadEndElement();
            return keyIdentifier;
        }

        protected virtual void WriteAdditionalAttributes(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
        }

        protected virtual void WriteAdditionalElements(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
        }

        protected abstract void WriteCipherData(XmlDictionaryWriter writer);

        public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            ValidateWriteState();
            writer.WriteStartElement(XmlEncryptionStrings.Prefix, this.OpeningElementName, NamespaceUri);
            if (this.id != null && this.id.Length != 0)
            {
                writer.WriteAttributeString(XD.XmlEncryptionDictionary.Id, null, this.Id);
            }
            if (this.type != null)
            {
                writer.WriteAttributeString(TypeAttribute, null, this.Type);
            }
            if (this.mimeType != null)
            {
                writer.WriteAttributeString(MimeTypeAttribute, null, this.MimeType);
            }
            if (this.encoding != null)
            {
                writer.WriteAttributeString(EncodingAttribute, null, this.Encoding);
            }
            WriteAdditionalAttributes(writer, dictionaryManager);
            if (this.encryptionMethod.algorithm != null)
            {
                this.encryptionMethod.WriteTo(writer);
            }
            if (this.KeyIdentifier != null)
            {
                this.tokenSerializer.WriteKeyIdentifier(writer, this.KeyIdentifier);
            }

            writer.WriteStartElement(CipherDataElementName, NamespaceUri);
            writer.WriteStartElement(CipherValueElementName, NamespaceUri);
            WriteCipherData(writer);
            writer.WriteEndElement(); // CipherValue
            writer.WriteEndElement(); // CipherData

            WriteAdditionalElements(writer, dictionaryManager);
            writer.WriteEndElement(); // OpeningElementName
        }

        void ValidateReadState()
        {
            if (this.State != EncryptionState.New)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityMessageSerializationException(SR.GetString(SR.BadEncryptionState)));
            }
        }

        void ValidateWriteState()
        {
            if (this.State == EncryptionState.EncryptionSetup)
            {
                ForceEncryption();
            }
            else if (this.State == EncryptionState.New)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityMessageSerializationException(SR.GetString(SR.BadEncryptionState)));
            }
        }

        protected enum EncryptionState
        {
            New,
            Read,
            DecryptionSetup,
            Decrypted,
            EncryptionSetup,
            Encrypted
        }
        
        struct EncryptionMethodElement
        {
            internal string algorithm;
            internal XmlDictionaryString algorithmDictionaryString;
            internal static readonly XmlDictionaryString ElementName = XD.XmlEncryptionDictionary.EncryptionMethod;

            public void Init()
            {
                this.algorithm = null;
            }

            public void ReadFrom(XmlDictionaryReader reader)
            {
                reader.MoveToStartElement(ElementName, XD.XmlEncryptionDictionary.Namespace);
                bool isEmptyElement = reader.IsEmptyElement;
                this.algorithm = reader.GetAttribute(XD.XmlSignatureDictionary.Algorithm, null);
                if (this.algorithm == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityMessageSerializationException(
                        SR.GetString(SR.RequiredAttributeMissing, XD.XmlSignatureDictionary.Algorithm.Value, ElementName.Value)));
                }
                reader.Read();
                if (!isEmptyElement)
                {
                    while (reader.IsStartElement())
                    {
                        reader.Skip();
                    }
                    reader.ReadEndElement();
                }
            }

            public void WriteTo(XmlDictionaryWriter writer)
            {
                writer.WriteStartElement(XmlEncryptionStrings.Prefix, ElementName, XD.XmlEncryptionDictionary.Namespace);
                if (this.algorithmDictionaryString != null)
                {
                    writer.WriteStartAttribute(XD.XmlSignatureDictionary.Algorithm, null);
                    writer.WriteString(this.algorithmDictionaryString);
                    writer.WriteEndAttribute();
                }
                else
                {
                    writer.WriteAttributeString(XD.XmlSignatureDictionary.Algorithm, null, this.algorithm);
                }
                if (this.algorithm == XD.SecurityAlgorithmDictionary.RsaOaepKeyWrap.Value)
                {
                    writer.WriteStartElement(XmlSignatureStrings.Prefix, XD.XmlSignatureDictionary.DigestMethod, XD.XmlSignatureDictionary.Namespace);
                    writer.WriteStartAttribute(XD.XmlSignatureDictionary.Algorithm, null);
                    writer.WriteString(XD.SecurityAlgorithmDictionary.Sha1Digest);
                    writer.WriteEndAttribute();
                    writer.WriteEndElement();
                }
                writer.WriteEndElement(); // EncryptionMethod
            }
        }
    }
}
