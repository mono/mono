//
// System.Runtime.Remoting.Contexts.ActivatorLevel..cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Runtime.Remoting.Activation {

	public enum ActivatorLevel  {
		Construction = 4,
		Context = 8,
		AppDomain = 12,
		Process = 16,
		Machine = 20,
	}
}
