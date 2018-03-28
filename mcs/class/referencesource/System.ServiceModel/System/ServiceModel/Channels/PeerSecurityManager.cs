//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    class PeerSecurityManager
    {
        PeerAuthenticationMode authenticationMode;
        bool enableSigning;
        internal string password;

        DuplexSecurityProtocolFactory securityProtocolFactory;

        // Double-checked locking pattern requires volatile for read/write synchronization
        volatile byte[] authenticatorHash;

        object thisLock;
        //public EventHandler OnNeighborOpened;
        public EventHandler OnNeighborAuthenticated;
        string meshId = String.Empty;
        ChannelProtectionRequirements protection;
        PeerSecurityCredentialsManager credManager;
        SecurityTokenManager tokenManager;

        // Double-checked locking pattern requires volatile for read/write synchronization
        volatile SelfSignedCertificate ssc;
        XmlDictionaryReaderQuotas readerQuotas;

        // Audit
        ServiceSecurityAuditBehavior auditBehavior;

        PeerSecurityManager(PeerAuthenticationMode authMode, bool signing)
        {
            this.authenticationMode = authMode;
            this.enableSigning = signing;
            thisLock = new object();
        }

        public PeerAuthenticationMode AuthenticationMode
        {
            get
            {
                return authenticationMode;
            }
        }

        public string Password
        {
            get
            {
                return password;
            }
        }
        public X509Certificate2 SelfCert
        {
            get
            {
                return credManager.Certificate;
            }
        }
        public bool MessageAuthentication
        {
            get
            {
                return this.enableSigning;
            }
        }

        internal string MeshId
        {
            get
            {
                return this.meshId;
            }
            set
            {
                this.meshId = value;
            }
        }

        internal SelfSignedCertificate GetCertificate()
        {
            if (this.ssc == null)
            {
                lock (ThisLock)
                {
                    if (ssc == null)
                        ssc = SelfSignedCertificate.Create("CN=" + Guid.NewGuid().ToString(), this.Password);
                }
            }
            return ssc;
        }

        object ThisLock
        {
            get { return thisLock; }
        }

        static PeerSecurityCredentialsManager GetCredentialsManager(PeerAuthenticationMode mode, bool signing, BindingContext context)
        {
            if (mode == PeerAuthenticationMode.None && !signing)
                return null;
            ClientCredentials clientCredentials = context.BindingParameters.Find<ClientCredentials>();
            if (clientCredentials != null)
            {
                return new PeerSecurityCredentialsManager(clientCredentials.Peer, mode, signing);
            }
            ServiceCredentials serviceCredentials = context.BindingParameters.Find<ServiceCredentials>();
            if (serviceCredentials != null)
            {
                return new PeerSecurityCredentialsManager(serviceCredentials.Peer, mode, signing);
            }
            SecurityCredentialsManager credman = context.BindingParameters.Find<SecurityCredentialsManager>();
            if (credman == null)
            {
                PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.Credentials);
            }
            return new PeerSecurityCredentialsManager(credman.CreateSecurityTokenManager(), mode, signing);
        }

        static void Convert(PeerSecuritySettings security, out PeerAuthenticationMode authMode, out bool signing)
        {
            authMode = PeerAuthenticationMode.None;
            signing = false;
            if (security.Mode == SecurityMode.Transport || security.Mode == SecurityMode.TransportWithMessageCredential)
            {
                switch (security.Transport.CredentialType)
                {
                    case PeerTransportCredentialType.Password:
                        authMode = PeerAuthenticationMode.Password;
                        break;
                    case PeerTransportCredentialType.Certificate:
                        authMode = PeerAuthenticationMode.MutualCertificate;
                        break;
                }
            }
            if (security.Mode == SecurityMode.Message || security.Mode == SecurityMode.TransportWithMessageCredential)
            {
                signing = true;
            }
        }

        static public PeerSecurityManager Create(PeerSecuritySettings security, BindingContext context, XmlDictionaryReaderQuotas readerQuotas)
        {
            PeerAuthenticationMode authMode = PeerAuthenticationMode.None;
            bool signing = false;
            Convert(security, out authMode, out signing);
            return Create(authMode, signing, context, readerQuotas);

        }

        static public PeerSecurityManager Create(PeerAuthenticationMode authenticationMode, bool signMessages, BindingContext context, XmlDictionaryReaderQuotas readerQuotas)
        {
            if (authenticationMode == PeerAuthenticationMode.None && !signMessages)
                return CreateDummy();

            // test FIPS mode
            if (authenticationMode == PeerAuthenticationMode.Password)
            {
                try
                {
                    using (HMACSHA256 algo = new HMACSHA256())
                    {
                        using (SHA256Managed sha = new SHA256Managed()) { }
                    }
                }
                catch (InvalidOperationException e)
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                    PeerExceptionHelper.ThrowInvalidOperation_InsufficientCryptoSupport(e);
                }
            }

            ChannelProtectionRequirements reqs = context.BindingParameters.Find<ChannelProtectionRequirements>();
            PeerSecurityCredentialsManager credman = GetCredentialsManager(authenticationMode, signMessages, context);
            if (credman.Credential != null)
            {
                //for compatibility with existing code:
                ValidateCredentialSettings(authenticationMode, signMessages, credman.Credential);
            }
            PeerSecurityManager manager = Create(authenticationMode, signMessages, credman, reqs, readerQuotas);
            credman.Parent = manager;
            manager.ApplyAuditBehaviorSettings(context);

            return manager;
        }

        static void ValidateCredentialSettings(PeerAuthenticationMode authenticationMode, bool signMessages, PeerCredential credential)
        {
            X509CertificateValidator validator;
            if (authenticationMode == PeerAuthenticationMode.None && !signMessages)
                return;
            switch (authenticationMode)
            {
                case PeerAuthenticationMode.Password:
                    {
                        if (String.IsNullOrEmpty(credential.MeshPassword))
                            PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.Password);
                    }
                    break;
                case PeerAuthenticationMode.MutualCertificate:
                    {
                        if (credential.Certificate == null)
                        {
                            PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.Certificate);
                        }
                        if (!credential.PeerAuthentication.TryGetCertificateValidator(out validator))
                        {
                            PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.PeerAuthentication);
                        }

                    }
                    break;
            }
            if (signMessages)
            {
                if (!credential.MessageSenderAuthentication.TryGetCertificateValidator(out validator))
                {
                    PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.MessageSenderAuthentication);
                }
            }
        }

        void ApplyAuditBehaviorSettings(BindingContext context)
        {
            ServiceSecurityAuditBehavior auditBehavior = context.BindingParameters.Find<ServiceSecurityAuditBehavior>();
            if (auditBehavior != null)
            {
                this.auditBehavior = auditBehavior.Clone();
            }
            else
            {
                this.auditBehavior = new ServiceSecurityAuditBehavior();
            }
        }

        public void ApplyServiceSecurity(ServiceDescription description)
        {
            if (this.AuthenticationMode == PeerAuthenticationMode.None)
                return;
            description.Behaviors.Add(credManager.CloneForTransport());
        }

        internal static PeerSecurityManager CreateDummy()
        {
            PeerSecurityManager manager = new PeerSecurityManager(PeerAuthenticationMode.None, false);
            return manager;
        }

        static public PeerSecurityManager Create(PeerAuthenticationMode authenticationMode, bool messageAuthentication, PeerSecurityCredentialsManager credman, ChannelProtectionRequirements reqs, XmlDictionaryReaderQuotas readerQuotas)
        {
            PeerSecurityManager manager = null;
            X509CertificateValidator connectionValidator = null;
            X509CertificateValidator messageValidator = null;
            PeerCredential credential = credman.Credential;

            if (null == credential && credman == null)
            {
                if (authenticationMode != PeerAuthenticationMode.None || messageAuthentication)
                    PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.Credentials);
                //create one that doesnt have any credentials in it.
                return CreateDummy();
            }

            manager = new PeerSecurityManager(authenticationMode, messageAuthentication);
            manager.credManager = credman;
            manager.password = credman.Password;
            manager.readerQuotas = readerQuotas;
            if (reqs != null)
            {
                manager.protection = new ChannelProtectionRequirements(reqs);
            }
            manager.tokenManager = credman.CreateSecurityTokenManager();
            if (credential == null)
                return manager;

            switch (authenticationMode)
            {
                case PeerAuthenticationMode.None:
                    break;
                case PeerAuthenticationMode.Password:
                    {
                        manager.password = credential.MeshPassword;
                        if (String.IsNullOrEmpty(manager.credManager.Password))
                        {
                            PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.Password);
                        }
                        connectionValidator = X509CertificateValidator.None;
                    }
                    break;
                case PeerAuthenticationMode.MutualCertificate:
                    {
                        if (manager.credManager.Certificate == null)
                        {
                            PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.Certificate);
                        }
                        if (!credential.PeerAuthentication.TryGetCertificateValidator(out connectionValidator))
                        {
                            PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.PeerAuthentication);
                        }
                    }
                    break;
            }
            if (messageAuthentication)
            {
                if (credential.MessageSenderAuthentication != null)
                {
                    if (!credential.MessageSenderAuthentication.TryGetCertificateValidator(out messageValidator))
                    {
                        PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.MessageSenderAuthentication);
                    }
                }
                else
                {
                    PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.MessageSenderAuthentication);
                }
            }
            return manager;
        }

        void ApplySigningRequirements(ScopedMessagePartSpecification spec)
        {
            //following are the headers that we add and want signed.
            MessagePartSpecification partSpec = new MessagePartSpecification(
                                new XmlQualifiedName(PeerStrings.Via, PeerStrings.Namespace),
                                new XmlQualifiedName(PeerOperationNames.Flood, PeerStrings.Namespace),
                                new XmlQualifiedName(PeerOperationNames.PeerTo, PeerStrings.Namespace),
                                new XmlQualifiedName(PeerStrings.MessageId, PeerStrings.Namespace));
            foreach (string action in spec.Actions)
            {
                spec.AddParts(partSpec, action);
            }
            spec.AddParts(partSpec, MessageHeaders.WildcardAction);
        }

        public void Open()
        {
            CreateSecurityProtocolFactory();
        }

        void CreateSecurityProtocolFactory()
        {
            SecurityProtocolFactory incomingProtocolFactory;
            SecurityProtocolFactory outgoingProtocolFactory;
            ChannelProtectionRequirements protectionRequirements;

            lock (ThisLock)
            {
                if (null != securityProtocolFactory)
                    return;

                TimeoutHelper timeoutHelper = new TimeoutHelper(ServiceDefaults.SendTimeout);
                if (!enableSigning)
                {
                    outgoingProtocolFactory = new PeerDoNothingSecurityProtocolFactory();
                    incomingProtocolFactory = new PeerDoNothingSecurityProtocolFactory();
                }
                else
                {
                    X509Certificate2 cert = credManager.Certificate;
                    if (cert != null)
                    {
                        SecurityBindingElement securityBindingElement = SecurityBindingElement.CreateCertificateSignatureBindingElement();
                        securityBindingElement.ReaderQuotas = this.readerQuotas;
                        BindingParameterCollection bpc = new BindingParameterCollection();
                        if (protection == null)
                        {
                            protectionRequirements = new ChannelProtectionRequirements();
                        }
                        else
                        {
                            protectionRequirements = new ChannelProtectionRequirements(protection);
                        }
                        ApplySigningRequirements(protectionRequirements.IncomingSignatureParts);
                        ApplySigningRequirements(protectionRequirements.OutgoingSignatureParts);

                        bpc.Add(protectionRequirements);
                        bpc.Add(this.auditBehavior);
                        bpc.Add(credManager);
                        BindingContext context = new BindingContext(new CustomBinding(securityBindingElement), bpc);
                        outgoingProtocolFactory = securityBindingElement.CreateSecurityProtocolFactory<IOutputChannel>(context, credManager, false, null);
                    }
                    else
                    {
                        outgoingProtocolFactory = new PeerDoNothingSecurityProtocolFactory();
                    }
                    SecurityTokenResolver resolver;
                    X509SecurityTokenAuthenticator auth = tokenManager.CreateSecurityTokenAuthenticator(PeerSecurityCredentialsManager.PeerClientSecurityTokenManager.CreateRequirement(SecurityTokenTypes.X509Certificate, true), out resolver) as X509SecurityTokenAuthenticator;
                    if (auth != null)
                    {
                        SecurityBindingElement securityBindingElement = SecurityBindingElement.CreateCertificateSignatureBindingElement();
                        securityBindingElement.ReaderQuotas = this.readerQuotas;
                        BindingParameterCollection bpc = new BindingParameterCollection();
                        if (protection == null)
                        {
                            protectionRequirements = new ChannelProtectionRequirements();
                        }
                        else
                        {
                            protectionRequirements = new ChannelProtectionRequirements(protection);
                        }
                        ApplySigningRequirements(protectionRequirements.IncomingSignatureParts);
                        ApplySigningRequirements(protectionRequirements.OutgoingSignatureParts);

                        bpc.Add(protectionRequirements);
                        bpc.Add(this.auditBehavior);
                        bpc.Add(credManager);
                        BindingContext context = new BindingContext(new CustomBinding(securityBindingElement), bpc);
                        incomingProtocolFactory = securityBindingElement.CreateSecurityProtocolFactory<IOutputChannel>(context, credManager, true, null);
                    }
                    else
                    {
                        incomingProtocolFactory = new PeerDoNothingSecurityProtocolFactory();
                    }
                }
                DuplexSecurityProtocolFactory tempFactory = new DuplexSecurityProtocolFactory(outgoingProtocolFactory, incomingProtocolFactory);
                tempFactory.Open(true, timeoutHelper.RemainingTime());
                securityProtocolFactory = tempFactory;
            }
        }

        public SecurityProtocolFactory GetProtocolFactory<TChannel>()
        {
            if (securityProtocolFactory == null)
            {
                CreateSecurityProtocolFactory();
            }
            if (typeof(TChannel) == typeof(IOutputChannel))
            {
                if (enableSigning && securityProtocolFactory.ForwardProtocolFactory is PeerDoNothingSecurityProtocolFactory)
                    PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.MessageSenderAuthentication);
                return securityProtocolFactory.ForwardProtocolFactory;
            }
            else if (typeof(TChannel) == typeof(IInputChannel))
            {
                if (enableSigning && securityProtocolFactory.ReverseProtocolFactory is PeerDoNothingSecurityProtocolFactory)
                    PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.MessageSenderAuthentication);
                return securityProtocolFactory.ReverseProtocolFactory;
            }
            else
            {
                if (enableSigning && ((securityProtocolFactory.ReverseProtocolFactory is PeerDoNothingSecurityProtocolFactory)
                                        || (securityProtocolFactory.ForwardProtocolFactory is PeerDoNothingSecurityProtocolFactory)))
                    PeerExceptionHelper.ThrowArgument_InsufficientCredentials(PeerPropertyNames.MessageSenderAuthentication);
                return securityProtocolFactory;
            }
        }

        public SecurityProtocol CreateSecurityProtocol<TChannel>(EndpointAddress target, TimeSpan timespan)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timespan);
            SecurityProtocolFactory factory = GetProtocolFactory<TChannel>();
            Fx.Assert(factory != null, "SecurityProtocolFactory is NULL!");
            SecurityProtocol instance = factory.CreateSecurityProtocol(target, null, /*listenerSecurityState*/null, /*isReturnLegSecurityRequired*/false, timeoutHelper.RemainingTime());
            if (instance != null)
                instance.Open(timeoutHelper.RemainingTime());
            return instance;
        }

        public void CheckIfCompatibleNodeSettings(object other)
        {
            string mismatch = null;
            PeerSecurityManager that = other as PeerSecurityManager;
            if (that == null)
                mismatch = PeerBindingPropertyNames.Security;
            else if (this.authenticationMode != that.authenticationMode)
                mismatch = PeerBindingPropertyNames.SecurityDotMode;
            else if (this.authenticationMode == PeerAuthenticationMode.None)
                return;
            else if (!this.tokenManager.Equals(that.tokenManager))
            {
                if (this.credManager != null)
                    this.credManager.CheckIfCompatible(that.credManager);
                else
                {
                    Fx.Assert(typeof(PeerSecurityCredentialsManager.PeerClientSecurityTokenManager).IsAssignableFrom(tokenManager.GetType()), "");
                    mismatch = PeerBindingPropertyNames.Credentials;
                }
            }
            if (mismatch != null)
                PeerExceptionHelper.ThrowInvalidOperation_PeerConflictingPeerNodeSettings(mismatch);
        }

        public bool HasCompatibleMessageSecurity(PeerSecurityManager that)
        {
            return (this.MessageAuthentication == that.MessageAuthentication);
        }

        public byte[] GetAuthenticator()
        {
            if (authenticationMode != PeerAuthenticationMode.Password)
                return null;
            if (authenticatorHash == null)
            {
                lock (ThisLock)
                {
                    if (authenticatorHash == null)
                    {
                        authenticatorHash = PeerSecurityHelpers.ComputeHash(credManager.Certificate, credManager.Password);
                    }
                }
            }
            return authenticatorHash;
        }


        public bool Authenticate(ServiceSecurityContext context, byte[] message)
        {
            Claim claim = null;
            if (context == null)
            {
                return (authenticationMode == PeerAuthenticationMode.None);
            }
            if (authenticationMode == PeerAuthenticationMode.Password)
            {
                if (!(context != null))
                {
                    throw Fx.AssertAndThrow("No SecurityContext attached in security mode!");
                }
                claim = FindClaim(context);
                return PeerSecurityHelpers.Authenticate(claim, this.credManager.Password, message);
            }
            else
            {
                if (message != null)
                {
                    PeerExceptionHelper.ThrowInvalidOperation_UnexpectedSecurityTokensDuringHandshake();
                }
                return true;
            }
        }

        public static Claim FindClaim(ServiceSecurityContext context)
        {
            Claim result = null;
            Fx.Assert(context != null, "ServiceSecurityContext is null!");
            for (int i = 0; i < context.AuthorizationContext.ClaimSets.Count; ++i)
            {
                ClaimSet claimSet = context.AuthorizationContext.ClaimSets[i];
                IEnumerator<Claim> claims = claimSet.FindClaims(ClaimTypes.Rsa, null).GetEnumerator();
                if (claims.MoveNext())
                {
                    result = claims.Current;
                    break;
                }
            }
            return result;
        }

        public void ApplyClientSecurity(ChannelFactory<IPeerProxy> factory)
        {
            factory.Endpoint.Behaviors.Remove<ClientCredentials>();
            if (authenticationMode != PeerAuthenticationMode.None)
            {
                factory.Endpoint.Behaviors.Add(this.credManager.CloneForTransport());
            }
        }

        public BindingElement GetSecurityBindingElement()
        {
            SslStreamSecurityBindingElement security = null;
            if (this.AuthenticationMode != PeerAuthenticationMode.None)
            {
                security = new SslStreamSecurityBindingElement();
                security.IdentityVerifier = new PeerIdentityVerifier();
                security.RequireClientCertificate = true;
            }
            return security;
        }

        public PeerHashToken GetSelfToken()
        {
            if (!(this.authenticationMode == PeerAuthenticationMode.Password))
            {
                throw Fx.AssertAndThrow("unexpected call to GetSelfToken");
            }
            return new PeerHashToken(this.credManager.Certificate, this.credManager.Password);
        }

        public PeerHashToken GetExpectedTokenForClaim(Claim claim)
        {
            return new PeerHashToken(claim, this.password);
        }

        public void OnNeighborOpened(object sender, EventArgs args)
        {
            IPeerNeighbor neighbor = sender as IPeerNeighbor;
            EventHandler handler = this.OnNeighborAuthenticated;
            if (handler == null)
            {
                neighbor.Abort(PeerCloseReason.LeavingMesh, PeerCloseInitiator.LocalNode);
                return;
            }
            if (this.authenticationMode == PeerAuthenticationMode.Password)
            {
                if (!(neighbor.Extensions.Find<PeerChannelAuthenticatorExtension>() == null))
                {
                    throw Fx.AssertAndThrow("extension already exists!");
                }
                PeerChannelAuthenticatorExtension extension = new PeerChannelAuthenticatorExtension(this, handler, args, this.MeshId);
                neighbor.Extensions.Add(extension);
                if (neighbor.IsInitiator)
                    extension.InitiateHandShake();
            }
            else
            {
                neighbor.TrySetState(PeerNeighborState.Authenticated);
                handler(sender, args);
            }
        }

        public Message ProcessRequest(IPeerNeighbor neighbor, Message request)
        {
            if (this.authenticationMode != PeerAuthenticationMode.Password || request == null)
            {
                Abort(neighbor);
                return null;
            }
            PeerChannelAuthenticatorExtension extension = neighbor.Extensions.Find<PeerChannelAuthenticatorExtension>();
            Claim claim = FindClaim(ServiceSecurityContext.Current);
            if (!(extension != null && claim != null))
            {
                throw Fx.AssertAndThrow("No suitable claim found in the context to do security negotiation!");
            }
            return extension.ProcessRst(request, claim);
        }

        void Abort(IPeerNeighbor neighbor)
        {
            neighbor.Abort(PeerCloseReason.AuthenticationFailure, PeerCloseInitiator.LocalNode);
        }
    }

    class PeerSecurityCredentialsManager : SecurityCredentialsManager, IEndpointBehavior, IServiceBehavior
    {
        SecurityTokenManager manager;
        PeerCredential credential;
        bool messageAuth;
        PeerAuthenticationMode mode = PeerAuthenticationMode.Password;
        SelfSignedCertificate ssl;
        PeerSecurityManager parent;

        public PeerSecurityCredentialsManager(SecurityTokenManager manager, PeerAuthenticationMode mode, bool messageAuth)
            : base()
        {
            this.manager = manager;
            this.mode = mode;
            this.messageAuth = messageAuth;
        }

        public PeerSecurityCredentialsManager(PeerCredential credential, PeerAuthenticationMode mode, bool messageAuth)
            : base()
        {
            this.credential = credential;
            this.mode = mode;
            this.messageAuth = messageAuth;
        }

        public PeerSecurityManager Parent
        {
            get
            {
                return this.parent;
            }
            set
            {
                parent = value;
            }
        }

        public override SecurityTokenManager CreateSecurityTokenManager()
        {
            if (manager != null)
                return new PeerClientSecurityTokenManager(this.parent, manager, mode, messageAuth);
            else
                return new PeerClientSecurityTokenManager(this.parent, credential, mode, messageAuth);
        }

        public PeerSecurityCredentialsManager() : base() { }

        public PeerSecurityCredentialsManager CloneForTransport()
        {
            PeerSecurityCredentialsManager cloner = new PeerSecurityCredentialsManager();
            if (this.credential != null)
                cloner.credential = new PeerCredential(this.credential);
            cloner.mode = this.mode;
            cloner.messageAuth = this.messageAuth;
            cloner.manager = this.manager;
            cloner.parent = parent;

            return cloner;
        }
        internal PeerCredential Credential
        {
            get
            {
                return this.credential;
            }
        }
        internal string Password
        {
            get
            {
                if (this.credential != null)
                    return credential.MeshPassword;
                ServiceModelSecurityTokenRequirement req = PeerClientSecurityTokenManager.CreateRequirement(SecurityTokenTypes.UserName);
                UserNameSecurityTokenProvider tokenProvider = this.manager.CreateSecurityTokenProvider(req) as UserNameSecurityTokenProvider;
                if (tokenProvider == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TokenProvider");
                UserNameSecurityToken token = tokenProvider.GetToken(ServiceDefaults.SendTimeout) as UserNameSecurityToken;
                if (token == null || String.IsNullOrEmpty(token.Password))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("password");
                return token.Password;
            }
        }

        internal X509Certificate2 Certificate
        {
            get
            {
                X509Certificate2 result = null;
                if (mode == PeerAuthenticationMode.Password)
                {
                    if (ssl != null)
                        result = ssl.GetX509Certificate();
                }
                if (this.credential != null)
                {
                    result = credential.Certificate;
                }
                else
                {
                    ServiceModelSecurityTokenRequirement req = PeerClientSecurityTokenManager.CreateRequirement(SecurityTokenTypes.X509Certificate);
                    X509SecurityTokenProvider tokenProvider = this.manager.CreateSecurityTokenProvider(req) as X509SecurityTokenProvider;
                    if (tokenProvider == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TokenProvider");
                    X509SecurityToken token = tokenProvider.GetToken(ServiceDefaults.SendTimeout) as X509SecurityToken;
                    if (token == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("token");
                    result = token.Certificate;
                }
                if (result == null && mode == PeerAuthenticationMode.Password)
                {
                    ssl = this.parent.GetCertificate();
                    result = ssl.GetX509Certificate();
                }
                return result;
            }
        }
        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection bindingParameters)
        {
            if (bindingParameters != null)
                bindingParameters.Add(this);
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {
        }

        void IServiceBehavior.Validate(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
            if (parameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }
            parameters.Add(this);
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        public override bool Equals(object other)
        {
            PeerSecurityCredentialsManager that = other as PeerSecurityCredentialsManager;
            if (that == null)
                return false;
            if (this.credential != null)
            {
                return this.credential.Equals(that.credential, mode, messageAuth);
            }
            else
            {
                return this.manager.Equals(that.manager);
            }
        }

        public void CheckIfCompatible(PeerSecurityCredentialsManager that)
        {
            if (that == null)
                PeerExceptionHelper.ThrowInvalidOperation_PeerConflictingPeerNodeSettings(PeerBindingPropertyNames.Credentials);
            if (this.mode == PeerAuthenticationMode.None)
                return;
            if (this.mode == PeerAuthenticationMode.Password)
            {
                if (this.Password != that.Password)
                    PeerExceptionHelper.ThrowInvalidOperation_PeerConflictingPeerNodeSettings(PeerBindingPropertyNames.Password);
            }
            if (!this.Certificate.Equals(that.Certificate))
                PeerExceptionHelper.ThrowInvalidOperation_PeerConflictingPeerNodeSettings(PeerBindingPropertyNames.Certificate);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public class PeerClientSecurityTokenManager : SecurityTokenManager
        {
            SecurityTokenManager delegateManager;
            PeerCredential credential;
            PeerAuthenticationMode mode;
            bool messageAuth;
            SelfSignedCertificate ssc;
            PeerSecurityManager parent;

            public PeerClientSecurityTokenManager(PeerSecurityManager parent, PeerCredential credential, PeerAuthenticationMode mode, bool messageAuth)
            {
                this.credential = credential;
                this.mode = mode;
                this.messageAuth = messageAuth;
                this.parent = parent;
            }

            public PeerClientSecurityTokenManager(PeerSecurityManager parent, SecurityTokenManager manager, PeerAuthenticationMode mode, bool messageAuth)
            {
                this.delegateManager = manager;
                this.mode = mode;
                this.messageAuth = messageAuth;
                this.parent = parent;
            }

            internal static ServiceModelSecurityTokenRequirement CreateRequirement(string tokenType)
            {
                return CreateRequirement(tokenType, false);
            }

            internal static ServiceModelSecurityTokenRequirement CreateRequirement(string tokenType, bool forMessageValidation)
            {
                InitiatorServiceModelSecurityTokenRequirement requirement = new InitiatorServiceModelSecurityTokenRequirement();
                requirement.TokenType = tokenType;
                requirement.TransportScheme = PeerStrings.Scheme;
                if (forMessageValidation)
                    requirement.Properties[SecurityTokenRequirement.PeerAuthenticationMode] = SecurityMode.Message;
                else
                    requirement.Properties[SecurityTokenRequirement.PeerAuthenticationMode] = SecurityMode.Transport;

                return requirement;
            }

            UserNameSecurityTokenProvider GetPasswordTokenProvider()
            {
                if (delegateManager != null)
                {
                    ServiceModelSecurityTokenRequirement requirement = CreateRequirement(SecurityTokenTypes.UserName);
                    UserNameSecurityTokenProvider tokenProvider = delegateManager.CreateSecurityTokenProvider(requirement) as UserNameSecurityTokenProvider;
                    if (tokenProvider == null)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SecurityTokenManagerCannotCreateProviderForRequirement, requirement)));
                    return tokenProvider;
                }
                else
                    return new UserNameSecurityTokenProvider(string.Empty, credential.MeshPassword);
            }
            public override SecurityTokenSerializer CreateSecurityTokenSerializer(SecurityTokenVersion version)
            {
                if (delegateManager != null)
                    return delegateManager.CreateSecurityTokenSerializer(version);
                else
                {
                    MessageSecurityTokenVersion wsVersion = version as MessageSecurityTokenVersion;
                    if (wsVersion != null)
                    {
                        return new WSSecurityTokenSerializer(wsVersion.SecurityVersion, wsVersion.TrustVersion, wsVersion.SecureConversationVersion, wsVersion.EmitBspRequiredAttributes, null, null, null);
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SecurityTokenManagerCannotCreateSerializerForVersion, version)));
                    }
                }
            }

            public override SecurityTokenProvider CreateSecurityTokenProvider(SecurityTokenRequirement tokenRequirement)
            {
                ServiceModelSecurityTokenRequirement requirement = tokenRequirement as ServiceModelSecurityTokenRequirement;
                if (requirement != null)
                {
                    if (IsX509TokenRequirement(requirement))
                    {
                        if (IsForConnectionValidator(requirement))
                        {
                            SecurityTokenProvider result = null;
                            if (this.ssc != null)
                            {
                                result = new X509SecurityTokenProvider(this.ssc.GetX509Certificate());
                            }
                            else
                            {
                                if (this.delegateManager != null)
                                {
                                    requirement.Properties[SecurityTokenRequirement.PeerAuthenticationMode] = SecurityMode.Transport;
                                    requirement.TransportScheme = PeerStrings.Scheme;
                                    result = delegateManager.CreateSecurityTokenProvider(tokenRequirement);
                                }
                                else
                                {
                                    if (this.credential.Certificate != null)
                                        result = new X509SecurityTokenProvider(this.credential.Certificate);
                                }
                            }
                            if (result == null && mode == PeerAuthenticationMode.Password)
                            {
                                this.ssc = parent.GetCertificate();
                                result = new X509SecurityTokenProvider(this.ssc.GetX509Certificate());
                            }

                            return result;
                        }
                        else
                        {
                            X509CertificateValidator validator;
                            if (this.delegateManager != null)
                            {
                                requirement.TransportScheme = PeerStrings.Scheme;
                                requirement.Properties[SecurityTokenRequirement.PeerAuthenticationMode] = SecurityMode.Message;
                                return delegateManager.CreateSecurityTokenProvider(tokenRequirement);
                            }
                            if (!this.credential.MessageSenderAuthentication.TryGetCertificateValidator(out validator))
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("TokenType");
                            return new PeerX509TokenProvider(validator, this.credential.Certificate);
                        }
                    }
                    else if (IsPasswordTokenRequirement(requirement))
                    {
                        return GetPasswordTokenProvider();
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("TokenType");
                    }
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenRequirement");
                }

            }

            bool IsPasswordTokenRequirement(ServiceModelSecurityTokenRequirement requirement)
            {
                return ((requirement != null) && (requirement.TokenType == SecurityTokenTypes.UserName));
            }

            bool IsX509TokenRequirement(ServiceModelSecurityTokenRequirement requirement)
            {
                return (requirement != null && requirement.TokenType == SecurityTokenTypes.X509Certificate);
            }

            bool IsForConnectionValidator(ServiceModelSecurityTokenRequirement requirement)
            {
                return (requirement.TransportScheme == "net.tcp" && requirement.SecurityBindingElement == null && requirement.MessageSecurityVersion == null);
            }

            public override SecurityTokenAuthenticator CreateSecurityTokenAuthenticator(SecurityTokenRequirement tokenRequirement, out SecurityTokenResolver outOfBandTokenResolver)
            {
                ServiceModelSecurityTokenRequirement requirement = tokenRequirement as ServiceModelSecurityTokenRequirement;
                outOfBandTokenResolver = null;
                if (requirement != null)
                {
                    if (IsX509TokenRequirement(requirement))
                    {
                        if (mode == PeerAuthenticationMode.Password && IsForConnectionValidator(requirement))
                        {
                            return new X509SecurityTokenAuthenticator(X509CertificateValidator.None);
                        }
                        if (delegateManager != null)
                        {
                            if (IsForConnectionValidator(requirement))
                            {
                                requirement.TransportScheme = PeerStrings.Scheme;
                                requirement.Properties[SecurityTokenRequirement.PeerAuthenticationMode] = SecurityMode.Transport;
                            }
                            else
                            {
                                requirement.TransportScheme = PeerStrings.Scheme;
                                requirement.Properties[SecurityTokenRequirement.PeerAuthenticationMode] = SecurityMode.Message;
                            }
                            return delegateManager.CreateSecurityTokenAuthenticator(tokenRequirement, out outOfBandTokenResolver);
                        }
                        else
                        {
                            X509CertificateValidator validator = null;
                            if (IsForConnectionValidator(requirement))
                            {
                                if (this.mode == PeerAuthenticationMode.MutualCertificate)
                                {
                                    if (!this.credential.PeerAuthentication.TryGetCertificateValidator(out validator))
                                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SecurityTokenManagerCannotCreateProviderForRequirement, requirement)));
                                }
                                else
                                    validator = X509CertificateValidator.None;
                            }
                            else
                            {
                                if (!this.credential.MessageSenderAuthentication.TryGetCertificateValidator(out validator))
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.SecurityTokenManagerCannotCreateProviderForRequirement, requirement)));

                            }
                            return new X509SecurityTokenAuthenticator(validator);
                        }
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("tokenRequirement");
                    }
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenRequirement");
                }
            }

            public override bool Equals(object other)
            {
                PeerClientSecurityTokenManager that = other as PeerClientSecurityTokenManager;
                if (that == null)
                    return false;
                if (this.credential != null)
                {
                    if (that.credential == null || !this.credential.Equals(that.credential, this.mode, this.messageAuth))
                        return false;
                    return true;
                }
                else
                {
                    return this.delegateManager.Equals(that.delegateManager);
                }
            }

            internal bool HasCompatibleMessageSecuritySettings(PeerClientSecurityTokenManager that)
            {
                if (this.credential != null)
                    return (that.credential != null && this.credential.Equals(that.credential));
                else
                    return this.delegateManager.Equals(that.delegateManager);
            }

            public override int GetHashCode()
            {
                if (credential != null)
                    return credential.GetHashCode();
                else if (delegateManager != null)
                    return delegateManager.GetHashCode();
                else
                    return 0;
            }

        }
    }
}
