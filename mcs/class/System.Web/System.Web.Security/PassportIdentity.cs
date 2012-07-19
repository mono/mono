//
// System.Web.Security.PassportIdentity.cs
//
// Authors:
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Security.Permissions;
using System.Security.Principal;

namespace System.Web.Security {

	[MonoNotSupported ("")]
	[MonoTODO("Not implemented")]
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#if NET_4_0
	[Obsolete ("This type is obsolete. The Passport authentication product is no longer supported and has been superseded by Live ID.")]
#endif
#if NET_2_0
	public sealed class PassportIdentity : IIdentity, IDisposable {
#else
	public sealed class PassportIdentity : IIdentity {
#endif
		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
		public PassportIdentity ()
		{
		}

		~PassportIdentity ()
		{
		}

		public string AuthUrl ()
		{
			return AuthUrl (null, -1, -1, null, -1, null, -1, -1);
		}

		public string AuthUrl (String strReturnUrl)
		{
			return AuthUrl (strReturnUrl, -1, -1, null, -1, null, -1, -1);
		}

		public string AuthUrl (string strReturnUrl,
				       int iTimeWindow,
				       bool fForceLogin,
				       string strCoBrandedArgs,
				       int iLangID,
				       string strNameSpace,
				       int iKPP,
				       bool bUseSecureAuth)
		{
			return AuthUrl (strReturnUrl, iTimeWindow, (fForceLogin ? 1 : 0), strCoBrandedArgs, iLangID, strNameSpace, iKPP, (bUseSecureAuth ? 1 : 0));
		}

		[MonoTODO("Not implemented")]
		public string AuthUrl (string strReturnUrl,
				       int iTimeWindow,
				       int iForceLogin,
				       string strCoBrandedArgs,
				       int iLangID,
				       string strNameSpace,
				       int iKPP,
				       int iUseSecureAuth)
		{
			throw new NotImplementedException ();
		}

		public string AuthUrl2 ()
		{
			return AuthUrl2 (null, -1, -1, null, -1, null, -1, -1);
		}

		public string AuthUrl2 (String strReturnUrl)
		{
			return AuthUrl2 (strReturnUrl, -1, -1, null, -1, null, -1, -1);
		}

		public string AuthUrl2 (string strReturnUrl,
					int iTimeWindow,
					bool fForceLogin,
					string strCoBrandedArgs,
					int iLangID,
					string strNameSpace,
					int iKPP,
					bool bUseSecureAuth)
		{
			return AuthUrl2 (strReturnUrl, iTimeWindow, (fForceLogin ? 1 : 0), strCoBrandedArgs, iLangID, strNameSpace, iKPP, (bUseSecureAuth ? 1 : 0));
		}

		[MonoTODO("Not implemented")]
		public string AuthUrl2 (string strReturnUrl,
					int iTimeWindow,
					int iForceLogin,
					string strCoBrandedArgs,
					int iLangID,
					string strNameSpace,
					int iKPP,
					int iUseSecureAuth)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public static string Compress (string strData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public static bool CryptIsValid ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public static int CryptPutHost (string strHost)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public static int CryptPutSite (string strSite)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public static string Decompress (string strData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public static string Decrypt (string strData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public static string Encrypt (string strData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public object GetCurrentConfig (string strAttribute)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public string GetDomainAttribute (string strAttribute, int iLCID, string strDomain)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public string GetDomainFromMemberName (string strMemberName)
		{
			throw new NotImplementedException ();
		}

		public bool GetIsAuthenticated (int iTimeWindow, bool bForceLogin, bool bCheckSecure)
		{
			return this.GetIsAuthenticated (iTimeWindow, (bForceLogin ? 1 : 0), (bCheckSecure ? 1 : 0));
		}

		[MonoTODO("Not implemented")]
		public bool GetIsAuthenticated (int iTimeWindow, int iForceLogin, int iCheckSecure)
		{
			throw new NotImplementedException ();
		}

		public string GetLoginChallenge ()
		{
			return GetLoginChallenge (null, -1, -1, null, -1, null, -1, -1, null);
		}

		public string GetLoginChallenge (String strReturnUrl)
		{
			return GetLoginChallenge (strReturnUrl, -1, -1, null, -1, null, -1, -1, null);
		}

		[MonoTODO("Not implemented")]
		public string GetLoginChallenge (string szRetURL,
						 int iTimeWindow,
						 int fForceLogin,
						 string szCOBrandArgs,
						 int iLangID,
						 string strNameSpace,
						 int iKPP,
						 int iUseSecureAuth,
						 object oExtraParams)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public object GetOption (string strOpt)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public object GetProfileObject (string strProfileName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public bool HasFlag (int iFlagMask)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public bool HasProfile (string strProfile)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public bool HaveConsent (bool bNeedFullConsent, bool bNeedBirthdate)
		{
			throw new NotImplementedException ();
		}

		public int LoginUser ()
		{
			return LoginUser (null, -1, -1, null, -1, null, -1, -1, null);
		}

		public int LoginUser (String strReturnUrl)
		{
			return LoginUser (strReturnUrl, -1, -1, null, -1, null, -1, -1, null);
		}

		public int LoginUser (string szRetURL,
				      int iTimeWindow,
				      bool fForceLogin,
				      string szCOBrandArgs,
				      int iLangID,
				      string strNameSpace,
				      int iKPP,
				      bool fUseSecureAuth,
				      object oExtraParams)
		{
			return LoginUser (szRetURL, iTimeWindow, (fForceLogin ? 1 : 0), szCOBrandArgs, iLangID, strNameSpace, iKPP, (fUseSecureAuth ? 1 : 0), null);
		}

		[MonoTODO("Not implemented")]
		public int LoginUser (string szRetURL,
				      int iTimeWindow,
				      int fForceLogin,
				      string szCOBrandArgs,
				      int iLangID,
				      string strNameSpace,
				      int iKPP,
				      int iUseSecureAuth,
				      object oExtraParams)
		{
			throw new NotImplementedException ();
		}

		public string LogoTag ()
		{
			return LogoTag (null, -1, -1, null, -1, -1, null, -1, -1);
		}

		public string LogoTag (String strReturnUrl)
		{
			return LogoTag (strReturnUrl, -1, -1, null, -1, -1, null, -1, -1);
		}

		public string LogoTag (string strReturnUrl,
				       int iTimeWindow,
				       bool fForceLogin,
				       string strCoBrandedArgs,
				       int iLangID,
				       bool fSecure,
				       string strNameSpace,
				       int iKPP,
				       bool bUseSecureAuth)
		{
			return LogoTag (strReturnUrl, iTimeWindow, (fForceLogin ? 1 : 0), strCoBrandedArgs, iLangID, (fSecure ? 1 : 0), strNameSpace, iKPP, (bUseSecureAuth ? 1 : 0));
		}

		[MonoTODO("Not implemented")]
		public string LogoTag (string strReturnUrl,
				       int iTimeWindow,
				       int iForceLogin,
				       string strCoBrandedArgs,
				       int iLangID,
				       int iSecure,
				       string strNameSpace,
				       int iKPP,
				       int iUseSecureAuth)
		{
			throw new NotImplementedException ();
		}

		public string LogoTag2 ()
		{
			return LogoTag2 (null, -1, -1, null, -1, -1, null, -1, -1);
		}

		public string LogoTag2 (String strReturnUrl)
		{
			return LogoTag2 (strReturnUrl, -1, -1, null, -1, -1, null, -1, -1);
		}

		public string LogoTag2 (string strReturnUrl,
				        int iTimeWindow,
				        bool fForceLogin,
				        string strCoBrandedArgs,
				        int iLangID,
				        bool fSecure,
				        string strNameSpace,
				        int iKPP,
				        bool bUseSecureAuth)
		{
			return LogoTag2 (strReturnUrl, iTimeWindow, (fForceLogin ? 1 : 0), strCoBrandedArgs, iLangID, (fSecure ? 1 : 0), strNameSpace, iKPP, (bUseSecureAuth ? 1 : 0));
		}

		[MonoTODO("Not implemented")]
		public string LogoTag2 (string strReturnUrl,
				        int iTimeWindow,
				        int iForceLogin,
				        string strCoBrandedArgs,
				        int iLangID,
				        int iSecure,
				        string strNameSpace,
				        int iKPP,
				        int iUseSecureAuth)
		{
			throw new NotImplementedException ();
		}

		public string LogoutURL ()
		{
			return LogoutURL (null, null, -1, null, -1);
		}

		[MonoTODO("Not implemented")]
		public string LogoutURL (string szReturnURL,
					 string szCOBrandArgs,
					 int iLangID,
					 string strDomain,
					 int iUseSecureAuth)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public void SetOption (string strOpt, object vOpt)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public static void SignOut (string strSignOutDotGifFileName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public object Ticket (string strAttribute)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public string AuthenticationType
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO("Not implemented")]
		public int Error
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO("Not implemented")]
		public bool GetFromNetworkServer
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO("Not implemented")]
		public bool HasSavedPassword
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO("Not implemented")]
		public bool HasTicket
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO("Not implemented")]
		public string HexPUID
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO("Not implemented")]
		public bool IsAuthenticated
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO("Not implemented")]
		public string this [string strProfileName]
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO("Not implemented")]
		public string Name
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO("Not implemented")]
		public int TicketAge
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO("Not implemented")]
		public int TimeSinceSignIn
		{
			get {
				throw new NotImplementedException ();
			}
		}

#if NET_2_0
		void IDisposable.Dispose ()
		{
		}
#endif
	}
}

