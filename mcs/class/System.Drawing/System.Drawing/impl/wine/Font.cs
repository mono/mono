//
// System.Drawing.Font.cs
//
// Author:
//   Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Drawing {
	namespace Win32Impl	{

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
			IntPtr hfont_ = IntPtr.Zero;
			bool ownObject_ = true;
			FontFamily fontFamily;
			byte gdiCharSet;
			bool gdiVerticalFont;
			int  height;
			string name;
			float size;
			float sizeInPoints;
			FontStyle style;
			GraphicsUnit unit;
			
			public void Dispose ()
			{
				if( ownObject_) {
					Win32.DeleteObject(hfont_);
				}
			}

			public static Font FromHfont(IntPtr font)
			{
				//FIXME: select font into temporary HDC to retrive parameters
				Font result = new Font("Arial", (float)12.0, FontStyle.Regular, GraphicsUnit.Point, (byte)0, false);
				result.hfont_ = font;
				result.ownObject_ = false;
				return result;
			}

			public IntPtr ToHfont () { return hfont_; }

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
