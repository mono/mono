//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security.Cryptography;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;
    using Reference = System.IdentityModel.Reference;
    using SignedInfo = System.IdentityModel.SignedInfo;
    using SignedXml = System.IdentityModel.SignedXml;
    using StandardSignedInfo = System.IdentityModel.StandardSignedInfo;

    class WSSecurityOneDotZeroReceiveSecurityHeader : ReceiveSecurityHeader
    {
        WrappedKeySecurityToken pendingDecryptionToken;
        ReferenceList pendingReferenceList;
        SignedXml pendingSignature;
        List<string> earlyDecryptedDataReferences;

        public WSSecurityOneDotZeroReceiveSecurityHeader(Message message, string actor, bool mustUnderstand, bool relay,
            SecurityStandardsManager standardsManager,
            SecurityAlgorithmSuite algorithmSuite,
            int headerIndex,
            MessageDirection transferDirection)
            : base(message, actor, mustUnderstand, relay, standardsManager, algorithmSuite, headerIndex, transferDirection)
        {
        }

        protected static SymmetricAlgorithm CreateDecryptionAlgorithm(SecurityToken token, string encryptionMethod, SecurityAlgorithmSuite suite)
        {
            if (encryptionMethod == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(
                    SR.GetString(SR.EncryptionMethodMissingInEncryptedData)));
            }
            suite.EnsureAcceptableEncryptionAlgorithm(encryptionMethod);
            SymmetricSecurityKey symmetricSecurityKey = SecurityUtils.GetSecurityKey<SymmetricSecurityKey>(token);
            if (symmetricSecurityKey == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(
                    SR.GetString(SR.TokenCannotCreateSymmetricCrypto, token)));
            }
            suite.EnsureAcceptableDecryptionSymmetricKeySize(symmetricSecurityKey, token);
            SymmetricAlgorithm algorithm = symmetricSecurityKey.GetSymmetricAlgorithm(encryptionMethod);
            if (algorithm == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(
                    SR.GetString(SR.UnableToCreateSymmetricAlgorithmFromToken, encryptionMethod)));
            }

            return algorithm;
        }

        void DecryptBody(XmlDictionaryReader bodyContentReader, SecurityToken token)
        {
            EncryptedData bodyXml = new EncryptedData();
            bodyXml.ShouldReadXmlReferenceKeyInfoClause = this.MessageDirection == MessageDirection.Output;
            bodyXml.SecurityTokenSerializer = this.StandardsManager.SecurityTokenSerializer;
            bodyXml.ReadFrom(bodyContentReader, MaxReceivedMessageSize);
            if (!bodyContentReader.EOF && bodyContentReader.NodeType != XmlNodeType.EndElement)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new FormatException(SR.GetString(SR.BadEncryptedBody)));
            }
            if (token == null)
            {
                token = ResolveKeyIdentifier(bodyXml.KeyIdentifier, this.PrimaryTokenResolver, false);
            }
            RecordEncryptionToken(token);
            using (SymmetricAlgorithm algorithm = CreateDecryptionAlgorithm(token, bodyXml.EncryptionMethod, this.AlgorithmSuite))
            {
                bodyXml.SetUpDecryption(algorithm);
                this.SecurityVerifiedMessage.SetDecryptedBody(bodyXml.GetDecryptedBuffer());
            }
        }

        protected virtual DecryptedHeader DecryptHeader(XmlDictionaryReader reader, WrappedKeySecurityToken wrappedKeyToken)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                new MessageSecurityException(SR.GetString(SR.HeaderDecryptionNotSupportedInWsSecurityJan2004)));
        }

        protected override byte[] DecryptSecurityHeaderElement(
            EncryptedData encryptedData, WrappedKeySecurityToken wrappedKeyToken, out SecurityToken encryptionToken)
        {
            if ((encryptedData.KeyIdentifier != null) || (wrappedKeyToken == null))
            {
                // The EncryptedData might have a KeyInfo inside it. Try resolving the SecurityKeyIdentifier. 
                encryptionToken = ResolveKeyIdentifier(encryptedData.KeyIdentifier, this.CombinedPrimaryTokenResolver, false);
                if (wrappedKeyToken != null && wrappedKeyToken.ReferenceList != null && encryptedData.HasId && wrappedKeyToken.ReferenceList.ContainsReferredId(encryptedData.Id) && (wrappedKeyToken != encryptionToken))
                {
                    // We have a EncryptedKey with a ReferenceList inside it. This would mean that 
                    // all the EncryptedData pointed by the ReferenceList should be encrypted only
                    // by this key. The individual EncryptedData elements if containing a KeyInfo
                    // clause should point back to the same EncryptedKey token.
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.EncryptedKeyWasNotEncryptedWithTheRequiredEncryptingToken, wrappedKeyToken)));
                }
            }
            else
            {
                encryptionToken = wrappedKeyToken;
            }
            using (SymmetricAlgorithm algorithm = CreateDecryptionAlgorithm(encryptionToken, encryptedData.EncryptionMethod, this.AlgorithmSuite))
            {
                encryptedData.SetUpDecryption(algorithm);
                return encryptedData.GetDecryptedBuffer();
            }
        }

        protected override WrappedKeySecurityToken DecryptWrappedKey(XmlDictionaryReader reader)
        {
            if (TD.WrappedKeyDecryptionStartIsEnabled())
            {
                TD.WrappedKeyDecryptionStart(this.EventTraceActivity);
            }

            WrappedKeySecurityToken token = (WrappedKeySecurityToken)this.StandardsManager.SecurityTokenSerializer.ReadToken(
                reader, this.PrimaryTokenResolver);
            this.AlgorithmSuite.EnsureAcceptableKeyWrapAlgorithm(token.WrappingAlgorithm, token.WrappingSecurityKey is AsymmetricSecurityKey);

            if (TD.WrappedKeyDecryptionSuccessIsEnabled())
            {
                TD.WrappedKeyDecryptionSuccess(this.EventTraceActivity);
            }
            return token;
        }

        bool EnsureDigestValidityIfIdMatches(
            SignedInfo signedInfo,
            string id, XmlDictionaryReader reader, bool doSoapAttributeChecks,
            MessagePartSpecification signatureParts, MessageHeaderInfo info, bool checkForTokensAtHeaders)
        {
            if (signedInfo == null)
            {
                return false;
            }
            if (doSoapAttributeChecks)
            {
                VerifySoapAttributeMatchForHeader(info, signatureParts, reader);
            }

            bool signed = false;
            bool isRecognizedSecurityToken = checkForTokensAtHeaders && this.StandardsManager.SecurityTokenSerializer.CanReadToken(reader);

            try
            {
                signed = signedInfo.EnsureDigestValidityIfIdMatches(id, reader);
            }
            catch (CryptographicException exception)
            {
                //
                // Wrap the crypto exception here so that the perf couter can be updated correctly
                //
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.FailedSignatureVerification), exception));
            }

            if (signed && isRecognizedSecurityToken)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.SecurityTokenFoundOutsideSecurityHeader, info.Namespace, info.Name)));
            }

            return signed;
        }

        protected override void ExecuteMessageProtectionPass(bool hasAtLeastOneSupportingTokenExpectedToBeSigned)
        {
            SignatureTargetIdManager idManager = this.StandardsManager.IdManager;
            MessagePartSpecification encryptionParts = this.RequiredEncryptionParts ?? MessagePartSpecification.NoParts;
            MessagePartSpecification signatureParts = this.RequiredSignatureParts ?? MessagePartSpecification.NoParts;

            bool checkForTokensAtHeaders = hasAtLeastOneSupportingTokenExpectedToBeSigned;
            bool doSoapAttributeChecks = !signatureParts.IsBodyIncluded;
            bool encryptBeforeSign = this.EncryptBeforeSignMode;
            SignedInfo signedInfo = this.pendingSignature != null ? this.pendingSignature.Signature.SignedInfo : null;

            SignatureConfirmations signatureConfirmations = this.GetSentSignatureConfirmations();
            if (signatureConfirmations != null && signatureConfirmations.Count > 0 && signatureConfirmations.IsMarkedForEncryption)
            {
                // If Signature Confirmations are encrypted then the signature should
                // be encrypted as well.
                this.VerifySignatureEncryption();
            }

            MessageHeaders headers = this.SecurityVerifiedMessage.Headers;
            XmlDictionaryReader reader = this.SecurityVerifiedMessage.GetReaderAtFirstHeader();

            bool atLeastOneHeaderOrBodyEncrypted = false;

            for (int i = 0; i < headers.Count; i++)
            {
                if (reader.NodeType != XmlNodeType.Element)
                {
                    reader.MoveToContent();
                }

                if (i == this.HeaderIndex)
                {
                    reader.Skip();
                    continue;
                }

                bool isHeaderEncrypted = false;

                string id = idManager.ExtractId(reader);

                if (id != null)
                {
                    isHeaderEncrypted = TryDeleteReferenceListEntry(id);
                }

                if (!isHeaderEncrypted && reader.IsStartElement(SecurityXXX2005Strings.EncryptedHeader, SecurityXXX2005Strings.Namespace))
                {                    
                    XmlDictionaryReader localreader = headers.GetReaderAtHeader(i);
                    localreader.ReadStartElement(SecurityXXX2005Strings.EncryptedHeader, SecurityXXX2005Strings.Namespace);

                    if (localreader.IsStartElement(EncryptedData.ElementName, XD.XmlEncryptionDictionary.Namespace))
                    {
                        string encryptedDataId = localreader.GetAttribute(XD.XmlEncryptionDictionary.Id, null);

                        if (encryptedDataId != null && TryDeleteReferenceListEntry(encryptedDataId))
                        {
                            isHeaderEncrypted = true;
                        }
                    }                   
                }

                this.ElementManager.VerifyUniquenessAndSetHeaderId(id, i);

                MessageHeaderInfo info = headers[i];

                if (!isHeaderEncrypted && encryptionParts.IsHeaderIncluded(info.Name, info.Namespace))
                {
                    this.SecurityVerifiedMessage.OnUnencryptedPart(info.Name, info.Namespace);
                }

                bool headerSigned;
                if ((!isHeaderEncrypted || encryptBeforeSign) && id != null)
                {
                    headerSigned = EnsureDigestValidityIfIdMatches(signedInfo, id, reader, doSoapAttributeChecks, signatureParts, info, checkForTokensAtHeaders);
                }
                else
                {
                    headerSigned = false;
                }

                if (isHeaderEncrypted)
                {
                    XmlDictionaryReader decryptionReader = headerSigned ? headers.GetReaderAtHeader(i) : reader;
                    DecryptedHeader decryptedHeader = DecryptHeader(decryptionReader, this.pendingDecryptionToken);
                    info = decryptedHeader;
                    id = decryptedHeader.Id;
                    this.ElementManager.VerifyUniquenessAndSetDecryptedHeaderId(id, i);
                    headers.ReplaceAt(i, decryptedHeader);
                    if (!ReferenceEquals(decryptionReader, reader))
                    {
                        decryptionReader.Close();
                    }

                    if (!encryptBeforeSign && id != null)
                    {
                        XmlDictionaryReader decryptedHeaderReader = decryptedHeader.GetHeaderReader();
                        headerSigned = EnsureDigestValidityIfIdMatches(signedInfo, id, decryptedHeaderReader, doSoapAttributeChecks, signatureParts, info, checkForTokensAtHeaders);
                        decryptedHeaderReader.Close();
                    }
                }

                if (!headerSigned && signatureParts.IsHeaderIncluded(info.Name, info.Namespace))
                {
                    this.SecurityVerifiedMessage.OnUnsignedPart(info.Name, info.Namespace);
                }

                if (headerSigned && isHeaderEncrypted)
                {
                    // We have a header that is signed and encrypted. So the accompanying primary signature
                    // should be encrypted as well.
                    this.VerifySignatureEncryption();
                }

                if (isHeaderEncrypted && !headerSigned)
                {
                    // We require all encrypted headers (outside the security header) to be signed.
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.EncryptedHeaderNotSigned, info.Name, info.Namespace)));
                }

                if (!headerSigned && !isHeaderEncrypted)
                {
                    reader.Skip();
                }

                atLeastOneHeaderOrBodyEncrypted |= isHeaderEncrypted;
            }

            reader.ReadEndElement();

            if (reader.NodeType != XmlNodeType.Element)
            {
                reader.MoveToContent();
            }

            string bodyId = idManager.ExtractId(reader);
            this.ElementManager.VerifyUniquenessAndSetBodyId(bodyId);
            this.SecurityVerifiedMessage.SetBodyPrefixAndAttributes(reader);

            bool expectBodyEncryption = encryptionParts.IsBodyIncluded || HasPendingDecryptionItem();

            bool bodySigned;
            if ((!expectBodyEncryption || encryptBeforeSign) && bodyId != null)
            {
                bodySigned = EnsureDigestValidityIfIdMatches(signedInfo, bodyId, reader, false, null, null, false);
            }
            else
            {
                bodySigned = false;
            }

            bool bodyEncrypted;
            if (expectBodyEncryption)
            {
                XmlDictionaryReader bodyReader = bodySigned ? this.SecurityVerifiedMessage.CreateFullBodyReader() : reader;
                bodyReader.ReadStartElement();
                string bodyContentId = idManager.ExtractId(bodyReader);
                this.ElementManager.VerifyUniquenessAndSetBodyContentId(bodyContentId);
                bodyEncrypted = bodyContentId != null && TryDeleteReferenceListEntry(bodyContentId);
                if (bodyEncrypted)
                {
                    DecryptBody(bodyReader, this.pendingDecryptionToken);
                }
                if (!ReferenceEquals(bodyReader, reader))
                {
                    bodyReader.Close();
                }
                if (!encryptBeforeSign && signedInfo != null && signedInfo.HasUnverifiedReference(bodyId))
                {
                    bodyReader = this.SecurityVerifiedMessage.CreateFullBodyReader();
                    bodySigned = EnsureDigestValidityIfIdMatches(signedInfo, bodyId, bodyReader, false, null, null, false);
                    bodyReader.Close();
                }
            }
            else
            {
                bodyEncrypted = false;
            }

            if (bodySigned && bodyEncrypted)
            {
                this.VerifySignatureEncryption();
            }

            reader.Close();

            if (this.pendingSignature != null)
            {
                this.pendingSignature.CompleteSignatureVerification();
                this.pendingSignature = null;
            }
            this.pendingDecryptionToken = null;
            atLeastOneHeaderOrBodyEncrypted |= bodyEncrypted;

            if (!bodySigned && signatureParts.IsBodyIncluded)
            {
                this.SecurityVerifiedMessage.OnUnsignedPart(XD.MessageDictionary.Body.Value, this.Version.Envelope.Namespace);
            }

            if (!bodyEncrypted && encryptionParts.IsBodyIncluded)
            {
                this.SecurityVerifiedMessage.OnUnencryptedPart(XD.MessageDictionary.Body.Value, this.Version.Envelope.Namespace);
            }

            this.SecurityVerifiedMessage.OnMessageProtectionPassComplete(atLeastOneHeaderOrBodyEncrypted);
        }

        protected override bool IsReaderAtEncryptedData(XmlDictionaryReader reader)
        {
            bool encrypted = reader.IsStartElement(EncryptedData.ElementName, XD.XmlEncryptionDictionary.Namespace);

            if (encrypted == true)
                this.HasAtLeastOneItemInsideSecurityHeaderEncrypted = true;

            return encrypted;
        }

        protected override bool IsReaderAtEncryptedKey(XmlDictionaryReader reader)
        {
            return reader.IsStartElement(EncryptedKey.ElementName, XD.XmlEncryptionDictionary.Namespace);
        }

        protected override bool IsReaderAtReferenceList(XmlDictionaryReader reader)
        {
            return reader.IsStartElement(ReferenceList.ElementName, ReferenceList.NamespaceUri);
        }

        protected override bool IsReaderAtSignature(XmlDictionaryReader reader)
        {
            return reader.IsStartElement(XD.XmlSignatureDictionary.Signature, XD.XmlSignatureDictionary.Namespace);
        }

        protected override bool IsReaderAtSecurityTokenReference(XmlDictionaryReader reader)
        {
            return reader.IsStartElement(XD.SecurityJan2004Dictionary.SecurityTokenReference, XD.SecurityJan2004Dictionary.Namespace);
        }

        protected override void ProcessReferenceListCore(ReferenceList referenceList, WrappedKeySecurityToken wrappedKeyToken)
        {
            this.pendingReferenceList = referenceList;
            this.pendingDecryptionToken = wrappedKeyToken;
        }

        protected override ReferenceList ReadReferenceListCore(XmlDictionaryReader reader)
        {
            ReferenceList referenceList = new ReferenceList();
            referenceList.ReadFrom(reader);
            return referenceList;
        }

        protected override EncryptedData ReadSecurityHeaderEncryptedItem(XmlDictionaryReader reader, bool readXmlreferenceKeyInfoClause)
        {
            EncryptedData encryptedData = new EncryptedData();
            encryptedData.ShouldReadXmlReferenceKeyInfoClause = readXmlreferenceKeyInfoClause;
            encryptedData.SecurityTokenSerializer = this.StandardsManager.SecurityTokenSerializer;
            encryptedData.ReadFrom(reader);
            return encryptedData;
        }

        protected override SignedXml ReadSignatureCore(XmlDictionaryReader signatureReader)
        {
            SignedXml signedXml = new SignedXml(ServiceModelDictionaryManager.Instance, this.StandardsManager.SecurityTokenSerializer);
            signedXml.Signature.SignedInfo.ResourcePool = this.ResourcePool;
            signedXml.ReadFrom(signatureReader);
            return signedXml;
        }

        protected static bool TryResolveKeyIdentifier(
            SecurityKeyIdentifier keyIdentifier, SecurityTokenResolver resolver, bool isFromSignature, out SecurityToken token)
        {
            if (keyIdentifier == null)
            {
                if (isFromSignature)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.NoKeyInfoInSignatureToFindVerificationToken)));
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.NoKeyInfoInEncryptedItemToFindDecryptingToken)));
                }
            }

            return resolver.TryResolveToken(keyIdentifier, out token);
        }

        protected static SecurityToken ResolveKeyIdentifier(SecurityKeyIdentifier keyIdentifier, SecurityTokenResolver resolver, bool isFromSignature)
        {
            SecurityToken token;
            if (!TryResolveKeyIdentifier(keyIdentifier, resolver, isFromSignature, out token))
            {
                if (isFromSignature)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(
                        SR.GetString(SR.UnableToResolveKeyInfoForVerifyingSignature, keyIdentifier, resolver)));
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(
                        SR.GetString(SR.UnableToResolveKeyInfoForDecryption, keyIdentifier, resolver)));
                }
            }

            return token;
        }

        SecurityToken ResolveSignatureToken(SecurityKeyIdentifier keyIdentifier, SecurityTokenResolver resolver, bool isPrimarySignature)
        {
            SecurityToken token;
            TryResolveKeyIdentifier(keyIdentifier, resolver, true, out token);
            if (token == null && !isPrimarySignature)
            {
                // check if there is a rsa key token authenticator
                if (keyIdentifier.Count == 1)
                {
                    RsaKeyIdentifierClause rsaClause;
                    if (keyIdentifier.TryFind<RsaKeyIdentifierClause>(out rsaClause))
                    {
                        RsaSecurityTokenAuthenticator rsaAuthenticator = FindAllowedAuthenticator<RsaSecurityTokenAuthenticator>(false);
                        if (rsaAuthenticator != null)
                        {
                            token = new RsaSecurityToken(rsaClause.Rsa);
                            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = rsaAuthenticator.ValidateToken(token);
                            SupportingTokenAuthenticatorSpecification spec;
                            TokenTracker rsaTracker = GetSupportingTokenTracker(rsaAuthenticator, out spec);
                            if (rsaTracker == null)
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(SR.GetString(SR.UnknownTokenAuthenticatorUsedInTokenProcessing, rsaAuthenticator)));
                            }
                            rsaTracker.RecordToken(token);
                            SecurityTokenAuthorizationPoliciesMapping.Add(token, authorizationPolicies);
                        }
                    }
                }
            }
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(
                        SR.GetString(SR.UnableToResolveKeyInfoForVerifyingSignature, keyIdentifier, resolver)));
            }
            return token;
        }

        protected override void ReadSecurityTokenReference(XmlDictionaryReader reader)
        {
            string strId = reader.GetAttribute(XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace);
            SecurityKeyIdentifierClause strClause = this.StandardsManager.SecurityTokenSerializer.ReadKeyIdentifierClause(reader);
            if (String.IsNullOrEmpty(strClause.Id))
            {
                strClause.Id = strId;
            }

            if (!String.IsNullOrEmpty(strClause.Id))
            {
                this.ElementManager.AppendSecurityTokenReference(strClause, strClause.Id);
            }
        }

        bool HasPendingDecryptionItem()
        {
            return this.pendingReferenceList != null && this.pendingReferenceList.DataReferenceCount > 0;
        }

        protected override bool TryDeleteReferenceListEntry(string id)
        {
            return this.pendingReferenceList != null && this.pendingReferenceList.TryRemoveReferredId(id);
        }

        protected override void EnsureDecryptionComplete()
        {
            if (this.earlyDecryptedDataReferences != null)
            {
                for (int i = 0; i < this.earlyDecryptedDataReferences.Count; i++)
                {
                    if (!TryDeleteReferenceListEntry(this.earlyDecryptedDataReferences[i]))
                    {
                        throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.UnexpectedEncryptedElementInSecurityHeader)), this.Message);
                    }
                }
            }
            if (HasPendingDecryptionItem())
            {
                throw TraceUtility.ThrowHelperError(
                    new MessageSecurityException(SR.GetString(SR.UnableToResolveDataReference, this.pendingReferenceList.GetReferredId(0))), this.Message);
            }
        }

        protected override void OnDecryptionOfSecurityHeaderItemRequiringReferenceListEntry(string id)
        {
            if (!TryDeleteReferenceListEntry(id))
            {
                if (this.earlyDecryptedDataReferences == null)
                {
                    this.earlyDecryptedDataReferences = new List<string>(4);
                }
                this.earlyDecryptedDataReferences.Add(id);
            }
        }

        protected override SecurityToken VerifySignature(SignedXml signedXml, bool isPrimarySignature,
            SecurityHeaderTokenResolver resolver, object signatureTarget, string id)
        {
            if (TD.SignatureVerificationStartIsEnabled())
            {
                TD.SignatureVerificationStart(this.EventTraceActivity);
            }

            SecurityToken token = ResolveSignatureToken(signedXml.Signature.KeyIdentifier, resolver, isPrimarySignature);
            if (isPrimarySignature)
            {
                RecordSignatureToken(token);
            }
            ReadOnlyCollection<SecurityKey> keys = token.SecurityKeys;
            SecurityKey securityKey = (keys != null && keys.Count > 0) ? keys[0] : null;
            if (securityKey == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(
                    SR.GetString(SR.UnableToCreateICryptoFromTokenForSignatureVerification, token)));
            }
            this.AlgorithmSuite.EnsureAcceptableSignatureKeySize(securityKey, token);
            this.AlgorithmSuite.EnsureAcceptableSignatureAlgorithm(securityKey, signedXml.Signature.SignedInfo.SignatureMethod);
            signedXml.StartSignatureVerification(securityKey);
            StandardSignedInfo signedInfo = (StandardSignedInfo)signedXml.Signature.SignedInfo;

            ValidateDigestsOfTargetsInSecurityHeader(signedInfo, this.Timestamp, isPrimarySignature, signatureTarget, id);

            if (!isPrimarySignature)
            {
                if ((!this.RequireMessageProtection) && (securityKey is AsymmetricSecurityKey) && (this.Version.Addressing != AddressingVersion.None))
                {
                    // For Transport Security using Asymmetric Keys verify that 
                    // the 'To' header is signed.
                    int headerIndex = this.Message.Headers.FindHeader(XD.AddressingDictionary.To.Value, this.Message.Version.Addressing.Namespace);
                    if (headerIndex == -1)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TransportSecuredMessageMissingToHeader)));
                    XmlDictionaryReader toHeaderReader = this.Message.Headers.GetReaderAtHeader(headerIndex);
                    id = toHeaderReader.GetAttribute(XD.UtilityDictionary.IdAttribute, XD.UtilityDictionary.Namespace);
                    if (id == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.UnsignedToHeaderInTransportSecuredMessage)));
                    signedXml.EnsureDigestValidity(id, toHeaderReader);
                }
                signedXml.CompleteSignatureVerification();
                return token;
            }
            this.pendingSignature = signedXml;

            if (TD.SignatureVerificationSuccessIsEnabled())
            {
                TD.SignatureVerificationSuccess(this.EventTraceActivity);
            }

            return token;
        }

        void ValidateDigestsOfTargetsInSecurityHeader(StandardSignedInfo signedInfo, SecurityTimestamp timestamp, bool isPrimarySignature, object signatureTarget, string id)
        {
            Fx.Assert(!isPrimarySignature || (isPrimarySignature && (signatureTarget == null)), "For primary signature we try to validate all the references.");

            for (int i = 0; i < signedInfo.ReferenceCount; i++)
            {
                Reference reference = signedInfo[i];
                this.AlgorithmSuite.EnsureAcceptableDigestAlgorithm(reference.DigestMethod);
                string referredId = reference.ExtractReferredId();
                if (isPrimarySignature || (id == referredId))
                {
                    if (timestamp != null && timestamp.Id == referredId && !reference.TransformChain.NeedsInclusiveContext &&
                        timestamp.DigestAlgorithm == reference.DigestMethod && timestamp.GetDigest() != null)
                    {
                        reference.EnsureDigestValidity(referredId, timestamp.GetDigest());
                        this.ElementManager.SetTimestampSigned(referredId);
                    }
                    else
                    {
                        if (signatureTarget != null)
                            reference.EnsureDigestValidity(id, signatureTarget);
                        else
                        {
                            int tokenIndex = -1;
                            XmlDictionaryReader reader = null;
                            if (reference.IsStrTranform())
                            {
                                if (this.ElementManager.TryGetTokenElementIndexFromStrId(referredId, out tokenIndex))
                                {
                                    ReceiveSecurityHeaderEntry entry;
                                    this.ElementManager.GetElementEntry(tokenIndex, out entry);
                                    bool isSignedToken = (entry.bindingMode == ReceiveSecurityHeaderBindingModes.Signed)
                                                       || (entry.bindingMode == ReceiveSecurityHeaderBindingModes.SignedEndorsing);
                                    // This means it is a protected(signed)primary token.
                                    if (!this.ElementManager.IsPrimaryTokenSigned)
                                    {
                                        this.ElementManager.IsPrimaryTokenSigned = entry.bindingMode == ReceiveSecurityHeaderBindingModes.Primary &&
                                                                                   entry.elementCategory == ReceiveSecurityHeaderElementCategory.Token;
                                    }
                                    this.ElementManager.SetSigned(tokenIndex);
                                    // We pass true if it is a signed supporting token, signed primary token or a SignedEndorsing token. We pass false if it is a SignedEncrypted Token. 
                                    reader = this.ElementManager.GetReader(tokenIndex, isSignedToken);
                                }
                            }
                            else
                                reader = this.ElementManager.GetSignatureVerificationReader(referredId, this.EncryptBeforeSignMode);

                            if (reader != null)
                            {
                                reference.EnsureDigestValidity(referredId, reader);
                                reader.Close();
                            }
                        }
                    }

                    if (!isPrimarySignature)
                    {
                        // We were given an id to verify and we have verified it. So just break out
                        // of the loop.
                        break;
                    }
                }
            }

              // This check makes sure that if RequireSignedPrimaryToken is true (ProtectTokens is enabled on sbe) then the incoming message 
            // should have the primary signature over the primary(signing)token.
            if (isPrimarySignature && this.RequireSignedPrimaryToken && !this.ElementManager.IsPrimaryTokenSigned)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.SupportingTokenIsNotSigned, new IssuedSecurityTokenParameters())));
            }

            // NOTE: On both client and server side, WCF quietly consumes protected tokens even if protect token is not enabled on sbe. 
            // To change this behaviour add another check below and throw appropriate exception message.
        }


        void VerifySoapAttributeMatchForHeader(MessageHeaderInfo info, MessagePartSpecification signatureParts, XmlDictionaryReader reader)
        {
            if (!signatureParts.IsHeaderIncluded(info.Name, info.Namespace))
            {
                return;
            }

            EnvelopeVersion currentVersion = this.Version.Envelope;
            EnvelopeVersion otherVersion = currentVersion == EnvelopeVersion.Soap11 ? EnvelopeVersion.Soap12 : EnvelopeVersion.Soap11;

            bool presentInCurrentVersion;
            bool presentInOtherVersion;

            presentInCurrentVersion = null != reader.GetAttribute(XD.MessageDictionary.MustUnderstand, currentVersion.DictionaryNamespace);
            presentInOtherVersion = null != reader.GetAttribute(XD.MessageDictionary.MustUnderstand, otherVersion.DictionaryNamespace);
            if (presentInOtherVersion && !presentInCurrentVersion)
            {
                throw TraceUtility.ThrowHelperError(
                    new MessageSecurityException(SR.GetString(
                        SR.InvalidAttributeInSignedHeader, info.Name, info.Namespace,
                        XD.MessageDictionary.MustUnderstand, otherVersion.DictionaryNamespace,
                        XD.MessageDictionary.MustUnderstand, currentVersion.DictionaryNamespace)), this.SecurityVerifiedMessage);
            }

            presentInCurrentVersion = null != reader.GetAttribute(currentVersion.DictionaryActor, currentVersion.DictionaryNamespace);
            presentInOtherVersion = null != reader.GetAttribute(otherVersion.DictionaryActor, otherVersion.DictionaryNamespace);
            if (presentInOtherVersion && !presentInCurrentVersion)
            {
                throw TraceUtility.ThrowHelperError(
                    new MessageSecurityException(SR.GetString(
                        SR.InvalidAttributeInSignedHeader, info.Name, info.Namespace,
                        otherVersion.DictionaryActor, otherVersion.DictionaryNamespace,
                        currentVersion.DictionaryActor, currentVersion.DictionaryNamespace)), this.SecurityVerifiedMessage);
            }
        }
    }
}
