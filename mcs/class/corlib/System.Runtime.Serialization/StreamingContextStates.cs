//
// System.Runtime.Serialization.StreamingContextStates.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Runtime.Serialization {

	public enum StreamingContextStates {
		All,
		Clone,
		CrossAppDomain,
		CrossMachine,
		CrossProcess,
		File,
		Other,
		Persistence,
		Remoting
	}
}
