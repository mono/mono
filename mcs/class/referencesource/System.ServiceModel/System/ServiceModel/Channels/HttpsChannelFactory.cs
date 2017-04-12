//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Collections.ObjectModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Net;
    using System.Net.Security;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;

    class HttpsChannelFactory<TChannel> : HttpChannelFactory<TChannel>
    {
        bool requireClientCertificate;
        IChannelBindingProvider channelBindingProvider;
        RemoteCertificateValidationCallback remoteCertificateValidationCallback;
        X509CertificateValidator sslCertificateValidator;

        internal HttpsChannelFactory(HttpsTransportBindingElement httpsBindingElement, BindingContext context)
            : base(httpsBindingElement, context)
        {
            this.requireClientCertificate = httpsBindingElement.RequireClientCertificate;
            this.channelBindingProvider = new ChannelBindingProviderHelper();
            ClientCredentials credentials = context.BindingParameters.Find<ClientCredentials>();
            if (credentials != null && credentials.ServiceCertificate.SslCertificateAuthentication != null)
            {
                this.sslCertificateValidator = credentials.ServiceCertificate.SslCertificateAuthentication.GetCertificateValidator();
                this.remoteCertificateValidationCallback = new RemoteCertificateValidationCallback(RemoteCertificateValidationCallback);
            }
        }

        public override string Scheme
        {
            get
            {
                return Uri.UriSchemeHttps;
            }
        }

        public bool RequireClientCertificate
        {
            get
            {
                return this.requireClientCertificate;
            }
        }

        public override bool IsChannelBindingSupportEnabled
        {
            get
            {
                return this.channelBindingProvider.IsChannelBindingSupportEnabled;
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

        internal override SecurityMessageProperty CreateReplySecurityProperty(HttpWebRequest request,
            HttpWebResponse response)
        {
            SecurityMessageProperty result = null;
            X509Certificate certificate = request.ServicePoint.Certificate;
            if (certificate != null)
            {
                X509Certificate2 certificateEx = new X509Certificate2(certificate);
                SecurityToken token = new X509SecurityToken(certificateEx, false);
                ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = SecurityUtils.NonValidatingX509Authenticator.ValidateToken(token);
                result = new SecurityMessageProperty();
                result.TransportToken = new SecurityTokenSpecification(token, authorizationPolicies);
                result.ServiceSecurityContext = new ServiceSecurityContext(authorizationPolicies);
            }
            else
            {
                result = base.CreateReplySecurityProperty(request, response);
            }
            return result;
        }

        protected override void ValidateCreateChannelParameters(EndpointAddress remoteAddress, Uri via)
        {
            if (remoteAddress.Identity != null)
            {
                X509CertificateEndpointIdentity certificateIdentity =
                    remoteAddress.Identity as X509CertificateEndpointIdentity;
                if (certificateIdentity != null)
                {
                    if (certificateIdentity.Certificates.Count > 1)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("remoteAddress", SR.GetString(
                            SR.HttpsIdentityMultipleCerts, remoteAddress.Uri));
                    }
                }

                EndpointIdentity identity = remoteAddress.Identity;
                bool validIdentity = (certificateIdentity != null)
                    || ClaimTypes.Spn.Equals(identity.IdentityClaim.ClaimType)
                    || ClaimTypes.Upn.Equals(identity.IdentityClaim.ClaimType)
                    || ClaimTypes.Dns.Equals(identity.IdentityClaim.ClaimType);

                if (!HttpChannelFactory<TChannel>.IsWindowsAuth(this.AuthenticationScheme)
                    && !validIdentity)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("remoteAddress", SR.GetString(
                        SR.HttpsExplicitIdentity));
                }
            }
            base.ValidateCreateChannelParameters(remoteAddress, via);
        }

        protected override TChannel OnCreateChannelCore(EndpointAddress address, Uri via)
        {
            ValidateCreateChannelParameters(address, via);
            this.ValidateWebSocketTransportUsage();

            if (typeof(TChannel) == typeof(IRequestChannel))
            {
                return (TChannel)(object)new HttpsRequestChannel((HttpsChannelFactory<IRequestChannel>)(object)this, address, via, ManualAddressing);
            }
            else
            {
                return (TChannel)(object)new ClientWebSocketTransportDuplexSessionChannel((HttpChannelFactory<IDuplexSessionChannel>)(object)this, this.ClientWebSocketFactory, address, via, this.WebSocketBufferPool);
            }
        }

        protected override bool IsSecurityTokenManagerRequired()
        {
            return this.requireClientCertificate || base.IsSecurityTokenManagerRequired();
        }

        protected override string OnGetConnectionGroupPrefix(HttpWebRequest httpWebRequest, SecurityTokenContainer clientCertificateToken)
        {
            System.Text.StringBuilder inputStringBuilder = new System.Text.StringBuilder();
            string delimiter = "\0"; // nonprintable characters are invalid for SSPI Domain/UserName/Password

            if (this.RequireClientCertificate)
            {
                HttpsChannelFactory<TChannel>.SetCertificate(httpWebRequest, clientCertificateToken);
                X509CertificateCollection certificateCollection = httpWebRequest.ClientCertificates;
                for (int i = 0; i < certificateCollection.Count; i++)
                {
                    inputStringBuilder.AppendFormat("{0}{1}", certificateCollection[i].GetCertHashString(), delimiter);
                }
            }

            return inputStringBuilder.ToString();
        }

        void OnOpenCore()
        {
            if (this.requireClientCertificate && this.SecurityTokenManager == null)
            {
                throw Fx.AssertAndThrow("HttpsChannelFactory: SecurityTokenManager is null on open.");
            }
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            base.OnEndOpen(result);
            OnOpenCore();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            base.OnOpen(timeout);
            OnOpenCore();
        }

        internal SecurityTokenProvider CreateAndOpenCertificateTokenProvider(EndpointAddress target, Uri via, ChannelParameterCollection channelParameters, TimeSpan timeout)
        {
            if (!this.RequireClientCertificate)
            {
                return null;
            }
            SecurityTokenProvider certificateProvider = TransportSecurityHelpers.GetCertificateTokenProvider(
                this.SecurityTokenManager, target, via, this.Scheme, channelParameters);
            SecurityUtils.OpenTokenProviderIfRequired(certificateProvider, timeout);
            return certificateProvider;
        }

        static void SetCertificate(HttpWebRequest request, SecurityTokenContainer clientCertificateToken)
        {
            if (clientCertificateToken != null)
            {
                X509SecurityToken x509Token = (X509SecurityToken)clientCertificateToken.Token;
                request.ClientCertificates.Add(x509Token.Certificate);
            }
        }

        internal SecurityTokenContainer GetCertificateSecurityToken(SecurityTokenProvider certificateProvider,
            EndpointAddress to, Uri via, ChannelParameterCollection channelParameters, ref TimeoutHelper timeoutHelper)
        {
            SecurityToken token = null;
            SecurityTokenContainer tokenContainer = null;
            SecurityTokenProvider webRequestCertificateProvider;
            if (ManualAddressing && this.RequireClientCertificate)
            {
                webRequestCertificateProvider = CreateAndOpenCertificateTokenProvider(to, via, channelParameters, timeoutHelper.RemainingTime());
            }
            else
            {
                webRequestCertificateProvider = certificateProvider;
            }

            if (webRequestCertificateProvider != null)
            {
                token = webRequestCertificateProvider.GetToken(timeoutHelper.RemainingTime());
            }

            if (ManualAddressing && this.RequireClientCertificate)
            {
                SecurityUtils.AbortTokenProviderIfRequired(webRequestCertificateProvider);
            }

            if (token != null)
            {
                tokenContainer = new SecurityTokenContainer(token);
            }

            return tokenContainer;
        }

        void AddServerCertMappingOrSetRemoteCertificateValidationCallback(HttpWebRequest request, EndpointAddress to)
        {
            Fx.Assert(request != null, "request should not be null.");
            if (this.sslCertificateValidator != null)
            {
                request.ServerCertificateValidationCallback = this.remoteCertificateValidationCallback;
            }
            else
            {
                HttpTransportSecurityHelpers.AddServerCertMapping(request, to);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(FxCop.Category.ReliabilityBasic,
            "Reliability104:CaughtAndHandledExceptionsRule",
            Justification = "The exception being thrown out comes from user code. Any non-fatal exception should be handled and translated into validation failure(return false).")]
        bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            Fx.Assert(sender is HttpWebRequest, "sender should be HttpWebRequest");
            Fx.Assert(this.sslCertificateValidator != null, "sslCertificateAuthentidation should not be null.");

            try
            {
                this.sslCertificateValidator.Validate(new X509Certificate2(certificate));
                return true;
            }
            catch (SecurityTokenValidationException ex)
            {
                FxTrace.Exception.AsInformation(ex);
                return false;
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }

                FxTrace.Exception.AsWarning(ex);
                return false;
            }
        }

        class HttpsRequestChannel : HttpRequestChannel
        {
            SecurityTokenProvider certificateProvider;
            HttpsChannelFactory<IRequestChannel> factory;

            public HttpsRequestChannel(HttpsChannelFactory<IRequestChannel> factory, EndpointAddress to, Uri via, bool manualAddressing)
                : base(factory, to, via, manualAddressing)
            {
                this.factory = factory;
            }

            public new HttpsChannelFactory<IRequestChannel> Factory
            {
                get { return this.factory; }
            }

            void CreateAndOpenTokenProvider(TimeSpan timeout)
            {
                if (!ManualAddressing && this.Factory.RequireClientCertificate)
                {
                    this.certificateProvider = Factory.CreateAndOpenCertificateTokenProvider(this.RemoteAddress, this.Via, this.ChannelParameters, timeout);
                }
            }

            void CloseTokenProvider(TimeSpan timeout)
            {
                if (this.certificateProvider != null)
                {
                    SecurityUtils.CloseTokenProviderIfRequired(this.certificateProvider, timeout);
                }
            }

            void AbortTokenProvider()
            {
                if (this.certificateProvider != null)
                {
                    SecurityUtils.AbortTokenProviderIfRequired(this.certificateProvider);
                }
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                CreateAndOpenTokenProvider(timeoutHelper.RemainingTime());
                return base.OnBeginOpen(timeoutHelper.RemainingTime(), callback, state);
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                CreateAndOpenTokenProvider(timeoutHelper.RemainingTime());
                base.OnOpen(timeoutHelper.RemainingTime());
            }

            protected override void OnAbort()
            {
                AbortTokenProvider();
                base.OnAbort();
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                CloseTokenProvider(timeoutHelper.RemainingTime());
                return base.OnBeginClose(timeoutHelper.RemainingTime(), callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                CloseTokenProvider(timeoutHelper.RemainingTime());
                base.OnClose(timeoutHelper.RemainingTime());
            }

            public IAsyncResult BeginBaseGetWebRequest(EndpointAddress to, Uri via, SecurityTokenContainer clientCertificateToken, ref TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
            {
                return base.BeginGetWebRequest(to, via, clientCertificateToken, ref timeoutHelper, callback, state);
            }

            public HttpWebRequest EndBaseGetWebRequest(IAsyncResult result)
            {
                return base.EndGetWebRequest(result);
            }

            public override HttpWebRequest GetWebRequest(EndpointAddress to, Uri via, ref TimeoutHelper timeoutHelper)
            {
                SecurityTokenContainer clientCertificateToken = Factory.GetCertificateSecurityToken(this.certificateProvider, to, via, this.ChannelParameters, ref timeoutHelper);
                HttpWebRequest request = base.GetWebRequest(to, via, clientCertificateToken, ref timeoutHelper);
                this.factory.AddServerCertMappingOrSetRemoteCertificateValidationCallback(request, to);
                return request;
            }

            public override IAsyncResult BeginGetWebRequest(EndpointAddress to, Uri via, ref TimeoutHelper timeoutHelper, AsyncCallback callback, object state)
            {
                return new GetWebRequestAsyncResult(this, to, via, ref timeoutHelper, callback, state);
            }

            public override HttpWebRequest EndGetWebRequest(IAsyncResult result)
            {
                return GetWebRequestAsyncResult.End(result);
            }

            public override bool WillGetWebRequestCompleteSynchronously()
            {
                if (!base.WillGetWebRequestCompleteSynchronously())
                {
                    return false;
                }

                return (this.certificateProvider == null && !Factory.ManualAddressing);
            }

            internal override void OnWebRequestCompleted(HttpWebRequest request)
            {
                HttpTransportSecurityHelpers.RemoveServerCertMapping(request);
            }

            class GetWebRequestAsyncResult : AsyncResult
            {
                SecurityTokenProvider certificateProvider;
                HttpsChannelFactory<IRequestChannel> factory;
                HttpsRequestChannel httpsChannel;
                HttpWebRequest request;
                EndpointAddress to;
                Uri via;
                TimeoutHelper timeoutHelper;
                SecurityTokenContainer tokenContainer;
                static AsyncCallback onGetBaseWebRequestCallback = Fx.ThunkCallback(new AsyncCallback(OnGetBaseWebRequestCallback));
                static AsyncCallback onGetTokenCallback;

                public GetWebRequestAsyncResult(HttpsRequestChannel httpsChannel, EndpointAddress to, Uri via,
                    ref TimeoutHelper timeoutHelper,
                    AsyncCallback callback, object state)
                    : base(callback, state)
                {
                    this.httpsChannel = httpsChannel;
                    this.to = to;
                    this.via = via;
                    this.timeoutHelper = timeoutHelper;
                    this.factory = httpsChannel.Factory;
                    this.certificateProvider = httpsChannel.certificateProvider;
                    if (this.factory.ManualAddressing && this.factory.RequireClientCertificate)
                    {
                        this.certificateProvider =
                            this.factory.CreateAndOpenCertificateTokenProvider(to, via, httpsChannel.ChannelParameters, timeoutHelper.RemainingTime());
                    }

                    if (!GetToken())
                    {
                        return;
                    }

                    if (!GetWebRequest())
                    {
                        return;
                    }

                    base.Complete(true);
                }

                bool GetWebRequest()
                {
                    IAsyncResult result = this.httpsChannel.BeginBaseGetWebRequest(to, via, tokenContainer, ref timeoutHelper, onGetBaseWebRequestCallback, this);

                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }

                    this.request = this.httpsChannel.EndBaseGetWebRequest(result);
                    this.factory.AddServerCertMappingOrSetRemoteCertificateValidationCallback(this.request, this.to);
                    return true;
                }

                bool GetToken()
                {
                    if (this.certificateProvider != null)
                    {
                        if (onGetTokenCallback == null)
                        {
                            onGetTokenCallback = Fx.ThunkCallback(new AsyncCallback(OnGetTokenCallback));
                        }

                        IAsyncResult result = this.certificateProvider.BeginGetToken(
                            timeoutHelper.RemainingTime(), onGetTokenCallback, this);

                        if (!result.CompletedSynchronously)
                        {
                            return false;
                        }
                        OnGetToken(result);
                    }

                    return true;
                }

                static void OnGetBaseWebRequestCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                        return;

                    GetWebRequestAsyncResult thisPtr = (GetWebRequestAsyncResult)result.AsyncState;

                    Exception completionException = null;
                    try
                    {
                        thisPtr.request = thisPtr.httpsChannel.EndBaseGetWebRequest(result);
                        thisPtr.factory.AddServerCertMappingOrSetRemoteCertificateValidationCallback(thisPtr.request, thisPtr.to);
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

                static void OnGetTokenCallback(IAsyncResult result)
                {
                    if (result.CompletedSynchronously)
                        return;

                    GetWebRequestAsyncResult thisPtr = (GetWebRequestAsyncResult)result.AsyncState;

                    Exception completionException = null;
                    bool completeSelf;
                    try
                    {
                        thisPtr.OnGetToken(result);
                        completeSelf = thisPtr.GetWebRequest();
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

                void OnGetToken(IAsyncResult result)
                {
                    SecurityToken token = this.certificateProvider.EndGetToken(result);
                    if (token != null)
                    {
                        this.tokenContainer = new SecurityTokenContainer(token);
                    }
                    CloseCertificateProviderIfRequired();
                }

                void CloseCertificateProviderIfRequired()
                {
                    if (this.factory.ManualAddressing && this.certificateProvider != null)
                    {
                        SecurityUtils.AbortTokenProviderIfRequired(this.certificateProvider);
                    }
                }

                public static HttpWebRequest End(IAsyncResult result)
                {
                    GetWebRequestAsyncResult thisPtr = AsyncResult.End<GetWebRequestAsyncResult>(result);
                    return thisPtr.request;
                }
            }
        }
    }
}
