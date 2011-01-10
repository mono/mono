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
		static ConfigurationProperty cacheRolesInCookieProp;
		static ConfigurationProperty cookieNameProp;
		static ConfigurationProperty cookiePathProp;
		static ConfigurationProperty cookieProtectionProp;
		static ConfigurationProperty cookieRequireSSLProp;
		static ConfigurationProperty cookieSlidingExpirationProp;
		static ConfigurationProperty cookieTimeoutProp;
		static ConfigurationProperty createPersistentCookieProp;
		static ConfigurationProperty defaultProviderProp;
		static ConfigurationProperty domainProp;
		static ConfigurationProperty enabledProp;
		static ConfigurationProperty maxCachedResultsProp;
		static ConfigurationProperty providersProp;
		
		static ConfigurationPropertyCollection properties;
		
		static RoleManagerSection ()
		{
			cacheRolesInCookieProp = new ConfigurationProperty ("cacheRolesInCookie", typeof (bool), false);
			cookieNameProp = new ConfigurationProperty ("cookieName", typeof (string), ".ASPXROLES");
			cookiePathProp = new ConfigurationProperty ("cookiePath", typeof (string), "/");
			cookieProtectionProp = new ConfigurationProperty ("cookieProtection", typeof (CookieProtection),
									  CookieProtection.All);
			cookieRequireSSLProp = new ConfigurationProperty ("cookieRequireSSL", typeof (bool), false);
			cookieSlidingExpirationProp = new ConfigurationProperty ("cookieSlidingExpiration", typeof (bool), true);
			cookieTimeoutProp = new ConfigurationProperty ("cookieTimeout", typeof (TimeSpan), TimeSpan.FromMinutes (30),
								       PropertyHelper.TimeSpanMinutesOrInfiniteConverter,
								       PropertyHelper.PositiveTimeSpanValidator,
								       ConfigurationPropertyOptions.None);
			createPersistentCookieProp = new ConfigurationProperty ("createPersistentCookie", typeof (bool), false);
			defaultProviderProp = new ConfigurationProperty ("defaultProvider", typeof (string), "AspNetSqlRoleProvider");
			domainProp = new ConfigurationProperty ("domain", typeof (string), "");
			enabledProp = new ConfigurationProperty ("enabled", typeof (bool), false);
			maxCachedResultsProp = new ConfigurationProperty ("maxCachedResults", typeof (int), 25);
			providersProp = new ConfigurationProperty ("providers", typeof (ProviderSettingsCollection));

			properties = new ConfigurationPropertyCollection ();
			properties.Add (cacheRolesInCookieProp);
			properties.Add (cookieNameProp);
			properties.Add (cookiePathProp);
			properties.Add (cookieProtectionProp);
			properties.Add (cookieRequireSSLProp);
			properties.Add (cookieSlidingExpirationProp);
			properties.Add (cookieTimeoutProp);
			properties.Add (createPersistentCookieProp);
			properties.Add (defaultProviderProp);
			properties.Add (domainProp);
			properties.Add (enabledProp);
			properties.Add (maxCachedResultsProp);
			properties.Add (providersProp);
		}
		
		[ConfigurationProperty ("cacheRolesInCookie", DefaultValue = false)]
		public bool CacheRolesInCookie {
			get { return (bool) base [cacheRolesInCookieProp]; }
			set { base [cacheRolesInCookieProp] = value; }
		}

		[TypeConverter (typeof (WhiteSpaceTrimStringConverter))]
		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("cookieName", DefaultValue = ".ASPXROLES")]
		public string CookieName {
			get { return (string) base [cookieNameProp]; }
			set { base [cookieNameProp] = value; }
		}

		[TypeConverter (typeof (WhiteSpaceTrimStringConverter))]
		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("cookiePath", DefaultValue = "/")]
		public string CookiePath {
			get { return (string) base [cookiePathProp]; }
			set { base [cookiePathProp] = value; }
		}

		[ConfigurationProperty ("cookieProtection", DefaultValue = "All")]
		public CookieProtection CookieProtection {
			get { return (CookieProtection) base [cookieProtectionProp]; }
			set { base [cookieProtectionProp] = value; }
		}

		[ConfigurationProperty ("cookieRequireSSL", DefaultValue = false)]
		public bool CookieRequireSSL {
			get { return (bool) base [cookieRequireSSLProp]; }
			set { base [cookieRequireSSLProp] = value; }
		}

		[ConfigurationProperty ("cookieSlidingExpiration", DefaultValue = true)]
		public bool CookieSlidingExpiration {
			get { return (bool) base [cookieSlidingExpirationProp]; }
			set { base [cookieSlidingExpirationProp] = value; }
		}

		[TimeSpanValidatorAttribute(MinValueString = "00:00:00", MaxValueString = "10675199.02:48:05.4775807")]
		[ConfigurationPropertyAttribute("cookieTimeout", DefaultValue = "00:30:00")]
		[TypeConverterAttribute(typeof(TimeSpanMinutesOrInfiniteConverter))]
		public TimeSpan CookieTimeout {
			get { return (TimeSpan) base [cookieTimeoutProp]; }
			set { base [cookieTimeoutProp] = value; }
		}

		[ConfigurationProperty ("createPersistentCookie", DefaultValue = false)]
		public bool CreatePersistentCookie {
			get { return (bool) base [createPersistentCookieProp]; }
			set { base [createPersistentCookieProp] = value; }
		}

		[TypeConverter (typeof (WhiteSpaceTrimStringConverter))]
		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("defaultProvider", DefaultValue = "AspNetSqlRoleProvider")]
		public string DefaultProvider {
			get { return (string) base [defaultProviderProp]; }
			set { base [defaultProviderProp] = value; }
		}

		[ConfigurationProperty ("domain")]
		public string Domain {
			get { return (string) base [domainProp]; }
			set { base [domainProp] = value; }
		}

		[ConfigurationProperty ("enabled", DefaultValue = false)]
		public bool Enabled {
			get { return (bool) base [enabledProp]; }
			set { base [enabledProp] = value; }
		}

		[ConfigurationProperty ("maxCachedResults", DefaultValue = 25)]
		public int MaxCachedResults {
			get { return (int) base [maxCachedResultsProp]; }
			set { base [maxCachedResultsProp] = value; }
		}

		[ConfigurationProperty ("providers")]
		public ProviderSettingsCollection Providers {
			get { return (ProviderSettingsCollection) base [providersProp]; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

#endif
