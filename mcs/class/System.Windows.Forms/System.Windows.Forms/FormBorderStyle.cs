//
// System.Windows.Forms.FormBorderStyle.cs
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {


	/// <summary>
  /// Specifies the border styles for a form.
	/// </summary>
	[ComVisible(true)]
	public enum FormBorderStyle {

		//Values were verified with enumcheck.
		None = 0,
		FixedSingle = 1,
		Fixed3D = 2,
		FixedDialog = 3,
		Sizable = 4,
		FixedToolWindow = 5,
		SizableToolWindow = 6
	}
}
