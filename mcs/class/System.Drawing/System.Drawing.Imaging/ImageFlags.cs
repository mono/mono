//
// System.Drawing.ImageFlags.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//
using System;
namespace System.Drawing.Imaging
{
	public enum ImageFlags {
		Caching,
		ColorSpaceCmyk,
		ColorSpaceGray,
		ColorSpaceRgb,
		ColorSpaceYcbcr,
		ColorSpaceYcck,
		HasAlpha,
		HasRealDpi,
		HasRealPixelSize,
		HasTranslucent,
		None,
		PartiallyScalable,
		ReadOnly,
		Scalable
	}
}
