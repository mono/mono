//
// System.Drawing.ItfFontFamily.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//   Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) 2002/2003 Ximian, Inc
//
using System;
using System.Drawing;
using System.Drawing.Text;

namespace System.Drawing {
	internal interface IFontFamilyFactory {
		IFontFamily FontFamily( GenericFontFamilies genericFamily);
		IFontFamily FontFamily( string familyName);
		IFontFamily FontFamily( string familyName, FontCollection collection);
 		IFontFamily[] GetFamilies(System.Drawing.Graphics graphics);		
	}
	
	internal interface IFontFamily : IDisposable {
		string Name { get; }
		int GetCellAscent (FontStyle style);
		int GetCellDescent (FontStyle style);		
		int GetEmHeight (FontStyle style);
		int GetLineSpacing (FontStyle style);				
		bool IsStyleAvailable (FontStyle style);		
	}
}
