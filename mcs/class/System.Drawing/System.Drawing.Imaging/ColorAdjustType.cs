//
// System.Drawing.Imaging.ColorAdjustType.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//
using System;
namespace System.Drawing.Imaging 
{
	[Serializable]
	public enum ColorAdjustType {
		Any = 6,
		Bitmap = 1,
		Brush = 2,
		Count = 5,
		Default = 0,
		Pen = 3,
		Text = 4
	}
}
