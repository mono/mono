//
// System.Convert.cs
//
// Author:
//   Derek Holden (dholden@draper.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// System.Convert class. This was written word for word off the 
// Library specification for System.Convert in the ECMA TC39 TG2 
// and TG3 working documents. The first page of which has a table
// for all legal conversion scenerios.
//
// This header and the one above it can be formatted however, just trying
// to keep it consistent w/ the existing mcs headers.
//
// This Convert class could be written another way, with each type 
// implementing IConvertible and defining their own conversion functions,
// and this class just calling the type's implementation. Or, they can 
// be defined here and the implementing type can use these functions when 
// defining their IConvertible interface. Byte's ToBoolean() calls 
// Convert.ToBoolean(byte), or Convert.ToBoolean(byte) calls 
// byte.ToBoolean(). The first case is what is done here.
//
// See http://lists.ximian.com/archives/public/mono-list/2001-July/000525.html
//
// There are also conversion functions that are not defined in
// the ECMA draft, such as there is no bool ToBoolean(DateTime value), 
// and placing that somewhere won't compile w/ this Convert since the
// function doesn't exist. However calling that when using Microsoft's
// System.Convert doesn't produce any compiler errors, it just throws
// an InvalidCastException at runtime.
//
// Whenever a decimal, double, or single is converted to an integer
// based type, it is even rounded. This uses Math.Round which only 
// has Round(decimal) and Round(double), so in the Convert from 
// single cases the value is passed to Math as a double. This 
// may not be completely necessary.
//
// The .NET Framework SDK lists DBNull as a member of this class
// as 'public static readonly object DBNull;'. 
//
// It should also be decided if all the cast return values should be
// returned as unchecked or not.
//
// All the XML function comments were auto generated which is why they
// sound someone redundant.
//
// TYPE | BOOL BYTE CHAR DT DEC DBL I16 I32 I64 SBYT SNGL STR UI16 UI32 UI64
// -----+--------------------------------------------------------------------
// BOOL |   X    X           X   X   X   X   X    X    X   X    X    X    X
// BYTE |   X    X    X      X   X   X   X   X    X    X   X    X    X    X
// CHAR |        X    X              X   X   X    X        X    X    X    X
// DT   |                 X                                X
// DEC  |   X    X           X   X   X   X   X    X    X   X    X    X    X
// DBL  |   X    X           X   X   X   X   X    X    X   X    X    X    X
// I16  |   X    X    X      X   X   X   X   X    X    X   X    X    X    X
// I32  |   X    X    X      X   X   X   X   X    X    X   X    X    X    X
// I64  |   X    X    X      X   X   X   X   X    X    X   X    X    X    X
// SBYT |   X    X    X      X   X   X   X   X    X    X   X    X    X    X
// SNGL |   X    X           X   X   X   X   X    X    X   X    X    X    X
// STR  |   X    X    X   X  X   X   X   X   X    X    X   X    X    X    X
// UI16 |   X    X    X      X   X   X   X   X    X    X   X    X    X    X
// UI32 |   X    X    X      X   X   X   X   X    X    X   X    X    X    X
// UI64 |   X    X    X      X   X   X   X   X    X    X   X    X    X    X
//

using System.Security.Cryptography;
using System.Globalization;

namespace System {
  
	[CLSCompliant(false)]
	public sealed class Convert {
	
		// ========== BASE 64 Conversions ========== //
		// the BASE64 convert methods are using the Base64 converting methods
		// from System.Security.Cryptography.ToBase64Transform and
		// System.Security.Cryptography.FromBase64Transform
		//
		// should be changed to a stand-alone class Base64Encoder & Base64Decoder
		
		public static byte[] FromBase64CharArray(char[] inArray, int offset, int length)
		{
			if (inArray == null)
				throw new ArgumentNullException();
			
			int len = inArray.Length;
			if (len < 4 || len % 4 != 0)
				throw new FormatException();
				
			byte[] inArr = new System.Text.UnicodeEncoding().GetBytes(inArray);
			FromBase64Transform t = new FromBase64Transform();
			
			return t.TransformFinalBlock(inArr, 0, inArr.Length);
		}
		
		public static byte[] FromBase64String(string s)
		{
			if (s == null)
				throw new ArgumentNullException();
			
			char[] inArr = s.ToCharArray();

			return FromBase64CharArray(inArr, 0, inArr.Length);
		}
		
		public static int ToBase64CharArray(byte[] inArray, int offsetIn, int length, 
		                                    char[] outArray, int offsetOut)
		{
			if (inArray == null || outArray == null)
				throw new ArgumentNullException();
			
			if (offsetIn < 0 || length < 0 || offsetOut < 0 || (offsetIn + length) > inArray.Length)
				throw new ArgumentOutOfRangeException();
			
			ToBase64Transform t = new ToBase64Transform();
			byte[] outArr = t.TransformFinalBlock(inArray, offsetIn, length);
			
			char[] cOutArr = new System.Text.ASCIIEncoding().GetChars(outArr);
			
			if ((offsetOut + cOutArr.Length) > outArray.Length)
				throw new ArgumentOutOfRangeException();
			
			Array.Copy(cOutArr, 0, outArray, offsetOut, cOutArr.Length);
			
			return cOutArr.Length;
		}
		
		public static string ToBase64String(byte[] inArray)
		{
			if (inArray == null)
				throw new ArgumentNullException();

			return ToBase64String(inArray, 0, inArray.Length);
		}
		
		public static string ToBase64String(byte[] inArray, int offset, int length)
		{
			if (inArray == null)
				throw new ArgumentNullException();
			
			if (offset < 0 || length < 0 || (offset + length) > inArray.Length)
				throw new ArgumentOutOfRangeException();
			
			// FIXME: change to stand alone Base64 Encoder class
			ToBase64Transform t = new ToBase64Transform();
			byte[] outArr = t.TransformFinalBlock(inArray, offset, length);
			
			return (new System.Text.ASCIIEncoding().GetString(outArr));
		}
		
		// ========== Boolean Conversions ========== //
	
		public static bool ToBoolean (bool value) 
		{ 
			return value; 
		}

		public static bool ToBoolean (byte value) 
		{ 
			return (value != 0); 
		}
 
		public static bool ToBoolean (char value)
		{
			throw new InvalidCastException (Locale.GetText ("Can't convert char to bool"));
		}
		
		public static bool ToBoolean (DateTime value)
		{
			throw new InvalidCastException (Locale.GetText ("Can't convert date to bool"));
		}
		
		public static bool ToBoolean (decimal value) 
		{ 
			return (value != 0M); 
		}

		public static bool ToBoolean (double value) 
		{ 
			return (value != 0); 
		}

		public static bool ToBoolean (float value) 
		{ 
			return (value != 0f); 
		}

		public static bool ToBoolean (int value) 
		{ 
			return (value != 0); 
		}

		public static bool ToBoolean (long value) 
		{ 
			return (value != 0); 
		}

		public static bool ToBoolean (sbyte value) 
		{ 
			return (value != 0); 
		}
	
		public static bool ToBoolean (short value) 
		{ 
			return (value != 0); 
		}

		public static bool ToBoolean (string value) 
		{
			return Boolean.Parse (value);
		}
	
		public static bool ToBoolean (uint value) 
		{ 
			return (value != 0);
		}

		public static bool ToBoolean (ulong value) 
		{ 
			return (value != 0); 
		}

		public static bool ToBoolean (ushort value) 
		{ 
			return (value != 0); 
		}

		// ========== Byte Conversions ========== //
	
		public static byte ToByte (bool value) 
		{ 
			return (byte)(value ? 1 : 0); 
		}
	
		public static byte ToByte (byte value) 
		{ 
			return value; 
		}

		public static byte ToByte (char value) 
		{ 
			if (value > Byte.MaxValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than Byte.MaxValue"));

			return (byte)value;
		}
	
		public static byte ToByte (decimal value) 
		{ 
			if (value > Byte.MaxValue || value < Byte.MinValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than Byte.MaxValue or less than Byte.MinValue"));
	  
			// Returned Even-Rounded
			return (byte)(Math.Round (value));
		}
	
		public static byte ToByte (double value) 
		{ 
			if (value > Byte.MaxValue || value < Byte.MinValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than Byte.MaxValue or less than Byte.MinValue"));
	  
			// This and the float version of ToByte are the only ones
			// the spec listed as checking for .NaN and Infinity overflow
			if (Double.IsNaN(value) || Double.IsInfinity(value))
				throw new OverflowException (Locale.GetText (
					"Value is equal to Double.NaN, Double.PositiveInfinity, or Double.NegativeInfinity"));

			// Returned Even-Rounded
			return (byte)(Math.Round (value));
		}

		public static byte ToByte (float value) 
		{ 
			if (value > Byte.MaxValue || value < Byte.MinValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than Byte.MaxValue or less than Byte.Minalue"));

			// This and the double version of ToByte are the only ones
			// the spec listed as checking for .NaN and Infinity overflow
			if (Single.IsNaN(value) || Single.IsInfinity(value))
				throw new OverflowException (Locale.GetText (
					"Value is equal to Single.NaN, Single.PositiveInfinity, or Single.NegativeInfinity"));
	  
			// Returned Even-Rounded, pass it as a double, could have this
			// method just call Convert.ToByte ( (double)value)
			return (byte)(Math.Round ( (double)value));
		}

		public static byte ToByte (int value) 
		{ 
			if (value > Byte.MaxValue || value < Byte.MinValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than Byte.MaxValue or less than Byte.MinValue"));
	  
			return (byte)value; 
		}

		public static byte ToByte (long value) 
		{ 
			if (value > Byte.MaxValue || value < Byte.MinValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than Byte.MaxValue or less than Byte.MinValue"));
	  
			return (byte)value;
		}

		public static byte ToByte (sbyte value) 
		{ 
			if (value < Byte.MinValue)
				throw new OverflowException (Locale.GetText (
					"Value is less than Byte.MinValue"));
	  
			return (byte)value;
		}
	
		public static byte ToByte (short value) 
		{ 
			if (value > Byte.MaxValue || value < Byte.MinValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than Byte.MaxValue or less than Byte.MinValue"));
	  
			return (byte)value; 
		}

		public static byte ToByte (string value) 
		{
			return Byte.Parse (value);
		}

		public static byte ToByte (string value, IFormatProvider provider) 
		{
			return Byte.Parse (value, provider);
		}
	
		public static byte ToByte (uint value) 
		{ 
			if (value > Byte.MaxValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than Byte.MaxValue"));

			return (byte)value;
		}

		public static byte ToByte (ulong value) 
		{ 
			if (value > Byte.MaxValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than Byte.MaxValue"));

			return (byte)value;
		}

		public static byte ToByte (ushort value) 
		{ 
			if (value > Byte.MaxValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than Byte.MaxValue"));

			return (byte)value;
		}

		// ========== Char Conversions ========== //
	
		public static char ToChar (byte value) 
		{ 
			return (char)value;
		}

		public static char ToChar (char value) 
		{ 
			return value;
		}

		public static char ToChar (int value) 
		{ 
			if (value > Char.MaxValue || value < Char.MinValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than Char.MaxValue or less than Char.MinValue"));
	  
			return (char)value; 
		}

		public static char ToChar (long value) 
		{ 
			if (value > Char.MaxValue || value < Char.MinValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than Char.MaxValue or less than Char.MinValue"));
	  
			return (char)value; 
		}

		public static char ToChar (sbyte value) 
		{ 
			if (value < Char.MinValue)
				throw new OverflowException (Locale.GetText (
					"Value is less than Char.MinValue"));
	  
			return (char)value; 
		}
	
		public static char ToChar (short value) 
		{ 
			if (value < Char.MinValue)
				throw new OverflowException (Locale.GetText (
					"Value is less than Char.MinValue"));
	  
			return (char)value; 
		}

		public static char ToChar (string value) 
		{
			return Char.Parse (value);
		}
	
		public static char ToChar (uint value) 
		{ 
			if (value > Char.MaxValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than Char.MaxValue"));
	  
			return (char)value; 
		}

		public static char ToChar (ulong value) 
		{ 
			if (value > Char.MaxValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than Char.MaxValue"));
	  
			return (char)value; 
		}

		public static char ToChar (ushort value) 
		{ 
			if (value > Char.MaxValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than Char.MaxValue"));
	  
			return (char)value; 
		}

		// ========== DateTime Conversions ========== //
	
		public static DateTime ToDateTime (string value) 
		{ 
			return DateTime.Parse (value);
		}
	
		public static DateTime ToDateTime (string value, IFormatProvider provider) 
		{
			return DateTime.Parse (value, provider);
		}

		// ========== Decimal Conversions ========== //
	
		public static decimal ToDecimal (bool value) 
		{ 
			return value ? 1 : 0; 
		}
	
		public static decimal ToDecimal (byte value) 
		{ 
			return (decimal)value; 
		}
	
		public static decimal ToDecimal (decimal value) 
		{ 
			return value; 
		}

		public static decimal ToDecimal (double value) 
		{ 
			if (value > (double)Decimal.MaxValue || value < (double)Decimal.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than Decimal.MaxValue or less than Decimal.MinValue"));

			return (decimal)value; 
		}

		public static decimal ToDecimal (float value) 
		{ 
			if (value > (double)Decimal.MaxValue || value < (double)Decimal.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than Decimal.MaxValue or less than Decimal.MinValue"));

			return (decimal)value; 
		}

		public static decimal ToDecimal (int value) 
		{ 
			return (decimal)value; 
		}
	
		public static decimal ToDecimal (long value) 
		{ 
			return (decimal)value; 
		}

		public static decimal ToDecimal (sbyte value) 
		{ 
			return (decimal)value; 
		}
	
		public static decimal ToDecimal (short value) 
		{ 
			return (decimal)value; 
		}

		public static decimal ToDecimal (string value) 
		{
			return Decimal.Parse (value);
		}

		public static decimal ToDecimal (string value, IFormatProvider provider) 
		{
			return Decimal.Parse (value, provider);
		}
	
		public static decimal ToDecimal (uint value) 
		{ 
			return (decimal)value; 
		}

		public static decimal ToDecimal (ulong value) 
		{ 
			return (decimal)value; 
		}

		public static decimal ToDecimal (ushort value) 
		{ 
			return (decimal)value; 
		}

		// ========== Double Conversions ========== //
	
		public static double ToDouble (bool value) 
		{ 
			return value ? 1 : 0; 
		}
	
		public static double ToDouble (byte value) 
		{ 
			return (double)value; 
		}
	
		public static double ToDouble (decimal value) 
		{ 
			return (double)value; 
		}

		public static double ToDouble (double value) 
		{ 
			return value; 
		}

		public static double ToDouble (float value) 
		{ 
			return (double)value; 
		}

		public static double ToDouble (int value) 
		{ 
			return (double)value; 
		}
	
		public static double ToDouble (long value) 
		{ 
			return (double)value; 
		}

		public static double ToDouble (sbyte value) 
		{ 
			return (double)value; 
		}
	
		public static double ToDouble (short value) 
		{ 
			return (double)value; 
		}

		public static double ToDouble (string value) 
		{
			return Double.Parse (value);
		}

		public static double ToDouble (string value, IFormatProvider provider) 
		{
			return Double.Parse (value, provider);
		}
	
		public static double ToDouble (uint value) 
		{ 
			return (double)value; 
		}

		public static double ToDouble (ulong value) 
		{ 
			return (double)value; 
		}

		public static double ToDouble (ushort value) 
		{ 
			return (double)value; 
		}

		// ========== Int16 Conversions ========== //

		public static short ToInt16 (bool value) 
		{ 
			return (short)(value ? 1 : 0); 
		}
	
		public static short ToInt16 (byte value) 
		{ 
			return (short)value; 
		}

		public static short ToInt16 (char value) 
		{ 
			if (value > Int16.MaxValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than Int16.MaxValue"));

			return (short)value; 
		}
	
		public static short ToInt16 (decimal value) 
		{ 
			if (value > Int16.MaxValue || value < Int16.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than Int16.MaxValue or less than Int16.MinValue"));
	  
			// Returned Even-Rounded
			return (short)(Math.Round (value));	  
		}

		public static short ToInt16 (double value) 
		{ 
			if (value > Int16.MaxValue || value < Int16.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than Int16.MaxValue or less than Int16.MinValue"));
	  
			// Returned Even-Rounded
			return (short)(Math.Round (value));	  
		}
 
		public static short ToInt16 (float value) 
		{ 
			if (value > Int16.MaxValue || value < Int16.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than Int16.MaxValue or less than Int16.MinValue"));
	  
			// Returned Even-Rounded, use Math.Round pass as a double.
			return (short)Math.Round ( (double)value);
		}

		public static short ToInt16 (int value) 
		{ 
			if (value > Int16.MaxValue || value < Int16.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than Int16.MaxValue or less than Int16.MinValue"));

			return (short)value; 
		}
	
		public static short ToInt16 (long value) 
		{ 
			if (value > Int16.MaxValue || value < Int16.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than Int16.MaxValue or less than Int16.MinValue"));

			return (short)value; 
		}

		public static short ToInt16 (sbyte value) 
		{ 
			return (short)value; 
		}
	
		public static short ToInt16 (short value) 
		{ 
			return value; 
		}

		public static short ToInt16 (string value) 
		{
			return Int16.Parse (value);
		}

		public static short ToInt16 (string value, IFormatProvider provider) 
		{
			return Int16.Parse (value, provider);
		}
	
		public static short ToInt16 (uint value) 
		{ 
			if (value > Int16.MaxValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than Int16.MaxValue"));

			return (short)value; 
		}

		public static short ToInt16 (ulong value) 
		{ 
			if (value > (ulong)Int16.MaxValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than Int16.MaxValue"));

			return (short)value; 
		}

		public static short ToInt16 (ushort value) 
		{ 
			if (value > Int16.MaxValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than Int16.MaxValue"));

			return (short)value; 
		}

		// ========== Int32 Conversions ========== //

		public static int ToInt32 (bool value) 
		{ 
			return value ? 1 : 0; 
		}
	
		public static int ToInt32 (byte value) 
		{ 
			return (int)value; 
		}

		public static int ToInt32 (char value) 
		{ 
			return (int)value; 
		}
	
		public static int ToInt32 (decimal value) 
		{ 
			if (value > Int32.MaxValue || value < Int32.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than Int32.MaxValue or less than Int32.MinValue"));

			// Returned Even-Rounded
			return (int)(Math.Round (value));	  
		}

		public static int ToInt32 (double value) 
		{ 
			if (value > Int32.MaxValue || value < Int32.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than Int32.MaxValue or less than Int32.MinValue"));
	  
			// Returned Even-Rounded
			return (int)(Math.Round (value));	  
		}
 
		public static int ToInt32 (float value) 
		{ 
			if (value > Int32.MaxValue || value < Int32.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than Int32.MaxValue or less than Int32.MinValue"));
	  
			// Returned Even-Rounded, pass as a double, could just call
			// Convert.ToInt32 ( (double)value);
			return (int)(Math.Round ( (double)value));
		}

		public static int ToInt32 (int value) 
		{ 
			return value; 
		}
	
		public static int ToInt32 (long value) 
		{ 
			if (value > Int32.MaxValue || value < Int32.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than Int32.MaxValue or less than Int32.MinValue"));

			return (int)value; 
		}

		public static int ToInt32 (sbyte value) 
		{ 
			return (int)value; 
		}
	
		public static int ToInt32 (short value) 
		{ 
			return (int)value; 
		}

		public static int ToInt32 (string value) 
		{
			return Int32.Parse (value);
		}

		public static int ToInt32 (string value, IFormatProvider provider) 
		{
			return Int32.Parse (value, provider);
		}
	
		public static int ToInt32 (uint value) 
		{ 
			if (value > Int32.MaxValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than Int32.MaxValue"));

			return (int)value; 
		}

		public static int ToInt32 (ulong value) 
		{ 
			if (value > Int32.MaxValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than Int32.MaxValue"));

			return (int)value; 
		}

		public static int ToInt32 (ushort value) 
		{ 
			return (int)value; 
		}

		// ========== Int64 Conversions ========== //

		public static long ToInt64 (bool value) 
		{ 
			return value ? 1 : 0; 
		}
	
		public static long ToInt64 (byte value) 
		{ 
			return (long)value; 
		}

		public static long ToInt64 (char value) 
		{ 
			return (long)value; 
		}
	
		public static long ToInt64 (decimal value) 
		{ 
			if (value > Int64.MaxValue || value < Int64.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than Int64.MaxValue or less than Int64.MinValue"));
	  
			// Returned Even-Rounded
			return (long)(Math.Round (value));	  
		}

		public static long ToInt64 (double value) 
		{ 
			if (value > Int64.MaxValue || value < Int64.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than Int64.MaxValue or less than Int64.MinValue"));
	  
			// Returned Even-Rounded
			return (long)(Math.Round (value));	  
		}
 
		public static long ToInt64 (float value) 
		{ 
			if (value > Int64.MaxValue || value < Int64.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than Int64.MaxValue or less than Int64.MinValue"));
	  
			// Returned Even-Rounded, pass to Math as a double, could
			// just call Convert.ToInt64 ( (double)value);
			return (long)(Math.Round ( (double)value));
		}

		public static long ToInt64 (int value) 
		{ 
			return (long)value; 
		}
	
		public static long ToInt64 (long value) 
		{ 
			return value; 
		}

		public static long ToInt64 (sbyte value) 
		{ 
			return (long)value; 
		}
	
		public static long ToInt64 (short value) 
		{ 
			return (long)value; 
		}

		public static long ToInt64 (string value) 
		{
			return Int64.Parse (value);
		}

		public static long ToInt64 (string value, IFormatProvider provider) 
		{
			return Int64.Parse (value, provider);
		}
	
		public static long ToInt64 (uint value) 
		{ 
			return (long)value; 
		}

		public static long ToInt64 (ulong value) 
		{ 
			if (value > Int64.MaxValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than Int64.MaxValue"));

			return (long)value; 
		}

		public static long ToInt64 (ushort value) 
		{ 
			return (long)value; 
		}

		// ========== SByte Conversions ========== //
	
		public static sbyte ToSByte (bool value) 
		{ 
			return (sbyte)(value ? 1 : 0); 
		}
	
		public static sbyte ToSByte (byte value) 
		{ 
			if (value > SByte.MaxValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than SByte.MaxValue"));

			return (sbyte)value; 
		}

		public static sbyte ToSByte (char value) 
		{ 
			if (value > SByte.MaxValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than SByte.MaxValue"));

			return (sbyte)value;
		}
	
		public static sbyte ToSByte (decimal value) 
		{ 
			if (value > SByte.MaxValue || value < SByte.MinValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than SByte.MaxValue or less than SByte.MinValue"));
	  
			// Returned Even-Rounded
			return (sbyte)(Math.Round (value));
		}
	
		public static sbyte ToSByte (double value) 
		{ 
			if (value > SByte.MaxValue || value < SByte.MinValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than SByte.MaxValue or less than SByte.MinValue"));

			// Returned Even-Rounded
			return (sbyte)(Math.Round (value));
		}

		public static sbyte ToSByte (float value) 
		{ 
			if (value > SByte.MaxValue || value < SByte.MinValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than SByte.MaxValue or less than SByte.Minalue"));

			// Returned Even-Rounded, pass as double to Math
			return (sbyte)(Math.Round ( (double)value));
		}

		public static sbyte ToSByte (int value) 
		{ 
			if (value > SByte.MaxValue || value < SByte.MinValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than SByte.MaxValue or less than SByte.MinValue"));
	  
			return (sbyte)value; 
		}

		public static sbyte ToSByte (long value) 
		{ 
			if (value > SByte.MaxValue || value < SByte.MinValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than SByte.MaxValue or less than SByte.MinValue"));
	  
			return (sbyte)value;
		}

		public static sbyte ToSByte (sbyte value) 
		{ 
			return value;
		}
	
		public static sbyte ToSByte (short value) 
		{ 
			if (value > SByte.MaxValue || value < SByte.MinValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than SByte.MaxValue or less than SByte.MinValue"));
	  
			return (sbyte)value; 
		}

		public static sbyte ToSByte (string value) 
		{
			return SByte.Parse (value);
		}

		public static sbyte ToSByte (string value, IFormatProvider provider) 
		{
			return SByte.Parse (value, provider);
		}
	
		public static sbyte ToSByte (uint value) 
		{ 
			if (value > SByte.MaxValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than SByte.MaxValue"));

			return (sbyte)value;
		}

		public static sbyte ToSByte (ulong value) 
		{ 
			if (value > (ulong)SByte.MaxValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than SByte.MaxValue"));

			return (sbyte)value;
		}

		public static sbyte ToSByte (ushort value) 
		{ 
			if (value > SByte.MaxValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than SByte.MaxValue"));

			return (sbyte)value;
		}

		// ========== Single Conversions ========== //
	
		public static float ToSingle (bool value) 
		{ 
			return value ? 1 : 0; 
		}
	
		public static float ToSingle (byte value) 
		{ 
			return (float)value; 
		}
	
		public static float ToSingle (decimal value) 
		{ 
			return (float)value; 
		}

		public static float ToSingle (double value) 
		{ 
			if (value > Single.MaxValue || value < Single.MinValue)
				throw new OverflowException (Locale.GetText (
					"Value is greater than Single.MaxValue or less than Single.MinValue"));

			return (float)value; 
		}
	
		public static float ToSingle (float value) 
		{ 
			return value; 
		}

		public static float ToSingle (int value) 
		{ 
			return (float)value; 
		}
	
		public static float ToSingle (long value) 
		{ 
			return (float)value; 
		}

		public static float ToSingle (sbyte value) 
		{ 
			return (float)value; 
		}
	
		public static float ToSingle (short value) 
		{ 
			return (float)value; 
		}

		public static float ToSingle (string value) 
		{
			return Single.Parse (value);
		}

		public static float ToSingle (string value, IFormatProvider provider) 
		{
			return Single.Parse (value, provider);
		}
	
		public static float ToSingle (uint value) 
		{ 
			return (float)value; 
		}

		public static float ToSingle (ulong value) 
		{ 
			return (float)value; 
		}

		public static float ToSingle (ushort value) 
		{ 
			return (float)value; 
		}

		// ========== String Conversions ========== //
	
		public static string ToString (bool value) 
		{ 
			return value.ToString (); 
		}
	
		public static string ToString (byte value) 
		{ 
			return value.ToString (); 
		}
	
		public static string ToString (byte value, IFormatProvider provider) 
		{
			return value.ToString (provider); 
		}

		public static string ToString (char value) 
		{ 
			return value.ToString (); 
		}

		public static string ToString (DateTime value) 
		{ 
			return value.ToString (); 
		}

		public static string ToString (DateTime value, IFormatProvider provider) 
		{ 
			return value.ToString (provider); 
		}

		public static string ToString (decimal value) 
		{
			return value.ToString ();
		}

		public static string ToString (decimal value, IFormatProvider provider) 
		{ 
			return value.ToString (provider); 
		}
	
		public static string ToString (double value) 
		{ 
			return value.ToString (); 
		}

		public static string ToString (double value, IFormatProvider provider) 
		{ 
			return value.ToString (provider);
		}
	
		public static string ToString (float value) 
		{ 
			return value.ToString (); 
		}

		public static string ToString (float value, IFormatProvider provider) 
		{ 
			return value.ToString (provider); 
		}

		public static string ToString (int value) 
		{ 
			return value.ToString (); 
		}

		public static string ToString (int value, IFormatProvider provider) 
		{ 
			return value.ToString (provider); 
		}
	
		public static string ToString (long value) 
		{ 
			return value.ToString (); 
		}

		public static string ToString (long value, IFormatProvider provider) 
		{ 
			return value.ToString (provider); 
		}

		public static string ToString (sbyte value) 
		{ 
			return value.ToString (); 
		}

		public static string ToString (sbyte value, IFormatProvider provider) 
		{ 
			return value.ToString (provider); 
		}
	
		public static string ToString (short value) 
		{ 
			return value.ToString (); 
		}

		public static string ToString (short value, IFormatProvider provider) 
		{ 
			return value.ToString (provider); 
		}

		public static string ToString (string value) 
		{
			return value;
		}

		public static string ToString (uint value) 
		{ 
			return value.ToString (); 
		}

		public static string ToString (uint value, IFormatProvider provider) 
		{ 
			return value.ToString (provider); 
		}

		public static string ToString (ulong value) 
		{ 
			return value.ToString (); 
		}

		public static string ToString (ulong value, IFormatProvider provider) 
		{ 
			return value.ToString (provider); 
		}

		public static string ToString (ushort value) 
		{ 
			return value.ToString (); 
		}

		public static string ToString (ushort value, IFormatProvider provider) 
		{ 
			return value.ToString (provider); 
		}

		// ========== UInt16 Conversions ========== //

		public static ushort ToUInt16 (bool value) 
		{ 
			return (ushort)(value ? 1 : 0); 
		}
	
		public static ushort ToUInt16 (byte value) 
		{ 
			return (ushort)value; 
		}

		public static ushort ToUInt16 (char value) 
		{ 
			return (ushort)value; 
		}
	
		public static ushort ToUInt16 (decimal value) 
		{ 
			if (value > UInt16.MaxValue || value < UInt16.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than UInt16.MaxValue or less than UInt16.MinValue"));
	  
			// Returned Even-Rounded
			return (ushort)(Math.Round (value));	  
		}

		public static ushort ToUInt16 (double value) 
		{ 
			if (value > UInt16.MaxValue || value < UInt16.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than UInt16.MaxValue or less than UInt16.MinValue"));
	  
			// Returned Even-Rounded
			return (ushort)(Math.Round (value));
		}
 
		public static ushort ToUInt16 (float value) 
		{ 
			if (value > UInt16.MaxValue || value < UInt16.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than UInt16.MaxValue or less than UInt16.MinValue"));
	  
			// Returned Even-Rounded, pass as double to Math
			return (ushort)(Math.Round ( (double)value));
		}

		public static ushort ToUInt16 (int value) 
		{ 
			if (value > UInt16.MaxValue || value < UInt16.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than UInt16.MaxValue or less than UInt16.MinValue"));

			return (ushort)value; 
		}
	
		public static ushort ToUInt16 (long value) 
		{ 
			if (value > UInt16.MaxValue || value < UInt16.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than UInt16.MaxValue or less than UInt16.MinValue"));

			return (ushort)value; 
		}

		public static ushort ToUInt16 (sbyte value) 
		{ 
			if (value < UInt16.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is less than UInt16.MinValue"));

			return (ushort)value; 
		}
	
		public static ushort ToUInt16 (short value) 
		{ 
			if (value < UInt16.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is less than UInt16.MinValue"));

			return (ushort)value; 
		}

		public static ushort ToUInt16 (string value) 
		{
			return UInt16.Parse (value);
		}

		public static ushort ToUInt16 (string value, IFormatProvider provider) 
		{
			return UInt16.Parse (value, provider);
		}
	
		public static ushort ToUInt16 (uint value) 
		{ 
			if (value > UInt16.MaxValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than UInt16.MaxValue"));

			return (ushort)value; 
		}

		public static ushort ToUInt16 (ulong value) 
		{ 
			if (value > (ulong)UInt16.MaxValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than UInt16.MaxValue"));

			return (ushort)value; 
		}

		public static ushort ToUInt16 (ushort value) 
		{ 
			return value; 
		}

		// ========== UInt32 Conversions ========== //

		public static uint ToUInt32 (bool value) 
		{ 
			return (uint)(value ? 1 : 0); 
		}
	
		public static uint ToUInt32 (byte value) 
		{ 
			return (uint)value; 
		}

		public static uint ToUInt32 (char value) 
		{ 
			return (uint)value; 
		}
	
		public static uint ToUInt32 (decimal value) 
		{ 
			if (value > UInt32.MaxValue || value < UInt32.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than UInt32.MaxValue or less than UInt32.MinValue"));
	  
			// Returned Even-Rounded
			return (uint)(Math.Round (value));	  
		}

		public static uint ToUInt32 (double value) 
		{ 
			if (value > UInt32.MaxValue || value < UInt32.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than UInt32.MaxValue or less than UInt32.MinValue"));
	  
			// Returned Even-Rounded
			return (uint)(Math.Round (value));	  
		}
 
		public static uint ToUInt32 (float value) 
		{ 
			if (value > UInt32.MaxValue || value < UInt32.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than UInt32.MaxValue or less than UInt32.MinValue"));
	  
			// Returned Even-Rounded, pass as double to Math
			return (uint)(Math.Round ( (double)value));
		}

		public static uint ToUInt32 (int value) 
		{ 
			if (value < UInt32.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is less than UInt32.MinValue"));

			return (uint)value; 
		}
	
		public static uint ToUInt32 (long value) 
		{ 
			if (value > UInt32.MaxValue || value < UInt32.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than UInt32.MaxValue or less than UInt32.MinValue"));

			return (uint)value; 
		}

		public static uint ToUInt32 (sbyte value) 
		{ 
			if (value < UInt32.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is less than UInt32.MinValue"));

			return (uint)value; 
		}
	
		public static uint ToUInt32 (short value) 
		{ 
			if (value < UInt32.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is less than UInt32.MinValue"));

			return (uint)value; 
		}

		public static uint ToUInt32 (string value) 
		{
			return UInt32.Parse (value);
		}

		public static uint ToUInt32 (string value, IFormatProvider provider) 
		{
			return UInt32.Parse (value, provider);
		}
	
		public static uint ToUInt32 (uint value) 
		{ 
			return value; 
		}

		public static uint ToUInt32 (ulong value) 
		{ 
			if (value > UInt32.MaxValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than UInt32.MaxValue"));

			return (uint)value; 
		}

		public static uint ToUInt32 (ushort value) 
		{ 
			return (uint)value; 
		}

		// ========== UInt64 Conversions ========== //

		public static ulong ToUInt64 (bool value) 
		{ 
			return (ulong)(value ? 1 : 0); 
		}
	
		public static ulong ToUInt64 (byte value) 
		{ 
			return (ulong)value; 
		}

		public static ulong ToUInt64 (char value) 
		{ 
			return (ulong)value; 
		}
	
		public static ulong ToUInt64 (decimal value) 
		{ 
			if (value > UInt64.MaxValue || value < UInt64.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than UInt64.MaxValue or less than UInt64.MinValue"));
	  
			// Returned Even-Rounded
			return (ulong)(Math.Round (value));	  
		}

		public static ulong ToUInt64 (double value) 
		{ 
			if (value > UInt64.MaxValue || value < UInt64.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than UInt64.MaxValue or less than UInt64.MinValue"));
	  
			// Returned Even-Rounded
			return (ulong)(Math.Round (value));	  
		}
 
		public static ulong ToUInt64 (float value) 
		{ 
			if (value > UInt64.MaxValue || value < UInt64.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is greater than UInt64.MaxValue or less than UInt64.MinValue"));
	  
			// Returned Even-Rounded, pass as a double to Math
			return (ulong)(Math.Round ( (double)value));
		}

		public static ulong ToUInt64 (int value) 
		{ 
			if (value < (int)UInt64.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is less than UInt64.MinValue"));

			return (ulong)value; 
		}
	
		public static ulong ToUInt64 (long value) 
		{ 
			if (value < (long)UInt64.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is less than UInt64.MinValue"));

			return (ulong)value; 
		}

		public static ulong ToUInt64 (sbyte value) 
		{ 
			if (value < (sbyte)UInt64.MinValue) 
				throw new OverflowException
				("Value is less than UInt64.MinValue");

			return (ulong)value; 
		}
	
		public static ulong ToUInt64 (short value) 
		{ 
			if (value < (short)UInt64.MinValue) 
				throw new OverflowException (Locale.GetText (
					"Value is less than UInt64.MinValue"));

			return (ulong)value; 
		}

		public static ulong ToUInt64 (string value) 
		{
			return UInt64.Parse (value);
		}

		public static ulong ToUInt64 (string value, IFormatProvider provider) 
		{
			return UInt64.Parse (value, provider);
		}
	
		public static ulong ToUInt64 (uint value) 
		{ 
			return (ulong)value; 
		}

		public static ulong ToUInt64 (ulong value) 
		{ 
			return value; 
		}

		public static ulong ToUInt64 (ushort value) 
		{ 
			return (ulong)value; 
		}

		// ========== Conversion / Helper Fucntions ========== //

		public static object ChangeType (object value, Type conversionType)
		{
			CultureInfo ci = CultureInfo.CurrentCulture;
			NumberFormatInfo number = ci.NumberFormat;
			return ToType (value, conversionType, number);
		}
		
		public static object ChangeType (object value, TypeCode typeCode)
		{
			CultureInfo ci = CultureInfo.CurrentCulture;
			Type conversionType = conversionTable [(int)typeCode];
			NumberFormatInfo number = ci.NumberFormat;
			return ToType (value, conversionType, number);
		}

		public static object ChangeType (object value, Type conversionType, IFormatProvider provider)
		{
			return ToType (value, conversionType, provider);
		}
		
		public static object ChangeType (object value, TypeCode typeCode, IFormatProvider provider)
		{
			Type conversionType = conversionTable [(int)typeCode];
			return ToType (value, conversionType, provider);
		}	

                // Lookup table for the conversion ToType method. Order
		// is important! Used by ToType for comparing the target
		// type, and uses hardcoded array indexes.
		private static Type[] conversionTable = {
			// Valid ICovnertible Types
			typeof (Boolean),  //  0 TypeCode.Boolean
			typeof (Byte),     //  1 TypeCode.Byte
			typeof (Char),     //  2 TypeCode.Char
			typeof (DateTime), //  3 TypeCode.DateTime
			typeof (Decimal),  //  4 TypeCode.Decimal
			typeof (Double),   //  5 TypeCode.Double
			typeof (Int16),    //  6 TypeCode.Int16
			typeof (Int32),    //  7 TypeCode.Int32
			typeof (Int64),    //  8 TypeCode.Int64
			typeof (SByte),    //  9 TypeCode.Sbyte
			typeof (Single),   // 10 TypeCode.Single
			typeof (String),   // 11 TypeCode.String
			typeof (UInt16),   // 12 TypeCode.UInt16
			typeof (UInt32),   // 13 TypeCode.UInt32
			typeof (UInt64),   // 14 TypeCode.UInt64
	  
			// Invalid IConvertible Interface Types
			typeof (Object)    // 15 TypeCode.Object
		};

		// Function to convert an object to another type and return
		// it as an object. In place for the core data types to use
		// when implementing IConvertible. Uses hardcoded indexes in 
		// the conversionTypes array, so if modify carefully.
		internal static object ToType (object value, Type conversionType, 
					       IFormatProvider provider) 
		{
			if (value == null)
				throw new ArgumentException (Locale.GetText (
					"Invalid conversion from null value"));

			if (value is IConvertible) {
				IConvertible convertValue = (IConvertible)value;

				if (conversionType == conversionTable[0]) {
					// 0 TypeCode.Boolean
					return (object)(convertValue.ToBoolean (provider));

				} else if (conversionType == conversionTable[1]) {
					// 1 TypeCode.Byte
					return (object)(convertValue.ToByte (provider));
		  
				} else if (conversionType == conversionTable[2]) {
					// 2 TypeCode.Char
					return (object)(convertValue.ToChar (provider));
		  
				} else if (conversionType == conversionTable[3]) {
					// 3 TypeCode.DateTime
					return (object)(convertValue.ToDateTime (provider));
		  
				} else if (conversionType == conversionTable[4]) {
					// 4 TypeCode.Decimal
					return (object)(convertValue.ToDecimal (provider));
		  
				} else if (conversionType == conversionTable[5]) {
					// 5 TypeCode.Double
					return (object)(convertValue.ToDouble (provider));

				} else if (conversionType == conversionTable[6]) {
					// 6 TypeCode.Int16
					return (object)(convertValue.ToInt16 (provider));
		  
				} else if (conversionType == conversionTable[7]) {
					// 7 TypeCode.Int32
					return (object)(convertValue.ToInt32 (provider));
		  
				} else if (conversionType == conversionTable[8]) {
					// 8 TypeCode.Int64
					return (object)(convertValue.ToInt64 (provider));
		  
				} else if (conversionType == conversionTable[9]) {
					// 9 TypeCode.Sbyte
					return (object)(convertValue.ToSByte (provider));
		  
				} else if (conversionType == conversionTable[10]) {
					// 10 TypeCode.Single
					return (object)(convertValue.ToSingle (provider));
		  
				} else if (conversionType == conversionTable[11]) {
					// 11 TypeCode.String
					return (object)(convertValue.ToString (provider));
		  
				} else if (conversionType == conversionTable[12]) {  
					// 12 TypeCode.UInt16
					return (object)(convertValue.ToUInt16 (provider));
		  
				} else if (conversionType == conversionTable[13]) {
					// 13 TypeCode.UInt32
					return (object)(convertValue.ToUInt32 (provider));
		  
				} else if (conversionType == conversionTable[14]) {
					// 14 TypeCode.UInt64
					return (object)(convertValue.ToUInt64 (provider));

				} else if (conversionType == conversionTable[15]) {
					// 15 TypeCode.Object
					return (object)(value);

				} else 	{ 		
					// Not in the conversion table
					throw new InvalidCastException (Locale.GetText (
						"Unknown target conversion type"));
				}
			} else {
				// Value is not IConvertible
				throw new ArgumentException (Locale.GetText (
					"Value is not a convertible object: "+ value.GetType().ToString()+" to "+conversionType.ToString()));
			}
		}
	}
}




