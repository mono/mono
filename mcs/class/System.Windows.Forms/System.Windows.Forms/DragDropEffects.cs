//
// System.Windows.Forms.DragDropEffects
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
	/// Specifies the effects of a drag-and-drop operation.
	/// This enumeration has a FlagsAttribute attribute that allows a bitwise combination of its member values.
	/// </summary>
	[Flags]
	[Serializable]
	public enum DragDropEffects
	{
		All = 1,
		Copy = 2,
		Link = 4,
		Move = 8,
		None = 0,
		Scroll = 16
	}
}