//
// System.Drawing.Drawing2D.LineJoin.cs
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
	/// Summary description for LineJoin.
	/// </summary>
	[Serializable]
	public enum LineJoin {
		Bevel = 1,
		Miter = 0,
		MiterClipped = 3,
		Round = 2
	}
}
