//
// System.Web.Configuration.HttpCookiesSection
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
using System.Configuration;


namespace System.Web.Configuration {

	public sealed class HttpCookiesSection : ConfigurationSection
	{
		static ConfigurationProperty domainProp;
		static ConfigurationProperty httpOnlyCookiesProp;
		static ConfigurationProperty requireSSLProp;
		static ConfigurationProperty sameSiteProp;
		static ConfigurationPropertyCollection properties;

		static HttpCookiesSection ()
		{
			domainProp = new ConfigurationProperty ("domain", typeof (string), "");
			httpOnlyCookiesProp = new ConfigurationProperty ("httpOnlyCookies", typeof (bool), false);
			requireSSLProp = new ConfigurationProperty ("requireSSL", typeof (bool), false);
			sameSiteProp = new ConfigurationProperty ("sameSite", typeof (SameSiteMode), (SameSiteMode)(-1), new SameSiteModeConverter(), null, ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (domainProp);
			properties.Add (httpOnlyCookiesProp);
			properties.Add (requireSSLProp);
			properties.Add (sameSiteProp);
		}

		[ConfigurationProperty ("domain", DefaultValue = "")]
		public string Domain {
			get { return (string) base [domainProp];}
			set { base[domainProp] = value; }
		}

		[ConfigurationProperty ("httpOnlyCookies", DefaultValue = "False")]
		public bool HttpOnlyCookies {
			get { return (bool) base [httpOnlyCookiesProp];}
			set { base[httpOnlyCookiesProp] = value; }
		}

		[ConfigurationProperty ("requireSSL", DefaultValue = "False")]
		public bool RequireSSL {
			get { return (bool) base [requireSSLProp];}
			set { base[requireSSLProp] = value; }
		}

		[ConfigurationProperty("sameSite", DefaultValue = (SameSiteMode)(-1))]
		public SameSiteMode SameSite
		{
			get { return (SameSiteMode)base[sameSiteProp]; }
			set { base[sameSiteProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

