//
// System.Windows.Forms.UICues.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Windows.Forms {

	// Specifies the state of the user interface
	[Flags]
	public enum UICues 
	{
		Changed			= 12,			// The state of the focus cues and keyboard cues has changed. 
		ChangeFocus		=  4,			// The state of the focus cues has changed. 
		ChangeKeyboard	=  8,			// The state of the keyboard cues has changed. 
		None			=  0,			// No change was made.
		ShowFocus		=  1,			// Focus rectangles are displayed after the change.
		ShowKeyboard	=  2,			// Keyboard cues are underlined after the change. 
		Shown			=  3,			// Focus rectangles are displayed and keyboard cues are underlined 
	}

}
