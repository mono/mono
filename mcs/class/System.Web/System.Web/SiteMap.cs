//
// System.Web.SiteMap
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
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
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Configuration.Provider;
using System.Configuration;
using System.Web.Configuration;

namespace System.Web {
	public static class SiteMap {
	
		static void Init ()
		{
			lock (locker) {
				if (provider == null) {
					SiteMapSection section = (SiteMapSection) WebConfigurationManager.GetSection ("system.web/siteMap");

					if (!section.Enabled)
						throw new InvalidOperationException ("This feature is currently disabled.  Please enable it in the system.web/siteMap section in the web.config file.");

					providers = section.ProvidersInternal;
					providers.SetReadOnly ();
					provider = providers[section.DefaultProvider];

					if (provider == null)
						throw new ConfigurationErrorsException (
							String.Format ("The default sitemap provider '{0}' does not exist in the provider collection.", section.DefaultProvider));
				}
			}
		}
		
		public static SiteMapNode CurrentNode { 
			get { return Provider.CurrentNode; }
		}

		public static SiteMapNode RootNode { 
			get { return Provider.RootNode; }
		}

		
		public static SiteMapProvider Provider {
			get {
				Init ();
				return provider;
			}
		}
		public static SiteMapProviderCollection Providers {
			get {
				Init ();
				return providers;
			}
		}
		
		public static event SiteMapResolveEventHandler SiteMapResolve {
			add { Provider.SiteMapResolve += value; }
			remove { Provider.SiteMapResolve -= value; }
		}

		public static bool Enabled {
			get {
				SiteMapSection section = (SiteMapSection) WebConfigurationManager.GetSection ("system.web/siteMap");
				return section.Enabled;
			}
		}		

#if TARGET_JVM
		const string SiteMap_provider = "SiteMap_provider";
		const string SiteMap_providers = "SiteMap_providers";
		static SiteMapProvider provider
		{
			get { return (SiteMapProvider) AppDomain.CurrentDomain.GetData (SiteMap_provider); }
			set { AppDomain.CurrentDomain.SetData (SiteMap_provider, value); }
		}
		static SiteMapProviderCollection providers
		{
			get { return (SiteMapProviderCollection) AppDomain.CurrentDomain.GetData (SiteMap_providers); }
			set { AppDomain.CurrentDomain.SetData (SiteMap_providers, value); }
		}
#else
		static SiteMapProvider provider;
		static SiteMapProviderCollection providers;
#endif
		static object locker = new object ();
	}
}
#endif

