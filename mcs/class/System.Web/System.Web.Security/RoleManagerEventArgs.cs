//
// System.Web.Security.RoleManagerEventArgs
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
	public sealed class RoleManagerEventArgs : EventArgs {
		public RoleManagerEventArgs (HttpContext context)
		{
			this.context = context;
		}
		
		HttpContext context;
		public HttpContext Context {
			get { return context; }
		}
		
		bool rolesPopulated;
		public bool RolesPopulated {
			get { return rolesPopulated; }
			set { rolesPopulated = value; }
		}
	}
}
#endif

