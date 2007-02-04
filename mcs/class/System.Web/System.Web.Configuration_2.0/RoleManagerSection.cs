//
// System.Web.Configuration.RoleManagerSection
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;
using System.Configuration;
using System.Web.Security;

#if NET_2_0

namespace System.Web.Configuration {

	public sealed class RoleManagerSection : ConfigurationSection
	{
		[ConfigurationProperty ("cacheRolesInCookie", DefaultValue = false)]
		public bool CacheRolesInCookie {
			get { return (bool) base ["cacheRolesInCookie"]; }
			set { base ["cacheRolesInCookie"] = value; }
		}

		[TypeConverter (typeof (WhiteSpaceTrimStringConverter))]
		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("cookieName", DefaultValue = ".ASPXROLES")]
		public string CookieName {
			get { return (string) base ["cookieName"]; }
			set { base ["cookieName"] = value; }
		}

		[TypeConverter (typeof (WhiteSpaceTrimStringConverter))]
		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("cookiePath", DefaultValue = "/")]
		public string CookiePath {
			get { return (string) base ["cookiePath"]; }
			set { base ["cookiePath"] = value; }
		}

		[ConfigurationProperty ("cookieProtection", DefaultValue = "All")]
		public CookieProtection CookieProtection {
			get { return (CookieProtection) base ["cookieProtection"]; }
			set { base ["cookieProtection"] = value; }
		}

		[ConfigurationProperty ("cookieRequireSSL", DefaultValue = false)]
		public bool CookieRequireSSL {
			get { return (bool) base ["cookieRequireSSL"]; }
			set { base ["cookieRequireSSL"] = value; }
		}

		[ConfigurationProperty ("cookieSlidingExpiration", DefaultValue = true)]
		public bool CookieSlidingExpiration {
			get { return (bool) base ["cookieSlidingExpiration"]; }
			set { base ["cookieSlidingExpiration"] = value; }
		}

		[TypeConverter (typeof (TimeSpanMinutesOrInfiniteConverter))]
		[PositiveTimeSpanValidator]
		[ConfigurationProperty ("cookieTimeout", DefaultValue = "30")]
		public TimeSpan CookieTimeout {
			get { return (TimeSpan) base ["cookieTimeout"]; }
			set { base ["cookieTimeout"] = value; }
		}

		[ConfigurationProperty ("createPersistentCookie", DefaultValue = false)]
		public bool CreatePersistentCookie {
			get { return (bool) base ["createPersistentCookie"]; }
			set { base ["createPersistentCookie"] = value; }
		}

		[TypeConverter (typeof (WhiteSpaceTrimStringConverter))]
		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("defaultProvider", DefaultValue = "AspNetSqlRoleProvider")]
		public string DefaultProvider {
			get { return (string) base ["defaultProvider"]; }
			set { base ["defaultProvider"] = value; }
		}

		[ConfigurationProperty ("domain")]
		public string Domain {
			get { return (string) base ["domain"]; }
			set { base ["domain"] = value; }
		}

		[ConfigurationProperty ("enabled", DefaultValue = false)]
		public bool Enabled {
			get { return (bool) base ["enabled"]; }
			set { base ["enabled"] = value; }
		}

		[ConfigurationProperty ("maxCachedResults", DefaultValue = 25)]
		public int MaxCachedResults {
			get { return (int) base ["maxCachedResults"]; }
			set { base ["maxCachedResults"] = value; }
		}

		[ConfigurationProperty ("providers")]
		public ProviderSettingsCollection Providers {
			get { return (ProviderSettingsCollection) base ["providers"]; }
		}
	}
}

#endif
