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
// There are also conversion functions that are not defined in
// the ECMA draft, such as there is no bool ToBoolean(DateTime value), 
// and placing that somewhere won't compile w/ this Convert since the
// function doesn't exist. However calling that when using Microsoft's
// System.Convert doesn't produce any compiler errors, it just throws
// an InvalidCastException at runtime.
//
// The .NET Framework SDK lists DBNull as a member of this class
// as 'public static readonly object DBNull;'. 
//
// It should also be decided if all the cast return values should be
// returned as unchecked or not.
//

namespace System {
    
    /// <summary>
    /// Class to convert between data types.</summary>
    public sealed class Convert {
	
	// ========== Boolean Conversions ========== //
	
	public static bool ToBoolean(bool value) { 
	    return value; 
	}
	
	public static bool ToBoolean(byte value) { 
	    return (value != 0); 
	}

	public static bool ToBoolean(decimal value) { 
	    return (value != 0M); 
	}

	public static bool ToBoolean(double value) { 
	    return (value != 0); 
	}

	public static bool ToBoolean(float value) { 
	    return (value != 0f); 
	}

	public static bool ToBoolean(int value) { 
	    return (value != 0); 
	}

	public static bool ToBoolean(long value) { 
	    return (value != 0); 
	}

	public static bool ToBoolean(sbyte value) { 
	    return (value != 0); 
	}
	
	public static bool ToBoolean(short value) { 
	    return (value != 0); 
	}

	public static bool ToBoolean(string value) {
	    return Boolean.Parse(value);
	}
	
	public static bool ToBoolean(uint value) { 
	    return (value != 0);
	}

	public static bool ToBoolean(ulong value) { 
	    return (value != 0); 
	}

	public static bool ToBoolean(ushort value) { 
	    return (value != 0); 
	}

	// ========== Byte Conversions ========== //
	
	public static byte ToByte(bool value) { 
	    return (byte)(value ? 1 : 0); 
	}
	
	public static byte ToByte(byte value) { 
	    return value; 
	}

	public static byte ToByte(char value) { 
	    if(value > Byte.MaxValue)
		throw new OverflowException
		    ("Value is greater than Byte.MaxValue");

	    return (byte)value;
	}
	
	public static byte ToByte(decimal value) { 
	    if(value > Byte.MaxValue || value < Byte.MinValue)
		throw new OverflowException
		    ("Value is greater than Byte.MaxValue or less than Byte.MinValue");
	    
	    return (byte)value;
	}
	
	public static byte ToByte(double value) { 
	    if(value > Byte.MaxValue || value < Byte.MinValue)
		throw new OverflowException
		    ("Value is greater than Byte.MaxValue or less than Byte.MinValue");
	   
	    // This and the float version of ToByte are the only ones
	    // the spec listed as checking for .NaN and Infinity overflow
	    if(value == Double.NaN || value == Double.PositiveInfinity ||
	       value == Double.NegativeInfinity)
		throw new OverflowException
		    ("Value is equal to Double.NaN, Double.PositiveInfinity, or Double.NegativeInfinity");

	    return (byte)value;
	}

	public static byte ToByte(float value) { 
	    if(value > Byte.MaxValue || value < Byte.MinValue)
		throw new OverflowException
		    ("Value is greater than Byte.MaxValue or less than Byte.Minalue");

	    // This and the double version of ToByte are the only ones
	    // the spec listed as checking for .NaN and Infinity overflow
	    if(value == Single.NaN || value == Single.PositiveInfinity ||
	       value == Single.NegativeInfinity)
		throw new OverflowException
		    ("Value is equal to Single.NaN, Single.PositiveInfinity, or Single.NegativeInfinity");

	    return (byte)value;
	}

	public static byte ToByte(int value) { 
	    if(value > Byte.MaxValue || value < Byte.MinValue)
		throw new OverflowException
		    ("Value is greater than Byte.MaxValue or less than Byte.MinValue");
	    
	    return (byte)value; 
	}

	public static byte ToByte(long value) { 
	    if(value > Byte.MaxValue || value < Byte.MinValue)
		throw new OverflowException
		    ("Value is greater than Byte.MaxValue or less than Byte.MinValue");
	    
	    return (byte)value;
	}

	public static byte ToByte(sbyte value) { 
	    if(value < Byte.MinValue)
		throw new OverflowException
		    ("Value is less than Byte.MinValue");
	    
	    return (byte)value;
	}
	
	public static byte ToByte(short value) { 
	    if(value > Byte.MaxValue || value < Byte.MinValue)
		throw new OverflowException
		    ("Value is greater than Byte.MaxValue or less than Byte.MinValue");
	    
	    return (byte)value; 
	}

	public static byte ToByte(string value) {
	    return Byte.Parse(value);
	}

	public static byte ToByte(string value, IFormatProvider provider) {
	    return Byte.Parse(value, provider);
	}
	
	public static byte ToByte(uint value) { 
	    if(value > Byte.MaxValue)
		throw new OverflowException
		    ("Value is greater than Byte.MaxValue");

	    return (byte)value;
	}

	public static byte ToByte(ulong value) { 
	    if(value > Byte.MaxValue)
		throw new OverflowException
		    ("Value is greater than Byte.MaxValue");

	    return (byte)value;
	}

	public static byte ToByte(ushort value) { 
	    if(value > Byte.MaxValue)
		throw new OverflowException
		    ("Value is greater than Byte.MaxValue");

	    return (byte)value;
	}

	// ========== Char Conversions ========== //
	
	public static char ToChar(byte value) { 
	    return (char)value;
	}

	public static char ToChar(char value) { 
	    return value;
	}

	public static char ToChar(int value) { 
	    if(value > Char.MaxValue || value < Char.MinValue)
		throw new OverflowException
		    ("Value is greater than Char.MaxValue or less than Char.MinValue");
	    
	    return (char)value; 
	}

	public static char ToChar(long value) { 
	    if(value > Char.MaxValue || value < Char.MinValue)
		throw new OverflowException
		    ("Value is greater than Char.MaxValue or less than Char.MinValue");
	    
	    return (char)value; 
	}

	public static char ToChar(sbyte value) { 
	    if(value < Char.MinValue)
		throw new OverflowException
		    ("Value is less than Char.MinValue");
	    
	    return (char)value; 
	}
	
	public static char ToChar(short value) { 
	    if(value < Char.MinValue)
		throw new OverflowException
		    ("Value is less than Char.MinValue");
	    
	    return (char)value;  
	}

	public static char ToChar(string value) {
	    return Char.Parse(value);
	}
	
	public static char ToChar(uint value) { 
	    if(value > Char.MaxValue)
		throw new OverflowException
		    ("Value is greater than Char.MaxValue");
	    
	    return (char)value;  
	}

	public static char ToChar(ulong value) { 
	    if(value > Char.MaxValue)
		throw new OverflowException
		    ("Value is greater than Char.MaxValue");
	    
	    return (char)value;  
	}

	public static char ToChar(ushort value) { 
	    if(value > Char.MaxValue)
		throw new OverflowException
		    ("Value is greater than Char.MaxValue");
	    
	    return (char)value;  
	}

	// ========== DateTime Conversions ========== //
	
	public static DateTime ToDateTime(string value) { 
	    return DateTime.Parse(value);
	}
	
	public static DateTime ToDateTime(string value, IFormatProvider provider) {
	    return DateTime.Parse(value, provider);
	}

	// ========== Decimal Conversions ========== //
	
	public static decimal ToDecimal(bool value) { 
	    return value ? 1 : 0; 
	}
	
	public static decimal ToDecimal(byte value) { 
	    return (decimal)value; 
	}
	
	public static decimal ToDecimal(decimal value) { 
	    return value; 
	}

	public static decimal ToDecimal(double value) { 
	    if(value > (double)Decimal.MaxValue || value < (double)Decimal.MinValue) 
		throw new OverflowException
		    ("Value is greater than Decimal.MaxValue or less than Decimal.MinValue");

	    return (decimal)value; 
	}

	public static decimal ToDecimal(float value) { 
	    if(value > (double)Decimal.MaxValue || value < (double)Decimal.MinValue) 
		throw new OverflowException
		    ("Value is greater than Decimal.MaxValue or less than Decimal.MinValue");

	    return (decimal)value; 
	}

	public static decimal ToDecimal(int value) { 
	    return (decimal)value; 
	}
	
	public static decimal ToDecimal(long value) { 
	    return (decimal)value; 
	}

	public static decimal ToDecimal(sbyte value) { 
	    return (decimal)value;  
	}
	
	public static decimal ToDecimal(short value) { 
	    return (decimal)value; 
	}

	public static decimal ToDecimal(string value) {
	    return Decimal.Parse(value);
	}

	public static decimal ToDecimal(string value, IFormatProvider provider) {
	    return Decimal.Parse(value, provider);
	}
	
	public static decimal ToDecimal(uint value) { 
	    return (decimal)value; 
	}

	public static decimal ToDecimal(ulong value) { 
	    return (decimal)value; 
	}

	public static decimal ToDecimal(ushort value) { 
	    return (decimal)value; 
	}

	// ========== Double Conversions ========== //
	
	public static double ToDouble(bool value) { 
	    return value ? 1 : 0; 
	}
	
	public static double ToDouble(byte value) { 
	    return (double)value; 
	}
	
	public static double ToDouble(decimal value) { 
	    return (double)value; 
	}

	public static double ToDouble(double value) { 
	    return value; 
	}

	public static double ToDouble(float value) { 
	    return (double)value; 
	}

	public static double ToDouble(int value) { 
	    return (double)value; 
	}
	
	public static double ToDouble(long value) { 
	    return (double)value; 
	}

	public static double ToDouble(sbyte value) { 
	    return (double)value;  
	}
	
	public static double ToDouble(short value) { 
	    return (double)value; 
	}

	public static double ToDouble(string value) {
	    return Double.Parse(value);
	}

	public static double ToDouble(string value, IFormatProvider provider) {
	    return Double.Parse(value, provider);
	}
	
	public static double ToDouble(uint value) { 
	    return (double)value; 
	}

	public static double ToDouble(ulong value) { 
	    return (double)value; 
	}

	public static double ToDouble(ushort value) { 
	    return (double)value; 
	}

	// ========== Int16 Conversions ========== //

	public static short ToInt16(bool value) { 
	    return (short)(value ? 1 : 0); 
	}
	
	public static short ToInt16(byte value) { 
	    return (short)value; 
	}

	public static short ToInt16(char value) { 
	    if(value > Int16.MaxValue) 
		throw new OverflowException
		    ("Value is greater than Int16.MaxValue");

	    return (short)value; 
	}
	
	public static short ToInt16(decimal value) { 
	    if(value > Int16.MaxValue || value < Int16.MinValue) 
		throw new OverflowException
		    ("Value is greater than Int16.MaxValue or less than Int16.MinValue");
	    
	    return (short)value;	    
	}

	public static short ToInt16(double value) { 
	    if(value > Int16.MaxValue || value < Int16.MinValue) 
		throw new OverflowException
		    ("Value is greater than Int16.MaxValue or less than Int16.MinValue");
	    
	    return (short)value;	    
	}
 
	public static short ToInt16(float value) { 
	    if(value > Int16.MaxValue || value < Int16.MinValue) 
		throw new OverflowException
		    ("Value is greater than Int16.MaxValue or less than Int16.MinValue");
	    
	    return (short)value;
	}

	public static short ToInt16(int value) { 
	    if(value > Int16.MaxValue || value < Int16.MinValue) 
		throw new OverflowException
		    ("Value is greater than Int16.MaxValue or less than Int16.MinValue");

	    return (short)value; 
	}
	
	public static short ToInt16(long value) { 
	    if(value > Int16.MaxValue || value < Int16.MinValue) 
		throw new OverflowException
		    ("Value is greater than Int16.MaxValue or less than Int16.MinValue");

	    return (short)value; 
	}

	public static short ToInt16(sbyte value) { 
	    return (short)value;  
	}
	
	public static short ToInt16(short value) { 
	    return value; 
	}

	public static short ToInt16(string value) {
	    return Int16.Parse(value);
	}

	public static short ToInt16(string value, IFormatProvider provider) {
	    return Int16.Parse(value, provider);
	}
	
	public static short ToInt16(uint value) { 
	    if(value > Int16.MaxValue) 
		throw new OverflowException
		    ("Value is greater than Int16.MaxValue");

	    return (short)value; 
	}

	public static short ToInt16(ulong value) { 
	    if(value > (ulong)Int16.MaxValue) 
		throw new OverflowException
		    ("Value is greater than Int16.MaxValue");

	    return (short)value; 
	}

	public static short ToInt16(ushort value) { 
	    if(value > Int16.MaxValue) 
		throw new OverflowException
		    ("Value is greater than Int16.MaxValue");

	    return (short)value; 
	}

	// ========== Int32 Conversions ========== //

	public static int ToInt32(bool value) { 
	    return value ? 1 : 0; 
	}
	
	public static int ToInt32(byte value) { 
	    return (int)value; 
	}

	public static int ToInt32(char value) { 
	    return (int)value; 
	}
	
	public static int ToInt32(decimal value) { 
	    if(value > Int32.MaxValue || value < Int32.MinValue) 
		throw new OverflowException
		    ("Value is greater than Int32.MaxValue or less than Int32.MinValue");
	    
	    return (int)value;	    
	}

	public static int ToInt32(double value) { 
	    if(value > Int32.MaxValue || value < Int32.MinValue) 
		throw new OverflowException
		    ("Value is greater than Int32.MaxValue or less than Int32.MinValue");
	    
	    return (int)value;	    
	}
 
	public static int ToInt32(float value) { 
	    if(value > Int32.MaxValue || value < Int32.MinValue) 
		throw new OverflowException
		    ("Value is greater than Int32.MaxValue or less than Int32.MinValue");
	    
	    return (int)value;
	}

	public static int ToInt32(int value) { 
	    return value; 
	}
	
	public static int ToInt32(long value) { 
	    if(value > Int32.MaxValue || value < Int32.MinValue) 
		throw new OverflowException
		    ("Value is greater than Int32.MaxValue or less than Int32.MinValue");

	    return (int)value; 
	}

	public static int ToInt32(sbyte value) { 
	    return (int)value;  
	}
	
	public static int ToInt32(short value) { 
	    return (int)value; 
	}

	public static int ToInt32(string value) {
	    return Int32.Parse(value);
	}

	public static int ToInt32(string value, IFormatProvider provider) {
	    return Int32.Parse(value, provider);
	}
	
	public static int ToInt32(uint value) { 
	    if(value > Int32.MaxValue) 
		throw new OverflowException
		    ("Value is greater than Int32.MaxValue");

	    return (int)value; 
	}

	public static int ToInt32(ulong value) { 
	    if(value > Int32.MaxValue) 
		throw new OverflowException
		    ("Value is greater than Int32.MaxValue");

	    return (int)value; 
	}

	public static int ToInt32(ushort value) { 
	    return (int)value; 
	}

	// ========== Int64 Conversions ========== //

	public static long ToInt64(bool value) { 
	    return value ? 1 : 0; 
	}
	
	public static long ToInt64(byte value) { 
	    return (long)value; 
	}

	public static long ToInt64(char value) { 
	    return (long)value; 
	}
	
	public static long ToInt64(decimal value) { 
	    if(value > Int64.MaxValue || value < Int64.MinValue) 
		throw new OverflowException
		    ("Value is greater than Int64.MaxValue or less than Int64.MinValue");
	    
	    return (long)value;	    
	}

	public static long ToInt64(double value) { 
	    if(value > Int64.MaxValue || value < Int64.MinValue) 
		throw new OverflowException
		    ("Value is greater than Int64.MaxValue or less than Int64.MinValue");
	    
	    return (long)value;	    
	}
 
	public static long ToInt64(float value) { 
	    if(value > Int64.MaxValue || value < Int64.MinValue) 
		throw new OverflowException
		    ("Value is greater than Int64.MaxValue or less than Int64.MinValue");
	    
	    return (long)value;
	}

	public static long ToInt64(int value) { 
	    return (long)value; 
	}
	
	public static long ToInt64(long value) { 
	    return value; 
	}

	public static long ToInt64(sbyte value) { 
	    return (long)value;  
	}
	
	public static long ToInt64(short value) { 
	    return (long)value; 
	}

	public static long ToInt64(string value) {
	    return Int64.Parse(value);
	}

	public static long ToInt64(string value, IFormatProvider provider) {
	    return Int64.Parse(value, provider);
	}
	
	public static long ToInt64(uint value) { 
	    return (long)value; 
	}

	public static long ToInt64(ulong value) { 
	    if(value > Int64.MaxValue) 
		throw new OverflowException
		    ("Value is greater than Int64.MaxValue");

	    return (long)value; 
	}

	public static long ToInt64(ushort value) { 
	    return (long)value; 
	}

	// ========== SByte Conversions ========== //
	
	public static sbyte ToSByte(bool value) { 
	    return (sbyte)(value ? 1 : 0); 
	}
	
	public static sbyte ToSByte(byte value) { 
	    if(value > SByte.MaxValue)
		throw new OverflowException
		    ("Value is greater than SByte.MaxValue");

	    return (sbyte)value; 
	}

	public static sbyte ToSByte(char value) { 
	    if(value > SByte.MaxValue)
		throw new OverflowException
		    ("Value is greater than SByte.MaxValue");

	    return (sbyte)value;
	}
	
	public static sbyte ToSByte(decimal value) { 
	    if(value > SByte.MaxValue || value < SByte.MinValue)
		throw new OverflowException
		    ("Value is greater than SByte.MaxValue or less than SByte.MinValue");
	    
	    return (sbyte)value;
	}
	
	public static sbyte ToSByte(double value) { 
	    if(value > SByte.MaxValue || value < SByte.MinValue)
		throw new OverflowException
		    ("Value is greater than SByte.MaxValue or less than SByte.MinValue");

	    return (sbyte)value;
	}

	public static sbyte ToSByte(float value) { 
	    if(value > SByte.MaxValue || value < SByte.MinValue)
		throw new OverflowException
		    ("Value is greater than SByte.MaxValue or less than SByte.Minalue");

	    return (sbyte)value;
	}

	public static sbyte ToSByte(int value) { 
	    if(value > SByte.MaxValue || value < SByte.MinValue)
		throw new OverflowException
		    ("Value is greater than SByte.MaxValue or less than SByte.MinValue");
	    
	    return (sbyte)value; 
	}

	public static sbyte ToSByte(long value) { 
	    if(value > SByte.MaxValue || value < SByte.MinValue)
		throw new OverflowException
		    ("Value is greater than SByte.MaxValue or less than SByte.MinValue");
	    
	    return (sbyte)value;
	}

	public static sbyte ToSByte(sbyte value) { 
	    return value;
	}
	
	public static sbyte ToSByte(short value) { 
	    if(value > SByte.MaxValue || value < SByte.MinValue)
		throw new OverflowException
		    ("Value is greater than SByte.MaxValue or less than SByte.MinValue");
	    
	    return (sbyte)value; 
	}

	public static sbyte ToSByte(string value) {
	    return SByte.Parse(value);
	}

	public static sbyte ToSByte(string value, IFormatProvider provider) {
	    return SByte.Parse(value, provider);
	}
	
	public static sbyte ToSByte(uint value) { 
	    if(value > SByte.MaxValue)
		throw new OverflowException
		    ("Value is greater than SByte.MaxValue");

	    return (sbyte)value;
	}

	public static sbyte ToSByte(ulong value) { 
	    if(value > (ulong)SByte.MaxValue)
		throw new OverflowException
		    ("Value is greater than SByte.MaxValue");

	    return (sbyte)value;
	}

	public static sbyte ToSByte(ushort value) { 
	    if(value > SByte.MaxValue)
		throw new OverflowException
		    ("Value is greater than SByte.MaxValue");

	    return (sbyte)value;
	}

	// ========== Single Conversions ========== //
	
	public static float ToSingle(bool value) { 
	    return value ? 1 : 0; 
	}
	
	public static float ToSingle(byte value) { 
	    return (float)value; 
	}
	
	public static float ToSingle(decimal value) { 
	    return (float)value; 
	}

	public static float ToSingle(double value) { 
	    if(value > Single.MaxValue || value < Single.MinValue)
		throw new OverflowException
		    ("Value is greater than Single.MaxValue or less than Single.MinValue");

	    return (float)value; 
	}
	
	public static float ToSingle(float value) { 
	    return value; 
	}

	public static float ToSingle(int value) { 
	    return (float)value; 
	}
	
	public static float ToSingle(long value) { 
	    return (float)value; 
	}

	public static float ToSingle(sbyte value) { 
	    return (float)value;  
	}
	
	public static float ToSingle(short value) { 
	    return (float)value; 
	}

	public static float ToSingle(string value) {
	    return Single.Parse(value);
	}

	public static float ToSingle(string value, IFormatProvider provider) {
	    return Single.Parse(value, provider);
	}
	
	public static float ToSingle(uint value) { 
	    return (float)value; 
	}

	public static float ToSingle(ulong value) { 
	    return (float)value; 
	}

	public static float ToSingle(ushort value) { 
	    return (float)value; 
	}

	// ========== String Conversions ========== //
	
	public static string ToString(bool value) { 
	    return value.ToString(); 
	}
	
	public static string ToString(byte value) { 
	    return value.ToString(); 
	}
	
	public static string ToString(byte value, IFormatProvider provider) {
	    return value.ToString(provider); 
	}

	public static string ToString(char value) { 
	    return value.ToString(); 
	}

	public static string ToString(DateTime value) { 
	    return value.ToString(); 
	}

	public static string ToString(DateTime value, IFormatProvider provider) { 
	    return value.ToString(provider); 
	}

	public static string ToString(decimal value) {
	    return value.ToString();
	}

	public static string ToString(decimal value, IFormatProvider provider) { 
	    return value.ToString(provider); 
	}
	
	public static string ToString(double value) { 
	    return value.ToString(); 
	}

	public static string ToString(double value, IFormatProvider provider) { 
	    return value.ToString(provider);
	}
	
	public static string ToString(float value) { 
	    return value.ToString(); 
	}

	public static string ToString(float value, IFormatProvider provider) { 
	    return value.ToString(provider); 
	}

	public static string ToString(int value) { 
	    return value.ToString(); 
	}

	public static string ToString(int value, IFormatProvider provider) { 
	    return value.ToString(provider); 
	}
	
	public static string ToString(long value) { 
	    return value.ToString(); 
	}

	public static string ToString(long value, IFormatProvider provider) { 
	    return value.ToString(provider); 
	}

	public static string ToString(sbyte value) { 
	    return value.ToString(); 
	}

	public static string ToString(sbyte value, IFormatProvider provider) { 
	    return value.ToString(provider); 
	}
	
	public static string ToString(short value) { 
	    return value.ToString(); 
	}

	public static string ToString(short value, IFormatProvider provider) { 
	    return value.ToString(provider); 
	}

	public static string ToString(string value) {
	    return value;
	}

	public static string ToString(uint value) { 
	    return value.ToString(); 
	}

	public static string ToString(uint value, IFormatProvider provider) { 
	    return value.ToString(provider); 
	}

	public static string ToString(ulong value) { 
	    return value.ToString(); 
	}

	public static string ToString(ulong value, IFormatProvider provider) { 
	    return value.ToString(provider); 
	}

	public static string ToString(ushort value) { 
	    return value.ToString(); 
	}

	public static string ToString(ushort value, IFormatProvider provider) { 
	    return value.ToString(provider); 
	}

	// ========== UInt16 Conversions ========== //

	public static ushort ToUInt16(bool value) { 
	    return (ushort)(value ? 1 : 0); 
	}
	
	public static ushort ToUInt16(byte value) { 
	    return (ushort)value; 
	}

	public static ushort ToUInt16(char value) { 
	    return (ushort)value; 
	}
	
	public static ushort ToUInt16(decimal value) { 
	    if(value > UInt16.MaxValue || value < UInt16.MinValue) 
		throw new OverflowException
		    ("Value is greater than UInt16.MaxValue or less than UInt16.MinValue");
	    
	    return (ushort)value;	    
	}

	public static ushort ToUInt16(double value) { 
	    if(value > UInt16.MaxValue || value < UInt16.MinValue) 
		throw new OverflowException
		    ("Value is greater than UInt16.MaxValue or less than UInt16.MinValue");
	    
	    return (ushort)value;	    
	}
 
	public static ushort ToUInt16(float value) { 
	    if(value > UInt16.MaxValue || value < UInt16.MinValue) 
		throw new OverflowException
		    ("Value is greater than UInt16.MaxValue or less than UInt16.MinValue");
	    
	    return (ushort)value;
	}

	public static ushort ToUInt16(int value) { 
	    if(value > UInt16.MaxValue || value < UInt16.MinValue) 
		throw new OverflowException
		    ("Value is greater than UInt16.MaxValue or less than UInt16.MinValue");

	    return (ushort)value; 
	}
	
	public static ushort ToUInt16(long value) { 
	    if(value > UInt16.MaxValue || value < UInt16.MinValue) 
		throw new OverflowException
		    ("Value is greater than UInt16.MaxValue or less than UInt16.MinValue");

	    return (ushort)value; 
	}

	public static ushort ToUInt16(sbyte value) { 
	    if(value < UInt16.MinValue) 
		throw new OverflowException
		    ("Value is less than UInt16.MinValue");

	    return (ushort)value;  
	}
	
	public static ushort ToUInt16(short value) { 
	    if(value < UInt16.MinValue) 
		throw new OverflowException
		    ("Value is less than UInt16.MinValue");

	    return (ushort)value;  
	}

	public static ushort ToUInt16(string value) {
	    return UInt16.Parse(value);
	}

	public static ushort ToUInt16(string value, IFormatProvider provider) {
	    return UInt16.Parse(value, provider);
	}
	
	public static ushort ToUInt16(uint value) { 
	    if(value > UInt16.MaxValue) 
		throw new OverflowException
		    ("Value is greater than UInt16.MaxValue");

	    return (ushort)value; 
	}

	public static ushort ToUInt16(ulong value) { 
	    if(value > (ulong)UInt16.MaxValue) 
		throw new OverflowException
		    ("Value is greater than UInt16.MaxValue");

	    return (ushort)value; 
	}

	public static ushort ToUInt16(ushort value) { 
	    return value; 
	}

	// ========== UInt32 Conversions ========== //

	public static uint ToUInt32(bool value) { 
	    return (uint)(value ? 1 : 0); 
	}
	
	public static uint ToUInt32(byte value) { 
	    return (uint)value; 
	}

	public static uint ToUInt32(char value) { 
	    return (uint)value; 
	}
	
	public static uint ToUInt32(decimal value) { 
	    if(value > UInt32.MaxValue || value < UInt32.MinValue) 
		throw new OverflowException
		    ("Value is greater than UInt32.MaxValue or less than UInt32.MinValue");
	    
	    return (uint)value;	    
	}

	public static uint ToUInt32(double value) { 
	    if(value > UInt32.MaxValue || value < UInt32.MinValue) 
		throw new OverflowException
		    ("Value is greater than UInt32.MaxValue or less than UInt32.MinValue");
	    
	    return (uint)value;	    
	}
 
	public static uint ToUInt32(float value) { 
	    if(value > UInt32.MaxValue || value < UInt32.MinValue) 
		throw new OverflowException
		    ("Value is greater than UInt32.MaxValue or less than UInt32.MinValue");
	    
	    return (uint)value;
	}

	public static uint ToUInt32(int value) { 
	    if(value < UInt32.MinValue) 
		throw new OverflowException
		    ("Value is less than UInt32.MinValue");

	    return (uint)value; 
	}
	
	public static uint ToUInt32(long value) { 
	    if(value > UInt32.MaxValue || value < UInt32.MinValue) 
		throw new OverflowException
		    ("Value is greater than UInt32.MaxValue or less than UInt32.MinValue");

	    return (uint)value; 
	}

	public static uint ToUInt32(sbyte value) { 
	    if(value < UInt32.MinValue) 
		throw new OverflowException
		    ("Value is less than UInt32.MinValue");

	    return (uint)value;  
	}
	
	public static uint ToUInt32(short value) { 
	    if(value < UInt32.MinValue) 
		throw new OverflowException
		    ("Value is less than UInt32.MinValue");

	    return (uint)value; 
	}

	public static uint ToUInt32(string value) {
	    return UInt32.Parse(value);
	}

	public static uint ToUInt32(string value, IFormatProvider provider) {
	    return UInt32.Parse(value, provider);
	}
	
	public static uint ToUInt32(uint value) { 
	    return value; 
	}

	public static uint ToUInt32(ulong value) { 
	    if(value > UInt32.MaxValue) 
		throw new OverflowException
		    ("Value is greater than UInt32.MaxValue");

	    return (uint)value; 
	}

	public static uint ToUInt32(ushort value) { 
	    return (uint)value; 
	}

	// ========== UInt64 Conversions ========== //

	public static ulong ToUInt64(bool value) { 
	    return (ulong)(value ? 1 : 0); 
	}
	
	public static ulong ToUInt64(byte value) { 
	    return (ulong)value; 
	}

	public static ulong ToUInt64(char value) { 
	    return (ulong)value; 
	}
	
	public static ulong ToUInt64(decimal value) { 
	    if(value > UInt64.MaxValue || value < UInt64.MinValue) 
		throw new OverflowException
		    ("Value is greater than UInt64.MaxValue or less than UInt64.MinValue");
	    
	    return (ulong)value;	    
	}

	public static ulong ToUInt64(double value) { 
	    if(value > UInt64.MaxValue || value < UInt64.MinValue) 
		throw new OverflowException
		    ("Value is greater than UInt64.MaxValue or less than UInt64.MinValue");
	    
	    return (ulong)value;	    
	}
 
	public static ulong ToUInt64(float value) { 
	    if(value > UInt64.MaxValue || value < UInt64.MinValue) 
		throw new OverflowException
		    ("Value is greater than UInt64.MaxValue or less than UInt64.MinValue");
	    
	    return (ulong)value;
	}

	public static ulong ToUInt64(int value) { 
	    if(value < (int)UInt64.MinValue) 
		throw new OverflowException
		    ("Value is less than UInt64.MinValue");

	    return (ulong)value; 
	}
	
	public static ulong ToUInt64(long value) { 
	    if(value < (long)UInt64.MinValue) 
		throw new OverflowException
		    ("Value is less than UInt64.MinValue");

	    return (ulong)value; 
	}

	public static ulong ToUInt64(sbyte value) { 
	    if(value < (sbyte)UInt64.MinValue) 
		throw new OverflowException
		    ("Value is less than UInt64.MinValue");

	    return (ulong)value;  
	}
	
	public static ulong ToUInt64(short value) { 
	    if(value < (short)UInt64.MinValue) 
		throw new OverflowException
		    ("Value is less than UInt64.MinValue");

	    return (ulong)value; 
	}

	public static ulong ToUInt64(string value) {
	    return UInt64.Parse(value);
	}

	public static ulong ToUInt64(string value, IFormatProvider provider) {
	    return UInt64.Parse(value, provider);
	}
	
	public static ulong ToUInt64(uint value) { 
	    return (ulong)value; 
	}

	public static ulong ToUInt64(ulong value) { 
	    return value; 
	}

	public static ulong ToUInt64(ushort value) { 
	    return (ulong)value; 
	}
    } 
}
