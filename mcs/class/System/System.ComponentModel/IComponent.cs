//
// System.ComponentModel.IComponent.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.ComponentModel {

	public interface IComponent : IDisposable {

		ISite Site {
			get; set;
		}

		event EventHandler Disposed;
	}
}
