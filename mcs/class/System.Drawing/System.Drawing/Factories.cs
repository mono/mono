//
// System.Drawing.Bitmap.cs
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//
// Authors:
// 	Alexandre Pigolkine ( pigolkine@gmx.de)
//

using System;
using System.Drawing;

internal class Factories {
	internal static IBitmapFactory GetBitmapFactory () {
		return new System.Drawing.Win32Impl.BitmapFactory ();
	}

	internal static IFontFactory GetFontFactory () {
		return new System.Drawing.Win32Impl.FontFactory ();
	}

	internal static IGraphicsFactory GetGraphicsFactory () {
		return new System.Drawing.Win32Impl.GraphicsFactory ();
	}

	internal static IPenFactory GetPenFactory () {
		return new System.Drawing.Win32Impl.PenFactory ();
	}

	internal static ISolidBrushFactory GetSolidBrushFactory () {
		return new System.Drawing.Win32Impl.SolidBrushFactory ();
	}
}