//
// System.Drawing.FontFamily.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) 2002/2003 Ximian, Inc
//
using System;
using System.Drawing.Text;

namespace System.Drawing {

	public sealed class FontFamily : MarshalByRefObject, IDisposable {
		
		static FontFamily genericMonospace;
		static FontFamily genericSansSerif;
		static FontFamily genericSerif;

		string name;
		
		public FontFamily(GenericFontFamilies genericFamily) {
		}
		
		public FontFamily(string familyName) {
			name = familyName;
		}
		
		public FontFamily(string familyName, FontCollection collection) {
			name = familyName;
		}
		
		public string Name {
			get {
				return name;
			}
		}
		
		public static FontFamily GenericMonospace {
			get {
				if( genericMonospace == null) {
					genericMonospace = new FontFamily(GenericFontFamilies.Monospace);
				}
				return genericMonospace;
			}
		}
		
		public static FontFamily GenericSansSerif {
			get {
				if( genericSansSerif == null) {
					genericSansSerif = new FontFamily(GenericFontFamilies.SansSerif);
				}
				return genericSansSerif;
			}
		}
		
		public static FontFamily GenericSerif {
			get {
				if( genericSerif == null) {
					genericSerif = new FontFamily(GenericFontFamilies.Serif);
				}
				return genericSerif;
			}
		}
		
		public int GetCellAscent (FontStyle style) {
			throw new NotImplementedException ();
		}
		
		public int GetCellDescent (FontStyle style) {
			throw new NotImplementedException ();
		}
		
		public int GetEmHeight (FontStyle style) {
			throw new NotImplementedException ();
		}
		
		public int GetLineSpacing (FontStyle style) {
			throw new NotImplementedException ();
		}
		
		public bool IsStyleAvailable (FontStyle style){
			throw new NotImplementedException ();
		}
		
		public void Dispose() {
		}
	}
}
