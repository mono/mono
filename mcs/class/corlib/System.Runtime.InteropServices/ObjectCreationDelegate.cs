//
// System.Runtime.InteropServices.ObjectCreationDelegate.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Novell, Inc.  http://www.ximian.com
//
using System;

namespace System.Runtime.InteropServices {

	public delegate IntPtr ObjectCreationDelegate (IntPtr aggregator);
}
