//
// System.Web.Configuration.AuthConfig
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
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
				// MS docs says it is default.aspx.
				// If null others will search for Default.aspx, default.aspx, index.aspx
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
	}
}

