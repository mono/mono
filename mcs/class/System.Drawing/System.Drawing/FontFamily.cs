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
	public class FontFamily {
	
		string name;
		static FontFamily genericMonospace = null;
		static FontFamily genericSansSerif = null;
		static FontFamily genericSerif = null;
		
		public FontFamily(GenericFontFamilies genericFamily) {
			throw new NotImplementedException();
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
					genericMonospace = new FontFamily("GenericMonospace");
				}
				return genericMonospace;
			}
		}
		
		public static FontFamily GenericSansSerif {
			get {
				if( genericSansSerif == null) {
					genericSansSerif = new FontFamily("GenericSansSerif");
				}
				return genericSansSerif;
			}
		}
		
		public static FontFamily GenericSerif {
			get {
				if( genericSerif == null) {
					genericSerif = new FontFamily("GenericSerif");
				}
				return genericSerif;
			}
		}
	}
}
