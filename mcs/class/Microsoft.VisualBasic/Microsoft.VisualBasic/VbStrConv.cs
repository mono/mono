//
// VbStrConv.cs
//
// Authors:
//   Martin Adoue (martin@cwanet.com)
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Ximian Inc.
//

using System;

namespace Microsoft.VisualBasic
{
	/// <summary>
	/// When you call the StrConv function, you can use the following enumeration 
	/// members in your code in place of the actual values.
	/// </summary>
	[System.FlagsAttribute] 
	public enum VbStrConv : int {
		/// <summary>
		/// Performs no conversion 
		/// </summary>
		None = 0,
		/// <summary>
		/// Uses linguistic rules for casing, rather than File System (default). Valid with UpperCase and LowerCase only. 
		/// </summary>
		LinguisticCasing = 1024,
		/// <summary>
		/// Converts the string to uppercase characters. 
		/// </summary>
		UpperCase = 1,
		/// <summary>
		/// Converts the string to lowercase characters. 
		/// </summary>
		LowerCase = 2,
		/// <summary>
		/// Converts the first letter of every word in string to uppercase.
		/// </summary>
		ProperCase = 3,
		/// <summary>
		/// Converts narrow (half-width) characters in the string to wide (full-width) characters. (Applies to Asian locales.)
		/// </summary>
		Wide = 4,					//*  
		/// <summary>
		/// Converts wide (full-width) characters in the string to narrow (half-width) characters. (Applies to Asian locales.)
		/// </summary>
		Narrow = 8,					//*  
		/// <summary>
		/// Converts Hiragana characters in the string to Katakana characters. (Applies to Japan only.)
		/// </summary>
		Katakana = 16,				//**  
		/// <summary>
		/// Converts Katakana characters in the string to Hiragana characters. (Applies to Japan only.)
		/// </summary>
		Hiragana = 32,				//** 
		/// <summary>
		/// Converts Traditional Chinese characters to Simplified Chinese. (Applies to Asian locales.)
		/// </summary>
		SimplifiedChinese =256,		//*  
		/// <summary>
		/// Converts Simplified Chinese characters to Traditional Chinese. (Applies to Asian locales.)
		/// </summary>
		TraditionalChinese = 512	//*  
		/*
		
		*   Applies to Asian locales.
		**  Applies to Japan only.
		
		*/
	}

}
