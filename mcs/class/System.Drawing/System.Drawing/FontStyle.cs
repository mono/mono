//
// System.Drawing.FontStyle.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//


using System;
namespace System.Drawing 
{
	[Flags]
	[Serializable]
	public enum FontStyle {
		Regular   = 0,
		Bold      = 1,
		Italic    = 2,
		Underline = 4,
		Strikeout = 8
	}
}
