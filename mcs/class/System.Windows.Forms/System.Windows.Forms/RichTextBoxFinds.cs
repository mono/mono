//
// System.Windows.Forms.RichTextBoxFinds.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

namespace System.Windows.Forms
{
	[Flags]
	public enum RichTextBoxFinds
	{
		MatchCase = 4,
		NoHighlight = 8,
		None = 0,
		Reverse = 16,
		WholeWord = 2
	}
}
