//
// System.Drawing.Imaging.PaletteFlags.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//
using System;
namespace System.Drawing.Imaging 
{
	[Serializable]
	public enum PaletteFlags {
		GrayScale = 2,
		Halftone = 4,
		HasAlpha = 1
	}
}
