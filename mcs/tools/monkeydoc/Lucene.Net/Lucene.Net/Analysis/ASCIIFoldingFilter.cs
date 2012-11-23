/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
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

using TermAttribute = Mono.Lucene.Net.Analysis.Tokenattributes.TermAttribute;
using ArrayUtil = Mono.Lucene.Net.Util.ArrayUtil;

namespace Mono.Lucene.Net.Analysis
{
	
	/// <summary> This class converts alphabetic, numeric, and symbolic Unicode characters
	/// which are not in the first 127 ASCII characters (the "Basic Latin" Unicode
	/// block) into their ASCII equivalents, if one exists.
	/// 
	/// Characters from the following Unicode blocks are converted; however, only
	/// those characters with reasonable ASCII alternatives are converted:
	/// 
	/// <ul>
	/// <li>C1 Controls and Latin-1 Supplement: <a href="http://www.unicode.org/charts/PDF/U0080.pdf">http://www.unicode.org/charts/PDF/U0080.pdf</a></li>
    /// <li>Latin Extended-A: <a href="http://www.unicode.org/charts/PDF/U0100.pdf">http://www.unicode.org/charts/PDF/U0100.pdf</a></li>
    /// <li>Latin Extended-B: <a href="http://www.unicode.org/charts/PDF/U0180.pdf">http://www.unicode.org/charts/PDF/U0180.pdf</a></li>
    /// <li>Latin Extended Additional: <a href="http://www.unicode.org/charts/PDF/U1E00.pdf">http://www.unicode.org/charts/PDF/U1E00.pdf</a></li>
    /// <li>Latin Extended-C: <a href="http://www.unicode.org/charts/PDF/U2C60.pdf">http://www.unicode.org/charts/PDF/U2C60.pdf</a></li>
    /// <li>Latin Extended-D: <a href="http://www.unicode.org/charts/PDF/UA720.pdf">http://www.unicode.org/charts/PDF/UA720.pdf</a></li>
    /// <li>IPA Extensions: <a href="http://www.unicode.org/charts/PDF/U0250.pdf">http://www.unicode.org/charts/PDF/U0250.pdf</a></li>
    /// <li>Phonetic Extensions: <a href="http://www.unicode.org/charts/PDF/U1D00.pdf">http://www.unicode.org/charts/PDF/U1D00.pdf</a></li>
    /// <li>Phonetic Extensions Supplement: <a href="http://www.unicode.org/charts/PDF/U1D80.pdf">http://www.unicode.org/charts/PDF/U1D80.pdf</a></li>
    /// <li>General Punctuation: <a href="http://www.unicode.org/charts/PDF/U2000.pdf">http://www.unicode.org/charts/PDF/U2000.pdf</a></li>
    /// <li>Superscripts and Subscripts: <a href="http://www.unicode.org/charts/PDF/U2070.pdf">http://www.unicode.org/charts/PDF/U2070.pdf</a></li>
    /// <li>Enclosed Alphanumerics: <a href="http://www.unicode.org/charts/PDF/U2460.pdf">http://www.unicode.org/charts/PDF/U2460.pdf</a></li>
    /// <li>Dingbats: <a href="http://www.unicode.org/charts/PDF/U2700.pdf">http://www.unicode.org/charts/PDF/U2700.pdf</a></li>
    /// <li>Supplemental Punctuation: <a href="http://www.unicode.org/charts/PDF/U2E00.pdf">http://www.unicode.org/charts/PDF/U2E00.pdf</a></li>
    /// <li>Alphabetic Presentation Forms: <a href="http://www.unicode.org/charts/PDF/UFB00.pdf">http://www.unicode.org/charts/PDF/UFB00.pdf</a></li>
    /// <li>Halfwidth and Fullwidth Forms: <a href="http://www.unicode.org/charts/PDF/UFF00.pdf">http://www.unicode.org/charts/PDF/UFF00.pdf</a></li>
	/// </ul>
	/// 
	/// See: <a href="http://en.wikipedia.org/wiki/Latin_characters_in_Unicode">http://en.wikipedia.org/wiki/Latin_characters_in_Unicode</a>
	/// 
	/// The set of character conversions supported by this class is a superset of
	/// those supported by Lucene's {@link ISOLatin1AccentFilter} which strips
	/// accents from Latin1 characters.  For example, '&#192;' will be replaced by
	/// 'a'.
	/// </summary>
	public sealed class ASCIIFoldingFilter:TokenFilter
	{
		public ASCIIFoldingFilter(TokenStream input):base(input)
		{
			termAtt = (TermAttribute) AddAttribute(typeof(TermAttribute));
		}
		
		private char[] output = new char[512];
		private int outputPos;
		private TermAttribute termAtt;
		
		public override bool IncrementToken()
		{
			if (input.IncrementToken())
			{
				char[] buffer = termAtt.TermBuffer();
				int length = termAtt.TermLength();
				
				// If no characters actually require rewriting then we
				// just return token as-is:
				for (int i = 0; i < length; ++i)
				{
					char c = buffer[i];
					if (c >= '\u0080')
					{
						FoldToASCII(buffer, length);
						termAtt.SetTermBuffer(output, 0, outputPos);
						break;
					}
				}
				return true;
			}
			else
			{
				return false;
			}
		}
		
		/// <summary> Converts characters above ASCII to their ASCII equivalents.  For example,
		/// accents are removed from accented characters.
		/// </summary>
		/// <param name="input">The string to fold
		/// </param>
		/// <param name="length">The number of characters in the input string
		/// </param>
		public void  FoldToASCII(char[] input, int length)
		{
			// Worst-case length required:
			int maxSizeNeeded = 4 * length;
			if (output.Length < maxSizeNeeded)
			{
				output = new char[ArrayUtil.GetNextSize(maxSizeNeeded)];
			}
			
			outputPos = 0;
			
			for (int pos = 0; pos < length; ++pos)
			{
				char c = input[pos];
				
				// Quick test: if it's not in range then just keep current character
				if (c < '\u0080')
				{
					output[outputPos++] = c;
				}
				else
				{
					switch (c)
					{
						
						case '\u00C0': 
						// Ãƒâ‚¬  [LATIN CAPITAL LETTER A WITH GRAVE]
						case '\u00C1': 
						// Ãƒï¿½  [LATIN CAPITAL LETTER A WITH ACUTE]
						case '\u00C2': 
						// Ãƒâ€š  [LATIN CAPITAL LETTER A WITH CIRCUMFLEX]
						case '\u00C3': 
						// ÃƒÆ’  [LATIN CAPITAL LETTER A WITH TILDE]
						case '\u00C4': 
						// Ãƒâ€ž  [LATIN CAPITAL LETTER A WITH DIAERESIS]
						case '\u00C5': 
						// Ãƒâ€¦  [LATIN CAPITAL LETTER A WITH RING ABOVE]
						case '\u0100': 
						// Ã„â‚¬  [LATIN CAPITAL LETTER A WITH MACRON]
						case '\u0102': 
						// Ã„â€š  [LATIN CAPITAL LETTER A WITH BREVE]
						case '\u0104': 
						// Ã„â€ž  [LATIN CAPITAL LETTER A WITH OGONEK]
						case '\u018F': 
						// Ã†ï¿½  http://en.wikipedia.org/wiki/Schwa  [LATIN CAPITAL LETTER SCHWA]
						case '\u01CD': 
						// Ã‡ï¿½  [LATIN CAPITAL LETTER A WITH CARON]
						case '\u01DE': 
						// Ã‡Å¾  [LATIN CAPITAL LETTER A WITH DIAERESIS AND MACRON]
						case '\u01E0': 
						// Ã‡Â   [LATIN CAPITAL LETTER A WITH DOT ABOVE AND MACRON]
						case '\u01FA': 
						// Ã‡Âº  [LATIN CAPITAL LETTER A WITH RING ABOVE AND ACUTE]
						case '\u0200': 
						// Ãˆâ‚¬  [LATIN CAPITAL LETTER A WITH DOUBLE GRAVE]
						case '\u0202': 
						// Ãˆâ€š  [LATIN CAPITAL LETTER A WITH INVERTED BREVE]
						case '\u0226': 
						// ÃˆÂ¦  [LATIN CAPITAL LETTER A WITH DOT ABOVE]
						case '\u023A': 
						// ÃˆÂº  [LATIN CAPITAL LETTER A WITH STROKE]
						case '\u1D00': 
						// Ã¡Â´â‚¬  [LATIN LETTER SMALL CAPITAL A]
						case '\u1E00': 
						// Ã¡Â¸â‚¬  [LATIN CAPITAL LETTER A WITH RING BELOW]
						case '\u1EA0': 
						// Ã¡ÂºÂ   [LATIN CAPITAL LETTER A WITH DOT BELOW]
						case '\u1EA2': 
						// Ã¡ÂºÂ¢  [LATIN CAPITAL LETTER A WITH HOOK ABOVE]
						case '\u1EA4': 
						// Ã¡ÂºÂ¤  [LATIN CAPITAL LETTER A WITH CIRCUMFLEX AND ACUTE]
						case '\u1EA6': 
						// Ã¡ÂºÂ¦  [LATIN CAPITAL LETTER A WITH CIRCUMFLEX AND GRAVE]
						case '\u1EA8': 
						// Ã¡ÂºÂ¨  [LATIN CAPITAL LETTER A WITH CIRCUMFLEX AND HOOK ABOVE]
						case '\u1EAA': 
						// Ã¡ÂºÂª  [LATIN CAPITAL LETTER A WITH CIRCUMFLEX AND TILDE]
						case '\u1EAC': 
						// Ã¡ÂºÂ¬  [LATIN CAPITAL LETTER A WITH CIRCUMFLEX AND DOT BELOW]
						case '\u1EAE': 
						// Ã¡ÂºÂ®  [LATIN CAPITAL LETTER A WITH BREVE AND ACUTE]
						case '\u1EB0': 
						// Ã¡ÂºÂ°  [LATIN CAPITAL LETTER A WITH BREVE AND GRAVE]
						case '\u1EB2': 
						// Ã¡ÂºÂ²  [LATIN CAPITAL LETTER A WITH BREVE AND HOOK ABOVE]
						case '\u1EB4': 
						// Ã¡ÂºÂ´  [LATIN CAPITAL LETTER A WITH BREVE AND TILDE]
						case '\u1EB6': 
						// Ã¡ÂºÂ¶  [LATIN CAPITAL LETTER A WITH BREVE AND DOT BELOW]
						case '\u24B6': 
						// Ã¢â€™Â¶  [CIRCLED LATIN CAPITAL LETTER A]
						case '\uFF21':  // Ã¯Â¼Â¡  [FULLWIDTH LATIN CAPITAL LETTER A]
							output[outputPos++] = 'A';
							break;
						
						case '\u00E0': 
						// ÃƒÂ   [LATIN SMALL LETTER A WITH GRAVE]
						case '\u00E1': 
						// ÃƒÂ¡  [LATIN SMALL LETTER A WITH ACUTE]
						case '\u00E2': 
						// ÃƒÂ¢  [LATIN SMALL LETTER A WITH CIRCUMFLEX]
						case '\u00E3': 
						// ÃƒÂ£  [LATIN SMALL LETTER A WITH TILDE]
						case '\u00E4': 
						// ÃƒÂ¤  [LATIN SMALL LETTER A WITH DIAERESIS]
						case '\u00E5': 
						// ÃƒÂ¥  [LATIN SMALL LETTER A WITH RING ABOVE]
						case '\u0101': 
						// Ã„ï¿½  [LATIN SMALL LETTER A WITH MACRON]
						case '\u0103': 
						// Ã„Æ’  [LATIN SMALL LETTER A WITH BREVE]
						case '\u0105': 
						// Ã„â€¦  [LATIN SMALL LETTER A WITH OGONEK]
						case '\u01CE': 
						// Ã‡Å½  [LATIN SMALL LETTER A WITH CARON]
						case '\u01DF': 
						// Ã‡Å¸  [LATIN SMALL LETTER A WITH DIAERESIS AND MACRON]
						case '\u01E1': 
						// Ã‡Â¡  [LATIN SMALL LETTER A WITH DOT ABOVE AND MACRON]
						case '\u01FB': 
						// Ã‡Â»  [LATIN SMALL LETTER A WITH RING ABOVE AND ACUTE]
						case '\u0201': 
						// Ãˆï¿½  [LATIN SMALL LETTER A WITH DOUBLE GRAVE]
						case '\u0203': 
						// ÃˆÆ’  [LATIN SMALL LETTER A WITH INVERTED BREVE]
						case '\u0227': 
						// ÃˆÂ§  [LATIN SMALL LETTER A WITH DOT ABOVE]
						case '\u0250': 
						// Ã‰ï¿½  [LATIN SMALL LETTER TURNED A]
						case '\u0259': 
						// Ã‰â„¢  [LATIN SMALL LETTER SCHWA]
						case '\u025A': 
						// Ã‰Å¡  [LATIN SMALL LETTER SCHWA WITH HOOK]
						case '\u1D8F': 
						// Ã¡Â¶ï¿½  [LATIN SMALL LETTER A WITH RETROFLEX HOOK]
						case '\u1D95': 
						// Ã¡Â¶â€¢  [LATIN SMALL LETTER SCHWA WITH RETROFLEX HOOK]
						case '\u1E01': 
						// Ã¡ÂºÂ¡  [LATIN SMALL LETTER A WITH RING BELOW]
						case '\u1E9A': 
						// Ã¡ÂºÂ£  [LATIN SMALL LETTER A WITH RIGHT HALF RING]
						case '\u1EA1': 
						// Ã¡ÂºÂ¡  [LATIN SMALL LETTER A WITH DOT BELOW]
						case '\u1EA3': 
						// Ã¡ÂºÂ£  [LATIN SMALL LETTER A WITH HOOK ABOVE]
						case '\u1EA5': 
						// Ã¡ÂºÂ¥  [LATIN SMALL LETTER A WITH CIRCUMFLEX AND ACUTE]
						case '\u1EA7': 
						// Ã¡ÂºÂ§  [LATIN SMALL LETTER A WITH CIRCUMFLEX AND GRAVE]
						case '\u1EA9': 
						// Ã¡ÂºÂ©  [LATIN SMALL LETTER A WITH CIRCUMFLEX AND HOOK ABOVE]
						case '\u1EAB': 
						// Ã¡ÂºÂ«  [LATIN SMALL LETTER A WITH CIRCUMFLEX AND TILDE]
						case '\u1EAD': 
						// Ã¡ÂºÂ­  [LATIN SMALL LETTER A WITH CIRCUMFLEX AND DOT BELOW]
						case '\u1EAF': 
						// Ã¡ÂºÂ¯  [LATIN SMALL LETTER A WITH BREVE AND ACUTE]
						case '\u1EB1': 
						// Ã¡ÂºÂ±  [LATIN SMALL LETTER A WITH BREVE AND GRAVE]
						case '\u1EB3': 
						// Ã¡ÂºÂ³  [LATIN SMALL LETTER A WITH BREVE AND HOOK ABOVE]
						case '\u1EB5': 
						// Ã¡ÂºÂµ  [LATIN SMALL LETTER A WITH BREVE AND TILDE]
						case '\u1EB7': 
						// Ã¡ÂºÂ·  [LATIN SMALL LETTER A WITH BREVE AND DOT BELOW]
						case '\u2090': 
						// Ã¢â€šï¿½  [LATIN SUBSCRIPT SMALL LETTER A]
						case '\u2094': 
						// Ã¢â€šï¿½?  [LATIN SUBSCRIPT SMALL LETTER SCHWA]
						case '\u24D0': 
						// Ã¢â€œï¿½  [CIRCLED LATIN SMALL LETTER A]
						case '\u2C65': 
						// Ã¢Â±Â¥  [LATIN SMALL LETTER A WITH STROKE]
						case '\u2C6F': 
						// Ã¢Â±Â¯  [LATIN CAPITAL LETTER TURNED A]
						case '\uFF41':  // Ã¯Â½ï¿½  [FULLWIDTH LATIN SMALL LETTER A]
							output[outputPos++] = 'a';
							break;
						
						case '\uA732':  // ÃªÅ“Â²  [LATIN CAPITAL LETTER AA]
							output[outputPos++] = 'A';
							output[outputPos++] = 'A';
							break;
						
						case '\u00C6': 
						// Ãƒâ€   [LATIN CAPITAL LETTER AE]
						case '\u01E2': 
						// Ã‡Â¢  [LATIN CAPITAL LETTER AE WITH MACRON]
						case '\u01FC': 
						// Ã‡Â¼  [LATIN CAPITAL LETTER AE WITH ACUTE]
						case '\u1D01':  // Ã¡Â´ï¿½  [LATIN LETTER SMALL CAPITAL AE]
							output[outputPos++] = 'A';
							output[outputPos++] = 'E';
							break;
						
						case '\uA734':  // ÃªÅ“Â´  [LATIN CAPITAL LETTER AO]
							output[outputPos++] = 'A';
							output[outputPos++] = 'O';
							break;
						
						case '\uA736':  // ÃªÅ“Â¶  [LATIN CAPITAL LETTER AU]
							output[outputPos++] = 'A';
							output[outputPos++] = 'U';
							break;
						
						case '\uA738': 
						// ÃªÅ“Â¸  [LATIN CAPITAL LETTER AV]
						case '\uA73A':  // ÃªÅ“Âº  [LATIN CAPITAL LETTER AV WITH HORIZONTAL BAR]
							output[outputPos++] = 'A';
							output[outputPos++] = 'V';
							break;
						
						case '\uA73C':  // ÃªÅ“Â¼  [LATIN CAPITAL LETTER AY]
							output[outputPos++] = 'A';
							output[outputPos++] = 'Y';
							break;
						
						case '\u249C':  // Ã¢â€™Å“  [PARENTHESIZED LATIN SMALL LETTER A]
							output[outputPos++] = '(';
							output[outputPos++] = 'a';
							output[outputPos++] = ')';
							break;
						
						case '\uA733':  // ÃªÅ“Â³  [LATIN SMALL LETTER AA]
							output[outputPos++] = 'a';
							output[outputPos++] = 'a';
							break;
						
						case '\u00E6': 
						// ÃƒÂ¦  [LATIN SMALL LETTER AE]
						case '\u01E3': 
						// Ã‡Â£  [LATIN SMALL LETTER AE WITH MACRON]
						case '\u01FD': 
						// Ã‡Â½  [LATIN SMALL LETTER AE WITH ACUTE]
						case '\u1D02':  // Ã¡Â´â€š  [LATIN SMALL LETTER TURNED AE]
							output[outputPos++] = 'a';
							output[outputPos++] = 'e';
							break;
						
						case '\uA735':  // ÃªÅ“Âµ  [LATIN SMALL LETTER AO]
							output[outputPos++] = 'a';
							output[outputPos++] = 'o';
							break;
						
						case '\uA737':  // ÃªÅ“Â·  [LATIN SMALL LETTER AU]
							output[outputPos++] = 'a';
							output[outputPos++] = 'u';
							break;
						
						case '\uA739': 
						// ÃªÅ“Â¹  [LATIN SMALL LETTER AV]
						case '\uA73B':  // ÃªÅ“Â»  [LATIN SMALL LETTER AV WITH HORIZONTAL BAR]
							output[outputPos++] = 'a';
							output[outputPos++] = 'v';
							break;
						
						case '\uA73D':  // ÃªÅ“Â½  [LATIN SMALL LETTER AY]
							output[outputPos++] = 'a';
							output[outputPos++] = 'y';
							break;
						
						case '\u0181': 
						// Ã†ï¿½  [LATIN CAPITAL LETTER B WITH HOOK]
						case '\u0182': 
						// Ã†â€š  [LATIN CAPITAL LETTER B WITH TOPBAR]
						case '\u0243': 
						// Ã‰Æ’  [LATIN CAPITAL LETTER B WITH STROKE]
						case '\u0299': 
						// ÃŠâ„¢  [LATIN LETTER SMALL CAPITAL B]
						case '\u1D03': 
						// Ã¡Â´Æ’  [LATIN LETTER SMALL CAPITAL BARRED B]
						case '\u1E02': 
						// Ã¡Â¸â€š  [LATIN CAPITAL LETTER B WITH DOT ABOVE]
						case '\u1E04': 
						// Ã¡Â¸â€ž  [LATIN CAPITAL LETTER B WITH DOT BELOW]
						case '\u1E06': 
						// Ã¡Â¸â€   [LATIN CAPITAL LETTER B WITH LINE BELOW]
						case '\u24B7': 
						// Ã¢â€™Â·  [CIRCLED LATIN CAPITAL LETTER B]
						case '\uFF22':  // Ã¯Â¼Â¢  [FULLWIDTH LATIN CAPITAL LETTER B]
							output[outputPos++] = 'B';
							break;
						
						case '\u0180': 
						// Ã†â‚¬  [LATIN SMALL LETTER B WITH STROKE]
						case '\u0183': 
						// Ã†Æ’  [LATIN SMALL LETTER B WITH TOPBAR]
						case '\u0253': 
						// Ã‰â€œ  [LATIN SMALL LETTER B WITH HOOK]
						case '\u1D6C': 
						// Ã¡ÂµÂ¬  [LATIN SMALL LETTER B WITH MIDDLE TILDE]
						case '\u1D80': 
						// Ã¡Â¶â‚¬  [LATIN SMALL LETTER B WITH PALATAL HOOK]
						case '\u1E03': 
						// Ã¡Â¸Æ’  [LATIN SMALL LETTER B WITH DOT ABOVE]
						case '\u1E05': 
						// Ã¡Â¸â€¦  [LATIN SMALL LETTER B WITH DOT BELOW]
						case '\u1E07': 
						// Ã¡Â¸â€¡  [LATIN SMALL LETTER B WITH LINE BELOW]
						case '\u24D1': 
						// Ã¢â€œâ€˜  [CIRCLED LATIN SMALL LETTER B]
						case '\uFF42':  // Ã¯Â½â€š  [FULLWIDTH LATIN SMALL LETTER B]
							output[outputPos++] = 'b';
							break;
						
						case '\u249D':  // Ã¢â€™ï¿½  [PARENTHESIZED LATIN SMALL LETTER B]
							output[outputPos++] = '(';
							output[outputPos++] = 'b';
							output[outputPos++] = ')';
							break;
						
						case '\u00C7': 
						// Ãƒâ€¡  [LATIN CAPITAL LETTER C WITH CEDILLA]
						case '\u0106': 
						// Ã„â€   [LATIN CAPITAL LETTER C WITH ACUTE]
						case '\u0108': 
						// Ã„Ë†  [LATIN CAPITAL LETTER C WITH CIRCUMFLEX]
						case '\u010A': 
						// Ã„Å   [LATIN CAPITAL LETTER C WITH DOT ABOVE]
						case '\u010C': 
						// Ã„Å’  [LATIN CAPITAL LETTER C WITH CARON]
						case '\u0187': 
						// Ã†â€¡  [LATIN CAPITAL LETTER C WITH HOOK]
						case '\u023B': 
						// ÃˆÂ»  [LATIN CAPITAL LETTER C WITH STROKE]
						case '\u0297': 
						// ÃŠâ€”  [LATIN LETTER STRETCHED C]
						case '\u1D04': 
						// Ã¡Â´â€ž  [LATIN LETTER SMALL CAPITAL C]
						case '\u1E08': 
						// Ã¡Â¸Ë†  [LATIN CAPITAL LETTER C WITH CEDILLA AND ACUTE]
						case '\u24B8': 
						// Ã¢â€™Â¸  [CIRCLED LATIN CAPITAL LETTER C]
						case '\uFF23':  // Ã¯Â¼Â£  [FULLWIDTH LATIN CAPITAL LETTER C]
							output[outputPos++] = 'C';
							break;
						
						case '\u00E7': 
						// ÃƒÂ§  [LATIN SMALL LETTER C WITH CEDILLA]
						case '\u0107': 
						// Ã„â€¡  [LATIN SMALL LETTER C WITH ACUTE]
						case '\u0109': 
						// Ã„â€°  [LATIN SMALL LETTER C WITH CIRCUMFLEX]
						case '\u010B': 
						// Ã„â€¹  [LATIN SMALL LETTER C WITH DOT ABOVE]
						case '\u010D': 
						// Ã„ï¿½  [LATIN SMALL LETTER C WITH CARON]
						case '\u0188': 
						// Ã†Ë†  [LATIN SMALL LETTER C WITH HOOK]
						case '\u023C': 
						// ÃˆÂ¼  [LATIN SMALL LETTER C WITH STROKE]
						case '\u0255': 
						// Ã‰â€¢  [LATIN SMALL LETTER C WITH CURL]
						case '\u1E09': 
						// Ã¡Â¸â€°  [LATIN SMALL LETTER C WITH CEDILLA AND ACUTE]
						case '\u2184': 
						// Ã¢â€ â€ž  [LATIN SMALL LETTER REVERSED C]
						case '\u24D2': 
						// Ã¢â€œâ€™  [CIRCLED LATIN SMALL LETTER C]
						case '\uA73E': 
						// ÃªÅ“Â¾  [LATIN CAPITAL LETTER REVERSED C WITH DOT]
						case '\uA73F': 
						// ÃªÅ“Â¿  [LATIN SMALL LETTER REVERSED C WITH DOT]
						case '\uFF43':  // Ã¯Â½Æ’  [FULLWIDTH LATIN SMALL LETTER C]
							output[outputPos++] = 'c';
							break;
						
						case '\u249E':  // Ã¢â€™Å¾  [PARENTHESIZED LATIN SMALL LETTER C]
							output[outputPos++] = '(';
							output[outputPos++] = 'c';
							output[outputPos++] = ')';
							break;
						
						case '\u00D0': 
						// Ãƒï¿½  [LATIN CAPITAL LETTER ETH]
						case '\u010E': 
						// Ã„Å½  [LATIN CAPITAL LETTER D WITH CARON]
						case '\u0110': 
						// Ã„ï¿½  [LATIN CAPITAL LETTER D WITH STROKE]
						case '\u0189': 
						// Ã†â€°  [LATIN CAPITAL LETTER AFRICAN D]
						case '\u018A': 
						// Ã†Å   [LATIN CAPITAL LETTER D WITH HOOK]
						case '\u018B': 
						// Ã†â€¹  [LATIN CAPITAL LETTER D WITH TOPBAR]
						case '\u1D05': 
						// Ã¡Â´â€¦  [LATIN LETTER SMALL CAPITAL D]
						case '\u1D06': 
						// Ã¡Â´â€   [LATIN LETTER SMALL CAPITAL ETH]
						case '\u1E0A': 
						// Ã¡Â¸Å   [LATIN CAPITAL LETTER D WITH DOT ABOVE]
						case '\u1E0C': 
						// Ã¡Â¸Å’  [LATIN CAPITAL LETTER D WITH DOT BELOW]
						case '\u1E0E': 
						// Ã¡Â¸Å½  [LATIN CAPITAL LETTER D WITH LINE BELOW]
						case '\u1E10': 
						// Ã¡Â¸ï¿½  [LATIN CAPITAL LETTER D WITH CEDILLA]
						case '\u1E12': 
						// Ã¡Â¸â€™  [LATIN CAPITAL LETTER D WITH CIRCUMFLEX BELOW]
						case '\u24B9': 
						// Ã¢â€™Â¹  [CIRCLED LATIN CAPITAL LETTER D]
						case '\uA779': 
						// Ãªï¿½Â¹  [LATIN CAPITAL LETTER INSULAR D]
						case '\uFF24':  // Ã¯Â¼Â¤  [FULLWIDTH LATIN CAPITAL LETTER D]
							output[outputPos++] = 'D';
							break;
						
						case '\u00F0': 
						// ÃƒÂ°  [LATIN SMALL LETTER ETH]
						case '\u010F': 
						// Ã„ï¿½  [LATIN SMALL LETTER D WITH CARON]
						case '\u0111': 
						// Ã„â€˜  [LATIN SMALL LETTER D WITH STROKE]
						case '\u018C': 
						// Ã†Å’  [LATIN SMALL LETTER D WITH TOPBAR]
						case '\u0221': 
						// ÃˆÂ¡  [LATIN SMALL LETTER D WITH CURL]
						case '\u0256': 
						// Ã‰â€“  [LATIN SMALL LETTER D WITH TAIL]
						case '\u0257': 
						// Ã‰â€”  [LATIN SMALL LETTER D WITH HOOK]
						case '\u1D6D': 
						// Ã¡ÂµÂ­  [LATIN SMALL LETTER D WITH MIDDLE TILDE]
						case '\u1D81': 
						// Ã¡Â¶ï¿½  [LATIN SMALL LETTER D WITH PALATAL HOOK]
						case '\u1D91': 
						// Ã¡Â¶â€˜  [LATIN SMALL LETTER D WITH HOOK AND TAIL]
						case '\u1E0B': 
						// Ã¡Â¸â€¹  [LATIN SMALL LETTER D WITH DOT ABOVE]
						case '\u1E0D': 
						// Ã¡Â¸ï¿½  [LATIN SMALL LETTER D WITH DOT BELOW]
						case '\u1E0F': 
						// Ã¡Â¸ï¿½  [LATIN SMALL LETTER D WITH LINE BELOW]
						case '\u1E11': 
						// Ã¡Â¸â€˜  [LATIN SMALL LETTER D WITH CEDILLA]
						case '\u1E13': 
						// Ã¡Â¸â€œ  [LATIN SMALL LETTER D WITH CIRCUMFLEX BELOW]
						case '\u24D3': 
						// Ã¢â€œâ€œ  [CIRCLED LATIN SMALL LETTER D]
						case '\uA77A': 
						// Ãªï¿½Âº  [LATIN SMALL LETTER INSULAR D]
						case '\uFF44':  // Ã¯Â½â€ž  [FULLWIDTH LATIN SMALL LETTER D]
							output[outputPos++] = 'd';
							break;
						
						case '\u01C4': 
						// Ã‡â€ž  [LATIN CAPITAL LETTER DZ WITH CARON]
						case '\u01F1':  // Ã‡Â±  [LATIN CAPITAL LETTER DZ]
							output[outputPos++] = 'D';
							output[outputPos++] = 'Z';
							break;
						
						case '\u01C5': 
						// Ã‡â€¦  [LATIN CAPITAL LETTER D WITH SMALL LETTER Z WITH CARON]
						case '\u01F2':  // Ã‡Â²  [LATIN CAPITAL LETTER D WITH SMALL LETTER Z]
							output[outputPos++] = 'D';
							output[outputPos++] = 'z';
							break;
						
						case '\u249F':  // Ã¢â€™Å¸  [PARENTHESIZED LATIN SMALL LETTER D]
							output[outputPos++] = '(';
							output[outputPos++] = 'd';
							output[outputPos++] = ')';
							break;
						
						case '\u0238':  // ÃˆÂ¸  [LATIN SMALL LETTER DB DIGRAPH]
							output[outputPos++] = 'd';
							output[outputPos++] = 'b';
							break;
						
						case '\u01C6': 
						// Ã‡â€   [LATIN SMALL LETTER DZ WITH CARON]
						case '\u01F3': 
						// Ã‡Â³  [LATIN SMALL LETTER DZ]
						case '\u02A3': 
						// ÃŠÂ£  [LATIN SMALL LETTER DZ DIGRAPH]
						case '\u02A5':  // ÃŠÂ¥  [LATIN SMALL LETTER DZ DIGRAPH WITH CURL]
							output[outputPos++] = 'd';
							output[outputPos++] = 'z';
							break;
						
						case '\u00C8': 
						// ÃƒË†  [LATIN CAPITAL LETTER E WITH GRAVE]
						case '\u00C9': 
						// Ãƒâ€°  [LATIN CAPITAL LETTER E WITH ACUTE]
						case '\u00CA': 
						// ÃƒÅ   [LATIN CAPITAL LETTER E WITH CIRCUMFLEX]
						case '\u00CB': 
						// Ãƒâ€¹  [LATIN CAPITAL LETTER E WITH DIAERESIS]
						case '\u0112': 
						// Ã„â€™  [LATIN CAPITAL LETTER E WITH MACRON]
						case '\u0114': 
						// Ã„ï¿½?  [LATIN CAPITAL LETTER E WITH BREVE]
						case '\u0116': 
						// Ã„â€“  [LATIN CAPITAL LETTER E WITH DOT ABOVE]
						case '\u0118': 
						// Ã„Ëœ  [LATIN CAPITAL LETTER E WITH OGONEK]
						case '\u011A': 
						// Ã„Å¡  [LATIN CAPITAL LETTER E WITH CARON]
						case '\u018E': 
						// Ã†Å½  [LATIN CAPITAL LETTER REVERSED E]
						case '\u0190': 
						// Ã†ï¿½  [LATIN CAPITAL LETTER OPEN E]
						case '\u0204': 
						// Ãˆâ€ž  [LATIN CAPITAL LETTER E WITH DOUBLE GRAVE]
						case '\u0206': 
						// Ãˆâ€   [LATIN CAPITAL LETTER E WITH INVERTED BREVE]
						case '\u0228': 
						// ÃˆÂ¨  [LATIN CAPITAL LETTER E WITH CEDILLA]
						case '\u0246': 
						// Ã‰â€   [LATIN CAPITAL LETTER E WITH STROKE]
						case '\u1D07': 
						// Ã¡Â´â€¡  [LATIN LETTER SMALL CAPITAL E]
						case '\u1E14': 
						// Ã¡Â¸ï¿½?  [LATIN CAPITAL LETTER E WITH MACRON AND GRAVE]
						case '\u1E16': 
						// Ã¡Â¸â€“  [LATIN CAPITAL LETTER E WITH MACRON AND ACUTE]
						case '\u1E18': 
						// Ã¡Â¸Ëœ  [LATIN CAPITAL LETTER E WITH CIRCUMFLEX BELOW]
						case '\u1E1A': 
						// Ã¡Â¸Å¡  [LATIN CAPITAL LETTER E WITH TILDE BELOW]
						case '\u1E1C': 
						// Ã¡Â¸Å“  [LATIN CAPITAL LETTER E WITH CEDILLA AND BREVE]
						case '\u1EB8': 
						// Ã¡ÂºÂ¸  [LATIN CAPITAL LETTER E WITH DOT BELOW]
						case '\u1EBA': 
						// Ã¡ÂºÂº  [LATIN CAPITAL LETTER E WITH HOOK ABOVE]
						case '\u1EBC': 
						// Ã¡ÂºÂ¼  [LATIN CAPITAL LETTER E WITH TILDE]
						case '\u1EBE': 
						// Ã¡ÂºÂ¾  [LATIN CAPITAL LETTER E WITH CIRCUMFLEX AND ACUTE]
						case '\u1EC0': 
						// Ã¡Â»â‚¬  [LATIN CAPITAL LETTER E WITH CIRCUMFLEX AND GRAVE]
						case '\u1EC2': 
						// Ã¡Â»â€š  [LATIN CAPITAL LETTER E WITH CIRCUMFLEX AND HOOK ABOVE]
						case '\u1EC4': 
						// Ã¡Â»â€ž  [LATIN CAPITAL LETTER E WITH CIRCUMFLEX AND TILDE]
						case '\u1EC6': 
						// Ã¡Â»â€   [LATIN CAPITAL LETTER E WITH CIRCUMFLEX AND DOT BELOW]
						case '\u24BA': 
						// Ã¢â€™Âº  [CIRCLED LATIN CAPITAL LETTER E]
						case '\u2C7B': 
						// Ã¢Â±Â»  [LATIN LETTER SMALL CAPITAL TURNED E]
						case '\uFF25':  // Ã¯Â¼Â¥  [FULLWIDTH LATIN CAPITAL LETTER E]
							output[outputPos++] = 'E';
							break;
						
						case '\u00E8': 
						// ÃƒÂ¨  [LATIN SMALL LETTER E WITH GRAVE]
						case '\u00E9': 
						// ÃƒÂ©  [LATIN SMALL LETTER E WITH ACUTE]
						case '\u00EA': 
						// ÃƒÂª  [LATIN SMALL LETTER E WITH CIRCUMFLEX]
						case '\u00EB': 
						// ÃƒÂ«  [LATIN SMALL LETTER E WITH DIAERESIS]
						case '\u0113': 
						// Ã„â€œ  [LATIN SMALL LETTER E WITH MACRON]
						case '\u0115': 
						// Ã„â€¢  [LATIN SMALL LETTER E WITH BREVE]
						case '\u0117': 
						// Ã„â€”  [LATIN SMALL LETTER E WITH DOT ABOVE]
						case '\u0119': 
						// Ã„â„¢  [LATIN SMALL LETTER E WITH OGONEK]
						case '\u011B': 
						// Ã„â€º  [LATIN SMALL LETTER E WITH CARON]
						case '\u01DD': 
						// Ã‡ï¿½  [LATIN SMALL LETTER TURNED E]
						case '\u0205': 
						// Ãˆâ€¦  [LATIN SMALL LETTER E WITH DOUBLE GRAVE]
						case '\u0207': 
						// Ãˆâ€¡  [LATIN SMALL LETTER E WITH INVERTED BREVE]
						case '\u0229': 
						// ÃˆÂ©  [LATIN SMALL LETTER E WITH CEDILLA]
						case '\u0247': 
						// Ã‰â€¡  [LATIN SMALL LETTER E WITH STROKE]
						case '\u0258': 
						// Ã‰Ëœ  [LATIN SMALL LETTER REVERSED E]
						case '\u025B': 
						// Ã‰â€º  [LATIN SMALL LETTER OPEN E]
						case '\u025C': 
						// Ã‰Å“  [LATIN SMALL LETTER REVERSED OPEN E]
						case '\u025D': 
						// Ã‰ï¿½  [LATIN SMALL LETTER REVERSED OPEN E WITH HOOK]
						case '\u025E': 
						// Ã‰Å¾  [LATIN SMALL LETTER CLOSED REVERSED OPEN E]
						case '\u029A': 
						// ÃŠÅ¡  [LATIN SMALL LETTER CLOSED OPEN E]
						case '\u1D08': 
						// Ã¡Â´Ë†  [LATIN SMALL LETTER TURNED OPEN E]
						case '\u1D92': 
						// Ã¡Â¶â€™  [LATIN SMALL LETTER E WITH RETROFLEX HOOK]
						case '\u1D93': 
						// Ã¡Â¶â€œ  [LATIN SMALL LETTER OPEN E WITH RETROFLEX HOOK]
						case '\u1D94': 
						// Ã¡Â¶ï¿½?  [LATIN SMALL LETTER REVERSED OPEN E WITH RETROFLEX HOOK]
						case '\u1E15': 
						// Ã¡Â¸â€¢  [LATIN SMALL LETTER E WITH MACRON AND GRAVE]
						case '\u1E17': 
						// Ã¡Â¸â€”  [LATIN SMALL LETTER E WITH MACRON AND ACUTE]
						case '\u1E19': 
						// Ã¡Â¸â„¢  [LATIN SMALL LETTER E WITH CIRCUMFLEX BELOW]
						case '\u1E1B': 
						// Ã¡Â¸â€º  [LATIN SMALL LETTER E WITH TILDE BELOW]
						case '\u1E1D': 
						// Ã¡Â¸ï¿½  [LATIN SMALL LETTER E WITH CEDILLA AND BREVE]
						case '\u1EB9': 
						// Ã¡ÂºÂ¹  [LATIN SMALL LETTER E WITH DOT BELOW]
						case '\u1EBB': 
						// Ã¡ÂºÂ»  [LATIN SMALL LETTER E WITH HOOK ABOVE]
						case '\u1EBD': 
						// Ã¡ÂºÂ½  [LATIN SMALL LETTER E WITH TILDE]
						case '\u1EBF': 
						// Ã¡ÂºÂ¿  [LATIN SMALL LETTER E WITH CIRCUMFLEX AND ACUTE]
						case '\u1EC1': 
						// Ã¡Â»ï¿½  [LATIN SMALL LETTER E WITH CIRCUMFLEX AND GRAVE]
						case '\u1EC3': 
						// Ã¡Â»Æ’  [LATIN SMALL LETTER E WITH CIRCUMFLEX AND HOOK ABOVE]
						case '\u1EC5': 
						// Ã¡Â»â€¦  [LATIN SMALL LETTER E WITH CIRCUMFLEX AND TILDE]
						case '\u1EC7': 
						// Ã¡Â»â€¡  [LATIN SMALL LETTER E WITH CIRCUMFLEX AND DOT BELOW]
						case '\u2091': 
						// Ã¢â€šâ€˜  [LATIN SUBSCRIPT SMALL LETTER E]
						case '\u24D4': 
						// Ã¢â€œï¿½?  [CIRCLED LATIN SMALL LETTER E]
						case '\u2C78': 
						// Ã¢Â±Â¸  [LATIN SMALL LETTER E WITH NOTCH]
						case '\uFF45':  // Ã¯Â½â€¦  [FULLWIDTH LATIN SMALL LETTER E]
							output[outputPos++] = 'e';
							break;
						
						case '\u24A0':  // Ã¢â€™Â   [PARENTHESIZED LATIN SMALL LETTER E]
							output[outputPos++] = '(';
							output[outputPos++] = 'e';
							output[outputPos++] = ')';
							break;
						
						case '\u0191': 
						// Ã†â€˜  [LATIN CAPITAL LETTER F WITH HOOK]
						case '\u1E1E': 
						// Ã¡Â¸Å¾  [LATIN CAPITAL LETTER F WITH DOT ABOVE]
						case '\u24BB': 
						// Ã¢â€™Â»  [CIRCLED LATIN CAPITAL LETTER F]
						case '\uA730': 
						// ÃªÅ“Â°  [LATIN LETTER SMALL CAPITAL F]
						case '\uA77B': 
						// Ãªï¿½Â»  [LATIN CAPITAL LETTER INSULAR F]
						case '\uA7FB': 
						// ÃªÅ¸Â»  [LATIN EPIGRAPHIC LETTER REVERSED F]
						case '\uFF26':  // Ã¯Â¼Â¦  [FULLWIDTH LATIN CAPITAL LETTER F]
							output[outputPos++] = 'F';
							break;
						
						case '\u0192': 
						// Ã†â€™  [LATIN SMALL LETTER F WITH HOOK]
						case '\u1D6E': 
						// Ã¡ÂµÂ®  [LATIN SMALL LETTER F WITH MIDDLE TILDE]
						case '\u1D82': 
						// Ã¡Â¶â€š  [LATIN SMALL LETTER F WITH PALATAL HOOK]
						case '\u1E1F': 
						// Ã¡Â¸Å¸  [LATIN SMALL LETTER F WITH DOT ABOVE]
						case '\u1E9B': 
						// Ã¡Âºâ€º  [LATIN SMALL LETTER LONG S WITH DOT ABOVE]
						case '\u24D5': 
						// Ã¢â€œâ€¢  [CIRCLED LATIN SMALL LETTER F]
						case '\uA77C': 
						// Ãªï¿½Â¼  [LATIN SMALL LETTER INSULAR F]
						case '\uFF46':  // Ã¯Â½â€   [FULLWIDTH LATIN SMALL LETTER F]
							output[outputPos++] = 'f';
							break;
						
						case '\u24A1':  // Ã¢â€™Â¡  [PARENTHESIZED LATIN SMALL LETTER F]
							output[outputPos++] = '(';
							output[outputPos++] = 'f';
							output[outputPos++] = ')';
							break;
						
						case '\uFB00':  // Ã¯Â¬â‚¬  [LATIN SMALL LIGATURE FF]
							output[outputPos++] = 'f';
							output[outputPos++] = 'f';
							break;
						
						case '\uFB03':  // Ã¯Â¬Æ’  [LATIN SMALL LIGATURE FFI]
							output[outputPos++] = 'f';
							output[outputPos++] = 'f';
							output[outputPos++] = 'i';
							break;
						
						case '\uFB04':  // Ã¯Â¬â€ž  [LATIN SMALL LIGATURE FFL]
							output[outputPos++] = 'f';
							output[outputPos++] = 'f';
							output[outputPos++] = 'l';
							break;
						
						case '\uFB01':  // Ã¯Â¬ï¿½  [LATIN SMALL LIGATURE FI]
							output[outputPos++] = 'f';
							output[outputPos++] = 'i';
							break;
						
						case '\uFB02':  // Ã¯Â¬â€š  [LATIN SMALL LIGATURE FL]
							output[outputPos++] = 'f';
							output[outputPos++] = 'l';
							break;
						
						case '\u011C': 
						// Ã„Å“  [LATIN CAPITAL LETTER G WITH CIRCUMFLEX]
						case '\u011E': 
						// Ã„Å¾  [LATIN CAPITAL LETTER G WITH BREVE]
						case '\u0120': 
						// Ã„Â   [LATIN CAPITAL LETTER G WITH DOT ABOVE]
						case '\u0122': 
						// Ã„Â¢  [LATIN CAPITAL LETTER G WITH CEDILLA]
						case '\u0193': 
						// Ã†â€œ  [LATIN CAPITAL LETTER G WITH HOOK]
						case '\u01E4': 
						// Ã‡Â¤  [LATIN CAPITAL LETTER G WITH STROKE]
						case '\u01E5': 
						// Ã‡Â¥  [LATIN SMALL LETTER G WITH STROKE]
						case '\u01E6': 
						// Ã‡Â¦  [LATIN CAPITAL LETTER G WITH CARON]
						case '\u01E7': 
						// Ã‡Â§  [LATIN SMALL LETTER G WITH CARON]
						case '\u01F4': 
						// Ã‡Â´  [LATIN CAPITAL LETTER G WITH ACUTE]
						case '\u0262': 
						// Ã‰Â¢  [LATIN LETTER SMALL CAPITAL G]
						case '\u029B': 
						// ÃŠâ€º  [LATIN LETTER SMALL CAPITAL G WITH HOOK]
						case '\u1E20': 
						// Ã¡Â¸Â   [LATIN CAPITAL LETTER G WITH MACRON]
						case '\u24BC': 
						// Ã¢â€™Â¼  [CIRCLED LATIN CAPITAL LETTER G]
						case '\uA77D': 
						// Ãªï¿½Â½  [LATIN CAPITAL LETTER INSULAR G]
						case '\uA77E': 
						// Ãªï¿½Â¾  [LATIN CAPITAL LETTER TURNED INSULAR G]
						case '\uFF27':  // Ã¯Â¼Â§  [FULLWIDTH LATIN CAPITAL LETTER G]
							output[outputPos++] = 'G';
							break;
						
						case '\u011D': 
						// Ã„ï¿½  [LATIN SMALL LETTER G WITH CIRCUMFLEX]
						case '\u011F': 
						// Ã„Å¸  [LATIN SMALL LETTER G WITH BREVE]
						case '\u0121': 
						// Ã„Â¡  [LATIN SMALL LETTER G WITH DOT ABOVE]
						case '\u0123': 
						// Ã„Â£  [LATIN SMALL LETTER G WITH CEDILLA]
						case '\u01F5': 
						// Ã‡Âµ  [LATIN SMALL LETTER G WITH ACUTE]
						case '\u0260': 
						// Ã‰Â   [LATIN SMALL LETTER G WITH HOOK]
						case '\u0261': 
						// Ã‰Â¡  [LATIN SMALL LETTER SCRIPT G]
						case '\u1D77': 
						// Ã¡ÂµÂ·  [LATIN SMALL LETTER TURNED G]
						case '\u1D79': 
						// Ã¡ÂµÂ¹  [LATIN SMALL LETTER INSULAR G]
						case '\u1D83': 
						// Ã¡Â¶Æ’  [LATIN SMALL LETTER G WITH PALATAL HOOK]
						case '\u1E21': 
						// Ã¡Â¸Â¡  [LATIN SMALL LETTER G WITH MACRON]
						case '\u24D6': 
						// Ã¢â€œâ€“  [CIRCLED LATIN SMALL LETTER G]
						case '\uA77F': 
						// Ãªï¿½Â¿  [LATIN SMALL LETTER TURNED INSULAR G]
						case '\uFF47':  // Ã¯Â½â€¡  [FULLWIDTH LATIN SMALL LETTER G]
							output[outputPos++] = 'g';
							break;
						
						case '\u24A2':  // Ã¢â€™Â¢  [PARENTHESIZED LATIN SMALL LETTER G]
							output[outputPos++] = '(';
							output[outputPos++] = 'g';
							output[outputPos++] = ')';
							break;
						
						case '\u0124': 
						// Ã„Â¤  [LATIN CAPITAL LETTER H WITH CIRCUMFLEX]
						case '\u0126': 
						// Ã„Â¦  [LATIN CAPITAL LETTER H WITH STROKE]
						case '\u021E': 
						// ÃˆÅ¾  [LATIN CAPITAL LETTER H WITH CARON]
						case '\u029C': 
						// ÃŠÅ“  [LATIN LETTER SMALL CAPITAL H]
						case '\u1E22': 
						// Ã¡Â¸Â¢  [LATIN CAPITAL LETTER H WITH DOT ABOVE]
						case '\u1E24': 
						// Ã¡Â¸Â¤  [LATIN CAPITAL LETTER H WITH DOT BELOW]
						case '\u1E26': 
						// Ã¡Â¸Â¦  [LATIN CAPITAL LETTER H WITH DIAERESIS]
						case '\u1E28': 
						// Ã¡Â¸Â¨  [LATIN CAPITAL LETTER H WITH CEDILLA]
						case '\u1E2A': 
						// Ã¡Â¸Âª  [LATIN CAPITAL LETTER H WITH BREVE BELOW]
						case '\u24BD': 
						// Ã¢â€™Â½  [CIRCLED LATIN CAPITAL LETTER H]
						case '\u2C67': 
						// Ã¢Â±Â§  [LATIN CAPITAL LETTER H WITH DESCENDER]
						case '\u2C75': 
						// Ã¢Â±Âµ  [LATIN CAPITAL LETTER HALF H]
						case '\uFF28':  // Ã¯Â¼Â¨  [FULLWIDTH LATIN CAPITAL LETTER H]
							output[outputPos++] = 'H';
							break;
						
						case '\u0125': 
						// Ã„Â¥  [LATIN SMALL LETTER H WITH CIRCUMFLEX]
						case '\u0127': 
						// Ã„Â§  [LATIN SMALL LETTER H WITH STROKE]
						case '\u021F': 
						// ÃˆÅ¸  [LATIN SMALL LETTER H WITH CARON]
						case '\u0265': 
						// Ã‰Â¥  [LATIN SMALL LETTER TURNED H]
						case '\u0266': 
						// Ã‰Â¦  [LATIN SMALL LETTER H WITH HOOK]
						case '\u02AE': 
						// ÃŠÂ®  [LATIN SMALL LETTER TURNED H WITH FISHHOOK]
						case '\u02AF': 
						// ÃŠÂ¯  [LATIN SMALL LETTER TURNED H WITH FISHHOOK AND TAIL]
						case '\u1E23': 
						// Ã¡Â¸Â£  [LATIN SMALL LETTER H WITH DOT ABOVE]
						case '\u1E25': 
						// Ã¡Â¸Â¥  [LATIN SMALL LETTER H WITH DOT BELOW]
						case '\u1E27': 
						// Ã¡Â¸Â§  [LATIN SMALL LETTER H WITH DIAERESIS]
						case '\u1E29': 
						// Ã¡Â¸Â©  [LATIN SMALL LETTER H WITH CEDILLA]
						case '\u1E2B': 
						// Ã¡Â¸Â«  [LATIN SMALL LETTER H WITH BREVE BELOW]
						case '\u1E96': 
						// Ã¡Âºâ€“  [LATIN SMALL LETTER H WITH LINE BELOW]
						case '\u24D7': 
						// Ã¢â€œâ€”  [CIRCLED LATIN SMALL LETTER H]
						case '\u2C68': 
						// Ã¢Â±Â¨  [LATIN SMALL LETTER H WITH DESCENDER]
						case '\u2C76': 
						// Ã¢Â±Â¶  [LATIN SMALL LETTER HALF H]
						case '\uFF48':  // Ã¯Â½Ë†  [FULLWIDTH LATIN SMALL LETTER H]
							output[outputPos++] = 'h';
							break;
						
						case '\u01F6':  // Ã‡Â¶  http://en.wikipedia.org/wiki/Hwair  [LATIN CAPITAL LETTER HWAIR]
							output[outputPos++] = 'H';
							output[outputPos++] = 'V';
							break;
						
						case '\u24A3':  // Ã¢â€™Â£  [PARENTHESIZED LATIN SMALL LETTER H]
							output[outputPos++] = '(';
							output[outputPos++] = 'h';
							output[outputPos++] = ')';
							break;
						
						case '\u0195':  // Ã†â€¢  [LATIN SMALL LETTER HV]
							output[outputPos++] = 'h';
							output[outputPos++] = 'v';
							break;
						
						case '\u00CC': 
						// ÃƒÅ’  [LATIN CAPITAL LETTER I WITH GRAVE]
						case '\u00CD': 
						// Ãƒï¿½  [LATIN CAPITAL LETTER I WITH ACUTE]
						case '\u00CE': 
						// ÃƒÅ½  [LATIN CAPITAL LETTER I WITH CIRCUMFLEX]
						case '\u00CF': 
						// Ãƒï¿½  [LATIN CAPITAL LETTER I WITH DIAERESIS]
						case '\u0128': 
						// Ã„Â¨  [LATIN CAPITAL LETTER I WITH TILDE]
						case '\u012A': 
						// Ã„Âª  [LATIN CAPITAL LETTER I WITH MACRON]
						case '\u012C': 
						// Ã„Â¬  [LATIN CAPITAL LETTER I WITH BREVE]
						case '\u012E': 
						// Ã„Â®  [LATIN CAPITAL LETTER I WITH OGONEK]
						case '\u0130': 
						// Ã„Â°  [LATIN CAPITAL LETTER I WITH DOT ABOVE]
						case '\u0196': 
						// Ã†â€“  [LATIN CAPITAL LETTER IOTA]
						case '\u0197': 
						// Ã†â€”  [LATIN CAPITAL LETTER I WITH STROKE]
						case '\u01CF': 
						// Ã‡ï¿½  [LATIN CAPITAL LETTER I WITH CARON]
						case '\u0208': 
						// ÃˆË†  [LATIN CAPITAL LETTER I WITH DOUBLE GRAVE]
						case '\u020A': 
						// ÃˆÅ   [LATIN CAPITAL LETTER I WITH INVERTED BREVE]
						case '\u026A': 
						// Ã‰Âª  [LATIN LETTER SMALL CAPITAL I]
						case '\u1D7B': 
						// Ã¡ÂµÂ»  [LATIN SMALL CAPITAL LETTER I WITH STROKE]
						case '\u1E2C': 
						// Ã¡Â¸Â¬  [LATIN CAPITAL LETTER I WITH TILDE BELOW]
						case '\u1E2E': 
						// Ã¡Â¸Â®  [LATIN CAPITAL LETTER I WITH DIAERESIS AND ACUTE]
						case '\u1EC8': 
						// Ã¡Â»Ë†  [LATIN CAPITAL LETTER I WITH HOOK ABOVE]
						case '\u1ECA': 
						// Ã¡Â»Å   [LATIN CAPITAL LETTER I WITH DOT BELOW]
						case '\u24BE': 
						// Ã¢â€™Â¾  [CIRCLED LATIN CAPITAL LETTER I]
						case '\uA7FE': 
						// ÃªÅ¸Â¾  [LATIN EPIGRAPHIC LETTER I LONGA]
						case '\uFF29':  // Ã¯Â¼Â©  [FULLWIDTH LATIN CAPITAL LETTER I]
							output[outputPos++] = 'I';
							break;
						
						case '\u00EC': 
						// ÃƒÂ¬  [LATIN SMALL LETTER I WITH GRAVE]
						case '\u00ED': 
						// ÃƒÂ­  [LATIN SMALL LETTER I WITH ACUTE]
						case '\u00EE': 
						// ÃƒÂ®  [LATIN SMALL LETTER I WITH CIRCUMFLEX]
						case '\u00EF': 
						// ÃƒÂ¯  [LATIN SMALL LETTER I WITH DIAERESIS]
						case '\u0129': 
						// Ã„Â©  [LATIN SMALL LETTER I WITH TILDE]
						case '\u012B': 
						// Ã„Â«  [LATIN SMALL LETTER I WITH MACRON]
						case '\u012D': 
						// Ã„Â­  [LATIN SMALL LETTER I WITH BREVE]
						case '\u012F': 
						// Ã„Â¯  [LATIN SMALL LETTER I WITH OGONEK]
						case '\u0131': 
						// Ã„Â±  [LATIN SMALL LETTER DOTLESS I]
						case '\u01D0': 
						// Ã‡ï¿½  [LATIN SMALL LETTER I WITH CARON]
						case '\u0209': 
						// Ãˆâ€°  [LATIN SMALL LETTER I WITH DOUBLE GRAVE]
						case '\u020B': 
						// Ãˆâ€¹  [LATIN SMALL LETTER I WITH INVERTED BREVE]
						case '\u0268': 
						// Ã‰Â¨  [LATIN SMALL LETTER I WITH STROKE]
						case '\u1D09': 
						// Ã¡Â´â€°  [LATIN SMALL LETTER TURNED I]
						case '\u1D62': 
						// Ã¡ÂµÂ¢  [LATIN SUBSCRIPT SMALL LETTER I]
						case '\u1D7C': 
						// Ã¡ÂµÂ¼  [LATIN SMALL LETTER IOTA WITH STROKE]
						case '\u1D96': 
						// Ã¡Â¶â€“  [LATIN SMALL LETTER I WITH RETROFLEX HOOK]
						case '\u1E2D': 
						// Ã¡Â¸Â­  [LATIN SMALL LETTER I WITH TILDE BELOW]
						case '\u1E2F': 
						// Ã¡Â¸Â¯  [LATIN SMALL LETTER I WITH DIAERESIS AND ACUTE]
						case '\u1EC9': 
						// Ã¡Â»â€°  [LATIN SMALL LETTER I WITH HOOK ABOVE]
						case '\u1ECB': 
						// Ã¡Â»â€¹  [LATIN SMALL LETTER I WITH DOT BELOW]
						case '\u2071': 
						// Ã¢ï¿½Â±  [SUPERSCRIPT LATIN SMALL LETTER I]
						case '\u24D8': 
						// Ã¢â€œËœ  [CIRCLED LATIN SMALL LETTER I]
						case '\uFF49':  // Ã¯Â½â€°  [FULLWIDTH LATIN SMALL LETTER I]
							output[outputPos++] = 'i';
							break;
						
						case '\u0132':  // Ã„Â²  [LATIN CAPITAL LIGATURE IJ]
							output[outputPos++] = 'I';
							output[outputPos++] = 'J';
							break;
						
						case '\u24A4':  // Ã¢â€™Â¤  [PARENTHESIZED LATIN SMALL LETTER I]
							output[outputPos++] = '(';
							output[outputPos++] = 'i';
							output[outputPos++] = ')';
							break;
						
						case '\u0133':  // Ã„Â³  [LATIN SMALL LIGATURE IJ]
							output[outputPos++] = 'i';
							output[outputPos++] = 'j';
							break;
						
						case '\u0134': 
						// Ã„Â´  [LATIN CAPITAL LETTER J WITH CIRCUMFLEX]
						case '\u0248': 
						// Ã‰Ë†  [LATIN CAPITAL LETTER J WITH STROKE]
						case '\u1D0A': 
						// Ã¡Â´Å   [LATIN LETTER SMALL CAPITAL J]
						case '\u24BF': 
						// Ã¢â€™Â¿  [CIRCLED LATIN CAPITAL LETTER J]
						case '\uFF2A':  // Ã¯Â¼Âª  [FULLWIDTH LATIN CAPITAL LETTER J]
							output[outputPos++] = 'J';
							break;
						
						case '\u0135': 
						// Ã„Âµ  [LATIN SMALL LETTER J WITH CIRCUMFLEX]
						case '\u01F0': 
						// Ã‡Â°  [LATIN SMALL LETTER J WITH CARON]
						case '\u0237': 
						// ÃˆÂ·  [LATIN SMALL LETTER DOTLESS J]
						case '\u0249': 
						// Ã‰â€°  [LATIN SMALL LETTER J WITH STROKE]
						case '\u025F': 
						// Ã‰Å¸  [LATIN SMALL LETTER DOTLESS J WITH STROKE]
						case '\u0284': 
						// ÃŠâ€ž  [LATIN SMALL LETTER DOTLESS J WITH STROKE AND HOOK]
						case '\u029D': 
						// ÃŠï¿½  [LATIN SMALL LETTER J WITH CROSSED-TAIL]
						case '\u24D9': 
						// Ã¢â€œâ„¢  [CIRCLED LATIN SMALL LETTER J]
						case '\u2C7C': 
						// Ã¢Â±Â¼  [LATIN SUBSCRIPT SMALL LETTER J]
						case '\uFF4A':  // Ã¯Â½Å   [FULLWIDTH LATIN SMALL LETTER J]
							output[outputPos++] = 'j';
							break;
						
						case '\u24A5':  // Ã¢â€™Â¥  [PARENTHESIZED LATIN SMALL LETTER J]
							output[outputPos++] = '(';
							output[outputPos++] = 'j';
							output[outputPos++] = ')';
							break;
						
						case '\u0136': 
						// Ã„Â¶  [LATIN CAPITAL LETTER K WITH CEDILLA]
						case '\u0198': 
						// Ã†Ëœ  [LATIN CAPITAL LETTER K WITH HOOK]
						case '\u01E8': 
						// Ã‡Â¨  [LATIN CAPITAL LETTER K WITH CARON]
						case '\u1D0B': 
						// Ã¡Â´â€¹  [LATIN LETTER SMALL CAPITAL K]
						case '\u1E30': 
						// Ã¡Â¸Â°  [LATIN CAPITAL LETTER K WITH ACUTE]
						case '\u1E32': 
						// Ã¡Â¸Â²  [LATIN CAPITAL LETTER K WITH DOT BELOW]
						case '\u1E34': 
						// Ã¡Â¸Â´  [LATIN CAPITAL LETTER K WITH LINE BELOW]
						case '\u24C0': 
						// Ã¢â€œâ‚¬  [CIRCLED LATIN CAPITAL LETTER K]
						case '\u2C69': 
						// Ã¢Â±Â©  [LATIN CAPITAL LETTER K WITH DESCENDER]
						case '\uA740': 
						// Ãªï¿½â‚¬  [LATIN CAPITAL LETTER K WITH STROKE]
						case '\uA742': 
						// Ãªï¿½â€š  [LATIN CAPITAL LETTER K WITH DIAGONAL STROKE]
						case '\uA744': 
						// Ãªï¿½â€ž  [LATIN CAPITAL LETTER K WITH STROKE AND DIAGONAL STROKE]
						case '\uFF2B':  // Ã¯Â¼Â«  [FULLWIDTH LATIN CAPITAL LETTER K]
							output[outputPos++] = 'K';
							break;
						
						case '\u0137': 
						// Ã„Â·  [LATIN SMALL LETTER K WITH CEDILLA]
						case '\u0199': 
						// Ã†â„¢  [LATIN SMALL LETTER K WITH HOOK]
						case '\u01E9': 
						// Ã‡Â©  [LATIN SMALL LETTER K WITH CARON]
						case '\u029E': 
						// ÃŠÅ¾  [LATIN SMALL LETTER TURNED K]
						case '\u1D84': 
						// Ã¡Â¶â€ž  [LATIN SMALL LETTER K WITH PALATAL HOOK]
						case '\u1E31': 
						// Ã¡Â¸Â±  [LATIN SMALL LETTER K WITH ACUTE]
						case '\u1E33': 
						// Ã¡Â¸Â³  [LATIN SMALL LETTER K WITH DOT BELOW]
						case '\u1E35': 
						// Ã¡Â¸Âµ  [LATIN SMALL LETTER K WITH LINE BELOW]
						case '\u24DA': 
						// Ã¢â€œÅ¡  [CIRCLED LATIN SMALL LETTER K]
						case '\u2C6A': 
						// Ã¢Â±Âª  [LATIN SMALL LETTER K WITH DESCENDER]
						case '\uA741': 
						// Ãªï¿½ï¿½  [LATIN SMALL LETTER K WITH STROKE]
						case '\uA743': 
						// Ãªï¿½Æ’  [LATIN SMALL LETTER K WITH DIAGONAL STROKE]
						case '\uA745': 
						// Ãªï¿½â€¦  [LATIN SMALL LETTER K WITH STROKE AND DIAGONAL STROKE]
						case '\uFF4B':  // Ã¯Â½â€¹  [FULLWIDTH LATIN SMALL LETTER K]
							output[outputPos++] = 'k';
							break;
						
						case '\u24A6':  // Ã¢â€™Â¦  [PARENTHESIZED LATIN SMALL LETTER K]
							output[outputPos++] = '(';
							output[outputPos++] = 'k';
							output[outputPos++] = ')';
							break;
						
						case '\u0139': 
						// Ã„Â¹  [LATIN CAPITAL LETTER L WITH ACUTE]
						case '\u013B': 
						// Ã„Â»  [LATIN CAPITAL LETTER L WITH CEDILLA]
						case '\u013D': 
						// Ã„Â½  [LATIN CAPITAL LETTER L WITH CARON]
						case '\u013F': 
						// Ã„Â¿  [LATIN CAPITAL LETTER L WITH MIDDLE DOT]
						case '\u0141': 
						// Ã…ï¿½  [LATIN CAPITAL LETTER L WITH STROKE]
						case '\u023D': 
						// ÃˆÂ½  [LATIN CAPITAL LETTER L WITH BAR]
						case '\u029F': 
						// ÃŠÅ¸  [LATIN LETTER SMALL CAPITAL L]
						case '\u1D0C': 
						// Ã¡Â´Å’  [LATIN LETTER SMALL CAPITAL L WITH STROKE]
						case '\u1E36': 
						// Ã¡Â¸Â¶  [LATIN CAPITAL LETTER L WITH DOT BELOW]
						case '\u1E38': 
						// Ã¡Â¸Â¸  [LATIN CAPITAL LETTER L WITH DOT BELOW AND MACRON]
						case '\u1E3A': 
						// Ã¡Â¸Âº  [LATIN CAPITAL LETTER L WITH LINE BELOW]
						case '\u1E3C': 
						// Ã¡Â¸Â¼  [LATIN CAPITAL LETTER L WITH CIRCUMFLEX BELOW]
						case '\u24C1': 
						// Ã¢â€œï¿½  [CIRCLED LATIN CAPITAL LETTER L]
						case '\u2C60': 
						// Ã¢Â±Â   [LATIN CAPITAL LETTER L WITH DOUBLE BAR]
						case '\u2C62': 
						// Ã¢Â±Â¢  [LATIN CAPITAL LETTER L WITH MIDDLE TILDE]
						case '\uA746': 
						// Ãªï¿½â€   [LATIN CAPITAL LETTER BROKEN L]
						case '\uA748': 
						// Ãªï¿½Ë†  [LATIN CAPITAL LETTER L WITH HIGH STROKE]
						case '\uA780': 
						// ÃªÅ¾â‚¬  [LATIN CAPITAL LETTER TURNED L]
						case '\uFF2C':  // Ã¯Â¼Â¬  [FULLWIDTH LATIN CAPITAL LETTER L]
							output[outputPos++] = 'L';
							break;
						
						case '\u013A': 
						// Ã„Âº  [LATIN SMALL LETTER L WITH ACUTE]
						case '\u013C': 
						// Ã„Â¼  [LATIN SMALL LETTER L WITH CEDILLA]
						case '\u013E': 
						// Ã„Â¾  [LATIN SMALL LETTER L WITH CARON]
						case '\u0140': 
						// Ã…â‚¬  [LATIN SMALL LETTER L WITH MIDDLE DOT]
						case '\u0142': 
						// Ã…â€š  [LATIN SMALL LETTER L WITH STROKE]
						case '\u019A': 
						// Ã†Å¡  [LATIN SMALL LETTER L WITH BAR]
						case '\u0234': 
						// ÃˆÂ´  [LATIN SMALL LETTER L WITH CURL]
						case '\u026B': 
						// Ã‰Â«  [LATIN SMALL LETTER L WITH MIDDLE TILDE]
						case '\u026C': 
						// Ã‰Â¬  [LATIN SMALL LETTER L WITH BELT]
						case '\u026D': 
						// Ã‰Â­  [LATIN SMALL LETTER L WITH RETROFLEX HOOK]
						case '\u1D85': 
						// Ã¡Â¶â€¦  [LATIN SMALL LETTER L WITH PALATAL HOOK]
						case '\u1E37': 
						// Ã¡Â¸Â·  [LATIN SMALL LETTER L WITH DOT BELOW]
						case '\u1E39': 
						// Ã¡Â¸Â¹  [LATIN SMALL LETTER L WITH DOT BELOW AND MACRON]
						case '\u1E3B': 
						// Ã¡Â¸Â»  [LATIN SMALL LETTER L WITH LINE BELOW]
						case '\u1E3D': 
						// Ã¡Â¸Â½  [LATIN SMALL LETTER L WITH CIRCUMFLEX BELOW]
						case '\u24DB': 
						// Ã¢â€œâ€º  [CIRCLED LATIN SMALL LETTER L]
						case '\u2C61': 
						// Ã¢Â±Â¡  [LATIN SMALL LETTER L WITH DOUBLE BAR]
						case '\uA747': 
						// Ãªï¿½â€¡  [LATIN SMALL LETTER BROKEN L]
						case '\uA749': 
						// Ãªï¿½â€°  [LATIN SMALL LETTER L WITH HIGH STROKE]
						case '\uA781': 
						// ÃªÅ¾ï¿½  [LATIN SMALL LETTER TURNED L]
						case '\uFF4C':  // Ã¯Â½Å’  [FULLWIDTH LATIN SMALL LETTER L]
							output[outputPos++] = 'l';
							break;
						
						case '\u01C7':  // Ã‡â€¡  [LATIN CAPITAL LETTER LJ]
							output[outputPos++] = 'L';
							output[outputPos++] = 'J';
							break;
						
						case '\u1EFA':  // Ã¡Â»Âº  [LATIN CAPITAL LETTER MIDDLE-WELSH LL]
							output[outputPos++] = 'L';
							output[outputPos++] = 'L';
							break;
						
						case '\u01C8':  // Ã‡Ë†  [LATIN CAPITAL LETTER L WITH SMALL LETTER J]
							output[outputPos++] = 'L';
							output[outputPos++] = 'j';
							break;
						
						case '\u24A7':  // Ã¢â€™Â§  [PARENTHESIZED LATIN SMALL LETTER L]
							output[outputPos++] = '(';
							output[outputPos++] = 'l';
							output[outputPos++] = ')';
							break;
						
						case '\u01C9':  // Ã‡â€°  [LATIN SMALL LETTER LJ]
							output[outputPos++] = 'l';
							output[outputPos++] = 'j';
							break;
						
						case '\u1EFB':  // Ã¡Â»Â»  [LATIN SMALL LETTER MIDDLE-WELSH LL]
							output[outputPos++] = 'l';
							output[outputPos++] = 'l';
							break;
						
						case '\u02AA':  // ÃŠÂª  [LATIN SMALL LETTER LS DIGRAPH]
							output[outputPos++] = 'l';
							output[outputPos++] = 's';
							break;
						
						case '\u02AB':  // ÃŠÂ«  [LATIN SMALL LETTER LZ DIGRAPH]
							output[outputPos++] = 'l';
							output[outputPos++] = 'z';
							break;
						
						case '\u019C': 
						// Ã†Å“  [LATIN CAPITAL LETTER TURNED M]
						case '\u1D0D': 
						// Ã¡Â´ï¿½  [LATIN LETTER SMALL CAPITAL M]
						case '\u1E3E': 
						// Ã¡Â¸Â¾  [LATIN CAPITAL LETTER M WITH ACUTE]
						case '\u1E40': 
						// Ã¡Â¹â‚¬  [LATIN CAPITAL LETTER M WITH DOT ABOVE]
						case '\u1E42': 
						// Ã¡Â¹â€š  [LATIN CAPITAL LETTER M WITH DOT BELOW]
						case '\u24C2': 
						// Ã¢â€œâ€š  [CIRCLED LATIN CAPITAL LETTER M]
						case '\u2C6E': 
						// Ã¢Â±Â®  [LATIN CAPITAL LETTER M WITH HOOK]
						case '\uA7FD': 
						// ÃªÅ¸Â½  [LATIN EPIGRAPHIC LETTER INVERTED M]
						case '\uA7FF': 
						// ÃªÅ¸Â¿  [LATIN EPIGRAPHIC LETTER ARCHAIC M]
						case '\uFF2D':  // Ã¯Â¼Â­  [FULLWIDTH LATIN CAPITAL LETTER M]
							output[outputPos++] = 'M';
							break;
						
						case '\u026F': 
						// Ã‰Â¯  [LATIN SMALL LETTER TURNED M]
						case '\u0270': 
						// Ã‰Â°  [LATIN SMALL LETTER TURNED M WITH LONG LEG]
						case '\u0271': 
						// Ã‰Â±  [LATIN SMALL LETTER M WITH HOOK]
						case '\u1D6F': 
						// Ã¡ÂµÂ¯  [LATIN SMALL LETTER M WITH MIDDLE TILDE]
						case '\u1D86': 
						// Ã¡Â¶â€   [LATIN SMALL LETTER M WITH PALATAL HOOK]
						case '\u1E3F': 
						// Ã¡Â¸Â¿  [LATIN SMALL LETTER M WITH ACUTE]
						case '\u1E41': 
						// Ã¡Â¹ï¿½  [LATIN SMALL LETTER M WITH DOT ABOVE]
						case '\u1E43': 
						// Ã¡Â¹Æ’  [LATIN SMALL LETTER M WITH DOT BELOW]
						case '\u24DC': 
						// Ã¢â€œÅ“  [CIRCLED LATIN SMALL LETTER M]
						case '\uFF4D':  // Ã¯Â½ï¿½  [FULLWIDTH LATIN SMALL LETTER M]
							output[outputPos++] = 'm';
							break;
						
						case '\u24A8':  // Ã¢â€™Â¨  [PARENTHESIZED LATIN SMALL LETTER M]
							output[outputPos++] = '(';
							output[outputPos++] = 'm';
							output[outputPos++] = ')';
							break;
						
						case '\u00D1': 
						// Ãƒâ€˜  [LATIN CAPITAL LETTER N WITH TILDE]
						case '\u0143': 
						// Ã…Æ’  [LATIN CAPITAL LETTER N WITH ACUTE]
						case '\u0145': 
						// Ã…â€¦  [LATIN CAPITAL LETTER N WITH CEDILLA]
						case '\u0147': 
						// Ã…â€¡  [LATIN CAPITAL LETTER N WITH CARON]
						case '\u014A': 
						// Ã…Å   http://en.wikipedia.org/wiki/Eng_(letter)  [LATIN CAPITAL LETTER ENG]
						case '\u019D': 
						// Ã†ï¿½  [LATIN CAPITAL LETTER N WITH LEFT HOOK]
						case '\u01F8': 
						// Ã‡Â¸  [LATIN CAPITAL LETTER N WITH GRAVE]
						case '\u0220': 
						// ÃˆÂ   [LATIN CAPITAL LETTER N WITH LONG RIGHT LEG]
						case '\u0274': 
						// Ã‰Â´  [LATIN LETTER SMALL CAPITAL N]
						case '\u1D0E': 
						// Ã¡Â´Å½  [LATIN LETTER SMALL CAPITAL REVERSED N]
						case '\u1E44': 
						// Ã¡Â¹â€ž  [LATIN CAPITAL LETTER N WITH DOT ABOVE]
						case '\u1E46': 
						// Ã¡Â¹â€   [LATIN CAPITAL LETTER N WITH DOT BELOW]
						case '\u1E48': 
						// Ã¡Â¹Ë†  [LATIN CAPITAL LETTER N WITH LINE BELOW]
						case '\u1E4A': 
						// Ã¡Â¹Å   [LATIN CAPITAL LETTER N WITH CIRCUMFLEX BELOW]
						case '\u24C3': 
						// Ã¢â€œÆ’  [CIRCLED LATIN CAPITAL LETTER N]
						case '\uFF2E':  // Ã¯Â¼Â®  [FULLWIDTH LATIN CAPITAL LETTER N]
							output[outputPos++] = 'N';
							break;
						
						case '\u00F1': 
						// ÃƒÂ±  [LATIN SMALL LETTER N WITH TILDE]
						case '\u0144': 
						// Ã…â€ž  [LATIN SMALL LETTER N WITH ACUTE]
						case '\u0146': 
						// Ã…â€   [LATIN SMALL LETTER N WITH CEDILLA]
						case '\u0148': 
						// Ã…Ë†  [LATIN SMALL LETTER N WITH CARON]
						case '\u0149': 
						// Ã…â€°  [LATIN SMALL LETTER N PRECEDED BY APOSTROPHE]
						case '\u014B': 
						// Ã…â€¹  http://en.wikipedia.org/wiki/Eng_(letter)  [LATIN SMALL LETTER ENG]
						case '\u019E': 
						// Ã†Å¾  [LATIN SMALL LETTER N WITH LONG RIGHT LEG]
						case '\u01F9': 
						// Ã‡Â¹  [LATIN SMALL LETTER N WITH GRAVE]
						case '\u0235': 
						// ÃˆÂµ  [LATIN SMALL LETTER N WITH CURL]
						case '\u0272': 
						// Ã‰Â²  [LATIN SMALL LETTER N WITH LEFT HOOK]
						case '\u0273': 
						// Ã‰Â³  [LATIN SMALL LETTER N WITH RETROFLEX HOOK]
						case '\u1D70': 
						// Ã¡ÂµÂ°  [LATIN SMALL LETTER N WITH MIDDLE TILDE]
						case '\u1D87': 
						// Ã¡Â¶â€¡  [LATIN SMALL LETTER N WITH PALATAL HOOK]
						case '\u1E45': 
						// Ã¡Â¹â€¦  [LATIN SMALL LETTER N WITH DOT ABOVE]
						case '\u1E47': 
						// Ã¡Â¹â€¡  [LATIN SMALL LETTER N WITH DOT BELOW]
						case '\u1E49': 
						// Ã¡Â¹â€°  [LATIN SMALL LETTER N WITH LINE BELOW]
						case '\u1E4B': 
						// Ã¡Â¹â€¹  [LATIN SMALL LETTER N WITH CIRCUMFLEX BELOW]
						case '\u207F': 
						// Ã¢ï¿½Â¿  [SUPERSCRIPT LATIN SMALL LETTER N]
						case '\u24DD': 
						// Ã¢â€œï¿½  [CIRCLED LATIN SMALL LETTER N]
						case '\uFF4E':  // Ã¯Â½Å½  [FULLWIDTH LATIN SMALL LETTER N]
							output[outputPos++] = 'n';
							break;
						
						case '\u01CA':  // Ã‡Å   [LATIN CAPITAL LETTER NJ]
							output[outputPos++] = 'N';
							output[outputPos++] = 'J';
							break;
						
						case '\u01CB':  // Ã‡â€¹  [LATIN CAPITAL LETTER N WITH SMALL LETTER J]
							output[outputPos++] = 'N';
							output[outputPos++] = 'j';
							break;
						
						case '\u24A9':  // Ã¢â€™Â©  [PARENTHESIZED LATIN SMALL LETTER N]
							output[outputPos++] = '(';
							output[outputPos++] = 'n';
							output[outputPos++] = ')';
							break;
						
						case '\u01CC':  // Ã‡Å’  [LATIN SMALL LETTER NJ]
							output[outputPos++] = 'n';
							output[outputPos++] = 'j';
							break;
						
						case '\u00D2': 
						// Ãƒâ€™  [LATIN CAPITAL LETTER O WITH GRAVE]
						case '\u00D3': 
						// Ãƒâ€œ  [LATIN CAPITAL LETTER O WITH ACUTE]
						case '\u00D4': 
						// Ãƒï¿½?  [LATIN CAPITAL LETTER O WITH CIRCUMFLEX]
						case '\u00D5': 
						// Ãƒâ€¢  [LATIN CAPITAL LETTER O WITH TILDE]
						case '\u00D6': 
						// Ãƒâ€“  [LATIN CAPITAL LETTER O WITH DIAERESIS]
						case '\u00D8': 
						// ÃƒËœ  [LATIN CAPITAL LETTER O WITH STROKE]
						case '\u014C': 
						// Ã…Å’  [LATIN CAPITAL LETTER O WITH MACRON]
						case '\u014E': 
						// Ã…Å½  [LATIN CAPITAL LETTER O WITH BREVE]
						case '\u0150': 
						// Ã…ï¿½  [LATIN CAPITAL LETTER O WITH DOUBLE ACUTE]
						case '\u0186': 
						// Ã†â€   [LATIN CAPITAL LETTER OPEN O]
						case '\u019F': 
						// Ã†Å¸  [LATIN CAPITAL LETTER O WITH MIDDLE TILDE]
						case '\u01A0': 
						// Ã†Â   [LATIN CAPITAL LETTER O WITH HORN]
						case '\u01D1': 
						// Ã‡â€˜  [LATIN CAPITAL LETTER O WITH CARON]
						case '\u01EA': 
						// Ã‡Âª  [LATIN CAPITAL LETTER O WITH OGONEK]
						case '\u01EC': 
						// Ã‡Â¬  [LATIN CAPITAL LETTER O WITH OGONEK AND MACRON]
						case '\u01FE': 
						// Ã‡Â¾  [LATIN CAPITAL LETTER O WITH STROKE AND ACUTE]
						case '\u020C': 
						// ÃˆÅ’  [LATIN CAPITAL LETTER O WITH DOUBLE GRAVE]
						case '\u020E': 
						// ÃˆÅ½  [LATIN CAPITAL LETTER O WITH INVERTED BREVE]
						case '\u022A': 
						// ÃˆÂª  [LATIN CAPITAL LETTER O WITH DIAERESIS AND MACRON]
						case '\u022C': 
						// ÃˆÂ¬  [LATIN CAPITAL LETTER O WITH TILDE AND MACRON]
						case '\u022E': 
						// ÃˆÂ®  [LATIN CAPITAL LETTER O WITH DOT ABOVE]
						case '\u0230': 
						// ÃˆÂ°  [LATIN CAPITAL LETTER O WITH DOT ABOVE AND MACRON]
						case '\u1D0F': 
						// Ã¡Â´ï¿½  [LATIN LETTER SMALL CAPITAL O]
						case '\u1D10': 
						// Ã¡Â´ï¿½  [LATIN LETTER SMALL CAPITAL OPEN O]
						case '\u1E4C': 
						// Ã¡Â¹Å’  [LATIN CAPITAL LETTER O WITH TILDE AND ACUTE]
						case '\u1E4E': 
						// Ã¡Â¹Å½  [LATIN CAPITAL LETTER O WITH TILDE AND DIAERESIS]
						case '\u1E50': 
						// Ã¡Â¹ï¿½  [LATIN CAPITAL LETTER O WITH MACRON AND GRAVE]
						case '\u1E52': 
						// Ã¡Â¹â€™  [LATIN CAPITAL LETTER O WITH MACRON AND ACUTE]
						case '\u1ECC': 
						// Ã¡Â»Å’  [LATIN CAPITAL LETTER O WITH DOT BELOW]
						case '\u1ECE': 
						// Ã¡Â»Å½  [LATIN CAPITAL LETTER O WITH HOOK ABOVE]
						case '\u1ED0': 
						// Ã¡Â»ï¿½  [LATIN CAPITAL LETTER O WITH CIRCUMFLEX AND ACUTE]
						case '\u1ED2': 
						// Ã¡Â»â€™  [LATIN CAPITAL LETTER O WITH CIRCUMFLEX AND GRAVE]
						case '\u1ED4': 
						// Ã¡Â»ï¿½?  [LATIN CAPITAL LETTER O WITH CIRCUMFLEX AND HOOK ABOVE]
						case '\u1ED6': 
						// Ã¡Â»â€“  [LATIN CAPITAL LETTER O WITH CIRCUMFLEX AND TILDE]
						case '\u1ED8': 
						// Ã¡Â»Ëœ  [LATIN CAPITAL LETTER O WITH CIRCUMFLEX AND DOT BELOW]
						case '\u1EDA': 
						// Ã¡Â»Å¡  [LATIN CAPITAL LETTER O WITH HORN AND ACUTE]
						case '\u1EDC': 
						// Ã¡Â»Å“  [LATIN CAPITAL LETTER O WITH HORN AND GRAVE]
						case '\u1EDE': 
						// Ã¡Â»Å¾  [LATIN CAPITAL LETTER O WITH HORN AND HOOK ABOVE]
						case '\u1EE0': 
						// Ã¡Â»Â   [LATIN CAPITAL LETTER O WITH HORN AND TILDE]
						case '\u1EE2': 
						// Ã¡Â»Â¢  [LATIN CAPITAL LETTER O WITH HORN AND DOT BELOW]
						case '\u24C4': 
						// Ã¢â€œâ€ž  [CIRCLED LATIN CAPITAL LETTER O]
						case '\uA74A': 
						// Ãªï¿½Å   [LATIN CAPITAL LETTER O WITH LONG STROKE OVERLAY]
						case '\uA74C': 
						// Ãªï¿½Å’  [LATIN CAPITAL LETTER O WITH LOOP]
						case '\uFF2F':  // Ã¯Â¼Â¯  [FULLWIDTH LATIN CAPITAL LETTER O]
							output[outputPos++] = 'O';
							break;
						
						case '\u00F2': 
						// ÃƒÂ²  [LATIN SMALL LETTER O WITH GRAVE]
						case '\u00F3': 
						// ÃƒÂ³  [LATIN SMALL LETTER O WITH ACUTE]
						case '\u00F4': 
						// ÃƒÂ´  [LATIN SMALL LETTER O WITH CIRCUMFLEX]
						case '\u00F5': 
						// ÃƒÂµ  [LATIN SMALL LETTER O WITH TILDE]
						case '\u00F6': 
						// ÃƒÂ¶  [LATIN SMALL LETTER O WITH DIAERESIS]
						case '\u00F8': 
						// ÃƒÂ¸  [LATIN SMALL LETTER O WITH STROKE]
						case '\u014D': 
						// Ã…ï¿½  [LATIN SMALL LETTER O WITH MACRON]
						case '\u014F': 
						// Ã…ï¿½  [LATIN SMALL LETTER O WITH BREVE]
						case '\u0151': 
						// Ã…â€˜  [LATIN SMALL LETTER O WITH DOUBLE ACUTE]
						case '\u01A1': 
						// Ã†Â¡  [LATIN SMALL LETTER O WITH HORN]
						case '\u01D2': 
						// Ã‡â€™  [LATIN SMALL LETTER O WITH CARON]
						case '\u01EB': 
						// Ã‡Â«  [LATIN SMALL LETTER O WITH OGONEK]
						case '\u01ED': 
						// Ã‡Â­  [LATIN SMALL LETTER O WITH OGONEK AND MACRON]
						case '\u01FF': 
						// Ã‡Â¿  [LATIN SMALL LETTER O WITH STROKE AND ACUTE]
						case '\u020D': 
						// Ãˆï¿½  [LATIN SMALL LETTER O WITH DOUBLE GRAVE]
						case '\u020F': 
						// Ãˆï¿½  [LATIN SMALL LETTER O WITH INVERTED BREVE]
						case '\u022B': 
						// ÃˆÂ«  [LATIN SMALL LETTER O WITH DIAERESIS AND MACRON]
						case '\u022D': 
						// ÃˆÂ­  [LATIN SMALL LETTER O WITH TILDE AND MACRON]
						case '\u022F': 
						// ÃˆÂ¯  [LATIN SMALL LETTER O WITH DOT ABOVE]
						case '\u0231': 
						// ÃˆÂ±  [LATIN SMALL LETTER O WITH DOT ABOVE AND MACRON]
						case '\u0254': 
						// Ã‰ï¿½?  [LATIN SMALL LETTER OPEN O]
						case '\u0275': 
						// Ã‰Âµ  [LATIN SMALL LETTER BARRED O]
						case '\u1D16': 
						// Ã¡Â´â€“  [LATIN SMALL LETTER TOP HALF O]
						case '\u1D17': 
						// Ã¡Â´â€”  [LATIN SMALL LETTER BOTTOM HALF O]
						case '\u1D97': 
						// Ã¡Â¶â€”  [LATIN SMALL LETTER OPEN O WITH RETROFLEX HOOK]
						case '\u1E4D': 
						// Ã¡Â¹ï¿½  [LATIN SMALL LETTER O WITH TILDE AND ACUTE]
						case '\u1E4F': 
						// Ã¡Â¹ï¿½  [LATIN SMALL LETTER O WITH TILDE AND DIAERESIS]
						case '\u1E51': 
						// Ã¡Â¹â€˜  [LATIN SMALL LETTER O WITH MACRON AND GRAVE]
						case '\u1E53': 
						// Ã¡Â¹â€œ  [LATIN SMALL LETTER O WITH MACRON AND ACUTE]
						case '\u1ECD': 
						// Ã¡Â»ï¿½  [LATIN SMALL LETTER O WITH DOT BELOW]
						case '\u1ECF': 
						// Ã¡Â»ï¿½  [LATIN SMALL LETTER O WITH HOOK ABOVE]
						case '\u1ED1': 
						// Ã¡Â»â€˜  [LATIN SMALL LETTER O WITH CIRCUMFLEX AND ACUTE]
						case '\u1ED3': 
						// Ã¡Â»â€œ  [LATIN SMALL LETTER O WITH CIRCUMFLEX AND GRAVE]
						case '\u1ED5': 
						// Ã¡Â»â€¢  [LATIN SMALL LETTER O WITH CIRCUMFLEX AND HOOK ABOVE]
						case '\u1ED7': 
						// Ã¡Â»â€”  [LATIN SMALL LETTER O WITH CIRCUMFLEX AND TILDE]
						case '\u1ED9': 
						// Ã¡Â»â„¢  [LATIN SMALL LETTER O WITH CIRCUMFLEX AND DOT BELOW]
						case '\u1EDB': 
						// Ã¡Â»â€º  [LATIN SMALL LETTER O WITH HORN AND ACUTE]
						case '\u1EDD': 
						// Ã¡Â»ï¿½  [LATIN SMALL LETTER O WITH HORN AND GRAVE]
						case '\u1EDF': 
						// Ã¡Â»Å¸  [LATIN SMALL LETTER O WITH HORN AND HOOK ABOVE]
						case '\u1EE1': 
						// Ã¡Â»Â¡  [LATIN SMALL LETTER O WITH HORN AND TILDE]
						case '\u1EE3': 
						// Ã¡Â»Â£  [LATIN SMALL LETTER O WITH HORN AND DOT BELOW]
						case '\u2092': 
						// Ã¢â€šâ€™  [LATIN SUBSCRIPT SMALL LETTER O]
						case '\u24DE': 
						// Ã¢â€œÅ¾  [CIRCLED LATIN SMALL LETTER O]
						case '\u2C7A': 
						// Ã¢Â±Âº  [LATIN SMALL LETTER O WITH LOW RING INSIDE]
						case '\uA74B': 
						// Ãªï¿½â€¹  [LATIN SMALL LETTER O WITH LONG STROKE OVERLAY]
						case '\uA74D': 
						// Ãªï¿½ï¿½  [LATIN SMALL LETTER O WITH LOOP]
						case '\uFF4F':  // Ã¯Â½ï¿½  [FULLWIDTH LATIN SMALL LETTER O]
							output[outputPos++] = 'o';
							break;
						
						case '\u0152': 
						// Ã…â€™  [LATIN CAPITAL LIGATURE OE]
						case '\u0276':  // Ã‰Â¶  [LATIN LETTER SMALL CAPITAL OE]
							output[outputPos++] = 'O';
							output[outputPos++] = 'E';
							break;
						
						case '\uA74E':  // Ãªï¿½Å½  [LATIN CAPITAL LETTER OO]
							output[outputPos++] = 'O';
							output[outputPos++] = 'O';
							break;
						
						case '\u0222': 
						// ÃˆÂ¢  http://en.wikipedia.org/wiki/OU  [LATIN CAPITAL LETTER OU]
						case '\u1D15':  // Ã¡Â´â€¢  [LATIN LETTER SMALL CAPITAL OU]
							output[outputPos++] = 'O';
							output[outputPos++] = 'U';
							break;
						
						case '\u24AA':  // Ã¢â€™Âª  [PARENTHESIZED LATIN SMALL LETTER O]
							output[outputPos++] = '(';
							output[outputPos++] = 'o';
							output[outputPos++] = ')';
							break;
						
						case '\u0153': 
						// Ã…â€œ  [LATIN SMALL LIGATURE OE]
						case '\u1D14':  // Ã¡Â´ï¿½?  [LATIN SMALL LETTER TURNED OE]
							output[outputPos++] = 'o';
							output[outputPos++] = 'e';
							break;
						
						case '\uA74F':  // Ãªï¿½ï¿½  [LATIN SMALL LETTER OO]
							output[outputPos++] = 'o';
							output[outputPos++] = 'o';
							break;
						
						case '\u0223':  // ÃˆÂ£  http://en.wikipedia.org/wiki/OU  [LATIN SMALL LETTER OU]
							output[outputPos++] = 'o';
							output[outputPos++] = 'u';
							break;
						
						case '\u01A4': 
						// Ã†Â¤  [LATIN CAPITAL LETTER P WITH HOOK]
						case '\u1D18': 
						// Ã¡Â´Ëœ  [LATIN LETTER SMALL CAPITAL P]
						case '\u1E54': 
						// Ã¡Â¹ï¿½?  [LATIN CAPITAL LETTER P WITH ACUTE]
						case '\u1E56': 
						// Ã¡Â¹â€“  [LATIN CAPITAL LETTER P WITH DOT ABOVE]
						case '\u24C5': 
						// Ã¢â€œâ€¦  [CIRCLED LATIN CAPITAL LETTER P]
						case '\u2C63': 
						// Ã¢Â±Â£  [LATIN CAPITAL LETTER P WITH STROKE]
						case '\uA750': 
						// Ãªï¿½ï¿½  [LATIN CAPITAL LETTER P WITH STROKE THROUGH DESCENDER]
						case '\uA752': 
						// Ãªï¿½â€™  [LATIN CAPITAL LETTER P WITH FLOURISH]
						case '\uA754': 
						// Ãªï¿½ï¿½?  [LATIN CAPITAL LETTER P WITH SQUIRREL TAIL]
						case '\uFF30':  // Ã¯Â¼Â°  [FULLWIDTH LATIN CAPITAL LETTER P]
							output[outputPos++] = 'P';
							break;
						
						case '\u01A5': 
						// Ã†Â¥  [LATIN SMALL LETTER P WITH HOOK]
						case '\u1D71': 
						// Ã¡ÂµÂ±  [LATIN SMALL LETTER P WITH MIDDLE TILDE]
						case '\u1D7D': 
						// Ã¡ÂµÂ½  [LATIN SMALL LETTER P WITH STROKE]
						case '\u1D88': 
						// Ã¡Â¶Ë†  [LATIN SMALL LETTER P WITH PALATAL HOOK]
						case '\u1E55': 
						// Ã¡Â¹â€¢  [LATIN SMALL LETTER P WITH ACUTE]
						case '\u1E57': 
						// Ã¡Â¹â€”  [LATIN SMALL LETTER P WITH DOT ABOVE]
						case '\u24DF': 
						// Ã¢â€œÅ¸  [CIRCLED LATIN SMALL LETTER P]
						case '\uA751': 
						// Ãªï¿½â€˜  [LATIN SMALL LETTER P WITH STROKE THROUGH DESCENDER]
						case '\uA753': 
						// Ãªï¿½â€œ  [LATIN SMALL LETTER P WITH FLOURISH]
						case '\uA755': 
						// Ãªï¿½â€¢  [LATIN SMALL LETTER P WITH SQUIRREL TAIL]
						case '\uA7FC': 
						// ÃªÅ¸Â¼  [LATIN EPIGRAPHIC LETTER REVERSED P]
						case '\uFF50':  // Ã¯Â½ï¿½  [FULLWIDTH LATIN SMALL LETTER P]
							output[outputPos++] = 'p';
							break;
						
						case '\u24AB':  // Ã¢â€™Â«  [PARENTHESIZED LATIN SMALL LETTER P]
							output[outputPos++] = '(';
							output[outputPos++] = 'p';
							output[outputPos++] = ')';
							break;
						
						case '\u024A': 
						// Ã‰Å   [LATIN CAPITAL LETTER SMALL Q WITH HOOK TAIL]
						case '\u24C6': 
						// Ã¢â€œâ€   [CIRCLED LATIN CAPITAL LETTER Q]
						case '\uA756': 
						// Ãªï¿½â€“  [LATIN CAPITAL LETTER Q WITH STROKE THROUGH DESCENDER]
						case '\uA758': 
						// Ãªï¿½Ëœ  [LATIN CAPITAL LETTER Q WITH DIAGONAL STROKE]
						case '\uFF31':  // Ã¯Â¼Â±  [FULLWIDTH LATIN CAPITAL LETTER Q]
							output[outputPos++] = 'Q';
							break;
						
						case '\u0138': 
						// Ã„Â¸  http://en.wikipedia.org/wiki/Kra_(letter)  [LATIN SMALL LETTER KRA]
						case '\u024B': 
						// Ã‰â€¹  [LATIN SMALL LETTER Q WITH HOOK TAIL]
						case '\u02A0': 
						// ÃŠÂ   [LATIN SMALL LETTER Q WITH HOOK]
						case '\u24E0': 
						// Ã¢â€œÂ   [CIRCLED LATIN SMALL LETTER Q]
						case '\uA757': 
						// Ãªï¿½â€”  [LATIN SMALL LETTER Q WITH STROKE THROUGH DESCENDER]
						case '\uA759': 
						// Ãªï¿½â„¢  [LATIN SMALL LETTER Q WITH DIAGONAL STROKE]
						case '\uFF51':  // Ã¯Â½â€˜  [FULLWIDTH LATIN SMALL LETTER Q]
							output[outputPos++] = 'q';
							break;
						
						case '\u24AC':  // Ã¢â€™Â¬  [PARENTHESIZED LATIN SMALL LETTER Q]
							output[outputPos++] = '(';
							output[outputPos++] = 'q';
							output[outputPos++] = ')';
							break;
						
						case '\u0239':  // ÃˆÂ¹  [LATIN SMALL LETTER QP DIGRAPH]
							output[outputPos++] = 'q';
							output[outputPos++] = 'p';
							break;
						
						case '\u0154': 
						// Ã…ï¿½?  [LATIN CAPITAL LETTER R WITH ACUTE]
						case '\u0156': 
						// Ã…â€“  [LATIN CAPITAL LETTER R WITH CEDILLA]
						case '\u0158': 
						// Ã…Ëœ  [LATIN CAPITAL LETTER R WITH CARON]
						case '\u0210': 
						// Ãˆâ€™  [LATIN CAPITAL LETTER R WITH DOUBLE GRAVE]
						case '\u0212': 
						// Ãˆâ€™  [LATIN CAPITAL LETTER R WITH INVERTED BREVE]
						case '\u024C': 
						// Ã‰Å’  [LATIN CAPITAL LETTER R WITH STROKE]
						case '\u0280': 
						// ÃŠâ‚¬  [LATIN LETTER SMALL CAPITAL R]
						case '\u0281': 
						// ÃŠï¿½  [LATIN LETTER SMALL CAPITAL INVERTED R]
						case '\u1D19': 
						// Ã¡Â´â„¢  [LATIN LETTER SMALL CAPITAL REVERSED R]
						case '\u1D1A': 
						// Ã¡Â´Å¡  [LATIN LETTER SMALL CAPITAL TURNED R]
						case '\u1E58': 
						// Ã¡Â¹Ëœ  [LATIN CAPITAL LETTER R WITH DOT ABOVE]
						case '\u1E5A': 
						// Ã¡Â¹Å¡  [LATIN CAPITAL LETTER R WITH DOT BELOW]
						case '\u1E5C': 
						// Ã¡Â¹Å“  [LATIN CAPITAL LETTER R WITH DOT BELOW AND MACRON]
						case '\u1E5E': 
						// Ã¡Â¹Å¾  [LATIN CAPITAL LETTER R WITH LINE BELOW]
						case '\u24C7': 
						// Ã¢â€œâ€¡  [CIRCLED LATIN CAPITAL LETTER R]
						case '\u2C64': 
						// Ã¢Â±Â¤  [LATIN CAPITAL LETTER R WITH TAIL]
						case '\uA75A': 
						// Ãªï¿½Å¡  [LATIN CAPITAL LETTER R ROTUNDA]
						case '\uA782': 
						// ÃªÅ¾â€š  [LATIN CAPITAL LETTER INSULAR R]
						case '\uFF32':  // Ã¯Â¼Â²  [FULLWIDTH LATIN CAPITAL LETTER R]
							output[outputPos++] = 'R';
							break;
						
						case '\u0155': 
						// Ã…â€¢  [LATIN SMALL LETTER R WITH ACUTE]
						case '\u0157': 
						// Ã…â€”  [LATIN SMALL LETTER R WITH CEDILLA]
						case '\u0159': 
						// Ã…â„¢  [LATIN SMALL LETTER R WITH CARON]
						case '\u0211': 
						// Ãˆâ€˜  [LATIN SMALL LETTER R WITH DOUBLE GRAVE]
						case '\u0213': 
						// Ãˆâ€œ  [LATIN SMALL LETTER R WITH INVERTED BREVE]
						case '\u024D': 
						// Ã‰ï¿½  [LATIN SMALL LETTER R WITH STROKE]
						case '\u027C': 
						// Ã‰Â¼  [LATIN SMALL LETTER R WITH LONG LEG]
						case '\u027D': 
						// Ã‰Â½  [LATIN SMALL LETTER R WITH TAIL]
						case '\u027E': 
						// Ã‰Â¾  [LATIN SMALL LETTER R WITH FISHHOOK]
						case '\u027F': 
						// Ã‰Â¿  [LATIN SMALL LETTER REVERSED R WITH FISHHOOK]
						case '\u1D63': 
						// Ã¡ÂµÂ£  [LATIN SUBSCRIPT SMALL LETTER R]
						case '\u1D72': 
						// Ã¡ÂµÂ²  [LATIN SMALL LETTER R WITH MIDDLE TILDE]
						case '\u1D73': 
						// Ã¡ÂµÂ³  [LATIN SMALL LETTER R WITH FISHHOOK AND MIDDLE TILDE]
						case '\u1D89': 
						// Ã¡Â¶â€°  [LATIN SMALL LETTER R WITH PALATAL HOOK]
						case '\u1E59': 
						// Ã¡Â¹â„¢  [LATIN SMALL LETTER R WITH DOT ABOVE]
						case '\u1E5B': 
						// Ã¡Â¹â€º  [LATIN SMALL LETTER R WITH DOT BELOW]
						case '\u1E5D': 
						// Ã¡Â¹ï¿½  [LATIN SMALL LETTER R WITH DOT BELOW AND MACRON]
						case '\u1E5F': 
						// Ã¡Â¹Å¸  [LATIN SMALL LETTER R WITH LINE BELOW]
						case '\u24E1': 
						// Ã¢â€œÂ¡  [CIRCLED LATIN SMALL LETTER R]
						case '\uA75B': 
						// Ãªï¿½â€º  [LATIN SMALL LETTER R ROTUNDA]
						case '\uA783': 
						// ÃªÅ¾Æ’  [LATIN SMALL LETTER INSULAR R]
						case '\uFF52':  // Ã¯Â½â€™  [FULLWIDTH LATIN SMALL LETTER R]
							output[outputPos++] = 'r';
							break;
						
						case '\u24AD':  // Ã¢â€™Â­  [PARENTHESIZED LATIN SMALL LETTER R]
							output[outputPos++] = '(';
							output[outputPos++] = 'r';
							output[outputPos++] = ')';
							break;
						
						case '\u015A': 
						// Ã…Å¡  [LATIN CAPITAL LETTER S WITH ACUTE]
						case '\u015C': 
						// Ã…Å“  [LATIN CAPITAL LETTER S WITH CIRCUMFLEX]
						case '\u015E': 
						// Ã…Å¾  [LATIN CAPITAL LETTER S WITH CEDILLA]
						case '\u0160': 
						// Ã…Â   [LATIN CAPITAL LETTER S WITH CARON]
						case '\u0218': 
						// ÃˆËœ  [LATIN CAPITAL LETTER S WITH COMMA BELOW]
						case '\u1E60': 
						// Ã¡Â¹Â   [LATIN CAPITAL LETTER S WITH DOT ABOVE]
						case '\u1E62': 
						// Ã¡Â¹Â¢  [LATIN CAPITAL LETTER S WITH DOT BELOW]
						case '\u1E64': 
						// Ã¡Â¹Â¤  [LATIN CAPITAL LETTER S WITH ACUTE AND DOT ABOVE]
						case '\u1E66': 
						// Ã¡Â¹Â¦  [LATIN CAPITAL LETTER S WITH CARON AND DOT ABOVE]
						case '\u1E68': 
						// Ã¡Â¹Â¨  [LATIN CAPITAL LETTER S WITH DOT BELOW AND DOT ABOVE]
						case '\u24C8': 
						// Ã¢â€œË†  [CIRCLED LATIN CAPITAL LETTER S]
						case '\uA731': 
						// ÃªÅ“Â±  [LATIN LETTER SMALL CAPITAL S]
						case '\uA785': 
						// ÃªÅ¾â€¦  [LATIN SMALL LETTER INSULAR S]
						case '\uFF33':  // Ã¯Â¼Â³  [FULLWIDTH LATIN CAPITAL LETTER S]
							output[outputPos++] = 'S';
							break;
						
						case '\u015B': 
						// Ã…â€º  [LATIN SMALL LETTER S WITH ACUTE]
						case '\u015D': 
						// Ã…ï¿½  [LATIN SMALL LETTER S WITH CIRCUMFLEX]
						case '\u015F': 
						// Ã…Å¸  [LATIN SMALL LETTER S WITH CEDILLA]
						case '\u0161': 
						// Ã…Â¡  [LATIN SMALL LETTER S WITH CARON]
						case '\u017F': 
						// Ã…Â¿  http://en.wikipedia.org/wiki/Long_S  [LATIN SMALL LETTER LONG S]
						case '\u0219': 
						// Ãˆâ„¢  [LATIN SMALL LETTER S WITH COMMA BELOW]
						case '\u023F': 
						// ÃˆÂ¿  [LATIN SMALL LETTER S WITH SWASH TAIL]
						case '\u0282': 
						// ÃŠâ€š  [LATIN SMALL LETTER S WITH HOOK]
						case '\u1D74': 
						// Ã¡ÂµÂ´  [LATIN SMALL LETTER S WITH MIDDLE TILDE]
						case '\u1D8A': 
						// Ã¡Â¶Å   [LATIN SMALL LETTER S WITH PALATAL HOOK]
						case '\u1E61': 
						// Ã¡Â¹Â¡  [LATIN SMALL LETTER S WITH DOT ABOVE]
						case '\u1E63': 
						// Ã¡Â¹Â£  [LATIN SMALL LETTER S WITH DOT BELOW]
						case '\u1E65': 
						// Ã¡Â¹Â¥  [LATIN SMALL LETTER S WITH ACUTE AND DOT ABOVE]
						case '\u1E67': 
						// Ã¡Â¹Â§  [LATIN SMALL LETTER S WITH CARON AND DOT ABOVE]
						case '\u1E69': 
						// Ã¡Â¹Â©  [LATIN SMALL LETTER S WITH DOT BELOW AND DOT ABOVE]
						case '\u1E9C': 
						// Ã¡ÂºÅ“  [LATIN SMALL LETTER LONG S WITH DIAGONAL STROKE]
						case '\u1E9D': 
						// Ã¡Âºï¿½  [LATIN SMALL LETTER LONG S WITH HIGH STROKE]
						case '\u24E2': 
						// Ã¢â€œÂ¢  [CIRCLED LATIN SMALL LETTER S]
						case '\uA784': 
						// ÃªÅ¾â€ž  [LATIN CAPITAL LETTER INSULAR S]
						case '\uFF53':  // Ã¯Â½â€œ  [FULLWIDTH LATIN SMALL LETTER S]
							output[outputPos++] = 's';
							break;
						
						case '\u1E9E':  // Ã¡ÂºÅ¾  [LATIN CAPITAL LETTER SHARP S]
							output[outputPos++] = 'S';
							output[outputPos++] = 'S';
							break;
						
						case '\u24AE':  // Ã¢â€™Â®  [PARENTHESIZED LATIN SMALL LETTER S]
							output[outputPos++] = '(';
							output[outputPos++] = 's';
							output[outputPos++] = ')';
							break;
						
						case '\u00DF':  // ÃƒÅ¸  [LATIN SMALL LETTER SHARP S]
							output[outputPos++] = 's';
							output[outputPos++] = 's';
							break;
						
						case '\uFB06':  // Ã¯Â¬â€   [LATIN SMALL LIGATURE ST]
							output[outputPos++] = 's';
							output[outputPos++] = 't';
							break;
						
						case '\u0162': 
						// Ã…Â¢  [LATIN CAPITAL LETTER T WITH CEDILLA]
						case '\u0164': 
						// Ã…Â¤  [LATIN CAPITAL LETTER T WITH CARON]
						case '\u0166': 
						// Ã…Â¦  [LATIN CAPITAL LETTER T WITH STROKE]
						case '\u01AC': 
						// Ã†Â¬  [LATIN CAPITAL LETTER T WITH HOOK]
						case '\u01AE': 
						// Ã†Â®  [LATIN CAPITAL LETTER T WITH RETROFLEX HOOK]
						case '\u021A': 
						// ÃˆÅ¡  [LATIN CAPITAL LETTER T WITH COMMA BELOW]
						case '\u023E': 
						// ÃˆÂ¾  [LATIN CAPITAL LETTER T WITH DIAGONAL STROKE]
						case '\u1D1B': 
						// Ã¡Â´â€º  [LATIN LETTER SMALL CAPITAL T]
						case '\u1E6A': 
						// Ã¡Â¹Âª  [LATIN CAPITAL LETTER T WITH DOT ABOVE]
						case '\u1E6C': 
						// Ã¡Â¹Â¬  [LATIN CAPITAL LETTER T WITH DOT BELOW]
						case '\u1E6E': 
						// Ã¡Â¹Â®  [LATIN CAPITAL LETTER T WITH LINE BELOW]
						case '\u1E70': 
						// Ã¡Â¹Â°  [LATIN CAPITAL LETTER T WITH CIRCUMFLEX BELOW]
						case '\u24C9': 
						// Ã¢â€œâ€°  [CIRCLED LATIN CAPITAL LETTER T]
						case '\uA786': 
						// ÃªÅ¾â€   [LATIN CAPITAL LETTER INSULAR T]
						case '\uFF34':  // Ã¯Â¼Â´  [FULLWIDTH LATIN CAPITAL LETTER T]
							output[outputPos++] = 'T';
							break;
						
						case '\u0163': 
						// Ã…Â£  [LATIN SMALL LETTER T WITH CEDILLA]
						case '\u0165': 
						// Ã…Â¥  [LATIN SMALL LETTER T WITH CARON]
						case '\u0167': 
						// Ã…Â§  [LATIN SMALL LETTER T WITH STROKE]
						case '\u01AB': 
						// Ã†Â«  [LATIN SMALL LETTER T WITH PALATAL HOOK]
						case '\u01AD': 
						// Ã†Â­  [LATIN SMALL LETTER T WITH HOOK]
						case '\u021B': 
						// Ãˆâ€º  [LATIN SMALL LETTER T WITH COMMA BELOW]
						case '\u0236': 
						// ÃˆÂ¶  [LATIN SMALL LETTER T WITH CURL]
						case '\u0287': 
						// ÃŠâ€¡  [LATIN SMALL LETTER TURNED T]
						case '\u0288': 
						// ÃŠË†  [LATIN SMALL LETTER T WITH RETROFLEX HOOK]
						case '\u1D75': 
						// Ã¡ÂµÂµ  [LATIN SMALL LETTER T WITH MIDDLE TILDE]
						case '\u1E6B': 
						// Ã¡Â¹Â«  [LATIN SMALL LETTER T WITH DOT ABOVE]
						case '\u1E6D': 
						// Ã¡Â¹Â­  [LATIN SMALL LETTER T WITH DOT BELOW]
						case '\u1E6F': 
						// Ã¡Â¹Â¯  [LATIN SMALL LETTER T WITH LINE BELOW]
						case '\u1E71': 
						// Ã¡Â¹Â±  [LATIN SMALL LETTER T WITH CIRCUMFLEX BELOW]
						case '\u1E97': 
						// Ã¡Âºâ€”  [LATIN SMALL LETTER T WITH DIAERESIS]
						case '\u24E3': 
						// Ã¢â€œÂ£  [CIRCLED LATIN SMALL LETTER T]
						case '\u2C66': 
						// Ã¢Â±Â¦  [LATIN SMALL LETTER T WITH DIAGONAL STROKE]
						case '\uFF54':  // Ã¯Â½ï¿½?  [FULLWIDTH LATIN SMALL LETTER T]
							output[outputPos++] = 't';
							break;
						
						case '\u00DE': 
						// ÃƒÅ¾  [LATIN CAPITAL LETTER THORN]
						case '\uA766':  // Ãªï¿½Â¦  [LATIN CAPITAL LETTER THORN WITH STROKE THROUGH DESCENDER]
							output[outputPos++] = 'T';
							output[outputPos++] = 'H';
							break;
						
						case '\uA728':  // ÃªÅ“Â¨  [LATIN CAPITAL LETTER TZ]
							output[outputPos++] = 'T';
							output[outputPos++] = 'Z';
							break;
						
						case '\u24AF':  // Ã¢â€™Â¯  [PARENTHESIZED LATIN SMALL LETTER T]
							output[outputPos++] = '(';
							output[outputPos++] = 't';
							output[outputPos++] = ')';
							break;
						
						case '\u02A8':  // ÃŠÂ¨  [LATIN SMALL LETTER TC DIGRAPH WITH CURL]
							output[outputPos++] = 't';
							output[outputPos++] = 'c';
							break;
						
						case '\u00FE': 
						// ÃƒÂ¾  [LATIN SMALL LETTER THORN]
						case '\u1D7A': 
						// Ã¡ÂµÂº  [LATIN SMALL LETTER TH WITH STRIKETHROUGH]
						case '\uA767':  // Ãªï¿½Â§  [LATIN SMALL LETTER THORN WITH STROKE THROUGH DESCENDER]
							output[outputPos++] = 't';
							output[outputPos++] = 'h';
							break;
						
						case '\u02A6':  // ÃŠÂ¦  [LATIN SMALL LETTER TS DIGRAPH]
							output[outputPos++] = 't';
							output[outputPos++] = 's';
							break;
						
						case '\uA729':  // ÃªÅ“Â©  [LATIN SMALL LETTER TZ]
							output[outputPos++] = 't';
							output[outputPos++] = 'z';
							break;
						
						case '\u00D9': 
						// Ãƒâ„¢  [LATIN CAPITAL LETTER U WITH GRAVE]
						case '\u00DA': 
						// ÃƒÅ¡  [LATIN CAPITAL LETTER U WITH ACUTE]
						case '\u00DB': 
						// Ãƒâ€º  [LATIN CAPITAL LETTER U WITH CIRCUMFLEX]
						case '\u00DC': 
						// ÃƒÅ“  [LATIN CAPITAL LETTER U WITH DIAERESIS]
						case '\u0168': 
						// Ã…Â¨  [LATIN CAPITAL LETTER U WITH TILDE]
						case '\u016A': 
						// Ã…Âª  [LATIN CAPITAL LETTER U WITH MACRON]
						case '\u016C': 
						// Ã…Â¬  [LATIN CAPITAL LETTER U WITH BREVE]
						case '\u016E': 
						// Ã…Â®  [LATIN CAPITAL LETTER U WITH RING ABOVE]
						case '\u0170': 
						// Ã…Â°  [LATIN CAPITAL LETTER U WITH DOUBLE ACUTE]
						case '\u0172': 
						// Ã…Â²  [LATIN CAPITAL LETTER U WITH OGONEK]
						case '\u01AF': 
						// Ã†Â¯  [LATIN CAPITAL LETTER U WITH HORN]
						case '\u01D3': 
						// Ã‡â€œ  [LATIN CAPITAL LETTER U WITH CARON]
						case '\u01D5': 
						// Ã‡â€¢  [LATIN CAPITAL LETTER U WITH DIAERESIS AND MACRON]
						case '\u01D7': 
						// Ã‡â€”  [LATIN CAPITAL LETTER U WITH DIAERESIS AND ACUTE]
						case '\u01D9': 
						// Ã‡â„¢  [LATIN CAPITAL LETTER U WITH DIAERESIS AND CARON]
						case '\u01DB': 
						// Ã‡â€º  [LATIN CAPITAL LETTER U WITH DIAERESIS AND GRAVE]
						case '\u0214': 
						// Ãˆï¿½?  [LATIN CAPITAL LETTER U WITH DOUBLE GRAVE]
						case '\u0216': 
						// Ãˆâ€“  [LATIN CAPITAL LETTER U WITH INVERTED BREVE]
						case '\u0244': 
						// Ã‰â€ž  [LATIN CAPITAL LETTER U BAR]
						case '\u1D1C': 
						// Ã¡Â´Å“  [LATIN LETTER SMALL CAPITAL U]
						case '\u1D7E': 
						// Ã¡ÂµÂ¾  [LATIN SMALL CAPITAL LETTER U WITH STROKE]
						case '\u1E72': 
						// Ã¡Â¹Â²  [LATIN CAPITAL LETTER U WITH DIAERESIS BELOW]
						case '\u1E74': 
						// Ã¡Â¹Â´  [LATIN CAPITAL LETTER U WITH TILDE BELOW]
						case '\u1E76': 
						// Ã¡Â¹Â¶  [LATIN CAPITAL LETTER U WITH CIRCUMFLEX BELOW]
						case '\u1E78': 
						// Ã¡Â¹Â¸  [LATIN CAPITAL LETTER U WITH TILDE AND ACUTE]
						case '\u1E7A': 
						// Ã¡Â¹Âº  [LATIN CAPITAL LETTER U WITH MACRON AND DIAERESIS]
						case '\u1EE4': 
						// Ã¡Â»Â¤  [LATIN CAPITAL LETTER U WITH DOT BELOW]
						case '\u1EE6': 
						// Ã¡Â»Â¦  [LATIN CAPITAL LETTER U WITH HOOK ABOVE]
						case '\u1EE8': 
						// Ã¡Â»Â¨  [LATIN CAPITAL LETTER U WITH HORN AND ACUTE]
						case '\u1EEA': 
						// Ã¡Â»Âª  [LATIN CAPITAL LETTER U WITH HORN AND GRAVE]
						case '\u1EEC': 
						// Ã¡Â»Â¬  [LATIN CAPITAL LETTER U WITH HORN AND HOOK ABOVE]
						case '\u1EEE': 
						// Ã¡Â»Â®  [LATIN CAPITAL LETTER U WITH HORN AND TILDE]
						case '\u1EF0': 
						// Ã¡Â»Â°  [LATIN CAPITAL LETTER U WITH HORN AND DOT BELOW]
						case '\u24CA': 
						// Ã¢â€œÅ   [CIRCLED LATIN CAPITAL LETTER U]
						case '\uFF35':  // Ã¯Â¼Âµ  [FULLWIDTH LATIN CAPITAL LETTER U]
							output[outputPos++] = 'U';
							break;
						
						case '\u00F9': 
						// ÃƒÂ¹  [LATIN SMALL LETTER U WITH GRAVE]
						case '\u00FA': 
						// ÃƒÂº  [LATIN SMALL LETTER U WITH ACUTE]
						case '\u00FB': 
						// ÃƒÂ»  [LATIN SMALL LETTER U WITH CIRCUMFLEX]
						case '\u00FC': 
						// ÃƒÂ¼  [LATIN SMALL LETTER U WITH DIAERESIS]
						case '\u0169': 
						// Ã…Â©  [LATIN SMALL LETTER U WITH TILDE]
						case '\u016B': 
						// Ã…Â«  [LATIN SMALL LETTER U WITH MACRON]
						case '\u016D': 
						// Ã…Â­  [LATIN SMALL LETTER U WITH BREVE]
						case '\u016F': 
						// Ã…Â¯  [LATIN SMALL LETTER U WITH RING ABOVE]
						case '\u0171': 
						// Ã…Â±  [LATIN SMALL LETTER U WITH DOUBLE ACUTE]
						case '\u0173': 
						// Ã…Â³  [LATIN SMALL LETTER U WITH OGONEK]
						case '\u01B0': 
						// Ã†Â°  [LATIN SMALL LETTER U WITH HORN]
						case '\u01D4': 
						// Ã‡ï¿½?  [LATIN SMALL LETTER U WITH CARON]
						case '\u01D6': 
						// Ã‡â€“  [LATIN SMALL LETTER U WITH DIAERESIS AND MACRON]
						case '\u01D8': 
						// Ã‡Ëœ  [LATIN SMALL LETTER U WITH DIAERESIS AND ACUTE]
						case '\u01DA': 
						// Ã‡Å¡  [LATIN SMALL LETTER U WITH DIAERESIS AND CARON]
						case '\u01DC': 
						// Ã‡Å“  [LATIN SMALL LETTER U WITH DIAERESIS AND GRAVE]
						case '\u0215': 
						// Ãˆâ€¢  [LATIN SMALL LETTER U WITH DOUBLE GRAVE]
						case '\u0217': 
						// Ãˆâ€”  [LATIN SMALL LETTER U WITH INVERTED BREVE]
						case '\u0289': 
						// ÃŠâ€°  [LATIN SMALL LETTER U BAR]
						case '\u1D64': 
						// Ã¡ÂµÂ¤  [LATIN SUBSCRIPT SMALL LETTER U]
						case '\u1D99': 
						// Ã¡Â¶â„¢  [LATIN SMALL LETTER U WITH RETROFLEX HOOK]
						case '\u1E73': 
						// Ã¡Â¹Â³  [LATIN SMALL LETTER U WITH DIAERESIS BELOW]
						case '\u1E75': 
						// Ã¡Â¹Âµ  [LATIN SMALL LETTER U WITH TILDE BELOW]
						case '\u1E77': 
						// Ã¡Â¹Â·  [LATIN SMALL LETTER U WITH CIRCUMFLEX BELOW]
						case '\u1E79': 
						// Ã¡Â¹Â¹  [LATIN SMALL LETTER U WITH TILDE AND ACUTE]
						case '\u1E7B': 
						// Ã¡Â¹Â»  [LATIN SMALL LETTER U WITH MACRON AND DIAERESIS]
						case '\u1EE5': 
						// Ã¡Â»Â¥  [LATIN SMALL LETTER U WITH DOT BELOW]
						case '\u1EE7': 
						// Ã¡Â»Â§  [LATIN SMALL LETTER U WITH HOOK ABOVE]
						case '\u1EE9': 
						// Ã¡Â»Â©  [LATIN SMALL LETTER U WITH HORN AND ACUTE]
						case '\u1EEB': 
						// Ã¡Â»Â«  [LATIN SMALL LETTER U WITH HORN AND GRAVE]
						case '\u1EED': 
						// Ã¡Â»Â­  [LATIN SMALL LETTER U WITH HORN AND HOOK ABOVE]
						case '\u1EEF': 
						// Ã¡Â»Â¯  [LATIN SMALL LETTER U WITH HORN AND TILDE]
						case '\u1EF1': 
						// Ã¡Â»Â±  [LATIN SMALL LETTER U WITH HORN AND DOT BELOW]
						case '\u24E4': 
						// Ã¢â€œÂ¤  [CIRCLED LATIN SMALL LETTER U]
						case '\uFF55':  // Ã¯Â½â€¢  [FULLWIDTH LATIN SMALL LETTER U]
							output[outputPos++] = 'u';
							break;
						
						case '\u24B0':  // Ã¢â€™Â°  [PARENTHESIZED LATIN SMALL LETTER U]
							output[outputPos++] = '(';
							output[outputPos++] = 'u';
							output[outputPos++] = ')';
							break;
						
						case '\u1D6B':  // Ã¡ÂµÂ«  [LATIN SMALL LETTER UE]
							output[outputPos++] = 'u';
							output[outputPos++] = 'e';
							break;
						
						case '\u01B2': 
						// Ã†Â²  [LATIN CAPITAL LETTER V WITH HOOK]
						case '\u0245': 
						// Ã‰â€¦  [LATIN CAPITAL LETTER TURNED V]
						case '\u1D20': 
						// Ã¡Â´Â   [LATIN LETTER SMALL CAPITAL V]
						case '\u1E7C': 
						// Ã¡Â¹Â¼  [LATIN CAPITAL LETTER V WITH TILDE]
						case '\u1E7E': 
						// Ã¡Â¹Â¾  [LATIN CAPITAL LETTER V WITH DOT BELOW]
						case '\u1EFC': 
						// Ã¡Â»Â¼  [LATIN CAPITAL LETTER MIDDLE-WELSH V]
						case '\u24CB': 
						// Ã¢â€œâ€¹  [CIRCLED LATIN CAPITAL LETTER V]
						case '\uA75E': 
						// Ãªï¿½Å¾  [LATIN CAPITAL LETTER V WITH DIAGONAL STROKE]
						case '\uA768': 
						// Ãªï¿½Â¨  [LATIN CAPITAL LETTER VEND]
						case '\uFF36':  // Ã¯Â¼Â¶  [FULLWIDTH LATIN CAPITAL LETTER V]
							output[outputPos++] = 'V';
							break;
						
						case '\u028B': 
						// ÃŠâ€¹  [LATIN SMALL LETTER V WITH HOOK]
						case '\u028C': 
						// ÃŠÅ’  [LATIN SMALL LETTER TURNED V]
						case '\u1D65': 
						// Ã¡ÂµÂ¥  [LATIN SUBSCRIPT SMALL LETTER V]
						case '\u1D8C': 
						// Ã¡Â¶Å’  [LATIN SMALL LETTER V WITH PALATAL HOOK]
						case '\u1E7D': 
						// Ã¡Â¹Â½  [LATIN SMALL LETTER V WITH TILDE]
						case '\u1E7F': 
						// Ã¡Â¹Â¿  [LATIN SMALL LETTER V WITH DOT BELOW]
						case '\u24E5': 
						// Ã¢â€œÂ¥  [CIRCLED LATIN SMALL LETTER V]
						case '\u2C71': 
						// Ã¢Â±Â±  [LATIN SMALL LETTER V WITH RIGHT HOOK]
						case '\u2C74': 
						// Ã¢Â±Â´  [LATIN SMALL LETTER V WITH CURL]
						case '\uA75F': 
						// Ãªï¿½Å¸  [LATIN SMALL LETTER V WITH DIAGONAL STROKE]
						case '\uFF56':  // Ã¯Â½â€“  [FULLWIDTH LATIN SMALL LETTER V]
							output[outputPos++] = 'v';
							break;
						
						case '\uA760':  // Ãªï¿½Â   [LATIN CAPITAL LETTER VY]
							output[outputPos++] = 'V';
							output[outputPos++] = 'Y';
							break;
						
						case '\u24B1':  // Ã¢â€™Â±  [PARENTHESIZED LATIN SMALL LETTER V]
							output[outputPos++] = '(';
							output[outputPos++] = 'v';
							output[outputPos++] = ')';
							break;
						
						case '\uA761':  // Ãªï¿½Â¡  [LATIN SMALL LETTER VY]
							output[outputPos++] = 'v';
							output[outputPos++] = 'y';
							break;
						
						case '\u0174': 
						// Ã…Â´  [LATIN CAPITAL LETTER W WITH CIRCUMFLEX]
						case '\u01F7': 
						// Ã‡Â·  http://en.wikipedia.org/wiki/Wynn  [LATIN CAPITAL LETTER WYNN]
						case '\u1D21': 
						// Ã¡Â´Â¡  [LATIN LETTER SMALL CAPITAL W]
						case '\u1E80': 
						// Ã¡Âºâ‚¬  [LATIN CAPITAL LETTER W WITH GRAVE]
						case '\u1E82': 
						// Ã¡Âºâ€š  [LATIN CAPITAL LETTER W WITH ACUTE]
						case '\u1E84': 
						// Ã¡Âºâ€ž  [LATIN CAPITAL LETTER W WITH DIAERESIS]
						case '\u1E86': 
						// Ã¡Âºâ€   [LATIN CAPITAL LETTER W WITH DOT ABOVE]
						case '\u1E88': 
						// Ã¡ÂºË†  [LATIN CAPITAL LETTER W WITH DOT BELOW]
						case '\u24CC': 
						// Ã¢â€œÅ’  [CIRCLED LATIN CAPITAL LETTER W]
						case '\u2C72': 
						// Ã¢Â±Â²  [LATIN CAPITAL LETTER W WITH HOOK]
						case '\uFF37':  // Ã¯Â¼Â·  [FULLWIDTH LATIN CAPITAL LETTER W]
							output[outputPos++] = 'W';
							break;
						
						case '\u0175': 
						// Ã…Âµ  [LATIN SMALL LETTER W WITH CIRCUMFLEX]
						case '\u01BF': 
						// Ã†Â¿  http://en.wikipedia.org/wiki/Wynn  [LATIN LETTER WYNN]
						case '\u028D': 
						// ÃŠï¿½  [LATIN SMALL LETTER TURNED W]
						case '\u1E81': 
						// Ã¡Âºï¿½  [LATIN SMALL LETTER W WITH GRAVE]
						case '\u1E83': 
						// Ã¡ÂºÆ’  [LATIN SMALL LETTER W WITH ACUTE]
						case '\u1E85': 
						// Ã¡Âºâ€¦  [LATIN SMALL LETTER W WITH DIAERESIS]
						case '\u1E87': 
						// Ã¡Âºâ€¡  [LATIN SMALL LETTER W WITH DOT ABOVE]
						case '\u1E89': 
						// Ã¡Âºâ€°  [LATIN SMALL LETTER W WITH DOT BELOW]
						case '\u1E98': 
						// Ã¡ÂºËœ  [LATIN SMALL LETTER W WITH RING ABOVE]
						case '\u24E6': 
						// Ã¢â€œÂ¦  [CIRCLED LATIN SMALL LETTER W]
						case '\u2C73': 
						// Ã¢Â±Â³  [LATIN SMALL LETTER W WITH HOOK]
						case '\uFF57':  // Ã¯Â½â€”  [FULLWIDTH LATIN SMALL LETTER W]
							output[outputPos++] = 'w';
							break;
						
						case '\u24B2':  // Ã¢â€™Â²  [PARENTHESIZED LATIN SMALL LETTER W]
							output[outputPos++] = '(';
							output[outputPos++] = 'w';
							output[outputPos++] = ')';
							break;
						
						case '\u1E8A': 
						// Ã¡ÂºÅ   [LATIN CAPITAL LETTER X WITH DOT ABOVE]
						case '\u1E8C': 
						// Ã¡ÂºÅ’  [LATIN CAPITAL LETTER X WITH DIAERESIS]
						case '\u24CD': 
						// Ã¢â€œï¿½  [CIRCLED LATIN CAPITAL LETTER X]
						case '\uFF38':  // Ã¯Â¼Â¸  [FULLWIDTH LATIN CAPITAL LETTER X]
							output[outputPos++] = 'X';
							break;
						
						case '\u1D8D': 
						// Ã¡Â¶ï¿½  [LATIN SMALL LETTER X WITH PALATAL HOOK]
						case '\u1E8B': 
						// Ã¡Âºâ€¹  [LATIN SMALL LETTER X WITH DOT ABOVE]
						case '\u1E8D': 
						// Ã¡Âºï¿½  [LATIN SMALL LETTER X WITH DIAERESIS]
						case '\u2093': 
						// Ã¢â€šâ€œ  [LATIN SUBSCRIPT SMALL LETTER X]
						case '\u24E7': 
						// Ã¢â€œÂ§  [CIRCLED LATIN SMALL LETTER X]
						case '\uFF58':  // Ã¯Â½Ëœ  [FULLWIDTH LATIN SMALL LETTER X]
							output[outputPos++] = 'x';
							break;
						
						case '\u24B3':  // Ã¢â€™Â³  [PARENTHESIZED LATIN SMALL LETTER X]
							output[outputPos++] = '(';
							output[outputPos++] = 'x';
							output[outputPos++] = ')';
							break;
						
						case '\u00DD': 
						// Ãƒï¿½  [LATIN CAPITAL LETTER Y WITH ACUTE]
						case '\u0176': 
						// Ã…Â¶  [LATIN CAPITAL LETTER Y WITH CIRCUMFLEX]
						case '\u0178': 
						// Ã…Â¸  [LATIN CAPITAL LETTER Y WITH DIAERESIS]
						case '\u01B3': 
						// Ã†Â³  [LATIN CAPITAL LETTER Y WITH HOOK]
						case '\u0232': 
						// ÃˆÂ²  [LATIN CAPITAL LETTER Y WITH MACRON]
						case '\u024E': 
						// Ã‰Å½  [LATIN CAPITAL LETTER Y WITH STROKE]
						case '\u028F': 
						// ÃŠï¿½  [LATIN LETTER SMALL CAPITAL Y]
						case '\u1E8E': 
						// Ã¡ÂºÅ½  [LATIN CAPITAL LETTER Y WITH DOT ABOVE]
						case '\u1EF2': 
						// Ã¡Â»Â²  [LATIN CAPITAL LETTER Y WITH GRAVE]
						case '\u1EF4': 
						// Ã¡Â»Â´  [LATIN CAPITAL LETTER Y WITH DOT BELOW]
						case '\u1EF6': 
						// Ã¡Â»Â¶  [LATIN CAPITAL LETTER Y WITH HOOK ABOVE]
						case '\u1EF8': 
						// Ã¡Â»Â¸  [LATIN CAPITAL LETTER Y WITH TILDE]
						case '\u1EFE': 
						// Ã¡Â»Â¾  [LATIN CAPITAL LETTER Y WITH LOOP]
						case '\u24CE': 
						// Ã¢â€œÅ½  [CIRCLED LATIN CAPITAL LETTER Y]
						case '\uFF39':  // Ã¯Â¼Â¹  [FULLWIDTH LATIN CAPITAL LETTER Y]
							output[outputPos++] = 'Y';
							break;
						
						case '\u00FD': 
						// ÃƒÂ½  [LATIN SMALL LETTER Y WITH ACUTE]
						case '\u00FF': 
						// ÃƒÂ¿  [LATIN SMALL LETTER Y WITH DIAERESIS]
						case '\u0177': 
						// Ã…Â·  [LATIN SMALL LETTER Y WITH CIRCUMFLEX]
						case '\u01B4': 
						// Ã†Â´  [LATIN SMALL LETTER Y WITH HOOK]
						case '\u0233': 
						// ÃˆÂ³  [LATIN SMALL LETTER Y WITH MACRON]
						case '\u024F': 
						// Ã‰ï¿½  [LATIN SMALL LETTER Y WITH STROKE]
						case '\u028E': 
						// ÃŠÅ½  [LATIN SMALL LETTER TURNED Y]
						case '\u1E8F': 
						// Ã¡Âºï¿½  [LATIN SMALL LETTER Y WITH DOT ABOVE]
						case '\u1E99': 
						// Ã¡Âºâ„¢  [LATIN SMALL LETTER Y WITH RING ABOVE]
						case '\u1EF3': 
						// Ã¡Â»Â³  [LATIN SMALL LETTER Y WITH GRAVE]
						case '\u1EF5': 
						// Ã¡Â»Âµ  [LATIN SMALL LETTER Y WITH DOT BELOW]
						case '\u1EF7': 
						// Ã¡Â»Â·  [LATIN SMALL LETTER Y WITH HOOK ABOVE]
						case '\u1EF9': 
						// Ã¡Â»Â¹  [LATIN SMALL LETTER Y WITH TILDE]
						case '\u1EFF': 
						// Ã¡Â»Â¿  [LATIN SMALL LETTER Y WITH LOOP]
						case '\u24E8': 
						// Ã¢â€œÂ¨  [CIRCLED LATIN SMALL LETTER Y]
						case '\uFF59':  // Ã¯Â½â„¢  [FULLWIDTH LATIN SMALL LETTER Y]
							output[outputPos++] = 'y';
							break;
						
						case '\u24B4':  // Ã¢â€™Â´  [PARENTHESIZED LATIN SMALL LETTER Y]
							output[outputPos++] = '(';
							output[outputPos++] = 'y';
							output[outputPos++] = ')';
							break;
						
						case '\u0179': 
						// Ã…Â¹  [LATIN CAPITAL LETTER Z WITH ACUTE]
						case '\u017B': 
						// Ã…Â»  [LATIN CAPITAL LETTER Z WITH DOT ABOVE]
						case '\u017D': 
						// Ã…Â½  [LATIN CAPITAL LETTER Z WITH CARON]
						case '\u01B5': 
						// Ã†Âµ  [LATIN CAPITAL LETTER Z WITH STROKE]
						case '\u021C': 
						// ÃˆÅ“  http://en.wikipedia.org/wiki/Yogh  [LATIN CAPITAL LETTER YOGH]
						case '\u0224': 
						// ÃˆÂ¤  [LATIN CAPITAL LETTER Z WITH HOOK]
						case '\u1D22': 
						// Ã¡Â´Â¢  [LATIN LETTER SMALL CAPITAL Z]
						case '\u1E90': 
						// Ã¡Âºï¿½  [LATIN CAPITAL LETTER Z WITH CIRCUMFLEX]
						case '\u1E92': 
						// Ã¡Âºâ€™  [LATIN CAPITAL LETTER Z WITH DOT BELOW]
						case '\u1E94': 
						// Ã¡Âºï¿½?  [LATIN CAPITAL LETTER Z WITH LINE BELOW]
						case '\u24CF': 
						// Ã¢â€œï¿½  [CIRCLED LATIN CAPITAL LETTER Z]
						case '\u2C6B': 
						// Ã¢Â±Â«  [LATIN CAPITAL LETTER Z WITH DESCENDER]
						case '\uA762': 
						// Ãªï¿½Â¢  [LATIN CAPITAL LETTER VISIGOTHIC Z]
						case '\uFF3A':  // Ã¯Â¼Âº  [FULLWIDTH LATIN CAPITAL LETTER Z]
							output[outputPos++] = 'Z';
							break;
						
						case '\u017A': 
						// Ã…Âº  [LATIN SMALL LETTER Z WITH ACUTE]
						case '\u017C': 
						// Ã…Â¼  [LATIN SMALL LETTER Z WITH DOT ABOVE]
						case '\u017E': 
						// Ã…Â¾  [LATIN SMALL LETTER Z WITH CARON]
						case '\u01B6': 
						// Ã†Â¶  [LATIN SMALL LETTER Z WITH STROKE]
						case '\u021D': 
						// Ãˆï¿½  http://en.wikipedia.org/wiki/Yogh  [LATIN SMALL LETTER YOGH]
						case '\u0225': 
						// ÃˆÂ¥  [LATIN SMALL LETTER Z WITH HOOK]
						case '\u0240': 
						// Ã‰â‚¬  [LATIN SMALL LETTER Z WITH SWASH TAIL]
						case '\u0290': 
						// ÃŠï¿½  [LATIN SMALL LETTER Z WITH RETROFLEX HOOK]
						case '\u0291': 
						// ÃŠâ€˜  [LATIN SMALL LETTER Z WITH CURL]
						case '\u1D76': 
						// Ã¡ÂµÂ¶  [LATIN SMALL LETTER Z WITH MIDDLE TILDE]
						case '\u1D8E': 
						// Ã¡Â¶Å½  [LATIN SMALL LETTER Z WITH PALATAL HOOK]
						case '\u1E91': 
						// Ã¡Âºâ€˜  [LATIN SMALL LETTER Z WITH CIRCUMFLEX]
						case '\u1E93': 
						// Ã¡Âºâ€œ  [LATIN SMALL LETTER Z WITH DOT BELOW]
						case '\u1E95': 
						// Ã¡Âºâ€¢  [LATIN SMALL LETTER Z WITH LINE BELOW]
						case '\u24E9': 
						// Ã¢â€œÂ©  [CIRCLED LATIN SMALL LETTER Z]
						case '\u2C6C': 
						// Ã¢Â±Â¬  [LATIN SMALL LETTER Z WITH DESCENDER]
						case '\uA763': 
						// Ãªï¿½Â£  [LATIN SMALL LETTER VISIGOTHIC Z]
						case '\uFF5A':  // Ã¯Â½Å¡  [FULLWIDTH LATIN SMALL LETTER Z]
							output[outputPos++] = 'z';
							break;
						
						case '\u24B5':  // Ã¢â€™Âµ  [PARENTHESIZED LATIN SMALL LETTER Z]
							output[outputPos++] = '(';
							output[outputPos++] = 'z';
							output[outputPos++] = ')';
							break;
						
						case '\u2070': 
						// Ã¢ï¿½Â°  [SUPERSCRIPT ZERO]
						case '\u2080': 
						// Ã¢â€šâ‚¬  [SUBSCRIPT ZERO]
						case '\u24EA': 
						// Ã¢â€œÂª  [CIRCLED DIGIT ZERO]
						case '\u24FF': 
						// Ã¢â€œÂ¿  [NEGATIVE CIRCLED DIGIT ZERO]
						case '\uFF10':  // Ã¯Â¼ï¿½  [FULLWIDTH DIGIT ZERO]
							output[outputPos++] = '0';
							break;
						
						case '\u00B9': 
						// Ã‚Â¹  [SUPERSCRIPT ONE]
						case '\u2081': 
						// Ã¢â€šï¿½  [SUBSCRIPT ONE]
						case '\u2460': 
						// Ã¢â€˜Â   [CIRCLED DIGIT ONE]
						case '\u24F5': 
						// Ã¢â€œÂµ  [DOUBLE CIRCLED DIGIT ONE]
						case '\u2776': 
						// Ã¢ï¿½Â¶  [DINGBAT NEGATIVE CIRCLED DIGIT ONE]
						case '\u2780': 
						// Ã¢Å¾â‚¬  [DINGBAT CIRCLED SANS-SERIF DIGIT ONE]
						case '\u278A': 
						// Ã¢Å¾Å   [DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT ONE]
						case '\uFF11':  // Ã¯Â¼â€˜  [FULLWIDTH DIGIT ONE]
							output[outputPos++] = '1';
							break;
						
						case '\u2488':  // Ã¢â€™Ë†  [DIGIT ONE FULL STOP]
							output[outputPos++] = '1';
							output[outputPos++] = '.';
							break;
						
						case '\u2474':  // Ã¢â€˜Â´  [PARENTHESIZED DIGIT ONE]
							output[outputPos++] = '(';
							output[outputPos++] = '1';
							output[outputPos++] = ')';
							break;
						
						case '\u00B2': 
						// Ã‚Â²  [SUPERSCRIPT TWO]
						case '\u2082': 
						// Ã¢â€šâ€š  [SUBSCRIPT TWO]
						case '\u2461': 
						// Ã¢â€˜Â¡  [CIRCLED DIGIT TWO]
						case '\u24F6': 
						// Ã¢â€œÂ¶  [DOUBLE CIRCLED DIGIT TWO]
						case '\u2777': 
						// Ã¢ï¿½Â·  [DINGBAT NEGATIVE CIRCLED DIGIT TWO]
						case '\u2781': 
						// Ã¢Å¾ï¿½  [DINGBAT CIRCLED SANS-SERIF DIGIT TWO]
						case '\u278B': 
						// Ã¢Å¾â€¹  [DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT TWO]
						case '\uFF12':  // Ã¯Â¼â€™  [FULLWIDTH DIGIT TWO]
							output[outputPos++] = '2';
							break;
						
						case '\u2489':  // Ã¢â€™â€°  [DIGIT TWO FULL STOP]
							output[outputPos++] = '2';
							output[outputPos++] = '.';
							break;
						
						case '\u2475':  // Ã¢â€˜Âµ  [PARENTHESIZED DIGIT TWO]
							output[outputPos++] = '(';
							output[outputPos++] = '2';
							output[outputPos++] = ')';
							break;
						
						case '\u00B3': 
						// Ã‚Â³  [SUPERSCRIPT THREE]
						case '\u2083': 
						// Ã¢â€šÆ’  [SUBSCRIPT THREE]
						case '\u2462': 
						// Ã¢â€˜Â¢  [CIRCLED DIGIT THREE]
						case '\u24F7': 
						// Ã¢â€œÂ·  [DOUBLE CIRCLED DIGIT THREE]
						case '\u2778': 
						// Ã¢ï¿½Â¸  [DINGBAT NEGATIVE CIRCLED DIGIT THREE]
						case '\u2782': 
						// Ã¢Å¾â€š  [DINGBAT CIRCLED SANS-SERIF DIGIT THREE]
						case '\u278C': 
						// Ã¢Å¾Å’  [DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT THREE]
						case '\uFF13':  // Ã¯Â¼â€œ  [FULLWIDTH DIGIT THREE]
							output[outputPos++] = '3';
							break;
						
						case '\u248A':  // Ã¢â€™Å   [DIGIT THREE FULL STOP]
							output[outputPos++] = '3';
							output[outputPos++] = '.';
							break;
						
						case '\u2476':  // Ã¢â€˜Â¶  [PARENTHESIZED DIGIT THREE]
							output[outputPos++] = '(';
							output[outputPos++] = '3';
							output[outputPos++] = ')';
							break;
						
						case '\u2074': 
						// Ã¢ï¿½Â´  [SUPERSCRIPT FOUR]
						case '\u2084': 
						// Ã¢â€šâ€ž  [SUBSCRIPT FOUR]
						case '\u2463': 
						// Ã¢â€˜Â£  [CIRCLED DIGIT FOUR]
						case '\u24F8': 
						// Ã¢â€œÂ¸  [DOUBLE CIRCLED DIGIT FOUR]
						case '\u2779': 
						// Ã¢ï¿½Â¹  [DINGBAT NEGATIVE CIRCLED DIGIT FOUR]
						case '\u2783': 
						// Ã¢Å¾Æ’  [DINGBAT CIRCLED SANS-SERIF DIGIT FOUR]
						case '\u278D': 
						// Ã¢Å¾ï¿½  [DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT FOUR]
						case '\uFF14':  // Ã¯Â¼ï¿½?  [FULLWIDTH DIGIT FOUR]
							output[outputPos++] = '4';
							break;
						
						case '\u248B':  // Ã¢â€™â€¹  [DIGIT FOUR FULL STOP]
							output[outputPos++] = '4';
							output[outputPos++] = '.';
							break;
						
						case '\u2477':  // Ã¢â€˜Â·  [PARENTHESIZED DIGIT FOUR]
							output[outputPos++] = '(';
							output[outputPos++] = '4';
							output[outputPos++] = ')';
							break;
						
						case '\u2075': 
						// Ã¢ï¿½Âµ  [SUPERSCRIPT FIVE]
						case '\u2085': 
						// Ã¢â€šâ€¦  [SUBSCRIPT FIVE]
						case '\u2464': 
						// Ã¢â€˜Â¤  [CIRCLED DIGIT FIVE]
						case '\u24F9': 
						// Ã¢â€œÂ¹  [DOUBLE CIRCLED DIGIT FIVE]
						case '\u277A': 
						// Ã¢ï¿½Âº  [DINGBAT NEGATIVE CIRCLED DIGIT FIVE]
						case '\u2784': 
						// Ã¢Å¾â€ž  [DINGBAT CIRCLED SANS-SERIF DIGIT FIVE]
						case '\u278E': 
						// Ã¢Å¾Å½  [DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT FIVE]
						case '\uFF15':  // Ã¯Â¼â€¢  [FULLWIDTH DIGIT FIVE]
							output[outputPos++] = '5';
							break;
						
						case '\u248C':  // Ã¢â€™Å’  [DIGIT FIVE FULL STOP]
							output[outputPos++] = '5';
							output[outputPos++] = '.';
							break;
						
						case '\u2478':  // Ã¢â€˜Â¸  [PARENTHESIZED DIGIT FIVE]
							output[outputPos++] = '(';
							output[outputPos++] = '5';
							output[outputPos++] = ')';
							break;
						
						case '\u2076': 
						// Ã¢ï¿½Â¶  [SUPERSCRIPT SIX]
						case '\u2086': 
						// Ã¢â€šâ€   [SUBSCRIPT SIX]
						case '\u2465': 
						// Ã¢â€˜Â¥  [CIRCLED DIGIT SIX]
						case '\u24FA': 
						// Ã¢â€œÂº  [DOUBLE CIRCLED DIGIT SIX]
						case '\u277B': 
						// Ã¢ï¿½Â»  [DINGBAT NEGATIVE CIRCLED DIGIT SIX]
						case '\u2785': 
						// Ã¢Å¾â€¦  [DINGBAT CIRCLED SANS-SERIF DIGIT SIX]
						case '\u278F': 
						// Ã¢Å¾ï¿½  [DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT SIX]
						case '\uFF16':  // Ã¯Â¼â€“  [FULLWIDTH DIGIT SIX]
							output[outputPos++] = '6';
							break;
						
						case '\u248D':  // Ã¢â€™ï¿½  [DIGIT SIX FULL STOP]
							output[outputPos++] = '6';
							output[outputPos++] = '.';
							break;
						
						case '\u2479':  // Ã¢â€˜Â¹  [PARENTHESIZED DIGIT SIX]
							output[outputPos++] = '(';
							output[outputPos++] = '6';
							output[outputPos++] = ')';
							break;
						
						case '\u2077': 
						// Ã¢ï¿½Â·  [SUPERSCRIPT SEVEN]
						case '\u2087': 
						// Ã¢â€šâ€¡  [SUBSCRIPT SEVEN]
						case '\u2466': 
						// Ã¢â€˜Â¦  [CIRCLED DIGIT SEVEN]
						case '\u24FB': 
						// Ã¢â€œÂ»  [DOUBLE CIRCLED DIGIT SEVEN]
						case '\u277C': 
						// Ã¢ï¿½Â¼  [DINGBAT NEGATIVE CIRCLED DIGIT SEVEN]
						case '\u2786': 
						// Ã¢Å¾â€   [DINGBAT CIRCLED SANS-SERIF DIGIT SEVEN]
						case '\u2790': 
						// Ã¢Å¾ï¿½  [DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT SEVEN]
						case '\uFF17':  // Ã¯Â¼â€”  [FULLWIDTH DIGIT SEVEN]
							output[outputPos++] = '7';
							break;
						
						case '\u248E':  // Ã¢â€™Å½  [DIGIT SEVEN FULL STOP]
							output[outputPos++] = '7';
							output[outputPos++] = '.';
							break;
						
						case '\u247A':  // Ã¢â€˜Âº  [PARENTHESIZED DIGIT SEVEN]
							output[outputPos++] = '(';
							output[outputPos++] = '7';
							output[outputPos++] = ')';
							break;
						
						case '\u2078': 
						// Ã¢ï¿½Â¸  [SUPERSCRIPT EIGHT]
						case '\u2088': 
						// Ã¢â€šË†  [SUBSCRIPT EIGHT]
						case '\u2467': 
						// Ã¢â€˜Â§  [CIRCLED DIGIT EIGHT]
						case '\u24FC': 
						// Ã¢â€œÂ¼  [DOUBLE CIRCLED DIGIT EIGHT]
						case '\u277D': 
						// Ã¢ï¿½Â½  [DINGBAT NEGATIVE CIRCLED DIGIT EIGHT]
						case '\u2787': 
						// Ã¢Å¾â€¡  [DINGBAT CIRCLED SANS-SERIF DIGIT EIGHT]
						case '\u2791': 
						// Ã¢Å¾â€˜  [DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT EIGHT]
						case '\uFF18':  // Ã¯Â¼Ëœ  [FULLWIDTH DIGIT EIGHT]
							output[outputPos++] = '8';
							break;
						
						case '\u248F':  // Ã¢â€™ï¿½  [DIGIT EIGHT FULL STOP]
							output[outputPos++] = '8';
							output[outputPos++] = '.';
							break;
						
						case '\u247B':  // Ã¢â€˜Â»  [PARENTHESIZED DIGIT EIGHT]
							output[outputPos++] = '(';
							output[outputPos++] = '8';
							output[outputPos++] = ')';
							break;
						
						case '\u2079': 
						// Ã¢ï¿½Â¹  [SUPERSCRIPT NINE]
						case '\u2089': 
						// Ã¢â€šâ€°  [SUBSCRIPT NINE]
						case '\u2468': 
						// Ã¢â€˜Â¨  [CIRCLED DIGIT NINE]
						case '\u24FD': 
						// Ã¢â€œÂ½  [DOUBLE CIRCLED DIGIT NINE]
						case '\u277E': 
						// Ã¢ï¿½Â¾  [DINGBAT NEGATIVE CIRCLED DIGIT NINE]
						case '\u2788': 
						// Ã¢Å¾Ë†  [DINGBAT CIRCLED SANS-SERIF DIGIT NINE]
						case '\u2792': 
						// Ã¢Å¾â€™  [DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT NINE]
						case '\uFF19':  // Ã¯Â¼â„¢  [FULLWIDTH DIGIT NINE]
							output[outputPos++] = '9';
							break;
						
						case '\u2490':  // Ã¢â€™ï¿½  [DIGIT NINE FULL STOP]
							output[outputPos++] = '9';
							output[outputPos++] = '.';
							break;
						
						case '\u247C':  // Ã¢â€˜Â¼  [PARENTHESIZED DIGIT NINE]
							output[outputPos++] = '(';
							output[outputPos++] = '9';
							output[outputPos++] = ')';
							break;
						
						case '\u2469': 
						// Ã¢â€˜Â©  [CIRCLED NUMBER TEN]
						case '\u24FE': 
						// Ã¢â€œÂ¾  [DOUBLE CIRCLED NUMBER TEN]
						case '\u277F': 
						// Ã¢ï¿½Â¿  [DINGBAT NEGATIVE CIRCLED NUMBER TEN]
						case '\u2789': 
						// Ã¢Å¾â€°  [DINGBAT CIRCLED SANS-SERIF NUMBER TEN]
						case '\u2793':  // Ã¢Å¾â€œ  [DINGBAT NEGATIVE CIRCLED SANS-SERIF NUMBER TEN]
							output[outputPos++] = '1';
							output[outputPos++] = '0';
							break;
						
						case '\u2491':  // Ã¢â€™â€˜  [NUMBER TEN FULL STOP]
							output[outputPos++] = '1';
							output[outputPos++] = '0';
							output[outputPos++] = '.';
							break;
						
						case '\u247D':  // Ã¢â€˜Â½  [PARENTHESIZED NUMBER TEN]
							output[outputPos++] = '(';
							output[outputPos++] = '1';
							output[outputPos++] = '0';
							output[outputPos++] = ')';
							break;
						
						case '\u246A': 
						// Ã¢â€˜Âª  [CIRCLED NUMBER ELEVEN]
						case '\u24EB':  // Ã¢â€œÂ«  [NEGATIVE CIRCLED NUMBER ELEVEN]
							output[outputPos++] = '1';
							output[outputPos++] = '1';
							break;
						
						case '\u2492':  // Ã¢â€™â€™  [NUMBER ELEVEN FULL STOP]
							output[outputPos++] = '1';
							output[outputPos++] = '1';
							output[outputPos++] = '.';
							break;
						
						case '\u247E':  // Ã¢â€˜Â¾  [PARENTHESIZED NUMBER ELEVEN]
							output[outputPos++] = '(';
							output[outputPos++] = '1';
							output[outputPos++] = '1';
							output[outputPos++] = ')';
							break;
						
						case '\u246B': 
						// Ã¢â€˜Â«  [CIRCLED NUMBER TWELVE]
						case '\u24EC':  // Ã¢â€œÂ¬  [NEGATIVE CIRCLED NUMBER TWELVE]
							output[outputPos++] = '1';
							output[outputPos++] = '2';
							break;
						
						case '\u2493':  // Ã¢â€™â€œ  [NUMBER TWELVE FULL STOP]
							output[outputPos++] = '1';
							output[outputPos++] = '2';
							output[outputPos++] = '.';
							break;
						
						case '\u247F':  // Ã¢â€˜Â¿  [PARENTHESIZED NUMBER TWELVE]
							output[outputPos++] = '(';
							output[outputPos++] = '1';
							output[outputPos++] = '2';
							output[outputPos++] = ')';
							break;
						
						case '\u246C': 
						// Ã¢â€˜Â¬  [CIRCLED NUMBER THIRTEEN]
						case '\u24ED':  // Ã¢â€œÂ­  [NEGATIVE CIRCLED NUMBER THIRTEEN]
							output[outputPos++] = '1';
							output[outputPos++] = '3';
							break;
						
						case '\u2494':  // Ã¢â€™ï¿½?  [NUMBER THIRTEEN FULL STOP]
							output[outputPos++] = '1';
							output[outputPos++] = '3';
							output[outputPos++] = '.';
							break;
						
						case '\u2480':  // Ã¢â€™â‚¬  [PARENTHESIZED NUMBER THIRTEEN]
							output[outputPos++] = '(';
							output[outputPos++] = '1';
							output[outputPos++] = '3';
							output[outputPos++] = ')';
							break;
						
						case '\u246D': 
						// Ã¢â€˜Â­  [CIRCLED NUMBER FOURTEEN]
						case '\u24EE':  // Ã¢â€œÂ®  [NEGATIVE CIRCLED NUMBER FOURTEEN]
							output[outputPos++] = '1';
							output[outputPos++] = '4';
							break;
						
						case '\u2495':  // Ã¢â€™â€¢  [NUMBER FOURTEEN FULL STOP]
							output[outputPos++] = '1';
							output[outputPos++] = '4';
							output[outputPos++] = '.';
							break;
						
						case '\u2481':  // Ã¢â€™ï¿½  [PARENTHESIZED NUMBER FOURTEEN]
							output[outputPos++] = '(';
							output[outputPos++] = '1';
							output[outputPos++] = '4';
							output[outputPos++] = ')';
							break;
						
						case '\u246E': 
						// Ã¢â€˜Â®  [CIRCLED NUMBER FIFTEEN]
						case '\u24EF':  // Ã¢â€œÂ¯  [NEGATIVE CIRCLED NUMBER FIFTEEN]
							output[outputPos++] = '1';
							output[outputPos++] = '5';
							break;
						
						case '\u2496':  // Ã¢â€™â€“  [NUMBER FIFTEEN FULL STOP]
							output[outputPos++] = '1';
							output[outputPos++] = '5';
							output[outputPos++] = '.';
							break;
						
						case '\u2482':  // Ã¢â€™â€š  [PARENTHESIZED NUMBER FIFTEEN]
							output[outputPos++] = '(';
							output[outputPos++] = '1';
							output[outputPos++] = '5';
							output[outputPos++] = ')';
							break;
						
						case '\u246F': 
						// Ã¢â€˜Â¯  [CIRCLED NUMBER SIXTEEN]
						case '\u24F0':  // Ã¢â€œÂ°  [NEGATIVE CIRCLED NUMBER SIXTEEN]
							output[outputPos++] = '1';
							output[outputPos++] = '6';
							break;
						
						case '\u2497':  // Ã¢â€™â€”  [NUMBER SIXTEEN FULL STOP]
							output[outputPos++] = '1';
							output[outputPos++] = '6';
							output[outputPos++] = '.';
							break;
						
						case '\u2483':  // Ã¢â€™Æ’  [PARENTHESIZED NUMBER SIXTEEN]
							output[outputPos++] = '(';
							output[outputPos++] = '1';
							output[outputPos++] = '6';
							output[outputPos++] = ')';
							break;
						
						case '\u2470': 
						// Ã¢â€˜Â°  [CIRCLED NUMBER SEVENTEEN]
						case '\u24F1':  // Ã¢â€œÂ±  [NEGATIVE CIRCLED NUMBER SEVENTEEN]
							output[outputPos++] = '1';
							output[outputPos++] = '7';
							break;
						
						case '\u2498':  // Ã¢â€™Ëœ  [NUMBER SEVENTEEN FULL STOP]
							output[outputPos++] = '1';
							output[outputPos++] = '7';
							output[outputPos++] = '.';
							break;
						
						case '\u2484':  // Ã¢â€™â€ž  [PARENTHESIZED NUMBER SEVENTEEN]
							output[outputPos++] = '(';
							output[outputPos++] = '1';
							output[outputPos++] = '7';
							output[outputPos++] = ')';
							break;
						
						case '\u2471': 
						// Ã¢â€˜Â±  [CIRCLED NUMBER EIGHTEEN]
						case '\u24F2':  // Ã¢â€œÂ²  [NEGATIVE CIRCLED NUMBER EIGHTEEN]
							output[outputPos++] = '1';
							output[outputPos++] = '8';
							break;
						
						case '\u2499':  // Ã¢â€™â„¢  [NUMBER EIGHTEEN FULL STOP]
							output[outputPos++] = '1';
							output[outputPos++] = '8';
							output[outputPos++] = '.';
							break;
						
						case '\u2485':  // Ã¢â€™â€¦  [PARENTHESIZED NUMBER EIGHTEEN]
							output[outputPos++] = '(';
							output[outputPos++] = '1';
							output[outputPos++] = '8';
							output[outputPos++] = ')';
							break;
						
						case '\u2472': 
						// Ã¢â€˜Â²  [CIRCLED NUMBER NINETEEN]
						case '\u24F3':  // Ã¢â€œÂ³  [NEGATIVE CIRCLED NUMBER NINETEEN]
							output[outputPos++] = '1';
							output[outputPos++] = '9';
							break;
						
						case '\u249A':  // Ã¢â€™Å¡  [NUMBER NINETEEN FULL STOP]
							output[outputPos++] = '1';
							output[outputPos++] = '9';
							output[outputPos++] = '.';
							break;
						
						case '\u2486':  // Ã¢â€™â€   [PARENTHESIZED NUMBER NINETEEN]
							output[outputPos++] = '(';
							output[outputPos++] = '1';
							output[outputPos++] = '9';
							output[outputPos++] = ')';
							break;
						
						case '\u2473': 
						// Ã¢â€˜Â³  [CIRCLED NUMBER TWENTY]
						case '\u24F4':  // Ã¢â€œÂ´  [NEGATIVE CIRCLED NUMBER TWENTY]
							output[outputPos++] = '2';
							output[outputPos++] = '0';
							break;
						
						case '\u249B':  // Ã¢â€™â€º  [NUMBER TWENTY FULL STOP]
							output[outputPos++] = '2';
							output[outputPos++] = '0';
							output[outputPos++] = '.';
							break;
						
						case '\u2487':  // Ã¢â€™â€¡  [PARENTHESIZED NUMBER TWENTY]
							output[outputPos++] = '(';
							output[outputPos++] = '2';
							output[outputPos++] = '0';
							output[outputPos++] = ')';
							break;
						
						case '\u00AB': 
						// Ã‚Â«  [LEFT-POINTING DOUBLE ANGLE QUOTATION MARK]
						case '\u00BB': 
						// Ã‚Â»  [RIGHT-POINTING DOUBLE ANGLE QUOTATION MARK]
						case '\u201C': 
						// Ã¢â‚¬Å“  [LEFT DOUBLE QUOTATION MARK]
						case '\u201D': 
						// Ã¢â‚¬ï¿½  [RIGHT DOUBLE QUOTATION MARK]
						case '\u201E': 
						// Ã¢â‚¬Å¾  [DOUBLE LOW-9 QUOTATION MARK]
						case '\u2033': 
						// Ã¢â‚¬Â³  [DOUBLE PRIME]
						case '\u2036': 
						// Ã¢â‚¬Â¶  [REVERSED DOUBLE PRIME]
						case '\u275D': 
						// Ã¢ï¿½ï¿½  [HEAVY DOUBLE TURNED COMMA QUOTATION MARK ORNAMENT]
						case '\u275E': 
						// Ã¢ï¿½Å¾  [HEAVY DOUBLE COMMA QUOTATION MARK ORNAMENT]
						case '\u276E': 
						// Ã¢ï¿½Â®  [HEAVY LEFT-POINTING ANGLE QUOTATION MARK ORNAMENT]
						case '\u276F': 
						// Ã¢ï¿½Â¯  [HEAVY RIGHT-POINTING ANGLE QUOTATION MARK ORNAMENT]
						case '\uFF02':  // Ã¯Â¼â€š  [FULLWIDTH QUOTATION MARK]
							output[outputPos++] = '"';
							break;
						
						case '\u2018': 
						// Ã¢â‚¬Ëœ  [LEFT SINGLE QUOTATION MARK]
						case '\u2019': 
						// Ã¢â‚¬â„¢  [RIGHT SINGLE QUOTATION MARK]
						case '\u201A': 
						// Ã¢â‚¬Å¡  [SINGLE LOW-9 QUOTATION MARK]
						case '\u201B': 
						// Ã¢â‚¬â€º  [SINGLE HIGH-REVERSED-9 QUOTATION MARK]
						case '\u2032': 
						// Ã¢â‚¬Â²  [PRIME]
						case '\u2035': 
						// Ã¢â‚¬Âµ  [REVERSED PRIME]
						case '\u2039': 
						// Ã¢â‚¬Â¹  [SINGLE LEFT-POINTING ANGLE QUOTATION MARK]
						case '\u203A': 
						// Ã¢â‚¬Âº  [SINGLE RIGHT-POINTING ANGLE QUOTATION MARK]
						case '\u275B': 
						// Ã¢ï¿½â€º  [HEAVY SINGLE TURNED COMMA QUOTATION MARK ORNAMENT]
						case '\u275C': 
						// Ã¢ï¿½Å“  [HEAVY SINGLE COMMA QUOTATION MARK ORNAMENT]
						case '\uFF07':  // Ã¯Â¼â€¡  [FULLWIDTH APOSTROPHE]
							output[outputPos++] = '\'';
							break;
						
						case '\u2010': 
						// Ã¢â‚¬ï¿½  [HYPHEN]
						case '\u2011': 
						// Ã¢â‚¬â€˜  [NON-BREAKING HYPHEN]
						case '\u2012': 
						// Ã¢â‚¬â€™  [FIGURE DASH]
						case '\u2013': 
						// Ã¢â‚¬â€œ  [EN DASH]
						case '\u2014': 
						// Ã¢â‚¬ï¿½?  [EM DASH]
						case '\u207B': 
						// Ã¢ï¿½Â»  [SUPERSCRIPT MINUS]
						case '\u208B': 
						// Ã¢â€šâ€¹  [SUBSCRIPT MINUS]
						case '\uFF0D':  // Ã¯Â¼ï¿½  [FULLWIDTH HYPHEN-MINUS]
							output[outputPos++] = '-';
							break;
						
						case '\u2045': 
						// Ã¢ï¿½â€¦  [LEFT SQUARE BRACKET WITH QUILL]
						case '\u2772': 
						// Ã¢ï¿½Â²  [LIGHT LEFT TORTOISE SHELL BRACKET ORNAMENT]
						case '\uFF3B':  // Ã¯Â¼Â»  [FULLWIDTH LEFT SQUARE BRACKET]
							output[outputPos++] = '[';
							break;
						
						case '\u2046': 
						// Ã¢ï¿½â€   [RIGHT SQUARE BRACKET WITH QUILL]
						case '\u2773': 
						// Ã¢ï¿½Â³  [LIGHT RIGHT TORTOISE SHELL BRACKET ORNAMENT]
						case '\uFF3D':  // Ã¯Â¼Â½  [FULLWIDTH RIGHT SQUARE BRACKET]
							output[outputPos++] = ']';
							break;
						
						case '\u207D': 
						// Ã¢ï¿½Â½  [SUPERSCRIPT LEFT PARENTHESIS]
						case '\u208D': 
						// Ã¢â€šï¿½  [SUBSCRIPT LEFT PARENTHESIS]
						case '\u2768': 
						// Ã¢ï¿½Â¨  [MEDIUM LEFT PARENTHESIS ORNAMENT]
						case '\u276A': 
						// Ã¢ï¿½Âª  [MEDIUM FLATTENED LEFT PARENTHESIS ORNAMENT]
						case '\uFF08':  // Ã¯Â¼Ë†  [FULLWIDTH LEFT PARENTHESIS]
							output[outputPos++] = '(';
							break;
						
						case '\u2E28':  // Ã¢Â¸Â¨  [LEFT DOUBLE PARENTHESIS]
							output[outputPos++] = '(';
							output[outputPos++] = '(';
							break;
						
						case '\u207E': 
						// Ã¢ï¿½Â¾  [SUPERSCRIPT RIGHT PARENTHESIS]
						case '\u208E': 
						// Ã¢â€šÅ½  [SUBSCRIPT RIGHT PARENTHESIS]
						case '\u2769': 
						// Ã¢ï¿½Â©  [MEDIUM RIGHT PARENTHESIS ORNAMENT]
						case '\u276B': 
						// Ã¢ï¿½Â«  [MEDIUM FLATTENED RIGHT PARENTHESIS ORNAMENT]
						case '\uFF09':  // Ã¯Â¼â€°  [FULLWIDTH RIGHT PARENTHESIS]
							output[outputPos++] = ')';
							break;
						
						case '\u2E29':  // Ã¢Â¸Â©  [RIGHT DOUBLE PARENTHESIS]
							output[outputPos++] = ')';
							output[outputPos++] = ')';
							break;
						
						case '\u276C': 
						// Ã¢ï¿½Â¬  [MEDIUM LEFT-POINTING ANGLE BRACKET ORNAMENT]
						case '\u2770': 
						// Ã¢ï¿½Â°  [HEAVY LEFT-POINTING ANGLE BRACKET ORNAMENT]
						case '\uFF1C':  // Ã¯Â¼Å“  [FULLWIDTH LESS-THAN SIGN]
							output[outputPos++] = '<';
							break;
						
						case '\u276D': 
						// Ã¢ï¿½Â­  [MEDIUM RIGHT-POINTING ANGLE BRACKET ORNAMENT]
						case '\u2771': 
						// Ã¢ï¿½Â±  [HEAVY RIGHT-POINTING ANGLE BRACKET ORNAMENT]
						case '\uFF1E':  // Ã¯Â¼Å¾  [FULLWIDTH GREATER-THAN SIGN]
							output[outputPos++] = '>';
							break;
						
						case '\u2774': 
						// Ã¢ï¿½Â´  [MEDIUM LEFT CURLY BRACKET ORNAMENT]
						case '\uFF5B':  // Ã¯Â½â€º  [FULLWIDTH LEFT CURLY BRACKET]
							output[outputPos++] = '{';
							break;
						
						case '\u2775': 
						// Ã¢ï¿½Âµ  [MEDIUM RIGHT CURLY BRACKET ORNAMENT]
						case '\uFF5D':  // Ã¯Â½ï¿½  [FULLWIDTH RIGHT CURLY BRACKET]
							output[outputPos++] = '}';
							break;
						
						case '\u207A': 
						// Ã¢ï¿½Âº  [SUPERSCRIPT PLUS SIGN]
						case '\u208A': 
						// Ã¢â€šÅ   [SUBSCRIPT PLUS SIGN]
						case '\uFF0B':  // Ã¯Â¼â€¹  [FULLWIDTH PLUS SIGN]
							output[outputPos++] = '+';
							break;
						
						case '\u207C': 
						// Ã¢ï¿½Â¼  [SUPERSCRIPT EQUALS SIGN]
						case '\u208C': 
						// Ã¢â€šÅ’  [SUBSCRIPT EQUALS SIGN]
						case '\uFF1D':  // Ã¯Â¼ï¿½  [FULLWIDTH EQUALS SIGN]
							output[outputPos++] = '=';
							break;
						
						case '\uFF01':  // Ã¯Â¼ï¿½  [FULLWIDTH EXCLAMATION MARK]
							output[outputPos++] = '!';
							break;
						
						case '\u203C':  // Ã¢â‚¬Â¼  [DOUBLE EXCLAMATION MARK]
							output[outputPos++] = '!';
							output[outputPos++] = '!';
							break;
						
						case '\u2049':  // Ã¢ï¿½â€°  [EXCLAMATION QUESTION MARK]
							output[outputPos++] = '!';
							output[outputPos++] = '?';
							break;
						
						case '\uFF03':  // Ã¯Â¼Æ’  [FULLWIDTH NUMBER SIGN]
							output[outputPos++] = '#';
							break;
						
						case '\uFF04':  // Ã¯Â¼â€ž  [FULLWIDTH DOLLAR SIGN]
							output[outputPos++] = '$';
							break;
						
						case '\u2052': 
						// Ã¢ï¿½â€™  [COMMERCIAL MINUS SIGN]
						case '\uFF05':  // Ã¯Â¼â€¦  [FULLWIDTH PERCENT SIGN]
							output[outputPos++] = '%';
							break;
						
						case '\uFF06':  // Ã¯Â¼â€   [FULLWIDTH AMPERSAND]
							output[outputPos++] = '&';
							break;
						
						case '\u204E': 
						// Ã¢ï¿½Å½  [LOW ASTERISK]
						case '\uFF0A':  // Ã¯Â¼Å   [FULLWIDTH ASTERISK]
							output[outputPos++] = '*';
							break;
						
						case '\uFF0C':  // Ã¯Â¼Å’  [FULLWIDTH COMMA]
							output[outputPos++] = ',';
							break;
						
						case '\uFF0E':  // Ã¯Â¼Å½  [FULLWIDTH FULL STOP]
							output[outputPos++] = '.';
							break;
						
						case '\u2044': 
						// Ã¢ï¿½â€ž  [FRACTION SLASH]
						case '\uFF0F':  // Ã¯Â¼ï¿½  [FULLWIDTH SOLIDUS]
							output[outputPos++] = '/';
							break;
						
						case '\uFF1A':  // Ã¯Â¼Å¡  [FULLWIDTH COLON]
							output[outputPos++] = ':';
							break;
						
						case '\u204F': 
						// Ã¢ï¿½ï¿½  [REVERSED SEMICOLON]
						case '\uFF1B':  // Ã¯Â¼â€º  [FULLWIDTH SEMICOLON]
							output[outputPos++] = ';';
							break;
						
						case '\uFF1F':  // Ã¯Â¼Å¸  [FULLWIDTH QUESTION MARK]
							output[outputPos++] = '?';
							break;
						
						case '\u2047':  // Ã¢ï¿½â€¡  [DOUBLE QUESTION MARK]
							output[outputPos++] = '?';
							output[outputPos++] = '?';
							break;
						
						case '\u2048':  // Ã¢ï¿½Ë†  [QUESTION EXCLAMATION MARK]
							output[outputPos++] = '?';
							output[outputPos++] = '!';
							break;
						
						case '\uFF20':  // Ã¯Â¼Â   [FULLWIDTH COMMERCIAL AT]
							output[outputPos++] = '@';
							break;
						
						case '\uFF3C':  // Ã¯Â¼Â¼  [FULLWIDTH REVERSE SOLIDUS]
							output[outputPos++] = '\\';
							break;
						
						case '\u2038': 
						// Ã¢â‚¬Â¸  [CARET]
						case '\uFF3E':  // Ã¯Â¼Â¾  [FULLWIDTH CIRCUMFLEX ACCENT]
							output[outputPos++] = '^';
							break;
						
						case '\uFF3F':  // Ã¯Â¼Â¿  [FULLWIDTH LOW LINE]
							output[outputPos++] = '_';
							break;
						
						case '\u2053': 
						// Ã¢ï¿½â€œ  [SWUNG DASH]
						case '\uFF5E':  // Ã¯Â½Å¾  [FULLWIDTH TILDE]
							output[outputPos++] = '~';
							break;
						
						default: 
							output[outputPos++] = c;
							break;
						
					}
				}
			}
		}
	}
}
