//
// System.Windows.Forms.SecurityIDType.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {

	/// <summary>
	/// </summary>
	public enum SecurityIDType {
		Alias = 4,
		Computer = 9,
		DeletedAccount = 6,
		Domain = 3,
		Group = 2,
		Invalid = 7,
		Unknown = 8,
		User = 1,
		WellKnownGroup = 5
	}
}
