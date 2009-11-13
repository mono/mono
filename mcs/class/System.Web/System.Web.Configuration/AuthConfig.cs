//
// System.Web.Configuration.AuthConfig
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
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
using System.Configuration;
using System.Xml;

namespace System.Web.Configuration
{
	class AuthConfig
	{
		AuthenticationMode mode;
		string cookieName;
		string cookiePath;
		string loginUrl;
		FormsProtectionEnum protection;
		int timeout;
		FormsAuthPasswordFormat pwdFormat;
		Hashtable credentialUsers;
		bool has_parent;
#if NET_1_1
		bool requireSSL;
		bool slidingExpiration = true;
#endif
#if NET_2_0
		string cookie_domain;
		HttpCookieMode cookie_mode;
		bool cookies_supported;
		string default_url;
		bool enable_crossapp_redirects;
#endif

		internal AuthConfig (object parent)
		{
			if (parent is AuthConfig) {
				has_parent = true;
				AuthConfig p = (AuthConfig) parent;
				mode = p.mode;
				cookieName = p.cookieName;
				cookiePath = p.cookiePath;
				loginUrl = p.loginUrl;
				protection = p.protection;
				timeout = p.timeout;
				pwdFormat = p.pwdFormat;
#if NET_1_1
				requireSSL = p.requireSSL;
				slidingExpiration = p.slidingExpiration;
#endif
#if NET_2_0
				cookie_domain = p.cookie_domain;
				cookie_mode = p.cookie_mode;
				cookies_supported = p.cookies_supported;
				default_url = p.default_url;
				enable_crossapp_redirects = p.enable_crossapp_redirects;
#endif
				credentialUsers = new Hashtable (p.CredentialUsers);
			}
		}

		internal void SetMode (string m)
		{
			if (m == null) {
				// we default to Forms authentication mode, MS defaults to Windows
				if (!has_parent)
					Mode = AuthenticationMode.Forms;
				return;
			}

			Mode = (AuthenticationMode) Enum.Parse (typeof (AuthenticationMode), m, true);
		}

		internal void SetProtection (string prot)
		{
			if (prot == null) {
				if (!has_parent)
					Protection = FormsProtectionEnum.All;
				return;
			}

			Protection = (FormsProtectionEnum) Enum.Parse (typeof (FormsProtectionEnum),
								       prot,
								       true);
		}

		internal void SetTimeout (string minutes)
		{
			if (minutes != null) {
				Timeout = Int32.Parse (minutes);
				return;
			}

			if (!has_parent)
				Timeout = 30;
		}

		internal void SetPasswordFormat (string pwdFormat)
		{
			if (pwdFormat == null) {
				if (!has_parent)
					PasswordFormat = FormsAuthPasswordFormat.Clear;
				return;
			}

			PasswordFormat =
				(FormsAuthPasswordFormat) Enum.Parse (typeof (FormsAuthPasswordFormat),
								      pwdFormat,
								      true);
		}

		internal AuthenticationMode Mode {
			get { return mode; }
			set { mode = value; }
		}

		internal string CookieName {
			get {
				if (cookieName == null)
					cookieName = ".ASPXAUTH";

				return cookieName;
			}
			set {
				if (value == null)
					return;

				cookieName = value;
			}
		}

		internal string CookiePath {
			get {
				if (cookiePath == null)
					cookiePath = "/";

				return cookiePath;
			}
			set {
				if (value == null)
					return;

				cookiePath = value;
			}
		}

		internal string LoginUrl {
			get {
				if (loginUrl == null)
					loginUrl = "login.aspx";

				return loginUrl;
			}
			set {
				if (value == null)
					return;

				loginUrl = value;
			}
		}

		internal FormsProtectionEnum Protection {
			get { return protection; }
			set { protection = value; }
		}

		internal int Timeout {
			get { return timeout; }
			set {
				if (value <= 0)
					throw new ArgumentException ("Timeout must be > 0", "value");

				timeout = value;
			}
		}

		internal FormsAuthPasswordFormat PasswordFormat {
			get { return pwdFormat; }
			set { pwdFormat = value; }
		}

		internal Hashtable CredentialUsers {
			get {
				if (credentialUsers == null)
					credentialUsers = new Hashtable ();

				return credentialUsers;
			}
		}

#if NET_1_1
		internal bool RequireSSL {
			get { return requireSSL; }
			set { requireSSL = value; }
		}

		internal bool SlidingExpiration {
			get { return slidingExpiration; }
			set { slidingExpiration = value; }
		}
#endif

#if NET_2_0
		internal string CookieDomain {
			get { return cookie_domain; }
			set { cookie_domain = value; }
		}

		internal HttpCookieMode CookieMode {
			get { return cookie_mode; }
			set { cookie_mode = value; }
		}

		internal bool CookiesSupported {
			get { return cookies_supported; }
			set { cookies_supported = value; }
		}

		internal string DefaultUrl {
			get { return default_url; }
			set { default_url = value; }
		}

		internal bool EnableCrossAppRedirects {
			get { return enable_crossapp_redirects; }
			set { enable_crossapp_redirects = value; }
		}
#endif
	}
}

