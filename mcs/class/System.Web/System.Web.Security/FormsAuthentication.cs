//
// System.Web.Security.FormsAuthentication
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
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

using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web.Security
{
	public sealed class FormsAuthentication
	{
		static string authConfigPath = "system.web/authentication";
		static bool initialized;
		static string cookieName;
		static string cookiePath;
		static int timeout;
		static FormsProtectionEnum protection;
#if NET_1_1
		static bool requireSSL;
		static bool slidingExpiration;
#endif

		// same names and order used in xsp
		static string [] indexFiles = { "index.aspx",
						"Default.aspx",
						"default.aspx",
						"index.html",
						"index.htm" };

		public static bool Authenticate (string name, string password)
		{
			if (name == null || password == null)
				return false;

			Initialize ();
			HttpContext context = HttpContext.Current;
			if (context == null)
				throw new HttpException ("Context is null!");

			AuthConfig config = context.GetConfig (authConfigPath) as AuthConfig;
			Hashtable users = config.CredentialUsers;
			string stored = users [name] as string;
			if (stored == null)
				return false;

			switch (config.PasswordFormat) {
			case FormsAuthPasswordFormat.Clear:
				/* Do nothing */
				break;
			case FormsAuthPasswordFormat.MD5:
				stored = HashPasswordForStoringInConfigFile (stored, "MD5");
				break;
			case FormsAuthPasswordFormat.SHA1:
				stored = HashPasswordForStoringInConfigFile (stored, "SHA1");
				break;
			}

			return (password == stored);
		}

		public static FormsAuthenticationTicket Decrypt (string encryptedTicket)
		{
			if (encryptedTicket == null || encryptedTicket == String.Empty)
				throw new ArgumentException ("Invalid encrypted ticket", "encryptedTicket");

			Initialize ();
			byte [] bytes = MachineKeyConfigHandler.GetBytes (encryptedTicket, encryptedTicket.Length);
			string decrypted = Encoding.ASCII.GetString (bytes);
			FormsAuthenticationTicket ticket = null;
			try {
				string [] values = decrypted.Split ((char) 1, (char) 2, (char) 3, (char) 4, (char) 5, (char) 6, (char) 7);
				if (values.Length != 8)
					throw new Exception (values.Length + " " + encryptedTicket);


				ticket = new FormsAuthenticationTicket (Int32.Parse (values [0]),
									values [1],
									new DateTime (Int64.Parse (values [2])),
									new DateTime (Int64.Parse (values [3])),
									(values [4] == "1"),
									values [5],
									values [6]);
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
			StringBuilder allTicket = new StringBuilder ();
			allTicket.Append (ticket.Version);
			allTicket.Append ('\u0001');
			allTicket.Append (ticket.Name);
			allTicket.Append ('\u0002');
			allTicket.Append (ticket.IssueDate.Ticks);
			allTicket.Append ('\u0003');
			allTicket.Append (ticket.Expiration.Ticks);
			allTicket.Append ('\u0004');
			allTicket.Append (ticket.IsPersistent ? '1' : '0');
			allTicket.Append ('\u0005');
			allTicket.Append (ticket.UserData);
			allTicket.Append ('\u0006');
			allTicket.Append (ticket.CookiePath);
			allTicket.Append ('\u0007');
			//if (protection == FormsProtectionEnum.None)
				return GetHexString (allTicket.ToString ());
			//TODO: encrypt and validate
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

			return new HttpCookie (cookieName, Encrypt (ticket), strCookiePath, then);
		}

		public static string GetRedirectUrl (string userName, bool createPersistentCookie)
		{
			if (userName == null)
				return null;

			//TODO: what's createPersistentCookie used for?
			Initialize ();
			HttpRequest request = HttpContext.Current.Request;
			string returnUrl = request ["RETURNURL"];
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

		static string GetHexString (string str)
		{
			return GetHexString (Encoding.ASCII.GetBytes (str));
		}

		static string GetHexString (byte [] bytes)
		{
			StringBuilder result = new StringBuilder (bytes.Length * 2);
			foreach (byte b in bytes)
				result.AppendFormat ("{0:x2}", (int) b);

			return result.ToString ();
		}

		public static string HashPasswordForStoringInConfigFile (string password, string passwordFormat)
		{
			if (password == null)
				throw new ArgumentNullException ("password");

			if (passwordFormat == null)
				throw new ArgumentNullException ("passwordFormat");

			byte [] bytes;
			if (String.Compare (passwordFormat, "MD5", true) == 0) {
				bytes = MD5.Create ().ComputeHash (Encoding.ASCII.GetBytes (password));
			} else if (String.Compare (passwordFormat, "SHA1", true) == 0) {
				bytes = SHA1.Create ().ComputeHash (Encoding.ASCII.GetBytes (password));
			} else {
				throw new ArgumentException ("The format must be either MD5 or SHA1", "passwordFormat");
			}

			return GetHexString (bytes);
		}

		public static void Initialize ()
		{
			if (initialized)
				return;

			lock (typeof (FormsAuthentication)) {
				if (initialized)
					return;

				HttpContext context = HttpContext.Current;
				if (context == null)
					throw new HttpException ("Context is null!");

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

				initialized = true;
			}
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
			HttpResponse resp = HttpContext.Current.Response;
			resp.Redirect (GetRedirectUrl (userName, createPersistentCookie), false);
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

			response.Cookies.MakeCookieExpire (cookieName, cookiePath);
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
	}
}

