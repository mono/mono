//
// System.Windows.Forms.CheckState.cs
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {


	/// <summary>
  /// Specifies the state of a control, such as a check box, 
  /// that can be checked, unchecked, or set to an indeterminate state.
	/// </summary>
	//[Serializable]
	public enum CheckState {

		//Values were verified with enumcheck.
		Unchecked = 0,
		Checked = 1,
		Indeterminate = 2,
	}
}
