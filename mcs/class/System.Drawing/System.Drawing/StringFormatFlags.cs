//
// System.Drawing.StringFormatFlags.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//


using System;
namespace System.Drawing 
{
	[Flags]
	public enum StringFormatFlags {
		DirectionRightToLeft  = 0x0001,
		DirectionVertical     = 0x0002,
		FitBlackBox           = 0x0004,
		DisplayFormatControl  = 0x0020,
		NoFontFallback        = 0x0400,
		MeasureTrailingSpaces = 0x0800,
		NoWrap                = 0x1000,
		LineLimit             = 0x2000,
		NoClip                = 0x4000
	}
}
