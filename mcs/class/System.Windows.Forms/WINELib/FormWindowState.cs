//
// System.Windows.Forms.FormWindowState.cs
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {


	/// <summary>
  /// Specifies how a form window is displayed.
	/// </summary>
	//[Serializable]
	[ComVisible(true)]
	public enum FormWindowState {

		//Values were verified with enumcheck.
		Normal = 0,
		Maximized = 1,
		Minimized = 2,
	}
}
