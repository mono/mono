//
// System.Web.Security.FormsAuthentication
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
// Copyright (c) 2005 Novell, Inc (http://www.novell.com)
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
using System.Web.Util;
using System.Globalization;

namespace System.Web.Security
{
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class FormsAuthentication
	{
		const int MD5_hash_size = 16;
		const int SHA1_hash_size = 20;

		static string authConfigPath = "system.web/authentication";
		static string machineKeyConfigPath = "system.web/machineKey";
#if TARGET_J2EE
		const string Forms_initialized = "Forms.initialized";
		const string Forms_cookieName = "Forms.cookieName";
		const string Forms_cookiePath = "Forms.cookiePath";
		const string Forms_timeout = "Forms.timeout";
		const string Forms_protection = "Forms.protection";
		const string Forms_init_vector = "Forms.init_vector";
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
		static byte [] init_vector
		{
			get { return (byte []) AppDomain.CurrentDomain.GetData (Forms_init_vector); }
			set { AppDomain.CurrentDomain.SetData (Forms_init_vector, value); }
		}
		static object locker = new object ();
#else
		static bool initialized;
		static string cookieName;
		static string cookiePath;
		static int timeout;
		static FormsProtectionEnum protection;
		static object locker = new object ();
		static byte [] init_vector; // initialization vector used for 3DES
#endif
#if NET_1_1
#if TARGET_J2EE
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
#else
		static bool requireSSL;
		static bool slidingExpiration;
#endif
#endif
#if NET_2_0
#if TARGET_J2EE
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
		static string cookie_domain;
		static HttpCookieMode cookie_mode;
		static bool cookies_supported;
		static string default_url;
		static bool enable_crossapp_redirects;
		static string login_url;
#endif
#endif
		// same names and order used in xsp
		static string [] indexFiles = { "index.aspx",
						"Default.aspx",
						"default.aspx",
						"index.html",
						"index.htm" };

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
#if NET_2_0
			AuthenticationSection section = (AuthenticationSection) WebConfigurationManager.GetSection (authConfigPath);
			FormsAuthenticationCredentials config = section.Forms.Credentials;
			FormsAuthenticationUser user = config.Users[name];
			string stored = null;

			if (user != null)
				stored = user.Password;
#else
			AuthConfig config = context.GetConfig (authConfigPath) as AuthConfig;
			Hashtable users = config.CredentialUsers;
			string stored = users [name] as string;
#endif
			if (stored == null)
				return false;

			switch (config.PasswordFormat) {
			case FormsAuthPasswordFormat.Clear:
				/* Do nothing */
				break;
			case FormsAuthPasswordFormat.MD5:
				password = HashPasswordForStoringInConfigFile (password, "MD5");
				break;
			case FormsAuthPasswordFormat.SHA1:
				password = HashPasswordForStoringInConfigFile (password, "SHA1");
				break;
			}

			return (password == stored);
		}

#if NET_2_0
		static byte [] GetDecryptionKey (MachineKeySection config)
		{
			return MachineKeySectionUtils.DecryptionKey192Bits (config);
		}
#else
		static byte [] GetDecryptionKey (MachineKeyConfig config)
		{
			return config.DecryptionKey192Bits;
		}
#endif
		
		static FormsAuthenticationTicket Decrypt2 (byte [] bytes)
		{
			if (protection == FormsProtectionEnum.None)
				return FormsAuthenticationTicket.FromByteArray (bytes);

#if NET_2_0
			MachineKeySection config = (MachineKeySection) WebConfigurationManager.GetWebApplicationSection (machineKeyConfigPath);
#else
			MachineKeyConfig config = HttpContext.GetAppConfig (machineKeyConfigPath) as MachineKeyConfig;
#endif
			bool all = (protection == FormsProtectionEnum.All);

			byte [] result = bytes;
			if (all || protection == FormsProtectionEnum.Encryption) {
				ICryptoTransform decryptor;
				decryptor = TripleDES.Create ().CreateDecryptor (GetDecryptionKey (config), init_vector);
				result = decryptor.TransformFinalBlock (bytes, 0, bytes.Length);
				bytes = null;
			}

			if (all || protection == FormsProtectionEnum.Validation) {
				int count;
				MachineKeyValidation validationType;

#if NET_2_0
				validationType = config.Validation;
#else
				validationType = config.ValidationType;
#endif
				if (validationType == MachineKeyValidation.MD5)
					count = MD5_hash_size;
				else
					count = SHA1_hash_size; // 3DES and SHA1

#if NET_2_0
				byte [] vk = MachineKeySectionUtils.ValidationKeyBytes (config);
#else
				byte [] vk = config.ValidationKey;
#endif
				byte [] mix = new byte [result.Length - count + vk.Length];
				Buffer.BlockCopy (result, 0, mix, 0, result.Length - count);
				Buffer.BlockCopy (vk, 0, mix, result.Length - count, vk.Length);

				byte [] hash = null;
				switch (validationType) {
				case MachineKeyValidation.MD5:
					hash = MD5.Create ().ComputeHash (mix);
					break;
				// From MS docs: "When 3DES is specified, forms authentication defaults to SHA1"
				case MachineKeyValidation.TripleDES:
				case MachineKeyValidation.SHA1:
					hash = SHA1.Create ().ComputeHash (mix);
					break;
				}

				if (result.Length < count)
					throw new ArgumentException ("Error validating ticket (length).", "encryptedTicket");

				int i, k;
				for (i = result.Length - count, k = 0; k < count; i++, k++) {
					if (result [i] != hash [k])
						throw new ArgumentException ("Error validating ticket.", "encryptedTicket");
				}
			}

			return FormsAuthenticationTicket.FromByteArray (result);
		}

		public static FormsAuthenticationTicket Decrypt (string encryptedTicket)
		{
			if (encryptedTicket == null || encryptedTicket == String.Empty)
				throw new ArgumentException ("Invalid encrypted ticket", "encryptedTicket");

			Initialize ();

			FormsAuthenticationTicket ticket;
#if NET_2_0
			byte [] bytes = MachineKeySectionUtils.GetBytes (encryptedTicket, encryptedTicket.Length);
#else
			byte [] bytes = MachineKeyConfig.GetBytes (encryptedTicket, encryptedTicket.Length);
#endif
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
				return GetHexString (ticket_bytes);

			byte [] result = ticket_bytes;
#if NET_2_0
			MachineKeySection config = (MachineKeySection) WebConfigurationManager.GetWebApplicationSection (machineKeyConfigPath);
#else
			MachineKeyConfig config = HttpContext.GetAppConfig (machineKeyConfigPath) as MachineKeyConfig;
#endif
			bool all = (protection == FormsProtectionEnum.All);
			if (all || protection == FormsProtectionEnum.Validation) {
				byte [] valid_bytes = null;
#if NET_2_0
				byte [] vk = MachineKeySectionUtils.ValidationKeyBytes (config);
#else
				byte [] vk = config.ValidationKey;
#endif
				byte [] mix = new byte [ticket_bytes.Length + vk.Length];
				Buffer.BlockCopy (ticket_bytes, 0, mix, 0, ticket_bytes.Length);
				Buffer.BlockCopy (vk, 0, mix, result.Length, vk.Length);

				switch (
#if NET_2_0
					config.Validation
#else
					config.ValidationType
#endif
					) {
				case MachineKeyValidation.MD5:
					valid_bytes = MD5.Create ().ComputeHash (mix);
					break;
				// From MS docs: "When 3DES is specified, forms authentication defaults to SHA1"
				case MachineKeyValidation.TripleDES:
				case MachineKeyValidation.SHA1:
					valid_bytes = SHA1.Create ().ComputeHash (mix);
					break;
				}

				int tlen = ticket_bytes.Length;
				int vlen = valid_bytes.Length;
				result = new byte [tlen + vlen];
				Buffer.BlockCopy (ticket_bytes, 0, result, 0, tlen);
				Buffer.BlockCopy (valid_bytes, 0, result, tlen, vlen);
			}

			if (all || protection == FormsProtectionEnum.Encryption) {
				ICryptoTransform encryptor;
				encryptor = TripleDES.Create ().CreateEncryptor (GetDecryptionKey (config), init_vector);
				result = encryptor.TransformFinalBlock (result, 0, result.Length);
			}

			return GetHexString (result);
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
#if NET_2_0
			if (!String.IsNullOrEmpty (cookie_domain))
				cookie.Domain = cookie_domain;
#endif
			return cookie;
		}

		internal static string ReturnUrl
		{
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

		static string GetHexString (byte [] bytes)
		{
			const int letterPart = 55;
			const int numberPart = 48;
			char [] result = new char [bytes.Length * 2];
			for (int i = 0; i < bytes.Length; i++) {
				int tmp = (int) bytes [i];
				int second = tmp & 15;
				int first = (tmp >> 4) & 15;
				result [(i * 2)] = (char) (first > 9 ? letterPart + first : numberPart + first);
				result [(i * 2) + 1] = (char) (second > 9 ? letterPart + second : numberPart + second);
			}
			return new string (result);
		}

		public static string HashPasswordForStoringInConfigFile (string password, string passwordFormat)
		{
			if (password == null)
				throw new ArgumentNullException ("password");

			if (passwordFormat == null)
				throw new ArgumentNullException ("passwordFormat");

			byte [] bytes;
			if (String.Compare (passwordFormat, "MD5", true, Helpers.InvariantCulture) == 0) {
				bytes = MD5.Create ().ComputeHash (Encoding.UTF8.GetBytes (password));
			} else if (String.Compare (passwordFormat, "SHA1", true, Helpers.InvariantCulture) == 0) {
				bytes = SHA1.Create ().ComputeHash (Encoding.UTF8.GetBytes (password));
			} else {
				throw new ArgumentException ("The format must be either MD5 or SHA1", "passwordFormat");
			}

			return GetHexString (bytes);
		}

		public static void Initialize ()
		{
			if (initialized)
				return;

			lock (locker) {
				if (initialized)
					return;

#if NET_2_0
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
				default_url = MapUrl(config.DefaultUrl);
				enable_crossapp_redirects = config.EnableCrossAppRedirects;
				login_url = MapUrl(config.LoginUrl);
#else
				HttpContext context = HttpContext.Current;
				AuthConfig authConfig = context.GetConfig (authConfigPath) as AuthConfig;
				if (authConfig != null) {
					cookieName = authConfig.CookieName;
					timeout = authConfig.Timeout;
					cookiePath = authConfig.CookiePath;
					protection = authConfig.Protection;
#if NET_1_1
					requireSSL = authConfig.RequireSSL;
					slidingExpiration = authConfig.SlidingExpiration;
#endif
				} else {
					cookieName = ".MONOAUTH";
					timeout = 30;
					cookiePath = "/";
					protection = FormsProtectionEnum.All;
#if NET_1_1
					slidingExpiration = true;
#endif
				}
#endif

				// IV is 8 bytes long for 3DES
				init_vector = new byte [8];
				int len = cookieName.Length;
				for (int i = 0; i < 8; i++) {
					if (i >= len)
						break;

					init_vector [i] = (byte) cookieName [i];
				}

				initialized = true;
			}
		}

#if NET_2_0
		static string MapUrl (string url) {
			if (UrlUtils.IsRelativeUrl (url))
				return UrlUtils.Combine (HttpRuntime.AppDomainAppVirtualPath, url);
			else
				return UrlUtils.ResolveVirtualPathFromAppAbsolute (url);
		}
#endif

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
#if NET_2_0
			if (!String.IsNullOrEmpty (cookie_domain))
				expiration_cookie.Domain = cookie_domain;
#endif
			cc.Add (expiration_cookie);

#if NET_2_0
			Roles.DeleteCookie ();
#endif
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
#if NET_1_1
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
#endif

#if NET_2_0
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
#endif

		static void Redirect (string url, bool end)
		{
			HttpContext.Current.Response.Redirect (url, end);
		}
	}
}
