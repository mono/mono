//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Net.Security;
    using System.Runtime;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Text;
    using System.Xml;

    public abstract class SecurityBindingElement : BindingElement
    {
        internal const string defaultAlgorithmSuiteString = ConfigurationStrings.Default;
        internal static readonly SecurityAlgorithmSuite defaultDefaultAlgorithmSuite = SecurityAlgorithmSuite.Default;
        internal const bool defaultIncludeTimestamp = true;
        internal const bool defaultAllowInsecureTransport = false;
        internal const MessageProtectionOrder defaultMessageProtectionOrder = MessageProtectionOrder.SignBeforeEncryptAndEncryptSignature;
        internal const bool defaultRequireSignatureConfirmation = false;
        internal const bool defaultEnableUnsecuredResponse = false;
        internal const bool defaultProtectTokens = false;
        
        SecurityAlgorithmSuite defaultAlgorithmSuite;
        SupportingTokenParameters endpointSupportingTokenParameters;
        SupportingTokenParameters optionalEndpointSupportingTokenParameters;
        bool includeTimestamp;
        SecurityKeyEntropyMode keyEntropyMode;
        Dictionary<string, SupportingTokenParameters> operationSupportingTokenParameters;
        Dictionary<string, SupportingTokenParameters> optionalOperationSupportingTokenParameters;
        LocalClientSecuritySettings localClientSettings;
        LocalServiceSecuritySettings localServiceSettings;
        MessageSecurityVersion messageSecurityVersion;
        SecurityHeaderLayout securityHeaderLayout;
        InternalDuplexBindingElement internalDuplexBindingElement;
        long maxReceivedMessageSize = TransportDefaults.MaxReceivedMessageSize;
        XmlDictionaryReaderQuotas readerQuotas;
        bool doNotEmitTrust = false; // true if user create a basic http standard binding, the custombinding equivalent will not set this flag 
        bool supportsExtendedProtectionPolicy;
        bool allowInsecureTransport;
        bool enableUnsecuredResponse;
        bool protectTokens = defaultProtectTokens;

        internal SecurityBindingElement()
            : base()
        {
            this.messageSecurityVersion = MessageSecurityVersion.Default;
            this.keyEntropyMode = AcceleratedTokenProvider.defaultKeyEntropyMode;
            this.includeTimestamp = defaultIncludeTimestamp;
            this.defaultAlgorithmSuite = defaultDefaultAlgorithmSuite;
            this.localClientSettings = new LocalClientSecuritySettings();
            this.localServiceSettings = new LocalServiceSecuritySettings();
            this.endpointSupportingTokenParameters = new SupportingTokenParameters();
            this.optionalEndpointSupportingTokenParameters = new SupportingTokenParameters();
            this.operationSupportingTokenParameters = new Dictionary<string, SupportingTokenParameters>();
            this.optionalOperationSupportingTokenParameters = new Dictionary<string, SupportingTokenParameters>();
            this.securityHeaderLayout = SecurityProtocolFactory.defaultSecurityHeaderLayout;
            this.allowInsecureTransport = defaultAllowInsecureTransport;
            this.enableUnsecuredResponse = defaultEnableUnsecuredResponse;
            this.protectTokens = defaultProtectTokens;
        }

        internal SecurityBindingElement(SecurityBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            if (elementToBeCloned == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("elementToBeCloned");

            this.defaultAlgorithmSuite = elementToBeCloned.defaultAlgorithmSuite;
            this.includeTimestamp = elementToBeCloned.includeTimestamp;
            this.keyEntropyMode = elementToBeCloned.keyEntropyMode;
            this.messageSecurityVersion = elementToBeCloned.messageSecurityVersion;
            this.securityHeaderLayout = elementToBeCloned.securityHeaderLayout;
            this.endpointSupportingTokenParameters = (SupportingTokenParameters)elementToBeCloned.endpointSupportingTokenParameters.Clone();
            this.optionalEndpointSupportingTokenParameters = (SupportingTokenParameters)elementToBeCloned.optionalEndpointSupportingTokenParameters.Clone();
            this.operationSupportingTokenParameters = new Dictionary<string, SupportingTokenParameters>();
            foreach (string key in elementToBeCloned.operationSupportingTokenParameters.Keys)
            {
                this.operationSupportingTokenParameters[key] = (SupportingTokenParameters)elementToBeCloned.operationSupportingTokenParameters[key].Clone();
            }
            this.optionalOperationSupportingTokenParameters = new Dictionary<string, SupportingTokenParameters>();
            foreach (string key in elementToBeCloned.optionalOperationSupportingTokenParameters.Keys)
            {
                this.optionalOperationSupportingTokenParameters[key] = (SupportingTokenParameters)elementToBeCloned.optionalOperationSupportingTokenParameters[key].Clone();
            }
            this.localClientSettings = (LocalClientSecuritySettings)elementToBeCloned.localClientSettings.Clone();
            this.localServiceSettings = (LocalServiceSecuritySettings)elementToBeCloned.localServiceSettings.Clone();
            this.internalDuplexBindingElement = elementToBeCloned.internalDuplexBindingElement;
            this.maxReceivedMessageSize = elementToBeCloned.maxReceivedMessageSize;
            this.readerQuotas = elementToBeCloned.readerQuotas;
            this.doNotEmitTrust = elementToBeCloned.doNotEmitTrust;
            this.allowInsecureTransport = elementToBeCloned.allowInsecureTransport;
            this.enableUnsecuredResponse = elementToBeCloned.enableUnsecuredResponse;
            this.supportsExtendedProtectionPolicy = elementToBeCloned.supportsExtendedProtectionPolicy;
            this.protectTokens = elementToBeCloned.protectTokens;
        }

        internal bool SupportsExtendedProtectionPolicy
        {
            get { return this.supportsExtendedProtectionPolicy; }
            set { this.supportsExtendedProtectionPolicy = value; }
        }

        public SupportingTokenParameters EndpointSupportingTokenParameters
        {
            get
            {
                return this.endpointSupportingTokenParameters;
            }
        }

        public SupportingTokenParameters OptionalEndpointSupportingTokenParameters
        {
            get
            {
                return this.optionalEndpointSupportingTokenParameters;
            }
        }


        public IDictionary<string, SupportingTokenParameters> OperationSupportingTokenParameters
        {
            get
            {
                return this.operationSupportingTokenParameters;
            }
        }

        public IDictionary<string, SupportingTokenParameters> OptionalOperationSupportingTokenParameters
        {
            get
            {
                return this.optionalOperationSupportingTokenParameters;
            }
        }

        public SecurityHeaderLayout SecurityHeaderLayout
        {
            get
            {
                return this.securityHeaderLayout;
            }
            set
            {
                if (!SecurityHeaderLayoutHelper.IsDefined(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));

                this.securityHeaderLayout = value;
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
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                this.messageSecurityVersion = value;
            }
        }

        public bool EnableUnsecuredResponse
        {
            get
            {
                return this.enableUnsecuredResponse;
            }
            set
            {
                this.enableUnsecuredResponse = value;
            }
        }

        public bool IncludeTimestamp
        {
            get
            {
                return this.includeTimestamp;
            }
            set
            {
                this.includeTimestamp = value;
            }
        }

        public bool AllowInsecureTransport
        {
            get
            {
                return this.allowInsecureTransport;
            }
            set
            {
                this.allowInsecureTransport = value;
            }
        }

        public SecurityAlgorithmSuite DefaultAlgorithmSuite
        {
            get
            {
                return this.defaultAlgorithmSuite;
            }
            set
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
                this.defaultAlgorithmSuite = value;
            }
        }

        public bool ProtectTokens
        {
            get
            {
                return this.protectTokens;
            }
            set
            {
                this.protectTokens = value;
            }
        }

        public LocalClientSecuritySettings LocalClientSettings
        {
            get
            {
                return this.localClientSettings;
            }
        }

        public LocalServiceSecuritySettings LocalServiceSettings
        {
            get
            {
                return this.localServiceSettings;
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
                if (!SecurityKeyEntropyModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.keyEntropyMode = value;
            }
        }

        internal virtual bool SessionMode
        {
            get { return false; }
        }

        internal virtual bool SupportsDuplex
        {
            get { return false; }
        }

        internal virtual bool SupportsRequestReply
        {
            get { return false; }
        }

        internal long MaxReceivedMessageSize
        {
            get { return this.maxReceivedMessageSize; }
            set { this.maxReceivedMessageSize = value; }
        }

        internal bool DoNotEmitTrust
        {
            get { return this.doNotEmitTrust; }
            set { this.doNotEmitTrust = value; }
        }

        internal XmlDictionaryReaderQuotas ReaderQuotas
        {
            get { return this.readerQuotas; }
            set { this.readerQuotas = value; }
        }

        void GetSupportingTokensCapabilities(ICollection<SecurityTokenParameters> parameters, out bool supportsClientAuth, out bool supportsWindowsIdentity)
        {
            supportsClientAuth = false;
            supportsWindowsIdentity = false;
            foreach (SecurityTokenParameters p in parameters)
            {
                if (p.SupportsClientAuthentication)
                    supportsClientAuth = true;
                if (p.SupportsClientWindowsIdentity)
                    supportsWindowsIdentity = true;
            }
        }

        void GetSupportingTokensCapabilities(SupportingTokenParameters requirements, out bool supportsClientAuth, out bool supportsWindowsIdentity)
        {
            supportsClientAuth = false;
            supportsWindowsIdentity = false;
            bool tmpSupportsClientAuth;
            bool tmpSupportsWindowsIdentity;
            this.GetSupportingTokensCapabilities(requirements.Endorsing, out tmpSupportsClientAuth, out tmpSupportsWindowsIdentity);
            supportsClientAuth = supportsClientAuth || tmpSupportsClientAuth;
            supportsWindowsIdentity = supportsWindowsIdentity || tmpSupportsWindowsIdentity;

            this.GetSupportingTokensCapabilities(requirements.SignedEndorsing, out tmpSupportsClientAuth, out tmpSupportsWindowsIdentity);
            supportsClientAuth = supportsClientAuth || tmpSupportsClientAuth;
            supportsWindowsIdentity = supportsWindowsIdentity || tmpSupportsWindowsIdentity;

            this.GetSupportingTokensCapabilities(requirements.SignedEncrypted, out tmpSupportsClientAuth, out tmpSupportsWindowsIdentity);
            supportsClientAuth = supportsClientAuth || tmpSupportsClientAuth;
            supportsWindowsIdentity = supportsWindowsIdentity || tmpSupportsWindowsIdentity;
        }

        internal void GetSupportingTokensCapabilities(out bool supportsClientAuth, out bool supportsWindowsIdentity)
        {
            this.GetSupportingTokensCapabilities(this.EndpointSupportingTokenParameters, out supportsClientAuth, out supportsWindowsIdentity);
        }

        // SecureConversation needs a demuxer below security to 1) demux between the security sessions and 2) demux the SCT issue and renewal messages
        // to the authenticator
        internal void AddDemuxerForSecureConversation(ChannelBuilder builder, BindingContext secureConversationBindingContext)
        {
            // add a demuxer element  right below security unless there's a demuxer already present below and the only 
            // binding elements between security and the demuxer are "ancillary" binding elements like message encoding element and
            // stream-security upgrade element. We could always add the channel demuxer below security but not doing so in the ancillary
            // binding elements case improves perf
            int numChannelDemuxersBelowSecurity = 0;
            bool doesBindingHaveShapeChangingElements = false;
            for (int i = 0; i < builder.Binding.Elements.Count; ++i)
            {
                if ((builder.Binding.Elements[i] is MessageEncodingBindingElement) || (builder.Binding.Elements[i] is StreamUpgradeBindingElement))
                {
                    continue;
                }
                if (builder.Binding.Elements[i] is ChannelDemuxerBindingElement)
                {
                    ++numChannelDemuxersBelowSecurity;
                }
                else if (builder.Binding.Elements[i] is TransportBindingElement)
                {
                    break;
                }
                else
                {
                    doesBindingHaveShapeChangingElements = true;
                }
            }
            if (numChannelDemuxersBelowSecurity == 1 && !doesBindingHaveShapeChangingElements)
            {
                return;
            }

            ChannelDemuxerBindingElement demuxer = new ChannelDemuxerBindingElement(false);
            demuxer.MaxPendingSessions = this.LocalServiceSettings.MaxPendingSessions;
            demuxer.PeekTimeout = this.LocalServiceSettings.NegotiationTimeout;

            builder.Binding.Elements.Insert(0, demuxer);
            secureConversationBindingContext.RemainingBindingElements.Insert(0, demuxer);
        }

        internal void ApplyPropertiesOnDemuxer(ChannelBuilder builder, BindingContext context)
        {
            Collection<ChannelDemuxerBindingElement> demuxerElements = builder.Binding.Elements.FindAll<ChannelDemuxerBindingElement>();
            foreach (ChannelDemuxerBindingElement element in demuxerElements)
            {
                if (element != null)
                {
                    element.MaxPendingSessions = this.LocalServiceSettings.MaxPendingSessions;
                    element.PeekTimeout = this.LocalServiceSettings.NegotiationTimeout;
                }
            }
        }
        
        static BindingContext CreateIssuerBindingContextForNegotiation(BindingContext issuerBindingContext)
        {
            TransportBindingElement transport = issuerBindingContext.RemainingBindingElements.Find<TransportBindingElement>();
            if (transport == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.TransportBindingElementNotFound)));
            }
            ChannelDemuxerBindingElement demuxer = null;
            // pick the demuxer above transport (i.e. the last demuxer in the array)
            for (int i = 0; i < issuerBindingContext.RemainingBindingElements.Count; ++i)
            {
                if (issuerBindingContext.RemainingBindingElements[i] is ChannelDemuxerBindingElement)
                {
                    demuxer = (ChannelDemuxerBindingElement) issuerBindingContext.RemainingBindingElements[i];
                }
            }
            if (demuxer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ChannelDemuxerBindingElementNotFound)));
            }
            BindingElementCollection negotiationBindingElements = new BindingElementCollection();
            negotiationBindingElements.Add(demuxer.Clone());
            negotiationBindingElements.Add(transport.Clone());
            CustomBinding binding = new CustomBinding(negotiationBindingElements);
            binding.OpenTimeout = issuerBindingContext.Binding.OpenTimeout;
            binding.CloseTimeout = issuerBindingContext.Binding.CloseTimeout;
            binding.SendTimeout = issuerBindingContext.Binding.SendTimeout;
            binding.ReceiveTimeout = issuerBindingContext.Binding.ReceiveTimeout;
            if (issuerBindingContext.ListenUriBaseAddress != null)
            {
                return new BindingContext(binding, new BindingParameterCollection(issuerBindingContext.BindingParameters), issuerBindingContext.ListenUriBaseAddress,
                    issuerBindingContext.ListenUriRelativeAddress, issuerBindingContext.ListenUriMode);
            }
            else
            {
                return new BindingContext(binding, new BindingParameterCollection(issuerBindingContext.BindingParameters));
            }
        }

        protected static void SetIssuerBindingContextIfRequired(SecurityTokenParameters parameters, BindingContext issuerBindingContext)
        {
            if (parameters is SslSecurityTokenParameters)
            {
                ((SslSecurityTokenParameters)parameters).IssuerBindingContext = CreateIssuerBindingContextForNegotiation(issuerBindingContext);
            }
            else if (parameters is SspiSecurityTokenParameters)
            {
                ((SspiSecurityTokenParameters)parameters).IssuerBindingContext = CreateIssuerBindingContextForNegotiation(issuerBindingContext);
            }
        }

        static void SetIssuerBindingContextIfRequired(SupportingTokenParameters supportingParameters, BindingContext issuerBindingContext)
        {
            for (int i = 0; i < supportingParameters.Endorsing.Count; ++i)
            {
                SetIssuerBindingContextIfRequired(supportingParameters.Endorsing[i], issuerBindingContext);
            }
            for (int i = 0; i < supportingParameters.SignedEndorsing.Count; ++i)
            {
                SetIssuerBindingContextIfRequired(supportingParameters.SignedEndorsing[i], issuerBindingContext);
            }
            for (int i = 0; i < supportingParameters.Signed.Count; ++i)
            {
                SetIssuerBindingContextIfRequired(supportingParameters.Signed[i], issuerBindingContext);
            }
            for (int i = 0; i < supportingParameters.SignedEncrypted.Count; ++i)
            {
                SetIssuerBindingContextIfRequired(supportingParameters.SignedEncrypted[i], issuerBindingContext);
            }
        }

        void SetIssuerBindingContextIfRequired(BindingContext issuerBindingContext)
        {
            SetIssuerBindingContextIfRequired(this.EndpointSupportingTokenParameters, issuerBindingContext);
            SetIssuerBindingContextIfRequired(this.OptionalEndpointSupportingTokenParameters, issuerBindingContext);
            foreach (SupportingTokenParameters parameters in this.OperationSupportingTokenParameters.Values)
            {
                SetIssuerBindingContextIfRequired(parameters, issuerBindingContext);
            }
            foreach (SupportingTokenParameters parameters in this.OptionalOperationSupportingTokenParameters.Values)
            {
                SetIssuerBindingContextIfRequired(parameters, issuerBindingContext);
            }
        }

        internal bool RequiresChannelDemuxer(SecurityTokenParameters parameters)
        {
            return ((parameters is SecureConversationSecurityTokenParameters)
                    || (parameters is SslSecurityTokenParameters)
                    || (parameters is SspiSecurityTokenParameters));
        }

        internal virtual bool RequiresChannelDemuxer()
        {
            foreach (SecurityTokenParameters parameters in EndpointSupportingTokenParameters.Endorsing)
            {
                if (RequiresChannelDemuxer(parameters))
                {
                    return true;
                }
            }
            foreach (SecurityTokenParameters parameters in EndpointSupportingTokenParameters.SignedEndorsing)
            {
                if (RequiresChannelDemuxer(parameters))
                {
                    return true;
                }
            }
            foreach (SecurityTokenParameters parameters in OptionalEndpointSupportingTokenParameters.Endorsing)
            {
                if (RequiresChannelDemuxer(parameters))
                {
                    return true;
                }
            }
            foreach (SecurityTokenParameters parameters in OptionalEndpointSupportingTokenParameters.SignedEndorsing)
            {
                if (RequiresChannelDemuxer(parameters))
                {
                    return true;
                }
            }
            foreach (SupportingTokenParameters supportingParameters in OperationSupportingTokenParameters.Values)
            {
                foreach (SecurityTokenParameters parameters in supportingParameters.Endorsing)
                {
                    if (RequiresChannelDemuxer(parameters))
                    {
                        return true;
                    }
                }
                foreach (SecurityTokenParameters parameters in supportingParameters.SignedEndorsing)
                {
                    if (RequiresChannelDemuxer(parameters))
                    {
                        return true;
                    }
                }
            }
            foreach (SupportingTokenParameters supportingParameters in OptionalOperationSupportingTokenParameters.Values)
            {
                foreach (SecurityTokenParameters parameters in supportingParameters.Endorsing)
                {
                    if (RequiresChannelDemuxer(parameters))
                    {
                        return true;
                    }
                }
                foreach (SecurityTokenParameters parameters in supportingParameters.SignedEndorsing)
                {
                    if (RequiresChannelDemuxer(parameters))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool IsUnderlyingListenerDuplex<TChannel>(BindingContext context)
        {
            return ((typeof(TChannel) == typeof(IDuplexSessionChannel)) && context.CanBuildInnerChannelListener<IDuplexChannel>()
                && !context.CanBuildInnerChannelListener<IDuplexSessionChannel>());
        }

        void SetPrivacyNoticeUriIfRequired(SecurityProtocolFactory factory, Binding binding)
        {
            PrivacyNoticeBindingElement privacyElement = binding.CreateBindingElements().Find<PrivacyNoticeBindingElement>();
            if (privacyElement != null)
            {
                factory.PrivacyNoticeUri = privacyElement.Url;
                factory.PrivacyNoticeVersion = privacyElement.Version;
            }
        }

        internal void ConfigureProtocolFactory(SecurityProtocolFactory factory, SecurityCredentialsManager credentialsManager, bool isForService, BindingContext issuerBindingContext, Binding binding)
        {
            if (factory == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("factory"));
            if (credentialsManager == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("credentialsManager"));

            factory.AddTimestamp = this.IncludeTimestamp;
            factory.IncomingAlgorithmSuite = this.DefaultAlgorithmSuite;
            factory.OutgoingAlgorithmSuite = this.DefaultAlgorithmSuite;
            factory.SecurityHeaderLayout = this.SecurityHeaderLayout;

            if (!isForService)
            {
                factory.TimestampValidityDuration = this.LocalClientSettings.TimestampValidityDuration;
                factory.DetectReplays = this.LocalClientSettings.DetectReplays;
                factory.MaxCachedNonces = this.LocalClientSettings.ReplayCacheSize;
                factory.MaxClockSkew = this.LocalClientSettings.MaxClockSkew;
                factory.ReplayWindow = this.LocalClientSettings.ReplayWindow;

                if (this.LocalClientSettings.DetectReplays)
                {
                  factory.NonceCache = this.LocalClientSettings.NonceCache;
                }
            }
            else
            {
                factory.TimestampValidityDuration = this.LocalServiceSettings.TimestampValidityDuration;
                factory.DetectReplays = this.LocalServiceSettings.DetectReplays;
                factory.MaxCachedNonces = this.LocalServiceSettings.ReplayCacheSize;
                factory.MaxClockSkew = this.LocalServiceSettings.MaxClockSkew;
                factory.ReplayWindow = this.LocalServiceSettings.ReplayWindow;

                if (this.LocalServiceSettings.DetectReplays)
                {
                   factory.NonceCache = this.LocalServiceSettings.NonceCache;
                }
            }
            
            factory.SecurityBindingElement = (SecurityBindingElement) this.Clone();
            factory.SecurityBindingElement.SetIssuerBindingContextIfRequired(issuerBindingContext);
            factory.SecurityTokenManager = credentialsManager.CreateSecurityTokenManager();
            SecurityTokenSerializer tokenSerializer = factory.SecurityTokenManager.CreateSecurityTokenSerializer(this.messageSecurityVersion.SecurityTokenVersion);
            factory.StandardsManager = new SecurityStandardsManager(this.messageSecurityVersion, tokenSerializer);
            if (!isForService)
            {
                SetPrivacyNoticeUriIfRequired(factory, binding);
            }
        }

        internal abstract SecurityProtocolFactory CreateSecurityProtocolFactory<TChannel>(BindingContext context, SecurityCredentialsManager credentialsManager,
        bool isForService, BindingContext issuanceBindingContext);

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");

            if (!this.CanBuildChannelFactory<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)), "TChannel"));
            }

            this.readerQuotas = context.GetInnerProperty<XmlDictionaryReaderQuotas>();
            if (readerQuotas == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.EncodingBindingElementDoesNotHandleReaderQuotas)));
            }

            TransportBindingElement transportBindingElement = null;
            
            if (context.RemainingBindingElements != null)
                transportBindingElement = context.RemainingBindingElements.Find<TransportBindingElement>();

            if (transportBindingElement != null)
                this.maxReceivedMessageSize = transportBindingElement.MaxReceivedMessageSize;

            IChannelFactory<TChannel> result = this.BuildChannelFactoryCore<TChannel>(context);

            // attach the ExtendedProtectionPolicy to the securityProtcolFactory so it will be 
            // available when building the channel.
            if (transportBindingElement != null)
            {
                SecurityChannelFactory<TChannel> scf = result as SecurityChannelFactory<TChannel>;
                if (scf != null && scf.SecurityProtocolFactory != null)
                {
                    scf.SecurityProtocolFactory.ExtendedProtectionPolicy = transportBindingElement.GetProperty<ExtendedProtectionPolicy>(context);
                }
            }

            return result;

        }

        protected abstract IChannelFactory<TChannel> BuildChannelFactoryCore<TChannel>(BindingContext context);

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");

            InternalDuplexBindingElement.AddDuplexFactorySupport(context, ref this.internalDuplexBindingElement);

            if (this.SessionMode)
            {
                return this.CanBuildSessionChannelFactory<TChannel>(context);
            }

            if (!context.CanBuildInnerChannelFactory<TChannel>())
            {
                return false;
            }

            return typeof(TChannel) == typeof(IOutputChannel) || typeof(TChannel) == typeof(IOutputSessionChannel) ||
                (this.SupportsDuplex && (typeof(TChannel) == typeof(IDuplexChannel) || typeof(TChannel) == typeof(IDuplexSessionChannel))) ||
                (this.SupportsRequestReply && (typeof(TChannel) == typeof(IRequestChannel) || typeof(TChannel) == typeof(IRequestSessionChannel)));
        }

        bool CanBuildSessionChannelFactory<TChannel>(BindingContext context)
        {
            if (!(context.CanBuildInnerChannelFactory<IRequestChannel>()
                || context.CanBuildInnerChannelFactory<IRequestSessionChannel>()
                || context.CanBuildInnerChannelFactory<IDuplexChannel>()
                || context.CanBuildInnerChannelFactory<IDuplexSessionChannel>()))
            {
                return false;
            }

            if (typeof(TChannel) == typeof(IRequestSessionChannel))
            {
                return (context.CanBuildInnerChannelFactory<IRequestChannel>() || context.CanBuildInnerChannelFactory<IRequestSessionChannel>());
            }
            else if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                return (context.CanBuildInnerChannelFactory<IDuplexChannel>() || context.CanBuildInnerChannelFactory<IDuplexSessionChannel>());
            }
            else
            {
                return false;
            }
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");

            if (!this.CanBuildChannelListener<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)), "TChannel"));
            }

            this.readerQuotas = context.GetInnerProperty<XmlDictionaryReaderQuotas>();
            if (readerQuotas == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.EncodingBindingElementDoesNotHandleReaderQuotas)));
            }

            TransportBindingElement transportBindingElement = null;
            if (context.RemainingBindingElements != null)
                transportBindingElement = context.RemainingBindingElements.Find<TransportBindingElement>();

            if (transportBindingElement != null)
                this.maxReceivedMessageSize = transportBindingElement.MaxReceivedMessageSize;

            return this.BuildChannelListenerCore<TChannel>(context);
        }

        protected abstract IChannelListener<TChannel> BuildChannelListenerCore<TChannel>(BindingContext context)
            where TChannel : class, IChannel;

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");

            InternalDuplexBindingElement.AddDuplexListenerSupport(context, ref this.internalDuplexBindingElement);

            if (this.SessionMode)
            {
                return this.CanBuildSessionChannelListener<TChannel>(context);
            }

            if (!context.CanBuildInnerChannelListener<TChannel>())
            {
                return false;
            }

            return typeof(TChannel) == typeof(IInputChannel) || typeof(TChannel) == typeof(IInputSessionChannel) ||
                (this.SupportsDuplex && (typeof(TChannel) == typeof(IDuplexChannel) || typeof(TChannel) == typeof(IDuplexSessionChannel))) ||
                (this.SupportsRequestReply && (typeof(TChannel) == typeof(IReplyChannel) || typeof(TChannel) == typeof(IReplySessionChannel)));
        }

        bool CanBuildSessionChannelListener<TChannel>(BindingContext context)
            where TChannel : class, IChannel
        {
            if (!(context.CanBuildInnerChannelListener<IReplyChannel>()
                || context.CanBuildInnerChannelListener<IReplySessionChannel>()
                || context.CanBuildInnerChannelListener<IDuplexChannel>()
                || context.CanBuildInnerChannelListener<IDuplexSessionChannel>()))
            {
                return false;
            }

            if (typeof(TChannel) == typeof(IReplySessionChannel))
            {
                return (context.CanBuildInnerChannelListener<IReplyChannel>() || context.CanBuildInnerChannelListener<IReplySessionChannel>());
            }
            else if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                return (context.CanBuildInnerChannelListener<IDuplexChannel>() || context.CanBuildInnerChannelListener<IDuplexSessionChannel>());
            }
            else
            {
                return false;
            }
        }

        public virtual void SetKeyDerivation(bool requireDerivedKeys)
        {
            this.EndpointSupportingTokenParameters.SetKeyDerivation(requireDerivedKeys);
            this.OptionalEndpointSupportingTokenParameters.SetKeyDerivation(requireDerivedKeys);
            foreach (SupportingTokenParameters t in this.OperationSupportingTokenParameters.Values)
                t.SetKeyDerivation(requireDerivedKeys);
            foreach (SupportingTokenParameters t in this.OptionalOperationSupportingTokenParameters.Values)
            {
                t.SetKeyDerivation(requireDerivedKeys);
            }
        }

        internal virtual bool IsSetKeyDerivation(bool requireDerivedKeys)
        {
            if (!this.EndpointSupportingTokenParameters.IsSetKeyDerivation(requireDerivedKeys))
                return false;

            if (!this.OptionalEndpointSupportingTokenParameters.IsSetKeyDerivation(requireDerivedKeys))
                return false;

            foreach (SupportingTokenParameters t in this.OperationSupportingTokenParameters.Values)
            {
                if (!t.IsSetKeyDerivation(requireDerivedKeys))
                    return false;
            }
            foreach (SupportingTokenParameters t in this.OptionalOperationSupportingTokenParameters.Values)
            {
                if (!t.IsSetKeyDerivation(requireDerivedKeys))
                    return false;
            }
            return true;
        }

        internal ChannelProtectionRequirements GetProtectionRequirements(AddressingVersion addressing, ProtectionLevel defaultProtectionLevel)
        {
            if (addressing == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("addressing");

            ChannelProtectionRequirements result = new ChannelProtectionRequirements();
            ProtectionLevel supportedRequestProtectionLevel = this.GetIndividualProperty<ISecurityCapabilities>().SupportedRequestProtectionLevel;
            ProtectionLevel supportedResponseProtectionLevel = this.GetIndividualProperty<ISecurityCapabilities>().SupportedResponseProtectionLevel;

            bool canSupportMoreThanTheDefault = 
                (ProtectionLevelHelper.IsStrongerOrEqual(supportedRequestProtectionLevel, defaultProtectionLevel)
                && ProtectionLevelHelper.IsStrongerOrEqual(supportedResponseProtectionLevel, defaultProtectionLevel));
            if (canSupportMoreThanTheDefault)
            {
                MessagePartSpecification signedParts = new MessagePartSpecification();
                MessagePartSpecification encryptedParts = new MessagePartSpecification();
                if (defaultProtectionLevel != ProtectionLevel.None)
                {
                    signedParts.IsBodyIncluded = true;
                    if (defaultProtectionLevel == ProtectionLevel.EncryptAndSign)
                    {
                        encryptedParts.IsBodyIncluded = true;
                    }
                }
                signedParts.MakeReadOnly();
                encryptedParts.MakeReadOnly();
                if (addressing.FaultAction != null)
                {
                    // Addressing faults
                    result.IncomingSignatureParts.AddParts(signedParts, addressing.FaultAction);
                    result.OutgoingSignatureParts.AddParts(signedParts, addressing.FaultAction);
                    result.IncomingEncryptionParts.AddParts(encryptedParts, addressing.FaultAction);
                    result.OutgoingEncryptionParts.AddParts(encryptedParts, addressing.FaultAction);
                }
                if (addressing.DefaultFaultAction != null)
                {
                    // Faults that do not specify a particular action
                    result.IncomingSignatureParts.AddParts(signedParts, addressing.DefaultFaultAction);
                    result.OutgoingSignatureParts.AddParts(signedParts, addressing.DefaultFaultAction);
                    result.IncomingEncryptionParts.AddParts(encryptedParts, addressing.DefaultFaultAction);
                    result.OutgoingEncryptionParts.AddParts(encryptedParts, addressing.DefaultFaultAction);
                }
                // Infrastructure faults
                result.IncomingSignatureParts.AddParts(signedParts, FaultCodeConstants.Actions.NetDispatcher);
                result.OutgoingSignatureParts.AddParts(signedParts, FaultCodeConstants.Actions.NetDispatcher);
                result.IncomingEncryptionParts.AddParts(encryptedParts, FaultCodeConstants.Actions.NetDispatcher);
                result.OutgoingEncryptionParts.AddParts(encryptedParts, FaultCodeConstants.Actions.NetDispatcher);
            }

            return result;
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return (T)(object)GetSecurityCapabilities(context);
            }
            else if (typeof(T) == typeof(IdentityVerifier))
            {
                return (T)(object)this.localClientSettings.IdentityVerifier;
            }
            else
            {
                return context.GetInnerProperty<T>();
            }
        }

        internal abstract ISecurityCapabilities GetIndividualISecurityCapabilities();

        ISecurityCapabilities GetSecurityCapabilities(BindingContext context)
        {
            ISecurityCapabilities thisSecurityCapability = this.GetIndividualISecurityCapabilities();
            ISecurityCapabilities lowerSecurityCapability = context.GetInnerProperty<ISecurityCapabilities>();
            if (lowerSecurityCapability == null)
            {
                return thisSecurityCapability;
            }
            else
            {
                bool supportsClientAuth = thisSecurityCapability.SupportsClientAuthentication;
                bool supportsClientWindowsIdentity = thisSecurityCapability.SupportsClientWindowsIdentity;
                bool supportsServerAuth = thisSecurityCapability.SupportsServerAuthentication || lowerSecurityCapability.SupportsServerAuthentication;
                ProtectionLevel requestProtectionLevel = ProtectionLevelHelper.Max(thisSecurityCapability.SupportedRequestProtectionLevel, lowerSecurityCapability.SupportedRequestProtectionLevel);
                ProtectionLevel responseProtectionLevel = ProtectionLevelHelper.Max(thisSecurityCapability.SupportedResponseProtectionLevel, lowerSecurityCapability.SupportedResponseProtectionLevel);
                return new SecurityCapabilities(supportsClientAuth, supportsServerAuth, supportsClientWindowsIdentity, requestProtectionLevel, responseProtectionLevel);
            }
        }

        // If any changes are made to this method, please make sure that they are
        // reflected in the corresponding IsMutualCertificateBinding() method.
        static public SecurityBindingElement CreateMutualCertificateBindingElement()
        {
            return CreateMutualCertificateBindingElement(MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11);
        }

        // this method reverses CreateMutualCertificateBindingElement() logic
        internal static bool IsMutualCertificateBinding(SecurityBindingElement sbe)
        {
            return IsMutualCertificateBinding(sbe, false);
        }

        static public AsymmetricSecurityBindingElement CreateCertificateSignatureBindingElement()
        {
            AsymmetricSecurityBindingElement result;

            result = new AsymmetricSecurityBindingElement(
                new X509SecurityTokenParameters( // recipient
                    X509KeyIdentifierClauseType.Any,
                    SecurityTokenInclusionMode.Never, false),
                new X509SecurityTokenParameters( // initiator
                    X509KeyIdentifierClauseType.Any,
                    SecurityTokenInclusionMode.AlwaysToRecipient, false));

            // this is a one way binding so the client cannot detect replays
            result.IsCertificateSignatureBinding = true;
            result.LocalClientSettings.DetectReplays = false;
            result.MessageProtectionOrder = MessageProtectionOrder.SignBeforeEncrypt;

            return result;
        }

        // If any changes are made to this method, please make sure that they are
        // reflected in the corresponding IsMutualCertificateBinding() method.
        static public SecurityBindingElement CreateMutualCertificateBindingElement(MessageSecurityVersion version)
        {
            return CreateMutualCertificateBindingElement(version, false);
        }

        // If any changes are made to this method, please make sure that they are
        // reflected in the corresponding IsMutualCertificateBinding() method.
        static public SecurityBindingElement CreateMutualCertificateBindingElement(MessageSecurityVersion version, bool allowSerializedSigningTokenOnReply)
        {
            if (version == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("version");
            }
            SecurityBindingElement result;

            if (version.SecurityVersion == SecurityVersion.WSSecurity10)
            {
                result = new AsymmetricSecurityBindingElement(
                    new X509SecurityTokenParameters( // recipient
                        X509KeyIdentifierClauseType.Any,
                        SecurityTokenInclusionMode.Never, 
                        false),
                    new X509SecurityTokenParameters( // initiator
                        X509KeyIdentifierClauseType.Any,
                        SecurityTokenInclusionMode.AlwaysToRecipient, false),
                    allowSerializedSigningTokenOnReply);
            }
            else
            {
                result = new SymmetricSecurityBindingElement(
                    new X509SecurityTokenParameters( // protection
                        X509KeyIdentifierClauseType.Thumbprint,
                        SecurityTokenInclusionMode.Never));
                result.EndpointSupportingTokenParameters.Endorsing.Add(
                    new X509SecurityTokenParameters(
                        X509KeyIdentifierClauseType.Thumbprint,
                        SecurityTokenInclusionMode.AlwaysToRecipient, 
                        false));
                ((SymmetricSecurityBindingElement)result).RequireSignatureConfirmation = true;
            }

            result.MessageSecurityVersion = version;

            return result;
        }

        // this method reverses CreateMutualCertificateDuplexBindingElement() logic

        internal static bool IsMutualCertificateDuplexBinding(SecurityBindingElement sbe)
        {

            // Do not check MessageSecurityVersion: it maybe changed by the wrapper element and gets checked later in the SecuritySection.AreBindingsMatching()

            AsymmetricSecurityBindingElement asbe = sbe as AsymmetricSecurityBindingElement;
            if (asbe != null)
            {
                X509SecurityTokenParameters recipient = asbe.RecipientTokenParameters as X509SecurityTokenParameters;
                if (recipient == null || (recipient.X509ReferenceStyle != X509KeyIdentifierClauseType.Any  && recipient.X509ReferenceStyle != X509KeyIdentifierClauseType.Thumbprint) || recipient.InclusionMode != SecurityTokenInclusionMode.AlwaysToInitiator)
                    return false;

                X509SecurityTokenParameters initiator = asbe.InitiatorTokenParameters as X509SecurityTokenParameters;
                if (initiator == null || (initiator.X509ReferenceStyle != X509KeyIdentifierClauseType.Any && initiator.X509ReferenceStyle != X509KeyIdentifierClauseType.Thumbprint) || initiator.InclusionMode != SecurityTokenInclusionMode.AlwaysToRecipient)
                    return false;

                if (!sbe.EndpointSupportingTokenParameters.IsEmpty())
                    return false;

                return true;
            }
            else
            {
                return false;
            }
        }

        // this method reverses CreateMutualCertificateBindingElement() logic
        internal static bool IsMutualCertificateBinding(SecurityBindingElement sbe, bool allowSerializedSigningTokenOnReply)
        {

            // Do not check MessageSecurityVersion: it maybe changed by the wrapper element and gets checked later in the SecuritySection.AreBindingsMatching()

            AsymmetricSecurityBindingElement asbe = sbe as AsymmetricSecurityBindingElement;
            if (asbe != null)
            {
                X509SecurityTokenParameters recipient = asbe.RecipientTokenParameters as X509SecurityTokenParameters;
                if (recipient == null || recipient.X509ReferenceStyle != X509KeyIdentifierClauseType.Any || recipient.InclusionMode != SecurityTokenInclusionMode.Never)
                    return false;

                X509SecurityTokenParameters initiator = asbe.InitiatorTokenParameters as X509SecurityTokenParameters;
                if (initiator == null || initiator.X509ReferenceStyle != X509KeyIdentifierClauseType.Any || initiator.InclusionMode != SecurityTokenInclusionMode.AlwaysToRecipient)
                    return false;

                if (!sbe.EndpointSupportingTokenParameters.IsEmpty())
                    return false;
            }
            else
            {
                SymmetricSecurityBindingElement ssbe = sbe as SymmetricSecurityBindingElement;
                if (ssbe == null)
                    return false;

                X509SecurityTokenParameters x509Parameters = ssbe.ProtectionTokenParameters as X509SecurityTokenParameters;
                if (x509Parameters == null || x509Parameters.X509ReferenceStyle != X509KeyIdentifierClauseType.Thumbprint || x509Parameters.InclusionMode != SecurityTokenInclusionMode.Never)
                    return false;

                SupportingTokenParameters parameters = sbe.EndpointSupportingTokenParameters;
                if (parameters.Signed.Count != 0 || parameters.SignedEncrypted.Count != 0 || parameters.Endorsing.Count != 1 || parameters.SignedEndorsing.Count != 0)
                    return false;

                x509Parameters = parameters.Endorsing[0] as X509SecurityTokenParameters;
                if (x509Parameters == null || x509Parameters.X509ReferenceStyle != X509KeyIdentifierClauseType.Thumbprint || x509Parameters.InclusionMode != SecurityTokenInclusionMode.AlwaysToRecipient)
                    return false;

                if (!ssbe.RequireSignatureConfirmation)
                    return false;

            }
            return true;
        }

        // If any changes are made to this method, please make sure that they are
        // reflected in the corresponding IsAnonymousForCertificateBinding() method.
        static public SymmetricSecurityBindingElement CreateAnonymousForCertificateBindingElement()
        {
            SymmetricSecurityBindingElement result;

            result = new SymmetricSecurityBindingElement(
                new X509SecurityTokenParameters( // protection
                    X509KeyIdentifierClauseType.Thumbprint,
                    SecurityTokenInclusionMode.Never));
            result.MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11;
            result.RequireSignatureConfirmation = true;

            return result;
        }

        // this method reverses CreateAnonymousForCertificateBindingElement() logic
        internal static bool IsAnonymousForCertificateBinding(SecurityBindingElement sbe)
        {
            SymmetricSecurityBindingElement ssbe = sbe as SymmetricSecurityBindingElement;
            if (ssbe == null)
                return false;

            if (!ssbe.RequireSignatureConfirmation)
                return false;

            // Do not check MessageSecurityVersion: it maybe changed by the wrapper element and gets checked later in the SecuritySection.AreBindingsMatching()

            X509SecurityTokenParameters x509Parameters = ssbe.ProtectionTokenParameters as X509SecurityTokenParameters;
            if (x509Parameters == null || x509Parameters.X509ReferenceStyle != X509KeyIdentifierClauseType.Thumbprint || x509Parameters.InclusionMode != SecurityTokenInclusionMode.Never)
                return false;

            if (!sbe.EndpointSupportingTokenParameters.IsEmpty())
                return false;

            return true;
        }

        static public AsymmetricSecurityBindingElement CreateMutualCertificateDuplexBindingElement()
        {
            return CreateMutualCertificateDuplexBindingElement(MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11);
        }

        static public AsymmetricSecurityBindingElement CreateMutualCertificateDuplexBindingElement(MessageSecurityVersion version)
        {
            if (version == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("version");
            }
            AsymmetricSecurityBindingElement result;

            if (version.SecurityVersion == SecurityVersion.WSSecurity10)
            {
                result = new AsymmetricSecurityBindingElement(
                    new X509SecurityTokenParameters( // recipient
                        X509KeyIdentifierClauseType.Any,
                        SecurityTokenInclusionMode.AlwaysToInitiator, 
                        false),
                    new X509SecurityTokenParameters( // initiator
                        X509KeyIdentifierClauseType.Any,
                        SecurityTokenInclusionMode.AlwaysToRecipient,
                        false));
            }
            else
            {
                result = new AsymmetricSecurityBindingElement(
                    new X509SecurityTokenParameters( // recipient
                        X509KeyIdentifierClauseType.Thumbprint,
                        SecurityTokenInclusionMode.AlwaysToInitiator, 
                        false),
                    new X509SecurityTokenParameters( // initiator
                        X509KeyIdentifierClauseType.Thumbprint,
                        SecurityTokenInclusionMode.AlwaysToRecipient,
                        false));
            }

            result.MessageSecurityVersion = version;

            return result;
        }

        // If any changes are made to this method, please make sure that they are
        // reflected in the corresponding IsUserNameForCertificateBinding() method.
        static public SymmetricSecurityBindingElement CreateUserNameForCertificateBindingElement()
        {
            SymmetricSecurityBindingElement result = new SymmetricSecurityBindingElement(
                new X509SecurityTokenParameters(
                    X509KeyIdentifierClauseType.Thumbprint,
                    SecurityTokenInclusionMode.Never));
            result.EndpointSupportingTokenParameters.SignedEncrypted.Add(
                new UserNameSecurityTokenParameters());
            result.MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11;

            return result;
        }

        // this method reverses CreateMutualCertificateBindingElement() logic
        internal static bool IsUserNameForCertificateBinding(SecurityBindingElement sbe)
        {
            // Do not check MessageSecurityVersion: it maybe changed by the wrapper element and gets checked later in the SecuritySection.AreBindingsMatching()

            SymmetricSecurityBindingElement ssbe = sbe as SymmetricSecurityBindingElement;
            if (ssbe == null)
                return false;

            X509SecurityTokenParameters x509Parameters = ssbe.ProtectionTokenParameters as X509SecurityTokenParameters;
            if (x509Parameters == null || x509Parameters.X509ReferenceStyle != X509KeyIdentifierClauseType.Thumbprint || x509Parameters.InclusionMode != SecurityTokenInclusionMode.Never)
                return false;

            SupportingTokenParameters parameters = sbe.EndpointSupportingTokenParameters;
            if (parameters.Signed.Count != 0 || parameters.SignedEncrypted.Count != 1 || parameters.Endorsing.Count != 0 || parameters.SignedEndorsing.Count != 0)
                return false;

            UserNameSecurityTokenParameters userNameParameters = parameters.SignedEncrypted[0] as UserNameSecurityTokenParameters;
            if (userNameParameters == null)
                return false;

            return true;
        }

        // If any changes are made to this method, please make sure that they are
        // reflected in the corresponding IsKerberosBinding() method.
        static public SymmetricSecurityBindingElement CreateKerberosBindingElement()
        {
            SymmetricSecurityBindingElement result = new SymmetricSecurityBindingElement(
                new KerberosSecurityTokenParameters());
            result.DefaultAlgorithmSuite = SecurityAlgorithmSuite.KerberosDefault;
            return result;
        }

        // this method reverses CreateMutualCertificateBindingElement() logic
        internal static bool IsKerberosBinding(SecurityBindingElement sbe)
        {
            // do not check DefaultAlgorithmSuite match: it is often changed by the caller of CreateKerberosBindingElement
            SymmetricSecurityBindingElement ssbe = sbe as SymmetricSecurityBindingElement;
            if (ssbe == null)
                return false;

            KerberosSecurityTokenParameters parameters = ssbe.ProtectionTokenParameters as KerberosSecurityTokenParameters;
            if (parameters == null)
                return false;

            if (!sbe.EndpointSupportingTokenParameters.IsEmpty())
                return false;

            return true;
        }

        static public SymmetricSecurityBindingElement CreateSspiNegotiationBindingElement()
        {
            return CreateSspiNegotiationBindingElement(SspiSecurityTokenParameters.defaultRequireCancellation);
        }

        // If any changes are made to this method, please make sure that they are
        // reflected in the corresponding IsSspiNegotiationBinding() method.
        static public SymmetricSecurityBindingElement CreateSspiNegotiationBindingElement(bool requireCancellation)
        {
            SymmetricSecurityBindingElement result = new SymmetricSecurityBindingElement(
                new SspiSecurityTokenParameters(requireCancellation));
            return result;
        }

        // this method reverses CreateMutualCertificateBindingElement() logic
        internal static bool IsSspiNegotiationBinding(SecurityBindingElement sbe, bool requireCancellation)
        {
            SymmetricSecurityBindingElement ssbe  = sbe as SymmetricSecurityBindingElement;

            if (ssbe == null)
                return false;

            if (!sbe.EndpointSupportingTokenParameters.IsEmpty())
                return false;

            SspiSecurityTokenParameters sspiParameters = ssbe.ProtectionTokenParameters as SspiSecurityTokenParameters;
            if (sspiParameters == null)
                return false;

            return sspiParameters.RequireCancellation == requireCancellation;
        }


        static public SymmetricSecurityBindingElement CreateSslNegotiationBindingElement(bool requireClientCertificate)
        {
            return CreateSslNegotiationBindingElement(requireClientCertificate, SslSecurityTokenParameters.defaultRequireCancellation);
        }

        // If any changes are made to this method, please make sure that they are
        // reflected in the corresponding IsSslNegotiationBinding() method.
        static public SymmetricSecurityBindingElement CreateSslNegotiationBindingElement(bool requireClientCertificate, bool requireCancellation)
        {
            SymmetricSecurityBindingElement result = new SymmetricSecurityBindingElement(
                new SslSecurityTokenParameters(requireClientCertificate, requireCancellation));
            return result;
        }

        // this method reverses CreateMutualCertificateBindingElement() logic
        internal static bool IsSslNegotiationBinding(SecurityBindingElement sbe, bool requireClientCertificate, bool requireCancellation)
        {
            SymmetricSecurityBindingElement ssbe = sbe as SymmetricSecurityBindingElement;
            if (ssbe == null)
                return false;

            if (!sbe.EndpointSupportingTokenParameters.IsEmpty())
                return false;

            SslSecurityTokenParameters sslParameters = ssbe.ProtectionTokenParameters as SslSecurityTokenParameters;
            if (sslParameters == null)
                return false;

            return sslParameters.RequireClientCertificate == requireClientCertificate && sslParameters.RequireCancellation == requireCancellation;

        }
        static public SymmetricSecurityBindingElement CreateIssuedTokenBindingElement(IssuedSecurityTokenParameters issuedTokenParameters)
        {
            if (issuedTokenParameters == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuedTokenParameters");
            if (issuedTokenParameters.KeyType != SecurityKeyType.SymmetricKey)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.IssuedTokenAuthenticationModeRequiresSymmetricIssuedKey));
            SymmetricSecurityBindingElement result = new SymmetricSecurityBindingElement(issuedTokenParameters);
            return result;
        }

        // If any changes are made to this method, please make sure that they are
        // reflected in the corresponding IsIssuedTokenForCertificateBinding() method.
        static public SymmetricSecurityBindingElement CreateIssuedTokenForCertificateBindingElement(IssuedSecurityTokenParameters issuedTokenParameters)
        {
            if (issuedTokenParameters == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuedTokenParameters");

            SymmetricSecurityBindingElement result = new SymmetricSecurityBindingElement(
                new X509SecurityTokenParameters(
                    X509KeyIdentifierClauseType.Thumbprint,
                    SecurityTokenInclusionMode.Never));
            if (issuedTokenParameters.KeyType == SecurityKeyType.BearerKey)
            {
                result.EndpointSupportingTokenParameters.SignedEncrypted.Add(issuedTokenParameters);
                result.MessageSecurityVersion = MessageSecurityVersion.WSSXDefault;
            }
            else
            {
                result.EndpointSupportingTokenParameters.Endorsing.Add(issuedTokenParameters);
                result.MessageSecurityVersion = MessageSecurityVersion.Default;
            }
            result.RequireSignatureConfirmation = true;
            return result;
        }

        // this method reverses CreateMutualCertificateBindingElement() logic
        internal static bool IsIssuedTokenForCertificateBinding(SecurityBindingElement sbe, out IssuedSecurityTokenParameters issuedTokenParameters)
        {
            issuedTokenParameters = null;
            SymmetricSecurityBindingElement ssbe = sbe as SymmetricSecurityBindingElement;
            if (ssbe == null)
                return false;

            if (!ssbe.RequireSignatureConfirmation)
                return false;

            // Do not check MessageSecurityVersion: it maybe changed by the wrapper element and gets checked later in the SecuritySection.AreBindingsMatching()

            X509SecurityTokenParameters x509Parameters = ssbe.ProtectionTokenParameters as X509SecurityTokenParameters;
            if (x509Parameters == null || x509Parameters.X509ReferenceStyle != X509KeyIdentifierClauseType.Thumbprint || x509Parameters.InclusionMode != SecurityTokenInclusionMode.Never)
                return false;

            SupportingTokenParameters parameters = ssbe.EndpointSupportingTokenParameters;
            if (parameters.Signed.Count != 0 || (parameters.SignedEncrypted.Count == 0 && parameters.Endorsing.Count == 0) || parameters.SignedEndorsing.Count != 0)
                return false;

            if ((parameters.SignedEncrypted.Count == 1) && (parameters.Endorsing.Count == 0))
            {
                issuedTokenParameters = parameters.SignedEncrypted[0] as IssuedSecurityTokenParameters;
                if (issuedTokenParameters != null && issuedTokenParameters.KeyType != SecurityKeyType.BearerKey)
                    return false;
            }
            else if ((parameters.Endorsing.Count == 1) && (parameters.SignedEncrypted.Count == 0))
            {
                issuedTokenParameters = parameters.Endorsing[0] as IssuedSecurityTokenParameters;
                if (issuedTokenParameters != null && (issuedTokenParameters.KeyType != SecurityKeyType.SymmetricKey && issuedTokenParameters.KeyType != SecurityKeyType.AsymmetricKey))
                    return false;
            }
            return (issuedTokenParameters != null);
        }

        // If any changes are made to this method, please make sure that they are
        // reflected in the corresponding IsIssuedTokenForSslBinding() method.
        static public SymmetricSecurityBindingElement CreateIssuedTokenForSslBindingElement(IssuedSecurityTokenParameters issuedTokenParameters)
        {
            return CreateIssuedTokenForSslBindingElement(issuedTokenParameters, SslSecurityTokenParameters.defaultRequireCancellation);
        }

        // this method reverses CreateMutualCertificateBindingElement() logic
        internal static bool IsIssuedTokenForSslBinding(SecurityBindingElement sbe, out IssuedSecurityTokenParameters issuedTokenParameters)
        {
            return IsIssuedTokenForSslBinding(sbe, SslSecurityTokenParameters.defaultRequireCancellation, out issuedTokenParameters);
        }

        // If any changes are made to this method, please make sure that they are
        // reflected in the corresponding IsIssuedTokenForSslBinding() method.
        static public SymmetricSecurityBindingElement CreateIssuedTokenForSslBindingElement(IssuedSecurityTokenParameters issuedTokenParameters, bool requireCancellation)
        {
            if (issuedTokenParameters == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuedTokenParameters");

            SymmetricSecurityBindingElement result = new SymmetricSecurityBindingElement(
                new SslSecurityTokenParameters(false, requireCancellation));
            if (issuedTokenParameters.KeyType == SecurityKeyType.BearerKey)
            {
                result.EndpointSupportingTokenParameters.SignedEncrypted.Add(issuedTokenParameters);
                result.MessageSecurityVersion = MessageSecurityVersion.WSSXDefault;
            }
            else
            {
                result.EndpointSupportingTokenParameters.Endorsing.Add(issuedTokenParameters);
                result.MessageSecurityVersion = MessageSecurityVersion.Default;
            }
            result.RequireSignatureConfirmation = true;
            return result;
        }

        // this method reverses CreateMutualCertificateBindingElement() logic
        internal static bool IsIssuedTokenForSslBinding(SecurityBindingElement sbe, bool requireCancellation, out IssuedSecurityTokenParameters issuedTokenParameters)
        {
            issuedTokenParameters = null;
            SymmetricSecurityBindingElement ssbe = sbe as SymmetricSecurityBindingElement;
            if (ssbe == null)
                return false;

            if (!ssbe.RequireSignatureConfirmation)
                return false;

            // Do not check MessageSecurityVersion: it maybe changed by the wrapper element and gets checked later in the SecuritySection.AreBindingsMatching()

            SslSecurityTokenParameters sslParameters = ssbe.ProtectionTokenParameters as SslSecurityTokenParameters;
            if (sslParameters == null)
                return false;

            if (sslParameters.RequireClientCertificate || sslParameters.RequireCancellation != requireCancellation)
                return false;

            SupportingTokenParameters parameters = ssbe.EndpointSupportingTokenParameters;
            if (parameters.Signed.Count != 0 || (parameters.SignedEncrypted.Count == 0 && parameters.Endorsing.Count == 0) || parameters.SignedEndorsing.Count != 0)
                return false;

            if ((parameters.SignedEncrypted.Count == 1) && (parameters.Endorsing.Count == 0))
            {
                issuedTokenParameters = parameters.SignedEncrypted[0] as IssuedSecurityTokenParameters;
                if (issuedTokenParameters != null && issuedTokenParameters.KeyType != SecurityKeyType.BearerKey)
                    return false;
            }
            else if ((parameters.Endorsing.Count == 1) && (parameters.SignedEncrypted.Count == 0))
            {
                issuedTokenParameters = parameters.Endorsing[0] as IssuedSecurityTokenParameters;
                if (issuedTokenParameters != null && (issuedTokenParameters.KeyType != SecurityKeyType.SymmetricKey && issuedTokenParameters.KeyType != SecurityKeyType.AsymmetricKey))
                    return false;
            }
            return (issuedTokenParameters != null);
        }

        static public SymmetricSecurityBindingElement CreateUserNameForSslBindingElement()
        {
            return CreateUserNameForSslBindingElement(SslSecurityTokenParameters.defaultRequireCancellation);
        }

        // If any changes are made to this method, please make sure that they are
        // reflected in the corresponding IsUserNameForSslBinding() method.
        static public SymmetricSecurityBindingElement CreateUserNameForSslBindingElement(bool requireCancellation)
        {
            SymmetricSecurityBindingElement result = new SymmetricSecurityBindingElement(
                new SslSecurityTokenParameters(false, requireCancellation));
            result.EndpointSupportingTokenParameters.SignedEncrypted.Add(
                new UserNameSecurityTokenParameters());
            result.MessageSecurityVersion = MessageSecurityVersion.WSSecurity11WSTrustFebruary2005WSSecureConversationFebruary2005WSSecurityPolicy11;

            return result;
        }

        // this method reverses CreateMutualCertificateBindingElement() logic
        internal static bool IsUserNameForSslBinding(SecurityBindingElement sbe, bool requireCancellation)
        {
            // Do not check MessageSecurityVersion: it maybe changed by the wrapper element and gets checked later in the SecuritySection.AreBindingsMatching()

            SymmetricSecurityBindingElement ssbe = sbe as SymmetricSecurityBindingElement;
            if (ssbe == null)
                return false;

            SupportingTokenParameters parameters = sbe.EndpointSupportingTokenParameters;
            if (parameters.Signed.Count != 0 || parameters.SignedEncrypted.Count != 1 || parameters.Endorsing.Count != 0 || parameters.SignedEndorsing.Count != 0)
                return false;

            if (!(parameters.SignedEncrypted[0] is UserNameSecurityTokenParameters))
                return false;

            SslSecurityTokenParameters sslParameters = ssbe.ProtectionTokenParameters as SslSecurityTokenParameters;
            if (sslParameters == null)
                return false;

            return sslParameters.RequireCancellation == requireCancellation && !sslParameters.RequireClientCertificate;
        }

        // If any changes are made to this method, please make sure that they are
        // reflected in the corresponding IsUserNameOverTransportBinding() method.
        static public TransportSecurityBindingElement CreateUserNameOverTransportBindingElement()
        {
            TransportSecurityBindingElement result = new TransportSecurityBindingElement();
            result.EndpointSupportingTokenParameters.SignedEncrypted.Add(
                new UserNameSecurityTokenParameters());
            result.IncludeTimestamp = true;
            result.LocalClientSettings.DetectReplays = false;
            result.LocalServiceSettings.DetectReplays = false;
            return result;
        }

        // this method reverses CreateMutualCertificateBindingElement() logic
        internal static bool IsUserNameOverTransportBinding(SecurityBindingElement sbe)
        {
            // do not check local settings: sbe.LocalServiceSettings and sbe.LocalClientSettings
            if (!sbe.IncludeTimestamp)
                return false;

            if (!(sbe is TransportSecurityBindingElement))
                return false;

            SupportingTokenParameters parameters = sbe.EndpointSupportingTokenParameters;
            if (parameters.Signed.Count != 0 || parameters.SignedEncrypted.Count != 1 || parameters.Endorsing.Count != 0 || parameters.SignedEndorsing.Count != 0)
                return false;

            UserNameSecurityTokenParameters userNameParameters = parameters.SignedEncrypted[0] as UserNameSecurityTokenParameters;
            if (userNameParameters == null)
                return false;

            return true;
        }

        // If any changes are made to this method, please make sure that they are
        // reflected in the corresponding IsCertificateOverTransportBinding() method.
        static public TransportSecurityBindingElement CreateCertificateOverTransportBindingElement()
        {
            return CreateCertificateOverTransportBindingElement(MessageSecurityVersion.Default);
        }

        // If any changes are made to this method, please make sure that they are
        // reflected in the corresponding IsCertificateOverTransportBinding() method.
        static public TransportSecurityBindingElement CreateCertificateOverTransportBindingElement(MessageSecurityVersion version)
        {
            if (version == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("version");
            }
            X509KeyIdentifierClauseType x509ReferenceType;

            if (version.SecurityVersion == SecurityVersion.WSSecurity10)
            {
                x509ReferenceType = X509KeyIdentifierClauseType.Any;
            }
            else
            {
                x509ReferenceType = X509KeyIdentifierClauseType.Thumbprint;
            }

            TransportSecurityBindingElement result = new TransportSecurityBindingElement();
            X509SecurityTokenParameters x509Parameters = new X509SecurityTokenParameters(
                    x509ReferenceType,
                    SecurityTokenInclusionMode.AlwaysToRecipient,
                    false);
            result.EndpointSupportingTokenParameters.Endorsing.Add(
                x509Parameters
                );
            result.IncludeTimestamp = true;
            result.LocalClientSettings.DetectReplays = false;
            result.LocalServiceSettings.DetectReplays = false;
            result.MessageSecurityVersion = version;

            return result;
        }

        // this method reverses CreateMutualCertificateBindingElement() logic
        internal static bool IsCertificateOverTransportBinding(SecurityBindingElement sbe)
        {
            // do not check local settings: sbe.LocalServiceSettings and sbe.LocalClientSettings
            if (!sbe.IncludeTimestamp)
                return false;

            if (!(sbe is TransportSecurityBindingElement))
                return false;

            SupportingTokenParameters parameters = sbe.EndpointSupportingTokenParameters;
            if (parameters.Signed.Count != 0 || parameters.SignedEncrypted.Count != 0 || parameters.Endorsing.Count != 1 || parameters.SignedEndorsing.Count != 0)
                return false;

            X509SecurityTokenParameters x509Parameters = parameters.Endorsing[0] as X509SecurityTokenParameters;
            if (x509Parameters == null)
                return false;

            if (x509Parameters.InclusionMode != SecurityTokenInclusionMode.AlwaysToRecipient)
                return false;

            return x509Parameters.X509ReferenceStyle == X509KeyIdentifierClauseType.Any || x509Parameters.X509ReferenceStyle == X509KeyIdentifierClauseType.Thumbprint;
        }

        static public TransportSecurityBindingElement CreateKerberosOverTransportBindingElement()
        {
            TransportSecurityBindingElement result = new TransportSecurityBindingElement();
            KerberosSecurityTokenParameters kerberosParameters = new KerberosSecurityTokenParameters();
            kerberosParameters.RequireDerivedKeys = false;
            result.EndpointSupportingTokenParameters.Endorsing.Add(
                kerberosParameters);
            result.IncludeTimestamp = true;
            result.LocalClientSettings.DetectReplays = false;
            result.LocalServiceSettings.DetectReplays = false;
            result.DefaultAlgorithmSuite = SecurityAlgorithmSuite.KerberosDefault;
            result.SupportsExtendedProtectionPolicy = true;

            return result;
        }
#if NO
        // this is reversing of the CreateKerberosOverTransportBindingElement() logic
        static bool IsKerberosOverTransportBinding(SecurityBindingElement sbe)
        {
            if (sbe.DefaultAlgorithmSuite != SecurityAlgorithmSuite.KerberosDefault)
                return false;

            // do not check local settings: sbe.LocalServiceSettings and sbe.LocalClientSettings

            if (!sbe.IncludeTimestamp)
                return false;

            if (!(sbe is TransportSecurityBindingElement))
                return false;

            SupportingTokenParameters parameters = sbe.EndpointSupportingTokenParameters;
            if (parameters.Signed.Count != 0 || parameters.SignedEncrypted.Count != 0 || parameters.Endorsing.Count != 1 || parameters.SignedEndorsing.Count != 0)
                return false;

            KerberosSecurityTokenParameters kerberosParameters = parameters.Endorsing[0] as KerberosSecurityTokenParameters;
            if (kerberosParameters == null)
                return false;

            if (kerberosParameters.RequireDerivedKeys)
                return false;

            return true;
        }
#endif
        static public TransportSecurityBindingElement CreateSspiNegotiationOverTransportBindingElement()
        {
            return CreateSspiNegotiationOverTransportBindingElement(true);
        }

        // If any changes are made to this method, please make sure that they are
        // reflected in the corresponding IsSspiNegotiationOverTransportBinding() method.
        static public TransportSecurityBindingElement CreateSspiNegotiationOverTransportBindingElement(bool requireCancellation)
        {
            TransportSecurityBindingElement result = new TransportSecurityBindingElement();
            SspiSecurityTokenParameters sspiParameters = new SspiSecurityTokenParameters(requireCancellation);
            sspiParameters.RequireDerivedKeys = false;
            result.EndpointSupportingTokenParameters.Endorsing.Add(
                sspiParameters);
            result.IncludeTimestamp = true;
            result.LocalClientSettings.DetectReplays = false;
            result.LocalServiceSettings.DetectReplays = false;
            result.SupportsExtendedProtectionPolicy = true;

            return result;
        }

        // this method reverses CreateSspiNegotiationOverTransportBindingElement() logic
        internal static bool IsSspiNegotiationOverTransportBinding(SecurityBindingElement sbe, bool requireCancellation)
        {
            // do not check local settings: sbe.LocalServiceSettings and sbe.LocalClientSettings

            if (!sbe.IncludeTimestamp)
                return false;

            SupportingTokenParameters parameters = sbe.EndpointSupportingTokenParameters;
            if (parameters.Signed.Count != 0 || parameters.SignedEncrypted.Count != 0 || parameters.Endorsing.Count != 1 || parameters.SignedEndorsing.Count != 0)
                return false;
            SspiSecurityTokenParameters sspiParameters = parameters.Endorsing[0] as SspiSecurityTokenParameters;
            if (sspiParameters == null)
                return false;

            if (sspiParameters.RequireDerivedKeys)
                return false;

            if (sspiParameters.RequireCancellation != requireCancellation)
                return false;

            if (!(sbe is TransportSecurityBindingElement))
                return false;

            return true;
        }

        // If any changes are made to this method, please make sure that they are
        // reflected in the corresponding IsIssuedTokenOverTransportBinding() method.
        static public TransportSecurityBindingElement CreateIssuedTokenOverTransportBindingElement(IssuedSecurityTokenParameters issuedTokenParameters)
        {
            if (issuedTokenParameters == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuedTokenParameters");

            issuedTokenParameters.RequireDerivedKeys = false;
            TransportSecurityBindingElement result = new TransportSecurityBindingElement();
            if (issuedTokenParameters.KeyType == SecurityKeyType.BearerKey)
            {
                result.EndpointSupportingTokenParameters.Signed.Add(issuedTokenParameters);
                result.MessageSecurityVersion = MessageSecurityVersion.WSSXDefault;
            }
            else
            {
                result.EndpointSupportingTokenParameters.Endorsing.Add(issuedTokenParameters);
                result.MessageSecurityVersion = MessageSecurityVersion.Default;
            }
            result.LocalClientSettings.DetectReplays = false;
            result.LocalServiceSettings.DetectReplays = false;
            result.IncludeTimestamp = true;

            return result;
        }

        // this method reverses CreateIssuedTokenOverTransportBindingElement() logic
        internal static bool IsIssuedTokenOverTransportBinding(SecurityBindingElement sbe, out IssuedSecurityTokenParameters issuedTokenParameters)
        {
            issuedTokenParameters = null;
            if (!(sbe is TransportSecurityBindingElement))
                return false;

            if (!sbe.IncludeTimestamp)
                return false;

            // do not check local settings: sbe.LocalServiceSettings and sbe.LocalClientSettings

            SupportingTokenParameters parameters = sbe.EndpointSupportingTokenParameters;
            if (parameters.SignedEncrypted.Count != 0 || (parameters.Signed.Count == 0 && parameters.Endorsing.Count == 0) || parameters.SignedEndorsing.Count != 0)
                return false;
            if ((parameters.Signed.Count == 1) && (parameters.Endorsing.Count == 0))
            {
                issuedTokenParameters = parameters.Signed[0] as IssuedSecurityTokenParameters;
                if (issuedTokenParameters != null && issuedTokenParameters.KeyType != SecurityKeyType.BearerKey)
                    return false;
            }
            else if ((parameters.Endorsing.Count == 1) && (parameters.Signed.Count == 0))
            {
                issuedTokenParameters = parameters.Endorsing[0] as IssuedSecurityTokenParameters;
                if (issuedTokenParameters != null && (issuedTokenParameters.KeyType != SecurityKeyType.SymmetricKey && issuedTokenParameters.KeyType != SecurityKeyType.AsymmetricKey))
                    return false;
            }
            if (issuedTokenParameters == null)
                return false;
            if (issuedTokenParameters.RequireDerivedKeys)
                return false;

            return true;
        }

        // If any changes are made to this method, please make sure that they are
        // reflected in the corresponding IsSecureConversationBinding() method.
        static public SecurityBindingElement CreateSecureConversationBindingElement(SecurityBindingElement bootstrapSecurity)
        {
            return CreateSecureConversationBindingElement(bootstrapSecurity, SecureConversationSecurityTokenParameters.defaultRequireCancellation, null);
        }

        // this method reverses CreateSecureConversationBindingElement() logic
        internal static bool IsSecureConversationBinding(SecurityBindingElement sbe, out SecurityBindingElement bootstrapSecurity)
        {
            return IsSecureConversationBinding(sbe, SecureConversationSecurityTokenParameters.defaultRequireCancellation, out bootstrapSecurity);
        }

        static public SecurityBindingElement CreateSecureConversationBindingElement(SecurityBindingElement bootstrapSecurity, bool requireCancellation)
        {
            return CreateSecureConversationBindingElement(bootstrapSecurity, requireCancellation, null);
        }

        // If any changes are made to this method, please make sure that they are
        // reflected in the corresponding IsSecureConversationBinding() method.
        static public SecurityBindingElement CreateSecureConversationBindingElement(SecurityBindingElement bootstrapSecurity, bool requireCancellation, ChannelProtectionRequirements bootstrapProtectionRequirements)
        {
            if (bootstrapSecurity == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bootstrapBinding");

            SecurityBindingElement result;

            if (bootstrapSecurity is TransportSecurityBindingElement)
            {
                // there is no need to do replay detection or key derivation for transport bindings
                TransportSecurityBindingElement primary = new TransportSecurityBindingElement();                
                SecureConversationSecurityTokenParameters scParameters = new SecureConversationSecurityTokenParameters(
                        bootstrapSecurity,
                        requireCancellation,
                        bootstrapProtectionRequirements);
                scParameters.RequireDerivedKeys = false;
                primary.EndpointSupportingTokenParameters.Endorsing.Add(
                    scParameters);                
                primary.LocalClientSettings.DetectReplays = false;
                primary.LocalServiceSettings.DetectReplays = false;
                primary.IncludeTimestamp = true;
                result = primary;
            }
            else // Symmetric- or AsymmetricSecurityBindingElement
            {
                SymmetricSecurityBindingElement primary = new SymmetricSecurityBindingElement(
                    new SecureConversationSecurityTokenParameters(
                        bootstrapSecurity,
                        requireCancellation,
                        bootstrapProtectionRequirements));
                // there is no need for signature confirmation on the steady state binding
                primary.RequireSignatureConfirmation = false;
                result = primary;
            }

            return result;
        }

        // this method reverses CreateSecureConversationBindingElement() logic
        internal static bool IsSecureConversationBinding(SecurityBindingElement sbe, bool requireCancellation, out SecurityBindingElement bootstrapSecurity)
        {
            bootstrapSecurity = null;
            SymmetricSecurityBindingElement ssbe = sbe as SymmetricSecurityBindingElement;
            if (ssbe != null)
            {
                if (ssbe.RequireSignatureConfirmation)
                    return false;

                SecureConversationSecurityTokenParameters parameters = ssbe.ProtectionTokenParameters as SecureConversationSecurityTokenParameters;
                if (parameters == null)
                    return false;
                if (parameters.RequireCancellation != requireCancellation)
                    return false;
                bootstrapSecurity = parameters.BootstrapSecurityBindingElement;
            }
            else
            {
                if (!sbe.IncludeTimestamp)
                    return false;

                // do not check local settings: sbe.LocalServiceSettings and sbe.LocalClientSettings

                if (!(sbe is TransportSecurityBindingElement))
                    return false;

                SupportingTokenParameters parameters = sbe.EndpointSupportingTokenParameters;
                if (parameters.Signed.Count != 0 || parameters.SignedEncrypted.Count != 0 || parameters.Endorsing.Count != 1 || parameters.SignedEndorsing.Count != 0)
                    return false;
                SecureConversationSecurityTokenParameters scParameters = parameters.Endorsing[0] as SecureConversationSecurityTokenParameters;
                if (scParameters == null)
                    return false;

                if (scParameters.RequireCancellation != requireCancellation)
                    return false;

                bootstrapSecurity = scParameters.BootstrapSecurityBindingElement;

            }

            if (bootstrapSecurity != null && bootstrapSecurity.SecurityHeaderLayout != SecurityProtocolFactory.defaultSecurityHeaderLayout)
                return false;

            return bootstrapSecurity != null;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "{0}:", this.GetType().ToString()));
            sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "DefaultAlgorithmSuite: {0}", this.defaultAlgorithmSuite.ToString()));
            sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "IncludeTimestamp: {0}", this.includeTimestamp.ToString()));
            sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "KeyEntropyMode: {0}", this.keyEntropyMode.ToString()));
            sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "MessageSecurityVersion: {0}", this.MessageSecurityVersion.ToString()));
            sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "SecurityHeaderLayout: {0}", this.securityHeaderLayout.ToString()));
            sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "ProtectTokens: {0}", this.protectTokens.ToString()));
            sb.AppendLine("EndpointSupportingTokenParameters:");
            sb.AppendLine("  " + this.EndpointSupportingTokenParameters.ToString().Trim().Replace("\n", "\n  "));
            sb.AppendLine("OptionalEndpointSupportingTokenParameters:");
            sb.AppendLine("  " + this.OptionalEndpointSupportingTokenParameters.ToString().Trim().Replace("\n", "\n  "));
            if (this.operationSupportingTokenParameters.Count == 0)
            {
                sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "OperationSupportingTokenParameters: none"));
            }
            else
            {
                foreach (string requestAction in this.OperationSupportingTokenParameters.Keys)
                {
                    sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "OperationSupportingTokenParameters[\"{0}\"]:", requestAction));
                    sb.AppendLine("  " + this.OperationSupportingTokenParameters[requestAction].ToString().Trim().Replace("\n", "\n  "));
                }
            }
            if (this.optionalOperationSupportingTokenParameters.Count == 0)
            {
                sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "OptionalOperationSupportingTokenParameters: none"));
            }
            else
            {
                foreach (string requestAction in this.OptionalOperationSupportingTokenParameters.Keys)
                {
                    sb.AppendLine(String.Format(CultureInfo.InvariantCulture, "OptionalOperationSupportingTokenParameters[\"{0}\"]:", requestAction));
                    sb.AppendLine("  " + this.OptionalOperationSupportingTokenParameters[requestAction].ToString().Trim().Replace("\n", "\n  "));
                }
            }

            return sb.ToString().Trim();
        }


        internal static ChannelProtectionRequirements ComputeProtectionRequirements(SecurityBindingElement security, BindingParameterCollection parameterCollection, BindingElementCollection bindingElements, bool isForService)
        {
            if (parameterCollection == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameterCollection");
            if (bindingElements == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingElements");
            if (security == null)
            {
                return null;
            }

            ChannelProtectionRequirements result = null;
            if ((security is SymmetricSecurityBindingElement) || (security is AsymmetricSecurityBindingElement))
            {
                result = new ChannelProtectionRequirements();
                ChannelProtectionRequirements contractRequirements = parameterCollection.Find<ChannelProtectionRequirements>();

                if (contractRequirements != null)
                    result.Add(contractRequirements);

                AddBindingProtectionRequirements(result, bindingElements, !isForService);
            }

            return result;
        }

        static void AddBindingProtectionRequirements(ChannelProtectionRequirements requirements, BindingElementCollection bindingElements, bool isForChannel)
        {
            // Gather custom requirements from bindingElements
            CustomBinding binding = new CustomBinding(bindingElements);
            BindingContext context = new BindingContext(binding, new BindingParameterCollection());
            // In theory, we can just do 
            //     context.GetInnerProperty<ChannelProtectionRequirements>()
            // but that relies on each binding element to correctly union-up its own requirements with
            // those of the rest of the stack.  So instead, we ask each BE individually, and we do the 
            // work of combining the results.  This protects us against this scenario: someone authors "FooBE"
            // with a a GetProperty implementation that always returns null (oops), and puts FooBE on the 
            // top of the stack, and so FooBE "hides" important protection requirements that inner BEs
            // require, resulting in an insecure binding.
            foreach (BindingElement bindingElement in bindingElements)
            {
                if (bindingElement != null)
                {
                    // ask each element individually for its requirements
                    context.RemainingBindingElements.Clear();
                    context.RemainingBindingElements.Add(bindingElement);
                    ChannelProtectionRequirements s = context.GetInnerProperty<ChannelProtectionRequirements>();
                    if (s != null)
                    {
                        //if (isForChannel)
                        //{
                        //    requirements.Add(s.CreateInverse());
                        //}
                        //else
                        //{
                            requirements.Add(s);
                        //}
                    }
                }
            }
        }

        internal void ApplyAuditBehaviorSettings(BindingContext context, SecurityProtocolFactory factory)
        {
            ServiceSecurityAuditBehavior auditBehavior = context.BindingParameters.Find<ServiceSecurityAuditBehavior>();
            if (auditBehavior != null)
            {
                factory.AuditLogLocation = auditBehavior.AuditLogLocation;
                factory.SuppressAuditFailure = auditBehavior.SuppressAuditFailure;
                factory.ServiceAuthorizationAuditLevel = auditBehavior.ServiceAuthorizationAuditLevel;
                factory.MessageAuthenticationAuditLevel = auditBehavior.MessageAuthenticationAuditLevel;
            }
            else
            {
                factory.AuditLogLocation = ServiceSecurityAuditBehavior.defaultAuditLogLocation;
                factory.SuppressAuditFailure = ServiceSecurityAuditBehavior.defaultSuppressAuditFailure;
                factory.ServiceAuthorizationAuditLevel = ServiceSecurityAuditBehavior.defaultServiceAuthorizationAuditLevel;
                factory.MessageAuthenticationAuditLevel = ServiceSecurityAuditBehavior.defaultMessageAuthenticationAuditLevel;
            }
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (b == null)
                return false;

            SecurityBindingElement security = b as SecurityBindingElement;
            if (security == null)
                return false;
            return SecurityElement.AreBindingsMatching(this, security);
        }

        static void AddAssertionIfNotNull(PolicyConversionContext policyContext, XmlElement assertion)
        {
            if (policyContext != null && assertion != null)
            {
                policyContext.GetBindingAssertions().Add(assertion);
            }
        }

        static void AddAssertionIfNotNull(PolicyConversionContext policyContext, Collection<XmlElement> assertions)
        {
            if (policyContext != null && assertions != null)
            {
                PolicyAssertionCollection existingAssertions = policyContext.GetBindingAssertions();
                for (int i = 0; i < assertions.Count; ++i)
                    existingAssertions.Add(assertions[i]);
            }
        }

        static void AddAssertionIfNotNull(PolicyConversionContext policyContext, OperationDescription operation, XmlElement assertion)
        {
            if (policyContext != null && assertion != null)
            {
                policyContext.GetOperationBindingAssertions(operation).Add(assertion);
            }
        }

        static void AddAssertionIfNotNull(PolicyConversionContext policyContext, OperationDescription operation, Collection<XmlElement> assertions)
        {
            if (policyContext != null && assertions != null)
            {
                PolicyAssertionCollection existingAssertions = policyContext.GetOperationBindingAssertions(operation);
                for (int i = 0; i < assertions.Count; ++i)
                    existingAssertions.Add(assertions[i]);
            }
        }

        static void AddAssertionIfNotNull(PolicyConversionContext policyContext, MessageDescription message, XmlElement assertion)
        {
            if (policyContext != null && assertion != null)
            {
                policyContext.GetMessageBindingAssertions(message).Add(assertion);
            }
        }

        static void AddAssertionIfNotNull(PolicyConversionContext policyContext, FaultDescription message, XmlElement assertion)
        {
            if (policyContext != null && assertion != null)
            {
                policyContext.GetFaultBindingAssertions(message).Add(assertion);
            }
        }

        internal static void ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            if (exporter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            SecurityTraceRecordHelper.TraceExportChannelBindingEntry();

            SecurityBindingElement binding = null;
            ITransportTokenAssertionProvider transportTokenAssertionProvider = null;
            BindingElementCollection bindingElementsBelowSecurity = new BindingElementCollection();
            if ((context != null) && (context.BindingElements != null))
            {
                foreach (BindingElement be in context.BindingElements)
                {
                    if (be is SecurityBindingElement)
                    {
                        binding = (SecurityBindingElement)be;
                    }
                    else
                    {
                        if (binding != null || be is MessageEncodingBindingElement || be is ITransportTokenAssertionProvider)
                        {
                            bindingElementsBelowSecurity.Add(be);
                        }
                        if (be is ITransportTokenAssertionProvider)
                        {
                            transportTokenAssertionProvider = (ITransportTokenAssertionProvider)be;
                        }
                    }
                }
            }

            // this is used when exporting bootstrap policy for secure conversation in SecurityPolicy11.CreateWsspBootstrapPolicyAssertion
            exporter.State[SecurityPolicyStrings.SecureConversationBootstrapBindingElementsBelowSecurityKey] = bindingElementsBelowSecurity;

            bool hasCompletedSuccessfully = false;
            try
            {
                if (binding is SymmetricSecurityBindingElement)
                {
                    ExportSymmetricSecurityBindingElement((SymmetricSecurityBindingElement)binding, exporter, context);
                    ExportOperationScopeSupportingTokensPolicy(binding, exporter, context);
                    ExportMessageScopeProtectionPolicy(binding, exporter, context);
                }
                else if (binding is AsymmetricSecurityBindingElement)
                {
                    ExportAsymmetricSecurityBindingElement((AsymmetricSecurityBindingElement)binding, exporter, context);
                    ExportOperationScopeSupportingTokensPolicy(binding, exporter, context);
                    ExportMessageScopeProtectionPolicy(binding, exporter, context);
                }

                hasCompletedSuccessfully = true;
            }
            finally
            {
                try
                {
                    exporter.State.Remove(SecurityPolicyStrings.SecureConversationBootstrapBindingElementsBelowSecurityKey);
                }
                catch (Exception e)
                {
                    // Always immediately rethrow fatal exceptions.
                    if (hasCompletedSuccessfully || Fx.IsFatal(e)) throw;
                }
            }
        }

        internal static void ExportPolicyForTransportTokenAssertionProviders(MetadataExporter exporter, PolicyConversionContext context)
        {
            if (exporter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            SecurityTraceRecordHelper.TraceExportChannelBindingEntry();

            SecurityBindingElement binding = null;
            ITransportTokenAssertionProvider transportTokenAssertionProvider = null;
            BindingElementCollection bindingElementsBelowSecurity = new BindingElementCollection();
            if ((context != null) && (context.BindingElements != null))
            {
                foreach (BindingElement be in context.BindingElements)
                {
                    if (be is SecurityBindingElement)
                    {
                        binding = (SecurityBindingElement)be;
                    }
                    else
                    {
                        if (binding != null || be is MessageEncodingBindingElement || be is ITransportTokenAssertionProvider)
                        {
                            bindingElementsBelowSecurity.Add(be);
                        }
                        if (be is ITransportTokenAssertionProvider)
                        {
                            transportTokenAssertionProvider = (ITransportTokenAssertionProvider)be;
                        }
                    }
                }
            }

            // this is used when exporting bootstrap policy for secure conversation in SecurityPolicy11.CreateWsspBootstrapPolicyAssertion
            exporter.State[SecurityPolicyStrings.SecureConversationBootstrapBindingElementsBelowSecurityKey] = bindingElementsBelowSecurity;

            bool hasCompletedSuccessfully = false;
            try
            {
                if (binding is TransportSecurityBindingElement)
                {
                    if (transportTokenAssertionProvider == null && !binding.AllowInsecureTransport)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ExportOfBindingWithTransportSecurityBindingElementAndNoTransportSecurityNotSupported)));
                    }

                    ExportTransportSecurityBindingElement((TransportSecurityBindingElement)binding, transportTokenAssertionProvider, exporter, context);
                    ExportOperationScopeSupportingTokensPolicy(binding, exporter, context);
                }
                else if (transportTokenAssertionProvider != null)
                {
                    TransportSecurityBindingElement dummyTransportBindingElement = new TransportSecurityBindingElement();
                    if (binding == null)
                    {
                        dummyTransportBindingElement.IncludeTimestamp = false;
                    }

                    // In order to generate the right sp assertion without SBE.
                    // scenario: WSxHttpBinding with SecurityMode.Transport.
                    // See CSD 3105 for detail
                    HttpsTransportBindingElement httpsBinding = transportTokenAssertionProvider as HttpsTransportBindingElement;
                    if (httpsBinding != null && httpsBinding.MessageSecurityVersion != null)
                    {
                        dummyTransportBindingElement.MessageSecurityVersion = httpsBinding.MessageSecurityVersion;
                    }

                    ExportTransportSecurityBindingElement(dummyTransportBindingElement, transportTokenAssertionProvider, exporter, context);
                }

                hasCompletedSuccessfully = true;
            }
            finally
            {
                try
                {
                    exporter.State.Remove(SecurityPolicyStrings.SecureConversationBootstrapBindingElementsBelowSecurityKey);
                }
                catch (Exception e)
                {
                    // Always immediately rethrow fatal exceptions.
                    if (hasCompletedSuccessfully || Fx.IsFatal(e)) throw;
                }
            }
        }

        //
        // We will emit the wssp trust 10 assertion for all the case except for the basic http binding
        // created through the BasicHttpBinding class.  The reason for this exception is to allow better 
        // interop with third party when the third party doesn't understand the trust ----erion
        //
        static bool RequiresWsspTrust(SecurityBindingElement sbe)
        {
            if (sbe == null)
                return false;

            return !(sbe.doNotEmitTrust);
        }

        static void ExportAsymmetricSecurityBindingElement(AsymmetricSecurityBindingElement binding, MetadataExporter exporter, PolicyConversionContext policyContext)
        {
            WSSecurityPolicy sp = WSSecurityPolicy.GetSecurityPolicyDriver(binding.MessageSecurityVersion);

            AddAssertionIfNotNull(policyContext, sp.CreateWsspAsymmetricBindingAssertion(exporter, policyContext, binding));

            AddAssertionIfNotNull(policyContext, sp.CreateWsspSupportingTokensAssertion(
                exporter,
                binding.EndpointSupportingTokenParameters.Signed,
                binding.EndpointSupportingTokenParameters.SignedEncrypted,
                binding.EndpointSupportingTokenParameters.Endorsing,
                binding.EndpointSupportingTokenParameters.SignedEndorsing,
                binding.OptionalEndpointSupportingTokenParameters.Signed,
                binding.OptionalEndpointSupportingTokenParameters.SignedEncrypted,
                binding.OptionalEndpointSupportingTokenParameters.Endorsing,
                binding.OptionalEndpointSupportingTokenParameters.SignedEndorsing));

            AddAssertionIfNotNull(policyContext, sp.CreateWsspWssAssertion(exporter, binding));

            if (RequiresWsspTrust(binding))
            {
                AddAssertionIfNotNull(policyContext, sp.CreateWsspTrustAssertion(exporter, binding.KeyEntropyMode));
            }
        }

        static void ExportTransportSecurityBindingElement(TransportSecurityBindingElement binding, ITransportTokenAssertionProvider transportTokenAssertionProvider, MetadataExporter exporter, PolicyConversionContext policyContext)
        {
            WSSecurityPolicy sp = WSSecurityPolicy.GetSecurityPolicyDriver(binding.MessageSecurityVersion);

            if (transportTokenAssertionProvider == null && binding.AllowInsecureTransport)
            {
                if ((policyContext != null) && (policyContext.BindingElements != null))
                {
                    foreach (BindingElement be in policyContext.BindingElements)
                    {
                        if (be is HttpTransportBindingElement)
                        {
                            transportTokenAssertionProvider = new HttpsTransportBindingElement();
                            break;
                        }
                        
                        if (be is TcpTransportBindingElement)
                        {
                            transportTokenAssertionProvider = new SslStreamSecurityBindingElement();
                            break;
                        }
                    }
                }
            }

            XmlElement transportTokenAssertion = transportTokenAssertionProvider.GetTransportTokenAssertion();

            if (transportTokenAssertion == null)
            {
                if (transportTokenAssertionProvider is HttpsTransportBindingElement)
                {
                    transportTokenAssertion = sp.CreateWsspHttpsTokenAssertion(exporter, (HttpsTransportBindingElement)transportTokenAssertionProvider);
                }

                if (transportTokenAssertion == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.NoTransportTokenAssertionProvided, transportTokenAssertionProvider.GetType().ToString())));
            }

            AddressingVersion addressingVersion = AddressingVersion.WSAddressing10;
            MessageEncodingBindingElement messageEncoderBindingElement = policyContext.BindingElements.Find<MessageEncodingBindingElement>();
            if (messageEncoderBindingElement != null)
            {
                addressingVersion = messageEncoderBindingElement.MessageVersion.Addressing;
            }

            AddAssertionIfNotNull(policyContext, sp.CreateWsspTransportBindingAssertion(exporter, binding, transportTokenAssertion));

            Collection<XmlElement> supportingTokenAssertions = sp.CreateWsspSupportingTokensAssertion(
                exporter,
                binding.EndpointSupportingTokenParameters.Signed,
                binding.EndpointSupportingTokenParameters.SignedEncrypted,
                binding.EndpointSupportingTokenParameters.Endorsing,
                binding.EndpointSupportingTokenParameters.SignedEndorsing,
                binding.OptionalEndpointSupportingTokenParameters.Signed,
                binding.OptionalEndpointSupportingTokenParameters.SignedEncrypted,
                binding.OptionalEndpointSupportingTokenParameters.Endorsing,
                binding.OptionalEndpointSupportingTokenParameters.SignedEndorsing,
                addressingVersion);

            AddAssertionIfNotNull(policyContext, supportingTokenAssertions);

            if (supportingTokenAssertions.Count > 0
                || HasEndorsingSupportingTokensAtOperationScope(binding))
            {
                AddAssertionIfNotNull(policyContext, sp.CreateWsspWssAssertion(exporter, binding));
                if (RequiresWsspTrust(binding))
                {
                    AddAssertionIfNotNull(policyContext, sp.CreateWsspTrustAssertion(exporter, binding.KeyEntropyMode));
                }
            }
        }

        static bool HasEndorsingSupportingTokensAtOperationScope(SecurityBindingElement binding)
        {
            foreach (SupportingTokenParameters r in binding.OperationSupportingTokenParameters.Values)
            {
                if (r.Endorsing.Count > 0 || r.SignedEndorsing.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        static void ExportSymmetricSecurityBindingElement(SymmetricSecurityBindingElement binding, MetadataExporter exporter, PolicyConversionContext policyContext)
        {
            WSSecurityPolicy sp = WSSecurityPolicy.GetSecurityPolicyDriver(binding.MessageSecurityVersion);

            AddAssertionIfNotNull(policyContext, sp.CreateWsspSymmetricBindingAssertion(exporter, policyContext, binding));

            AddAssertionIfNotNull(policyContext, sp.CreateWsspSupportingTokensAssertion(
                exporter,
                binding.EndpointSupportingTokenParameters.Signed,
                binding.EndpointSupportingTokenParameters.SignedEncrypted,
                binding.EndpointSupportingTokenParameters.Endorsing,
                binding.EndpointSupportingTokenParameters.SignedEndorsing,
                binding.OptionalEndpointSupportingTokenParameters.Signed,
                binding.OptionalEndpointSupportingTokenParameters.SignedEncrypted,
                binding.OptionalEndpointSupportingTokenParameters.Endorsing,
                binding.OptionalEndpointSupportingTokenParameters.SignedEndorsing));

            AddAssertionIfNotNull(policyContext, sp.CreateWsspWssAssertion(exporter, binding));

            if (RequiresWsspTrust(binding))
            {
                AddAssertionIfNotNull(policyContext, sp.CreateWsspTrustAssertion(exporter, binding.KeyEntropyMode));
            }
        }

        static void ExportMessageScopeProtectionPolicy(SecurityBindingElement security, MetadataExporter exporter, PolicyConversionContext policyContext)
        {
            BindingParameterCollection bindingParameters = new BindingParameterCollection();
            bindingParameters.Add(ChannelProtectionRequirements.CreateFromContract(policyContext.Contract, policyContext.BindingElements.Find<SecurityBindingElement>().GetIndividualProperty<ISecurityCapabilities>(), false));
            ChannelProtectionRequirements protectionRequirements = SecurityBindingElement.ComputeProtectionRequirements(security, bindingParameters, policyContext.BindingElements, true);
            protectionRequirements.MakeReadOnly();

            WSSecurityPolicy sp = WSSecurityPolicy.GetSecurityPolicyDriver(security.MessageSecurityVersion);

            foreach (OperationDescription operation in policyContext.Contract.Operations)
            {
                // export policy for application messages
                foreach (MessageDescription message in operation.Messages)
                {
                    MessagePartSpecification parts;
                    ScopedMessagePartSpecification scopedParts;

                    // integrity
                    if (message.Direction == MessageDirection.Input)
                    {
                        scopedParts = protectionRequirements.IncomingSignatureParts;
                    }
                    else
                    {
                        scopedParts = protectionRequirements.OutgoingSignatureParts;
                    }

                    if (scopedParts.TryGetParts(message.Action, out parts))
                    {
                        AddAssertionIfNotNull(policyContext, message, sp.CreateWsspSignedPartsAssertion(parts));
                    }

                    // confidentiality
                    if (message.Direction == MessageDirection.Input)
                    {
                        scopedParts = protectionRequirements.IncomingEncryptionParts;
                    }
                    else
                    {
                        scopedParts = protectionRequirements.OutgoingEncryptionParts;
                    }

                    if (scopedParts.TryGetParts(message.Action, out parts))
                    {
                        AddAssertionIfNotNull(policyContext, message, sp.CreateWsspEncryptedPartsAssertion(parts));
                    }
                }

                // export policy for faults
                foreach (FaultDescription fault in operation.Faults)
                {
                    MessagePartSpecification parts;

                    // integrity
                    if (protectionRequirements.OutgoingSignatureParts.TryGetParts(fault.Action, out parts))
                    {
                        AddAssertionIfNotNull(policyContext, fault, sp.CreateWsspSignedPartsAssertion(parts));
                    }

                    // confidentiality
                    if (protectionRequirements.OutgoingEncryptionParts.TryGetParts(fault.Action, out parts))
                    {
                        AddAssertionIfNotNull(policyContext, fault, sp.CreateWsspEncryptedPartsAssertion(parts));
                    }
                }
            }
        }

        static void ExportOperationScopeSupportingTokensPolicy(SecurityBindingElement binding, MetadataExporter exporter, PolicyConversionContext policyContext)
        {
            WSSecurityPolicy sp = WSSecurityPolicy.GetSecurityPolicyDriver(binding.MessageSecurityVersion);

            if (binding.OperationSupportingTokenParameters.Count == 0 && binding.OptionalOperationSupportingTokenParameters.Count == 0)
            {
                return;
            }

            foreach (OperationDescription operation in policyContext.Contract.Operations)
            {
                foreach (MessageDescription message in operation.Messages)
                {

                    if (message.Direction == MessageDirection.Input)
                    {
                        SupportingTokenParameters requirements = null;
                        SupportingTokenParameters optionalRequirements = null;

                        if (binding.OperationSupportingTokenParameters.ContainsKey(message.Action))
                        {
                            requirements = binding.OperationSupportingTokenParameters[message.Action];
                        }
                        if (binding.OptionalOperationSupportingTokenParameters.ContainsKey(message.Action))
                        {
                            optionalRequirements = binding.OptionalOperationSupportingTokenParameters[message.Action];
                        }

                        if (requirements == null && optionalRequirements == null)
                        {
                            continue;
                        }

                        AddAssertionIfNotNull(policyContext, operation, sp.CreateWsspSupportingTokensAssertion(
                            exporter,
                            requirements == null ? null : requirements.Signed,
                            requirements == null ? null : requirements.SignedEncrypted,
                            requirements == null ? null : requirements.Endorsing,
                            requirements == null ? null : requirements.SignedEndorsing,
                            optionalRequirements == null ? null : optionalRequirements.Signed,
                            optionalRequirements == null ? null : optionalRequirements.SignedEncrypted,
                            optionalRequirements == null ? null : optionalRequirements.Endorsing,
                            optionalRequirements == null ? null : optionalRequirements.SignedEndorsing));
                    }
                }
            }
        }
    }
}
