//
// System.Windows.Forms.FormBorderStyle
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
  /// Specifies the border styles for a form.
	/// </summary>
	[Serializable]
	[ComVisible(true)]
	public enum FormBorderStyle
	{
		None = 0,
		FixedSingle = 1,
		Fixed3D = 2,
		FixedDialog = 3,
		Sizable = 4,
		FixedToolWindows = 5,
		SizableToolWindow = 6
	}
}
