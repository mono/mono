//
// System.Windows.Forms.RichTextBoxSelectionTypes.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

namespace System.Windows.Forms
{
	[Flags]
	public enum RichTextBoxSelectionTypes
	{
		Empty = 0,
		MultiChar = 4,
		MultiObject = 8,
		Object = 2,
		Text = 1
	}
}
