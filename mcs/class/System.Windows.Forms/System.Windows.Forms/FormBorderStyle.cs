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
		Fixed3D = 1,
		FixedDialog = 2,
		FixedSingle = 3,
		FixedToolWindow = 4,
		None = 0,
		Sizable = 5,
		SizableToolWindow = 6
	}
}