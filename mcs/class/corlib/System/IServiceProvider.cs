//
// System.IServiceProvider.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System {

//	[ComVisible(false)]
	public interface IServiceProvider {

		object GetService (Type serviceType);

	}
}
