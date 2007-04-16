//
// System.Char.cs
//
// Authors:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Miguel de Icaza (miguel@ximian.com)
//   Jackson Harper (jackson@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

// Note about the ToString()'s. ECMA says there's only a ToString() method, 
// BUT it is just a wrapper for ToString(null). However there is no other ToString
// in the docs. Turning to the NET framework sdk reveals that there is a 
// ToString(formatprovider) method, as well as a 'static ToString (char c)' method, 
// which appears to just be a Convert.ToString(char c) type method. ECMA also
// doesn't list this class as implementing IFormattable.

using System.Globalization;
using System.Runtime.CompilerServices;
#if NET_2_0
using System.Runtime.InteropServices;
#endif

namespace System
{
	[Serializable]
#if NET_2_0
	[ComVisible (true)]
	public struct Char : IComparable, IConvertible, IComparable <char>, IEquatable <char>
#else
	public struct Char : IComparable, IConvertible
#endif
	{
		public const char MaxValue = (char) 0xffff;
		public const char MinValue = (char) 0;

		internal char m_value;

		static Char ()
		{
			unsafe {
				GetDataTablePointers (out category_data, out numeric_data, out numeric_data_values,
					out to_lower_data_low, out to_lower_data_high, out to_upper_data_low, out to_upper_data_high);
			}
		}

		private readonly unsafe static byte *category_data;
		private readonly unsafe static byte *numeric_data;
		private readonly unsafe static double *numeric_data_values;
		private readonly unsafe static ushort *to_lower_data_low;
		private readonly unsafe static ushort *to_lower_data_high;
		private readonly unsafe static ushort *to_upper_data_low;
		private readonly unsafe static ushort *to_upper_data_high;

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		private unsafe static extern void GetDataTablePointers (out byte *category_data,
				out byte *numeric_data, out double *numeric_data_values,
				out ushort *to_lower_data_low, out ushort *to_lower_data_high,
				out ushort *to_upper_data_low, out ushort *to_upper_data_high);

		public int CompareTo (object v)
		{
			if (v == null)
				return 1;
			
			if (!(v is System.Char))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Char"));

			char xv = (char) v;
			if (m_value == xv)
				return 0;

			if (m_value > xv)
				return 1;
			else
				return -1;
		}

		public override bool Equals (object o)
		{
			if (!(o is Char))
				return false;

			return ((char) o) == m_value;
		}

#if NET_2_0
		public int CompareTo (char value)
		{
			if (m_value == value)
				return 0;

			if (m_value > value)
				return 1;
			else
				return -1;
		}

		public static string ConvertFromUtf32 (int utf32)
		{
			if (utf32 < 0 || utf32 > 0x10FFFF)
				throw new ArgumentOutOfRangeException ("utf32", "The argument must be from 0 to 0x10FFFF.");
			if (0xD800 <= utf32 && utf32 <= 0xDFFF)
				throw new ArgumentOutOfRangeException ("utf32", "The argument must not be in surrogate pair range.");
			if (utf32 < 0x10000)
				return new string ((char) utf32, 1);
			utf32 -= 0x10000;
			return new string (
				new char [] {(char) ((utf32 >> 10) + 0xD800),
				(char) (utf32 % 0x0400 + 0xDC00)});
		}

		public static int ConvertToUtf32 (char highSurrogate, char lowSurrogate)
		{
			if (highSurrogate < 0xD800 || 0xDBFF < highSurrogate)
				throw new ArgumentOutOfRangeException ("highSurrogate");
			if (lowSurrogate < 0xDC00 || 0xDFFF < lowSurrogate)
				throw new ArgumentOutOfRangeException ("lowSurrogate");

			return 0x10000 + ((highSurrogate - 0xD800) << 10) + (lowSurrogate - 0xDC00);
		}

		public static int ConvertToUtf32 (string s, int index)
		{
			if (s == null)
				throw new ArgumentNullException ("s");
			if (index < 0 || index >= s.Length)
				throw new ArgumentOutOfRangeException ("index");
			if (!Char.IsSurrogate (s [index]))
				return s [index];
			if (!Char.IsHighSurrogate (s [index])
			    || index == s.Length - 1
			    || !Char.IsLowSurrogate (s [index + 1]))
				throw new ArgumentException (String.Format ("The string contains invalid surrogate pair character at {0}", index));
			return ConvertToUtf32 (s [index], s [index + 1]);
		}

		public bool Equals (char value)
		{
			return m_value == value;
		}

		public static bool IsSurrogatePair (char high, char low)
		{
			return '\uD800' <= high && high <= '\uDBFF'
				&& '\uDC00' <= low && low <= '\uDFFF';
		}

		public static bool IsSurrogatePair (string s, int index)
		{
			if (s == null)
				throw new ArgumentNullException ("s");
			if (index < 0 || index >= s.Length)
				throw new ArgumentOutOfRangeException ("index");
			return index + 1 < s.Length && IsSurrogatePair (s [index], s [index + 1]);
		}
#endif

		public override int GetHashCode ()
		{
			return m_value;
		}

		public static double GetNumericValue (char c)
		{
			if (c > (char)0x3289) {
				if (c >= (char)0xFF10 && c <= (char)0xFF19)
					return (c - 0xFF10); // Numbers 0-9

				// Default not set data
				return -1;
			}
			else {
				unsafe {
					return numeric_data_values [numeric_data [c]];
				}
			}
			
		}

		public static double GetNumericValue (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("str");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
					"The value of index is less than zero, or greater than or equal to the length of str."));	
			
			return GetNumericValue (str[index]);
		}

		public static UnicodeCategory GetUnicodeCategory (char c)
		{
			unsafe {
				return (UnicodeCategory)(category_data [c]);
			}
		}

		public static UnicodeCategory GetUnicodeCategory (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("str");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
					"The value of index is less than zero, or greater than or equal to the length of str."));
			
			return GetUnicodeCategory (str[index]);
		}

		public static bool IsControl (char c)
		{
			unsafe {
				return (category_data [c] == (byte)UnicodeCategory.Control);
			}
		}

		public static bool IsControl (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("str");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
					"The value of index is less than zero, or greater than or equal to the length of str."));
			
			return IsControl (str[index]);
		}	

		public static bool IsDigit (char c)
		{
			unsafe {
				return (category_data [c] == (byte)UnicodeCategory.DecimalDigitNumber);
			}
		}

		public static bool IsDigit (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("str");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
					"The value of index is less than zero, or greater than or equal to the length of str."));
			
			return IsDigit (str[index]);
		}

#if NET_2_0
		public static bool IsHighSurrogate (char c)
		{
			return c >= '\uD800' && c <= '\uDBFF';
		}

		public static bool IsHighSurrogate (string s, int index)
		{
			if (s == null) 
				throw new ArgumentNullException ("s");
			
			if (index < 0 || index >= s.Length)
				throw new ArgumentOutOfRangeException ("index");
			
			return IsHighSurrogate (s [index]);
		}
#endif

		public static bool IsLetter (char c)
		{
			unsafe {
				return category_data [c] <= ((byte)UnicodeCategory.OtherLetter);
			}
		}

		public static bool IsLetter (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("str");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
					"The value of index is less than zero, or greater than or equal to the length of str."));
			
			return IsLetter (str[index]);
		}

		public static bool IsLetterOrDigit (char c)
		{
			unsafe {
				UnicodeCategory Category = (UnicodeCategory)category_data [c];
				switch (Category) {
				case UnicodeCategory.DecimalDigitNumber:
				case UnicodeCategory.UppercaseLetter:
				case UnicodeCategory.LowercaseLetter:
				case UnicodeCategory.TitlecaseLetter:
				case UnicodeCategory.ModifierLetter:
				case UnicodeCategory.OtherLetter:
					return true;
				default:
					return false;
				}
			}
		}

		public static bool IsLetterOrDigit (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("str");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
					"The value of index is less than zero, or greater than or equal to the length of str."));
			
			return IsLetterOrDigit (str[index]);
		}

		public static bool IsLower (char c)
		{
			unsafe {
				return (category_data [c] == (byte)UnicodeCategory.LowercaseLetter);
			}
		}
		
		public static bool IsLower (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("str");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
					"The value of index is less than zero, or greater than or equal to the length of str."));
			
			return IsLower (str[index]);
		}

#if NET_2_0
		public static bool IsLowSurrogate (char c)
		{
			return c >= '\uDC00' && c <= '\uDFFF';
		}

		public static bool IsLowSurrogate (string s, int index)
		{
			if (s == null) 
				throw new ArgumentNullException ("s");
			
			if (index < 0 || index >= s.Length)
				throw new ArgumentOutOfRangeException ("index");
			
			return IsLowSurrogate (s [index]);
		}
#endif

		public static bool IsNumber (char c)
		{
			unsafe {
				UnicodeCategory Category = (UnicodeCategory)category_data [c];
				switch (Category) {
				case UnicodeCategory.DecimalDigitNumber:
				case UnicodeCategory.LetterNumber:
				case UnicodeCategory.OtherNumber:
					return true;
				default:
					return false;
				}
			}
		}
		
		public static bool IsNumber (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("str");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
					"The value of index is less than zero, or greater than or equal to the length of str."));
			
			return IsNumber (str[index]);
		}

		public static bool IsPunctuation (char c)
		{
			unsafe {
				UnicodeCategory Category = (UnicodeCategory)category_data [c];
				switch (Category) {
				case UnicodeCategory.ConnectorPunctuation:
				case UnicodeCategory.DashPunctuation:
				case UnicodeCategory.OpenPunctuation:
				case UnicodeCategory.ClosePunctuation:
				case UnicodeCategory.InitialQuotePunctuation:
				case UnicodeCategory.FinalQuotePunctuation:
				case UnicodeCategory.OtherPunctuation:
					return true;
				default:
					return false;
				}
			}
		}
		
		public static bool IsPunctuation (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("str");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
					"The value of index is less than zero, or greater than or equal to the length of str."));
			
			return IsPunctuation (str[index]);
		}

		public static bool IsSeparator (char c)
		{
			unsafe {
				UnicodeCategory Category = (UnicodeCategory)category_data [c];
				switch (Category) {
				case UnicodeCategory.SpaceSeparator:
				case UnicodeCategory.LineSeparator:
				case UnicodeCategory.ParagraphSeparator:
					return true;
				default:
					return false;
				}
			}
		}
		
		public static bool IsSeparator (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("str");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
					"The value of index is less than zero, or greater than or equal to the length of str."));
			
			return IsSeparator (str[index]);
		}

		public static bool IsSurrogate (char c)
		{
			unsafe {
				return (category_data [c] == (byte)UnicodeCategory.Surrogate);
			}
		}
		
		public static bool IsSurrogate (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("str");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
					"The value of index is less than zero, or greater than or equal to the length of str."));
			
			return IsSurrogate (str[index]);
		}

		public static bool IsSymbol (char c)
		{
			unsafe {
				UnicodeCategory Category = (UnicodeCategory)category_data [c];
				switch (Category) {
				case UnicodeCategory.MathSymbol:
				case UnicodeCategory.CurrencySymbol:
				case UnicodeCategory.ModifierSymbol:
				case UnicodeCategory.OtherSymbol:
					return true;
				default:
					return false;
				}
			}
		}
		
		public static bool IsSymbol (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("str");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
					"The value of index is less than zero, or greater than or equal to the length of str."));
			
			return IsSymbol (str[index]);
		}

		public static bool IsUpper (char c)
		{
			unsafe {
				return (category_data [c] == (byte)UnicodeCategory.UppercaseLetter);
			}
		}
		
		public static bool IsUpper (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("str");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
					"The value of index is less than zero, or greater than or equal to the length of str."));
			
			return IsUpper (str[index]);
		}

		public static bool IsWhiteSpace (char c)
		{
			unsafe {
				if (category_data [c] == (byte)UnicodeCategory.SpaceSeparator)
					return true;
				
				switch (c) {
				case (char)0x9:
				case (char)0x0a:
				case (char)0x0b:
				case (char)0x0c:
				case (char)0x0d:
				case (char)0x85: // NEL 
				case (char)0x2028: // Line Separator
				case (char)0x2029: // Paragraph Separator
#if NET_2_0
				case (char)0x205F: // Medium Mathematical Space
#endif
					return true;
				default:
					return false;
				}
			}
		}
		
		public static bool IsWhiteSpace (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("str");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
					"The value of index is less than zero, or greater than or equal to the length of str."));
			
			return IsWhiteSpace (str[index]);
		}

#if NET_2_0
		public static bool TryParse (string s, out char result)
		{
			if (s == null || s.Length != 1) {
				result = (char) 0;
				return false;
			}

			result = s [0];
			return true;
		}
#endif

		public static char Parse (string str)
		{
			if (str == null) 
				throw new ArgumentNullException ("str");

			if (str.Length != 1)
				throw new FormatException ("string contains more than one character.");
			
			return str [0];
		}

		public static char ToLower (char c)
		{
			return ToLower (c, CultureInfo.CurrentCulture);
		}

#if NET_2_0
		public static char ToLowerInvariant (char c)
#else
		internal static char ToLowerInvariant (char c)
#endif
		{
			unsafe {
				if (c <= ((char)0x24cf))
					return (char) to_lower_data_low [c];
				if (c >= ((char)0xff21))
					return (char) to_lower_data_high[c - 0xff21];
			}
			return c;
		}

		public static char ToLower (char c, CultureInfo culture)
		{
			if (culture == null)
				throw new ArgumentNullException ("culture");
			if (culture.LCID == 0x007F) // Invariant
				return ToLowerInvariant (c);

			return culture.TextInfo.ToLower (c);
		}

		public static char ToUpper (char c)
		{
			return ToUpper (c, CultureInfo.CurrentCulture);
		}

#if NET_2_0
		public static char ToUpperInvariant (char c)
#else
		internal static char ToUpperInvariant (char c)
#endif
		{
			unsafe {
				if (c <= ((char)0x24e9))
					return (char) to_upper_data_low [c];
				if (c >= ((char)0xff21))
					return (char) to_upper_data_high [c - 0xff21];
			}
			return c;
		}

		public static char ToUpper (char c, CultureInfo culture)
		{
			if (culture == null)
				throw new ArgumentNullException ("culture");
			if (culture.LCID == 0x007F) // Invariant
				return ToUpperInvariant (c);

			return culture.TextInfo.ToUpper (c);
		}
		
		public override string ToString ()
		{
			// LAMESPEC: ECMA draft lists this as returning ToString (null), 
			// However it doesn't list another ToString() method.
			return new String (m_value, 1);
		}

		public static string ToString(char c)
		{
			return new String (c, 1);
		}

		public string ToString (IFormatProvider fp)
		{
			// LAMESPEC: ECMA draft doesn't say Char implements IFormattable
			return new String (m_value, 1);
		}

		// =========== IConvertible Methods =========== //
		
		public TypeCode GetTypeCode ()
		{
			return TypeCode.Char;
		}	  

		object IConvertible.ToType (Type conversionType, IFormatProvider provider)
		{
			return System.Convert.ToType(m_value, conversionType, provider);
		}
		
		bool IConvertible.ToBoolean (IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
		
		byte IConvertible.ToByte (IFormatProvider provider)
		{
			return System.Convert.ToByte(m_value);
		}
		
		char IConvertible.ToChar (IFormatProvider provider)
		{
			return m_value;
		}
		
		DateTime IConvertible.ToDateTime (IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
		
		decimal IConvertible.ToDecimal (IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
		
		double IConvertible.ToDouble (IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
		
		short IConvertible.ToInt16 (IFormatProvider provider)
		{
			return System.Convert.ToInt16(m_value);
		}
		
		int IConvertible.ToInt32 (IFormatProvider provider)
		{
			return System.Convert.ToInt32(m_value);
		}
		
		long IConvertible.ToInt64 (IFormatProvider provider)
		{
			return System.Convert.ToInt64(m_value);
		}
		
		sbyte IConvertible.ToSByte (IFormatProvider provider)
		{
			return System.Convert.ToSByte(m_value);
		}
		
		float IConvertible.ToSingle (IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
		
		ushort IConvertible.ToUInt16 (IFormatProvider provider)
		{
			return System.Convert.ToUInt16(m_value);
		}
		
		uint IConvertible.ToUInt32 (IFormatProvider provider)
		{
			return System.Convert.ToUInt32(m_value);
		}
		
		ulong IConvertible.ToUInt64 (IFormatProvider provider)
		{
			return System.Convert.ToUInt64(m_value);
		}
	}
}

