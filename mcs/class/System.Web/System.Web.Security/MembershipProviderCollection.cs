//
// System.Web.Security.MembershipProviderCollection
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Configuration.Provider;

namespace System.Web.Security {
	public class MembershipProviderCollection : ProviderCollection {
		public override void Add (IProvider provider)
		{
			if (provider is IMembershipProvider)
				base.Add (provider);
			else
				throw new HttpException ();
		}
		
		public IMembershipProvider this [string name] {
			get { return (IMembershipProvider) base [name]; }
		}
	}
}
#endif

