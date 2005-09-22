//
// System.Drawing.FontFamily.cs
//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
// Author: Konstantin Triger (kostat@mainsoft.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Drawing.Text;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;
using awt = java.awt;
using geom = java.awt.geom;
using font = java.awt.font;

namespace System.Drawing {

	public sealed class FontFamily : MarshalByRefObject, IDisposable {
		
		static readonly FontFamily _genericMonospace;
		static readonly FontFamily _genericSansSerif;
		static readonly FontFamily _genericSerif;
		static readonly FontCollection _installedFonts;

		static FontFamily() {
			_installedFonts = new InstalledFontCollection();
			_genericMonospace = new FontFamily("Monospaced");
			_genericSansSerif = new FontFamily("SansSerif");
			_genericSerif = new FontFamily("Serif");
		}
		
		private readonly string _name;

		// this is unavailable through Java API, usually 2048 for TT fonts
		const int UnitsPerEm = 2048;
		
		
//		~FontFamily()
//		{	
//		}

		// dummy ctors to work around convertor problems
		internal FontFamily() {}
		internal FontFamily(IntPtr family) {}
		
		public FontFamily(string familyName) : this(familyName, null)
		{}

		public FontFamily(string name, FontCollection fontCollection) {
			if (fontCollection == null)
				fontCollection = _installedFonts;

			string familyName = fontCollection.GetFamilyName(name);
			if (familyName == null)
				throw new ArgumentException(String.Format("Font family '{0}' not found", name));

			_name = familyName;
		}

		public FontFamily(GenericFontFamilies genericFamily) {
			switch(genericFamily) {
				case GenericFontFamilies.SansSerif:
					_name = _genericSansSerif._name;
					break;
				case GenericFontFamilies.Serif:
					_name = _genericSerif._name;
					break;
				default:
					_name = _genericMonospace._name;
					break;
			}
		}
		
		
		public string Name {
			get {
				return _name;
			}
		}

		public static FontFamily[] Families {
			get {
				return _installedFonts.Families;
			}
		}
		
		public static FontFamily GenericMonospace {
			get {
				return _genericMonospace;
			}
		}
		
		public static FontFamily GenericSansSerif {
			get {
				return _genericSansSerif;
			}
		}
		
		public static FontFamily GenericSerif {
			get {
				return _genericSerif;
			}
		}		

		public override bool Equals(object obj) {
			if (this == obj)
				return true;

			if (!(obj is FontFamily))
				return false;

			return string.Compare(Name, ((FontFamily)obj).Name, true) == 0;
		}

		awt.FontMetrics GetMetrics(FontStyle style) {
			awt.Container c = new awt.Container();
			return c.getFontMetrics(new Font(this, (float)(UnitsPerEm<<1), style, GraphicsUnit.World).NativeObject);
		}

		public int GetCellAscent(FontStyle style) {
			return GetMetrics(style).getMaxAscent()>>1;
		}

		public int GetCellDescent(FontStyle style) {
			return GetMetrics(style).getMaxDecent()>>1;
		}

		public int GetEmHeight(FontStyle style) {
			return UnitsPerEm;
		}

		public static FontFamily[] GetFamilies(Graphics graphics) {
			if (graphics == null) {
				throw new ArgumentNullException("graphics");
			}
			return _installedFonts.Families;
		}

		public override int GetHashCode() {
			return Name.ToLower().GetHashCode();
		}

		public int GetLineSpacing(FontStyle style) {
			return GetMetrics(style).getHeight()>>1;
		}

		public string GetName(int language) {
			CultureInfo culture = new CultureInfo(language, false);
			//TBD: get java locale
			return new awt.Font(_name, awt.Font.PLAIN, 1).getFamily(null);
		}

		public bool IsStyleAvailable(FontStyle style) {
			return (new Font(this, (float)(UnitsPerEm<<1), style, GraphicsUnit.World).Style & style) == style;
		}

		public override string ToString() {
			return string.Format("[{0}: Name={1}]", GetType().Name, Name);
		}



		#region IDisposable Members

		public void Dispose() {
			// TODO:  Add FontFamily.Dispose implementation
		}

		#endregion
	}
}

