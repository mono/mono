//
// System.ComponentModel.ISite.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Runtime.InteropServices;

namespace System.ComponentModel 
{

	[ComVisible (true)]
	public interface ISite : IServiceProvider
	{
		IComponent Component { get; }

		IContainer Container { get; }

		bool DesignMode { get; }

		string Name { get; set; }
	}
}
