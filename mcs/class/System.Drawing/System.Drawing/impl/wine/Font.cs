//
// System.Drawing.Brush.cs
//
// Author:
//   Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Drawing {
	namespace Win32Impl	{

		internal class FontFactory : IFontFactory {

			IFont IFontFactory.Font(FontFamily family, float size, FontStyle style){
				return new Font(family, size, style);
			}

			IFont IFontFactory.Font(string familyName, float size, FontStyle style) {
				return new Font(familyName, size, style);
			}

			IFont IFontFactory.FromHfont(IntPtr font) {
				return Win32Impl.Font.FromHfont(font);
			}
		}

		internal sealed class Font : MarshalByRefObject, IFont
		{
			private IntPtr hfont_ = IntPtr.Zero;
			private bool ownObject_ = true;
		
			public void Dispose ()
			{
				if( ownObject_) {
					Win32.DeleteObject(hfont_);
				}
			}

			public static Font FromHfont(IntPtr font)
			{
				//FIXME: select font into temporary HDC to retrive parameters
				Font result = new Font("Arial", (float)12.0, FontStyle.Regular);
				result.hfont_ = font;
				result.ownObject_ = false;
				return result;
			}

			public IntPtr ToHfont () { return hfont_; }

			public Font(FontFamily family, float size, FontStyle style)
			{
			}

			public Font(string familyName, float size, FontStyle style)
			{
			}

		}
	}
}
