//
// System.Diagnostics.EntryWrittenEventHandler.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.Diagnostics {

	public delegate void EntryWrittenEventHandler(
		object sender, 
		EntryWrittenEventArgs e);

}

