//
// System.Drawing.GraphicsUnit.cs
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Author: Dennis Hayes (dennish@raytek.com)
//

using System;
namespace System.Drawing 
{
	[Serializable]
	public enum GraphicsUnit {
		World      = 0,
		Display    = 1,
		Pixel      = 2,
		Point      = 3,
		Inch       = 4,
		Document   = 5,
		Millimeter = 6
	}
}
