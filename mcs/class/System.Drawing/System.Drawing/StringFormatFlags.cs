//
// System.Drawing.StringFormatFlags.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//


using System;
namespace System.Drawing 
{
	public enum StringFormatFlags {
		DirectionRightToLeft = 1,
		DirectionVertical = 2,
		DisplayFormatControl = 3,
		FitBlackBox = 4,
		LineLimit = 5,
		MeasureTrailingSpaces = 6,
		NoClip = 7,
		NoFontFallback = 8,
		NoWrap = 9,
	}
}
