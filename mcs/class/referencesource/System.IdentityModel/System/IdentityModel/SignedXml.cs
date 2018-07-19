//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Selectors;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml;

    sealed class SignedXml : ISignatureValueSecurityElement
    {
        internal const string DefaultPrefix = XmlSignatureStrings.Prefix;

        SecurityTokenSerializer tokenSerializer;
        readonly Signature signature;
        TransformFactory transformFactory;
        DictionaryManager dictionaryManager;

        public SignedXml(DictionaryManager dictionaryManager, SecurityTokenSerializer tokenSerializer)
            : this(new StandardSignedInfo(dictionaryManager), dictionaryManager, tokenSerializer)
        {
        }

        internal SignedXml(SignedInfo signedInfo, DictionaryManager dictionaryManager, SecurityTokenSerializer tokenSerializer)
        {
            if (signedInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("signedInfo"));
            }
            if (dictionaryManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionaryManager");
            }
            if (tokenSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenSerializer");
            }
            this.transformFactory = StandardTransformFactory.Instance;
            this.tokenSerializer = tokenSerializer;
            this.signature = new Signature(this, signedInfo);
            this.dictionaryManager = dictionaryManager;
        }

        public bool HasId
        {
            get { return true; }
        }

        public string Id
        {
            get { return this.signature.Id; }
            set { this.signature.Id = value; }
        }

        public SecurityTokenSerializer SecurityTokenSerializer
        {
            get { return this.tokenSerializer; }
        }

        public Signature Signature
        {
            get { return this.signature; }
        }

        public TransformFactory TransformFactory
        {
            get { return this.transformFactory; }
            set { this.transformFactory = value; }
        }

        void ComputeSignature(HashAlgorithm hash, AsymmetricSignatureFormatter formatter, string signatureMethod)
        {
            this.Signature.SignedInfo.ComputeReferenceDigests();
            this.Signature.SignedInfo.ComputeHash(hash);
            byte[] signature;
            if (SecurityUtils.RequiresFipsCompliance && signatureMethod == SecurityAlgorithms.RsaSha256Signature)
            {
                // This is to avoid the RSAPKCS1SignatureFormatter.CreateSignature from using SHA256Managed (non-FIPS-Compliant).
                // Hence we precompute the hash using SHA256CSP (FIPS compliant) and pass it to method.
                // NOTE: RSAPKCS1SignatureFormatter does not understand SHA256CSP inherently and hence this workaround. 
                formatter.SetHashAlgorithm("SHA256");
                signature = formatter.CreateSignature(hash.Hash);
            }
            else
            {
                signature = formatter.CreateSignature(hash);
            }
            this.Signature.SetSignatureValue(signature);
        }

        void ComputeSignature(KeyedHashAlgorithm hash)
        {
            this.Signature.SignedInfo.ComputeReferenceDigests();
            this.Signature.SignedInfo.ComputeHash(hash);
            byte[] signature = hash.Hash;
            this.Signature.SetSignatureValue(signature);
        }

        public void ComputeSignature(SecurityKey signingKey)
        {
            string signatureMethod = this.Signature.SignedInfo.SignatureMethod;
            SymmetricSecurityKey symmetricKey = signingKey as SymmetricSecurityKey;
            if (symmetricKey != null)
            {
                using (KeyedHashAlgorithm algorithm = symmetricKey.GetKeyedHashAlgorithm(signatureMethod))
                {
                    if (algorithm == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.UnableToCreateKeyedHashAlgorithm, symmetricKey, signatureMethod)));
                    }
                    ComputeSignature(algorithm);
                }
            }
            else
            {
                AsymmetricSecurityKey asymmetricKey = signingKey as AsymmetricSecurityKey;
                if (asymmetricKey == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.UnknownICryptoType, signingKey)));
                }
                using (HashAlgorithm hash = asymmetricKey.GetHashAlgorithmForSignature(signatureMethod))
                {
                    if (hash == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.UnableToCreateHashAlgorithmFromAsymmetricCrypto, signatureMethod, asymmetricKey)));
                    }

                    AsymmetricSignatureFormatter formatter = asymmetricKey.GetSignatureFormatter(signatureMethod);
                    if (formatter == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                            SR.GetString(SR.UnableToCreateSignatureFormatterFromAsymmetricCrypto, signatureMethod, asymmetricKey)));
                    }
                    ComputeSignature(hash, formatter, signatureMethod);
                }
            }
        }

        public void CompleteSignatureVerification()
        {
            this.Signature.SignedInfo.EnsureAllReferencesVerified();
        }

        public void EnsureDigestValidity(string id, object resolvedXmlSource)
        {
            this.Signature.SignedInfo.EnsureDigestValidity(id, resolvedXmlSource);
        }

        public bool EnsureDigestValidityIfIdMatches(string id, object resolvedXmlSource)
        {
            return this.Signature.SignedInfo.EnsureDigestValidityIfIdMatches(id, resolvedXmlSource);
        }

        public byte[] GetSignatureValue()
        {
            return this.Signature.GetSignatureBytes();
        }

        public void ReadFrom(XmlReader reader)
        {
            ReadFrom(XmlDictionaryReader.CreateDictionaryReader(reader));
        }

        public void ReadFrom(XmlDictionaryReader reader)
        {
            this.signature.ReadFrom(reader, this.dictionaryManager);
        }

        void VerifySignature(KeyedHashAlgorithm hash)
        {
            this.Signature.SignedInfo.ComputeHash(hash);
            if (!CryptoHelper.IsEqual(hash.Hash, GetSignatureValue()))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.SignatureVerificationFailed)));
            }
        }

        void VerifySignature(HashAlgorithm hash, AsymmetricSignatureDeformatter deformatter, string signatureMethod)
        {
            this.Signature.SignedInfo.ComputeHash(hash);
            bool result;

            if (SecurityUtils.RequiresFipsCompliance && signatureMethod == SecurityAlgorithms.RsaSha256Signature)
            {
                // This is to avoid the RSAPKCS1SignatureFormatter.VerifySignature from using SHA256Managed (non-FIPS-Compliant).
                // Hence we precompute the hash using SHA256CSP (FIPS compliant) and pass it to method.
                // NOTE: RSAPKCS1SignatureFormatter does not understand SHA256CSP inherently and hence this workaround. 
                deformatter.SetHashAlgorithm("SHA256");
                result = deformatter.VerifySignature(hash.Hash, GetSignatureValue());
            }
            else
            {
                result = deformatter.VerifySignature(hash, GetSignatureValue());
            }

            if (!result)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.SignatureVerificationFailed)));
            }
        }

        public void StartSignatureVerification(SecurityKey verificationKey)
        {
            string signatureMethod = this.Signature.SignedInfo.SignatureMethod;
            SymmetricSecurityKey symmetricKey = verificationKey as SymmetricSecurityKey;
            if (symmetricKey != null)
            {
                using (KeyedHashAlgorithm hash = symmetricKey.GetKeyedHashAlgorithm(signatureMethod))
                {
                    if (hash == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(
                            SR.GetString(SR.UnableToCreateKeyedHashAlgorithmFromSymmetricCrypto, signatureMethod, symmetricKey)));
                    }
                    VerifySignature(hash);
                }
            }
            else
            {
                AsymmetricSecurityKey asymmetricKey = verificationKey as AsymmetricSecurityKey;
                if (asymmetricKey == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UnknownICryptoType, verificationKey)));
                }
                using (HashAlgorithm hash = asymmetricKey.GetHashAlgorithmForSignature(signatureMethod))
                {
                    if (hash == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(
                            SR.GetString(SR.UnableToCreateHashAlgorithmFromAsymmetricCrypto, signatureMethod, asymmetricKey)));
                    }
                    AsymmetricSignatureDeformatter deformatter = asymmetricKey.GetSignatureDeformatter(signatureMethod);
                    if (deformatter == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(
                            SR.GetString(SR.UnableToCreateSignatureDeformatterFromAsymmetricCrypto, signatureMethod, asymmetricKey)));
                    }

                    VerifySignature(hash, deformatter, signatureMethod);
                }
            }
        }

        public void WriteTo(XmlDictionaryWriter writer)
        {
            this.WriteTo(writer, this.dictionaryManager);
        }

        public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            this.signature.WriteTo(writer, dictionaryManager);
        }
    }

    sealed class Signature
    {
        SignedXml signedXml;
        string id;
        SecurityKeyIdentifier keyIdentifier;
        string prefix = SignedXml.DefaultPrefix;
        readonly SignatureValueElement signatureValueElement = new SignatureValueElement();
        readonly SignedInfo signedInfo;

        public Signature(SignedXml signedXml, SignedInfo signedInfo)
        {
            this.signedXml = signedXml;
            this.signedInfo = signedInfo;
        }

        public string Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        public SecurityKeyIdentifier KeyIdentifier
        {
            get { return this.keyIdentifier; }
            set { this.keyIdentifier = value; }
        }

        public SignedInfo SignedInfo
        {
            get { return this.signedInfo; }
        }

        public ISignatureValueSecurityElement SignatureValue
        {
            get { return this.signatureValueElement; }
        }

        public byte[] GetSignatureBytes()
        {
            return this.signatureValueElement.Value;
        }

        public void ReadFrom(XmlDictionaryReader reader, DictionaryManager dictionaryManager)
        {
            reader.MoveToStartElement(dictionaryManager.XmlSignatureDictionary.Signature, dictionaryManager.XmlSignatureDictionary.Namespace);
            this.prefix = reader.Prefix;
            this.Id = reader.GetAttribute(dictionaryManager.UtilityDictionary.IdAttribute, null);
            reader.Read();

            this.signedInfo.ReadFrom(reader, signedXml.TransformFactory, dictionaryManager);
            this.signatureValueElement.ReadFrom(reader, dictionaryManager);
            if (signedXml.SecurityTokenSerializer.CanReadKeyIdentifier(reader))
            {
                this.keyIdentifier = signedXml.SecurityTokenSerializer.ReadKeyIdentifier(reader);
            }
            reader.ReadEndElement(); // Signature
        }

        public void SetSignatureValue(byte[] signatureValue)
        {
            this.signatureValueElement.Value = signatureValue;
        }

        public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            writer.WriteStartElement(this.prefix, dictionaryManager.XmlSignatureDictionary.Signature, dictionaryManager.XmlSignatureDictionary.Namespace);
            if (this.id != null)
            {
                writer.WriteAttributeString(dictionaryManager.UtilityDictionary.IdAttribute, null, this.id);
            }
            this.signedInfo.WriteTo(writer, dictionaryManager);
            this.signatureValueElement.WriteTo(writer, dictionaryManager);
            if (this.keyIdentifier != null)
            {
                this.signedXml.SecurityTokenSerializer.WriteKeyIdentifier(writer, this.keyIdentifier);
            }

            writer.WriteEndElement(); // Signature
        }

        sealed class SignatureValueElement : ISignatureValueSecurityElement
        {
            string id;
            string prefix = SignedXml.DefaultPrefix;
            byte[] signatureValue;
            string signatureText;

            public bool HasId
            {
                get { return true; }
            }

            public string Id
            {
                get { return this.id; }
                set { this.id = value; }
            }

            internal byte[] Value
            {
                get { return this.signatureValue; }
                set
                {
                    this.signatureValue = value;
                    this.signatureText = null;
                }
            }

            public void ReadFrom(XmlDictionaryReader reader, DictionaryManager dictionaryManager)
            {
                reader.MoveToStartElement(dictionaryManager.XmlSignatureDictionary.SignatureValue, dictionaryManager.XmlSignatureDictionary.Namespace);
                this.prefix = reader.Prefix;
                this.Id = reader.GetAttribute(UtilityStrings.IdAttribute, null);
                reader.Read();

                this.signatureText = reader.ReadString();
                this.signatureValue = System.Convert.FromBase64String(signatureText.Trim());

                reader.ReadEndElement(); // SignatureValue
            }

            public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
            {
                writer.WriteStartElement(this.prefix, dictionaryManager.XmlSignatureDictionary.SignatureValue, dictionaryManager.XmlSignatureDictionary.Namespace);
                if (this.id != null)
                {
                    writer.WriteAttributeString(dictionaryManager.UtilityDictionary.IdAttribute, null, this.id);
                }
                if (this.signatureText != null)
                {
                    writer.WriteString(this.signatureText);
                }
                else
                {
                    writer.WriteBase64(this.signatureValue, 0, this.signatureValue.Length);
                }
                writer.WriteEndElement(); // SignatureValue
            }

            byte[] ISignatureValueSecurityElement.GetSignatureValue()
            {
                return this.Value;
            }
        }
    }

    internal interface ISignatureReaderProvider
    {
        XmlDictionaryReader GetReader(object callbackContext);
    }

    abstract class SignedInfo : ISecurityElement
    {
        readonly ExclusiveCanonicalizationTransform canonicalizationMethodElement = new ExclusiveCanonicalizationTransform(true);
        string id;
        ElementWithAlgorithmAttribute signatureMethodElement;
        SignatureResourcePool resourcePool;
        DictionaryManager dictionaryManager;
        MemoryStream canonicalStream;
        ISignatureReaderProvider readerProvider;
        object signatureReaderProviderCallbackContext;
        bool sendSide = true;

        protected SignedInfo(DictionaryManager dictionaryManager)
        {
            if (dictionaryManager == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionaryManager");

            this.signatureMethodElement = new ElementWithAlgorithmAttribute(dictionaryManager.XmlSignatureDictionary.SignatureMethod);
            this.dictionaryManager = dictionaryManager;
        }

        protected DictionaryManager DictionaryManager
        {
            get { return this.dictionaryManager; }
        }

        protected MemoryStream CanonicalStream
        {
            get { return this.canonicalStream; }
            set { this.canonicalStream = value; }
        }

        protected bool SendSide
        {
            get { return this.sendSide; }
            set { this.sendSide = value; }
        }

        public ISignatureReaderProvider ReaderProvider
        {
            get { return this.readerProvider; }
            set { this.readerProvider = value; }
        }

        public object SignatureReaderProviderCallbackContext
        {
            get { return this.signatureReaderProviderCallbackContext; }
            set { this.signatureReaderProviderCallbackContext = value; }
        }

        public string CanonicalizationMethod
        {
            get { return this.canonicalizationMethodElement.Algorithm; }
            set
            {
                if (value != this.canonicalizationMethodElement.Algorithm)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedTransformAlgorithm)));
                }
            }
        }

        public XmlDictionaryString CanonicalizationMethodDictionaryString
        {
            set
            {
                if (value != null && value.Value != this.canonicalizationMethodElement.Algorithm)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedTransformAlgorithm)));
                }
            }
        }

        public bool HasId
        {
            get { return true; }
        }

        public string Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        public abstract int ReferenceCount
        {
            get;
        }

        public string SignatureMethod
        {
            get { return this.signatureMethodElement.Algorithm; }
            set { this.signatureMethodElement.Algorithm = value; }
        }

        public XmlDictionaryString SignatureMethodDictionaryString
        {
            get { return this.signatureMethodElement.AlgorithmDictionaryString; }
            set { this.signatureMethodElement.AlgorithmDictionaryString = value; }
        }

        public SignatureResourcePool ResourcePool
        {
            get
            {
                if (this.resourcePool == null)
                {
                    this.resourcePool = new SignatureResourcePool();
                }
                return this.resourcePool;
            }
            set
            {
                this.resourcePool = value;
            }
        }

        public void ComputeHash(HashAlgorithm algorithm)
        {
            if ((this.CanonicalizationMethod != SecurityAlgorithms.ExclusiveC14n) && (this.CanonicalizationMethod != SecurityAlgorithms.ExclusiveC14nWithComments))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.UnsupportedTransformAlgorithm)));
            }
            HashStream hashStream = this.ResourcePool.TakeHashStream(algorithm);
            ComputeHash(hashStream);
            hashStream.FlushHash();
        }

        protected virtual void ComputeHash(HashStream hashStream)
        {
            if (this.sendSide)
            {
                XmlDictionaryWriter utf8Writer = this.ResourcePool.TakeUtf8Writer();
                utf8Writer.StartCanonicalization(hashStream, false, null);
                WriteTo(utf8Writer, this.dictionaryManager);
                utf8Writer.EndCanonicalization();
            }
            else if (this.canonicalStream != null)
            {
                this.canonicalStream.WriteTo(hashStream);
            }
            else
            {
                if (this.readerProvider == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.InclusiveNamespacePrefixRequiresSignatureReader)));

                XmlDictionaryReader signatureReader = this.readerProvider.GetReader(this.signatureReaderProviderCallbackContext);

                DiagnosticUtility.DebugAssert(signatureReader != null, "Require a Signature reader to validate signature.");

                if (!signatureReader.CanCanonicalize)
                {
                    MemoryStream stream = new MemoryStream();
                    XmlDictionaryWriter bufferingWriter = XmlDictionaryWriter.CreateBinaryWriter(stream, this.DictionaryManager.ParentDictionary);
                    string[] inclusivePrefix = GetInclusivePrefixes();
                    if (inclusivePrefix != null)
                    {
                        bufferingWriter.WriteStartElement("a");
                        for (int i = 0; i < inclusivePrefix.Length; ++i)
                        {
                            string ns = GetNamespaceForInclusivePrefix(inclusivePrefix[i]);
                            if (ns != null)
                            {
                                bufferingWriter.WriteXmlnsAttribute(inclusivePrefix[i], ns);
                            }
                        }
                    }
                    signatureReader.MoveToContent();
                    bufferingWriter.WriteNode(signatureReader, false);
                    if (inclusivePrefix != null)
                        bufferingWriter.WriteEndElement();
                    bufferingWriter.Flush();
                    byte[] buffer = stream.ToArray();
                    int bufferLength = (int)stream.Length;
                    bufferingWriter.Close();

                    signatureReader.Close();

                    // Create a reader around the buffering Stream.
                    signatureReader = XmlDictionaryReader.CreateBinaryReader(buffer, 0, bufferLength, this.DictionaryManager.ParentDictionary, XmlDictionaryReaderQuotas.Max);
                    if (inclusivePrefix != null)
                        signatureReader.ReadStartElement("a");
                }
                signatureReader.ReadStartElement(dictionaryManager.XmlSignatureDictionary.Signature, dictionaryManager.XmlSignatureDictionary.Namespace);
                signatureReader.MoveToStartElement(dictionaryManager.XmlSignatureDictionary.SignedInfo, dictionaryManager.XmlSignatureDictionary.Namespace);
                signatureReader.StartCanonicalization(hashStream, false, GetInclusivePrefixes());
                signatureReader.Skip();
                signatureReader.EndCanonicalization();
                signatureReader.Close();
            }
        }

        public abstract void ComputeReferenceDigests();

        protected string[] GetInclusivePrefixes()
        {
            return this.canonicalizationMethodElement.GetInclusivePrefixes();
        }

        protected virtual string GetNamespaceForInclusivePrefix(string prefix)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        public abstract void EnsureAllReferencesVerified();

        public void EnsureDigestValidity(string id, object resolvedXmlSource)
        {
            if (!EnsureDigestValidityIfIdMatches(id, resolvedXmlSource))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(
                    SR.GetString(SR.RequiredTargetNotSigned, id)));
            }
        }

        public abstract bool EnsureDigestValidityIfIdMatches(string id, object resolvedXmlSource);

        public virtual bool HasUnverifiedReference(string id)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
        }

        protected void ReadCanonicalizationMethod(XmlDictionaryReader reader, DictionaryManager dictionaryManager)
        {
            // we will ignore any comments in the SignedInfo elemnt when verifying signature
            this.canonicalizationMethodElement.ReadFrom(reader, dictionaryManager, false);
        }

        public abstract void ReadFrom(XmlDictionaryReader reader, TransformFactory transformFactory, DictionaryManager dictionaryManager);

        protected void ReadSignatureMethod(XmlDictionaryReader reader, DictionaryManager dictionaryManager)
        {
            this.signatureMethodElement.ReadFrom(reader, dictionaryManager);
        }

        protected void WriteCanonicalizationMethod(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            this.canonicalizationMethodElement.WriteTo(writer, dictionaryManager);
        }

        protected void WriteSignatureMethod(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            this.signatureMethodElement.WriteTo(writer, dictionaryManager);
        }

        public abstract void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager);
    }

    // whitespace preservation convention: ws1 immediately inside open tag; ws2 immediately after end tag.
    class StandardSignedInfo : SignedInfo
    {
        string prefix = SignedXml.DefaultPrefix;
        List<Reference> references;
        Dictionary<string, string> context;

        public StandardSignedInfo(DictionaryManager dictionaryManager)
            : base(dictionaryManager)
        {
            this.references = new List<Reference>();
        }

        public override int ReferenceCount
        {
            get { return this.references.Count; }
        }

        public Reference this[int index]
        {
            get { return this.references[index]; }
        }

        public void AddReference(Reference reference)
        {
            reference.ResourcePool = this.ResourcePool;
            this.references.Add(reference);
        }

        public override void EnsureAllReferencesVerified()
        {
            for (int i = 0; i < this.references.Count; i++)
            {
                if (!this.references[i].Verified)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new CryptographicException(SR.GetString(SR.UnableToResolveReferenceUriForSignature, this.references[i].Uri)));
                }
            }
        }

        public override bool EnsureDigestValidityIfIdMatches(string id, object resolvedXmlSource)
        {
            for (int i = 0; i < this.references.Count; i++)
            {
                if (this.references[i].EnsureDigestValidityIfIdMatches(id, resolvedXmlSource))
                {
                    return true;
                }
            }
            return false;
        }

        public override bool HasUnverifiedReference(string id)
        {
            for (int i = 0; i < this.references.Count; i++)
            {
                if (!this.references[i].Verified && this.references[i].ExtractReferredId() == id)
                {
                    return true;
                }
            }
            return false;
        }

        public override void ComputeReferenceDigests()
        {
            if (this.references.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.AtLeastOneReferenceRequired)));
            }
            for (int i = 0; i < this.references.Count; i++)
            {
                this.references[i].ComputeAndSetDigest();
            }
        }

        public override void ReadFrom(XmlDictionaryReader reader, TransformFactory transformFactory, DictionaryManager dictionaryManager)
        {
            this.SendSide = false;
            if (reader.CanCanonicalize)
            {
                this.CanonicalStream = new MemoryStream();
                reader.StartCanonicalization(this.CanonicalStream, false, null);
            }

            reader.MoveToStartElement(dictionaryManager.XmlSignatureDictionary.SignedInfo, dictionaryManager.XmlSignatureDictionary.Namespace);
            this.prefix = reader.Prefix;
            this.Id = reader.GetAttribute(dictionaryManager.UtilityDictionary.IdAttribute, null);
            reader.Read();

            ReadCanonicalizationMethod(reader, dictionaryManager);
            ReadSignatureMethod(reader, dictionaryManager);
            while (reader.IsStartElement(dictionaryManager.XmlSignatureDictionary.Reference, dictionaryManager.XmlSignatureDictionary.Namespace))
            {
                Reference reference = new Reference(dictionaryManager);
                reference.ReadFrom(reader, transformFactory, dictionaryManager);
                AddReference(reference);
            }
            reader.ReadEndElement(); // SignedInfo

            if (reader.CanCanonicalize)
                reader.EndCanonicalization();

            string[] inclusivePrefixes = GetInclusivePrefixes();
            if (inclusivePrefixes != null)
            {
                // Clear the canonicalized stream. We cannot use this while inclusive prefixes are
                // specified.
                this.CanonicalStream = null;
                this.context = new Dictionary<string, string>(inclusivePrefixes.Length);
                for (int i = 0; i < inclusivePrefixes.Length; i++)
                {
                    this.context.Add(inclusivePrefixes[i], reader.LookupNamespace(inclusivePrefixes[i]));
                }
            }
        }

        public override void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            writer.WriteStartElement(this.prefix, dictionaryManager.XmlSignatureDictionary.SignedInfo, dictionaryManager.XmlSignatureDictionary.Namespace);
            if (this.Id != null)
            {
                writer.WriteAttributeString(dictionaryManager.UtilityDictionary.IdAttribute, null, this.Id);
            }
            WriteCanonicalizationMethod(writer, dictionaryManager);
            WriteSignatureMethod(writer, dictionaryManager);
            for (int i = 0; i < this.references.Count; i++)
            {
                this.references[i].WriteTo(writer, dictionaryManager);
            }
            writer.WriteEndElement(); // SignedInfo
        }

        protected override string GetNamespaceForInclusivePrefix(string prefix)
        {
            if (this.context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());

            if (prefix == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("prefix");

            return context[prefix];
        }

        protected string Prefix
        {
            get { return prefix; }
            set { prefix = value; }
        }

        protected Dictionary<string, string> Context
        {
            get { return context; }
            set { context = value; }
        }
    }

    sealed class WifSignedInfo : StandardSignedInfo, IDisposable
    {
        MemoryStream _bufferedStream;
        string _defaultNamespace = String.Empty;
        bool _disposed;

        public WifSignedInfo(DictionaryManager dictionaryManager)
            : base(dictionaryManager)
        {
        }

        ~WifSignedInfo()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                //
                // Free all of our managed resources
                //                
                if (_bufferedStream != null)
                {
                    _bufferedStream.Close();
                    _bufferedStream = null;
                }
            }

            // Free native resources, if any.

            _disposed = true;

        }

        protected override void ComputeHash(HashStream hashStream)
        {
            if (SendSide)
            {
                using (XmlDictionaryWriter utf8Writer = XmlDictionaryWriter.CreateTextWriter(Stream.Null, Encoding.UTF8, false))
                {
                    utf8Writer.StartCanonicalization(hashStream, false, null);
                    WriteTo(utf8Writer, DictionaryManager);
                    utf8Writer.EndCanonicalization();
                }
            }
            else if (CanonicalStream != null)
            {
                CanonicalStream.WriteTo(hashStream);
            }
            else
            {
                _bufferedStream.Position = 0;
                // We are creating a XmlDictionaryReader with a hard-coded Max XmlDictionaryReaderQuotas. This is a reader that we
                // are creating over an already buffered content. The content was initially read off user provided XmlDictionaryReader
                // with the correct quotas and hence we know the data is valid.
                // Note: signedinfoReader will close _bufferedStream on Dispose.
                using (XmlDictionaryReader signedinfoReader = XmlDictionaryReader.CreateTextReader(_bufferedStream, XmlDictionaryReaderQuotas.Max))
                {
                    signedinfoReader.MoveToContent();
                    using (XmlDictionaryWriter bufferingWriter = XmlDictionaryWriter.CreateTextWriter(Stream.Null, Encoding.UTF8, false))
                    {
                        bufferingWriter.WriteStartElement("a", _defaultNamespace);
                        string[] inclusivePrefix = GetInclusivePrefixes();
                        for (int i = 0; i < inclusivePrefix.Length; ++i)
                        {
                            string ns = GetNamespaceForInclusivePrefix(inclusivePrefix[i]);
                            if (ns != null)
                            {
                                bufferingWriter.WriteXmlnsAttribute(inclusivePrefix[i], ns);
                            }
                        }
                        bufferingWriter.StartCanonicalization(hashStream, false, inclusivePrefix);
                        bufferingWriter.WriteNode(signedinfoReader, false);
                        bufferingWriter.EndCanonicalization();
                        bufferingWriter.WriteEndElement();
                    }
                }
            }
        }

        public override void ReadFrom(XmlDictionaryReader reader, TransformFactory transformFactory, DictionaryManager dictionaryManager)
        {
            reader.MoveToStartElement(XmlSignatureConstants.Elements.SignedInfo, XmlSignatureConstants.Namespace);

            SendSide = false;
            _defaultNamespace = reader.LookupNamespace(String.Empty);
            _bufferedStream = new MemoryStream();


            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.NewLineHandling = NewLineHandling.None;

            using (XmlWriter bufferWriter = XmlTextWriter.Create(_bufferedStream, settings))
            {
                bufferWriter.WriteNode(reader, true);
                bufferWriter.Flush();
            }

            _bufferedStream.Position = 0;

            //
            // We are creating a XmlDictionaryReader with a hard-coded Max XmlDictionaryReaderQuotas. This is a reader that we
            // are creating over an already buffered content. The content was initially read off user provided XmlDictionaryReader
            // with the correct quotas and hence we know the data is valid.
            // Note: effectiveReader will close _bufferedStream on Dispose.
            //
            using (XmlDictionaryReader effectiveReader = XmlDictionaryReader.CreateTextReader(_bufferedStream, XmlDictionaryReaderQuotas.Max))
            {
                CanonicalStream = new MemoryStream();
                effectiveReader.StartCanonicalization(CanonicalStream, false, null);

                effectiveReader.MoveToStartElement(XmlSignatureConstants.Elements.SignedInfo, XmlSignatureConstants.Namespace);
                Prefix = effectiveReader.Prefix;
                Id = effectiveReader.GetAttribute(WSSecurityUtilityConstants.Attributes.Id, null);
                effectiveReader.Read();

                ReadCanonicalizationMethod(effectiveReader, DictionaryManager);
                ReadSignatureMethod(effectiveReader, DictionaryManager);
                while (effectiveReader.IsStartElement(XmlSignatureConstants.Elements.Reference, XmlSignatureConstants.Namespace))
                {
                    Reference reference = new Reference(DictionaryManager);
                    reference.ReadFrom(effectiveReader, transformFactory, DictionaryManager);
                    AddReference(reference);
                }
                effectiveReader.ReadEndElement();

                effectiveReader.EndCanonicalization();
            }

            string[] inclusivePrefixes = GetInclusivePrefixes();
            if (inclusivePrefixes != null)
            {
                // Clear the canonicalized stream. We cannot use this while inclusive prefixes are
                // specified.
                CanonicalStream = null;
                Context = new Dictionary<string, string>(inclusivePrefixes.Length);
                for (int i = 0; i < inclusivePrefixes.Length; i++)
                {
                    Context.Add(inclusivePrefixes[i], reader.LookupNamespace(inclusivePrefixes[i]));
                }
            }
        }
    }

    sealed class Reference
    {
        ElementWithAlgorithmAttribute digestMethodElement;
        DigestValueElement digestValueElement = new DigestValueElement();
        string id;
        string prefix = SignedXml.DefaultPrefix;
        object resolvedXmlSource;
        readonly TransformChain transformChain = new TransformChain();
        string type;
        string uri;
        SignatureResourcePool resourcePool;
        bool verified;
        string referredId;
        DictionaryManager dictionaryManager;

        public Reference(DictionaryManager dictionaryManager)
            : this(dictionaryManager, null)
        {
        }

        public Reference(DictionaryManager dictionaryManager, string uri)
            : this(dictionaryManager, uri, null)
        {
        }

        public Reference(DictionaryManager dictionaryManager, string uri, object resolvedXmlSource)
        {
            if (dictionaryManager == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionaryManager");

            this.dictionaryManager = dictionaryManager;
            this.digestMethodElement = new ElementWithAlgorithmAttribute(dictionaryManager.XmlSignatureDictionary.DigestMethod);
            this.uri = uri;
            this.resolvedXmlSource = resolvedXmlSource;
        }

        public string DigestMethod
        {
            get { return this.digestMethodElement.Algorithm; }
            set { this.digestMethodElement.Algorithm = value; }
        }

        public XmlDictionaryString DigestMethodDictionaryString
        {
            get { return this.digestMethodElement.AlgorithmDictionaryString; }
            set { this.digestMethodElement.AlgorithmDictionaryString = value; }
        }

        public string Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        public SignatureResourcePool ResourcePool
        {
            get { return this.resourcePool; }
            set { this.resourcePool = value; }
        }

        public TransformChain TransformChain
        {
            get { return this.transformChain; }
        }

        public int TransformCount
        {
            get { return this.transformChain.TransformCount; }
        }

        public string Type
        {
            get { return this.type; }
            set { this.type = value; }
        }

        public string Uri
        {
            get { return this.uri; }
            set { this.uri = value; }
        }

        public bool Verified
        {
            get { return this.verified; }
        }

        public void AddTransform(Transform transform)
        {
            this.transformChain.Add(transform);
        }

        public void EnsureDigestValidity(string id, byte[] computedDigest)
        {
            if (!EnsureDigestValidityIfIdMatches(id, computedDigest))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(
                    SR.GetString(SR.RequiredTargetNotSigned, id)));
            }
        }

        public void EnsureDigestValidity(string id, object resolvedXmlSource)
        {
            if (!EnsureDigestValidityIfIdMatches(id, resolvedXmlSource))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(
                    SR.GetString(SR.RequiredTargetNotSigned, id)));
            }
        }

        public bool EnsureDigestValidityIfIdMatches(string id, byte[] computedDigest)
        {
            if (this.verified || id != ExtractReferredId())
            {
                return false;
            }
            if (!CryptoHelper.IsEqual(computedDigest, GetDigestValue()))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new CryptographicException(SR.GetString(SR.DigestVerificationFailedForReference, this.uri)));
            }
            this.verified = true;
            return true;
        }

        public bool EnsureDigestValidityIfIdMatches(string id, object resolvedXmlSource)
        {
            if (this.verified)
            {
                return false;
            }

            // During StrTransform the extractedReferredId on the reference will point to STR and hence will not be 
            // equal to the referred element ie security token Id.
            if (id != ExtractReferredId() && !this.IsStrTranform())
            {
                return false;
            }

            this.resolvedXmlSource = resolvedXmlSource;
            if (!CheckDigest())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new CryptographicException(SR.GetString(SR.DigestVerificationFailedForReference, this.uri)));
            }
            this.verified = true;
            return true;
        }

        public bool IsStrTranform()
        {
            return this.TransformChain.TransformCount == 1 && this.TransformChain[0].Algorithm == SecurityAlgorithms.StrTransform;
        }


        public string ExtractReferredId()
        {
            if (this.referredId == null)
            {
                if (StringComparer.OrdinalIgnoreCase.Equals(uri, String.Empty))
                {
                    return String.Empty;
                }

                if (this.uri == null || this.uri.Length < 2 || this.uri[0] != '#')
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new CryptographicException(SR.GetString(SR.UnableToResolveReferenceUriForSignature, this.uri)));
                }
                this.referredId = this.uri.Substring(1);
            }
            return this.referredId;
        }


        /// <summary>
        /// We look at the URI reference to decide if we should preserve comments while canonicalization.
        /// Only when the reference is xpointer(/) or xpointer(id(SomeId)) do we preserve comments during canonicalization 
        /// of the reference element for computing the digest.
        /// </summary>
        /// <param name="uri">The Uri reference </param>
        /// <returns>true if comments should be preserved.</returns>
        private static bool ShouldPreserveComments(string uri)
        {
            bool preserveComments = false;

            if (!String.IsNullOrEmpty(uri))
            {
                //removes the hash
                string idref = uri.Substring(1);

                if (idref == "xpointer(/)")
                {
                    preserveComments = true;
                }
                else if (idref.StartsWith("xpointer(id(", StringComparison.Ordinal) && (idref.IndexOf(")", StringComparison.Ordinal) > 0))
                {
                    // Dealing with XPointer of type #xpointer(id("ID")). Other XPointer support isn't handled here and is anyway optional 
                    preserveComments = true;
                }
            }

            return preserveComments;
        }

        public bool CheckDigest()
        {
            byte[] computedDigest = ComputeDigest();
            bool result = CryptoHelper.IsEqual(computedDigest, GetDigestValue());
#if LOG_DIGESTS
            Console.WriteLine(">>> Checking digest for reference '{0}', result {1}", uri, result);
            Console.WriteLine("    Computed digest {0}", Convert.ToBase64String(computedDigest));
            Console.WriteLine("    Received digest {0}", Convert.ToBase64String(GetDigestValue()));
#endif
            return result;
        }

        public void ComputeAndSetDigest()
        {
            this.digestValueElement.Value = ComputeDigest();
        }

        public byte[] ComputeDigest()
        {
            if (this.transformChain.TransformCount == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.EmptyTransformChainNotSupported)));
            }

            if (this.resolvedXmlSource == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(
                    SR.GetString(SR.UnableToResolveReferenceUriForSignature, this.uri)));
            }
            return this.transformChain.TransformToDigest(this.resolvedXmlSource, this.ResourcePool, this.DigestMethod, this.dictionaryManager);
        }

        public byte[] GetDigestValue()
        {
            return this.digestValueElement.Value;
        }

        public void ReadFrom(XmlDictionaryReader reader, TransformFactory transformFactory, DictionaryManager dictionaryManager)
        {
            reader.MoveToStartElement(dictionaryManager.XmlSignatureDictionary.Reference, dictionaryManager.XmlSignatureDictionary.Namespace);
            this.prefix = reader.Prefix;
            this.Id = reader.GetAttribute(UtilityStrings.IdAttribute, null);
            this.Uri = reader.GetAttribute(dictionaryManager.XmlSignatureDictionary.URI, null);
            this.Type = reader.GetAttribute(dictionaryManager.XmlSignatureDictionary.Type, null);
            reader.Read();

            if (reader.IsStartElement(dictionaryManager.XmlSignatureDictionary.Transforms, dictionaryManager.XmlSignatureDictionary.Namespace))
            {
                this.transformChain.ReadFrom(reader, transformFactory, dictionaryManager, ShouldPreserveComments(this.Uri));
            }

            this.digestMethodElement.ReadFrom(reader, dictionaryManager);
            this.digestValueElement.ReadFrom(reader, dictionaryManager);

            reader.MoveToContent();
            reader.ReadEndElement(); // Reference
        }

        public void SetResolvedXmlSource(object resolvedXmlSource)
        {
            this.resolvedXmlSource = resolvedXmlSource;
        }

        public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            writer.WriteStartElement(this.prefix, dictionaryManager.XmlSignatureDictionary.Reference, dictionaryManager.XmlSignatureDictionary.Namespace);
            if (this.id != null)
            {
                writer.WriteAttributeString(dictionaryManager.UtilityDictionary.IdAttribute, null, this.id);
            }
            if (this.uri != null)
            {
                writer.WriteAttributeString(dictionaryManager.XmlSignatureDictionary.URI, null, this.uri);
            }
            if (this.type != null)
            {
                writer.WriteAttributeString(dictionaryManager.XmlSignatureDictionary.Type, null, this.type);
            }

            if (this.transformChain.TransformCount > 0)
            {
                this.transformChain.WriteTo(writer, dictionaryManager);
            }

            this.digestMethodElement.WriteTo(writer, dictionaryManager);
            this.digestValueElement.WriteTo(writer, dictionaryManager);

            writer.WriteEndElement(); // Reference
        }

        struct DigestValueElement
        {
            byte[] digestValue;
            string digestText;
            string prefix;

            internal byte[] Value
            {
                get { return this.digestValue; }
                set
                {
                    this.digestValue = value;
                    this.digestText = null;
                }
            }

            public void ReadFrom(XmlDictionaryReader reader, DictionaryManager dictionaryManager)
            {
                reader.MoveToStartElement(dictionaryManager.XmlSignatureDictionary.DigestValue, dictionaryManager.XmlSignatureDictionary.Namespace);
                this.prefix = reader.Prefix;
                reader.Read();
                reader.MoveToContent();

                this.digestText = reader.ReadString();
                this.digestValue = System.Convert.FromBase64String(digestText.Trim());

                reader.MoveToContent();
                reader.ReadEndElement(); // DigestValue
            }

            public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
            {
                writer.WriteStartElement(this.prefix ?? XmlSignatureStrings.Prefix, dictionaryManager.XmlSignatureDictionary.DigestValue, dictionaryManager.XmlSignatureDictionary.Namespace);
                if (this.digestText != null)
                {
                    writer.WriteString(this.digestText);
                }
                else
                {
                    writer.WriteBase64(this.digestValue, 0, this.digestValue.Length);
                }
                writer.WriteEndElement(); // DigestValue
            }
        }
    }

    sealed class TransformChain
    {
        string prefix = SignedXml.DefaultPrefix;
        MostlySingletonList<Transform> transforms;

        public TransformChain()
        {
        }

        public int TransformCount
        {
            get { return this.transforms.Count; }
        }

        public Transform this[int index]
        {
            get
            {
                return this.transforms[index];
            }
        }

        public bool NeedsInclusiveContext
        {
            get
            {
                for (int i = 0; i < this.TransformCount; i++)
                {
                    if (this[i].NeedsInclusiveContext)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public void Add(Transform transform)
        {
            this.transforms.Add(transform);
        }

        public void ReadFrom(XmlDictionaryReader reader, TransformFactory transformFactory, DictionaryManager dictionaryManager, bool preserveComments)
        {
            reader.MoveToStartElement(dictionaryManager.XmlSignatureDictionary.Transforms, dictionaryManager.XmlSignatureDictionary.Namespace);
            this.prefix = reader.Prefix;
            reader.Read();

            while (reader.IsStartElement(dictionaryManager.XmlSignatureDictionary.Transform, dictionaryManager.XmlSignatureDictionary.Namespace))
            {
                string transformAlgorithmUri = reader.GetAttribute(dictionaryManager.XmlSignatureDictionary.Algorithm, null);
                Transform transform = transformFactory.CreateTransform(transformAlgorithmUri);
                transform.ReadFrom(reader, dictionaryManager, preserveComments);
                Add(transform);
            }
            reader.MoveToContent();
            reader.ReadEndElement(); // Transforms
            if (this.TransformCount == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(SR.GetString(SR.AtLeastOneTransformRequired)));
            }
        }

        public byte[] TransformToDigest(object data, SignatureResourcePool resourcePool, string digestMethod, DictionaryManager dictionaryManager)
        {
            DiagnosticUtility.DebugAssert(TransformCount > 0, "");
            for (int i = 0; i < this.TransformCount - 1; i++)
            {
                data = this[i].Process(data, resourcePool, dictionaryManager);
            }
            return this[this.TransformCount - 1].ProcessAndDigest(data, resourcePool, digestMethod, dictionaryManager);
        }

        public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            writer.WriteStartElement(this.prefix, dictionaryManager.XmlSignatureDictionary.Transforms, dictionaryManager.XmlSignatureDictionary.Namespace);
            for (int i = 0; i < this.TransformCount; i++)
            {
                this[i].WriteTo(writer, dictionaryManager);
            }
            writer.WriteEndElement(); // Transforms
        }
    }

    struct ElementWithAlgorithmAttribute
    {
        readonly XmlDictionaryString elementName;
        string algorithm;
        XmlDictionaryString algorithmDictionaryString;
        string prefix;

        public ElementWithAlgorithmAttribute(XmlDictionaryString elementName)
        {
            if (elementName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("elementName"));
            }
            this.elementName = elementName;
            this.algorithm = null;
            this.algorithmDictionaryString = null;
            this.prefix = SignedXml.DefaultPrefix;
        }

        public string Algorithm
        {
            get { return this.algorithm; }
            set { this.algorithm = value; }
        }

        public XmlDictionaryString AlgorithmDictionaryString
        {
            get { return this.algorithmDictionaryString; }
            set { this.algorithmDictionaryString = value; }
        }

        public void ReadFrom(XmlDictionaryReader reader, DictionaryManager dictionaryManager)
        {
            reader.MoveToStartElement(this.elementName, dictionaryManager.XmlSignatureDictionary.Namespace);
            this.prefix = reader.Prefix;
            bool isEmptyElement = reader.IsEmptyElement;
            this.algorithm = reader.GetAttribute(dictionaryManager.XmlSignatureDictionary.Algorithm, null);
            if (this.algorithm == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(
                    SR.GetString(SR.RequiredAttributeMissing, dictionaryManager.XmlSignatureDictionary.Algorithm, this.elementName)));
            }
            reader.Read();
            reader.MoveToContent();

            if (!isEmptyElement)
            {
                reader.MoveToContent();
                reader.ReadEndElement();
            }
        }

        public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            writer.WriteStartElement(this.prefix, this.elementName, dictionaryManager.XmlSignatureDictionary.Namespace);
            writer.WriteStartAttribute(dictionaryManager.XmlSignatureDictionary.Algorithm, null);
            if (this.algorithmDictionaryString != null)
            {
                writer.WriteString(this.algorithmDictionaryString);
            }
            else
            {
                writer.WriteString(this.algorithm);
            }
            writer.WriteEndAttribute();
            writer.WriteEndElement();
        }
    }
}
