//
// System.Drawing.RotateFlipType .cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//

using System;
namespace System.Drawing 
{
	public enum RotateFlipType {
		RotateNoneFlipNone = 0,
		Rotate180FlipXY    = 0,
		Rotate90FlipNone   = 1,
		Rotate270FlipXY    = 1,
		Rotate180FlipNone  = 2,
		RotateNoneFlipXY   = 2,
		Rotate270FlipNone  = 3,
		Rotate90FlipXY     = 3,
		RotateNoneFlipX    = 4,
		Rotate180FlipY     = 4,
		Rotate90FlipX      = 5,
		Rotate270FlipY     = 5,
		Rotate180FlipX     = 6,
		RotateNoneFlipY    = 6,
		Rotate270FlipX     = 7,
		Rotate90FlipY      = 7,
	}
}
