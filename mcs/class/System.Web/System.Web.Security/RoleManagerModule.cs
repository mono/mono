//
// System.Web.Security.RoleManagerModule
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

namespace System.Web.Security {
	public sealed class RoleManagerModule : IHttpModule {
		public event RoleManagerEventHandler GetRoles;

		[MonoTODO]
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Init (HttpApplication app)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif

