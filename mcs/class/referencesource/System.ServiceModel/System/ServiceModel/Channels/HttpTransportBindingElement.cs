//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Net;
    using System.Net.Security;
    using System.Runtime;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Description;
    using System.Xml;
    using WsdlNS = System.Web.Services.Description;

    public class HttpTransportBindingElement
        : TransportBindingElement,
        IWsdlExportExtension, IPolicyExportExtension, ITransportPolicyImport
    {
        bool allowCookies;
        AuthenticationSchemes authenticationScheme;
        bool bypassProxyOnLocal;
        bool decompressionEnabled;
        HostNameComparisonMode hostNameComparisonMode;
        bool keepAliveEnabled;
        bool inheritBaseAddressSettings;
        int maxBufferSize;
        bool maxBufferSizeInitialized;
        string method;
        Uri proxyAddress;
        AuthenticationSchemes proxyAuthenticationScheme;
        string realm;
        TimeSpan requestInitializationTimeout;
        TransferMode transferMode;
        bool unsafeConnectionNtlmAuthentication;
        bool useDefaultWebProxy;
        WebSocketTransportSettings webSocketSettings;
        IWebProxy webProxy;
        ExtendedProtectionPolicy extendedProtectionPolicy;
        HttpAnonymousUriPrefixMatcher anonymousUriPrefixMatcher;
        HttpMessageHandlerFactory httpMessageHandlerFactory;
        int maxPendingAccepts;
        
        public HttpTransportBindingElement()
            : base()
        {
            this.allowCookies = HttpTransportDefaults.AllowCookies;
            this.authenticationScheme = HttpTransportDefaults.AuthenticationScheme;
            this.bypassProxyOnLocal = HttpTransportDefaults.BypassProxyOnLocal;
            this.decompressionEnabled = HttpTransportDefaults.DecompressionEnabled;
            this.hostNameComparisonMode = HttpTransportDefaults.HostNameComparisonMode;
            this.keepAliveEnabled = HttpTransportDefaults.KeepAliveEnabled;
            this.maxBufferSize = TransportDefaults.MaxBufferSize;
            this.maxPendingAccepts = HttpTransportDefaults.DefaultMaxPendingAccepts;
            this.method = string.Empty;
            this.proxyAuthenticationScheme = HttpTransportDefaults.ProxyAuthenticationScheme;
            this.proxyAddress = HttpTransportDefaults.ProxyAddress;
            this.realm = HttpTransportDefaults.Realm;
            this.requestInitializationTimeout = HttpTransportDefaults.RequestInitializationTimeout;
            this.transferMode = HttpTransportDefaults.TransferMode;
            this.unsafeConnectionNtlmAuthentication = HttpTransportDefaults.UnsafeConnectionNtlmAuthentication;
            this.useDefaultWebProxy = HttpTransportDefaults.UseDefaultWebProxy;
            this.webSocketSettings = HttpTransportDefaults.GetDefaultWebSocketTransportSettings();
            this.webProxy = null;
            this.extendedProtectionPolicy = ChannelBindingUtility.DefaultPolicy;
        }

        protected HttpTransportBindingElement(HttpTransportBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.allowCookies = elementToBeCloned.allowCookies;
            this.authenticationScheme = elementToBeCloned.authenticationScheme;
            this.bypassProxyOnLocal = elementToBeCloned.bypassProxyOnLocal;
            this.decompressionEnabled = elementToBeCloned.decompressionEnabled;
            this.hostNameComparisonMode = elementToBeCloned.hostNameComparisonMode;
            this.inheritBaseAddressSettings = elementToBeCloned.InheritBaseAddressSettings;
            this.keepAliveEnabled = elementToBeCloned.keepAliveEnabled;
            this.maxBufferSize = elementToBeCloned.maxBufferSize;
            this.maxBufferSizeInitialized = elementToBeCloned.maxBufferSizeInitialized;
            this.maxPendingAccepts = elementToBeCloned.maxPendingAccepts;
            this.method = elementToBeCloned.method;
            this.proxyAddress = elementToBeCloned.proxyAddress;
            this.proxyAuthenticationScheme = elementToBeCloned.proxyAuthenticationScheme;
            this.realm = elementToBeCloned.realm;
            this.requestInitializationTimeout = elementToBeCloned.requestInitializationTimeout;
            this.transferMode = elementToBeCloned.transferMode;
            this.unsafeConnectionNtlmAuthentication = elementToBeCloned.unsafeConnectionNtlmAuthentication;
            this.useDefaultWebProxy = elementToBeCloned.useDefaultWebProxy;
            this.webSocketSettings = elementToBeCloned.webSocketSettings.Clone();
            this.webProxy = elementToBeCloned.webProxy;
            this.extendedProtectionPolicy = elementToBeCloned.ExtendedProtectionPolicy;
            if (elementToBeCloned.anonymousUriPrefixMatcher != null)
            {
                this.anonymousUriPrefixMatcher = new HttpAnonymousUriPrefixMatcher(elementToBeCloned.anonymousUriPrefixMatcher);
            }
            this.MessageHandlerFactory = elementToBeCloned.MessageHandlerFactory;
        }

        [DefaultValue(HttpTransportDefaults.AllowCookies)]
        public bool AllowCookies
        {
            get
            {
                return this.allowCookies;
            }
            set
            {
                this.allowCookies = value;
            }
        }

        [DefaultValue(HttpTransportDefaults.AuthenticationScheme)]
        public AuthenticationSchemes AuthenticationScheme
        {
            get
            {
                return this.authenticationScheme;
            }

            set
            {
                this.authenticationScheme = value;
            }
        }

        [DefaultValue(HttpTransportDefaults.BypassProxyOnLocal)]
        public bool BypassProxyOnLocal
        {
            get
            {
                return this.bypassProxyOnLocal;
            }
            set
            {
                this.bypassProxyOnLocal = value;
            }
        }

        [DefaultValue(HttpTransportDefaults.DecompressionEnabled)]
        public bool DecompressionEnabled
        {
            get
            {
                return this.decompressionEnabled;
            }
            set
            {
                this.decompressionEnabled = value;
            }
        }

        [DefaultValue(HttpTransportDefaults.HostNameComparisonMode)]
        public HostNameComparisonMode HostNameComparisonMode
        {
            get
            {
                return this.hostNameComparisonMode;
            }
            set
            {
                HostNameComparisonModeHelper.Validate(value);
                this.hostNameComparisonMode = value;
            }
        }

        public HttpMessageHandlerFactory MessageHandlerFactory
        {
            get
            {
                return this.httpMessageHandlerFactory;
            }
            set
            {
                this.httpMessageHandlerFactory = value;
            }
        }

        public ExtendedProtectionPolicy ExtendedProtectionPolicy
        {
            get
            {
                return this.extendedProtectionPolicy;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                if (value.PolicyEnforcement == PolicyEnforcement.Always &&
                    !System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy.OSSupportsExtendedProtection)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new PlatformNotSupportedException(SR.GetString(SR.ExtendedProtectionNotSupported)));
                }

                this.extendedProtectionPolicy = value;
            }
        }

        // MB#26970: used by MEX to ensure that we don't conflict on base-address scoped settings
        internal bool InheritBaseAddressSettings
        {
            get
            {
                return this.inheritBaseAddressSettings;
            }
            set
            {
                this.inheritBaseAddressSettings = value;
            }
        }

        [DefaultValue(HttpTransportDefaults.KeepAliveEnabled)]
        public bool KeepAliveEnabled
        {
            get
            {
                return this.keepAliveEnabled;
            }
            set
            {
                this.keepAliveEnabled = value;
            }
        }

        // client
        // server
        [DefaultValue(TransportDefaults.MaxBufferSize)]
        public int MaxBufferSize
        {
            get
            {
                if (maxBufferSizeInitialized || TransferMode != TransferMode.Buffered)
                    return maxBufferSize;

                long maxReceivedMessageSize = MaxReceivedMessageSize;
                if (maxReceivedMessageSize > int.MaxValue)
                    return int.MaxValue;
                else
                    return (int)maxReceivedMessageSize;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.ValueMustBePositive)));
                }

                maxBufferSizeInitialized = true;
                this.maxBufferSize = value;
            }
        }

        // server
        [DefaultValue(HttpTransportDefaults.DefaultMaxPendingAccepts)]
        public int MaxPendingAccepts
        {
            get
            {
                return this.maxPendingAccepts;
            }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.ValueMustBeNonNegative)));
                }

                if (value > HttpTransportDefaults.MaxPendingAcceptsUpperLimit)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value,
                        SR.GetString(SR.HttpMaxPendingAcceptsTooLargeError, HttpTransportDefaults.MaxPendingAcceptsUpperLimit)));
                }

                this.maxPendingAccepts = value;
            }
        }

        // string.Empty == wildcard
        internal string Method
        {
            get
            {
                return this.method;
            }

            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.method = value;
            }
        }

        [DefaultValue(HttpTransportDefaults.ProxyAddress)]
        [TypeConverter(typeof(UriTypeConverter))]
        public Uri ProxyAddress
        {
            get
            {
                return this.proxyAddress;
            }
            set
            {
                this.proxyAddress = value;
            }
        }

        [DefaultValue(HttpTransportDefaults.ProxyAuthenticationScheme)]
        public AuthenticationSchemes ProxyAuthenticationScheme
        {
            get
            {
                return this.proxyAuthenticationScheme;
            }

            set
            {
                if (!value.IsSingleton())
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.HttpProxyRequiresSingleAuthScheme,
                        value));
                }
                this.proxyAuthenticationScheme = value;
            }
        }

        // CSDMain#17853: used by cardspace to ensure that the correct proxy is picked up
        internal IWebProxy Proxy
        {
            set
            {
                this.webProxy = value;
            }
            get
            {
                return this.webProxy;
            }
        }

        [DefaultValue(HttpTransportDefaults.Realm)]
        public string Realm
        {
            get
            {
                return this.realm;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.realm = value;
            }
        }

        [DefaultValue(typeof(TimeSpan), HttpTransportDefaults.RequestInitializationTimeoutString)]
        public TimeSpan RequestInitializationTimeout
        {
            get
            {
                return this.requestInitializationTimeout;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.requestInitializationTimeout = value;
            }
        }

        public override string Scheme { get { return "http"; } }

        // client
        // server
        [DefaultValue(HttpTransportDefaults.TransferMode)]
        public TransferMode TransferMode
        {
            get
            {
                return this.transferMode;
            }
            set
            {
                TransferModeHelper.Validate(value);
                this.transferMode = value;
            }
        }

        public WebSocketTransportSettings WebSocketSettings
        {
            get
            {
                return this.webSocketSettings;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.webSocketSettings = value;
            }
        }

        internal virtual bool GetSupportsClientAuthenticationImpl(AuthenticationSchemes effectiveAuthenticationSchemes)
        {
            return effectiveAuthenticationSchemes != AuthenticationSchemes.None &&
                effectiveAuthenticationSchemes.IsNotSet(AuthenticationSchemes.Anonymous);
        }

        internal virtual bool GetSupportsClientWindowsIdentityImpl(AuthenticationSchemes effectiveAuthenticationSchemes)
        {
            return effectiveAuthenticationSchemes != AuthenticationSchemes.None &&
                effectiveAuthenticationSchemes.IsNotSet(AuthenticationSchemes.Anonymous);
        }

        internal HttpAnonymousUriPrefixMatcher AnonymousUriPrefixMatcher
        {
            get
            {
                return this.anonymousUriPrefixMatcher;
            }
        }

        [DefaultValue(HttpTransportDefaults.UnsafeConnectionNtlmAuthentication)]
        public bool UnsafeConnectionNtlmAuthentication
        {
            get
            {
                return this.unsafeConnectionNtlmAuthentication;
            }

            set
            {
                this.unsafeConnectionNtlmAuthentication = value;
            }
        }

        [DefaultValue(HttpTransportDefaults.UseDefaultWebProxy)]
        public bool UseDefaultWebProxy
        {
            get
            {
                return this.useDefaultWebProxy;
            }
            set
            {
                this.useDefaultWebProxy = value;
            }
        }

        internal string GetWsdlTransportUri(bool useWebSocketTransport)
        {
            if (useWebSocketTransport)
            {
                return TransportPolicyConstants.WebSocketTransportUri;
            }

            return TransportPolicyConstants.HttpTransportUri;
        }

        public override BindingElement Clone()
        {
            return new HttpTransportBindingElement(this);
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(ISecurityCapabilities))
            {
                AuthenticationSchemes effectiveAuthenticationSchemes = HttpTransportBindingElement.GetEffectiveAuthenticationSchemes(this.AuthenticationScheme,
                    context.BindingParameters);

                return (T)(object)new SecurityCapabilities(this.GetSupportsClientAuthenticationImpl(effectiveAuthenticationSchemes),
                    effectiveAuthenticationSchemes == AuthenticationSchemes.Negotiate,
                    this.GetSupportsClientWindowsIdentityImpl(effectiveAuthenticationSchemes),
                    ProtectionLevel.None,
                    ProtectionLevel.None);
            }
            else if (typeof(T) == typeof(IBindingDeliveryCapabilities))
            {
                return (T)(object)new BindingDeliveryCapabilitiesHelper();
            }
            else if (typeof(T) == typeof(TransferMode))
            {
                return (T)(object)this.TransferMode;
            }
            else if (typeof(T) == typeof(ExtendedProtectionPolicy))
            {
                return (T)(object)this.ExtendedProtectionPolicy;
            }
            else if (typeof(T) == typeof(IAnonymousUriPrefixMatcher))
            {
                if (this.anonymousUriPrefixMatcher == null)
                {
                    this.anonymousUriPrefixMatcher = new HttpAnonymousUriPrefixMatcher();
                }

                return (T)(object)this.anonymousUriPrefixMatcher;
            }
            else if (typeof(T) == typeof(ITransportCompressionSupport))
            {
                return (T)(object)new TransportCompressionSupportHelper();
            }
            else
            {
#pragma warning suppress 56506 // [....], BindingContext.BindingParameters cannot be null
                if (context.BindingParameters.Find<MessageEncodingBindingElement>() == null)
                {
                    context.BindingParameters.Add(new TextMessageEncodingBindingElement());
                }
                return base.GetProperty<T>(context);
            }
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (typeof(TChannel) == typeof(IRequestChannel))
            {
                return this.WebSocketSettings.TransportUsage != WebSocketTransportUsage.Always;
            }
            else if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                return this.WebSocketSettings.TransportUsage != WebSocketTransportUsage.Never;
            }
            return false;
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (typeof(TChannel) == typeof(IReplyChannel))
            {
                return this.WebSocketSettings.TransportUsage != WebSocketTransportUsage.Always;
            }
            else if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                return this.WebSocketSettings.TransportUsage != WebSocketTransportUsage.Never;
            }
            return false;
        }


        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (this.MessageHandlerFactory != null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.HttpPipelineNotSupportedOnClientSide, "MessageHandlerFactory")));
            }

            if (!this.CanBuildChannelFactory<TChannel>(context))
            {
#pragma warning suppress 56506 // [....], context.Binding will never be null.
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", SR.GetString(SR.CouldnTCreateChannelForChannelType2, context.Binding.Name, typeof(TChannel)));
            }

            if (this.authenticationScheme == AuthenticationSchemes.None)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.HttpAuthSchemeCannotBeNone,
                    this.authenticationScheme));
            }
            else if (!this.authenticationScheme.IsSingleton())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.HttpRequiresSingleAuthScheme,
                    this.authenticationScheme));
            }

            return (IChannelFactory<TChannel>)(object)new HttpChannelFactory<TChannel>(this, context);
        }

        internal static AuthenticationSchemes GetEffectiveAuthenticationSchemes(AuthenticationSchemes currentAuthenticationSchemes,
            BindingParameterCollection bindingParameters)
        {
            if (bindingParameters == null)
            {
                return currentAuthenticationSchemes;
            }

            AuthenticationSchemes hostSupportedAuthenticationSchemes;

            if (!AuthenticationSchemesBindingParameter.TryExtract(bindingParameters, out hostSupportedAuthenticationSchemes))
            {
                return currentAuthenticationSchemes;
            }

            if (currentAuthenticationSchemes == AuthenticationSchemes.None ||
                (AspNetEnvironment.Current.IsMetadataListener(bindingParameters) &&
                currentAuthenticationSchemes == AuthenticationSchemes.Anonymous &&
                hostSupportedAuthenticationSchemes.IsNotSet(AuthenticationSchemes.Anonymous)))
            {
                //Inherit authentication schemes from host.
                //This logic of inheriting from the host for anonymous MEX endpoints was previously implemented in HostedAspNetEnvironment.ValidateHttpSettings.
                //We moved it here to maintain the pre-multi-auth behavior. (see CSDMain 183553)

                if (!hostSupportedAuthenticationSchemes.IsSingleton() &&
                     hostSupportedAuthenticationSchemes.IsSet(AuthenticationSchemes.Anonymous) &&
                     AspNetEnvironment.Current.AspNetCompatibilityEnabled &&
                     AspNetEnvironment.Current.IsSimpleApplicationHost &&
                     AspNetEnvironment.Current.IsWindowsAuthenticationConfigured())
                {
                    // Remove Anonymous if ASP.Net authentication mode is Windows (Asp.Net would not allow anonymous requests in this case anyway)
                    hostSupportedAuthenticationSchemes ^= AuthenticationSchemes.Anonymous;
                }

                return hostSupportedAuthenticationSchemes;
            }
            else
            {
                //build intersection between AuthenticationSchemes supported on the HttpTransportbidningELement and ServiceHost/IIS
                return currentAuthenticationSchemes & hostSupportedAuthenticationSchemes;
            }
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (!this.CanBuildChannelListener<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
#pragma warning suppress 56506 // [....], context.Binding will never be null.
"TChannel", SR.GetString(SR.CouldnTCreateChannelForChannelType2, context.Binding.Name, typeof(TChannel)));
            }

            UpdateAuthenticationSchemes(context);

            HttpChannelListener listener = new HttpChannelListener<TChannel>(this, context);
            
            AspNetEnvironment.Current.ApplyHostedContext(listener, context);
            return (IChannelListener<TChannel>)(object)listener;
        }

        protected void UpdateAuthenticationSchemes(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            AuthenticationSchemes effectiveAutheSchemes = HttpTransportBindingElement.GetEffectiveAuthenticationSchemes(this.AuthenticationScheme,
                context.BindingParameters);

            if (effectiveAutheSchemes == AuthenticationSchemes.None)
            {
#pragma warning suppress 56506 // [....], context.Binding will never be null.
                string bindingName = context.Binding.Name;

                if (this.AuthenticationScheme == AuthenticationSchemes.None)
                {
                    //can't inherit from host because none were configured.
                    //We are throwing a "NotSupportedException" to be consistent with the type of exception that was thrown in this scenario,
                    //before the multi-auth feature, in HostedAspNetEnvironment.ValidateHttpSettings.
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new NotSupportedException(SR.GetString(SR.AuthenticationSchemesCannotBeInheritedFromHost, bindingName)));
                }
                else
                {
                    //settings configured on the host and binding conflict.
                    AuthenticationSchemes hostSchemes;
                    if (!AuthenticationSchemesBindingParameter.TryExtract(context.BindingParameters, out hostSchemes))
                    {
                        //The host/binding settings can only conflict if host has settings specified, so we should never
                        //hit this line of code
                        DiagnosticUtility.DebugAssert("Failed to find AuthenticationSchemesBindingParameter");
                    }

                    //We are throwing a "NotSupportedException" to be consistent with the type of exception that was thrown in this scenario,
                    //before the multi-auth feature, in HostedAspNetEnvironment.ValidateHttpSettings.
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new NotSupportedException(SR.GetString(SR.AuthenticationSchemes_BindingAndHostConflict, hostSchemes, bindingName, this.AuthenticationScheme)));
                }
            }
            this.AuthenticationScheme = effectiveAutheSchemes;
        }

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            if (exporter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            }

            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            this.OnExportPolicy(exporter, context);

            bool createdNew;
            MessageEncodingBindingElement encodingBindingElement = FindMessageEncodingBindingElement(context.BindingElements, out createdNew);
            if (createdNew && encodingBindingElement is IPolicyExportExtension)
            {
                ((IPolicyExportExtension)encodingBindingElement).ExportPolicy(exporter, context);
            }

            WsdlExporter.WSAddressingHelper.AddWSAddressingAssertion(exporter, context, encodingBindingElement.MessageVersion.Addressing);
        }

        internal virtual void OnExportPolicy(MetadataExporter exporter, PolicyConversionContext policyContext)
        {
            List<string> assertionNames = new List<string>();
            AuthenticationSchemes effectiveAuthenticationSchemes = HttpTransportBindingElement.GetEffectiveAuthenticationSchemes(this.AuthenticationScheme,
                    policyContext.BindingParameters);

            if (effectiveAuthenticationSchemes != AuthenticationSchemes.None && !(effectiveAuthenticationSchemes.IsSet(AuthenticationSchemes.Anonymous)))
            {
                // ATTENTION: The order of the if-statements below is essential! When importing WSDL svcutil is actually
                // using the first assertion - and the HTTP spec requires clients to use the most secure authentication
                // scheme supported by the client. (especially important for downlevel (3.5/4.0) clients
                if (effectiveAuthenticationSchemes.IsSet(AuthenticationSchemes.Negotiate))
                {
                    assertionNames.Add(TransportPolicyConstants.NegotiateHttpAuthenticationName);
                }

                if (effectiveAuthenticationSchemes.IsSet(AuthenticationSchemes.Ntlm))
                {
                    assertionNames.Add(TransportPolicyConstants.NtlmHttpAuthenticationName);
                }

                if (effectiveAuthenticationSchemes.IsSet(AuthenticationSchemes.Digest))
                {
                    assertionNames.Add(TransportPolicyConstants.DigestHttpAuthenticationName);
                }

                if (effectiveAuthenticationSchemes.IsSet(AuthenticationSchemes.Basic))
                {
                    assertionNames.Add(TransportPolicyConstants.BasicHttpAuthenticationName);
                }

                if (assertionNames != null && assertionNames.Count > 0)
                {
                    if (assertionNames.Count == 1)
                    {
                        policyContext.GetBindingAssertions().Add(new XmlDocument().CreateElement(TransportPolicyConstants.HttpTransportPrefix,
                            assertionNames[0], TransportPolicyConstants.HttpTransportNamespace));
                    }
                    else
                    {
                        XmlDocument dummy = new XmlDocument();
                        XmlElement root = dummy.CreateElement(MetadataStrings.WSPolicy.Prefix,
                            MetadataStrings.WSPolicy.Elements.ExactlyOne,
                            exporter.PolicyVersion.Namespace);

                        foreach (string assertionName in assertionNames)
                        {
                            root.AppendChild(dummy.CreateElement(TransportPolicyConstants.HttpTransportPrefix,
                                assertionName,
                                TransportPolicyConstants.HttpTransportNamespace));
                        }

                        policyContext.GetBindingAssertions().Add(root);
                    }
                }
            }

            bool useWebSocketTransport = WebSocketHelper.UseWebSocketTransport(this.WebSocketSettings.TransportUsage, policyContext.Contract.IsDuplex());
            if (useWebSocketTransport && this.TransferMode != TransferMode.Buffered)
            {
                policyContext.GetBindingAssertions().Add(new XmlDocument().CreateElement(TransportPolicyConstants.WebSocketPolicyPrefix,
                this.TransferMode.ToString(), TransportPolicyConstants.WebSocketPolicyNamespace));
            }
        }

        internal virtual void OnImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
        }

        void ITransportPolicyImport.ImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
            ICollection<XmlElement> bindingAssertions = policyContext.GetBindingAssertions();
            List<XmlElement> httpAuthAssertions = new List<XmlElement>();

            bool foundAssertion = false;
            foreach (XmlElement assertion in bindingAssertions)
            {
                if (assertion.NamespaceURI != TransportPolicyConstants.HttpTransportNamespace)
                {
                    continue;
                }

                switch (assertion.LocalName)
                {
                    case TransportPolicyConstants.BasicHttpAuthenticationName:
                        this.AuthenticationScheme = AuthenticationSchemes.Basic;
                        break;
                    case TransportPolicyConstants.DigestHttpAuthenticationName:
                        this.AuthenticationScheme = AuthenticationSchemes.Digest;
                        break;
                    case TransportPolicyConstants.NegotiateHttpAuthenticationName:
                        this.AuthenticationScheme = AuthenticationSchemes.Negotiate;
                        break;
                    case TransportPolicyConstants.NtlmHttpAuthenticationName:
                        this.AuthenticationScheme = AuthenticationSchemes.Ntlm;
                        break;
                    default:
                        continue;
                }

                if (foundAssertion)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(
                        SR.GetString(SR.HttpTransportCannotHaveMultipleAuthenticationSchemes, policyContext.Contract.Namespace, policyContext.Contract.Name)));
                }

                foundAssertion = true;
                httpAuthAssertions.Add(assertion);
            }
            httpAuthAssertions.ForEach(delegate(XmlElement element) { bindingAssertions.Remove(element); });

            // This code is being used when we are generating the client configration. Before that, we should already set TransportUsage as Always if the server
            // is using WebSocket.
            if (this.WebSocketSettings.TransportUsage == WebSocketTransportUsage.Always)
            {
                foreach (XmlElement assertion in bindingAssertions)
                {
                    if (assertion.NamespaceURI != TransportPolicyConstants.WebSocketPolicyNamespace)
                    {
                        continue;
                    }

                    string transferMode = assertion.LocalName;
                    TransferMode result;
                    if (!Enum.TryParse<TransferMode>(transferMode, true, out result) || !TransferModeHelper.IsDefined(result) || result == TransferMode.Buffered)
                    {

                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(
                                SR.WebSocketTransportPolicyAssertionInvalid,
                                policyContext.Contract.Namespace,
                                policyContext.Contract.Name,
                                transferMode,
                                TransferMode.Streamed,
                                TransferMode.StreamedRequest,
                                TransferMode.StreamedResponse)));
                    }

                    this.TransferMode = result;
                    bindingAssertions.Remove(assertion);
                    break;
                }
            }

            OnImportPolicy(importer, policyContext);
        }

        void IWsdlExportExtension.ExportContract(WsdlExporter exporter, WsdlContractConversionContext context) { }

        void IWsdlExportExtension.ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext endpointContext)
        {
            bool createdNew;
            MessageEncodingBindingElement encodingBindingElement = FindMessageEncodingBindingElement(endpointContext, out createdNew);
            bool useWebSocketTransport = WebSocketHelper.UseWebSocketTransport(this.WebSocketSettings.TransportUsage, endpointContext.ContractConversionContext.Contract.IsDuplex());

            EndpointAddress address = endpointContext.Endpoint.Address;
            if (useWebSocketTransport)
            {
                address = new EndpointAddress(WebSocketHelper.GetWebSocketUri(endpointContext.Endpoint.Address.Uri), endpointContext.Endpoint.Address);
                WsdlNS.SoapAddressBinding binding = SoapHelper.GetSoapAddressBinding(endpointContext.WsdlPort);
                if (binding != null)
                {
                    binding.Location = address.Uri.AbsoluteUri;
                }
            }

            TransportBindingElement.ExportWsdlEndpoint(exporter, endpointContext,
                this.GetWsdlTransportUri(useWebSocketTransport), address, encodingBindingElement.MessageVersion.Addressing);
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (!base.IsMatch(b))
                return false;
            HttpTransportBindingElement http = b as HttpTransportBindingElement;
            if (http == null)
                return false;
            if (this.allowCookies != http.allowCookies)
                return false;
            if (this.authenticationScheme != http.authenticationScheme)
                return false;
            if (this.decompressionEnabled != http.decompressionEnabled)
                return false;
            if (this.hostNameComparisonMode != http.hostNameComparisonMode)
                return false;
            if (this.inheritBaseAddressSettings != http.inheritBaseAddressSettings)
                return false;
            if (this.keepAliveEnabled != http.keepAliveEnabled)
                return false;
            if (this.maxBufferSize != http.maxBufferSize)
                return false;
            if (this.method != http.method)
                return false;
            if (this.proxyAddress != http.proxyAddress)
                return false;
            if (this.proxyAuthenticationScheme != http.proxyAuthenticationScheme)
                return false;
            if (this.realm != http.realm)
                return false;
            if (this.transferMode != http.transferMode)
                return false;
            if (this.unsafeConnectionNtlmAuthentication != http.unsafeConnectionNtlmAuthentication)
                return false;
            if (this.useDefaultWebProxy != http.useDefaultWebProxy)
                return false;
            if (!this.WebSocketSettings.Equals(http.WebSocketSettings))
                return false;
            if (this.webProxy != http.webProxy)
                return false;

            if (!ChannelBindingUtility.AreEqual(this.ExtendedProtectionPolicy, http.ExtendedProtectionPolicy))
            {
                return false;
            }

            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeExtendedProtectionPolicy()
        {
            return !ChannelBindingUtility.AreEqual(this.ExtendedProtectionPolicy, ChannelBindingUtility.DefaultPolicy);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeMessageHandlerFactory()
        {
            return false;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeWebSocketSettings()
        {
            return !this.WebSocketSettings.Equals(HttpTransportDefaults.GetDefaultWebSocketTransportSettings());
        }

        MessageEncodingBindingElement FindMessageEncodingBindingElement(BindingElementCollection bindingElements, out bool createdNew)
        {
            createdNew = false;
            MessageEncodingBindingElement encodingBindingElement = bindingElements.Find<MessageEncodingBindingElement>();
            if (encodingBindingElement == null)
            {
                createdNew = true;
                encodingBindingElement = new TextMessageEncodingBindingElement();
            }
            return encodingBindingElement;
        }

        MessageEncodingBindingElement FindMessageEncodingBindingElement(WsdlEndpointConversionContext endpointContext, out bool createdNew)
        {
            BindingElementCollection bindingElements = endpointContext.Endpoint.Binding.CreateBindingElements();
            return FindMessageEncodingBindingElement(bindingElements, out createdNew);
        }

        class BindingDeliveryCapabilitiesHelper : IBindingDeliveryCapabilities
        {
            internal BindingDeliveryCapabilitiesHelper()
            {
            }
            bool IBindingDeliveryCapabilities.AssuresOrderedDelivery
            {
                get { return false; }
            }

            bool IBindingDeliveryCapabilities.QueuedDelivery
            {
                get { return false; }
            }
        }

        class TransportCompressionSupportHelper : ITransportCompressionSupport
        {
            public bool IsCompressionFormatSupported(CompressionFormat compressionFormat)
            {
                return true;
            }
        }
    }
}
