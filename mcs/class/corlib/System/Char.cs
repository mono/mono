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
	public struct Char : IComparable, IConvertible { //, IFormattable {
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
		
		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		public static extern bool IsDigit (char c);

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

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		public static extern bool IsSeparator (char c);
		
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

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		public static extern bool IsWhiteSpace (char c);
		
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

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		public static extern char ToUpper (char c);

		public override string ToString ()
		{
			// LAMESPEC: ECMA draft lists this as returning ToString (null), 
			// However it doesn't list another ToString() method.
			return new String (value, 1);
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
