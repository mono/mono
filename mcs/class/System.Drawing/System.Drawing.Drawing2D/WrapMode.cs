//
// System.Drawing.Drawing2D.WrapMode.cs
//
// Authors:
//   Stefan Maierhofer <sm@cg.tuwien.ac.at>
//   Dennis Hayes (dennish@Raytek.com)
//   Ravindra (rkumar@novell.com)
//
// (C) 2002/3 Ximian, Inc. http://www.ximian.com
// (C) 2004 Novell, Inc. http://www.novell.com
//
using System;

namespace System.Drawing.Drawing2D {
	/// <summary>
	/// Summary description for WrapMode.
	/// </summary>
	[Serializable]
	public enum WrapMode {
		Tile,
		TileFlipX,
		TileFlipY,
		TileFlipXY,
		Clamp
	}
}
