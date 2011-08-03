//
// System.Web.Configuration.SiteMapSection.cs
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
	public sealed class SiteMapSection: ConfigurationSection
	{
		static ConfigurationProperty defaultProviderProp;
		static ConfigurationProperty enabledProp;
		static ConfigurationProperty providersProp;
		static ConfigurationPropertyCollection properties;
		
		static SiteMapSection ()
		{
			defaultProviderProp = new ConfigurationProperty ("defaultProvider", typeof (string), "AspNetXmlSiteMapProvider");
			enabledProp = new ConfigurationProperty ("enabled", typeof (bool), true);
			providersProp = new ConfigurationProperty ("providers", typeof (ProviderSettingsCollection));
			properties = new ConfigurationPropertyCollection ();

			properties.Add (defaultProviderProp);
			properties.Add (enabledProp);
			properties.Add (providersProp);
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("defaultProvider", DefaultValue = "AspNetXmlSiteMapProvider")]
		public string DefaultProvider {
			get { return (string) base ["defaultProvider"]; }
			set { base ["defaultProvider"] = value; }
		}
		
		[ConfigurationProperty ("enabled", DefaultValue = "True")]
		public bool Enabled {
			get { return (bool) base ["enabled"]; }
			set { base ["enabled"] = value; }
		}

		[ConfigurationProperty ("providers")]
		public ProviderSettingsCollection Providers {
			get { return (ProviderSettingsCollection) base ["providers"]; }
		}

		SiteMapProviderCollection providers;
		internal SiteMapProviderCollection ProvidersInternal {
			get {
				if (providers == null) {
					SiteMapProviderCollection providersTmp = new SiteMapProviderCollection ();
					ProvidersHelper.InstantiateProviders (Providers, providersTmp, typeof (SiteMapProvider));
					providers = providersTmp;
				}
				return providers;
			}
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
	}
}

#endif
