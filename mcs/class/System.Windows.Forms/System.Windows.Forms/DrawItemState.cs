//
// System.Windows.Forms.DrawItemState.cs
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {


	/// <summary>
	/// Specifies the state of an item that is being drawn.
	/// this enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values.
	/// </summary>
	[Flags]
	public enum DrawItemState {

		None = 0,
		Selected = 1,
		Grayed = 2,
		Disabled = 4,
		Checked = 8,
		Focus = 16,
		Default = 32,
		HotLight = 64,
		Inactive = 128,
		NoAccelerator = 256,
		NoFocusRect = 512,
		ComboBoxEdit = 1024,
	}
}
