//
// System.Diagnostics.EventLogPermissionAccess.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

using System;
using System.Diagnostics;

namespace System.Diagnostics {

	[Flags, Serializable]
	public enum EventLogPermissionAccess {
		None=0,
		Browse=0x2,
		Instrument=0x6,
		Audit=0xA,
	}
}

