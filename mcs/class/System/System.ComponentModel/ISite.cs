//
// System.ComponentModel.Component.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.ComponentModel {

	public interface ISite : IServiceProvider {
		IComponent Component { get; }

		IContainer Container { get; }

		bool DesignMode { get; }

		string Name { get; set; }
	}
}
