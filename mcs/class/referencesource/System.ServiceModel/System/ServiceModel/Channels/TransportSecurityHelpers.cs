//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Net;
    using System.Net.Security;
    using System.Runtime;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;

    static class AuthenticationLevelHelper
    {
        internal static string ToString(AuthenticationLevel authenticationLevel)
        {
            if (authenticationLevel == AuthenticationLevel.MutualAuthRequested)
            {
                return "mutualAuthRequested";
            }
            else if (authenticationLevel == AuthenticationLevel.MutualAuthRequired)
            {
                return "mutualAuthRequired";
            }
            else if (authenticationLevel == AuthenticationLevel.None)
            {
                return "none";
            }

            Fx.Assert("unknown authentication level");
            return authenticationLevel.ToString();
        }
    }

    static class HttpTransportSecurityHelpers
    {
        static Dictionary<string, int> targetNameCounter = new Dictionary<string, int>();

        public static bool AddIdentityMapping(Uri via, EndpointAddress target)
        {
            string key = via.AbsoluteUri;
            string value;
            EndpointIdentity identity = target.Identity;

            if (identity != null && !(identity is X509CertificateEndpointIdentity))
            {
                value = SecurityUtils.GetSpnFromIdentity(identity, target);
            }
            else
            {
                value = SecurityUtils.GetSpnFromTarget(target);
            }

            lock (targetNameCounter)
            {
                int refCount = 0;
                if (targetNameCounter.TryGetValue(key, out refCount))
                {
                    if (!AuthenticationManager.CustomTargetNameDictionary.ContainsKey(key)
                        || AuthenticationManager.CustomTargetNameDictionary[key] != value)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR.GetString(SR.HttpTargetNameDictionaryConflict, key, value)));
                    }
                    targetNameCounter[key] = refCount + 1;
                }
                else
                {
                    if (AuthenticationManager.CustomTargetNameDictionary.ContainsKey(key)
                        && AuthenticationManager.CustomTargetNameDictionary[key] != value)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR.GetString(SR.HttpTargetNameDictionaryConflict, key, value)));
                    }

                    AuthenticationManager.CustomTargetNameDictionary[key] = value;
                    targetNameCounter.Add(key, 1);
                }
            }

            return true;
        }

        public static void RemoveIdentityMapping(Uri via, EndpointAddress target, bool validateState)
        {
            string key = via.AbsoluteUri;
            string value;
            EndpointIdentity identity = target.Identity;

            if (identity != null && !(identity is X509CertificateEndpointIdentity))
            {
                value = SecurityUtils.GetSpnFromIdentity(identity, target);
            }
            else
            {
                value = SecurityUtils.GetSpnFromTarget(target);
            }

            lock (targetNameCounter)
            {
                int refCount = targetNameCounter[key];
                if (refCount == 1)
                {
                    targetNameCounter.Remove(key);
                }
                else
                {
                    targetNameCounter[key] = refCount - 1;
                }

                if (validateState)
                {
                    if (!AuthenticationManager.CustomTargetNameDictionary.ContainsKey(key)
                        || AuthenticationManager.CustomTargetNameDictionary[key] != value)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR.GetString(SR.HttpTargetNameDictionaryConflict, key, value)));
                    }
                }
            }
        }

        static Dictionary<HttpWebRequest, string> serverCertMap = new Dictionary<HttpWebRequest, string>();
        static RemoteCertificateValidationCallback chainedServerCertValidationCallback = null;
        static bool serverCertValidationCallbackInstalled = false;

        public static void AddServerCertMapping(HttpWebRequest request, EndpointAddress to)
        {
            Fx.Assert(request.RequestUri.Scheme == Uri.UriSchemeHttps,
                "Wrong URI scheme for AddServerCertMapping().");
            X509CertificateEndpointIdentity remoteCertificateIdentity = to.Identity as X509CertificateEndpointIdentity;
            if (remoteCertificateIdentity != null)
            {
                // The following condition should have been validated when the channel was created.
                Fx.Assert(remoteCertificateIdentity.Certificates.Count <= 1,
                    "HTTPS server certificate identity contains multiple certificates");
                AddServerCertMapping(request, remoteCertificateIdentity.Certificates[0].Thumbprint);
            }
        }

        static void AddServerCertMapping(HttpWebRequest request, string thumbprint)
        {
            lock (serverCertMap)
            {
                if (!serverCertValidationCallbackInstalled)
                {
                    chainedServerCertValidationCallback = ServicePointManager.ServerCertificateValidationCallback;
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(
                        OnValidateServerCertificate);
                    serverCertValidationCallbackInstalled = true;
                }

                serverCertMap.Add(request, thumbprint);
            }
        }

        static bool OnValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            HttpWebRequest request = sender as HttpWebRequest;
            if (request != null)
            {
                string thumbprint;
                lock (serverCertMap)
                {
                    serverCertMap.TryGetValue(request, out thumbprint);
                }
                if (thumbprint != null)
                {
                    try
                    {
                        ValidateServerCertificate(certificate, thumbprint);
                    }
                    catch (SecurityNegotiationException e)
                    {
                        DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                        return false;
                    }
                }
            }

            if (chainedServerCertValidationCallback == null)
            {
                return (sslPolicyErrors == SslPolicyErrors.None);
            }
            else
            {
                return chainedServerCertValidationCallback(sender, certificate, chain, sslPolicyErrors);
            }
        }

        public static void RemoveServerCertMapping(HttpWebRequest request)
        {
            lock (serverCertMap)
            {
                serverCertMap.Remove(request);
            }
        }

        static void ValidateServerCertificate(X509Certificate certificate, string thumbprint)
        {
            string certHashString = certificate.GetCertHashString();
            if (!thumbprint.Equals(certHashString))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new SecurityNegotiationException(SR.GetString(SR.HttpsServerCertThumbprintMismatch,
                    certificate.Subject, certHashString, thumbprint)));
            }
        }
    }

    static class TransportSecurityHelpers
    {
        public static IAsyncResult BeginGetSspiCredential(SecurityTokenProviderContainer tokenProvider, TimeSpan timeout,
            AsyncCallback callback, object state)
        {
            return new GetSspiCredentialAsyncResult(tokenProvider.TokenProvider as SspiSecurityTokenProvider, timeout, callback, state);
        }

        public static IAsyncResult BeginGetSspiCredential(SecurityTokenProvider tokenProvider, TimeSpan timeout,
            AsyncCallback callback, object state)
        {
            return new GetSspiCredentialAsyncResult((SspiSecurityTokenProvider)tokenProvider, timeout, callback, state);
        }

        public static NetworkCredential EndGetSspiCredential(IAsyncResult result,
            out TokenImpersonationLevel impersonationLevel, out AuthenticationLevel authenticationLevel)
        {
            return GetSspiCredentialAsyncResult.End(result, out impersonationLevel, out authenticationLevel);
        }

        public static NetworkCredential EndGetSspiCredential(IAsyncResult result,
            out TokenImpersonationLevel impersonationLevel, out bool allowNtlm)
        {
            return GetSspiCredentialAsyncResult.End(result, out impersonationLevel, out allowNtlm);
        }

        // used for HTTP (from HttpChannelUtilities.GetCredential)
        public static NetworkCredential GetSspiCredential(SecurityTokenProviderContainer tokenProvider, TimeSpan timeout,
            out TokenImpersonationLevel impersonationLevel, out AuthenticationLevel authenticationLevel)
        {
            bool dummyExtractWindowsGroupClaims;
            bool allowNtlm;
            NetworkCredential result = GetSspiCredential(tokenProvider.TokenProvider as SspiSecurityTokenProvider, timeout,
                out dummyExtractWindowsGroupClaims, out impersonationLevel, out allowNtlm);
            authenticationLevel = allowNtlm ?
                AuthenticationLevel.MutualAuthRequested : AuthenticationLevel.MutualAuthRequired;
            return result;
        }

        // used by client WindowsStream security (from InitiateUpgrade)
        public static NetworkCredential GetSspiCredential(SspiSecurityTokenProvider tokenProvider, TimeSpan timeout,
            out TokenImpersonationLevel impersonationLevel, out bool allowNtlm)
        {
            bool dummyExtractWindowsGroupClaims;
            return GetSspiCredential(tokenProvider, timeout,
                out dummyExtractWindowsGroupClaims, out impersonationLevel, out allowNtlm);
        }

        // used by server WindowsStream security (from Open)
        public static NetworkCredential GetSspiCredential(SecurityTokenManager credentialProvider,
            SecurityTokenRequirement sspiTokenRequirement, TimeSpan timeout,
            out bool extractGroupsForWindowsAccounts)
        {
            extractGroupsForWindowsAccounts = TransportDefaults.ExtractGroupsForWindowsAccounts;
            NetworkCredential result = null;

            if (credentialProvider != null)
            {
                SecurityTokenProvider tokenProvider = credentialProvider.CreateSecurityTokenProvider(sspiTokenRequirement);
                if (tokenProvider != null)
                {
                    TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                    SecurityUtils.OpenTokenProviderIfRequired(tokenProvider, timeoutHelper.RemainingTime());
                    bool success = false;
                    try
                    {
                        TokenImpersonationLevel dummyImpersonationLevel;
                        bool dummyAllowNtlm;
                        result = GetSspiCredential((SspiSecurityTokenProvider)tokenProvider, timeoutHelper.RemainingTime(), out extractGroupsForWindowsAccounts,
                            out dummyImpersonationLevel, out dummyAllowNtlm);

                        success = true;
                    }
                    finally
                    {
                        if (!success)
                        {
                            SecurityUtils.AbortTokenProviderIfRequired(tokenProvider);
                        }
                    }
                    SecurityUtils.CloseTokenProviderIfRequired(tokenProvider, timeoutHelper.RemainingTime());
                }
            }

            return result;
        }

        // core Cred lookup code
        static NetworkCredential GetSspiCredential(SspiSecurityTokenProvider tokenProvider, TimeSpan timeout,
            out bool extractGroupsForWindowsAccounts, out TokenImpersonationLevel impersonationLevel,
            out bool allowNtlm)
        {
            NetworkCredential credential = null;
            extractGroupsForWindowsAccounts = TransportDefaults.ExtractGroupsForWindowsAccounts;
            impersonationLevel = TokenImpersonationLevel.Identification;
            allowNtlm = ConnectionOrientedTransportDefaults.AllowNtlm;

            if (tokenProvider != null)
            {
                SspiSecurityToken token = TransportSecurityHelpers.GetToken<SspiSecurityToken>(tokenProvider, timeout);
                if (token != null)
                {
                    extractGroupsForWindowsAccounts = token.ExtractGroupsForWindowsAccounts;
                    impersonationLevel = token.ImpersonationLevel;
                    allowNtlm = token.AllowNtlm;
                    if (token.NetworkCredential != null)
                    {
                        credential = token.NetworkCredential;
                        SecurityUtils.FixNetworkCredential(ref credential);
                    }
                }
            }

            // Initialize to the default value if no token provided. A partial trust app should not have access to the
            // default network credentials but should be able to provide credentials. The DefaultNetworkCredentials
            // getter will throw under partial trust.
            if (credential == null)
            {
                credential = CredentialCache.DefaultNetworkCredentials;
            }

            return credential;
        }

        public static SecurityTokenRequirement CreateSspiTokenRequirement(string transportScheme, Uri listenUri)
        {
            RecipientServiceModelSecurityTokenRequirement tokenRequirement = new RecipientServiceModelSecurityTokenRequirement();
            tokenRequirement.TransportScheme = transportScheme;
            tokenRequirement.RequireCryptographicToken = false;
            tokenRequirement.ListenUri = listenUri;
            tokenRequirement.TokenType = ServiceModelSecurityTokenTypes.SspiCredential;
            return tokenRequirement;
        }

        static SecurityTokenRequirement CreateSspiTokenRequirement(EndpointAddress target, Uri via, string transportScheme)
        {
            InitiatorServiceModelSecurityTokenRequirement sspiTokenRequirement = new InitiatorServiceModelSecurityTokenRequirement();
            sspiTokenRequirement.TokenType = ServiceModelSecurityTokenTypes.SspiCredential;
            sspiTokenRequirement.RequireCryptographicToken = false;
            sspiTokenRequirement.TransportScheme = transportScheme;
            sspiTokenRequirement.TargetAddress = target;
            sspiTokenRequirement.Via = via;
            return sspiTokenRequirement;
        }

        public static SspiSecurityTokenProvider GetSspiTokenProvider(
            SecurityTokenManager tokenManager, EndpointAddress target, Uri via, string transportScheme, AuthenticationSchemes authenticationScheme, ChannelParameterCollection channelParameters)
        {
            if (tokenManager != null)
            {
                SecurityTokenRequirement sspiRequirement = CreateSspiTokenRequirement(target, via, transportScheme);
                sspiRequirement.Properties[ServiceModelSecurityTokenRequirement.HttpAuthenticationSchemeProperty] = authenticationScheme;
                if (channelParameters != null)
                {
                    sspiRequirement.Properties[ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty] = channelParameters;
                }
                SspiSecurityTokenProvider tokenProvider = tokenManager.CreateSecurityTokenProvider(sspiRequirement) as SspiSecurityTokenProvider;
                return tokenProvider;
            }
            else
            {
                return null;
            }
        }

        public static SspiSecurityTokenProvider GetSspiTokenProvider(
            SecurityTokenManager tokenManager, EndpointAddress target, Uri via, string transportScheme,
            out IdentityVerifier identityVerifier)
        {
            identityVerifier = null;
            if (tokenManager != null)
            {
                SspiSecurityTokenProvider tokenProvider =
                    tokenManager.CreateSecurityTokenProvider(CreateSspiTokenRequirement(target, via, transportScheme)) as SspiSecurityTokenProvider;

                if (tokenProvider != null)
                {
                    identityVerifier = IdentityVerifier.CreateDefault();
                }

                return tokenProvider;
            }
            return null;
        }

        public static SecurityTokenProvider GetDigestTokenProvider(
            SecurityTokenManager tokenManager, EndpointAddress target, Uri via,
            string transportScheme, AuthenticationSchemes authenticationScheme, ChannelParameterCollection channelParameters)
        {
            if (tokenManager != null)
            {
                InitiatorServiceModelSecurityTokenRequirement digestTokenRequirement =
                    new InitiatorServiceModelSecurityTokenRequirement();
                digestTokenRequirement.TokenType = ServiceModelSecurityTokenTypes.SspiCredential;
                digestTokenRequirement.TargetAddress = target;
                digestTokenRequirement.Via = via;
                digestTokenRequirement.RequireCryptographicToken = false;
                digestTokenRequirement.TransportScheme = transportScheme;
                digestTokenRequirement.Properties[ServiceModelSecurityTokenRequirement.HttpAuthenticationSchemeProperty] = authenticationScheme;
                if (channelParameters != null)
                {
                    digestTokenRequirement.Properties[ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty] = channelParameters;
                }
                return tokenManager.CreateSecurityTokenProvider(digestTokenRequirement) as SspiSecurityTokenProvider;
            }
            return null;
        }

        public static SecurityTokenAuthenticator GetCertificateTokenAuthenticator(SecurityTokenManager tokenManager, string transportScheme, Uri listenUri)
        {
            RecipientServiceModelSecurityTokenRequirement clientAuthRequirement = new RecipientServiceModelSecurityTokenRequirement();
            clientAuthRequirement.TokenType = SecurityTokenTypes.X509Certificate;
            clientAuthRequirement.RequireCryptographicToken = true;
            clientAuthRequirement.KeyUsage = SecurityKeyUsage.Signature;
            clientAuthRequirement.TransportScheme = transportScheme;
            clientAuthRequirement.ListenUri = listenUri;
            SecurityTokenResolver dummy;
            return tokenManager.CreateSecurityTokenAuthenticator(clientAuthRequirement, out dummy);
        }

        public static SecurityTokenProvider GetCertificateTokenProvider(
            SecurityTokenManager tokenManager, EndpointAddress target, Uri via, string transportScheme, ChannelParameterCollection channelParameters)
        {
            if (tokenManager != null)
            {
                InitiatorServiceModelSecurityTokenRequirement certificateTokenRequirement =
                    new InitiatorServiceModelSecurityTokenRequirement();
                certificateTokenRequirement.TokenType = SecurityTokenTypes.X509Certificate;
                certificateTokenRequirement.TargetAddress = target;
                certificateTokenRequirement.Via = via;
                certificateTokenRequirement.RequireCryptographicToken = false;
                certificateTokenRequirement.TransportScheme = transportScheme;
                if (channelParameters != null)
                {
                    certificateTokenRequirement.Properties[ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty] = channelParameters;
                }
                return tokenManager.CreateSecurityTokenProvider(certificateTokenRequirement);
            }
            return null;
        }

        static T GetToken<T>(SecurityTokenProvider tokenProvider, TimeSpan timeout)
            where T : SecurityToken
        {
            SecurityToken result = tokenProvider.GetToken(timeout);
            if ((result != null) && !(result is T))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                    SR.InvalidTokenProvided, tokenProvider.GetType(), typeof(T))));
            }
            return result as T;
        }

        public static IAsyncResult BeginGetUserNameCredential(SecurityTokenProviderContainer tokenProvider, TimeSpan timeout,
            AsyncCallback callback, object state)
        {
            return new GetUserNameCredentialAsyncResult(tokenProvider, timeout, callback, state);
        }

        public static NetworkCredential EndGetUserNameCredential(IAsyncResult result)
        {
            return GetUserNameCredentialAsyncResult.End(result);
        }

        public static NetworkCredential GetUserNameCredential(SecurityTokenProviderContainer tokenProvider, TimeSpan timeout)
        {
            NetworkCredential result = null;

            if (tokenProvider != null && tokenProvider.TokenProvider != null)
            {
                UserNameSecurityToken token = GetToken<UserNameSecurityToken>(tokenProvider.TokenProvider, timeout);
                if (token != null)
                {
                    SecurityUtils.PrepareNetworkCredential();
                    result = new NetworkCredential(token.UserName, token.Password);
                }
            }

            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                    SR.NoUserNameTokenProvided)));
            }

            return result;
        }

        static InitiatorServiceModelSecurityTokenRequirement CreateUserNameTokenRequirement(
            EndpointAddress target, Uri via, string transportScheme)
        {
            InitiatorServiceModelSecurityTokenRequirement usernameRequirement = new InitiatorServiceModelSecurityTokenRequirement();
            usernameRequirement.RequireCryptographicToken = false;
            usernameRequirement.TokenType = SecurityTokenTypes.UserName;
            usernameRequirement.TargetAddress = target;
            usernameRequirement.Via = via;
            usernameRequirement.TransportScheme = transportScheme;
            return usernameRequirement;
        }

        public static SecurityTokenProvider GetUserNameTokenProvider(
            SecurityTokenManager tokenManager, EndpointAddress target, Uri via, string transportScheme, AuthenticationSchemes authenticationScheme,
            ChannelParameterCollection channelParameters)
        {
            SecurityTokenProvider result = null;
            if (tokenManager != null)
            {
                SecurityTokenRequirement usernameRequirement = CreateUserNameTokenRequirement(target, via, transportScheme);
                usernameRequirement.Properties[ServiceModelSecurityTokenRequirement.HttpAuthenticationSchemeProperty] = authenticationScheme;
                if (channelParameters != null)
                {
                    usernameRequirement.Properties[ServiceModelSecurityTokenRequirement.ChannelParametersCollectionProperty] = channelParameters;
                }
                result = tokenManager.CreateSecurityTokenProvider(usernameRequirement);
            }
            return result;
        }

        public static Uri GetListenUri(Uri baseAddress, string relativeAddress)
        {
            Uri fullUri = baseAddress;

            // Ensure that baseAddress Path does end with a slash if we have a relative address
            if (!String.IsNullOrEmpty(relativeAddress))
            {
                if (!baseAddress.AbsolutePath.EndsWith("/", StringComparison.Ordinal))
                {
                    UriBuilder uriBuilder = new UriBuilder(baseAddress);
                    TcpChannelListener.FixIpv6Hostname(uriBuilder, baseAddress);
                    uriBuilder.Path = uriBuilder.Path + "/";
                    baseAddress = uriBuilder.Uri;
                }

                fullUri = new Uri(baseAddress, relativeAddress);
            }

            return fullUri;
        }

        class GetUserNameCredentialAsyncResult : AsyncResult
        {
            NetworkCredential credential;
            static AsyncCallback onGetToken = Fx.ThunkCallback(new AsyncCallback(OnGetToken));
            SecurityTokenProvider tokenProvider;

            public GetUserNameCredentialAsyncResult(SecurityTokenProviderContainer tokenProvider, TimeSpan timeout,
                AsyncCallback callback, object state)
                : base(callback, state)
            {
                if (tokenProvider == null || tokenProvider.TokenProvider == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                        SR.NoUserNameTokenProvided)));
                }

                this.tokenProvider = tokenProvider.TokenProvider;
                IAsyncResult result = this.tokenProvider.BeginGetToken(timeout, onGetToken, this);
                if (result.CompletedSynchronously)
                {
                    CompleteGetToken(result);
                    base.Complete(true);
                }
            }

            void CompleteGetToken(IAsyncResult result)
            {
                UserNameSecurityToken token = (UserNameSecurityToken)this.tokenProvider.EndGetToken(result);
                if (token != null)
                {
                    SecurityUtils.PrepareNetworkCredential();
                    this.credential = new NetworkCredential(token.UserName, token.Password);
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(
                        SR.NoUserNameTokenProvided)));
                }
            }

            static void OnGetToken(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                GetUserNameCredentialAsyncResult thisPtr = (GetUserNameCredentialAsyncResult)result.AsyncState;

                Exception completionException = null;
                try
                {
                    thisPtr.CompleteGetToken(result);
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
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

            public static NetworkCredential End(IAsyncResult result)
            {
                GetUserNameCredentialAsyncResult thisPtr = AsyncResult.End<GetUserNameCredentialAsyncResult>(result);
                return thisPtr.credential;
            }
        }

        class GetSspiCredentialAsyncResult : AsyncResult
        {
            bool allowNtlm;
            NetworkCredential credential;
            TokenImpersonationLevel impersonationLevel;
            static AsyncCallback onGetToken;
            SspiSecurityTokenProvider credentialProvider;

            public GetSspiCredentialAsyncResult(SspiSecurityTokenProvider credentialProvider, TimeSpan timeout,
                AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.allowNtlm = ConnectionOrientedTransportDefaults.AllowNtlm;
                this.impersonationLevel = TokenImpersonationLevel.Identification;
                if (credentialProvider == null)
                {
                    this.EnsureCredentialInitialized();
                    base.Complete(true);
                    return;
                }

                this.credentialProvider = credentialProvider;

                if (onGetToken == null)
                {
                    onGetToken = Fx.ThunkCallback(new AsyncCallback(OnGetToken));
                }
                IAsyncResult result = credentialProvider.BeginGetToken(timeout, onGetToken, this);
                if (result.CompletedSynchronously)
                {
                    CompleteGetToken(result);
                    base.Complete(true);
                }
            }

            void CompleteGetToken(IAsyncResult result)
            {
                SspiSecurityToken token = (SspiSecurityToken)this.credentialProvider.EndGetToken(result);
                if (token != null)
                {
                    this.impersonationLevel = token.ImpersonationLevel;
                    this.allowNtlm = token.AllowNtlm;
                    if (token.NetworkCredential != null)
                    {
                        this.credential = token.NetworkCredential;
                        SecurityUtils.FixNetworkCredential(ref this.credential);
                    }
                }

                this.EnsureCredentialInitialized();
            }

            void EnsureCredentialInitialized()
            {
                // Initialize to the default value if no token provided. A partial trust app should not have access to
                // the default network credentials but should be able to provide credentials. The
                // DefaultNetworkCredentials getter will throw under partial trust.
                if (this.credential == null)
                {
                    this.credential = CredentialCache.DefaultNetworkCredentials;
                }
            }

            static void OnGetToken(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                    return;

                GetSspiCredentialAsyncResult thisPtr = (GetSspiCredentialAsyncResult)result.AsyncState;

                Exception completionException = null;
                try
                {
                    thisPtr.CompleteGetToken(result);
                }
#pragma warning suppress 56500 // [....], transferring exception to another thread
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

            public static NetworkCredential End(IAsyncResult result,
                out TokenImpersonationLevel impersonationLevel, out AuthenticationLevel authenticationLevel)
            {
                GetSspiCredentialAsyncResult thisPtr = AsyncResult.End<GetSspiCredentialAsyncResult>(result);
                impersonationLevel = thisPtr.impersonationLevel;
                authenticationLevel = thisPtr.allowNtlm ?
                    AuthenticationLevel.MutualAuthRequested : AuthenticationLevel.MutualAuthRequired;
                return thisPtr.credential;
            }

            public static NetworkCredential End(IAsyncResult result,
                out TokenImpersonationLevel impersonationLevel, out bool allowNtlm)
            {
                GetSspiCredentialAsyncResult thisPtr = AsyncResult.End<GetSspiCredentialAsyncResult>(result);
                impersonationLevel = thisPtr.impersonationLevel;
                allowNtlm = thisPtr.allowNtlm;
                return thisPtr.credential;
            }
        }
    }
}
