//
// System.Windows.Forms.DialogResult.cs
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {

	/// <summary>
	/// Specifies identifiers to indicate the return value of a dialog box.
	/// </summary>
	//LAMESPEC: Docs say serializable, verifer says no.
	[ComVisible(true)]
	public enum DialogResult {

		//Values were verified with enumcheck.
		None = 0,
		OK = 1,
		Cancel = 2,
		Abort = 3,
		Retry = 4,
		Ignore = 5,
		Yes = 6,
		No = 7,
	}
}
