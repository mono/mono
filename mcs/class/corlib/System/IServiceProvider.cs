//
// System.IServiceProvider.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Runtime.InteropServices;

namespace System {

	[ComVisible(true)]
	public interface IServiceProvider {
		object GetService (Type serviceType);
	}
}
