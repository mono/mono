//
// System.Drawing.Drawing2D.PathPointType.cs
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
	/// Summary description for PathPointType.
	/// </summary>
	public enum PathPointType {
		Bezier = 3,
		Bezier3 = 3,
		CloseSubpath = 128,
		DashMode = 16,
		Line = 1,
		PathMarker = 32,
		PathTypeMask = 7,
		Start = 0
	}
}
