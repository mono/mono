//
// System.Drawing.Font.cs
//
// Author:
//   Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Drawing;

namespace System.Drawing {
	namespace XrImpl	{

		internal class FontFactory : IFontFactory {

			IFont IFontFactory.Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte charSet, bool isVertical){
				return new Font(family, emSize, style, unit, charSet, isVertical);
			}

			IFont IFontFactory.Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte charSet, bool isVertical) {
				return new Font(familyName, emSize, style, unit, charSet, isVertical);
			}

			IFont IFontFactory.FromHfont(IntPtr font) {
				return Win32Impl.Font.FromHfont(font);
			}
		}

		internal sealed class Font : MarshalByRefObject, IFont
		{
			FontFamily fontFamily;
			byte gdiCharSet;
			bool gdiVerticalFont;
			int  height;
			string name;
			float size;
			float sizeInPoints;
			FontStyle style;
			GraphicsUnit unit;
			
			public void Dispose () {
			}

			public static Font FromHfont(IntPtr font)
			{
				//FIXME: how to get those values ?
				Font result = new Font("Arial", (float)12.0, FontStyle.Regular, GraphicsUnit.Point, (byte)0, false);
				return result;
			}

			public IntPtr ToHfont () { throw new NotImplementedException(); }

			public Font(FontFamily family, float size, FontStyle style, GraphicsUnit unit, byte charSet, bool isVertical){
				fontFamily = family;
				this.size = size;
				this.style = style;
				this.unit = unit;
				this.gdiCharSet = charSet;
				this.gdiVerticalFont = isVertical;
			}

			public Font(string familyName, float size, FontStyle style, GraphicsUnit unit, byte charSet, bool isVertical){
				fontFamily = new FontFamily(familyName);
				this.size = size;
				this.style = style;
				this.unit = unit;
				this.gdiCharSet = charSet;
				this.gdiVerticalFont = isVertical;
			}

			internal void SetXrValues(IntPtr xrs) {
				Xr.XrSelectFont( xrs, String.Format("{0}-{1}", fontFamily.Name, size) );
			}
			
			public bool Bold {
				get {
					return (style & FontStyle.Bold) == FontStyle.Bold;
				}
			}
		
			public FontFamily FontFamily {
				get {
					return fontFamily;
				}
			}
		
			public byte GdiCharSet {
				get {
					return gdiCharSet;
				}
			}
		
			public bool GdiVerticalFont {
				get {
					return gdiVerticalFont;
				}
			}
		
			public int Height {
				get {
					return height;
				}
			}

			public bool Italic {
				get {
					return (style & FontStyle.Italic) == FontStyle.Italic;
				}
			}

			public string Name {
				get {
					return name;
				}
			}

			public float Size {
				get {
					return size;
				}
			}

			public float SizeInPoints {
				get {
					return sizeInPoints;
				}
			}

			public bool Strikeout {
				get {
					return (style & FontStyle.Strikeout) == FontStyle.Strikeout;
				}
			}
		
			public FontStyle Style {
				get {
					return style;
				}
			}

			public bool Underline {
				get {
					return (style & FontStyle.Underline) == FontStyle.Underline;
				}
			}

			public GraphicsUnit Unit {
				get {
					return unit;
				}
			}
		}
	}
}
