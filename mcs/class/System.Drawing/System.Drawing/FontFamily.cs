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

	public class FontFamily : MarshalByRefObject, IDisposable {
		internal IFontFamily implementation;
		internal static IFontFamilyFactory factory = Factories.GetFontFamilyFactory();
		
		static FontFamily genericMonospace;
		static FontFamily genericSansSerif;
		static FontFamily genericSerif;

		string name;
		
		public FontFamily(GenericFontFamilies genericFamily) {
			implementation = factory.FontFamily(genericFamily);
		}
		
		public FontFamily(string familyName) {
			name = familyName;
			implementation = factory.FontFamily(familyName);
		}
		
		public FontFamily(string familyName, FontCollection collection) {
			name = familyName;
			implementation = factory.FontFamily(familyName, collection);
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
			return implementation.GetCellAscent(style);
		}
		
		public int GetCellDescent (FontStyle style) {
			return implementation.GetCellDescent(style);
		}
		
		public int GetEmHeight (FontStyle style) {
			return implementation.GetEmHeight(style);
		}
		
		public int GetLineSpacing (FontStyle style) {
			return implementation.GetLineSpacing(style);
		}
		
		public bool IsStyleAvailable (FontStyle style){
			return implementation.IsStyleAvailable(style);
		}
		
		public void Dispose() {
			implementation.Dispose();
		}
	}
}
