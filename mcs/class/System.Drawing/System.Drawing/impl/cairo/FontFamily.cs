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

namespace System.Drawing.Cairo {

        internal class FontFamilyFactory : IFontFamilyFactory {

                IFontFamily IFontFamilyFactory.FontFamily(GenericFontFamilies genericFamily){
                        return new CairoFontFamily (genericFamily);
                }

                IFontFamily IFontFamilyFactory.FontFamily(string familyName){
                        return new CairoFontFamily (familyName);
                }
			
                IFontFamily IFontFamilyFactory.FontFamily( string familyName, FontCollection collection){
                        return new CairoFontFamily (familyName,collection);
                }
			
                IFontFamily[] IFontFamilyFactory.GetFamilies(System.Drawing.Graphics graphics) { 
                        throw new NotImplementedException();
                }		
			
        }

        internal class CairoFontFamily : MarshalByRefObject, IFontFamily
        {
                string name;
                static CairoFontFamily genericMonospace;
                static CairoFontFamily genericSansSerif;
                static CairoFontFamily genericSerif;
			
                public CairoFontFamily (GenericFontFamilies genericFamily)
                {
                        throw new NotImplementedException();
                }
		
                public CairoFontFamily (string familyName)
                {
                        name = familyName;
                }
		
                public CairoFontFamily (string familyName, FontCollection collection)
                {
                        name = familyName;
                }
		
                public string Name {
                        get {
                                return name;
                        }
                }
		
                public int GetCellAscent (FontStyle style)
                {
                        return 0;
                }
		
                public int GetCellDescent (FontStyle style)
                {
                        return 0;
                }
		
                public int GetEmHeight (FontStyle style)
                {
                        return 0;
                }
		
                public int GetLineSpacing (FontStyle style)
                {
                        return 0;
                }
		
                public bool IsStyleAvailable (FontStyle style)
                {
                        return false;
                }
			
                public void Dispose()
                {
                }
        }
}
