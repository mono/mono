//
// System.Web.SiteMapProviderCollection
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
using System.Configuration.Provider;
using System.Web.UI;

namespace System.Web {
	public class SiteMapProviderCollection : ProviderCollection
	{
		public SiteMapProviderCollection () {}
		
		public override void Add (IProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");
			if ((provider as ISiteMapProvider) == null)
				throw new InvalidOperationException(String.Format ("{0} must implement {1} to act as a site map provider", provider.GetType (), typeof (ISiteMapProvider)));
			
			base.Add (provider);
		}
		
		public virtual void AddArray (IProvider[] providerArray)
		{			
			foreach (IProvider p in providerArray) {
				if (this [p.Name] != null)
					throw new ArgumentException ("Duplicate site map providers");
				Add (p);
			}
		}
		
		public ISiteMapProvider this [string name] { get { return (ISiteMapProvider) base [name]; } }
	}
}
#endif

