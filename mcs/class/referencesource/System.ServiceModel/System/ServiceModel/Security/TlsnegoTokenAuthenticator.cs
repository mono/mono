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
    using System.Security.Principal;
    using System.ServiceModel;
    using System.Xml;
    using SchProtocols = System.IdentityModel.SchProtocols;

    sealed class TlsnegoTokenAuthenticator : SspiNegotiationTokenAuthenticator
    {
        SecurityTokenAuthenticator clientTokenAuthenticator;
        SecurityTokenProvider serverTokenProvider;
        X509SecurityToken serverToken;
        bool mapCertificateToWindowsAccount;

        public TlsnegoTokenAuthenticator()
            : base()
        {
            // empty
        }

        public SecurityTokenAuthenticator ClientTokenAuthenticator
        {
            get
            {
                return this.clientTokenAuthenticator;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.clientTokenAuthenticator = value;
            }
        }

        public SecurityTokenProvider ServerTokenProvider
        {
            get
            {
                return this.serverTokenProvider;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.serverTokenProvider = value;
            }
        }

        public bool MapCertificateToWindowsAccount
        {
            get
            {
                return this.mapCertificateToWindowsAccount;
            }
            set
            {
                this.CommunicationObject.ThrowIfDisposedOrImmutable();
                this.mapCertificateToWindowsAccount = value;
            }
        }

        X509SecurityToken ValidateX509Token(SecurityToken token)
        {
            X509SecurityToken result = token as X509SecurityToken;
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.TokenProviderReturnedBadToken, token == null ? "<null>" : token.GetType().ToString())));
            }
            SecurityUtils.EnsureCertificateCanDoKeyExchange(result.Certificate);
            return result;
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

        public override void OnOpen(TimeSpan timeout)
        {
            if (this.serverTokenProvider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.NoServerX509TokenProvider)));
            }
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            SecurityUtils.OpenTokenProviderIfRequired(this.serverTokenProvider, timeoutHelper.RemainingTime());
            if (this.clientTokenAuthenticator != null)
            {
                SecurityUtils.OpenTokenAuthenticatorIfRequired(this.clientTokenAuthenticator, timeoutHelper.RemainingTime());
            }
            SecurityToken token = this.serverTokenProvider.GetToken(timeoutHelper.RemainingTime());
            this.serverToken = ValidateX509Token(token);
            base.OnOpen(timeoutHelper.RemainingTime());
        }

        public override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            if (this.serverTokenProvider != null)
            {
                SecurityUtils.CloseTokenProviderIfRequired(this.serverTokenProvider, timeoutHelper.RemainingTime());
                this.serverTokenProvider = null;
            }
            if (this.clientTokenAuthenticator != null)
            {
                SecurityUtils.CloseTokenAuthenticatorIfRequired(this.clientTokenAuthenticator, timeoutHelper.RemainingTime());
                this.clientTokenAuthenticator = null;
            }
            if (this.serverToken != null)
            {
                this.serverToken = null;
            }
            base.OnClose(timeoutHelper.RemainingTime());
        }

        public override void OnAbort()
        {
            if (this.serverTokenProvider != null)
            {
                SecurityUtils.AbortTokenProviderIfRequired(this.serverTokenProvider);
                this.serverTokenProvider = null;
            }
            if (this.clientTokenAuthenticator != null)
            {
                SecurityUtils.AbortTokenAuthenticatorIfRequired(this.clientTokenAuthenticator);
                this.clientTokenAuthenticator = null;
            }
            if (this.serverToken != null)
            {
                this.serverToken = null;
            }
            base.OnAbort();
        }

        protected override void ValidateIncomingBinaryNegotiation(BinaryNegotiation incomingNego)
        {
            // Accept both strings for WSTrustFeb2005
            if (incomingNego != null &&
                incomingNego.ValueTypeUri != this.NegotiationValueType.Value &&
                this.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005)
            {
                incomingNego.Validate(DXD.TrustDec2005Dictionary.TlsnegoValueTypeUri);
            }
            else
            {
                base.ValidateIncomingBinaryNegotiation(incomingNego);
            }
        }

        protected override SspiNegotiationTokenAuthenticatorState CreateSspiState(byte[] incomingBlob, string incomingValueTypeUri)
        {
            TlsSspiNegotiation tlsNegotiation = new TlsSspiNegotiation(SchProtocols.TlsServer | SchProtocols.Ssl3Server,
                this.serverToken.Certificate, this.ClientTokenAuthenticator != null);
            // Echo only for TrustFeb2005 and ValueType mismatch
            if (this.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005 && 
                this.NegotiationValueType.Value != incomingValueTypeUri)
            {
                tlsNegotiation.IncomingValueTypeUri = incomingValueTypeUri;
            }
            return new SspiNegotiationTokenAuthenticatorState(tlsNegotiation);
        }

        protected override BinaryNegotiation GetOutgoingBinaryNegotiation(ISspiNegotiation sspiNegotiation, byte[] outgoingBlob)
        {
            TlsSspiNegotiation tlsNegotiation = sspiNegotiation as TlsSspiNegotiation;
            // Echo only for TrustFeb2005 and ValueType mismatch
            if (this.StandardsManager.MessageSecurityVersion.TrustVersion == TrustVersion.WSTrustFeb2005 &&
                tlsNegotiation != null &&
                tlsNegotiation.IncomingValueTypeUri != null)
            {
                return new BinaryNegotiation(tlsNegotiation.IncomingValueTypeUri, outgoingBlob);
            }
            else
            {
                return base.GetOutgoingBinaryNegotiation(sspiNegotiation, outgoingBlob);
            }
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateSspiNegotiation(ISspiNegotiation sspiNegotiation)
        {
            TlsSspiNegotiation tlsNegotiation = (TlsSspiNegotiation)sspiNegotiation;
            if (tlsNegotiation.IsValidContext == false)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityNegotiationException(SR.GetString(SR.InvalidSspiNegotiation)));
            }

            if (this.ClientTokenAuthenticator == null)
            {
                return EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
            }

            X509Certificate2 clientCertificate = tlsNegotiation.RemoteCertificate;
            if (clientCertificate == null)
            {
                // isAnonymous is false. So, fail the negotiation
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityTokenValidationException(SR.GetString(SR.ClientCertificateNotProvided)));
            }

            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies;
            if (this.ClientTokenAuthenticator != null)
            {
                X509SecurityToken clientToken;
                WindowsIdentity preMappedIdentity;
                if (!this.MapCertificateToWindowsAccount || !tlsNegotiation.TryGetContextIdentity(out preMappedIdentity))
                {
                    clientToken = new X509SecurityToken(clientCertificate);
                }
                else
                {
                    clientToken = new X509WindowsSecurityToken(clientCertificate, preMappedIdentity, preMappedIdentity.AuthenticationType, true);
                    preMappedIdentity.Dispose();
                }
                authorizationPolicies = this.ClientTokenAuthenticator.ValidateToken(clientToken);
                clientToken.Dispose();
            }
            else
            {
                authorizationPolicies = EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
            }
            return authorizationPolicies;
        }
    }
}
