//
// System.Web.Security.PassportIdentity
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Security.Principal;

namespace System.Web.Security
{
	public sealed class PassportIdentity : IIdentity
	{
		[MonoTODO]
		public PassportIdentity ()
		{
			throw new NotImplementedException ();
		}

		~PassportIdentity ()
		{
		}

		[MonoTODO]
		public string AuthUrl ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string AuthUrl (string strReturnUrl,
				       int iTimeWindow,
				       bool fForceLogin,
				       string strCoBrandedArgs,
				       int iLangID,
				       string strNameSpace,
				       int iKPP,
				       bool bUseSecureAuth)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
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

		[MonoTODO]
		public string AuthUrl2 ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string AuthUrl2 (string strReturnUrl,
					int iTimeWindow,
					bool fForceLogin,
					string strCoBrandedArgs,
					int iLangID,
					string strNameSpace,
					int iKPP,
					bool bUseSecureAuth)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
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

		[MonoTODO]
		public static string Compress (string strData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool CryptIsValid ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int CryptPutHost (string strHost)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int CryptPutSite (string strSite)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string Decompress (string strData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string Decrypt (string strData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string Encrypt (string strData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object GetCurrentConfig (string strAttribute)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetDomainAttribute (string strAttribute, int iLCID, string strDomain)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetDomainFromMemberName (string strMemberName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool GetIsAuthenticated (int iTimeWindow, bool bForceLogin, bool bCheckSecure)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool GetIsAuthenticated (int iTimeWindow, int iForceLogin, int iCheckSecure)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetLoginChallenge ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
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

		[MonoTODO]
		public object GetOption (string strOpt)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object GetProfileObject (string strProfileName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool HasFlag (int iFlagMask)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool HasProfile (string strProfile)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool HaveConsent (bool bNeedFullConsent, bool bNeedBirthdate)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int LoginUser ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
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
			throw new NotImplementedException ();
		}

		[MonoTODO]
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

		[MonoTODO]
		public string LogoTag ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
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
			throw new NotImplementedException ();
		}

		[MonoTODO]
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

		[MonoTODO]
		public string LogoTag2 ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
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
			throw new NotImplementedException ();
		}

		[MonoTODO]
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

		[MonoTODO]
		public string LogoutURL ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string LogoutURL (string szReturnURL,
					 string szCOBrandArgs,
					 int iLangID,
					 string strDomain,
					 int iUseSecureAuth)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetOption (string strOpt, object vOpt)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void SignOut (string strSignOutDotGifFileName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object Ticket (string strAttribute)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string AuthenticationType
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public int Error
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool GetFromNetworkServer
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool HasSavedPassword
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool HasTicket
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string HexPUID
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool IsAuthenticated
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string this [string strProfileName]
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string Name
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public int TicketAge
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public int TimeSinceSignIn
		{
			get {
				throw new NotImplementedException ();
			}
		}
	}
}

