//
// System.Web.Security.RoleProviderCollection
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Configuration.Provider;

namespace System.Web.Security {
	public class RoleProviderCollection : ProviderCollection {
		public override void Add (IProvider provider)
		{
			if (provider is IRoleProvider)
				base.Add (provider);
			else
				throw new HttpException ();
		}
		
		public IRoleProvider this [string name] {
			get { return (IRoleProvider) base [name]; }
		}
	}
}
#endif

