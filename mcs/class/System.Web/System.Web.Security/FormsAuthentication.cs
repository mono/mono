//
// System.Web.Security.FormsAuthentication
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
// Copyright (c) 2005-2010 Novell, Inc (http://www.novell.com)
//

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

using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Compilation;
using System.Web.Util;
using System.Globalization;
using System.Collections.Specialized;

namespace System.Web.Security
{
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class FormsAuthentication
	{
		static string authConfigPath = "system.web/authentication";
		static string machineKeyConfigPath = "system.web/machineKey";
		static object locker = new object ();
#if TARGET_J2EE
		const string Forms_initialized = "Forms.initialized";
		const string Forms_cookieName = "Forms.cookieName";
		const string Forms_cookiePath = "Forms.cookiePath";
		const string Forms_timeout = "Forms.timeout";
		const string Forms_protection = "Forms.protection";
		static bool initialized
		{
			get {
				object o = AppDomain.CurrentDomain.GetData (Forms_initialized);
				return o != null ? (bool) o : false;
			}
			set { AppDomain.CurrentDomain.SetData (Forms_initialized, value); }
		}
		static string cookieName
		{
			get { return (string) AppDomain.CurrentDomain.GetData (Forms_cookieName); }
			set { AppDomain.CurrentDomain.SetData (Forms_cookieName, value); }
		}
		static string cookiePath
		{
			get { return (string) AppDomain.CurrentDomain.GetData (Forms_cookiePath); }
			set { AppDomain.CurrentDomain.SetData (Forms_cookiePath, value); }
		}
		static int timeout
		{
			get {
				object o = AppDomain.CurrentDomain.GetData (Forms_timeout);
				return o != null ? (int) o : 0;
			}
			set { AppDomain.CurrentDomain.SetData (Forms_timeout, value); }
		}
		static FormsProtectionEnum protection
		{
			get { return (FormsProtectionEnum) AppDomain.CurrentDomain.GetData (Forms_protection); }
			set { AppDomain.CurrentDomain.SetData (Forms_protection, value); }
		}

		const string Forms_requireSSL = "Forms.requireSSL";
		const string Forms_slidingExpiration = "Forms.slidingExpiration";

		static bool requireSSL
		{
			get {
				object o = AppDomain.CurrentDomain.GetData (Forms_requireSSL);
				return o != null ? (bool) o : false;
			}
			set { AppDomain.CurrentDomain.SetData (Forms_requireSSL, value); }
		}
		static bool slidingExpiration
		{
			get {
				object o = AppDomain.CurrentDomain.GetData (Forms_slidingExpiration);
				return o != null ? (bool) o : false;
			}
			set { AppDomain.CurrentDomain.SetData (Forms_slidingExpiration, value); }
		}

		const string Forms_cookie_domain = "Forms.cookie_domain";
		const string Forms_cookie_mode = "Forms.cookie_mode";
		const string Forms_cookies_supported = "Forms.cookies_supported";
		const string Forms_default_url = "Forms.default_url";
		const string Forms_enable_crossapp_redirects = "Forms.enable_crossapp_redirects";
		const string Forms_login_url = "Forms.login_url";
		static string cookie_domain
		{
			get { return (string) AppDomain.CurrentDomain.GetData (Forms_cookie_domain); }
			set { AppDomain.CurrentDomain.SetData (Forms_cookie_domain, value); }
		}
		static HttpCookieMode cookie_mode
		{
			get { return (HttpCookieMode) AppDomain.CurrentDomain.GetData (Forms_cookie_mode); }
			set { AppDomain.CurrentDomain.SetData (Forms_cookie_mode, value); }
		}
		static bool cookies_supported
		{
			get {
				object o = AppDomain.CurrentDomain.GetData (Forms_cookies_supported);
				return o != null ? (bool) o : false;
			}
			set { AppDomain.CurrentDomain.SetData (Forms_cookies_supported, value); }
		}
		static string default_url
		{
			get { return (string) AppDomain.CurrentDomain.GetData (Forms_default_url); }
			set { AppDomain.CurrentDomain.SetData (Forms_default_url, value); }
		}
		static bool enable_crossapp_redirects
		{
			get {
				object o = AppDomain.CurrentDomain.GetData (Forms_enable_crossapp_redirects);
				return o != null ? (bool) o : false;
			}
			set { AppDomain.CurrentDomain.SetData (Forms_enable_crossapp_redirects, value); }
		}
		static string login_url
		{
			get { return (string) AppDomain.CurrentDomain.GetData (Forms_login_url); }
			set { AppDomain.CurrentDomain.SetData (Forms_login_url, value); }
		}
#else
		static bool initialized;
		static string cookieName;
		static string cookiePath;
		static int timeout;
		static FormsProtectionEnum protection;
		static bool requireSSL;
		static bool slidingExpiration;
		static string cookie_domain;
		static HttpCookieMode cookie_mode;
		static bool cookies_supported;
		static string default_url;
		static bool enable_crossapp_redirects;
		static string login_url;
#endif
		// same names and order used in xsp
		static string [] indexFiles = { "index.aspx",
						"Default.aspx",
						"default.aspx",
						"index.html",
						"index.htm" };
#if NET_4_0
		public static void EnableFormsAuthentication (NameValueCollection configurationData)
		{
			BuildManager.AssertPreStartMethodsRunning ();
			if (configurationData == null || configurationData.Count == 0)
				return;

			string value = configurationData ["loginUrl"];
			if (!String.IsNullOrEmpty (value))
				login_url = value;

			value = configurationData ["defaultUrl"];
			if (!String.IsNullOrEmpty (value))
				default_url = value;
		}
#endif
		public FormsAuthentication ()
		{
		}

		public static bool Authenticate (string name, string password)
		{
			if (name == null || password == null)
				return false;

			Initialize ();
			HttpContext context = HttpContext.Current;
			if (context == null)
				throw new HttpException ("Context is null!");

			name = name.ToLower (Helpers.InvariantCulture);

			AuthenticationSection section = (AuthenticationSection) WebConfigurationManager.GetSection (authConfigPath);
			FormsAuthenticationCredentials config = section.Forms.Credentials;
			FormsAuthenticationUser user = config.Users[name];
			string stored = null;

			if (user != null)
				stored = user.Password;

			if (stored == null)
				return false;

			bool caseInsensitive = true;
			switch (config.PasswordFormat) {
				case FormsAuthPasswordFormat.Clear:
					caseInsensitive = false;
					/* Do nothing */
					break;
				case FormsAuthPasswordFormat.MD5:
					password = HashPasswordForStoringInConfigFile (password, FormsAuthPasswordFormat.MD5);
					break;
				case FormsAuthPasswordFormat.SHA1:
					password = HashPasswordForStoringInConfigFile (password, FormsAuthPasswordFormat.SHA1);
					break;
			}

			return String.Compare (password, stored, caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0;
		}

		static FormsAuthenticationTicket Decrypt2 (byte [] bytes)
		{
			if (protection == FormsProtectionEnum.None)
				return FormsAuthenticationTicket.FromByteArray (bytes);

			MachineKeySection config = (MachineKeySection) WebConfigurationManager.GetWebApplicationSection (machineKeyConfigPath);
			byte [] result = null;
			if (protection == FormsProtectionEnum.All) {
				result = MachineKeySectionUtils.VerifyDecrypt (config, bytes);
			} else if (protection == FormsProtectionEnum.Encryption) {
				result = MachineKeySectionUtils.Decrypt (config, bytes);
			} else if (protection == FormsProtectionEnum.Validation) {
				result = MachineKeySectionUtils.Verify (config, bytes);
			}

			return FormsAuthenticationTicket.FromByteArray (result);
		}

		public static FormsAuthenticationTicket Decrypt (string encryptedTicket)
		{
			if (String.IsNullOrEmpty (encryptedTicket))
				throw new ArgumentException ("Invalid encrypted ticket", "encryptedTicket");

			Initialize ();

			FormsAuthenticationTicket ticket;
			byte [] bytes = Convert.FromBase64String (encryptedTicket);

			try {
				ticket = Decrypt2 (bytes);
			} catch (Exception) {
				ticket = null;
			}

			return ticket;
		}

		public static string Encrypt (FormsAuthenticationTicket ticket)
		{
			if (ticket == null)
				throw new ArgumentNullException ("ticket");

			Initialize ();
			byte [] ticket_bytes = ticket.ToByteArray ();
			if (protection == FormsProtectionEnum.None)
				return Convert.ToBase64String (ticket_bytes);

			byte [] result = null;
			MachineKeySection config = (MachineKeySection) WebConfigurationManager.GetWebApplicationSection (machineKeyConfigPath);

			if (protection == FormsProtectionEnum.All) {
				result = MachineKeySectionUtils.EncryptSign (config, ticket_bytes);
			} else if (protection == FormsProtectionEnum.Encryption) {
				result = MachineKeySectionUtils.Encrypt (config, ticket_bytes);
			} else if (protection == FormsProtectionEnum.Validation) {
				result = MachineKeySectionUtils.Sign (config, ticket_bytes);
			}

			return Convert.ToBase64String (result);
		}

		public static HttpCookie GetAuthCookie (string userName, bool createPersistentCookie)
		{
			return GetAuthCookie (userName, createPersistentCookie, null);
		}

		public static HttpCookie GetAuthCookie (string userName, bool createPersistentCookie, string strCookiePath)
		{
			Initialize ();

			if (userName == null)
				userName = String.Empty;

			if (strCookiePath == null || strCookiePath.Length == 0)
				strCookiePath = cookiePath;

			DateTime now = DateTime.Now;
			DateTime then;
			if (createPersistentCookie)
				then = now.AddYears (50);
			else
				then = now.AddMinutes (timeout);

			FormsAuthenticationTicket ticket = new FormsAuthenticationTicket (1,
											  userName,
											  now,
											  then,
											  createPersistentCookie,
											  String.Empty,
											  cookiePath);

			if (!createPersistentCookie)
				then = DateTime.MinValue;

			HttpCookie cookie = new HttpCookie (cookieName, Encrypt (ticket), strCookiePath, then);
			if (requireSSL)
				cookie.Secure = true;
			if (!String.IsNullOrEmpty (cookie_domain))
				cookie.Domain = cookie_domain;

			return cookie;
		}

		internal static string ReturnUrl {
			get { return HttpContext.Current.Request ["RETURNURL"]; }
		}

		public static string GetRedirectUrl (string userName, bool createPersistentCookie)
		{
			if (userName == null)
				return null;

			Initialize ();
			HttpRequest request = HttpContext.Current.Request;
			string returnUrl = ReturnUrl;
			if (returnUrl != null)
				return returnUrl;

			returnUrl = request.ApplicationPath;
			string apppath = request.PhysicalApplicationPath;
			bool found = false;

			foreach (string indexFile in indexFiles) {
				string filePath = Path.Combine (apppath, indexFile);
				if (File.Exists (filePath)) {
					returnUrl = UrlUtils.Combine (returnUrl, indexFile);
					found = true;
					break;
				}
			}

			if (!found)
				returnUrl = UrlUtils.Combine (returnUrl, "index.aspx");

			return returnUrl;
		}

		static string HashPasswordForStoringInConfigFile (string password, FormsAuthPasswordFormat passwordFormat)
		{
			if (password == null)
				throw new ArgumentNullException ("password");
			
			byte [] bytes;
			switch (passwordFormat) {
				case FormsAuthPasswordFormat.MD5:
					bytes = MD5.Create ().ComputeHash (Encoding.UTF8.GetBytes (password));
					break;

				case FormsAuthPasswordFormat.SHA1:
					bytes = SHA1.Create ().ComputeHash (Encoding.UTF8.GetBytes (password));
					break;

				default:
					throw new ArgumentException ("The format must be either MD5 or SHA1", "passwordFormat");
			}

			return MachineKeySectionUtils.GetHexString (bytes);
		}
		
		public static string HashPasswordForStoringInConfigFile (string password, string passwordFormat)
		{
			if (password == null)
				throw new ArgumentNullException ("password");

			if (passwordFormat == null)
				throw new ArgumentNullException ("passwordFormat");

			if (String.Compare (passwordFormat, "MD5", StringComparison.OrdinalIgnoreCase) == 0) {
				return HashPasswordForStoringInConfigFile (password, FormsAuthPasswordFormat.MD5);
			} else if (String.Compare (passwordFormat, "SHA1", StringComparison.OrdinalIgnoreCase) == 0) {
				return HashPasswordForStoringInConfigFile (password, FormsAuthPasswordFormat.SHA1);
			} else {
				throw new ArgumentException ("The format must be either MD5 or SHA1", "passwordFormat");
			}
		}

		public static void Initialize ()
		{
			if (initialized)
				return;

			lock (locker) {
				if (initialized)
					return;

				AuthenticationSection section = (AuthenticationSection)WebConfigurationManager.GetSection (authConfigPath);
				FormsAuthenticationConfiguration config = section.Forms;

				cookieName = config.Name;
				timeout = (int)config.Timeout.TotalMinutes;
				cookiePath = config.Path;
				protection = config.Protection;
				requireSSL = config.RequireSSL;
				slidingExpiration = config.SlidingExpiration;
				cookie_domain = config.Domain;
				cookie_mode = config.Cookieless;
				cookies_supported = true; /* XXX ? */
#if NET_4_0
				if (!String.IsNullOrEmpty (default_url))
					default_url = MapUrl (default_url);
				else
#endif
					default_url = MapUrl(config.DefaultUrl);
				enable_crossapp_redirects = config.EnableCrossAppRedirects;
#if NET_4_0
				if (!String.IsNullOrEmpty (login_url))
					login_url = MapUrl (login_url);
				else
#endif
					login_url = MapUrl(config.LoginUrl);

				initialized = true;
			}
		}

		static string MapUrl (string url) {
			if (UrlUtils.IsRelativeUrl (url))
				return UrlUtils.Combine (HttpRuntime.AppDomainAppVirtualPath, url);
			else
				return UrlUtils.ResolveVirtualPathFromAppAbsolute (url);
		}

		public static void RedirectFromLoginPage (string userName, bool createPersistentCookie)
		{
			RedirectFromLoginPage (userName, createPersistentCookie, null);
		}

		public static void RedirectFromLoginPage (string userName, bool createPersistentCookie, string strCookiePath)
		{
			if (userName == null)
				return;

			Initialize ();
			SetAuthCookie (userName, createPersistentCookie, strCookiePath);
			Redirect (GetRedirectUrl (userName, createPersistentCookie), false);
		}

		public static FormsAuthenticationTicket RenewTicketIfOld (FormsAuthenticationTicket tOld)
		{
			if (tOld == null)
				return null;

			DateTime now = DateTime.Now;
			TimeSpan toIssue = now - tOld.IssueDate;
			TimeSpan toExpiration = tOld.Expiration - now;
			if (toExpiration > toIssue)
				return tOld;

			FormsAuthenticationTicket tNew = tOld.Clone ();
			tNew.SetDates (now, now + (tOld.Expiration - tOld.IssueDate));
			return tNew;
		}

		public static void SetAuthCookie (string userName, bool createPersistentCookie)
		{
			Initialize ();
			SetAuthCookie (userName, createPersistentCookie, cookiePath);
		}

		public static void SetAuthCookie (string userName, bool createPersistentCookie, string strCookiePath)
		{
			HttpContext context = HttpContext.Current;
			if (context == null)
				throw new HttpException ("Context is null!");

			HttpResponse response = context.Response;
			if (response == null)
				throw new HttpException ("Response is null!");

			response.Cookies.Add (GetAuthCookie (userName, createPersistentCookie, strCookiePath));
		}

		public static void SignOut ()
		{
			Initialize ();

			HttpContext context = HttpContext.Current;
			if (context == null)
				throw new HttpException ("Context is null!");

			HttpResponse response = context.Response;
			if (response == null)
				throw new HttpException ("Response is null!");

			HttpCookieCollection cc = response.Cookies;
			cc.Remove (cookieName);
			HttpCookie expiration_cookie = new HttpCookie (cookieName, String.Empty);
			expiration_cookie.Expires = new DateTime (1999, 10, 12);
			expiration_cookie.Path = cookiePath;
			if (!String.IsNullOrEmpty (cookie_domain))
				expiration_cookie.Domain = cookie_domain;
			cc.Add (expiration_cookie);
			Roles.DeleteCookie ();
		}

		public static string FormsCookieName
		{
			get {
				Initialize ();
				return cookieName;
			}
		}

		public static string FormsCookiePath
		{
			get {
				Initialize ();
				return cookiePath;
			}
		}

		public static bool RequireSSL {
			get {
				Initialize ();
				return requireSSL;
			}
		}

		public static bool SlidingExpiration {
			get {
				Initialize ();
				return slidingExpiration;
			}
		}

		public static string CookieDomain {
			get { Initialize (); return cookie_domain; }
		}

		public static HttpCookieMode CookieMode {
			get { Initialize (); return cookie_mode; }
		}

		public static bool CookiesSupported {
			get { Initialize (); return cookies_supported; }
		}

		public static string DefaultUrl {
			get { Initialize (); return default_url; }
		}

		public static bool EnableCrossAppRedirects {
			get { Initialize (); return enable_crossapp_redirects; }
		}

		public static string LoginUrl {
			get { Initialize (); return login_url; }
		}

		public static void RedirectToLoginPage ()
		{
			Redirect (LoginUrl);
		}

		[MonoTODO ("needs more tests")]
		public static void RedirectToLoginPage (string extraQueryString)
		{
			// TODO: if ? is in LoginUrl (legal?), ? in query (legal?) ...
			Redirect (LoginUrl + "?" + extraQueryString);
		}

		static void Redirect (string url)
		{
			HttpContext.Current.Response.Redirect (url);
		}

		static void Redirect (string url, bool end)
		{
			HttpContext.Current.Response.Redirect (url, end);
		}
	}
}
