//
// System.Security.Principal.GenericPrincipal.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Security.Principal {

	[Serializable]
	public class GenericPrincipal : IPrincipal {
		IIdentity identity;
		string [] roles;
		
		public GenericPrincipal (IIdentity identity, string [] roles)
		{
			this.identity = identity;
			this.roles = roles;
		}

		public virtual IIdentity Identity {
			get {
				return identity;
			}
		}

		public virtual bool IsInRole (string role)
		{
			foreach (string r in roles)
				if (role == r)
					return true;

			return false;
		}
	}
}
