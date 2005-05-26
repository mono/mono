//
// System.Web.SiteMapProviderCollection
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Lluis Sanchez Gual (lluis@novell.com)
//
//  (C) 2003 Ben Maurer
//  (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Web.UI;

namespace System.Web {
	public sealed class SiteMapProviderCollection : ProviderCollection
	{
		public SiteMapProviderCollection () {}
		
		public override void Add (ProviderBase provider)
		{
			if (provider == null)
				throw new ArgumentNullException ("provider");
			if (!(provider is SiteMapProvider))
				throw new InvalidOperationException(String.Format ("{0} must implement {1} to act as a site map provider", provider.GetType (), typeof (SiteMapProvider)));
			if (this [provider.Name] != null)
				throw new ArgumentException ("Duplicate site map providers");
			base.Add (provider);
		}
		
		public void Add (SiteMapProvider provider)
		{
			Add ((ProviderBase)provider);
		}
		
		public void AddArray (SiteMapProvider[] providerArray)
		{
			foreach (SiteMapProvider p in providerArray)
				Add (p);
		}
		
		public new SiteMapProvider this [string name] {
			get { return (SiteMapProvider) base [name]; }
		}
	}
}
#endif

