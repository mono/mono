//
// StringType.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//   Francesco Delfino (pluto@tipic.com)
//	 Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Chris J Breisch
//     2002 Tipic, Inc. (http://www.tipic.com)
//
 /*
  * Copyright (c) 2002-2003 Mainsoft Corporation.
  * Copyright (C) 2004 Novell, Inc (http://www.novell.com)
  *
  * Permission is hereby granted, free of charge, to any person obtaining a
  * copy of this software and associated documentation files (the "Software"),
  * to deal in the Software without restriction, including without limitation
  * the rights to use, copy, modify, merge, publish, distribute, sublicense,
  * and/or sell copies of the Software, and to permit persons to whom the
  * Software is furnished to do so, subject to the following conditions:
  * 
  * The above copyright notice and this permission notice shall be included in
  * all copies or substantial portions of the Software.
  * 
  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
  * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
  * DEALINGS IN THE SOFTWARE.
  */

using System;
using System.Globalization;
using System.Text;
using System.ComponentModel;

namespace Microsoft.VisualBasic.CompilerServices {
	[StandardModule, EditorBrowsableAttribute(EditorBrowsableState.Never)] 
	sealed public class StringType {
		private StringType () {}

		/**
		 * This method is called when a seconf asterisk appears in the pattern.
		 * @param pattern the relevant part of the original user pattern
		 * @param source the relevanr part of the original user source string
		 * @param count number of characters that where skipped due to the first 
		 * asterisk. 
		 * @param compareOption the comparision method Binary or Text
		 * @return int number of characters that should be skipped due to all the 
		 * asterisk that where found.
		 */
		private static int multipleAsteriskSkip(
			string pattern,
			string source,
			int count,
			CompareMethod compareOption) {
			string subString;
			bool isLike;
			int length = (source == null) ? 0 : source.Length;
			while (count < length) {
				subString = source.Substring(length - count);
				try {
					isLike = StrLike(subString, pattern, compareOption);
				}
				catch /*(Exception exp)*/ {
					isLike = false;
				}
				if (!isLike)
					count = count + 1;
			}
			return count;
		}
    
		/**
		 * This method determines the number of characters that can be skipped due to
		 * the asterisk.
		 * @param pattern the relevant part of the original user pattern
		 * @param source the relevanr part of the original user source string
		 * @param compareOption the comparision method Binary or Text
		 * @return int number of characters that should be skipped due to all the 
		 * asterisk that where found.
		 */
    
		private static int asteriskSkip(
			string pattern,
			string source,
			CompareMethod compareOption) {
			int sourceLength = source.Length;
			int numberOfSkipedChars = 0;
			int patternLen;
			int patternIndex = 0;
			bool exactMatch = false;
			string sub;
			char currentPatternChar;

			//java patternLen = Strings.Len(pattern); /elsewhere patteren.toString is used, why not here?
			patternLen = pattern.Length;
			while (patternIndex < patternLen) {
             
				currentPatternChar = pattern[patternIndex];
				if (currentPatternChar == '*') {
					//if in the original pattern the where two asterisks together the
					//second is ignored 
					if (numberOfSkipedChars > 0) {
						if (exactMatch) {
							numberOfSkipedChars =
								multipleAsteriskSkip(
								pattern,
								source,
								numberOfSkipedChars,
								compareOption);
							return sourceLength - numberOfSkipedChars;
						}
						sub = pattern.Substring(0, patternIndex);
						numberOfSkipedChars =
							Strings.InStrRev(
							source,
							sub,
							source.Length,
							compareOption);
						return numberOfSkipedChars;
					}
				}
				else if (currentPatternChar == '[') {
					sub = pattern.Substring(patternIndex);
					int skipCharInPattern = sub.IndexOf(']');
					if (skipCharInPattern < 0)
						break;
					numberOfSkipedChars ++;
					patternIndex += skipCharInPattern;    
				}
				else if (currentPatternChar == ']' || currentPatternChar == '?' || 
					currentPatternChar == '#' || currentPatternChar == '!' ||
					currentPatternChar == '-') {
					numberOfSkipedChars++;
					patternIndex ++;
					exactMatch = true;
				}
				else {
					numberOfSkipedChars++;
					patternIndex++;         
				}
			}
			//this value is returned if there are not more '*' in the pattern
			return sourceLength - numberOfSkipedChars;
		}

		public static string FromBoolean (bool Value) { 
			return Convert.ToString(Value);
		}

		public static string FromByte(byte Value) {
			return Convert.ToString(Value);
		}

		public static string FromChar(char Value) {
			return Convert.ToString(Value);
		}

		private static string FromCharAndCount(char Value, int count) {
			//return StringStaticWrapper.Ctor(Value,count);
			return Value.ToString().Substring(0,count);
		}

		private static string FromCharArray(char[] Value) {
			return new string(Value);
		}

		private static string FromCharArraySubset(
			char[] Value,
			int startIndex,
			int length) {
			return new string(Value, startIndex, length);
		}

		public static string FromDate(DateTime Value) {
			//TODO: which is right, Mono or Mainsoft
			//implmented in mono by
			//return Convert.ToString(Value);
		
			//implmented in java
			long lTime;
			TimeSpan ts;

			ts = Value.TimeOfDay;
			lTime = ts.Ticks;
			//if only the part of the hours ,minute ... is relevant (not day, month,
			// year) the format is "T"        
			if (lTime == Value.Ticks ||
				(Value.Year == 1899
				&& Value.Month == 12
				&& Value.Day == 30))
				return Value.ToString("T", null);
			// only the part of day,month and year is relevant , the format is "d".    
			if (lTime == 0)   
				return Value.ToString("d", null);
			return Value.ToString("G", null);
		}

		public static string FromDecimal(Decimal Value) {
			return FromDecimal(Value, null);
		}

		public static string FromDecimal(Decimal Value, NumberFormatInfo NumberFormat) {
			return Convert.ToString(Value, NumberFormat);
			//java code return Value.ToString("G", numberFormat);
		}

		public static string FromDouble(double Value) {
			return Convert.ToString(Value);
			//java return FromDouble(Value, null);
		}

		public static string FromDouble(double Value, NumberFormatInfo NumberFormat) {
			return Convert.ToString(Value,NumberFormat);
			//return new ClrDouble(Value).ToString("G");
		}

		public static string FromInteger(int Value) {
			return Value.ToString();
		}

		public static string FromLong(long Value) {
			return Value.ToString();
			//return Convert.ToString(Value);
		}

		public static string FromShort(short Value) {
			return Value.ToString();
		}

		public static string FromSingle(float Value) {
			return Convert.ToString(Value);
			//return FromSingle(Value, null);
		}

		public static string FromSingle(float Value, NumberFormatInfo NumberFormat) {
			return Convert.ToString(Value,NumberFormat);
			//return new ClrSingle(Value).ToString(NumberFormat);
		}

		public static string FromObject(object Value) {
			if (Value == null)
				return null;

			if (Value is string)
				return (string) Value;

			if (
				(Value is char[])
				&& ((Array)Value).Rank == 1)
				return new string(CharArrayType.FromObject(Value));
            
			return Convert.ToString(Value);   

		}


		/**
		 * This method replace in the reference strDesRef parameter the characters 
		 * from position  startPosition. the number of characters that are been replaced
		 * is the minimum between maxInsertLength and the length of sInsert.
		 * @param strDesRef the destination string reference. 
		 * @param startPosition the index from which the change should be done.      
		 * @param maxInsertLength the maximum number of characters that should be change.
		 * @param sInsert the string from which the character should be taken
		 */

		public static void MidStmtStr(ref string strDesRef, int startPosition, int maxInsertLength, string sInsert) 
		{
			int destLen = 0;
			int insertLen = 0;
			string dest = strDesRef;
		
			if (dest != null)
				destLen = dest.Length;
			if (sInsert != null)
				insertLen = sInsert.Length;
			else
				return;    

			//change to java location in array.
			startPosition = startPosition - 1;
			if (startPosition < 0 || startPosition >= destLen) {
				throw new ArgumentException("Invalid Argument Value", "Start");
				//throw new IllegalArgumentException(//java
				//	Utils.GetResourceString("Argument_InvalidValue1", "Start"));
			}
			if (maxInsertLength < 0) {
				throw new ArgumentException("Invalid Argument Value", "Length");
				//throw new IllegalArgumentException(//java
				//	Utils.GetResourceString("Argument_InvalidValue1", "Length"));
			}
			if (insertLen > maxInsertLength)
				insertLen = maxInsertLength;
			if (insertLen > destLen - startPosition)
				insertLen = destLen - startPosition;
			if (insertLen == 0)
				return;
			//TODO: are the next two lines equvlent to the 8 that follow?
            dest.Remove(startPosition,insertLen);
			dest.Insert(startPosition,sInsert.Substring(0,insertLen));
			//Java version
			//sb = new StringBuilder(dest);
			//if (sInsert.Length == insertLen)
			//	sb.Replace(startPosition ,startPosition + insertLen, sInsert);
			//else
			//	sb.Replace(
			//		startPosition,
			//		startPosition + insertLen,
			//		sInsert.Substring(0, insertLen));


			//strDesRef.setValue(sb.ToString());//java
			strDesRef = dest;//sb.ToString();
		}

		public static int StrCmp(string sLeft, string sRight, bool TextCompare) {
			if (sLeft == null)
				sLeft = "";
			if (sRight == null)
				sRight = "";

			if (TextCompare)
				return string.Compare(sLeft, sRight, TextCompare);
			//return StringStaticWrapper.Compare(sLeft, sRight, TextCompare);
			return sLeft.CompareTo(sRight);
			// return StringStaticWrapper.CompareOrdinal(sLeft, sRight);
		}

		internal static string ToHalfwidthNumbers(string s) {
			return s;
		}

		private static bool compareBinary(
			bool seenNot,
			bool match,
			char patternChar,
			char sourceChar) {
			// if (seenNot ^ notMatch ) == true then a previous pattern character
			// matched the current source character . the current comparision is not
			// required
			if (seenNot ^ match )
				return match;   
			else if (seenNot && match)
				return patternChar != sourceChar;
			else 
				return patternChar == sourceChar;
		}

		private static bool compare(        
			bool seenNot,
			bool match,
			char patternChar,
			char sourceChar) {
			// if (seenNot ^ notMatch ) == true then a previous pattern character
			// matched the current source character . the current comparision is not
			// required
			//if (seenNot ^ match )
			if (seenNot || match )
				return match;         
			else if (seenNot && match)
				return string.Compare(FromChar(patternChar), FromChar(sourceChar)) != 0;
			//return StringStaticWrapper.Compare(FromChar(patternChar), FromChar(sourceChar)) != 0;
			//else 
				return string.Compare(FromChar(patternChar), FromChar(sourceChar)) == 0;
//				return StringStaticWrapper.Compare(FromChar(patternChar), FromChar(sourceChar)) != 0;
//			return StringStaticWrapper.Compare(FromChar(patternChar), FromChar(sourceChar)) != 0;
//			else 
//				return StringStaticWrapper.Compare(FromChar(patternChar), FromChar(sourceChar)) == 0;
		}

		/**
		 * check if a specified string is an hex or oct representation of
		 * an integer number
		 * @param Value The string Value
		 * @param res a long array (minimum size 1).
		 * @return true if <code>Value<\code> can be parse into integer Value.
		 * the result of parsing is located in the <code>res[0]<\code>.
		 */
		internal static bool IsHexOrOctValue(string Value, long[] res) {
			try {
				// if the string starts with '&h' or '&H' it represents an Hex number
				if (Value.StartsWith("&H") || Value.StartsWith("&h"))
					res[0] = Convert.ToInt64(Value.Substring(2), 16);
					// if the string starts with '&o' or '&O' it represents an Oct number
				else if (Value.StartsWith("&O") || Value.StartsWith("&o"))
					res[0] = Convert.ToInt64(Value.Substring(2), 8);
				else
					return false;
			}
			catch /*(Java catches NumberFormatException, presumable passing all other excptions back up to the caller
				   * //TODO: we should narrow the ececptions that we catch here)*/ 
			{
			//catch (NumberFormatException e) {
				return false;
			}
			return true;
		}
    
		/**
		 * This method matches a pattern between brackets and the relevant character.
		 * The pattern matching is for binary comparision   
		 * @param pattern the part of the pattern that is between the brackets without
		 * the brackets 
		 * @param sourceChar the relevant character in the source string
		 * @return bool true if the character matches the pattern and false otherwise.
		 */
		private static bool inBracketBinary(string pattern, char sourceChar) {
			char currentPatternChar = (char)0;
			char currentCharInRange = (char)0;
			char previousCharInRange = (char)0;
			int patternIndex = 0;
			bool isMatch = false;
			bool isNotSignAppears = false;
			bool specialChar = false;
			bool isRangeSignAppears = false;
			int patternLength = (pattern == null)? 0 : pattern.Length;
			while (patternIndex < patternLength) {
				currentPatternChar = pattern[patternIndex];
				if (currentPatternChar == '*') {
					isMatch =
						compareBinary(
						isNotSignAppears,
						isMatch,
						currentPatternChar,
						sourceChar);
					specialChar = true;
				}
				else if (currentPatternChar == '?') {
					// the pattern is '[previousCharInRange-?
					if (isRangeSignAppears) {
						isRangeSignAppears = false;
						currentCharInRange = currentPatternChar;
						if (previousCharInRange > currentCharInRange)
							throw new Exception("Bad patteren string");
							//throw (Exception) ExceptionUtils.VbMakeException(
							//	vbErrors.BadPatStr);
						//when the previous char in the range matches the char in
						// the source 
						if (!(isNotSignAppears || isMatch)){
							if (sourceChar == previousCharInRange)
								isMatch = true;
							if (isNotSignAppears)
								isMatch = !isMatch;
						}
					}
						//the first place in the range '[?'
					else {
						previousCharInRange = currentPatternChar;
						specialChar = true;
						isMatch =
							compareBinary(
							isNotSignAppears,
							isMatch,
							currentPatternChar,
							sourceChar);
					}
				}
				else if (currentPatternChar == '#') {
					//the pattern is like '[previousCharInRange-#' 
					if (isRangeSignAppears) {
						isRangeSignAppears = false;
						currentCharInRange = currentPatternChar;
						if (previousCharInRange > currentCharInRange)
							throw new Exception("Bad patteren string");
							//throw (
							//	Exception) ExceptionUtils.VbMakeException(
							//	vbErrors.BadPatStr);
						//when the previous char in the range matches the char in
						// the source 
						if (!(isNotSignAppears || isMatch)){
							if (sourceChar == previousCharInRange)
								isMatch = true;
							if (isNotSignAppears)
								isMatch = !isMatch;
						}
					}
						//the first place in range. 
					else {
						previousCharInRange = currentPatternChar;
						specialChar = true;
						isMatch =
							compareBinary(
							isNotSignAppears,
							isMatch,
							currentPatternChar,
							sourceChar);
					}
				}
				else if (currentPatternChar == '-') {
					//this pattern is not valid example is '[9--'
					if (isRangeSignAppears && specialChar)
						throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", "Pattern"));
					if (!(specialChar) && !(isRangeSignAppears)) {
						isMatch =
							compareBinary(isNotSignAppears, isMatch, currentPatternChar, sourceChar);
					}
					isRangeSignAppears = true;
				}
				else if (currentPatternChar == '!') {
					//this is the first symbol is the range
					if (!(isNotSignAppears)) {
						isNotSignAppears = true;
						isMatch = true;
					}
						//this is the second '!' in the range '[!! ' or appears
						//as a symbol in the pattern
					else {
						specialChar = true;
						isMatch =
							compareBinary(isNotSignAppears, isMatch, currentPatternChar, sourceChar);
					}
				}
				else if (currentPatternChar == '[') {
					if (isRangeSignAppears) {
						isRangeSignAppears = false;
						currentCharInRange = currentPatternChar;
						if (previousCharInRange > currentCharInRange)
							throw new Exception("Bad patteren string");
							//throw (
							//	Exception) ExceptionUtils.VbMakeException(
							//	vbErrors.BadPatStr);
						//when the previous char in the range matches the char in
						// the source 
						if (!(isNotSignAppears ^ isMatch)){
							if (sourceChar == previousCharInRange)
								isMatch = true;
							if (isNotSignAppears)
								isMatch = !isMatch;
						}
					}
						//the first sign in the range of chars
					else {
						previousCharInRange = currentPatternChar;
						specialChar = true;
						isMatch =
							compareBinary(
							isNotSignAppears,
							isMatch,
							currentPatternChar,
							sourceChar);
					}
				}
				else if (currentPatternChar == ']') {
					isMatch =
						compareBinary(
						isNotSignAppears,
						isMatch,
						currentPatternChar,
						sourceChar);
					if (!(isMatch))
						break;
				}
					//this pattern char appears in range of chars.
				else {
					specialChar = true;
					if (isRangeSignAppears) {
						isRangeSignAppears = false;
						currentCharInRange = currentPatternChar;
						if (previousCharInRange > currentCharInRange)
							throw new Exception("Bad patteren string");
							//throw (Exception)ExceptionUtils.VbMakeException(vbErrors.BadPatStr);
						if (!(isNotSignAppears || isMatch)){
							if (sourceChar <= previousCharInRange || sourceChar > currentCharInRange)
								isMatch = true;
							else
								isMatch = false;    
							if (isNotSignAppears)
								isMatch = !isMatch;
						}
					}
					else {
						previousCharInRange = currentPatternChar;
						isMatch =
							compareBinary(isNotSignAppears, isMatch, currentPatternChar, sourceChar);
					}
				}
				patternIndex++;

			}
			if (isNotSignAppears ^ isMatch) return true;
			if (isRangeSignAppears && !isMatch)
				return false;
			else if (isNotSignAppears) {
				if ('!' != sourceChar)
					return false;
			}
			return true;
		}
    
		/**
		 * this method checks if the soutce string matches the pattern according to 
		 * binary comparision.
		 * @param source the source string 
		 * @param pattern the pattern string
		 * @return bool True if the source match the pattern and false otherwise.
		 */

		public static bool StrLikeBinary(string source, string pattern) {
			bool startRangeSignAppears = false;
			bool isMatch = false;
			char currentPatternChar = (char)0;
			int patternLength = (char)0;
			int patternIndex = (char)0;
			char currentSourceChar = (char)0;
			//bool isRangeSignAppears = false;
			//bool specialChar = false;
			bool isNotSignAppears = false;
			int numberOfSkipedChars = 0;
			int sourceLength = 0;
			int sourceIndex = 0;
         

			patternLength = (pattern == null)? 0 : pattern.Length;
			sourceLength = (source == null)? 0 : source.Length ;
        
			if (sourceIndex < sourceLength)
				currentSourceChar = source[sourceIndex];
			while (patternIndex < patternLength) {
				currentPatternChar = pattern[patternIndex];
				if (currentPatternChar == '*') {
					numberOfSkipedChars =
						asteriskSkip(
						pattern.Substring(patternIndex + 1),
						source.Substring(sourceIndex),
						CompareMethod.Binary);
					if (numberOfSkipedChars < 0) {
						break;
					}
					if (numberOfSkipedChars > 0) {
						sourceIndex += numberOfSkipedChars;
						if (sourceIndex < sourceLength)
							currentSourceChar = source[sourceIndex];        
					}
				}
				else if (currentPatternChar == '?') {
					sourceIndex++;
					if (sourceIndex < sourceLength)
						currentSourceChar = source[sourceIndex];
				}
				else if (currentPatternChar == '#') {
					if (!(char.IsDigit(currentSourceChar)))
						break;
					sourceIndex++;
					if (sourceIndex < sourceLength)
						currentSourceChar = source[sourceIndex];
				}          
				else if (currentPatternChar == '-') {
					isMatch =
						compareBinary(
						isNotSignAppears,
						isMatch,
						currentPatternChar,
						currentSourceChar);
					sourceIndex++;
					if (sourceIndex < sourceLength)
						currentSourceChar = source[sourceIndex];
					//isRangeSignAppears = true;
				}
				else if (currentPatternChar == '!') {
					//specialChar = true;
					isMatch =
						compareBinary(
						isNotSignAppears,
						isMatch,
						currentPatternChar,
						currentSourceChar);
					sourceIndex++;
					if (sourceIndex < sourceLength)
						currentSourceChar = source[sourceIndex];
				}
				else if (currentPatternChar == '[') {
					string sub = pattern.Substring(patternIndex);
					int indexOfEndBracket = sub.IndexOf(']');
					startRangeSignAppears = true;
					if (indexOfEndBracket == -1)
						break;
					sub = sub.Substring(1, indexOfEndBracket);
					startRangeSignAppears = false;
					bool isOk = inBracketBinary(sub, currentSourceChar);
					if (!isOk)                                 
						break;                
					sourceIndex++;
					if (sourceIndex < sourceLength)
						currentSourceChar = source[sourceIndex];
					patternIndex += (sub.Length + 2);
					continue;
				}
				else if (currentPatternChar == ']') {
					isMatch =
						compareBinary(
						isNotSignAppears,
						isMatch,
						currentPatternChar,
						currentSourceChar);
					if (!(isMatch))
						break;
					sourceIndex++;
					if (sourceIndex < sourceLength)
						currentSourceChar = source[sourceIndex];
					isMatch = false;
					//specialChar = false;
					isNotSignAppears = false;
					//isRangeSignAppears = false;
				}
				else if (currentPatternChar == currentSourceChar || isNotSignAppears) {
					//specialChar = true;
					isNotSignAppears = false;
					sourceIndex++;
					if (sourceIndex < sourceLength)
						currentSourceChar = source[sourceIndex];
					else if (sourceIndex > sourceLength)
						return false;
				}
				else
					break;

				patternIndex++;
			}
			if (startRangeSignAppears) {
				if (sourceLength == 0)
					return false;
				throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", "Pattern"));
			}
			if (patternIndex != patternLength || sourceIndex != sourceLength)
				return false;
			return true;
		}

		public static bool StrLike(
			string source,
			string pattern,
			CompareMethod compareOption) {
			if(compareOption == CompareMethod.Text)
				return StrLikeBinary(source, pattern);
			else
				return StrLikeText(source, pattern);
		}

//		/**
//		 * this method checks if the soutce string matches the pattern according to
//		 * the comparision method.
//		 * @param source the source string 
//		 * @param pattern the pattern string
//		 * @param compareOption this param determines if the comparision is binary
//		 * or text
//		 * @return bool True if the source match the pattern and false otherwise.
//		 */
//
//		public static bool StrLike(
//			string source,
//			string pattern,
//			int compareOption) {
//			if (compareOption == 0)
//				
//			
//		}
    

		/**
		 * this method checks if the soutce string matches the pattern according to 
		 * binary comparision.
		 * @param source the source string 
		 * @param pattern the pattern string
		 * @return bool True if the source match the pattern and false otherwise.
		 */

		public static bool StrLikeText(string source, string pattern) {
			//char currentCharInRange = (char)0;
			bool startRangeSignAppears = false;
			bool isMatch = false;
			char currentPatternChar = (char)0;
			int patternLength = 0;
			int patternIndex = 0;
			char currentSourceChar = (char)0;
			//bool isRangeSignAppears = false;
			//bool specialChar = false;
			bool isNotSignAppears = false;
			int numberOfSkipedChars = 0;
			int sourceLength = 0;
			int sourceIndex = 0;
			//char previousCharInRange = (char)0;

			patternLength = (pattern == null)? 0 : pattern.Length;
			sourceLength = (source == null)? 0 : source.Length ;

			if (sourceIndex < sourceLength)
				currentSourceChar = source[sourceIndex];

			while (patternIndex < patternLength) {
				currentPatternChar = pattern[patternIndex];
				if (currentPatternChar == '*') {
					numberOfSkipedChars =
						asteriskSkip(
						pattern.Substring(patternIndex + 1),
						source.Substring(sourceIndex),
						CompareMethod.Text);
					if (numberOfSkipedChars < 0)
						return false;
					if (numberOfSkipedChars > 0) {
						sourceIndex += numberOfSkipedChars;
						if (sourceIndex < sourceLength)
							currentSourceChar = source[sourceIndex];
					}
				}
				else if (currentPatternChar == '?') {
					sourceIndex++;
					if (sourceIndex < sourceLength)
						currentSourceChar = source[sourceIndex];
				}
				else if (currentPatternChar == '#') {
					if (!(char.IsDigit(currentSourceChar)))
						break;
					sourceIndex++;
					if (sourceIndex < sourceLength)
						currentSourceChar = source[sourceIndex];
				}
				else if (currentPatternChar == '-') {
					isMatch =
						compare(
						isNotSignAppears,
						isMatch,
						currentPatternChar,
						currentSourceChar);
					sourceIndex++;
					if (sourceIndex < sourceLength)
						currentSourceChar = source[sourceIndex];
					//isRangeSignAppears = true;
				}
				else if (currentPatternChar == '!') {
					//specialChar = true;
					isMatch =
						compare(
						isNotSignAppears,
						isMatch,
						currentPatternChar,
						currentSourceChar);
					sourceIndex++;
					if (sourceIndex < sourceLength)
						currentSourceChar = source[sourceIndex];
				}
				else if (currentPatternChar == '[') {
					string sub = pattern.Substring(patternIndex);
					startRangeSignAppears = true;
					int indexOfEndBracket = sub.IndexOf(']');
					if (indexOfEndBracket == -1)
						break;
					sub = sub.Substring(1, indexOfEndBracket);
					startRangeSignAppears = false;
					bool isOk = inBracketBinary(sub, currentSourceChar);
					if (!isOk)
						break;                
					sourceIndex++;
					if (sourceIndex < sourceLength)
						currentSourceChar = source[sourceIndex];
					patternIndex += (sub.Length + 2);
					continue;
				}
				else if (currentPatternChar == ']') {
					isMatch =
						compare(
						isNotSignAppears,
						isMatch,
						currentPatternChar,
						currentSourceChar);
					//specialChar = true;
					if (!(isMatch))
						break;
					sourceIndex++;
					if (sourceIndex < sourceLength)
						currentSourceChar = source[sourceIndex];
					isMatch = false;
					//specialChar = false;
					isNotSignAppears = false;
					//isRangeSignAppears = false;
				}
				else {
					//specialChar = true;
					if (!(currentPatternChar == currentSourceChar || isNotSignAppears))
						break;
					isNotSignAppears = false;
					sourceIndex++;
					if (sourceIndex < sourceLength)
						currentSourceChar = source[sourceIndex];
					else if (sourceIndex > sourceLength)
						return false;
				}
				patternIndex++;
			}
			if (startRangeSignAppears) {
				if (sourceLength == 0)
					return false;
				throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", "Pattern"));
			}
			if (patternIndex != patternLength || sourceIndex != sourceLength)
				return false;
			return true;
		}
    
		/**
		 * This method matches a pattern between brackets and the relevant character.
		 * The pattern matching is for text comparision   
		 * @param pattern the part of the pattern that is between the brackets without
		 * the brackets 
		 * @param sourceChar the relevant character in the source string
		 * @return bool true if the character matches the pattern and false otherwise.
		 */
    
		private static bool inBracketText(string pattern, char sourceChar) {
			char currentPatternChar = (char)0;
			char currentCharInRange = (char)0;
			char previousCharInRange = (char)0;
			int patternIndex = 0;
			bool isMatch = false;
			bool isNotSignAppears = false;
			bool specialChar = false;
			bool isRangeSignAppears = false;
			int patternLength = (pattern == null)? 0 : pattern.Length;
			while (patternIndex < patternLength) {
				currentPatternChar = pattern[patternIndex];
				if (currentPatternChar == '*') {
					isMatch =
						compare(                            
						isNotSignAppears,
						isMatch,
						currentPatternChar,
						sourceChar);
					specialChar = true;
				}
				else if (currentPatternChar == '?') {
					if (isRangeSignAppears) {
						isRangeSignAppears = false;
						currentCharInRange = currentPatternChar;
						if (previousCharInRange > currentCharInRange)
							throw new Exception("Bad patteren string");
							//throw (
							//	Exception) ExceptionUtils.VbMakeException(
							//	vbErrors.BadPatStr);
						//when the previous char in the range matches the char in
						// the source
						if (!(isNotSignAppears ^ isMatch)){
							if (!(string.Compare(
								FromChar(previousCharInRange),
								FromChar(sourceChar),
								true)
								>= 0
								|| string.Compare(
								FromChar(currentCharInRange),
								FromChar(sourceChar),
								true)
								< 0))
								isMatch = true;
							if (isNotSignAppears)
								isMatch = (isMatch == false);
						}
					}
						// the first place in the range '[?'
					else {
						previousCharInRange = currentPatternChar;
						specialChar = true;
						isMatch =
							compare(
							isNotSignAppears,
							isMatch,
							currentPatternChar,
							sourceChar);
					}
				}
				else if (currentPatternChar == '#') {
					// the pattern is '[previousCharInRange-#
					if (isRangeSignAppears) {
						isRangeSignAppears = false;
						currentCharInRange = currentPatternChar;
						if (previousCharInRange > currentCharInRange)
							throw new Exception("Bad pattern string");
							//throw (Exception)ExceptionUtils.VbMakeException(vbErrors.BadPatStr);
						if (!(isNotSignAppears || isMatch)){
							if (!(string.Compare(
								FromChar(previousCharInRange),
								FromChar(sourceChar),
								true)
								>= 0
								|| string.Compare(
								FromChar(currentCharInRange),
								FromChar(sourceChar),
								true)
								< 0))
								isMatch = true;
							if (isNotSignAppears)
								isMatch = isMatch == false;
						}
					}
						// the first place in the range '[#'
					else {
						previousCharInRange = currentPatternChar;
						specialChar = true;
						isMatch =
							compare(
							isNotSignAppears,
							isMatch,
							currentPatternChar,
							sourceChar);
					}
				}
				else if (currentPatternChar == '-') {
					//this pattern is not valid example is '[9--'
					if (isRangeSignAppears && specialChar)
						throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", "Pattern"));
					if (!(specialChar) && !(isRangeSignAppears)) {
						isMatch =
							compare(isNotSignAppears, isMatch, currentPatternChar, sourceChar);
					}
					isRangeSignAppears = true;
				}
				else if (currentPatternChar == '!') {
					//this is the first symbol is the range
					if (!(isNotSignAppears)) {
						isNotSignAppears = true;
						isMatch = true;
					}
						//this is the second '!' in the range '[!! ' or appears
						//as a symbol in the pattern
					else {
						specialChar = true;
						isMatch =
							compare(isNotSignAppears, isMatch, currentPatternChar, sourceChar);
					}
				}
				else if (currentPatternChar == '[') {
					if (isRangeSignAppears) {
						isRangeSignAppears = false;
						currentCharInRange = currentPatternChar;
						if (previousCharInRange > currentCharInRange)
							throw new Exception("Bad patteren string");
							//throw (
							//	Exception) ExceptionUtils.VbMakeException(
							//	vbErrors.BadPatStr);
						//when the previous char in the range matches the char in
						// the source 
						if (!(isNotSignAppears || isMatch)){
							if (!(string.Compare(
								FromChar(previousCharInRange),
								FromChar(sourceChar),
								true)
								>= 0
								|| string.Compare(
								FromChar(currentCharInRange),
								FromChar(sourceChar),
								true)
								< 0))
								isMatch = true;
							if (isNotSignAppears)
								isMatch = (isMatch == false);
						}
					}
						// the first sign in the range of chars
					else {
						previousCharInRange = currentPatternChar;
						specialChar = true;
						isMatch = compare(isNotSignAppears,isMatch,
							currentPatternChar,sourceChar);
					}
				}
				else if (currentPatternChar == ']') {
					isMatch =
						compare(
						isNotSignAppears,
						isMatch,
						currentPatternChar,
						sourceChar);
					if (!(isMatch))
						break;
				}
					//this pattern char appears in range of chars.
				else {
					specialChar = true;
					if (isRangeSignAppears) {
						isRangeSignAppears = false;
						currentCharInRange = currentPatternChar;
						if (previousCharInRange > currentCharInRange)
							throw new Exception("Bad patteren string");
							//throw (Exception)ExceptionUtils.VbMakeException(vbErrors.BadPatStr);
						if (!(isNotSignAppears ^ isMatch)){
							if (sourceChar <= previousCharInRange || sourceChar > currentCharInRange)
								isMatch = true;
							else
								isMatch = false;    
							if (isNotSignAppears)
								isMatch = !isMatch;
						}
					}
					else {
						previousCharInRange = currentPatternChar;
						isMatch =
							compare(isNotSignAppears, isMatch, currentPatternChar, sourceChar);
					}
				}
				patternIndex++;

			}
			if (isNotSignAppears || isMatch) return true;
			if (isRangeSignAppears && !isMatch)
				return false;
			else if (isNotSignAppears) {
				if ('!' != sourceChar)
					return false;
			}
			return true;
		}    
    
	}
}
