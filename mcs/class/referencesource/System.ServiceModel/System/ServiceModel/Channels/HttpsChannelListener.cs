//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Net;
    using System.Runtime;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;

    class HttpsChannelListener<TChannel> : HttpChannelListener<TChannel>
                        where TChannel : class, IChannel
    {
        readonly bool useCustomClientCertificateVerification;
        readonly bool shouldValidateClientCertificate;
        bool useHostedClientCertificateMapping;
        bool requireClientCertificate;
        SecurityTokenAuthenticator certificateAuthenticator;
        const HttpStatusCode CertificateErrorStatusCode = HttpStatusCode.Forbidden;
        IChannelBindingProvider channelBindingProvider;

        public HttpsChannelListener(HttpsTransportBindingElement httpsBindingElement, BindingContext context)
            : base(httpsBindingElement, context)
        {
            this.requireClientCertificate = httpsBindingElement.RequireClientCertificate;
            this.shouldValidateClientCertificate = ShouldValidateClientCertificate(this.requireClientCertificate, context);

            // Pick up the MapCertificateToWindowsAccount setting from the configured token authenticator.
            SecurityCredentialsManager credentialProvider =
                context.BindingParameters.Find<SecurityCredentialsManager>();
            if (credentialProvider == null)
            {
                credentialProvider = ServiceCredentials.CreateDefaultCredentials();
            }
            SecurityTokenManager tokenManager = credentialProvider.CreateSecurityTokenManager();
            this.certificateAuthenticator =
                    TransportSecurityHelpers.GetCertificateTokenAuthenticator(tokenManager, context.Binding.Scheme,
                    TransportSecurityHelpers.GetListenUri(context.ListenUriBaseAddress, context.ListenUriRelativeAddress));


            ServiceCredentials serviceCredentials = credentialProvider as ServiceCredentials;

            if (serviceCredentials != null &&
                serviceCredentials.ClientCertificate.Authentication.CertificateValidationMode == X509CertificateValidationMode.Custom)
            {
                useCustomClientCertificateVerification = true;
            }
            else
            {
                useCustomClientCertificateVerification = false;

                X509SecurityTokenAuthenticator authenticator = this.certificateAuthenticator as X509SecurityTokenAuthenticator;

                if (authenticator != null)
                {
                    this.certificateAuthenticator = new X509SecurityTokenAuthenticator(X509CertificateValidator.None,
                        authenticator.MapCertificateToWindowsAccount, this.ExtractGroupsForWindowsAccounts, false);
                }
            }

            if (this.RequireClientCertificate &&
                this.AuthenticationScheme.IsNotSet(AuthenticationSchemes.Anonymous))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new InvalidOperationException(SR.GetString(
                    SR.HttpAuthSchemeAndClientCert, this.AuthenticationScheme)), TraceEventType.Error);
            }

            this.channelBindingProvider = new ChannelBindingProviderHelper();
        }

        public bool RequireClientCertificate
        {
            get
            {
                return this.requireClientCertificate;
            }
        }

        public override string Scheme
        {
            get
            {
                return Uri.UriSchemeHttps;
            }
        }

        public override bool IsChannelBindingSupportEnabled
        {
            get
            {
                return this.channelBindingProvider.IsChannelBindingSupportEnabled;
            }
        }

        internal override UriPrefixTable<ITransportManagerRegistration> TransportManagerTable
        {
            get
            {
                return SharedHttpsTransportManager.StaticTransportManagerTable;
            }
        }

        public override T GetProperty<T>()
        {
            if (typeof(T) == typeof(IChannelBindingProvider))
            {
                return (T)(object)this.channelBindingProvider;
            }

            return base.GetProperty<T>();
        }

        internal override void ApplyHostedContext(string virtualPath, bool isMetadataListener)
        {
            base.ApplyHostedContext(virtualPath, isMetadataListener);
            useHostedClientCertificateMapping = AspNetEnvironment.Current.ValidateHttpsSettings(virtualPath, ref this.requireClientCertificate);
        }

        internal override ITransportManagerRegistration CreateTransportManagerRegistration(Uri listenUri)
        {
            return new SharedHttpsTransportManager(listenUri, this);
        }

        // Note: the returned SecurityMessageProperty has ownership of certificate and identity.
        SecurityMessageProperty CreateSecurityProperty(X509Certificate2 certificate, WindowsIdentity identity, string authType)
        {
            SecurityToken token;
            if (identity != null)
            {
                token = new X509WindowsSecurityToken(certificate, identity, authType, false);
            }
            else
            {
                token = new X509SecurityToken(certificate, false);
            }

            ReadOnlyCollection<IAuthorizationPolicy> policies = this.certificateAuthenticator.ValidateToken(token);
            SecurityMessageProperty result = new SecurityMessageProperty();
            result.TransportToken = new SecurityTokenSpecification(token, policies);
            result.ServiceSecurityContext = new ServiceSecurityContext(policies);
            return result;
        }

        public override SecurityMessageProperty ProcessAuthentication(IHttpAuthenticationContext authenticationContext)
        {
            if (this.shouldValidateClientCertificate)
            {
                SecurityMessageProperty retValue;
                X509Certificate2 certificate = null;

                try
                {
                    bool isCertificateValid;
                    certificate = authenticationContext.GetClientCertificate(out isCertificateValid);
                    Fx.Assert(!this.requireClientCertificate || certificate != null, "ClientCertificate must be present");

                    if (certificate != null)
                    {
                        if (!this.useCustomClientCertificateVerification)
                        {
                            Fx.Assert(isCertificateValid, "ClientCertificate must be valid");
                        }

                        WindowsIdentity identity = null;
                        string authType = base.GetAuthType(authenticationContext);

                        if (this.useHostedClientCertificateMapping)
                        {
                            identity = authenticationContext.LogonUserIdentity;
                            if (identity == null || !identity.IsAuthenticated)
                            {
                                identity = WindowsIdentity.GetAnonymous();
                            }
                            else
                            {
                                // it is not recommended to call identity.AuthenticationType as this is a privileged instruction.
                                // when the identity is cloned, it will be created with an authtype indicating WindowsIdentity from a cert.
                                identity = SecurityUtils.CloneWindowsIdentityIfNecessary(identity, SecurityUtils.AuthTypeCertMap);
                                authType = SecurityUtils.AuthTypeCertMap;
                            }
                        }

                        retValue = CreateSecurityProperty(certificate, identity, authType);
                    }
                    else if (this.AuthenticationScheme == AuthenticationSchemes.Anonymous)
                    {
                        return new SecurityMessageProperty();
                    }
                    else
                    {
                        return base.ProcessAuthentication(authenticationContext);
                    }
                }
#pragma warning suppress 56500 // covered by FXCop
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                        throw;

                    // Audit Authentication failure
                    if (AuditLevel.Failure == (this.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Failure))
                        WriteAuditEvent(AuditLevel.Failure, (certificate != null) ? SecurityUtils.GetCertificateId(certificate) : String.Empty, exception);

                    throw;
                }

                // Audit Authentication success
                if (AuditLevel.Success == (this.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Success))
                    WriteAuditEvent(AuditLevel.Success, (certificate != null) ? SecurityUtils.GetCertificateId(certificate) : String.Empty, null);

                return retValue;
            }
            else if (this.AuthenticationScheme == AuthenticationSchemes.Anonymous)
            {
                return new SecurityMessageProperty();
            }
            else
            {
                return base.ProcessAuthentication(authenticationContext);
            }
        }

        public override SecurityMessageProperty ProcessAuthentication(HttpListenerContext listenerContext)
        {
            if (this.shouldValidateClientCertificate)
            {
                SecurityMessageProperty retValue;
                X509Certificate2 certificateEx = null;

                try
                {
                    X509Certificate certificate = listenerContext.Request.GetClientCertificate();
                    Fx.Assert(!this.requireClientCertificate || certificate != null,
                        "HttpListenerRequest.ClientCertificate is not present");

                    if (certificate != null)
                    {
                        if (!useCustomClientCertificateVerification)
                        {
                            Fx.Assert(listenerContext.Request.ClientCertificateError == 0,
                                "HttpListenerRequest.ClientCertificate is not valid");
                        }
                        certificateEx = new X509Certificate2(certificate);
                        retValue = CreateSecurityProperty(certificateEx, null, string.Empty);
                    }
                    else if (this.AuthenticationScheme == AuthenticationSchemes.Anonymous)
                    {
                        return new SecurityMessageProperty();
                    }
                    else
                    {
                        return base.ProcessAuthentication(listenerContext);
                    }
                }
#pragma warning suppress 56500 // covered by FXCop
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                        throw;

                    // Audit Authentication failure
                    if (AuditLevel.Failure == (this.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Failure))
                        WriteAuditEvent(AuditLevel.Failure, (certificateEx != null) ? SecurityUtils.GetCertificateId(certificateEx) : String.Empty, exception);

                    throw;
                }

                // Audit Authentication success
                if (AuditLevel.Success == (this.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Success))
                    WriteAuditEvent(AuditLevel.Success, (certificateEx != null) ? SecurityUtils.GetCertificateId(certificateEx) : String.Empty, null);

                return retValue;
            }
            else if (this.AuthenticationScheme == AuthenticationSchemes.Anonymous)
            {
                return new SecurityMessageProperty();
            }
            else
            {
                return base.ProcessAuthentication(listenerContext);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, "Reliability103",
                            Justification = "The exceptions are wrapped already.")]
        public override HttpStatusCode ValidateAuthentication(IHttpAuthenticationContext authenticationContext)
        {
            HttpStatusCode result = base.ValidateAuthentication(authenticationContext);
            if (result == HttpStatusCode.OK)
            {
                if (this.shouldValidateClientCertificate)
                {
                    bool isValidCertificate;
                    X509Certificate2 clientCertificate = authenticationContext.GetClientCertificate(out isValidCertificate);
                    if (clientCertificate == null)
                    {
                        if (this.RequireClientCertificate)
                        {
                            if (DiagnosticUtility.ShouldTraceError)
                            {
                                TraceUtility.TraceEvent(TraceEventType.Error, TraceCode.HttpsClientCertificateNotPresent, SR.GetString(SR.TraceCodeHttpsClientCertificateNotPresent),
                                    authenticationContext.CreateTraceRecord(), this, null);
                            }
                            result = CertificateErrorStatusCode;
                        }
                    }
                    else if (!isValidCertificate && !this.useCustomClientCertificateVerification)
                    {
                        if (DiagnosticUtility.ShouldTraceError)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Error, TraceCode.HttpsClientCertificateInvalid, SR.GetString(SR.TraceCodeHttpsClientCertificateInvalid),
                                authenticationContext.CreateTraceRecord(), this, null);
                        }
                        result = CertificateErrorStatusCode;
                    }

                    // Audit Authentication failure
                    if (result != HttpStatusCode.OK && (AuditLevel.Failure == (this.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Failure)))
                    {
                        string message = SR.GetString(SR.HttpAuthenticationFailed, this.AuthenticationScheme, result);
                        Exception exception = DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(message));
                        WriteAuditEvent(AuditLevel.Failure, (clientCertificate != null) ? SecurityUtils.GetCertificateId(clientCertificate) : String.Empty, exception);
                    }
                }
            }

            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic, "Reliability103",
                            Justification = "The exceptions are wrapped already.")]
        public override HttpStatusCode ValidateAuthentication(HttpListenerContext listenerContext)
        {
            HttpStatusCode result = base.ValidateAuthentication(listenerContext);
            if (result == HttpStatusCode.OK)
            {
                if (this.shouldValidateClientCertificate)
                {
                    HttpListenerRequest request = listenerContext.Request;
                    X509Certificate2 certificateEx = request.GetClientCertificate();
                    if (certificateEx == null)
                    {
                        if (this.RequireClientCertificate)
                        {
                            if (DiagnosticUtility.ShouldTraceWarning)
                            {
                                TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.HttpsClientCertificateNotPresent,
                                    SR.GetString(SR.TraceCodeHttpsClientCertificateNotPresent),
                                    new HttpListenerRequestTraceRecord(listenerContext.Request), this, null);
                            }
                            result = CertificateErrorStatusCode;
                        }
                    }
                    else if (request.ClientCertificateError != 0 && !useCustomClientCertificateVerification)
                    {
                        if (DiagnosticUtility.ShouldTraceWarning)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Warning, TraceCode.HttpsClientCertificateInvalid,
                                SR.GetString(SR.TraceCodeHttpsClientCertificateInvalid1, "0x" + (request.ClientCertificateError & 65535).ToString("X", CultureInfo.InvariantCulture)),
                                new HttpListenerRequestTraceRecord(listenerContext.Request), this, null);
                        }
                        result = CertificateErrorStatusCode;
                    }

                    // Audit Authentication failure
                    if (result != HttpStatusCode.OK && (AuditLevel.Failure == (this.AuditBehavior.MessageAuthenticationAuditLevel & AuditLevel.Failure)))
                    {
                        string message = SR.GetString(SR.HttpAuthenticationFailed, this.AuthenticationScheme, result);
                        Exception exception = DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(message));
                        WriteAuditEvent(AuditLevel.Failure, (certificateEx != null) ? SecurityUtils.GetCertificateId(certificateEx) : String.Empty, exception);
                    }
                }
            }
            return result;
        }

        private static bool ShouldValidateClientCertificate(bool requireClientCertificateValidation, BindingContext context)
        {
            if (requireClientCertificateValidation)
            {
                return true;
            }

            return EndpointSettings.GetValue<bool>(context, EndpointSettings.ValidateOptionalClientCertificates, false);
        }
    }
}
