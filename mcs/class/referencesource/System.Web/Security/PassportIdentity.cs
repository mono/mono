//------------------------------------------------------------------------------
// <copyright file="PassportIdentity.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * PassportIdentity
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Security {
    using System.Text;
    using System.Web;
    using System.Web.Util;
    using System.Security.Principal;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Globalization;


    /// <devdoc>
    ///    This IIdenty derived class provides access
    ///    to the Passport profile information contained in the Passport profile cookies.
    /// </devdoc>
    [Obsolete("This type is obsolete. The Passport authentication product is no longer supported and has been superseded by Live ID.")]
    public sealed class PassportIdentity : IIdentity, IDisposable {
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        // Private Data
        private  String        _Name;
        private  bool          _Authenticated;
        private  IntPtr        _iPassport;
        private static int     _iPassportVer=0;
        private  bool          _WWWAuthHeaderSet = false;
        internal bool          WWWAuthHeaderSet { get { return _WWWAuthHeaderSet; }}

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        // Construtor

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public PassportIdentity() {
            HttpContext context  = HttpContext.Current;

            if (_iPassportVer == 0)
                _iPassportVer = UnsafeNativeMethods.PassportVersion();

            if (_iPassportVer < 3) {
                String      strTVariable       = context.Request.QueryString["t"];
                String      strPVariable       = context.Request.QueryString["p"];
                HttpCookie  cookieAuth         = context.Request.Cookies["MSPAuth"];
                HttpCookie  cookieProf         = context.Request.Cookies["MSPProf"];
                HttpCookie  cookieProfC        = context.Request.Cookies["MSPProfC"];
                String      strMSPAuthCookie   = ((cookieAuth  != null && cookieAuth.Value  != null) ? cookieAuth.Value  : String.Empty);
                String      strMSPProfCookie   = ((cookieProf  != null && cookieProf.Value  != null) ? cookieProf.Value  : String.Empty);
                String      strMSPProfCCookie  = ((cookieProfC != null && cookieProfC.Value != null) ? cookieProfC.Value : String.Empty);

                StringBuilder strA = new  StringBuilder(1028);
                StringBuilder strP = new  StringBuilder(1028);

                strMSPAuthCookie  = HttpUtility.UrlDecode(strMSPAuthCookie);
                strMSPProfCookie  = HttpUtility.UrlDecode(strMSPProfCookie);
                strMSPProfCCookie = HttpUtility.UrlDecode(strMSPProfCCookie);

                int iRet = UnsafeNativeMethods.PassportCreate(strTVariable, strPVariable, strMSPAuthCookie, 
                                                              strMSPProfCookie, strMSPProfCCookie, strA, strP, 1024, ref _iPassport);
                if (_iPassport == IntPtr.Zero)
                    throw new COMException(SR.GetString(SR.Could_not_create_passport_identity), iRet);

                String strACookie = UrlEncodeCookie(strA.ToString()); //HttpUtility.AspCompatUrlEncode(strA.ToString());
                String strPCookie = UrlEncodeCookie(strP.ToString()); //HttpUtility.AspCompatUrlEncode(strP.ToString());

                if (strACookie.Length > 1)
                {
                    context.Response.AppendHeader("Set-Cookie", strACookie);
                }             
                if (strPCookie.Length > 1) {
                    context.Response.AppendHeader("Set-Cookie", strPCookie);
                }

            } else {                
                String   strRequestLine = context.Request.HttpMethod + " " + 
                                          context.Request.RawUrl + " " + 
                                          context.Request.ServerVariables["SERVER_PROTOCOL"] + "\r\n";
                StringBuilder szOut = new StringBuilder(4092);
                int iRet = UnsafeNativeMethods.PassportCreateHttpRaw(strRequestLine, 
                                                                     context.Request.ServerVariables["ALL_RAW"],
                                                                     context.Request.IsSecureConnection ? 1 : 0,
                                                                     szOut, 4090, ref _iPassport);
                
                if (_iPassport == IntPtr.Zero)
                    throw new COMException(SR.GetString(SR.Could_not_create_passport_identity), iRet);
                
                String strResponseHeaders = szOut.ToString();                
                SetHeaders(context, strResponseHeaders);
            }

            _Authenticated = GetIsAuthenticated(-1, -1, -1);
            if (_Authenticated == false) {
                _Name = String.Empty;            
            }                        
        }


        private void SetHeaders(HttpContext context, String strResponseHeaders) {
            for(int iStart = 0; iStart < strResponseHeaders.Length;)
            {
                int iEnd = strResponseHeaders.IndexOf('\r', iStart);
                if (iEnd < 0)
                    iEnd = strResponseHeaders.Length;
                String strCurrentHeader = strResponseHeaders.Substring(iStart, iEnd - iStart);
                int iColon = strCurrentHeader.IndexOf(':');
                if (iColon > 0)
                {
                    String strHeader = strCurrentHeader.Substring(0, iColon);
                    String strValue = strCurrentHeader.Substring(iColon+1);
                    context.Response.AppendHeader(strHeader, strValue);
                }

                iStart = iEnd + 2;
            }
        } 

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////


        ~PassportIdentity() {
            UnsafeNativeMethods.PassportDestroy(_iPassport);
            _iPassport = IntPtr.Zero;
        }


        private static string UrlEncodeCookie(string strIn) {
            if (strIn == null || strIn.Length < 1)
                return String.Empty;
            int iPos1 = strIn.IndexOf('=');
            if (iPos1 < 0)
                return HttpUtility.AspCompatUrlEncode(strIn);
            
            iPos1++;
            int iPos2 = strIn.IndexOf(';', iPos1);
            if (iPos2 < 0)
                return HttpUtility.AspCompatUrlEncode(strIn);           

            string str1 = strIn.Substring(0, iPos1);
            string str2 = strIn.Substring(iPos1, iPos2-iPos1);
            string str3 = strIn.Substring(iPos2, strIn.Length-iPos2);

            return str1 + HttpUtility.AspCompatUrlEncode(str2) + str3;
        }


        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        // Properties

        /// <devdoc>
        ///    The name of the identity. In this
        ///    case, the Passport user name.
        /// </devdoc>
        public   String    Name { 
            get { 
                if (_Name == null) {
                    if (_iPassportVer >= 3)
                        _Name = HexPUID;
                    else
                    if (HasProfile("core")) {
                        _Name = Int32.Parse(this["MemberIDHigh"], CultureInfo.InvariantCulture).ToString("X8", CultureInfo.InvariantCulture) + 
                                Int32.Parse(this["MemberIDLow"],  CultureInfo.InvariantCulture).ToString("X8", CultureInfo.InvariantCulture);
                    } else {
                        _Name = String.Empty;
                    }
                }

                return _Name; 
            } 
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////

        /// <devdoc>
        ///    The type of the identity. In this
        ///    case, "Passport".
        /// </devdoc>
        public   String    AuthenticationType { get { return "Passport";}}

        /// <devdoc>
        ///    <para>True if the user is authenticated against a
        ///       Passport authority.</para>
        /// </devdoc>
        public   bool      IsAuthenticated { get { return _Authenticated;}}

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public String  this[String strProfileName]
        {
            get { 
                Object oValue = GetProfileObject(strProfileName);
                if (oValue == null)
                    return String.Empty;

                if (oValue is string)
                    return(String) oValue;

                return oValue.ToString();
            }            
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////

        /// <devdoc>
        ///    Returns true if the user is authenticated
        ///    against a Passport authority.
        /// </devdoc>
        public bool GetIsAuthenticated(int    iTimeWindow,
                                       bool   bForceLogin,
                                       bool   bCheckSecure)
        {
            return GetIsAuthenticated(iTimeWindow, bForceLogin ? 1 : 0, bCheckSecure ? 10 : 0);
        }


        public bool GetIsAuthenticated(int   iTimeWindow,
                                       int   iForceLogin,
                                       int   iCheckSecure)
        {
            int iRet = UnsafeNativeMethods.PassportIsAuthenticated(_iPassport, 
                                                                   iTimeWindow, 
                                                                   iForceLogin, 
                                                                   iCheckSecure);
            if (iRet < 0)
                throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
                     
            return (iRet == 0);            
        }
                                       

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////

        /// <devdoc>
        ///    Returns Passport profile information for
        ///    the supplied profile attribute.
        /// </devdoc>
        public Object GetProfileObject(String strProfileName) {
            Object oOut = new Object();

            int iRet = UnsafeNativeMethods.PassportGetProfile(_iPassport, strProfileName, out oOut);
            if (iRet < 0)
                throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
            return oOut;
        } 

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////


        /// <devdoc>
        ///    Returns an error state associated with the
        ///    current Passport ticket. See the error property in the Passport documentation
        ///    for more information.
        /// </devdoc>
        public int Error {
            get {
                return UnsafeNativeMethods.PassportGetError(_iPassport);
            }
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////


        /// <devdoc>
        ///    True if a connection is coming back from
        ///    the Passport server (log-in, update, or registration) and if the Passport data
        ///    contained on the query string is valid.
        /// </devdoc>
        public bool GetFromNetworkServer {
            get {
                int iRet = UnsafeNativeMethods.PassportGetFromNetworkServer(_iPassport);
                if (iRet < 0)
                    throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
                return(iRet == 0);
            }
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////


        /// <devdoc>
        ///    Returns the Passport domain from the member
        ///    name string.
        /// </devdoc>
        public String GetDomainFromMemberName(String strMemberName) {
            StringBuilder str = new StringBuilder(1028);                
            int iRet = UnsafeNativeMethods.PassportDomainFromMemberName(_iPassport, strMemberName, str, 1024);
            if (iRet < 0)
                throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
            return str.ToString();
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////


        /// <devdoc>
        ///    Determines whether or not a given profile
        ///    attribute exists in this user's profile.
        /// </devdoc>
        public bool HasProfile(String strProfile) {
            int iRet = UnsafeNativeMethods.PassportHasProfile(_iPassport, strProfile);
            if (iRet < 0)
                throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
            return(iRet == 0);
        }


        public bool HasFlag(int iFlagMask) {
            int iRet = UnsafeNativeMethods.PassportHasFlag(_iPassport, iFlagMask);
            if (iRet < 0)
                throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
            return(iRet == 0);
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool HaveConsent(bool bNeedFullConsent, bool bNeedBirthdate) {
            int iRet = UnsafeNativeMethods.PassportHasConsent(_iPassport, bNeedFullConsent ? 1 : 0, bNeedBirthdate ? 1 : 0);
            if (iRet < 0)
                throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
            return(iRet == 0);
        }



        public Object GetOption(String strOpt) {
            Object vOut = new Object();
            int iRet = UnsafeNativeMethods.PassportGetOption(_iPassport, strOpt, out vOut);
            if (iRet < 0)
                throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
            return vOut;
        } 


        public void SetOption(String strOpt, Object vOpt) {
            int iRet = UnsafeNativeMethods.PassportSetOption(_iPassport, strOpt, vOpt);
            if (iRet < 0)
                throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
        } 


        public String LogoutURL() {
            return LogoutURL(null, null, -1, null, -1);
        }
    

        public String LogoutURL(String      szReturnURL,
                                String      szCOBrandArgs,
                                int         iLangID,
                                String      strDomain,
                                int         iUseSecureAuth) {
            StringBuilder szOut = new StringBuilder(4096);                
            int iRet = UnsafeNativeMethods.PassportLogoutURL(_iPassport,
                                                             szReturnURL,
                                                             szCOBrandArgs,
                                                             iLangID,
                                                             strDomain,
                                                             iUseSecureAuth,
                                                             szOut,
                                                             4096);

            if (iRet < 0)
                throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
            return szOut.ToString();
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////


        /// <devdoc>
        ///    True if the Passport member's ticket
        ///    indicates that they have chosen to save the password on the Passport login page
        ///    of the last ticket refresh.
        /// </devdoc>
        public bool HasSavedPassword {
            get {
                int iRet = UnsafeNativeMethods.PassportGetHasSavedPassword(_iPassport);
                if (iRet < 0)
                    throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
                return(iRet == 0);
            }
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////


        /// <devdoc>
        ///    True if there is a Passport ticket as a
        ///    cookie on thequery string.
        /// </devdoc>
        public bool HasTicket {
            get {
                int iRet = UnsafeNativeMethods.PassportHasTicket(_iPassport);
                if (iRet < 0)
                    throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
                return(iRet == 0);
            }
        }


        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////


        /// <devdoc>
        ///    Time in seconds since the last ticket was
        ///    issued or refreshed.
        /// </devdoc>
        public int TicketAge {
            get {
                int iRet = UnsafeNativeMethods.PassportGetTicketAge(_iPassport);
                if (iRet < 0)
                    throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
                return iRet;
            }
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////


        /// <devdoc>
        ///    The time is seconds since a member's
        ///    sign-in to the Passport login server.
        /// </devdoc>
        public int TimeSinceSignIn {
            get {
                int iRet = UnsafeNativeMethods.PassportGetTimeSinceSignIn(_iPassport);
                if (iRet < 0)
                    throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
                return iRet;
            }
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        // Functions

        /// <devdoc>
        ///    <para>Returns an HTML snippet containing an image tag for a Passport link. This is 
        ///       based on the current state of the identity (already signed in, and such).</para>
        /// </devdoc>
          public  String  LogoTag() {
              return LogoTag(null, -1, 
                             -1, null, -1, -1,
                             null, -1, -1);
          }


          public  String  LogoTag(String strReturnUrl) {
              return LogoTag(strReturnUrl, -1, 
                             -1, null, -1, -1,
                             null, -1, -1);
          }



        public  String  LogoTag2() {
            return LogoTag2(null, -1, 
                           -1, null, -1, -1,
                           null, -1, -1);
        }


        public  String  LogoTag2(String strReturnUrl) {
            return LogoTag2(strReturnUrl, -1, 
                           -1, null, -1, -1,
                           null, -1, -1);
        }


        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////


        /// <devdoc>
        ///    Similar to LogoTag(), this method returns
        ///    an HTML snippet for the Passport Logo for the current member
        /// </devdoc>
          public String LogoTag(String    strReturnUrl, 
                                  int       iTimeWindow, 
                                  bool      fForceLogin, 
                                  String    strCoBrandedArgs,
                                  int       iLangID,
                                bool      fSecure,
                                String    strNameSpace,
                                int       iKPP,
                                bool      bUseSecureAuth)
        {
            return LogoTag(                                                
                    strReturnUrl, 
                    iTimeWindow, 
                    (fForceLogin ? 1 : 0), 
                    strCoBrandedArgs, 
                    iLangID,
                    fSecure ? 1 : 0,
                    strNameSpace,
                    iKPP,
                    bUseSecureAuth ? 10 : 0);
        }


        public String LogoTag(String    strReturnUrl, 
                              int       iTimeWindow, 
                              int       iForceLogin, 
                              String    strCoBrandedArgs,
                              int       iLangID,
                              int       iSecure,
                              String    strNameSpace,
                              int       iKPP,
                              int       iUseSecureAuth)
        {
            StringBuilder str = new StringBuilder(4092);
            int iRet = UnsafeNativeMethods.PassportLogoTag(_iPassport, 
                                                     strReturnUrl, 
                                                     iTimeWindow, 
                                                     iForceLogin, 
                                                     strCoBrandedArgs, 
                                                     iLangID,
                                                     iSecure,
                                                     strNameSpace,
                                                     iKPP,
                                                     iUseSecureAuth,
                                                     str, 
                                                     4090);

            if (iRet < 0)
                throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
            return str.ToString();
        }




        public String LogoTag2(String    strReturnUrl, 
                                int       iTimeWindow, 
                                bool      fForceLogin, 
                                String    strCoBrandedArgs,
                                int       iLangID,
                                bool      fSecure,
                                String    strNameSpace,
                                int       iKPP,
                                bool      bUseSecureAuth)
        {
            return LogoTag2(
                    strReturnUrl, 
                    iTimeWindow, 
                    fForceLogin ? 1 : 0, 
                    strCoBrandedArgs,
                    iLangID,
                    fSecure ? 1 : 0,
                    strNameSpace,
                    iKPP,
                    bUseSecureAuth ? 10 : 0);
        }


        public String LogoTag2(String    strReturnUrl, 
                              int       iTimeWindow, 
                              int       iForceLogin, 
                              String    strCoBrandedArgs,
                              int       iLangID,
                              int       iSecure,
                              String    strNameSpace,
                              int       iKPP,
                              int       iUseSecureAuth)
        {
            StringBuilder str = new StringBuilder(4092);
            int iRet = UnsafeNativeMethods.PassportLogoTag2(_iPassport, 
                                                     strReturnUrl, 
                                                     iTimeWindow, 
                                                     iForceLogin, 
                                                     strCoBrandedArgs, 
                                                     iLangID,
                                                     iSecure,
                                                     strNameSpace,
                                                     iKPP,
                                                     iUseSecureAuth,
                                                     str, 
                                                     4090);

            if (iRet < 0)
                throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
            return str.ToString();
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////


        /// <devdoc>
        /// </devdoc>
        public  String  AuthUrl() {
            return AuthUrl(null, -1, -1, null, -1, null, -1, -1); 
        }


        public  String  AuthUrl(String strReturnUrl) {
            return AuthUrl(strReturnUrl, -1, -1, null, -1, null, -1, -1); 
        }


        public  String  AuthUrl2() {
            return AuthUrl2(null, -1, -1, null, -1, null, -1, -1); 
        }


        public  String  AuthUrl2(String strReturnUrl) {
            return AuthUrl2(strReturnUrl, -1, -1, null, -1, null, -1, -1); 
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////


        /// <devdoc>
        ///    <para>Similar to AuthUrl(). Returns the authentication server URL for
        ///       a member</para>
        /// </devdoc>
        public String AuthUrl  (String    strReturnUrl, 
                                int       iTimeWindow, 
                                bool      fForceLogin, 
                                String    strCoBrandedArgs,
                                int       iLangID,
                                String    strNameSpace,
                                int       iKPP,
                                bool      bUseSecureAuth)            
        {
            StringBuilder str = new StringBuilder(4092);
            int iRet = UnsafeNativeMethods.PassportAuthURL(_iPassport, 
                                                     strReturnUrl, 
                                                     iTimeWindow, 
                                                     (fForceLogin ? 1 : 0), 
                                                     strCoBrandedArgs, 
                                                     iLangID, 
                                                     strNameSpace,
                                                     iKPP,
                                                     bUseSecureAuth ? 10 : 0,
                                                     str,
                                                     4090);

            if (iRet < 0)
                throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
            return str.ToString();
        }


        public String AuthUrl2  (String    strReturnUrl, 
                                int       iTimeWindow, 
                                bool      fForceLogin, 
                                String    strCoBrandedArgs,
                                int       iLangID,
                                String    strNameSpace,
                                int       iKPP,
                                bool      bUseSecureAuth)
            
        {
            StringBuilder str = new StringBuilder(4092);
            int iRet = UnsafeNativeMethods.PassportAuthURL2(_iPassport, 
                                                     strReturnUrl, 
                                                     iTimeWindow, 
                                                     (fForceLogin ? 1 : 0), 
                                                     strCoBrandedArgs, 
                                                     iLangID, 
                                                     strNameSpace,
                                                     iKPP,
                                                     bUseSecureAuth ? 10 : 0,
                                                     str,
                                                     4090);

            if (iRet < 0)
                throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
            return str.ToString();
        }


        public String AuthUrl  (String    strReturnUrl, 
                                int       iTimeWindow, 
                                int       iForceLogin, 
                                String    strCoBrandedArgs,
                                int       iLangID,
                                String    strNameSpace,
                                int       iKPP,
                                int       iUseSecureAuth)

        {
            StringBuilder str = new StringBuilder(4092);
            int iRet = UnsafeNativeMethods.PassportAuthURL(_iPassport, 
                                                     strReturnUrl, 
                                                     iTimeWindow, 
                                                     iForceLogin, 
                                                     strCoBrandedArgs, 
                                                     iLangID, 
                                                     strNameSpace,
                                                     iKPP,
                                                     iUseSecureAuth,
                                                     str,
                                                     4090);

            if (iRet < 0)
                throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
            return str.ToString();
        }


        public String AuthUrl2  (String    strReturnUrl, 
                                int       iTimeWindow, 
                                int       iForceLogin, 
                                String    strCoBrandedArgs,
                                int       iLangID,
                                String    strNameSpace,
                                int       iKPP,
                                int       iUseSecureAuth) {
            StringBuilder str = new StringBuilder(4092);
            int iRet = UnsafeNativeMethods.PassportAuthURL2(_iPassport, 
                                                     strReturnUrl, 
                                                     iTimeWindow, 
                                                     iForceLogin, 
                                                     strCoBrandedArgs, 
                                                     iLangID, 
                                                     strNameSpace,
                                                     iKPP,
                                                     iUseSecureAuth,
                                                     str,
                                                     4090);

            if (iRet < 0)
                throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
            return str.ToString();
        }


        public int LoginUser(
                String        szRetURL,
                int           iTimeWindow,
                bool          fForceLogin,
                String        szCOBrandArgs,
                int           iLangID,
                String        strNameSpace,
                int           iKPP,
                bool          fUseSecureAuth,
                object        oExtraParams) {
            return LoginUser(
                szRetURL,
                iTimeWindow,
                fForceLogin ? 1 : 0,
                szCOBrandArgs,
                iLangID,
                strNameSpace,
                iKPP,
                fUseSecureAuth ? 10 : 0,
                oExtraParams);
        }
                

        public int LoginUser(
                String        szRetURL,
                int           iTimeWindow,
                int           fForceLogin,
                String        szCOBrandArgs,
                int           iLangID,
                String        strNameSpace,
                int           iKPP,
                int           iUseSecureAuth,
                object        oExtraParams) {

            String str = GetLoginChallenge(szRetURL, iTimeWindow, fForceLogin, szCOBrandArgs, 
                                           iLangID, strNameSpace, iKPP, iUseSecureAuth, oExtraParams);

            if (str == null || str.Length < 1)
                return -1;

            HttpContext context = HttpContext.Current;
            SetHeaders(context, str);
            _WWWAuthHeaderSet = true;

            str = context.Request.Headers["Accept-Auth"];
            if (str != null && str.Length > 0 && str.IndexOf("Passport", StringComparison.Ordinal) >= 0) {
                context.Response.StatusCode = 401;
                context.Response.End();
                return 0;
            }

            str = AuthUrl(szRetURL, iTimeWindow, fForceLogin, szCOBrandArgs,
                          iLangID, strNameSpace, iKPP, iUseSecureAuth);

            if (!String.IsNullOrEmpty(str)) {
                context.Response.Redirect(str, false);
                return 0;
            }

            return -1;
        }
                


        public int LoginUser() {
            return LoginUser(null, -1, -1, null, -1, null, -1, -1, null);            
        }


        public int LoginUser(String strReturnUrl) {
            return LoginUser(strReturnUrl, -1, -1, null, -1, null, -1, -1, null);            
        }
                


        public String GetLoginChallenge() {
            return GetLoginChallenge(null, -1, -1, null, -1, null, -1, -1, null);
        }


        public String GetLoginChallenge(String strReturnUrl) {
            return GetLoginChallenge(strReturnUrl, -1, -1, null, -1, null, -1, -1, null);
        }


        public String GetLoginChallenge(
                String        szRetURL,
                int           iTimeWindow,
                int           fForceLogin,
                String        szCOBrandArgs,
                int           iLangID,
                String        strNameSpace,
                int           iKPP,
                int           iUseSecureAuth,
                object        oExtraParams) {

            StringBuilder str = new StringBuilder(4092);
            int iRet = UnsafeNativeMethods.PassportGetLoginChallenge(
                    _iPassport,                    
                    szRetURL,
                    iTimeWindow,
                    fForceLogin,
                    szCOBrandArgs,
                    iLangID,
                    strNameSpace,
                    iKPP,
                    iUseSecureAuth,
                    oExtraParams,
                    str, 4090);

            if (iRet < 0)
                throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
            String strRet = str.ToString();
            if (strRet != null && !StringUtil.StringStartsWith(strRet, "WWW-Authenticate"))
                strRet = "WWW-Authenticate: " + strRet;
            return strRet;
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////


        /// <devdoc>
        ///    Provides information for a Passport domain
        ///    by querying the Passport CCD for the requested domain attribute.
        /// </devdoc>
        public String GetDomainAttribute(String strAttribute, int iLCID, String strDomain) {
            StringBuilder str = new  StringBuilder(1028);            
            int iRet = UnsafeNativeMethods.PassportGetDomainAttribute(_iPassport, strAttribute, iLCID, strDomain, str, 1024);
            if (iRet >= 0)
                return str.ToString();
            else
                throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
        }


        public Object Ticket(String strAttribute) {
            Object oOut = new Object();
            int iRet = UnsafeNativeMethods.PassportTicket(_iPassport, strAttribute, out oOut);
            if (iRet < 0)
                throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
            return oOut;
        }    


        public Object GetCurrentConfig(String strAttribute) {
            Object oOut = new Object();
            int iRet = UnsafeNativeMethods.PassportGetCurrentConfig(_iPassport, strAttribute, out oOut);
            if (iRet < 0)
                throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
            return oOut;
        }    


        public String HexPUID { 
            get {
                StringBuilder str = new StringBuilder(1024);
                int iRet = UnsafeNativeMethods.PassportHexPUID(_iPassport, str, 1024);
                if (iRet >= 0)
                    return str.ToString();
                else
                    throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
            } 
        }
        


        /// <internalonly/>
        void IDisposable.Dispose()
        {
            if (_iPassport != IntPtr.Zero)
                UnsafeNativeMethods.PassportDestroy(_iPassport);
            _iPassport = IntPtr.Zero;

            GC.SuppressFinalize(this);
        }

        /// <devdoc>
        ///    Signs out the given Passport member from
        ///    their current session.
        /// </devdoc>
        public static void SignOut(String strSignOutDotGifFileName) {
            HttpContext      context         = HttpContext.Current;
            String []        sCookieNames    = {"MSPAuth", "MSPProf", "MSPConsent", "MSPSecAuth", "MSPProfC"};
            String []        sCookieDomains  = {"TicketDomain", "TicketDomain", "ProfileDomain", "SecureDomain", "TicketDomain"};
            String []        sCookiePaths    = {"TicketPath", "TicketPath", "ProfilePath", "SecurePath", "TicketPath"};
            String []        sCookieDomainsV = new String[5];
            String []        sCookiePathsV   = new String[5];
            PassportIdentity pi              = null;
            int              iter            = 0;
            
            ////////////////////////////////////////////////////////////
            // Step 1: Clear all headers
            context.Response.ClearHeaders();

            ////////////////////////////////////////////////////////////
            // Step 2: Get passport config information (if using Passport 2.0 sdk)
            try {
                if (context.User.Identity is PassportIdentity) {
                    pi = (PassportIdentity) context.User.Identity;
                } else {
                    pi = new PassportIdentity();
                }

                if (pi != null && _iPassportVer >= 3) {
                    // Get Domains
                    for(iter=0; iter<5; iter++) {
                        Object obj = pi.GetCurrentConfig(sCookieDomains[iter]);
                        if (obj != null && (obj is String))
                            sCookieDomainsV[iter] = (String) obj; 
                    }
                    
                    // Get Paths
                    for(iter=0; iter<5; iter++) {
                        Object obj = pi.GetCurrentConfig(sCookiePaths[iter]);
                        if (obj != null && (obj is String))
                            sCookiePathsV[iter] = (String) obj; 
                    }
                }
            }
            catch { 
                // ---- exceptions
            }                            


            ////////////////////////////////////////////////////////////
            // Step 3: Add cookies
            for(iter=0; iter<5; iter++) {
                HttpCookie cookie = new HttpCookie(sCookieNames[iter], String.Empty);
                cookie.Expires = new DateTime(1998, 1, 1);
                if (sCookieDomainsV[iter] != null && sCookieDomainsV[iter].Length > 0)
                    cookie.Domain = sCookieDomainsV[iter];
                if (sCookiePathsV[iter] != null && sCookiePathsV[iter].Length > 0)
                    cookie.Path = sCookiePathsV[iter];
                else
                    cookie.Path = "/";
                context.Response.Cookies.Add(cookie);      
            }

            // context.Response.AppendHeader("P3P", "CP=\"TST\"");

            ////////////////////////////////////////////////////////////
            // Step 4: Add no-cache headers
            context.Response.Expires = -1;
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.AppendHeader("Pragma", "no-cache");
            context.Response.ContentType = "image/gif";

            ////////////////////////////////////////////////////////////
            // Step 5: Write Image file
            context.Response.WriteFile(strSignOutDotGifFileName);

            ////////////////////////////////////////////////////////////
            // Step 6: Mobile device support: Redirect to the "ru" in the QS (if present)
            String strRU = context.Request.QueryString["ru"];
            if (strRU != null && strRU.Length > 1) 
                context.Response.Redirect(strRU, false);
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////

        /// <devdoc>
        ///    Encrypts data using the Passport
        ///    participant keycfor the current site. Maximum input size is 2045 characters.
        /// </devdoc>
        static public String Encrypt(String strData) {
            return CallPassportCryptFunction(0, strData);
        }


        /// <devdoc>
        ///    Decrypts data using the Passport
        ///    participant key for the current site.
        /// </devdoc>
        static public String Decrypt(String strData) {
            return CallPassportCryptFunction(1, strData);
        }


        static public String Compress(String strData) {
            return CallPassportCryptFunction(2, strData);
        }

        static public String Decompress(String strData) {
            return CallPassportCryptFunction(3, strData);
        }


        static public int CryptPutHost(String strHost) {
            int iRet =  UnsafeNativeMethods.PassportCryptPut(0, strHost);
            if (iRet < 0)
                throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    

            return iRet;
        }


        static public int CryptPutSite(String strSite) {
            int iRet = UnsafeNativeMethods.PassportCryptPut(1, strSite);
            if (iRet < 0)
                throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    

            return iRet;
        }


        static public bool CryptIsValid() {
            int iRet = UnsafeNativeMethods.PassportCryptIsValid();
            if (iRet < 0)
                throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
            return (iRet == 0);
        }  


        static private String CallPassportCryptFunction(int iFunctionID, String strData) {           
            int   iRet  = 0;
            int   iSize = ((strData == null || strData.Length < 512) ? 512 : strData.Length);

            do  {
                iSize *= 2;
                StringBuilder str = new StringBuilder(iSize);                
                iRet = UnsafeNativeMethods.PassportCrypt(iFunctionID, strData, str, iSize);                            

                if (iRet == 0) // Worked
                    return str.ToString();
                if (iRet != HResults.E_INSUFFICIENT_BUFFER && iRet < 0)
                    throw new COMException(SR.GetString(SR.Passport_method_failed), iRet);                    
            }
            while  ( iRet ==  HResults.E_INSUFFICIENT_BUFFER &&
                     iSize < 10*1024*1024 ); // Less than 10MB

            return null;
        }
    }
}
