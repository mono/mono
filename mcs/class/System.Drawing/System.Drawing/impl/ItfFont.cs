//
// System.Drawing.Brush.cs
//
// Author:
//   Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Drawing 
{
	internal interface IFontFactory {
		IFont Font(FontFamily family, float size, FontStyle style);
		IFont Font(string familyName, float size, FontStyle style);
		IFont FromHfont(IntPtr font);
	}

	internal interface IFont : IDisposable
	{
		IntPtr ToHfont ();
	}
}
