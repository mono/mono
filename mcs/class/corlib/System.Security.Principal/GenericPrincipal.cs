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
			if (identity == null)
				throw new ArgumentNullException ("identity");

			this.identity = identity;
			if (roles != null) {
				// make our own (unchangeable) copy of the roles
				this.roles = new string [roles.Length];
				for (int i=0; i < roles.Length; i++)
					this.roles [i] = roles [i];
			}
		}

		public virtual IIdentity Identity {
			get { return identity; }
		}

		public virtual bool IsInRole (string role)
		{
			if (roles == null)
				return false;

			foreach (string r in roles)
				if (role == r)
					return true;

			return false;
		}
	}
}
