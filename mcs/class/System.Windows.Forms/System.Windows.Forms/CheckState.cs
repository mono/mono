//
// System.Windows.Forms.CheckState
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
  /// Specifies the state of a control, such as a check box, 
  /// that can be checked, unchecked, or set to an indeterminate state.
	/// </summary>
	[Serializable]
	public enum CheckState
	{
		Unchecked = 0,
		Checked = 1,
		Indeterminate = 2,
	}
}
