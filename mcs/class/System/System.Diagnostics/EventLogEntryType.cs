//
// System.Diagnostics.EventLogEntryType.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

using System;
using System.Diagnostics;

namespace System.Diagnostics {

	[Serializable]
	public enum EventLogEntryType {
		Error = 0x01,
		Warning = 0x02,
		Information = 0x04,
		SuccessAudit = 0x08,
		FailureAudit = 0x10
	}
}

