/*
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
namespace Monodoc.Lucene.Net.Analysis.RU
{
	/// <summary> RussianCharsets class contains encodings schemes (charsets) and toLowerCase() method implementation
	/// for russian characters in Unicode, KOI8 and CP1252.
	/// Each encoding scheme contains lowercase (positions 0-31) and uppercase (position 32-63) characters.
	/// One should be able to add other encoding schemes (like ISO-8859-5 or customized) by adding a new charset
	/// and adding logic to toLowerCase() method for that charset.
	/// 
	/// </summary>
	/// <author>   Boris Okner, b.okner@rogers.com
	/// </author>
	/// <version>  $Id: RussianCharsets.java,v 1.3 2004/03/29 22:48:01 cutting Exp $
	/// </version>
	public class RussianCharsets
	{
		// Unicode Russian charset (lowercase letters only)
		public static char[] UnicodeRussian = new char[]{'\u0430', '\u0431', '\u0432', '\u0433', '\u0434', '\u0435', '\u0436', '\u0437', '\u0438', '\u0439', '\u043A', '\u043B', '\u043C', '\u043D', '\u043E', '\u043F', '\u0440', '\u0441', '\u0442', '\u0443', '\u0444', '\u0445', '\u0446', '\u0447', '\u0448', '\u0449', '\u044A', '\u044B', '\u044C', '\u044D', '\u044E', '\u044F', '\u0410', '\u0411', '\u0412', '\u0413', '\u0414', '\u0415', '\u0416', '\u0417', '\u0418', '\u0419', '\u041A', '\u041B', '\u041C', '\u041D', '\u041E', '\u041F', '\u0420', '\u0421', '\u0422', '\u0423', '\u0424', '\u0425', '\u0426', '\u0427', '\u0428', '\u0429', '\u042A', '\u042B', '\u042C', '\u042D', '\u042E', '\u042F'};
		
		// KOI8 charset
		public static char[] KOI8 = new char[]{(char) (0xc1), (char) (0xc2), (char) (0xd7), (char) (0xc7), (char) (0xc4), (char) (0xc5), (char) (0xd6), (char) (0xda), (char) (0xc9), (char) (0xca), (char) (0xcb), (char) (0xcc), (char) (0xcd), (char) (0xce), (char) (0xcf), (char) (0xd0), (char) (0xd2), (char) (0xd3), (char) (0xd4), (char) (0xd5), (char) (0xc6), (char) (0xc8), (char) (0xc3), (char) (0xde), (char) (0xdb), (char) (0xdd), (char) (0xdf), (char) (0xd9), (char) (0xd8), (char) (0xdc), (char) (0xc0), (char) (0xd1), (char) (0xe1), (char) (0xe2), (char) (0xf7), (char) (0xe7), (char) (0xe4), (char) (0xe5), (char) (0xf6), (char) (0xfa), (char) (0xe9), (char) (0xea), (char) (0xeb), (char) (0xec), (char) (0xed), (char) (0xee), (char) (0xef), (char) (0xf0), (char) (0xf2), (char) (0xf3), (char) (0xf4), (char) (0xf5), (char) (0xe6), (char) (0xe8), (char) (0xe3), (char) (0xfe), (char) (0xfb), (char) (0xfd), (char) (0xff), (char) (0xf9), (char) (0xf8), (char) (0xfc), (char) (0xe0), (char) (0xf1)};
		
		// CP1251 eharset
		public static char[] CP1251 = new char[]{(char) (0xE0), (char) (0xE1), (char) (0xE2), (char) (0xE3), (char) (0xE4), (char) (0xE5), (char) (0xE6), (char) (0xE7), (char) (0xE8), (char) (0xE9), (char) (0xEA), (char) (0xEB), (char) (0xEC), (char) (0xED), (char) (0xEE), (char) (0xEF), (char) (0xF0), (char) (0xF1), (char) (0xF2), (char) (0xF3), (char) (0xF4), (char) (0xF5), (char) (0xF6), (char) (0xF7), (char) (0xF8), (char) (0xF9), (char) (0xFA), (char) (0xFB), (char) (0xFC), (char) (0xFD), (char) (0xFE), (char) (0xFF), (char) (0xC0), (char) (0xC1), (char) (0xC2), (char) (0xC3), (char) (0xC4), (char) (0xC5), (char) (0xC6), (char) (0xC7), (char) (0xC8), (char) (0xC9), (char) (0xCA), (char) (0xCB), (char) (0xCC), (char) (0xCD), (char) (0xCE), (char) (0xCF), (char) (0xD0), (char) (0xD1), (char) (0xD2), (char) (0xD3), (char) (0xD4), (char) (0xD5), (char) (0xD6), (char) (0xD7), (char) (0xD8), (char) (0xD9), (char) (0xDA), (char) (0xDB), (char) (0xDC), (char) (0xDD), (char) (0xDE), (char) (0xDF)};
		
		public static char ToLowerCase(char letter, char[] charset)
		{
			if (charset == UnicodeRussian)
			{
				if (letter >= '\u0430' && letter <= '\u044F')
				{
					return letter;
				}
				if (letter >= '\u0410' && letter <= '\u042F')
				{
					return (char) (letter + 32);
				}
			}
			
			if (charset == KOI8)
			{
				if (letter >= 0xe0 && letter <= 0xff)
				{
					return (char) (letter - 32);
				}
				if (letter >= 0xc0 && letter <= 0xdf)
				{
					return letter;
				}
			}
			
			if (charset == CP1251)
			{
				if (letter >= 0xC0 && letter <= 0xDF)
				{
					return (char) (letter + 32);
				}
				if (letter >= 0xE0 && letter <= 0xFF)
				{
					return letter;
				}
			}
			
			return System.Char.ToLower(letter);
		}
	}
}