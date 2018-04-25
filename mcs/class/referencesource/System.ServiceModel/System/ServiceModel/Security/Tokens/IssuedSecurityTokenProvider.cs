//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.Xml;

    using SafeCloseHandle = System.IdentityModel.SafeCloseHandle;
    using SafeFreeCredentials = System.IdentityModel.SafeFreeCredentials;
    using SafeNativeMethods = System.ServiceModel.ComIntegration.SafeNativeMethods;
    using Win32Error = System.ServiceModel.ComIntegration.Win32Error;
    using WSTrustFeb2005Constants = System.IdentityModel.Protocols.WSTrust.WSTrustFeb2005Constants;
    using WSTrust13Constants = System.IdentityModel.Protocols.WSTrust.WSTrust13Constants;
    using WSTrust14Constants = System.IdentityModel.Protocols.WSTrust.WSTrust14Constants;
    using System.IO;
    using System.Text;
    
    public class IssuedSecurityTokenProvider : SecurityTokenProvider, ICommunicationObject
    {
        CoreFederatedTokenProvider federatedTokenProvider;
        MessageSecurityVersion messageSecurityVersion;
        SecurityTokenSerializer securityTokenSerializer;
        SecurityTokenHandlerCollectionManager tokenHandlerCollectionManager = null;

        public IssuedSecurityTokenProvider()
            : this(null)
        {
        }

        internal IssuedSecurityTokenProvider(SafeFreeCredentials credentialsHandle)
        {
            this.federatedTokenProvider = new CoreFederatedTokenProvider(credentialsHandle);
            this.messageSecurityVersion = MessageSecurityVersion.Default;
        }

        public event EventHandler Closed
        {
            add { this.federatedTokenProvider.Closed += value; }
            remove { this.federatedTokenProvider.Closed -= value; }
        }

        public event EventHandler Closing
        {
            add { this.federatedTokenProvider.Closing += value; }
            remove { this.federatedTokenProvider.Closing -= value; }
        }

        public event EventHandler Faulted
        {
            add { this.federatedTokenProvider.Faulted += value; }
            remove { this.federatedTokenProvider.Faulted -= value; }
        }

        public event EventHandler Opened
        {
            add { this.federatedTokenProvider.Opened += value; }
            remove { this.federatedTokenProvider.Opened -= value; }
        }

        public event EventHandler Opening
        {
            add { this.federatedTokenProvider.Opening += value; }
            remove { this.federatedTokenProvider.Opening -= value; }
        }

        public Binding IssuerBinding
        {
            get
            {
                return this.federatedTokenProvider.IssuerBinding;
            }
            set
            {
                this.federatedTokenProvider.IssuerBinding = value;
            }
        }

        public KeyedByTypeCollection<IEndpointBehavior> IssuerChannelBehaviors
        {
            get
            {
                return this.federatedTokenProvider.IssuerChannelBehaviors;
            }
        }

        public Collection<XmlElement> TokenRequestParameters
        {
            get
            {
                return this.federatedTokenProvider.RequestProperties;
            }
        }

        public EndpointAddress IssuerAddress
        {
            get
            {
                return this.federatedTokenProvider.IssuerAddress;
            }
            set
            {
                this.federatedTokenProvider.IssuerAddress = value;
            }
        }

        public EndpointAddress TargetAddress
        {
            get
            {
                return this.federatedTokenProvider.TargetAddress;
            }
            set
            {
                this.federatedTokenProvider.TargetAddress = value;
            }
        }

        public SecurityKeyEntropyMode KeyEntropyMode
        {
            get
            {
                return this.federatedTokenProvider.KeyEntropyMode;
            }
            set
            {
                this.federatedTokenProvider.KeyEntropyMode = value;
            }
        }

        public IdentityVerifier IdentityVerifier
        {
            get
            {
                return this.federatedTokenProvider.IdentityVerifier;
            }
            set
            {
                this.federatedTokenProvider.IdentityVerifier = value;
            }
        }

        public bool CacheIssuedTokens
        {
            get
            {
                return this.federatedTokenProvider.CacheServiceTokens;
            }
            set
            {
                this.federatedTokenProvider.CacheServiceTokens = value;
            }
        }

        public TimeSpan MaxIssuedTokenCachingTime
        {
            get
            {
                return this.federatedTokenProvider.MaxServiceTokenCachingTime;
            }
            set
            {
                this.federatedTokenProvider.MaxServiceTokenCachingTime = value;
            }
        }

        public MessageSecurityVersion MessageSecurityVersion
        {
            get
            {
                return this.messageSecurityVersion;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                }
                this.messageSecurityVersion = value;
            }
        }

        public SecurityTokenSerializer SecurityTokenSerializer
        {
            get
            {
                return this.securityTokenSerializer;
            }
            set
            {
                this.securityTokenSerializer = value;
            }
        }

        public SecurityAlgorithmSuite SecurityAlgorithmSuite
        {
            get
            {
                return this.federatedTokenProvider.SecurityAlgorithmSuite;
            }
            set
            {
                this.federatedTokenProvider.SecurityAlgorithmSuite = value;
            }
        }

        public int IssuedTokenRenewalThresholdPercentage
        {
            get
            {
                return this.federatedTokenProvider.ServiceTokenValidityThresholdPercentage;
            }
            set
            {
                this.federatedTokenProvider.ServiceTokenValidityThresholdPercentage = value;
            }
        }

        public CommunicationState State
        {
            get { return this.federatedTokenProvider.State; }
        }

        public virtual TimeSpan DefaultOpenTimeout
        {
            get { return ServiceDefaults.OpenTimeout; }
        }

        public virtual TimeSpan DefaultCloseTimeout
        {
            get { return ServiceDefaults.CloseTimeout; }
        }

        public override bool SupportsTokenCancellation
        {
            get
            {
                return this.federatedTokenProvider.SupportsTokenCancellation;
            }
        }

        internal ChannelParameterCollection ChannelParameters
        {
            get
            {
                return this.federatedTokenProvider.ChannelParameters;
            }
            set
            {
                this.federatedTokenProvider.ChannelParameters = value;
            }
        }

        internal SecurityTokenHandlerCollectionManager TokenHandlerCollectionManager
        {
            get
            {
                return this.tokenHandlerCollectionManager;
            }
            set
            {
                this.tokenHandlerCollectionManager = value;
            }
        }

        // communication object methods
        public void Abort()
        {
            this.federatedTokenProvider.Abort();
        }

        public void Close()
        {
            this.federatedTokenProvider.Close();
        }

        public void Close(TimeSpan timeout)
        {
            this.federatedTokenProvider.Close(timeout);
        }

        public IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            return this.federatedTokenProvider.BeginClose(callback, state);
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.federatedTokenProvider.BeginClose(timeout, callback, state);
        }

        public void EndClose(IAsyncResult result)
        {
            this.federatedTokenProvider.EndClose(result);
        }

        void OnOpenCore()
        {
            if (this.securityTokenSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.TokenSerializerNotSetonFederationProvider)));
            }
            this.federatedTokenProvider.StandardsManager = new SecurityStandardsManager(this.messageSecurityVersion, this.securityTokenSerializer);
        }

        public void Open()
        {
            OnOpenCore();

            this.federatedTokenProvider.Open();
        }

        public void Open(TimeSpan timeout)
        {
            OnOpenCore();
            this.federatedTokenProvider.Open(timeout);
        }

        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            OnOpenCore();
            return this.federatedTokenProvider.BeginOpen(callback, state);
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            OnOpenCore();
            return this.federatedTokenProvider.BeginOpen(timeout, callback, state);
        }

        public void EndOpen(IAsyncResult result)
        {
            this.federatedTokenProvider.EndOpen(result);
        }

        public void Dispose()
        {
            this.Close();
        }

        // token provider methods

        protected override IAsyncResult BeginGetTokenCore(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.federatedTokenProvider.BeginGetToken(timeout, callback, state);
        }

        protected override SecurityToken GetTokenCore(TimeSpan timeout)
        {
            return this.federatedTokenProvider.GetToken(timeout);
        }

        protected override SecurityToken EndGetTokenCore(IAsyncResult result)
        {
            return this.federatedTokenProvider.EndGetToken(result);
        }

        internal void SetupActAsOnBehalfOfParameters(System.IdentityModel.Protocols.WSTrust.FederatedClientCredentialsParameters actAsOnBehalfOfParameters)
        {
            if (actAsOnBehalfOfParameters == null)
                return;

            if (actAsOnBehalfOfParameters.IssuedSecurityToken != null)
            {
                throw System.IdentityModel.DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.AuthFailed));
            }

            if (actAsOnBehalfOfParameters.OnBehalfOf != null)
            {
                if (MessageSecurityVersion.TrustVersion == TrustVersion.WSTrust13)
                {
                    if (TokenRequestParameterExists(WSTrust13Constants.ElementNames.OnBehalfOf, WSTrust13Constants.NamespaceURI))
                    {
                        throw System.IdentityModel.DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.DuplicateFederatedClientCredentialsParameters, WSTrust13Constants.ElementNames.OnBehalfOf));
                    }

                    TokenRequestParameters.Add(CreateXmlTokenElement(actAsOnBehalfOfParameters.OnBehalfOf,
                                                                   WSTrust13Constants.Prefix,
                                                                   WSTrust13Constants.ElementNames.OnBehalfOf,
                                                                   WSTrust13Constants.NamespaceURI,
                                                                   SecurityTokenHandlerCollectionManager.Usage.OnBehalfOf));

                }
                else if (MessageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005)
                {
                    if (TokenRequestParameterExists(WSTrustFeb2005Constants.ElementNames.OnBehalfOf, WSTrustFeb2005Constants.NamespaceURI))
                    {
                        throw System.IdentityModel.DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.DuplicateFederatedClientCredentialsParameters, WSTrustFeb2005Constants.ElementNames.OnBehalfOf));
                    }

                    TokenRequestParameters.Add(CreateXmlTokenElement(actAsOnBehalfOfParameters.OnBehalfOf,
                                                                   WSTrustFeb2005Constants.Prefix,
                                                                   WSTrustFeb2005Constants.ElementNames.OnBehalfOf,
                                                                   WSTrustFeb2005Constants.NamespaceURI,
                                                                   SecurityTokenHandlerCollectionManager.Usage.OnBehalfOf));

                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.UnsupportedTrustVersion, MessageSecurityVersion.TrustVersion.Namespace)));
                }
            }
            if (actAsOnBehalfOfParameters.ActAs != null)
            {
                if (TokenRequestParameterExists(WSTrust14Constants.ElementNames.ActAs, WSTrust14Constants.NamespaceURI))
                {
                    throw System.IdentityModel.DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.DuplicateFederatedClientCredentialsParameters, WSTrust14Constants.ElementNames.ActAs));
                }

                TokenRequestParameters.Add(CreateXmlTokenElement(actAsOnBehalfOfParameters.ActAs,
                                                                   WSTrust14Constants.Prefix,
                                                                   WSTrust14Constants.ElementNames.ActAs,
                                                                   WSTrust14Constants.NamespaceURI,
                                                                   SecurityTokenHandlerCollectionManager.Usage.ActAs));
            }
        }

        bool TokenRequestParameterExists(string localName, string xmlNamespace)
        {
            foreach (XmlElement element in TokenRequestParameters)
            {
                if (element.LocalName == localName &&
                    element.NamespaceURI == xmlNamespace)
                {
                    return true;
                }
            }
            return false;
        }

        XmlElement CreateXmlTokenElement(SecurityToken token, string prefix, string name, string ns, string usage)
        {
            Stream stream = new MemoryStream();

            using (XmlDictionaryWriter xmlWriter = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8, false))
            {
                xmlWriter.WriteStartElement(prefix, name, ns);
                WriteToken(xmlWriter, token, usage);
                xmlWriter.WriteEndElement();
                xmlWriter.Flush();
            }

            stream.Seek(0, SeekOrigin.Begin);

            XmlDocument dom = new XmlDocument();
            dom.PreserveWhitespace = true;
            dom.Load(new XmlTextReader(stream) { DtdProcessing = DtdProcessing.Prohibit });
            stream.Close();

            return dom.DocumentElement;
        }

        void WriteToken(XmlWriter xmlWriter, SecurityToken token, string usage)
        {
            SecurityTokenHandlerCollection tokenHandlerCollection = null;
            if (this.tokenHandlerCollectionManager.ContainsKey(usage))
            {
                tokenHandlerCollection = this.tokenHandlerCollectionManager[usage];
            }
            else
            {
                tokenHandlerCollection = this.tokenHandlerCollectionManager[SecurityTokenHandlerCollectionManager.Usage.Default];
            }

            if (tokenHandlerCollection != null && tokenHandlerCollection.CanWriteToken(token))
            {
                tokenHandlerCollection.WriteToken(xmlWriter, token);
            }
            else
            {
                SecurityTokenSerializer.WriteToken(xmlWriter, token);
            }
        }

        private class CoreFederatedTokenProvider : IssuanceTokenProviderBase<FederatedTokenProviderState>
        {
            internal const SecurityKeyEntropyMode defaultKeyEntropyMode = SecurityKeyEntropyMode.CombinedEntropy;
            static int MaxRsaSecurityTokenCacheSize = 1024;
            IChannelFactory<IRequestChannel> channelFactory;
            Binding issuerBinding;
            KeyedByTypeCollection<IEndpointBehavior> channelBehaviors;
            Collection<XmlElement> requestProperties = new Collection<XmlElement>();
            IdentityVerifier identityVerifier = IdentityVerifier.CreateDefault();
            bool addTargetServiceAppliesTo;
            SecurityKeyEntropyMode keyEntropyMode;
            SecurityKeyType keyType;
            bool isKeyTypePresentInRstProperties;
            int keySize;
            bool isKeySizePresentInRstProperties;
            int defaultPublicKeySize = 1024;
            MessageVersion messageVersion;
            ChannelParameterCollection channelParameters;
            readonly List<RsaSecurityToken> rsaSecurityTokens = new List<RsaSecurityToken>();
            SafeFreeCredentials credentialsHandle;
            bool ownCredentialsHandle;

            public CoreFederatedTokenProvider(SafeFreeCredentials credentialsHandle) : base()
            {
                this.credentialsHandle = credentialsHandle;
                this.channelBehaviors = new KeyedByTypeCollection<IEndpointBehavior>();
                this.addTargetServiceAppliesTo = true;
                this.keyEntropyMode = defaultKeyEntropyMode;
            }

            public Binding IssuerBinding
            {
                get
                {
                    return this.issuerBinding;
                }
                set
                {
                    this.CommunicationObject.ThrowIfDisposedOrImmutable();
                    this.issuerBinding = value;
                }
            }

            public Collection<XmlElement> RequestProperties
            {
                get
                {
                    return this.requestProperties;
                }
            }

            public SecurityKeyEntropyMode KeyEntropyMode
            {
                get
                {
                    return this.keyEntropyMode;
                }
                set
                {
                    this.CommunicationObject.ThrowIfDisposedOrImmutable();
                    SecurityKeyEntropyModeHelper.Validate(value);
                    this.keyEntropyMode = value;
                }
            }

            public IdentityVerifier IdentityVerifier
            {
                get
                {
                    return this.identityVerifier;
                }
                set
                {
                    this.CommunicationObject.ThrowIfDisposedOrImmutable();
                    this.identityVerifier = value;
                }
            }

            public ChannelParameterCollection ChannelParameters
            {
                get
                {
                    return this.channelParameters;
                }
                set
                {
                    this.CommunicationObject.ThrowIfDisposedOrImmutable();
                    this.channelParameters = value;
                }
            }

            public KeyedByTypeCollection<IEndpointBehavior> IssuerChannelBehaviors
            {
                get
                {
                    return this.channelBehaviors;
                }
            }

            public override XmlDictionaryString RequestSecurityTokenAction
            {
                get
                {
                    return this.StandardsManager.TrustDriver.RequestSecurityTokenAction;
                }
            }

            public override XmlDictionaryString RequestSecurityTokenResponseAction
            {
                get
                {
                    return this.StandardsManager.TrustDriver.RequestSecurityTokenResponseAction;
                }
            }


            protected override MessageVersion MessageVersion
            {
                get
                {
                    return this.messageVersion;
                }
            }

            protected override bool RequiresManualReplyAddressing
            {
                get 
                {
                    // the proxy adds reply headers automatically
                    return false;
                }
            }

            bool TryGetKeyType(out SecurityKeyType keyType)
            {
                if (this.requestProperties != null)
                {
                    for (int i = 0; i < this.requestProperties.Count; ++i)
                    {
                        if (this.StandardsManager.TrustDriver.TryParseKeyTypeElement(this.requestProperties[i], out keyType))
                        {
                            return true;
                        }
                    }
                }
                keyType = SecurityKeyType.SymmetricKey;
                return false;
            }

            bool TryGetKeySize(out int keySize)
            {
                if (this.requestProperties != null)
                {
                    for (int i = 0; i < this.requestProperties.Count; ++i)
                    {
                        if (this.StandardsManager.TrustDriver.TryParseKeySizeElement(this.requestProperties[i], out keySize))
                        {
                            return true;
                        }
                    }
                }
                keySize = 0;
                return false;
            }

            public override void OnOpen(TimeSpan timeout)
            {
                if (this.IssuerAddress == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.StsAddressNotSet, this.TargetAddress)));
                }
                if (this.IssuerBinding == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.StsBindingNotSet, this.IssuerAddress)));
                }
                if (this.SecurityAlgorithmSuite == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SecurityAlgorithmSuiteNotSet, typeof(IssuedSecurityTokenProvider))));
                }
                this.channelFactory = this.StandardsManager.TrustDriver.CreateFederationProxy(this.IssuerAddress, this.IssuerBinding, this.IssuerChannelBehaviors);
                this.messageVersion = this.IssuerBinding.MessageVersion;

                // if an appliesTo is specified in the request properties, then do not add the target service EPR as
                // appliesTo
                for (int i = 0; i < this.requestProperties.Count; ++i)
                {
                    if (this.StandardsManager.TrustDriver.IsAppliesTo(this.requestProperties[i].LocalName, this.requestProperties[i].NamespaceURI))
                    {
                        this.addTargetServiceAppliesTo = false;
                        break;
                    }
                }
                this.isKeyTypePresentInRstProperties = TryGetKeyType(out this.keyType);
                if (!this.isKeyTypePresentInRstProperties)
                {
                    this.keyType = SecurityKeyType.SymmetricKey;
                }
                this.isKeySizePresentInRstProperties = TryGetKeySize(out this.keySize);
                if (!this.isKeySizePresentInRstProperties && this.keyType != SecurityKeyType.BearerKey)
                {
                    this.keySize = (this.keyType == SecurityKeyType.SymmetricKey) ? this.SecurityAlgorithmSuite.DefaultSymmetricKeyLength : this.defaultPublicKeySize;
                }

                base.OnOpen(timeout);
            }

            public override void OnOpening()
            {
                base.OnOpening();
                if (this.credentialsHandle == null)
                {
                    if (this.IssuerBinding == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.StsBindingNotSet, this.IssuerAddress)));
                    }
                    this.credentialsHandle = SecurityUtils.GetCredentialsHandle(this.IssuerBinding, this.IssuerChannelBehaviors);
                    this.ownCredentialsHandle = true;
                }
            }

            public override void OnAbort()
            {
                if (this.channelFactory != null && this.channelFactory.State == CommunicationState.Opened)
                {
                    this.channelFactory.Abort();
                    this.channelFactory = null;
                }
                CleanUpRsaSecurityTokenCache();
                FreeCredentialsHandle();
                base.OnAbort();
            }

            public override void OnClose(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                if (this.channelFactory != null && this.channelFactory.State == CommunicationState.Opened)
                {
                    this.channelFactory.Close(timeoutHelper.RemainingTime());
                    this.channelFactory = null;
                    CleanUpRsaSecurityTokenCache();
                    FreeCredentialsHandle();
                    base.OnClose(timeoutHelper.RemainingTime());
                }
            }

            void FreeCredentialsHandle()
            {
                if (this.credentialsHandle != null)
                {
                    if (this.ownCredentialsHandle)
                    {
                        this.credentialsHandle.Close();
                    }
                    this.credentialsHandle = null;
                }
            }

            protected override bool WillInitializeChannelFactoriesCompleteSynchronously(EndpointAddress target)
            {
                return (this.channelFactory.State != CommunicationState.Opened);
            }

            protected override void InitializeChannelFactories(EndpointAddress target, TimeSpan timeout)
            {
                if (this.channelFactory.State == CommunicationState.Created)
                {
                    this.channelFactory.Open(timeout); 
                }
            }

            protected override IAsyncResult BeginInitializeChannelFactories(EndpointAddress target, TimeSpan timeout, AsyncCallback callback, object state)
            {
                if (this.channelFactory.State == CommunicationState.Created)
                {
                    return this.channelFactory.BeginOpen(timeout, callback, state); 
                }
                else
                {
                    return new CompletedAsyncResult(callback, state);
                }
            }

            protected override void EndInitializeChannelFactories(IAsyncResult result)
            {
                if (result is CompletedAsyncResult)
                {
                    CompletedAsyncResult.End(result);
                }
                else
                {
                    this.channelFactory.EndOpen(result);
                }
            }

            protected override IRequestChannel CreateClientChannel(EndpointAddress target, Uri via)
            {
                IRequestChannel result = this.channelFactory.CreateChannel(this.IssuerAddress);
                if (this.channelParameters != null)
                {
                    this.channelParameters.PropagateChannelParameters(result);
                }
                if (this.ownCredentialsHandle)
                {
                    ChannelParameterCollection newParameters = result.GetProperty<ChannelParameterCollection>();
                    if (newParameters != null)
                    {
                        newParameters.Add(new SspiIssuanceChannelParameter(true, this.credentialsHandle));
                    }
                }
                ReplaceSspiIssuanceChannelParameter(result.GetProperty<ChannelParameterCollection>(), new SspiIssuanceChannelParameter(true, this.credentialsHandle));

                return result;
            }

            void ReplaceSspiIssuanceChannelParameter( ChannelParameterCollection channelParameters, SspiIssuanceChannelParameter sicp )
            {
                if (channelParameters != null)
                {
                    for (int i = 0; i < channelParameters.Count; ++i)
                    {
                        if (channelParameters[i] is SspiIssuanceChannelParameter)
                        {
                            channelParameters.RemoveAt(i);
                        }
                    }

                    channelParameters.Add(sicp);
                }
            }

            protected override bool CreateNegotiationStateCompletesSynchronously(EndpointAddress target, Uri via)
            {
                return true;
            }

            protected override IAsyncResult BeginCreateNegotiationState(EndpointAddress target, Uri via, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CompletedAsyncResult<FederatedTokenProviderState>(this.CreateNegotiationState(target, via, timeout), callback, state);
            }

            protected override FederatedTokenProviderState EndCreateNegotiationState(IAsyncResult result)
            {
                return CompletedAsyncResult<FederatedTokenProviderState>.End(result);
            }

            protected override FederatedTokenProviderState CreateNegotiationState(EndpointAddress target, Uri via, TimeSpan timeout)
            {
                if ((this.keyType == SecurityKeyType.SymmetricKey) || (this.keyType == SecurityKeyType.BearerKey))
                {
                    byte[] keyEntropy;
                    if (this.KeyEntropyMode == SecurityKeyEntropyMode.CombinedEntropy || this.KeyEntropyMode == SecurityKeyEntropyMode.ClientEntropy)
                    {
                        keyEntropy = new byte[this.keySize / 8];
                        CryptoHelper.FillRandomBytes(keyEntropy);
                    }
                    else
                    {
                        keyEntropy = null;
                    }
                    return new FederatedTokenProviderState(keyEntropy);
                }
                else if (this.keyType == SecurityKeyType.AsymmetricKey)
                {
                    return new FederatedTokenProviderState(CreateAndCacheRsaSecurityToken());
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
            }

            protected override BodyWriter GetFirstOutgoingMessageBody(FederatedTokenProviderState negotiationState, out MessageProperties messageProperties)
            {
                messageProperties = null;
                RequestSecurityToken rst = new RequestSecurityToken(this.StandardsManager);
                if (this.addTargetServiceAppliesTo)
                {
                    if (this.MessageVersion.Addressing == AddressingVersion.WSAddressing10)
                    {
                        rst.SetAppliesTo<EndpointAddress10>(
                            EndpointAddress10.FromEndpointAddress(negotiationState.TargetAddress),
                            DataContractSerializerDefaults.CreateSerializer(typeof(EndpointAddress10), DataContractSerializerDefaults.MaxItemsInObjectGraph));
                    }
                    else if (this.MessageVersion.Addressing == AddressingVersion.WSAddressingAugust2004)
                    {
                        rst.SetAppliesTo<EndpointAddressAugust2004>(
                            EndpointAddressAugust2004.FromEndpointAddress(negotiationState.TargetAddress),
                            DataContractSerializerDefaults.CreateSerializer(typeof(EndpointAddressAugust2004), DataContractSerializerDefaults.MaxItemsInObjectGraph));
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new ProtocolException(SR.GetString(SR.AddressingVersionNotSupported, this.MessageVersion.Addressing)));
                    }
                }
                rst.Context = negotiationState.Context;
                if (!this.isKeySizePresentInRstProperties)
                {
                    rst.KeySize = this.keySize;
                }
                Collection<XmlElement> newRequestProperties = new Collection<XmlElement>();
                if (this.requestProperties != null)
                {
                    for (int i = 0; i < this.requestProperties.Count; ++i)
                    {
                        newRequestProperties.Add(this.requestProperties[i]);
                    }
                }
                if (!isKeyTypePresentInRstProperties)
                {
                    XmlElement keyTypeElement = this.StandardsManager.TrustDriver.CreateKeyTypeElement(this.keyType);
                    newRequestProperties.Insert(0, keyTypeElement);
                }
                if (this.keyType == SecurityKeyType.SymmetricKey)
                {
                    byte[] requestorEntropy = negotiationState.GetRequestorEntropy();
                    rst.SetRequestorEntropy(requestorEntropy);
                }
                else if (this.keyType == SecurityKeyType.AsymmetricKey)
                {
                    RsaKeyIdentifierClause rsaClause = new RsaKeyIdentifierClause(negotiationState.Rsa);
                    SecurityKeyIdentifier keyIdentifier = new SecurityKeyIdentifier(rsaClause);
                    newRequestProperties.Add(this.StandardsManager.TrustDriver.CreateUseKeyElement(keyIdentifier, this.StandardsManager));
                    RsaSecurityTokenParameters rsaParameters = new RsaSecurityTokenParameters();
                    rsaParameters.InclusionMode = SecurityTokenInclusionMode.Never;
                    rsaParameters.RequireDerivedKeys = false;
                    SupportingTokenSpecification rsaSpec = new SupportingTokenSpecification(negotiationState.RsaSecurityToken, EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance, SecurityTokenAttachmentMode.Endorsing, rsaParameters);
                    messageProperties = new MessageProperties();
                    SecurityMessageProperty security = new SecurityMessageProperty();
                    security.OutgoingSupportingTokens.Add(rsaSpec);
                    messageProperties.Security = security;
                }
                if (this.keyType == SecurityKeyType.SymmetricKey && this.KeyEntropyMode == SecurityKeyEntropyMode.CombinedEntropy)
                {
                    newRequestProperties.Add(this.StandardsManager.TrustDriver.CreateComputedKeyAlgorithmElement(this.StandardsManager.TrustDriver.ComputedKeyAlgorithm));
                }
                rst.RequestProperties = newRequestProperties;
                rst.MakeReadOnly();
                return rst;
            }

            protected ReadOnlyCollection<IAuthorizationPolicy> GetServiceAuthorizationPolicies(AcceleratedTokenProviderState negotiationState)
            {
                EndpointIdentity identity;
                if (this.identityVerifier.TryGetIdentity(negotiationState.TargetAddress, out identity))
                {
                    List<Claim> claims = new List<Claim>(1);
                    claims.Add(identity.IdentityClaim);

                    List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>(1);
                    policies.Add(new UnconditionalPolicy(SecurityUtils.CreateIdentity(identity.IdentityClaim.Resource.ToString()), 
                            new DefaultClaimSet(ClaimSet.System, claims)));
                    return policies.AsReadOnly();
                }
                else
                {
                    return EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
                }
            }

            protected override BodyWriter GetNextOutgoingMessageBody(Message incomingMessage, FederatedTokenProviderState negotiationState)
            {
                ThrowIfFault(incomingMessage, this.IssuerAddress);
                if ((this.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005 && incomingMessage.Headers.Action != this.StandardsManager.TrustDriver.RequestSecurityTokenResponseAction.Value) ||
                    (this.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrust13 && incomingMessage.Headers.Action != this.StandardsManager.TrustDriver.RequestSecurityTokenResponseFinalAction.Value) ||
                    incomingMessage.Headers.Action == null)
                {
                    throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.InvalidActionForNegotiationMessage, incomingMessage.Headers.Action)), incomingMessage);
                }
                RequestSecurityTokenResponse rstr = null;
                XmlDictionaryReader bodyReader = incomingMessage.GetReaderAtBodyContents();
                using (bodyReader)
                {
                    if (this.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005)
                        rstr = this.StandardsManager.TrustDriver.CreateRequestSecurityTokenResponse(bodyReader);
                    else if (this.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrust13)
                    {
                        RequestSecurityTokenResponseCollection rstrc = this.StandardsManager.TrustDriver.CreateRequestSecurityTokenResponseCollection(bodyReader);
                        foreach (RequestSecurityTokenResponse rstrItem in rstrc.RstrCollection)
                        {
                            if (rstr != null)
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.MoreThanOneRSTRInRSTRC)));
                            rstr = rstrItem;
                        }
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                    }

                    incomingMessage.ReadFromBodyContentsToEnd(bodyReader);
                }
                if (rstr.Context != negotiationState.Context)
                {
                    throw TraceUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.BadSecurityNegotiationContext)), incomingMessage);
                }
                GenericXmlSecurityToken serviceToken;
                if ((this.keyType == SecurityKeyType.SymmetricKey) ||
                    (this.keyType == SecurityKeyType.BearerKey))
                {
                    ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = GetServiceAuthorizationPolicies(negotiationState);
                    byte[] keyEntropy = negotiationState.GetRequestorEntropy();
                    serviceToken = rstr.GetIssuedToken(null, null, this.KeyEntropyMode, keyEntropy, null, authorizationPolicies, this.keySize, this.keyType == SecurityKeyType.BearerKey);
                }
                else if (this.keyType == SecurityKeyType.AsymmetricKey)
                {
                    serviceToken = rstr.GetIssuedToken(null, EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance, negotiationState.Rsa);
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
                }
                negotiationState.SetServiceToken(serviceToken);
                return null;
            }

            // SC/Trust workshop change to turn off context
            protected override bool IsMultiLegNegotiation
            {
                get { return false; }
            }

            // This is to address RSACryptoServiceProvider finalizer exception issue
            // Step 1. Create Rsa and force deterministic keypair gen in this calling context.
            // Step 2. Cache if the calling thread is under impersonation context.  The cache will 
            // be disposed on Close/Abort assuming same calling context thread as the one calling open.
            RsaSecurityToken CreateAndCacheRsaSecurityToken()
            {
                RsaSecurityToken token;
                // Cache only under impersonation context.
                // 1) set cacheSize less than 0, to ignore this new behavior at all.
                // 2) set cacheSize to 0, if token provider should not dispose issued tokens on close/abort.
                // 3) other than that, the token provider will track and dispose issued tokens as much.
                if (MaxRsaSecurityTokenCacheSize >= 0 && IsImpersonatedContext())
                {
                    // This will force deterministic keypair gen in this context.
                    token = RsaSecurityToken.CreateSafeRsaSecurityToken(this.keySize);
                    if (MaxRsaSecurityTokenCacheSize > 0)
                    {
                        lock (this.rsaSecurityTokens)
                        {
                            // Remove/Dispose the first token if cache is full.
                            // The first token (if not disposed) will rely on GC for finalization.
                            if (this.rsaSecurityTokens.Count >= MaxRsaSecurityTokenCacheSize)
                            {
                                this.rsaSecurityTokens.RemoveAt(0);
                            }
                            this.rsaSecurityTokens.Add(token);
                        }
                    }
                }
                else
                {
                    token = new RsaSecurityToken(new RSACryptoServiceProvider(this.keySize));
                }
                return token;
            }

            void CleanUpRsaSecurityTokenCache()
            {
                lock (this.rsaSecurityTokens)
                {
                    for (int i = 0; i < this.rsaSecurityTokens.Count; ++i)
                    {
                        this.rsaSecurityTokens[i].Dispose();
                    }
                    this.rsaSecurityTokens.Clear();
                }
            }

            // This api simply check if the calling thread is process primary thread.
            // We are not trying to be smart if the impersonation to the same user as 
            // process token since privileges could be different.
            bool IsImpersonatedContext()
            {
                SafeCloseHandle tokenHandle = null;
                if (!SafeNativeMethods.OpenCurrentThreadToken(
                                SafeNativeMethods.GetCurrentThread(),
                                TokenAccessLevels.Query,
                                true,
                                out tokenHandle))
                {
                    int error = Marshal.GetLastWin32Error();
                    Utility.CloseInvalidOutSafeHandle(tokenHandle);
                    if (error == (int)Win32Error.ERROR_NO_TOKEN)
                    {
                        return false;
                    }
                    System.ServiceModel.Dispatcher.ErrorBehavior.ThrowAndCatch(new Win32Exception(error));
                    return true;
                }
                tokenHandle.Close();
                return true;
            }

            protected override void ValidateKeySize(GenericXmlSecurityToken issuedToken)
            {
                if (this.keyType == SecurityKeyType.BearerKey)
                {
                    // We do not have a proof key associated with bearer 
                    // key type. So skip key size validation.
                    return;
                }
                base.ValidateKeySize(issuedToken);
            }

        }

        class FederatedTokenProviderState : AcceleratedTokenProviderState
        {
            RsaSecurityToken rsaToken;

            public FederatedTokenProviderState(byte[] entropy)
                : base(entropy)
            {
            }

            public FederatedTokenProviderState(RsaSecurityToken rsaToken)
                : base(null)
            {
                this.rsaToken = rsaToken;
            }

            public RSA Rsa
            {
                get { return this.rsaToken.Rsa; }
            }

            public RsaSecurityToken RsaSecurityToken
            {
                get { return this.rsaToken; }
            }
        }

    }
}
