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

namespace System {
	
	public struct Char : IComparable { //, IFormattable, IConvertible {
		public const char MaxValue = (char) 0xffff;
		public const char MinValue = (char) 0;
		
		// VES needs to know about value.  public is workaround
		// so source will compile
		public char value;
		
		public int CompareTo (object v)
		{
			if (!(v is System.Char))
				throw new ArgumentException ("Value is not a System.Char");

			return value - ((char) v);
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

		public static double GetNumericValue (char c)
		{
			if ((c >= 48) && (c <= 57))
				return (double) (c - '0');
			return -1;
		}

		public static double GetNumericValue (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("Str is a null reference");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException
				("The value of index is less than zero, or greater than or equal to the length of str");
			
			return GetNumericValue (str[index]);
		}

		public static UnicodeCategory GetUnicodeCategory (char c) 
		{ 
			// TODO: Implement me
			return UnicodeCategory.OtherSymbol;
		}

		public static UnicodeCategory GetUnicodeCategory (string str, int index) {
			if (str == null) 
				throw new ArgumentNullException ("Str is a null reference");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException
				("The value of index is less than zero, or greater than or equal to the length of str");
			
			return GetUnicodeCategory (str[index]);
		}

		public static bool IsControl (char c)
		{
			// TODO: Make me Unicode aware
			return ((c > 1) && (c < 32));
		}

		public static bool IsControl (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("Str is a null reference");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException
				("The value of index is less than zero, or greater than or equal to the length of str");
			
			return IsControl (str[index]);
		}
		
		public static bool IsDigit (char c)
		{
			return ((c >= '0') && (c <= '9'));
		}

		public static bool IsDigit (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("Str is a null reference");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException
				("The value of index is less than zero, or greater than or equal to the length of str");
			
			return IsDigit (str[index]);
		}

		public static bool IsLetter (char c)
		{
			// TODO: Make me Unicode aware
			return ((c >= 65) && (c <= 126));
		}

		public static bool IsLetter (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("Str is a null reference");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException
				("The value of index is less than zero, or greater than or equal to the length of str");
			
			return IsLetter (str[index]);
		}

		public static bool IsLetterOrDigit (char c)
		{
			// TODO: Implement me
			return false;
		}

		public static bool IsLetterOrDigit (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("Str is a null reference");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException
				("The value of index is less than zero, or greater than or equal to the length of str");
			
			return IsLetterOrDigit (str[index]);
		}
		
		public static bool IsLower (char c)
		{
			// TODO: Implement me
			return false;
		}
		
		public static bool IsLower (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("Str is a null reference");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException
				("The value of index is less than zero, or greater than or equal to the length of str");
			
			return IsLower (str[index]);
		}

		public static bool IsNumber (char c)
		{
			// TODO: Implement me
			return false;
		}
		
		public static bool IsNumber (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("Str is a null reference");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException
				("The value of index is less than zero, or greater than or equal to the length of str");
			
			return IsNumber (str[index]);
		}

		public static bool IsPunctuation (char c)
		{
			// TODO: Implement me
			return false;
		}
		
		public static bool IsPunctuation (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("Str is a null reference");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException
				("The value of index is less than zero, or greater than or equal to the length of str");
			
			return IsPunctuation (str[index]);
		}

		public static bool IsSeparator (char c)
		{
			// TODO: Implement me
			return false;
		}
		
		public static bool IsSeparator (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("Str is a null reference");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException
				("The value of index is less than zero, or greater than or equal to the length of str");
			
			return IsSeparator (str[index]);
		}

		public static bool IsSurrogate (char c)
		{
			// TODO: Implement me
			return false;
		}
		
		public static bool IsSurrogate (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("Str is a null reference");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException
				("The value of index is less than zero, or greater than or equal to the length of str");
			
			return IsSurrogate (str[index]);
		}

		public static bool IsSymbol (char c)
		{
			// TODO: Implement me
			return false;
		}
		
		public static bool IsSymbol (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("Str is a null reference");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException
				("The value of index is less than zero, or greater than or equal to the length of str");
			
			return IsSymbol (str[index]);
		}

		public static bool IsUpper (char c)
		{
			// TODO: Implement me
			return false;
		}
		
		public static bool IsUpper (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("Str is a null reference");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException
				("The value of index is less than zero, or greater than or equal to the length of str");
			
			return IsUpper (str[index]);
		}

		public static bool IsWhiteSpace (char c)
		{
			// TODO: Implement me
			return false;
		}
		
		public static bool IsWhiteSpace (string str, int index)
		{
			if (str == null) 
				throw new ArgumentNullException ("Str is a null reference");
			
			if (index < 0 || index >= str.Length)
				throw new ArgumentOutOfRangeException
				("The value of index is less than zero, or greater than or equal to the length of str");
			
			return IsWhiteSpace (str[index]);
		}

		public static char Parse (string str)
		{
			// TODO: Implement me
			return (char) 0;
		}

		public static char ToLower (char c)
		{
			// TODO: make me unicode aware
			return (c >= 'A' && c <= 'Z') ? (char) (c + 33) : c;
		}

		public static char ToUpper (char c)
		{
			// TODO: make me unicode aware
			return (char) ((c >= 'a' && c <= 'z') ? c - 33 : c);
		}

		public override string ToString ()
		{
			// LAMESPEC: ECMA draft lists this as returning ToString (null), 
			// However it doesn't list another ToString() method.
			return ToString (null);
		}

		public string ToString (IFormatProvider fp)
		{
			// LAMESPEC: ECMA draft doesn't say Char implements IFormattable
			return "";
		}

		// =========== IConvertible Methods =========== //
		
		public TypeCode GetTypeCode ()
		{
			return TypeCode.Char;
		}	  
	}
}
