//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.Xml;
    using SchProtocols = System.IdentityModel.SchProtocols;

    class TlsnegoTokenProvider : SspiNegotiationTokenProvider
    {
        SecurityTokenAuthenticator serverTokenAuthenticator;
        SecurityTokenProvider clientTokenProvider;

        public TlsnegoTokenProvider()
            : base()
        {
            // empty
        }

        public SecurityTokenAuthenticator ServerTokenAuthenticator
        {
            get
            {
                return this.serverTokenAuthenticator;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.serverTokenAuthenticator = value;
            }
        }

        public SecurityTokenProvider ClientTokenProvider
        {
            get
            {
                return this.clientTokenProvider;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.clientTokenProvider = value;
            }
        }

        // helpers
        static X509SecurityToken ValidateToken(SecurityToken token)
        {
            X509SecurityToken result = token as X509SecurityToken;
            if (result == null && token != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.TokenProviderReturnedBadToken, token.GetType().ToString())));
            }
            return result;
        }

        SspiNegotiationTokenProviderState CreateTlsSspiState(X509SecurityToken token)
        {
            X509Certificate2 clientCertificate;
            if (token == null)
            {
                clientCertificate = null;
            }
            else 
            {
                clientCertificate = token.Certificate;
            }
            TlsSspiNegotiation tlsNegotiation = null;
            if(LocalAppContextSwitches.DisableUsingServicePointManagerSecurityProtocols)
            {
                tlsNegotiation = new TlsSspiNegotiation(String.Empty, SchProtocols.Ssl3Client | SchProtocols.TlsClient, clientCertificate);
            }
            else
            {
                var protocol = (SchProtocols)System.Net.ServicePointManager.SecurityProtocol & SchProtocols.ClientMask;
                tlsNegotiation = new TlsSspiNegotiation(String.Empty, protocol, clientCertificate);
            }
            return new SspiNegotiationTokenProviderState(tlsNegotiation);
        }

        // overrides
        public override XmlDictionaryString NegotiationValueType
        {
            get 
            {
                if (this.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005)
                {
                    return XD.TrustApr2004Dictionary.TlsnegoValueTypeUri;
                }
                else if (this.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrust13)
                {
                    return DXD.TrustDec2005Dictionary.TlsnegoValueTypeUri;
                }
                // Not supported
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());
            }
        }

        protected override bool CreateNegotiationStateCompletesSynchronously(EndpointAddress target, Uri via)
        {
            if (this.ClientTokenProvider == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override IAsyncResult BeginCreateNegotiationState(EndpointAddress target, Uri via, TimeSpan timeout, AsyncCallback callback, object state)
        {
            EnsureEndpointAddressDoesNotRequireEncryption(target);
            if (this.ClientTokenProvider == null)
            {
                return new CompletedAsyncResult<SspiNegotiationTokenProviderState>(CreateTlsSspiState(null), callback, state);
            }
            else
            {
                return new CreateSspiStateAsyncResult(target, via, this, timeout, callback, state);
            }
        }

        protected override SspiNegotiationTokenProviderState EndCreateNegotiationState(IAsyncResult result)
        {
            if (result is CompletedAsyncResult<SspiNegotiationTokenProviderState>)
            {
                return CompletedAsyncResult<SspiNegotiationTokenProviderState>.End(result);
            }
            else
            {
                return CreateSspiStateAsyncResult.End(result);
            }
        }

        protected override SspiNegotiationTokenProviderState CreateNegotiationState(EndpointAddress target, Uri via, TimeSpan timeout)
        {
            EnsureEndpointAddressDoesNotRequireEncryption(target);
            X509SecurityToken clientToken;
            if (this.ClientTokenProvider == null)
            {
                clientToken = null;
            }
            else
            {
                SecurityToken token = this.ClientTokenProvider.GetToken(timeout);
                clientToken = ValidateToken(token);
            }
            return CreateTlsSspiState(clientToken);
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateSspiNegotiation(ISspiNegotiation sspiNegotiation)
        {
            TlsSspiNegotiation tlsNegotiation = (TlsSspiNegotiation)sspiNegotiation;
            if (tlsNegotiation.IsValidContext == false)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.InvalidSspiNegotiation)));
            }
            X509Certificate2 serverCert = tlsNegotiation.RemoteCertificate;
            if (serverCert == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityNegotiationException(SR.GetString(SR.ServerCertificateNotProvided)));
            }
            ReadOnlyCollection<IAuthorizationPolicy> authzPolicies;
            if (this.ServerTokenAuthenticator != null)
            {
                X509SecurityToken certToken = new X509SecurityToken(serverCert, false);
                authzPolicies = this.ServerTokenAuthenticator.ValidateToken(certToken);
            }
            else
            {
                authzPolicies = EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
            }
            return authzPolicies;
        }

        public override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (this.ClientTokenProvider != null)
            {
                SecurityUtils.OpenTokenProviderIfRequired(this.ClientTokenProvider, timeoutHelper.RemainingTime());
            }
            if (this.ServerTokenAuthenticator != null)
            {
                SecurityUtils.OpenTokenAuthenticatorIfRequired(this.ServerTokenAuthenticator, timeoutHelper.RemainingTime());
            }
            base.OnOpen(timeoutHelper.RemainingTime());
        }

        public override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (this.clientTokenProvider != null)
            {
                SecurityUtils.CloseTokenProviderIfRequired(this.ClientTokenProvider, timeoutHelper.RemainingTime());
                this.clientTokenProvider = null;
            }
            if (this.serverTokenAuthenticator != null)
            {
                SecurityUtils.CloseTokenAuthenticatorIfRequired(this.ServerTokenAuthenticator, timeoutHelper.RemainingTime());
                this.serverTokenAuthenticator = null;
            }
            base.OnClose(timeoutHelper.RemainingTime());
        }

        public override void OnAbort()
        {
            if (this.clientTokenProvider != null)
            {
                SecurityUtils.AbortTokenProviderIfRequired(this.ClientTokenProvider);
                this.clientTokenProvider = null;
            }
            if (this.serverTokenAuthenticator != null)
            {
                SecurityUtils.AbortTokenAuthenticatorIfRequired(this.ServerTokenAuthenticator);
                this.serverTokenAuthenticator = null;
            }
            base.OnAbort();
        }

        class CreateSspiStateAsyncResult : AsyncResult
        {
            static readonly AsyncCallback getTokensCallback = Fx.ThunkCallback(new AsyncCallback(GetTokensCallback));
            TlsnegoTokenProvider tlsTokenProvider;
            SspiNegotiationTokenProviderState sspiState;

            public CreateSspiStateAsyncResult(EndpointAddress target, Uri via, TlsnegoTokenProvider tlsTokenProvider, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.tlsTokenProvider = tlsTokenProvider;
                IAsyncResult result = this.tlsTokenProvider.ClientTokenProvider.BeginGetToken(timeout, getTokensCallback, this);
                if (!result.CompletedSynchronously)
                {
                    return;
                }
                SecurityToken token = this.tlsTokenProvider.ClientTokenProvider.EndGetToken(result);
                X509SecurityToken clientToken = ValidateToken(token);
                this.sspiState = this.tlsTokenProvider.CreateTlsSspiState(clientToken);
                base.Complete(true);
            }

            static void GetTokensCallback(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }
                CreateSspiStateAsyncResult typedResult = (CreateSspiStateAsyncResult)result.AsyncState;
                try
                {
                    SecurityToken token = typedResult.tlsTokenProvider.ClientTokenProvider.EndGetToken(result);
                    X509SecurityToken clientToken = TlsnegoTokenProvider.ValidateToken(token);
                    typedResult.sspiState = typedResult.tlsTokenProvider.CreateTlsSspiState(clientToken);
                    typedResult.Complete(false);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e)) throw;

                    typedResult.Complete(false, e);
                }
            }

            public static SspiNegotiationTokenProviderState End(IAsyncResult result)
            {
                CreateSspiStateAsyncResult asyncResult = AsyncResult.End<CreateSspiStateAsyncResult>(result);
                return asyncResult.sspiState;
            }
        }
    }
}
