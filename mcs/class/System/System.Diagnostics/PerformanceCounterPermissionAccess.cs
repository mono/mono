//
// System.Diagnostics.PerformanceCounterPermissionAccess.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

using System;
using System.Diagnostics;

namespace System.Diagnostics {

	public enum PerformanceCounterPermissionAccess {
		Administrator=0x0E,
		Browse=0x02,
		Instrument=0x06,
		None=0x00
	}
}

