//
// System.Security.Principal.IPrincipal.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Security.Principal {

	public interface IPrincipal {

		IIdentity Identity {
			get;
		}

		bool IsInRole (string role);
	}
}
