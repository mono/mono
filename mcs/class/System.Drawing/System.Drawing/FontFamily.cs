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
	/// <summary>
	/// Summary description for FontFamily.
	/// </summary>
	public class FontFamily : MarshalByRefObject, IDisposable {
		internal IFontFamily implementation_;
		internal static IFontFamilyFactory factory_ = Factories.GetFontFamilyFactory();
		
		static FontFamily genericMonospace;
		static FontFamily genericSansSerif;
		static FontFamily genericSerif;

		string name;
		
		public FontFamily(GenericFontFamilies genericFamily) {
			implementation_ = factory_.FontFamily(genericFamily);
		}
		
		public FontFamily(string familyName) {
			name = familyName;
			implementation_ = factory_.FontFamily(familyName);
		}
		
		public FontFamily(string familyName, FontCollection collection) {
			name = familyName;
			implementation_ = factory_.FontFamily(familyName, collection);
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
			return implementation_.GetCellAscent(style);
		}
		
		public int GetCellDescent (FontStyle style) {
			return implementation_.GetCellDescent(style);
		}
		
		public int GetEmHeight (FontStyle style) {
			return implementation_.GetEmHeight(style);
		}
		
		public int GetLineSpacing (FontStyle style) {
			return implementation_.GetLineSpacing(style);
		}
		
		public bool IsStyleAvailable (FontStyle style){
			return implementation_.IsStyleAvailable(style);
		}
		
		public void Dispose() {
			implementation_.Dispose();
		}
	}
}
