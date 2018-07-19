//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Runtime;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Xml;
    using ExclusiveCanonicalizationTransform = System.IdentityModel.ExclusiveCanonicalizationTransform;
    using HashStream = System.IdentityModel.HashStream;
    using IPrefixGenerator = System.IdentityModel.IPrefixGenerator;
    using ISecurityElement = System.IdentityModel.ISecurityElement;
    using ISignatureValueSecurityElement = System.IdentityModel.ISignatureValueSecurityElement;
    using PreDigestedSignedInfo = System.IdentityModel.PreDigestedSignedInfo;
    using Reference = System.IdentityModel.Reference;
    using SignedInfo = System.IdentityModel.SignedInfo;
    using SignedXml = System.IdentityModel.SignedXml;
    using StandardSignedInfo = System.IdentityModel.StandardSignedInfo;
    using System.ServiceModel.Security.Tokens;

    class WSSecurityOneDotZeroSendSecurityHeader : SendSecurityHeader
    {
        HashStream hashStream;
        PreDigestedSignedInfo signedInfo;
        SignedXml signedXml;
        SecurityKey signatureKey;
        MessagePartSpecification effectiveSignatureParts;

        SymmetricAlgorithm encryptingSymmetricAlgorithm;
        ReferenceList referenceList;
        SecurityKeyIdentifier encryptionKeyIdentifier;

        bool hasSignedEncryptedMessagePart;

        // For Transport Secrity we have to sign the 'To' header with the 
        // supporting tokens.
        byte[] toHeaderHash = null;
        string toHeaderId = null;

        public WSSecurityOneDotZeroSendSecurityHeader(Message message, string actor, bool mustUnderstand, bool relay,
            SecurityStandardsManager standardsManager,
            SecurityAlgorithmSuite algorithmSuite,
            MessageDirection direction)
            : base(message, actor, mustUnderstand, relay, standardsManager, algorithmSuite, direction)
        {
        }

        protected string EncryptionAlgorithm
        {
            get { return this.AlgorithmSuite.DefaultEncryptionAlgorithm; }
        }

        protected XmlDictionaryString EncryptionAlgorithmDictionaryString
        {
            get { return this.AlgorithmSuite.DefaultEncryptionAlgorithmDictionaryString; }
        }

        protected override bool HasSignedEncryptedMessagePart
        {
            get
            {
                return this.hasSignedEncryptedMessagePart;
            }
        }

        void AddEncryptionReference(MessageHeader header, string headerId, IPrefixGenerator prefixGenerator, bool sign,
            out MemoryStream plainTextStream, out string encryptedDataId)
        {
            plainTextStream = new MemoryStream();
            XmlDictionaryWriter encryptingWriter = XmlDictionaryWriter.CreateTextWriter(plainTextStream);
            if (sign)
            {
                AddSignatureReference(header, headerId, prefixGenerator, encryptingWriter);
            }
            else
            {
                header.WriteHeader(encryptingWriter, this.Version);
                encryptingWriter.Flush();
            }
            encryptedDataId = this.GenerateId();
            referenceList.AddReferredId(encryptedDataId);
        }

        void AddSignatureReference(SecurityToken token, int position, SecurityTokenAttachmentMode mode)
        {
            SecurityKeyIdentifierClause keyIdentifierClause = null;
            bool strTransformEnabled = this.ShouldUseStrTransformForToken(token, position, mode, out keyIdentifierClause);
            AddTokenSignatureReference(token, keyIdentifierClause, strTransformEnabled);
        }

        void AddPrimaryTokenSignatureReference(SecurityToken token, SecurityTokenParameters securityTokenParameters)
        {
            // Currently we only support signing the primary token if the primary token is an issued token and protectTokens knob is set to true.
            // We will get rid of the below check when we support all token types.
            IssuedSecurityTokenParameters istp = securityTokenParameters as IssuedSecurityTokenParameters;
            if (istp == null)
            {
                return;
            }

            bool strTransformEnabled = istp != null && istp.UseStrTransform;
            SecurityKeyIdentifierClause keyIdentifierClause = null;
            // Only if the primary token is included in the message that we sign it because WCF at present does not resolve externally referenced tokens. 
            // This means in the server's response 
            if (ShouldSerializeToken(securityTokenParameters, this.MessageDirection))
            {
                if (strTransformEnabled)
                {
                    keyIdentifierClause = securityTokenParameters.CreateKeyIdentifierClause(token, GetTokenReferenceStyle(securityTokenParameters));
                }
                AddTokenSignatureReference(token, keyIdentifierClause, strTransformEnabled);
            }
        }

        // Given a token and useStarTransform value this method adds apporopriate reference accordingly.
        // 1. If strTransform is disabled, it adds a reference to the token's id. 
        // 2. Else if strtransform is enabled it adds a reference the security token's keyIdentifier's id.
        void AddTokenSignatureReference(SecurityToken token, SecurityKeyIdentifierClause keyIdentifierClause, bool strTransformEnabled)
        {
            if (!strTransformEnabled && token.Id == null)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.ElementToSignMustHaveId)), this.Message);
            }

            HashStream hashStream = TakeHashStream();
            XmlDictionaryWriter utf8Writer = TakeUtf8Writer();
            utf8Writer.StartCanonicalization(hashStream, false, null);
            this.StandardsManager.SecurityTokenSerializer.WriteToken(utf8Writer, token);
            utf8Writer.EndCanonicalization();

            if (strTransformEnabled)
            {
                if (keyIdentifierClause != null)
                {
                    if (String.IsNullOrEmpty(keyIdentifierClause.Id))
                        keyIdentifierClause.Id = SecurityUniqueId.Create().Value;
                    this.ElementContainer.MapSecurityTokenToStrClause(token, keyIdentifierClause);
                    this.signedInfo.AddReference(keyIdentifierClause.Id, hashStream.FlushHashAndGetValue(), true);
                }
                else
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TokenManagerCannotCreateTokenReference)), this.Message);
            }
            else
                this.signedInfo.AddReference(token.Id, hashStream.FlushHashAndGetValue());
        }

        void AddSignatureReference(SendSecurityHeaderElement[] elements)
        {
            if (elements != null)
            {
                for (int i = 0; i < elements.Length; ++i)
                {
                    SecurityKeyIdentifierClause keyIdentifierClause = null;
                    TokenElement signedEncryptedTokenElement = elements[i].Item as TokenElement;

                    // signedEncryptedTokenElement can either be a TokenElement ( in SignThenEncrypt case) or EncryptedData ( in !SignThenEncryptCase)
                    // STR-Transform does not make sense in !SignThenEncrypt case .
                    // note: signedEncryptedTokenElement can also be SignatureConfirmation but we do not care about it here.
                    bool useStrTransform = signedEncryptedTokenElement != null
                                           && SignThenEncrypt
                                           && this.ShouldUseStrTransformForToken(signedEncryptedTokenElement.Token,
                                                                                 i,
                                                                                 SecurityTokenAttachmentMode.SignedEncrypted,
                                                                                 out keyIdentifierClause);

                    if (!useStrTransform && elements[i].Id == null)
                    {
                        throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.ElementToSignMustHaveId)), this.Message);
                    }

                    HashStream hashStream = TakeHashStream();
                    XmlDictionaryWriter utf8Writer = TakeUtf8Writer();
                    utf8Writer.StartCanonicalization(hashStream, false, null);
                    elements[i].Item.WriteTo(utf8Writer, ServiceModelDictionaryManager.Instance);
                    utf8Writer.EndCanonicalization();

                    if (useStrTransform)
                    {
                        if (keyIdentifierClause != null)
                        {
                            if (String.IsNullOrEmpty(keyIdentifierClause.Id))
                                keyIdentifierClause.Id = SecurityUniqueId.Create().Value;

                            this.ElementContainer.MapSecurityTokenToStrClause(signedEncryptedTokenElement.Token, keyIdentifierClause);
                            this.signedInfo.AddReference(keyIdentifierClause.Id, hashStream.FlushHashAndGetValue(), true);
                        }
                        else
                            throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TokenManagerCannotCreateTokenReference)), this.Message);
                    }
                    else
                        this.signedInfo.AddReference(elements[i].Id, hashStream.FlushHashAndGetValue());
                }
            }
        }

        void AddSignatureReference(SecurityToken[] tokens, SecurityTokenAttachmentMode mode)
        {
            if (tokens != null)
            {
                for (int i = 0; i < tokens.Length; ++i)
                {
                    AddSignatureReference(tokens[i], i, mode);
                }
            }
        }

        string GetSignatureHash(MessageHeader header, string headerId, IPrefixGenerator prefixGenerator, XmlDictionaryWriter writer, out byte[] hash)
        {
            HashStream hashStream = TakeHashStream();
            XmlDictionaryWriter effectiveWriter;
            XmlBuffer canonicalBuffer = null;

            if (writer.CanCanonicalize)
            {
                effectiveWriter = writer;
            }
            else
            {
                canonicalBuffer = new XmlBuffer(int.MaxValue);
                effectiveWriter = canonicalBuffer.OpenSection(XmlDictionaryReaderQuotas.Max);
            }

            effectiveWriter.StartCanonicalization(hashStream, false, null);

            header.WriteStartHeader(effectiveWriter, this.Version);
            if (headerId == null)
            {
                headerId = GenerateId();
                this.StandardsManager.IdManager.WriteIdAttribute(effectiveWriter, headerId);
            }
            header.WriteHeaderContents(effectiveWriter, this.Version);
            effectiveWriter.WriteEndElement();
            effectiveWriter.EndCanonicalization();
            effectiveWriter.Flush();

            if (!ReferenceEquals(effectiveWriter, writer))
            {
                Fx.Assert(canonicalBuffer != null, "Canonical buffer cannot be null.");
                canonicalBuffer.CloseSection();
                canonicalBuffer.Close();
                XmlDictionaryReader dicReader = canonicalBuffer.GetReader(0);
                writer.WriteNode(dicReader, false);
                dicReader.Close();
            }

            hash = hashStream.FlushHashAndGetValue();

            return headerId;
        }

        void AddSignatureReference(MessageHeader header, string headerId, IPrefixGenerator prefixGenerator, XmlDictionaryWriter writer)
        {
            byte[] hashValue;
            headerId = GetSignatureHash(header, headerId, prefixGenerator, writer, out hashValue);
            this.signedInfo.AddReference(headerId, hashValue);
        }

        void ApplySecurityAndWriteHeader(MessageHeader header, string headerId, XmlDictionaryWriter writer, IPrefixGenerator prefixGenerator)
        {
            if (!this.RequireMessageProtection && this.ShouldSignToHeader)
            {
                if ((header.Name == XD.AddressingDictionary.To.Value) &&
                    (header.Namespace == this.Message.Version.Addressing.Namespace))
                {
                    if (this.toHeaderHash == null)
                    {
                        byte[] headerHash;
                        headerId = GetSignatureHash(header, headerId, prefixGenerator, writer, out headerHash);
                        this.toHeaderHash = headerHash;
                        this.toHeaderId = headerId;
                    }
                    else
                        // More than one 'To' header is specified in the message.
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TransportSecuredMessageHasMoreThanOneToHeader)));

                    return;
                }
            }

            MessagePartProtectionMode protectionMode = GetProtectionMode(header);
            MemoryStream plainTextStream;
            string encryptedDataId;
            switch (protectionMode)
            {
                case MessagePartProtectionMode.None:
                    header.WriteHeader(writer, this.Version);
                    return;
                case MessagePartProtectionMode.Sign:
                    AddSignatureReference(header, headerId, prefixGenerator, writer);
                    return;
                case MessagePartProtectionMode.SignThenEncrypt:
                    AddEncryptionReference(header, headerId, prefixGenerator, true, out plainTextStream, out encryptedDataId);
                    EncryptAndWriteHeader(header, encryptedDataId, plainTextStream, writer);
                    this.hasSignedEncryptedMessagePart = true;
                    return;
                case MessagePartProtectionMode.Encrypt:
                    AddEncryptionReference(header, headerId, prefixGenerator, false, out plainTextStream, out encryptedDataId);
                    EncryptAndWriteHeader(header, encryptedDataId, plainTextStream, writer);
                    return;
                case MessagePartProtectionMode.EncryptThenSign:
                    AddEncryptionReference(header, headerId, prefixGenerator, false, out plainTextStream, out encryptedDataId);
                    EncryptedHeader encryptedHeader = EncryptHeader(
                        header, this.encryptingSymmetricAlgorithm, this.encryptionKeyIdentifier, this.Version, encryptedDataId, plainTextStream);
                    AddSignatureReference(encryptedHeader, encryptedDataId, prefixGenerator, writer);
                    return;
                default:
                    Fx.Assert("Invalid MessagePartProtectionMode");
                    return;
            }
        }

        public override void ApplySecurityAndWriteHeaders(MessageHeaders headers, XmlDictionaryWriter writer, IPrefixGenerator prefixGenerator)
        {
            string[] headerIds;
            if (this.RequireMessageProtection || this.ShouldSignToHeader)
            {
                headerIds = headers.GetHeaderAttributes(UtilityStrings.IdAttribute,
                    this.StandardsManager.IdManager.DefaultIdNamespaceUri);
            }
            else
            {
                headerIds = null;
            }
            for (int i = 0; i < headers.Count; i++)
            {
                MessageHeader header = headers.GetMessageHeader(i);
                if (this.Version.Addressing == AddressingVersion.None && header.Namespace == AddressingVersion.None.Namespace)
                {
                    continue;
                }

                if (header != this)
                {
                    ApplySecurityAndWriteHeader(header, headerIds == null ? null : headerIds[i], writer, prefixGenerator);
                }
            }
        }

        static bool CanCanonicalizeAndFragment(XmlDictionaryWriter writer)
        {
            if (!writer.CanCanonicalize)
            {
                return false;
            }
            IFragmentCapableXmlDictionaryWriter fragmentingWriter = writer as IFragmentCapableXmlDictionaryWriter;
            return fragmentingWriter != null && fragmentingWriter.CanFragment;
        }

        public override void ApplyBodySecurity(XmlDictionaryWriter writer, IPrefixGenerator prefixGenerator)
        {
            SecurityAppliedMessage message = this.SecurityAppliedMessage;
            EncryptedData encryptedData;
            HashStream hashStream;
            switch (message.BodyProtectionMode)
            {
                case MessagePartProtectionMode.None:
                    return;
                case MessagePartProtectionMode.Sign:
                    hashStream = TakeHashStream();
                    if (CanCanonicalizeAndFragment(writer))
                    {
                        message.WriteBodyToSignWithFragments(hashStream, false, null, writer);
                    }
                    else
                    {
                        message.WriteBodyToSign(hashStream);
                    }
                    this.signedInfo.AddReference(message.BodyId, hashStream.FlushHashAndGetValue());
                    return;
                case MessagePartProtectionMode.SignThenEncrypt:
                    hashStream = TakeHashStream();
                    encryptedData = CreateEncryptedDataForBody();
                    if (CanCanonicalizeAndFragment(writer))
                    {
                        message.WriteBodyToSignThenEncryptWithFragments(hashStream, false, null, encryptedData, this.encryptingSymmetricAlgorithm, writer);
                    }
                    else
                    {
                        message.WriteBodyToSignThenEncrypt(hashStream, encryptedData, this.encryptingSymmetricAlgorithm);
                    }
                    this.signedInfo.AddReference(message.BodyId, hashStream.FlushHashAndGetValue());
                    this.referenceList.AddReferredId(encryptedData.Id);
                    this.hasSignedEncryptedMessagePart = true;
                    return;
                case MessagePartProtectionMode.Encrypt:
                    encryptedData = CreateEncryptedDataForBody();
                    message.WriteBodyToEncrypt(encryptedData, this.encryptingSymmetricAlgorithm);
                    this.referenceList.AddReferredId(encryptedData.Id);
                    return;
                case MessagePartProtectionMode.EncryptThenSign:
                    hashStream = TakeHashStream();
                    encryptedData = CreateEncryptedDataForBody();
                    message.WriteBodyToEncryptThenSign(hashStream, encryptedData, this.encryptingSymmetricAlgorithm);
                    this.signedInfo.AddReference(message.BodyId, hashStream.FlushHashAndGetValue());
                    this.referenceList.AddReferredId(encryptedData.Id);
                    return;
                default:
                    Fx.Assert("Invalid MessagePartProtectionMode");
                    return;
            }
        }

        protected static MemoryStream CaptureToken(SecurityToken token, SecurityStandardsManager serializer)
        {
            MemoryStream stream = new MemoryStream();
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream);
            serializer.SecurityTokenSerializer.WriteToken(writer, token);
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        protected static MemoryStream CaptureSecurityElement(ISecurityElement element)
        {
            MemoryStream stream = new MemoryStream();
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream);
            element.WriteTo(writer, ServiceModelDictionaryManager.Instance);
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        protected override ISecurityElement CompleteEncryptionCore(
            SendSecurityHeaderElement primarySignature,
            SendSecurityHeaderElement[] basicTokens,
            SendSecurityHeaderElement[] signatureConfirmations,
            SendSecurityHeaderElement[] endorsingSignatures)
        {
            if (this.referenceList == null)
            {
                return null;
            }

            if (primarySignature != null && primarySignature.Item != null && primarySignature.MarkedForEncryption)
            {
                EncryptElement(primarySignature);
            }

            if (basicTokens != null)
            {
                for (int i = 0; i < basicTokens.Length; ++i)
                {
                    if (basicTokens[i].MarkedForEncryption)
                        EncryptElement(basicTokens[i]);
                }
            }

            if (signatureConfirmations != null)
            {
                for (int i = 0; i < signatureConfirmations.Length; ++i)
                {
                    if (signatureConfirmations[i].MarkedForEncryption)
                        EncryptElement(signatureConfirmations[i]);
                }
            }

            if (endorsingSignatures != null)
            {
                for (int i = 0; i < endorsingSignatures.Length; ++i)
                {
                    if (endorsingSignatures[i].MarkedForEncryption)
                        EncryptElement(endorsingSignatures[i]);
                }
            }

            try
            {
                return this.referenceList.DataReferenceCount > 0 ? this.referenceList : null;
            }
            finally
            {
                this.referenceList = null;
                this.encryptingSymmetricAlgorithm = null;
                this.encryptionKeyIdentifier = null;
            }
        }

        protected override ISignatureValueSecurityElement CompletePrimarySignatureCore(
            SendSecurityHeaderElement[] signatureConfirmations,
            SecurityToken[] signedEndorsingTokens,
            SecurityToken[] signedTokens,
            SendSecurityHeaderElement[] basicTokens, bool isPrimarySignature)
        {
            if (this.signedXml == null)
            {
                return null;
            }

            SecurityTimestamp timestamp = this.Timestamp;
            if (timestamp != null)
            {
                if (timestamp.Id == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.TimestampToSignHasNoId)));
                }
                HashStream hashStream = TakeHashStream();
                this.StandardsManager.WSUtilitySpecificationVersion.WriteTimestampCanonicalForm(
                    hashStream, timestamp, this.signedInfo.ResourcePool.TakeEncodingBuffer());
                signedInfo.AddReference(timestamp.Id, hashStream.FlushHashAndGetValue());
            }

            if ((this.ShouldSignToHeader) && (this.signatureKey is AsymmetricSecurityKey) && (this.Version.Addressing != AddressingVersion.None))
            {
                if (this.toHeaderHash != null)
                    signedInfo.AddReference(this.toHeaderId, this.toHeaderHash);
                else
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.TransportSecurityRequireToHeader)));
            }

            AddSignatureReference(signatureConfirmations);
            if (isPrimarySignature && this.ShouldProtectTokens)
            {
                AddPrimaryTokenSignatureReference(this.ElementContainer.SourceSigningToken, this.SigningTokenParameters);
            }

            if (this.RequireMessageProtection)
            {
                AddSignatureReference(signedEndorsingTokens, SecurityTokenAttachmentMode.SignedEndorsing);
                AddSignatureReference(signedTokens, SecurityTokenAttachmentMode.Signed);
                AddSignatureReference(basicTokens);
            }

            if (this.signedInfo.ReferenceCount == 0)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.NoPartsOfMessageMatchedPartsToSign)), this.Message);
            }
            try
            {
                this.signedXml.ComputeSignature(this.signatureKey);
                return this.signedXml;
            }
            finally
            {
                this.hashStream = null;
                this.signedInfo = null;
                this.signedXml = null;
                this.signatureKey = null;
                this.effectiveSignatureParts = null;
            }
        }

        EncryptedData CreateEncryptedData()
        {
            EncryptedData encryptedData = new EncryptedData();
            encryptedData.SecurityTokenSerializer = this.StandardsManager.SecurityTokenSerializer;
            encryptedData.KeyIdentifier = this.encryptionKeyIdentifier;
            encryptedData.EncryptionMethod = this.EncryptionAlgorithm;
            encryptedData.EncryptionMethodDictionaryString = this.EncryptionAlgorithmDictionaryString;
            return encryptedData;
        }

        EncryptedData CreateEncryptedData(MemoryStream stream, string id, bool typeElement)
        {
            EncryptedData encryptedData = CreateEncryptedData();
            encryptedData.Id = id;
            encryptedData.SetUpEncryption(this.encryptingSymmetricAlgorithm, new ArraySegment<byte>(stream.GetBuffer(), 0, (int) stream.Length));
            if (typeElement)
            {
                encryptedData.Type = EncryptedData.ElementType;
            }
            return encryptedData;
        }

        EncryptedData CreateEncryptedDataForBody()
        {
            EncryptedData encryptedData = CreateEncryptedData();
            encryptedData.Type = EncryptedData.ContentType;
            return encryptedData;
        }

        void EncryptAndWriteHeader(MessageHeader plainTextHeader, string id, MemoryStream stream, XmlDictionaryWriter writer)
        {
            EncryptedHeader encryptedHeader = EncryptHeader(
                plainTextHeader,
                this.encryptingSymmetricAlgorithm, this.encryptionKeyIdentifier, this.Version,
                id, stream);
            encryptedHeader.WriteHeader(writer, this.Version);
        }

        void EncryptElement(SendSecurityHeaderElement element)
        {
            string id = GenerateId();
            ISecurityElement encryptedElement = CreateEncryptedData(CaptureSecurityElement(element.Item), id, true);
            this.referenceList.AddReferredId(id);
            element.Replace(id, encryptedElement);
        }

        protected virtual EncryptedHeader EncryptHeader(MessageHeader plainTextHeader, SymmetricAlgorithm algorithm,
                SecurityKeyIdentifier keyIdentifier, MessageVersion version, string id, MemoryStream stream)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                SR.GetString(SR.HeaderEncryptionNotSupportedInWsSecurityJan2004, plainTextHeader.Name, plainTextHeader.Namespace)));
        }

        HashStream TakeHashStream()
        {
            HashStream hashStream = null;
            if (this.hashStream == null)
            {
                this.hashStream = hashStream = new HashStream(CryptoHelper.CreateHashAlgorithm(this.AlgorithmSuite.DefaultDigestAlgorithm));
            }
            else
            {
                hashStream = this.hashStream;;
                hashStream.Reset();
            }
            return hashStream;
        }

        XmlDictionaryWriter TakeUtf8Writer()
        {
            return this.signedInfo.ResourcePool.TakeUtf8Writer();
        }

        MessagePartProtectionMode GetProtectionMode(MessageHeader header)
        {
            if (!this.RequireMessageProtection)
            {
                return MessagePartProtectionMode.None;
            }
            bool sign = this.signedInfo != null && this.effectiveSignatureParts.IsHeaderIncluded(header);
            bool encrypt = this.referenceList != null && this.EncryptionParts.IsHeaderIncluded(header);
            return MessagePartProtectionModeHelper.GetProtectionMode(sign, encrypt, this.SignThenEncrypt);
        }

        protected override void StartEncryptionCore(SecurityToken token, SecurityKeyIdentifier keyIdentifier)
        {
            this.encryptingSymmetricAlgorithm = SecurityUtils.GetSymmetricAlgorithm(this.EncryptionAlgorithm, token);
            if (this.encryptingSymmetricAlgorithm == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(
                    SR.GetString(SR.UnableToCreateSymmetricAlgorithmFromToken, this.EncryptionAlgorithm)));
            }
            this.encryptionKeyIdentifier = keyIdentifier;
            this.referenceList = new ReferenceList();
        }

        protected override void StartPrimarySignatureCore(SecurityToken token,
            SecurityKeyIdentifier keyIdentifier,
            MessagePartSpecification signatureParts,
            bool generateTargettableSignature)
        {
            SecurityAlgorithmSuite suite = this.AlgorithmSuite;
            string canonicalizationAlgorithm = suite.DefaultCanonicalizationAlgorithm;
            XmlDictionaryString canonicalizationAlgorithmDictionaryString = suite.DefaultCanonicalizationAlgorithmDictionaryString;
            if (canonicalizationAlgorithm != SecurityAlgorithms.ExclusiveC14n)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new MessageSecurityException(SR.GetString(SR.UnsupportedCanonicalizationAlgorithm, suite.DefaultCanonicalizationAlgorithm)));
            }
            string signatureAlgorithm;
            XmlDictionaryString signatureAlgorithmDictionaryString;
            suite.GetSignatureAlgorithmAndKey(token, out signatureAlgorithm, out this.signatureKey, out signatureAlgorithmDictionaryString);
            string digestAlgorithm = suite.DefaultDigestAlgorithm;
            XmlDictionaryString digestAlgorithmDictionaryString = suite.DefaultDigestAlgorithmDictionaryString;
            this.signedInfo = new PreDigestedSignedInfo(ServiceModelDictionaryManager.Instance, canonicalizationAlgorithm, canonicalizationAlgorithmDictionaryString, digestAlgorithm, digestAlgorithmDictionaryString, signatureAlgorithm, signatureAlgorithmDictionaryString);
            this.signedXml = new SignedXml(this.signedInfo, ServiceModelDictionaryManager.Instance, this.StandardsManager.SecurityTokenSerializer);
            if (keyIdentifier != null)
            {
                this.signedXml.Signature.KeyIdentifier = keyIdentifier;
            }
            if (generateTargettableSignature)
            {
                this.signedXml.Id = GenerateId();
            }
            this.effectiveSignatureParts = signatureParts;
            this.hashStream = this.signedInfo.ResourcePool.TakeHashStream(digestAlgorithm);
        }

        protected override ISignatureValueSecurityElement CreateSupportingSignature(SecurityToken token, SecurityKeyIdentifier identifier)
        {
            StartPrimarySignatureCore(token, identifier, MessagePartSpecification.NoParts, false);
            return CompletePrimarySignatureCore(null, null, null, null, false);
        }

        protected override ISignatureValueSecurityElement CreateSupportingSignature(SecurityToken token, SecurityKeyIdentifier identifier, ISecurityElement elementToSign)
        {
            SecurityAlgorithmSuite algorithmSuite = this.AlgorithmSuite;
            string signatureAlgorithm;
            XmlDictionaryString signatureAlgorithmDictionaryString;
            SecurityKey signatureKey;
            algorithmSuite.GetSignatureAlgorithmAndKey(token, out signatureAlgorithm, out signatureKey, out signatureAlgorithmDictionaryString);
            SignedXml signedXml = new SignedXml(ServiceModelDictionaryManager.Instance, this.StandardsManager.SecurityTokenSerializer);
            SignedInfo signedInfo = signedXml.Signature.SignedInfo;
            signedInfo.CanonicalizationMethod = algorithmSuite.DefaultCanonicalizationAlgorithm;
            signedInfo.CanonicalizationMethodDictionaryString = algorithmSuite.DefaultCanonicalizationAlgorithmDictionaryString;
            signedInfo.SignatureMethod = signatureAlgorithm;
            signedInfo.SignatureMethodDictionaryString = signatureAlgorithmDictionaryString;

            if (elementToSign.Id == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ElementToSignMustHaveId)));
            }
            Reference reference = new Reference(ServiceModelDictionaryManager.Instance, "#" + elementToSign.Id, elementToSign);
            reference.DigestMethod = algorithmSuite.DefaultDigestAlgorithm;
            reference.DigestMethodDictionaryString = algorithmSuite.DefaultDigestAlgorithmDictionaryString;
            reference.AddTransform(new ExclusiveCanonicalizationTransform());
            ((StandardSignedInfo)signedInfo).AddReference(reference);

            signedXml.ComputeSignature(signatureKey);
            if (identifier != null)
            {
                signedXml.Signature.KeyIdentifier = identifier;
            }
            return signedXml;
        }

          protected override void WriteSecurityTokenReferencyEntry(XmlDictionaryWriter writer, SecurityToken securityToken, SecurityTokenParameters securityTokenParameters)
        {
            SecurityKeyIdentifierClause keyIdentifierClause = null;

            // Given a token this method writes its corresponding security token reference entry in the security header 
            // 1. If the token parameters is an issuedSecurityTokenParamter 
            // 2. If UseStrTransform is enabled on it.

            IssuedSecurityTokenParameters issuedSecurityTokenParameters = securityTokenParameters as IssuedSecurityTokenParameters;
            if (issuedSecurityTokenParameters == null || !issuedSecurityTokenParameters.UseStrTransform)
                return;

            if (this.ElementContainer.TryGetIdentifierClauseFromSecurityToken(securityToken, out keyIdentifierClause))
            {
                if (keyIdentifierClause != null && !String.IsNullOrEmpty(keyIdentifierClause.Id))
                {
                    WrappedXmlDictionaryWriter wrappedLocalWriter = new WrappedXmlDictionaryWriter(writer, keyIdentifierClause.Id);
                    this.StandardsManager.SecurityTokenSerializer.WriteKeyIdentifierClause(wrappedLocalWriter, keyIdentifierClause);
                }
                else
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TokenManagerCannotCreateTokenReference)), this.Message);
            }
        }
    }

    class WrappedXmlDictionaryWriter : XmlDictionaryWriter
    {
        XmlDictionaryWriter innerWriter;
        int index;
        bool insertId;
        bool isStrReferenceElement;
        string id;

        public WrappedXmlDictionaryWriter(XmlDictionaryWriter writer, string id)
        {
            this.innerWriter = writer;
            this.index = 0;
            this.insertId = false;
            this.isStrReferenceElement = false;
            this.id = id;
        }

        public override void WriteStartAttribute(string prefix, string localName, string namespaceUri)
        {
            if (isStrReferenceElement && this.insertId && localName == XD.UtilityDictionary.IdAttribute.Value)
            {
                // This means the serializer is already writing the Id out, so we don't write it again.
                this.insertId = false;
            }
            this.innerWriter.WriteStartAttribute(prefix, localName, namespaceUri);
        }

        public override void WriteStartElement(string prefix, string localName, string namespaceUri)
        {
            if (isStrReferenceElement && this.insertId)
            {
                if (id != null)
                {
                    this.innerWriter.WriteAttributeString(XD.UtilityDictionary.Prefix.Value, XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace, id);
                }

                isStrReferenceElement = false;
                this.insertId = false;
            }

            index++;

            if (index == 1 && localName == XD.SecurityJan2004Dictionary.SecurityTokenReference.Value)
            {
                this.insertId = true;
                isStrReferenceElement = true;
            }

            this.innerWriter.WriteStartElement(prefix, localName, namespaceUri);
        }

        // Below methods simply call into innerWritter
        public override void Close()
        {
            this.innerWriter.Close();
        }

        public override void Flush()
        {
            this.innerWriter.Flush();
        }

        public override string LookupPrefix(string ns)
        {
            return this.innerWriter.LookupPrefix(ns);
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            this.innerWriter.WriteBase64(buffer, index, count);
        }

        public override void WriteCData(string text)
        {
            this.innerWriter.WriteCData(text);
        }

        public override void WriteCharEntity(char ch)
        {
            this.innerWriter.WriteCharEntity(ch);
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            this.innerWriter.WriteChars(buffer, index, count);
        }

        public override void WriteComment(string text)
        {
            this.innerWriter.WriteComment(text);
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            this.innerWriter.WriteDocType(name, pubid, sysid, subset);
        }

        public override void WriteEndAttribute()
        {
            this.innerWriter.WriteEndAttribute();
        }

        public override void WriteEndDocument()
        {
            this.innerWriter.WriteEndDocument();
        }

        public override void WriteEndElement()
        {
            this.innerWriter.WriteEndElement();
        }

        public override void WriteEntityRef(string name)
        {
            this.innerWriter.WriteEntityRef(name);
        }

        public override void WriteFullEndElement()
        {
            this.innerWriter.WriteFullEndElement();
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            this.innerWriter.WriteProcessingInstruction(name, text);
        }

        public override void WriteRaw(string data)
        {
            this.innerWriter.WriteRaw(data);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            this.innerWriter.WriteRaw(buffer, index, count);
        }

        public override void WriteStartDocument(bool standalone)
        {
            this.innerWriter.WriteStartDocument(standalone);
        }

        public override void WriteStartDocument()
        {
            this.innerWriter.WriteStartDocument();
        }

        public override WriteState WriteState
        {
            get { return this.innerWriter.WriteState; }
        }

        public override void WriteString(string text)
        {
            this.innerWriter.WriteString(text);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            this.innerWriter.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public override void WriteWhitespace(string ws)
        {
            this.innerWriter.WriteWhitespace(ws);
        }
    }
}
