//
// System.Windows.Forms.DragDropEffects.cs
//
// Author:
//   Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {


	/// <summary>
	/// Specifies the effects of a drag-and-drop operation.
	/// This enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values.
	/// </summary>
	[Flags]
	//[Serializable]
	public enum DragDropEffects {

		None = 0,
		Copy = 1,
		Move = 2,
		Link = 4,
		Scroll = -2147483648,
		All = -2147483645,
	}
}
