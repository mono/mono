//
// System.Windows.Forms.AccessibleSelection.cs
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {


	/// <summary>
	/// Specifies how an accessible object is selected or receives focus.
	/// This enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values.
	/// </summary>
	[Flags]
	public enum AccessibleSelection {

		AddSelection = 1,
		ExtendSelection = 2,
		None = 0,
		RemoveSelection = 4,
		TakeFocus = 8,
		TakeSelection = 16
	}
}
