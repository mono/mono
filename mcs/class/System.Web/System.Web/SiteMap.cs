//
// System.Web.SiteMap
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace System.Web {
	public sealed class SiteMap {
		[MonoTODO ("Get everything from the config")]
		private static void Init ()
		{
			throw new NotImplementedException ();
		}
		
		public static SiteMapNode CurrentNode { 
			get { return Provider.CurrentNode; }
		}
		public static SiteMapNode RootNode { 
			get { return Provider.RootNode; }
		}
		
		public static ISiteMapProvider Provider {
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
		
		static ISiteMapProvider provider;
		static SiteMapProviderCollection providers;
	}
}
#endif

