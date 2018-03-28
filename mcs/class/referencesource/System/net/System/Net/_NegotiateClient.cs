//------------------------------------------------------------------------------
// <copyright file="_NegotiateClient.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System.Security.Authentication.ExtendedProtection;

    internal class NegotiateClient : ISessionAuthenticationModule {

        internal const string AuthType = "Negotiate";
        private const string negotiateHeader = "Negotiate";
        private const string negotiateSignature = "negotiate";
        private const string nego2Header = "Nego2";
        private const string nego2Signature = "nego2";

        public Authorization Authenticate(string challenge, WebRequest webRequest, ICredentials credentials) {
            GlobalLog.Print("NegotiateClient::Authenticate() challenge:[" + ValidationHelper.ToString(challenge) + "] webRequest#" + ValidationHelper.HashString(webRequest) + " credentials#" + ValidationHelper.HashString(credentials) + " calling DoAuthenticate()");
            return DoAuthenticate(challenge, webRequest, credentials, false);
        }

        private Authorization DoAuthenticate(string challenge, WebRequest webRequest, ICredentials credentials, bool preAuthenticate) {
            GlobalLog.Print("NegotiateClient::DoAuthenticate() challenge:[" + ValidationHelper.ToString(challenge) + "] webRequest#" + ValidationHelper.HashString(webRequest) + " credentials#" + ValidationHelper.HashString(credentials) + " preAuthenticate:" + preAuthenticate.ToString());

            GlobalLog.Assert(credentials != null, "NegotiateClient::DoAuthenticate()|credentials == null");
            if (credentials == null) {
                return null;
            }

            HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;

            GlobalLog.Assert(httpWebRequest != null, "NegotiateClient::DoAuthenticate()|httpWebRequest == null");
            GlobalLog.Assert(httpWebRequest.ChallengedUri != null, "NegotiateClient::DoAuthenticate()|httpWebRequest.ChallengedUri == null");

            NTAuthentication authSession = null;
            string incoming = null;
            bool useNego2 = false; // In case of pre-auth we always use "Negotiate", never "Nego2".

            if (!preAuthenticate) {
                int index = GetSignatureIndex(challenge, out useNego2);
                if (index < 0) {
                    return null;
                }

                int blobBegin = index + (useNego2 ? nego2Signature.Length : negotiateSignature.Length);

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

                if (index >= 0 && challenge.Length > blobBegin)
                {
                    // Strip other modules information in case of multiple challenges
                    // i.e do not take ", NTLM" as part of the following Negotiate blob
                    // Negotiate TlRMTVNTUAACAAAADgAOADgAAAA1wo ... MAbwBmAHQALgBjAG8AbQAAAAAA,NTLM
                    index = challenge.IndexOf(',', blobBegin);
                    if (index != -1)
                        incoming = challenge.Substring(blobBegin, index - blobBegin);
                    else
                        incoming = challenge.Substring(blobBegin);
                }

                authSession = httpWebRequest.CurrentAuthenticationState.GetSecurityContext(this);
                GlobalLog.Print("NegotiateClient::DoAuthenticate() key:" + ValidationHelper.HashString(httpWebRequest.CurrentAuthenticationState) + " retrieved authSession:" + ValidationHelper.HashString(authSession));
            }

            if (authSession==null)
            {
                // Credentials are always set for "Negotiate", never for "Nego2". A customer shouldn't even know
                // about "Nego2".
                NetworkCredential NC = credentials.GetCredential(httpWebRequest.ChallengedUri, negotiateSignature);
                GlobalLog.Print("NegotiateClient::DoAuthenticate() GetCredential() returns:" + ValidationHelper.ToString(NC));

                string username = string.Empty;
                if (NC == null || (!(NC is SystemNetworkCredential) && (username = NC.InternalGetUserName()).Length == 0))
                {
                    return null;
                }

                ICredentialPolicy policy = AuthenticationManager.CredentialPolicy;
                if (policy != null && !policy.ShouldSendCredential(httpWebRequest.ChallengedUri, httpWebRequest, NC, this))
                    return null;

                SpnToken spn = httpWebRequest.CurrentAuthenticationState.GetComputeSpn(httpWebRequest);
                GlobalLog.Print("NegotiateClient::Authenticate() ChallengedSpn:" + ValidationHelper.ToString(spn));

                ChannelBinding binding = null;
                if (httpWebRequest.CurrentAuthenticationState.TransportContext != null) 
                {
                    binding = httpWebRequest.CurrentAuthenticationState.TransportContext.GetChannelBinding(ChannelBindingKind.Endpoint);
                }

                authSession =
                    new NTAuthentication(
                        AuthType,
                        NC,
                        spn,
                        httpWebRequest,
                        binding);

                GlobalLog.Print("NegotiateClient::DoAuthenticate() setting SecurityContext for:" + ValidationHelper.HashString(httpWebRequest.CurrentAuthenticationState) + " to authSession:" + ValidationHelper.HashString(authSession));
                httpWebRequest.CurrentAuthenticationState.SetSecurityContext(authSession, this);
            }

            string clientResponse = authSession.GetOutgoingBlob(incoming);
            if (clientResponse==null) {
                return null;
            }

            bool canShareConnection = httpWebRequest.UnsafeOrProxyAuthenticatedConnectionSharing;
            if (canShareConnection) {
                httpWebRequest.LockConnection = true;
            }

            // this is the first leg of an NTLM handshake,
            // set the NtlmKeepAlive override *STRICTLY* only in this case.
            httpWebRequest.NtlmKeepAlive = incoming==null && authSession.IsValidContext && !authSession.IsKerberos;

            // If we received a "Nego2" header value from the server, we'll respond with "Nego2" in the "Authorization"
            // header. If the server sent a "Negotiate" header value or if pre-authenticate is used (i.e. the auth blob
            // is sent with the first request), we send "Negotiate" in the "Authorization" header.
            return AuthenticationManager.GetGroupAuthorization(this, (useNego2 ? nego2Header : negotiateHeader) +
                " " + clientResponse, authSession.IsCompleted, authSession, canShareConnection, authSession.IsKerberos);
        }

        public bool CanPreAuthenticate {
            get {
                return true;
            }
        }

        public Authorization PreAuthenticate(WebRequest webRequest, ICredentials credentials) {
            GlobalLog.Print("NegotiateClient::PreAuthenticate() webRequest#" + ValidationHelper.HashString(webRequest) + " credentials#" + ValidationHelper.HashString(credentials) + " calling DoAuthenticate()");
            return DoAuthenticate(null, webRequest, credentials, true);
        }

        public string AuthenticationType {
            get {
                return AuthType;
            }
        }

        //
        // called when getting the final blob on the 200 OK from the server
        //
        public bool Update(string challenge, WebRequest webRequest) {
            GlobalLog.Print("NegotiateClient::Update(): " + challenge);

            HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;

            GlobalLog.Assert(httpWebRequest != null, "NegotiateClient::Update()|httpWebRequest == null");
            GlobalLog.Assert(httpWebRequest.ChallengedUri != null, "NegotiateClient::Update()|httpWebRequest.ChallengedUri == null");

            //
            // try to retrieve the state of the ongoing handshake
            //

            NTAuthentication authSession = httpWebRequest.CurrentAuthenticationState.GetSecurityContext(this);
            GlobalLog.Print("NegotiateClient::Update() key:" + ValidationHelper.HashString(httpWebRequest.CurrentAuthenticationState) + " retrieved authSession:" + ValidationHelper.HashString(authSession));

            if (authSession==null) {
                GlobalLog.Print("NegotiateClient::Update() null session returning true");
                return true;
            }

            GlobalLog.Print("NegotiateClient::Update() authSession.IsCompleted:" + authSession.IsCompleted.ToString());

            if (!authSession.IsCompleted && httpWebRequest.CurrentAuthenticationState.StatusCodeMatch==httpWebRequest.ResponseStatusCode) {
                GlobalLog.Print("NegotiateClient::Update() still handshaking (based on status code) returning false");
                return false;
            }

            // now possibly close the ConnectionGroup after authentication is done.
            if (!httpWebRequest.UnsafeOrProxyAuthenticatedConnectionSharing) {
                GlobalLog.Print("NegotiateClient::Update() releasing ConnectionGroup:" + httpWebRequest.GetConnectionGroupLine());
                httpWebRequest.ServicePoint.ReleaseConnectionGroup(httpWebRequest.GetConnectionGroupLine());
            }

            //
            // the whole point here is to close the Security Context (this will complete the authentication handshake
            // with server authentication for schemese that support it such as Kerberos)
            //
            bool useNego2 = true;
            int index = challenge==null ? -1 : GetSignatureIndex(challenge, out useNego2);
            if (index>=0) {
                int blobBegin = index + (useNego2 ? nego2Signature.Length : negotiateSignature.Length);
                string incoming = null;

                //
                // there may be multiple challenges. If the next character after the
                // package name is not a comma then it is challenge data
                //
                if (challenge.Length > blobBegin && challenge[blobBegin] != ',') {
                    ++blobBegin;
                } else {
                    index = -1;
                }
                if (index >= 0 && challenge.Length > blobBegin) {
                    incoming = challenge.Substring(blobBegin);
                }
                GlobalLog.Print("NegotiateClient::Update() this must be a final incoming blob:[" + ValidationHelper.ToString(incoming) + "]");
                string clientResponse = authSession.GetOutgoingBlob(incoming);
                httpWebRequest.CurrentAuthenticationState.Authorization.MutuallyAuthenticated = authSession.IsMutualAuthFlag;
                GlobalLog.Print("NegotiateClient::Update() GetOutgoingBlob() returns clientResponse:[" + ValidationHelper.ToString(clientResponse) + "] IsCompleted:" + authSession.IsCompleted.ToString());
            }

            // Extract the CBT we used and cache it for future requests that want to do preauth
            httpWebRequest.ServicePoint.SetCachedChannelBinding(httpWebRequest.ChallengedUri, authSession.ChannelBinding);

            GlobalLog.Print("NegotiateClient::Update() session removed and ConnectionGroup released returning true");
            ClearSession(httpWebRequest);
            return true;
        }

        public void ClearSession(WebRequest webRequest) {
            HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;
            GlobalLog.Assert(httpWebRequest != null, "NegotiateClient::ClearSession()|httpWebRequest == null");
            httpWebRequest.CurrentAuthenticationState.ClearSession();
        }

        public bool CanUseDefaultCredentials {
            get {
                return true;
            }
        }

        private static int GetSignatureIndex(string challenge, out bool useNego2) {
            // Negotiate supports two header fields "Nego2" and "Negotiate". If we find "Nego2" we use it,
            // otherwise we fall back to "Negotiate" (if available).
            useNego2 = true;
            
            int index = -1;
            
            // Consider Nego2 headers only on Win7 and later. Older OS version don't support LiveSSP.
            if (ComNetOS.IsWin7orLater) {
                index = AuthenticationManager.FindSubstringNotInQuotes(challenge, nego2Signature);
            }

            if (index < 0) {
                useNego2 = false;
                index = AuthenticationManager.FindSubstringNotInQuotes(challenge, negotiateSignature);
            }
            return index;
        }
    }; // class NegotiateClient


} // namespace System.Net
