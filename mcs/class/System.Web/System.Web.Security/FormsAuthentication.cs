//
// System.Web.Security.FormsAuthentication
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Web;

namespace System.Web.Security
{
	public sealed class FormsAuthentication
	{
		[MonoTODO]
		public static bool Authenticate (string name, string password)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static FormsAuthenticationTicket Decrypt (string encryptedTicket)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string Encrypt (FormsAuthenticationTicket ticket)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static HttpCookie GetAuthCookie (string userName, bool createPersistentCookie)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static HttpCookie GetAuthCookie (string userName, bool createPersistentCookie, string strCookiePath)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static string GetRedirectUrl (string userName, bool createPersistentCookie)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string HashPasswordForStoringInConfigFile (string password, string passwordFormat)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void Initialize ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void RedirectFromLoginPage (string userName, bool createPersistentCookie)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void RedirectFromLoginPage (string userName, bool createPersistentCookie, string strCookiePath)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static FormsAuthenticationTicket RenewTicketIfOld (FormsAuthenticationTicket tOld)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void SetAuthCookie (string userName, bool createPersistentCookie)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void SetAuthCookie (string userName, bool createPersistentCookie, string strCookiePath)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public static void SignOut ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string FormsCookieName
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public static string FormsCookiePath
		{
			get {
				throw new NotImplementedException ();
			}
		}
	}
}

