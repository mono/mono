//
// System.IServiceProvider.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System {

	public interface IServiceProvider {

		object GetService (Type serviceType);

	}
}
