//------------------------------------------------------------------------------
// <copyright file="_BasicClient.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {
    using System.Text;
    using System.Globalization;
    using System.Security.Permissions;

    internal class BasicClient : IAuthenticationModule {

        internal const string AuthType = "Basic";
        internal static string Signature = AuthType.ToLower(CultureInfo.InvariantCulture);
        internal static int SignatureSize = Signature.Length;

        public Authorization Authenticate(string challenge, WebRequest webRequest, ICredentials credentials) {
            GlobalLog.Print("BasicClient::Authenticate(): " + challenge);

            GlobalLog.Assert(credentials != null, "BasicClient::Authenticate()|credentials == null");
#if !FEATURE_PAL
            if (credentials==null || credentials is SystemNetworkCredential) {
#else
            if (credentials==null) {
#endif // !FEATURE_PAL
                return null;
            }

            HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;

            GlobalLog.Assert(httpWebRequest != null, "BasicClient::Authenticate()|httpWebRequest == null");
            if (httpWebRequest==null || httpWebRequest.ChallengedUri==null) {
                //
                // there has been no challenge:
                // 1) the request never went on the wire
                // 2) somebody other than us is calling into AuthenticationManager
                //
                return null;
            }

            int index = AuthenticationManager.FindSubstringNotInQuotes(challenge, Signature);
            if (index < 0) {
                return null;
            }

            return Lookup(httpWebRequest, credentials);
        }

        public bool CanPreAuthenticate {
            get {
                return true;
            }
        }

        public Authorization PreAuthenticate(WebRequest webRequest, ICredentials credentials) {
            GlobalLog.Print("BasicClient::PreAuthenticate()");

            GlobalLog.Assert(credentials != null, "BasicClient::Authenticate()|credentials == null");
#if !FEATURE_PAL
            if (credentials==null || credentials is SystemNetworkCredential) {
#else
            if (credentials==null) {
#endif // !FEATURE_PAL
                return null;
            }

            HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;

            GlobalLog.Assert(httpWebRequest != null, "BasicClient::Authenticate()|httpWebRequest == null");
            if (httpWebRequest==null) {
                return null;
            }

            return Lookup(httpWebRequest, credentials);
        }

        public string AuthenticationType {
            get {
                return AuthType;
            }
        }

        
        private Authorization Lookup(HttpWebRequest httpWebRequest, ICredentials credentials) {
            GlobalLog.Print("BasicClient::Lookup(): ChallengedUri:" + httpWebRequest.ChallengedUri.ToString());

            NetworkCredential NC = credentials.GetCredential(httpWebRequest.ChallengedUri, Signature);
            GlobalLog.Print("BasicClient::Lookup() GetCredential() returns:" + ValidationHelper.ToString(NC));

            if (NC==null) {
                return null;
            }

            ICredentialPolicy policy = AuthenticationManager.CredentialPolicy;
            if (policy != null && !policy.ShouldSendCredential(httpWebRequest.ChallengedUri, httpWebRequest, NC, this))
                return null;


            string username = NC.InternalGetUserName();
            string domain = NC.InternalGetDomain();

            if (ValidationHelper.IsBlankString(username)) {
                return null;
            }

            string rawString = ((!ValidationHelper.IsBlankString(domain)) ? (domain + "\\") : "") + username + ":" + NC.InternalGetPassword();

            // The response is an "Authorization:" header where the value is
            // the text "Basic" followed by BASE64 encoded (as defined by RFC1341) value

            byte[] bytes = EncodingRightGetBytes(rawString);
            string responseHeader = BasicClient.AuthType + " " + Convert.ToBase64String(bytes);

            return new Authorization(responseHeader, true);
        }

        internal static byte[] EncodingRightGetBytes(string rawString) {
            GlobalLog.Enter("BasicClient::EncodingRightGetBytes", "[" + rawString.Length.ToString() + ":" + rawString + "]");
            //
            // in order to know if there will not be any '?' translations (which means
            // we should use the Default Encoding) we need to attempt encoding and then decoding.
            // <


            GlobalLog.Print("BasicClient::EncodingRightGetBytes(): Default Encoding is:" + Encoding.Default.EncodingName);

            byte[] bytes = Encoding.Default.GetBytes(rawString);
            string rawCopy = Encoding.Default.GetString(bytes);
            bool canMapToCurrentCodePage = string.Compare(rawString, rawCopy, StringComparison.Ordinal)==0;

            GlobalLog.Print("BasicClient::EncodingRightGetBytes(): canMapToCurrentCodePage:" + canMapToCurrentCodePage.ToString());

            //<




            if (!canMapToCurrentCodePage) {
                //<


                GlobalLog.LeaveException("BasicClient::EncodingRightGetBytes", ExceptionHelper.MethodNotSupportedException);
                throw ExceptionHelper.MethodNotSupportedException;
                /*
                GlobalLog.Print("BasicClient::EncodingRightGetBytes(): using:" + Encoding.UTF8.EncodingName);
                bytes = Encoding.UTF8.GetBytes(rawString);

                string blob = "=?utf-8?B?" + Convert.ToBase64String(bytes) + "?=";
                bytes = Encoding.ASCII.GetBytes(blob);
                */
            }

            GlobalLog.Dump(bytes);
            GlobalLog.Leave("BasicClient::EncodingRightGetBytes", bytes.Length.ToString());

            return bytes;
        }

    }; // class BasicClient


} // namespace System.Net
