//
// System.Char.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

// Note about the ToString()'s. ECMA says there's only a ToString() method, 
// BUT it is just a wrapper for ToString(null). However there is no other ToString
// in the docs. Turning to the NET framework sdk reveals that there is a 
// ToString(formatprovider) method, as well as a 'static ToString (char c)' method, 
// which appears to just be a Convert.ToString(char c) type method. ECMA also
// doesn't list this class as implementing IFormattable.

using System.Globalization;
using System.Runtime.CompilerServices;

namespace System {
	
	[Serializable]
	public struct Char : IComparable, IConvertible {
		public const char MaxValue = (char) 0xffff;
		public const char MinValue = (char) 0;
		
		internal char value;
		
		public int CompareTo (object v)
		{
			if (v == null)
				return 1;
			
			if (!(v is System.Char))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Char"));

			char xv = (char) v;
			if (value == xv)
				return 0;

			if (value > xv)
				return 1;
			else
				return -1;
		}

		public override bool Equals (object o)
		{
			if (!(o is System.Char))
				return false;

			return ((Char) o) == value;
		}

		public override int GetHashCode ()
		{
			return value;
		}

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		public static extern double GetNumericValue (char c);

		public static double GetNumericValue (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException (Locale.GetText ("str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
					"The value of index is less than zero, or greater than or equal to the length of str"));
					
			
			return GetNumericValue (str[index]);
		}

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		public static extern UnicodeCategory GetUnicodeCategory (char c); 

		public static UnicodeCategory GetUnicodeCategory (string str, int index) {
			if (str == null) 
				throw new ArgumentNullException (Locale.GetText ("str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText ("The value of index is less "+
					                  "than zero, or greater than or equal to the length of str"));
			
			return GetUnicodeCategory (str[index]);
		}

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		public static extern bool IsControl (char c);

		public static bool IsControl (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException (Locale.GetText ("Str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
					"The value of index is less than zero, or greater than or equal to the length of str"));
			
			return IsControl (str[index]);
		}
		
		public static bool IsDigit (char c)
		{
			// You will find that this int-comparison version 
			// is faster than char-comparison version.
			int i = (int) c;
			if (i >= 0x30 && i <= 0x39) // ASCII digits
				return true;
			if (i < 0x660) // quick check for ASCII range.
				return false;

			// hereby all ASCII characters are evaluated quickly.

			// the largest ranges of digits
			if (i >= 0xff10 && i <= 0xff19) // fullwidth digits
				return true;
			// after the block above, there is a wide range of non-digits.
			if (i > 0x1820)
				return false;

			if (i >= 0x660 && i <= 0x669 || // arabic-indic
				i >= 0x6f0 && i <= 0x6f9)  // extended arabic-indic
				return true;
			if (i < 0x966)
				return false;
			if (i == 0xbe6)
				return false; // (reserved - Tamil number 0 does not exist in Unicode spec)
			// Devanagari, Bengali, Gurmukhi, Gujarati, Oriya, 
			// Tamil, Telugu, Kannada and Malayalam digits.
			if (i >= 0x960 && i <= 0xd6f &&
				(i & 0xf) > 5 &&
				((i & 0xf0) == 0x60 || (i & 0xf0) == 0xe0))
				return true;
			if (i < 0xe50)
				return false;
			return // rest are boring check ;-)
				i >= 0xe50 && i <= 0xe59 || // Thai
				i >= 0xed0 && i <= 0xed9 || // Lao
				i >= 0xf20 && i <= 0xf29 || // Tibetan
				i >= 0x1040 && i <= 0x1049 || // Myanmer
				i >= 0x1369 && i <= 0x1371 || // Ethiopic
				i >= 0x17e0 && i <= 0x17e9 || // Buhid
				i >= 0x1810 && i <= 0x1819; // Mongolian
		}

		public static bool IsDigit (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException (Locale.GetText ("Str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
				 "The value of index is less than zero, or greater than or equal to the length of str"));
			
			return IsDigit (str[index]);
		}

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		public static extern bool IsLetter (char c);

		public static bool IsLetter (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException (Locale.GetText ("str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
			         "The value of index is less than zero, or greater than or equal to the length of str"));
			
			return IsLetter (str[index]);
		}

		public static bool IsLetterOrDigit (char c)
		{
			if (IsLetter (c) == false && IsDigit (c) == false)
				return false;
			else
				return true;
		}

		public static bool IsLetterOrDigit (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException (Locale.GetText ("str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
				 "The value of index is less than zero, or greater than or equal to the length of str"));
			
			return IsLetterOrDigit (str[index]);
		}

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		public static extern bool IsLower (char c);
		
		public static bool IsLower (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException (Locale.GetText ("str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
				 "The value of index is less than zero, or greater than or equal to the length of str"));
			
			return IsLower (str[index]);
		}

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		public static extern bool IsNumber (char c);
		
		public static bool IsNumber (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException (Locale.GetText ("str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
				"The value of index is less than zero, or greater than or equal to the length of str"));
			
			return IsNumber (str[index]);
		}

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		public static extern bool IsPunctuation (char c);
		
		public static bool IsPunctuation (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException (Locale.GetText ("str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
				 "The value of index is less than zero, or greater than or equal to the length of str"));
			
			return IsPunctuation (str[index]);
		}

		public static bool IsSeparator (char c)
		{
			int i = (int) c;
			switch (i) {
			case 0x20:
			case 0xa0: // &nbsp;
			case 0x1680: // Ogham space mark
			case 0x202f: // Narrow nbsp
			case 0x3000: // Ideographic space
			case 0x2028:
			case 0x2029:
				return true;
			default:
				// general punctuations :: spaces
				return i >= 0x2000 && i <= 0x200b;
			}
		}
		
		public static bool IsSeparator (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException (Locale.GetText ("str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
				 "The value of index is less than zero, or greater than or equal to the length of str"));
			
			return IsSeparator (str[index]);
		}

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		public static extern bool IsSurrogate (char c);
		
		public static bool IsSurrogate (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException (Locale.GetText ("str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
			         "The value of index is less than zero, or greater than or equal to the length of str"));
			
			return IsSurrogate (str[index]);
		}

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		public static extern bool IsSymbol (char c);
		
		public static bool IsSymbol (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException (Locale.GetText ("str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
					"The value of index is less than zero, or greater than or equal to the length of str"));
			
			return IsSymbol (str[index]);
		}

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		public static extern bool IsUpper (char c);
		
		public static bool IsUpper (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException (Locale.GetText ("str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
				 "The value of index is less than zero, or greater than or equal to the length of str"));
			
			return IsUpper (str[index]);
		}

		public static bool IsWhiteSpace (char c)
		{
			int i = (int) c;
			switch (i) {
			case 0x20:
			case 0x9:
			case 0x0a:
			case 0x0b:
			case 0x0c:
			case 0x0d:
			case 0x85: // NEL
			case 0x2028: // Line Separator
			case 0x2029: // Paragraph Separator
			// Below are copy of IsSeparator test.
			case 0xa0: // &nbsp;
			case 0x1680: // Ogham space mark
			case 0x202f: // Narrow nbsp
			case 0x3000: // Ideographic space
				return true;
			default:
				// general punctuations :: spaces
				return i >= 0x2000 && i <= 0x200b;
			}
		}
		
		public static bool IsWhiteSpace (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException (Locale.GetText ("str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
					"The value of index is less than zero, or greater than or equal to the length of str"));
			
			return IsWhiteSpace (str[index]);
		}

		public static char Parse (string str)
		{
			if (str == null)
				throw new ArgumentNullException (Locale.GetText ("str is a null reference"));

			if (str.Length != 1)
				throw new FormatException ("string contains more than one character.");
			
			return str [0];
		}

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		public static extern char ToLower (char c);

		[MonoTODO]
		public static char ToLower (char c, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
		

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		public static extern char ToUpper (char c);

		[MonoTODO]
		public static char ToUpper(char c, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public override string ToString ()
		{
			// LAMESPEC: ECMA draft lists this as returning ToString (null), 
			// However it doesn't list another ToString() method.
			return new String (value, 1);
		}

		public static string ToString(char c)
		{
			return new String (new char [] {c});
		}

		public string ToString (IFormatProvider fp)
		{
			// LAMESPEC: ECMA draft doesn't say Char implements IFormattable
			return new String (value, 1);
		}

		// =========== IConvertible Methods =========== //
		
		public TypeCode GetTypeCode ()
		{
			return TypeCode.Char;
		}	  

		object IConvertible.ToType (Type conversionType, IFormatProvider provider)
		{
			return System.Convert.ToType(value, conversionType, provider);
		}
		
		bool IConvertible.ToBoolean (IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
		
		byte IConvertible.ToByte (IFormatProvider provider)
		{
			return System.Convert.ToByte(value);
		}
		
		char IConvertible.ToChar (IFormatProvider provider)
		{
			return value;
		}
		
		[CLSCompliant(false)]
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
			return System.Convert.ToInt16(value);
		}
		
		int IConvertible.ToInt32 (IFormatProvider provider)
		{
			return System.Convert.ToInt32(value);
		}
		
		long IConvertible.ToInt64 (IFormatProvider provider)
		{
			return System.Convert.ToInt64(value);
		}
		
		[CLSCompliant(false)] 
		sbyte IConvertible.ToSByte (IFormatProvider provider)
		{
			return System.Convert.ToSByte(value);
		}
		
		float IConvertible.ToSingle (IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
		
		string IConvertible.ToString (IFormatProvider provider)
		{
			return ToString(provider);
		}

		[CLSCompliant(false)]
		ushort IConvertible.ToUInt16 (IFormatProvider provider)
		{
			return System.Convert.ToUInt16(value);
		}
		
		[CLSCompliant(false)]
		uint IConvertible.ToUInt32 (IFormatProvider provider)
		{
			return System.Convert.ToUInt32(value);
		}
		
		[CLSCompliant(false)]
		ulong IConvertible.ToUInt64 (IFormatProvider provider)
		{
			return System.Convert.ToUInt64(value);
		}
	}
}
