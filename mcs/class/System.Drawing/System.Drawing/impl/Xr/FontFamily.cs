//
// System.Drawing.XrImpl.FontFamily.cs
//
// Author:
//   Alexandre Pigolkine (pigolkine@gmx.de)
//
//

using System;
using System.Drawing;
using System.Drawing.Text;

namespace System.Drawing {
	namespace XrImpl	{

		internal class FontFamilyFactory : IFontFamilyFactory {

			IFontFamily IFontFamilyFactory.FontFamily(GenericFontFamilies genericFamily){
				return new XrFontFamily(genericFamily);
			}

			IFontFamily IFontFamilyFactory.FontFamily(string familyName){
				return new XrFontFamily(familyName);
			}
			
			IFontFamily IFontFamilyFactory.FontFamily( string familyName, FontCollection collection){
				return new XrFontFamily(familyName,collection);
			}
			
	 		IFontFamily[] IFontFamilyFactory.GetFamilies(System.Drawing.Graphics graphics) { 
				throw new NotImplementedException();
			}		
			
		}

		internal class XrFontFamily : MarshalByRefObject, IFontFamily
		{
			string name;
			static XrFontFamily genericMonospace;
			static XrFontFamily genericSansSerif;
			static XrFontFamily genericSerif;
			
			public XrFontFamily(GenericFontFamilies genericFamily) {
				throw new NotImplementedException();
			}
		
			public XrFontFamily(string familyName) {
				name = familyName;
			}
		
			public XrFontFamily(string familyName, FontCollection collection) {
				name = familyName;
			}
		
			public string Name {
				get {
					return name;
				}
			}
		
			public int GetCellAscent (FontStyle style) {
				return 0;
			}
		
			public int GetCellDescent (FontStyle style) {
				return 0;
			}
		
			public int GetEmHeight (FontStyle style) {
				return 0;
			}
		
			public int GetLineSpacing (FontStyle style) {
				return 0;
			}
		
			public bool IsStyleAvailable (FontStyle style){
				return false;
			}
			
			public void Dispose() {
			}
		}
	}
}
