//------------------------------------------------------------------------------
// <copyright file="_AuthenticationState.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System.Collections;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Globalization;
    using System.Net.Security;

    /// <devdoc>
    /// <para>Used by HttpWebRequest to syncronize and orchestrate authentication<para>
    /// </devdoc>
    internal class AuthenticationState {

        // true if we already attempted pre-authentication regardless if it has been
        // 1) possible,
        // 2) succesfull or
        // 3) unsuccessfull
        private bool                     TriedPreAuth;

        internal Authorization           Authorization;

        internal IAuthenticationModule   Module;

        // used to request a special connection for NTLM
        internal string                  UniqueGroupId;

        // used to distinguish proxy auth from server auth
        private bool                     IsProxyAuth;

        // the Uri of the host we're authenticating (proxy/server)
        // used to match entries in the CredentialCache
        internal Uri                     ChallengedUri;
        private SpnToken                 ChallengedSpn;

#if !FEATURE_PAL
        // this is the client's security context for SSPI based authentication
        // pared with the authentication module that set it.
        private NTAuthentication         SecurityContext;

        internal NTAuthentication GetSecurityContext(IAuthenticationModule module) {
            GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::GetSecurityContext(" + module.AuthenticationType + ") returning NTAuthentication#" + ValidationHelper.HashString((object)module==(object)Module ? SecurityContext : null));
            return (object)module==(object)Module ? SecurityContext : null;
        }

        internal void SetSecurityContext(NTAuthentication securityContext, IAuthenticationModule module) {
            GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::SetSecurityContext(" + module.AuthenticationType + ") was NTAuthentication#" + ValidationHelper.HashString(SecurityContext) + " now NTAuthentication#" + ValidationHelper.HashString(securityContext));
            SecurityContext = securityContext;
        }
#endif // !FEATURE_PAL

        private TransportContext _TransportContext;
        internal TransportContext TransportContext
        {
            get { return _TransportContext; }
            set { _TransportContext = value; }
        }

        internal HttpResponseHeader AuthenticateHeader {
            get {
                return IsProxyAuth ? HttpResponseHeader.ProxyAuthenticate : HttpResponseHeader.WwwAuthenticate;
            }
        }
        internal string AuthorizationHeader {
            get {
                return IsProxyAuth ? HttpKnownHeaderNames.ProxyAuthorization : HttpKnownHeaderNames.Authorization;
            }
        }
        internal HttpStatusCode StatusCodeMatch {
            get {
                return IsProxyAuth ? HttpStatusCode.ProxyAuthenticationRequired : HttpStatusCode.Unauthorized;
            }
        }

        internal AuthenticationState(bool isProxyAuth) {
            IsProxyAuth = isProxyAuth;
        }

        //
        // we need to do this to handle proxies in the correct way before
        // calling into the AuthenticationManager APIs
        //
        private void PrepareState(HttpWebRequest httpWebRequest)
        {
            Uri newUri = IsProxyAuth ? httpWebRequest.ServicePoint.InternalAddress : httpWebRequest.GetRemoteResourceUri();

            if ((object)ChallengedUri != (object)newUri)
            {
                if ((object)ChallengedUri == null || (object)ChallengedUri.Scheme != (object)newUri.Scheme || ChallengedUri.Host != newUri.Host || ChallengedUri.Port != newUri.Port)
                {
                    //
                    // must be a new server/port/scheme for this auth state, can happen on a redirect
                    //
                    ChallengedSpn = null;
                }
                ChallengedUri = newUri;
            }
            httpWebRequest.CurrentAuthenticationState = this;
        }
        //
        //
        //
        internal SpnToken GetComputeSpn(HttpWebRequest httpWebRequest)
        {
            if (ChallengedSpn != null)
                return ChallengedSpn;

            bool trustNewHost = true; // Assume trusted unless proven otherwise

            string spnKey = httpWebRequest.ChallengedUri.GetParts(UriComponents.Scheme | UriComponents.Host | UriComponents.Port | UriComponents.Path, UriFormat.SafeUnescaped);
            SpnToken spnToken = AuthenticationManager.SpnDictionary.InternalGet(spnKey);
            if (spnToken == null || spnToken.Spn == null)
            {
                string host;
                if (!IsProxyAuth && (httpWebRequest.ServicePoint.InternalProxyServicePoint || httpWebRequest.UseCustomHost))
                {
                    // Here the NT-Security folks need us to attempt a DNS lookup to figure out
                    // the FQDN. only do the lookup for short names (no IP addresses or DNS names)
                    //
                    // Initialize a backup value
                    host = httpWebRequest.ChallengedUri.Host;
                    // This host comes from the request/user, assume Trusted unless proven otherwise.

                    if (httpWebRequest.ChallengedUri.HostNameType != UriHostNameType.IPv6 
                        && httpWebRequest.ChallengedUri.HostNameType != UriHostNameType.IPv4 
                        && host.IndexOf('.') == -1)
                    {
                        try {
                            // 



                            IPHostEntry result;
                            if (Dns.TryInternalResolve(host, out result))
                            {
                                host = result.HostName;
                                trustNewHost &= result.isTrustedHost; // Can only lose trust
                            }
                        }
                        catch (Exception exception) {
                            if (NclUtilities.IsFatal(exception)) throw;
                            GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::GetComputeSpn() GetHostByName(host) failed:" + ValidationHelper.ToString(exception));
                        }
                    }
                }
                else
                {
                    // For this cases we already did a DNS lookup

                    // 



                    host = httpWebRequest.ServicePoint.Hostname;
                    trustNewHost &= httpWebRequest.ServicePoint.IsTrustedHost; // Can only lose trust                    
                }
                string spn = "HTTP/" + host;
                spnKey = httpWebRequest.ChallengedUri.GetParts(UriComponents.SchemeAndServer, UriFormat.SafeUnescaped) + "/";
                spnToken = new SpnToken(spn, trustNewHost);
                AuthenticationManager.SpnDictionary.InternalSet(spnKey, spnToken);
            }
            ChallengedSpn = spnToken;
            return ChallengedSpn;
        }
        //
        internal void PreAuthIfNeeded(HttpWebRequest httpWebRequest, ICredentials authInfo) {
            //
            // attempt to do preauth, if needed
            //
            GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::PreAuthIfNeeded() TriedPreAuth:" + TriedPreAuth.ToString() + " authInfo:" + ValidationHelper.HashString(authInfo));
            if (!TriedPreAuth) {
                TriedPreAuth = true;
                if (authInfo!=null) {
                    PrepareState(httpWebRequest);
                    Authorization preauth = null;
                    try {
                        preauth = AuthenticationManager.PreAuthenticate(httpWebRequest, authInfo);
                        GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::PreAuthIfNeeded() preauth:" + ValidationHelper.HashString(preauth));
                        if (preauth!=null && preauth.Message!=null) {
                            GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::PreAuthIfNeeded() setting TriedPreAuth to Complete:" + preauth.Complete.ToString());
                            UniqueGroupId = preauth.ConnectionGroupId;
                            httpWebRequest.Headers.Set(AuthorizationHeader, preauth.Message);
                        }
                    }
                    catch (Exception exception) {
                        GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::PreAuthIfNeeded() PreAuthenticate() returned exception:" + exception.Message);
                        ClearSession(httpWebRequest);
                    }
                }
            }
        }

        //
        // attempts to authenticate the request:
        // returns true only if it succesfully called into the AuthenticationManager
        // and got back a valid Authorization and succesfully set the appropriate auth headers
        //
        internal bool AttemptAuthenticate(HttpWebRequest httpWebRequest, ICredentials authInfo) {
            //
            // Check for previous authentication attempts or the presence of credentials
            //
            GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::AttemptAuthenticate() httpWebRequest#" + ValidationHelper.HashString(httpWebRequest) + " AuthorizationHeader:" + AuthorizationHeader.ToString());

            if (Authorization!=null && Authorization.Complete) {
                //
                // here the design gets "dirty".
                // if this is proxy auth, we might have been challenged by an external
                // server as well. in this case we will have to clear our previous proxy
                // auth state before we go any further. this will be broken if the handshake
                // requires more than one dropped connection (which NTLM is a border case for,
                // since it droppes the connection on the 1st challenge but not on the second)
                //
                GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::AttemptAuthenticate() Authorization!=null Authorization.Complete:" + Authorization.Complete.ToString());
                if (IsProxyAuth) {
                    //
                    // so, we got passed a 407 but now we got a 401, the proxy probably
                    // dropped the connection on us so we need to reset our proxy handshake
                    // Consider: this should have been taken care by Update()
                    //
                    GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::AttemptAuthenticate() ProxyAuth cleaning up auth status");
                    ClearAuthReq(httpWebRequest);
                }
                return false;
            }

            if (authInfo==null) {
                GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::AttemptAuthenticate() authInfo==null Authorization#" + ValidationHelper.HashString(Authorization));
                return false;
            }

            string challenge = httpWebRequest.AuthHeader(AuthenticateHeader);

            if (challenge==null) {
                //
                // the server sent no challenge, but this might be the case
                // in which we're succeeding an authorization handshake to
                // a proxy while a handshake with the server is still in progress.
                // if the handshake with the proxy is complete and we actually have
                // a handshake with the server in progress we can send the authorization header for the server as well.
                //
                if (!IsProxyAuth && Authorization!=null && httpWebRequest.ProxyAuthenticationState.Authorization!=null) {
                    httpWebRequest.Headers.Set(AuthorizationHeader, Authorization.Message);
                }
                GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::AttemptAuthenticate() challenge==null Authorization#" + ValidationHelper.HashString(Authorization));
                return false;
            }

            //
            // if the AuthenticationManager throws on Authenticate,
            // bubble up that Exception to the user
            //
            GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::AttemptAuthenticate() challenge:" + challenge);

            PrepareState(httpWebRequest);
            try {
                Authorization = AuthenticationManager.Authenticate(challenge, httpWebRequest, authInfo);
            }
            catch (Exception exception) {
                Authorization = null;
                GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::PreAuthIfNeeded() PreAuthenticate() returned exception:" + exception.Message);
                ClearSession(httpWebRequest);
                throw;
            }


            if (Authorization==null) {
                GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::AttemptAuthenticate() Authorization==null");
                return false;
            }
            if (Authorization.Message==null) {
                GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::AttemptAuthenticate() Authorization.Message==null");
                Authorization = null;
                return false;
            }

            UniqueGroupId = Authorization.ConnectionGroupId;
            GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::AttemptAuthenticate() AuthorizationHeader:" + AuthorizationHeader + " blob: " + Authorization.Message.Length + "bytes Complete:" + Authorization.Complete.ToString());

            try {
                //
                // a "bad" module could try sending bad characters in the HTTP headers.
                // catch the exception from WebHeaderCollection.CheckBadChars()
                // fail the auth process
                // and return the exception to the user as InnerException
                //
                httpWebRequest.Headers.Set(AuthorizationHeader, Authorization.Message);
            }
            catch {
                Authorization = null;
                ClearSession(httpWebRequest);
                throw;
            }

            return true;
        }

        internal void ClearAuthReq(HttpWebRequest httpWebRequest) {
            //
            // if we are authenticating and we're being redirected to
            // another authentication space then remove the current
            // authentication header
            //
            GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::ClearAuthReq() httpWebRequest#" + ValidationHelper.HashString(httpWebRequest) + " " + AuthorizationHeader.ToString() + ": " + ValidationHelper.ToString(httpWebRequest.Headers[AuthorizationHeader]));
            TriedPreAuth = false;
            Authorization = null;
            UniqueGroupId = null;
            httpWebRequest.Headers.Remove(AuthorizationHeader);
        }

        //
        // gives the IAuthenticationModule a chance to update its internal state.
        // do any necessary cleanup and update the Complete status of the associated Authorization.
        //
        internal void Update(HttpWebRequest httpWebRequest) {
            //
            // RAID#86753
            // Microsoft: this is just a fix for redirection & kerberos.
            // we need to close the Context and call ISC() again with the final
            // blob returned from the server. to do this in general
            // we would probably need to change the IAuthenticationMdule interface and
            // add this Update() method. for now we just have it internally.
            //
            // actually this turns out to be quite handy for 2 more cases:
            // NTLM auth: we need to clear the connection group after we suceed to prevent leakage.
            // Digest auth: we need to support stale credentials, if we fail with a 401 and stale is true we need to retry.
            //
            GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::Update() httpWebRequest#" + ValidationHelper.HashString(httpWebRequest) + " Authorization#" + ValidationHelper.HashString(Authorization) + " ResponseStatusCode:" + httpWebRequest.ResponseStatusCode.ToString());

            if (Authorization!=null) {

                PrepareState(httpWebRequest);

                ISessionAuthenticationModule myModule = Module as ISessionAuthenticationModule;

                if (myModule!=null) {
                    //
                    // the whole point here is to complete the Security Context. Sometimes, though,
                    // a bad cgi script or a bad server, could miss sending back the final blob.
                    // in this case we won't be able to complete the handshake, but we'll have to clean up anyway.
                    //
                    string challenge = httpWebRequest.AuthHeader(AuthenticateHeader);
                    GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::Update() Complete:" + Authorization.Complete.ToString() + " Module:" + ValidationHelper.ToString(Module) + " challenge:" + ValidationHelper.ToString(challenge));

                    if (!IsProxyAuth && httpWebRequest.ResponseStatusCode==HttpStatusCode.ProxyAuthenticationRequired) {
                        //
                        // don't call Update on the module, since there's an ongoing
                        // handshake and we don't need to update any state in such a case
                        //
                        GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::Update() skipping call to " + myModule.ToString() + ".Update() since we need to reauthenticate with the proxy");
                    }
                    else {
                        bool complete = true;
                        try {
                            complete = myModule.Update(challenge, httpWebRequest);
                            GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::Update() " + myModule.ToString() + ".Update() returned complete:" + complete.ToString());
                        }
                        catch (Exception exception) {
                            GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::Update() " + myModule.ToString() + ".Update() caught exception:" + exception.Message);
                            ClearSession(httpWebRequest);

#if !FEATURE_PAL
                            if ((httpWebRequest.AuthenticationLevel == AuthenticationLevel.MutualAuthRequired) &&
                                (httpWebRequest.CurrentAuthenticationState == null || httpWebRequest.CurrentAuthenticationState.Authorization == null || !httpWebRequest.CurrentAuthenticationState.Authorization.MutuallyAuthenticated))
                            {
                                throw;
                            }
#endif // !FEATURE_PAL

                        }

                        Authorization.SetComplete(complete);
                    }

                }

                //
                // If authentication was successful, create binding between
                // the request and the authorization for future preauthentication
                //
                if (httpWebRequest.PreAuthenticate && Module != null && Authorization.Complete && Module.CanPreAuthenticate && httpWebRequest.ResponseStatusCode != StatusCodeMatch) {
                    GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::Update() handshake is Complete calling BindModule()");
                    AuthenticationManager.BindModule(ChallengedUri, Authorization, Module);
                }
            }
        }

        internal void ClearSession() {
#if !FEATURE_PAL // Security
           GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::ClearSession() NTAuthentication#" + ValidationHelper.HashString(SecurityContext));
           if (SecurityContext!=null) {
               SecurityContext.CloseContext();
               SecurityContext = null;
           }
#endif // FEATURE_PAL // Security
       }

       internal void ClearSession(HttpWebRequest httpWebRequest) {
            PrepareState(httpWebRequest);
            ISessionAuthenticationModule myModule = Module as ISessionAuthenticationModule;
            Module = null;

            if (myModule!=null) {
                try {
                    myModule.ClearSession(httpWebRequest);
                }
                catch (Exception exception) {
                    if (NclUtilities.IsFatal(exception)) throw;

                    GlobalLog.Print("AuthenticationState#" + ValidationHelper.HashString(this) + "::ClearSession() " + myModule.ToString() + ".Update() caught exception:" + exception.Message);
                }
            }

        }

    }
}
