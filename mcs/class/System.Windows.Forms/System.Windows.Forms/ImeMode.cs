//
// System.Windows.Forms.ImeMode.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[ComVisible (true)]
	public enum ImeMode
	{
		Alpha = 8,
		AlphaFull = 7,
		Disable = 3,
		Hangul = 10,
		HangulFull = 9,
		Hiragana = 4,
		Inherit = -1,
		Katakana = 5,
		KatakanaHalf = 6,
		NoControl = 0,
		Off = 2,
		On = 1
	}
}
