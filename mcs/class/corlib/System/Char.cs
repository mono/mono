//
// System.Char.cs
//
// Authors:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Miguel de Icaza (miguel@ximian.com)
//   Jackson Harper (jackson@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) 2004 Novell, Inc (http://www.novell.com)
//

// Note about the ToString()'s. ECMA says there's only a ToString() method, 
// BUT it is just a wrapper for ToString(null). However there is no other ToString
// in the docs. Turning to the NET framework sdk reveals that there is a 
// ToString(formatprovider) method, as well as a 'static ToString (char c)' method, 
// which appears to just be a Convert.ToString(char c) type method. ECMA also
// doesn't list this class as implementing IFormattable.

using System; 
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System
{
	[Serializable]
	public struct Char : IComparable, IConvertible
	{
		public const char MaxValue = (char) 0xffff;
		public const char MinValue = (char) 0;

		internal char m_value;

		static Char () {
			unsafe {
				GetDataTablePointers (out category_data, out numeric_data,
						out numeric_data_values,
						out to_lower_data_low, out to_lower_data_high,
						out to_upper_data_low, out to_upper_data_high);
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

			return ((Char) o).m_value == m_value;
		}

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
				throw new ArgumentNullException (Locale.GetText ("str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
					"The value of index is less than zero, or greater than or equal to the length of str"));
					
			
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
				throw new ArgumentNullException (Locale.GetText ("str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText ("The value of index is less "+
							  "than zero, or greater than or equal to the length of str"));
			
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
				throw new ArgumentNullException (Locale.GetText ("Str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
					"The value of index is less than zero, or greater than or equal to the length of str"));
			
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
				throw new ArgumentNullException (Locale.GetText ("Str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
				 "The value of index is less than zero, or greater than or equal to the length of str"));
			
			return IsDigit (str[index]);
		}

		public static bool IsLetter (char c)
		{
			unsafe {
				UnicodeCategory Category = (UnicodeCategory)category_data [c];
				switch (Category) {
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
				throw new ArgumentNullException (Locale.GetText ("str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
				 "The value of index is less than zero, or greater than or equal to the length of str"));
			
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
				throw new ArgumentNullException (Locale.GetText ("str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
				 "The value of index is less than zero, or greater than or equal to the length of str"));
			
			return IsLower (str[index]);
		}

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
				throw new ArgumentNullException (Locale.GetText ("str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
				"The value of index is less than zero, or greater than or equal to the length of str"));
			
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
				throw new ArgumentNullException (Locale.GetText ("str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
				 "The value of index is less than zero, or greater than or equal to the length of str"));
			
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
				throw new ArgumentNullException (Locale.GetText ("str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
				 "The value of index is less than zero, or greater than or equal to the length of str"));
			
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
				throw new ArgumentNullException (Locale.GetText ("str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
				 "The value of index is less than zero, or greater than or equal to the length of str"));
			
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
				throw new ArgumentNullException (Locale.GetText ("str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
					"The value of index is less than zero, or greater than or equal to the length of str"));
			
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
				throw new ArgumentNullException (Locale.GetText ("str is a null reference"));
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException (Locale.GetText (
				 "The value of index is less than zero, or greater than or equal to the length of str"));
			
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
					return true;
				default:
					return false;
				}
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

		public static char ToLower (char c)
		{
			unsafe {
				if (c <= ((char)0x24cf))
					return (char) to_lower_data_low [c];
				if (c >= ((char)0xff41))
					return (char) to_lower_data_high[c - 0xff41];
			}
			return c;
		}

		[MonoTODO]
		public static char ToLower (char c, CultureInfo culture)
		{
			//TODO ignored culture for now
			return ToLower (c);
		}

		public static char ToUpper (char c)
		{
			unsafe {
				if (c <= ((char)0x24cf))
					return (char) to_upper_data_low [c];
				if (c >= ((char)0xff21))
					return (char) to_upper_data_high [c - 0xff21];
			}
			return c;
		}

		[MonoTODO]
		public static char ToUpper(char c, CultureInfo culture)
		{
			//TODO ignored culture for now
			return ToUpper (c);
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
		
		[CLSCompliant(false)] 
		sbyte IConvertible.ToSByte (IFormatProvider provider)
		{
			return System.Convert.ToSByte(m_value);
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
			return System.Convert.ToUInt16(m_value);
		}
		
		[CLSCompliant(false)]
		uint IConvertible.ToUInt32 (IFormatProvider provider)
		{
			return System.Convert.ToUInt32(m_value);
		}
		
		[CLSCompliant(false)]
		ulong IConvertible.ToUInt64 (IFormatProvider provider)
		{
			return System.Convert.ToUInt64(m_value);
		}
	}
}

