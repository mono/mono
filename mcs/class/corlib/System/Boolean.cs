//
// System.Boolean.cs
//
// Author:
//   Derek Holden (dholden@draper.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// I guess this is the Boolean class. This was written word for word
// off the Library specification for System.Boolean in the ECMA
// TC39 TG2 and TG3 working documents.
//
// The XML style documentation isn't that elegant, but it seems to 
// be the standard way according to the poorly documented C# 
// Programmer's Reference section on XML Documentation.
//
// This header and the one above it can be formatted however, just trying
// to keep it consistent w/ the existing mcs headers.
//
// Even though it's not in the ECMA docs, the .NET Framework Class Library
// says this implements IConvertible, but if it does it has some other
// member functions to implement. 
//

using System.Globalization;
namespace System {

	/// <summary>
	/// Represents the boolean values of logical true and false.
	/// </summary>
	[Serializable]
	public struct Boolean : IComparable, IConvertible {
		
		/// <value>
		/// The String representation of Boolean False
		/// </value>	
		public static readonly string FalseString;

		/// <value>
		/// The String representation of Boolean True
		/// </value>	
		public static readonly string TrueString;
      
		/// <value>
		/// Internal bool value for for this instance
		/// </value>

		internal bool value;
	
		static Boolean () 
		{
			FalseString = "False";
			TrueString = "True";
		}

		/// <summary>
		/// Compares the current Boolean instance against another object.
		/// </summary>
		/// <remarks>
		/// Throws an ArgumentException if <c>obj</c> isn't null or 
		/// a Boolean.
		/// </remarks>
		/// <param name="obj">
		/// The object to compare against
		/// </param>
		/// <returns>
		/// An int reflecting the sort order of this instance as 
		/// compared to <c>obj</c>
		/// -1 if this instance is false and <c>obj</c> is true
		///  0 if this instance is equal to <c>obj</c>
		///  1 if this instance is true and <c>obj</c> is false, 
		///    or <c>obj</c> is null
		/// </returns>
		public int CompareTo (object obj) 
		{
			if (obj == null)
				return 1;
			
			if (!(obj is System.Boolean))
				throw new ArgumentException
				(Locale.GetText ("Object is not a Boolean and is not a null reference"));
			
			// for case #3
			if (obj == null || (value == true && (bool)obj == false))
				return 1;
	    
			// for case #2, else it's #1
			return (value == (bool)obj) ? 0 : -1;
		}
	
		/// <summary>
		/// Determines whether this instance and another object represent the
		/// same type and value.
		/// </summary>
		/// <param name="obj">
		/// The object to check against
		/// </param>
		/// <returns>
		/// true if this instnace and <c>obj</c> are same value, 
		/// otherwise false if it is not or null
		/// </returns>
		public override bool Equals (Object obj) 
		{
			if (obj == null || !(obj is System.Boolean))
				return false;

			return ((bool)obj) == value;
		}
	
		/// <summary>
		/// Generates a hashcode for this object.
		/// </summary>
		/// <returns>
		/// An Int32 value holding the hash code
		/// </returns>
		public override int GetHashCode () 
		{
			// Guess there's not too many ways to hash a Boolean
			return value ? 1 : 0;
		}

		/// <summary>
		/// Returns a given string as a boolean value. The string must be 
		/// equivalent to either TrueString or FalseString, with leading and/or
		/// trailing spaces, and is parsed case-insensitively.
		/// </summary>
		/// <remarks>
		/// Throws an ArgumentNullException if <c>val</c> is null, or a 
		/// FormatException if <c>val</c> doesn't match <c>TrueString</c> 
		/// or <c>FalseString</c>
		/// </remarks>
		/// <param name="val">
		/// The string value to parse
		/// </param>
		/// <returns>
		/// true if <c>val</c> is equivalent to TrueString, 
		/// otherwise false
		/// </returns>
		public static bool Parse (string val) 
		{
			if (val == null)
				throw new ArgumentNullException (
					Locale.GetText ("Value is a null reference"));
	    
			val = val.Trim ();
	    
			if (String.Compare (val, TrueString, true,
					    CultureInfo.InvariantCulture) == 0)
				return true;
	    
			if (String.Compare (val, FalseString, true,
					    CultureInfo.InvariantCulture) == 0)
				return false;
	    
			throw new FormatException (Locale.GetText (
				"Value is not equivalent to either TrueString or FalseString"));
		}

		/// <summary>
		/// Returns a string representation of this Boolean object.
		/// </summary>
		/// <returns>
		/// <c>FalseString</c> if the instance value is false, otherwise 
		/// <c>TrueString</c>
		/// </returns>
		public override string ToString () 
		{
			return value ? TrueString : FalseString;
		}
		
		// =========== IConvertible Methods =========== //

		public TypeCode GetTypeCode () 
		{ 
			return TypeCode.Boolean;
		}

		object IConvertible.ToType (Type conversionType, IFormatProvider provider)
		{
			return System.Convert.ToType(value, conversionType, provider);
		}
		
		bool IConvertible.ToBoolean (IFormatProvider provider)
		{
			return value;
		}
		
		byte IConvertible.ToByte (IFormatProvider provider)
		{
			return System.Convert.ToByte(value);
		}
		
		char IConvertible.ToChar (IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
		
		[CLSCompliant(false)]
		DateTime IConvertible.ToDateTime (IFormatProvider provider)
		{
			throw new InvalidCastException();
		}
		
		decimal IConvertible.ToDecimal (IFormatProvider provider)
		{
			return System.Convert.ToDecimal(value);
		}
		
		double IConvertible.ToDouble (IFormatProvider provider)
		{
			return System.Convert.ToDouble(value);
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
			return System.Convert.ToSingle(value);
		}
		
		public string ToString (IFormatProvider provider)
		{
			return ToString();
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
		
	} // System.Boolean

} // Namespace System
