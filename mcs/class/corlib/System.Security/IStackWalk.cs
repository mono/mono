//
// System.Security.IStackWalk.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

namespace System.Security {

	interface IStackWalk {

		void Assert ();

		void Demand ();

		void Deny ();

		void PermitOnly ();
	}
}
