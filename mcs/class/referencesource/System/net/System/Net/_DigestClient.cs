//------------------------------------------------------------------------------
// <copyright file="_DigestClient.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System.Net.Sockets;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using Microsoft.Win32;
    using System.IO;
    using System.Security;
    using System.Diagnostics;

    internal class DigestClient : ISessionAuthenticationModule {

        internal const string AuthType = "Digest";
        internal static string Signature = AuthType.ToLower(CultureInfo.InvariantCulture);
        internal static int SignatureSize = Signature.Length;

        private static PrefixLookup challengeCache = new PrefixLookup();
        private static readonly char[] singleSpaceArray = new char[]{' '};

        // [....]: make sure WDigest fixes these bugs before we
        // enable this code ("Windows OS" Product Studio database):
        //
        // 921024   1   Wdigest should support MD5, at least for explicit (non-default) credentials.
        // 762116   2   WDigest should ignore directives that do not have a value
        // 762115   3   WDigest should allow an application to retrieve the parsed domain directive
        //
        private static bool _WDigestAvailable;

        static DigestClient() {
            _WDigestAvailable = SSPIWrapper.GetVerifyPackageInfo(GlobalSSPI.SSPIAuth, NegotiationInfoClass.WDigest)!=null;
        }

        public Authorization Authenticate(string challenge, WebRequest webRequest, ICredentials credentials) {
            GlobalLog.Print("DigestClient::Authenticate() challenge:[" + ValidationHelper.ToString(challenge) + "] webRequest#" + ValidationHelper.HashString(webRequest) + " credentials#" + ValidationHelper.HashString(credentials) + " calling DoAuthenticate()");
            return DoAuthenticate(challenge, webRequest, credentials, false);
        }

        private Authorization DoAuthenticate(string challenge, WebRequest webRequest, ICredentials credentials, bool preAuthenticate) {
            GlobalLog.Print("DigestClient::DoAuthenticate() challenge:[" + ValidationHelper.ToString(challenge) + "] webRequest#" + ValidationHelper.HashString(webRequest) + " credentials#" + ValidationHelper.HashString(credentials) + " preAuthenticate:" + preAuthenticate.ToString());

            GlobalLog.Assert(credentials != null, "DigestClient::DoAuthenticate()|credentials == null");
            if (credentials==null) {
                return null;
            }

            HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;
            GlobalLog.Assert(httpWebRequest != null, "DigestClient::DoAuthenticate()|httpWebRequest == null");
            GlobalLog.Assert(httpWebRequest.ChallengedUri != null, "DigestClient::DoAuthenticate()|httpWebRequest.ChallengedUri == null");

            // If it's default credentials, we support them on XP and up through WDigest.


            NetworkCredential NC = credentials.GetCredential(httpWebRequest.ChallengedUri, DigestClient.Signature);
            GlobalLog.Print("DigestClient::DoAuthenticate() GetCredential() returns:" + ValidationHelper.ToString(NC));

            if (NC is SystemNetworkCredential) {
                if (WDigestAvailable) {
                    return XPDoAuthenticate(challenge, httpWebRequest, credentials, preAuthenticate);
                }
                else {
                    return null;
                }
            }

            HttpDigestChallenge digestChallenge;
            if (!preAuthenticate) {
                int index = AuthenticationManager.FindSubstringNotInQuotes(challenge, Signature);
                if (index < 0) {
                    return null;
                }
                digestChallenge = HttpDigest.Interpret(challenge, index, httpWebRequest);
            }
            else {
                GlobalLog.Print("DigestClient::DoAuthenticate() looking up digestChallenge for prefix:" + httpWebRequest.ChallengedUri.AbsoluteUri);
                digestChallenge = challengeCache.Lookup(httpWebRequest.ChallengedUri.AbsoluteUri) as HttpDigestChallenge;
            }
            if (digestChallenge==null) {
                return null;
            }

            bool supported = CheckQOP(digestChallenge);
            if (!supported) {
                if (Logging.On)
                    Logging.PrintError(Logging.Web, SR.GetString(SR.net_log_digest_qop_not_supported, digestChallenge.QualityOfProtection));
                return null;
            }

            if (preAuthenticate) {
                GlobalLog.Print("DigestClient::DoAuthenticate() retrieved digestChallenge#" + ValidationHelper.HashString(digestChallenge) + " digestChallenge for prefix:" + httpWebRequest.ChallengedUri.AbsoluteUri);
                digestChallenge = digestChallenge.CopyAndIncrementNonce();
                digestChallenge.SetFromRequest(httpWebRequest);
            }

            if (NC==null) {
                return null;
            }

            ICredentialPolicy policy = AuthenticationManager.CredentialPolicy;
            if (policy != null && !policy.ShouldSendCredential(httpWebRequest.ChallengedUri, httpWebRequest, NC, this))
                return null;

            SpnToken spnToken = httpWebRequest.CurrentAuthenticationState.GetComputeSpn(httpWebRequest);

            ChannelBinding binding = null;
            if (httpWebRequest.CurrentAuthenticationState.TransportContext != null)
            {
                binding = httpWebRequest.CurrentAuthenticationState.TransportContext.GetChannelBinding(ChannelBindingKind.Endpoint);
            }

            Authorization digestResponse = HttpDigest.Authenticate(digestChallenge, NC, spnToken.Spn, binding);
            if (!preAuthenticate && webRequest.PreAuthenticate && digestResponse != null) {
                // add this to the cache of challenges so we can preauthenticate
                string[] prefixes = digestChallenge.Domain==null ?
                        new string[]{httpWebRequest.ChallengedUri.GetParts(UriComponents.SchemeAndServer, UriFormat.UriEscaped)}
                        : digestChallenge.Domain.Split(singleSpaceArray);

                // If Domain property is not set we associate Digest only with the server Uri namespace (used to do with whole server)
                digestResponse.ProtectionRealm = digestChallenge.Domain==null ? null: prefixes;

                for (int i=0; i<prefixes.Length; i++) {
                    GlobalLog.Print("DigestClient::DoAuthenticate() adding digestChallenge#" + ValidationHelper.HashString(digestChallenge) + " for prefix:" + prefixes[i]);
                    challengeCache.Add(prefixes[i], digestChallenge);
                }
            }
            return digestResponse;
        }

        public Authorization PreAuthenticate(WebRequest webRequest, ICredentials credentials) {
            GlobalLog.Print("DigestClient::PreAuthenticate() webRequest#" + ValidationHelper.HashString(webRequest) + " credentials#" + ValidationHelper.HashString(credentials) + " calling DoAuthenticate()");
            return DoAuthenticate(null, webRequest, credentials, true);
        }

        public bool CanPreAuthenticate {
            get {
                return true;
            }
        }

        public string AuthenticationType {
            get {
                return AuthType;
            }
        }

        internal static bool CheckQOP(HttpDigestChallenge challenge) {
            // our internal implementatoin only support "auth" QualityOfProtection.
            // if it's not what the server wants we'll have to throw:
            // the case in which the server sends no qop directive we default to "auth"
            if (challenge.QopPresent) {
                int index = 0;
                while (index>=0) {
                    // find the next occurence of "auth"
                    index = challenge.QualityOfProtection.IndexOf(HttpDigest.SupportedQuality, index);
                    if (index<0) {
                        return false;
                    }
                    // if it's a whole word we're done
                    if ((index==0 || HttpDigest.ValidSeparator.IndexOf(challenge.QualityOfProtection[index - 1])>=0) &&
                        (index+HttpDigest.SupportedQuality.Length==challenge.QualityOfProtection.Length || HttpDigest.ValidSeparator.IndexOf(challenge.QualityOfProtection[index + HttpDigest.SupportedQuality.Length])>=0) ) {
                        break;
                    }
                    index += HttpDigest.SupportedQuality.Length;
                }
            }
            return true;
        }

        public bool Update(string challenge, WebRequest webRequest) {
            GlobalLog.Print("DigestClient::Update(): [" + challenge + "]");
            HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;

            GlobalLog.Assert(httpWebRequest!=null, "DigestClient::Update()|httpWebRequest == null");
            GlobalLog.Assert(httpWebRequest.ChallengedUri != null, "DigestClient::Update()|httpWebRequest.ChallengedUri == null");

            // make sure WDigest fixes these bugs before we enable this code ("Windows OS"):
            // 921024   1   WDigest should support MD5, at least for explicit (non-default) credentials.
            // 762116   2   WDigest should ignore directives that do not have a value
            // 762115   3   WDigest should allow an application to retrieve the parsed domain directive
            if (httpWebRequest.CurrentAuthenticationState.GetSecurityContext(this) != null) {
                return XPUpdate(challenge, httpWebRequest);
            }

            // here's how we know if the handshake is complete when we get the response back,
            // (keeping in mind that we need to support stale credentials):
            // !40X - complete & success
            // 40X & stale=false - complete & failure
            // 40X & stale=true - !complete

            if (httpWebRequest.ResponseStatusCode!=httpWebRequest.CurrentAuthenticationState.StatusCodeMatch) {
                GlobalLog.Print("DigestClient::Update(): no status code match. returning true");

                ChannelBinding binding = null;
                if (httpWebRequest.CurrentAuthenticationState.TransportContext != null)
                {
                    binding = httpWebRequest.CurrentAuthenticationState.TransportContext.GetChannelBinding(ChannelBindingKind.Endpoint);
                }
                httpWebRequest.ServicePoint.SetCachedChannelBinding(httpWebRequest.ChallengedUri, binding);

                return true;
            }

            int index = challenge==null ? -1 : AuthenticationManager.FindSubstringNotInQuotes(challenge, Signature);
            if (index < 0) {
                GlobalLog.Print("DigestClient::Update(): no challenge. returning true");
                return true;
            }

            int blobBegin = index + SignatureSize;
            string incoming = null;

            //
            // there may be multiple challenges. If the next character after the
            // package name is not a comma then it is challenge data
            //
            if (challenge.Length > blobBegin && challenge[blobBegin] != ',') {
                ++blobBegin;
            }
            else {
                index = -1;
            }
            if (index >= 0 && challenge.Length > blobBegin) {
                incoming = challenge.Substring(blobBegin);
            }

            HttpDigestChallenge digestChallenge = HttpDigest.Interpret(challenge, index, httpWebRequest);
            if (digestChallenge==null) {
                GlobalLog.Print("DigestClient::Update(): not a valid digest challenge. returning true");
                return true;
            }

            GlobalLog.Print("DigestClient::Update(): returning digestChallenge.Stale:" + digestChallenge.Stale.ToString());
            return !digestChallenge.Stale;
        }

        public bool CanUseDefaultCredentials {
            get {
                return WDigestAvailable;
            }
        }

        internal static bool WDigestAvailable {
            get {
                return _WDigestAvailable;
            }
        }

        public void ClearSession(WebRequest webRequest) {
            HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;
            GlobalLog.Assert(httpWebRequest != null, "NtlmClient::ClearSession()|httpWebRequest == null");
            // when we're using WDigest.dll we need to keep the NTAuthentication instance around, since it's in the
            // challengeCache so remove the reference in the AuthenticationState to avoid closing it in ClearSession
#if WDIGEST_PREAUTH
            httpWebRequest.CurrentAuthenticationState.SetSecurityContext(null, this);
#else
            httpWebRequest.CurrentAuthenticationState.ClearSession();
#endif
        }


        // On Windows XP and up, WDigest.dll supports the Digest authentication scheme (in addition to
        // support for HTTP client sides, it also supports HTTP server side and SASL) through SSPI.

        private Authorization XPDoAuthenticate(string challenge, HttpWebRequest httpWebRequest, ICredentials credentials, bool preAuthenticate) {
            GlobalLog.Print("DigestClient::XPDoAuthenticate() challenge:[" + ValidationHelper.ToString(challenge) + "] httpWebRequest#" + ValidationHelper.HashString(httpWebRequest) + " credentials#" + ValidationHelper.HashString(credentials) + " preAuthenticate:" + preAuthenticate.ToString());

            NTAuthentication authSession = null;
            string incoming = null;
            int index;

            if (!preAuthenticate) {
                index = AuthenticationManager.FindSubstringNotInQuotes(challenge, Signature);
                if (index < 0) {
                    return null;
                }
                authSession = httpWebRequest.CurrentAuthenticationState.GetSecurityContext(this);
                GlobalLog.Print("DigestClient::XPDoAuthenticate() key:" + ValidationHelper.HashString(httpWebRequest.CurrentAuthenticationState) + " retrieved authSession:" + ValidationHelper.HashString(authSession));
                incoming = RefineDigestChallenge(challenge, index);
            }
            else {
#if WDIGEST_PREAUTH
                GlobalLog.Print("DigestClient::XPDoAuthenticate() looking up digestChallenge for prefix:" + httpWebRequest.ChallengedUri.AbsoluteUri);
                authSession = challengeCache.Lookup(httpWebRequest.ChallengedUri.AbsoluteUri) as NTAuthentication;
                if (authSession==null) {
                    return null;
                }
#else
                GlobalLog.Print("DigestClient::XPDoAuthenticate() looking up digestChallenge for prefix:" + httpWebRequest.ChallengedUri.AbsoluteUri);
                HttpDigestChallenge digestChallenge = challengeCache.Lookup(httpWebRequest.ChallengedUri.AbsoluteUri) as HttpDigestChallenge;
                if (digestChallenge==null) {
                    return null;
                }

                GlobalLog.Print("DigestClient::XPDoAuthenticate() retrieved digestChallenge#" + ValidationHelper.HashString(digestChallenge) + " digestChallenge for prefix:" + httpWebRequest.ChallengedUri.AbsoluteUri);
                digestChallenge = digestChallenge.CopyAndIncrementNonce();
                digestChallenge.SetFromRequest(httpWebRequest);
                incoming = digestChallenge.ToBlob();
#endif
            }

            UriComponents uriParts = 0;
            Uri remoteUri = httpWebRequest.GetRemoteResourceUri();
            if (httpWebRequest.CurrentMethod.ConnectRequest) {
                uriParts = UriComponents.HostAndPort;
                // Use the orriginal request Uri, not the proxy Uri
                remoteUri = httpWebRequest.RequestUri;
            }
            else {
                uriParts = UriComponents.PathAndQuery;
            }
            // here we use Address instead of ChallengedUri. This is because the
            // Digest hash is generated using the uri as it is present on the wire
            string rawUri = remoteUri.GetParts(uriParts, UriFormat.UriEscaped);
            GlobalLog.Print("DigestClient::XPDoAuthenticate() rawUri:" + ValidationHelper.ToString(rawUri));

            if (authSession==null) {
                NetworkCredential NC = credentials.GetCredential(httpWebRequest.ChallengedUri, Signature);
                GlobalLog.Print("DigestClient::XPDoAuthenticate() GetCredential() returns:" + ValidationHelper.ToString(NC));

                if (NC == null || (!(NC is SystemNetworkCredential) && NC.InternalGetUserName().Length == 0))
                {
                    return null;
                }

                ICredentialPolicy policy = AuthenticationManager.CredentialPolicy;
                if (policy != null && !policy.ShouldSendCredential(httpWebRequest.ChallengedUri, httpWebRequest, NC, this))
                    return null;

                SpnToken spn = httpWebRequest.CurrentAuthenticationState.GetComputeSpn(httpWebRequest);
                GlobalLog.Print("NtlmClient::Authenticate() ChallengedSpn:" + ValidationHelper.ToString(spn));

                ChannelBinding binding = null;
                if (httpWebRequest.CurrentAuthenticationState.TransportContext != null)
                {
                    binding = httpWebRequest.CurrentAuthenticationState.TransportContext.GetChannelBinding(ChannelBindingKind.Endpoint);
                }

                authSession =
                    new NTAuthentication(
                        NegotiationInfoClass.WDigest,
                        NC,
                        spn,
                        httpWebRequest,
                        binding);

                GlobalLog.Print("DigestClient::XPDoAuthenticate() setting SecurityContext for:" + ValidationHelper.HashString(httpWebRequest.CurrentAuthenticationState) + " to authSession:" + ValidationHelper.HashString(authSession));
                httpWebRequest.CurrentAuthenticationState.SetSecurityContext(authSession, this);
            }

            SecurityStatus statusCode;
            string clientResponse;

            GlobalLog.Print("DigestClient::XPDoAuthenticate() incoming:" + ValidationHelper.ToString(incoming));

#if WDIGEST_PREAUTH
            clientResponse = authSession.GetOutgoingDigestBlob(incoming, httpWebRequest.CurrentMethod.Name, rawUri, null, preAuthenticate, true, out statusCode);
#else
            clientResponse = authSession.GetOutgoingDigestBlob(incoming, httpWebRequest.CurrentMethod.Name, rawUri, null, false, false, out statusCode);
#endif
            if (clientResponse == null)
                return null;

            GlobalLog.Print("DigestClient::XPDoAuthenticate() GetOutgoingDigestBlob(" + incoming + ") returns:" + ValidationHelper.ToString(clientResponse));

            Authorization digestResponse = new Authorization(AuthType + " " + clientResponse, authSession.IsCompleted, string.Empty, authSession.IsMutualAuthFlag);

            if (!preAuthenticate && httpWebRequest.PreAuthenticate) {
                // add this to the cache of challenges so we can preauthenticate
                // use this DCR when avaialble to do this without calling HttpDigest.Interpret():
                // 762115   3   WDigest should allow an application to retrieve the parsed domain directive
                HttpDigestChallenge digestChallenge = HttpDigest.Interpret(incoming, -1, httpWebRequest);

                string[] prefixes = digestChallenge.Domain==null ?
                        new string[]{httpWebRequest.ChallengedUri.GetParts(UriComponents.SchemeAndServer, UriFormat.UriEscaped)}
                        : digestChallenge.Domain.Split(singleSpaceArray);

                // If Domain property is not set we associate Digest only with the server Uri namespace (used to do with whole server)
                digestResponse.ProtectionRealm = digestChallenge.Domain==null ? null: prefixes;

                for (int i=0; i<prefixes.Length; i++) {
                    GlobalLog.Print("DigestClient::XPDoAuthenticate() adding authSession#" + ValidationHelper.HashString(authSession) + " for prefix:" + prefixes[i]);
#if WDIGEST_PREAUTH
                    challengeCache.Add(prefixes[i], authSession);
#else
                    challengeCache.Add(prefixes[i], digestChallenge);
#endif
                }
            }
            return digestResponse;
        }


        private bool XPUpdate(string challenge, HttpWebRequest httpWebRequest) {
            GlobalLog.Print("DigestClient::XPUpdate(): " + challenge);

            NTAuthentication authSession = httpWebRequest.CurrentAuthenticationState.GetSecurityContext(this);
            GlobalLog.Print("DigestClient::XPUpdate() key:" + ValidationHelper.HashString(httpWebRequest.CurrentAuthenticationState) + " retrieved authSession:" + ValidationHelper.HashString(authSession));
            if (authSession==null) {
                return false;
            }

            int index = challenge==null ? -1 : AuthenticationManager.FindSubstringNotInQuotes(challenge, Signature);
            if (index < 0) {
                GlobalLog.Print("DigestClient::XPUpdate(): no challenge. returning true");

                // Extract the CBT we used and cache it for future requests that want to do preauth
                httpWebRequest.ServicePoint.SetCachedChannelBinding(httpWebRequest.ChallengedUri, authSession.ChannelBinding);

                ClearSession(httpWebRequest);
                return true;
            }

            // here's how we know if the handshake is complete when we get the response back,
            // (keeping in mind that we need to support stale credentials):
            // !40X - complete & success
            // 40X & stale=false - complete & failure
            // 40X & stale=true - !complete

            if (httpWebRequest.ResponseStatusCode!=httpWebRequest.CurrentAuthenticationState.StatusCodeMatch) {
                GlobalLog.Print("DigestClient::XPUpdate(): no status code match. returning true");

                // Extract the CBT we used and cache it for future requests that want to do preauth
                httpWebRequest.ServicePoint.SetCachedChannelBinding(httpWebRequest.ChallengedUri, authSession.ChannelBinding);

                ClearSession(httpWebRequest);
                return true;
            }

            string incoming = RefineDigestChallenge(challenge, index);
            GlobalLog.Print("DigestClient::XPDoAuthenticate() incoming:" + ValidationHelper.ToString(incoming));

            // we should get here only on invalid or stale credentials:
            SecurityStatus statusCode;
            string clientResponse = authSession.GetOutgoingDigestBlob(incoming, httpWebRequest.CurrentMethod.Name, null, null, false, true, out statusCode);
            httpWebRequest.CurrentAuthenticationState.Authorization.MutuallyAuthenticated = authSession.IsMutualAuthFlag;

            GlobalLog.Print("DigestClient::XPUpdate() GetOutgoingDigestBlob(" + incoming + ") returns:" + ValidationHelper.ToString(clientResponse));
            GlobalLog.Assert(authSession.IsCompleted, "DigestClient::XPUpdate()|IsCompleted == false");
            GlobalLog.Print("DigestClient::XPUpdate() GetOutgoingBlob() returns clientResponse:[" + ValidationHelper.ToString(clientResponse) + "] IsCompleted:" + authSession.IsCompleted.ToString());

            return authSession.IsCompleted;
        }

        //
        // Extract digest relevant part from a raw server blob
        //
        private static string RefineDigestChallenge(string challenge, int index)
        {
            string incoming = null;

            Debug.Assert(challenge != null);
            Debug.Assert(index >= 0 && index < challenge.Length);

            int blobBegin = index + SignatureSize;

            //
            // there may be multiple challenges. If the next character after the
            // package name is not a comma then it is challenge data
            //
            if (challenge.Length > blobBegin && challenge[blobBegin] != ',') {
                ++blobBegin;
            }
            else {
                index = -1;
            }

            if (index >= 0 && challenge.Length > blobBegin) {
                incoming = challenge.Substring(blobBegin);
            }
            else
            {
                Logging.PrintError(Logging.Web, SR.GetString(SR.net_log_auth_invalid_challenge, DigestClient.AuthType));
                return String.Empty; // Error, no valid digest challenge, no further processing required
            }

            // now make sure there's nothing at the end of the challenge that is not part of the digest challenge
            // this would happen if I have a Digest challenge followed by another challenge like ",NTLM,Negotiate"
            // use this DCR when avaialble to do this without parsing:
            // 762116   2   WDigest should ignore directives that do not have a value
            int startingPoint = 0;
            int start = startingPoint;
            int offset;
            bool valid = true;
            string name, value;
            HttpDigestChallenge digestChallenge = new HttpDigestChallenge();
            for (;;) {
                offset = start;
                index = AuthenticationManager.SplitNoQuotes(incoming, ref offset);
                GlobalLog.Print("DigestClient::XPDoAuthenticate() SplitNoQuotes() returning index:" + index + " offset:" + offset);
                if (offset<0) {
                    break;
                }
                name = incoming.Substring(start, offset-start);
                if (index<0) {
                    value = HttpDigest.unquote(incoming.Substring(offset+1));
                }
                else {
                    value = HttpDigest.unquote(incoming.Substring(offset+1, index-offset-1));
                }
                valid = digestChallenge.defineAttribute(name, value);
                GlobalLog.Print("DigestClient::XPDoAuthenticate() defineAttribute(" + name + ", " + value + ") returns " + valid);
                if (index<0 || !valid) {
                    break;
                }
                start = ++index;
            }
            GlobalLog.Print("DigestClient::XPDoAuthenticate() start:" + start + " offset:" + offset + " index:" + index + " valid:" + valid + " incoming.Length:" + incoming.Length + " incoming:" + incoming);
            if ((!valid || offset<0) && start<incoming.Length) {
                incoming = start > 0 ? incoming.Substring(0, start-1) : ""; // First parameter might have been invalid, leaving start at 0
            }
            return incoming;
        }
    }

    internal class HttpDigestChallenge {

        // General authentication related information
        internal string   HostName;
        internal string   Realm;
        internal Uri      ChallengedUri;

        // Digest specific fields
        internal string   Uri;
        internal string   Nonce;
        internal string   Opaque;
        internal bool     Stale;
        internal string   Algorithm;
        internal string   Method;
        internal string   Domain;
        internal string   QualityOfProtection;
        internal string   ClientNonce;
        internal int      NonceCount;
        internal string   Charset;
        internal string   ServiceName;
        internal string   ChannelBinding;

        internal bool     UTF8Charset;
        internal bool     QopPresent;

        internal MD5CryptoServiceProvider MD5provider = new MD5CryptoServiceProvider();

        internal void SetFromRequest(HttpWebRequest httpWebRequest) {
            this.HostName = httpWebRequest.ChallengedUri.Host;
            this.Method = httpWebRequest.CurrentMethod.Name;

            if (httpWebRequest.CurrentMethod.ConnectRequest) {
                // Use the orriginal request Uri, not the proxy Uri
                this.Uri = httpWebRequest.RequestUri.GetParts(UriComponents.HostAndPort, UriFormat.UriEscaped);
            }
            else {
                // Don't use PathAndQuery, it breaks IIS6
                // GetParts(Path) doesn't return the initial slash
                this.Uri = "/" + httpWebRequest.GetRemoteResourceUri().GetParts(UriComponents.Path, 
                    UriFormat.UriEscaped);
            }

            this.ChallengedUri = httpWebRequest.ChallengedUri;
        }

        internal HttpDigestChallenge CopyAndIncrementNonce() {
            HttpDigestChallenge challengeCopy = null;
            lock(this) {
                challengeCopy = this.MemberwiseClone() as HttpDigestChallenge;
                ++NonceCount;
            }
            challengeCopy.MD5provider = new MD5CryptoServiceProvider();
            return challengeCopy;
        }

        public bool defineAttribute(string name, string value) {
            name = name.Trim().ToLower(CultureInfo.InvariantCulture);
            if (name.Equals(HttpDigest.DA_algorithm)) {
                Algorithm = value;
            }
            else if (name.Equals(HttpDigest.DA_cnonce)) {
                ClientNonce = value;
            }
            else if (name.Equals(HttpDigest.DA_nc)) {
                NonceCount = Int32.Parse(value, NumberFormatInfo.InvariantInfo);
            }
            else if (name.Equals(HttpDigest.DA_nonce)) {
                Nonce = value;
            }
            else if (name.Equals(HttpDigest.DA_opaque)) {
                Opaque = value;
            }
            else if (name.Equals(HttpDigest.DA_qop)) {
                QualityOfProtection = value;
                QopPresent = QualityOfProtection!=null && QualityOfProtection.Length>0;
            }
            else if (name.Equals(HttpDigest.DA_realm)) {
                Realm = value;
            }
            else if (name.Equals(HttpDigest.DA_domain)) {
                Domain = value;
            }
            else if (name.Equals(HttpDigest.DA_response)) {
            }
            else if (name.Equals(HttpDigest.DA_stale)) {
                Stale = value.ToLower(CultureInfo.InvariantCulture).Equals("true");
            }
            else if (name.Equals(HttpDigest.DA_uri)) {
                Uri = value;
            }
            else if (name.Equals(HttpDigest.DA_charset)) {
                Charset = value;
            }
            else if (name.Equals(HttpDigest.DA_cipher)) {
                // ignore
            }
            else if (name.Equals(HttpDigest.DA_username)) {
                // ignore
            }
            else {
                //
                // the token is not recognized, this usually
                // happens when there are multiple challenges
                //
                return false;
            }
            return true;
        }

        internal string ToBlob() {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(HttpDigest.pair(HttpDigest.DA_realm, Realm, true));
            if (Algorithm!=null) {
                stringBuilder.Append(",");
                stringBuilder.Append(HttpDigest.pair(HttpDigest.DA_algorithm, Algorithm, true));
            }
            if (Charset!=null) {
                stringBuilder.Append(",");
                stringBuilder.Append(HttpDigest.pair(HttpDigest.DA_charset, Charset, false));
            }
            if (Nonce!=null) {
                stringBuilder.Append(",");
                stringBuilder.Append(HttpDigest.pair(HttpDigest.DA_nonce, Nonce, true));
            }
            if (Uri!=null) {
                stringBuilder.Append(",");
                stringBuilder.Append(HttpDigest.pair(HttpDigest.DA_uri, Uri, true));
            }
            if (ClientNonce!=null) {
                stringBuilder.Append(",");
                stringBuilder.Append(HttpDigest.pair(HttpDigest.DA_cnonce, ClientNonce, true));
            }
            if (NonceCount>0) {
                stringBuilder.Append(",");
                stringBuilder.Append(HttpDigest.pair(HttpDigest.DA_nc, NonceCount.ToString("x8", NumberFormatInfo.InvariantInfo), true));
            }
            if (QualityOfProtection!=null) {
                stringBuilder.Append(",");
                stringBuilder.Append(HttpDigest.pair(HttpDigest.DA_qop, QualityOfProtection, true));
            }
            if (Opaque!=null) {
                stringBuilder.Append(",");
                stringBuilder.Append(HttpDigest.pair(HttpDigest.DA_opaque, Opaque, true));
            }
            if (Domain!=null) {
                stringBuilder.Append(",");
                stringBuilder.Append(HttpDigest.pair(HttpDigest.DA_domain, Domain, true));
            }
            if (Stale) {
                stringBuilder.Append(",");
                stringBuilder.Append(HttpDigest.pair(HttpDigest.DA_stale, "true", true));
            }
            return stringBuilder.ToString();
        }

    }


    internal static class HttpDigest {
        //
        // these are the tokens defined by Digest
        // http://www.ietf.org/rfc/rfc2831.txt
        //
        internal const string DA_algorithm  = "algorithm";
        internal const string DA_cnonce     = "cnonce"; // client-nonce
        internal const string DA_domain     = "domain";
        internal const string DA_nc         = "nc"; // nonce-count
        internal const string DA_nonce      = "nonce";
        internal const string DA_opaque     = "opaque";
        internal const string DA_qop        = "qop"; // quality-of-protection
        internal const string DA_realm      = "realm";
        internal const string DA_response   = "response";
        internal const string DA_stale      = "stale";
        internal const string DA_uri        = "uri";
        internal const string DA_username   = "username";
        internal const string DA_charset    = "charset";
        internal const string DA_cipher     = "cipher";

        // directives specific to CBT.  hashed-dirs contains a comma-separated list of directives
        // that have been hashed into the client nonce.  service-name contains the client-provided
        // SPN.  channel-binding contains the hex-encoded MD5 hash of the channel binding token.
        internal const string DA_hasheddirs = "hashed-dirs";
        internal const string DA_servicename = "service-name";
        internal const string DA_channelbinding = "channel-binding";

        internal const string SupportedQuality = "auth";
        internal const string ValidSeparator = ", \"\'\t\r\n";

        // The value of the hashed-dirs directive.  It's always "service-name,channel-binding".
        internal const string HashedDirs = DA_servicename + "," + DA_channelbinding;

        // A server which understands CBT will send a nonce with this prefix.  If we see it,
        // send a response containing the CBT directives.
        internal const string Upgraded = "+Upgraded+";

        // When sending an upgraded response, we prefix this string to the client-nonce directive
        // to let the server know.
        internal const string UpgradedV1 = Upgraded + "v1";

        // If the client application doesn't provide a ChannelBinding, this is what we send
        // as the channel-binding directive, meaning that the client had no outer secure channel.
        // See ZeroBindHash in ds/security/protocols/sspcommon/sspbindings.cxx
        internal const string ZeroChannelBindingHash = "00000000000000000000000000000000";

        private const string suppressExtendedProtectionKey = @"System\CurrentControlSet\Control\Lsa";
        private const string suppressExtendedProtectionKeyPath = @"HKEY_LOCAL_MACHINE\" + suppressExtendedProtectionKey;
        private const string suppressExtendedProtectionValueName = "SuppressExtendedProtection";

        private static volatile bool suppressExtendedProtection;

        static HttpDigest() {
            ReadSuppressExtendedProtectionRegistryValue();
        }

        [RegistryPermission(SecurityAction.Assert, Read = suppressExtendedProtectionKeyPath)]
        private static void ReadSuppressExtendedProtectionRegistryValue() {

            // In Win7 and later, the default value for SuppressExtendedProtection is 0 (enable
            // CBT support), whereas in pre-Win7 OS versions it is 1 (suppress CBT support).
            suppressExtendedProtection = !ComNetOS.IsWin7orLater;

            try {
                using (RegistryKey lsaKey = Registry.LocalMachine.OpenSubKey(suppressExtendedProtectionKey)) {

                    try
                    {
                        // We only consider value 1 (2 is only used for Kerberos and 0 means CBT should
                        // be supported). We ignore all other values.
                        if (lsaKey.GetValueKind(suppressExtendedProtectionValueName) == RegistryValueKind.DWord) {
                            suppressExtendedProtection = ((int)lsaKey.GetValue(suppressExtendedProtectionValueName)) == 1;
                        }
                    }
                    catch (UnauthorizedAccessException e) {
                        if (Logging.On) Logging.PrintWarning(Logging.Web, typeof(HttpDigest), "ReadSuppressExtendedProtectionRegistryValue", e.Message);
                    }
                    catch (IOException e) {
                        if (Logging.On) Logging.PrintWarning(Logging.Web, typeof(HttpDigest), "ReadSuppressExtendedProtectionRegistryValue", e.Message);
                    }
                }
            }
            catch (SecurityException e) {
                if (Logging.On) Logging.PrintWarning(Logging.Web, typeof(HttpDigest), "ReadSuppressExtendedProtectionRegistryValue", e.Message);
            }
            catch (ObjectDisposedException e) {
                if (Logging.On) Logging.PrintWarning(Logging.Web, typeof(HttpDigest), "ReadSuppressExtendedProtectionRegistryValue", e.Message);
            }
        }

        //
        // consider internally caching the nonces sent to us by a server so that
        // we can correctly send out nonce counts for subsequent requests

        //
        // used to create a random nonce
        //
        private static readonly RNGCryptoServiceProvider RandomGenerator = new RNGCryptoServiceProvider();
        //
        // this method parses the challenge and breaks it into the
        // fundamental pieces that Digest defines and understands
        //
        internal static HttpDigestChallenge Interpret(string challenge, int startingPoint, HttpWebRequest httpWebRequest) {
            HttpDigestChallenge digestChallenge = new HttpDigestChallenge();
            digestChallenge.SetFromRequest(httpWebRequest);
            //
            // define the part of the challenge we really care about
            //
            startingPoint = startingPoint==-1 ? 0 : startingPoint + DigestClient.SignatureSize;

            bool valid;
            int start, offset, index;
            string name, value;

            // forst time parse looking for a charset="utf-8" directive
            // not too bad, IIS 6.0, by default, sends this as the first directive.
            // if the server does not send this we'll end up parsing twice.
            start = startingPoint;
            for (;;) {
                offset = start;
                index = AuthenticationManager.SplitNoQuotes(challenge, ref offset);
                if (offset<0) {
                    break;
                }
                name = challenge.Substring(start, offset-start);
                if (string.Compare(name, DA_charset, StringComparison.OrdinalIgnoreCase)==0) {
                    if (index<0) {
                        value = unquote(challenge.Substring(offset+1));
                    }
                    else {
                        value = unquote(challenge.Substring(offset+1, index-offset-1));
                    }
                    GlobalLog.Print("HttpDigest::Interpret() server provided a hint to use [" + value + "] encoding");
                    if (string.Compare(value, "utf-8", StringComparison.OrdinalIgnoreCase)==0) {
                        digestChallenge.UTF8Charset = true;
                        break;
                    }
                }
                if (index<0) {
                    break;
                }
                start = ++index;
            }

            // this time go through the directives, parse them and call defineAttribute()
            start = startingPoint;
            for (;;) {
                offset = start;
                index = AuthenticationManager.SplitNoQuotes(challenge, ref offset);
                GlobalLog.Print("HttpDigest::Interpret() SplitNoQuotes() returning index:" + index.ToString() + " offset:" + offset.ToString());
                if (offset<0) {
                    break;
                }
                name = challenge.Substring(start, offset-start);
                if (index<0) {
                    value = unquote(challenge.Substring(offset+1));
                }
                else {
                    value = unquote(challenge.Substring(offset+1, index-offset-1));
                }
                if (digestChallenge.UTF8Charset) {
                    bool isAscii = true;
                    for (int i=0; i<value.Length; i++) {
                        if (value[i]>(char)0x7F) {
                            isAscii = false;
                            break;
                        }
                    }
                    if (!isAscii) {
                        GlobalLog.Print("HttpDigest::Interpret() UTF8 decoding required value:[" + value + "]");
                        byte[] bytes = new byte[value.Length];
                        for (int i=0; i<value.Length; i++) {
                            bytes[i] = (byte)value[i];
                        }
                        value = Encoding.UTF8.GetString(bytes);
                        GlobalLog.Print("HttpDigest::Interpret() UTF8 decoded value:[" + value + "]");
                    }
                    else {
                        GlobalLog.Print("HttpDigest::Interpret() no need for special encoding");
                    }
                }
                valid = digestChallenge.defineAttribute(name, value);
                GlobalLog.Print("HttpDigest::Interpret() defineAttribute(" + name + ", " + value + ") returns " + valid.ToString());
                if (index<0 || !valid) {
                    break;
                }
                start = ++index;
            }
            // We must absolutely have a nonce for Digest to work.
            if (digestChallenge.Nonce == null) {
                if (Logging.On)
                    Logging.PrintError(Logging.Web, SR.GetString(SR.net_log_digest_requires_nonce));
                return null;
            }

            return digestChallenge;
        }

        private enum Charset {
            ASCII,
            ANSI,
            UTF8
        }

        private static string CharsetEncode(string rawString, Charset charset) {
#if TRAVE
            GlobalLog.Print("HttpDigest::CharsetEncode() encoding rawString:[" + rawString + "] Chars(rawString):[" + Chars(rawString) + "] charset:[" + charset + "]");
#endif // #if TRAVE
            if (charset==Charset.UTF8 || charset==Charset.ANSI) {
                byte[] bytes = charset==Charset.UTF8 ? Encoding.UTF8.GetBytes(rawString) : Encoding.Default.GetBytes(rawString);
                // the following code is the same as:
                // rawString = Encoding.Default.GetString(bytes);
                // but it's faster.
                char[] chars = new char[bytes.Length];
                bytes.CopyTo(chars, 0);
                rawString = new string(chars);
            }
#if TRAVE
            GlobalLog.Print("HttpDigest::CharsetEncode() encoded rawString:[" + rawString + "] Chars(rawString):[" + Chars(rawString) + "] charset:[" + charset + "]");
#endif // #if TRAVE
            return rawString;
        }

        private static Charset DetectCharset(string rawString) {
            Charset charset = Charset.ASCII;
            for (int i=0; i<rawString.Length; i++) {
                if (rawString[i]>(char)0x7F) {
                    GlobalLog.Print("HttpDigest::DetectCharset() found non ASCII character:[" + ((int)rawString[i]).ToString() + "] at offset i:[" + i.ToString() + "] charset:[" + charset.ToString() + "]");
                    // ----, but the only way we can tell if we can use default ANSI encoding is see
                    // in the encode/decode process there is no loss of information.
                    byte[] bytes = Encoding.Default.GetBytes(rawString);
                    string rawCopy = Encoding.Default.GetString(bytes);
                    charset = string.Compare(rawString, rawCopy, StringComparison.Ordinal)==0 ? Charset.ANSI : Charset.UTF8;
                    break;
                }
            }
            GlobalLog.Print("HttpDigest::DetectCharset() rawString:[" + rawString + "] has charset:[" + charset.ToString() + "]");
            return charset;
        }

#if TRAVE
        private static string Chars(string rawString) {
            string returnString = "[";
            for (int i=0; i<rawString.Length; i++) {
                if (i>0) {
                    returnString += ",";
                }
                returnString += ((int)rawString[i]).ToString();
            }
            return returnString + "]";
        }
#endif // #if TRAVE

        //
        // CONSIDER V.NEXT
        // creating a static hashtable for server nonces and keep track of nonce count
        //
        internal static Authorization Authenticate(HttpDigestChallenge digestChallenge, NetworkCredential NC, string spn, ChannelBinding binding) {

            string username = NC.InternalGetUserName();
            if (ValidationHelper.IsBlankString(username)) {
                return null;
            }
            string password = NC.InternalGetPassword();

            bool upgraded = IsUpgraded(digestChallenge.Nonce, binding);
            if (upgraded)
            {
                digestChallenge.ServiceName = spn;
                digestChallenge.ChannelBinding = hashChannelBinding(binding, digestChallenge.MD5provider);
            }

            if (digestChallenge.QopPresent) {
                if (digestChallenge.ClientNonce==null || digestChallenge.Stale) {
                    GlobalLog.Print("HttpDigest::Authenticate() QopPresent:True, need new nonce. digestChallenge.ClientNonce:" + ValidationHelper.ToString(digestChallenge.ClientNonce) + " digestChallenge.Stale:" + digestChallenge.Stale.ToString());

                    if (upgraded)
                    {
                        digestChallenge.ClientNonce = createUpgradedNonce(digestChallenge);
                    }
                    else
                    {
                        digestChallenge.ClientNonce = createNonce(32);
                    }

                    digestChallenge.NonceCount = 1;
                }
                else {
                    GlobalLog.Print("HttpDigest::Authenticate() QopPresent:True, reusing nonce. digestChallenge.NonceCount:" + digestChallenge.NonceCount.ToString());
                    digestChallenge.NonceCount++;
                }
            }

            StringBuilder authorization = new StringBuilder();

            //
            // look at username & password, if it's not ASCII we need to attempt some
            // kind of encoding because we need to calculate the hash on byte[]
            //
            Charset usernameCharset = DetectCharset(username);
            if (!digestChallenge.UTF8Charset && usernameCharset==Charset.UTF8) {
                GlobalLog.Print("HttpDigest::Authenticate() can't authenticate with UNICODE username. failing auth.");
                return null;
            }
            Charset passwordCharset = DetectCharset(password);
            if (!digestChallenge.UTF8Charset && passwordCharset==Charset.UTF8) {
                GlobalLog.Print("HttpDigest::Authenticate() can't authenticate with UNICODE password. failing auth.");
                return null;
            }
            if (digestChallenge.UTF8Charset) {
                // on the wire always use UTF8 when the server supports it
                authorization.Append(pair(DA_charset, "utf-8", false));
                authorization.Append(",");
                if (usernameCharset==Charset.UTF8) {
                    username = CharsetEncode(username, Charset.UTF8);
                    authorization.Append(pair(DA_username, username, true));
                    authorization.Append(",");
                }
                else {
                    authorization.Append(pair(DA_username, CharsetEncode(username, Charset.UTF8), true));
                    authorization.Append(",");
                    username = CharsetEncode(username, usernameCharset);
                }
            }
            else {
                // otherwise UTF8 is not required
                username = CharsetEncode(username, usernameCharset);
                authorization.Append(pair(DA_username, username, true));
                authorization.Append(",");
            }

            password = CharsetEncode(password, passwordCharset);

            // no special encoding for the realm since we're just going to echo it back (encoding must have happened on the server).
            authorization.Append(pair(DA_realm, digestChallenge.Realm, true));
            authorization.Append(",");
            authorization.Append(pair(DA_nonce, digestChallenge.Nonce, true));
            authorization.Append(",");
            authorization.Append(pair(DA_uri, digestChallenge.Uri, true));

            if (digestChallenge.QopPresent) {
                if (digestChallenge.Algorithm!=null) {
                    // consider: should we default to "MD5" here? IE does
                    authorization.Append(",");
                    authorization.Append(pair(DA_algorithm, digestChallenge.Algorithm, true)); // IE sends quotes - IIS needs them
                }
                authorization.Append(",");
                authorization.Append(pair(DA_cnonce, digestChallenge.ClientNonce, true));
                authorization.Append(",");
                authorization.Append(pair(DA_nc, digestChallenge.NonceCount.ToString("x8", NumberFormatInfo.InvariantInfo), false));
                // RAID#47397
                // send only the QualityOfProtection we're using
                // since we support only "auth" that's what we will send out
                authorization.Append(",");
                authorization.Append(pair(DA_qop, SupportedQuality, true)); // IE sends quotes - IIS needs them

                if (upgraded)
                {
                    authorization.Append(",");
                    authorization.Append(pair(DA_hasheddirs, HashedDirs, true));
                    authorization.Append(",");
                    authorization.Append(pair(DA_servicename, digestChallenge.ServiceName, true));
                    authorization.Append(",");
                    authorization.Append(pair(DA_channelbinding, digestChallenge.ChannelBinding, true));
                }
            }

            // warning: this must be computed here
            string responseValue = HttpDigest.responseValue(digestChallenge, username, password);
            if (responseValue==null) {
                return null;
            }

            authorization.Append(",");
            authorization.Append(pair(DA_response, responseValue, true)); // IE sends quotes - IIS needs them

            if (digestChallenge.Opaque!=null) {
                authorization.Append(",");
                authorization.Append(pair(DA_opaque, digestChallenge.Opaque, true));
            }

            GlobalLog.Print("HttpDigest::Authenticate() digestChallenge.Stale:" + digestChallenge.Stale.ToString());

            // completion is decided in Update()
            Authorization finalAuthorization = new Authorization(DigestClient.AuthType + " " + authorization.ToString(), false);

            return finalAuthorization;
        }

        private static bool IsUpgraded(string nonce, ChannelBinding binding) {

            GlobalLog.Assert(nonce != null, "HttpDigest::IsUpgraded()|'nonce' must not be null.");

            // Digest-SSP ignores the SuppressExtendedProtection Registry value, if the the caller
            // passes a channel binding. I.e. we must consider SuppressExtendedProtection only if
            // there is no channel binding (e.g. in the http:// case).
            if ((binding == null) && (suppressExtendedProtection)) {
                return false;
            }

            // Extended Protection is only possible if both the SSPs on the current system support
            // EP and the server sent a 'nonce' containing the +Upgraded+ prefix.
            return AuthenticationManager.SspSupportsExtendedProtection &&
                nonce.StartsWith(Upgraded, StringComparison.Ordinal);
        }

        internal static string unquote(string quotedString) {
            return quotedString.Trim().Trim("\"".ToCharArray());
        }

        // Returns the string consisting of <name> followed by
        // an equal sign, followed by the <value> in double-quotes
        internal static string pair(string name, string value, bool quote) {
            if (quote) {
                return name + "=\"" + value + "\"";
            }
            return name + "=" + value;
        }

        //
        // this method computes the response-value according to the
        // rules described in RFC2831 section 2.1.2.1
        //
        private static string responseValue(HttpDigestChallenge challenge, string username, string password) {
            string secretString = computeSecret(challenge, username, password);
            if (secretString == null) {
                return null;
            }

            // we assume auth here, since it's the only one we support, the check happened earlier
            string dataString = challenge.Method + ":" + challenge.Uri;
            if (dataString == null) {
                return null;
            }

            string secret = hashString(secretString, challenge.MD5provider);
            string hexMD2 = hashString(dataString, challenge.MD5provider);

            string data =
                challenge.Nonce + ":" +
                    (challenge.QopPresent ?
                        challenge.NonceCount.ToString("x8", NumberFormatInfo.InvariantInfo) + ":" +
                        challenge.ClientNonce + ":" +
                        SupportedQuality + ":" + // challenge.QualityOfProtection + ":" +
                        hexMD2
                        :
                        hexMD2);

            return hashString(secret + ":" + data, challenge.MD5provider);
        }

        private static string computeSecret(HttpDigestChallenge challenge, string username, string password) {
            if (challenge.Algorithm==null || string.Compare(challenge.Algorithm, "md5" ,StringComparison.OrdinalIgnoreCase)==0) {
                return username + ":" + challenge.Realm + ":" + password;
            }
            else if (string.Compare(challenge.Algorithm, "md5-sess" ,StringComparison.OrdinalIgnoreCase)==0) {
                return hashString(username + ":" + challenge.Realm + ":" + password, challenge.MD5provider) + ":" + challenge.Nonce + ":" + challenge.ClientNonce;
            }
            if (Logging.On)
                Logging.PrintError(Logging.Web, SR.GetString(SR.net_log_digest_hash_algorithm_not_supported, challenge.Algorithm));
            return null;
        }

        // Where in the SecChannelBindings struct to find these fields
        private static int InitiatorTypeOffset = (int)Marshal.OffsetOf(typeof(SecChannelBindings), "dwInitiatorAddrType");
        private static int InitiatorLengthOffset = (int)Marshal.OffsetOf(typeof(SecChannelBindings), "cbInitiatorLength");
        private static int InitiatorOffsetOffset = (int)Marshal.OffsetOf(typeof(SecChannelBindings), "dwInitiatorOffset");
        private static int AcceptorTypeOffset = (int)Marshal.OffsetOf(typeof(SecChannelBindings), "dwAcceptorAddrType");
        private static int AcceptorLengthOffset = (int)Marshal.OffsetOf(typeof(SecChannelBindings), "cbAcceptorLength");
        private static int AcceptorOffsetOffset = (int)Marshal.OffsetOf(typeof(SecChannelBindings), "dwAcceptorOffset");
        private static int ApplicationDataLengthOffset = (int)Marshal.OffsetOf(typeof(SecChannelBindings), "cbApplicationDataLength");
        private static int ApplicationDataOffsetOffset = (int)Marshal.OffsetOf(typeof(SecChannelBindings), "dwApplicationDataOffset");

        private static int SizeOfInt = Marshal.SizeOf(typeof(int));
        private static int MinimumFormattedBindingLength = 5 * SizeOfInt;

        //
        // Adapted from ComputeGssBindHash() in ds\security\protocols\sspcommon\sspbindings.cxx
        //
        // The formatted binding is:
        //   1. the initiator type and length
        //   2. the initiator data, if any
        //   3. the acceptor type and length
        //   4. the acceptor data, if any
        //   5. the application data length
        //   6. the application data, if any
        //
        private static byte[] formatChannelBindingForHash(ChannelBinding binding)
        {
            int initiatorType = Marshal.ReadInt32(binding.DangerousGetHandle(), InitiatorTypeOffset);
            int initiatorLength = Marshal.ReadInt32(binding.DangerousGetHandle(), InitiatorLengthOffset);
            int acceptorType = Marshal.ReadInt32(binding.DangerousGetHandle(), AcceptorTypeOffset);
            int acceptorLength = Marshal.ReadInt32(binding.DangerousGetHandle(), AcceptorLengthOffset);
            int applicationDataLength = Marshal.ReadInt32(binding.DangerousGetHandle(), ApplicationDataLengthOffset);

            byte[] formattedData = new byte[MinimumFormattedBindingLength + initiatorLength + acceptorLength + applicationDataLength];

            BitConverter.GetBytes(initiatorType).CopyTo(formattedData, 0);
            BitConverter.GetBytes(initiatorLength).CopyTo(formattedData, SizeOfInt);

            int offset = 2 * SizeOfInt;
            if (initiatorLength > 0)
            {
                int initiatorOffset = Marshal.ReadInt32(binding.DangerousGetHandle(), InitiatorOffsetOffset);
                Marshal.Copy(IntPtrHelper.Add(binding.DangerousGetHandle(), initiatorOffset), formattedData, offset, initiatorLength);
                offset += initiatorLength;
            }

            BitConverter.GetBytes(acceptorType).CopyTo(formattedData, offset);
            BitConverter.GetBytes(acceptorLength).CopyTo(formattedData, offset + SizeOfInt);

            offset += 2 * SizeOfInt;
            if (acceptorLength > 0)
            {
                int acceptorOffset = Marshal.ReadInt32(binding.DangerousGetHandle(), AcceptorOffsetOffset);
                Marshal.Copy(IntPtrHelper.Add(binding.DangerousGetHandle(), acceptorOffset), formattedData, offset, acceptorLength);
                offset += acceptorLength;
            }

            BitConverter.GetBytes(applicationDataLength).CopyTo(formattedData, offset);

            offset += SizeOfInt;
            if (applicationDataLength > 0)
            {
                int applicationDataOffset = Marshal.ReadInt32(binding.DangerousGetHandle(), ApplicationDataOffsetOffset);
                Marshal.Copy(IntPtrHelper.Add(binding.DangerousGetHandle(), applicationDataOffset), formattedData, offset, applicationDataLength);
            }

            return formattedData;
        }

        private static string hashChannelBinding(ChannelBinding binding, MD5CryptoServiceProvider MD5provider)
        {
            if (binding == null)
            {
                return ZeroChannelBindingHash;
            }

            byte[] formattedData = formatChannelBindingForHash(binding);
            byte[] hash = MD5provider.ComputeHash(formattedData);

            return hexEncode(hash);
        }

        private static string hashString(string myString, MD5CryptoServiceProvider MD5provider) {
            GlobalLog.Enter("HttpDigest::hashString", "[" + myString.Length.ToString() + ":" + myString + "]");
            byte[] encodedBytes = new byte[myString.Length];
            for (int i=0; i<myString.Length; i++) {
                encodedBytes[i] = (byte)myString[i];
            }
            byte[] hash = MD5provider.ComputeHash(encodedBytes);
            string hashString = hexEncode(hash);
            GlobalLog.Leave("HttpDigest::hashString", "[" + hashString.Length.ToString() + ":" + hashString + "]");
            return hashString;
        }

        private static string hexEncode(byte[] rawbytes) {
            int size = rawbytes.Length;
            char[] wa = new char[2*size];

            for (int i=0, dp=0; i<size; i++) {
                // warning: these ARE case sensitive
                wa[dp++] = Uri.HexLowerChars[rawbytes[i]>>4];
                wa[dp++] = Uri.HexLowerChars[rawbytes[i]&0x0F];
            }

            return new string(wa);
        }

        /* returns a random nonce of given length */
        private static string createNonce(int length) {
            // we'd need less (half of that), but this makes the code much simpler
            int bytesNeeded = length;
            byte[] randomBytes = new byte[bytesNeeded];
            char[] digits = new char[length];
            RandomGenerator.GetBytes(randomBytes);
            for (int i=0; i<length; i++) {
                // warning: these ARE case sensitive
                digits[i] = Uri.HexLowerChars[randomBytes[i]&0x0F];
            }
            return new string(digits);
        }

        private static string createUpgradedNonce(HttpDigestChallenge digestChallenge)
        {
            string hashMe = digestChallenge.ServiceName + ":" + digestChallenge.ChannelBinding;
            byte[] hash = digestChallenge.MD5provider.ComputeHash(Encoding.ASCII.GetBytes(hashMe));

            return UpgradedV1 + hexEncode(hash) + createNonce(32);
        }
    }

}
