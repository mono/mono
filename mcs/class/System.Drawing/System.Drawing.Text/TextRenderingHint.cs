//
// System.Drawing.TextRenderingHint.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//
using System;
namespace System.Drawing.Text 
{
	public enum TextRenderingHint {
		AntiAlias = 4,
		AntiAliasGridFit = 3,
		ClearTypeGridFit = 5,
		SingleBitPerPixel = 2,
		SingleBitPerPixelGridFit = 1,
		SystemDefault = 0
	}
}
