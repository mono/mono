//----------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Selectors;
    using System.Security.Cryptography;
    
    using System.ServiceModel.Security.Tokens;
    using System.Xml;
    using System.ServiceModel.Diagnostics;

    using DictionaryManager = System.IdentityModel.DictionaryManager;
    using ISecurityElement = System.IdentityModel.ISecurityElement;
    using ISignatureValueSecurityElement = System.IdentityModel.ISignatureValueSecurityElement;
    using IPrefixGenerator = System.IdentityModel.IPrefixGenerator;
   
    abstract class SendSecurityHeader : SecurityHeader, IMessageHeaderWithSharedNamespace
    {
        bool basicTokenEncrypted;
        SendSecurityHeaderElementContainer elementContainer;
        bool primarySignatureDone;
        bool encryptSignature;
        SignatureConfirmations signatureValuesGenerated;
        SignatureConfirmations signatureConfirmationsToSend;
        int idCounter;
        string idPrefix;
        bool hasSignedTokens;
        bool hasEncryptedTokens;
        MessagePartSpecification signatureParts;
        MessagePartSpecification encryptionParts;  
        SecurityTokenParameters signingTokenParameters;
        SecurityTokenParameters encryptingTokenParameters;
        List<SecurityToken> basicTokens = null;
        List<SecurityTokenParameters> basicSupportingTokenParameters = null;
        List<SecurityTokenParameters> endorsingTokenParameters = null;
        List<SecurityTokenParameters> signedEndorsingTokenParameters = null;
        List<SecurityTokenParameters> signedTokenParameters = null;
        SecurityToken encryptingToken;
        bool skipKeyInfoForEncryption;
        byte[] primarySignatureValue = null;
        bool shouldProtectTokens;
        BufferManager bufferManager;

        bool shouldSignToHeader = false;

        SecurityProtocolCorrelationState correlationState;
        bool signThenEncrypt = true;
        static readonly string[] ids = new string[] { "_0", "_1", "_2", "_3", "_4", "_5", "_6", "_7", "_8", "_9" };

        protected SendSecurityHeader(Message message, string actor, bool mustUnderstand, bool relay,
            SecurityStandardsManager standardsManager,
            SecurityAlgorithmSuite algorithmSuite, 
            MessageDirection transferDirection)
            : base(message, actor, mustUnderstand, relay, standardsManager, algorithmSuite, transferDirection)
        {
            this.elementContainer = new SendSecurityHeaderElementContainer();
        }

        public SendSecurityHeaderElementContainer ElementContainer
        {
            get { return this.elementContainer; }
        }

        public SecurityProtocolCorrelationState CorrelationState
        {
            get { return this.correlationState; }
            set
            {
                ThrowIfProcessingStarted();
                this.correlationState = value;
            }
        }

        public BufferManager StreamBufferManager
        {
            get
            {
                if (this.bufferManager == null)
                {
                    this.bufferManager = BufferManager.CreateBufferManager(0, int.MaxValue);
                }

                return this.bufferManager;
            }
            set
            {
                this.bufferManager = value;
            }
        }

        public MessagePartSpecification EncryptionParts
        {
            get { return this.encryptionParts; }
            set
            {
                ThrowIfProcessingStarted();
                if (value == null)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentNullException("value"), this.Message);
                }
                if (!value.IsReadOnly)
                {
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.MessagePartSpecificationMustBeImmutable)), this.Message);
                }
                this.encryptionParts = value;
            }
        }

        public bool EncryptPrimarySignature
        {
            get { return this.encryptSignature; }
            set
            {
                ThrowIfProcessingStarted();
                this.encryptSignature = value;
            }
        }

        internal byte[] PrimarySignatureValue
        {
            get { return this.primarySignatureValue; }
        }

        protected internal SecurityTokenParameters SigningTokenParameters
        {
            get { return this.signingTokenParameters; }
        }

        protected bool ShouldSignToHeader
        {
            get { return this.shouldSignToHeader; }
        }

        public string IdPrefix
        {
            get { return this.idPrefix; }
            set
            {
                ThrowIfProcessingStarted();
                this.idPrefix = string.IsNullOrEmpty(value) || value == "_" ? null : value;
            }
        }

        public override string Name
        {
            get { return this.StandardsManager.SecurityVersion.HeaderName.Value; }
        }

        public override string Namespace
        {
            get { return this.StandardsManager.SecurityVersion.HeaderNamespace.Value; }
        }

        protected SecurityAppliedMessage SecurityAppliedMessage
        {
            get { return (SecurityAppliedMessage) this.Message; }
        }

        public bool SignThenEncrypt
        {
            get { return this.signThenEncrypt; }
            set
            {
                ThrowIfProcessingStarted();
                this.signThenEncrypt = value;
            }
        }

        public bool ShouldProtectTokens
        {
            get { return this.shouldProtectTokens; }
            set
            {
                ThrowIfProcessingStarted();
                this.shouldProtectTokens = value;
            }
        }

        public MessagePartSpecification SignatureParts
        {
            get { return this.signatureParts; }
            set
            {
                ThrowIfProcessingStarted();
                if (value == null)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentNullException("value"), this.Message);
                }
                if (!value.IsReadOnly)
                {
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.MessagePartSpecificationMustBeImmutable)), this.Message);
                }
                this.signatureParts = value;
            }
        }

        public SecurityTimestamp Timestamp
        {
            get { return this.elementContainer.Timestamp; }
        }

        public bool HasSignedTokens
        {
            get
            {
                return this.hasSignedTokens;
            }
        }

        public bool HasEncryptedTokens
        {
            get
            {
                return this.hasEncryptedTokens;
            }
        }

        public void AddPrerequisiteToken(SecurityToken token)
        {
            ThrowIfProcessingStarted();
            if (token == null)
            {
                throw TraceUtility.ThrowHelperArgumentNull("token", this.Message);
            }
            this.elementContainer.PrerequisiteToken = token;
        }

        void AddParameters(ref List<SecurityTokenParameters> list, SecurityTokenParameters item)
        {
            if (list == null)
            {
                list = new List<SecurityTokenParameters>();
            }
            list.Add(item);
        }

        public abstract void ApplyBodySecurity(XmlDictionaryWriter writer, IPrefixGenerator prefixGenerator);

        public abstract void ApplySecurityAndWriteHeaders(MessageHeaders headers, XmlDictionaryWriter writer, IPrefixGenerator prefixGenerator);

        protected virtual bool HasSignedEncryptedMessagePart
        {
            get { return false; }
        }

        public void SetSigningToken(SecurityToken token, SecurityTokenParameters tokenParameters)
        {
            ThrowIfProcessingStarted();
            if ((token == null && tokenParameters != null) || (token != null && tokenParameters == null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.TokenMustBeNullWhenTokenParametersAre)));
            }
            this.elementContainer.SourceSigningToken = token;
            this.signingTokenParameters = tokenParameters;
        }

        public void SetEncryptionToken(SecurityToken token, SecurityTokenParameters tokenParameters)
        {
            ThrowIfProcessingStarted();
            if ((token == null && tokenParameters != null) || (token != null && tokenParameters == null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.TokenMustBeNullWhenTokenParametersAre)));
            }
            this.elementContainer.SourceEncryptionToken = token;
            this.encryptingTokenParameters = tokenParameters;
        }


        public void AddBasicSupportingToken(SecurityToken token, SecurityTokenParameters parameters)
        {
            if (token == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            if (parameters == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            ThrowIfProcessingStarted();
            SendSecurityHeaderElement tokenElement = new SendSecurityHeaderElement(token.Id, new TokenElement(token, this.StandardsManager));
            tokenElement.MarkedForEncryption = true;
            this.elementContainer.AddBasicSupportingToken(tokenElement);
            hasEncryptedTokens = true;
            hasSignedTokens = true;
            this.AddParameters(ref this.basicSupportingTokenParameters, parameters);
            if (this.basicTokens == null)
            {
                this.basicTokens = new List<SecurityToken>();
            }

            //  We maintain a list of the basic tokens for the SignThenEncrypt case as we will 
            //  need this token to write STR entry on OnWriteHeaderContents. 
            this.basicTokens.Add(token);

        }

        public void AddEndorsingSupportingToken(SecurityToken token, SecurityTokenParameters parameters)
        {
            if (token == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            if (parameters == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            ThrowIfProcessingStarted();
            this.elementContainer.AddEndorsingSupportingToken(token);
            // The ProviderBackedSecurityToken was added for the ChannelBindingToken (CBT) effort for win7.  
            // We can assume the key is of type symmetric key.
            //
            // Asking for the key type from the token will cause the ProviderBackedSecurityToken 
            // to attempt to resolve the token and the nego will start.  
            //
            // We don't want that.  
            // We want to defer the nego until after the CBT is available in SecurityAppliedMessage.OnWriteMessage.
            if (!(token is ProviderBackedSecurityToken))
            {
                this.shouldSignToHeader |= (!this.RequireMessageProtection) && (SecurityUtils.GetSecurityKey<AsymmetricSecurityKey>(token) != null);
            }
            this.AddParameters(ref this.endorsingTokenParameters, parameters);
        }

        public void AddSignedEndorsingSupportingToken(SecurityToken token, SecurityTokenParameters parameters)
        {
            if (token == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            if (parameters == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            ThrowIfProcessingStarted();
            this.elementContainer.AddSignedEndorsingSupportingToken(token);
            hasSignedTokens = true;
            this.shouldSignToHeader |= (!this.RequireMessageProtection) && (SecurityUtils.GetSecurityKey<AsymmetricSecurityKey>(token) != null);            
            this.AddParameters(ref this.signedEndorsingTokenParameters, parameters);
        }

        public void AddSignedSupportingToken(SecurityToken token, SecurityTokenParameters parameters)
        {
            if (token == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            if (parameters == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            ThrowIfProcessingStarted();
            this.elementContainer.AddSignedSupportingToken(token);
            hasSignedTokens = true;
            this.AddParameters(ref this.signedTokenParameters, parameters);
        }

        public void AddSignatureConfirmations(SignatureConfirmations confirmations)
        {
            ThrowIfProcessingStarted();
            this.signatureConfirmationsToSend = confirmations;
        }

        public void AddTimestamp(TimeSpan timestampValidityDuration)
        {
            DateTime now = DateTime.UtcNow;
            string id = this.RequireMessageProtection ? SecurityUtils.GenerateId() : GenerateId();
            AddTimestamp(new SecurityTimestamp(now, now + timestampValidityDuration, id));
        }

        public void AddTimestamp(SecurityTimestamp timestamp)
        {
            ThrowIfProcessingStarted();
            if (this.elementContainer.Timestamp != null)
            {
                throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.TimestampAlreadySetForSecurityHeader)), this.Message);
            }
            if (timestamp == null)
            {
                throw TraceUtility.ThrowHelperArgumentNull("timestamp", this.Message);
            }

            this.elementContainer.Timestamp = timestamp;
        }

        protected virtual ISignatureValueSecurityElement[] CreateSignatureConfirmationElements(SignatureConfirmations signatureConfirmations)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                SR.GetString(SR.SignatureConfirmationNotSupported)));
        }

        void StartEncryption()
        {
            if (this.elementContainer.SourceEncryptionToken == null)
            {
                return;
            }
            // determine the key identifier clause to use for the source
            SecurityTokenReferenceStyle sourceEncryptingKeyReferenceStyle = GetTokenReferenceStyle(this.encryptingTokenParameters);
            bool encryptionTokenSerialized = sourceEncryptingKeyReferenceStyle == SecurityTokenReferenceStyle.Internal;
            SecurityKeyIdentifierClause sourceEncryptingKeyIdentifierClause = this.encryptingTokenParameters.CreateKeyIdentifierClause(this.elementContainer.SourceEncryptionToken, sourceEncryptingKeyReferenceStyle);
            if (sourceEncryptingKeyIdentifierClause == null)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TokenManagerCannotCreateTokenReference)), this.Message);
            }
            SecurityToken sourceToken;
            SecurityKeyIdentifierClause sourceTokenIdentifierClause;

            // if the source token cannot do symmetric crypto, create a wrapped key
            if (!SecurityUtils.HasSymmetricSecurityKey(elementContainer.SourceEncryptionToken))
            {
                int keyLength = Math.Max(128, this.AlgorithmSuite.DefaultSymmetricKeyLength);
                CryptoHelper.ValidateSymmetricKeyLength(keyLength, this.AlgorithmSuite);
                byte[] key = new byte[keyLength / 8];
                CryptoHelper.FillRandomBytes(key);
                string keyWrapAlgorithm;
                XmlDictionaryString keyWrapAlgorithmDictionaryString;
                this.AlgorithmSuite.GetKeyWrapAlgorithm(elementContainer.SourceEncryptionToken, out keyWrapAlgorithm, out keyWrapAlgorithmDictionaryString);
                WrappedKeySecurityToken wrappedKey = new WrappedKeySecurityToken(GenerateId(), key, keyWrapAlgorithm, keyWrapAlgorithmDictionaryString,
                    elementContainer.SourceEncryptionToken, new SecurityKeyIdentifier(sourceEncryptingKeyIdentifierClause));
                elementContainer.WrappedEncryptionToken = wrappedKey;
                sourceToken = wrappedKey;
                sourceTokenIdentifierClause = new LocalIdKeyIdentifierClause(wrappedKey.Id, wrappedKey.GetType());
                encryptionTokenSerialized = true;
            }
            else
            {
                sourceToken = elementContainer.SourceEncryptionToken;
                sourceTokenIdentifierClause = sourceEncryptingKeyIdentifierClause;
            }

            // determine if a key needs to be derived
            SecurityKeyIdentifierClause encryptingKeyIdentifierClause;
            // determine if a token needs to be derived
            if (this.encryptingTokenParameters.RequireDerivedKeys)
            {
                string derivationAlgorithm = this.AlgorithmSuite.GetEncryptionKeyDerivationAlgorithm(sourceToken, this.StandardsManager.MessageSecurityVersion.SecureConversationVersion);
                string expectedDerivationAlgorithm = SecurityUtils.GetKeyDerivationAlgorithm(this.StandardsManager.MessageSecurityVersion.SecureConversationVersion);
                if (derivationAlgorithm == expectedDerivationAlgorithm)
                {
                    DerivedKeySecurityToken derivedEncryptingToken = new DerivedKeySecurityToken(-1, 0,
                        this.AlgorithmSuite.GetEncryptionKeyDerivationLength(sourceToken, this.StandardsManager.MessageSecurityVersion.SecureConversationVersion), null, DerivedKeySecurityToken.DefaultNonceLength, sourceToken, sourceTokenIdentifierClause, derivationAlgorithm, GenerateId());
                    this.encryptingToken = this.elementContainer.DerivedEncryptionToken = derivedEncryptingToken;
                    encryptingKeyIdentifierClause = new LocalIdKeyIdentifierClause(derivedEncryptingToken.Id, derivedEncryptingToken.GetType());
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedCryptoAlgorithm, derivationAlgorithm)));
                }
            }
            else
            {
                this.encryptingToken = sourceToken;
                encryptingKeyIdentifierClause = sourceTokenIdentifierClause;
            }

            this.skipKeyInfoForEncryption = encryptionTokenSerialized && this.EncryptedKeyContainsReferenceList && (this.encryptingToken is WrappedKeySecurityToken) && this.signThenEncrypt;
            SecurityKeyIdentifier identifier;
            if (this.skipKeyInfoForEncryption)
            {
                identifier = null;
            }
            else
            {
                identifier = new SecurityKeyIdentifier(encryptingKeyIdentifierClause);
            }

            StartEncryptionCore(this.encryptingToken, identifier);
        }

        void CompleteEncryption()
        {
            ISecurityElement referenceList = CompleteEncryptionCore(
                elementContainer.PrimarySignature,
                elementContainer.GetBasicSupportingTokens(),
                elementContainer.GetSignatureConfirmations(),
                elementContainer.GetEndorsingSignatures());

            if (referenceList == null)
            {
                // null out all the encryption fields since there is no encryption needed
                this.elementContainer.SourceEncryptionToken = null;
                this.elementContainer.WrappedEncryptionToken = null;
                this.elementContainer.DerivedEncryptionToken = null;
                return;
            }

            if (this.skipKeyInfoForEncryption)
            {
                WrappedKeySecurityToken wrappedKeyToken = this.encryptingToken as WrappedKeySecurityToken;
                wrappedKeyToken.EnsureEncryptedKeySetUp();
                wrappedKeyToken.EncryptedKey.ReferenceList = (ReferenceList) referenceList;
            }
            else
            {
                this.elementContainer.ReferenceList = referenceList;
            }
            basicTokenEncrypted = true;
        }

        internal void StartSecurityApplication()
        {
            if (this.SignThenEncrypt)
            {
                StartSignature();
                StartEncryption();
            }
            else
            {
                StartEncryption();
                StartSignature();
            }
        }

        internal void CompleteSecurityApplication()
        {
            if (this.SignThenEncrypt)
            {
                CompleteSignature();
                SignWithSupportingTokens();
                CompleteEncryption();
            }
            else
            {
                CompleteEncryption();
                CompleteSignature();
                SignWithSupportingTokens();
            }

            if (this.correlationState != null)
            {
                this.correlationState.SignatureConfirmations = GetSignatureValues();
            }
        }

        public void RemoveSignatureEncryptionIfAppropriate()
        {
            if (this.SignThenEncrypt && 
                this.EncryptPrimarySignature && 
                (this.SecurityAppliedMessage.BodyProtectionMode != MessagePartProtectionMode.SignThenEncrypt) &&
                (this.basicSupportingTokenParameters == null || this.basicSupportingTokenParameters.Count == 0) &&
                (this.signatureConfirmationsToSend == null || this.signatureConfirmationsToSend.Count == 0 || !this.signatureConfirmationsToSend.IsMarkedForEncryption) &&
                !this.HasSignedEncryptedMessagePart)
            {
                this.encryptSignature = false;
            }
        }

        public string GenerateId()
        {
            int id = this.idCounter++;

            if (this.idPrefix != null)
            {
                return this.idPrefix + id;
            }

            if (id < ids.Length)
            {
                return ids[id];
            }
            else
            {
                return "_" + id;
            }
        }

        SignatureConfirmations GetSignatureValues()
        {
            return this.signatureValuesGenerated;
        }

        protected override void OnWriteStartHeader(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            this.StandardsManager.SecurityVersion.WriteStartHeader(writer);
            WriteHeaderAttributes(writer, messageVersion);
        }

        internal static bool ShouldSerializeToken(SecurityTokenParameters parameters, MessageDirection transferDirection)
        {
            switch (parameters.InclusionMode)
            {
                case SecurityTokenInclusionMode.AlwaysToInitiator:
                    return (transferDirection == MessageDirection.Output);
                case SecurityTokenInclusionMode.Once:
                case SecurityTokenInclusionMode.AlwaysToRecipient:
                    return (transferDirection == MessageDirection.Input);
                case SecurityTokenInclusionMode.Never:
                    return false;
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedTokenInclusionMode, parameters.InclusionMode)));
            }
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            if (this.basicSupportingTokenParameters != null && this.basicSupportingTokenParameters.Count > 0 
                && this.RequireMessageProtection && !basicTokenEncrypted)
            {
                throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.BasicTokenCannotBeWrittenWithoutEncryption)), this.Message);
            }

            if (this.elementContainer.Timestamp != null && this.Layout != SecurityHeaderLayout.LaxTimestampLast)
            {
                this.StandardsManager.WSUtilitySpecificationVersion.WriteTimestamp(writer, this.elementContainer.Timestamp);
            }
            if (elementContainer.PrerequisiteToken != null)
            {
                this.StandardsManager.SecurityTokenSerializer.WriteToken(writer, elementContainer.PrerequisiteToken);
            }
            if (elementContainer.SourceSigningToken != null)
            {
                if (ShouldSerializeToken(this.signingTokenParameters, this.MessageDirection))
                {
                    this.StandardsManager.SecurityTokenSerializer.WriteToken(writer, elementContainer.SourceSigningToken);

                    // Implement Protect token 
                    // NOTE: The spec says sign the primary token if it is not included in the message. But we currently are not supporting it
                    // as we do not support STR-Transform for external references. Hence we can not sign the token which is external ie not in the message.
                    // This only affects the messages from service to client where 
                    // 1. allowSerializedSigningTokenOnReply is false.
                    // 2. SymmetricSecurityBindingElement with IssuedTokens binding where the issued token has a symmetric key.

                    if (this.ShouldProtectTokens)
                    {
                        this.WriteSecurityTokenReferencyEntry(writer, elementContainer.SourceSigningToken, this.signingTokenParameters);
                    }
                }
            }
            if (elementContainer.DerivedSigningToken != null)
            {
                this.StandardsManager.SecurityTokenSerializer.WriteToken(writer, elementContainer.DerivedSigningToken);
            }
            if (elementContainer.SourceEncryptionToken != null && elementContainer.SourceEncryptionToken != elementContainer.SourceSigningToken && ShouldSerializeToken(encryptingTokenParameters, this.MessageDirection))
            {
                this.StandardsManager.SecurityTokenSerializer.WriteToken(writer, elementContainer.SourceEncryptionToken);
            }
            if (elementContainer.WrappedEncryptionToken != null)
            {
                this.StandardsManager.SecurityTokenSerializer.WriteToken(writer, elementContainer.WrappedEncryptionToken);
            }
            if (elementContainer.DerivedEncryptionToken != null)
            {
                this.StandardsManager.SecurityTokenSerializer.WriteToken(writer, elementContainer.DerivedEncryptionToken); 
            }
            if (this.SignThenEncrypt)
            {
                if (elementContainer.ReferenceList != null)
                {
                    elementContainer.ReferenceList.WriteTo(writer, ServiceModelDictionaryManager.Instance);
                }
            }
          
            SecurityToken[] signedTokens = elementContainer.GetSignedSupportingTokens();
            if (signedTokens != null)
            {
                for (int i = 0; i < signedTokens.Length; ++i)
                {
                    this.StandardsManager.SecurityTokenSerializer.WriteToken(writer, signedTokens[i]);
                    this.WriteSecurityTokenReferencyEntry(writer, signedTokens[i], this.signedTokenParameters[i]);
                }
            }
            SendSecurityHeaderElement[] basicTokensXml = elementContainer.GetBasicSupportingTokens();
            if (basicTokensXml != null)
            {
                for (int i = 0; i < basicTokensXml.Length; ++i)
                {
                    basicTokensXml[i].Item.WriteTo(writer, ServiceModelDictionaryManager.Instance);
                    if (this.SignThenEncrypt)
                    {
                        this.WriteSecurityTokenReferencyEntry(writer, this.basicTokens[i], this.basicSupportingTokenParameters[i]);
                    }
                }
            }
            SecurityToken[] endorsingTokens = elementContainer.GetEndorsingSupportingTokens();
            if (endorsingTokens != null)
            {
                for (int i = 0; i < endorsingTokens.Length; ++i)
                {
                    if (ShouldSerializeToken(endorsingTokenParameters[i], this.MessageDirection))
                    {
                        this.StandardsManager.SecurityTokenSerializer.WriteToken(writer, endorsingTokens[i]);
                    }
                }
            }
            SecurityToken[] endorsingDerivedTokens = elementContainer.GetEndorsingDerivedSupportingTokens();
            if (endorsingDerivedTokens != null)
            {
                for (int i = 0; i < endorsingDerivedTokens.Length; ++i)
                {
                    this.StandardsManager.SecurityTokenSerializer.WriteToken(writer, endorsingDerivedTokens[i]);
                }
            }
            SecurityToken[] signedEndorsingTokens = elementContainer.GetSignedEndorsingSupportingTokens();
            if (signedEndorsingTokens != null)
            {
                for (int i = 0; i < signedEndorsingTokens.Length; ++i)
                {
                    this.StandardsManager.SecurityTokenSerializer.WriteToken(writer, signedEndorsingTokens[i]);
                    this.WriteSecurityTokenReferencyEntry(writer, signedEndorsingTokens[i], this.signedEndorsingTokenParameters[i]);
                }
            }
            SecurityToken[] signedEndorsingDerivedTokens = elementContainer.GetSignedEndorsingDerivedSupportingTokens();
            if (signedEndorsingDerivedTokens != null)
            {
                for (int i = 0; i < signedEndorsingDerivedTokens.Length; ++i)
                {
                    this.StandardsManager.SecurityTokenSerializer.WriteToken(writer, signedEndorsingDerivedTokens[i]);
                }
            }
            SendSecurityHeaderElement[] signatureConfirmations = elementContainer.GetSignatureConfirmations();
            if (signatureConfirmations != null)
            {
                for (int i = 0; i < signatureConfirmations.Length; ++i)
                {
                    signatureConfirmations[i].Item.WriteTo(writer, ServiceModelDictionaryManager.Instance);
                }
            }
            if (elementContainer.PrimarySignature != null && elementContainer.PrimarySignature.Item != null)
            {
                elementContainer.PrimarySignature.Item.WriteTo(writer, ServiceModelDictionaryManager.Instance);
            }
            SendSecurityHeaderElement[] endorsingSignatures = elementContainer.GetEndorsingSignatures();
            if (endorsingSignatures != null)
            {
                for (int i = 0; i < endorsingSignatures.Length; ++i)
                {
                    endorsingSignatures[i].Item.WriteTo(writer, ServiceModelDictionaryManager.Instance);
                }
            }
            if (!this.SignThenEncrypt)
            {
                if (elementContainer.ReferenceList != null)
                {
                    elementContainer.ReferenceList.WriteTo(writer, ServiceModelDictionaryManager.Instance);
                }
            }
            if (this.elementContainer.Timestamp != null && this.Layout == SecurityHeaderLayout.LaxTimestampLast)
            {
                this.StandardsManager.WSUtilitySpecificationVersion.WriteTimestamp(writer, this.elementContainer.Timestamp);
            }
        }

        protected abstract void WriteSecurityTokenReferencyEntry(XmlDictionaryWriter writer, SecurityToken securityToken, SecurityTokenParameters securityTokenParameters);

        public Message SetupExecution()
        {
            ThrowIfProcessingStarted();
            SetProcessingStarted();

            bool signBody = false;
            if (this.elementContainer.SourceSigningToken != null)
            {
                if (this.signatureParts == null)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentNullException("SignatureParts"), this.Message);
                }
                signBody = this.signatureParts.IsBodyIncluded;
            }

            bool encryptBody = false;
            if (this.elementContainer.SourceEncryptionToken != null)
            {
                if (this.encryptionParts == null)
                {
                    throw TraceUtility.ThrowHelperError(new ArgumentNullException("EncryptionParts"), this.Message);
                }
                encryptBody = this.encryptionParts.IsBodyIncluded;
            }

            SecurityAppliedMessage message = new SecurityAppliedMessage(this.Message, this, signBody, encryptBody);
            this.Message = message;
            return message;
        }

        protected internal SecurityTokenReferenceStyle GetTokenReferenceStyle(SecurityTokenParameters parameters)
        {
            return (ShouldSerializeToken(parameters, this.MessageDirection)) ? SecurityTokenReferenceStyle.Internal : SecurityTokenReferenceStyle.External;
        }

        void StartSignature()
        {
            if (this.elementContainer.SourceSigningToken == null)
            {
                return;
            }

            // determine the key identifier clause to use for the source
            SecurityTokenReferenceStyle sourceSigningKeyReferenceStyle = GetTokenReferenceStyle(this.signingTokenParameters);
            SecurityKeyIdentifierClause sourceSigningKeyIdentifierClause = this.signingTokenParameters.CreateKeyIdentifierClause(this.elementContainer.SourceSigningToken, sourceSigningKeyReferenceStyle);
            if (sourceSigningKeyIdentifierClause == null)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TokenManagerCannotCreateTokenReference)), this.Message);
            }

            SecurityToken signingToken;
            SecurityKeyIdentifierClause signingKeyIdentifierClause;

            // determine if a token needs to be derived
            if (this.signingTokenParameters.RequireDerivedKeys && !this.signingTokenParameters.HasAsymmetricKey)
            {
                string derivationAlgorithm = this.AlgorithmSuite.GetSignatureKeyDerivationAlgorithm(this.elementContainer.SourceSigningToken, this.StandardsManager.MessageSecurityVersion.SecureConversationVersion);
                string expectedDerivationAlgorithm = SecurityUtils.GetKeyDerivationAlgorithm(this.StandardsManager.MessageSecurityVersion.SecureConversationVersion);
                if (derivationAlgorithm == expectedDerivationAlgorithm)
                {
                    DerivedKeySecurityToken derivedSigningToken = new DerivedKeySecurityToken(-1, 0, this.AlgorithmSuite.GetSignatureKeyDerivationLength(this.elementContainer.SourceSigningToken, this.StandardsManager.MessageSecurityVersion.SecureConversationVersion), null, DerivedKeySecurityToken.DefaultNonceLength, this.elementContainer.SourceSigningToken,
                        sourceSigningKeyIdentifierClause, derivationAlgorithm, GenerateId());
                    signingToken = this.elementContainer.DerivedSigningToken = derivedSigningToken;
                    signingKeyIdentifierClause = new LocalIdKeyIdentifierClause(signingToken.Id, signingToken.GetType());
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedCryptoAlgorithm, derivationAlgorithm)));
                }
            }
            else
            {
                signingToken = elementContainer.SourceSigningToken;
                signingKeyIdentifierClause = sourceSigningKeyIdentifierClause;
            }

            SecurityKeyIdentifier signingKeyIdentifier = new SecurityKeyIdentifier(signingKeyIdentifierClause);
            
            if (signatureConfirmationsToSend != null && signatureConfirmationsToSend.Count > 0)
            {
                ISecurityElement[] signatureConfirmationElements;
                signatureConfirmationElements = CreateSignatureConfirmationElements(signatureConfirmationsToSend);
                for (int i = 0; i < signatureConfirmationElements.Length; ++i)
                {
                    SendSecurityHeaderElement sigConfElement = new SendSecurityHeaderElement(signatureConfirmationElements[i].Id, signatureConfirmationElements[i]);
                    sigConfElement.MarkedForEncryption = signatureConfirmationsToSend.IsMarkedForEncryption;
                    this.elementContainer.AddSignatureConfirmation(sigConfElement);
                }
            }

            bool generateTargettablePrimarySignature = ((this.endorsingTokenParameters != null) || (this.signedEndorsingTokenParameters != null));
            this.StartPrimarySignatureCore(signingToken, signingKeyIdentifier, this.signatureParts, generateTargettablePrimarySignature);
        }

        void CompleteSignature()
        {
            ISignatureValueSecurityElement signedXml = this.CompletePrimarySignatureCore(
                elementContainer.GetSignatureConfirmations(), elementContainer.GetSignedEndorsingSupportingTokens(), 
                elementContainer.GetSignedSupportingTokens(), elementContainer.GetBasicSupportingTokens(), true);
            if (signedXml == null)
            {
                return;
            }
            this.elementContainer.PrimarySignature = new SendSecurityHeaderElement(signedXml.Id, signedXml);
            this.elementContainer.PrimarySignature.MarkedForEncryption = this.encryptSignature;
            AddGeneratedSignatureValue(signedXml.GetSignatureValue(), this.EncryptPrimarySignature);
            this.primarySignatureDone = true;
            this.primarySignatureValue = signedXml.GetSignatureValue();
        }

        protected abstract void StartPrimarySignatureCore(SecurityToken token, SecurityKeyIdentifier identifier, MessagePartSpecification signatureParts, bool generateTargettablePrimarySignature);

        protected abstract ISignatureValueSecurityElement CompletePrimarySignatureCore(SendSecurityHeaderElement[] signatureConfirmations,
           SecurityToken[] signedEndorsingTokens, SecurityToken[] signedTokens, SendSecurityHeaderElement[] basicTokens, bool isPrimarySignature);


        protected abstract ISignatureValueSecurityElement CreateSupportingSignature(SecurityToken token, SecurityKeyIdentifier identifier);

        protected abstract ISignatureValueSecurityElement CreateSupportingSignature(SecurityToken token, SecurityKeyIdentifier identifier, ISecurityElement primarySignature);

        protected abstract void StartEncryptionCore(SecurityToken token, SecurityKeyIdentifier keyIdentifier);

        protected abstract ISecurityElement CompleteEncryptionCore(SendSecurityHeaderElement primarySignature, 
            SendSecurityHeaderElement[] basicTokens, SendSecurityHeaderElement[] signatureConfirmations, SendSecurityHeaderElement[] endorsingSignatures);

        void SignWithSupportingToken(SecurityToken token, SecurityKeyIdentifierClause identifierClause)
        {
            if (token == null)
            {
                throw TraceUtility.ThrowHelperArgumentNull("token", this.Message);
            }
            if (identifierClause == null)
            {
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TokenManagerCannotCreateTokenReference)), this.Message);
            }
            if (!this.RequireMessageProtection)
            {
                if (this.elementContainer.Timestamp == null)
                {
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.SigningWithoutPrimarySignatureRequiresTimestamp)), this.Message);
                }
            }
            else
            {
                if (!this.primarySignatureDone)
                {
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.PrimarySignatureMustBeComputedBeforeSupportingTokenSignatures)), this.Message);
                }
                if (this.elementContainer.PrimarySignature.Item == null)
                {
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.SupportingTokenSignaturesNotExpected)), this.Message);
                }
            }

            SecurityKeyIdentifier identifier = new SecurityKeyIdentifier(identifierClause);
            ISignatureValueSecurityElement supportingSignature;
            if (!this.RequireMessageProtection)
            {
                supportingSignature = CreateSupportingSignature(token, identifier);
            }
            else
            {
                supportingSignature = CreateSupportingSignature(token, identifier, elementContainer.PrimarySignature.Item);
            }
            AddGeneratedSignatureValue(supportingSignature.GetSignatureValue(), encryptSignature);
            SendSecurityHeaderElement supportingSignatureElement = new SendSecurityHeaderElement(supportingSignature.Id, supportingSignature);
            supportingSignatureElement.MarkedForEncryption = encryptSignature;
            this.elementContainer.AddEndorsingSignature(supportingSignatureElement);
        }

        void SignWithSupportingTokens()
        {
            SecurityToken[] endorsingTokens = this.elementContainer.GetEndorsingSupportingTokens();
            if (endorsingTokens != null)
            {
                for (int i = 0; i < endorsingTokens.Length; ++i)
                {
                    SecurityToken source = endorsingTokens[i];
                    SecurityKeyIdentifierClause sourceKeyClause = endorsingTokenParameters[i].CreateKeyIdentifierClause(source, GetTokenReferenceStyle(endorsingTokenParameters[i]));
                    if (sourceKeyClause == null)
                    {
                        throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TokenManagerCannotCreateTokenReference)), this.Message);
                    }
                    SecurityToken signingToken;
                    SecurityKeyIdentifierClause signingKeyClause;
                    if (endorsingTokenParameters[i].RequireDerivedKeys && !endorsingTokenParameters[i].HasAsymmetricKey)
                    {
                        string derivationAlgorithm = SecurityUtils.GetKeyDerivationAlgorithm(this.StandardsManager.MessageSecurityVersion.SecureConversationVersion);
                        DerivedKeySecurityToken dkt = new DerivedKeySecurityToken(-1, 0, 
                            this.AlgorithmSuite.GetSignatureKeyDerivationLength(source, this.StandardsManager.MessageSecurityVersion.SecureConversationVersion), null,
                            DerivedKeySecurityToken.DefaultNonceLength, source, sourceKeyClause, derivationAlgorithm, GenerateId());
                        signingToken = dkt;
                        signingKeyClause = new LocalIdKeyIdentifierClause(dkt.Id, dkt.GetType());
                        this.elementContainer.AddEndorsingDerivedSupportingToken(dkt);
                    }
                    else
                    {
                        signingToken = source;
                        signingKeyClause = sourceKeyClause;
                    }
                    SignWithSupportingToken(signingToken, signingKeyClause);
                }
            }
            SecurityToken[] signedEndorsingSupportingTokens = this.elementContainer.GetSignedEndorsingSupportingTokens();
            if (signedEndorsingSupportingTokens != null)
            {
                for (int i = 0; i < signedEndorsingSupportingTokens.Length; ++i)
                {
                    SecurityToken source = signedEndorsingSupportingTokens[i];
                    SecurityKeyIdentifierClause sourceKeyClause = signedEndorsingTokenParameters[i].CreateKeyIdentifierClause(source, GetTokenReferenceStyle(signedEndorsingTokenParameters[i]));
                    if (sourceKeyClause == null)
                    {
                        throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TokenManagerCannotCreateTokenReference)), this.Message);
                    }
                    SecurityToken signingToken;
                    SecurityKeyIdentifierClause signingKeyClause;
                    if (signedEndorsingTokenParameters[i].RequireDerivedKeys && !signedEndorsingTokenParameters[i].HasAsymmetricKey)
                    {
                        string derivationAlgorithm = SecurityUtils.GetKeyDerivationAlgorithm(this.StandardsManager.MessageSecurityVersion.SecureConversationVersion);
                        DerivedKeySecurityToken dkt = new DerivedKeySecurityToken(-1, 0, 
                            this.AlgorithmSuite.GetSignatureKeyDerivationLength(source, this.StandardsManager.MessageSecurityVersion.SecureConversationVersion), null,
                            DerivedKeySecurityToken.DefaultNonceLength, source, sourceKeyClause, derivationAlgorithm, GenerateId());
                        signingToken = dkt;
                        signingKeyClause = new LocalIdKeyIdentifierClause(dkt.Id, dkt.GetType());
                        this.elementContainer.AddSignedEndorsingDerivedSupportingToken(dkt);
                    }
                    else
                    {
                        signingToken = source;
                        signingKeyClause = sourceKeyClause;
                    }
                    SignWithSupportingToken(signingToken, signingKeyClause);
                }
            }
        }

        protected bool ShouldUseStrTransformForToken(SecurityToken securityToken, int position, SecurityTokenAttachmentMode mode, out SecurityKeyIdentifierClause keyIdentifierClause)
        {
            IssuedSecurityTokenParameters tokenParams = null;
            keyIdentifierClause = null;

            switch (mode)
            {
                case SecurityTokenAttachmentMode.SignedEndorsing:
                    tokenParams = this.signedEndorsingTokenParameters[position] as IssuedSecurityTokenParameters;
                    break;
                case SecurityTokenAttachmentMode.Signed:
                    tokenParams = this.signedTokenParameters[position] as IssuedSecurityTokenParameters;
                    break;
                case SecurityTokenAttachmentMode.SignedEncrypted:
                    tokenParams = this.basicSupportingTokenParameters[position] as IssuedSecurityTokenParameters;
                    break;
                default:
                    return false;
            }

            if (tokenParams != null && tokenParams.UseStrTransform)
            {
                keyIdentifierClause = tokenParams.CreateKeyIdentifierClause(securityToken, GetTokenReferenceStyle(tokenParams));
                if (keyIdentifierClause == null)
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.TokenManagerCannotCreateTokenReference)), this.Message);
                }

                return true;
            }

            return false;
        }

        XmlDictionaryString IMessageHeaderWithSharedNamespace.SharedNamespace
        {
            get { return XD.UtilityDictionary.Namespace; }
        }

        XmlDictionaryString IMessageHeaderWithSharedNamespace.SharedPrefix
        {
            get { return XD.UtilityDictionary.Prefix; }
        }

        void AddGeneratedSignatureValue(byte[] signatureValue, bool wasEncrypted)
        {
            // cache outgoing signatures only on the client side
            if (this.MaintainSignatureConfirmationState && (this.signatureConfirmationsToSend == null))
            {
                if (this.signatureValuesGenerated == null)
                {
                    this.signatureValuesGenerated = new SignatureConfirmations();
                }
                this.signatureValuesGenerated.AddConfirmation(signatureValue, wasEncrypted);
            }
        }
    }

    class TokenElement : ISecurityElement
    {
        SecurityStandardsManager standardsManager;
        SecurityToken token;

        public TokenElement(SecurityToken token, SecurityStandardsManager standardsManager)
        {
            this.token = token;
            this.standardsManager = standardsManager;
        }

        public override bool Equals(object item)
        {
            TokenElement element = item as TokenElement;
            return (element != null && this.token == element.token && this.standardsManager == element.standardsManager);
        }

        public override int GetHashCode()
        {
            return token.GetHashCode() ^ standardsManager.GetHashCode();
        }

        public bool HasId
        {
            get { return true; }
        }

        public string Id
        {
            get { return token.Id; }
        }

        public SecurityToken Token
        {
            get { return token; }
        }

        public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            standardsManager.SecurityTokenSerializer.WriteToken(writer, token);
        }
    }
}
