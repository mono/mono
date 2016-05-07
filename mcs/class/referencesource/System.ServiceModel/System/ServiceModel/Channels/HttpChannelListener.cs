//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.WebSockets;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;

    abstract class HttpChannelListener : TransportChannelListener,
        IHttpTransportFactorySettings
    {
        AuthenticationSchemes authenticationScheme;
        bool extractGroupsForWindowsAccounts;
        EndpointIdentity identity;
        bool keepAliveEnabled;
        int maxBufferSize;
        readonly int maxPendingAccepts;
        string method;
        string realm;
        readonly TimeSpan requestInitializationTimeout;
        TransferMode transferMode;
        bool unsafeConnectionNtlmAuthentication;
        ISecurityCapabilities securityCapabilities;

        SecurityCredentialsManager credentialProvider;
        SecurityTokenAuthenticator userNameTokenAuthenticator;
        SecurityTokenAuthenticator windowsTokenAuthenticator;
        ExtendedProtectionPolicy extendedProtectionPolicy;
        bool usingDefaultSpnList;
        HttpAnonymousUriPrefixMatcher anonymousUriPrefixMatcher;

        HttpMessageSettings httpMessageSettings;
        WebSocketTransportSettings webSocketSettings;

        static UriPrefixTable<ITransportManagerRegistration> transportManagerTable =
            new UriPrefixTable<ITransportManagerRegistration>(true);

        public HttpChannelListener(HttpTransportBindingElement bindingElement, BindingContext context)
            : base(bindingElement, context, HttpTransportDefaults.GetDefaultMessageEncoderFactory(),
                bindingElement.HostNameComparisonMode)
        {
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

            if (bindingElement.AuthenticationScheme.IsSet(AuthenticationSchemes.Basic) &&
                bindingElement.AuthenticationScheme.IsNotSet(AuthenticationSchemes.Digest | AuthenticationSchemes.Ntlm | AuthenticationSchemes.Negotiate) &&
                bindingElement.ExtendedProtectionPolicy.PolicyEnforcement == PolicyEnforcement.Always)
            {
                //Basic auth + PolicyEnforcement.Always doesn't make sense because basic auth can't support CBT.
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.ExtendedProtectionPolicyBasicAuthNotSupported)));
            }

            this.authenticationScheme = bindingElement.AuthenticationScheme;
            this.keepAliveEnabled = bindingElement.KeepAliveEnabled;
            this.InheritBaseAddressSettings = bindingElement.InheritBaseAddressSettings;
            this.maxBufferSize = bindingElement.MaxBufferSize;
            this.maxPendingAccepts = HttpTransportDefaults.GetEffectiveMaxPendingAccepts(bindingElement.MaxPendingAccepts);
            this.method = bindingElement.Method;
            this.realm = bindingElement.Realm;
            this.requestInitializationTimeout = bindingElement.RequestInitializationTimeout;
            this.transferMode = bindingElement.TransferMode;
            this.unsafeConnectionNtlmAuthentication = bindingElement.UnsafeConnectionNtlmAuthentication;
            this.credentialProvider = context.BindingParameters.Find<SecurityCredentialsManager>();
            this.securityCapabilities = bindingElement.GetProperty<ISecurityCapabilities>(context);
            this.extendedProtectionPolicy = GetPolicyWithDefaultSpnCollection(bindingElement.ExtendedProtectionPolicy, this.authenticationScheme, this.HostNameComparisonModeInternal, base.Uri, out this.usingDefaultSpnList);

            this.webSocketSettings = WebSocketHelper.GetRuntimeWebSocketSettings(bindingElement.WebSocketSettings);

            if (bindingElement.AnonymousUriPrefixMatcher != null)
            {
                this.anonymousUriPrefixMatcher = new HttpAnonymousUriPrefixMatcher(bindingElement.AnonymousUriPrefixMatcher);
            }

            this.httpMessageSettings = context.BindingParameters.Find<HttpMessageSettings>() ?? new HttpMessageSettings();

            if (this.httpMessageSettings.HttpMessagesSupported && this.MessageVersion != MessageVersion.None)
            {
                throw FxTrace.Exception.AsError(
                    new NotSupportedException(SR.GetString(
                            SR.MessageVersionNoneRequiredForHttpMessageSupport,
                            typeof(HttpRequestMessage).Name,
                            typeof(HttpResponseMessage).Name,
                            typeof(HttpMessageSettings).Name,
                            typeof(MessageVersion).Name,
                            typeof(MessageEncodingBindingElement).Name,
                            this.MessageVersion.ToString(),
                            MessageVersion.None.ToString())));
            }
        }

        public TimeSpan RequestInitializationTimeout
        {
            get { return this.requestInitializationTimeout; }
        }

        public WebSocketTransportSettings WebSocketSettings
        {
            get { return this.webSocketSettings; }
        }

        public HttpMessageSettings HttpMessageSettings
        {
            get { return this.httpMessageSettings; }
        }
        
        public ExtendedProtectionPolicy ExtendedProtectionPolicy
        {
            get
            {
                return this.extendedProtectionPolicy;
            }
        }

        public virtual bool IsChannelBindingSupportEnabled
        {
            get
            {
                return false;
            }
        }

        public abstract bool UseWebSocketTransport { get; }

        internal HttpAnonymousUriPrefixMatcher AnonymousUriPrefixMatcher
        {
            get
            {
                return this.anonymousUriPrefixMatcher;
            }
        }

        protected SecurityTokenAuthenticator UserNameTokenAuthenticator
        {
            get { return this.userNameTokenAuthenticator; }
        }

        internal override void ApplyHostedContext(string virtualPath, bool isMetadataListener)
        {
            base.ApplyHostedContext(virtualPath, isMetadataListener);
            AspNetEnvironment.Current.ValidateHttpSettings(virtualPath, isMetadataListener, this.usingDefaultSpnList, ref this.authenticationScheme, ref this.extendedProtectionPolicy, ref this.realm);
        }

        public AuthenticationSchemes AuthenticationScheme
        {
            get
            {
                return this.authenticationScheme;
            }
        }

        public bool KeepAliveEnabled
        {
            get
            {
                return this.keepAliveEnabled;
            }
        }

        public bool ExtractGroupsForWindowsAccounts
        {
            get
            {
                return this.extractGroupsForWindowsAccounts;
            }
        }

        public HostNameComparisonMode HostNameComparisonMode
        {
            get
            {
                return this.HostNameComparisonModeInternal;
            }
        }

        //Returns true if one of the non-anonymous authentication schemes is set on this.AuthenticationScheme
        protected bool IsAuthenticationSupported
        {
            get
            {
                return this.authenticationScheme != AuthenticationSchemes.Anonymous;
            }
        }

        bool IsAuthenticationRequired
        {
            get
            {
                return this.AuthenticationScheme.IsNotSet(AuthenticationSchemes.Anonymous);
            }
        }

        public int MaxBufferSize
        {
            get
            {
                return this.maxBufferSize;
            }
        }

        public int MaxPendingAccepts
        {
            get { return this.maxPendingAccepts; }
        }

        public virtual string Method
        {
            get
            {
                return this.method;
            }
        }

        public TransferMode TransferMode
        {
            get
            {
                return transferMode;
            }
        }

        public string Realm
        {
            get { return this.realm; }
        }

        int IHttpTransportFactorySettings.MaxBufferSize
        {
            get { return MaxBufferSize; }
        }

        TransferMode IHttpTransportFactorySettings.TransferMode
        {
            get { return TransferMode; }
        }

        public override string Scheme
        {
            get { return Uri.UriSchemeHttp; }
        }

        internal static UriPrefixTable<ITransportManagerRegistration> StaticTransportManagerTable
        {
            get
            {
                return transportManagerTable;
            }
        }

        public bool UnsafeConnectionNtlmAuthentication
        {
            get
            {
                return this.unsafeConnectionNtlmAuthentication;
            }
        }

        internal override UriPrefixTable<ITransportManagerRegistration> TransportManagerTable
        {
            get
            {
                return transportManagerTable;
            }
        }

        internal override ITransportManagerRegistration CreateTransportManagerRegistration(Uri listenUri)
        {
            return new SharedHttpTransportManager(listenUri, this);
        }

        string GetAuthType(HttpListenerContext listenerContext)
        {
            string authType = null;
            IPrincipal principal = listenerContext.User;
            if ((principal != null) && (principal.Identity != null))
            {
                authType = principal.Identity.AuthenticationType;
            }
            return authType;
        }

        protected string GetAuthType(IHttpAuthenticationContext authenticationContext)
        {
            string authType = null;
            if (authenticationContext.LogonUserIdentity != null)
            {
                authType = authenticationContext.LogonUserIdentity.AuthenticationType;
            }
            return authType;
        }

        bool IsAuthSchemeValid(string authType)
        {
            return AuthenticationSchemesHelper.DoesAuthTypeMatch(this.authenticationScheme, authType);
        }

        internal override int GetMaxBufferSize()
        {
            return MaxBufferSize;
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(EndpointIdentity))
            {
                return (T)(object)(this.identity);
            }
            else if (typeof(T) == typeof(ILogonTokenCacheManager))
            {
                object cacheManager = (object)GetIdentityModelProperty<T>();
                if (cacheManager != null)
                {
                    return (T)cacheManager;
                }
            }
            else if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return (T)(object)this.securityCapabilities;
            }
            else if (typeof(T) == typeof(ExtendedProtectionPolicy))
            {
                return (T)(object)this.extendedProtectionPolicy;
            }

            return base.GetProperty<T>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        T GetIdentityModelProperty<T>()
        {
            if (typeof(T) == typeof(EndpointIdentity))
            {
                if (this.identity == null)
                {
                    if (this.authenticationScheme.IsSet(AuthenticationSchemes.Negotiate) ||
                        this.authenticationScheme.IsSet(AuthenticationSchemes.Ntlm))
                    {
                        this.identity = SecurityUtils.CreateWindowsIdentity();
                    }
                }

                return (T)(object)this.identity;
            }
            else if (typeof(T) == typeof(ILogonTokenCacheManager)
                && (this.userNameTokenAuthenticator != null))
            {
                ILogonTokenCacheManager retVal = this.userNameTokenAuthenticator as ILogonTokenCacheManager;

                if (retVal != null)
                {
                    return (T)(object)retVal;
                }
            }

            return default(T);
        }

        internal abstract IAsyncResult BeginHttpContextReceived(
                                                HttpRequestContext context,
                                                Action acceptorCallback,
                                                AsyncCallback callback,
                                                object state);

        internal abstract bool EndHttpContextReceived(IAsyncResult result);

        [MethodImpl(MethodImplOptions.NoInlining)]
        void InitializeSecurityTokenAuthenticator()
        {
            Fx.Assert(this.IsAuthenticationSupported, "SecurityTokenAuthenticator should only be initialized when authentication is supported.");
            ServiceCredentials serviceCredentials = this.credentialProvider as ServiceCredentials;

            if (serviceCredentials != null)
            {
                if (this.AuthenticationScheme == AuthenticationSchemes.Basic)
                {
                    // when Basic authentiction is enabled - but Digest and Windows are disabled use the UsernameAuthenticationSetting
                    this.extractGroupsForWindowsAccounts = serviceCredentials.UserNameAuthentication.IncludeWindowsGroups;
                }
                else
                {
                    if (this.AuthenticationScheme.IsSet(AuthenticationSchemes.Basic) &&
                        serviceCredentials.UserNameAuthentication.IncludeWindowsGroups != serviceCredentials.WindowsAuthentication.IncludeWindowsGroups)
                    {
                        // Ensure there are no inconsistencies when Basic and (Digest and/or Ntlm and/or Negotiate) are both enabled
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SecurityTokenProviderIncludeWindowsGroupsInconsistent,
                                (AuthenticationSchemes)authenticationScheme - AuthenticationSchemes.Basic,
                                serviceCredentials.UserNameAuthentication.IncludeWindowsGroups,
                                serviceCredentials.WindowsAuthentication.IncludeWindowsGroups)));
                    }

                    this.extractGroupsForWindowsAccounts = serviceCredentials.WindowsAuthentication.IncludeWindowsGroups;
                }

                // we will only support custom and windows validation modes, if anything else is specified, we'll fall back to windows user name.
                if (serviceCredentials.UserNameAuthentication.UserNamePasswordValidationMode == UserNamePasswordValidationMode.Custom)
                {
                    this.userNameTokenAuthenticator = new CustomUserNameSecurityTokenAuthenticator(serviceCredentials.UserNameAuthentication.GetUserNamePasswordValidator());
                }
                else
                {
                    if (serviceCredentials.UserNameAuthentication.CacheLogonTokens)
                    {
                        this.userNameTokenAuthenticator = new WindowsUserNameCachingSecurityTokenAuthenticator(this.extractGroupsForWindowsAccounts,
                            serviceCredentials.UserNameAuthentication.MaxCachedLogonTokens, serviceCredentials.UserNameAuthentication.CachedLogonTokenLifetime);
                    }
                    else
                    {
                        this.userNameTokenAuthenticator = new WindowsUserNameSecurityTokenAuthenticator(this.extractGroupsForWindowsAccounts);
                    }
                }
            }
            else
            {
                this.extractGroupsForWindowsAccounts = TransportDefaults.ExtractGroupsForWindowsAccounts;
                this.userNameTokenAuthenticator = new WindowsUserNameSecurityTokenAuthenticator(this.extractGroupsForWindowsAccounts);
            }

            this.windowsTokenAuthenticator = new WindowsSecurityTokenAuthenticator(this.extractGroupsForWindowsAccounts);
        }

        protected override void OnOpened()
        {
            base.OnOpened();

            if (this.IsAuthenticationSupported)
            {
                InitializeSecurityTokenAuthenticator();
                this.identity = GetIdentityModelProperty<EndpointIdentity>();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected void CloseUserNameTokenAuthenticator(TimeSpan timeout)
        {
            SecurityUtils.CloseTokenAuthenticatorIfRequired(this.userNameTokenAuthenticator, timeout);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected void AbortUserNameTokenAuthenticator()
        {
            SecurityUtils.AbortTokenAuthenticatorIfRequired(this.userNameTokenAuthenticator);
        }

        bool ShouldProcessAuthentication(IHttpAuthenticationContext authenticationContext)
        {
            Fx.Assert(authenticationContext != null, "IsAuthenticated should only be called if authenticationContext != null");
            Fx.Assert(authenticationContext.LogonUserIdentity != null, "IsAuthenticated should only be called if authenticationContext.LogonUserIdentity != null");
            return this.IsAuthenticationRequired || (this.IsAuthenticationSupported && authenticationContext.LogonUserIdentity.IsAuthenticated);
        }

        bool ShouldProcessAuthentication(HttpListenerContext listenerContext)
        {
            Fx.Assert(listenerContext != null, "IsAuthenticated should only be called if listenerContext != null");
            Fx.Assert(listenerContext.Request != null, "IsAuthenticated should only be called if listenerContext.Request != null");
            return this.IsAuthenticationRequired || (this.IsAuthenticationSupported && listenerContext.Request.IsAuthenticated);
        }

        public virtual SecurityMessageProperty ProcessAuthentication(IHttpAuthenticationContext authenticationContext)
        {
            if (this.ShouldProcessAuthentication(authenticationContext))
            {
                SecurityMessageProperty retValue;
                try
                {
                    retValue = this.ProcessAuthentication(authenticationContext.LogonUserIdentity, GetAuthType(authenticationContext));
                }
#pragma warning suppress 56500 // covered by FXCop
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                        throw;

                    // Audit Authentication failure
                    if (AuditLevel.Failure == (this.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Failure))
                        WriteAuditEvent(AuditLevel.Failure, (authenticationContext.LogonUserIdentity != null) ? authenticationContext.LogonUserIdentity.Name : String.Empty, exception);

                    throw;
                }

                // Audit Authentication success
                if (AuditLevel.Success == (this.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Success))
                    WriteAuditEvent(AuditLevel.Success, (authenticationContext.LogonUserIdentity != null) ? authenticationContext.LogonUserIdentity.Name : String.Empty, null);

                return retValue;
            }
            else
            {
                return null;
            }
        }

        public virtual SecurityMessageProperty ProcessAuthentication(HttpListenerContext listenerContext)
        {
            if (this.ShouldProcessAuthentication(listenerContext))
            {
                return this.ProcessRequiredAuthentication(listenerContext);
            }
            else
            {
                return null;
            }
        }

        SecurityMessageProperty ProcessRequiredAuthentication(HttpListenerContext listenerContext)
        {
            SecurityMessageProperty retValue;
            HttpListenerBasicIdentity identity = null;
            WindowsIdentity wid = null;
            try
            {
                Fx.Assert(listenerContext.User != null, "HttpListener delivered authenticated request without an IPrincipal.");
                wid = listenerContext.User.Identity as WindowsIdentity;

                if (this.AuthenticationScheme.IsSet(AuthenticationSchemes.Basic)
                    && wid == null)
                {
                    identity = listenerContext.User.Identity as HttpListenerBasicIdentity;
                    Fx.Assert(identity != null, "HttpListener delivered Basic authenticated request with a non-Basic IIdentity.");
                    retValue = this.ProcessAuthentication(identity);
                }
                else
                {
                    Fx.Assert(wid != null, "HttpListener delivered non-Basic authenticated request with a non-Windows IIdentity.");
                    retValue = this.ProcessAuthentication(wid, GetAuthType(listenerContext));
                }
            }
#pragma warning suppress 56500 // covered by FXCop
            catch (Exception exception)
            {
                if (!Fx.IsFatal(exception))
                {
                    // Audit Authentication failure
                    if (AuditLevel.Failure == (this.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Failure))
                    {
                        WriteAuditEvent(AuditLevel.Failure, (identity != null) ? identity.Name : ((wid != null) ? wid.Name : String.Empty), exception);
                    }
                }
                throw;
            }

            // Audit Authentication success
            if (AuditLevel.Success == (this.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Success))
            {
                WriteAuditEvent(AuditLevel.Success, (identity != null) ? identity.Name : ((wid != null) ? wid.Name : String.Empty), null);
            }

            return retValue;
        }

        protected override bool TryGetTransportManagerRegistration(HostNameComparisonMode hostNameComparisonMode,
            out ITransportManagerRegistration registration)
        {
            if (this.TransportManagerTable.TryLookupUri(this.Uri, hostNameComparisonMode, out registration))
            {
                HttpTransportManager httpTransportManager = registration as HttpTransportManager;
                if (httpTransportManager != null && httpTransportManager.IsHosted)
                {
                    return true;
                }
                // Due to HTTP.SYS behavior, we don't reuse registrations from a higher point in the URI hierarchy.
                if (registration.ListenUri.Segments.Length >= this.BaseUri.Segments.Length)
                {
                    return true;
                }
            }
            return false;
        }

        protected void WriteAuditEvent(AuditLevel auditLevel, string primaryIdentity, Exception exception)
        {
            try
            {
                if (auditLevel == AuditLevel.Success)
                {
                    SecurityAuditHelper.WriteTransportAuthenticationSuccessEvent(this.AuditBehavior.AuditLogLocation,
                        this.AuditBehavior.SuppressAuditFailure, null, this.Uri, primaryIdentity);
                }
                else
                {
                    SecurityAuditHelper.WriteTransportAuthenticationFailureEvent(this.AuditBehavior.AuditLogLocation,
                        this.AuditBehavior.SuppressAuditFailure, null, this.Uri, primaryIdentity, exception);
                }
            }
#pragma warning suppress 56500
            catch (Exception auditException)
            {
                if (Fx.IsFatal(auditException) || auditLevel == AuditLevel.Success)
                    throw;

                DiagnosticUtility.TraceHandledException(auditException, TraceEventType.Error);
            }
        }

        SecurityMessageProperty ProcessAuthentication(HttpListenerBasicIdentity identity)
        {
            SecurityToken securityToken = new UserNameSecurityToken(identity.Name, identity.Password);
            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = this.userNameTokenAuthenticator.ValidateToken(securityToken);
            SecurityMessageProperty security = new SecurityMessageProperty();
            security.TransportToken = new SecurityTokenSpecification(securityToken, authorizationPolicies);
            security.ServiceSecurityContext = new ServiceSecurityContext(authorizationPolicies);
            return security;
        }

        SecurityMessageProperty ProcessAuthentication(WindowsIdentity identity, string authenticationType)
        {
            SecurityUtils.ValidateAnonymityConstraint(identity, false);
            SecurityToken securityToken = new WindowsSecurityToken(identity, SecurityUniqueId.Create().Value, authenticationType);
            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = this.windowsTokenAuthenticator.ValidateToken(securityToken);
            SecurityMessageProperty security = new SecurityMessageProperty();
            security.TransportToken = new SecurityTokenSpecification(securityToken, authorizationPolicies);
            security.ServiceSecurityContext = new ServiceSecurityContext(authorizationPolicies);
            return security;
        }

        HttpStatusCode ValidateAuthentication(string authType)
        {
            if (this.IsAuthSchemeValid(authType))
            {
                return HttpStatusCode.OK;
            }
            else
            {
                // Audit Authentication failure
                if (AuditLevel.Failure == (this.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Failure))
                {
                    string message = SR.GetString(SR.HttpAuthenticationFailed, this.AuthenticationScheme, HttpStatusCode.Unauthorized);
                    Exception exception = DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(message));
                    WriteAuditEvent(AuditLevel.Failure, String.Empty, exception);
                }

                return HttpStatusCode.Unauthorized;
            }
        }

        public virtual HttpStatusCode ValidateAuthentication(IHttpAuthenticationContext authenticationContext)
        {
            HttpStatusCode result = HttpStatusCode.OK;

            if (this.IsAuthenticationSupported)
            {
                string authType = GetAuthType(authenticationContext);
                result = ValidateAuthentication(authType);
            }

            if (result == HttpStatusCode.OK &&
                authenticationContext.LogonUserIdentity != null &&
                authenticationContext.LogonUserIdentity.IsAuthenticated &&
                this.ExtendedProtectionPolicy.PolicyEnforcement == PolicyEnforcement.Always &&
                !authenticationContext.IISSupportsExtendedProtection)
            {
                Exception exception = DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new PlatformNotSupportedException(SR.GetString(SR.ExtendedProtectionNotSupported)));
                WriteAuditEvent(AuditLevel.Failure, String.Empty, exception);

                result = HttpStatusCode.Unauthorized;
            }

            return result;
        }

        public virtual HttpStatusCode ValidateAuthentication(HttpListenerContext listenerContext)
        {
            HttpStatusCode result = HttpStatusCode.OK;

            if (this.IsAuthenticationSupported)
            {
                string authType = GetAuthType(listenerContext);
                result = ValidateAuthentication(authType);
            }

            return result;
        }

        static ExtendedProtectionPolicy GetPolicyWithDefaultSpnCollection(ExtendedProtectionPolicy policy, AuthenticationSchemes authenticationScheme, HostNameComparisonMode hostNameComparisonMode, Uri listenUri, out bool usingDefaultSpnList)
        {
            if (policy.PolicyEnforcement != PolicyEnforcement.Never &&
                policy.CustomServiceNames == null && //null indicates "use default"
                policy.CustomChannelBinding == null && //not needed if a channel binding is provided.
                authenticationScheme != AuthenticationSchemes.Anonymous && //SPN list only needed with authentication (mixed mode uses own default list)
                string.Equals(listenUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))//SPN list not used for HTTPS (CBT is used instead).
            {
                usingDefaultSpnList = true;
                return new ExtendedProtectionPolicy(policy.PolicyEnforcement, policy.ProtectionScenario, GetDefaultSpnList(hostNameComparisonMode, listenUri));
            }

            usingDefaultSpnList = false;
            return policy;
        }

        static ServiceNameCollection GetDefaultSpnList(HostNameComparisonMode hostNameComparisonMode, Uri listenUri)
        {
            //In 3.5 SP1, we started sending the HOST/xyz format, so we have to accept it for compat reasons.
            //with this change, we will be changing our client so that it lets System.Net pick the SPN by default
            //which will usually mean they use the HTTP/xyz format, which is more likely to interop with
            //other web service stacks that support windows auth...
            const string hostSpnFormat = "HOST/{0}";
            const string httpSpnFormat = "HTTP/{0}";
            const string localhost = "localhost";

            Dictionary<string, string> serviceNames = new Dictionary<string, string>();

            string hostName = null;
            string dnsSafeHostName = listenUri.DnsSafeHost;

            switch (hostNameComparisonMode)
            {
                case HostNameComparisonMode.Exact:
                    UriHostNameType hostNameType = listenUri.HostNameType;
                    if (hostNameType == UriHostNameType.IPv4 || hostNameType == UriHostNameType.IPv6)
                    {
                        hostName = Dns.GetHostEntry(string.Empty).HostName;
                        AddSpn(serviceNames, string.Format(CultureInfo.InvariantCulture, hostSpnFormat, hostName));
                        AddSpn(serviceNames, string.Format(CultureInfo.InvariantCulture, httpSpnFormat, hostName));
                    }
                    else
                    {
                        if (listenUri.DnsSafeHost.Contains("."))
                        {
                            //since we are listening explicitly on the FQDN, we should add only the FQDN SPN
                            AddSpn(serviceNames, string.Format(CultureInfo.InvariantCulture, hostSpnFormat, dnsSafeHostName));
                            AddSpn(serviceNames, string.Format(CultureInfo.InvariantCulture, httpSpnFormat, dnsSafeHostName));
                        }
                        else
                        {
                            hostName = Dns.GetHostEntry(string.Empty).HostName;
                            //add the short name (from the URI) and the FQDN (from Dns)
                            AddSpn(serviceNames, string.Format(CultureInfo.InvariantCulture, hostSpnFormat, dnsSafeHostName));
                            AddSpn(serviceNames, string.Format(CultureInfo.InvariantCulture, httpSpnFormat, dnsSafeHostName));
                            AddSpn(serviceNames, string.Format(CultureInfo.InvariantCulture, hostSpnFormat, hostName));
                            AddSpn(serviceNames, string.Format(CultureInfo.InvariantCulture, httpSpnFormat, hostName));
                        }
                    }
                    break;
                case HostNameComparisonMode.StrongWildcard:
                case HostNameComparisonMode.WeakWildcard:
                    hostName = Dns.GetHostEntry(string.Empty).HostName;
                    AddSpn(serviceNames, string.Format(CultureInfo.InvariantCulture, hostSpnFormat, hostName));
                    AddSpn(serviceNames, string.Format(CultureInfo.InvariantCulture, httpSpnFormat, hostName));
                    break;
                default:
                    Fx.Assert("Unhandled HostNameComparisonMode: " + hostNameComparisonMode);
                    break;
            }

            AddSpn(serviceNames, string.Format(CultureInfo.InvariantCulture, hostSpnFormat, localhost));
            AddSpn(serviceNames, string.Format(CultureInfo.InvariantCulture, httpSpnFormat, localhost));

            return new ServiceNameCollection(serviceNames.Values);
        }

        static void AddSpn(Dictionary<string, string> list, string value)
        {
            string key = value.ToLowerInvariant();

            if (!list.ContainsKey(key))
            {
                list.Add(key, value);
            }
        }

        public abstract bool CreateWebSocketChannelAndEnqueue(HttpRequestContext httpRequestContext, HttpPipeline httpPipeline, HttpResponseMessage httpResponseMessage, string subProtocol, Action dequeuedCallback);

        public abstract byte[] TakeWebSocketInternalBuffer();
        public abstract void ReturnWebSocketInternalBuffer(byte[] buffer);

        internal interface IHttpAuthenticationContext
        {
            WindowsIdentity LogonUserIdentity { get; }
            X509Certificate2 GetClientCertificate(out bool isValidCertificate);
            bool IISSupportsExtendedProtection { get; }
            TraceRecord CreateTraceRecord();
        }
    }

    class HttpChannelListener<TChannel> : HttpChannelListener,
        IChannelListener<TChannel> where TChannel : class, IChannel
    {
        InputQueueChannelAcceptor<TChannel> acceptor;
        bool useWebSocketTransport;
        CommunicationObjectManager<ServerWebSocketTransportDuplexSessionChannel> webSocketLifetimeManager;
        TransportIntegrationHandler transportIntegrationHandler;
        ConnectionBufferPool bufferPool;
        string currentWebSocketVersion;

        public HttpChannelListener(HttpTransportBindingElement bindingElement, BindingContext context)
            : base(bindingElement, context)
        {
            this.useWebSocketTransport = bindingElement.WebSocketSettings.TransportUsage == WebSocketTransportUsage.Always
                || (bindingElement.WebSocketSettings.TransportUsage == WebSocketTransportUsage.WhenDuplex && typeof(TChannel) != typeof(IReplyChannel));
            if (this.useWebSocketTransport)
            {
                if (AspNetEnvironment.Enabled)
                {
                    AspNetEnvironment env = AspNetEnvironment.Current;

                    // When IIS hosted, WebSockets can be used if the pipeline mode is integrated and the WebSocketModule is loaded.
                    // Otherwise, the client requests will not be upgraded to web sockets (see the code in HostedHttpTransportManager.HttpContextReceived(..)).
                    // We do the checks below (and fail the service activation), to avoid starting a WebSockets listener that won't get called.
                    if (!env.UsingIntegratedPipeline)
                    {
                        throw FxTrace.Exception.AsError(new NotSupportedException(SR.GetString(SR.WebSocketsNotSupportedInClassicPipeline)));
                    }
                    else if (!env.IsWebSocketModuleLoaded)
                    {
                        throw FxTrace.Exception.AsError(new NotSupportedException(SR.GetString(SR.WebSocketModuleNotLoaded)));
                    }
                }
                else if (!WebSocketHelper.OSSupportsWebSockets())
                {
                    throw FxTrace.Exception.AsError(new PlatformNotSupportedException(SR.GetString(SR.WebSocketsServerSideNotSupported)));
                }

                this.currentWebSocketVersion = WebSocketHelper.GetCurrentVersion();
                this.acceptor = new InputQueueChannelAcceptor<TChannel>(this);
                int webSocketBufferSize = WebSocketHelper.ComputeServerBufferSize(bindingElement.MaxReceivedMessageSize);
                this.bufferPool = new ConnectionBufferPool(webSocketBufferSize);
                this.webSocketLifetimeManager = new CommunicationObjectManager<ServerWebSocketTransportDuplexSessionChannel>(this.ThisLock);
            }
            else
            {
                this.acceptor = (InputQueueChannelAcceptor<TChannel>)(object)(new TransportReplyChannelAcceptor(this));
            }

            this.CreatePipeline(bindingElement.MessageHandlerFactory);
        }

        public override bool UseWebSocketTransport
        {
            get
            {
                return this.useWebSocketTransport;
            }
        }

        public InputQueueChannelAcceptor<TChannel> Acceptor
        {
            get { return this.acceptor; }
        }

        public override string Method
        {
            get
            {
                if (this.UseWebSocketTransport)
                {
                    return WebSocketTransportSettings.WebSocketMethod;
                }
                return base.Method;
            }
        }

        public TChannel AcceptChannel()
        {
            return this.AcceptChannel(this.DefaultReceiveTimeout);
        }

        public IAsyncResult BeginAcceptChannel(AsyncCallback callback, object state)
        {
            return this.BeginAcceptChannel(this.DefaultReceiveTimeout, callback, state);
        }

        public TChannel AcceptChannel(TimeSpan timeout)
        {
            base.ThrowIfNotOpened();
            return this.Acceptor.AcceptChannel(timeout);
        }

        public IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            base.ThrowIfNotOpened();
            return this.Acceptor.BeginAcceptChannel(timeout, callback, state);
        }

        public TChannel EndAcceptChannel(IAsyncResult result)
        {
            base.ThrowPending();
            return this.Acceptor.EndAcceptChannel(result);
        }

        public override bool CreateWebSocketChannelAndEnqueue(HttpRequestContext httpRequestContext, HttpPipeline pipeline, HttpResponseMessage httpResponseMessage, string subProtocol, Action dequeuedCallback)
        {
            Fx.Assert(this.WebSocketSettings.MaxPendingConnections > 0, "MaxPendingConnections should be positive.");
            if (this.Acceptor.PendingCount >= this.WebSocketSettings.MaxPendingConnections)
            {
                if (TD.MaxPendingConnectionsExceededIsEnabled())
                {
                    TD.MaxPendingConnectionsExceeded(SR.GetString(SR.WebSocketMaxPendingConnectionsReached, this.WebSocketSettings.MaxPendingConnections, WebSocketHelper.MaxPendingConnectionsString, WebSocketHelper.WebSocketTransportSettingsString));
                }

                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning,
                        TraceCode.MaxPendingConnectionsReached, SR.GetString(SR.WebSocketMaxPendingConnectionsReached, this.WebSocketSettings.MaxPendingConnections, WebSocketHelper.MaxPendingConnectionsString, WebSocketHelper.WebSocketTransportSettingsString),
                        new StringTraceRecord(WebSocketHelper.MaxPendingConnectionsString, this.WebSocketSettings.MaxPendingConnections.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                        this,
                        null);
                }

                return false;
            }

            ServerWebSocketTransportDuplexSessionChannel channel = new ServerWebSocketTransportDuplexSessionChannel(this,
                                    new EndpointAddress(this.Uri), this.Uri, this.bufferPool, httpRequestContext, pipeline, httpResponseMessage, subProtocol);
            httpRequestContext.WebSocketChannel = channel;

            // webSocketLifetimeManager hooks into the channel.Closed event as well and will take care of cleaning itself up OnClosed. 
            // We want to be called before any user-specified close handlers are called. 
            this.webSocketLifetimeManager.Add(channel);
            this.Acceptor.EnqueueAndDispatch((TChannel)(object)channel, dequeuedCallback, true);
            return true;
        }

        public override byte[] TakeWebSocketInternalBuffer()
        {
            Fx.Assert(this.bufferPool != null, "bufferPool should not be null.");
            return this.bufferPool.Take();
        }

        public override void ReturnWebSocketInternalBuffer(byte[] buffer)
        {
            Fx.Assert(this.bufferPool != null, "bufferPool should not be null.");
            this.bufferPool.Return(buffer);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedOpenAsyncResult(timeout, callback, state, base.OnBeginOpen, base.OnEndOpen, this.Acceptor);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnOpen(timeoutHelper.RemainingTime());
            this.Acceptor.Open(timeoutHelper.RemainingTime());
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ChainedOpenAsyncResult.End(result);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            this.Acceptor.Close(timeoutHelper.RemainingTime());
            if (this.IsAuthenticationSupported)
            {
                CloseUserNameTokenAuthenticator(timeoutHelper.RemainingTime());
            }
            if (this.useWebSocketTransport)
            {
                this.webSocketLifetimeManager.Close(timeoutHelper.RemainingTime());
            }
            base.OnClose(timeoutHelper.RemainingTime());
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            ICommunicationObject[] communicationObjects;
            ICommunicationObject communicationObject = this.UserNameTokenAuthenticator as ICommunicationObject;
            if (communicationObject == null)
            {
                if (this.IsAuthenticationSupported)
                {
                    CloseUserNameTokenAuthenticator(timeoutHelper.RemainingTime());
                }
                communicationObjects = new ICommunicationObject[] { this.Acceptor };
            }
            else
            {
                communicationObjects = new ICommunicationObject[] { this.Acceptor, communicationObject };
            }

            if (this.useWebSocketTransport)
            {
                return new LifetimeWrappedCloseAsyncResult<ServerWebSocketTransportDuplexSessionChannel>(
                    timeoutHelper.RemainingTime(),
                    callback,
                    state,
                    this.webSocketLifetimeManager,
                    base.OnBeginClose,
                    base.OnEndClose,
                    communicationObjects);
            }
            else
            {
                return new ChainedCloseAsyncResult(timeoutHelper.RemainingTime(), callback, state, base.OnBeginClose, base.OnEndClose, communicationObjects);
            }
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            if (this.useWebSocketTransport)
            {
                LifetimeWrappedCloseAsyncResult<ServerWebSocketTransportDuplexSessionChannel>.End(result);
            }
            else
            {
                ChainedCloseAsyncResult.End(result);
            }
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            if (this.bufferPool != null)
            {
                this.bufferPool.Close();
            }

            if (this.transportIntegrationHandler != null)
            {
                this.transportIntegrationHandler.Dispose();
            }
        }

        protected override void OnAbort()
        {
            if (this.IsAuthenticationSupported)
            {
                AbortUserNameTokenAuthenticator();
            }

            this.Acceptor.Abort();

            if (this.useWebSocketTransport)
            {
                this.webSocketLifetimeManager.Abort();
            }

            base.OnAbort();
        }

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            return Acceptor.WaitForChannel(timeout);
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return Acceptor.BeginWaitForChannel(timeout, callback, state);
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            return Acceptor.EndWaitForChannel(result);
        }

        internal override IAsyncResult BeginHttpContextReceived(HttpRequestContext context,
                                                        Action acceptorCallback,
                                                        AsyncCallback callback,
                                                        object state)
        {
            return new HttpContextReceivedAsyncResult<TChannel>(
                context,
                acceptorCallback,
                this,
                callback,
                state);
        }

        internal override bool EndHttpContextReceived(IAsyncResult result)
        {
            return HttpContextReceivedAsyncResult<TChannel>.End(result);
        }

        void CreatePipeline(HttpMessageHandlerFactory httpMessageHandlerFactory)
        {
            HttpMessageHandler innerPipeline;
            if (this.UseWebSocketTransport)
            {
                innerPipeline = new DefaultWebSocketConnectionHandler(this.WebSocketSettings.SubProtocol, this.currentWebSocketVersion, this.MessageVersion, this.MessageEncoderFactory, this.TransferMode);
                if (httpMessageHandlerFactory != null)
                {
                    innerPipeline = httpMessageHandlerFactory.Create(innerPipeline);
                }
            }
            else
            {
                if (httpMessageHandlerFactory == null)
                {
                    return;
                }

                innerPipeline = httpMessageHandlerFactory.Create(new ChannelModelIntegrationHandler());
            }

            if (innerPipeline == null)
            {
                throw FxTrace.Exception.AsError(
                    new InvalidOperationException(SR.GetString(SR.HttpMessageHandlerChannelFactoryNullPipeline,
                        httpMessageHandlerFactory.GetType().Name, typeof(HttpRequestContext).Name)));
            }

            this.transportIntegrationHandler = new TransportIntegrationHandler(innerPipeline);
        }

        static void HandleProcessInboundException(Exception ex, HttpRequestContext context)
        {
            if (Fx.IsFatal(ex))
            {
                return;
            }

            if (ex is ProtocolException)
            {
                ProtocolException protocolException = (ProtocolException)ex;
                HttpStatusCode statusCode = HttpStatusCode.BadRequest;
                string statusDescription = string.Empty;
                if (protocolException.Data.Contains(HttpChannelUtilities.HttpStatusCodeExceptionKey))
                {
                    statusCode = (HttpStatusCode)protocolException.Data[HttpChannelUtilities.HttpStatusCodeExceptionKey];
                    protocolException.Data.Remove(HttpChannelUtilities.HttpStatusCodeExceptionKey);
                }
                if (protocolException.Data.Contains(HttpChannelUtilities.HttpStatusDescriptionExceptionKey))
                {
                    statusDescription = (string)protocolException.Data[HttpChannelUtilities.HttpStatusDescriptionExceptionKey];
                    protocolException.Data.Remove(HttpChannelUtilities.HttpStatusDescriptionExceptionKey);
                }
                context.SendResponseAndClose(statusCode, statusDescription);

            }
            else
            {
                try
                {
                    context.SendResponseAndClose(HttpStatusCode.BadRequest);
                }
                catch (Exception closeException)
                {
                    if (Fx.IsFatal(closeException))
                    {
                        throw;
                    }

                    DiagnosticUtility.TraceHandledException(closeException, TraceEventType.Error);
                }
            }
        }

        static bool ContextReceiveExceptionHandled(Exception e)
        {
            if (Fx.IsFatal(e))
            {
                return false;
            }

            if (e is CommunicationException)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            else if (e is XmlException)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            else if (e is IOException)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            else if (e is TimeoutException)
            {
                if (TD.ReceiveTimeoutIsEnabled())
                {
                    TD.ReceiveTimeout(e.Message);
                }
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            else if (e is OperationCanceledException)
            {
                DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
            }
            else if (!ExceptionHandler.HandleTransportExceptionHelper(e))
            {
                return false;
            }

            return true;
        }

        class HttpContextReceivedAsyncResult<TListenerChannel> : TraceAsyncResult where TListenerChannel : class, IChannel
        {
            static AsyncCallback onProcessInboundRequest = Fx.ThunkCallback(OnProcessInboundRequest);
            bool enqueued;
            HttpRequestContext context;
            Action acceptorCallback;
            HttpChannelListener<TListenerChannel> listener;

            public HttpContextReceivedAsyncResult(
                                                HttpRequestContext requestContext,
                                                Action acceptorCallback,
                                                HttpChannelListener<TListenerChannel> listener,
                                                AsyncCallback callback,
                                                object state)
                : base(callback, state)
            {
                this.context = requestContext;
                this.acceptorCallback = acceptorCallback;
                this.listener = listener;

                if (this.ProcessHttpContextAsync() == AsyncCompletionResult.Completed)
                {
                    base.Complete(true);
                }
            }

            public static bool End(IAsyncResult result)
            {
                return AsyncResult.End<HttpContextReceivedAsyncResult<TListenerChannel>>(result).enqueued;
            }

            static void OnProcessInboundRequest(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                HttpContextReceivedAsyncResult<TListenerChannel> thisPtr = (HttpContextReceivedAsyncResult<TListenerChannel>)result.AsyncState;
                Exception completionException = null;

                try
                {
                    thisPtr.HandleProcessInboundRequest(result);
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }

                    completionException = ex;
                }

                thisPtr.Complete(false, completionException);
            }

            AsyncCompletionResult ProcessHttpContextAsync()
            {
                bool abort = false;
                try
                {
                    this.context.InitializeHttpPipeline(this.listener.transportIntegrationHandler);
                    if (!this.Authenticate())
                    {
                        return AsyncCompletionResult.Completed;
                    }

                    if (listener.UseWebSocketTransport && !context.IsWebSocketRequest)
                    {
                        this.context.SendResponseAndClose(HttpStatusCode.BadRequest, SR.GetString(SR.WebSocketEndpointOnlySupportWebSocketError));
                        return AsyncCompletionResult.Completed;
                    }

                    if (!listener.UseWebSocketTransport && context.IsWebSocketRequest)
                    {
                        this.context.SendResponseAndClose(HttpStatusCode.BadRequest, SR.GetString(SR.WebSocketEndpointDoesNotSupportWebSocketError));
                        return AsyncCompletionResult.Completed;
                    }

                    try
                    {
                        IAsyncResult result = context.BeginProcessInboundRequest(listener.Acceptor as ReplyChannelAcceptor,
                                                                                            this.acceptorCallback,
                                                                                            onProcessInboundRequest,
                                                                                            this);
                        if (result.CompletedSynchronously)
                        {
                            this.EndInboundProcessAndEnqueue(result);
                            return AsyncCompletionResult.Completed;
                        }
                    }
                    catch (Exception ex)
                    {
                        HandleProcessInboundException(ex, this.context);
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    // containment -- we abort the context in all error cases, no additional containment action needed                
                    abort = true;
                    if (!ContextReceiveExceptionHandled(ex))
                    {
                        throw;
                    }
                }
                finally
                {
                    if (abort)
                    {
                        context.Abort();
                    }
                }

                return abort ? AsyncCompletionResult.Completed : AsyncCompletionResult.Queued;
            }

            bool Authenticate()
            {
                if (!this.context.ProcessAuthentication())
                {
                    if (TD.HttpAuthFailedIsEnabled())
                    {
                        TD.HttpAuthFailed(context.EventTraceActivity);
                    }

                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.HttpAuthFailed, SR.GetString(SR.TraceCodeHttpAuthFailed), this);
                    }

                    return false;
                }

                return true;
            }

            void HandleProcessInboundRequest(IAsyncResult result)
            {
                bool abort = true;
                try
                {
                    try
                    {
                        this.EndInboundProcessAndEnqueue(result);
                        abort = false;
                    }
                    catch (Exception ex)
                    {
                        HandleProcessInboundException(ex, this.context);
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    // containment -- we abort the context in all error cases, no additional containment action needed                                    
                    if (!ContextReceiveExceptionHandled(ex))
                    {
                        throw;
                    }
                }
                finally
                {
                    if (abort)
                    {
                        context.Abort();
                    }
                }
            }

            void EndInboundProcessAndEnqueue(IAsyncResult result)
            {
                Fx.Assert(result != null, "Trying to complete without issuing a BeginProcessInboundRequest.");
                context.EndProcessInboundRequest(result);

                //We have finally managed to enqueue the message.
                this.enqueued = true;
            }
        }

        class LifetimeWrappedCloseAsyncResult<TCommunicationObject> : AsyncResult where TCommunicationObject : CommunicationObject
        {
            static AsyncCompletion handleLifetimeManagerClose = new AsyncCompletion(HandleLifetimeManagerClose);
            static AsyncCompletion handleChannelClose = new AsyncCompletion(HandleChannelClose);

            TimeoutHelper timeoutHelper;

            ICommunicationObject[] communicationObjects;
            CommunicationObjectManager<TCommunicationObject> communicationObjectManager;
            ChainedBeginHandler begin1;
            ChainedEndHandler end1;

            public LifetimeWrappedCloseAsyncResult(TimeSpan timeout, AsyncCallback callback, object state, CommunicationObjectManager<TCommunicationObject> communicationObjectManager, ChainedBeginHandler begin1, ChainedEndHandler end1, ICommunicationObject[] communicationObjects)
                : base(callback, state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.begin1 = begin1;
                this.end1 = end1;
                this.communicationObjects = communicationObjects;
                this.communicationObjectManager = communicationObjectManager;

                IAsyncResult result = communicationObjectManager.BeginClose(
                    this.timeoutHelper.RemainingTime(),
                    PrepareAsyncCompletion(handleLifetimeManagerClose),
                    this);

                bool completeSelf = SyncContinue(result);

                if (completeSelf)
                {
                    this.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<LifetimeWrappedCloseAsyncResult<TCommunicationObject>>(result);
            }

            static bool HandleLifetimeManagerClose(IAsyncResult result)
            {
                LifetimeWrappedCloseAsyncResult<TCommunicationObject> thisPtr = (LifetimeWrappedCloseAsyncResult<TCommunicationObject>)result.AsyncState;
                thisPtr.communicationObjectManager.EndClose(result);

                // begin second step of the close... 
                ChainedCloseAsyncResult closeResult = new ChainedCloseAsyncResult(
                    thisPtr.timeoutHelper.RemainingTime(),
                    thisPtr.PrepareAsyncCompletion(handleChannelClose),
                    thisPtr,
                    thisPtr.begin1,
                    thisPtr.end1,
                    thisPtr.communicationObjects);

                return thisPtr.SyncContinue(closeResult);
            }

            static bool HandleChannelClose(IAsyncResult result)
            {
                ChainedCloseAsyncResult.End(result);
                return true;
            }
        }
    }

    /// <summary>
    /// Handler wrapping the bottom (towards network) of the <see cref="HttpMessageHandler"/> and integrates
    /// back into the <see cref="IReplyChannel"/>.
    /// </summary>
    class TransportIntegrationHandler : DelegatingHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransportIntegrationHandler"/> class.
        /// </summary>
        /// <param name="innerChannel">The inner <see cref="HttpMessageHandler"/> on which we send the <see cref="HttpRequestMessage"/>.</param>
        public TransportIntegrationHandler(HttpMessageHandler innerChannel)
            : base(innerChannel)
        {
        }

        /// <summary>
        /// Submits an <see cref="HttpRequestMessage"/> on the inner channel asynchronously.
        /// </summary>
        /// <param name="request"><see cref="HttpRequestMessage"/> to submit</param>
        /// <param name="cancellationToken">Token used to cancel operation.</param>
        /// <returns>A <see cref="Task&lt;T&gt;"/> representing the operation.</returns>
        public Task<HttpResponseMessage> ProcessPipelineAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken).ContinueWith(task =>
                {
                    HttpResponseMessage httpResponse;
                    if (task.IsFaulted) 
                    {
                        if (Fx.IsFatal(task.Exception))
                        {
                            throw task.Exception;
                        }

                        // We must inspect task.Exception -- otherwise it is automatically rethrown.
                        FxTrace.Exception.AsError<FaultException>(task.Exception);
                        httpResponse = TraceFaultAndGetResponseMessasge(request);
                    }
                    else if (task.IsCanceled)
                    {
                        HttpPipeline pipeline = HttpPipeline.GetHttpPipeline(request);
                        if (TD.HttpPipelineTimeoutExceptionIsEnabled())
                        {
                            TD.HttpPipelineTimeoutException(pipeline != null ? pipeline.EventTraceActivity : null);
                        }

                        FxTrace.Exception.AsError(new TimeoutException(SR.GetString(SR.HttpPipelineOperationCanceledError)));
                        pipeline.Cancel();
                        httpResponse = null;
                    }
                    else
                    {
                        httpResponse = task.Result;
                        if (httpResponse == null)
                        {
                            FxTrace.Exception.AsError(new NotSupportedException(SR.GetString(SR.HttpPipelineNotSupportNullResponseMessage, typeof(DelegatingHandler).Name, typeof(HttpResponseMessage).Name)));
                            httpResponse = TraceFaultAndGetResponseMessasge(request);
                        }
                    }

                    return httpResponse;
                },
                TaskContinuationOptions.ExecuteSynchronously);
        }

        static HttpResponseMessage TraceFaultAndGetResponseMessasge(HttpRequestMessage request)
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            response.RequestMessage = request;

            if (TD.HttpPipelineFaultedIsEnabled())
            {
                HttpPipeline pipeline = HttpPipeline.GetHttpPipeline(request);
                TD.HttpPipelineFaulted(pipeline != null ? pipeline.EventTraceActivity : null);
            }

            return response;
        }
    }

    /// <summary>
    /// Handler wrapping the top (towards Channel Model) of the <see cref="HttpMessageHandler"/> and integrates
    /// bask into the <see cref="IReplyChannel"/>.
    /// </summary>
    class ChannelModelIntegrationHandler : HttpMessageHandler
    {
        /// <summary>
        /// Submits an <see cref="HttpRequestMessage"/> on the inner channel asynchronously.
        /// </summary>
        /// <param name="request"><see cref="HttpRequestMessage"/> to submit</param>
        /// <param name="cancellationToken">Token used to cancel operation.</param>
        /// <returns>A <see cref="Task&lt;T&gt;"/> representing the operation.</returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw FxTrace.Exception.ArgumentNull("request");
            }

            if (cancellationToken == null)
            {
                throw FxTrace.Exception.ArgumentNull("cancellationToken");
            }

            cancellationToken.ThrowIfCancellationRequested();

            HttpChannelUtilities.EnsureHttpRequestMessageContentNotNull(request);
            //// We ran up through the pipeline and are now ready to hook back into the WCF channel model
            HttpPipeline httpPipeline = HttpPipeline.GetHttpPipeline(request);

            return httpPipeline.Dispatch(request);
        }
    }
}
