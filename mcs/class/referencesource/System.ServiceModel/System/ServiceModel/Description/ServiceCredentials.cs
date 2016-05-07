//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Configuration;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.ServiceModel.Dispatcher;

    public class ServiceCredentials : SecurityCredentialsManager, IServiceBehavior
    {
        UserNamePasswordServiceCredential userName;
        X509CertificateInitiatorServiceCredential clientCertificate;
        X509CertificateRecipientServiceCredential serviceCertificate;
        WindowsServiceCredential windows;
        IssuedTokenServiceCredential issuedToken;
        PeerCredential peer;
        SecureConversationServiceCredential secureConversation;
        bool useIdentityConfiguration = false;

        bool isReadOnly = false;
        bool saveBootstrapTokenInSession = true;

        IdentityConfiguration identityConfiguration;
        ExceptionMapper exceptionMapper;

        public ServiceCredentials()
        {
            this.userName = new UserNamePasswordServiceCredential();
            this.clientCertificate = new X509CertificateInitiatorServiceCredential();
            this.serviceCertificate = new X509CertificateRecipientServiceCredential();
            this.windows = new WindowsServiceCredential();
            this.issuedToken = new IssuedTokenServiceCredential();
            this.peer = new PeerCredential();
            this.secureConversation = new SecureConversationServiceCredential();
            this.exceptionMapper = new ExceptionMapper();
            this.UseIdentityConfiguration = false;
        }

        protected ServiceCredentials(ServiceCredentials other)
        {
            if (other == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("other");
            }
            this.userName = new UserNamePasswordServiceCredential(other.userName);
            this.clientCertificate = new X509CertificateInitiatorServiceCredential(other.clientCertificate);
            this.serviceCertificate = new X509CertificateRecipientServiceCredential(other.serviceCertificate);
            this.windows = new WindowsServiceCredential(other.windows);
            this.issuedToken = new IssuedTokenServiceCredential(other.issuedToken);
            this.peer = new PeerCredential(other.peer);
            this.secureConversation = new SecureConversationServiceCredential(other.secureConversation);
            this.identityConfiguration = other.identityConfiguration;
            this.saveBootstrapTokenInSession = other.saveBootstrapTokenInSession;
            this.exceptionMapper = other.exceptionMapper;
            this.UseIdentityConfiguration = other.useIdentityConfiguration;
        }

        public UserNamePasswordServiceCredential UserNameAuthentication
        {
            get
            {
                return this.userName;
            }
        }

        public X509CertificateInitiatorServiceCredential ClientCertificate
        {
            get
            {
                return this.clientCertificate;
            }
        }

        public X509CertificateRecipientServiceCredential ServiceCertificate
        {
            get
            {
                return this.serviceCertificate;
            }
        }

        public WindowsServiceCredential WindowsAuthentication
        {
            get
            {
                return this.windows;
            }
        }

        public IssuedTokenServiceCredential IssuedTokenAuthentication
        {
            get
            {
                return this.issuedToken;
            }
        }

        public PeerCredential Peer
        {
            get
            {
                return this.peer;
            }
        }

        public SecureConversationServiceCredential SecureConversationAuthentication
        {
            get
            {
                return this.secureConversation;
            }
        }

        /// <summary>
        /// Gets or sets the ExceptionMapper to be used when throwing exceptions.
        /// </summary>
        public ExceptionMapper ExceptionMapper
        {
            get
            {
                return this.exceptionMapper;
            }
            set
            {
                ThrowIfImmutable();
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.exceptionMapper = value;
            }
        }

        public IdentityConfiguration IdentityConfiguration
        {
            get
            {
                return this.identityConfiguration;
            }
            set
            {
                ThrowIfImmutable();
                this.identityConfiguration = value;
            }
        }

        public bool UseIdentityConfiguration
        {
            get
            {
                return this.useIdentityConfiguration;
            }
            set
            {
                ThrowIfImmutable();
                this.useIdentityConfiguration = value;

                if (this.identityConfiguration == null && this.useIdentityConfiguration)
                {
                    this.identityConfiguration = new IdentityConfiguration();
                }
            }
        }

        internal static ServiceCredentials CreateDefaultCredentials()
        {
            return new ServiceCredentials();
        }

        public override SecurityTokenManager CreateSecurityTokenManager()
        {
            if (this.useIdentityConfiguration)
            {
                //
                // Note: the token manager we create here is always a wrapper over the default collection of token handlers
                //
                return new FederatedSecurityTokenManager(this.Clone());
            }
            else
            {
                return new ServiceCredentialsSecurityTokenManager(this.Clone());
            }
        }

        protected virtual ServiceCredentials CloneCore()
        {
            return new ServiceCredentials(this);
        }

        public ServiceCredentials Clone()
        {
            ServiceCredentials result = CloneCore();
            if (result == null || result.GetType() != this.GetType())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(SR.GetString(SR.CloneNotImplementedCorrectly, this.GetType(), (result != null) ? result.ToString() : "null")));
            }
            return result;
        }

        void IServiceBehavior.Validate(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            //
            // Only pass a name if there was a name explicitly given to this class, otherwise ServiceConfig will require 
            // a config section with the default configuration.
            //
            if (this.UseIdentityConfiguration)
            {
                ConfigureServiceHost(serviceHostBase);
            }
        }

        /// <summary>
        /// Helper method that Initializes the SecurityTokenManager used by the ServiceHost. 
        /// By default the method sets the SecurityTokenHandlers initialized with IdentityConfiguration on the ServiceHost.
        /// </summary>
        /// <param name="serviceHost">ServiceHost instance to configure with FederatedSecurityTokenManager.</param>
        /// <exception cref="ArgumentNullException">One of the input argument is null.</exception>
        void ConfigureServiceHost(ServiceHostBase serviceHost)
        {
            if (serviceHost == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceHost");
            }

            // Throw if the serviceHost is in a bad state to do the configuration
            if (!(serviceHost.State == CommunicationState.Created || serviceHost.State == CommunicationState.Opening))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4041, serviceHost));
            }

#pragma warning suppress 56506

            if (this.ServiceCertificate != null)
            {
                X509Certificate2 serverCert = this.ServiceCertificate.Certificate;
                if (serverCert != null)
                {
                    this.IdentityConfiguration.ServiceCertificate = serverCert;
                }
            }

            if (this.IssuedTokenAuthentication != null && this.IssuedTokenAuthentication.KnownCertificates != null && this.IssuedTokenAuthentication.KnownCertificates.Count > 0)
            {
                this.IdentityConfiguration.KnownIssuerCertificates = new List<X509Certificate2> (this.IssuedTokenAuthentication.KnownCertificates);
            }

            //
            // Initialize the service configuration
            //
            if (!this.IdentityConfiguration.IsInitialized)
            {
                this.IdentityConfiguration.Initialize();
            }

            // 

#pragma warning suppress 56506 // serviceHost.Authorization is never null.
            if (serviceHost.Authorization.ServiceAuthorizationManager == null)
            {
                serviceHost.Authorization.ServiceAuthorizationManager = new IdentityModelServiceAuthorizationManager();
            }
            else if (!(serviceHost.Authorization.ServiceAuthorizationManager is IdentityModelServiceAuthorizationManager))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ID4039)));
            }

            // If SecuritySessionTokenHandler is being used then null the WCF SecurityStateEncoder.
            if ((this.IdentityConfiguration.SecurityTokenHandlers[typeof(SecurityContextSecurityToken)] != null) &&
                (serviceHost.Credentials.SecureConversationAuthentication.SecurityStateEncoder == null))
            {
                serviceHost.Credentials.SecureConversationAuthentication.SecurityStateEncoder = new NoOpSecurityStateEncoder();
            }
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
            if (parameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }
            // throw if bindingParameters already has a SecurityCredentialsManager
            SecurityCredentialsManager otherCredentialsManager = parameters.Find<SecurityCredentialsManager>();
            if (otherCredentialsManager != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MultipleSecurityCredentialsManagersInServiceBindingParameters, otherCredentialsManager)));
            }
            parameters.Add(this);
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            for (int i = 0; i < serviceHostBase.ChannelDispatchers.Count; i++)
            {
                ChannelDispatcher channelDispatcher = serviceHostBase.ChannelDispatchers[i] as ChannelDispatcher;
                if (channelDispatcher != null && !ServiceMetadataBehavior.IsHttpGetMetadataDispatcher(description, channelDispatcher))
                {
                    foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
                    {
                        DispatchRuntime behavior = endpointDispatcher.DispatchRuntime;
                        behavior.RequireClaimsPrincipalOnOperationContext = this.useIdentityConfiguration;
                    }
                }
            }
        }

        internal void MakeReadOnly()
        {
            this.isReadOnly = true;
            this.ClientCertificate.MakeReadOnly();
            this.IssuedTokenAuthentication.MakeReadOnly();
            this.Peer.MakeReadOnly();
            this.SecureConversationAuthentication.MakeReadOnly();
            this.ServiceCertificate.MakeReadOnly();
            this.UserNameAuthentication.MakeReadOnly();
            this.WindowsAuthentication.MakeReadOnly();
        }

        void ThrowIfImmutable()
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
            }
        }
    }
}
