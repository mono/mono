//
// System.Diagnostics.PerformanceCounterPermissionAccess.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002
// (C) 2003 Andreas Nahr
//

using System;
using System.Diagnostics;

namespace System.Diagnostics {

	[Flags ()]
	public enum PerformanceCounterPermissionAccess {
		Administer=0x0E,
		Browse=0x02,
		Instrument=0x06,
		None=0x00
	}
}

