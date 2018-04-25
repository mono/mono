//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System.Diagnostics.CodeAnalysis;
    using System.IdentityModel.Selectors;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;
    using System.IdentityModel.Tokens;

    public class ClientCredentials : SecurityCredentialsManager, IEndpointBehavior
    {
        internal const bool SupportInteractiveDefault = true;

        UserNamePasswordClientCredential userName;
        X509CertificateInitiatorClientCredential clientCertificate;
        X509CertificateRecipientClientCredential serviceCertificate;
        WindowsClientCredential windows;
        HttpDigestClientCredential httpDigest;
        IssuedTokenClientCredential issuedToken;
        PeerCredential peer;
        bool supportInteractive;
        bool isReadOnly;
        GetInfoCardTokenCallback getInfoCardTokenCallback = null;
        bool useIdentityConfiguration = false;
        SecurityTokenHandlerCollectionManager securityTokenHandlerCollectionManager = null;
        object handlerCollectionLock = new object();

        public ClientCredentials()
        {
            this.supportInteractive = SupportInteractiveDefault;
        }

        protected ClientCredentials(ClientCredentials other)
        {
            if (other == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("other");
            if (other.userName != null)
                this.userName = new UserNamePasswordClientCredential(other.userName);
            if (other.clientCertificate != null)
                this.clientCertificate = new X509CertificateInitiatorClientCredential(other.clientCertificate);
            if (other.serviceCertificate != null)
                this.serviceCertificate = new X509CertificateRecipientClientCredential(other.serviceCertificate);
            if (other.windows != null)
                this.windows = new WindowsClientCredential(other.windows);
            if (other.httpDigest != null)
                this.httpDigest = new HttpDigestClientCredential(other.httpDigest);
            if (other.issuedToken != null)
                this.issuedToken = new IssuedTokenClientCredential(other.issuedToken);
            if (other.peer != null)
                this.peer = new PeerCredential(other.peer);

            this.getInfoCardTokenCallback = other.getInfoCardTokenCallback;
            this.supportInteractive = other.supportInteractive;
            this.securityTokenHandlerCollectionManager = other.securityTokenHandlerCollectionManager;
            this.useIdentityConfiguration = other.useIdentityConfiguration;
            this.isReadOnly = other.isReadOnly;
        }

        internal GetInfoCardTokenCallback GetInfoCardTokenCallback
        {
            get
            {
                if (this.getInfoCardTokenCallback == null)
                {
                    GetInfoCardTokenCallback gtc = new GetInfoCardTokenCallback(this.GetInfoCardSecurityToken);
                    this.getInfoCardTokenCallback = gtc;
                }
                return this.getInfoCardTokenCallback;
            }
        }

        public IssuedTokenClientCredential IssuedToken
        {
            get
            {
                if (this.issuedToken == null)
                {
                    this.issuedToken = new IssuedTokenClientCredential();
                    if (isReadOnly)
                        this.issuedToken.MakeReadOnly();
                }
                return this.issuedToken;
            }
        }

        public UserNamePasswordClientCredential UserName
        {
            get
            {
                if (this.userName == null)
                {
                    this.userName = new UserNamePasswordClientCredential();
                    if (isReadOnly)
                        this.userName.MakeReadOnly();
                }
                return this.userName;
            }
        }

        public X509CertificateInitiatorClientCredential ClientCertificate
        {
            get
            {
                if (this.clientCertificate == null)
                {
                    this.clientCertificate = new X509CertificateInitiatorClientCredential();
                    if (isReadOnly)
                        this.clientCertificate.MakeReadOnly();
                }
                return this.clientCertificate;
            }
        }

        public X509CertificateRecipientClientCredential ServiceCertificate
        {
            get
            {
                if (this.serviceCertificate == null)
                {
                    this.serviceCertificate = new X509CertificateRecipientClientCredential();
                    if (isReadOnly)
                        this.serviceCertificate.MakeReadOnly();
                }
                return this.serviceCertificate;
            }
        }

        public WindowsClientCredential Windows
        {
            get
            {
                if (this.windows == null)
                {
                    this.windows = new WindowsClientCredential();
                    if (isReadOnly)
                        this.windows.MakeReadOnly();
                }
                return this.windows;
            }
        }

        public HttpDigestClientCredential HttpDigest
        {
            get
            {
                if (this.httpDigest == null)
                {
                    this.httpDigest = new HttpDigestClientCredential();
                    if (isReadOnly)
                        this.httpDigest.MakeReadOnly();
                }
                return this.httpDigest;
            }
        }

        public PeerCredential Peer
        {
            get
            {
                if (this.peer == null)
                {
                    this.peer = new PeerCredential();
                    if (isReadOnly)
                        this.peer.MakeReadOnly();
                }
                return this.peer;
            }
        }

        /// <summary>
        /// The <see cref="SecurityTokenHandlerCollectionManager" /> containing the set of <see cref="SecurityTokenHandler" />
        /// objects used for serializing and validating tokens found in WS-Trust messages.
        /// </summary>
        public SecurityTokenHandlerCollectionManager SecurityTokenHandlerCollectionManager
        {
            get
            {
                if (this.securityTokenHandlerCollectionManager == null)
                {
                    lock (this.handlerCollectionLock)
                    {
                        if (this.securityTokenHandlerCollectionManager == null)
                        {
                            this.securityTokenHandlerCollectionManager = SecurityTokenHandlerCollectionManager.CreateDefaultSecurityTokenHandlerCollectionManager();
                        }
                    }
                }

                return this.securityTokenHandlerCollectionManager;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
                }

                this.securityTokenHandlerCollectionManager = value;
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
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
                }
                this.useIdentityConfiguration = value;
            }
        }

        public bool SupportInteractive
        {
            get
            {
                return this.supportInteractive;
            }
            set
            {
                if (this.isReadOnly)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
                }
                this.supportInteractive = value;
            }
        }

        internal static ClientCredentials CreateDefaultCredentials()
        {
            return new ClientCredentials();
        }

        public override SecurityTokenManager CreateSecurityTokenManager()
        {
            return new ClientCredentialsSecurityTokenManager(this.Clone());
        }

        protected virtual ClientCredentials CloneCore()
        {
            return new ClientCredentials(this);
        }

        public ClientCredentials Clone()
        {
            ClientCredentials result = CloneCore();
            if (result == null || result.GetType() != this.GetType())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException(SR.GetString(SR.CloneNotImplementedCorrectly, this.GetType(), (result != null) ? result.ToString() : "null")));
            }
            return result;
        }

        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection bindingParameters)
        {
            if (bindingParameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingParameters");
            }
            // throw if bindingParameters already has a SecurityCredentialsManager
            SecurityCredentialsManager otherCredentialsManager = bindingParameters.Find<SecurityCredentialsManager>();
            if (otherCredentialsManager != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.MultipleSecurityCredentialsManagersInChannelBindingParameters, otherCredentialsManager)));
            }
            bindingParameters.Add(this);
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                SR.GetString(SR.SFXEndpointBehaviorUsedOnWrongSide, typeof(ClientCredentials).Name)));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void AddInteractiveInitializers(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {
            CardSpacePolicyElement[] dummyPolicyElements;
            Uri dummyRelyingPartyIssuer;
            // we add the initializer only if infocard is required. At this point, serviceEndpoint.Address is not populated correctly but that's not needed to
            // determine whether infocard is required or not.
            if (InfoCardHelper.IsInfocardRequired(serviceEndpoint.Binding, this, this.CreateSecurityTokenManager(), EndpointAddress.AnonymousAddress, out dummyPolicyElements, out dummyRelyingPartyIssuer))
            {
                behavior.InteractiveChannelInitializers.Add(new InfocardInteractiveChannelInitializer(this, serviceEndpoint.Binding));
            }
        }

        public virtual void ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {

            if (serviceEndpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceEndpoint");
            }

            if (serviceEndpoint.Binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceEndpoint.Binding");
            }


            if (serviceEndpoint.Binding.CreateBindingElements().Find<SecurityBindingElement>() == null)
            {
                return;
            }

            try
            {
                AddInteractiveInitializers(serviceEndpoint, behavior);
            }
            catch (System.IO.FileNotFoundException)
            {

            }

        }

        // RC0 workaround to freeze credentials when the channel factory is opened
        internal void MakeReadOnly()
        {
            this.isReadOnly = true;
            if (this.clientCertificate != null)
                this.clientCertificate.MakeReadOnly();
            if (this.serviceCertificate != null)
                this.serviceCertificate.MakeReadOnly();
            if (this.userName != null)
                this.userName.MakeReadOnly();
            if (this.windows != null)
                this.windows.MakeReadOnly();
            if (this.httpDigest != null)
                this.httpDigest.MakeReadOnly();
            if (this.issuedToken != null)
                this.issuedToken.MakeReadOnly();
            if (this.peer != null)
                this.peer.MakeReadOnly();
        }

        // This APTCA method calls CardSpaceSelector.GetToken(..), which is defined in a non-APTCA assembly. It would be a breaking change to add a Demand, 
        // while we don't have an identified security vulnerability here.
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods)]
        internal protected virtual SecurityToken GetInfoCardSecurityToken(bool requiresInfoCard, CardSpacePolicyElement[] chain, SecurityTokenSerializer tokenSerializer)
        {
            if (!requiresInfoCard)
            {
                return null;
            }
            return CardSpaceSelector.GetToken(chain, tokenSerializer);
        }

    }
}
