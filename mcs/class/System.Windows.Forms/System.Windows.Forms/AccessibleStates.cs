//
// System.Windows.Forms.AccessibleStates.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {

	/// <summary>
	/// </summary>
	public enum AccessibleStates{
		AlertLow = 67108864,
		AlertMedium = 134217728,
		AlertHigh = 268435456,
		Animated = 16384,
		Busy = 2048,
		Checked = 16,
		Collapsed = 1024,
		Default = 256,
		Expanded = 512,
		ExtSelectable = 33554432,
		Floating = 4096,
		Focusable = 1048576,
		Focused = 4,
		HotTracked = 128,
		Indeterminate =32,
		Mixed = 32,
		Invisible = 32768,
		Linked = 4194304,
		Marqueed = 8192,
		Moveable = 262144,
		MultiSelectable = 16777216,
		None = 0,
		Pressed = 8,
		Protected = 536870912,
		ReadOnly = 64,
		Offscreen = 65536,
		Selectable = 2097152,
		Selected = 2,
		SelfVoicing = 524288,
		Sizeable = 131072,
		Traversed =8388608,
		Unavailable = 1,
		Valid = 1073741823,		


		//Where did these come from, are they missing from elsewhere?
		//AddSelection = ,
		//ExtendSelection = ,
		//None = ,
		//RemoveSelection = ,
		//TakeFocus = ,
		//TakeSelection = 
	}
}
