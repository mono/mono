//
// System.Windows.Forms.DialogResult
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms
{

	/// <summary>
  /// Specifies identifiers to indicate the return value of a dialog box.
	/// </summary>
	[Serializable]
	[ComVisible(true)]
	public enum DialogResult
	{
		Abort = 1,
		Cancel = 2,
		Ignore = 3,
		No = 4,
		None = 0,
		OK = 5,
		Retry = 6,
		Yes = 7
	}
}