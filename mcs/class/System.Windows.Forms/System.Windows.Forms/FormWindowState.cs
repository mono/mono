//
// System.Windows.Forms.FormWindowState
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
  /// Specifies how a form window is displayed.
	/// </summary>
	[Serializable]
//	[ComVisible(true)]
	public enum FormWindowState
	{
		Normal = 0,
		Maximized = 1,
		Minimized = 2,
	}
}
