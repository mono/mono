//
// Strings.cs
//
// Authors:
//   Martin Adoue (martin@cwanet.com)
//   Chris J Breisch (cjbreisch@altavista.net)
//   Francesco Delfino (pluto@tipic.com)
//   Daniel Campos (danielcampos@netcourrier.com)
//   Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) 2002 Ximian Inc.
//     2002 Tipic, Inc. (http://www.tipic.com)
//

using System;
using System.Text;
using System.ComponentModel;

using System.Runtime.InteropServices;
using Microsoft.VisualBasic.CompilerServices;

namespace Microsoft.VisualBasic
{
	/// <summary>
	/// The Strings module contains procedures used to perform string operations. 
	/// </summary>

	[StandardModule] 
	[StructLayout(LayoutKind.Auto)] 
	public class Strings
	{
		private Strings()
		{
			//Do nothing. Nobody should be creating this.
		}

		
		/// <summary>
		/// Returns an Integer value representing the character code corresponding to a character.
		/// </summary>
		/// <param name="String">Required. Any valid Char or String expression. If String is a String expression, only the first character of the string is used for input. If String is Nothing or contains no characters, an ArgumentException error occurs.</param>
		[MonoTODO]
		public static int Asc(char Char) 
		{
			//FIXME: Check the docs, it says something about Locales, DBCS, etc.
			return (int)Char;
		}


		/// <summary>
		/// Returns an Integer value representing the character code corresponding to a character.
		/// </summary>
		/// <param name="String">Required. Any valid Char or String expression. If String is a String expression, only the first character of the string is used for input. If String is Nothing or contains no characters, an ArgumentException error occurs.</param>
		[MonoTODO]
		public static int Asc(string String) 
		{
			if ((String == null) || (String.Length < 1))
				throw new ArgumentException("Length of argument 'String' must be at least one.", "String");

			//FIXME: Check the docs, it says something about Locales, DBCS, etc.
			return (int) String.ToCharArray(0, 1)[0];
			//why? check http://bugzilla.ximian.com/show_bug.cgi?id=23540
		}


		/// <summary>
		/// Returns an Integer value representing the character code corresponding to a character.
		/// </summary>
		/// <param name="String">Required. Any valid Char or String expression. If String is a String expression, only the first character of the string is used for input. If String is Nothing or contains no characters, an ArgumentException error occurs.</param>
		[MonoTODO("Needs testing")]
		public static int AscW(char String) 
		{
			/*
			 * AscW returns the Unicode code point for the input character. 
			 * This can be 0 through 65535. The returned value is independent 
			 * of the culture and code page settings for the current thread.
			 */

			return (int) String;
		}
		
		/// <summary>
		/// Returns an Integer value representing the character code corresponding to a character.
		/// </summary>
		/// <param name="String">Required. Any valid Char or String expression. If String is a String expression, only the first character of the string is used for input. If String is Nothing or contains no characters, an ArgumentException error occurs.</param>
		[MonoTODO("Needs testing")]
		public static int AscW(string String) 
		{
			/*
			 * AscW returns the Unicode code point for the input character. 
			 * This can be 0 through 65535. The returned value is independent 
			 * of the culture and code page settings for the current thread.
			 */
			if ((String == null) || (String.Length == 0))
				throw new ArgumentException("Length of argument 'String' must be at leasr one.", "String");
			return (int) String.ToCharArray(0, 1)[0];

		}

		/// <summary>
		/// Returns the character associated with the specified character code.
		/// </summary>
		/// <param name="CharCode">Required. An Integer expression representing the code point, or character code, for the character. If CharCode is outside the range -32768 through 65535, an ArgumentException error occurs.</param>
		[MonoTODO]
		public static char Chr(int CharCode) 
		{

			// According to docs (ms-help://MS.VSCC/MS.MSDNVS/vblr7/html/vafctchr.htm)
			// Chr and ChrW should throw ArgumentException if ((CharCode < -32768) || (CharCode > 65535))
			// Instead, VB.net throws an OverflowException. I'm following the implementation
			// instead of the docs. 

			if ((CharCode < -32768) || (CharCode > 65535))
				throw new OverflowException("Value was either too large or too small for a character.");

			//FIXME: Check the docs, it says something about Locales, DBCS, etc.
			return System.Convert.ToChar(CharCode);
		}

		/// <summary>
		/// Returns the character associated with the specified character code.
		/// </summary>
		/// <param name="CharCode">Required. An Integer expression representing the code point, or character code, for the character. If CharCode is outside the range -32768 through 65535, an ArgumentException error occurs.</param>
		[MonoTODO("Needs testing")]
		public static char ChrW(int CharCode ) 
		{
			/*
			 * According to docs ()
			 * Chr and ChrW should throw ArgumentException if ((CharCode < -32768) || (CharCode > 65535))
			 * Instead, VB.net throws an OverflowException. I'm following the implementation
			 * instead of the docs
			 */
			if ((CharCode < -32768) || (CharCode > 65535))
				throw new OverflowException("Value was either too large or too small for a character.");

			/*
			 * ChrW takes CharCode as a Unicode code point. The range is independent of the 
			 * culture and code page settings for the current thread. Values from -32768 through 
			 * -1 are treated the same as values in the range +32768 through +65535.
			 */
			if (CharCode < 0)
				CharCode += 0x10000;

			return System.Convert.ToChar(CharCode);
		}

		/// <summary>
		/// Returns a zero-based array containing a subset of a String array based on specified filter criteria.
		/// </summary>
		/// <param name="Source">Required. One-dimensional array of strings to be searched.</param>
		/// <param name="Match">Required. String to search for.</param>
		/// <param name="Include">Optional. Boolean value indicating whether to return substrings that include or exclude Match. If Include is True, the Filter function returns the subset of the array that contains Match as a substring. If Include is False, the Filter function returns the subset of the array that does not contain Match as a substring.</param>
		/// <param name="Compare">Optional. Numeric value indicating the kind of string comparison to use. See Settings for values.</param>
		[MonoTODO("Needs testing")]
		public static string[] Filter(object[] Source, 
			string Match, 
			[Optional]
			[DefaultValue(true)] 
			bool Include,
			[OptionCompare] [Optional]
			[DefaultValue(CompareMethod.Binary)] 
			CompareMethod Compare)
		{

			if (Source == null)
				throw new ArgumentException("Argument 'Source' can not be null.", "Source");
			if (Source.Rank > 1)
				throw new ArgumentException("Argument 'Source' can have only one dimension.", "Source");

			string[] strings;
			strings = new string[Source.Length];

			Source.CopyTo(strings, 0);
			return Filter(strings, Match, Include, Compare);

		}

		/// <summary>
		/// Returns a zero-based array containing a subset of a String array based on specified filter criteria.
		/// </summary>
		/// <param name="Source">Required. One-dimensional array of strings to be searched.</param>
		/// <param name="Match">Required. String to search for.</param>
		/// <param name="Include">Optional. Boolean value indicating whether to return substrings that include or exclude Match. If Include is True, the Filter function returns the subset of the array that contains Match as a substring. If Include is False, the Filter function returns the subset of the array that does not contain Match as a substring.</param>
		/// <param name="Compare">Optional. Numeric value indicating the kind of string comparison to use. See Settings for values.</param>
		public static string[] Filter(string[] Source, 
			string Match, 
			[Optional]
			[DefaultValue(true)] 
			bool Include,
			[Optional]
			[DefaultValue(CompareMethod.Binary)] 
			CompareMethod Compare)
		{

			if (Source == null)
				throw new ArgumentException("Argument 'Source' can not be null.", "Source");
			if (Source.Rank > 1)
				throw new ArgumentException("Argument 'Source' can have only one dimension.", "Source");

			/*
			 * Well, I don't like it either. But I figured that two iterations
			 * on the array would be better than many aloocations. Besides, this
			 * way I can isolate the special cases.
			 * I'd love to hear from a different approach.
			 */

			int count = Source.Length;
			bool[] matches = new bool[count];
			int matchesCount = 0;

			for (int i = 0; i < count; i++)
			{
				if (InStr(1, Match, Source[i], Compare) != 0)
				{
					//found one more
					matches[i] = true;
					matchesCount ++;
				}
				else
				{
					matches[i] = false;
				}
			}

			if (matchesCount == 0)
			{
				if (Include)
					return new string[0];
				else
					return Source;
			}
			else
			{
				if (matchesCount == count)
				{
					if (Include)
						return Source;
					else
						return new string[0];
				}
				else
				{
					string[] ret;
					int j = 0;
					if (Include)
						ret = new string [matchesCount];
					else
						ret = new string [count - matchesCount];

					for (int i=0; i < count; i++)
					{
						if ((matches[i] && Include) || !(matches[i] || Include))
						{
							ret[j] = Source[i];
							j++;
						}
					}
					return ret;
				}
			}
		}

		/// <summary>
		/// Returns a string formatted according to instructions contained in a format String expression.
		/// </summary>
		/// <param name="Expression">Required. Any valid expression.</param>
		/// <param name="Style">Optional. A valid named or user-defined format String expression. </param>
		[MonoTODO("Needs Testing")]
		public static string Format(object expression, [Optional][DefaultValue("")]string style)
		{
			string returnstr=null;
			string expstring=expression.GetType().ToString();;
			switch(expstring)
			{
				case "System.Char":
					if ( style!="")
						throw new System.ArgumentException("'expression' argument has a not valid value");
					returnstr=Convert.ToChar(expression).ToString();
					break;
				case "System.String":
					if (style == "")
						returnstr=expression.ToString();
					else
					{
						switch ( style.ToLower ())
						{
							case "yes/no":
							case "on/off":
								switch (expression.ToString().ToLower())
								{
									case "true":
									case "On":
										if (style.ToLower ()=="yes/no")
											returnstr="Yes"; // TODO : must be translated
										else
											returnstr="On"; // TODO : must be translated
										break;
									case "false":
									case "off":
										if (style.ToLower ()=="yes/no")
											returnstr="No"; // TODO : must be translated
										else
											returnstr="Off"; // TODO : must be translated
										break;
									default:
										throw new System.ArgumentException();

								}
								break;
							default:
								returnstr=style.ToString();
								break;
						}
					}
					break;
				case "System.Boolean":
					if ( style=="")
					{
						if ( Convert.ToBoolean(expression)==true)
							returnstr="True"; // must not be translated
						else
							returnstr="False"; // must not be translated
					}
					else
						returnstr=style;
					break;
				case "System.DateTime":
					returnstr=Convert.ToDateTime(expression).ToString (style) ;
					break;
				case "System.Decimal":	case "System.Byte":	case "System.SByte":
				case "System.Int16":	case "System.Int32":	case "System.Int64":
				case "System.Double":	case "System.Single":	case "System.UInt16":
				case "System.UInt32":	case "System.UInt64":
					switch (style.ToLower ())
					{
						case "yes/no": case "true":	case "false": case "on/off":
							style=style.ToLower();
							double dblbuffer=Convert.ToDouble(expression);
							if (dblbuffer == 0)
							{
								switch (style)
								{
									case "on/off":
										returnstr= "Off";break; // TODO : must be translated
									case "yes/no":
										returnstr= "No";break; // TODO : must be translated
									case "true":
									case "false":
										returnstr= "False";break; // must not be translated
								}
							}
							else
							{
								switch (style)
								{
									case "on/off":
										returnstr="On";break; // TODO : must be translated
									case "yes/no":
										returnstr="Yes";break; // TODO : must be translated
									case "true":
									case "false":
										returnstr="True";break; // must not be translated
								}
							}
							break;
						default:
							switch (expstring)
							{
								case "System.Byte": returnstr=Convert.ToByte(expression).ToString (style);break;
								case "System.SByte": returnstr=Convert.ToSByte(expression).ToString (style);break;
								case "System.Int16": returnstr=Convert.ToInt16(expression).ToString (style);break;
								case "System.UInt16": returnstr=Convert.ToUInt16(expression).ToString (style);break;
								case "System.Int32":  returnstr=Convert.ToInt32(expression).ToString (style);break;
								case "System.UInt32":  returnstr=Convert.ToUInt32(expression).ToString (style);break;
								case "System.Int64":  returnstr=Convert.ToUInt64(expression).ToString (style);break;
								case "System.UInt64":returnstr=Convert.ToUInt64(expression).ToString (style);break;
								case "System.Single": returnstr=Convert.ToSingle(expression).ToString (style);break;
								case "System.Double":  returnstr=Convert.ToDouble(expression).ToString (style);break;
								case "System.Decimal": returnstr=Convert.ToDecimal(expression).ToString (style);break;

							}
							break;
					}
					break;
			}
			if (returnstr==null)
				throw new System.ArgumentException();
			return returnstr;
		}

		/// <summary>
		/// Returns an expression formatted as a currency value using the currency symbol defined in the system control panel.
		/// </summary>
		/// <param name="Expression">Required. Expression to be formatted.</param>
		/// <param name="NumDigitsAfterDecimal">Optional. Numeric value indicating how many places are displayed to the right of the decimal. Default value is 1, which indicates that the computer's regional settings are used.</param>
		/// <param name="IncludeLeadingDigit">Optional. Tristate enumeration that indicates whether or not a leading zero is displayed for fractional values. See Settings for values.</param>
		/// <param name="UseParensForNegativeNumbers">Optional. Tristate enumeration that indicates whether or not to place negative values within parentheses. See Settings for values.</param>
		/// <param name="GroupDigits">Optional. Tristate enumeration that indicates whether or not numbers are grouped using the group delimiter specified in the computer's regional settings. See Settings for values.</param>
		[MonoTODO]
		public static string FormatCurrency(object Expression, 
			[Optional]
			[DefaultValue(-1)] 
			int NumDigitsAfterDecimal, 
			[Optional]
			[DefaultValue(TriState.UseDefault)] 
			TriState IncludeLeadingDigit, 
			[Optional]
			[DefaultValue(TriState.UseDefault)] 
			TriState UseParensForNegativeNumbers, 
			[Optional]
			[DefaultValue(TriState.UseDefault)] 
			TriState GroupDigits)
		{
			//FIXME
			throw new NotImplementedException();
			//throws InvalidCastException
			//throws ArgumentException
		}

		/// <summary>
		/// Returns an expression formatted as a date or time.
		/// </summary>
		/// <param name="Expression">Required. Date expression to be formatted. </param>
		/// <param name="NamedFormat">Optional. Numeric value that indicates the date or time format used. If omitted, GeneralDate is used.</param>
		[MonoTODO]
		public static string FormatDateTime(DateTime Expression, 
			[Optional]
			[DefaultValue(DateFormat.GeneralDate)] 
			DateFormat NamedFormat)
		{
			switch(NamedFormat)
			{
				case DateFormat.GeneralDate:
					//FIXME: WTF should I do with it?
					throw new NotImplementedException(); 	
				case DateFormat.LongDate:  
					return Expression.ToLongDateString();
				case DateFormat.ShortDate:
					return Expression.ToShortDateString();
				case DateFormat.LongTime:
					return Expression.ToLongTimeString();
				case DateFormat.ShortTime:
					return Expression.ToShortTimeString();
				default:
					throw new ArgumentException("Argument 'NamedFormat' must be a member of DateFormat", "NamedFormat");
			}
		}

		/// <summary>
		/// Returns an expression formatted as a number.
		/// </summary>
		/// <param name="Expression">Required. Expression to be formatted.</param>
		/// <param name="NumDigitsAfterDecimal">Optional. Numeric value indicating how many places are displayed to the right of the decimal. Default value is 1, which indicates that the computer's regional settings are used.</param>
		/// <param name="IncludeLeadingDigit">Optional. Tristate enumeration that indicates whether or not a leading zero is displayed for fractional values. See Settings for values.</param>
		/// <param name="UseParensForNegativeNumbers">Optional. Tristate enumeration that indicates whether or not to place negative values within parentheses. See Settings for values.</param>
		/// <param name="GroupDigits">Optional. Tristate enumeration that indicates whether or not numbers are grouped using the group delimiter specified in the computer's regional settings. See Settings for values.</param>
		[MonoTODO]
		public static string FormatNumber(object Expression, 
			[Optional]
			[DefaultValue(-1)] 
			int NumDigitsAfterDecimal, 
			[Optional]
			[DefaultValue(TriState.UseDefault)] 
			TriState IncludeLeadingDigit, 
			[Optional]
			[DefaultValue(TriState.UseDefault)] 
			TriState UseParensForNegativeNumbers, 
			[Optional]
			[DefaultValue(TriState.UseDefault)] 
			TriState GroupDigits)
		{
			//FIXME
			throw new NotImplementedException();
			//throws InvalidCastException
		}

		/// <summary>
		/// Returns an expression formatted as a percentage (that is, multiplied by 100) with a trailing % character.
		/// </summary>
		/// <param name="Expression">Required. Expression to be formatted.</param>
		/// <param name="NumDigitsAfterDecimal">Optional. Numeric value indicating how many places are displayed to the right of the decimal. Default value is 1, which indicates that the computer's regional settings are used.</param>
		/// <param name="IncludeLeadingDigit">Optional. Tristate enumeration that indicates whether or not a leading zero is displayed for fractional values. See Settings for values.</param>
		/// <param name="UseParensForNegativeNumbers">Optional. Tristate enumeration that indicates whether or not to place negative values within parentheses. See Settings for values.</param>
		/// <param name="GroupDigits">Optional. Tristate enumeration that indicates whether or not numbers are grouped using the group delimiter specified in the computer's regional settings. See Settings for values.</param>
		[MonoTODO]
		public static string FormatPercent(object Expression, 
			[Optional]
			[DefaultValue(-1)] 
			int NumDigitsAfterDecimal, 
			[Optional]
			[DefaultValue(TriState.UseDefault)] 
			TriState IncludeLeadingDigit, 
			[Optional]
			[DefaultValue(TriState.UseDefault)] 
			TriState UseParensForNegativeNumbers, 
			[Optional]
			[DefaultValue(TriState.UseDefault)] 
			TriState GroupDigits)
		{
			//FIXME
			throw new NotImplementedException();
			//throws InvalidCastException
		}

		/// <summary>
		/// Returns a Char value representing the character from the specified index in the supplied string.
		/// </summary>
		/// <param name="Str">Required. Any valid String expression.</param>
		/// <param name="Index">Required. Integer expression. The (1-based) index of the character in Str to be returned.</param>
		[MonoTODO("Needs testing")]
		public static char GetChar(string Str, 
			int Index)
		{

			if ((Str == null) || (Str.Length == 0))
				throw new ArgumentException("Length of argument 'Str' must be greater than zero.", "Sre");
			if (Index < 1) 
				throw new ArgumentException("Argument 'Index' must be greater than or equal to 1.", "Index");
			if (Index > Str.Length)
				throw new ArgumentException("Argument 'Index' must be less than or equal to the length of argument 'String'.", "Index");

			return Str.ToCharArray(Index -1, 1)[0];
		}

		/// <summary>
		/// Returns an integer specifying the start position of the first occurrence of one string within another.
		/// </summary>
		/// <param name="Start">Required. Numeric expression that sets the starting position for each search. If omitted, search begins at the first character position. The start index is 1 based.</param>
		/// <param name="String1">Required. String expression being searched.</param>
		/// <param name="String2">Required. String expression sought.</param>
		/// <param name="Compare">Optional. Specifies the type of string comparison. If Compare is omitted, the Option Compare setting determines the type of comparison. Specify a valid LCID (LocaleID) to use locale-specific rules in the comparison. </param>
		[MonoTODO("Needs testing")]
		public static int InStr(string String1, 
			string String2, 
			[OptionCompare]
			[Optional]
			[DefaultValue(CompareMethod.Binary)] 
			CompareMethod Compare)
		{
			return InStr(1, String1, String2, Compare);
		}
		
		/// <summary>
		/// Returns an integer specifying the start position of the first occurrence of one string within another.
		/// </summary>
		/// <param name="Start">Required. Numeric expression that sets the starting position for each search. If omitted, search begins at the first character position. The start index is 1 based.</param>
		/// <param name="String1">Required. String expression being searched.</param>
		/// <param name="String2">Required. String expression sought.</param>
		/// <param name="Compare">Optional. Specifies the type of string comparison. If Compare is omitted, the Option Compare setting determines the type of comparison. Specify a valid LCID (LocaleID) to use locale-specific rules in the comparison. </param>
		[MonoTODO("Needs testing")]
		public static int InStr(int Start, 
			string String1, 
			string String2, 
			[OptionCompare]
			[Optional]
			[DefaultValue(CompareMethod.Binary)] 
			CompareMethod Compare)
		{
			if (Start < 1)
				throw new ArgumentException("Argument 'Start' must be non-negative.", "Start");

			/* 
			 * FIXME: ms-help://MS.VSCC/MS.MSDNVS/vblr7/html/vafctinstr.htm
			 * If Compare is omitted, the Option Compare setting determines the type of comparison. Specify 
			 * a valid LCID (LocaleID) to use locale-specific rules in the comparison.
			 * How do I do this?
			 */

			/*
			 * If									InStr returns 
			 *
			 * String1 is zero length or Nothing	0 
			 * String2 is zero length or Nothing	start 
			 * String2 is not found					0 
			 * String2 is found within String1		Position where match begins 
			 * Start > String2						0 
			 */

			//FIXME: someone with a non US setup should test this.
			switch (Compare)
			{
				case CompareMethod.Text:
					return System.Globalization.CultureInfo.CurrentCulture.CompareInfo.IndexOf(String2, String1, Start - 1) + 1;

				case CompareMethod.Binary:
					return String1.IndexOf(String2, Start - 1) + 1;
				default:
					throw new System.ArgumentException("Argument 'Compare' must be CompareMethod.Binary or CompareMethod.Text.", "Compare");
			}


		}

		/// <summary>
		/// Returns the position of the first occurrence of one string within another, starting from the right side of the string.
		/// </summary>
		/// <param name="StringCheck">Required. String expression being searched.</param>
		/// <param name="StringMatch">Required. String expression being searched for.</param>
		/// <param name="Start">Optional. Numeric expression that sets the one-based starting position for each search, starting from the left side of the string. If Start is omitted, 1 is used, which means that the search begins at the last character position. Search then proceeds from right to left.</param>
		/// <param name="Compare">Optional. Numeric value indicating the kind of comparison to use when evaluating substrings. If omitted, a binary comparison is performed. See Settings for values.</param>
		[MonoTODO]
		public static int InStrRev(string StringCheck, 
			string StringMatch, 
			string String2, 
			[Optional]
			[DefaultValue(-1)] 
			int Start,
			[OptionCompare]  
			[Optional]
			[DefaultValue(CompareMethod.Binary)] 
			CompareMethod Compare)
		{
			if ((Start == 0) || (Start < -1))
				throw new ArgumentException("Argument 'Start' must be greater than 0 or equal to -1", "Start");
 
			//FIXME: Use LastIndexOf()
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns a string created by joining a number of substrings contained in an array.
		/// </summary>
		/// <param name="SourceArray">Required. One-dimensional array containing substrings to be joined.</param>
		/// <param name="Delimiter">Optional. String used to separate the substrings in the returned string. If omitted, the space character (" ") is used. If Delimiter is a zero-length string (""), all items in the list are concatenated with no delimiters.</param>
		[MonoTODO("Needs testing")]
		public static string Join(string[] SourceArray, 
			[Optional]
			[DefaultValue(" ")] 
			string Delimiter)
		{
			if (SourceArray == null)
				throw new ArgumentException("Argument 'SourceArray' can not be null.", "SourceArray");
			if (SourceArray.Rank > 1)
				throw new ArgumentException("Argument 'SourceArray' can have only one dimension.", "SourceArray");

			return string.Join(Delimiter, SourceArray);
		}
		/// <summary>
		/// Returns a string created by joining a number of substrings contained in an array.
		/// </summary>
		/// <param name="SourceArray">Required. One-dimensional array containing substrings to be joined.</param>
		/// <param name="Delimiter">Optional. String used to separate the substrings in the returned string. If omitted, the space character (" ") is used. If Delimiter is a zero-length string (""), all items in the list are concatenated with no delimiters.</param>
		[MonoTODO("Needs testing")]
		public static string Join(object[] SourceArray, 
			[Optional]
			[DefaultValue(" ")] 
			string Delimiter)
		{

			if (SourceArray == null)
				throw new ArgumentException("Argument 'SourceArray' can not be null.", "SourceArray");
			if (SourceArray.Rank > 1)
				throw new ArgumentException("Argument 'SourceArray' can have only one dimension.", "SourceArray");

			string[] dest;
			dest = new string[SourceArray.Length];

			SourceArray.CopyTo(dest, 0);
			return string.Join(Delimiter, dest);
		}

		/// <summary>
		/// Returns a string or character converted to lowercase.
		/// </summary>
		/// <param name="Value">Required. Any valid String or Char expression.</param>
		[MonoTODO("Needs testing")]
		public static char LCase(char Value) 
		{
			return char.ToLower(Value);
		}

		/// <summary>
		/// Returns a string or character converted to lowercase.
		/// </summary>
		/// <param name="Value">Required. Any valid String or Char expression.</param>
		[MonoTODO("Needs testing")]
		public static string LCase(string Value) 
		{
			if ((Value == null) || (Value.Length == 0))
				return String.Empty; // VB.net does this.

			return Value.ToLower();
		}

		
		/// <summary>
		/// Returns a string containing a specified number of characters from the left side of a string.
		/// </summary>
		/// <param name="Str">Required. String expression from which the leftmost characters are returned.</param>
		/// <param name="Length">Required. Integer expression. Numeric expression indicating how many characters to return. If 0, a zero-length string ("") is returned. If greater than or equal to the number of characters in Str, the entire string is returned.</param>
		[MonoTODO]
		public static string Left(string Str, 
			int Length) 
		{
			if (Length < 0)
				throw new ArgumentException("Argument 'Length' must be non-negative.", "Length");
			if ((Str == null) || (Str.Length == 0))
				return String.Empty; // VB.net does this.

			return Str.Substring(0, Length);
		}

		/// <summary>
		/// Returns an integer containing either the number of characters in a string or the number of bytes required to store a variable.
		/// </summary>
		/// <param name="Expression">Any valid String expression or variable name. If Expression is of type Object, the Len function returns the size as it will be written to the file.</param>
		[MonoTODO("Needs testing")]
		public static int Len(bool Expression) 
		{
			return 2; //sizeof(bool)
		}

		/// <summary>
		/// Returns an integer containing either the number of characters in a string or the number of bytes required to store a variable.
		/// </summary>
		/// <param name="Expression">Any valid String expression or variable name. If Expression is of type Object, the Len function returns the size as it will be written to the file.</param>
		[MonoTODO("Needs testing")]
		public static int Len(byte Expression) 
		{
			return 1; //sizeof(byte)
		}
		
		/// <summary>
		/// Returns an integer containing either the number of characters in a string or the number of bytes required to store a variable.
		/// </summary>
		/// <param name="Expression">Any valid String expression or variable name. If Expression is of type Object, the Len function returns the size as it will be written to the file.</param>
		[MonoTODO("Needs testing")]
		public static int Len(char Expression) 
		{
			return 2; //sizeof(char)
		}
		
		/// <summary>
		/// Returns an integer containing either the number of characters in a string or the number of bytes required to store a variable.
		/// </summary>
		/// <param name="Expression">Any valid String expression or variable name. If Expression is of type Object, the Len function returns the size as it will be written to the file.</param>
		[MonoTODO("Needs testing")]
		public static int Len(double Expression) 
		{
			return 8; //sizeof(double)
		}
		
		/// <summary>
		/// Returns an integer containing either the number of characters in a string or the number of bytes required to store a variable.
		/// </summary>
		/// <param name="Expression">Any valid String expression or variable name. If Expression is of type Object, the Len function returns the size as it will be written to the file.</param>
		[MonoTODO("Needs testing")]
		public static int Len(int Expression) 
		{
			return 4; //sizeof(int)
		}
		
		/// <summary>
		/// Returns an integer containing either the number of characters in a string or the number of bytes required to store a variable.
		/// </summary>
		/// <param name="Expression">Any valid String expression or variable name. If Expression is of type Object, the Len function returns the size as it will be written to the file.</param>
		[MonoTODO("Needs testing")]
		public static int Len(long Expression) 
		{
			return 8; //sizeof(long)
		}

		/// <summary>
		/// Returns an integer containing either the number of characters in a string or the number of bytes required to store a variable.
		/// </summary>
		/// <param name="Expression">Any valid String expression or variable name. If Expression is of type Object, the Len function returns the size as it will be written to the file.</param>
		[MonoTODO]
		public static int Len(object Expression) 
		{
			 // FIXME: 
			 // With user-defined types and Object variables, the Len function returns the size as it will 
			 // be written to the file. If an Object contains a String, it will return the length of the string. 
			 // If an Object contains any other type, it will return the size of the object as it will be written 
			 // to the file.
			throw new NotImplementedException(); 
		}
		
		/// <summary>
		/// Returns an integer containing either the number of characters in a string or the number of bytes required to store a variable.
		/// </summary>
		/// <param name="Expression">Any valid String expression or variable name. If Expression is of type Object, the Len function returns the size as it will be written to the file.</param>
		[MonoTODO("Needs testing")]
		public static int Len(short Expression) 
		{
			return 2; //sizeof(short)
		}
		
		/// <summary>
		/// Returns an integer containing either the number of characters in a string or the number of bytes required to store a variable.
		/// </summary>
		/// <param name="Expression">Any valid String expression or variable name. If Expression is of type Object, the Len function returns the size as it will be written to the file.</param>
		[MonoTODO("Needs testing")]
		public static int Len(Single Expression) 
		{
			return 4; //sizeof(Single)
		}
		
		/// <summary>
		/// Returns an integer containing either the number of characters in a string or the number of bytes required to store a variable.
		/// </summary>
		/// <param name="Expression">Any valid String expression or variable name. If Expression is of type Object, the Len function returns the size as it will be written to the file.</param>
		[MonoTODO("Needs testing")]
		public static int Len(string Expression) 
		{
			return Expression.Length; //length of the string
		}
		
		/// <summary>
		/// Returns an integer containing either the number of characters in a string or the number of bytes required to store a variable.
		/// </summary>
		/// <param name="Expression">Any valid String expression or variable name. If Expression is of type Object, the Len function returns the size as it will be written to the file.</param>
		[MonoTODO("Needs testing")]
		public static int Len(DateTime Expression) 
		{
			return 8; //sizeof(DateTime)
		}
		
		/// <summary>
		/// Returns an integer containing either the number of characters in a string or the number of bytes required to store a variable.
		/// </summary>
		/// <param name="Expression">Any valid String expression or variable name. If Expression is of type Object, the Len function returns the size as it will be written to the file.</param>
		[MonoTODO("Needs testing")]
		public static int Len(decimal Expression) 
		{
			return 16; //sizeof(decimal)
		}

		/// <summary>
		/// Returns a left-aligned string containing the specified string adjusted to the specified length.
		/// </summary>
		/// <param name="Source">Required. String expression. Name of string variable.</param>
		/// <param name="Length">Required. Integer expression. Length of returned string.</param>
		[MonoTODO("Needs testing")]
		public static string LSet(string Source, 
			int Length) 
		{
			if (Length < 0)
				throw new ArgumentOutOfRangeException("Length", "Length must be must be non-negative.");
			if (Source == null)
				Source = String.Empty;

			return Source.PadRight(Length);
		}

		/// <summary>
		/// Returns a string containing a copy of a specified string with no leading spaces.
		/// </summary>
		/// <param name="Str">Required. Any valid String expression.</param>
		[MonoTODO("Needs testing")]
		public static string LTrim(string Str) 
		{
			if ((Str == null) || (Str.Length == 0))
				return String.Empty; // VB.net does this.

			return Str.TrimStart(null);
		}

		/// <summary>
		/// Returns a string containing a copy of a specified string with no trailing spaces.
		/// </summary>
		/// <param name="Str">Required. Any valid String expression.</param>
		[MonoTODO("Needs testing")]
		public static string RTrim(string Str) 
		{
			if ((Str == null) || (Str.Length == 0))
				return String.Empty; // VB.net does this.

			return Str.TrimEnd(null);
		}
	
		/// <summary>
		/// Returns a string containing a copy of a specified string with no leading or trailing spaces.
		/// </summary>
		/// <param name="Str">Required. Any valid String expression.</param>
		[MonoTODO("Needs testing")]
		public static string Trim(string Str) 
		{
			if ((Str == null) || (Str.Length == 0))
				return String.Empty; // VB.net does this.
			
			return Str.Trim();
		}

		/// <summary>
		/// Returns a string containing a specified number of characters from a string.
		/// </summary>
		/// <param name="Str">Required. String expression from which characters are returned.</param>
		/// <param name="Start">Required. Integer expression. Character position in Str at which the part to be taken starts. If Start is greater than the number of characters in Str, the Mid function returns a zero-length string (""). Start is one based.</param>
		/// <param name="Length">Required Integer expression. Number of characters to return. If there are fewer than Length characters in the text (including the character at position Start), all characters from the start position to the end of the string are returned.</param>
		[MonoTODO("Verify if this is the correct behaviour for Length==0...[Rafael]")]
		public static string Mid(string Str, 
			int Start, 
			int Length)
		{

			if (Length < 0)
				throw new System.ArgumentException("Argument 'Length' must be greater or equal to zero.", "Length");
			if (Start <= 0)
				throw new System.ArgumentException("Argument 'Start' must be greater than zero.", "Start");
			if ((Str == null) || (Str.Length == 0))
				return String.Empty; // VB.net does this.

			if ((Length == 0) || (Start > Str.Length))
				return String.Empty;

			if (Length > (Str.Length - Start))
				Length = (Str.Length - Start) + 1;

			return Str.Substring(Start - 1, Length);

		}

		/// <summary>
		/// Returns a string containing all characters from a string beyond an start point.
		/// </summary>
		/// <param name="Str">Required. String expression from which characters are returned.</param>
		/// <param name="Start">Required. Integer expression. Character position in Str at which the part to be taken starts. If Start is greater than the number of characters in Str, the Mid function returns a zero-length string (""). Start is one based.</param>
		[MonoTODO("Needs testing")]
		public static string Mid (string Str, int Start) 
		{
			if (Start <= 0)
				throw new System.ArgumentException("Argument 'Start' must be greater than zero.", "Start");
			if ((Str == null) || (Str.Length == 0))
				return String.Empty; // VB.net does this.

			if (Start > Str.Length)
				return String.Empty;

			return Str.Substring(Start - 1);
		}

		/// <summary>
		/// Returns a string in which a specified substring has been replaced with another substring a specified number of times.
		/// </summary>
		/// <param name="Expression">Required. String expression containing substring to replace.</param>
		/// <param name="Find">Required. Substring being searched for.</param>
		/// <param name="Replacement">Required. Replacement substring.</param>
		/// <param name="Start">Optional. Position within Expression where substring search is to begin. If omitted, 1 is assumed.</param>
		/// <param name="Count">Optional. Number of substring substitutions to perform. If omitted, the default value is 1, which means make all possible substitutions.</param>
		/// <param name="Compare">Optional. Numeric value indicating the kind of comparison to use when evaluating substrings. See Settings for values.</param>
		[MonoTODO("Needs testing")]
		public static string Replace(string Expression, 
			string Find, 
			string Replacement, 
			[Optional]
			[DefaultValue(1)] 
			int Start,
			[Optional]
			[DefaultValue(-1)] 
			int Count,
			[OptionCompare] 
			[Optional]
			[DefaultValue(CompareMethod.Binary)] 
			CompareMethod Compare)
		{

			if (Count < -1)
				throw new ArgumentException("Argument 'Count' must be greater than or equal to -1.", "Count");
			if (Start <= 0)
				throw new ArgumentException("Argument 'Start' must be greater than zero.", "Start");

			if ((Expression == null) || (Expression.Length == 0))
				return String.Empty; // VB.net does this.
			if ((Find == null) || (Find.Length == 0))
				return Expression; // VB.net does this.
			if (Replacement == null)
				Replacement = String.Empty; // VB.net does this.

			return Expression.Replace(Find, Replacement);
		}
 
		/// <summary>
		/// Returns a string containing a specified number of characters from the right side of a string.
		/// </summary>
		/// <param name="Str">Required. String expression from which the rightmost characters are returned.</param>
		/// <param name="Length">Required. Integer. Numeric expression indicating how many characters to return. If 0, a zero-length string ("") is returned. If greater than or equal to the number of characters in Str, the entire string is returned.</param>
		[MonoTODO("Needs testing")]
		public static string Right(string Str, 
			int Length) 
		{
			if (Length < 0)
				throw new ArgumentException("Argument 'Length' must be greater or equal to zero.", "Length");

			return Str.Substring (Str.Length - Length);
		}

		/// <summary>
		/// Returns a right-aligned string containing the specified string adjusted to the specified length.
		/// </summary>
		/// <param name="Source">Required. String expression. Name of string variable.</param>
		/// <param name="Length">Required. Integer expression. Length of returned string.</param>
		[MonoTODO("Needs testing")]
		public static string RSet(string Source, 
			int Length) 
		{
		
			if (Source == null)
				Source = String.Empty;
			if (Length < 0)
				throw new ArgumentOutOfRangeException("Length", "Length must be non-negative.");

			return Source.PadLeft(Length);
		}

		/// <summary>
		/// Returns a string consisting of the specified number of spaces.
		/// </summary>
		/// <param name="Number">Required. Integer expression. The number of spaces you want in the string.</param>
		[MonoTODO("Needs testing")]
		public static string Space(int Number) 
		{
			if (Number < 0)
				throw new ArgumentException("Argument 'Number' must be greater or equal to zero.", "Number");

			return new string((char) ' ', Number);
		}

		/// <summary>
		/// Returns a zero-based, one-dimensional array containing a specified number of substrings.
		/// </summary>
		/// <param name="Expression">Required. String expression containing substrings and delimiters. If Expression is a zero-length string (""), the Split function returns an array with no elements and no data.</param>
		/// <param name="Delimiter">Optional. Single character used to identify substring limits. If Delimiter is omitted, the space character (" ") is assumed to be the delimiter. If Delimiter is a zero-length string, a single-element array containing the entire Expression string is returned.</param>
		/// <param name="Limit">Optional. Number of substrings to be returned; the default, 1, indicates that all substrings are returned.</param>
		/// <param name="Compare">Optional. Numeric value indicating the comparison to use when evaluating substrings. See Settings for values.</param>
		[MonoTODO]
		public static string[] Split(string Expression, 
			[Optional]
			[DefaultValue(" ")] 
			string Delimiter,
			[Optional]
			[DefaultValue(-1)] 
			int Limit,
			[OptionCompare] 
			[Optional]
			[DefaultValue(CompareMethod.Binary)] 
			CompareMethod Compare)
		{

			
			if (Expression == null)
				return new string[0];
			if ((Delimiter == null) || (Delimiter.Length == 0))
			{
				string [] ret = new string[0];
				ret[0] = Expression;
				return ret;
			}
			if (Limit == 0)
				Limit = 1; // VB.net does this. I call it a bug.

			/*
			 * FIXME: VB.net does NOT do this. It simply fails with AritmethicException.
			 * What should I do?
			 */
			if (Limit < -1)
				throw new ArgumentOutOfRangeException("Limit", "Argument 'Limit' must be -1 or greater than zero.");

			switch (Compare)
			{
				case CompareMethod.Binary:
					return Expression.Split(Delimiter.ToCharArray(0, 1), Limit);
				case CompareMethod.Text:
					//FIXME
					throw new NotImplementedException();
				default:
					throw new System.ArgumentException("Argument 'Compare' must be CompareMethod.Binary or CompareMethod.Text.", "Compare");
			}

			
		}

		/// <summary>
		/// Returns -1, 0, or 1, based on the result of a string comparison. 
		/// </summary>
		/// <param name="String1">Required. Any valid String expression.</param>
		/// <param name="String2">Required. Any valid String expression.</param>
		/// <param name="Compare">Optional. Specifies the type of string comparison. If compare is omitted, the Option Compare setting determines the type of comparison.</param>
		[MonoTODO("Needs testing")]
		public static int StrComp(string String1, 
			string String2,
			[OptionCompare] 
			[Optional]
			[DefaultValue(CompareMethod.Binary)] 
			CompareMethod Compare)
		{
			
			switch (Compare)
			{
				case CompareMethod.Binary:
					return string.Compare(String1, String2, true);
				case CompareMethod.Text:
					//FIXME: someone with a non US setup should test this.
					return System.Globalization.CultureInfo.CurrentCulture.CompareInfo.Compare(String1, String2);
				default:
					throw new System.ArgumentException("Argument 'Compare' must be CompareMethod.Binary or CompareMethod.Text", "Compare");
			}

		}

		/// <summary>
		/// Returns a string converted as specified.
		/// </summary>
		/// <param name="Str">Required. String expression to be converted.</param>
		/// <param name="Conversion">Required. VbStrConv member. The enumeration value specifying the type of conversion to perform. </param>
		/// <param name="LocaleID">Optional. The LocaleID value, if different from the system LocaleID value. (The system LocaleID value is the default.)</param>
		[MonoTODO("Not impemented")]
		public static string StrConv (string str, 
			VbStrConv Conversion, 
			[Optional]
			[DefaultValue(0)]
			int LocaleID)
		{
			//FIXME
			throw new NotImplementedException(); 
			//throws ArgumentException
		}

		/// <summary>
		/// Returns a string or object consisting of the specified character repeated the specified number of times.
		/// </summary>
		/// <param name="Number">Required. Integer expression. The length to the string to be returned.</param>
		/// <param name="Character">Required. Any valid Char, String, or Object expression. Only the first character of the expression will be used. If Character is of type Object, it must contain either a Char or a String value.</param>
		[MonoTODO("Needs testing")]
		public static string StrDup(int Number, 
			char Character)
		{
			if (Number < 0)
				throw new ArgumentException("Argument 'Number' must be non-negative.", "Number");

			return new string(Character, Number);
		}
		/// <summary>
		/// Returns a string or object consisting of the specified character repeated the specified number of times.
		/// </summary>
		/// <param name="Number">Required. Integer expression. The length to the string to be returned.</param>
		/// <param name="Character">Required. Any valid Char, String, or Object expression. Only the first character of the expression will be used. If Character is of type Object, it must contain either a Char or a String value.</param>
		[MonoTODO("Needs testing")]
		public static string StrDup(int Number, 
			string Character)
		{
			if (Number < 0)
				throw new ArgumentException("Argument 'Number' must be greater or equal to zero.", "Number");
			if ((Character == null) || (Character.Length == 0))
				throw new ArgumentNullException("Character", "Length of argument 'Character' must be greater than zero.");

			return new string(Character.ToCharArray()[0], Number);
		}

		/// <summary>
		/// Returns a string or object consisting of the specified character repeated the specified number of times.
		/// </summary>
		/// <param name="Number">Required. Integer expression. The length to the string to be returned.</param>
		/// <param name="Character">Required. Any valid Char, String, or Object expression. Only the first character of the expression will be used. If Character is of type Object, it must contain either a Char or a String value.</param>
		[MonoTODO("Needs testing")]
		public static object StrDup(int Number, 
			object Character)
		{
			if (Number < 0)
				throw new ArgumentException("Argument 'Number' must be non-negative.", "Number");
			
			if (Character is string)
			{
				string sCharacter = (string) Character;
				if ((sCharacter == null) || (sCharacter.Length == 0))
					throw new ArgumentNullException("Character", "Length of argument 'Character' must be greater than zero.");

				return StrDup(Number, sCharacter);
			}
			else
			{
				if (Character is char)
				{
					return StrDup(Number, (char) Character);
				}
				else
				{
					// "If Character is of type Object, it must contain either a Char or a String value."
					throw new ArgumentException("Argument 'Character' is not a valid value.", "Character");
				}
			}
		}

		/// <summary>
		/// Returns a string in which the character order of a specified string is reversed.
		/// </summary>
		/// <param name="Expression">Required. String expression whose characters are to be reversed. If Expression is a zero-length string (""), a zero-length string is returned.</param>
		public static string StrReverse(string Expression)
		{
			// patched by Daniel Campos 
			// danielcampos@myway.com
			if (Expression != null)
			{
				if ( Expression.Length>0)
				{
					int counter=0;
					char[] buf=new char[Expression.Length];

					for (int backwardsCounter=Expression.Length - 1;
					backwardsCounter>=0;
					backwardsCounter--)
						buf[counter++]=Expression[backwardsCounter];
					return new string(buf);
				}
				else
					return String.Empty;
			}
			else
				return String.Empty;
		}

		/// <summary>
		/// Returns a string or character containing the specified string converted to uppercase.
		/// </summary>
		/// <param name="Value">Required. Any valid String or Char expression.</param>
		public static char UCase(char Value) 
		{
			return char.ToUpper(Value);
		}

		/// <summary>
		/// Returns a string or character containing the specified string converted to uppercase.
		/// </summary>
		/// <param name="Value">Required. Any valid String or Char expression.</param>
		public static string UCase(string Value) 
		{
			if ((Value == null) || (Value.Length == 0))
				return String.Empty; // VB.net does this. 

			return Value.ToUpper();
		}



	}

}
