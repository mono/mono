//
// System.Drawing.IFont.cs
//
// Author:
//   Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Drawing 
{
	internal interface IFontFactory {
		IFont Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte charSet, bool isVertical);
		IFont Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte charSet, bool isVertical);
		IFont FromHfont(IntPtr font);
	}

	internal interface IFont : IDisposable
	{
		IntPtr ToHfont ();
		
		bool Bold { get ; }
		
		FontFamily FontFamily { get ;}
		
		byte GdiCharSet { get; }
		
		bool GdiVerticalFont { get; }
		
		int Height { get; }

		bool Italic { get; }

		string Name { get; }

		float Size { get; }

		float SizeInPoints { get; }

		bool Strikeout { get; }
		
		FontStyle Style { get; }

		bool Underline { get; }

		GraphicsUnit Unit { get; }
	}
}
