//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Net;
    using System.Net.Cache;
    using System.Net.Security;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Text;
    using System.Threading;

    class HttpChannelFactory<TChannel>
        : TransportChannelFactory<TChannel>,
        IHttpTransportFactorySettings
    {
        static bool httpWebRequestWebPermissionDenied = false;
        static RequestCachePolicy requestCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
        static long connectionGroupNamePrefix = 0;

        readonly ClientWebSocketFactory clientWebSocketFactory;

        bool allowCookies;
        AuthenticationSchemes authenticationScheme;
        HttpCookieContainerManager httpCookieContainerManager;

        // Double-checked locking pattern requires volatile for read/write synchronization
        volatile MruCache<Uri, Uri> credentialCacheUriPrefixCache;
        bool decompressionEnabled;

        // Double-checked locking pattern requires volatile for read/write synchronization
        [Fx.Tag.SecurityNote(Critical = "This cache stores strings that contain domain/user name/password. Must not be settable from PT code.")]
        [SecurityCritical]
        volatile MruCache<string, string> credentialHashCache;

        [Fx.Tag.SecurityNote(Critical = "This hash algorithm takes strings that contain domain/user name/password. Must not be settable from PT code.")]
        [SecurityCritical]
        HashAlgorithm hashAlgorithm;
        bool keepAliveEnabled;
        int maxBufferSize;
        IWebProxy proxy;
        WebProxyFactory proxyFactory;
        SecurityCredentialsManager channelCredentials;
        SecurityTokenManager securityTokenManager;
        TransferMode transferMode;
        ISecurityCapabilities securityCapabilities;
        WebSocketTransportSettings webSocketSettings;
        ConnectionBufferPool bufferPool;
        Lazy<string> webSocketSoapContentType;
        string uniqueConnectionGroupNamePrefix;

        internal HttpChannelFactory(HttpTransportBindingElement bindingElement, BindingContext context)
            : base(bindingElement, context, HttpTransportDefaults.GetDefaultMessageEncoderFactory())
        {
            // validate setting interactions
            if (bindingElement.TransferMode == TransferMode.Buffered)
            {
                if (bindingElement.MaxReceivedMessageSize > int.MaxValue)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ArgumentOutOfRangeException("bindingElement.MaxReceivedMessageSize",
                        SR.GetString(SR.MaxReceivedMessageSizeMustBeInIntegerRange)));
                }

                if (bindingElement.MaxBufferSize != bindingElement.MaxReceivedMessageSize)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("bindingElement",
                        SR.GetString(SR.MaxBufferSizeMustMatchMaxReceivedMessageSize));
                }
            }
            else
            {
                if (bindingElement.MaxBufferSize > bindingElement.MaxReceivedMessageSize)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("bindingElement",
                        SR.GetString(SR.MaxBufferSizeMustNotExceedMaxReceivedMessageSize));
                }
            }

            if (TransferModeHelper.IsRequestStreamed(bindingElement.TransferMode) &&
                bindingElement.AuthenticationScheme != AuthenticationSchemes.Anonymous)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("bindingElement", SR.GetString(
                    SR.HttpAuthDoesNotSupportRequestStreaming));
            }

            this.allowCookies = bindingElement.AllowCookies;
#pragma warning disable 618
            if (!this.allowCookies)
            {
                Collection<HttpCookieContainerBindingElement> httpCookieContainerBindingElements = context.BindingParameters.FindAll<HttpCookieContainerBindingElement>();
                if (httpCookieContainerBindingElements.Count > 1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MultipleCCbesInParameters, typeof(HttpCookieContainerBindingElement))));
                }
                if (httpCookieContainerBindingElements.Count == 1)
                {
                    this.allowCookies = true;
                    context.BindingParameters.Remove<HttpCookieContainerBindingElement>();
                }
            }
#pragma warning restore 618

            if (this.allowCookies)
            {
                this.httpCookieContainerManager = new HttpCookieContainerManager();
            }

            if (!bindingElement.AuthenticationScheme.IsSingleton())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.HttpRequiresSingleAuthScheme,
                    bindingElement.AuthenticationScheme));
            }

            this.authenticationScheme = bindingElement.AuthenticationScheme;
            this.decompressionEnabled = bindingElement.DecompressionEnabled;
            this.keepAliveEnabled = bindingElement.KeepAliveEnabled;
            this.maxBufferSize = bindingElement.MaxBufferSize;
            this.transferMode = bindingElement.TransferMode;

            if (bindingElement.Proxy != null)
            {
                this.proxy = bindingElement.Proxy;
            }
            else if (bindingElement.ProxyAddress != null)
            {
                if (bindingElement.UseDefaultWebProxy)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.UseDefaultWebProxyCantBeUsedWithExplicitProxyAddress)));
                }

                if (bindingElement.ProxyAuthenticationScheme == AuthenticationSchemes.Anonymous)
                {
                    this.proxy = new WebProxy(bindingElement.ProxyAddress, bindingElement.BypassProxyOnLocal);
                }
                else
                {
                    this.proxy = null;
                    this.proxyFactory =
                        new WebProxyFactory(bindingElement.ProxyAddress, bindingElement.BypassProxyOnLocal,
                        bindingElement.ProxyAuthenticationScheme);
                }
            }
            else if (!bindingElement.UseDefaultWebProxy)
            {
                this.proxy = new WebProxy();
            }

            this.channelCredentials = context.BindingParameters.Find<SecurityCredentialsManager>();
            this.securityCapabilities = bindingElement.GetProperty<ISecurityCapabilities>(context);
            this.webSocketSettings = WebSocketHelper.GetRuntimeWebSocketSettings(bindingElement.WebSocketSettings);

            int webSocketBufferSize = WebSocketHelper.ComputeClientBufferSize(this.MaxReceivedMessageSize);
            this.bufferPool = new ConnectionBufferPool(webSocketBufferSize);

            Collection<ClientWebSocketFactory> clientWebSocketFactories = context.BindingParameters.FindAll<ClientWebSocketFactory>();
            if (clientWebSocketFactories.Count > 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                    "context", 
                    SR.GetString(SR.MultipleClientWebSocketFactoriesSpecified, typeof(BindingContext).Name, typeof(ClientWebSocketFactory).Name));
            }
            else
            {
                this.clientWebSocketFactory = clientWebSocketFactories.Count == 0 ? null : clientWebSocketFactories[0];
            }

            this.webSocketSoapContentType = new Lazy<string>(() => { return this.MessageEncoderFactory.CreateSessionEncoder().ContentType; }, LazyThreadSafetyMode.ExecutionAndPublication);

            if (ServiceModelAppSettings.HttpTransportPerFactoryConnectionPool)
            {
                this.uniqueConnectionGroupNamePrefix = Interlocked.Increment(ref connectionGroupNamePrefix).ToString();
            }
            else
            {
                this.uniqueConnectionGroupNamePrefix = string.Empty;
            }
        }

        public bool AllowCookies
        {
            get
            {
                return this.allowCookies;
            }
        }

        public AuthenticationSchemes AuthenticationScheme
        {
            get
            {
                return this.authenticationScheme;
            }
        }

        public bool DecompressionEnabled
        {
            get
            {
                return this.decompressionEnabled;
            }
        }

        public virtual bool IsChannelBindingSupportEnabled
        {
            get
            {
                return false;
            }
        }

        public bool KeepAliveEnabled
        {
            get
            {
                return this.keepAliveEnabled;
            }
        }

        public SecurityTokenManager SecurityTokenManager
        {
            get
            {
                return this.securityTokenManager;
            }
        }

        public int MaxBufferSize
        {
            get
            {
                return maxBufferSize;
            }
        }

        public IWebProxy Proxy
        {
            get
            {
                return this.proxy;
            }
        }

        public TransferMode TransferMode
        {
            get
            {
                return transferMode;
            }
        }

        public override string Scheme
        {
            get
            {
                return Uri.UriSchemeHttp;
            }
        }

        public WebSocketTransportSettings WebSocketSettings
        {
            get { return this.webSocketSettings; }
        }

        internal string WebSocketSoapContentType
        {
            get
            {
                return this.webSocketSoapContentType.Value;
            }
        }

        protected ConnectionBufferPool WebSocketBufferPool
        {
            get { return this.bufferPool; }
        }

        // must be called under lock (this.credentialHashCache)
        HashAlgorithm HashAlgorithm
        {
            [SecurityCritical]
            get
            {
                if (this.hashAlgorithm == null)
                {
                    this.hashAlgorithm = CryptoHelper.CreateHashAlgorithm(SecurityAlgorithms.Sha256Digest);
                }
                else
                {
                    this.hashAlgorithm.Initialize();
                }

                return this.hashAlgorithm;
            }
        }

        int IHttpTransportFactorySettings.MaxBufferSize
        {
            get { return MaxBufferSize; }
        }

        TransferMode IHttpTransportFactorySettings.TransferMode
        {
            get { return TransferMode; }
        }

        protected ClientWebSocketFactory ClientWebSocketFactory
        {
            get
            {
                return this.clientWebSocketFactory;
            }
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return (T)(object)this.securityCapabilities;
            }
            if (typeof(T) == typeof(IHttpCookieContainerManager))
            {
                return (T)(object)this.GetHttpCookieContainerManager();
            }

            return base.GetProperty<T>();
        }

        [PermissionSet(SecurityAction.Demand, Unrestricted = true), SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private HttpCookieContainerManager GetHttpCookieContainerManager()
        {
            return this.httpCookieContainerManager;
        }

        internal virtual SecurityMessageProperty CreateReplySecurityProperty(HttpWebRequest request,
            HttpWebResponse response)
        {
            // Don't pull in System.Authorization if we don't need to!
            if (!response.IsMutuallyAuthenticated)
            {
                return null;
            }

            return CreateMutuallyAuthenticatedReplySecurityProperty(response);
        }

        internal Exception CreateToMustEqualViaException(Uri to, Uri via)
        {
            return new ArgumentException(SR.GetString(SR.HttpToMustEqualVia, to, via));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        SecurityMessageProperty CreateMutuallyAuthenticatedReplySecurityProperty(HttpWebResponse response)
        {
            string spn = AuthenticationManager.CustomTargetNameDictionary[response.ResponseUri.AbsoluteUri];
            if (spn == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(SR.GetString(SR.HttpSpnNotFound,
                    response.ResponseUri)));
            }
            ReadOnlyCollection<IAuthorizationPolicy> spnPolicies = SecurityUtils.CreatePrincipalNameAuthorizationPolicies(spn);
            SecurityMessageProperty remoteSecurity = new SecurityMessageProperty();
            remoteSecurity.TransportToken = new SecurityTokenSpecification(null, spnPolicies);
            remoteSecurity.ServiceSecurityContext = new ServiceSecurityContext(spnPolicies);
            return remoteSecurity;
        }

        internal override int GetMaxBufferSize()
        {
            return MaxBufferSize;
        }

        SecurityTokenProviderContainer CreateAndOpenTokenProvider(TimeSpan timeout, AuthenticationSchemes authenticationScheme,
            EndpointAddress target, Uri via, ChannelParameterCollection channelParameters)
        {
            SecurityTokenProvider tokenProvider = null;
            switch (authenticationScheme)
            {
                case AuthenticationSchemes.Anonymous:
                    break;
                case AuthenticationSchemes.Basic:
                    tokenProvider = TransportSecurityHelpers.GetUserNameTokenProvider(this.SecurityTokenManager, target, via, this.Scheme, authenticationScheme, channelParameters);
                    break;
                case AuthenticationSchemes.Negotiate:
                case AuthenticationSchemes.Ntlm:
                    tokenProvider = TransportSecurityHelpers.GetSspiTokenProvider(this.SecurityTokenManager, target, via, this.Scheme, authenticationScheme, channelParameters);
                    break;
                case AuthenticationSchemes.Digest:
                    tokenProvider = TransportSecurityHelpers.GetDigestTokenProvider(this.SecurityTokenManager, target, via, this.Scheme, authenticationScheme, channelParameters);
                    break;
                default:
                    // The setter for this property should prevent this.
                    throw Fx.AssertAndThrow("CreateAndOpenTokenProvider: Invalid authentication scheme");
            }
            SecurityTokenProviderContainer result;
            if (tokenProvider != null)
            {
                result = new SecurityTokenProviderContainer(tokenProvider);
                result.Open(timeout);
            }
            else
            {
                result = null;
            }
            return result;
        }

        protected virtual void ValidateCreateChannelParameters(EndpointAddress remoteAddress, Uri via)
        {
            base.ValidateScheme(via);

            if (this.MessageVersion.Addressing == AddressingVersion.None && remoteAddress.Uri != via)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateToMustEqualViaException(remoteAddress.Uri, via));
            }
        }

        protected override TChannel OnCreateChannel(EndpointAddress remoteAddress, Uri via)
        {
            EndpointAddress httpRemoteAddress = remoteAddress != null && WebSocketHelper.IsWebSocketUri(remoteAddress.Uri) ?
                new EndpointAddress(WebSocketHelper.NormalizeWsSchemeWithHttpScheme(remoteAddress.Uri), remoteAddress) :
                remoteAddress;

            Uri httpVia = WebSocketHelper.IsWebSocketUri(via) ? WebSocketHelper.NormalizeWsSchemeWithHttpScheme(via) : via;
            return this.OnCreateChannelCore(httpRemoteAddress, httpVia);
        }

        protected virtual TChannel OnCreateChannelCore(EndpointAddress remoteAddress, Uri via)
        {
            ValidateCreateChannelParameters(remoteAddress, via);
            this.ValidateWebSocketTransportUsage();

            if (typeof(TChannel) == typeof(IRequestChannel))
            {
                return (TChannel)(object)new HttpRequestChannel((HttpChannelFactory<IRequestChannel>)(object)this, remoteAddress, via, ManualAddressing);
            }
            else
            {
                return (TChannel)(object)new ClientWebSocketTransportDuplexSessionChannel((HttpChannelFactory<IDuplexSessionChannel>)(object)this, this.clientWebSocketFactory, remoteAddress, via, this.WebSocketBufferPool);
            }
        }

        protected void ValidateWebSocketTransportUsage()
        {
            Type channelType = typeof(TChannel);
            if (channelType == typeof(IRequestChannel) && this.WebSocketSettings.TransportUsage == WebSocketTransportUsage.Always)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(
                            SR.WebSocketCannotCreateRequestClientChannelWithCertainWebSocketTransportUsage,
                            typeof(TChannel),
                            WebSocketTransportSettings.TransportUsageMethodName,
                            typeof(WebSocketTransportSettings).Name,
                            this.WebSocketSettings.TransportUsage)));

            }
            else if (channelType == typeof(IDuplexSessionChannel))
            {
                if (this.WebSocketSettings.TransportUsage == WebSocketTransportUsage.Never)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(
                                SR.WebSocketCannotCreateRequestClientChannelWithCertainWebSocketTransportUsage,
                                typeof(TChannel),
                                WebSocketTransportSettings.TransportUsageMethodName,
                                typeof(WebSocketTransportSettings).Name,
                                this.WebSocketSettings.TransportUsage)));
                }
                else if (!WebSocketHelper.OSSupportsWebSockets() && this.ClientWebSocketFactory == null)
                {
                    throw FxTrace.Exception.AsError(new PlatformNotSupportedException(SR.GetString(SR.WebSocketsClientSideNotSupported, typeof(ClientWebSocketFactory).FullName)));
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void InitializeSecurityTokenManager()
        {
            if (this.channelCredentials == null)
            {
                this.channelCredentials = ClientCredentials.CreateDefaultCredentials();
            }
            this.securityTokenManager = this.channelCredentials.CreateSecurityTokenManager();
        }

        protected virtual bool IsSecurityTokenManagerRequired()
        {
            if (this.AuthenticationScheme != AuthenticationSchemes.Anonymous)
            {
                return true;
            }
            if (this.proxyFactory != null && this.proxyFactory.AuthenticationScheme != AuthenticationSchemes.Anonymous)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnOpen(timeout);
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            if (IsSecurityTokenManagerRequired())
            {
                this.InitializeSecurityTokenManager();
            }

            if (this.AllowCookies &&
                !this.httpCookieContainerManager.IsInitialized) // We don't want to overwrite the CookieContainer if someone has set it already.
            {                
                this.httpCookieContainerManager.CookieContainer = new CookieContainer();
            }

            // we need to make sure System.Net will buffer faults (sent as 500 requests) up to our allowed size
            // Their value is in Kbytes and ours is in bytes. We round up so that the KB value is large enough to
            // encompass our MaxReceivedMessageSize. See MB#20860 and related for details

            if (!httpWebRequestWebPermissionDenied && HttpWebRequest.DefaultMaximumErrorResponseLength != -1)
            {
                int MaxReceivedMessageSizeKbytes;
                if (MaxBufferSize >= (int.MaxValue - 1024)) // make sure NCL doesn't overflow
                {
                    MaxReceivedMessageSizeKbytes = -1;
                }
                else
                {
                    MaxReceivedMessageSizeKbytes = (int)(MaxBufferSize / 1024);
                    if (MaxReceivedMessageSizeKbytes * 1024 < MaxBufferSize)
                    {
                        MaxReceivedMessageSizeKbytes++;
                    }
                }

                if (MaxReceivedMessageSizeKbytes == -1
                    || MaxReceivedMessageSizeKbytes > HttpWebRequest.DefaultMaximumErrorResponseLength)
                {
                    try
                    {
                        HttpWebRequest.DefaultMaximumErrorResponseLength = MaxReceivedMessageSizeKbytes;
                    }
                    catch (SecurityException exception)
                    {
                        // CSDMain\33725 - setting DefaultMaximumErrorResponseLength should not fail HttpChannelFactory.OnOpen
                        // if the user does not have the permission to do so. 
                        httpWebRequestWebPermissionDenied = true;

                        DiagnosticUtility.TraceHandledException(exception, TraceEventType.Warning);
                    }
                }
            }
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            if (this.bufferPool != null)
            {
                this.bufferPool.Close();
            }
        }

        static internal void TraceResponseReceived(HttpWebResponse response, Message message, object receiver)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                if (response != null && response.ResponseUri != null)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.HttpResponseReceived, SR.GetString(SR.TraceCodeHttpResponseReceived), new StringTraceRecord("ResponseUri", response.ResponseUri.ToString()), receiver, null, message);
                }
                else
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.HttpResponseReceived, SR.GetString(SR.TraceCodeHttpResponseReceived), receiver, message);
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Uses unsafe critical method AppendWindowsAuthenticationInfo to access the credential domain/user name/password.")]
        [SecurityCritical]
        [MethodImpl(MethodImplOptions.NoInlining)]
        string AppendWindowsAuthenticationInfo(string inputString, NetworkCredential credential,
            AuthenticationLevel authenticationLevel, TokenImpersonationLevel impersonationLevel)
        {
            return SecurityUtils.AppendWindowsAuthenticationInfo(inputString, credential, authenticationLevel, impersonationLevel);
        }

        protected virtual string OnGetConnectionGroupPrefix(HttpWebRequest httpWebRequest, SecurityTokenContainer clientCertificateToken)
        {
            return string.Empty;
        }

        internal static bool IsWindowsAuth(AuthenticationSchemes authScheme)
        {
            Fx.Assert(authScheme.IsSingleton(), "authenticationScheme used in an Http(s)ChannelFactory must be a singleton value.");

            return authScheme == AuthenticationSchemes.Negotiate ||
                authScheme == AuthenticationSchemes.Ntlm;
        }

        [Fx.Tag.SecurityNote(Critical = "Uses unsafe critical method AppendWindowsAuthenticationInfo to access the credential domain/user name/password.",
            Safe = "Uses the domain/user name/password to store and compute a hash. The store is SecurityCritical. The hash leaks but" +
            "the hash cannot be reversed to the domain/user name/password.")]
        [SecuritySafeCritical]
        string GetConnectionGroupName(HttpWebRequest httpWebRequest, NetworkCredential credential, AuthenticationLevel authenticationLevel,
            TokenImpersonationLevel impersonationLevel, SecurityTokenContainer clientCertificateToken)
        {
            if (this.credentialHashCache == null)
            {
                lock (ThisLock)
                {
                    if (this.credentialHashCache == null)
                    {
                        this.credentialHashCache = new MruCache<string, string>(5);
                    }
                }
            }

            // The following line is a work-around for VSWhidbey 558605.  In particular, we need to isolate our 
            // connection groups based on whether we are streaming the request.
            string inputString = TransferModeHelper.IsRequestStreamed(this.TransferMode) ? "streamed" : string.Empty;

            if (IsWindowsAuth(this.AuthenticationScheme))
            {
                // for NTLM & Negotiate, System.Net doesn't pool connections by default. This is because
                // IIS doesn't re-authenticate NTLM connections (made for a perf reason), and HttpWebRequest
                // shared connections among multiple callers. 
                // This causes Indigo a performance problem in turn. We mitigate this by (1) enabling
                // connection sharing for NTLM connections on our pool, and (2) scoping the pool we use
                // to be based on the NetworkCredential that is being used to authenticate the connection.
                // Therefore we're only sharing connections among the same Credential.

                // Setting this will fail in partial trust, and that's ok since this is an optimization.
                if (!httpWebRequestWebPermissionDenied)
                {
                    try
                    {
                        httpWebRequest.UnsafeAuthenticatedConnectionSharing = true;
                    }
                    catch (SecurityException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        httpWebRequestWebPermissionDenied = true;
                    }
                }

                inputString = AppendWindowsAuthenticationInfo(inputString, credential, authenticationLevel, impersonationLevel);
            }

            string prefix = this.OnGetConnectionGroupPrefix(httpWebRequest, clientCertificateToken);
            inputString = string.Concat(this.uniqueConnectionGroupNamePrefix, prefix, inputString);

            string credentialHash = null;

            // we have to lock around each call to TryGetValue since the MruCache modifies the
            // contents of it's mruList in a single-threaded manner underneath TryGetValue

            if (!string.IsNullOrEmpty(inputString))
            {
                lock (this.credentialHashCache)
                {
                    if (!this.credentialHashCache.TryGetValue(inputString, out credentialHash))
                    {
                        byte[] inputBytes = new UTF8Encoding().GetBytes(inputString);
                        byte[] digestBytes = this.HashAlgorithm.ComputeHash(inputBytes);
                        credentialHash = Convert.ToBase64String(digestBytes);
                        this.credentialHashCache.Add(inputString, credentialHash);
                    }
                }
            }

            return credentialHash;
        }

        Uri GetCredentialCacheUriPrefix(Uri via)
        {
            Uri result;

            if (this.credentialCacheUriPrefixCache == null)
            {
                lock (ThisLock)
                {
                    if (this.credentialCacheUriPrefixCache == null)
                    {
                        this.credentialCacheUriPrefixCache = new MruCache<Uri, Uri>(10);
                    }
                }
            }

            lock (this.credentialCacheUriPrefixCache)
            {
                if (!this.credentialCacheUriPrefixCache.TryGetValue(via, out result))
                {
                    result = new UriBuilder(via.Scheme, via.Host, via.Port).Uri;
                    this.credentialCacheUriPrefixCache.Add(via, result);
                }
            }

            return result;
        }

        // core code for creating an HttpWebRequest
        HttpWebRequest GetWebRequest(EndpointAddress to, Uri via, NetworkCredential credential,
            TokenImpersonationLevel impersonationLevel, AuthenticationLevel authenticationLevel,
            SecurityTokenProviderContainer proxyTokenProvider, SecurityTokenContainer clientCertificateToken, TimeSpan timeout, bool isWebSocketRequest)
        {
            Uri httpWebRequestUri = isWebSocketRequest ? WebSocketHelper.GetWebSocketUri(via) : via;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(httpWebRequestUri);
            Fx.Assert(httpWebRequest.Method.Equals("GET", StringComparison.OrdinalIgnoreCase), "the default HTTP method of HttpWebRequest should be 'Get'.");

            if (!isWebSocketRequest)
            {
                httpWebRequest.Method = "POST";

                if (TransferModeHelper.IsRequestStreamed(TransferMode))
                {
                    httpWebRequest.SendChunked = true;
                    httpWebRequest.AllowWriteStreamBuffering = false;
                }
                else
                {
                    httpWebRequest.AllowWriteStreamBuffering = true;
                }
            }

            httpWebRequest.CachePolicy = requestCachePolicy;
            httpWebRequest.KeepAlive = this.keepAliveEnabled;

            if (this.decompressionEnabled)
            {
                httpWebRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }
            else
            {
                httpWebRequest.AutomaticDecompression = DecompressionMethods.None;
            }

            if (credential != null)
            {
                CredentialCache credentials = new CredentialCache();
                credentials.Add(this.GetCredentialCacheUriPrefix(via),
                    AuthenticationSchemesHelper.ToString(this.authenticationScheme), credential);
                httpWebRequest.Credentials = credentials;
            }
            httpWebRequest.AuthenticationLevel = authenticationLevel;
            httpWebRequest.ImpersonationLevel = impersonationLevel;

            string connectionGroupName = GetConnectionGroupName(httpWebRequest, credential, authenticationLevel, impersonationLevel, clientCertificateToken);

            X509CertificateEndpointIdentity remoteCertificateIdentity = to.Identity as X509CertificateEndpointIdentity;
            if (remoteCertificateIdentity != null)
            {
                connectionGroupName = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}[{1}]", connectionGroupName, remoteCertificateIdentity.Certificates[0].Thumbprint);
            }

            if (!string.IsNullOrEmpty(connectionGroupName))
            {
                httpWebRequest.ConnectionGroupName = connectionGroupName;
            }

            if (AuthenticationScheme == AuthenticationSchemes.Basic)
            {
                httpWebRequest.PreAuthenticate = true;
            }

            if (this.proxy != null)
            {
                httpWebRequest.Proxy = this.proxy;
            }
            else if (this.proxyFactory != null)
            {
                httpWebRequest.Proxy = this.proxyFactory.CreateWebProxy(httpWebRequest, proxyTokenProvider, timeout);
            }

            if (this.AllowCookies)
            {
                httpWebRequest.CookieContainer = this.httpCookieContainerManager.CookieContainer;
            }

            // we do this at the end so that we access the correct ServicePoint
            httpWebRequest.ServicePoint.UseNagleAlgorithm = false;

            return httpWebRequest;
        }

        void ApplyManualAddressing(ref EndpointAddress to, ref Uri via, Message message)
        {
            if (ManualAddressing)
            {
                Uri toHeader = message.Headers.To;
                if (toHeader == null)
                {
                    throw TraceUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ManualAddressingRequiresAddressedMessages)), message);
                }

                to = new EndpointAddress(toHeader);

                if (this.MessageVersion.Addressing == AddressingVersion.None)
                {
                    via = toHeader;
                }
            }

            // now apply query string property
            object property;
            if (message.Properties.TryGetValue(HttpRequestMessageProperty.Name, out property))
            {
                HttpRequestMessageProperty requestProperty = (HttpRequestMessageProperty)property;
                if (!string.IsNullOrEmpty(requestProperty.QueryString))
                {
                    UriBuilder uriBuilder = new UriBuilder(via);

                    if (requestProperty.QueryString.StartsWith("?", StringComparison.Ordinal))
                    {
                        uriBuilder.Query = requestProperty.QueryString.Substring(1);
                    }
                    else
                    {
                        uriBuilder.Query = requestProperty.QueryString;
                    }

                    via = uriBuilder.Uri;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void CreateAndOpenTokenProvidersCore(EndpointAddress to, Uri via, ChannelParameterCollection channelParameters, TimeSpan timeout, out SecurityTokenProviderContainer tokenProvider, out SecurityTokenProviderContainer proxyTokenProvider)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            tokenProvider = CreateAndOpenTokenProvider(timeoutHelper.RemainingTime(), this.AuthenticationScheme, to, via, channelParameters);
            if (this.proxyFactory != null)
            {
                proxyTokenProvider = CreateAndOpenTokenProvider(timeoutHelper.RemainingTime(), this.proxyFactory.AuthenticationScheme, to, via, channelParameters);
            }
            else
            {
                proxyTokenProvider = null;
            }
        }

        internal void CreateAndOpenTokenProviders(EndpointAddress to, Uri via, ChannelParameterCollection channelParameters, TimeSpan timeout, out SecurityTokenProviderContainer tokenProvider, out SecurityTokenProviderContainer proxyTokenProvider)
        {
            if (!IsSecurityTokenManagerRequired())
            {
                tokenProvider = null;
                proxyTokenProvider = null;
            }
            else
            {
                CreateAndOpenTokenProvidersCore(to, via, channelParameters, timeout, out tokenProvider, out proxyTokenProvider);
            }
        }

        internal HttpWebRequest GetWebRequest(EndpointAddress to, Uri via, SecurityTokenProviderContainer tokenProvider,
            SecurityTokenProviderContainer proxyTokenProvider, SecurityTokenContainer clientCertificateToken, TimeSpan timeout, bool isWebSocketRequest)
        {
            TokenImpersonationLevel impersonationLevel;
            AuthenticationLevel authenticationLevel;
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            NetworkCredential credential = HttpChannelUtilities.GetCredential(this.authenticationScheme,
                tokenProvider, timeoutHelper.RemainingTime(), out impersonationLevel, out authenticationLevel);

            return GetWebRequest(to, via, credential, impersonationLevel, authenticationLevel, proxyTokenProvider, clientCertificateToken, timeoutHelper.RemainingTime(), isWebSocketRequest);
        }

        internal static bool MapIdentity(EndpointAddress target, AuthenticationSchemes authenticationScheme)
        {
            if ((target.Identity == null) || (target.Identity is X509CertificateEndpointIdentity))
            {
                return false;
            }

            return IsWindowsAuth(authenticationScheme);
        }

        bool MapIdentity(EndpointAddress target)
        {
            return MapIdentity(target, this.AuthenticationScheme);
        }

        protected class HttpRequestChannel : RequestChannel
        {
            // Double-checked locking pattern requires volatile for read/write synchronization
            volatile bool cleanupIdentity;
            HttpChannelFactory<IRequestChannel> factory;
            SecurityTokenProviderContainer tokenProvider;
            SecurityTokenProviderContainer proxyTokenProvider;
            ServiceModelActivity activity = null;
            ChannelParameterCollection channelParameters;

            public HttpRequestChannel(HttpChannelFactory<IRequestChannel> factory, EndpointAddress to, Uri via, bool manualAddressing)
                : base(factory, to, via, manualAddressing)
            {
                this.factory = factory;
            }

            public HttpChannelFactory<IRequestChannel> Factory
            {
                get { return this.factory; }
            }

            internal ServiceModelActivity Activity
            {
                get { return this.activity; }
            }

            protected ChannelParameterCollection ChannelParameters
            {
                get
                {
                    return this.channelParameters;
                }
            }

            public override T GetProperty<T>()
            {
                if (typeof(T) == typeof(ChannelParameterCollection))
                {
                    if (this.State == CommunicationState.Created)
                    {
                        lock (ThisLock)
                        {
                            if (this.channelParameters == null)
                            {
                                this.channelParameters = new ChannelParameterCollection();
                            }
                        }
                    }
                    return (T)(object)this.channelParameters;
                }
                else
                {
                    return base.GetProperty<T>();
                }
            }

            void PrepareOpen()
            {
                if (Factory.MapIdentity(RemoteAddress))
                {
                    lock (ThisLock)
                    {
                        cleanupIdentity = HttpTransportSecurityHelpers.AddIdentityMapping(Via, RemoteAddress);
                    }
                }
            }

            void CreateAndOpenTokenProviders(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                if (!ManualAddressing)
                {
                    Factory.CreateAndOpenTokenProviders(this.RemoteAddress, this.Via, this.channelParameters, timeoutHelper.RemainingTime(), out this.tokenProvider, out this.proxyTokenProvider);
                }
            }

            void CloseTokenProviders(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                if (this.tokenProvider != null)
                {
                    tokenProvider.Close(timeoutHelper.RemainingTime());
                }
                if (this.proxyTokenProvider != null)
                {
                    proxyTokenProvider.Close(timeoutHelper.RemainingTime());
                }
            }

            void AbortTokenProviders()
            {
                if (this.tokenProvider != null)
                {
                    tokenProvider.Abort();
                }
                if (this.proxyTokenProvider != null)
                {
                    proxyTokenProvider.Abort();
                }
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                PrepareOpen();
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                CreateAndOpenTokenProviders(timeoutHelper.RemainingTime());
                return new CompletedAsyncResult(callback, state);
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                PrepareOpen();
                CreateAndOpenTokenProviders(timeout);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }

            void PrepareClose(bool aborting)
            {
                if (cleanupIdentity)
                {
                    lock (ThisLock)
                    {
                        if (cleanupIdentity)
                        {
                            cleanupIdentity = false;
                            HttpTransportSecurityHelpers.RemoveIdentityMapping(Via, RemoteAddress, !aborting);
                        }
                    }
                }
            }

            protected override void OnAbort()
            {
                PrepareClose(true);
                AbortTokenProviders();
                base.OnAbort();
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                IAsyncResult retval = null;
                using (ServiceModelActivity.BoundOperation(this.activity))
                {
                    PrepareClose(false);
                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                    CloseTokenProviders(timeoutHelper.RemainingTime());
                    retval = base.BeginWaitForPendingRequests(timeoutHelper.RemainingTime(), callback, state);
                }
                ServiceModelActivity.Stop(this.activity);
                return retval;
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                using (ServiceModelActivity.BoundOperation(this.activity))
                {
                    base.EndWaitForPendingRequests(result);
                }
                ServiceModelActivity.Stop(this.activity);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                using (ServiceModelActivity.BoundOperation(this.activity))
                {
                    PrepareClose(false);
                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                    CloseTokenProviders(timeoutHelper.RemainingTime());
                    base.WaitForPendingRequests(timeoutHelper.RemainingTime());
                }
                ServiceModelActivity.Stop(this.activity);
            }

            protected override IAsyncRequest CreateAsyncRequest(Message message, AsyncCallback callback, object state)
            {
                if (DiagnosticUtility.ShouldUseActivity && this.activity == null)
                {
                    this.activity = ServiceModelActivity.CreateActivity();
                    if (null != FxTrace.Trace)
                    {
                        FxTrace.Trace.TraceTransfer(this.activity.Id);
                    }
                    ServiceModelActivity.Start(this.activity, SR.GetString(SR.ActivityReceiveBytes, this.RemoteAddress.Uri.ToString()), ActivityType.ReceiveBytes);
                }

                return new HttpChannelAsyncRequest(this, callback, state);
            }

            protected override IRequest CreateRequest(Message message)
            {
                return new HttpChannelRequest(this, Factory);
            }

            public virtual HttpWebRequest GetWebRequest(EndpointAddress to, Uri via, ref TimeoutHelper timeoutHelper)
            {
                return GetWebRequest(to, via, null, ref timeoutHelper);
            }

            protected HttpWebRequest GetWebRequest(EndpointAddress to, Uri via, SecurityTokenContainer clientCertificateToken, ref TimeoutHelper timeoutHelper)
            {
                SecurityTokenProviderContainer webRequestTokenProvider;
                SecurityTokenProviderContainer webRequestProxyTokenProvider;
                if (this.ManualAddressing)
                {
                    this.Factory.CreateAndOpenTokenProviders(to, via, this.channelParameters, timeoutHelper.RemainingTime(),
                        out webRequestTokenProvider, out webRequestProxyTokenProvider);
                }
                else
                {
                    webRequestTokenProvider = this.tokenProvider;
                    webRequestProxyTokenProvider = this.proxyTokenProvider;
                }
                try
                {
                    return this.Factory.GetWebRequest(to, via, webRequestTokenProvider, webRequestProxyTokenProvider, clientCertificateToken, timeoutHelper.RemainingTime(), false);
                }
                finally
                {
                    if (this.ManualAddressing)
                    {
                        if (webRequestTokenProvider != null)
                        {
                            webRequestTokenProvider.Abort();
                        }
                        if (webRequestProxyTokenProvider != null)
                        {
                            webRequestProxyTokenProvider.Abort();
                        }
                    }
                }
            }

            protected IAsyncResult BeginGetWebRequest(
                EndpointAddress to, Uri via, SecurityTokenContainer clientCertificateToken, ref TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
            {
                return new GetWebRequestAsyncResult(this, to, via, clientCertificateToken, ref timeoutHelper, callback, state);
            }

            public virtual IAsyncResult BeginGetWebRequest(
                EndpointAddress to, Uri via, ref TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
            {
                return BeginGetWebRequest(to, via, null, ref timeoutHelper, callback, state);
            }

            public virtual HttpWebRequest EndGetWebRequest(IAsyncResult result)
            {
                return GetWebRequestAsyncResult.End(result);
            }

            public virtual bool WillGetWebRequestCompleteSynchronously()
            {
                return ((this.tokenProvider == null) && !Factory.ManualAddressing);
            }

            internal virtual void OnWebRequestCompleted(HttpWebRequest request)
            {
                // empty
            }

            class HttpChannelRequest : IRequest
            {
                HttpRequestChannel channel;
                HttpChannelFactory<IRequestChannel> factory;
                EndpointAddress to;
                Uri via;
                HttpWebRequest webRequest;
                HttpAbortReason abortReason;
                ChannelBinding channelBinding;
                int webRequestCompleted;
                EventTraceActivity eventTraceActivity;
                const string ConnectionGroupPrefixMessagePropertyName = "HttpTransportConnectionGroupNamePrefix";

                public HttpChannelRequest(HttpRequestChannel channel, HttpChannelFactory<IRequestChannel> factory)
                {
                    this.channel = channel;
                    this.to = channel.RemoteAddress;
                    this.via = channel.Via;
                    this.factory = factory;
                }

                private string GetConnectionGroupPrefix(Message message)
                {
                    object property;
                    if (message.Properties.TryGetValue(ConnectionGroupPrefixMessagePropertyName, out property))
                    {
                        string prefix = property as string;
                        if (prefix != null)
                        {
                            return prefix;
                        }
                    }

                    return string.Empty;
                }

                public void SendRequest(Message message, TimeSpan timeout)
                {
                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                    factory.ApplyManualAddressing(ref this.to, ref this.via, message);
                    this.webRequest = channel.GetWebRequest(this.to, this.via, ref timeoutHelper);
                    this.webRequest.ConnectionGroupName = GetConnectionGroupPrefix(message) + this.webRequest.ConnectionGroupName;

                    Message request = message;

                    try
                    {
                        if (channel.State != CommunicationState.Opened)
                        {
                            // if we were aborted while getting our request or doing correlation, 
                            // we need to abort the web request and bail
                            Cleanup();
                            channel.ThrowIfDisposedOrNotOpen();
                        }

                        HttpChannelUtilities.SetRequestTimeout(this.webRequest, timeoutHelper.RemainingTime());
                        HttpOutput httpOutput = HttpOutput.CreateHttpOutput(this.webRequest, this.factory, request, this.factory.IsChannelBindingSupportEnabled);

                        bool success = false;
                        try
                        {

                            httpOutput.Send(timeoutHelper.RemainingTime());

                            this.channelBinding = httpOutput.TakeChannelBinding();
                            httpOutput.Close();
                            success = true;

                            if (FxTrace.Trace.IsEnd2EndActivityTracingEnabled)
                            {
                                this.eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
                                if (TD.MessageSentByTransportIsEnabled())
                                {
                                    TD.MessageSentByTransport(eventTraceActivity, this.to.Uri.AbsoluteUri);
                                }
                            }
                        }
                        finally
                        {
                            if (!success)
                            {
                                httpOutput.Abort(HttpAbortReason.Aborted);
                            }
                        }
                    }
                    finally
                    {
                        if (!object.ReferenceEquals(request, message))
                        {
                            request.Close();
                        }
                    }
                }

                void Cleanup()
                {
                    if (this.webRequest != null)
                    {
                        HttpChannelUtilities.AbortRequest(this.webRequest);
                        this.TryCompleteWebRequest(this.webRequest);
                    }

                    ChannelBindingUtility.Dispose(ref this.channelBinding);
                }

                public void Abort(RequestChannel channel)
                {
                    Cleanup();
                    abortReason = HttpAbortReason.Aborted;
                }

                public void Fault(RequestChannel channel)
                {
                    Cleanup();
                }

                [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, "Reliability104",
                            Justification = "This is an old method from previous release.")]
                public Message WaitForReply(TimeSpan timeout)
                {
                    if (TD.HttpResponseReceiveStartIsEnabled())
                    {
                        TD.HttpResponseReceiveStart(this.eventTraceActivity);
                    }

                    HttpWebResponse response = null;
                    WebException responseException = null;
                    try
                    {
                        try
                        {
                            response = (HttpWebResponse)webRequest.GetResponse();
                        }
                        catch (NullReferenceException nullReferenceException)
                        {
                            // workaround for Whidbey bug #558605 - only happens in streamed case.
                            if (TransferModeHelper.IsRequestStreamed(this.factory.transferMode))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                    HttpChannelUtilities.CreateNullReferenceResponseException(nullReferenceException));
                            }
                            throw;
                        }

                        if (TD.MessageReceivedByTransportIsEnabled())
                        {
                            TD.MessageReceivedByTransport(this.eventTraceActivity ?? EventTraceActivity.Empty,
                                response.ResponseUri != null ? response.ResponseUri.AbsoluteUri : string.Empty,
                                EventTraceActivity.GetActivityIdFromThread());
                        }

                        if (DiagnosticUtility.ShouldTraceVerbose)
                        {
                            HttpChannelFactory<TChannel>.TraceResponseReceived(response, null, this);
                        }
                    }
                    catch (WebException webException)
                    {
                        responseException = webException;
                        response = HttpChannelUtilities.ProcessGetResponseWebException(webException, this.webRequest,
                            abortReason);
                    }

                    HttpInput httpInput = HttpChannelUtilities.ValidateRequestReplyResponse(this.webRequest, response,
                        this.factory, responseException, this.channelBinding);
                    this.channelBinding = null;

                    Message replyMessage = null;
                    if (httpInput != null)
                    {
                        Exception exception = null;
                        replyMessage = httpInput.ParseIncomingMessage(out exception);
                        Fx.Assert(exception == null, "ParseIncomingMessage should not set an exception after parsing a response message.");

                        if (replyMessage != null)
                        {
                            HttpChannelUtilities.AddReplySecurityProperty(this.factory, this.webRequest, response,
                                replyMessage);

                            if (FxTrace.Trace.IsEnd2EndActivityTracingEnabled && (eventTraceActivity != null))
                            {
                                EventTraceActivityHelper.TryAttachActivity(replyMessage, eventTraceActivity);
                            }
                        }
                    }

                    this.TryCompleteWebRequest(this.webRequest);
                    return replyMessage;
                }

                public void OnReleaseRequest()
                {
                    this.TryCompleteWebRequest(this.webRequest);
                }

                void TryCompleteWebRequest(HttpWebRequest request)
                {
                    if (request == null)
                    {
                        return;
                    }

                    if (Interlocked.CompareExchange(ref this.webRequestCompleted, 1, 0) == 0)
                    {
                        this.channel.OnWebRequestCompleted(request);
                    }
                }
            }

            class HttpChannelAsyncRequest : TraceAsyncResult, IAsyncRequest
            {
                static AsyncCallback onProcessIncomingMessage = Fx.ThunkCallback(new AsyncCallback(OnParseIncomingMessage));
                static AsyncCallback onGetResponse = Fx.ThunkCallback(new AsyncCallback(OnGetResponse));
                static AsyncCallback onGetWebRequestCompleted;
                static AsyncCallback onSend = Fx.ThunkCallback(new AsyncCallback(OnSend));
                static Action<object> onSendTimeout;
                ChannelBinding channelBinding;

                HttpChannelFactory<IRequestChannel> factory;
                HttpRequestChannel channel;
                HttpOutput httpOutput;
                HttpInput httpInput;
                Message message;
                Message requestMessage;
                Message replyMessage;
                HttpWebResponse response;
                HttpWebRequest request;
                object sendLock = new object();
                IOThreadTimer sendTimer;
                TimeoutHelper timeoutHelper;
                EndpointAddress to;
                Uri via;
                HttpAbortReason abortReason;
                int webRequestCompleted;
                EventTraceActivity eventTraceActivity;

                public HttpChannelAsyncRequest(HttpRequestChannel channel, AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.channel = channel;
                    this.to = channel.RemoteAddress;
                    this.via = channel.Via;
                    this.factory = channel.Factory;
                }

                IOThreadTimer SendTimer
                {
                    get
                    {
                        if (this.sendTimer == null)
                        {
                            if (onSendTimeout == null)
                            {
                                onSendTimeout = new Action<object>(OnSendTimeout);
                            }

                            this.sendTimer = new IOThreadTimer(onSendTimeout, this, false);
                        }

                        return this.sendTimer;
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<HttpChannelAsyncRequest>(result);
                }

                public void BeginSendRequest(Message message, TimeSpan timeout)
                {
                    this.message = this.requestMessage = message;
                    this.timeoutHelper = new TimeoutHelper(timeout);

                    if (FxTrace.Trace.IsEnd2EndActivityTracingEnabled)
                    {
                        this.eventTraceActivity = EventTraceActivityHelper.TryExtractActivity(message);
                    }

                    factory.ApplyManualAddressing(ref this.to, ref this.via, this.requestMessage);
                    if (this.channel.WillGetWebRequestCompleteSynchronously())
                    {
                        SetWebRequest(channel.GetWebRequest(this.to, this.via, ref this.timeoutHelper));
                        if (this.SendWebRequest())
                        {
                            base.Complete(true);
                        }
                    }
                    else
                    {
                        if (onGetWebRequestCompleted == null)
                        {
                            onGetWebRequestCompleted = Fx.ThunkCallback(
                                new AsyncCallback(OnGetWebRequestCompletedCallback));
                        }

                        IAsyncResult result = channel.BeginGetWebRequest(
                            to, via, ref this.timeoutHelper, onGetWebRequestCompleted, this);

                        if (result.CompletedSynchronously)
                        {
                            if (TD.MessageSentByTransportIsEnabled())
                            {
                                TD.MessageSentByTransport(this.eventTraceActivity, this.to.Uri.AbsoluteUri);
                            }
                            if (this.OnGetWebRequestCompleted(result))
                            {
                                base.Complete(true);
                            }
                        }
                    }
                }

                static void OnGetWebRequestCompletedCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }

                    HttpChannelAsyncRequest thisPtr = (HttpChannelAsyncRequest)result.AsyncState;
                    Exception completionException = null;
                    bool completeSelf;
                    try
                    {
                        completeSelf = thisPtr.OnGetWebRequestCompleted(result);
                    }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        completeSelf = true;
                        completionException = e;
                    }
                    if (completeSelf)
                    {
                        thisPtr.Complete(false, completionException);
                    }
                }

                void AbortSend()
                {
                    CancelSendTimer();
                    if (this.request != null)
                    {
                        this.TryCompleteWebRequest(this.request);
                        this.abortReason = HttpAbortReason.TimedOut;
                        httpOutput.Abort(this.abortReason);
                    }
                }

                void CancelSendTimer()
                {
                    lock (sendLock)
                    {
                        if (this.sendTimer != null)
                        {
                            this.sendTimer.Cancel();
                            this.sendTimer = null;
                        }
                    }
                }

                bool OnGetWebRequestCompleted(IAsyncResult result)
                {
                    SetWebRequest(this.channel.EndGetWebRequest(result));
                    return this.SendWebRequest();
                }

                bool SendWebRequest()
                {
                    this.httpOutput = HttpOutput.CreateHttpOutput(this.request, this.factory, this.requestMessage, this.factory.IsChannelBindingSupportEnabled);

                    bool success = false;
                    try
                    {
                        bool result = false;
                        SetSendTimeout(timeoutHelper.RemainingTime());
                        IAsyncResult asyncResult = httpOutput.BeginSend(timeoutHelper.RemainingTime(), onSend, this);
                        success = true;

                        if (asyncResult.CompletedSynchronously)
                        {
                            result = CompleteSend(asyncResult);
                        }

                        return result;
                    }
                    finally
                    {
                        if (!success)
                        {
                            this.httpOutput.Abort(HttpAbortReason.Aborted);

                            if (!object.ReferenceEquals(this.message, this.requestMessage))
                            {
                                this.requestMessage.Close();
                            }
                        }
                    }
                }

                bool CompleteSend(IAsyncResult result)
                {
                    bool success = false;
                    try
                    {
                        httpOutput.EndSend(result);
                        this.channelBinding = httpOutput.TakeChannelBinding();
                        httpOutput.Close();
                        success = true;
                        if (TD.MessageSentByTransportIsEnabled())
                        {
                            TD.MessageSentByTransport(this.eventTraceActivity, this.to.Uri.AbsoluteUri);
                        }
                    }
                    finally
                    {
                        if (!success)
                        {
                            httpOutput.Abort(HttpAbortReason.Aborted);
                        }

                        if (!object.ReferenceEquals(this.message, this.requestMessage))
                        {
                            this.requestMessage.Close();
                        }
                    }

                    try
                    {
                        IAsyncResult getResponseResult;
                        try
                        {
                            getResponseResult = request.BeginGetResponse(onGetResponse, this);
                        }
                        catch (NullReferenceException nullReferenceException)
                        {
                            // workaround for Whidbey bug #558605 - only happens in streamed case.
                            if (TransferModeHelper.IsRequestStreamed(this.factory.transferMode))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                    HttpChannelUtilities.CreateNullReferenceResponseException(nullReferenceException));
                            }
                            throw;
                        }


                        if (getResponseResult.CompletedSynchronously)
                        {
                            return CompleteGetResponse(getResponseResult);
                        }

                        return false;
                    }
                    catch (IOException ioException)
                    {
                        throw TraceUtility.ThrowHelperError(new CommunicationException(ioException.Message,
                            ioException), this.requestMessage);
                    }
                    catch (WebException webException)
                    {
                        throw TraceUtility.ThrowHelperError(new CommunicationException(webException.Message,
                            webException), this.requestMessage);
                    }
                    catch (ObjectDisposedException objectDisposedException)
                    {
                        if (abortReason == HttpAbortReason.Aborted)
                        {
                            throw TraceUtility.ThrowHelperError(new CommunicationObjectAbortedException(SR.GetString(SR.HttpRequestAborted, to.Uri),
                                objectDisposedException), this.requestMessage);
                        }

                        throw TraceUtility.ThrowHelperError(new TimeoutException(SR.GetString(SR.HttpRequestTimedOut,
                            to.Uri, this.timeoutHelper.OriginalTimeout), objectDisposedException), this.requestMessage);
                    }
                }

                [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, "Reliability104",
                            Justification = "This is an old method from previous release.")]
                bool CompleteGetResponse(IAsyncResult result)
                {
                    using (ServiceModelActivity.BoundOperation(this.channel.Activity))
                    {
                        HttpWebResponse response = null;
                        WebException responseException = null;
                        try
                        {
                            try
                            {
                                CancelSendTimer();
                                response = (HttpWebResponse)request.EndGetResponse(result);
                            }
                            catch (NullReferenceException nullReferenceException)
                            {
                                // workaround for Whidbey bug #558605 - only happens in streamed case.
                                if (TransferModeHelper.IsRequestStreamed(this.factory.transferMode))
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                        HttpChannelUtilities.CreateNullReferenceResponseException(nullReferenceException));
                                }
                                throw;
                            }

                            if (TD.MessageReceivedByTransportIsEnabled())
                            {
                                TD.MessageReceivedByTransport(
                                    this.eventTraceActivity ?? EventTraceActivity.Empty,
                                    this.to.Uri.AbsoluteUri,
                                    EventTraceActivity.GetActivityIdFromThread());
                            }

                            if (DiagnosticUtility.ShouldTraceVerbose)
                            {
                                HttpChannelFactory<TChannel>.TraceResponseReceived(response, this.message, this);
                            }
                        }
                        catch (WebException webException)
                        {
                            responseException = webException;
                            response = HttpChannelUtilities.ProcessGetResponseWebException(webException, request,
                                abortReason);
                        }

                        return ProcessResponse(response, responseException);
                    }
                }

                void Cleanup()
                {
                    if (this.request != null)
                    {
                        HttpChannelUtilities.AbortRequest(this.request);
                        this.TryCompleteWebRequest(this.request);
                    }

                    ChannelBindingUtility.Dispose(ref this.channelBinding);
                }

                void SetSendTimeout(TimeSpan timeout)
                {
                    // We also set the timeout on the HttpWebRequest so that we can subsequently use it in the 
                    // exception message in the event of a timeout.
                    HttpChannelUtilities.SetRequestTimeout(this.request, timeout);

                    if (timeout == TimeSpan.MaxValue)
                    {
                        CancelSendTimer();
                    }
                    else
                    {
                        SendTimer.Set(timeout);
                    }
                }

                public void Abort(RequestChannel channel)
                {
                    Cleanup();
                    abortReason = HttpAbortReason.Aborted;
                }

                public void Fault(RequestChannel channel)
                {
                    Cleanup();
                }

                void SetWebRequest(HttpWebRequest webRequest)
                {
                    this.request = webRequest;

                    if (channel.State != CommunicationState.Opened)
                    {
                        // if we were aborted while getting our request, we need to abort the web request and bail
                        Cleanup();
                        channel.ThrowIfDisposedOrNotOpen();
                    }
                }

                public Message End()
                {
                    HttpChannelAsyncRequest.End(this);
                    return replyMessage;
                }

                bool ProcessResponse(HttpWebResponse response, WebException responseException)
                {
                    this.httpInput = HttpChannelUtilities.ValidateRequestReplyResponse(this.request, response,
                        this.factory, responseException, this.channelBinding);
                    this.channelBinding = null;

                    if (httpInput != null)
                    {
                        this.response = response;
                        IAsyncResult result =
                            httpInput.BeginParseIncomingMessage(onProcessIncomingMessage, this);
                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }

                        CompleteParseIncomingMessage(result);
                    }
                    else
                    {
                        this.replyMessage = null;
                    }

                    this.TryCompleteWebRequest(this.request);
                    return true;
                }

                void CompleteParseIncomingMessage(IAsyncResult result)
                {
                    Exception exception = null;
                    this.replyMessage = this.httpInput.EndParseIncomingMessage(result, out exception);
                    Fx.Assert(exception == null, "ParseIncomingMessage should not set an exception after parsing a response message.");

                    if (this.replyMessage != null)
                    {
                        HttpChannelUtilities.AddReplySecurityProperty(this.factory, this.request, this.response,
                            this.replyMessage);
                    }
                }

                static void OnParseIncomingMessage(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }

                    HttpChannelAsyncRequest thisPtr = (HttpChannelAsyncRequest)result.AsyncState;

                    Exception completionException = null;
                    try
                    {
                        thisPtr.CompleteParseIncomingMessage(result);
                    }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        completionException = e;
                    }
                    thisPtr.Complete(false, completionException);
                }

                static void OnSend(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }

                    HttpChannelAsyncRequest thisPtr = (HttpChannelAsyncRequest)result.AsyncState;

                    Exception completionException = null;
                    bool completeSelf;
                    try
                    {
                        completeSelf = thisPtr.CompleteSend(result);
                    }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completeSelf = true;
                        completionException = e;
                    }
                    if (completeSelf)
                    {
                        thisPtr.Complete(false, completionException);
                    }
                }

                static void OnSendTimeout(object state)
                {
                    HttpChannelAsyncRequest thisPtr = (HttpChannelAsyncRequest)state;
                    thisPtr.AbortSend();
                }

                [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, "Reliability104",
                            Justification = "This is an old method from previous release.")]
                static void OnGetResponse(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }

                    HttpChannelAsyncRequest thisPtr = (HttpChannelAsyncRequest)result.AsyncState;

                    Exception completionException = null;
                    bool completeSelf;
                    try
                    {
                        completeSelf = thisPtr.CompleteGetResponse(result);
                    }
                    catch (WebException webException)
                    {
                        completeSelf = true;
                        completionException = new CommunicationException(webException.Message, webException);
                    }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }
                        completeSelf = true;
                        completionException = e;
                    }
                    if (completeSelf)
                    {
                        thisPtr.Complete(false, completionException);
                    }
                }

                public void OnReleaseRequest()
                {
                    this.TryCompleteWebRequest(this.request);
                }

                void TryCompleteWebRequest(HttpWebRequest request)
                {
                    if (request == null)
                    {
                        return;
                    }

                    if (Interlocked.CompareExchange(ref this.webRequestCompleted, 1, 0) == 0)
                    {
                        this.channel.OnWebRequestCompleted(request);
                    }
                }
            }

            class GetWebRequestAsyncResult : AsyncResult
            {
                static AsyncCallback onGetSspiCredential;
                static AsyncCallback onGetUserNameCredential;

                SecurityTokenContainer clientCertificateToken;
                HttpChannelFactory<IRequestChannel> factory;
                SecurityTokenProviderContainer proxyTokenProvider;
                HttpWebRequest request;
                EndpointAddress to;
                TimeoutHelper timeoutHelper;
                SecurityTokenProviderContainer tokenProvider;
                Uri via;

                public GetWebRequestAsyncResult(HttpRequestChannel channel,
                    EndpointAddress to, Uri via, SecurityTokenContainer clientCertificateToken, ref TimeoutHelper timeoutHelper,
                    AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.to = to;
                    this.via = via;
                    this.clientCertificateToken = clientCertificateToken;
                    this.timeoutHelper = timeoutHelper;
                    this.factory = channel.Factory;
                    this.tokenProvider = channel.tokenProvider;
                    this.proxyTokenProvider = channel.proxyTokenProvider;
                    if (factory.ManualAddressing)
                    {
                        this.factory.CreateAndOpenTokenProviders(to, via, channel.channelParameters, timeoutHelper.RemainingTime(),
                            out this.tokenProvider, out this.proxyTokenProvider);
                    }

                    bool completeSelf = false;
                    IAsyncResult result = null;
                    if (factory.AuthenticationScheme == AuthenticationSchemes.Anonymous)
                    {
                        SetupWebRequest(AuthenticationLevel.None, TokenImpersonationLevel.None, null);
                        completeSelf = true;
                    }
                    else if (factory.AuthenticationScheme == AuthenticationSchemes.Basic)
                    {
                        if (onGetUserNameCredential == null)
                        {
                            onGetUserNameCredential = Fx.ThunkCallback(new AsyncCallback(OnGetUserNameCredential));
                        }

                        result = TransportSecurityHelpers.BeginGetUserNameCredential(
                            tokenProvider, timeoutHelper.RemainingTime(), onGetUserNameCredential, this);

                        if (result.CompletedSynchronously)
                        {
                            CompleteGetUserNameCredential(result);
                            completeSelf = true;
                        }
                    }
                    else
                    {
                        if (onGetSspiCredential == null)
                        {
                            onGetSspiCredential = Fx.ThunkCallback(new AsyncCallback(OnGetSspiCredential));
                        }

                        result = TransportSecurityHelpers.BeginGetSspiCredential(
                            tokenProvider, timeoutHelper.RemainingTime(), onGetSspiCredential, this);

                        if (result.CompletedSynchronously)
                        {
                            CompleteGetSspiCredential(result);
                            completeSelf = true;
                        }
                    }

                    if (completeSelf)
                    {
                        CloseTokenProvidersIfRequired();
                        base.Complete(true);
                    }
                }

                public static HttpWebRequest End(IAsyncResult result)
                {
                    GetWebRequestAsyncResult thisPtr = AsyncResult.End<GetWebRequestAsyncResult>(result);
                    return thisPtr.request;
                }

                void CompleteGetUserNameCredential(IAsyncResult result)
                {
                    NetworkCredential credential =
                        TransportSecurityHelpers.EndGetUserNameCredential(result);
                    SetupWebRequest(AuthenticationLevel.None, TokenImpersonationLevel.None, credential);
                }

                void CompleteGetSspiCredential(IAsyncResult result)
                {
                    AuthenticationLevel authenticationLevel;
                    TokenImpersonationLevel impersonationLevel;
                    NetworkCredential credential =
                        TransportSecurityHelpers.EndGetSspiCredential(result, out impersonationLevel, out authenticationLevel);

                    if (factory.AuthenticationScheme == AuthenticationSchemes.Digest)
                    {
                        HttpChannelUtilities.ValidateDigestCredential(ref credential, impersonationLevel);
                    }
                    else if (factory.AuthenticationScheme == AuthenticationSchemes.Ntlm)
                    {
                        if (authenticationLevel == AuthenticationLevel.MutualAuthRequired)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                                SR.GetString(SR.CredentialDisallowsNtlm)));
                        }
                    }

                    SetupWebRequest(authenticationLevel, impersonationLevel, credential);
                }

                void SetupWebRequest(AuthenticationLevel authenticationLevel, TokenImpersonationLevel impersonationLevel, NetworkCredential credential)
                {
                    this.request = factory.GetWebRequest(to, via, credential, impersonationLevel,
                        authenticationLevel, this.proxyTokenProvider, this.clientCertificateToken, timeoutHelper.RemainingTime(), false);
                }

                void CloseTokenProvidersIfRequired()
                {
                    if (this.factory.ManualAddressing)
                    {
                        if (this.tokenProvider != null)
                        {
                            tokenProvider.Abort();
                        }
                        if (this.proxyTokenProvider != null)
                        {
                            proxyTokenProvider.Abort();
                        }
                    }
                }

                static void OnGetSspiCredential(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }

                    GetWebRequestAsyncResult thisPtr = (GetWebRequestAsyncResult)result.AsyncState;

                    Exception completionException = null;
                    try
                    {
                        thisPtr.CompleteGetSspiCredential(result);
                        thisPtr.CloseTokenProvidersIfRequired();
                    }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completionException = e;
                    }
                    thisPtr.Complete(false, completionException);
                }

                static void OnGetUserNameCredential(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                    {
                        return;
                    }

                    GetWebRequestAsyncResult thisPtr = (GetWebRequestAsyncResult)result.AsyncState;

                    Exception completionException = null;
                    try
                    {
                        thisPtr.CompleteGetUserNameCredential(result);
                        thisPtr.CloseTokenProvidersIfRequired();
                    }
#pragma warning suppress 56500 // Microsoft, transferring exception to another thread
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                        {
                            throw;
                        }

                        completionException = e;
                    }
                    thisPtr.Complete(false, completionException);
                }
            }
        }

        class WebProxyFactory
        {
            Uri address;
            bool bypassOnLocal;
            AuthenticationSchemes authenticationScheme;

            public WebProxyFactory(Uri address, bool bypassOnLocal, AuthenticationSchemes authenticationScheme)
            {
                this.address = address;
                this.bypassOnLocal = bypassOnLocal;

                if (!authenticationScheme.IsSingleton())
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.HttpRequiresSingleAuthScheme,
                        authenticationScheme));
                }

                this.authenticationScheme = authenticationScheme;
            }

            internal AuthenticationSchemes AuthenticationScheme
            {
                get
                {
                    return authenticationScheme;
                }
            }

            public IWebProxy CreateWebProxy(HttpWebRequest request, SecurityTokenProviderContainer tokenProvider, TimeSpan timeout)
            {
                WebProxy result = new WebProxy(this.address, this.bypassOnLocal);

                if (this.authenticationScheme != AuthenticationSchemes.Anonymous)
                {
                    TokenImpersonationLevel impersonationLevel;
                    AuthenticationLevel authenticationLevel;
                    NetworkCredential credential = HttpChannelUtilities.GetCredential(this.authenticationScheme,
                        tokenProvider, timeout, out impersonationLevel, out authenticationLevel);

                    // The impersonation level for target auth is also used for proxy auth (by System.Net).  Therefore,
                    // fail if the level stipulated for proxy auth is more restrictive than that for target auth.
                    if (!TokenImpersonationLevelHelper.IsGreaterOrEqual(impersonationLevel, request.ImpersonationLevel))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                            SR.ProxyImpersonationLevelMismatch, impersonationLevel, request.ImpersonationLevel)));
                    }

                    // The authentication level for target auth is also used for proxy auth (by System.Net).  
                    // Therefore, fail if proxy auth requires mutual authentication but target auth does not.
                    if ((authenticationLevel == AuthenticationLevel.MutualAuthRequired) &&
                        (request.AuthenticationLevel != AuthenticationLevel.MutualAuthRequired))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                            SR.ProxyAuthenticationLevelMismatch, authenticationLevel, request.AuthenticationLevel)));
                    }

                    CredentialCache credentials = new CredentialCache();
                    credentials.Add(this.address, AuthenticationSchemesHelper.ToString(this.authenticationScheme),
                        credential);
                    result.Credentials = credentials;
                }

                return result;
            }
        }
    }
}
