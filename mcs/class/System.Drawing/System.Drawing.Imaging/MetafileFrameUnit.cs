//
// System.Drawing.Imaging.MetafileFrameUnit.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//
using System;
namespace System.Drawing.Imaging 
{
	[Serializable]
	public enum MetafileFrameUnit {
		Document = 5,
		GdiCompatible = 7,
		Inch = 4,
		Millimeter = 6,
		Pixel = 2,
		Point = 3
	}
}
