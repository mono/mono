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

namespace System.Web.Configuration
{
	public class AnonymousIdentificationSection: InternalSection
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty enabledProp;
		static ConfigurationProperty cookieNameProp;
		static ConfigurationProperty cookieTimeoutProp;
		static ConfigurationProperty cookiePathProp;
		static ConfigurationProperty cookieRequireSSLProp;
		static ConfigurationProperty cookieSlidingExpirationProp;
		static ConfigurationProperty cookieProtectionProp;
		static ConfigurationProperty cookilessProp;
		static ConfigurationProperty domainProp;
		
		static AnonymousIdentificationSection ()
		{
			enabledProp = new ConfigurationProperty ("enabled", typeof(bool), false);
			cookieNameProp = new NonEmptyStringConfigurationProperty ("cookieName", ".ASPXANONYMOUS", ConfigurationPropertyFlags.None);
			cookieTimeoutProp = new TimeSpanConfigurationProperty ("cookieTimeout", new TimeSpan (69,10,40,0), TimeSpanSerializedFormat.Minutes, TimeSpanPropertyFlags.AllowInfinite | TimeSpanPropertyFlags.ProhibitZero, ConfigurationPropertyFlags.None);
			cookiePathProp = new NonEmptyStringConfigurationProperty ("cookiePath", "/", ConfigurationPropertyFlags.None);
			cookieRequireSSLProp = new ConfigurationProperty ("cookieRequireSSL", typeof(bool), false);
			cookieSlidingExpirationProp = new ConfigurationProperty ("cookieSlidingExpiration", typeof(bool), true);
			cookieProtectionProp = new ConfigurationProperty ("cookieProtection", typeof(CookieProtection), CookieProtection.Validation);
			cookilessProp = new ConfigurationProperty ("cookiless", typeof(HttpCookieMode), HttpCookieMode.UseDeviceProfile);
			domainProp = new ConfigurationProperty ("domain", typeof(string), null);
			
			properties.Add (enabledProp);
			properties.Add (cookieNameProp);
			properties.Add (cookieTimeoutProp);
			properties.Add (cookiePathProp);
			properties.Add (cookieRequireSSLProp);
			properties.Add (cookieSlidingExpirationProp);
			properties.Add (cookieProtectionProp);
			properties.Add (cookilessProp);
			properties.Add (domainProp);
		}
		
		public HttpCookieMode Cookiless {
			get { return (HttpCookieMode) base [cookilessProp]; }
			set { base [cookilessProp] = value; }
		}
		
		public string CookieName {
			get { return (string) base [cookieNameProp]; }
			set { base [cookieNameProp] = value; }
		}
		
		public string CookiePath {
			get { return (string) base [cookiePathProp]; }
			set { base [cookiePathProp] = value; }
		}
		
		public CookieProtection CookieProtection {
			get { return (CookieProtection) base [cookieProtectionProp]; }
			set { base [cookieProtectionProp] = value; }
		}
		
		public bool CookieRequireSSL {
			get { return (bool) base [cookieRequireSSLProp]; }
			set { base [cookieRequireSSLProp] = value; }
		}
		
		public bool CookieSlidingExpiration {
			get { return (bool) base [cookieSlidingExpirationProp]; }
			set { base [cookieSlidingExpirationProp] = value; }
		}
		
		public TimeSpan CookieTimeout {
			get { return (TimeSpan) base [cookieTimeoutProp]; }
			set { base [cookieTimeoutProp] = value; }
		}
		
		public string Domain {
			get { return (string) base [domainProp]; }
			set { base [domainProp] = value; }
		}
		
		public bool Enabled {
			get { return (bool) base [enabledProp]; }
			set { base [enabledProp] = value; }
		}

/*		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
*/
	}
}

#endif
