//
// System.Security.Principal.IIdentity.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Security.Principal {

	public interface IIdentity {

		string AuthenticationType {
			get;
		}

		bool IsAuthenticated {
			get;
		}

		string Name {
			get;
		}
	}
}
