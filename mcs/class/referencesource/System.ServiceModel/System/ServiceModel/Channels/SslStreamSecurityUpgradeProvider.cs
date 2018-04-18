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
    using System.Net.Security;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security.Authentication;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;

    class SslStreamSecurityUpgradeProvider : StreamSecurityUpgradeProvider, IStreamUpgradeChannelBindingProvider
    {
        SecurityTokenAuthenticator clientCertificateAuthenticator;
        SecurityTokenManager clientSecurityTokenManager;
        SecurityTokenProvider serverTokenProvider;
        EndpointIdentity identity;
        IdentityVerifier identityVerifier;
        X509Certificate2 serverCertificate;
        bool requireClientCertificate;
        string scheme;
        bool enableChannelBinding;
        SslProtocols sslProtocols;

        SslStreamSecurityUpgradeProvider(IDefaultCommunicationTimeouts timeouts, SecurityTokenManager clientSecurityTokenManager, bool requireClientCertificate, string scheme, IdentityVerifier identityVerifier, SslProtocols sslProtocols)
            : base(timeouts)
        {
            this.identityVerifier = identityVerifier;
            this.scheme = scheme;
            this.clientSecurityTokenManager = clientSecurityTokenManager;
            this.requireClientCertificate = requireClientCertificate;
            this.sslProtocols = sslProtocols;
        }

        SslStreamSecurityUpgradeProvider(IDefaultCommunicationTimeouts timeouts, SecurityTokenProvider serverTokenProvider, bool requireClientCertificate, SecurityTokenAuthenticator clientCertificateAuthenticator, string scheme, IdentityVerifier identityVerifier, SslProtocols sslProtocols)
            : base(timeouts)
        {
            this.serverTokenProvider = serverTokenProvider;
            this.requireClientCertificate = requireClientCertificate;
            this.clientCertificateAuthenticator = clientCertificateAuthenticator;
            this.identityVerifier = identityVerifier;
            this.scheme = scheme;
            this.sslProtocols = sslProtocols;
        }

        public static SslStreamSecurityUpgradeProvider CreateClientProvider(
            SslStreamSecurityBindingElement bindingElement, BindingContext context)
        {
            SecurityCredentialsManager credentialProvider = context.BindingParameters.Find<SecurityCredentialsManager>();

            if (credentialProvider == null)
            {
                credentialProvider = ClientCredentials.CreateDefaultCredentials();
            }
            SecurityTokenManager tokenManager = credentialProvider.CreateSecurityTokenManager();

            return new SslStreamSecurityUpgradeProvider(context.Binding, tokenManager, bindingElement.RequireClientCertificate, context.Binding.Scheme, bindingElement.IdentityVerifier, bindingElement.SslProtocols);
        }

        public static SslStreamSecurityUpgradeProvider CreateServerProvider(
            SslStreamSecurityBindingElement bindingElement, BindingContext context)
        {
            SecurityCredentialsManager credentialProvider =
                context.BindingParameters.Find<SecurityCredentialsManager>();

            if (credentialProvider == null)
            {
                credentialProvider = ServiceCredentials.CreateDefaultCredentials();
            }

            Uri listenUri = TransportSecurityHelpers.GetListenUri(context.ListenUriBaseAddress, context.ListenUriRelativeAddress);
            SecurityTokenManager tokenManager = credentialProvider.CreateSecurityTokenManager();

            RecipientServiceModelSecurityTokenRequirement serverCertRequirement = new RecipientServiceModelSecurityTokenRequirement();
            serverCertRequirement.TokenType = SecurityTokenTypes.X509Certificate;
            serverCertRequirement.RequireCryptographicToken = true;
            serverCertRequirement.KeyUsage = SecurityKeyUsage.Exchange;
            serverCertRequirement.TransportScheme = context.Binding.Scheme;
            serverCertRequirement.ListenUri = listenUri;

            SecurityTokenProvider tokenProvider = tokenManager.CreateSecurityTokenProvider(serverCertRequirement);
            if (tokenProvider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ClientCredentialsUnableToCreateLocalTokenProvider, serverCertRequirement)));
            }

            SecurityTokenAuthenticator certificateAuthenticator =
                TransportSecurityHelpers.GetCertificateTokenAuthenticator(tokenManager, context.Binding.Scheme, listenUri);

            return new SslStreamSecurityUpgradeProvider(context.Binding, tokenProvider, bindingElement.RequireClientCertificate,
                certificateAuthenticator, context.Binding.Scheme, bindingElement.IdentityVerifier, bindingElement.SslProtocols);
        }

        public override EndpointIdentity Identity
        {
            get
            {
                if ((this.identity == null) && (this.serverCertificate != null))
                {
                    this.identity = SecurityUtils.GetServiceCertificateIdentity(this.serverCertificate);
                }
                return this.identity;
            }
        }

        public IdentityVerifier IdentityVerifier
        {
            get
            {
                return this.identityVerifier;
            }
        }

        public bool RequireClientCertificate
        {
            get
            {
                return this.requireClientCertificate;
            }
        }

        public X509Certificate2 ServerCertificate
        {
            get
            {
                return this.serverCertificate;
            }
        }

        public SecurityTokenAuthenticator ClientCertificateAuthenticator
        {
            get
            {
                if (this.clientCertificateAuthenticator == null)
                {
                    this.clientCertificateAuthenticator = new X509SecurityTokenAuthenticator(X509ClientCertificateAuthentication.DefaultCertificateValidator);
                }

                return this.clientCertificateAuthenticator;
            }
        }

        public SecurityTokenManager ClientSecurityTokenManager
        {
            get
            {
                return this.clientSecurityTokenManager;
            }
        }

        public string Scheme
        {
            get { return this.scheme; }
        }

        public SslProtocols SslProtocols
        {
            get { return this.sslProtocols; }
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IChannelBindingProvider) || typeof(T) == typeof(IStreamUpgradeChannelBindingProvider))
            {
                return (T)(object)this;
            }
            return base.GetProperty<T>();
        }

        ChannelBinding IStreamUpgradeChannelBindingProvider.GetChannelBinding(StreamUpgradeInitiator upgradeInitiator, ChannelBindingKind kind)
        {
            if (upgradeInitiator == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("upgradeInitiator");
            }

            SslStreamSecurityUpgradeInitiator sslUpgradeInitiator = upgradeInitiator as SslStreamSecurityUpgradeInitiator;

            if (sslUpgradeInitiator == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("upgradeInitiator", SR.GetString(SR.UnsupportedUpgradeInitiator, upgradeInitiator.GetType()));
            }

            if (kind != ChannelBindingKind.Endpoint)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("kind", SR.GetString(SR.StreamUpgradeUnsupportedChannelBindingKind, this.GetType(), kind));
            }

            return sslUpgradeInitiator.ChannelBinding;
        }

        ChannelBinding IStreamUpgradeChannelBindingProvider.GetChannelBinding(StreamUpgradeAcceptor upgradeAcceptor, ChannelBindingKind kind)
        {
            if (upgradeAcceptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("upgradeAcceptor");
            }

            SslStreamSecurityUpgradeAcceptor sslupgradeAcceptor = upgradeAcceptor as SslStreamSecurityUpgradeAcceptor;

            if (sslupgradeAcceptor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("upgradeAcceptor", SR.GetString(SR.UnsupportedUpgradeAcceptor, upgradeAcceptor.GetType()));
            }

            if (kind != ChannelBindingKind.Endpoint)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("kind", SR.GetString(SR.StreamUpgradeUnsupportedChannelBindingKind, this.GetType(), kind));
            }

            return sslupgradeAcceptor.ChannelBinding;
        }

        void IChannelBindingProvider.EnableChannelBindingSupport()
        {
            this.enableChannelBinding = true;
        }


        bool IChannelBindingProvider.IsChannelBindingSupportEnabled
        {
            get
            {
                return this.enableChannelBinding;
            }
        }

        public override StreamUpgradeAcceptor CreateUpgradeAcceptor()
        {
            ThrowIfDisposedOrNotOpen();
            return new SslStreamSecurityUpgradeAcceptor(this);
        }

        public override StreamUpgradeInitiator CreateUpgradeInitiator(EndpointAddress remoteAddress, Uri via)
        {
            ThrowIfDisposedOrNotOpen();
            return new SslStreamSecurityUpgradeInitiator(this, remoteAddress, via);
        }

        protected override void OnAbort()
        {
            if (this.clientCertificateAuthenticator != null)
            {
                SecurityUtils.AbortTokenAuthenticatorIfRequired(this.clientCertificateAuthenticator);
            }
            CleanupServerCertificate();
        }

        protected override void OnClose(TimeSpan timeout)
        {
            if (this.clientCertificateAuthenticator != null)
            {
                SecurityUtils.CloseTokenAuthenticatorIfRequired(this.clientCertificateAuthenticator, timeout);
            }
            CleanupServerCertificate();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return SecurityUtils.BeginCloseTokenAuthenticatorIfRequired(this.clientCertificateAuthenticator, timeout, callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            SecurityUtils.EndCloseTokenAuthenticatorIfRequired(result);
            CleanupServerCertificate();
        }

        void SetupServerCertificate(SecurityToken token)
        {
            X509SecurityToken x509Token = token as X509SecurityToken;
            if (x509Token == null)
            {
                SecurityUtils.AbortTokenProviderIfRequired(this.serverTokenProvider);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                    SR.InvalidTokenProvided, this.serverTokenProvider.GetType(), typeof(X509SecurityToken))));
            }
            this.serverCertificate = new X509Certificate2(x509Token.Certificate);
        }

        void CleanupServerCertificate()
        {
            if (this.serverCertificate != null)
            {
                SecurityUtils.ResetCertificate(this.serverCertificate);
                this.serverCertificate = null;
            }
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            SecurityUtils.OpenTokenAuthenticatorIfRequired(this.ClientCertificateAuthenticator, timeoutHelper.RemainingTime());

            if (this.serverTokenProvider != null)
            {
                SecurityUtils.OpenTokenProviderIfRequired(this.serverTokenProvider, timeoutHelper.RemainingTime());
                SecurityToken token = this.serverTokenProvider.GetToken(timeout);
                SetupServerCertificate(token);
                SecurityUtils.CloseTokenProviderIfRequired(this.serverTokenProvider, timeoutHelper.RemainingTime());
                this.serverTokenProvider = null;
            }
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenAsyncResult(this, timeout, callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            OpenAsyncResult.End(result);
        }

        class OpenAsyncResult : AsyncResult
        {
            SslStreamSecurityUpgradeProvider parent;
            TimeoutHelper timeoutHelper;
            AsyncCallback onOpenTokenAuthenticator;
            AsyncCallback onOpenTokenProvider;
            AsyncCallback onGetToken;
            AsyncCallback onCloseTokenProvider;

            public OpenAsyncResult(SslStreamSecurityUpgradeProvider parent, TimeSpan timeout,
                AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.parent = parent;
                this.timeoutHelper = new TimeoutHelper(timeout);



                // since we're at channel.Open and not per-message, minimize our statics overhead and leverage GC for our callbacks
                this.onOpenTokenAuthenticator = Fx.ThunkCallback(new AsyncCallback(OnOpenTokenAuthenticator));
                IAsyncResult result = SecurityUtils.BeginOpenTokenAuthenticatorIfRequired(parent.ClientCertificateAuthenticator,
                    timeoutHelper.RemainingTime(), onOpenTokenAuthenticator, this);

                if (!result.CompletedSynchronously)
                {
                    return;
                }

                if (HandleOpenAuthenticatorComplete(result))
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<OpenAsyncResult>(result);
            }

            bool HandleOpenAuthenticatorComplete(IAsyncResult result)
            {
                SecurityUtils.EndOpenTokenAuthenticatorIfRequired(result);

                if (parent.serverTokenProvider == null)
                {
                    return true;
                }

                this.onOpenTokenProvider = Fx.ThunkCallback(new AsyncCallback(OnOpenTokenProvider));
                IAsyncResult openTokenProviderResult = SecurityUtils.BeginOpenTokenProviderIfRequired(
                    parent.serverTokenProvider, timeoutHelper.RemainingTime(), onOpenTokenProvider, this);

                if (!openTokenProviderResult.CompletedSynchronously)
                {
                    return false;
                }

                return HandleOpenTokenProviderComplete(openTokenProviderResult);
            }

            bool HandleOpenTokenProviderComplete(IAsyncResult result)
            {
                SecurityUtils.EndOpenTokenProviderIfRequired(result);
                this.onGetToken = Fx.ThunkCallback(new AsyncCallback(OnGetToken));

                IAsyncResult getTokenResult = parent.serverTokenProvider.BeginGetToken(timeoutHelper.RemainingTime(),
                    onGetToken, this);

                if (!getTokenResult.CompletedSynchronously)
                {
                    return false;
                }

                return HandleGetTokenComplete(getTokenResult);
            }

            bool HandleGetTokenComplete(IAsyncResult result)
            {
                SecurityToken token = parent.serverTokenProvider.EndGetToken(result);
                parent.SetupServerCertificate(token);
                this.onCloseTokenProvider = Fx.ThunkCallback(new AsyncCallback(OnCloseTokenProvider));
                IAsyncResult closeTokenProviderResult =
                    SecurityUtils.BeginCloseTokenProviderIfRequired(parent.serverTokenProvider, timeoutHelper.RemainingTime(),
                    onCloseTokenProvider, this);

                if (!closeTokenProviderResult.CompletedSynchronously)
                {
                    return false;
                }

                return HandleCloseTokenProviderComplete(closeTokenProviderResult);
            }

            bool HandleCloseTokenProviderComplete(IAsyncResult result)
            {
                SecurityUtils.EndCloseTokenProviderIfRequired(result);
                parent.serverTokenProvider = null;
                return true;
            }

            void OnOpenTokenAuthenticator(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                Exception completionException = null;
                bool completeSelf = false;
                try
                {
                    completeSelf = this.HandleOpenAuthenticatorComplete(result);
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
                    base.Complete(false, completionException);
                }
            }

            void OnOpenTokenProvider(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                Exception completionException = null;
                bool completeSelf = false;
                try
                {
                    completeSelf = this.HandleOpenTokenProviderComplete(result);
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
                    base.Complete(false, completionException);
                }
            }

            void OnGetToken(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                Exception completionException = null;
                bool completeSelf = false;
                try
                {
                    completeSelf = this.HandleGetTokenComplete(result);
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
                    base.Complete(false, completionException);
                }
            }

            void OnCloseTokenProvider(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                Exception completionException = null;
                bool completeSelf = false;
                try
                {
                    completeSelf = this.HandleCloseTokenProviderComplete(result);
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
                    base.Complete(false, completionException);
                }
            }

        }
    }

    class SslStreamSecurityUpgradeAcceptor : StreamSecurityUpgradeAcceptorBase
    {
        SslStreamSecurityUpgradeProvider parent;
        SecurityMessageProperty clientSecurity;
        // for audit
        X509Certificate2 clientCertificate = null;
        ChannelBinding channelBindingToken;

        public SslStreamSecurityUpgradeAcceptor(SslStreamSecurityUpgradeProvider parent)
            : base(FramingUpgradeString.SslOrTls)
        {
            this.parent = parent;
            this.clientSecurity = new SecurityMessageProperty();
        }

        internal ChannelBinding ChannelBinding
        {
            get
            {
                Fx.Assert(this.IsChannelBindingSupportEnabled, "A request for the ChannelBinding is not permitted without enabling ChannelBinding first (through the IChannelBindingProvider interface)");
                return this.channelBindingToken;
            }
        }

        internal bool IsChannelBindingSupportEnabled
        {
            get
            {
                return ((IChannelBindingProvider)parent).IsChannelBindingSupportEnabled;
            }
        }

        protected override Stream OnAcceptUpgrade(Stream stream, out SecurityMessageProperty remoteSecurity)
        {
            if (TD.SslOnAcceptUpgradeIsEnabled())
            {
                TD.SslOnAcceptUpgrade(this.EventTraceActivity);
            }

            SslStream sslStream = new SslStream(stream, false, this.ValidateRemoteCertificate);

            try
            {
                sslStream.AuthenticateAsServer(this.parent.ServerCertificate, this.parent.RequireClientCertificate,
                    this.parent.SslProtocols, false);
            }
            catch (AuthenticationException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(exception.Message,
                    exception));
            }
            catch (IOException ioException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(
                    SR.GetString(SR.NegotiationFailedIO, ioException.Message), ioException));
            }
            if (SecurityUtils.ShouldValidateSslCipherStrength())
            {
                SecurityUtils.ValidateSslCipherStrength(sslStream.CipherStrength);
            }

            remoteSecurity = this.clientSecurity;

            if (this.IsChannelBindingSupportEnabled)
            {
                this.channelBindingToken = ChannelBindingUtility.GetToken(sslStream);
            }

            return sslStream;
        }

        protected override IAsyncResult OnBeginAcceptUpgrade(Stream stream, AsyncCallback callback, object state)
        {
            AcceptUpgradeAsyncResult result = new AcceptUpgradeAsyncResult(this, callback, state);
            result.Begin(stream);
            return result;
        }

        protected override Stream OnEndAcceptUpgrade(IAsyncResult result, out SecurityMessageProperty remoteSecurity)
        {
            return AcceptUpgradeAsyncResult.End(result, out remoteSecurity, out this.channelBindingToken);
        }
        
        // callback from schannel
        bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (this.parent.RequireClientCertificate)
            {
                if (certificate == null)
                {
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Error, TraceCode.SslClientCertMissing,
                            SR.GetString(SR.TraceCodeSslClientCertMissing), this);
                    }
                    return false;
                }
                // Note: add ref to handle since the caller will reset the cert after the callback return.
                X509Certificate2 certificate2 = new X509Certificate2(certificate);
                this.clientCertificate = certificate2;
                try
                {
                    SecurityToken token = new X509SecurityToken(certificate2, false);
                    ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = this.parent.ClientCertificateAuthenticator.ValidateToken(token);
                    this.clientSecurity = new SecurityMessageProperty();
                    this.clientSecurity.TransportToken = new SecurityTokenSpecification(token, authorizationPolicies);
                    this.clientSecurity.ServiceSecurityContext = new ServiceSecurityContext(authorizationPolicies);
                }
                catch (SecurityTokenException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    return false;
                }
            }
            return true;
        }

        public override SecurityMessageProperty GetRemoteSecurity()
        {
            if (this.clientSecurity.TransportToken != null)
            {
                return this.clientSecurity;
            }
            if (this.clientCertificate != null)
            {
                SecurityToken token = new X509SecurityToken(this.clientCertificate);
                ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = SecurityUtils.NonValidatingX509Authenticator.ValidateToken(token);
                this.clientSecurity = new SecurityMessageProperty();
                this.clientSecurity.TransportToken = new SecurityTokenSpecification(token, authorizationPolicies);
                this.clientSecurity.ServiceSecurityContext = new ServiceSecurityContext(authorizationPolicies);
                return this.clientSecurity;
            }
            return base.GetRemoteSecurity();
        }

        class AcceptUpgradeAsyncResult : StreamSecurityUpgradeAcceptorAsyncResult
        {
            SslStreamSecurityUpgradeAcceptor acceptor;
            SslStream sslStream;
            ChannelBinding channelBindingToken;

            public AcceptUpgradeAsyncResult(SslStreamSecurityUpgradeAcceptor acceptor, AsyncCallback callback,
                object state)
                : base(callback, state)
            {
                this.acceptor = acceptor;
            }

            protected override IAsyncResult OnBegin(Stream stream, AsyncCallback callback)
            {
                if (TD.SslOnAcceptUpgradeIsEnabled())
                {
                    TD.SslOnAcceptUpgrade(acceptor.EventTraceActivity);
                }

                this.sslStream = new SslStream(stream, false, this.acceptor.ValidateRemoteCertificate);
                return this.sslStream.BeginAuthenticateAsServer(this.acceptor.parent.ServerCertificate,
                    this.acceptor.parent.RequireClientCertificate, this.acceptor.parent.SslProtocols, false, callback, this);
            }

            protected override Stream OnCompleteAuthenticateAsServer(IAsyncResult result)
            {
                this.sslStream.EndAuthenticateAsServer(result);

                if (SecurityUtils.ShouldValidateSslCipherStrength())
                {
                    SecurityUtils.ValidateSslCipherStrength(sslStream.CipherStrength);
                }

                if (this.acceptor.IsChannelBindingSupportEnabled)
                {
                    this.channelBindingToken = ChannelBindingUtility.GetToken(this.sslStream);
                }

                return this.sslStream;
            }

            protected override SecurityMessageProperty ValidateCreateSecurity()
            {
                return this.acceptor.clientSecurity;
            }

            public static Stream End(IAsyncResult result, out SecurityMessageProperty remoteSecurity, out ChannelBinding channelBinding)
            {
                Stream stream = StreamSecurityUpgradeAcceptorAsyncResult.End(result, out remoteSecurity);
                channelBinding = ((AcceptUpgradeAsyncResult)result).channelBindingToken;
                return stream;
            }
        }
    }

    class SslStreamSecurityUpgradeInitiator : StreamSecurityUpgradeInitiatorBase
    {
        SslStreamSecurityUpgradeProvider parent;
        SecurityMessageProperty serverSecurity;
        SecurityTokenProvider clientCertificateProvider;
        X509SecurityToken clientToken;
        SecurityTokenAuthenticator serverCertificateAuthenticator;
        ChannelBinding channelBindingToken;

        static LocalCertificateSelectionCallback clientCertificateSelectionCallback;

        public SslStreamSecurityUpgradeInitiator(SslStreamSecurityUpgradeProvider parent,
            EndpointAddress remoteAddress, Uri via)
            : base(FramingUpgradeString.SslOrTls, remoteAddress, via)
        {
            this.parent = parent;

            InitiatorServiceModelSecurityTokenRequirement serverCertRequirement = new InitiatorServiceModelSecurityTokenRequirement();
            serverCertRequirement.TokenType = SecurityTokenTypes.X509Certificate;
            serverCertRequirement.RequireCryptographicToken = true;
            serverCertRequirement.KeyUsage = SecurityKeyUsage.Exchange;
            serverCertRequirement.TargetAddress = remoteAddress;
            serverCertRequirement.Via = via;
            serverCertRequirement.TransportScheme = this.parent.Scheme;
            serverCertRequirement.PreferSslCertificateAuthenticator = true;

            SecurityTokenResolver dummy;
            this.serverCertificateAuthenticator = (parent.ClientSecurityTokenManager.CreateSecurityTokenAuthenticator(serverCertRequirement, out dummy));

            if (parent.RequireClientCertificate)
            {
                InitiatorServiceModelSecurityTokenRequirement clientCertRequirement = new InitiatorServiceModelSecurityTokenRequirement();
                clientCertRequirement.TokenType = SecurityTokenTypes.X509Certificate;
                clientCertRequirement.RequireCryptographicToken = true;
                clientCertRequirement.KeyUsage = SecurityKeyUsage.Signature;
                clientCertRequirement.TargetAddress = remoteAddress;
                clientCertRequirement.Via = via;
                clientCertRequirement.TransportScheme = this.parent.Scheme;
                this.clientCertificateProvider = parent.ClientSecurityTokenManager.CreateSecurityTokenProvider(clientCertRequirement);
                if (clientCertificateProvider == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ClientCredentialsUnableToCreateLocalTokenProvider, clientCertRequirement)));
                }
            }
        }

        static LocalCertificateSelectionCallback ClientCertificateSelectionCallback
        {
            get
            {
                if (clientCertificateSelectionCallback == null)
                {
                    clientCertificateSelectionCallback = new LocalCertificateSelectionCallback(SelectClientCertificate);
                }
                return clientCertificateSelectionCallback;
            }
        }

        internal ChannelBinding ChannelBinding
        {
            get
            {
                Fx.Assert(this.IsChannelBindingSupportEnabled, "A request for the ChannelBinding is not permitted without enabling ChannelBinding first (through the IChannelBindingProvider interface)");
                return this.channelBindingToken;
            }
        }

        internal bool IsChannelBindingSupportEnabled
        {
            get
            {
                return ((IChannelBindingProvider)parent).IsChannelBindingSupportEnabled;
            }
        }

        IAsyncResult BaseBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return base.BeginOpen(timeout, callback, state);
        }

        void BaseEndOpen(IAsyncResult result)
        {
            base.EndOpen(result);
        }

        internal override IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenAsyncResult(this, timeout, callback, state);
        }

        internal override void EndOpen(IAsyncResult result)
        {
            OpenAsyncResult.End(result);
        }

        internal override void Open(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.Open(timeoutHelper.RemainingTime());
            if (this.clientCertificateProvider != null)
            {
                SecurityUtils.OpenTokenProviderIfRequired(this.clientCertificateProvider, timeoutHelper.RemainingTime());
                this.clientToken = (X509SecurityToken)this.clientCertificateProvider.GetToken(timeoutHelper.RemainingTime());
            }
        }

        IAsyncResult BaseBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return base.BeginClose(timeout, callback, state);
        }

        void BaseEndClose(IAsyncResult result)
        {
            base.EndClose(result);
        }

        internal override IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseAsyncResult(this, timeout, callback, state);
        }

        internal override void EndClose(IAsyncResult result)
        {
            CloseAsyncResult.End(result);
        }

        internal override void Close(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.Close(timeoutHelper.RemainingTime());
            if (this.clientCertificateProvider != null)
            {
                SecurityUtils.CloseTokenProviderIfRequired(this.clientCertificateProvider, timeoutHelper.RemainingTime());
            }
        }

        protected override IAsyncResult OnBeginInitiateUpgrade(Stream stream, AsyncCallback callback, object state)
        {
            if (TD.SslOnInitiateUpgradeIsEnabled())
            {
                TD.SslOnInitiateUpgrade();
            }

            InitiateUpgradeAsyncResult result = new InitiateUpgradeAsyncResult(this, callback, state);
            result.Begin(stream);
            return result;
        }

        protected override Stream OnEndInitiateUpgrade(IAsyncResult result,
            out SecurityMessageProperty remoteSecurity)
        {
            return InitiateUpgradeAsyncResult.End(result, out remoteSecurity, out this.channelBindingToken);
        }

        protected override Stream OnInitiateUpgrade(Stream stream, out SecurityMessageProperty remoteSecurity)
        {
            if (TD.SslOnInitiateUpgradeIsEnabled())
            {
                TD.SslOnInitiateUpgrade();
            }

            X509CertificateCollection clientCertificates = null;
            LocalCertificateSelectionCallback selectionCallback = null;
            if (this.clientToken != null)
            {
                clientCertificates = new X509CertificateCollection();
                clientCertificates.Add(clientToken.Certificate);
                selectionCallback = ClientCertificateSelectionCallback;
            }

            SslStream sslStream = new SslStream(stream, false, this.ValidateRemoteCertificate, selectionCallback);
            try
            {
                sslStream.AuthenticateAsClient(string.Empty, clientCertificates, this.parent.SslProtocols, false);
            }
            catch (SecurityTokenValidationException tokenValidationException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(tokenValidationException.Message,
                    tokenValidationException));
            }
            catch (AuthenticationException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(exception.Message,
                    exception));
            }
            catch (IOException ioException)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(
                    SR.GetString(SR.NegotiationFailedIO, ioException.Message), ioException));
            }

            if (SecurityUtils.ShouldValidateSslCipherStrength())
            {
                SecurityUtils.ValidateSslCipherStrength(sslStream.CipherStrength);
            }

            remoteSecurity = this.serverSecurity;

            if (this.IsChannelBindingSupportEnabled)
            {
                this.channelBindingToken = ChannelBindingUtility.GetToken(sslStream);
            }

            return sslStream;
        }

        static X509Certificate SelectClientCertificate(object sender, string targetHost,
            X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return localCertificates[0];
        }

        bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            // Note: add ref to handle since the caller will reset the cert after the callback return.
            X509Certificate2 certificate2 = new X509Certificate2(certificate);
            SecurityToken token = new X509SecurityToken(certificate2, false);
            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = this.serverCertificateAuthenticator.ValidateToken(token);
            this.serverSecurity = new SecurityMessageProperty();
            this.serverSecurity.TransportToken = new SecurityTokenSpecification(token, authorizationPolicies);
            this.serverSecurity.ServiceSecurityContext = new ServiceSecurityContext(authorizationPolicies);

            AuthorizationContext authzContext = this.serverSecurity.ServiceSecurityContext.AuthorizationContext;
            this.parent.IdentityVerifier.EnsureOutgoingIdentity(this.RemoteAddress, this.Via, authzContext);

            return true;
        }

        class InitiateUpgradeAsyncResult : StreamSecurityUpgradeInitiatorAsyncResult
        {
            X509CertificateCollection clientCertificates;
            SslStreamSecurityUpgradeInitiator initiator;
            LocalCertificateSelectionCallback selectionCallback;
            SslStream sslStream;
            ChannelBinding channelBindingToken;

            public InitiateUpgradeAsyncResult(SslStreamSecurityUpgradeInitiator initiator, AsyncCallback callback,
                object state)
                : base(callback, state)
            {
                this.initiator = initiator;
                if (initiator.clientToken != null)
                {
                    this.clientCertificates = new X509CertificateCollection();
                    this.clientCertificates.Add(initiator.clientToken.Certificate);
                    this.selectionCallback = ClientCertificateSelectionCallback;
                }
            }

            protected override IAsyncResult OnBeginAuthenticateAsClient(Stream stream, AsyncCallback callback)
            {
                this.sslStream = new SslStream(stream, false, this.initiator.ValidateRemoteCertificate,
                    this.selectionCallback);

                try
                {
                    return this.sslStream.BeginAuthenticateAsClient(string.Empty, this.clientCertificates,
                        this.initiator.parent.SslProtocols, false, callback, this);
                }
                catch (SecurityTokenValidationException tokenValidationException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(tokenValidationException.Message,
                        tokenValidationException));
                }
            }

            protected override Stream OnCompleteAuthenticateAsClient(IAsyncResult result)
            {
                try
                {
                    this.sslStream.EndAuthenticateAsClient(result);
                }
                catch (SecurityTokenValidationException tokenValidationException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(tokenValidationException.Message,
                        tokenValidationException));
                }

                if (SecurityUtils.ShouldValidateSslCipherStrength())
                {
                    SecurityUtils.ValidateSslCipherStrength(sslStream.CipherStrength);
                }

                if (this.initiator.IsChannelBindingSupportEnabled)
                {
                    this.channelBindingToken = ChannelBindingUtility.GetToken(this.sslStream);
                }

                return this.sslStream;
            }

            protected override SecurityMessageProperty ValidateCreateSecurity()
            {
                return this.initiator.serverSecurity;
            }

            public static Stream End(IAsyncResult result, out SecurityMessageProperty remoteSecurity, out ChannelBinding channelBinding)
            {
                Stream stream = StreamSecurityUpgradeInitiatorAsyncResult.End(result, out remoteSecurity);
                channelBinding = ((InitiateUpgradeAsyncResult)result).channelBindingToken;
                return stream;
            }
        }

        class OpenAsyncResult : AsyncResult
        {
            SslStreamSecurityUpgradeInitiator parent;
            TimeoutHelper timeoutHelper;
            AsyncCallback onBaseOpen;
            AsyncCallback onOpenTokenProvider;
            AsyncCallback onGetClientToken;

            public OpenAsyncResult(SslStreamSecurityUpgradeInitiator parent, TimeSpan timeout,
                AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.parent = parent;
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

                // since we're at channel.Open and not per-message, minimize our statics overhead and leverage GC for our callback
                this.onBaseOpen = Fx.ThunkCallback(new AsyncCallback(OnBaseOpen));
                if (parent.clientCertificateProvider != null)
                {
                    this.onOpenTokenProvider = Fx.ThunkCallback(new AsyncCallback(OnOpenTokenProvider));
                    this.onGetClientToken = Fx.ThunkCallback(new AsyncCallback(OnGetClientToken));
                }
                IAsyncResult result = parent.BaseBeginOpen(timeoutHelper.RemainingTime(), onBaseOpen, this);

                if (!result.CompletedSynchronously)
                {
                    return;
                }

                if (HandleBaseOpenComplete(result))
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<OpenAsyncResult>(result);
            }

            bool HandleBaseOpenComplete(IAsyncResult result)
            {
                parent.BaseEndOpen(result);
                if (parent.clientCertificateProvider == null)
                {
                    return true;
                }

                IAsyncResult openTokenProviderResult = SecurityUtils.BeginOpenTokenProviderIfRequired(
                    parent.clientCertificateProvider, timeoutHelper.RemainingTime(), onOpenTokenProvider, this);

                if (!openTokenProviderResult.CompletedSynchronously)
                {
                    return false;
                }

                return HandleOpenTokenProviderComplete(openTokenProviderResult);
            }

            bool HandleOpenTokenProviderComplete(IAsyncResult result)
            {
                SecurityUtils.EndOpenTokenProviderIfRequired(result);
                IAsyncResult getTokenResult = parent.clientCertificateProvider.BeginGetToken(timeoutHelper.RemainingTime(),
                    onGetClientToken, this);

                if (!getTokenResult.CompletedSynchronously)
                {
                    return false;
                }

                return HandleGetTokenComplete(getTokenResult);
            }

            bool HandleGetTokenComplete(IAsyncResult result)
            {
                parent.clientToken = (X509SecurityToken)parent.clientCertificateProvider.EndGetToken(result);
                return true;
            }

            void OnBaseOpen(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                Exception completionException = null;
                bool completeSelf = false;
                try
                {
                    completeSelf = this.HandleBaseOpenComplete(result);
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
                    base.Complete(false, completionException);
                }
            }

            void OnOpenTokenProvider(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                Exception completionException = null;
                bool completeSelf = false;
                try
                {
                    completeSelf = this.HandleOpenTokenProviderComplete(result);
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
                    base.Complete(false, completionException);
                }
            }

            void OnGetClientToken(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                Exception completionException = null;
                bool completeSelf = false;
                try
                {
                    completeSelf = this.HandleGetTokenComplete(result);
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
                    base.Complete(false, completionException);
                }
            }
        }

        class CloseAsyncResult : AsyncResult
        {
            SslStreamSecurityUpgradeInitiator parent;
            TimeoutHelper timeoutHelper;
            AsyncCallback onBaseClose;
            AsyncCallback onCloseTokenProvider;

            public CloseAsyncResult(SslStreamSecurityUpgradeInitiator parent, TimeSpan timeout,
                AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.parent = parent;
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

                // since we're at channel.Open and not per-message, minimize our statics overhead and leverage GC for our callback
                this.onBaseClose = Fx.ThunkCallback(new AsyncCallback(OnBaseClose));
                if (parent.clientCertificateProvider != null)
                {
                    this.onCloseTokenProvider = Fx.ThunkCallback(new AsyncCallback(OnCloseTokenProvider));
                }
                IAsyncResult result = parent.BaseBeginClose(timeoutHelper.RemainingTime(), onBaseClose, this);

                if (!result.CompletedSynchronously)
                {
                    return;
                }

                if (HandleBaseCloseComplete(result))
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseAsyncResult>(result);
            }

            bool HandleBaseCloseComplete(IAsyncResult result)
            {
                parent.BaseEndClose(result);
                if (parent.clientCertificateProvider == null)
                {
                    return true;
                }

                IAsyncResult closeTokenProviderResult = SecurityUtils.BeginCloseTokenProviderIfRequired(
                    parent.clientCertificateProvider, timeoutHelper.RemainingTime(), onCloseTokenProvider, this);

                if (!closeTokenProviderResult.CompletedSynchronously)
                {
                    return false;
                }

                SecurityUtils.EndCloseTokenProviderIfRequired(closeTokenProviderResult);
                return true;
            }

            void OnBaseClose(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                Exception completionException = null;
                bool completeSelf = false;
                try
                {
                    completeSelf = this.HandleBaseCloseComplete(result);
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
                    base.Complete(false, completionException);
                }
            }

            void OnCloseTokenProvider(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                Exception completionException = null;
                try
                {
                    SecurityUtils.EndCloseTokenProviderIfRequired(result);
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

                base.Complete(false, completionException);
            }
        }
    }
}
