//
// System.Windows.Forms.FormStartPosition.cs
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {


	/// <summary>
  /// Specifies the initial position of a form.
	/// </summary>
	[ComVisible(true)]
	public enum FormStartPosition {

		//Values were verified with enumcheck.
		Manual = 0,
		CenterScreen = 1,
		WindowsDefaultLocation = 2,
		WindowsDefaultBounds = 3,
		CenterParent = 4,		
	}
}
