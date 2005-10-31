//------------------------------------------------------------------------------
// 
// System.Globalization.NumberStyles.cs 
//
// Copyright (C) 2001 Michael Lambert, All Rights Reserved
// 
// Author:         Michael Lambert, michaellambert@email.com
// Created:        Thu 07/18/2001 
//
// Modified:       7/20/01, Derek Holden (dholden@draper.com)
//                 Added ECMA values for allows and masks for data types
//
//------------------------------------------------------------------------------

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace System.Globalization {

	[Flags]
	[Serializable]
#if NET_2_0
	[System.Runtime.InteropServices.ComVisible(true)]
#endif
	public enum NumberStyles {
		None                 = 0x00000000,
		AllowLeadingWhite    = 0x00000001,
		AllowTrailingWhite   = 0x00000002,
		AllowLeadingSign     = 0x00000004,
		AllowTrailingSign    = 0x00000008,
		AllowParentheses     = 0x00000010,
		AllowDecimalPoint    = 0x00000020,
		AllowThousands       = 0x00000040,
		AllowExponent        = 0x00000080,
		AllowCurrencySymbol  = 0x00000100,
		AllowHexSpecifier    = 0x00000200,

		Integer   = ( AllowLeadingWhite | AllowTrailingWhite | AllowLeadingSign ),
		HexNumber = ( AllowLeadingWhite | AllowTrailingWhite | AllowHexSpecifier ),
		Number    = ( AllowLeadingWhite | AllowTrailingWhite | AllowLeadingSign | 
			      AllowTrailingSign | AllowDecimalPoint  | AllowThousands ), 
		Float     = ( AllowLeadingWhite | AllowTrailingWhite | AllowLeadingSign | 
			      AllowDecimalPoint | AllowExponent ), 	     
		Currency  = ( AllowLeadingWhite | AllowTrailingWhite | AllowLeadingSign | 
			      AllowTrailingSign | AllowParentheses   | AllowDecimalPoint |
			      AllowThousands    | AllowCurrencySymbol ), 
		Any 	  = ( AllowLeadingWhite | AllowTrailingWhite | AllowLeadingSign | 
			      AllowTrailingSign | AllowParentheses   | AllowDecimalPoint |
			      AllowThousands    | AllowExponent      | AllowCurrencySymbol ), 
	}

} // Namespace
