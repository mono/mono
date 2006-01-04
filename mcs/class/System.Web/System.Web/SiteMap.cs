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
	public abstract class SiteMap {
	
		internal SiteMap ()
		{
		}
		
		private static void Init ()
		{
			lock (locker) {
				if (provider == null) {
					SiteMapSection section = (SiteMapSection) WebConfigurationManager.GetWebApplicationSection ("system.web/siteMap");
					providers = new SiteMapProviderCollection ();
					
					if (section.Enabled) {
						foreach (ProviderSettings prov in section.Providers) {
							Type t = Type.GetType (prov.Type);
							if (t == null)
								throw new ConfigurationException ("Cannot find type: " + prov.Type);
							if (!typeof(SiteMapProvider).IsAssignableFrom (t))
								throw new ConfigurationException ("The provided type is not a SiteMapProvider subclass: " + prov.Type);
							
							SiteMapProvider pr = (SiteMapProvider) Activator.CreateInstance (t);
							pr.Initialize (prov.Name, prov.Parameters);
							
							if (provider == null || prov.Name == section.DefaultProvider)
								provider = pr;

							providers.Add (pr);
						}
					}
					
					if (providers.Count == 0) {
						provider = new XmlSiteMapProvider ();
						NameValueCollection attributes = new NameValueCollection ();
						attributes.Add ("siteMapFile", "Web.sitemap");
						provider.Initialize ("AspNetXmlSiteMapProvider", attributes);
						providers.Add (provider);
					}
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
		
		public event SiteMapResolveEventHandler SiteMapResolve {
			add { Provider.SiteMapResolve += value; }
			remove { Provider.SiteMapResolve -= value; }
		}
		
		static SiteMapProvider provider;
		static SiteMapProviderCollection providers;
		static object locker = new object ();
	}
}
#endif

