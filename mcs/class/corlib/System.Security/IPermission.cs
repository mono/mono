//
// System.Security.IPermission.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

namespace System.Security {

	interface IPermission {

		IPermission Copy ();

		void Demand ();

		IPermission Intersect (IPermission target);

		bool IsSubsetOf (IPermission target);

		IPermission Union (IPermission target);
	}
}
