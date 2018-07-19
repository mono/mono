//------------------------------------------------------------------------------
// <copyright file="_NtlmClient.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System.Collections;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Permissions;
    using System.Globalization;

    internal class NtlmClient : ISessionAuthenticationModule {

        internal const string AuthType = "NTLM";
        internal static string Signature = AuthType.ToLower(CultureInfo.InvariantCulture);
        internal static int SignatureSize = Signature.Length;

        public Authorization Authenticate(string challenge, WebRequest webRequest, ICredentials credentials) {
            GlobalLog.Print("NtlmClient::Authenticate() challenge:[" + ValidationHelper.ToString(challenge) + "] webRequest#" + ValidationHelper.HashString(webRequest) + " credentials#" + ValidationHelper.HashString(credentials) + " calling DoAuthenticate()");
            return DoAuthenticate(challenge, webRequest, credentials, false);
        }

        private Authorization DoAuthenticate(string challenge, WebRequest webRequest, ICredentials credentials, bool preAuthenticate) {
            GlobalLog.Print("NtlmClient::DoAuthenticate() challenge:[" + ValidationHelper.ToString(challenge) + "] webRequest#" + ValidationHelper.HashString(webRequest) + " credentials#" + ValidationHelper.HashString(credentials) + " preAuthenticate:" + preAuthenticate.ToString());

            GlobalLog.Assert(credentials != null, "NtlmClient::DoAuthenticate()|credentials == null");
            if (credentials == null) {
                return null;
            }

            HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;

            GlobalLog.Assert(httpWebRequest != null, "NtlmClient::DoAuthenticate()|httpWebRequest == null");
            GlobalLog.Assert(httpWebRequest.ChallengedUri != null, "NtlmClient::DoAuthenticate()|httpWebRequest.ChallengedUri == null");

            NTAuthentication authSession = null;
            string incoming = null;

            if (!preAuthenticate) {
                int index = AuthenticationManager.FindSubstringNotInQuotes(challenge, Signature);
                if (index < 0) {
                    return null;
                }

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
                GlobalLog.Print("NtlmClient::DoAuthenticate() key:" + ValidationHelper.HashString(httpWebRequest.CurrentAuthenticationState) + " retrieved authSession:" + ValidationHelper.HashString(authSession));
            }

            if (authSession==null) {
                NetworkCredential NC = credentials.GetCredential(httpWebRequest.ChallengedUri, Signature);
                GlobalLog.Print("NtlmClient::DoAuthenticate() GetCredential() returns:" + ValidationHelper.ToString(NC));

                string username = string.Empty;
                if (NC == null || (!(NC is SystemNetworkCredential) && (username = NC.InternalGetUserName()).Length == 0))
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
                        AuthType,
                        NC,
                        spn,
                        httpWebRequest,
                        binding);

                GlobalLog.Print("NtlmClient::DoAuthenticate() setting SecurityContext for:" + ValidationHelper.HashString(httpWebRequest.CurrentAuthenticationState) + " to authSession:" + ValidationHelper.HashString(authSession));
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
            httpWebRequest.NtlmKeepAlive = incoming==null;

            return AuthenticationManager.GetGroupAuthorization(this, AuthType + " " + clientResponse, authSession.IsCompleted, authSession, canShareConnection, false);
        }

        public bool CanPreAuthenticate {
            get {
                return true;
            }
        }

        public Authorization PreAuthenticate(WebRequest webRequest, ICredentials credentials) {
            GlobalLog.Print("NtlmClient::PreAuthenticate() webRequest#" + ValidationHelper.HashString(webRequest) + " credentials#" + ValidationHelper.HashString(credentials) + " calling DoAuthenticate()");
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
            GlobalLog.Print("NtlmClient::Update(): " + challenge);

            HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;

            GlobalLog.Assert(httpWebRequest != null, "NtlmClient::Update()|httpWebRequest == null");
            GlobalLog.Assert(httpWebRequest.ChallengedUri != null, "NtlmClient::Update()|httpWebRequest.ChallengedUri == null");

            //
            // try to retrieve the state of the ongoing handshake
            //
            NTAuthentication authSession = httpWebRequest.CurrentAuthenticationState.GetSecurityContext(this);
            GlobalLog.Print("NtlmClient::Update() key:" + ValidationHelper.HashString(httpWebRequest.CurrentAuthenticationState) + " retrieved authSession:" + ValidationHelper.HashString(authSession));

            if (authSession==null) {
                GlobalLog.Print("NtlmClient::Update() null session returning true");
                return true;
            }

            GlobalLog.Print("NtlmClient::Update() authSession.IsCompleted:" + authSession.IsCompleted.ToString());

            if (!authSession.IsCompleted && httpWebRequest.CurrentAuthenticationState.StatusCodeMatch==httpWebRequest.ResponseStatusCode) {
                GlobalLog.Print("NtlmClient::Update() still handshaking (based on status code) returning false");
                return false;
            }

            ClearSession(httpWebRequest);

            // now possibly close the ConnectionGroup after authentication is done.
            if (!httpWebRequest.UnsafeOrProxyAuthenticatedConnectionSharing) {
                GlobalLog.Print("NtlmClient::Update() releasing ConnectionGroup:" + httpWebRequest.GetConnectionGroupLine());
                httpWebRequest.ServicePoint.ReleaseConnectionGroup(httpWebRequest.GetConnectionGroupLine());
            }

            // Extract the CBT we used and cache it for future requests that want to do preauth
            httpWebRequest.ServicePoint.SetCachedChannelBinding(httpWebRequest.ChallengedUri, authSession.ChannelBinding);

            GlobalLog.Print("NtlmClient::Update() session removed and ConnectionGorup released returning true");
            return true;
        }

        public void ClearSession(WebRequest webRequest) {
            HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;
            GlobalLog.Assert(httpWebRequest != null, "NtlmClient::ClearSession()|httpWebRequest == null");
            httpWebRequest.CurrentAuthenticationState.ClearSession();
        }

        public bool CanUseDefaultCredentials {
            get {
                return true;
            }
        }

    }; // class NtlmClient


} // namespace System.Net
