//
// System.Drawing.Drawing2D.CombineMode.cs
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
	/// Summary description for CombineMode.
	/// </summary>
    public enum CombineMode
    {
        Complement = 5,
        Exclude = 4,
        Intersect = 1,
        Replace = 0,
        Union = 2,
        Xor = 3
    }
}
