//
// System.Drawing.Win32Impl.FontFamily.cs
//
// Author:
//   Alexandre Pigolkine (pigolkine@gmx.de)
//
//

using System;
using System.Drawing;
using System.Drawing.Text;

namespace System.Drawing {
	namespace Win32Impl	{

		internal class FontFamilyFactory : IFontFamilyFactory {

			IFontFamily IFontFamilyFactory.FontFamily(GenericFontFamilies genericFamily){
				return new Win32FontFamily(genericFamily);
			}

			IFontFamily IFontFamilyFactory.FontFamily(string familyName){
				return new Win32FontFamily(familyName);
			}
			
			IFontFamily IFontFamilyFactory.FontFamily( string familyName, FontCollection collection){
				return new Win32FontFamily(familyName,collection);
			}
			
	 		IFontFamily[] IFontFamilyFactory.GetFamilies(System.Drawing.Graphics graphics) { 
				throw new NotImplementedException();
			}		
			
		}

		internal class Win32FontFamily : MarshalByRefObject, IFontFamily
		{
			string name;
			static Win32FontFamily genericMonospace;
			static Win32FontFamily genericSansSerif;
			static Win32FontFamily genericSerif;
			
			public Win32FontFamily(GenericFontFamilies genericFamily) {
				throw new NotImplementedException();
			}
		
			public Win32FontFamily(string familyName) {
				name = familyName;
			}
		
			public Win32FontFamily(string familyName, FontCollection collection) {
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
