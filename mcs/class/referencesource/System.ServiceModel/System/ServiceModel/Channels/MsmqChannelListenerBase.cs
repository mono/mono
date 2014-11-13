//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using SR = System.ServiceModel.SR;

    abstract class MsmqChannelListenerBase : TransportChannelListener
    {
        MsmqReceiveParameters receiveParameters;

        protected MsmqChannelListenerBase(MsmqBindingElementBase bindingElement,
                                          BindingContext context,
                                          MsmqReceiveParameters receiveParameters,
                                          MessageEncoderFactory messageEncoderFactory)
            : base(bindingElement, context, messageEncoderFactory)
        {
            this.receiveParameters = receiveParameters;
        }

        internal MsmqReceiveParameters ReceiveParameters
        {
            get { return this.receiveParameters; }
        }

        internal Exception NormalizePoisonException(long lookupId, Exception innerException)
        {
            if (this.ReceiveParameters.ExactlyOnce)
                return DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqPoisonMessageException(lookupId, innerException));
            else if (null != innerException)
                return DiagnosticUtility.ExceptionUtility.ThrowHelperError(innerException);
            else
            {
                throw Fx.AssertAndThrow("System.ServiceModel.Channels.MsmqChannelListenerBase.NormalizePoisonException(): (innerException == null)");
            }
        }

        internal void FaultListener()
        {
            this.Fault();
        }
    }

    abstract class MsmqChannelListenerBase<TChannel>
        : MsmqChannelListenerBase, IChannelListener<TChannel>
    where TChannel : class, IChannel
    {
        SecurityTokenAuthenticator x509SecurityTokenAuthenticator;

        protected MsmqChannelListenerBase(MsmqBindingElementBase bindingElement,
                                          BindingContext context,
                                          MsmqReceiveParameters receiveParameters,
                                          MessageEncoderFactory messageEncoderFactory)
            : base(bindingElement, context, receiveParameters, messageEncoderFactory)
        { }

        public override string Scheme
        {
            get { return "net.msmq"; }
        }

        internal override UriPrefixTable<ITransportManagerRegistration> TransportManagerTable
        {
            get { return Msmq.StaticTransportManagerTable; }
        }

        internal override ITransportManagerRegistration CreateTransportManagerRegistration(Uri listenUri)
        {
            return null;
        }

        protected virtual void OnCloseCore(bool isAborting)
        { }

        protected virtual void OnOpenCore(TimeSpan timeout)
        {
            if (MsmqAuthenticationMode.Certificate == this.ReceiveParameters.TransportSecurity.MsmqAuthenticationMode)
                SecurityUtils.OpenTokenAuthenticatorIfRequired(this.x509SecurityTokenAuthenticator, timeout);
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            OnCloseCore(false);
            return base.OnBeginClose(timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            OnCloseCore(false);
            base.OnClose(timeout);
        }

        protected override void OnAbort()
        {
            OnCloseCore(true);
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            IAsyncResult result = base.OnBeginOpen(timeoutHelper.RemainingTime(), callback, state);
            OnOpenCore(timeoutHelper.RemainingTime());
            return result;
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
            base.OnOpen(timeoutHelper.RemainingTime());
            OnOpenCore(timeoutHelper.RemainingTime());
        }


        internal override IList<TransportManager> SelectTransportManagers()
        {
            lock (this.TransportManagerTable)
            {
                // Look up an existing transport manager registration. We use registration only
                // for WebHosted case.
                ITransportManagerRegistration registration;
                if (this.TransportManagerTable.TryLookupUri(this.Uri, TransportDefaults.HostNameComparisonMode, out registration))
                {
                    // no need to use TransportManagerContainer because we never use the transport manager from channels
                    // Use the registration to select a set of compatible transport managers.
                    IList<TransportManager> foundTransportManagers = registration.Select(this);
                    if (foundTransportManagers != null)
                    {
                        for (int i = 0; i < foundTransportManagers.Count; i++)
                        {
                            foundTransportManagers[i].Open(this);
                        }
                    }
                }
            }

            return null;
        }

        protected void SetSecurityTokenAuthenticator(string scheme, BindingContext context)
        {
            if (this.ReceiveParameters.TransportSecurity.MsmqAuthenticationMode == MsmqAuthenticationMode.Certificate)
            {
                SecurityCredentialsManager credentials = context.BindingParameters.Find<SecurityCredentialsManager>();
                if (credentials == null)
                {
                    credentials = ServiceCredentials.CreateDefaultCredentials();
                }
                SecurityTokenManager tokenManager = credentials.CreateSecurityTokenManager();
                RecipientServiceModelSecurityTokenRequirement x509Requirement = new RecipientServiceModelSecurityTokenRequirement();
                x509Requirement.TokenType = SecurityTokenTypes.X509Certificate;
                x509Requirement.TransportScheme = scheme;
                x509Requirement.ListenUri = this.Uri;
                x509Requirement.KeyUsage = SecurityKeyUsage.Signature;
                SecurityTokenResolver dummy;
                this.x509SecurityTokenAuthenticator = tokenManager.CreateSecurityTokenAuthenticator(x509Requirement, out dummy);
            }
        }

        internal SecurityMessageProperty ValidateSecurity(MsmqInputMessage msmqMessage)
        {
            SecurityMessageProperty result = null;
            X509Certificate2 certificate = null;
            WindowsSidIdentity wsid = null;
            try
            {
                if (MsmqAuthenticationMode.Certificate == this.ReceiveParameters.TransportSecurity.MsmqAuthenticationMode)
                {
                    try
                    {
                        certificate = new X509Certificate2(msmqMessage.SenderCertificate.GetBufferCopy(msmqMessage.SenderCertificateLength.Value));
                        X509SecurityToken token = new X509SecurityToken(certificate, false);
                        ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = this.x509SecurityTokenAuthenticator.ValidateToken(token);
                        SecurityMessageProperty security = new SecurityMessageProperty();
                        security.TransportToken = new SecurityTokenSpecification(token, authorizationPolicies);
                        security.ServiceSecurityContext = new ServiceSecurityContext(authorizationPolicies);
                        result = security;
                    }
                    catch (SecurityTokenValidationException ex)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(SR.GetString(SR.MsmqBadCertificate), ex));
                    }
                    catch (System.Security.Cryptography.CryptographicException ex)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(SR.GetString(SR.MsmqBadCertificate), ex));
                    }
                }
                else if (MsmqAuthenticationMode.WindowsDomain == this.ReceiveParameters.TransportSecurity.MsmqAuthenticationMode)
                {
                    byte[] sid = msmqMessage.SenderId.GetBufferCopy(msmqMessage.SenderIdLength.Value);
                    if (0 == sid.Length)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ProtocolException(SR.GetString(SR.MsmqNoSid)));

                    SecurityIdentifier securityIdentifier = new SecurityIdentifier(sid, 0);
                    List<Claim> claims = new List<Claim>(2);
                    claims.Add(new Claim(ClaimTypes.Sid, securityIdentifier, Rights.Identity));
                    claims.Add(Claim.CreateWindowsSidClaim(securityIdentifier));

                    ClaimSet claimSet = new DefaultClaimSet(ClaimSet.System, claims);
                    List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>(1);
                    wsid = new WindowsSidIdentity(securityIdentifier);
                    policies.Add(new UnconditionalPolicy(wsid, claimSet));

                    ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = policies.AsReadOnly();
                    SecurityMessageProperty security = new SecurityMessageProperty();
                    security.TransportToken = new SecurityTokenSpecification(null, authorizationPolicies);
                    security.ServiceSecurityContext = new ServiceSecurityContext(authorizationPolicies);
                    result = security;
                }
            }
#pragma warning suppress 56500 // covered by FXCop
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                    throw;

                // Audit Authentication failure
                if (AuditLevel.Failure == (this.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Failure))
                    WriteAuditEvent(AuditLevel.Failure, certificate, wsid, null);

                throw;
            }

            // Audit Authentication success
            if (result != null && AuditLevel.Success == (this.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Success))
                WriteAuditEvent(AuditLevel.Success, certificate, wsid, null);

            return result;
        }

        void WriteAuditEvent(AuditLevel auditLevel, X509Certificate2 certificate, WindowsSidIdentity wsid, Exception exception)
        {
            try
            {
                String primaryIdentity = String.Empty;
                if (certificate != null)
                {
                    primaryIdentity = SecurityUtils.GetCertificateId(certificate);
                }
                else if (wsid != null)
                {
                    primaryIdentity = SecurityUtils.GetIdentityName(wsid);
                }

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

        public abstract TChannel AcceptChannel();
        public abstract IAsyncResult BeginAcceptChannel(AsyncCallback callback, object state);
        public abstract TChannel AcceptChannel(TimeSpan timeout);
        public abstract IAsyncResult BeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state);
        public abstract TChannel EndAcceptChannel(IAsyncResult result);
    }
}
