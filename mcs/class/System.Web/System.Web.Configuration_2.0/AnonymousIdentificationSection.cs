//
// System.Web.Configuration.AnonymousIdentificationSection.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Configuration;
using System.Web.Security;
using System.ComponentModel;

namespace System.Web.Configuration
{
	public sealed class AnonymousIdentificationSection: ConfigurationSection
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty enabledProp;
		static ConfigurationProperty cookielessProp;
		static ConfigurationProperty cookieNameProp;
		static ConfigurationProperty cookieTimeoutProp;
		static ConfigurationProperty cookiePathProp;
		static ConfigurationProperty cookieRequireSSLProp;
		static ConfigurationProperty cookieSlidingExpirationProp;
		static ConfigurationProperty cookieProtectionProp;
		static ConfigurationProperty domainProp;

		static AnonymousIdentificationSection ()
		{
			enabledProp = new ConfigurationProperty ("enabled", typeof(bool), false);
			cookielessProp = new ConfigurationProperty ("cookieless", typeof (HttpCookieMode), HttpCookieMode.UseCookies,
								    new GenericEnumConverter (typeof (HttpCookieMode)),
								    PropertyHelper.DefaultValidator,
								    ConfigurationPropertyOptions.None);
			cookieNameProp = new ConfigurationProperty ("cookieName", typeof (string), ".ASPXANONYMOUS", TypeDescriptor.GetConverter (typeof (string)),
								    PropertyHelper.NonEmptyStringValidator, ConfigurationPropertyOptions.None);
			cookieTimeoutProp = new ConfigurationProperty ("cookieTimeout", typeof (TimeSpan), new TimeSpan (69,10,40,0), new TimeSpanMinutesOrInfiniteConverter(),
								       PropertyHelper.PositiveTimeSpanValidator,
								       ConfigurationPropertyOptions.None);
			cookiePathProp = new ConfigurationProperty ("cookiePath", typeof (string), "/", TypeDescriptor.GetConverter (typeof (string)),
								    PropertyHelper.NonEmptyStringValidator, ConfigurationPropertyOptions.None);
			cookieRequireSSLProp = new ConfigurationProperty ("cookieRequireSSL", typeof(bool), false);
			cookieSlidingExpirationProp = new ConfigurationProperty ("cookieSlidingExpiration", typeof(bool), true);
			cookieProtectionProp = new ConfigurationProperty ("cookieProtection", typeof(CookieProtection), CookieProtection.Validation,
									  new GenericEnumConverter (typeof (CookieProtection)),
									  null, ConfigurationPropertyOptions.None);

			domainProp = new ConfigurationProperty ("domain", typeof(string), null);
			
			properties = new ConfigurationPropertyCollection ();
			properties.Add (enabledProp);
			properties.Add (cookielessProp);
			properties.Add (cookieNameProp);
			properties.Add (cookieTimeoutProp);
			properties.Add (cookiePathProp);
			properties.Add (cookieRequireSSLProp);
			properties.Add (cookieSlidingExpirationProp);
			properties.Add (cookieProtectionProp);
			properties.Add (domainProp);
		}
		
		[ConfigurationProperty ("cookieless", DefaultValue = "UseCookies")]
		public HttpCookieMode Cookieless {
			get { return (HttpCookieMode) base [cookielessProp]; }
			set { base [cookielessProp] = value; }
		}
		
		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("cookieName", DefaultValue = ".ASPXANONYMOUS")]
		public string CookieName {
			get { return (string) base [cookieNameProp]; }
			set { base [cookieNameProp] = value; }
		}
		
		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("cookiePath", DefaultValue = "/")]
		public string CookiePath {
			get { return (string) base [cookiePathProp]; }
			set { base [cookiePathProp] = value; }
		}
		
		[ConfigurationProperty ("cookieProtection", DefaultValue = "Validation")]
		public CookieProtection CookieProtection {
			get { return (CookieProtection) base [cookieProtectionProp]; }
			set { base [cookieProtectionProp] = value; }
		}
		
		[ConfigurationProperty ("cookieRequireSSL", DefaultValue = "False")]
		public bool CookieRequireSSL {
			get { return (bool) base [cookieRequireSSLProp]; }
			set { base [cookieRequireSSLProp] = value; }
		}
		
		[ConfigurationProperty ("cookieSlidingExpiration", DefaultValue = "True")]
		public bool CookieSlidingExpiration {
			get { return (bool) base [cookieSlidingExpirationProp]; }
			set { base [cookieSlidingExpirationProp] = value; }
		}
		
		[TimeSpanValidator (MinValueString = "00:00:00", MaxValueString = "10675199.02:48:05.4775807")]
		[TypeConverter (typeof(TimeSpanMinutesOrInfiniteConverter))]
		[ConfigurationProperty ("cookieTimeout", DefaultValue = "69.10:40:00")]
		public TimeSpan CookieTimeout {
			get { return (TimeSpan) base [cookieTimeoutProp]; }
			set { base [cookieTimeoutProp] = value; }
		}
		
		[ConfigurationProperty ("domain")]
		public string Domain {
			get { return (string) base [domainProp]; }
			set { base [domainProp] = value; }
		}
		
		[ConfigurationProperty ("enabled", DefaultValue = "False")]
		public bool Enabled {
			get { return (bool) base [enabledProp]; }
			set { base [enabledProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}		
	}
}

#endif
