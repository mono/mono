//
// System.Windows.Forms.DragAction.cs
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {


	/// <summary>
	/// Specifies how and if a drag-and-drop operation should continue.
	/// </summary>

	[ComVisible(true)]
	public enum DragAction {

		//Values were verified with enumcheck.
		Continue = 0,
		Drop = 1,
		Cancel = 2,
	}
}
