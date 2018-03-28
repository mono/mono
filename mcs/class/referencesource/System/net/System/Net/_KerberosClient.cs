//------------------------------------------------------------------------------
// <copyright file="_KerberosClient.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System.Collections;
    using System.Net.Sockets;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Permissions;
    using System.Globalization;

    internal class KerberosClient : ISessionAuthenticationModule {

        internal const string AuthType = "Kerberos";
        internal static string Signature = AuthType.ToLower(CultureInfo.InvariantCulture);
        internal static int SignatureSize = Signature.Length;

        public Authorization Authenticate(string challenge, WebRequest webRequest, ICredentials credentials) {
            GlobalLog.Print("KerberosClient::Authenticate() challenge:[" + ValidationHelper.ToString(challenge) + "] webRequest#" + ValidationHelper.HashString(webRequest) + " credentials#" + ValidationHelper.HashString(credentials) + " calling DoAuthenticate()");
            return DoAuthenticate(challenge, webRequest, credentials, false);
        }

        
        private Authorization DoAuthenticate(string challenge, WebRequest webRequest, ICredentials credentials, bool preAuthenticate) {
            GlobalLog.Print("KerberosClient::DoAuthenticate() challenge:[" + ValidationHelper.ToString(challenge) + "] webRequest#" + ValidationHelper.HashString(webRequest) + " credentials#" + ValidationHelper.HashString(credentials) + " preAuthenticate:" + preAuthenticate.ToString());

            GlobalLog.Assert(credentials != null, "KerberosClient::DoAuthenticate()|credentials == null");
            if (credentials == null) {
                return null;
            }

            HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;

            GlobalLog.Assert(httpWebRequest != null, "KerberosClient::DoAuthenticate()|httpWebRequest == null");
            GlobalLog.Assert(httpWebRequest.ChallengedUri != null, "KerberosClient::DoAuthenticate()|httpWebRequest.ChallengedUri == null");

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
                GlobalLog.Print("KerberosClient::DoAuthenticate() key:" + ValidationHelper.HashString(httpWebRequest.CurrentAuthenticationState) + " retrieved authSession:" + ValidationHelper.HashString(authSession));
            }

            if (authSession==null) {
                NetworkCredential NC = credentials.GetCredential(httpWebRequest.ChallengedUri, Signature);
                GlobalLog.Print("KerberosClient::DoAuthenticate() GetCredential() returns:" + ValidationHelper.ToString(NC));

                if (NC == null || (!(NC is SystemNetworkCredential) && NC.InternalGetUserName().Length == 0))
                {
                    return null;
                }

                ICredentialPolicy policy = AuthenticationManager.CredentialPolicy;
                if (policy != null && !policy.ShouldSendCredential(httpWebRequest.ChallengedUri, httpWebRequest, NC, this))
                    return null;

                SpnToken spn = httpWebRequest.CurrentAuthenticationState.GetComputeSpn(httpWebRequest);
                GlobalLog.Print("KerberosClient::Authenticate() ChallengedSpn:" + ValidationHelper.ToString(spn));

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


                GlobalLog.Print("KerberosClient::DoAuthenticate() setting SecurityContext for:" + ValidationHelper.HashString(httpWebRequest.CurrentAuthenticationState) + " to authSession:" + ValidationHelper.HashString(authSession));
                httpWebRequest.CurrentAuthenticationState.SetSecurityContext(authSession, this);
            }

            string clientResponse = authSession.GetOutgoingBlob(incoming);
            if (clientResponse==null) {
                return null;
            }

            return new Authorization(AuthType + " " + clientResponse, authSession.IsCompleted, string.Empty, authSession.IsMutualAuthFlag);
        }

        public bool CanPreAuthenticate {
            get {
                return true;
            }
        }

        public Authorization PreAuthenticate(WebRequest webRequest, ICredentials credentials) {
            GlobalLog.Print("KerberosClient::PreAuthenticate() webRequest#" + ValidationHelper.HashString(webRequest) + " credentials#" + ValidationHelper.HashString(credentials) + " calling DoAuthenticate()");
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
            GlobalLog.Print("KerberosClient::Update(): " + challenge);

            HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;

            GlobalLog.Assert(httpWebRequest != null, "KerberosClient::Update()|httpWebRequest == null");
            GlobalLog.Assert(httpWebRequest.ChallengedUri != null, "KerberosClient::Update()|httpWebRequest.ChallengedUri == null");

            //
            // try to retrieve the state of the ongoing handshake
            //
            NTAuthentication authSession = httpWebRequest.CurrentAuthenticationState.GetSecurityContext(this);
            GlobalLog.Print("KerberosClient::Update() key:" + ValidationHelper.HashString(httpWebRequest.CurrentAuthenticationState) + " retrieved authSession:" + ValidationHelper.HashString(authSession));

            if (authSession==null) {
                GlobalLog.Print("KerberosClient::Update() null session returning true");
                return true;
            }

            GlobalLog.Print("KerberosClient::Update() authSession.IsCompleted:" + authSession.IsCompleted.ToString());

            if (httpWebRequest.CurrentAuthenticationState.StatusCodeMatch==httpWebRequest.ResponseStatusCode) {
                GlobalLog.Print("KerberosClient::Update() still handshaking (based on status code) returning false");
                return false;
            }

            //
            // the whole point here is to to close the Security Context (this will complete the authentication handshake
            // with server authentication for schemese that support it such as Kerberos)
            //
            int index = challenge==null ? -1 : AuthenticationManager.FindSubstringNotInQuotes(challenge, Signature);
            if (index>=0) {
                int blobBegin = index + SignatureSize;
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
                GlobalLog.Print("KerberosClient::Update() closing security context using last incoming blob:[" + ValidationHelper.ToString(incoming) + "]");
                string clientResponse = authSession.GetOutgoingBlob(incoming);
                httpWebRequest.CurrentAuthenticationState.Authorization.MutuallyAuthenticated = authSession.IsMutualAuthFlag;
                GlobalLog.Print("KerberosClient::Update() GetOutgoingBlob() returns clientResponse:[" + ValidationHelper.ToString(clientResponse) + "] IsCompleted:" + authSession.IsCompleted.ToString());
            }

            // Extract the CBT we used and cache it for future requests that want to do preauth
            httpWebRequest.ServicePoint.SetCachedChannelBinding(httpWebRequest.ChallengedUri, authSession.ChannelBinding);

            GlobalLog.Print("KerberosClient::Update() session removed and ConnectionGroup released returning true");
            ClearSession(httpWebRequest);
            return true;
        }

        public void ClearSession(WebRequest webRequest) {
            HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;
            GlobalLog.Assert(httpWebRequest != null, "KerberosClient::ClearSession()|httpWebRequest == null");
            httpWebRequest.CurrentAuthenticationState.ClearSession();
        }

        public bool CanUseDefaultCredentials {
            get {
                return true;
            }
        }

    }; // class KerberosClient


} // namespace System.Net
