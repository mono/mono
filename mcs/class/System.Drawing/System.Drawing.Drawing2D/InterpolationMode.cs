//
// System.Drawing.Drawing2D.InterpolationMode.cs
//
// Author:
//   Stefan Maierhofer <sm@cg.tuwien.ac.at>
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002/3 Ximian, Inc
//
using System;

namespace System.Drawing.Drawing2D {
	/// <summary>
	/// Summary description for InterpolationMode.
	/// </summary>
	public enum InterpolationMode {
		Bicubic = 4,
		Bilinear = 3,
		Default = 0,
		High = 2,
		HighQualityBicubic = 7,
		HighQualityBilinear = 6,
		Invalid = -1,
		Low = 1,
		NearestNeighbour = 5
	}
}
