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

namespace System.Globalization {

	[Flags]
	[Serializable]
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
